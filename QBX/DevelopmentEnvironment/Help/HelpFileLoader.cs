using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

using QBX.ExecutionEngine.Execution;

namespace QBX.DevelopmentEnvironment.Help;

public static class HelpFileLoader
{
	public static IEnumerable<HelpDatabase> LoadDatabases(string path)
	{
		using (var baseStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
			return LoadDatabases(baseStream);
	}

	public static IEnumerable<HelpDatabase> LoadDatabases(Stream baseStream)
	{
		while (baseStream.Position < baseStream.Length)
		{
			var stream = new OffsetStream(baseStream, baseStream.Position);

			using (var reader = new BinaryReader(stream))
			{
				// SECTION: Header
				var signature = reader.ReadUInt16();

				if (signature != 0x4E4C)
					throw new FormatException("File signature not present");

				var version = reader.ReadUInt16();

				if (version != 2)
					throw new FormatException("Unsupported version " + version);

				var attributes = reader.ReadUInt16();

				var ret = new HelpDatabase(
					caseSensitive: (attributes & 1) != 0);

				ret.IsProtected = (attributes & 2) != 0;

				reader.ReadByte(); // control character, meaning unknown
				reader.ReadByte(); // padding

				int topicCount = reader.ReadUInt16();
				int contextStringCount = reader.ReadUInt16();
				int displayWidth = reader.ReadUInt16();
				int predefinedContextStringCount = reader.ReadUInt16(); // usually 0

				var databaseNameBytes = new byte[14];

				reader.ReadExactly(databaseNameBytes); // null-terminated

				ret.DatabaseName = new StringValue(databaseNameBytes).ToStringZ();

				reader.ReadInt32(); // reserved

				int topicIndexOffset = reader.ReadInt32();
				int contextStringsOffset = reader.ReadInt32();
				int contextMappingOffset = reader.ReadInt32();
				int keywordsOffset = reader.ReadInt32(); // 0 if keyword compression is not used
				int huffmanTreeOffset = reader.ReadInt32(); // 0 if huffman compression is not used
				int topicTextOffset = reader.ReadInt32(); // start of topic texts

				reader.ReadInt32(); // reserved
				reader.ReadInt32(); // reserved

				int databaseSize = reader.ReadInt32();

				int headerEnd = (int)stream.Position;

				var contextStrings = new ContextString[contextStringCount];

				// SECTION: Topic Index
				int[] topicIndex = new int[topicCount + 1];

				stream.Position = topicIndexOffset;

				reader.ReadExactly(MemoryMarshal.Cast<int, byte>(topicIndex.AsSpan()));

				if (topicIndex.Any(topicOffset => (topicOffset < headerEnd) || (topicOffset > databaseSize)))
					throw new FormatException("Invalid topic offset in the topic index");

				// SECTION: Context Strings
				var contextMappings = new ContextString[contextStringCount];

				stream.Position = contextStringsOffset;

				var buffer = new StringValue();

				for (int i = 0; i < contextStringCount; i++)
				{
					contextMappings[i] = new ContextString();

					buffer.Length = 0;

					while (true)
					{
						int b = stream.ReadByte();

						if (b < 0)
							throw new FormatException("Unexpected end of file reading context strings");

						if (b == 0)
							break;

						buffer.Append((byte)b);
					}

					contextMappings[i].String = buffer.ToString();
				}

				// SECTION: Context Map
				stream.Position = contextMappingOffset;

				for (int i = 0; i < contextStringCount; i++)
					contextMappings[i].TopicIndex = reader.ReadUInt16();

				// SECTION: Keyword Compression Table
				byte[][]? keywords = null;

				if (keywordsOffset != 0)
				{
					keywords = new byte[1024][];

					int sectionEnd = huffmanTreeOffset;
					if (sectionEnd == 0)
						sectionEnd = topicTextOffset;

					int sectionLength = sectionEnd - keywordsOffset;

					stream.Position = keywordsOffset;

					var sectionData = new byte[sectionLength].AsSpan();

					reader.ReadExactly(sectionData);

					int keywordIndex = 0;

					while ((sectionData.Length > 0) && (keywordIndex < keywords.Length))
					{
						int availableBytes = sectionData.Length - 1;

						int keywordLength = sectionData[0];

						if (keywordLength > availableBytes)
							keywordLength = availableBytes;

						keywords[keywordIndex] = new byte[keywordLength];

						sectionData.Slice(1, keywordLength).CopyTo(keywords[keywordIndex]);

						keywordIndex++;

						sectionData = sectionData.Slice(keywordLength + 1);
					}
				}

				// SECTION: Huffman Tree
				HuffmanTreeNode? huffmanTree = null;

				if (huffmanTreeOffset != 0)
				{
					int sectionLength = topicTextOffset - huffmanTreeOffset;

					short[] treeNodeData = new short[sectionLength / 2];
					HuffmanTreeNode[] treeNodes = new HuffmanTreeNode[treeNodeData.Length];

					stream.Position = huffmanTreeOffset;

					reader.ReadExactly(MemoryMarshal.Cast<short, byte>(treeNodeData.AsSpan()));

					if (treeNodeData.Last() != 0)
						throw new FormatException("Huffman tree is not properly terminated");

					HuffmanTreeNode BuildTree(int index)
					{
						if (treeNodes[index] != null)
							throw new FormatException("Cycle in Huffman tree data");

						var node = new HuffmanTreeNode();

						treeNodes[index] = node;

						bool isLeafNode = (treeNodeData[index] & 0x8000) != 0;

						if (isLeafNode)
							node.Symbol = treeNodeData[index] & 0xFF;
						else
						{
							node.Left = BuildTree(treeNodeData[index] / 2);
							node.Right = BuildTree(index + 1);
						}

						return node;
					}

					huffmanTree = BuildTree(0);
				}

				// SECTION: Topic Text
				for (int i = 0; i < topicCount; i++)
				{
					int sectionLength = topicIndex[i + 1] - topicIndex[i];

					byte[] compressedData = new byte[sectionLength];

					stream.Position = topicIndex[i];

					reader.ReadExactly(compressedData);

					var contextStringsForTopic = contextMappings
						.Where(mapping => mapping.TopicIndex == i)
						.Select(mapping => mapping.String);

					ret.AddTopic(DecodeTopic(compressedData, huffmanTree, keywords), contextStringsForTopic);
				}

				yield return ret;
			}
		}
	}

	static HelpDatabaseTopic DecodeTopic(byte[] compressedData, HuffmanTreeNode? huffmanTree, byte[][]? keywords)
	{
		if (compressedData.Length < 2)
			throw new FormatException("Invalid section data: length is " + compressedData.Length);

		// Step 1: Data is prefixed by its decompressed length
		ushort decompressedDataLength = BitConverter.ToUInt16(compressedData, 0);

		var dataSpan = compressedData.AsSpan().Slice(2);

		// Step 2: Huffman decoding
		if (huffmanTree != null)
			dataSpan = HuffmanDecompress(dataSpan, huffmanTree);

		// Step 3: Keyword decoding
		if (keywords != null)
			dataSpan = KeywordDecompress(dataSpan, keywords);

		if (dataSpan.Length < decompressedDataLength)
			throw new FormatException("Invalid section data: decompressed data is too short");

		dataSpan = dataSpan.Slice(0, decompressedDataLength);

		// Step 4: Transform to in-memory HelpFileTopic
		return HelpDatabaseTopic.Parse(dataSpan);
	}

	static Span<byte> HuffmanDecompress(ReadOnlySpan<byte> dataSpan, HuffmanTreeNode huffmanTreeRoot)
	{
		int nextBit = 128;

		var output = new List<byte>();

		var currentNode = huffmanTreeRoot;

		while (true)
		{
			bool bitValue = (dataSpan[0] & nextBit) != 0;

			var nextNode = bitValue ? currentNode.Right : currentNode.Left;

			if (nextNode == null)
				throw new FormatException("Invalid code in Huffman data stream");

			if (nextNode.Symbol < 0)
				currentNode = nextNode;
			else
			{
				output.Add(unchecked((byte)nextNode.Symbol));
				currentNode = huffmanTreeRoot;
			}

			nextBit >>= 1;

			if (nextBit == 0)
			{
				dataSpan = dataSpan.Slice(1);
				nextBit = 128;

				if (dataSpan.Length == 0)
					break;
			}
		}

		return CollectionsMarshal.AsSpan(output);
	}

	static Span<byte> KeywordDecompress(ReadOnlySpan<byte> dataSpan, byte[][] keywords)
	{
		var output = new List<byte>();

		while (dataSpan.Length > 0)
		{
			byte b = dataSpan[0];
			dataSpan = dataSpan.Slice(1);

			bool isControlByte = (b >= 0x10) && (b <= 0x1A);

			if (!isControlByte)
				output.Add(b);
			else
			{
				if (dataSpan.Length == 0)
					break;

				switch (b)
				{
					case 0x18: // Spaces
					{
						int spaceCount = dataSpan[0];
						dataSpan = dataSpan.Slice(1);

						for (int i = 0; i < spaceCount; i++)
							output.Add((byte)' ');

						break;
					}
					case 0x19: // Run of byte
					{
						if (dataSpan.Length < 2)
							dataSpan = Span<byte>.Empty;
						else
						{
							byte repeatByte = dataSpan[0];
							int repeatLength = dataSpan[1];
							dataSpan = dataSpan.Slice(2);

							for (int i = 0; i < repeatLength; i++)
								output.Add(repeatByte);
						}

						break;
					}
					case 0x1A: // Escaped byte
					{
						byte escapedByte = dataSpan[0];
						dataSpan = dataSpan.Slice(1);

						output.Add(escapedByte);

						break;
					}
					default: // Token reference
					{
						bool appendSpace = (b & 4) != 0;
						int keywordIndex = (b & 3) << 8;

						keywordIndex |= dataSpan[0];
						dataSpan = dataSpan.Slice(1);

						var keyword = keywords[keywordIndex];

						if (keyword != null)
							output.AddRange(keyword);
						if (appendSpace)
							output.Add((byte)' ');

						break;
					}
				}
			}
		}

		return CollectionsMarshal.AsSpan(output);
	}

	class ContextString
	{
		public string String = "";
		public int TopicIndex;

		public override string ToString() => $"\"{String}\" -> [{TopicIndex}]";
	}

	class HuffmanTreeNode
	{
		public int Symbol = -1;
		public HuffmanTreeNode? Left;
		public HuffmanTreeNode? Right;
	}
}


using System;
using System.Collections.Generic;
using System.Linq;

using QBX.ExecutionEngine.Execution.Variables;
using QBX.OperatingSystem;
using QBX.OperatingSystem.FileStructures;

namespace QBX.ExecutionEngine.Execution;

public class OpenFile
{
	public const int NoLineWidthLimit = 255;

	public int FileHandle;
	public OpenFileIOMode IOMode;
	public bool OpenedForAppend;
	public int BufferSize = 512;
	public int RecordLength = 128;
	public int CurrentRecordNumber;
	public int LineWidth = int.MaxValue;

	public List<FileRecordField> Fields = new List<FileRecordField>();
	public bool FieldsPristine = true;
	public int RecordOffset = 0;

	public void ConfigureFields(IEnumerable<FileRecordField> fields, ExecutionContext context)
	{
		ClearFieldConfiguration(context);

		int bytesSpecified = fields.Sum(field => field.Width);

		if (bytesSpecified > RecordLength)
			throw RuntimeException.FieldOverflow();

		Fields.AddRange(fields);

		if (bytesSpecified < RecordLength)
		{
			int unspecifiedBytes = RecordLength - bytesSpecified;

			Fields.AddRange(new FileRecordField(unspecifiedBytes, new StringVariable()));
		}

		foreach (var field in Fields)
		{
			if (field.Variable.Value.Length > field.Width)
				field.Variable.Value.Remove(field.Width, field.Variable.Value.Length - field.Width);

			field.Variable.Value.Capacity = field.Width;

			field.Variable.ValueSpan.Clear();
			while (field.Variable.Value.Length < field.Width)
				field.Variable.Value.Append((byte)0);

			context.FieldVariables[field.Variable] = this;
		}
	}

	public void ClearFieldConfiguration(ExecutionContext context)
	{
		foreach (var previousMapping in Fields)
			context.FieldVariables.Remove(previousMapping.Variable);

		Fields.Clear();
	}

	public void UnlinkFieldVariable(StringVariable variable)
	{
		StringVariable? replacement = null;

		foreach (var field in Fields)
		{
			if (field.Variable == variable)
			{
				// It's not well-formed, but the code is permitted to map the
				// same variable to more than one field.
				replacement ??= new StringVariable(variable.Value);
				field.Variable = replacement;
			}
		}
	}

	public void WriteToFields(ReadOnlySpan<byte> data)
	{
		int fieldIndex = 0;
		int fieldOffset = RecordOffset;

		while ((fieldIndex < Fields.Count) && (fieldOffset >= Fields[fieldIndex].Width))
		{
			fieldOffset -= Fields[fieldIndex].Width;
			fieldIndex++;
		}

		while ((data.Length > 0) && (fieldIndex < Fields.Count))
		{
			var fieldSpan = Fields[fieldIndex].Variable.ValueSpan.Slice(fieldOffset);

			int copySize = Math.Min(data.Length, fieldSpan.Length);

			data.Slice(0, copySize).CopyTo(fieldSpan);
			data = data.Slice(copySize);

			RecordOffset += copySize;
			fieldOffset += copySize;

			if (fieldOffset >= Fields[fieldIndex].Width)
			{
				fieldOffset -= Fields[fieldIndex].Width;
				fieldIndex++;
			}
		}

		if ((fieldIndex >= Fields.Count) && (data.Length != 0))
			throw RuntimeException.FieldOverflow();
	}

	public void ReadFromFields(Span<byte> buffer)
	{
		int fieldIndex = 0;
		int fieldOffset = RecordOffset;

		while ((fieldIndex < Fields.Count) && (fieldOffset >= Fields[fieldIndex].Width))
		{
			fieldOffset -= Fields[fieldIndex].Width;
			fieldIndex++;
		}

		while ((buffer.Length > 0) && (fieldIndex < Fields.Count))
		{
			var fieldSpan = Fields[fieldIndex].Variable.ValueSpan.Slice(fieldOffset);

			int copySize = Math.Min(buffer.Length, fieldSpan.Length);

			fieldSpan.Slice(0, copySize).CopyTo(buffer);
			buffer = buffer.Slice(copySize);

			RecordOffset += copySize;
			fieldOffset += copySize;

			if (fieldOffset >= Fields[fieldIndex].Width)
			{
				fieldOffset -= Fields[fieldIndex].Width;
				fieldIndex++;
			}
		}

		if ((fieldIndex >= Fields.Count) && (buffer.Length != 0))
			throw RuntimeException.FieldOverflow();
	}

	public StringValue ReadLine(DOS dos)
	{
		var buffer = new StringValue();

		Func<byte> readByte;
		Action<byte> unreadByte;

		if (IOMode != OpenFileIOMode.Random)
		{
			readByte = () => dos.ReadByte(FileHandle);
			unreadByte =
				b =>
				{
					var fileDescriptor = dos.Files[FileHandle];

					fileDescriptor?.ReadBuffer.Inject(b);
				};
		}
		else
		{
			readByte =
				() =>
				{
					unsafe
					{
						Span<byte> byteBuffer = stackalloc byte[1];

						ReadFromFields(byteBuffer);

						return byteBuffer[0];
					}
				};

			unreadByte =
				b =>
				{
					RecordOffset--;
				};
		}

		while (true)
		{
			byte b = readByte();

			if (b == 13)
			{
				b = readByte();

				if (b != 10)
					unreadByte(b);

				break;
			}

			buffer.Append(b);
		}

		return buffer;
	}

	public void FlushFields(DOS dos)
	{
		if (!FieldsPristine)
		{
			dos.SeekFile(FileHandle, CurrentRecordNumber * RecordLength, MoveMethod.FromBeginning);

			foreach (var field in Fields)
				dos.Write(FileHandle, field.Variable.Value.AsSpan(), out _);

			FieldsPristine = true;
		}
	}
}

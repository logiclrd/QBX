using System.Runtime.InteropServices;

using QBX.ExecutionEngine.Compiled.Statements;
using QBX.Firmware;
using QBX.Firmware.Fonts;
using QBX.Hardware;

namespace QBX.Tests.ExecutionEngine;

public class NumberFormatDirectiveTests
{
	class TestConfiguration(
		string pattern, char leftPadChar, bool leadingDollarSign, bool separateThousands, int exponentCharacters, bool trailingMinusSign,
		string inputAsString,
		string expectedOutput)
	{
		public string Pattern => pattern;
		public char LeftPadChar => leftPadChar;
		public bool LeadingDollarSign => leadingDollarSign;
		public bool SeparateThousands => separateThousands;
		public int ExponentCharacters => exponentCharacters;
		public bool TrailingMinusSign => trailingMinusSign;
		public string InputAsString => inputAsString;
		public string ExpectedOutput => expectedOutput;
	}

	static IEnumerable<TestConfiguration> TestConfigurationGenerator()
	{
		yield return new TestConfiguration(
			"0", ' ', false, false, 0, false,
			"1",
			"1");
		yield return new TestConfiguration(
			"00", ' ', false, false, 0, false,
			"1",
			" 1");
		yield return new TestConfiguration(
			"00000", ' ', false, false, 0, false,
			"5",
			"    5");
		yield return new TestConfiguration(
			"00", ' ', false, false, 0, false,
			"10",
			"10");
		yield return new TestConfiguration(
			"00", ' ', false, false, 0, false,
			"100",
			"%100");
		yield return new TestConfiguration(
			"00", ' ', false, false, 0, false,
			"1000",
			"%1000");
		yield return new TestConfiguration(
			"00", ' ', false, true, 0, false,
			"1000",
			"%1,000");
		yield return new TestConfiguration(
			"0000", ' ', false, true, 0, false,
			"1",
			"   1");
		yield return new TestConfiguration(
			"0000", ' ', false, true, 0, false,
			"10",
			"  10");
		yield return new TestConfiguration(
			"0000", '*', false, true, 0, false,
			"1",
			"***1");
		yield return new TestConfiguration(
			"0000", '*', false, true, 0, false,
			"10",
			"**10");
		yield return new TestConfiguration(
			"0000", '*', true, true, 0, false,
			"10",
			"*$10");
		yield return new TestConfiguration(
			"0000", '*', true, true, 0, false,
			"100",
			"$100");
		yield return new TestConfiguration(
			"0000", '*', true, true, 0, false,
			"1000",
			"%$1,000");
		yield return new TestConfiguration(
			"00.00", ' ', false, false, 0, false,
			"1",
			" 1.00");
		yield return new TestConfiguration(
			"00.00", ' ', false, false, 4, false,
			"1",
			" 1.00D+00");
		yield return new TestConfiguration(
			"00.00", ' ', false, false, 4, false,
			"10",
			" 1.00D+01");
		yield return new TestConfiguration(
			"00.00", ' ', false, false, 4, false,
			"100",
			" 1.00D+02");
		yield return new TestConfiguration(
			"00.00", ' ', false, false, 4, false,
			"1000",
			" 1.00D+03");
		yield return new TestConfiguration(
			"00.00", ' ', false, false, 5, false,
			"100",
			" 1.00D+002");
		yield return new TestConfiguration(
			"00.00", ' ', false, false, 5, false,
			"1000",
			" 1.00D+003");
		yield return new TestConfiguration(
			"00.00", ' ', false, false, 5, false,
			"1000",
			" 1.00D+003");
		yield return new TestConfiguration(
			"000.00", ' ', false, false, 4, false,
			"10",
			" 10.00D+00");
		yield return new TestConfiguration(
			"00.00", ' ', false, false, 4, true,
			"10",
			"10.00D+00 ");
		yield return new TestConfiguration(
			"00.00", ' ', false, false, 4, true,
			"-10",
			"10.00D+00-");
		yield return new TestConfiguration(
			"00.00", ' ', false, false, 0, true,
			"-10",
			"10.00-");
		yield return new TestConfiguration(
			"0.00", ' ', false, false, 4, false,
			"0.1",
			"0.10D+00");
		yield return new TestConfiguration(
			"00.00", ' ', false, false, 4, false,
			"0.1",
			" 1.00D-01");
		yield return new TestConfiguration(
			"000.00", ' ', false, false, 4, false,
			"0.1",
			" 10.00D-02");
		yield return new TestConfiguration(
			"000.00", ' ', false, false, 4, false,
			"0.1234",
			" 12.34D-02");
		yield return new TestConfiguration(
			"000.00", ' ', false, false, 4, false,
			"0.12345",
			" 12.35D-02");
		yield return new TestConfiguration(
			"000.00", ' ', false, false, 4, false,
			"1.2345",
			" 12.35D-01");
		yield return new TestConfiguration(
			"000.00", ' ', false, false, 4, false,
			"9.9999",
			" 10.00D+00");
		yield return new TestConfiguration(
			"000.00", ' ', false, false, 0, false,
			"9.9999",
			" 10.00");
	}

	static IEnumerable<object[]> DoubleTestCaseGenerator()
	{
		string exponentCharacter = "E";

		foreach (var config in TestConfigurationGenerator())
		{
			yield return
				new object[]
				{
					config.Pattern,
					config.LeftPadChar,
					config.LeadingDollarSign,
					config.SeparateThousands,
					config.ExponentCharacters,
					config.TrailingMinusSign,

					double.Parse(config.InputAsString),
					exponentCharacter[0],

					config.ExpectedOutput.Replace("D", exponentCharacter),
				};

			exponentCharacter = (exponentCharacter == "E") ? "D" : "E";
		}
	}

	static IEnumerable<object[]> DecimalTestCaseGenerator()
	{
		char exponentCharacter = 'E';

		foreach (var config in TestConfigurationGenerator())
		{
			// Ignore test cases depending on input precision beyond the range of CURRENCY.
			int decimalPosition = config.InputAsString.IndexOf('.');

			if (decimalPosition >= 0)
			{
				int digitsAfterDecimal = config.InputAsString.Length - decimalPosition - 1;

				if (digitsAfterDecimal > 4)
					continue;
			}

			yield return
				new object[]
				{
					config.Pattern,
					config.LeftPadChar,
					config.LeadingDollarSign,
					config.SeparateThousands,
					config.ExponentCharacters,
					config.TrailingMinusSign,

					decimal.Parse(config.InputAsString),

					config.ExpectedOutput,
				};

			exponentCharacter = (exponentCharacter == 'E') ? 'D' : 'E';
		}
	}

	[TestCaseSource(nameof(DoubleTestCaseGenerator))]
	public void EmitDouble(
		string pattern, char leftPadChar, bool leadingDollarSign, bool separateThousands, int exponentCharacters, bool trailingMinusSign,
		double value, char exponentLetter,
		string expectedOutput)
	{
		// Arrange
		var machine = new Machine();

		var sink = new CapturingVisualLibrary(machine);

		var sut = new NumericFormatDirective(pattern, leftPadChar, leadingDollarSign, separateThousands, exponentCharacters, trailingMinusSign);

		// Act
		sut.Emit(value, exponentLetter, sink);

		// Assert
		sink.GetCapturedOutput().Should().Be(expectedOutput);
	}

	[TestCaseSource(nameof(DecimalTestCaseGenerator))]
	public void EmitDecimal(
		string pattern, char leftPadChar, bool leadingDollarSign, bool separateThousands, int exponentCharacters, bool trailingMinusSign,
		decimal value,
		string expectedOutput)
	{
		// Arrange
		var machine = new Machine();

		var sink = new CapturingVisualLibrary(machine);

		var sut = new NumericFormatDirective(pattern, leftPadChar, leadingDollarSign, separateThousands, exponentCharacters, trailingMinusSign);

		// Act
		sut.Emit(value, sink);

		// Assert
		sink.GetCapturedOutput().Should().Be(expectedOutput);
	}

	class CapturingVisualLibrary(Machine machine) : VisualLibrary(machine)
	{
		List<byte> _buffer = new List<byte>();

		public override void WriteText(ReadOnlySpan<byte> bytes)
		{
			_buffer.AddRange(bytes);
		}

		public string GetCapturedOutput()
		{
			return new CP437Encoding().GetString(CollectionsMarshal.AsSpan(_buffer));
		}

		public override void RefreshParameters() { }
		public override void ScrollText() { }
		protected override void ClearImplementation() { }
	}
}

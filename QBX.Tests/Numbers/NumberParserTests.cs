// TODO: include a test for "" parsing to 0
using QBX.Numbers;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace QBX.Tests.Numbers;

public class NumberParserTests
{
	[TestCase("", true, 0)]
	[TestCase("0", true, 0)]
	[TestCase("0000", true, 0)]
	[TestCase("1234", true, 1234)]
	[TestCase("32767", true, 32767)]
	[TestCase("-32768", true, -32768)]
	[TestCase("32768", false, 0)]
	[TestCase("-32769", false, 0)]
	[TestCase("1.0", false, 0)]
	[TestCase("35%", true, 35)]
	[TestCase("35&", false, 0)]
	[TestCase("35!", false, 0)]
	[TestCase("35#", false, 0)]
	[TestCase("35@", false, 0)]
	[TestCase("foo", false, 0)]
	[TestCase("\"foo\"", false, 0)]
	public void TryAsInteger(string input, bool expectedResult, short expectedValue)
	{
		// Act
		var result = NumberParser.TryAsInteger(input, out var value);

		// Assert
		result.Should().Be(expectedResult);

		if (result)
			value.Should().Be(expectedValue);
	}

	[TestCase("", true, 0)]
	[TestCase("0", true, 0)]
	[TestCase("0000", true, 0)]
	[TestCase("1234", true, 1234)]
	[TestCase("32767", true, 32767)]
	[TestCase("-32768", true, -32768)]
	[TestCase("32768", true, 32768)]
	[TestCase("-32769", true, -32769)]
	[TestCase("2147483647", true, 2147483647)]
	[TestCase("-2147483648", true, -2147483648)]
	[TestCase("2147483648", false, 0)]
	[TestCase("-2147483649", false, 0)]
	[TestCase("1.0", false, 0)]
	[TestCase("35%", false, 0)]
	[TestCase("35&", true, 35)]
	[TestCase("35!", false, 0)]
	[TestCase("35#", false, 0)]
	[TestCase("35@", false, 0)]
	[TestCase("foo", false, 0)]
	[TestCase("\"foo\"", false, 0)]
	public void TryAsLong(string input, bool expectedResult, int expectedValue)
	{
		// Act
		var result = NumberParser.TryAsLong(input, out var value);

		// Assert
		result.Should().Be(expectedResult);

		if (result)
			value.Should().Be(expectedValue);
	}

	[TestCase("", true, 0f)]
	[TestCase("0", true, 0f)]
	[TestCase("0000", true, 0f)]
	[TestCase("1234", true, 1234f)]
	[TestCase("32767", true, 32767f)]
	[TestCase("-32768", true, -32768f)]
	[TestCase("32768", true, 32768f)]
	[TestCase("-32769", true, -32769f)]
	[TestCase("3276800", true, 3276800)]
	[TestCase("-3276900", true, -3276900)]
	[TestCase("3276800000000", false, 0)]
	[TestCase("-3276900000000", false, 0)]
	[TestCase("327.68", true, 327.68f)]
	[TestCase("-327.69", true, -327.69f)]
	[TestCase("3.2768", true, 3.2768f)]
	[TestCase("-3.2769", true, -3.2769f)]
	[TestCase("0.0032768", true, 0.0032768f)]
	[TestCase("-0.0032769", true, -0.0032769f)]
	[TestCase("0.0000000032768", false, 0)]
	[TestCase("-0.0000000032769", false, 0)]
	[TestCase("1.2345e20", true, 1.2345e20f)]
	[TestCase("1.2345e-20", true, 1.2345e-20f)]
	[TestCase("-1.2345e20", true, -1.2345e20f)]
	[TestCase("-1.2345e-20", true, -1.2345e-20f)]
	[TestCase("2147483648", false, 0)]
	[TestCase("-2147483649", false, 0)]
	[TestCase("2147483648!", true, 2147483648f)] // will be truncated
	[TestCase("-2147483649!", true, -2147483649f)]
	[TestCase("35%", false, 0)]
	[TestCase("35&", false, 0)]
	[TestCase("35!", true, 35)]
	[TestCase("35#", false, 0)]
	[TestCase("35@", false, 0)]
	[TestCase("foo", false, 0)]
	[TestCase("\"foo\"", false, 0)]
	public void TryAsSingle(string input, bool expectedResult, float expectedValue)
	{
		// Act
		var result = NumberParser.TryAsSingle(input, out var value);

		// Assert
		result.Should().Be(expectedResult);

		if (result)
			value.Should().Be(expectedValue);
	}

	[TestCase("", true, 0d)]
	[TestCase("0", true, 0d)]
	[TestCase("0000", true, 0d)]
	[TestCase("1234", true, 1234d)]
	[TestCase("32767", true, 32767d)]
	[TestCase("-32768", true, -32768d)]
	[TestCase("32768", true, 32768d)]
	[TestCase("-32769", true, -32769d)]
	[TestCase("3276800000000", true, 3276800000000d)]
	[TestCase("-3276900000000", true, -3276900000000d)]
	[TestCase("327.68", true, 327.68d)]
	[TestCase("-327.69", true, -327.69d)]
	[TestCase("3.2768", true, 3.2768d)]
	[TestCase("-3.2769", true, -3.2769d)]
	[TestCase("0.0000000032768", true, 0.0000000032768d)]
	[TestCase("-0.0000000032769", true, -0.0000000032769d)]
	[TestCase("1.2345e20", true, 1.2345e20d)]
	[TestCase("1.2345e-20", true, 1.2345e-20d)]
	[TestCase("-1.2345e20", true, -1.2345e20d)]
	[TestCase("-1.2345e-20", true, -1.2345e-20d)]
	[TestCase("2147483648", true, 2147483648d)]
	[TestCase("-2147483649", true, -2147483649d)]
	[TestCase("3.14159265358979323864264", true, 3.1415926535897931d)]
	[TestCase("3.14159265358979323864264e90", true, 3.14159265358979323864264e90d)]
	[TestCase("3141592653589793238642640000000000000000000000000000000000000000000000000000000000000000000", true, 3.14159265358979323864264e90d)]
	[TestCase("3.14159265358979323864264e-90", true, 3.14159265358979323864264e-90d)]
	[TestCase("0.00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000314159265358979323864264", true, 3.14159265358979323864264e-90d)]
	[TestCase("35%", false, 0)]
	[TestCase("35&", false, 0)]
	[TestCase("35!", false, 0)]
	[TestCase("35#", true, 35)]
	[TestCase("35@", false, 0)]
	[TestCase("foo", false, 0)]
	[TestCase("\"foo\"", false, 0)]
	public void TryAsDouble(string input, bool expectedResult, double expectedValue)
	{
		// Act
		var result = NumberParser.TryAsDouble(input, out var value);

		// Assert
		result.Should().Be(expectedResult);

		if (result)
			value.Should().Be(expectedValue);
	}

	const string DummyValue = "0";

	[TestCase("", true, "0")]
	[TestCase("0", true, "0")]
	[TestCase("0000", true, "0")]
	[TestCase("1234", true, "1234")]
	[TestCase("32767", true, "32767")]
	[TestCase("-32768", true, "-32768")]
	[TestCase("32768", true, "32768")]
	[TestCase("-32769", true, "-32769")]
	[TestCase("3276800000000", true, "3276800000000")]
	[TestCase("-3276900000000", true, "-3276900000000")]
	[TestCase("327.68", true, "327.68")]
	[TestCase("-327.69", true, "-327.69")]
	[TestCase("3.2768", true, "3.2768")]
	[TestCase("-3.2769", true, "-3.2769")]
	[TestCase("0.0000000032768", true, "0.0000")]
	[TestCase("-0.0000000032769", true, "-0")]
	[TestCase("2147483648", true, "2147483648")]
	[TestCase("-2147483649", true, "-2147483649")]
	[TestCase("922337203685477.5807", true, "922337203685477.5807")]
	[TestCase("-922337203685477.5808", true, "-922337203685477.5808")]
	[TestCase("922337203685477.5808", false, DummyValue)]
	[TestCase("-922337203685477.5809", false, DummyValue)]
	[TestCase("3.14159265358979323864264", true, "3.1416")]
	[TestCase("0.00001", true, "0.0000")]
	[TestCase("0.00005", true, "0.0001")]
	[TestCase("-0.00001", true, "0.0000")]
	[TestCase("-0.00005", true, "0.00000")]
	[TestCase("35%", false, DummyValue)]
	[TestCase("35&", false, DummyValue)]
	[TestCase("35!", false, DummyValue)]
	[TestCase("35#", false, DummyValue)]
	[TestCase("35@", true, "35")]
	[TestCase("foo", false, DummyValue)]
	[TestCase("\"foo\"", false, DummyValue)]
	public void TryAsCurrency(string input, bool expectedResult, string expectedValueString)
	{
		// Arrange
		decimal expectedValue = decimal.Parse(expectedValueString);

		// Act
		var result = NumberParser.TryAsCurrency(input, out var value);

		// Assert
		result.Should().Be(expectedResult);

		if (result)
			value.Should().Be(expectedValue);
	}

	[TestCase("", true, typeof(short), "0")]
	[TestCase("1", true, typeof(short), "1")]
	[TestCase("32767", true, typeof(short), "32767")]
	[TestCase("32768", true, typeof(int), "32768")]
	[TestCase("2147483647", true, typeof(int), "2147483647")]
	[TestCase("2147483648", true, typeof(double), "2147483648")]
	[TestCase("35@", true, typeof(decimal), "35.0000")]
	[TestCase("35.0001@", true, typeof(decimal), "35.0001")]
	[TestCase("-35.00005@", false, typeof(void), "")]
	[TestCase("banana", false, typeof(void), "")]
	public void TryParse(string input, bool expectedResult, Type expectedValueType, string expectedValueString)
	{
		// Act
		var result = NumberParser.TryParse(input, out var value);

		// Assert
		result.Should().Be(expectedResult);

		if (result)
		{
			value.Should().BeOfType(expectedValueType);
			value.ToString().Should().Be(expectedValueString);
		}
	}
}

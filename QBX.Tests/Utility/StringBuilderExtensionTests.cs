using System.Text;

using QBX.Utility;

namespace QBX.Tests.Utility;

public class StringBuilderExtensionsTests
{
	[TestCase(new string[] { "123" }, "123", StringComparison.InvariantCulture, true)]
	[TestCase(new string[] { "123" }, "123", StringComparison.OrdinalIgnoreCase, true)]
	[TestCase(new string[] { "123" }, "1234", StringComparison.OrdinalIgnoreCase, false)]
	[TestCase(new string[] { "test", "CASE" }, "testCASE", StringComparison.Ordinal, true)]
	[TestCase(new string[] { "test", "CASE" }, "TESTcase", StringComparison.Ordinal, false)]
	[TestCase(new string[] { "test", "CASE" }, "TESTcase", StringComparison.OrdinalIgnoreCase, true)]
	[TestCase(new string[] { "test", "CASE" }, "TEST", StringComparison.OrdinalIgnoreCase, false)]
	[TestCase(new string[] { "testCASE" }, "testCASE", StringComparison.Ordinal, true)]
	[TestCase(new string[] { "testCASE" }, "TESTcase", StringComparison.Ordinal, false)]
	[TestCase(new string[] { "testCASE" }, "TESTcase", StringComparison.OrdinalIgnoreCase, true)]
	[TestCase(new string[] { "testCASE" }, "TEST", StringComparison.OrdinalIgnoreCase, false)]
	public void Equals_should_return_correct_result(string[] inputs, string comparand, StringComparison comparison, bool expectedResult)
	{
		// Arrange
		var buffer = new StringBuilder(capacity: inputs[0].Length);

		for (int i = 0; i < inputs.Length; i++)
			buffer.Append(inputs[i]);

		// Act
		bool result = buffer.Equals(comparand, comparison);

		// Assert
		result.Should().Be(expectedResult);
	}
}

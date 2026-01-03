using QBX.CodeModel.Statements;

namespace QBX.Tests.Parser.Statements;

public class DefTypeRangeTests
{
	[TestCase('A', null, false)]
	[TestCase('Z', null, false)]
	[TestCase('1', null, true)]
	[TestCase('!', null, true)]
	[TestCase('A', 'B', false)]
	[TestCase('A', 'Z', false)]
	[TestCase('Y', 'Z', false)]
	[TestCase('A', '1', true)]
	[TestCase('A', '!', true)]
	[TestCase('A', 'A', false)]
	[TestCase('B', 'A', true)]
	public void Validate(char start, char? end, bool expectException)
	{
		// Arrange
		var sut = new DefTypeRange();

		sut.Start = start;
		sut.End = end;

		// Act
		Action action = () => sut.Validate();

		// Assert
		if (expectException)
			action.Should().Throw<Exception>();
		else
			action.Should().NotThrow();
	}

	[TestCase('A', null, 'A', null, true)] // identical single letter
	[TestCase('A', null, 'B', null, true)] // adjacent single letter
	[TestCase('A', null, 'C', null, false)] // non-adjacent single letter
	[TestCase('C', 'E', 'A', null, false)] // range vs. disjoint single letter
	[TestCase('C', 'E', 'B', null, true)]  // range vs. adjacent single letter
	[TestCase('C', 'E', 'C', null, true)]  // range vs. contained single letter
	[TestCase('C', 'E', 'D', null, true)]  // |
	[TestCase('C', 'E', 'E', null, true)]  // |
	[TestCase('C', 'E', 'F', null, true)]  // range vs. adjacent single letter
	[TestCase('C', 'E', 'G', null, false)] // range vs. disjoint single letter
	[TestCase('L', null, 'N', 'P', false)] // disjoint single letter vs. range
	[TestCase('M', null, 'N', 'P', true)]  // adjacent single letter vs. range
	[TestCase('N', null, 'N', 'P', true)]  // contained single letter vs. range
	[TestCase('O', null, 'N', 'P', true)]  // |
	[TestCase('P', null, 'N', 'P', true)]  // |
	[TestCase('Q', null, 'N', 'P', true)]  // adjacent single letter vs. range
	[TestCase('R', null, 'N', 'P', false)] // disjoint single letter vs. range
	[TestCase('A', 'C', 'E', 'G', false)] // (r1) [r2]
	[TestCase('B', 'D', 'E', 'G', true)]  // (r1)[r2]
	[TestCase('C', 'E', 'E', 'G', true)]  // (r1|r2]
	[TestCase('D', 'F', 'E', 'G', true)]  // (r1[ )r2]
	[TestCase('E', 'G', 'E', 'G', true)]  // (r[1 r)2]
	[TestCase('F', 'H', 'E', 'G', true)]  // [r2( [r1)
	[TestCase('G', 'I', 'E', 'G', true)]  // [r2|r1)
	[TestCase('H', 'J', 'E', 'G', true)]  // [r2](r1)
	[TestCase('I', 'K', 'E', 'G', false)] // [r2] (r1)
	[TestCase('A', 'C', 'A', 'B', true)]  // |r2]1)
	[TestCase('A', 'B', 'A', 'C', true)]  // |r1)2]
	[TestCase('A', 'C', 'B', 'C', true)]  // (r[12|
	[TestCase('B', 'C', 'A', 'C', true)]  // [r(21|
	[TestCase('A', 'G', 'C', 'E', true)]  // (r [r2] 1)
	[TestCase('C', 'E', 'A', 'G', true)]  // [r (r1) 2]
	public void OverlapsWith(char start, char? end, char otherStart, char? otherEnd, bool expectedResult)
	{
		// Arrange
		var sut = new DefTypeRange();

		sut.Start = start;
		sut.End = end;

		var other = new DefTypeRange();

		other.Start = otherStart;
		other.End = otherEnd;

		// Act
		bool result = sut.OverlapsWith(other);

		// Assert
		result.Should().Be(expectedResult);
	}

	[TestCase('A', null, 'A', null, 'A', null)]
	[TestCase('A', null, 'B', null, 'A', 'B')]
	[TestCase('B', null, 'A', null, 'A', 'B')]
	[TestCase('A', null, 'B', 'C', 'A', 'C')]
	[TestCase('B', null, 'B', 'C', 'B', 'C')]
	[TestCase('C', null, 'B', 'C', 'B', 'C')]
	[TestCase('D', null, 'B', 'C', 'B', 'D')]
	[TestCase('B', 'C', 'A', null, 'A', 'C')]
	[TestCase('B', 'C', 'B', null, 'B', 'C')]
	[TestCase('B', 'C', 'C', null, 'B', 'C')]
	[TestCase('B', 'C', 'D', null, 'B', 'D')]
	[TestCase('A', 'B', 'A', 'B', 'A', 'B')]
	[TestCase('A', 'B', 'A', 'C', 'A', 'C')]
	[TestCase('B', 'C', 'A', 'C', 'A', 'C')]
	[TestCase('A', 'B', 'B', 'C', 'A', 'C')]
	[TestCase('B', 'C', 'A', 'B', 'A', 'C')]
	[TestCase('A', 'B', 'C', 'D', 'A', 'D')]
	[TestCase('C', 'D', 'A', 'B', 'A', 'D')]
	[TestCase('A', 'C', 'B', 'D', 'A', 'D')]
	[TestCase('B', 'D', 'A', 'C', 'A', 'D')]
	[TestCase('A', 'D', 'B', 'C', 'A', 'D')]
	[TestCase('B', 'C', 'A', 'D', 'A', 'D')]
	public void Merge(char start, char? end, char otherStart, char? otherEnd, char expectedStart, char? expectedEnd)
	{
		// Arrange
		var sut = new DefTypeRange();

		sut.Start = start;
		sut.End = end;

		var other = new DefTypeRange();

		other.Start = otherStart;
		other.End = otherEnd;

		// Act
		sut.Merge(other);

		// Assert
		sut.Start.Should().Be(expectedStart);
		sut.End.Should().Be(expectedEnd);
	}

	[TestCase('A', null, 'A', null, 0)]
	[TestCase('A', 'B', 'A', null, 0)]
	[TestCase('A', null, 'A', 'B', 0)]
	[TestCase('A', 'B', 'A', 'B', 0)]
	[TestCase('A', null, 'B', null, -1)]
	[TestCase('A', null, 'B', 'C', -1)]
	[TestCase('A', 'B', 'B', null, -1)]
	[TestCase('A', 'B', 'B', 'C', -1)]
	[TestCase('B', null, 'A', null, +1)]
	[TestCase('B', null, 'A', 'B', +1)]
	[TestCase('B', 'C', 'A', null, +1)]
	[TestCase('B', 'C', 'A', 'B', +1)]
	public void CompareTo(char start, char? end, char otherStart, char? otherEnd, int expectedResultSign)
	{
		// Arrange
		var sut = new DefTypeRange();

		sut.Start = start;
		sut.End = end;

		var other = new DefTypeRange();

		other.Start = otherStart;
		other.End = otherEnd;

		// Act
		int result = sut.CompareTo(other);

		// Assert
		Math.Sign(result).Should().Be(expectedResultSign);
	}

	[TestCase('A', null, "A")]
	[TestCase('A', 'B', "A-B")]
	[TestCase('G', 'R', "G-R")]
	public void Render(char start, char? end, string expectedResult)
	{
		// Arrange
		var sut = new DefTypeRange();

		sut.Start = start;
		sut.End = end;

		var buffer = new StringWriter();

		// Act
		sut.Render(buffer);

		// Assert
		buffer.ToString().Should().Be(expectedResult);
	}
}

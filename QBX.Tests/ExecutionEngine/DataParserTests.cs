using QBX.ExecutionEngine;
using QBX.ExecutionEngine.Compiled;
using QBX.ExecutionEngine.Compiled.Expressions;
using QBX.ExecutionEngine.Compiled.Statements;
using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;
using QBX.Hardware;

using ExecutionContext = QBX.ExecutionEngine.Execution.ExecutionContext;

namespace QBX.Tests.ExecutionEngine;

public class DataParserTests
{
	[Test]
	public void Restart_should_restart_enumeration()
	{
		// Arrange
		var parser = new DataParser();

		parser.AddDataSource(["one", "two", "three", "four", "five"]);

		// Act
		var result1 = parser.GetNextDataItem(null);
		var result2 = parser.GetNextDataItem(null);
		var result3 = parser.GetNextDataItem(null);

		parser.Restart();

		var result4 = parser.GetNextDataItem(null);
		var result5 = parser.GetNextDataItem(null);

		parser.Restart();

		var result6 = parser.GetNextDataItem(null);
		var result7 = parser.GetNextDataItem(null);
		var result8 = parser.GetNextDataItem(null);
		var result9 = parser.GetNextDataItem(null);
		var result10 = parser.GetNextDataItem(null);

		// Assert
		result1.Should().Be("one");
		result2.Should().Be("two");
		result3.Should().Be("three");

		result4.Should().Be("one");
		result5.Should().Be("two");

		result6.Should().Be("one");
		result7.Should().Be("two");
		result8.Should().Be("three");
		result9.Should().Be("four");
		result10.Should().Be("five");
	}

	[Test]
	public void TryGetLineNumber_should_find_matching_lines()
	{
		// Arrange
		QBX.CodeModel.Statements.EmptyStatement dummy = new();

		var parser = new DataParser();

		var label1 = new LabelStatement("label1", dummy);
		var label2 = new LabelStatement("label2", dummy);
		var label3 = new LabelStatement("label3", dummy);
		var label4 = new LabelStatement("label4", dummy);
		var label5 = new LabelStatement("label5", dummy);

		parser.AddLabel(label1);
		parser.AddDataSource(["foo"]);
		parser.AddDataSource(["foo"]);
		parser.AddDataSource(["foo"]);
		parser.AddLabel(label2);
		parser.AddLabel(label3);
		parser.AddDataSource(["foo"]);
		parser.AddDataSource(["foo"]);
		parser.AddDataSource(["foo"]);
		parser.AddLabel(label4);
		parser.AddDataSource(["foo"]);
		parser.AddDataSource(["foo"]);
		parser.AddLabel(label5);

		// Act
		bool result1 = parser.TryGetLineNumber(label1.LabelName, out var lineNumber1);
		bool result2 = parser.TryGetLineNumber(label2.LabelName, out var lineNumber2);
		bool result3 = parser.TryGetLineNumber(label3.LabelName, out var lineNumber3);
		bool result4 = parser.TryGetLineNumber(label4.LabelName, out var lineNumber4);
		bool result5 = parser.TryGetLineNumber(label5.LabelName, out var lineNumber5);

		// Assert
		result1.Should().BeTrue();
		lineNumber1.Should().Be(0);

		result2.Should().BeTrue();
		lineNumber2.Should().Be(3);

		result3.Should().BeTrue();
		lineNumber3.Should().Be(3);

		result4.Should().BeTrue();
		lineNumber4.Should().Be(6);

		result5.Should().BeTrue();
		lineNumber5.Should().Be(8);
	}

	[Test]
	public void TryGetLineNumber_should_return_false_when_no_match()
	{
		// Arrange
		QBX.CodeModel.Statements.EmptyStatement dummy = new();

		var parser = new DataParser();

		var label = new LabelStatement("label1", dummy);

		parser.AddDataSource(["foo"]);
		parser.AddLabel(label);
		parser.AddDataSource(["foo"]);

		// Act
		bool result = parser.TryGetLineNumber("no such label", out var lineNumber);

		// Assert
		result.Should().BeFalse();
	}

	[Test]
	public void RestartAtLine_with_label_should_restart_at_correct_points()
	{
		// Arrange
		QBX.CodeModel.Statements.EmptyStatement dummy = new();

		var parser = new DataParser();

		var label1 = new LabelStatement("label1", dummy);
		var label2 = new LabelStatement("label2", dummy);
		var label3 = new LabelStatement("label3", dummy);
		var label4 = new LabelStatement("label4", dummy);
		var label5 = new LabelStatement("label5", dummy);

		parser.AddLabel(label1);
		parser.AddDataSource(["one"]);
		parser.AddDataSource(["two"]);
		parser.AddDataSource(["three"]);
		parser.AddLabel(label2);
		parser.AddLabel(label3);
		parser.AddDataSource(["four"]);
		parser.AddDataSource(["five"]);
		parser.AddDataSource(["six"]);
		parser.AddLabel(label4);
		parser.AddDataSource(["seven"]);
		parser.AddDataSource(["eight"]);
		parser.AddLabel(label5);

		// Act
		var result1 = parser.GetNextDataItem(dummy);
		var result2 = parser.GetNextDataItem(dummy);
		parser.RestartAtLine(label2.LabelName);
		var result3 = parser.GetNextDataItem(dummy);
		var result4 = parser.GetNextDataItem(dummy);
		parser.RestartAtLine(label3.LabelName);
		var result5 = parser.GetNextDataItem(dummy);
		var result6 = parser.GetNextDataItem(dummy);
		var result7 = parser.GetNextDataItem(dummy);
		var result8 = parser.GetNextDataItem(dummy);
		parser.RestartAtLine(label4.LabelName);
		var result9 = parser.GetNextDataItem(dummy);
		parser.RestartAtLine(label1.LabelName);
		var result10 = parser.GetNextDataItem(dummy);
		parser.RestartAtLine(label5.LabelName);

		// Assert
		result1.Should().Be("one");
		result2.Should().Be("two");

		result3.Should().Be("four");
		result4.Should().Be("five");

		result5.Should().Be("four");
		result6.Should().Be("five");
		result7.Should().Be("six");
		result8.Should().Be("seven");

		result9.Should().Be("seven");

		result10.Should().Be("one");
	}

	[Test]
	public void RestartAtLine_with_line_number_should_restart_at_correct_points()
	{
		// Arrange
		QBX.CodeModel.Statements.EmptyStatement dummy = new();

		var parser = new DataParser();

		parser.AddDataSource(["one"]);
		parser.AddDataSource(["two"]);
		parser.AddDataSource(["three"]);
		parser.AddDataSource(["four"]);
		parser.AddDataSource(["five"]);
		parser.AddDataSource(["six"]);
		parser.AddDataSource(["seven"]);
		parser.AddDataSource(["eight"]);

		// Act
		var result1 = parser.GetNextDataItem(dummy);
		var result2 = parser.GetNextDataItem(dummy);
		parser.RestartAtLine(3);
		var result3 = parser.GetNextDataItem(dummy);
		var result4 = parser.GetNextDataItem(dummy);
		parser.RestartAtLine(3);
		var result5 = parser.GetNextDataItem(dummy);
		var result6 = parser.GetNextDataItem(dummy);
		var result7 = parser.GetNextDataItem(dummy);
		var result8 = parser.GetNextDataItem(dummy);
		parser.RestartAtLine(6);
		var result9 = parser.GetNextDataItem(dummy);
		parser.RestartAtLine(0);
		var result10 = parser.GetNextDataItem(dummy);
		parser.RestartAtLine(8);

		// Assert
		result1.Should().Be("one");
		result2.Should().Be("two");

		result3.Should().Be("four");
		result4.Should().Be("five");

		result5.Should().Be("four");
		result6.Should().Be("five");
		result7.Should().Be("six");
		result8.Should().Be("seven");

		result9.Should().Be("seven");

		result10.Should().Be("one");
	}

	[Test]
	public void GetNextDataItem_should_read_first_item()
	{
		// Arrange
		QBX.CodeModel.Statements.EmptyStatement dummy = new();

		var parser = new DataParser();

		parser.AddDataSource(["one", "two", "three"]);

		// Act
		var result = parser.GetNextDataItem(dummy);

		// Assert
		result.Should().Be("one");
	}

	[Test]
	public void GetNextDataItem_should_read_second_item_from_first_source()
	{
		// Arrange
		QBX.CodeModel.Statements.EmptyStatement dummy = new();

		var parser = new DataParser();

		parser.AddDataSource(["one", "two", "three"]);

		// Act
		parser.GetNextDataItem(dummy);
		var result = parser.GetNextDataItem(dummy);

		// Assert
		result.Should().Be("two");
	}

	[Test]
	public void GetNextDataItem_should_read_second_item_from_second_source()
	{
		// Arrange
		QBX.CodeModel.Statements.EmptyStatement dummy = new();

		var parser = new DataParser();

		parser.AddDataSource(["one"]);
		parser.AddDataSource(["two", "three"]);

		// Act
		parser.GetNextDataItem(dummy);
		var result = parser.GetNextDataItem(dummy);

		// Assert
		result.Should().Be("two");
	}

	[Test]
	public void GetNextDataItem_should_read_last_item()
	{
		// Arrange
		QBX.CodeModel.Statements.EmptyStatement dummy = new();

		var parser = new DataParser();

		parser.AddDataSource(["one"]);
		parser.AddDataSource(["two", "three"]);

		// Act
		parser.GetNextDataItem(dummy);
		parser.GetNextDataItem(dummy);
		var result = parser.GetNextDataItem(dummy);

		// Assert
		result.Should().Be("three");
	}

	[Test]
	public void IsAtStart_should_return_true_initially()
	{
		// Arrange
		var parser = new DataParser();

		parser.AddDataSource(["one"]);

		// Act
		var result = parser.IsAtStart;

		// Assert
		result.Should().BeTrue();
	}

	[Test]
	public void IsAtStart_should_return_false_after_reading()
	{
		// Arrange
		var dummy = new QBX.CodeModel.Statements.EmptyStatement();

		var parser = new DataParser();

		parser.AddDataSource(["one"]);

		// Act & Assert
		parser.GetNextDataItem(dummy);
		var result = parser.IsAtStart;

		// Assert
		result.Should().BeFalse();
	}

	[Test]
	public void IsAtStart_should_return_true_after_restarting()
	{
		// Arrange
		var dummy = new QBX.CodeModel.Statements.EmptyStatement();

		var parser = new DataParser();

		parser.AddDataSource(["one"]);

		// Act
		parser.GetNextDataItem(dummy);
		parser.Restart();

		var result = parser.IsAtStart;

		// Assert
		result.Should().BeTrue();
	}

	[Test]
	public void IsAtEnd_should_return_true_when_no_data_sources()
	{
		// Arrange
		var dummy = new QBX.CodeModel.Statements.EmptyStatement();

		var parser = new DataParser();

		// Act
		var result1 = parser.IsAtEnd;
		parser.Restart();
		var result2 = parser.IsAtEnd;

		// Assert
		result1.Should().BeTrue();
		result2.Should().BeTrue();
	}

	static StackFrame CreateDummyStackFrame(params Variable[] variables)
	{
		var dummyUnit = new QBX.CodeModel.CompilationUnit();

		var dummySource = new QBX.CodeModel.CompilationElement(dummyUnit);

		var dummyModule = new Module();

		var dummyRoutine = new Routine(dummyModule, dummySource);

		return new StackFrame(dummyRoutine, variables);
	}

	[Test]
	public void ReadDataItem_should_populate_variable_INTEGER()
	{
		// Arrange
		var targetVariable = new IntegerVariable(0);

		var stackFrame = CreateDummyStackFrame(targetVariable);

		var targetExpression = new IdentifierExpression(0, targetVariable.DataType);

		var machine = new Machine();

		var playProcessor = new PlayProcessor(machine);

		var executionContext = new ExecutionContext(machine, playProcessor);

		var parser = new DataParser();

		const int TestValue = 42;

		parser.AddDataSource([TestValue.ToString()]);

		// Act
		parser.ReadDataItem(
			targetExpression,
			executionContext,
			stackFrame);

		// Assert
		targetVariable.Value.Should().Be(TestValue);
	}

	[Test]
	public void ReadDataItem_should_populate_variable_LONG()
	{
		// Arrange
		var targetVariable = new LongVariable(0);

		var stackFrame = CreateDummyStackFrame(targetVariable);

		var targetExpression = new IdentifierExpression(0, targetVariable.DataType);

		var machine = new Machine();

		var playProcessor = new PlayProcessor(machine);

		var executionContext = new ExecutionContext(machine, playProcessor);

		var parser = new DataParser();

		const int TestValue = 42;

		parser.AddDataSource([TestValue.ToString()]);

		// Act
		parser.ReadDataItem(
			targetExpression,
			executionContext,
			stackFrame);

		// Assert
		targetVariable.Value.Should().Be(TestValue);
	}

	[Test]
	public void ReadDataItem_should_populate_variable_SINGLE()
	{
		// Arrange
		var targetVariable = new SingleVariable(0);

		var stackFrame = CreateDummyStackFrame(targetVariable);

		var targetExpression = new IdentifierExpression(0, targetVariable.DataType);

		var machine = new Machine();

		var playProcessor = new PlayProcessor(machine);

		var executionContext = new ExecutionContext(machine, playProcessor);

		var parser = new DataParser();

		const float TestValue = 234.567f;

		parser.AddDataSource([TestValue.ToString()]);

		// Act
		parser.ReadDataItem(
			targetExpression,
			executionContext,
			stackFrame);

		// Assert
		targetVariable.Value.Should().Be(TestValue);
	}

	[Test]
	public void ReadDataItem_should_populate_variable_DOUBLE()
	{
		// Arrange
		var targetVariable = new DoubleVariable(0);

		var stackFrame = CreateDummyStackFrame(targetVariable);

		var targetExpression = new IdentifierExpression(0, targetVariable.DataType);

		var machine = new Machine();

		var playProcessor = new PlayProcessor(machine);

		var executionContext = new ExecutionContext(machine, playProcessor);

		var parser = new DataParser();

		const double TestValue = 3.14159265358979d;

		parser.AddDataSource([TestValue.ToString()]);

		// Act
		parser.ReadDataItem(
			targetExpression,
			executionContext,
			stackFrame);

		// Assert
		targetVariable.Value.Should().Be(TestValue);
	}

	[Test]
	public void ReadDataItem_should_populate_variable_CURRENCY()
	{
		// Arrange
		var targetVariable = new CurrencyVariable(0);

		var stackFrame = CreateDummyStackFrame(targetVariable);

		var targetExpression = new IdentifierExpression(0, targetVariable.DataType);

		var machine = new Machine();

		var playProcessor = new PlayProcessor(machine);

		var executionContext = new ExecutionContext(machine, playProcessor);

		var parser = new DataParser();

		const decimal TestValue = 52833235912134.1234M;

		parser.AddDataSource([TestValue.ToString()]);

		// Act
		parser.ReadDataItem(
			targetExpression,
			executionContext,
			stackFrame);

		// Assert
		targetVariable.Value.Should().Be(TestValue);
	}

	[Test]
	public void ReadDataItem_should_populate_variable_STRING()
	{
		// Arrange
		var targetVariable = new StringVariable();

		var stackFrame = CreateDummyStackFrame(targetVariable);

		var targetExpression = new IdentifierExpression(0, targetVariable.DataType);

		var machine = new Machine();

		var playProcessor = new PlayProcessor(machine);

		var executionContext = new ExecutionContext(machine, playProcessor);

		var parser = new DataParser();

		StringValue TestValue = new StringValue("This... is a dream.");

		parser.AddDataSource([TestValue.ToString()]);

		// Act
		parser.ReadDataItem(
			targetExpression,
			executionContext,
			stackFrame);

		// Assert
		targetVariable.Value.Should().BeEquivalentTo(TestValue);
	}

	[Test]
	public void ReadDataItems_should_populate_sequence()
	{
		// Arrange
		var targetVariable1 = new IntegerVariable();
		var targetVariable2 = new StringVariable();
		var targetVariable3 = new DoubleVariable();
		var targetVariable4 = new CurrencyVariable();

		var stackFrame = CreateDummyStackFrame(targetVariable1, targetVariable2, targetVariable3, targetVariable4);

		var targetExpression1 = new IdentifierExpression(0, targetVariable1.DataType);
		var targetExpression2 = new IdentifierExpression(1, targetVariable1.DataType);
		var targetExpression3 = new IdentifierExpression(2, targetVariable1.DataType);
		var targetExpression4 = new IdentifierExpression(3, targetVariable1.DataType);

		Evaluable[] targetExpressions = [targetExpression1, targetExpression2, targetExpression3, targetExpression4];

		var machine = new Machine();

		var playProcessor = new PlayProcessor(machine);

		var executionContext = new ExecutionContext(machine, playProcessor);

		var parser = new DataParser();

		const int TestValue1 = 1;
		StringValue TestValue2 = new StringValue("two");
		const double TestValue3 = Math.E;
		const decimal TestValue4 = 6.95M;

		parser.AddDataSource([TestValue1.ToString()]);
		parser.AddDataSource([TestValue2.ToString(), TestValue3.ToString()]);
		parser.AddDataSource([TestValue4.ToString()]);

		// Act
		parser.ReadDataItems(
			targetExpressions,
			executionContext,
			stackFrame);

		// Assert
		targetVariable1.Value.Should().Be(TestValue1);
		targetVariable2.Value.Should().BeEquivalentTo(TestValue2);
		targetVariable3.Value.Should().Be(TestValue3);
		targetVariable4.Value.Should().Be(TestValue4);
	}

	[TestCase("", new string[] { "" })]
	[TestCase("         ", new string[] { "" })]
	[TestCase(",", new string[] { "", "" })]
	[TestCase("(", new string[] { "(" })]
	[TestCase("Mary had a little lamb", new string[] { "Mary had a little lamb" })]
	[TestCase("\"Mary had a little lamb\"", new string[] { "Mary had a little lamb" })]
	[TestCase("Wherever you go, there you are", new string[] { "Wherever you go", "there you are" })]
	[TestCase("\"Wherever you go, there you are\"", new string[] { "Wherever you go, there you are" })]
	[TestCase("3.14159265358979,2.71828182845905,meaning of life,42", new string[] { "3.14159265358979", "2.71828182845905", "meaning of life", "42" })]
	[TestCase("   3.14159265358979  , 2.71828182845905  ,   meaning of life    ,   42  ", new string[] { "3.14159265358979", "2.71828182845905", "meaning of life", "42" })]
	public void ParseDataItems_should_parse_raw_string_correctly(string rawString, string[] expectedDataItems)
	{
		// Act
		var result = DataParser.ParseDataItems(rawString).ToList();

		// Assert
		result.Should().BeEquivalentTo(expectedDataItems);
	}
}

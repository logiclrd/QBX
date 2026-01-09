using System;
using System.Collections.Generic;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;
using QBX.Firmware;
using QBX.Numbers;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class FormattedPrintStatement(CodeModel.Statements.Statement? source) : Statement(source)
{
	public IEvaluable? Format;
	public List<PrintArgument> Arguments = new List<PrintArgument>();
	public bool EmitNewLine = true;

	public CodeModel.Statements.PrintStatement? Statement;

	[ThreadStatic]
	static string? s_spaces;

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		if (Format == null)
			throw new Exception("FormattedPrintStatement does not have a Format expression");

		var formatVariable = (StringVariable)Format.Evaluate(context, stackFrame);
		string format = formatVariable.Value;

		int nextArgumentIndex = 0;

		var dummyVariable = formatVariable;

		while (nextArgumentIndex < Arguments.Count)
		{
			FormatDirective.SplitString(
				format, Statement,
				directive =>
				{
					Variable argumentValue = dummyVariable;

					if (directive.UsesArgument)
					{
						while (true)
						{
							if (nextArgumentIndex >= Arguments.Count)
								return false; // break out of loop

							var argument = Arguments[nextArgumentIndex++];

							if (argument.Expression == null)
								throw new Exception("PrintArgument with no Expression");

							if (argument.ArgumentType == PrintArgumentType.Value)
							{
								argumentValue = argument.Expression.Evaluate(context, stackFrame);

								break;
							}

							int newCursorX = argument.Expression.Evaluate(context, stackFrame).CoerceToInt();

							newCursorX = (newCursorX - 1) % context.VisualLibrary.CharacterWidth;

							if (newCursorX < context.VisualLibrary.CursorX)
								context.VisualLibrary.NewLine();

							if (argument.ArgumentType == PrintArgumentType.Tab)
								context.VisualLibrary.MoveCursor(newCursorX, context.VisualLibrary.CursorY);
							else
							{
								if ((s_spaces == null) || (s_spaces.Length < newCursorX))
									s_spaces = new string(' ', newCursorX * 2);

								context.VisualLibrary.WriteText(s_spaces, 0, newCursorX);
							}
						}
					}

					directive.Emit(argumentValue, context, Statement);

					return true; // continue loop
				});

			if (nextArgumentIndex == 0)
				throw RuntimeException.IllegalFunctionCall(Statement);
		}

		if (EmitNewLine)
			context.VisualLibrary.NewLine();
	}

	class Emitter(ExecutionContext context)
	{
		VisualLibrary _visual = context.VisualLibrary;

		public void NextLine()
		{
			_visual.NewLine();
		}

		public const string Zone = "              "; // 14 characters

		public void NextZone()
		{
			int currentZoneStart = Zone.Length * (_visual.CursorX / Zone.Length);
			int nextZoneStart = currentZoneStart + Zone.Length;

			if (nextZoneStart >= _visual.Width)
				_visual.NewLine();
			else
			{
				int offset = _visual.CursorX - currentZoneStart;

				_visual.WriteText(Zone.AsSpan().Slice(offset));
			}
		}

		public void Emit(Variable value)
		{
			switch (value)
			{
				case IntegerVariable integerValue: Emit(integerValue.Value); break;
				case LongVariable longValue: Emit(longValue.Value); break;
				case SingleVariable singleValue: Emit(singleValue.Value); break;
				case DoubleVariable doubleValue: Emit(doubleValue.Value); break;
				case CurrencyVariable currencyValue: Emit(currencyValue.Value); break;
				case StringVariable stringValue: Emit(stringValue.Value); break;
			}
		}

		public void Emit(char ch) => _visual.WriteText(ch);
		public void Emit(string str) => _visual.WriteText(str);

		public void Emit(short integerValue)
		{
			if (integerValue < 0)
				Emit(' ');
			else
				Emit('-');

			Emit(integerValue.ToString());

			Emit(' ');
		}

		public void Emit(int longValue)
		{
			if (longValue < 0)
				Emit(' ');
			else
				Emit('-');

			Emit(longValue.ToString());

			Emit(' ');
		}

		public void Emit(float singleValue)
		{
			if (singleValue >= 0)
				Emit(' ');

			Emit(NumberFormatter.Format(singleValue, qualify: false));
			Emit(' ');
		}

		public void Emit(double doubleValue)
		{
			if (doubleValue >= 0)
				Emit(' ');

			Emit(NumberFormatter.Format(doubleValue, qualify: false));
			Emit(' ');
		}

		public void Emit(decimal currencyValue)
		{
			if (!currencyValue.IsInCurrencyRange())
				throw new Exception("Internal error: Currency value is out of range");

			string formatted = currencyValue.ToString("###############.####");

			if (formatted == "")
				Emit(" 0 ");
			else
			{
				Emit(' ');
				Emit(formatted);
				Emit(' ');
			}
		}
	}
}

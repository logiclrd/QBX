using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

using QBX.ExecutionEngine.Compiled;
using QBX.ExecutionEngine.Compiled.BitwiseOperators;
using QBX.ExecutionEngine.Compiled.Expressions;
using QBX.ExecutionEngine.Compiled.Functions;
using QBX.ExecutionEngine.Compiled.Operations;
using QBX.ExecutionEngine.Compiled.RelationalOperators;
using QBX.ExecutionEngine.Compiled.Statements;
using QBX.ExecutionEngine.Execution;

using QBX.LexicalAnalysis;

using QBX.Numbers;
using QBX.Utility;

namespace QBX.ExecutionEngine;

public class Compiler
{
	public bool DetectDelayLoops { get; set; }

	public Module Compile(CodeModel.CompilationUnit unit, Compilation compilation)
	{
		var module = new Module();

		Mapper? rootMapper = null;

		var routines = new List<Routine>();

		// First pass: collect all routines
		foreach (var element in unit.Elements)
		{
			var routine = new Routine(module, rootMapper, element);

			if (compilation.IsRegistered(routine.Name))
				throw CompilerException.DuplicateDefinition(element.AllStatements.FirstOrDefault());

			if (rootMapper == null)
			{
				if (routine.Name != Routine.MainRoutineName)
					throw new Exception("First routine is not the main routine");

				rootMapper = routine.Mapper;
			}

			if (routine.Name == Routine.MainRoutineName)
				module.MainRoutine = routine;
			else
				routine.Register(compilation);

			if (routine.OpeningStatement is not null)
			{
				routine.ApplyOpeningDefTypeStatements(routine.Mapper);

				if (routine.OpeningStatement is CodeModel.Statements.FunctionStatement)
					routine.SetReturnType(routine.Mapper, compilation.TypeRepository);
			}

			routines.Add(routine);
		}

		if (rootMapper == null)
			throw new Exception("CompilationUnit does not have any CompilationElements");

		// Second pass: process all TYPE definitions
		CodeModel.Statements.TypeStatement? typeStatement = null;
		var typeElementStatements = new List<CodeModel.Statements.TypeElementStatement>();

		foreach (var statement in unit.Elements[0].AllStatements)
		{
			if (typeStatement == null)
				typeStatement = statement as CodeModel.Statements.TypeStatement;
			else
			{
				switch (statement)
				{
					case CodeModel.Statements.TypeStatement:
						throw CompilerException.TypeWithoutEndType(typeStatement);

					case CodeModel.Statements.TypeElementStatement typeElementStatement:
						typeElementStatements.Add(typeElementStatement);
						break;

					case CodeModel.Statements.EndTypeStatement:
						TranslateTypeDefinition(typeStatement, typeElementStatements, rootMapper, compilation);

						typeStatement = null;
						typeElementStatements.Clear();

						break;
				}
			}
		}

		if (typeStatement != null)
			throw CompilerException.TypeWithoutEndType(typeStatement);

		// Third pass: process parameters, which requires that we know all the FUNCTIONs and UDTs.
		// We can also match up forward references.
		foreach (var routine in routines)
		{
			if (routine.ReturnType != null)
				routine.ReturnValueVariableIndex = routine.Mapper.DeclareVariable(routine.Name, routine.ReturnType);

			if (routine.Source.Type != CodeModel.CompilationElementType.Main)
				routine.TranslateParameters(routine.Mapper, compilation);

			string unqualifiedName = Mapper.UnqualifyIdentifier(routine.Name);

			if (compilation.UnresolvedReferences.TryGetDeclaration(unqualifiedName, out var forwardReference))
			{
				routine.ValidateDeclaration(
					forwardReference.ParameterTypes,
					forwardReference.ReturnType,
					routine.OpeningStatement,
					routine.OpeningStatement?.NameToken,
					getBlameParameterType: i => routine.OpeningStatement?.Parameters?.Parameters[i].TypeToken);
			}
		}

		// Fourth pass: collect line numbers for error reporting.
		foreach (var element in unit.Elements)
		{
			// BC doesn't reset this for each new element, but QBX does
			int lineNumberForReporting = 0;

			foreach (var line in element.Lines)
			{
				if ((line.LineNumber != null)
				 && int.TryParse(line.LineNumber, out var parsedLineNumber)
				 && (parsedLineNumber <= 65529)) // Observed in QuickBASIC 7.1
					lineNumberForReporting = parsedLineNumber;

				foreach (var statement in line.Statements)
					statement.LineNumberForErrorReporting = lineNumberForReporting;
			}
		}

		// Fifth pass: Collect constants and then translate statements.
		// => CONST definitions inside DEF FN are local to the DEF FN and are not processed here
		foreach (var routine in routines)
		{
			if (routine.Source.Type != CodeModel.CompilationElementType.Main)
				routine.Mapper.LinkGlobalVariablesAndArrays();

			var element = routine.Source;

			var mapper = routine.Mapper;

			mapper.ScanForDisallowedSlugs(element.AllStatements);

			bool inDefFn = false;

			mapper.PushIdentifierTypes();

			foreach (var statement in element.AllStatements)
			{
				switch (statement)
				{
					case CodeModel.Statements.DefFnStatement: inDefFn = true; break;
					case CodeModel.Statements.EndDefStatement: inDefFn = false; break;

					case CodeModel.Statements.DefTypeStatement defTypeStatement:
						mapper.ApplyDefTypeStatement(defTypeStatement);
						break;

					case CodeModel.Statements.ConstStatement constStatement:
						if (!inDefFn)
						{
							foreach (var definition in constStatement.Definitions)
							{
								var constValueExpression = TranslateExpression(definition.Value, container: null, mapper, compilation);

								var targetType = mapper.GetTypeForIdentifier(definition.Identifier);

								if (constValueExpression.Type.IsPrimitiveType
								 && (constValueExpression.Type.PrimitiveType != targetType))
								{
									constValueExpression =
										Conversion.Construct(constValueExpression, targetType);
								}

								mapper.DefineConstant(
									definition.Identifier,
									constValueExpression.EvaluateConstant());
							}
						}

						break;
				}
			}

			mapper.PopIdentifierTypes();

			int lineIndex = 0;
			int statementIndex = 0;

			if (routine.Source.Type != CodeModel.CompilationElementType.Main)
			{
				// Skip to the body of function.
				while (lineIndex < element.Lines.Count)
				{
					var line = element.Lines[lineIndex];

					if (line.Statements.FirstOrDefault() is CodeModel.Statements.ProperSubroutineOpeningStatement)
					{
						statementIndex++;
						break;
					}

					foreach (var defTypeStatement in line.Statements.OfType<CodeModel.Statements.DefTypeStatement>())
						mapper.ApplyDefTypeStatement(defTypeStatement);

					lineIndex++;
				}
			}

			while (lineIndex < element.Lines.Count)
				TranslateStatement(element, ref lineIndex, ref statementIndex, routine, routine, compilation, module);

			routine.VariableTypes = mapper.GetVariableTypes();
			routine.LinkedVariables = mapper.GetLinkedVariables();

			routine.ResolveJumpStatements();

			foreach (var statement in routine.AllStatements)
				if (statement is IUnresolvedLineReference unresolvedLineReference)
					unresolvedLineReference.Resolve(routine);

			routine.Mapper.Freeze();
		}

		compilation.Modules.Add(module);

		return module;
	}

	public Evaluable CompileExpression(CodeModel.Expressions.Expression expression, Mapper mapper, Compilation compilation)
	{
		var dummyContainer = new Sequence();

		return TranslateExpressionUncollapsed(
			expression,
			forAssignment: false,
			dummyContainer,
			mapper,
			compilation);
	}

	public void ProcessCommentForDirectives(string commentText, Compilation compilation)
	{
		var commentSpan = commentText.AsSpan();

		bool IsSpace(char ch) => (ch == ' ') || (ch == '\t');

		while ((commentSpan.Length > 0) && IsSpace(commentSpan[0]))
			commentSpan = commentSpan.Slice(1);

		// Metacommand comments must start with $.
		if ((commentSpan.Length == 0) || (commentSpan[0] != '$'))
			return;

		int directiveIndex = commentSpan.IndexOf('$');

		while (directiveIndex >= 0)
		{
			commentSpan = commentSpan.Slice(directiveIndex);

			int directiveEnd = 1;

			while ((directiveEnd < commentSpan.Length) && char.IsAsciiLetterOrDigit(commentSpan[directiveEnd]))
				directiveEnd++;

			var directive = commentSpan.Slice(0, directiveEnd);

			commentSpan = commentSpan.Slice(directiveEnd);

			if (directive.Equals("$STATIC", StringComparison.OrdinalIgnoreCase))
				compilation.UseStaticArrays = true;
			else if (directive.Equals("$DYNAMIC", StringComparison.OrdinalIgnoreCase))
				compilation.UseStaticArrays = false;
			else if (directive.Equals("$DIRECT", StringComparison.OrdinalIgnoreCase))
				compilation.UseDirectMarshalling = true;
			else if (directive.Equals("$INDIRECT", StringComparison.OrdinalIgnoreCase))
				compilation.UseDirectMarshalling = false;
			else if (directive.Equals("$INCLUDE", StringComparison.OrdinalIgnoreCase))
			{
				// TODO: include file

				break; // $INCLUDE directives consume to the end of the line
			}

			directiveIndex = commentSpan.IndexOf('$');
		}
	}

	void TranslateTypeDefinition(CodeModel.Statements.TypeStatement typeStatement, List<CodeModel.Statements.TypeElementStatement> elements, Mapper mapper, Compilation compilation)
	{
		var typeRepository = compilation.TypeRepository;

		var udt = new UserDataType(typeStatement);

		foreach (var typeElementStatement in elements)
		{
			var type = typeRepository.ResolveType(
				typeElementStatement.ElementType,
				typeElementStatement.ElementUserType,
				typeElementStatement.FixedStringLength,
				isArray: false,
				typeElementStatement.TypeToken);

			ArraySubscripts? translatedSubscripts = null;

			if (typeElementStatement.Subscripts != null)
			{
				translatedSubscripts = new ArraySubscripts();

				foreach (var subscript in typeElementStatement.Subscripts.Subscripts)
				{
					var translatedSubscript = new ArraySubscript();

					var translatedBound1 = TranslateExpression(subscript.Bound1, container: null, mapper, compilation);
					var translatedBound2 = TranslateExpression(subscript.Bound2, container: null, mapper, compilation);

					if (translatedBound1 == null)
						throw new Exception("Internal error: Array subscript missing bound");

					if (translatedBound2 == null)
					{
						translatedSubscript.LowerBound = 0;
						translatedSubscript.UpperBound = NumberConverter.ToInteger(translatedBound1.EvaluateConstant());
					}
					else
					{
						translatedSubscript.LowerBound = NumberConverter.ToInteger(translatedBound1.EvaluateConstant());
						translatedSubscript.UpperBound = NumberConverter.ToInteger(translatedBound2.EvaluateConstant());
					}

					translatedSubscripts.Subscripts.Add(translatedSubscript);
				}
			}

			udt.Fields.Add(
				new UserDataTypeField(
					typeElementStatement.Name,
					type,
					translatedSubscripts));
		}

		typeRepository.RegisterType(udt);
	}

	void TranslateStatement(CodeModel.CompilationElement element, ref int lineIndexRef, ref int statementIndexRef, Sequence container, Routine routine, Compilation compilation, Module module)
	{
		int lineIndex = lineIndexRef;
		int statementIndex = statementIndexRef;

		if (lineIndex >= element.Lines.Count)
			return;

		var line = element.Lines[lineIndex];

		if (statementIndex >= line.Statements.Count)
		{
			line.SourceLineIndex ??= new MutableBox<int>(lineIndex); // Just in case there are no statements on this line
			lineIndexRef = lineIndex + 1;
			statementIndexRef = 0;
			return;
		}

		try
		{
			var statement = line.Statements[statementIndex];

			if (statement.FirstToken != null) // should always be true
				line.SourceLineIndex = statement.FirstToken.LineNumberBox;

			if (statementIndex == 0)
			{
				if (line.LineNumber != null)
				{
					var labelStatement = new LabelStatement(line.LineNumber, statement);

					module.DataParser.AddLabel(labelStatement);
					container.Append(labelStatement);
				}

				if (line.Label != null)
				{
					var labelStatement = new LabelStatement(line.Label.Name, statement);

					module.DataParser.AddLabel(labelStatement);
					container.Append(labelStatement);
				}
			}

			var iterator = new CompilationElementStatementIterator(
				element,
				line,
				lineIndex,
				statementIndex);

			iterator.Comment +=
				(commentText) =>
				{
					ProcessCommentForDirectives(commentText, compilation);
				};

			iterator.Advanced +=
				(newStatement) =>
				{
					statement = newStatement;
				};

			TranslateStatement(element, ref statement, iterator, container, routine, compilation, module, out var nextStatementInfo);

			lineIndex = iterator.LineIndex;
			statementIndex = iterator.StatementIndex;

			if (nextStatementInfo != null)
			{
				throw CompilerException.NextWithoutFor(
					nextStatementInfo.Statement.CounterExpressions[nextStatementInfo.LoopsMatched].Token);
			}
		}
		finally
		{
			lineIndexRef = lineIndex;
			statementIndexRef = statementIndex;
		}
	}

	void TranslateStatement(CodeModel.CompilationElement element, IList<CodeModel.Statements.Statement> statements, ref int statementIndexRef, Sequence container, Routine routine, Compilation compilation, Module module)
	{
		int statementIndex = statementIndexRef;

		if (statementIndex >= statements.Count)
			return;

		try
		{
			var statement = statements[statementIndex];

			var iterator = new ListStatementIterator(statements, statementIndex);

			iterator.Advanced +=
				newStatement =>
				{
					statement = newStatement;
				};

			TranslateStatement(element, ref statement, iterator, container, routine, compilation, module, out var nextStatementInfo);

			statementIndex = iterator.StatementIndex;

			if (nextStatementInfo != null)
			{
				throw CompilerException.NextWithoutFor(
					nextStatementInfo.Statement.CounterExpressions[nextStatementInfo.LoopsMatched].Token);
			}
		}
		finally
		{
			statementIndexRef = statementIndex;
		}
	}

	class NextStatementInfo(CodeModel.Statements.NextStatement statement)
	{
		public CodeModel.Statements.NextStatement Statement = statement;
		public int LoopsMatched = 0;
	}

	void TranslateStatement(CodeModel.CompilationElement element, ref CodeModel.Statements.Statement statement, StatementIterator iterator, Sequence container, Routine routine, Compilation compilation, Module module, out NextStatementInfo? nextStatementInfo)
	{
		var mapper = routine.Mapper;

		var typeRepository = compilation.TypeRepository;

		nextStatementInfo = null;

		void TranslateNumericArgumentExpression(ref Evaluable? target, CodeModel.Expressions.Expression? expression)
		{
			if (expression != null)
			{
				target = TranslateExpression(expression, container, mapper, compilation);

				if (!target.Type.IsNumeric)
					throw CompilerException.TypeMismatch(expression?.Token);
			}
		}

		void TranslateStringArgumentExpression(ref Evaluable? target, CodeModel.Expressions.Expression? expression)
		{
			if (expression != null)
			{
				target = TranslateExpression(expression, container, mapper, compilation);

				if (!target.Type.IsString)
					throw CompilerException.TypeMismatch(expression?.Token);
			}
		}

		switch (statement)
		{
			case CodeModel.Statements.AssignmentStatement assignmentStatement:
			{
				var targetExpression = TranslateExpression(assignmentStatement.TargetExpression, forAssignment: true, container, mapper, compilation);
				var valueExpression = TranslateExpression(assignmentStatement.ValueExpression, container, mapper, compilation);

				if (targetExpression == null)
					throw new BadModelException("AssignmentStatement with no TargetExpression");
				if (valueExpression == null)
					throw new BadModelException("AssignmentStatement with no ValueExpression");

				if (targetExpression.Type != valueExpression.Type)
				{
					if (targetExpression.Type.IsString != valueExpression.Type.IsString)
						throw CompilerException.TypeMismatch(assignmentStatement.ValueExpression?.Token);
					if (!targetExpression.Type.IsPrimitiveType
					 || !valueExpression.Type.IsPrimitiveType)
						throw CompilerException.TypeMismatch(assignmentStatement.ValueExpression?.Token);

					valueExpression = Conversion.Construct(valueExpression, targetExpression.Type.PrimitiveType);
				}

				var translatedAssignmentStatement = new AssignmentStatement(assignmentStatement);

				translatedAssignmentStatement.TargetExpression = targetExpression;
				translatedAssignmentStatement.ValueExpression = valueExpression;

				container.Append(translatedAssignmentStatement);

				break;
			}
			case CodeModel.Statements.BeepStatement beepStatement:
			{
				var translatedBeepStatement = new BeepStatement(beepStatement);

				container.Append(translatedBeepStatement);

				break;
			}
			case CodeModel.Statements.CallStatement callStatement:
			{
				if (compilation.TryGetNativeProcedure(callStatement.TargetName, out var nativeProcedure))
				{
					var translatedCallStatement = new NativeProcedureCallStatement(callStatement);

					if (nativeProcedure.ParameterTypes == null)
					{
						if (callStatement.Arguments != null)
						{
							foreach (var argument in callStatement.Arguments.Expressions)
							{
								var translatedExpression = TranslateExpression(argument, container, mapper, compilation);

								if (translatedExpression == null)
									throw new Exception("Call argument translated to null");

								translatedCallStatement.Arguments.Add(translatedExpression);
							}
						}

						translatedCallStatement.LocalThunk = nativeProcedure.BuildThunk(
							translatedCallStatement.Arguments.Select(arg => arg.Type).ToList(),
							compilation.UseDirectMarshalling);
					}
					else
					{
						int callArgumentCount = callStatement.Arguments?.Count ?? 0;
						int targetArgumentCount = nativeProcedure.ParameterTypes?.Length ?? 0;

						if (callArgumentCount != targetArgumentCount)
							throw CompilerException.ArgumentCountMismatch(callStatement.FirstToken);

						translatedCallStatement.Target = nativeProcedure;

						if (callStatement.Arguments != null)
						{
							foreach (var argument in callStatement.Arguments.Expressions)
							{
								var translatedExpression = TranslateExpression(argument, container, mapper, compilation);

								if (translatedExpression == null)
									throw new Exception("Call argument translated to null");

								translatedCallStatement.Arguments.Add(translatedExpression);
							}

							translatedCallStatement.EnsureParameterTypes();
						}
					}

					container.Append(translatedCallStatement);
				}
				else
				{
					var translatedCallStatement = new CallStatement(callStatement);

					if (compilation.TryGetSub(callStatement.TargetName, out var sub))
					{
						int callArgumentCount = callStatement.Arguments?.Count ?? 0;

						if (callArgumentCount != sub.ParameterTypes.Count)
							throw CompilerException.ArgumentCountMismatch(callStatement.FirstToken);

						translatedCallStatement.Target = sub;
					}
					else if (compilation.TryGetFunction(callStatement.TargetName, out var function))
						throw CompilerException.DuplicateDefinition(callStatement);
					else if (compilation.UnresolvedReferences.TryGetDeclaration(callStatement.TargetName, out var forwardReference))
					{
						if (forwardReference.RoutineType != RoutineType.Sub)
							throw CompilerException.DuplicateDefinition(callStatement);

						translatedCallStatement.UnresolvedTargetName = callStatement.TargetName;
						forwardReference.UnresolvedCalls.Add(translatedCallStatement);
					}

					if (callStatement.Arguments != null)
					{
						foreach (var argument in callStatement.Arguments.Expressions)
						{
							var translatedExpression = TranslateExpression(argument, container, mapper, compilation);

							if (translatedExpression == null)
								throw new Exception("Call argument translated to null");

							translatedCallStatement.Arguments.Add(translatedExpression);
						}

						if (translatedCallStatement.Target != null)
							translatedCallStatement.EnsureParameterTypes();
					}

					container.Append(translatedCallStatement);
				}

				break;
			}
			case CodeModel.Statements.CircleStatement circleStatement:
			{
				var translatedCircleStatement = new CircleStatement(circleStatement);

				if (circleStatement.XExpression == null)
					throw new Exception("CircleStatement with no XExpression");
				if (circleStatement.YExpression == null)
					throw new Exception("CircleStatement with no YExpression");
				if (circleStatement.RadiusExpression == null)
					throw new Exception("CircleStatement with no RadiusExpression");

				TranslateNumericArgumentExpression(
					ref translatedCircleStatement.XExpression, circleStatement.XExpression);
				TranslateNumericArgumentExpression(
					ref translatedCircleStatement.YExpression, circleStatement.YExpression);
				TranslateNumericArgumentExpression(
					ref translatedCircleStatement.RadiusExpression, circleStatement.RadiusExpression);

				TranslateNumericArgumentExpression(
					ref translatedCircleStatement.ColourExpression, circleStatement.ColourExpression);
				TranslateNumericArgumentExpression(
					ref translatedCircleStatement.StartExpression, circleStatement.StartExpression);
				TranslateNumericArgumentExpression(
					ref translatedCircleStatement.EndExpression, circleStatement.EndExpression);
				TranslateNumericArgumentExpression(
					ref translatedCircleStatement.AspectExpression, circleStatement.AspectExpression);

				container.Append(translatedCircleStatement);

				break;
			}
			case CodeModel.Statements.ClearStatement clearStatement:
			{
				if (!routine.IsMainRoutine)
					throw CompilerException.IllegalInSubFunctionOrDefFn(clearStatement);

				var translatedClearStatement = new ClearStatement(clearStatement);

				TranslateNumericArgumentExpression(
					ref translatedClearStatement.StringSpaceExpression, clearStatement.StringSpaceExpression);
				TranslateNumericArgumentExpression(
					ref translatedClearStatement.MaximumMemoryAddressExpression, clearStatement.MaximumMemoryAddressExpression);
				TranslateNumericArgumentExpression(
					ref translatedClearStatement.StackSpaceExpression, clearStatement.StackSpaceExpression);

				container.Append(translatedClearStatement);

				break;
			}
			case CodeModel.Statements.ClsStatement clsStatement:
			{
				var translatedClsStatement = new ClsStatement(clsStatement);

				TranslateNumericArgumentExpression(
					ref translatedClsStatement.ArgumentExpression, clsStatement.Mode);

				container.Append(translatedClsStatement);

				break;
			}
			case CodeModel.Statements.ColorStatement colorStatement:
			{
				var translatedColorStatement = new ColorStatement(colorStatement);

				var argument1 = colorStatement.Arguments.Count > 0 ? colorStatement.Arguments[0] : null;
				var argument2 = colorStatement.Arguments.Count > 1 ? colorStatement.Arguments[1] : null;
				var argument3 = colorStatement.Arguments.Count > 2 ? colorStatement.Arguments[2] : null;

				translatedColorStatement.Argument1Expression = TranslateExpression(argument1, container, mapper, compilation);
				translatedColorStatement.Argument2Expression = TranslateExpression(argument2, container, mapper, compilation);
				translatedColorStatement.Argument3Expression = TranslateExpression(argument3, container, mapper, compilation);

				container.Append(translatedColorStatement);

				break;
			}
			case CodeModel.Statements.CommentStatement commentStatement:
			{
				ProcessCommentForDirectives(commentStatement.Comment, compilation);
				break;
			}
			case CodeModel.Statements.ConstStatement constStatement:
			{
				// Gathered centrally before main translation begins.
				break;
			}
			case CodeModel.Statements.DataStatement dataStatement:
			{
				if (dataStatement.RawString is null)
					throw new Exception("DataStatement with no RawString");

				var dataSource = DataParser.ParseDataItems(dataStatement.RawString);

				module.DataParser.AddDataSource(dataSource);

				break;
			}
			case CodeModel.Statements.DeclareStatement declareStatement:
			{
				// If the name matches a registered NativeProcedure, set up the argument
				// and return types and generate the thunk.
				//
				// If the declared routine is already known, verify that the parameters
				// and type match.
				//
				// If the declared routine is not known, then record the fact that the
				// routine should exist so that we know we can generate
				// UnresolvedCallStatements and UnresolvedFunctionCalls to be linked
				// up later.

				if ((declareStatement.Parameters != null)
				 && declareStatement.Parameters.Parameters.Any(definition => definition.AnyType))
				{
					string unqualifiedName = Mapper.UnqualifyIdentifier(declareStatement.Name);

					if (compilation.TryGetNativeProcedure(unqualifiedName, out var nativeProcedure)
					 || !compilation.TryGetRoutine(unqualifiedName, out _))
						throw CompilerException.AnyIsNotSupported(declareStatement);
				}
				else
				{
					var parameterTypes = declareStatement.Parameters?.Parameters
						.Select(parameterDefinition => compilation.TypeRepository.ResolveType(parameterDefinition, mapper))
						.ToList();

					DataType? returnType = null;

					if (declareStatement.DeclarationType.Type == TokenType.FUNCTION)
					{
						if (declareStatement.TypeCharacter != null)
							returnType = DataType.FromCodeModelDataType(
								declareStatement.TypeCharacter.Type);
						else
							returnType = DataType.ForPrimitiveDataType(
								mapper.GetTypeForIdentifier(declareStatement.Name));
					}

					string unqualifiedName = Mapper.UnqualifyIdentifier(declareStatement.Name);

					if (compilation.TryGetNativeProcedure(unqualifiedName, out var nativeProcedure))
					{
						if (nativeProcedure.ParameterTypes != null)
							throw CompilerException.DuplicateDefinition(declareStatement);

						nativeProcedure.ParameterTypes = parameterTypes?.ToArray() ?? System.Array.Empty<DataType>();
						nativeProcedure.ReturnType = returnType;

						nativeProcedure.BuildThunk(compilation.UseDirectMarshalling);
					}
					else if (compilation.TryGetRoutine(unqualifiedName, out var declaredRoutine))
					{
						declaredRoutine.ValidateDeclaration(
							parameterTypes,
							returnType,
							blameStatement: declareStatement,
							blameName: declareStatement.NameToken,
							getBlameParameterType: i => declareStatement.Parameters?.Parameters[i].TypeToken);
					}
					else
					{
						compilation.UnresolvedReferences.DeclareSymbol(
							declareStatement.Name,
							mapper,
							declareStatement,
							declareStatement.DeclarationType.Type switch
							{
								TokenType.SUB => RoutineType.Sub,
								TokenType.FUNCTION => RoutineType.Function,

								_ => throw new Exception("Unrecognized DeclarationType " + declareStatement.DeclarationType)
							},
							parameterTypes?.ToArray() ?? System.Array.Empty<DataType>(),
							returnType);
					}
				}

				break;
			}
			case CodeModel.Statements.DefFnStatement defFnStatement:
			{
				// One of:
				//
				//   DEF FNspoon% (a%) = a% * 2
				//
				//   DEF FNfork (x%)
				//     FNfork = SQR(x%)
				//   END DEF

				var defFnRoutine = new Routine(module, mapper, element, defFnStatement, typeRepository);

				if (defFnRoutine.ReturnType == null)
				{
					defFnRoutine.ReturnType = DataType.ForPrimitiveDataType(
						mapper.GetTypeForIdentifier(defFnRoutine.Name));
				}

				mapper.StartSemiscopeSetup();

				string qualifiedName = mapper.QualifyIdentifier(
					defFnRoutine.Name,
					defFnRoutine.ReturnType);

				defFnRoutine.ReturnValueVariableIndex = mapper.DeclareVariable(
					qualifiedName,
					defFnRoutine.ReturnType);

				defFnRoutine.TranslateParameters(mapper, compilation);

				mapper.EnterSemiscope();

				try
				{
					if (defFnStatement.ExpressionBody != null)
					{
						defFnRoutine.Append(
							new AssignmentStatement(defFnStatement)
							{
								TargetExpression =
									new IdentifierExpression(defFnRoutine.ReturnValueVariableIndex, defFnRoutine.ReturnType),

								ValueExpression =
									TranslateExpression(defFnStatement.ExpressionBody, container, mapper, compilation),
							});
					}
					else
					{
						iterator.Advance();
						iterator.ProcessLabels(module.DataParser, defFnRoutine);

						while (iterator.HaveCurrentStatement)
						{
							if (statement is CodeModel.Statements.EndDefStatement)
								break;

							if (statement is CodeModel.Statements.DefFnStatement)
								throw CompilerException.IllegalInSubFunctionOrDefFn(statement);

							TranslateStatement(element, ref statement, iterator, defFnRoutine, defFnRoutine, compilation, module, out nextStatementInfo);

							if (nextStatementInfo != null)
							{
								throw CompilerException.NextWithoutFor(
									nextStatementInfo.Statement.CounterExpressions[nextStatementInfo.LoopsMatched].Token);
							}

							iterator.Advance();
							iterator.ProcessLabels(module.DataParser, defFnRoutine);
						}
					}
				}
				finally
				{
					mapper.ExitSemiscope();
				}

				defFnRoutine.UseRootFrame = true;

				compilation.RegisterFunction(defFnRoutine);

				break;
			}
			case CodeModel.Statements.DefSegStatement defSegStatement:
			{
				var translatedDefSegStatement = new DefSegStatement(defSegStatement);

				translatedDefSegStatement.SegmentExpression =
					TranslateExpression(defSegStatement.SegmentExpression, container, mapper, compilation);

				container.Append(translatedDefSegStatement);

				break;
			}
			case CodeModel.Statements.DefTypeStatement defTypeStatement:
			{
				mapper.ApplyDefTypeStatement(defTypeStatement);
				break;
			}
			case CodeModel.Statements.DimStatement dimStatement: // also matches RedimStatement
			{
				if (dimStatement.Shared && (element.Type != CodeModel.CompilationElementType.Main))
					throw CompilerException.IllegalInSubFunctionOrDefFn(statement);

				foreach (var declaration in dimStatement.Declarations)
				{
					DataType dataType;

					// TODO: it needs to be possible to DIM dotted identifiers

					if (declaration.UserType != null)
						dataType = compilation.TypeRepository.ResolveType(declaration.UserType);
					else if (declaration.Type != null)
						dataType = DataType.FromCodeModelDataType(declaration.Type.Value);
					else
						dataType = DataType.ForPrimitiveDataType(mapper.GetTypeForIdentifier(declaration.Name));

					int variableIndex;

					if (declaration.Subscripts == null)
					{
						if (!dimStatement.DeclareScalars)
							throw new Exception("Internal error: DimStatement that does not declare scalars with a Declaration with no Subscripts");

						variableIndex = mapper.DeclareVariable(declaration.Name, dataType);

						if (dimStatement.Shared)
							mapper.MakeGlobalVariable(declaration.Name);
					}
					else
					{
						dataType = dataType.MakeArrayType();

						bool isNewArrayVariable = true;

						if (dimStatement.AlwaysDeclareArrays)
							variableIndex = mapper.DeclareArray(declaration.Name, dataType);
						else
						{
							variableIndex = mapper.ResolveArray(declaration.Name, out isNewArrayVariable, dataType);

							if (routine.IsStaticArray(variableIndex))
								throw CompilerException.ArrayAlreadyDimensioned(declaration.NameToken);
						}

						if (dimStatement.Shared)
							mapper.MakeGlobalArray(declaration.Name, dataType);

						if (declaration.Subscripts != null)
						{
							var translatedDimStatement = new DimensionArrayStatement(dimStatement);

							translatedDimStatement.VariableIndex = variableIndex;

							if (dimStatement is CodeModel.Statements.RedimStatement redimStatement)
							{
								translatedDimStatement.IsRedimension = true;
								translatedDimStatement.PreserveData = redimStatement.Preserve;
							}

							bool constantBounds = true;

							foreach (var subscript in declaration.Subscripts.Subscripts)
							{
								var bound1 = TranslateExpression(subscript.Bound1, container, mapper, compilation);
								var bound2 = TranslateExpression(subscript.Bound2, container, mapper, compilation);

								if (bound1 == null)
									throw new Exception("Must specify the first bound for an array subscript");

								if (constantBounds)
									constantBounds = bound1.IsConstant;

								if (bound2 == null)
									translatedDimStatement.Subscripts.Add(new IntegerLiteralValue(0), bound1);
								else
								{
									translatedDimStatement.Subscripts.Add(bound1, bound2);

									if (constantBounds)
										constantBounds = bound2.IsConstant;
								}
							}

							// When the following conditions are met, arrays are configured prior to execution commenting:
							// - '$STATIC
							// - The DIM statement is not in any sort of conditional compilation clause (IF, FOR, etc.)
							// - The DIM statement is not inside a SUB, FUNCTION or DEF FN
							// - All of the bounds expressions are evaluable at compile time
							// - There is no preceding DIM statement for the same identifier

							bool isStatic =
								compilation.UseStaticArrays &&
								(container == routine) && // not in any subsequence
								routine.IsMainRoutine &&
								constantBounds &&
								isNewArrayVariable;

							if (isStatic)
								routine.AddStaticArray(translatedDimStatement);
							else
								container.Append(translatedDimStatement);
						}
					}
				}

				break;
			}
			case CodeModel.Statements.DoStatement doStatement:
			{
				if (doStatement is CodeModel.Statements.LoopStatement)
					throw CompilerException.LoopWithoutDo(statement);

				Evaluable? preCondition = null;
				Evaluable? postCondition = null;

				if (doStatement.ConditionType != CodeModel.Statements.DoConditionType.None)
				{
					preCondition = TranslateExpression(doStatement.Expression, container, mapper, compilation);

					if (preCondition == null)
						throw new Exception("DoStatement with no Condition but specifying a ConditionType");

					if (doStatement.ConditionType == CodeModel.Statements.DoConditionType.Until)
					{
						preCondition = Not.Construct(preCondition);
						Evaluable.CollapseConstantExpression(ref preCondition);
					}
				}

				var body = new Sequence();

				iterator.Advance();
				iterator.ProcessLabels(module.DataParser, body);

				CodeModel.Statements.LoopStatement? loopStatement = null;

				while (iterator.HaveCurrentStatement)
				{
					loopStatement = statement as CodeModel.Statements.LoopStatement;

					if (loopStatement != null)
					{
						if (loopStatement.ConditionType != CodeModel.Statements.DoConditionType.None)
						{
							postCondition = TranslateExpression(loopStatement.Expression, container, mapper, compilation);

							if (postCondition == null)
								throw new Exception("LoopStatement with no Condition but specifying a ConditionType");

							if (loopStatement.ConditionType == CodeModel.Statements.DoConditionType.Until)
							{
								postCondition = Not.Construct(postCondition);
								Evaluable.CollapseConstantExpression(ref postCondition);
							}
						}

						break;
					}

					TranslateStatement(element, ref statement, iterator, body, routine, compilation, module, out nextStatementInfo);

					iterator.ProcessLabels(module.DataParser, body);
				}

				if (loopStatement == null)
					throw CompilerException.DoWithoutLoop(doStatement);
				if ((preCondition != null) && (postCondition != null))
					throw CompilerException.LoopWithoutDo(loopStatement);

				LoopStatement translatedLoopStatement;

				if (preCondition != null)
					translatedLoopStatement = LoopStatement.ConstructPreConditionLoop(
						preCondition,
						body,
						doStatement,
						DetectDelayLoops);
				else if (postCondition != null)
					translatedLoopStatement = LoopStatement.ConstructPostConditionLoop(
						body,
						postCondition,
						loopStatement,
						doStatement,
						DetectDelayLoops);
				else
					translatedLoopStatement = LoopStatement.ConstructUnconditionalLoop(
						body,
						doStatement,
						DetectDelayLoops);

				translatedLoopStatement.Type = LoopType.Do;

				body.OwnerExecutable = translatedLoopStatement;

				container.Append(translatedLoopStatement);

				break;
			}
			case CodeModel.Statements.ElseStatement: // these are normally subsumed by IfStatement parsing
			case CodeModel.Statements.ElseIfStatement:
				throw new RuntimeException(statement, "ELSE without IF");
			case CodeModel.Statements.EmptyStatement:
				break;
			case CodeModel.Statements.EndScopeStatement endScopeStatement:
			{
				if (element.Type == CodeModel.CompilationElementType.Main)
					throw CompilerException.IllegalOutsideOfSubOrFunction(endScopeStatement);

				iterator.Advance();

				if (!iterator.ExpectEnd())
					throw CompilerException.EndSubOrEndFunctionMustBeLastLine(statement);

				// A no-op that makes it possible to pause execution on the END SUB/FUNCTION.
				container.Append(new EndOfRoutineStatement(endScopeStatement));

				break;
			}
			case CodeModel.Statements.EndStatement endStatement:
			{
				var translatedEndStatement = new EndStatement(endStatement);

				translatedEndStatement.ExitCodeExpression = TranslateExpression(endStatement.ExitCodeExpression, container, mapper, compilation);

				container.Append(translatedEndStatement);

				break;
			}
			case CodeModel.Statements.ErrorStatement errorStatement:
			{
				var translatedErrorStatement = new ErrorStatement(errorStatement);

				TranslateNumericArgumentExpression(
					ref translatedErrorStatement.ErrorNumberExpression, errorStatement.ErrorNumberExpression);

				container.Append(translatedErrorStatement);

				break;
			}
			case CodeModel.Statements.ExitScopeStatement exitScopeStatement:
			{
				// TODO: validation
				// => EXIT DEF only inside DEF FN
				// => EXIT SUB only if the current routine is a SUB
				// => EXIT FUNCTION only if the current routine is a FUNCTION
				// => EXIT DO only if we are locally inside a DO loop
				// => EXIT FOR only if we are locally inside a FOR loop

				var translatedExitScopeStatement = new ExitScopeStatement(exitScopeStatement);

				translatedExitScopeStatement.ScopeExitThrowable =
					exitScopeStatement.ScopeType switch
					{
						CodeModel.Statements.ScopeType.Def => new ExitRoutine(),
						CodeModel.Statements.ScopeType.Sub => new ExitRoutine(),
						CodeModel.Statements.ScopeType.Function => new ExitRoutine(),
						CodeModel.Statements.ScopeType.Do => new ExitDo(),
						CodeModel.Statements.ScopeType.For => new ExitFor(),

						_ => throw new Exception("Unrecognized ScopeType")
					};

				container.Append(translatedExitScopeStatement);

				break;
			}
			case CodeModel.Statements.ForStatement forStatement:
			{
				var iteratorVariableIndex = mapper.ResolveVariable(forStatement.CounterVariable);

				var iteratorVariableType = mapper.GetVariableType(iteratorVariableIndex);

				if (!iteratorVariableType.IsNumeric)
					throw CompilerException.TypeMismatch(forStatement.CounterVariableToken);

				var fromExpression = TranslateExpression(forStatement.StartExpression, container, mapper, compilation);
				var toExpression = TranslateExpression(forStatement.EndExpression, container, mapper, compilation);
				var stepExpression = TranslateExpression(forStatement.StepExpression, container, mapper, compilation);

				if (fromExpression == null)
					throw new Exception("ForStatement with no StartExpression");
				if (toExpression == null)
					throw new Exception("ForStatement with no EndExpression");

				var body = new Sequence();

				iterator.Advance();
				iterator.ProcessLabels(module.DataParser, body);

				CodeModel.Statements.NextStatement? nextStatement = null;

				while (iterator.HaveCurrentStatement)
				{
					nextStatement = statement as CodeModel.Statements.NextStatement;

					if (nextStatement != null)
					{
						if (nextStatement.CounterExpressions.Count > 0)
						{
							if (nextStatement.CounterExpressions[0] is not CodeModel.Expressions.IdentifierExpression identifierExpression)
								throw new BadModelException("NextStatement has a CounterExpression that is not an IdentifierExpression");

							if (mapper.ResolveVariable(identifierExpression.Identifier) != iteratorVariableIndex)
								throw CompilerException.NextWithoutFor(identifierExpression.Token);

							if (nextStatement.CounterExpressions.Count > 1)
							{
								nextStatementInfo = new NextStatementInfo(nextStatement);
								nextStatementInfo.LoopsMatched = 1;
							}
						}

						break;
					}

					TranslateStatement(element, ref statement, iterator, body, routine, compilation, module, out nextStatementInfo);

					iterator.ProcessLabels(module.DataParser, body);

					if (nextStatementInfo != null)
					{
						nextStatement = nextStatementInfo.Statement;

						int idx = nextStatementInfo.LoopsMatched;

						if (nextStatement.CounterExpressions[idx] is not CodeModel.Expressions.IdentifierExpression identifierExpression)
							throw new BadModelException("NextStatement has a CounterExpression that is not an IdentifierExpression");

						if (mapper.ResolveVariable(identifierExpression.Identifier) != iteratorVariableIndex)
							throw CompilerException.NextWithoutFor(identifierExpression.Token);

						if (idx + 1 < nextStatement.CounterExpressions.Count)
							nextStatementInfo.LoopsMatched++;
						else
							nextStatementInfo = null;
					}
				}

				var translatedForStatement = ForStatement.Construct(
					iteratorVariableIndex,
					iteratorVariableType.PrimitiveType,
					fromExpression,
					toExpression,
					stepExpression,
					body,
					forStatement,
					nextStatement,
					DetectDelayLoops);

				body.OwnerExecutable = translatedForStatement;

				container.Append(translatedForStatement);

				break;
			}
			case CodeModel.Statements.GetSpriteStatement getSpriteStatement:
			{
				var translatedGetStatement = new GetSpriteStatement(getSpriteStatement);

				translatedGetStatement.FromStep = getSpriteStatement.FromStep;

				TranslateNumericArgumentExpression(
					ref translatedGetStatement.FromXExpression, getSpriteStatement.FromXExpression);
				TranslateNumericArgumentExpression(
					ref translatedGetStatement.FromYExpression, getSpriteStatement.FromYExpression);

				translatedGetStatement.ToStep = getSpriteStatement.ToStep;

				TranslateNumericArgumentExpression(
					ref translatedGetStatement.ToXExpression, getSpriteStatement.ToXExpression);
				TranslateNumericArgumentExpression(
					ref translatedGetStatement.ToYExpression, getSpriteStatement.ToYExpression);

				translatedGetStatement.TargetExpression = TranslateExpression(
					getSpriteStatement.TargetExpression,
					container,
					mapper,
					compilation,
					parseIdentifiersAsArrays: true);

				container.Append(translatedGetStatement);

				break;
			}
			case CodeModel.Statements.GoToStatement goToStatement:
			{
				string target =
					goToStatement.TargetLabel ??
					goToStatement.TargetLineNumber?.ToString() ??
					throw new Exception("CodeModel GoToStatement has no target");

				var translatedGoToStatement = new GoToStatement(target, goToStatement);

				container.Append(translatedGoToStatement);

				break;
			}
			case CodeModel.Statements.GoSubStatement goSubStatement:
			{
				string target =
					goSubStatement.TargetLabel ??
					goSubStatement.TargetLineNumber?.ToString() ??
					throw new Exception("CodeModel GoToStatement has no target");

				var translatedGoSubStatement = new GoSubStatement(target, goSubStatement);

				container.Append(translatedGoSubStatement);

				break;
			}
			case CodeModel.Statements.IfStatement ifStatement:
			{
				var translatedIfStatement = new IfStatement(ifStatement);

				translatedIfStatement.Condition = TranslateExpression(ifStatement.ConditionExpression, container, mapper, compilation);

				if (ifStatement.ThenBody == null)
				{
					// Block IF/ELSEIF/ELSE/END IF

					var block = translatedIfStatement;
					var subsequence = new Sequence();

					translatedIfStatement.ThenBody = subsequence;
					subsequence.OwnerExecutable = translatedIfStatement;

					var blame = statement;

					iterator.Advance();
					iterator.ProcessLabels(module.DataParser, subsequence);

					while (iterator.HaveCurrentStatement)
					{
						if (statement is CodeModel.Statements.EndIfStatement)
						{
							blame = null;
							break;
						}

						switch (statement)
						{
							case CodeModel.Statements.ElseStatement:
							{
								if (block.ElseBody != null)
									throw new RuntimeException(statement, "ELSE without IF");

								blame = statement;

								subsequence = new Sequence();

								block.ElseBody = subsequence;
								subsequence.OwnerExecutable = block;

								if (!iterator.Advance())
									throw CompilerException.BlockIfWithoutEndIf(blame);

								iterator.ProcessLabels(module.DataParser, subsequence);

								break;
							}
							case CodeModel.Statements.ElseIfStatement elseIfStatement:
							{
								// Transform: ELSEIF becomes an IF statement in an ELSE block.
								//
								// The block variable points at the IF statement that owns the
								// current ThenBody. With this transform, block switches to
								// pointing at the nested IfStatement.

								blame = statement;

								var elseBody = new Sequence();

								block.ElseBody = elseBody;
								elseBody.OwnerExecutable = block;

								block = new IfStatement(elseIfStatement);
								block.Condition = TranslateExpression(elseIfStatement.ConditionExpression, container, mapper, compilation);

								elseBody.Append(block);

								subsequence = new Sequence();

								block.ThenBody = subsequence;
								subsequence.OwnerExecutable = block;

								if (elseIfStatement.ThenBody != null)
								{
									// Weird syntax: ELSEIF can have an inline THEN block. The statements are
									// just part of the multi-line THEN block up to the next ELSEIF/ELSE/END IF.
									int idx = 0;

									while (idx < elseIfStatement.ThenBody.Count)
										TranslateStatement(element, elseIfStatement.ThenBody, ref idx, subsequence, routine, compilation, module);
								}

								if (!iterator.Advance())
									throw CompilerException.BlockIfWithoutEndIf(blame);

								iterator.ProcessLabels(module.DataParser, subsequence);

								break;
							}

							default:
							{
								TranslateStatement(element, ref statement, iterator, subsequence, routine, compilation, module, out nextStatementInfo);

								if (nextStatementInfo != null)
								{
									throw CompilerException.NextWithoutFor(
										nextStatementInfo.Statement.CounterExpressions[nextStatementInfo.LoopsMatched].Token);
								}

								break;
							}
						}
					}

					if (blame != null)
						throw CompilerException.BlockIfWithoutEndIf(blame);
				}
				else
				{
					// Inline IF statement.

					var thenBody = new Sequence();

					int idx = 0;

					while (idx < ifStatement.ThenBody.Count)
						TranslateStatement(element, ifStatement.ThenBody, ref idx, thenBody, routine, compilation, module);

					translatedIfStatement.ThenBody = thenBody;
					thenBody.OwnerExecutable = translatedIfStatement;

					if (ifStatement.ElseBody != null)
					{
						var elseBody = new Sequence();

						idx = 0;

						while (idx < ifStatement.ThenBody.Count)
							TranslateStatement(element, ifStatement.ElseBody, ref idx, elseBody, routine, compilation, module);

						translatedIfStatement.ElseBody = elseBody;
						elseBody.OwnerExecutable = translatedIfStatement;
					}
				}

				container.Append(translatedIfStatement);

				break;
			}
			case CodeModel.Statements.InputStatement inputStatement:
			{
				string promptString;

				if (inputStatement.PromptString != null)
				{
					promptString = inputStatement.PromptString;

					if (inputStatement.PromptQuestionMark)
						promptString += "? ";
				}
				else
					promptString = "? ";

				var translatedInputStatement = new InputStatement(promptString, inputStatement);

				foreach (var target in inputStatement.Targets)
				{
					var translatedTarget = TranslateExpression(target, container, mapper, compilation);

					translatedInputStatement.TargetExpressions.Add(translatedTarget);
				}

				container.Append(translatedInputStatement);

				break;
			}
			case CodeModel.Statements.LineStatement lineStatement:
			{
				var translatedLineStatement = new LineStatement(lineStatement);

				translatedLineStatement.FromStep = lineStatement.FromStep;

				TranslateNumericArgumentExpression(
					ref translatedLineStatement.FromXExpression, lineStatement.FromXExpression);
				TranslateNumericArgumentExpression(
					ref translatedLineStatement.FromYExpression, lineStatement.FromYExpression);

				translatedLineStatement.ToStep = lineStatement.ToStep;

				TranslateNumericArgumentExpression(
					ref translatedLineStatement.ToXExpression, lineStatement.ToXExpression);
				TranslateNumericArgumentExpression(
					ref translatedLineStatement.ToYExpression, lineStatement.ToYExpression);

				TranslateNumericArgumentExpression(
					ref translatedLineStatement.ColourExpression, lineStatement.ColourExpression);

				translatedLineStatement.DrawStyle =
					lineStatement.DrawStyle switch
					{
						CodeModel.Statements.LineDrawStyle.Line => LineDrawStyle.Line,
						CodeModel.Statements.LineDrawStyle.Box => LineDrawStyle.Box,
						CodeModel.Statements.LineDrawStyle.FilledBox => LineDrawStyle.FilledBox,

						_ => throw new Exception("Unrecognized LineDrawStyle value " + lineStatement.DrawStyle)
					};

				TranslateNumericArgumentExpression(
					ref translatedLineStatement.StyleExpression, lineStatement.StyleExpression);

				container.Append(translatedLineStatement);

				break;
			}
			case CodeModel.Statements.LineInputStatement lineInputStatement:
			{
				var translatedLineInputStatement = new LineInputStatement(
					lineInputStatement.PromptString,
					lineInputStatement.EchoNewLine,
					lineInputStatement);

				try
				{
					translatedLineInputStatement.TargetExpression = TranslateExpression(
						lineInputStatement.TargetExpression,
						forAssignment: true,
						container,
						mapper,
						compilation);

					if (translatedLineInputStatement.TargetExpression is Function)
						throw new Exception();
				}
				catch
				{
					throw CompilerException.ExpectedVariable(lineInputStatement.TargetExpression?.Token);
				}

				container.Append(translatedLineInputStatement);

				break;
			}
			case CodeModel.Statements.LocateStatement locateStatement:
			{
				var translatedLocateStatement = new LocateStatement(locateStatement);

				TranslateNumericArgumentExpression(
					ref translatedLocateStatement.RowExpression, locateStatement.RowExpression);
				TranslateNumericArgumentExpression(
					ref translatedLocateStatement.ColumnExpression, locateStatement.ColumnExpression);
				TranslateNumericArgumentExpression(
					ref translatedLocateStatement.CursorVisibilityExpression, locateStatement.CursorVisibilityExpression);
				TranslateNumericArgumentExpression(
					ref translatedLocateStatement.CursorStartExpression, locateStatement.CursorStartExpression);
				TranslateNumericArgumentExpression(
					ref translatedLocateStatement.CursorEndExpression, locateStatement.CursorEndExpression);

				if ((translatedLocateStatement.RowExpression == null)
				 && (translatedLocateStatement.ColumnExpression == null)
				 && (translatedLocateStatement.CursorVisibilityExpression == null)
				 && (translatedLocateStatement.CursorStartExpression == null)
				 && (translatedLocateStatement.CursorEndExpression == null))
					throw new Exception("LocateStatement with no argument expressions");

				container.Append(translatedLocateStatement);

				break;
			}
			case CodeModel.Statements.OnErrorStatement onErrorStatement:
			{
				Executable translatedOnErrorStatement;

				switch (onErrorStatement.Action)
				{
					case CodeModel.Statements.OnErrorAction.DoNotHandle:
						translatedOnErrorStatement = new OnErrorGoTo0Statement(onErrorStatement.LocalHandler, onErrorStatement);
						break;
					case CodeModel.Statements.OnErrorAction.ResumeNext:
						translatedOnErrorStatement = new OnErrorResumeNextStatement(onErrorStatement.LocalHandler, onErrorStatement);
						break;
					case CodeModel.Statements.OnErrorAction.GoToHandler:
						translatedOnErrorStatement = new OnErrorGoToLineStatement(
							target: onErrorStatement.TargetLabel ?? onErrorStatement.TargetLineNumber ?? throw new Exception("Internal error: OnErrorStatement with Action GoTo but no target line"),
							local: onErrorStatement.LocalHandler,
							source: onErrorStatement);
						break;

					default: throw new Exception("Unrecognized OnErrorAction " + onErrorStatement.Action);
				}

				container.Append(translatedOnErrorStatement);

				break;
			}
			case CodeModel.Statements.OutStatement outStatement:
			{
				var translatedOutStatement = new OutStatement(outStatement);

				TranslateNumericArgumentExpression(
					ref translatedOutStatement.PortExpression, outStatement.PortExpression);
				TranslateNumericArgumentExpression(
					ref translatedOutStatement.DataExpression, outStatement.DataExpression);

				container.Append(translatedOutStatement);

				break;
			}
			case CodeModel.Statements.PaintStatement paintStatement:
			{
				var xExpression = TranslateExpression(paintStatement.XExpression, container, mapper, compilation);
				var yExpression = TranslateExpression(paintStatement.YExpression, container, mapper, compilation);
				var paintExpression = TranslateExpression(paintStatement.PaintExpression, container, mapper, compilation);
				var borderColourExpression = TranslateExpression(paintStatement.BorderColourExpression, container, mapper, compilation);
				var backgroundExpression = TranslateExpression(paintStatement.BackgroundExpression, container, mapper, compilation);

				if (xExpression == null)
					throw new Exception("PaintStatement with no XExpression");
				if (yExpression == null)
					throw new Exception("PaintStatement with no YExpression");

				var translatedPaintStatement = PaintStatement.Construct(
					paintStatement,
					paintStatement.Step,
					xExpression,
					yExpression,
					paintExpression,
					borderColourExpression,
					backgroundExpression);

				container.Append(translatedPaintStatement);

				break;
			}
			case CodeModel.Statements.PaletteStatement paletteStatement:
			{
				var translatedPaletteStatement = new PaletteStatement(paletteStatement);

				TranslateNumericArgumentExpression(
					ref translatedPaletteStatement.AttributeExpression, paletteStatement.AttributeExpression);
				TranslateNumericArgumentExpression(
					ref translatedPaletteStatement.ColourExpression, paletteStatement.ColourExpression);

				container.Append(translatedPaletteStatement);

				break;
			}
			case CodeModel.Statements.PixelSetStatement pixelSetStatement:
			{
				var translatedPixelSetStatement = new PixelSetStatement(pixelSetStatement);

				translatedPixelSetStatement.StepCoordinates = pixelSetStatement.StepCoordinates;
				translatedPixelSetStatement.XExpression = TranslateExpression(pixelSetStatement.XExpression, container, mapper, compilation);
				translatedPixelSetStatement.YExpression = TranslateExpression(pixelSetStatement.YExpression, container, mapper, compilation);
				translatedPixelSetStatement.ColourExpression = TranslateExpression(pixelSetStatement.ColourExpression, container, mapper, compilation);
				translatedPixelSetStatement.UseForegroundColour =
					(pixelSetStatement.DefaultColour == CodeModel.Statements.PixelSetDefaultColour.Foreground);

				container.Append(translatedPixelSetStatement);

				break;
			}
			case CodeModel.Statements.PlayStatement playStatement:
			{
				var translatedPlayStatement = new PlayStatement(playStatement);

				TranslateStringArgumentExpression(
					ref translatedPlayStatement.CommandStringExpression,
					playStatement.CommandExpression);

				container.Append(translatedPlayStatement);

				break;
			}
			case CodeModel.Statements.PokeStatement pokeStatement:
			{
				var translatedPokeStatement = new PokeStatement(pokeStatement);

				translatedPokeStatement.AddressExpression = TranslateExpression(pokeStatement.AddressExpression, container, mapper, compilation);
				translatedPokeStatement.ValueExpression = TranslateExpression(pokeStatement.ValueExpression, container, mapper, compilation);

				container.Append(translatedPokeStatement);

				break;
			}
			case CodeModel.Statements.PrintStatement printStatement:
			{
				if (printStatement.FileNumberExpression != null)
					throw new NotImplementedException("TODO");

				PrintArgument TranslatePrintArgument(CodeModel.Statements.PrintArgument argument)
				{
					var translatedArgument = new PrintArgument();

					translatedArgument.ArgumentType =
						argument.ArgumentType switch
						{
							CodeModel.Statements.PrintArgumentType.Value => PrintArgumentType.Value,
							CodeModel.Statements.PrintArgumentType.Tab => PrintArgumentType.Tab,
							CodeModel.Statements.PrintArgumentType.Space => PrintArgumentType.Space,
							_ => throw new Exception("Internal error: Unrecognized PrintArgument ExpressionType")
						};

					translatedArgument.Expression = TranslateExpression(argument.Expression, container, mapper, compilation);

					translatedArgument.CursorAction =
						argument.CursorAction switch
						{
							CodeModel.Statements.PrintCursorAction.None => PrintCursorAction.None,
							CodeModel.Statements.PrintCursorAction.NextZone => PrintCursorAction.NextZone,
							CodeModel.Statements.PrintCursorAction.NextLine => PrintCursorAction.NextLine,

							_ => throw new Exception("Internal error")
						};

					return translatedArgument;
				}


				if (printStatement.UsingExpression == null)
				{
					var translatedPrintStatement = new UnformattedPrintStatement(printStatement);

					foreach (var argument in printStatement.Arguments)
					{
						translatedPrintStatement.Arguments.Add(
							TranslatePrintArgument(argument));
					}

					container.Append(translatedPrintStatement);
				}
				else
				{
					var translatedPrintStatement = new FormattedPrintStatement(printStatement);

					translatedPrintStatement.Format = TranslateExpression(printStatement.UsingExpression, container, mapper, compilation);

					if (printStatement.Arguments.Count > 0)
					{
						translatedPrintStatement.EmitNewLine =
							(printStatement.Arguments.Last().CursorAction == CodeModel.Statements.PrintCursorAction.NextLine);

						foreach (var argument in printStatement.Arguments)
						{
							translatedPrintStatement.Arguments.Add(
								TranslatePrintArgument(argument));
						}
					}

					container.Append(translatedPrintStatement);
				}

				break;
			}
			case CodeModel.Statements.PutSpriteStatement putSpriteStatement:
			{
				var translatedPutStatement = new PutSpriteStatement(putSpriteStatement);

				translatedPutStatement.Step = putSpriteStatement.Step;

				TranslateNumericArgumentExpression(
					ref translatedPutStatement.XExpression, putSpriteStatement.XExpression);
				TranslateNumericArgumentExpression(
					ref translatedPutStatement.YExpression, putSpriteStatement.YExpression);

				translatedPutStatement.SourceExpression = TranslateExpression(
					putSpriteStatement.SourceExpression,
					container,
					mapper,
					compilation,
					parseIdentifiersAsArrays: true);

				translatedPutStatement.ActionVerb =
					putSpriteStatement.ActionVerb switch
					{
						CodeModel.Statements.PutSpriteAction.PixelSet => PutSpriteAction.PixelSet,
						CodeModel.Statements.PutSpriteAction.PixelSetInverted => PutSpriteAction.PixelSet,
						CodeModel.Statements.PutSpriteAction.And => PutSpriteAction.And,
						CodeModel.Statements.PutSpriteAction.Or => PutSpriteAction.Or,
						CodeModel.Statements.PutSpriteAction.ExclusiveOr => PutSpriteAction.ExclusiveOr,

						_ => PutSpriteAction.PixelSet
					};

				container.Append(translatedPutStatement);

				break;
			}
			case CodeModel.Statements.RandomizeStatement randomizeStatement:
			{
				var translatedRandomizeStatement = new RandomizeStatement(randomizeStatement);

				translatedRandomizeStatement.ArgumentExpression =
					TranslateExpression(randomizeStatement.ArgumentExpression, container, mapper, compilation);

				container.Append(translatedRandomizeStatement);

				break;
			}
			case CodeModel.Statements.ReadStatement readStatement:
			{
				var translatedReadStatement = new ReadStatement(module, readStatement);

				foreach (var target in readStatement.Targets)
				{
					var translatedTarget = TranslateExpression(target, container, mapper, compilation);

					translatedReadStatement.TargetExpressions.Add(translatedTarget);
				}

				container.Append(translatedReadStatement);

				break;
			}
			case CodeModel.Statements.RestoreStatement restoreStatement:
			{
				var translatedRestoreStatement = new RestoreStatement(module, restoreStatement);

				translatedRestoreStatement.LabelName = restoreStatement.TargetLabel ?? restoreStatement.TargetLineNumber;

				container.Append(translatedRestoreStatement);

				break;
			}
			case CodeModel.Statements.ResumeStatement resumeStatement:
			{
				Executable translatedResumeStatement;

				if ((resumeStatement.TargetLabel != null) || (resumeStatement.TargetLineNumber != null))
				{
					translatedResumeStatement = new ResumeLineStatement(
						target: resumeStatement.TargetLabel ?? resumeStatement.TargetLineNumber ?? throw new Exception("Sanity failure"),
						source: resumeStatement);
				}
				else
					translatedResumeStatement = new ResumeStatement(resumeStatement.SameStatement, resumeStatement);

				container.Append(translatedResumeStatement);

				break;
			}
			case CodeModel.Statements.ReturnStatement returnStatement:
			{
				string? target =
					returnStatement.TargetLabel ??
					returnStatement.TargetLineNumber?.ToString();

				Executable translatedReturnStatement;

				if (target == null)
					translatedReturnStatement = new ReturnStatement(returnStatement);
				else
					translatedReturnStatement = new ReturnToLabelStatement(target, returnStatement);

				container.Append(translatedReturnStatement);

				break;
			}
			case CodeModel.Statements.ScreenStatement screenStatement:
			{
				var translatedScreenStatement = new ScreenStatement(screenStatement);

				translatedScreenStatement.ModeExpression = TranslateExpression(screenStatement.ModeExpression, container, mapper, compilation);
				translatedScreenStatement.ColourSwitchExpression = TranslateExpression(screenStatement.ColourSwitchExpression, container, mapper, compilation);
				translatedScreenStatement.ActivePageExpression = TranslateExpression(screenStatement.ActivePageExpression, container, mapper, compilation);
				translatedScreenStatement.VisiblePageExpression = TranslateExpression(screenStatement.VisiblePageExpression, container, mapper, compilation);

				container.Append(translatedScreenStatement);

				break;
			}
			case CodeModel.Statements.ScreenWidthStatement screenWidthStatement:
			{
				var translatedScreenWidthStatement = new ScreenWidthStatement(screenWidthStatement);

				TranslateNumericArgumentExpression(
					ref translatedScreenWidthStatement.WidthExpression, screenWidthStatement.WidthExpression);
				TranslateNumericArgumentExpression(
					ref translatedScreenWidthStatement.HeightExpression, screenWidthStatement.HeightExpression);

				container.Append(translatedScreenWidthStatement);

				break;
			}
			case CodeModel.Statements.SelectCaseStatement selectCaseStatement:
			{
				var translatedSelectCaseStatement = new SelectCaseStatement(selectCaseStatement);

				translatedSelectCaseStatement.TestExpression = TranslateExpression(selectCaseStatement.Expression, container, mapper, compilation);

				if (translatedSelectCaseStatement.TestExpression == null)
					throw new Exception("SelectCaseStatement expression translated to null");

				if (!translatedSelectCaseStatement.TestExpression.Type.IsPrimitiveType)
					throw CompilerException.TypeMismatch(selectCaseStatement);

				var testExpressionType = translatedSelectCaseStatement.TestExpression.Type.PrimitiveType;

				CaseBlock? block = null;

				iterator.Advance();

				bool isTerminated = false;

				while (iterator.HaveCurrentStatement)
				{
					if (iterator.ProcessLabels(module.DataParser, block, out var lineNumberStatement))
					{
						if (block == null)
							throw CompilerException.StatementsAndLabelsIllegalBetweenSelectCaseAndCase(lineNumberStatement?.Source);
					}

					if (statement is CodeModel.Statements.EndSelectStatement)
					{
						isTerminated = true;
						break;
					}

					if (statement is CodeModel.Statements.CaseStatement caseStatement)
					{
						block = new CaseBlock();
						block.OwnerExecutable = translatedSelectCaseStatement;

						if (caseStatement.MatchElse)
							block.MatchAll = true;
						else
						{
							if (caseStatement.Expressions is null)
								throw new Exception("CodeModel CaseStatement with no expressions and not MatchElse");

							foreach (var caseExpression in caseStatement.Expressions.Expressions)
							{
								var expression = Conversion.Construct(
									TranslateExpression(caseExpression.Expression, container, mapper, compilation),
									testExpressionType) ?? throw new Exception("Case expression translated to null");

								var rangeEndExpression = Conversion.Construct(
									TranslateExpression(caseExpression.RangeEndExpression, container, mapper, compilation),
									testExpressionType);

								if (expression.IsConstant)
									expression = expression.EvaluateConstant();
								if ((rangeEndExpression is not null) && rangeEndExpression.IsConstant)
									rangeEndExpression = rangeEndExpression.EvaluateConstant();

								var relationToExpression =
									caseExpression.RelationToExpression switch
									{
										null => RelationalOperator.None,

										CodeModel.Statements.RelationalOperator.Equals => RelationalOperator.Equals,
										CodeModel.Statements.RelationalOperator.NotEquals => RelationalOperator.NotEquals,
										CodeModel.Statements.RelationalOperator.LessThan => RelationalOperator.LessThan,
										CodeModel.Statements.RelationalOperator.LessThanOrEquals => RelationalOperator.LessThanOrEquals,
										CodeModel.Statements.RelationalOperator.GreaterThan => RelationalOperator.GreaterThan,
										CodeModel.Statements.RelationalOperator.GreaterThanOrEquals => RelationalOperator.GreaterThanOrEquals,

										_ => throw new Exception("Unrecognized relation to expression: " + caseExpression.RelationToExpression)
									};

								var translatedCaseExpression = CaseExpression.Construct(
									expression,
									rangeEndExpression,
									relationToExpression);

								block.Expressions.Add(translatedCaseExpression);
							}
						}

						translatedSelectCaseStatement.Cases.Add(block);

						iterator.Advance();
					}
					else if (statement is CodeModel.Statements.EmptyStatement)
						iterator.Advance();
					else
					{
						if (block == null)
							throw CompilerException.StatementsAndLabelsIllegalBetweenSelectCaseAndCase(statement);

						TranslateStatement(element, ref statement, iterator, block, routine, compilation, module, out nextStatementInfo);

						if (nextStatementInfo != null)
						{
							throw CompilerException.NextWithoutFor(
								nextStatementInfo.Statement.CounterExpressions[nextStatementInfo.LoopsMatched].Token);
						}
					}
				}

				if (!isTerminated)
					throw CompilerException.SelectWithoutEndSelect(selectCaseStatement);

				if (iterator.ProcessLabels(module.DataParser, block, out var finalLineNumberStatement))
				{
					if (block == null)
						throw CompilerException.StatementsAndLabelsIllegalBetweenSelectCaseAndCase(finalLineNumberStatement?.Source);
				}

				container.Append(translatedSelectCaseStatement);

				break;
			}
			case CodeModel.Statements.SleepStatement sleepStatement:
			{
				var translatedSleepStatement = new SleepStatement(sleepStatement);

				TranslateNumericArgumentExpression(
					ref translatedSleepStatement.SecondsExpression, sleepStatement.Seconds);

				container.Append(translatedSleepStatement);

				break;
			}
			case CodeModel.Statements.SoftKeyConfigStatement softKeyConfigStatement:
			{
				var translatedSoftKeyConfigStatement = new SoftKeyConfigStatement(softKeyConfigStatement);

				translatedSoftKeyConfigStatement.KeyExpression =
					TranslateExpression(softKeyConfigStatement.KeyExpression, container, mapper, compilation);
				translatedSoftKeyConfigStatement.MacroExpression =
					TranslateExpression(softKeyConfigStatement.MacroExpression, container, mapper, compilation);

				container.Append(translatedSoftKeyConfigStatement);

				break;
			}
			case CodeModel.Statements.SoftKeyControlStatement softKeyControlStatement:
			{
				var translatedSoftKeyControlStatement = new SoftKeyControlStatement(softKeyControlStatement);

				translatedSoftKeyControlStatement.Enable = softKeyControlStatement.Enable;

				container.Append(translatedSoftKeyControlStatement);

				break;
			}
			case CodeModel.Statements.SoftKeyListStatement softKeyListStatement:
			{
				var translatedSoftKeyListStatement = new SoftKeyListStatement(softKeyListStatement);

				container.Append(translatedSoftKeyListStatement);

				break;
			}
			case CodeModel.Statements.SoundStatement soundStatement:
			{
				var translatedSoundStatement = new SoundStatement(soundStatement);

				TranslateNumericArgumentExpression(
					ref translatedSoundStatement.FrequencyExpression, soundStatement.FrequencyExpression);
				TranslateNumericArgumentExpression(
					ref translatedSoundStatement.DurationExpression, soundStatement.DurationExpression);

				container.Append(translatedSoundStatement);

				break;
			}
			case CodeModel.Statements.TextViewportStatement textViewportStatement:
			{
				var translatedTextViewportStatement = new TextViewportStatement(textViewportStatement);

				translatedTextViewportStatement.WindowStartExpression =
					TranslateExpression(textViewportStatement.TopExpression, container, mapper, compilation);
				translatedTextViewportStatement.WindowEndExpression =
					TranslateExpression(textViewportStatement.BottomExpression, container, mapper, compilation);

				container.Append(translatedTextViewportStatement);

				break;
			}
			case CodeModel.Statements.TypeStatement typeStatement:
			{
				if ((element.Type != CodeModel.CompilationElementType.Main)
				 || routine.IsDefFn)
					throw CompilerException.IllegalInSubFunctionOrDefFn(statement);

				// Types are gathered in a separate pass, since they need to be known before
				// SUB and FUNCTION parameters are processed. Here, we just skip over them.

				while (iterator.Advance())
				{
					if ((statement is CodeModel.Statements.EmptyStatement))
						continue;
					if (statement is CodeModel.Statements.EndTypeStatement)
						break;

					if (statement is not CodeModel.Statements.TypeElementStatement typeElementStatement)
						throw CompilerException.StatementIllegalInTypeBlock(statement);
				}

				if (statement is not CodeModel.Statements.EndTypeStatement)
					throw new RuntimeException(typeStatement, "Unterminated TYPE definition");

				break;
			}
			case CodeModel.Statements.WEndStatement:
				throw CompilerException.WEndWithoutWhile(statement);
			case CodeModel.Statements.WhileStatement whileStatement:
			{
				if (whileStatement.Condition == null)
					throw new Exception("WhileStatement with no Condition");

				Evaluable condition =
					TranslateExpression(whileStatement.Condition, container, mapper, compilation);

				var body = new Sequence();

				iterator.Advance();

				iterator.ProcessLabels(module.DataParser, body);

				bool haveWEndStatement = false;

				while (iterator.HaveCurrentStatement)
				{
					if (statement is CodeModel.Statements.WEndStatement)
					{
						haveWEndStatement = true;
						break;
					}

					TranslateStatement(element, ref statement, iterator, body, routine, compilation, module, out nextStatementInfo);

					iterator.ProcessLabels(module.DataParser, body);
				}

				if (!haveWEndStatement)
					throw CompilerException.WhileWithoutWEnd(whileStatement);

				LoopStatement translatedLoopStatement = LoopStatement.ConstructPreConditionLoop(
					condition,
					body,
					whileStatement,
					DetectDelayLoops);

				translatedLoopStatement.Type = LoopType.While;

				body.OwnerExecutable = translatedLoopStatement;

				container.Append(translatedLoopStatement);

				break;
			}

			case CodeModel.Statements.UnresolvedWidthStatement unresolvedWidthStatement:
			{
				// WIDTH has two forms that parse the same way:
				//
				//   WIDTH deviceexpression, widthexpression
				//   WIDTH screewidthexpression, screenheightexpression
				//
				// We need to reach this point in semantic analysis to know whether the first
				// expression evaluates to a string or a number.

				if (unresolvedWidthStatement.Expression1 == null)
					throw new Exception("UnresolvedWidthStatement without Expression1");

				var firstArgumentExpression =
					TranslateExpression(unresolvedWidthStatement.Expression1, container, mapper, compilation);

				if (firstArgumentExpression.Type.IsString)
				{
					// WIDTH device$, width%
					statement = unresolvedWidthStatement.ResolveToDeviceWidth();
				}
				else if (firstArgumentExpression.Type.IsNumeric)
				{
					// WIDTH width%, height%
					statement = unresolvedWidthStatement.ResolveToScreenWidth();
				}
				else
					throw RuntimeException.TypeMismatch(firstArgumentExpression.Source);

				TranslateStatement(element, ref statement, iterator, container, routine, compilation, module, out nextStatementInfo);

				// The recursive TranslateStatement has already advanced the iterator.
				return;
			}

			default: throw new NotImplementedException("Statement not implemented: " + statement.Type);
		}

		iterator.Advance();
	}

	[return: NotNullIfNotNull(nameof(expression))]
	private Evaluable? TranslateExpression(CodeModel.Expressions.Expression? expression, Sequence? container, Mapper mapper, Compilation compilation, bool createImplicitArray = false, bool parseIdentifiersAsArrays = false)
	{
		return TranslateExpression(expression, forAssignment: false, container, mapper, compilation, createImplicitArray, parseIdentifiersAsArrays);
	}

	[return: NotNullIfNotNull(nameof(expression))]
	private Evaluable? TranslateExpression(CodeModel.Expressions.Expression? expression, bool forAssignment, Sequence? container, Mapper mapper, Compilation compilation, bool createImplicitArray = false, bool parseIdentifiersAsArrays = false)
	{
		if (expression == null)
			return null;

		var translatedExpression = TranslateExpressionUncollapsed(expression, forAssignment, container, mapper, compilation, createImplicitArray, parseIdentifiersAsArrays: parseIdentifiersAsArrays);

		translatedExpression.Source = expression;

		Evaluable.CollapseConstantExpression(ref translatedExpression);

		return translatedExpression;
	}

	private Evaluable TranslateExpressionUncollapsed(CodeModel.Expressions.Expression expression, bool forAssignment, Sequence? container, Mapper mapper, Compilation compilation, bool constantValue = false, bool createImplicitArray = false, bool parseIdentifiersAsArrays = false)
	{
		MutableBox<int> BlameLineNumber(Token? token)
			=> token?.LineNumberBox ?? new MutableBox<int>(-1);

		switch (expression)
		{
			case CodeModel.Expressions.ParenthesizedExpression parenthesized:
				if (parenthesized.Child is null)
					throw new Exception("ParenthesizedExpression with no Child");

				if (forAssignment)
					throw CompilerException.ExpectedStatement(parenthesized.Token);

				return TranslateExpressionUncollapsed(parenthesized.Child, forAssignment: false, container, mapper, compilation, constantValue);

			case CodeModel.Expressions.LiteralExpression literal:
				if (forAssignment)
					throw CompilerException.ExpectedStatement(literal.Token);

				return LiteralValue.ConstructFromCodeModel(literal);

			case CodeModel.Expressions.IdentifierExpression identifier:
			{
				string qualifiedIdentifier = mapper.QualifyIdentifier(identifier.Identifier);
				string unqualifiedIdentifier = Mapper.UnqualifyIdentifier(identifier.Identifier);

				if (mapper.TryResolveConstant(unqualifiedIdentifier, out var literal))
					return literal;

				if (constantValue)
					throw CompilerException.InvalidConstant(identifier.Token);

				if (compilation.TryGetNativeProcedure(unqualifiedIdentifier, out var nativeProcedure))
				{
					if (nativeProcedure.ParameterTypes == null)
						throw CompilerException.SubprogramNotDefined(identifier.Token);

					var call = new NativeProcedureCallExpression();

					if (nativeProcedure.ParameterTypes == null)
						call.LocalThunk = nativeProcedure.BuildThunk(System.Array.Empty<DataType>(), default);
					else
					{
						if (nativeProcedure.ParameterTypes.Length != 0)
							throw CompilerException.ArgumentCountMismatch(identifier.Token);
					}

					call.Target = nativeProcedure;

					return call;
				}
				else if (compilation.Functions.TryGetValue(unqualifiedIdentifier, out var function))
				{
					if (forAssignment)
					{
						if (function != mapper.Routine)
							throw CompilerException.DuplicateDefinition(identifier.Token);

						// Fall through to treat function name as a variable.
					}
					else
					{
						var returnType = function.ReturnType ?? throw new Exception("Internal error: function with no return type");

						if (function.ParameterTypes.Count > 0)
							throw CompilerException.ArgumentCountMismatch(expression.Token);

						var call = new CallExpression();

						call.Target = function;

						return call;
					}
				}

				if (parseIdentifiersAsArrays)
				{
					int variableIndex = mapper.ResolveArray(identifier.Identifier, out _);
					var variableType = mapper.GetVariableType(variableIndex);

					if (variableIndex < 0)
						throw CompilerException.ArrayNotDefined(identifier.Token);

					return new IdentifierExpression(variableIndex, variableType);
				}
				else
				{
					int variableIndex = mapper.ResolveVariable(identifier.Identifier);

					if (variableIndex < 0)
					{
						var type = mapper.GetTypeForIdentifier(identifier.Identifier);

						return LiteralValue.Construct(0, type, identifier.Token);
					}
					else
					{
						var variableType = mapper.GetVariableType(variableIndex);

						return new IdentifierExpression(variableIndex, variableType);
					}
				}
			}

			case CodeModel.Expressions.CallOrIndexExpression callOrIndexExpression:
			{
				// The identifier could be a dotted identifier. If so,
				// collapse it.
				//
				// Is the identifier a defined function?
				// -> Translate to a call to the function.
				//
				// Is the identifier an undefined but declared function?
				// -> Translate to an unresolved call to the function.
				//
				// Else:
				// -> If the identifier is undefined, define it as an array with a
				//    matching number of subscripts. Each subscript is 0 TO 10.
				// -> Translate to an array access.

				if (constantValue)
					throw CompilerException.InvalidConstant(callOrIndexExpression.Token);

				string? identifier = (callOrIndexExpression.Subject as CodeModel.Expressions.IdentifierExpression)?.Identifier;
				Token? identifierToken = callOrIndexExpression.Subject.Token;

				if (identifier == null)
				{
					identifier = CollapseDottedIdentifierExpression(callOrIndexExpression.Subject, mapper, out int column);

					if (identifier != null)
					{
						identifierToken = new Token(
							BlameLineNumber(callOrIndexExpression.Subject.Token),
							column,
							TokenType.Identifier,
							identifier);

						identifierToken.OwnerStatement = expression.Token?.OwnerStatement;
					}
				}

				// TODO: standard library functions
				// DTFMT:
				// - Weekday&()
				// - Day&()
				// - Month&()
				// - Year&){
				// - Hour&()
				// - Minute&()
				// - Second&()
				// - DateSerial#()
				// - TimeSerial#()
				// - DateValue#()
				// - TimeValue#()
				// - Now#
				// FORMAT:
				// - FormatI$()
				// - FormatL$()
				// - FormatS$()
				// - FormatD$()
				// - FormatC$()
				// FINANCE:
				// - FV#()
				// - IPmt#()
				// - NPer#()
				// - Pmt#()
				// - PPmt#()
				// - PV#()
				// - Rate#()
				// - DDB#()
				// - SLN#()
				// - SYD#()

				if (identifier != null)
				{
					string unqualifiedIdentifier = Mapper.UnqualifyIdentifier(identifier);

					if (compilation.TryGetNativeProcedure(unqualifiedIdentifier, out var nativeProcedure))
					{
						var translatedCallExpression = new NativeProcedureCallExpression();

						if (nativeProcedure.ParameterTypes == null)
						{
							if (callOrIndexExpression.Arguments != null)
							{
								foreach (var argument in callOrIndexExpression.Arguments.Expressions)
								{
									var translatedExpression = TranslateExpression(argument, container, mapper, compilation);

									if (translatedExpression == null)
										throw new Exception("Call argument translated to null");

									translatedCallExpression.Arguments.Add(translatedExpression);
								}
							}

							translatedCallExpression.LocalThunk = nativeProcedure.BuildThunk(
								translatedCallExpression.Arguments.Select(arg => arg.Type).ToList(),
								compilation.UseDirectMarshalling);
						}
						else
						{
							int callArgumentCount = callOrIndexExpression.Arguments?.Count ?? 0;
							int targetArgumentCount = nativeProcedure.ParameterTypes?.Length ?? 0;

							if (callArgumentCount != targetArgumentCount)
								throw CompilerException.ArgumentCountMismatch(callOrIndexExpression.Token);

							translatedCallExpression.Target = nativeProcedure;

							if (callOrIndexExpression.Arguments != null)
							{
								foreach (var argument in callOrIndexExpression.Arguments.Expressions)
								{
									var translatedExpression = TranslateExpression(argument, container, mapper, compilation);

									if (translatedExpression == null)
										throw new Exception("Call argument translated to null");

									translatedCallExpression.Arguments.Add(translatedExpression);
								}

								translatedCallExpression.EnsureParameterTypes();
							}
						}

						return translatedCallExpression;
					}

					bool isForwardReference = compilation.UnresolvedReferences.TryGetDeclaration(unqualifiedIdentifier, out var forwardReference);

					if (compilation.IsRegistered(unqualifiedIdentifier) || isForwardReference)
					{
						if (forAssignment)
							throw CompilerException.DuplicateDefinition(callOrIndexExpression.Subject.Token);

						if (compilation.Subs.ContainsKey(unqualifiedIdentifier))
							throw new CompilerException(callOrIndexExpression.Subject.Token, "Cannot invoke a SUB as a function");

						if (!compilation.Functions.TryGetValue(unqualifiedIdentifier, out var function))
						{
							if (!isForwardReference)
								throw new Exception("Internal error: identifier " + unqualifiedIdentifier + " is registered but is neither a SUB nor a FUNCTION?");

							if (forwardReference!.RoutineType != RoutineType.Function)
								throw CompilerException.DuplicateDefinition(expression.Token);
						}
						else
						{
							if (callOrIndexExpression.Arguments.Count != function.ParameterTypes.Count)
								throw CompilerException.ArgumentCountMismatch(callOrIndexExpression.Subject.Token);
						}

						var translatedCallExpression = new CallExpression();

						translatedCallExpression.Target = function;

						if (function == null)
						{
							translatedCallExpression.UnresolvedTargetName = unqualifiedIdentifier;
							if (forwardReference != null)
								translatedCallExpression.UnresolvedTargetType = forwardReference.ReturnType;
							translatedCallExpression.UnresolvedTargetToken = identifierToken;
							forwardReference!.UnresolvedCalls.Add(translatedCallExpression);
						}

						foreach (var argument in callOrIndexExpression.Arguments.Expressions)
						{
							var translatedArgument = TranslateExpression(argument, container, mapper, compilation);

							if (translatedArgument == null)
								throw new Exception("Internal error: call argument translated to null");

							translatedCallExpression.Arguments.Add(translatedArgument);
						}

						if (translatedCallExpression.Target != null)
							translatedCallExpression.EnsureParameterTypes();

						return translatedCallExpression;
					}
				}

				// It's not a function call, so it's an array access.
				Evaluable subject;

				if (identifier == null)
				{
					subject = TranslateExpression(
						callOrIndexExpression.Subject,
						container,
						mapper,
						compilation);
				}
				else
				{
					var variableIndex = mapper.ResolveArray(identifier, out bool implicitlyCreated);

					if (variableIndex < 0)
					{
						var type = mapper.GetTypeForIdentifier(identifier);

						return LiteralValue.Construct(0, type, identifierToken);
					}

					if (implicitlyCreated)
					{
						if (container == null)
							throw new Exception("TranslateExpression needs to create an implicit array but no container was specified");

						var implicitDimStatement = new DimensionArrayStatement(null);

						implicitDimStatement.CanBreak = false;

						implicitDimStatement.VariableIndex = variableIndex;

						for (int i = 0; i < callOrIndexExpression.Arguments.Expressions.Count; i++)
						{
							implicitDimStatement.Subscripts.Add(
								new IntegerLiteralValue(0),
								new IntegerLiteralValue(10));
						}

						container.Inject(implicitDimStatement);
					}

					subject = new IdentifierExpression(
						variableIndex,
						mapper.GetVariableType(variableIndex));
				}

				var translatedArrayElementExpression = new ArrayElementExpression(subject, subject.Type.MakeElementType());

				foreach (var subscript in callOrIndexExpression.Arguments.Expressions)
				{
					var translatedArgument = TranslateExpression(subscript, container, mapper, compilation);

					if (translatedArgument == null)
						throw new Exception("Internal error: call argument translated to null");

					translatedArrayElementExpression.SubscriptExpressions.Add(translatedArgument);
				}

				return translatedArrayElementExpression;
			}

			case CodeModel.Expressions.KeywordFunctionExpression keywordFunction:
			{
				if (constantValue)
					throw CompilerException.InvalidConstant(keywordFunction.Token);

				if (forAssignment)
				{
					if (!keywordFunction.IsValidAssignmentTarget())
						throw CompilerException.ExpectedStatement(keywordFunction.Token);
				}

				IEnumerable<Evaluable> arguments = Enumerable.Empty<Evaluable>();

				if (keywordFunction.Arguments != null)
				{
					int arrayArgumentIndex = keywordFunction.GetArrayArgumentIndex(); // -1 if there isn't one

					arguments =
						keywordFunction.Arguments!.Expressions.Select((expr, idx) =>
							TranslateExpression(expr, container, mapper, compilation, parseIdentifiersAsArrays: idx == arrayArgumentIndex)
								?? throw new Exception("Argument expression translated to null"));
				}

				Function function;

				switch (keywordFunction.Function)
				{
					case TokenType.ABS: return AbsFunction.Construct(keywordFunction.Token, arguments);
					case TokenType.ASC: function = new AscFunction(); break;
					case TokenType.ATN: function = new AtnFunction(); break;
					case TokenType.CCUR: function = new CCurFunction(); break;
					case TokenType.CDBL: function = new CDblFunction(); break;
					case TokenType.CHR: function = new ChrFunction(); break;
					case TokenType.CINT: function = new CIntFunction(); break;
					case TokenType.CLNG: function = new CLngFunction(); break;
					case TokenType.COS: function = new CosFunction(); break;
					case TokenType.CSNG: function = new CSngFunction(); break;
					case TokenType.ERR: function = new ErrFunction(); break;
					case TokenType.ERL: function = new ErlFunction(); break;
					case TokenType.FIX: return FixFunction.Construct(keywordFunction.Token, arguments);
					case TokenType.INKEY: function = new InKeyFunction(); break;
					case TokenType.INP: function = new InpFunction(); break;
					case TokenType.INSTR: function = new InStrFunction(); break;
					case TokenType.INT: return IntFunction.Construct(keywordFunction.Token, arguments);
					case TokenType.LBOUND: function = new LBoundFunction(); break;
					case TokenType.LCASE: function = new LCaseFunction(); break;
					case TokenType.LEFT: function = new LeftFunction(); break;
					case TokenType.LEN: function = new LenFunction(); break;
					case TokenType.LTRIM: function = new LTrimFunction(); break;
					case TokenType.MID: function = new MidFunction(); break;
					case TokenType.PEEK: function = new PeekFunction(); break;
					case TokenType.POINT: function = new PointFunction(); break;
					case TokenType.RIGHT: function = new RightFunction(); break;
					case TokenType.RND: function = new RndFunction(); break;
					case TokenType.RTRIM: function = new RTrimFunction(); break;
					case TokenType.SIN: function = new SinFunction(); break;
					case TokenType.SPACE: function = new SpaceFunction(); break;
					case TokenType.STR: function = new StrFunction(); break;
					case TokenType.TAN: function = new TanFunction(); break;
					case TokenType.TIMER: function = new TimerFunction(); break;
					case TokenType.UBOUND: function = new UBoundFunction(); break;
					case TokenType.UCASE: function = new UCaseFunction(); break;
					case TokenType.VAL: function = new ValFunction(); break;

					default: throw new NotImplementedException("Keyword function: " + keywordFunction.Function);
				}

				if (function is not ConstructibleFunction)
					function.SetArguments(arguments);

				return function;
			}

			case CodeModel.Expressions.UnaryExpression unaryExpression:
			{
				if (forAssignment)
					throw CompilerException.ExpectedStatement(unaryExpression.Token);

				var right = TranslateExpression(unaryExpression.Child, container, mapper, compilation, constantValue);

				if (right == null)
					throw new Exception("Internal error: Unary expression operand translated to null");

				switch (unaryExpression.Operator)
				{
					case CodeModel.Expressions.Operator.Negate: return Negation.Construct(right);
					case CodeModel.Expressions.Operator.Not: return Not.Construct(right);

					default: throw new Exception("Internal error: Unrecognized unary expression operator " + unaryExpression.Operator);
				}
			}

			case CodeModel.Expressions.BinaryExpression binaryExpression:
			{
				if (binaryExpression.Operator == CodeModel.Expressions.Operator.Field)
				{
					string? dottedIdentifier = CollapseDottedIdentifierExpression(binaryExpression, mapper, out int column);

					if (dottedIdentifier != null)
					{
						if (mapper.TryResolveConstant(dottedIdentifier, out var literal))
							return literal;

						if (constantValue)
						{
							var blameToken = new Token(
								BlameLineNumber(binaryExpression.Token),
								column,
								TokenType.Identifier,
								dottedIdentifier);

							blameToken.OwnerStatement = expression.Token?.OwnerStatement;

							throw CompilerException.InvalidConstant(blameToken);
						}

						int variableIndex = mapper.ResolveVariable(dottedIdentifier);

						if (variableIndex < 0)
						{
							var type = mapper.GetTypeForIdentifier(dottedIdentifier);

							return LiteralValue.Construct(0, type, expression.Token);
						}
						else
						{
							var variableType = mapper.GetVariableType(variableIndex);

							return new IdentifierExpression(variableIndex, variableType);
						}
					}
					else
					{
						var subjectExpression = TranslateExpression(binaryExpression.Left, container, mapper, compilation, constantValue);

						if (constantValue)
							throw CompilerException.InvalidConstant(binaryExpression?.Token);

						if (binaryExpression.Right is not CodeModel.Expressions.IdentifierExpression identifierExpression)
							throw new Exception("Member access expressions require the right-hand operand to be an identifier");

						string identifier = identifierExpression.Identifier;
						var identifierToken = identifierExpression.Token;

						return FieldAccessExpression.Construct(subjectExpression, identifier, identifierToken);
					}
				}

				if (forAssignment)
				{
					var leftest = binaryExpression.Left;

					while (true)
					{
						if (leftest is CodeModel.Expressions.BinaryExpression nestedBinaryExpression)
						{
							leftest = nestedBinaryExpression.Left;
							continue;
						}

						if (leftest is CodeModel.Expressions.CallOrIndexExpression nestedCallOrIndexExpression)
						{
							leftest = nestedCallOrIndexExpression.Subject;
							continue;
						}

						break;
					}

					throw CompilerException.ExpectedStatement(leftest.Token);
				}

				var left = TranslateExpression(binaryExpression.Left, container, mapper, compilation, constantValue);
				var right = TranslateExpression(binaryExpression.Right, container, mapper, compilation, constantValue);

				if ((left == null) || (right == null))
					throw new Exception("Internal error: Binary expression operand translated to null");

				switch (binaryExpression.Operator)
				{
					case CodeModel.Expressions.Operator.Add: return Addition.Construct(left, right);
					case CodeModel.Expressions.Operator.Subtract: return Subtraction.Construct(left, right);
					case CodeModel.Expressions.Operator.Multiply: return Multiplication.Construct(left, right);
					case CodeModel.Expressions.Operator.Divide: return Division.Construct(left, right);
					case CodeModel.Expressions.Operator.Exponentiate: return Exponentiation.Construct(left, right);
					case CodeModel.Expressions.Operator.IntegerDivide: return IntegerDivision.Construct(left, right);
					case CodeModel.Expressions.Operator.Modulo: return Modulo.Construct(left, right);

					case CodeModel.Expressions.Operator.Equals: return Compiled.RelationalOperators.Equals.Construct(left, right);
					case CodeModel.Expressions.Operator.NotEquals: return NotEquals.Construct(left, right);
					case CodeModel.Expressions.Operator.LessThan: return LessThan.Construct(left, right);
					case CodeModel.Expressions.Operator.LessThanOrEquals: return LessThanOrEquals.Construct(left, right);
					case CodeModel.Expressions.Operator.GreaterThan: return GreaterThan.Construct(left, right);
					case CodeModel.Expressions.Operator.GreaterThanOrEquals: return GreaterThanOrEquals.Construct(left, right);

					case CodeModel.Expressions.Operator.And: return And.Construct(left, right);
					case CodeModel.Expressions.Operator.Or: return Or.Construct(left, right);
					case CodeModel.Expressions.Operator.ExclusiveOr: return ExclusiveOr.Construct(left, right);
					case CodeModel.Expressions.Operator.Equivalent: return Equivalent.Construct(left, right);
					case CodeModel.Expressions.Operator.Implies: return Implies.Construct(left, right);

					default: throw new Exception("Internal error: Unrecognized binary expression operator " + binaryExpression.Operator);
				}
			}
		}

		throw new Exception("Internal error: Can't translate expression");
	}

	string? CollapseDottedIdentifierExpression(CodeModel.Expressions.Expression expression, Mapper mapper, out int column)
	{
		if (expression is CodeModel.Expressions.BinaryExpression binaryExpression)
			return CollapseDottedIdentifierExpression(binaryExpression, mapper, out column);
		else
		{
			column = 0;
			return null;
		}
	}

	internal string? CollapseDottedIdentifierExpression(CodeModel.Expressions.BinaryExpression binaryExpression, Mapper mapper, out int column)
	{
		StringBuilder? builder = null;

		CollapseDottedIdentifierExpression(binaryExpression, mapper, ref builder, out column);

		return builder?.ToString();
	}

	void CollapseDottedIdentifierExpression(CodeModel.Expressions.BinaryExpression binaryExpression, Mapper mapper, ref StringBuilder? identifierBuilder, out int column)
	{
		// The specific pattern we're looking for is a left tree of field access expressions where
		// every leaf is an identifier. If we identify that the tree has the correct operator and
		// an identifier on the right, then we can just recursively process the left subtree.
		//
		// When we hit the leftmost node, that's the part of the dotted identifier we're calling
		// the "slug". We can check if the current Mapper allows that slug or not.

		column = default;

		if (binaryExpression.Operator != CodeModel.Expressions.Operator.Field)
			return;

		if (binaryExpression.Right is not CodeModel.Expressions.IdentifierExpression rightIdentifier)
			return;

		switch (binaryExpression.Left)
		{
			case CodeModel.Expressions.IdentifierExpression leftIdentifier:
				if (!mapper.IsDisallowedSlug(leftIdentifier.Identifier))
				{
					identifierBuilder = new StringBuilder(leftIdentifier.Identifier);
					column = leftIdentifier.Token?.Column ?? -1;
				}
				break;
			case CodeModel.Expressions.BinaryExpression leftBinary:
				CollapseDottedIdentifierExpression(leftBinary, mapper, ref identifierBuilder, out column);
				break;
		}

		if (identifierBuilder != null)
			identifierBuilder.Append('.').Append(rightIdentifier.Identifier);
	}
}

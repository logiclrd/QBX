using System;
using System.Collections.Generic;
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

using QBX.LexicalAnalysis;

namespace QBX.ExecutionEngine;

public class Compiler
{
	class TranslationInfo(CodeModel.CompilationElement element, Mapper rootMapper, Routine routine)
	{
		public CodeModel.CompilationElement Element = element;

		public Routine Routine = routine;

		public Mapper Mapper =
			(element.Type == CodeModel.CompilationElementType.Main)
			? rootMapper
			: rootMapper.CreateScope();
	}

	public Module Compile(CodeModel.CompilationUnit unit, Compilation compilation)
	{
		var module = new Module();

		var rootMapper = new Mapper();

		var routineByName = module.Routines;

		var translationInfo = new List<TranslationInfo>();

		// First pass: collect all routines
		foreach (var element in unit.Elements)
		{
			var routine = new Routine(module, element, compilation.TypeRepository);

			if (compilation.IsRegistered(routine.Name))
				throw CompilerException.DuplicateDefinition(element.AllStatements.FirstOrDefault());

			if (routine.Name == Routine.MainRoutineName)
				module.MainRoutine = routine;
			else
				routine.Register(compilation);

			var info = new TranslationInfo(element, rootMapper, routine);

			translationInfo.Add(info);
			routineByName[routine.Name] = routine;
		}

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
						TranslateTypeDefinition(typeStatement, typeElementStatements, compilation);

						typeStatement = null;
						typeElementStatements.Clear();

						break;
				}
			}
		}

		if (typeStatement != null)
			throw CompilerException.TypeWithoutEndType(typeStatement);

		// Third pass: process parameters, which requires that we know all the FUNCTIONs and UDTs
		foreach (var info in translationInfo)
		{
			if (info.Element.Type != CodeModel.CompilationElementType.Main)
			{
				info.Routine.TranslateParameters(info.Mapper, compilation);
				info.Mapper.LinkGlobalVariablesAndArrays();
			}

			if (info.Routine.ReturnType != null)
				info.Routine.ReturnValueVariableIndex = info.Mapper.DeclareVariable(info.Routine.Name, info.Routine.ReturnType);
		}

		// Fourth pass: Collect constants and then translate statements.
		// => CONST definitions inside DEF FN are local to the DEF FN and are not processed here
		foreach (var info in translationInfo)
		{
			var element = info.Element;

			var mapper = (element.Type == CodeModel.CompilationElementType.Main)
				? rootMapper
				: rootMapper.CreateScope();

			mapper.ScanForDisallowedSlugs(element.AllStatements);

			bool inDefFn = false;

			foreach (var statement in element.AllStatements)
			{
				switch (statement)
				{
					case CodeModel.Statements.DefFnStatement: inDefFn = true; break;
					case CodeModel.Statements.EndDefStatement: inDefFn = false; break;
					case CodeModel.Statements.ConstStatement constStatement:
						if (!inDefFn)
						{
							foreach (var definition in constStatement.Definitions)
							{
								var constValueExpression = TranslateExpression(definition.Value, container: null, mapper, compilation);

								mapper.DefineConstant(
									definition.Identifier,
									constValueExpression.EvaluateConstant());
							}
						}

						break;
				}
			}

			string routineName = Routine.GetName(element);

			var routine = routineByName[routineName];

			int lineIndex = 0;
			int statementIndex = 0;

			while (lineIndex < element.Lines.Count)
				TranslateStatement(element, ref lineIndex, ref statementIndex, routine, mapper, compilation, module);

			routine.VariableTypes = mapper.GetVariableTypes();
			routine.LinkedVariables = mapper.GetLinkedVariables();

			routine.ResolveJumpStatements();

			foreach (var statement in routine.AllStatements)
				if (statement is IUnresolvedLineReference unresolvedLineReference)
					unresolvedLineReference.Resolve(routine);
		}

		compilation.Modules.Add(module);

		return module;
	}

	void TranslateTypeDefinition(CodeModel.Statements.TypeStatement typeStatement, List<CodeModel.Statements.TypeElementStatement> elements, Compilation compilation)
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

			udt.Fields.Add(
				new UserDataTypeField(
					typeElementStatement.Name,
					type));
		}

		typeRepository.RegisterType(udt);
	}

	void TranslateStatement(CodeModel.CompilationElement element, ref int lineIndexRef, ref int statementIndexRef, Sequence container, Mapper mapper, Compilation compilation, Module module)
	{
		int lineIndex = lineIndexRef;
		int statementIndex = statementIndexRef;

		if (lineIndex >= element.Lines.Count)
			return;

		var line = element.Lines[lineIndex];

		if (statementIndex >= line.Statements.Count)
		{
			lineIndexRef = lineIndex + 1;
			statementIndexRef = 0;
			return;
		}

		try
		{
			var statement = line.Statements[statementIndex];

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

			iterator.Advanced +=
				newStatement =>
				{
					statement = newStatement;
				};

			TranslateStatement(element, ref statement, iterator, container, mapper, compilation, module, out var nextStatementInfo);

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

	void TranslateStatement(CodeModel.CompilationElement element, IList<CodeModel.Statements.Statement> statements, ref int statementIndexRef, Sequence container, Mapper mapper, Compilation compilation, Module module)
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

			TranslateStatement(element, ref statement, iterator, container, mapper, compilation, module, out var nextStatementInfo);

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

	void TranslateStatement(CodeModel.CompilationElement element, ref CodeModel.Statements.Statement statement, StatementIterator iterator, Sequence container, Mapper mapper, Compilation compilation, Module module, out NextStatementInfo? nextStatementInfo)
	{
		var typeRepository = compilation.TypeRepository;

		nextStatementInfo = null;

		switch (statement)
		{
			case CodeModel.Statements.AssignmentStatement assignmentStatement:
			{
				var targetExpression = TranslateExpression(assignmentStatement.TargetExpression, container, mapper, compilation);
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
			case CodeModel.Statements.CallStatement callStatement:
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
				}

				container.Append(translatedCallStatement);

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
			case CodeModel.Statements.ConstStatement constStatement:
			{
				// Gathered centrally before main translation begins.
				break;
			}
			case CodeModel.Statements.DataStatement dataStatement:
			{
				module.DataParser.AddDataSource(dataStatement.ParseDataItems());
				break;
			}
			case CodeModel.Statements.DeclareStatement declareStatement:
			{
				// If the declared routine is already known, verify that the parameters
				// and type match.
				//
				// If the declared routine is not known, then record the fact that the
				// routine should exist so that we know we can generate
				// UnresolvedCallStatements and UnresolvedFunctionCalls to be linked
				// up later.

				// TODO

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

				var routine = new Routine(module, element, defFnStatement, typeRepository);

				if (routine.ReturnType == null)
				{
					routine.ReturnType = DataType.ForPrimitiveDataType(
						mapper.GetTypeForIdentifier(routine.Name));
				}

				mapper.StartSemiscopeSetup();

				string qualifiedName = mapper.QualifyIdentifier(
					routine.Name,
					routine.ReturnType);

				routine.ReturnValueVariableIndex = mapper.DeclareVariable(
					qualifiedName,
					routine.ReturnType);

				routine.TranslateParameters(mapper, compilation);

				mapper.EnterSemiscope();

				try
				{
					if (defFnStatement.ExpressionBody != null)
					{
						routine.Append(
							new AssignmentStatement(defFnStatement)
							{
								TargetExpression =
									new IdentifierExpression(routine.ReturnValueVariableIndex, routine.ReturnType),

								ValueExpression =
									TranslateExpression(defFnStatement.ExpressionBody, container, mapper, compilation),
							});
					}
					else
					{
						iterator.Advance();

						var labelStatement = iterator.GetLabelStatement();

						module.DataParser.AddLabel(labelStatement);
						routine.AppendIfNotNull(labelStatement);

						while (iterator.HaveCurrentStatement)
						{
							if (statement is CodeModel.Statements.EndDefStatement)
								break;

							if (statement is CodeModel.Statements.DefFnStatement)
								throw CompilerException.IllegalInSubFunctionOrDefFn(statement);

							TranslateStatement(element, ref statement, iterator, routine, mapper, compilation, module, out nextStatementInfo);

							if (nextStatementInfo != null)
							{
								throw CompilerException.NextWithoutFor(
									nextStatementInfo.Statement.CounterExpressions[nextStatementInfo.LoopsMatched].Token);
							}

							iterator.Advance();

							labelStatement = iterator.GetLabelStatement();

							module.DataParser.AddLabel(labelStatement);
							routine.AppendIfNotNull(iterator.GetLabelStatement());
						}
					}
				}
				finally
				{
					mapper.ExitSemiscope();
				}

				routine.UseRootFrame = true;

				compilation.RegisterFunction(routine);

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
				var dataType = DataType.FromCodeModelDataType(defTypeStatement.DataType);

				if (!dataType.IsPrimitiveType)
					throw new Exception("DefTypeStatement's DataType is not a primitive type");

				foreach (var range in defTypeStatement.Ranges)
					mapper.SetIdentifierTypes(range.Start, range.End ?? range.Start, dataType.PrimitiveType);

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
					else
						dataType = DataType.ForPrimitiveDataType(mapper.GetTypeForIdentifier(declaration.Name));

					int variableIndex;

					if (declaration.Subscripts == null)
					{
						variableIndex = mapper.DeclareVariable(declaration.Name, dataType);

						if (dimStatement.Shared)
							mapper.MakeGlobalVariable(declaration.Name);
					}
					else
					{
						dataType = dataType.MakeArrayType();

						variableIndex = mapper.DeclareArray(declaration.Name, dataType);

						if (dimStatement.Shared)
							mapper.MakeGlobalArray(declaration.Name);

						if (declaration.Subscripts != null)
						{
							var translatedDimStatement = new DimensionArrayStatement(dimStatement);

							translatedDimStatement.VariableIndex = variableIndex;

							// TODO: '$STATIC and '$DYNAMIC (can also be used with REM)
							// => when the following conditions are met, arrays are configured prior to execution commenting:
							//    - '$STATIC
							//    - the DIM statement is not in any sort of conditional compilation clause (IF, FOR, etc.)
							//    - the DIM statement is not inside a SUB, FUNCTION or DEF FN
							//    - all of the bounds expressions are evaluable at compile time
							//    - there is no preceding DIM statement for the same identifier
							// in this instance, we set up the array right here and now

							// TODO: if the array has already been initialized, then:
							// - if DimStatement then runtime error "Duplicate definition"
							// - if RedimStatement then dynamically resize
							// - RedimStatements fail on arrays set up statically (see preceding TODO)

							if (dimStatement is CodeModel.Statements.RedimStatement redimStatement)
							{
								translatedDimStatement.IsRedimension = true;
								translatedDimStatement.PreserveData = redimStatement.Preserve;
							}

							foreach (var subscript in declaration.Subscripts.Subscripts)
							{
								var bound1 = TranslateExpression(subscript.Bound1, container, mapper, compilation);
								var bound2 = TranslateExpression(subscript.Bound2, container, mapper, compilation);

								if (bound1 == null)
									throw new Exception("Must specify the first bound for an array subscript");

								if (bound2 == null)
									translatedDimStatement.Subscripts.Add(new IntegerLiteralValue(0), bound1);
								else
									translatedDimStatement.Subscripts.Add(bound1, bound2);
							}

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

				var labelStatement = iterator.GetLabelStatement();

				module.DataParser.AddLabel(labelStatement);
				body.AppendIfNotNull(labelStatement);

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

					TranslateStatement(element, ref statement, iterator, body, mapper, compilation, module, out nextStatementInfo);

					labelStatement = iterator.GetLabelStatement();

					module.DataParser.AddLabel(labelStatement);
					body.AppendIfNotNull(labelStatement);
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
						doStatement);
				else if (postCondition != null)
					translatedLoopStatement = LoopStatement.ConstructPostConditionLoop(
						body,
						postCondition,
						loopStatement,
						doStatement);
				else
					translatedLoopStatement = LoopStatement.ConstructUnconditionalLoop(
						body,
						doStatement);

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
			case CodeModel.Statements.EndStatement endStatement:
			{
				var translatedEndStatement = new EndStatement(endStatement);

				translatedEndStatement.ExitCodeExpression = TranslateExpression(endStatement.ExitCodeExpression, container, mapper, compilation);

				container.Append(translatedEndStatement);

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

				var fromExpression = TranslateExpression(forStatement.StartExpression, container, mapper, compilation);
				var toExpression = TranslateExpression(forStatement.EndExpression, container, mapper, compilation);
				var stepExpression = TranslateExpression(forStatement.StepExpression, container, mapper, compilation);

				if (fromExpression == null)
					throw new Exception("ForStatement with no StartExpression");
				if (toExpression == null)
					throw new Exception("ForStatement with no EndExpression");

				var body = new Sequence();

				iterator.Advance();

				var labelStatement = iterator.GetLabelStatement();

				module.DataParser.AddLabel(labelStatement);
				body.AppendIfNotNull(labelStatement);

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

					TranslateStatement(element, ref statement, iterator, body, mapper, compilation, module, out nextStatementInfo);

					labelStatement = iterator.GetLabelStatement();

					module.DataParser.AddLabel(labelStatement);
					body.AppendIfNotNull(labelStatement);

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
					mapper.GetTypeForIdentifier(forStatement.CounterVariable),
					fromExpression,
					toExpression,
					stepExpression,
					body,
					forStatement,
					nextStatement);

				body.OwnerExecutable = translatedForStatement;

				container.Append(translatedForStatement);

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

					var labelStatement = iterator.GetLabelStatement();

					module.DataParser.AddLabel(labelStatement);
					subsequence.AppendIfNotNull(labelStatement);

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

								labelStatement = iterator.GetLabelStatement();

								module.DataParser.AddLabel(labelStatement);
								subsequence.AppendIfNotNull(labelStatement);

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
										TranslateStatement(element, elseIfStatement.ThenBody, ref idx, subsequence, mapper, compilation, module);
								}

								if (!iterator.Advance())
									throw CompilerException.BlockIfWithoutEndIf(blame);

								labelStatement = iterator.GetLabelStatement();

								module.DataParser.AddLabel(labelStatement);
								subsequence.AppendIfNotNull(labelStatement);

								break;
							}

							default:
							{
								TranslateStatement(element, ref statement, iterator, subsequence, mapper, compilation, module, out nextStatementInfo);

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
						TranslateStatement(element, ifStatement.ThenBody, ref idx, thenBody, mapper, compilation, module);

					translatedIfStatement.ThenBody = thenBody;
					thenBody.OwnerExecutable = translatedIfStatement;

					if (ifStatement.ElseBody != null)
					{
						var elseBody = new Sequence();

						idx = 0;

						while (idx < ifStatement.ThenBody.Count)
							TranslateStatement(element, ifStatement.ElseBody, ref idx, elseBody, mapper, compilation, module);

						translatedIfStatement.ElseBody = elseBody;
						elseBody.OwnerExecutable = translatedIfStatement;
					}
				}

				container.Append(translatedIfStatement);

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
					if (iterator.GetLabelStatement() is LabelStatement labelStatement)
					{
						if (block == null)
							throw CompilerException.StatementsAndLabelsIllegalBetweenSelectCaseAndCase(labelStatement.Source);

						module.DataParser.AddLabel(labelStatement);

						block.Append(labelStatement);
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
					else
					{
						if (block == null)
							throw CompilerException.StatementsAndLabelsIllegalBetweenSelectCaseAndCase(statement);

						TranslateStatement(element, ref statement, iterator, block, mapper, compilation, module, out nextStatementInfo);

						if (nextStatementInfo != null)
						{
							throw CompilerException.NextWithoutFor(
								nextStatementInfo.Statement.CounterExpressions[nextStatementInfo.LoopsMatched].Token);
						}
					}
				}

				if (!isTerminated)
					throw CompilerException.SelectWithoutEndSelect(selectCaseStatement);

				if (iterator.GetLabelStatement() is LabelStatement finalLabelStatement)
				{
					if (block == null)
						throw CompilerException.StatementsAndLabelsIllegalBetweenSelectCaseAndCase(finalLabelStatement.Source);

					module.DataParser.AddLabel(finalLabelStatement);

					block.Append(finalLabelStatement);
				}

				container.Append(translatedSelectCaseStatement);

				break;
			}
			case CodeModel.Statements.TypeStatement typeStatement:
			{
				// TODO: track whether we are in a DEF FN
				if (element.Type != CodeModel.CompilationElementType.Main)
					throw CompilerException.IllegalInSubFunctionOrDefFn(statement);

				// Types are gathered in a separate pass, since they need to be known before
				// SUB and FUNCTION parameters are processed. Here, we just skip over them.

				while (iterator.Advance())
				{
					if ((statement is CodeModel.Statements.EmptyStatement) || (statement is CodeModel.Statements.CommentStatement))
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

				body.AppendIfNotNull(iterator.GetLabelStatement());

				bool haveWEndStatement = false;

				while (iterator.HaveCurrentStatement)
				{
					if (statement is CodeModel.Statements.WEndStatement)
					{
						haveWEndStatement = true;
						break;
					}

					TranslateStatement(element, ref statement, iterator, body, mapper, compilation, module, out nextStatementInfo);

					body.AppendIfNotNull(iterator.GetLabelStatement());
				}

				if (!haveWEndStatement)
					throw CompilerException.WhileWithoutWEnd(whileStatement);

				LoopStatement translatedLoopStatement = LoopStatement.ConstructPreConditionLoop(
					condition,
					body,
					whileStatement);

				translatedLoopStatement.Type = LoopType.While;

				body.OwnerExecutable = translatedLoopStatement;

				container.Append(translatedLoopStatement);

				break;
			}

			default: throw new NotImplementedException("Statement not implemented: " + statement.Type);
		}

		iterator.Advance();
	}

	[return: NotNullIfNotNull(nameof(expression))]
	private Evaluable? TranslateExpression(CodeModel.Expressions.Expression? expression, Sequence? container, Mapper mapper, Compilation compilation, bool createImplicitArray = false)
	{
		if (expression == null)
			return null;

		var translatedExpression = TranslateExpressionUncollapsed(expression, container, mapper, compilation, createImplicitArray);

		Evaluable.CollapseConstantExpression(ref translatedExpression);

		return translatedExpression;
	}

	private Evaluable TranslateExpressionUncollapsed(CodeModel.Expressions.Expression expression, Sequence? container, Mapper mapper, Compilation compilation, bool constantValue = false, bool createImplicitArray = false)
	{
		switch (expression)
		{
			case CodeModel.Expressions.ParenthesizedExpression parenthesized:
				if (parenthesized.Child is null)
					throw new Exception("ParenthesizedExpression with no Child");

				return TranslateExpressionUncollapsed(parenthesized.Child, container, mapper, compilation, constantValue);

			case CodeModel.Expressions.LiteralExpression literal:
				return LiteralValue.ConstructFromCodeModel(literal);

			case CodeModel.Expressions.IdentifierExpression identifier:
			{
				string qualifiedIdentifier = mapper.QualifyIdentifier(identifier.Identifier);
				string unqualifiedIdentifier = mapper.StripTypeCharacter(identifier.Identifier);

				if (mapper.TryResolveConstant(qualifiedIdentifier, out var literal))
					return literal;

				if (constantValue)
					throw CompilerException.InvalidConstant(identifier.Token);

				if (compilation.Functions.TryGetValue(unqualifiedIdentifier, out var function))
				{
					var returnType = function.ReturnType ?? throw new Exception("Internal error: function with no return type");

					string qualifiedFunction = mapper.QualifyIdentifier(function.Name, function.ReturnType);

					if (qualifiedIdentifier != qualifiedFunction)
						throw CompilerException.DuplicateDefinition(expression.Token);

					if (function.ParameterTypes.Count > 0)
						throw CompilerException.ArgumentCountMismatch(expression.Token);

					var call = new CallExpression();

					call.Target = function;

					return call;
				}

				int variableIndex = mapper.ResolveVariable(identifier.Identifier);
				var variableType = mapper.GetVariableType(variableIndex);

				return new IdentifierExpression(variableIndex, variableType);
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

				if (identifier == null)
				{
					identifier = CollapseDottedIdentifierExpression(callOrIndexExpression.Subject, mapper);

					if (identifier == null)
						throw new CompilerException("Can't translate Subject expression for CallOrIndexExpression");
				}

				// TODO: standard library functions
				// QB:
				// - Interrupt
				// - InterruptX
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

				bool isForwardReference = compilation.UnresolvedReferences.TryGetDeclaration(identifier, out var forwardReference);

				string unqualifiedIdentifier = mapper.UnqualifyIdentifier(identifier);

				if (compilation.IsRegistered(unqualifiedIdentifier) || isForwardReference)
				{
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
						forwardReference!.UnresolvedCalls.Add(translatedCallExpression);
					}

					foreach (var argument in callOrIndexExpression.Arguments.Expressions)
					{
						var translatedArgument = TranslateExpression(argument, container, mapper, compilation);

						if (translatedArgument == null)
							throw new Exception("Internal error: call argument translated to null");

						translatedCallExpression.Arguments.Add(translatedArgument);
					}

					return translatedCallExpression;
				}

				// It's not a function call, so it's an array access.
				var variableIndex = mapper.ResolveArray(identifier, out bool implicitlyCreated);

				if (implicitlyCreated)
				{
					if (container == null)
						throw new Exception("TranslateExpression needs to create an implicit array but no container was specified");

					var implicitDimStatement = new DimensionArrayStatement(null);

					implicitDimStatement.CanBreak = false;

					implicitDimStatement.VariableIndex = variableIndex;

					for (int i=0; i < callOrIndexExpression.Arguments.Expressions.Count; i++)
					{
						implicitDimStatement.Subscripts.Add(
							new IntegerLiteralValue(0),
							new IntegerLiteralValue(10));
					}

					container.Inject(implicitDimStatement);
				}

				var variableType = mapper.GetVariableType(variableIndex);

				var translatedArrayElementExpression = new ArrayElementExpression(variableIndex, variableType.MakeElementType());

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

				IEnumerable<Evaluable> arguments = Enumerable.Empty<Evaluable>();

				if (keywordFunction.Arguments != null)
				{
					arguments =
						keywordFunction.Arguments!.Expressions.Select(expr =>
							TranslateExpression(expr, container, mapper, compilation)
								?? throw new Exception("Argument expression translated to null"));
				}

				Function function;

				switch (keywordFunction.Function)
				{
					case TokenType.ASC: function = new AscFunction(); break;
					case TokenType.INT: return IntFunction.Construct(keywordFunction.Token, arguments);
					case TokenType.LEFT: function = new LeftFunction(); break;
					case TokenType.LEN: function = new LenFunction(); break;
					case TokenType.MID: function = new MidFunction(); break;
					case TokenType.PEEK: function = new PeekFunction(); break;
					case TokenType.RIGHT: function = new RightFunction(); break;
					case TokenType.RND:
						if (keywordFunction.Arguments == null)
							return RndFunction.NoParameterInstance;
						else
							function = new RndFunction();
						break;
					case TokenType.TIMER: function = new TimerFunction(); break;

					default: throw new NotImplementedException("Keyword function: " + keywordFunction.Function);
				}

				if (function is not ConstructibleFunction)
					function.SetArguments(arguments);

				return function;
			}

			case CodeModel.Expressions.UnaryExpression unaryExpression:
			{
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
					string? dottedIdentifier = CollapseDottedIdentifierExpression(binaryExpression, mapper);

					if (dottedIdentifier != null)
					{
						if (mapper.TryResolveConstant(dottedIdentifier, out var literal))
							return literal;

						if (constantValue)
						{
							var blame = binaryExpression.Left;

							while (blame is CodeModel.Expressions.BinaryExpression subBinary)
								blame = subBinary.Left;

							var blameToken = new Token(
								blame.Token?.Line ?? 0,
								blame.Token?.Column ?? 0,
								TokenType.Identifier,
								dottedIdentifier);

							throw CompilerException.InvalidConstant(blameToken);
						}

						int variableIndex = mapper.ResolveVariable(dottedIdentifier);
						var variableType = mapper.GetVariableType(variableIndex);

						return new IdentifierExpression(variableIndex, variableType);
					}
					else
					{
						var subjectExpression = TranslateExpression(binaryExpression.Left, container, mapper, compilation, constantValue);

						if (constantValue)
							throw CompilerException.InvalidConstant(binaryExpression?.Token);

						if (binaryExpression.Right is not CodeModel.Expressions.IdentifierExpression identifierExpression)
							throw new Exception("Member access expressions require the right-hand operand to be an identifier");

						string identifier = identifierExpression.Identifier;

						return FieldAccessExpression.Construct(subjectExpression, identifier);
					}
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

	string? CollapseDottedIdentifierExpression(CodeModel.Expressions.Expression expression, Mapper mapper)
	{
		if (expression is CodeModel.Expressions.BinaryExpression binaryExpression)
			return CollapseDottedIdentifierExpression(binaryExpression, mapper);
		else
			return null;
	}

	internal string? CollapseDottedIdentifierExpression(CodeModel.Expressions.BinaryExpression binaryExpression, Mapper mapper)
	{
		StringBuilder? builder = null;

		CollapseDottedIdentifierExpression(binaryExpression, mapper, ref builder);

		return builder?.ToString();
	}

	void CollapseDottedIdentifierExpression(CodeModel.Expressions.BinaryExpression binaryExpression, Mapper mapper, ref StringBuilder? identifierBuilder)
	{
		// The specific pattern we're looking for is a left tree of field access expressions where
		// every leaf is an identifier. If we identify that the tree has the correct operator and
		// an identifier on the right, then we can just recursively process the left subtree.
		//
		// When we hit the leftmost node, that's the part of the dotted identifier we're calling
		// the "slug". We can check if the current Mapper allows that slug or not.

		if (binaryExpression.Operator != CodeModel.Expressions.Operator.Field)
			return;

		if (binaryExpression.Right is not CodeModel.Expressions.IdentifierExpression rightIdentifier)
			return;

		switch (binaryExpression.Left)
		{
			case CodeModel.Expressions.IdentifierExpression leftIdentifier:
				if (!mapper.IsDisallowedSlug(leftIdentifier.Identifier))
					identifierBuilder = new StringBuilder(leftIdentifier.Identifier);
				break;
			case CodeModel.Expressions.BinaryExpression leftBinary:
				CollapseDottedIdentifierExpression(leftBinary, mapper, ref identifierBuilder);
				break;
		}

		if (identifierBuilder != null)
			identifierBuilder.Append('.').Append(rightIdentifier.Identifier);
	}
}

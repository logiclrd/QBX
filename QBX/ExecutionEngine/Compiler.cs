using QBX.ExecutionEngine.Compiled;

namespace QBX.ExecutionEngine;

public class Compiler
{
	public Compiled.Module Compile(CodeModel.CompilationUnit unit)
	{
		var module = new Compiled.Module();

		var routineByName = new Dictionary<string, Compiled.Routine>();

		// TODO: gather user types, stash them in a place where they can be looked up later on in compilation

		foreach (var element in unit.Elements)
		{
			var routine = new Routine(element);

			routineByName[routine.Name] = routine;
			// TODO: search element for SubStatement or FunctionStatement
			// TODO: if not found, default to @Main and no args
		}

		throw new NotImplementedException();
	}

	DataType ResolveType(CodeModel.DataType primitiveType, string? userType)
	{
		// TODO: implement
		// TODO: actually define the types as they're yielded by CollectUserDataTypes so that
		//       they can be used by the elements of subsequent types
		throw new NotImplementedException();
	}

	IEnumerable<UserDataType> CollectUserDataTypes(CodeModel.CompilationElement element)
	{
		UserDataType? udt = null;

		foreach (var statement in element.AllStatements)
		{
			if (udt == null)
			{
				if (statement is CodeModel.Statements.TypeStatement typeStatement)
				{
					// TODO: track whether we are in a DEF FN
					if (element.Type != CodeModel.CompilationElementType.Main)
						throw new RuntimeException(statement, "Illegal in SUB, FUNCTION or DEF FN");

					udt = new UserDataType(typeStatement.Name);
				}

				if (statement is CodeModel.Statements.EndTypeStatement)
					throw new Exception("Internal error: CompilationElement contains END TYPE statement with no matching previous TYPE statement");
			}
			else
			{
				if (statement is CodeModel.Statements.TypeElementStatement typeElementStatement)
				{
					udt.Members.Add(
						new UserDataTypeMember(
							typeElementStatement.Name,
							ResolveType(typeElementStatement.ElementType, typeElementStatement.ElementUserType)));
				}

				if (statement is CodeModel.Statements.EndTypeStatement)
				{
					yield return udt;
					udt = null;
				}

				if ((statement is not CodeModel.Statements.EmptyStatement)
				 && (statement is not CodeModel.Statements.CommentStatement))
					throw new Exception("Internal error: CompilationElement has statements inside of a TYPE definition");
			}
		}

		if (udt != null)
			throw new Exception("Internal error: CompilationElement ended in the middle of a TYPE definition");
	}
}

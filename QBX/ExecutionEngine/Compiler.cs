using QBX.ExecutionEngine.Compiled;

namespace QBX.ExecutionEngine;

public class Compiler
{
	public Compiled.Module Compile(CodeModel.CompilationUnit unit, TypeRepository typeRepository)
	{
		var module = new Compiled.Module();

		var routineByName = new Dictionary<string, Compiled.Routine>();
		var unresolvedCallStatements = new List<Compiled.Statements.CallStatement>();

		foreach (var element in unit.Elements)
		{
			foreach (var userDataType in CollectUserDataTypes(element, typeRepository))
				typeRepository.RegisterType(userDataType);

			var routine = new Routine(element, typeRepository);

			routineByName[routine.Name] = routine;

			int lineIndex = 0;
			int statementIndex = 0;

			while (lineIndex < element.Lines.Count)
				ParseStatement(element, ref lineIndex, ref statementIndex, routine);
		}

		throw new NotImplementedException();
	}

	void ParseStatement(CodeModel.CompilationElement element, ref int lineIndex, ref int statementIndex, ISequence container)
	{
		if (lineIndex >= element.Lines.Count)
			return;

		var line = element.Lines[lineIndex];

		if (statementIndex >= line.Statements.Count)
		{
			lineIndex++;
			statementIndex = 0;
			return;
		}

		var statement = line.Statements[statementIndex];

		/*
		switch (statement.Type)
		{

		}
		*/
	}

	IEnumerable<UserDataType> CollectUserDataTypes(CodeModel.CompilationElement element, TypeRepository typeRepository)
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

					udt = new UserDataType(typeStatement);
				}

				if (statement is CodeModel.Statements.EndTypeStatement)
					throw new Exception("Internal error: CompilationElement contains END TYPE statement with no matching previous TYPE statement");
			}
			else
			{
				if (statement is CodeModel.Statements.TypeElementStatement typeElementStatement)
				{
					var type = typeRepository.ResolveType(
						typeElementStatement.ElementType,
						typeElementStatement.ElementUserType,
						typeElementStatement.TypeToken);

					udt.Members.Add(
						new UserDataTypeMember(
							typeElementStatement.Name,
							type));
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

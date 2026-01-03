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

	IEnumerable<UserDataType> ExtractUserDataTypes(CodeModel.CompilationElement element)
	{
		for (int i = 0; i < element.Lines.Count; i++)
		{
			//if (element.Lines[i] is 
		}
		throw new NotImplementedException();
	}


}

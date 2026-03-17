using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using QBX.ExecutionEngine.Execution;

namespace QBX.ExecutionEngine.Compiled;

public class Module
{
	public Routine? MainRoutine;
	public Dictionary<string, Routine> Routines = new Dictionary<string, Routine>();
	public DataParser DataParser = new DataParser();
	public UnresolvedReferences UnresolvedReferences;

	public StackFrame? ModuleFrame => _moduleFrame;

	StackFrame? _moduleFrame;

	public void SetModuleFrame(StackFrame moduleFrame)
	{
		if (!moduleFrame.IsModuleFrame)
			throw new Exception("Internal error: Trying to set a non-module frame as module frame");

		_moduleFrame = moduleFrame;
	}

	public ErrorHandler? MainErrorHandler;

	// SUBs and FUNCTIONs:
	//   In each module, each SUB/FUNCTION can have its own declared signature.
	//   These must line up in order to be compatible, but the names of
	//   parameters and any user-defined type facades can vary.
	public Dictionary<string, RoutineFacade> SubFacades;
	public Dictionary<string, RoutineFacade> FunctionFacades;
	public Dictionary<string, NativeProcedure> NativeProcedures;

	public Module(Compilation compilation)
	{
		UnresolvedReferences = new UnresolvedReferences(compilation, this);

		SubFacades = new Dictionary<string, RoutineFacade>(StringComparer.OrdinalIgnoreCase);
		FunctionFacades = new Dictionary<string, RoutineFacade>(StringComparer.OrdinalIgnoreCase);

		NativeProcedures = compilation.NativeProcedures.ToDictionary(
			key => key.Name,
			element => element.Clone(),
			StringComparer.OrdinalIgnoreCase);
	}

	public bool IsRegistered(string identifier)
		=> Routines.ContainsKey(identifier) || NativeProcedures.ContainsKey(identifier);

	public void AddSubFacade(string name, RoutineFacade facade)
		=> SubFacades.Add(name, facade);
	public void AddFunctionFacade(string name, RoutineFacade facade)
		=> FunctionFacades.Add(name, facade);

	public bool TryGetSubFacade(string name, [NotNullWhen(true)] out RoutineFacade? facade)
		=> SubFacades.TryGetValue(name, out facade);
	public bool TryGetFunctionFacade(string name, [NotNullWhen(true)] out RoutineFacade? facade)
		=> FunctionFacades.TryGetValue(name, out facade);

	public bool TryGetNativeProcedure(string name, [NotNullWhen(true)] out NativeProcedure? procedure)
		=> NativeProcedures.TryGetValue(name, out procedure);

	// Error handling: module-level error handlers are assigned per-module and only handle
	// errors that occur within that module
	public void SetErrorHandler(ErrorResponse response, StatementPath? handlerPath = null)
	{
		if ((response == ErrorResponse.ExecuteHandler) && (handlerPath == null))
			throw new Exception("Internal error: SetErrorHandler called with ErrorResponse.ExecuteHandler but no handlerPath");

		MainErrorHandler ??= new ErrorHandler() { StackFrame = ModuleFrame };
		MainErrorHandler.Response = response;
		MainErrorHandler.HandlerPath = handlerPath;
	}

	public void ClearErrorHandler(CodeModel.Statements.Statement source)
	{
		if (ModuleFrame!.IsHandlingError)
		{
			// If this stack frame is already handling an error, the dispatch
			// has pulled the handler off of the error handlers stack and
			// will be reinstalling it on resume. That reinstallation doesn't
			// support clearing the handler. This matches QuickBASIC's behaviour.
			throw RuntimeException.IllegalFunctionCall(source);
		}

		MainErrorHandler = null;
	}
}

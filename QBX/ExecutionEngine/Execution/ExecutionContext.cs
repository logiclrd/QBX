using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

using QBX.ExecutionEngine.Compiled;
using QBX.ExecutionEngine.Compiled.Statements;
using QBX.ExecutionEngine.Execution.Events;
using QBX.ExecutionEngine.Execution.Variables;
using QBX.Firmware;
using QBX.Hardware;
using QBX.Parser;

namespace QBX.ExecutionEngine.Execution;

// ON [LOCAL] ERROR:
// - error handler state lives in the stack frame, which is by definition
//   the root frame if "LOCAL" isn't specified
// - handler must live in the same Routine as the stack frame it's registered in
// - ON ERROR (non-local) can be run at any time in any context and changes the
//   registered handler in the module frame
// - execution of the handler re-enters the associated stack frame if necessary
// - the execution of the handler is modelled as a type of Call
// - RESUME retries the statement that failed
// - RESUME NEXT also goes back but skips the statement that failed
// - if an error happens and there's already a return location from a previous
//   error being handled, then it doesn't get handled
// - if code flows off the end of the routine (as opposed to the program being explicitly
//   ENDed or EXIT SUB/FUNCTIONed) then an error "No RESUME" is raised

public class ExecutionContext
{
	public Machine Machine;
	public PlayProcessor PlayProcessor;
	public DrawProcessor DrawProcessor;

	public VisualLibrary VisualLibrary;

	public StackFrame? RootFrame => _rootFrame;

	public IReadOnlyExecutionState ExecutionState => _executionState;
	public IExecutionControls Controls => _executionState;

	public RuntimeState RuntimeState => _runtimeState;

	ExecutionState _executionState;

	Dictionary<Identifier, CommonBlockStorage> _commonBlocks;

	StackFrame? _rootFrame;
	StatementPath? _goTo;

	ManualResetEvent _rootFrameEstablished = new ManualResetEvent(initialState: false);

	Stack<ErrorHandler> _localErrorHandlers = new Stack<ErrorHandler>();

	RuntimeState _runtimeState = new RuntimeState();

	public readonly ErrorNumberVariable ErrVariable = new ErrorNumberVariable();
	public readonly LongVariable ErlVariable = new LongVariable();

	public readonly StringValue CommandLine = new StringValue();

	public readonly Dictionary<ushort, SurfacedVariable> SurfacedVariables = new();
	public ushort NextSurfacedVariableKey = 1;

	ushort AddSurfacedVariable(SurfacedVariable surfacedVariable)
	{
		ushort key = NextSurfacedVariableKey++;

		SurfacedVariables[key] = surfacedVariable;

		return key;
	}

	public ushort SurfaceVariable(StackFrame stackFrame, int variableIndex)
	{
		var surfacedVariable = new SurfacedVariable();

		surfacedVariable.StackFrame = new WeakReference<StackFrame>(stackFrame);
		surfacedVariable.Index = variableIndex;

		return AddSurfacedVariable(surfacedVariable);
	}

	public ushort SurfaceVariable(ArrayVariable array, int arrayIndex)
	{
		var surfacedVariable = new SurfacedVariable();

		surfacedVariable.Array = new WeakReference<ArrayVariable>(array);
		surfacedVariable.Index = arrayIndex;

		return AddSurfacedVariable(surfacedVariable);
	}

	public Variable? GetSurfacedVariable(ushort key)
	{
		if (!SurfacedVariables.TryGetValue(key, out var surfacedVariable))
			return null;

		if (surfacedVariable.Get() is not Variable variable)
		{
			SurfacedVariables.Remove(key);
			return null;
		}

		return variable;
	}

	public readonly Dictionary<int, OpenFile> Files = new Dictionary<int, OpenFile>();

	public void CloseAllFiles()
	{
		foreach (var file in Files.Values)
			Machine.DOS.CloseFile(file.FileHandle);

		Files.Clear();
	}

	readonly Dictionary<StringVariable, List<OpenFile>> _openFilesByFieldVariable = new Dictionary<StringVariable, List<OpenFile>>();
	readonly Dictionary<OpenFile, List<StringVariable>> _fieldVariablesByOpenFile = new Dictionary<OpenFile, List<StringVariable>>();

	public void AddFieldVariableLink(StringVariable variable, OpenFile openFile)
	{
		if (!_openFilesByFieldVariable.TryGetValue(variable, out var openFiles))
		{
			openFiles = new List<OpenFile>();
			_openFilesByFieldVariable[variable] = openFiles;
		}

		if (!openFiles.Contains(openFile))
			openFiles.Add(openFile);

		if (!_fieldVariablesByOpenFile.TryGetValue(openFile, out var fieldVariables))
		{
			fieldVariables = new List<StringVariable>();
			_fieldVariablesByOpenFile[openFile] = fieldVariables;
		}

		if (!fieldVariables.Contains(variable))
			fieldVariables.Add(variable);

		variable.IsMappedFieldCount++;
	}

	public void RemoveAllFieldVariableLinks(OpenFile openFile)
	{
		if (_fieldVariablesByOpenFile.TryGetValue(openFile, out var fieldVariablesForOpenFile))
		{
			foreach (var fieldVariable in fieldVariablesForOpenFile)
			{
				if (_openFilesByFieldVariable.TryGetValue(fieldVariable, out var openFilesForFieldVariable))
				{
					openFilesForFieldVariable.Remove(openFile);
					if (openFilesForFieldVariable.Count == 0)
						_openFilesByFieldVariable.Remove(fieldVariable);

					fieldVariable.IsMappedFieldCount--;
				}
			}

			_fieldVariablesByOpenFile.Remove(openFile);
		}
	}

	public void UnlinkFieldVariable(StringVariable variable)
	{
		if (_openFilesByFieldVariable.TryGetValue(variable, out var openFiles))
		{
			foreach (var openFile in openFiles)
			{
				openFile.UnlinkFieldVariable(variable);

				if (_fieldVariablesByOpenFile.TryGetValue(openFile, out var fieldVariablesForFile))
				{
					fieldVariablesForFile.Remove(variable);
					if (fieldVariablesForFile.Count == 0)
						_fieldVariablesByOpenFile.Remove(openFile);

					variable.IsMappedFieldCount--;
				}
			}

			_openFilesByFieldVariable.Remove(variable);
		}
	}

	public EventHub EventHub { get; }
	public EventCheckGranularity EventCheckGranularity { get; set; } = EventCheckGranularity.EveryStatement;

	List<int> _queuedPinnedMemoryReleases = new List<int>();
	Lock _sync = new Lock();

	public void QueuePinnedMemoryRelease(int pinnedMemoryAddress)
	{
		lock (_sync)
			_queuedPinnedMemoryReleases.Add(pinnedMemoryAddress);
	}

	public void ReleasePinnedMemory()
	{
		lock (_sync)
		{
			foreach (var memoryAddress in _queuedPinnedMemoryReleases)
				Machine.DOS.MemoryManager.FreeMemory(memoryAddress);

			_queuedPinnedMemoryReleases.Clear();
		}
	}

	public ExecutionContext(Machine machine, PlayProcessor playProcessor, EventHub eventHub)
	{
		_executionState = new ExecutionState();

		Machine = machine;
		PlayProcessor = playProcessor;
		EventHub = eventHub;

		VisualLibrary = Machine.VideoFirmware.VisualLibrary;

		DrawProcessor = new DrawProcessor();

		_executionState.EnterExecution += AttachKeyEventInterceptor;
		_executionState.ExitExecution += DetachKeyEventInterceptor;

		_commonBlocks = new Dictionary<Identifier, CommonBlockStorage>();
	}

	void AttachKeyEventInterceptor()
	{
		Machine.Keyboard.InterceptKeyEvent += Keyboard_InterceptKeyEvent;
	}

	void DetachKeyEventInterceptor()
	{
		Machine.Keyboard.InterceptKeyEvent -= Keyboard_InterceptKeyEvent;
	}

	bool Keyboard_InterceptKeyEvent(KeyEvent keyEvent)
	{
		if (keyEvent.IsRelease)
			return false;

		return EventHub.PostKeyEvent(keyEvent);
	}

	void AttachEvents()
	{
		Machine.MouseDriver.PositionChanged += MouseDriver_PositionChanged;
		PlayProcessor.QueueLengthChanged += PlayProcessor_QueueLengthChanged;
	}

	void DetachEvents()
	{
		Machine.MouseDriver.PositionChanged -= MouseDriver_PositionChanged;
		PlayProcessor.QueueLengthChanged -= PlayProcessor_QueueLengthChanged;
	}

	void MouseDriver_PositionChanged()
	{
		if (Machine.MouseDriver.LightPenEmulationEnabled
			&& Machine.MouseDriver.LightPenIsDown)
				EventHub.PostEvent(EventType.Pen);
	}

	int _lastPlayProcessorQueueLength;

	void PlayProcessor_QueueLengthChanged()
	{
		int triggerLength = EventHub.Configuration.PlayQueueTriggerLength;

		int currentPlayProcessorQueueLength = PlayProcessor.QueueLength;

		if ((_lastPlayProcessorQueueLength > triggerLength)
		 && (currentPlayProcessorQueueLength <= triggerLength))
			EventHub.PostEvent(EventType.Play);

		_lastPlayProcessorQueueLength = currentPlayProcessorQueueLength;
	}

	Dictionary<Event, (Module Module, StatementPath Path)> _eventHandlers = new();

	public void SetEventHandler(Event evt, Module module, StatementPath handlerPath)
	{
		_eventHandlers[evt] = (module, handlerPath);
	}

	public void ClearEventHandler(Event evt)
	{
		_eventHandlers.Remove(evt);
	}

	StatementPath? _returnFromEventHandlerSurrogatePath = null;

	void HandleEvent(Event evt, Executable currentStatement, StackFrame currentStackFrame)
	{
		if (_eventHandlers.TryGetValue(evt, out var handler))
		{
			using (EventHub.SuspendAllEvents())
			{
				_returnFromEventHandlerSurrogatePath = new StatementPath();

				try
				{
					var handlerFrame = handler.Module.ModuleFrame ?? throw new Exception("Internal error: Module with no ModuleFrame");

					handlerFrame.PushReturnPath(_returnFromEventHandlerSurrogatePath);

					_goTo = handler.Path.Clone();

					Call(handlerFrame.Routine, handlerFrame, enterRoutine: currentStackFrame != handlerFrame);
				}
				finally
				{
					_returnFromEventHandlerSurrogatePath = null;
				}
			}
		}
	}

	public void SetLocalErrorHandler(StackFrame stackFrame, ErrorResponse response, StatementPath? handlerPath = null)
	{
		if ((response == ErrorResponse.ExecuteHandler) && (handlerPath == null))
			throw new Exception("Internal error: SetLocalErrorHandler called with ErrorResponse.ExecuteHandler but no handlerPath");

		if (stackFrame.IsHandlingError)
		{
			// If this stack frame is already handling an error, then
			//
			// * We don't want to be able to re-entrantly catch errors
			//   that happen during the handling.
			// * The dispatch of the error handler has pulled the
			//   handler off the stack and will be reinstalling it on
			//   exit.
			//
			// So, we just stash the change and defer processing.

			var newHandler = new ErrorHandler();

			newHandler.StackFrame = stackFrame;
			newHandler.Response = response;
			newHandler.HandlerPath = handlerPath;

			stackFrame.NewLocalErrorHandler = newHandler;

			return;
		}

		// If the top of the stack is not a match for the stack frame,
		// then it is further up the stack, so we need to make a new
		// error handler frame.

		if (_localErrorHandlers.TryPeek(out var handler))
		{
			if (handler.StackFrame != stackFrame)
				handler = null;
		}

		if (handler == null)
		{
			handler = new ErrorHandler();
			handler.StackFrame = stackFrame;
		}

		handler.Response = response;
		handler.HandlerPath = handlerPath;
	}

	public void ClearLocalErrorHandler(StackFrame stackFrame, CodeModel.Statements.Statement? source)
	{
		if (stackFrame.IsHandlingError)
		{
			// If this stack frame is already handling an error, the dispatch
			// has pulled the handler off of the error handlers stack and
			// will be reinstalling it on resume. That reinstallation doesn't
			// support clearing the handler. This matches QuickBASIC's behaviour.
			throw RuntimeException.IllegalFunctionCall(source);
		}

		if (_localErrorHandlers.TryPeek(out var handler)
		 && (handler.StackFrame == stackFrame))
			_localErrorHandlers.Pop();
	}

	public bool WaitForRootFrame()
		=> _rootFrameEstablished.WaitOne(TimeSpan.FromSeconds(5));

	public int Run(Compilation compilation)
	{
		var entrypoint = compilation.EntrypointRoutine;

		if (entrypoint == null)
			throw new Exception("The Compilation's EntrypointRoutine is not set");

		_commonBlocks.Clear();

		foreach (var block in compilation.CommonBlocks)
			_commonBlocks[block.Key] = block.Value.CreateStorage();

		foreach (var module in compilation.Modules)
		{
			module.SetModuleFrame(CreateFrame(
				module,
				module.MainRoutine ?? throw new Exception("Internal error: Module has no MainRoutine"),
				arguments: System.Array.Empty<Variable>(),
				isModuleFrame: true));
		}

		_rootFrame = entrypoint.Module.ModuleFrame!;

		_executionState.StartExecution(_rootFrame);

		_rootFrameEstablished.Set();

		try
		{
			AttachEvents();

			try
			{
				Call(entrypoint, _rootFrame);
			}
			catch (EndProgram) { }

			int exitCode = _rootFrame.Variables[0].CoerceToInt(context: null);

			return exitCode;
		}
		catch (GoTo)
		{
			Debugger.Break();
			throw new Exception("Internal error: GoTo was thrown with a TargetFrame that didn't match anything");
		}
		catch (TerminatedException)
		{
			return -1;
		}
		finally
		{
			DetachEvents();

			_rootFrameEstablished.Reset();
			_rootFrame = null;

			_executionState.EndExecution();
		}
	}

	public void SetExitCode(int exitCode)
	{
		_rootFrame?.Variables[0].SetData(exitCode);
	}

	public void Dispatch(Executable? executable, StackFrame stackFrame)
	{
		if (executable != null)
		{
			if (_goTo != null)
			{
				int subsequenceIndex = _goTo.Pop();

				// I don't think this should actually happen, it should always
				// end on an index into a sequence. Better safe than sorry, though.
				if (_goTo.Count == 0)
					_goTo = null;

				if (executable.SelfSequenceDispatch)
					executable.Dispatch(this, stackFrame, subsequenceIndex, ref _goTo);
				else
				{
					var subsequence = executable.GetSequenceByIndex(subsequenceIndex);

					if (subsequence == null)
						throw new Exception("Internal Error: ExecutionPath specified subsequence " + subsequenceIndex + " within a " + executable.GetType().Name + " and it does not exist");

					Dispatch(subsequence, stackFrame);
				}
			}
			else
			{
				bool retryStatement = false;

				do
				{
					bool shouldCheckForEvents =
						(EventCheckGranularity == EventCheckGranularity.EveryStatement) ||
						executable.IsLabel;

					if (shouldCheckForEvents && EventHub.HaveEvents)
					{
						if (EventHub.TryGetEvent(out Event evt))
							HandleEvent(evt, executable, stackFrame);
					}

					if (executable.CanBreak)
						_executionState.NextStatement(executable.Source);

					RuntimeException.LastLineNumber = executable.LineNumberForErrorReporting;

					try
					{
						executable.Execute(this, stackFrame);
					}
					catch (RuntimeException error)
					{
						ErrorHandler? handler = null;

						if (!stackFrame.IsHandlingError)
						{
							if (_localErrorHandlers.TryPeek(out handler))
							{
								if (handler.StackFrame == stackFrame)
									_localErrorHandlers.Pop();
								else
									handler = null;
							}

							handler ??= stackFrame.Module.MainErrorHandler;
						}

						if (handler != null)
							CallErrorHandler(handler, error, switchFrame: handler.StackFrame != stackFrame, out retryStatement);
						else
						{
							_executionState.Error(error);
							retryStatement = true;
						}
					}
				} while (retryStatement);
			}
		}
	}

	public void Dispatch(Sequence? sequence, StackFrame stackFrame)
	{
		if (sequence != null)
		{
			int startIndex = 0;

			if (_goTo != null)
			{
				startIndex = _goTo.Pop();
				if (_goTo.Count == 0)
					_goTo = null;
			}
			else
			{
				foreach (var statement in sequence.InjectedStatements)
					Dispatch(statement, stackFrame);
			}

			for (int i = startIndex; i < sequence.Count; i++)
				Dispatch(sequence[i], stackFrame);
		}
	}

	private void CallErrorHandler(ErrorHandler handler, RuntimeException error, bool switchFrame, out bool retryStatement)
	{
		retryStatement = false;

		ErrVariable.Value = (short)error.ErrorNumber;
		ErlVariable.Value = error.LineNumber;

		if (handler.Response == ErrorResponse.SkipStatement)
			return;

		var stackFrame = handler.StackFrame ?? throw new Exception("Internal error: ErrorHandler does not have StackFrame set");
		var handlerPath = handler.HandlerPath ?? throw new Exception("Internal error: ErrorHandler's Response is not SkipStatement but it has no HandlerPath set");

		try
		{
			stackFrame.IsHandlingError = true;

			_goTo = handler.HandlerPath.Clone();

			Call(stackFrame.Routine, stackFrame, enterRoutine: switchFrame, handlingError: true);

			// TODO: find end token of the compilation element
			throw RuntimeException.NoResume(stackFrame.CurrentStatement);
		}
		catch (ExitRoutine exitRoutine)
		{
			exitRoutine.StackFrame = stackFrame;
			throw;
		}
		catch (Resume resume)
		{
			stackFrame.IsHandlingError = false;

			if (!handler.StackFrame.IsModuleFrame)
				_localErrorHandlers.Push(stackFrame.NewLocalErrorHandler ?? handler);

			if (resume.GoTo != null)
				throw resume.GoTo;

			retryStatement = resume.RetryStatement;
		}
		finally
		{
			stackFrame.IsHandlingError = false;

			ErrVariable.Value = 0;
			ErlVariable.Value = 0;
		}
	}

	static Variable s_dummyVariable = new DummyVariable();

	public Variable Call(Routine routine, Variable[] arguments)
	{
		StackFrame frame;

		if (routine.UseModuleFrame)
		{
			frame = routine.Module.ModuleFrame ?? throw new Exception("No module frame");

			for (int i=0; i < arguments.Length; i++)
				frame.Variables[routine.ParameterVariableIndices[i]] = arguments[i];
		}
		else
			frame = CreateFrame(routine.Module, routine, arguments);

		try
		{
			return Call(routine, frame);
		}
		finally
		{
			ClearLocalErrorHandler(frame, source: null);
		}
	}

	Variable Call(Routine routine, StackFrame frame, bool enterRoutine = true, bool handlingError = false)
	{
		if (enterRoutine)
			_executionState.EnterRoutine(routine, frame);

		try
		{
		goTo_:
			try
			{
				Dispatch(routine, frame);
			}
			catch (GoTo goTo)
			{
				if ((goTo.TargetFrame != null) && (goTo.TargetFrame != frame))
					throw; // "RESUME linenumber/label" in a different stack frame -- keep searching

				if (goTo.StatementPath == _returnFromEventHandlerSurrogatePath)
					return s_dummyVariable;

				_goTo = goTo.StatementPath.Clone();
				goto goTo_;
			}
			catch (ExitRoutine exitRoutine)
			{
				if ((exitRoutine.StackFrame != null)
				 && (exitRoutine.StackFrame != frame))
				{
					// This ExitRoutine came from an EXIT SUB/EXIT FUNCTION inside an
					// event handler that was handling an error from further down the
					// call stack.
					//
					//   SUB a                                1
					//   |- ON LOCAL ERROR GOTO x             1
					//   |- b                                 1
					//   `- SUB b                             2
					//      :                                 2
					//      :  d                              2
					//      `- SUB d                          3
					//         |- ERROR                       3
					//         `- SUB a:handler               1
					//            `- EXIT SUB => ExitRoutine  1
					//
					// (2 here represents any number of intermediary calls, including
					// possibly none)
					//
					// Since StackFrame doesn't match, we're frame 3, and we need
					// to keep walking up the stack so that 1 can process this
					// ExitRoutine.
					throw;
				}
			}

			if (routine.ReturnType != null)
				return frame.Variables[routine.ReturnValueVariableIndex];
			else
				return s_dummyVariable;
		}
		finally
		{
			if (enterRoutine)
				_executionState.ExitRoutine();
		}
	}

	StackFrame CreateFrame(Module module, Routine routine, Variable[] arguments, bool isModuleFrame = false)
	{
		var variableTypes = routine.VariableTypes;
		var commonVariableLinkGroups = routine.CommonVariableLinkGroups;
		var linkedVariables = routine.LinkedVariables;

		if (arguments.Length > variableTypes.Count)
			throw new Exception("Internal error: Variable slots not properly allocated for arguments");

		int totalSlots = variableTypes.Count;

		var variables = new Variable[totalSlots];

		int argumentOffset = routine.ReturnType != null ? 1 : 0;

		arguments.CopyTo(variables, argumentOffset);

		if (!isModuleFrame)
		{
			if (commonVariableLinkGroups.Count > 0)
				throw new Exception("Internal error: Creating frame with common variable link groups when this is not a module frame");

			var moduleFrame = module.ModuleFrame ?? throw new Exception("Internal: Module should already have ModuleFrame");

			foreach (var link in linkedVariables)
				variables[link.LocalIndex] = moduleFrame.Variables[link.RemoteIndex];
		}
		else
		{
			if (linkedVariables.Count > 0)
				throw new Exception("Internal error: Creating frame with linked variables when there is no module frame yet");

			foreach (var linkGroup in commonVariableLinkGroups)
			{
				if (!_commonBlocks.TryGetValue(linkGroup.CommonBlockName, out var commonBlock))
					throw new Exception("Internal error: Couldn't find common block /" + linkGroup.CommonBlockName + "/");

				foreach (var link in linkGroup.LinkedVariables)
					variables[link.LocalIndex] = commonBlock.Variables[link.RemoteIndex];
			}
		}

		for (int i = 0; i < variableTypes.Count; i++)
		{
			if (variables[i] == null)
			{
				var type = variableTypes[i];

				if (type.IsArray)
					variables[i] = Variable.ConstructArray(type);
				else
					variables[i] = Variable.Construct(type);
			}
		}

		var stackFrame = new StackFrame(routine, variables);

		stackFrame.IsModuleFrame = isModuleFrame;

		foreach (var staticArrayInitializer in routine.StaticArrays)
			staticArrayInitializer.Execute(this, stackFrame);

		return stackFrame;
	}

	public void Reset()
	{
		foreach (var openFile in Files.Values)
		{
			try
			{
				Machine.DOS.CloseFile(openFile.FileHandle);
			}
			catch { }
		}

		Files.Clear();

		_rootFrame?.Reset();

		GC.Collect();
		GC.WaitForPendingFinalizers();

		ReleasePinnedMemory();
	}
}

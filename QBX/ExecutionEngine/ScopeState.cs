namespace QBX.ExecutionEngine;

public struct ScopeState(CodeModel.Statements.ScopeType rootScope)
{
	public readonly CodeModel.Statements.ScopeType RootScope = rootScope;
	public bool InDoLoop;
	public bool InForNext;

	public static ScopeState BeginMainModule()
	{
		return new ScopeState(CodeModel.Statements.ScopeType.None);
	}

	public static ScopeState BeginDef()
	{
		return new ScopeState(CodeModel.Statements.ScopeType.Def);
	}

	public static ScopeState BeginSub()
	{
		return new ScopeState(CodeModel.Statements.ScopeType.Sub);
	}

	public static ScopeState BeginFunction()
	{
		return new ScopeState(CodeModel.Statements.ScopeType.Function);
	}

	public ScopeState EnterDo()
	{
		var subscope = this;

		subscope.InDoLoop = true;

		return subscope;
	}

	public ScopeState EnterFor()
	{
		var subscope = this;

		subscope.InForNext = true;

		return subscope;
	}
}

using System;

namespace QBX.ExecutionEngine.Execution;

public class RuntimeState
{
	public int SegmentBase;
	public bool EnablePaletteRemapping = true;

	public RuntimeState()
	{
		SegmentBase = GetDataSegmentBase();
	}

	public int GetDataSegmentBase()
	{
		return 0x40000;
	}
}

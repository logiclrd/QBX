namespace QBX.ExecutionEngine.Execution;

public class RuntimeState
{
	public int SegmentBase;
	public bool EnablePaletteRemapping = true;

	// TODO: soft key mapping & display on screen

	public RuntimeState()
	{
		SegmentBase = GetDataSegmentBase();
	}

	public int GetDataSegmentBase()
	{
		return 0x40000;
	}
}

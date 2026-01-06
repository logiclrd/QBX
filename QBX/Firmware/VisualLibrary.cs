using QBX.Hardware;

namespace QBX.Firmware;

public abstract class VisualLibrary
{
	public readonly GraphicsArray Array;

	public VisualLibrary(GraphicsArray array)
	{
		Array = array;

		RefreshParameters();
	}

	public int Width;
	public int Height;

	public int StartAddress;

	public int CursorX = 0;
	public int CursorY = 0;

	public abstract void RefreshParameters();

	public bool SetActivePage(int pageNumber)
	{
		int pageSize = Video.ComputePageSize(Array);
		int pageCount = 16384 / pageSize;

		if ((pageNumber >= 0) && (pageNumber < pageCount))
		{
			StartAddress = pageNumber * pageSize;
			RefreshParameters();

			return true;
		}

		return false;
	}
}

using QBX.Hardware;
using System;
using System.Collections.Generic;
using System.Text;

namespace QBX.Firmware;

public abstract class LibraryBase
{
	public readonly GraphicsArray Array;

	public LibraryBase(GraphicsArray array)
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

	public void SetActivePage(int pageNumber)
	{
		int pageSize = Video.ComputePageSize(Array);
		int pageCount = 16384 / pageSize;

		if ((pageNumber >= 0) && (pageNumber < pageCount))
		{
			StartAddress = pageNumber * pageSize;
			RefreshParameters();
		}
	}
}

namespace QBX.Hardware;

public enum GraphicsArrayVRAMWriteMode
{
	Direct = 0,
	Latched = 1, // uses read latches as data source -- NB: read latches only populated if read via array indexer
	BulkPlanar = 2, // expand bits 0-3 to full bytes in planes 0-3, still subject to write enable
	RenderBitmap = 3, // take attribute from Set/Reset, expand bits 0-7 across 8 adjacent pixels

	Source_CPU = 0,
	Source_Latch = 1,
	SourceMask = 1,
}


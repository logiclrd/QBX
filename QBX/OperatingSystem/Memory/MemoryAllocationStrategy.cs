namespace QBX.OperatingSystem.Memory;

public enum MemoryAllocationStrategy
{
	FirstFit = 0,
	BestFit = 1,
	LastFit = 2,

	LowMemory = 0x00,
	HighMemory = 0x80,
	HighMemoryOnly = 0x40,

	FirstFitLow = LowMemory | FirstFit,
	BestFitLow = LowMemory | BestFit,
	LastFitLow = LowMemory | LastFit,

	FirstFitHigh = HighMemory | FirstFit,
	BestFitHigh = HighMemory | BestFit,
	LastFitHigh = HighMemory | LastFit,

	FirstFitHighOnly = HighMemoryOnly | FirstFit,
	BestFitHighOnly = HighMemoryOnly | BestFit,
	LastFitHighOnly = HighMemoryOnly | LastFit,

	StrategyMask = FirstFit | BestFit | LastFit,
	AreaMask = LowMemory | HighMemory | HighMemoryOnly,
}

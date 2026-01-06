namespace QBX.Firmware;

public class ModeParameters
{
	public int? ScreenNumber; // QB
	public (int Width, int Height) Pixels;
	public (int Width, int Height) Characters;
	public bool IsGraphicsMode;
	public bool IsMonochrome;
	public PaletteType PaletteType;
	public BaseAddress BaseAddress;
	public CharacterWidth CharacterWidth;
	public int CharacterHeight;
	public int PixelsPerAddress = 1;
	public int MemoryAddressSize;
	public bool ScanDoubling;
	public bool OddEvenAddressing;
	public bool ShiftRegisterInterleave;
	public bool Use256Colours;
	public byte PlaneMask;
	public bool HalfDotClockRate;
	public bool Chain4Mode;
}

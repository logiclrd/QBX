namespace QBX.Firmware;

public partial class Video
{
	// 1: 40x25 text mode rendered to 360x400 dots
	static readonly ModeParameters Mode1 =
		new ModeParameters()
		{
			ScreenNumber = 0,
			Characters = (40, 25),
			IsGraphicsMode = false,
			PaletteType = PaletteType.EGA,
			BaseAddress = BaseAddress.B800,
			CharacterWidth = CharacterWidth._9,
			CharacterHeight = 16,
			MemoryAddressSize = 1,
			ScanDoubling = false,
			OddEvenAddressing = true,
			ShiftRegisterInterleave = false,
			Use256Colours = false,
			PlaneMask = 0b0011,
			HalfDotClockRate = true,
			Chain4Mode = false,
		};

	// 3: 80x25 text mode rendered to 720x400 dots
	static readonly ModeParameters Mode3 =
		new ModeParameters()
		{
			ScreenNumber = 0,
			Characters = (80, 25),
			IsGraphicsMode = false,
			PaletteType = PaletteType.EGA,
			BaseAddress = BaseAddress.B800,
			CharacterWidth = CharacterWidth._9,
			CharacterHeight = 16,
			MemoryAddressSize = 1,
			ScanDoubling = false,
			OddEvenAddressing = true,
			ShiftRegisterInterleave = false,
			Use256Colours = false,
			PlaneMask = 0b0011,
			HalfDotClockRate = false,
			Chain4Mode = false,
		};

	// 5: 320x200 graphics using planes 0 and 1 for pixels
	static readonly ModeParameters Mode5 =
		new ModeParameters()
		{
			ScreenNumber = 1,
			Pixels = (320, 200),
			Characters = (40, 25),
			IsGraphicsMode = true,
			PaletteType = PaletteType.CGA,
			BaseAddress = BaseAddress.B800,
			CharacterWidth = CharacterWidth._8,
			CharacterHeight = 8,
			PixelsPerAddress = 4,
			MemoryAddressSize = 2,
			ScanDoubling = true,
			OddEvenAddressing = false,
			ShiftRegisterInterleave = true,
			Use256Colours = false,
			PlaneMask = 0,
			HalfDotClockRate = true,
			Chain4Mode = false,
		};

	// 6: 640x200 graphics emulating monochrome by aliasing all the planes together
	static readonly ModeParameters Mode6 =
		new ModeParameters()
		{
			ScreenNumber = 2,
			Pixels = (640, 200),
			Characters = (80, 25),
			IsGraphicsMode = true,
			IsMonochrome = true,
			PaletteType = PaletteType.EGA,
			BaseAddress = BaseAddress.B800,
			CharacterWidth = CharacterWidth._8,
			CharacterHeight = 8,
			PixelsPerAddress = 8,
			MemoryAddressSize = 2,
			ScanDoubling = true,
			OddEvenAddressing = false,
			ShiftRegisterInterleave = false,
			Use256Colours = false,
			PlaneMask = 0b1111,
			HalfDotClockRate = false,
			Chain4Mode = false,
		};

	// 7: 80x25 text rendered to 720x400 dots
	static readonly ModeParameters Mode7 =
		new ModeParameters()
		{
			Characters = (80, 25),
			IsGraphicsMode = false,
			PaletteType = PaletteType.EGA,
			BaseAddress = BaseAddress.B800,
			CharacterWidth = CharacterWidth._9,
			CharacterHeight = 16,
			MemoryAddressSize = 1,
			ScanDoubling = false,
			OddEvenAddressing = false,
			ShiftRegisterInterleave = false,
			Use256Colours = false,
			PlaneMask = 0b0011,
			HalfDotClockRate = false,
			Chain4Mode = false,
		};

	// D: 320x200 graphics with 4bpp planar colour
	static readonly ModeParameters ModeD =
		new ModeParameters()
		{
			ScreenNumber = 7,
			Pixels = (320, 200),
			Characters = (40, 25),
			IsGraphicsMode = true,
			PaletteType = PaletteType.EGA,
			BaseAddress = BaseAddress.A000,
			CharacterWidth = CharacterWidth._8,
			CharacterHeight = 8,
			PixelsPerAddress = 8,
			MemoryAddressSize = 1,
			ScanDoubling = true,
			OddEvenAddressing = false,
			ShiftRegisterInterleave = false,
			Use256Colours = false,
			PlaneMask = 0b1111,
			HalfDotClockRate = true,
			Chain4Mode = false,
		};

	// E: 640x200 graphics with 4bpp planar colour
	static readonly ModeParameters ModeE =
		new ModeParameters()
		{
			ScreenNumber = 8,
			Pixels = (640, 200),
			Characters = (80, 25),
			IsGraphicsMode = true,
			PaletteType = PaletteType.EGA,
			BaseAddress = BaseAddress.A000,
			CharacterWidth = CharacterWidth._8,
			CharacterHeight = 8,
			PixelsPerAddress = 8,
			MemoryAddressSize = 1,
			ScanDoubling = true,
			OddEvenAddressing = false,
			ShiftRegisterInterleave = false,
			Use256Colours = false,
			PlaneMask = 0b1111,
			HalfDotClockRate = false,
			Chain4Mode = false,
		};

	// F: 640x350 graphics emulating monochrome by aliasing all the planes together
	static readonly ModeParameters ModeF =
		new ModeParameters()
		{
			ScreenNumber = 10,
			Pixels = (640, 350),
			Characters = (80, 25),
			IsGraphicsMode = true,
			IsMonochrome = true,
			PaletteType = PaletteType.EGA,
			BaseAddress = BaseAddress.A000,
			CharacterWidth = CharacterWidth._8,
			CharacterHeight = 14,
			PixelsPerAddress = 8,
			MemoryAddressSize = 2,
			ScanDoubling = false,
			OddEvenAddressing = false,
			ShiftRegisterInterleave = false,
			Use256Colours = false,
			PlaneMask = 0b1111,
			HalfDotClockRate = false,
			Chain4Mode = false,
		};

	// 10: 640x350 graphics with 4bbp planar colour
	static readonly ModeParameters Mode10 =
		new ModeParameters()
		{
			ScreenNumber = 9,
			Pixels = (640, 350),
			Characters = (80, 25),
			IsGraphicsMode = true,
			PaletteType = PaletteType.EGA,
			BaseAddress = BaseAddress.A000,
			CharacterWidth = CharacterWidth._8,
			CharacterHeight = 14,
			PixelsPerAddress = 8,
			MemoryAddressSize = 2,
			ScanDoubling = false,
			OddEvenAddressing = false,
			ShiftRegisterInterleave = false,
			Use256Colours = false,
			PlaneMask = 0b1111,
			HalfDotClockRate = false,
			Chain4Mode = false,
		};

	// 11: 640x480 emulating monochrome by aliasing all the planes together
	static readonly ModeParameters Mode11 =
		new ModeParameters()
		{
			ScreenNumber = 11,
			Pixels = (640, 480),
			Characters = (80, 30),
			IsGraphicsMode = true,
			IsMonochrome = true,
			PaletteType = PaletteType.EGA,
			BaseAddress = BaseAddress.A000,
			CharacterWidth = CharacterWidth._8,
			CharacterHeight = 16,
			PixelsPerAddress = 8,
			MemoryAddressSize = 1,
			ScanDoubling = false,
			OddEvenAddressing = false,
			ShiftRegisterInterleave = false,
			Use256Colours = false,
			PlaneMask = 0b1111,
			HalfDotClockRate = false,
			Chain4Mode = false,
		};

	// 12: 640x480 with 4bpp planar colour
	static readonly ModeParameters Mode12 =
		new ModeParameters()
		{
			ScreenNumber = 12,
			Pixels = (640, 480),
			Characters = (80, 30),
			IsGraphicsMode = true,
			PaletteType = PaletteType.VGA,
			BaseAddress = BaseAddress.A000,
			CharacterWidth = CharacterWidth._8,
			CharacterHeight = 16,
			PixelsPerAddress = 8,
			MemoryAddressSize = 2,
			ScanDoubling = false,
			OddEvenAddressing = false,
			ShiftRegisterInterleave = false,
			Use256Colours = false,
			PlaneMask = 0,
			HalfDotClockRate = false,
			Chain4Mode = false,
		};

	// 13: 320x200 with 8bpp linear colour
	static readonly ModeParameters Mode13 =
		new ModeParameters()
		{
			ScreenNumber = 13,
			Pixels = (320, 200),
			Characters = (40, 25),
			IsGraphicsMode = true,
			PaletteType = PaletteType.VGA,
			BaseAddress = BaseAddress.A000,
			CharacterWidth = CharacterWidth._8,
			CharacterHeight = 8,
			PixelsPerAddress = 1,
			MemoryAddressSize = 4,
			ScanDoubling = true,
			OddEvenAddressing = false,
			ShiftRegisterInterleave = false,
			Use256Colours = true,
			PlaneMask = 0,
			HalfDotClockRate = true,
			Chain4Mode = true,
		};

	public static readonly ModeParameters?[] Modes =
		new[]
		{
			Mode1, // 0: a monochrome version of mode 1 on CGA, but the same as mode 1 on VGA
			Mode1,
			Mode3, // 2: a monochrome version of mode 3 on CGA, but the same as mode 3 on VGA
			Mode3,
			Mode5, // 4: a monochrome version of mode 5 on CGA, but the same as mode 5 on VGA
			Mode5,
			Mode6,
			Mode7,
			null, // no mode 8
			null, // no mode 9
			null, // no mode A
			null, // mode B is reserved for EGA internal use
			null, // mode C is reserved for EGA internal use
			ModeD,
			ModeE,
			ModeF,
			Mode10,
			Mode11,
			Mode12,
			Mode13,
		};
}

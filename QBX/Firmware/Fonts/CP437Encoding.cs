using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QBX.Firmware.Fonts;

class CP437Encoding : Encoding
{
	Dictionary<char, byte> _charToByte;
	char[] _byteToChar;

	public CP437Encoding(ControlCharacterInterpretation controlCharacters)
	{
		switch (controlCharacters)
		{
			case ControlCharacterInterpretation.Graphic:
				_charToByte = s_charToByteGraphic;
				_byteToChar = s_byteToCharGraphic;
				break;
			case ControlCharacterInterpretation.Semantic:
				_charToByte = s_charToByteSemantic;
				_byteToChar = s_byteToCharSemantic;
				break;

			default: throw new Exception("Unknown control character interpretation " + controlCharacters);
		}
	}

	public override int GetByteCount(char[] chars, int index, int count)
	{
		return count;
	}

	public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
	{
		for (int i = 0; i < charCount; i++)
			if (!_charToByte.TryGetValue(chars[i + charIndex], out bytes[i + byteIndex]))
				bytes[i + byteIndex] = UnknownCharacterByte;

		return charCount;
	}

	public override int GetCharCount(byte[] bytes, int index, int count)
	{
		return count;
	}

	public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
	{
		for (int i = 0; i < byteCount; i++)
			chars[i + byteIndex] = _byteToChar[bytes[i + byteIndex]];

		return byteCount;
	}

	public override int GetMaxByteCount(int charCount)
	{
		return charCount;
	}

	public override int GetMaxCharCount(int byteCount)
	{
		return byteCount;
	}

	public static byte GetByteGraphic(char ch)
		=> s_charToByteGraphic.TryGetValue(ch, out var @byte) ? @byte : UnknownCharacterByte;
	public static char GetCharGraphic(byte @byte)
		=> s_byteToCharGraphic[@byte];

	public static byte GetByteSemantic(char ch)
		=> s_charToByteSemantic.TryGetValue(ch, out var @byte) ? @byte : UnknownCharacterByte;
	public static char GetCharSemantic(byte @byte)
		=> s_byteToCharSemantic[@byte];

	const byte UnknownCharacterByte = (byte)'?';

	static readonly Dictionary<char, byte> s_charToByteGraphic =
		new Dictionary<char, byte>()
		{
			{ '\u263A', 0x01 }, // ☺
			{ '\u263B', 0x02 }, // ☻
			{ '\u2665', 0x03 }, // ♥
			{ '\u2666', 0x04 }, // ♦
			{ '\u2663', 0x05 }, // ♣
			{ '\u2660', 0x06 }, // ♠
			{ '\u2022', 0x07 }, // ●
			{ '\u25CF', 0x07 }, // ●
			{ '\u25D8', 0x08 }, // ◘
			{ '\u25CB', 0x09 }, // ○
			{ '\u25D9', 0x0A }, // ◙
			{ '\u2642', 0x0B }, // ♂
			{ '\u2640', 0x0C }, // ♀
			{ '\u266A', 0x0D }, // ♪
			{ '\u266B', 0x0E }, // ♬
			{ '\u266C', 0x0E }, // ♬
			{ '\u066D', 0x0F }, // ✵
			{ '\u06DE', 0x0F }, // ✵
			{ '\u235F', 0x0F }, // ✵
			{ '\u2605', 0x0F }, // ✵
			{ '\u2606', 0x0F }, // ✵
			{ '\u269D', 0x0F }, // ✵
			{ '\u2721', 0x0F }, // ✵
			{ '\u2726', 0x0F }, // ✵
			{ '\u2727', 0x0F }, // ✵
			{ '\u2729', 0x0F }, // ✵
			{ '\u272A', 0x0F }, // ✵
			{ '\u272B', 0x0F }, // ✵
			{ '\u272C', 0x0F }, // ✵
			{ '\u272D', 0x0F }, // ✵
			{ '\u272E', 0x0F }, // ✵
			{ '\u272F', 0x0F }, // ✵
			{ '\u2730', 0x0F }, // ✵
			{ '\u2734', 0x0F }, // ✵
			{ '\u2735', 0x0F }, // ✵
			{ '\u2736', 0x0F }, // ✵
			{ '\u2737', 0x0F }, // ✵
			{ '\u2738', 0x0F }, // ✵
			{ '\u2739', 0x0F }, // ✵
			{ '\u2742', 0x0F }, // ✵
			{ '\u25B6', 0x10 }, // ⯈
			{ '\u2BC8', 0x10 }, // ⯈
			{ '\u25C0', 0x11 }, // ⯇
			{ '\u2BC7', 0x11 }, // ⯇
			{ '\u2195', 0x12 }, // ↕
			{ '\u203C', 0x13 }, // ‼
			{ '\u00B6', 0x14 }, // ¶
			{ '\u00A7', 0x15 }, // §
			{ '\u25AC', 0x16 }, // ▬
			{ '\u21A8', 0x17 }, // ↨
			{ '\u2191', 0x18 }, // ↑
			{ '\u2193', 0x19 }, // ↓
			{ '\u2192', 0x1A }, // →
			{ '\u2190', 0x1B }, // ←
			{ '\u221F', 0x1C }, // ∟
			{ '\u2194', 0x1D }, // ↔
			{ '\u25B2', 0x1E }, // ⯅
			{ '\u2BC5', 0x1E }, // ⯅
			{ '\u25BC', 0x1F }, // ⯆
			{ '\u2BC6', 0x1F }, // ⯆
			{ '\u0020', 0x20 },
			{ '\u0021', 0x21 }, // !
			{ '\u0022', 0x22 }, // "
			{ '\u0023', 0x23 }, // #
			{ '\u0024', 0x24 }, // $
			{ '\u0025', 0x25 }, // %
			{ '\u0026', 0x26 }, // &
			{ '\u0027', 0x27 }, // '
			{ '\u0028', 0x28 }, // (
			{ '\u0029', 0x29 }, // )
			{ '\u002A', 0x2A }, // *
			{ '\u002B', 0x2B }, // +
			{ '\u002C', 0x2C }, // ,
			{ '\u002D', 0x2D }, // -
			{ '\u002E', 0x2E }, // .
			{ '\u002F', 0x2F }, // /
			{ '\u0030', 0x30 }, // 0
			{ '\u0031', 0x31 }, // 1
			{ '\u0032', 0x32 }, // 2
			{ '\u0033', 0x33 }, // 3
			{ '\u0034', 0x34 }, // 4
			{ '\u0035', 0x35 }, // 5
			{ '\u0036', 0x36 }, // 6
			{ '\u0037', 0x37 }, // 7
			{ '\u0038', 0x38 }, // 8
			{ '\u0039', 0x39 }, // 9
			{ '\u003A', 0x3A }, // :
			{ '\u003B', 0x3B }, // ;
			{ '\u003C', 0x3C }, // <
			{ '\u003D', 0x3D }, // =
			{ '\u003E', 0x3E }, // >
			{ '\u003F', 0x3F }, // ?
			{ '\u0040', 0x40 }, // @
			{ '\u0041', 0x41 }, // A
			{ '\u0042', 0x42 }, // B
			{ '\u0043', 0x43 }, // C
			{ '\u0044', 0x44 }, // D
			{ '\u0045', 0x45 }, // E
			{ '\u0046', 0x46 }, // F
			{ '\u0047', 0x47 }, // G
			{ '\u0048', 0x48 }, // H
			{ '\u0049', 0x49 }, // I
			{ '\u004A', 0x4A }, // J
			{ '\u004B', 0x4B }, // K
			{ '\u004C', 0x4C }, // L
			{ '\u004D', 0x4D }, // M
			{ '\u004E', 0x4E }, // N
			{ '\u004F', 0x4F }, // O
			{ '\u0050', 0x50 }, // P
			{ '\u0051', 0x51 }, // Q
			{ '\u0052', 0x52 }, // R
			{ '\u0053', 0x53 }, // S
			{ '\u0054', 0x54 }, // T
			{ '\u0055', 0x55 }, // U
			{ '\u0056', 0x56 }, // V
			{ '\u0057', 0x57 }, // W
			{ '\u0058', 0x58 }, // X
			{ '\u0059', 0x59 }, // Y
			{ '\u005A', 0x5A }, // Z
			{ '\u005B', 0x5B }, // [
			{ '\u005C', 0x5C }, // \
			{ '\u005D', 0x5D }, // ]
			{ '\u005E', 0x5E }, // ^
			{ '\u005F', 0x5F }, // _
			{ '\u0060', 0x60 }, // `
			{ '\u0061', 0x61 }, // a
			{ '\u0062', 0x62 }, // b
			{ '\u0063', 0x63 }, // c
			{ '\u0064', 0x64 }, // d
			{ '\u0065', 0x65 }, // e
			{ '\u0066', 0x66 }, // f
			{ '\u0067', 0x67 }, // g
			{ '\u0068', 0x68 }, // h
			{ '\u0069', 0x69 }, // i
			{ '\u006A', 0x6A }, // j
			{ '\u006B', 0x6B }, // k
			{ '\u006C', 0x6C }, // l
			{ '\u006D', 0x6D }, // m
			{ '\u006E', 0x6E }, // n
			{ '\u006F', 0x6F }, // o
			{ '\u0070', 0x70 }, // p
			{ '\u0071', 0x71 }, // q
			{ '\u0072', 0x72 }, // r
			{ '\u0073', 0x73 }, // s
			{ '\u0074', 0x74 }, // t
			{ '\u0075', 0x75 }, // u
			{ '\u0076', 0x76 }, // v
			{ '\u0077', 0x77 }, // w
			{ '\u0078', 0x78 }, // x
			{ '\u0079', 0x79 }, // y
			{ '\u007A', 0x7A }, // z
			{ '\u007B', 0x7B }, // {
			{ '\u007C', 0x7C }, // |
			{ '\u007D', 0x7D }, // }
			{ '\u007E', 0x7E }, // ~
			{ '\u2302', 0x7F }, // ⌂
			{ '\u00C7', 0x80 }, // Ç
			{ '\u00FC', 0x81 }, // ü
			{ '\u00E9', 0x82 }, // é
			{ '\u00E2', 0x83 }, // â
			{ '\u00E4', 0x84 }, // ä
			{ '\u00E0', 0x85 }, // à
			{ '\u00E5', 0x86 }, // å
			{ '\u00E7', 0x87 }, // ç
			{ '\u00EA', 0x88 }, // ê
			{ '\u00EB', 0x89 }, // ë
			{ '\u00E8', 0x8A }, // è
			{ '\u00EF', 0x8B }, // ï
			{ '\u00EE', 0x8C }, // î
			{ '\u00EC', 0x8D }, // ì
			{ '\u00C4', 0x8E }, // Ä
			{ '\u00C5', 0x8F }, // Å
			{ '\u00C9', 0x90 }, // É
			{ '\u00E6', 0x91 }, // æ
			{ '\u00C6', 0x92 }, // Æ
			{ '\u00F4', 0x93 }, // ô
			{ '\u00F6', 0x94 }, // ö
			{ '\u00F2', 0x95 }, // ò
			{ '\u00FB', 0x96 }, // û
			{ '\u00F9', 0x97 }, // ù
			{ '\u00FF', 0x98 }, // ÿ
			{ '\u00D6', 0x99 }, // Ö
			{ '\u00DC', 0x9A }, // Ü
			{ '\u00A2', 0x9B }, // ¢
			{ '\u00A3', 0x9C }, // £
			{ '\u00A5', 0x9D }, // ¥
			{ '\u20A7', 0x9E }, // ₧
			{ '\u0192', 0x9F }, // ƒ
			{ '\u00E1', 0xA0 }, // á
			{ '\u00ED', 0xA1 }, // í
			{ '\u00F3', 0xA2 }, // ó
			{ '\u00FA', 0xA3 }, // ú
			{ '\u00F1', 0xA4 }, // ñ
			{ '\u00D1', 0xA5 }, // Ñ
			{ '\u00AA', 0xA6 }, // ª
			{ '\u00BA', 0xA7 }, // º
			{ '\u00BF', 0xA8 }, // ¿
			{ '\u2310', 0xA9 }, // ⌐
			{ '\u00AC', 0xAA }, // ¬
			{ '\u00BD', 0xAB }, // ½
			{ '\u00BC', 0xAC }, // ¼
			{ '\u00A1', 0xAD }, // ¡
			{ '\u00AB', 0xAE }, // «
			{ '\u00BB', 0xAF }, // »
			{ '\u2591', 0xB0 }, // ░
			{ '\u2592', 0xB1 }, // ▒
			{ '\u2593', 0xB2 }, // ▓
			{ '\u2502', 0xB3 }, // │
			{ '\u2524', 0xB4 }, // ┤
			{ '\u2561', 0xB5 }, // ╡
			{ '\u2562', 0xB6 }, // ╢
			{ '\u2556', 0xB7 }, // ╖
			{ '\u2555', 0xB8 }, // ╕
			{ '\u2563', 0xB9 }, // ╣
			{ '\u2551', 0xBA }, // ║
			{ '\u2557', 0xBB }, // ╗
			{ '\u255D', 0xBC }, // ╝
			{ '\u255C', 0xBD }, // ╜
			{ '\u255B', 0xBE }, // ╛
			{ '\u2510', 0xBF }, // ┐
			{ '\u2514', 0xC0 }, // └
			{ '\u2534', 0xC1 }, // ┴
			{ '\u252C', 0xC2 }, // ┬
			{ '\u251C', 0xC3 }, // ├
			{ '\u2500', 0xC4 }, // ─
			{ '\u253C', 0xC5 }, // ┼
			{ '\u255E', 0xC6 }, // ╞
			{ '\u255F', 0xC7 }, // ╟
			{ '\u255A', 0xC8 }, // ╚
			{ '\u2554', 0xC9 }, // ╔
			{ '\u2569', 0xCA }, // ╩
			{ '\u2566', 0xCB }, // ╦
			{ '\u2560', 0xCC }, // ╠
			{ '\u2550', 0xCD }, // ═
			{ '\u256C', 0xCE }, // ╬
			{ '\u2567', 0xCF }, // ╧
			{ '\u2568', 0xD0 }, // ╨
			{ '\u2564', 0xD1 }, // ╤
			{ '\u2565', 0xD2 }, // ╥
			{ '\u2559', 0xD3 }, // ╙
			{ '\u2558', 0xD4 }, // ╘
			{ '\u2552', 0xD5 }, // ╒
			{ '\u2553', 0xD6 }, // ╓
			{ '\u256B', 0xD7 }, // ╫
			{ '\u256A', 0xD8 }, // ╪
			{ '\u2518', 0xD9 }, // ┘
			{ '\u250C', 0xDA }, // ┌
			{ '\u2588', 0xDB }, // █
			{ '\u2584', 0xDC }, // ▄
			{ '\u258C', 0xDD }, // ▌
			{ '\u2590', 0xDE }, // ▐
			{ '\u2580', 0xDF }, // ▀
			{ '\u03B1', 0xE0 }, // α
			{ '\u00DF', 0xE1 }, // ß
			{ '\u0393', 0xE2 }, // Γ
			{ '\u03C0', 0xE3 }, // π
			{ '\u03A3', 0xE4 }, // Σ
			{ '\u03C3', 0xE5 }, // σ
			{ '\u00B5', 0xE6 }, // µ
			{ '\u03C4', 0xE7 }, // τ
			{ '\u03A6', 0xE8 }, // Φ
			{ '\u0398', 0xE9 }, // Θ
			{ '\u03A9', 0xEA }, // Ω
			{ '\u03B4', 0xEB }, // δ
			{ '\u221E', 0xEC }, // ∞
			{ '\u03C6', 0xED }, // φ
			{ '\u03B5', 0xEE }, // ε
			{ '\u2229', 0xEF }, // ∩
			{ '\u2261', 0xF0 }, // ≡
			{ '\u00B1', 0xF1 }, // ±
			{ '\u2265', 0xF2 }, // ≥
			{ '\u2264', 0xF3 }, // ≤
			{ '\u2320', 0xF4 }, // ⌠
			{ '\u2321', 0xF5 }, // ⌡
			{ '\u00F7', 0xF6 }, // ÷
			{ '\u2248', 0xF7 }, // ≈
			{ '\u00B0', 0xF8 }, // °
			{ '\u2219', 0xF9 }, // ∙
			{ '\u00B7', 0xFA }, // ·
			{ '\u221A', 0xFB }, // √
			{ '\u207F', 0xFC }, // ⁿ
			{ '\u00B2', 0xFD }, // ²
			{ '\u25A0', 0xFE }, // ■
			{ '\u00A0', 0xFF },
		};

	static readonly char[] s_byteToCharGraphic =
		[
			'\u0000', // 00
			'\u263A', // 01  ☺
			'\u263B', // 02  ☻
			'\u2665', // 03  ♥
			'\u2666', // 04  ♦
			'\u2663', // 05  ♣
			'\u2660', // 06  ♠
			'\u25CF', // 07  ●
			'\u25D8', // 08  ◘
			'\u25CB', // 09  ○
			'\u25D9', // 0A  ◙
			'\u2642', // 0B  ♂
			'\u2640', // 0C  ♀
			'\u266A', // 0D  ♪
			'\u266C', // 0E  ♬
			'\u2735', // 0F  ✵
			'\u25B6', // 10  ⯈
			'\u25C0', // 11  ⯇
			'\u2195', // 12  ↕
			'\u203C', // 13  ‼
			'\u00B6', // 14  ¶
			'\u00A7', // 15  §
			'\u25AC', // 16  ▬
			'\u21A8', // 17  ↨
			'\u2191', // 18  ↑
			'\u2193', // 19  ↓
			'\u2192', // 1A  →
			'\u2190', // 1B  ←
			'\u221F', // 1C  ∟
			'\u2194', // 1D  ↔
			'\u25B2', // 1E  ⯅
			'\u25BC', // 1F  ⯆
			'\u0020', // 20   
			'\u0021', // 21  !
			'\u0022', // 22  "
			'\u0023', // 23  #
			'\u0024', // 24  $
			'\u0025', // 25  %
			'\u0026', // 26  &
			'\u0027', // 27  '
			'\u0028', // 28  (
			'\u0029', // 29  )
			'\u002A', // 2A  *
			'\u002B', // 2B  +
			'\u002C', // 2C  ,
			'\u002D', // 2D  -
			'\u002E', // 2E  .
			'\u002F', // 2F  /
			'\u0030', // 30  0
			'\u0031', // 31  1
			'\u0032', // 32  2
			'\u0033', // 33  3
			'\u0034', // 34  4
			'\u0035', // 35  5
			'\u0036', // 36  6
			'\u0037', // 37  7
			'\u0038', // 38  8
			'\u0039', // 39  9
			'\u003A', // 3A  :
			'\u003B', // 3B  ;
			'\u003C', // 3C  <
			'\u003D', // 3D  =
			'\u003E', // 3E  >
			'\u003F', // 3F  ?
			'\u0040', // 40  @
			'\u0041', // 41  A
			'\u0042', // 42  B
			'\u0043', // 43  C
			'\u0044', // 44  D
			'\u0045', // 45  E
			'\u0046', // 46  F
			'\u0047', // 47  G
			'\u0048', // 48  H
			'\u0049', // 49  I
			'\u004A', // 4A  J
			'\u004B', // 4B  K
			'\u004C', // 4C  L
			'\u004D', // 4D  M
			'\u004E', // 4E  N
			'\u004F', // 4F  O
			'\u0050', // 50  P
			'\u0051', // 51  Q
			'\u0052', // 52  R
			'\u0053', // 53  S
			'\u0054', // 54  T
			'\u0055', // 55  U
			'\u0056', // 56  V
			'\u0057', // 57  W
			'\u0058', // 58  X
			'\u0059', // 59  Y
			'\u005A', // 5A  Z
			'\u005B', // 5B  [
			'\u005C', // 5C  \
			'\u005D', // 5D  ]
			'\u005E', // 5E  ^
			'\u005F', // 5F  _
			'\u0060', // 60  `
			'\u0061', // 61  a
			'\u0062', // 62  b
			'\u0063', // 63  c
			'\u0064', // 64  d
			'\u0065', // 65  e
			'\u0066', // 66  f
			'\u0067', // 67  g
			'\u0068', // 68  h
			'\u0069', // 69  i
			'\u006A', // 6A  j
			'\u006B', // 6B  k
			'\u006C', // 6C  l
			'\u006D', // 6D  m
			'\u006E', // 6E  n
			'\u006F', // 6F  o
			'\u0070', // 70  p
			'\u0071', // 71  q
			'\u0072', // 72  r
			'\u0073', // 73  s
			'\u0074', // 74  t
			'\u0075', // 75  u
			'\u0076', // 76  v
			'\u0077', // 77  w
			'\u0078', // 78  x
			'\u0079', // 79  y
			'\u007A', // 7A  z
			'\u007B', // 7B  {
			'\u007C', // 7C  |
			'\u007D', // 7D  }
			'\u007E', // 7E  ~
			'\u2302', // 7F  ⌂
			'\u00C7', // 80  Ç
			'\u00FC', // 81  ü
			'\u00E9', // 82  é
			'\u00E2', // 83  â
			'\u00E4', // 84  ä
			'\u00E0', // 85  à
			'\u00E5', // 86  å
			'\u00E7', // 87  ç
			'\u00EA', // 88  ê
			'\u00EB', // 89  ë
			'\u00E8', // 8A  è
			'\u00EF', // 8B  ï
			'\u00EE', // 8C  î
			'\u00EC', // 8D  ì
			'\u00C4', // 8E  Ä
			'\u00C5', // 8F  Å
			'\u00C9', // 90  É
			'\u00E6', // 91  æ
			'\u00C6', // 92  Æ
			'\u00F4', // 93  ô
			'\u00F6', // 94  ö
			'\u00F2', // 95  ò
			'\u00FB', // 96  û
			'\u00F9', // 97  ù
			'\u00FF', // 98  ÿ
			'\u00D6', // 99  Ö
			'\u00DC', // 9A  Ü
			'\u00A2', // 9B  ¢
			'\u00A3', // 9C  £
			'\u00A5', // 9D  ¥
			'\u20A7', // 9E  ₧
			'\u0192', // 9F  ƒ
			'\u00E1', // A0  á
			'\u00ED', // A1  í
			'\u00F3', // A2  ó
			'\u00FA', // A3  ú
			'\u00F1', // A4  ñ
			'\u00D1', // A5  Ñ
			'\u00AA', // A6  ª
			'\u00BA', // A7  º
			'\u00BF', // A8  ¿
			'\u2310', // A9  ⌐
			'\u00AC', // AA  ¬
			'\u00BD', // AB  ½
			'\u00BC', // AC  ¼
			'\u00A1', // AD  ¡
			'\u00AB', // AE  «
			'\u00BB', // AF  »
			'\u2591', // B0  ░
			'\u2592', // B1  ▒
			'\u2593', // B2  ▓
			'\u2502', // B3  │
			'\u2524', // B4  ┤
			'\u2561', // B5  ╡
			'\u2562', // B6  ╢
			'\u2556', // B7  ╖
			'\u2555', // B8  ╕
			'\u2563', // B9  ╣
			'\u2551', // BA  ║
			'\u2557', // BB  ╗
			'\u255D', // BC  ╝
			'\u255C', // BD  ╜
			'\u255B', // BE  ╛
			'\u2510', // BF  ┐
			'\u2514', // C0  └
			'\u2534', // C1  ┴
			'\u252C', // C2  ┬
			'\u251C', // C3  ├
			'\u2500', // C4  ─
			'\u253C', // C5  ┼
			'\u255E', // C6  ╞
			'\u255F', // C7  ╟
			'\u255A', // C8  ╚
			'\u2554', // C9  ╔
			'\u2569', // CA  ╩
			'\u2566', // CB  ╦
			'\u2560', // CC  ╠
			'\u2550', // CD  ═
			'\u256C', // CE  ╬
			'\u2567', // CF  ╧
			'\u2568', // D0  ╨
			'\u2564', // D1  ╤
			'\u2565', // D2  ╥
			'\u2559', // D3  ╙
			'\u2558', // D4  ╘
			'\u2552', // D5  ╒
			'\u2553', // D6  ╓
			'\u256B', // D7  ╫
			'\u256A', // D8  ╪
			'\u2518', // D9  ┘
			'\u250C', // DA  ┌
			'\u2588', // DB  █
			'\u2584', // DC  ▄
			'\u258C', // DD  ▌
			'\u2590', // DE  ▐
			'\u2580', // DF  ▀
			'\u03B1', // E0  α
			'\u00DF', // E1  ß
			'\u0393', // E2  Γ
			'\u03C0', // E3  π
			'\u03A3', // E4  Σ
			'\u03C3', // E5  σ
			'\u00B5', // E6  µ
			'\u03C4', // E7  τ
			'\u03A6', // E8  Φ
			'\u0398', // E9  Θ
			'\u03A9', // EA  Ω
			'\u03B4', // EB  δ
			'\u221E', // EC  ∞
			'\u03C6', // ED  φ
			'\u03B5', // EE  ε
			'\u2229', // EF  ∩
			'\u2261', // F0  ≡
			'\u00B1', // F1  ±
			'\u2265', // F2  ≥
			'\u2264', // F3  ≤
			'\u2320', // F4  ⌠
			'\u2321', // F5  ⌡
			'\u00F7', // F6  ÷
			'\u2248', // F7  ≈
			'\u00B0', // F8  °
			'\u2219', // F9  ∙
			'\u00B7', // FA  ·
			'\u221A', // FB  √
			'\u207F', // FC  ⁿ
			'\u00B2', // FD  ²
			'\u25A0', // FE  ■
			'\u00A0', // FF
		];

	static Dictionary<char, byte> s_charToByteSemantic;
	static char[] s_byteToCharSemantic;

	static CP437Encoding()
	{
		s_charToByteSemantic = new Dictionary<char, byte>(s_charToByteGraphic);
		s_byteToCharSemantic = s_byteToCharGraphic.ToArray();

		for (int i = 0; i < 32; i++)
		{
			char c = (char)i;
			byte b = (byte)i;

			s_charToByteSemantic[c] = b;
			s_byteToCharSemantic[b] = c;
		}
	}


	public bool IsAsciiLetterOrDigit(byte v)
		=> IsAsciiLetter(v) || IsDigit(v);
	public bool IsAsciiLetter(byte v)
		=> IsAsciiLetterLower(v) || IsAsciiLetterUpper(v);
	public bool IsAsciiLetterUpper(byte v)
		=> (v >= (byte)'A') && (v <= (byte)'Z');
	public bool IsAsciiLetterLower(byte v)
		=> (v >= (byte)'a') && (v <= (byte)'z');
	public bool IsDigit(byte v)
		=> (v >= (byte)'0') && (v <= (byte)'9');

	public byte ToUpper(byte v)
		=> IsAsciiLetterLower(v) ? unchecked((byte)(v & ~0x20)) : v;
	public byte ToLower(byte v)
		=> IsAsciiLetterUpper(v) ? unchecked((byte)(v | 0x20)) : v;

	public int DigitValue(byte v)
		=> (int)(v - (byte)'0');
}

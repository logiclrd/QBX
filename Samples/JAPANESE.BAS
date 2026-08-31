DECLARE SUB DrawGlyph (x%, y%, glyphs%(), BYVAL o%)
DECLARE SUB DrawGlyph2x (x%, y%, glyphs%(), BYVAL o%)
DECLARE SUB ENprint (x%, y%, t$)
DECLARE SUB ENprint2x (x%, y%, t$)
DECLARE SUB InitializeC64Japanese (sw%)
DECLARE SUB JPprint (x%, y%, t$)
DECLARE SUB JPprint2x (x%, y%, t$)
DECLARE SUB JPprintImpl (x%, y%, scale%, t$)
DECLARE SUB LoadENGlyphs ()
DECLARE SUB LoadJPGlyphs ()

' This module allows basic Japanese to be drawn using the C64/Vic
' Japanese region font mapping. This mapping replaces 64 characters
' of the PETSCII set with katakana, and a handpicked few kanji
' related to dates.
'
' To use:
' - Put the screen into mode 13
' - Call InitializeC64Japanese w%, where w% is the pixel width of the
'   mode in which you intend to draw glyphs (you can use any value, it's
'   just the point at which text will wrap)
' - After this, you can use any graphics mode
' - Call ENprint x%, y%, "text" to emit ASCII text in the current code page
' - Call JPprint x%, y%, "ROMAJI" to emit Japanese text
'
' The ROMAJI syntax uses capital letters to straightforwardly identify
' katakana characters. Use TI not CHI, TU not TSU. You can use a hyphen
' for choo onpu (vowel extender). Any characters not recognized (e.g.
' numbers, punctuation) are sent to ENprint inline.

DIM ENglyphs%(256 * 8)
DIM JPglyphs%(64 * 8)
DIM JPmappings$(64)

'SCREEN 13

'InitializeC64Japanese 320

'JPprint 10, 10, "OHAYO-"

DATA ",": ' U+3001 "TOOTEN"
DATA "........"
DATA "........"
DATA "........"
DATA "........"
DATA "........"
DATA ".X......"
DATA "..X....."
DATA "........"

DATA "TI": ' 2-41: U+30C1 (U+FF81) "TI"
DATA "......X."
DATA "..XXXX.."
DATA "....X..."
DATA ".XXXXXX."
DATA "....X..."
DATA "....X..."
DATA "...X...."
DATA "........"

DATA "TU": ' 2-42: U+30C4 (U+FF82) "TU"
DATA "........"
DATA ".X.X..X."
DATA ".X.X..X."
DATA "......X."
DATA "......X."
DATA ".....X.."
DATA "...XX..."
DATA "........"

DATA "TE": ' 2-43: U+30C6 (U+FF83) "TE"
DATA "..XXXX.."
DATA "........"
DATA ".XXXXXX."
DATA "....X..."
DATA "....X..."
DATA "....X..."
DATA "...X...."
DATA "........"

DATA "TO": ' 2-44: U+30C8 (U+FF84) "TO"
DATA "...X...."
DATA "...X...."
DATA "...X...."
DATA "...XX..."
DATA "...X.X.."
DATA "...X...."
DATA "...X...."
DATA "........"

DATA "NA": ' 2-45: U+30CA (U+FF85) "NA"
DATA "....X..."
DATA "....X..."
DATA ".XXXXXX."
DATA "....X..."
DATA "....X..."
DATA "...X...."
DATA "..X....."
DATA "........"

DATA "NI": ' 2-46: U+30CB (U+FF86) "NI"
DATA "........"
DATA "..XXXX.."
DATA "........"
DATA "........"
DATA "........"
DATA ".XXXXXX."
DATA "........"
DATA "........"

DATA "NU": ' 2-47: U+30CC (U+FF87) "NU"
DATA "..XXXXX."
DATA "......X."
DATA "...X.X.."
DATA "....X..."
DATA "...X.X.."
DATA "..X....."
DATA "........"
DATA "........"

DATA "NE": ' 2-48: U+30CD (U+FF88) "NE"
DATA "....X..."
DATA "..XXXXX."
DATA ".....X.."
DATA "....X..."
DATA "...XXX.."
DATA "..X.X.X."
DATA "....X..."
DATA "........"


DATA "NO": ' 2-49: U+30CE (U+FF89) "NO"
DATA ".....X.."
DATA ".....X.."
DATA ".....X.."
DATA "....X..."
DATA "...X...."
DATA "..X....."
DATA "........"
DATA "........"

DATA "HA": ' 2-4A: U+30CF (U+FF8A) "HA"
DATA "....X..."
DATA "..X..X.."
DATA "..X...X."
DATA "..X...X."
DATA "..X...X."
DATA ".X....X."
DATA "........"
DATA "........"

DATA "HI": ' 2-4B: U+30D2 (U+FF8B) "HI"
DATA ".X......"
DATA ".X......"
DATA ".XXXXX.."
DATA ".X......"
DATA ".X......"
DATA "..XXXXX."
DATA "........"
DATA "........"

DATA "HU": ' 2-4C: U+30D5 (U+FF8C) "HU"
DATA "..XXXXX."
DATA "......X."
DATA "......X."
DATA ".....X.."
DATA "....X..."
DATA "...X...."
DATA "........"
DATA "........"

DATA "HE": ' 2-4D: U+30D8 (U+FF8D) "HE"
DATA "........"
DATA "...X...."
DATA "..X.X..."
DATA ".X...X.."
DATA "......X."
DATA "......X."
DATA "........"
DATA "........"

DATA "HO": ' 2-4E: U+30DB (U+FF8E) "HO"
DATA "....X..."
DATA ".XXXXXX."
DATA "....X..."
DATA "..X.X.X."
DATA "..X.X.X."
DATA ".X..X.X."
DATA "....X..."
DATA "........"

DATA "MA": ' 2-4F: U+30DE (U+FF8F) "MA"
DATA ".XXXXXX."
DATA "......X."
DATA "..X..X.."
DATA "...XX..."
DATA "....X..."
DATA ".....X.."
DATA "........"
DATA "........"

DATA "MI": ' 2-50: U+30DF (U+FF90) "MI"
DATA "...XX..."
DATA ".....X.."
DATA "...X..X."
DATA "....X..."
DATA "..X..X.."
DATA "...X...."
DATA "....XX.."
DATA "........"

DATA "MU": ' 2-51: U+30E0 (U+FF91) "MU"
DATA "....X..."
DATA "...X...."
DATA "..X..X.."
DATA ".X...X.."
DATA ".XXXXX.."
DATA "......X."
DATA "........"
DATA "........"

DATA "ME": ' 2-52: U+30E1 (U+FF92) "ME"
DATA "......X."
DATA "......X."
DATA "...X.X.."
DATA "....X..."
DATA "...X.X.."
DATA "..X....."
DATA "........"
DATA "........"

DATA "MO": ' 2-53: U+30E2 (U+FF93) "MO"
DATA "..XXXX.."
DATA "...X...."
DATA ".XXXXXX."
DATA "...X...."
DATA "...X...."
DATA "....XXX."
DATA "........"
DATA "........"

DATA "YA": ' 2-54: U+30E4 (U+FF94) "YA"
DATA "...X...."
DATA ".XXXXXX."
DATA "...X..X."
DATA "...X.X.."
DATA "...X...."
DATA "...X...."
DATA "...X...."
DATA "........"

DATA "YU": ' 2-55: U+30E6 (U+FF95) "YU"
DATA "........"
DATA "..XXXX.."
DATA ".....X.."
DATA ".....X.."
DATA ".....X.."
DATA ".XXXXXX."
DATA "........"
DATA "........"

DATA "YO": ' 2-56: U+30E8 (U+FF96) "YO"
DATA "........"
DATA ".XXXXXX."
DATA "......X."
DATA "..XXXXX."
DATA "......X."
DATA ".XXXXXX."
DATA "........"
DATA "........"

DATA "RA": ' 2-57: U+30E9 (U+FF97) "RA"
DATA "..XXXX.."
DATA "........"
DATA ".XXXXXX."
DATA "......X."
DATA ".....X.."
DATA "....X..."
DATA "........"
DATA "........"

DATA "RI": ' 2-58: U+30EA (U+FF98) "RI"
DATA "..X...X."
DATA "..X...X."
DATA "..X...X."
DATA "..X...X."
DATA "..X...X."
DATA ".....X.."
DATA "....X..."
DATA "........"

DATA "RU": ' 2-59: U+30EB (U+FF99) "RU"
DATA "..X.X..."
DATA "..X.X..."
DATA "..X.X..."
DATA "..X.X.X."
DATA "..X.X.X."
DATA ".X..XX.."
DATA "........"
DATA "........"

DATA "RE": ' 2-5A: U+30EC (U+FF9A) "RE"
DATA "..X....."
DATA "..X....."
DATA "..X....."
DATA "..X...X."
DATA "..X..X.."
DATA "..XXX..."
DATA "........"
DATA "........"

DATA "WA": ' 2-5C: U+30EF (U+FF9C) "WA"
DATA ".XXXXXX."
DATA ".X....X."
DATA ".X....X."
DATA ".....X.."
DATA "....X..."
DATA "...X...."
DATA "........"
DATA "........"

DATA "WO": ' 2-5F: U+30F2 (U+FF66) "WO"
DATA ".XXXXXX."
DATA "......X."
DATA ".XXXXXX."
DATA "......X."
DATA ".....X.."
DATA "....X..."
DATA "........"
DATA "........"

DATA "A": ' 2-61: U+30A2 (U+FF67) "A"
DATA ".XXXXXX."
DATA "......X."
DATA "...X.X.."
DATA "...XX..."
DATA "...X...."
DATA "...X...."
DATA "..X....."
DATA "........"

DATA "I": ' 2-62: U+30A4 (U+FF68) "I"
DATA "......X."
DATA "....XX.."
DATA "...XX..."
DATA "..X.X..."
DATA ".X..X..."
DATA "....X..."
DATA "....X..."
DATA "........"

DATA "U": ' 2-63: U+30A6 (U+FF69) "U"
DATA "...X...."
DATA ".XXXXXX."
DATA ".X....X."
DATA "......X."
DATA ".....X.."
DATA "....X..."
DATA "........"
DATA "........"

DATA "E": ' 2-64: U+30A8 (U+FF6A) "E"
DATA "........"
DATA "..XXXXX."
DATA "....X..."
DATA "....X..."
DATA "....X..."
DATA "..XXXXX."
DATA "........"
DATA "........"

DATA "O": ' 2-65: U+30AA (U+FF6B) "O"
DATA "....X..."
DATA ".XXXXXX."
DATA "....X..."
DATA "...XX..."
DATA "..X.X..."
DATA ".X..X..."
DATA "....X..."
DATA "........"

DATA "KA": ' 2-66: U+30AB (U+FF76) "KA"
DATA "...X...."
DATA ".XXXXXX."
DATA "...X..X."
DATA "...X..X."
DATA "..X...X."
DATA ".X...XX."
DATA "........"
DATA "........"

DATA "KI": ' 2-67: U+30AD (U+FF77) "KI"
DATA "....X..."
DATA "..XXXXX."
DATA "....X..."
DATA "..XXXXX."
DATA "....X..."
DATA "....X..."
DATA "....X..."
DATA "........"

DATA "KU": ' 2-68: U+30AF (U+FF78) "KU"
DATA "...XXXX."
DATA "..X...X."
DATA ".X....X."
DATA ".....X.."
DATA "....X..."
DATA "...X...."
DATA "..X....."
DATA "........"

DATA "KE": ' 2-69: U+30B1 (U+FF79) "KE"
DATA "..X....."
DATA "..XXXXX."
DATA ".X..X..."
DATA "....X..."
DATA "....X..."
DATA "...X...."
DATA "..X....."
DATA "........"

DATA "tenten": ' 2-6A: U+309B, U+3099 (U+FF9E) "tenten" (VOICED SOUND MARK)
DATA ".X.X...."
DATA ".X.X...."
DATA ".X.X...."
DATA "........"
DATA "........"
DATA "........"
DATA "........"
DATA "........"

DATA "SU": ' 2-6C: U+30B9 (U+FF7D) "SU"
DATA ".XXXXXX."
DATA ".....X.."
DATA "....X..."
DATA "...XX..."
DATA "..X..X.."
DATA ".X....X."
DATA "........"
DATA "........"

DATA "maru": ' 2-6F: U+309C, U+309A (U+FF9F) "maru" (SEMI-VOICED SOUND MARK)
DATA ".XXX...."
DATA ".X.X...."
DATA ".XXX...."
DATA "........"
DATA "........"
DATA "........"
DATA "........"
DATA "........"

DATA "nen": ' 2-74: U+5E74 "YEAR, PERSON'S AGE"
DATA ".X......"
DATA ".XXXXXX."
DATA "....X..."
DATA "..XXXXX."
DATA "..X.X..."
DATA ".XXXXXX."
DATA "....X..."
DATA "........"

DATA "gatsu": ' 2-75: U+6708 "MONTH"
DATA "..XXXXX."
DATA "..X...X."
DATA "..XXXXX."
DATA "..X...X."
DATA "..XXXXX."
DATA "..X...X."
DATA "..X...X."
DATA "........"

DATA "ka": ' 2-76: U+65E5 "DAY, DAYTIME"
DATA "..XXXXX."
DATA "..X...X."
DATA "..X...X."
DATA "..XXXXX."
DATA "..X...X."
DATA "..X...X."
DATA "..XXXXX."
DATA "........"

DATA "TA": ' 2-77: U+30BF (U+FF80) "TA"
DATA "...XXXX."
DATA "..X...X."
DATA ".X.X..X."
DATA "....XX.."
DATA "....X..."
DATA "...X...."
DATA "..X....."
DATA "........"

DATA "RO": ' 2-78: U+30ED (U+FF9B) "RO"
DATA "........"
DATA ".XXXXXX."
DATA ".X....X."
DATA ".X....X."
DATA ".X....X."
DATA ".XXXXXX."
DATA "........"
DATA "........"

DATA "N": ' 2-79: U+30F3 (U+FF9D) "N"
DATA ".XX....."
DATA "........"
DATA "......X."
DATA "......X."
DATA ".....X.."
DATA ".XXXX..."
DATA "........"
DATA "........"

DATA "KO": ' 2-7A: U+30B3 (U+FF7A) "KO"
DATA "........"
DATA ".XXXXXX."
DATA "......X."
DATA "......X."
DATA "......X."
DATA ".XXXXXX."
DATA "........"
DATA "........"

DATA "SA": ' 2-7B: U+30B5 (U+FF7B) "SA"
DATA "..X..X.."
DATA ".XXXXXX."
DATA "..X..X.."
DATA "..X..X.."
DATA ".....X.."
DATA "....X..."
DATA "........"
DATA "........"

DATA "SI": ' 2-7C: U+30B7 (U+FF7C) "SI"
DATA ".XX....."
DATA "........"
DATA ".XX...X."
DATA "......X."
DATA ".....X.."
DATA ".XXXX..."
DATA "........"
DATA "........"

DATA "SE": ' 2-7E: U+30BB (U+FF7E) "SE"
DATA "..X....."
DATA ".XXXXXX."
DATA "..X...X."
DATA "..X..X.."
DATA "..X....."
DATA "...XXXX."
DATA "........"
DATA "........"

DATA "SO": ' 2-7F: U+30BD (U+FF7F) "SO"
DATA ".X....X."
DATA "..X...X."
DATA "......X."
DATA ".....X.."
DATA "....X..."
DATA "...X...."
DATA "........"
DATA "........"

DATA "": ' end of data

SUB DrawGlyph (x%, y%, glyphs%(), BYVAL o%)
FOR yy% = 0 TO 7
  g% = glyphs%(o%)

  bit% = 1
  FOR xx% = 7 TO 0 STEP -1
    IF g% AND bit% THEN PSET (x% + xx%, y% + yy%)
    bit% = bit% + bit%
  NEXT xx%

  o% = o% + 1
NEXT yy%
END SUB

SUB DrawGlyph2x (x%, y%, glyphs%(), BYVAL o%)
FOR yy% = 0 TO 7
  g% = glyphs%(o%)

  bit% = 1
  FOR xx% = 7 TO 0 STEP -1
    IF g% AND bit% THEN
      LINE (x% + xx% + xx%, y% + yy% + yy%)-STEP(1, 1), , BF
    END IF

    bit% = bit% + bit%
  NEXT xx%

  o% = o% + 1
NEXT yy%
END SUB

SUB ENprint (x%, y%, t$)
SHARED ENglyphs%()
SHARED sw%

FOR i% = 1 TO LEN(t$)
  ch% = ASC(MID$(t$, i%, 1))

  DrawGlyph x%, y%, ENglyphs%(), ch% * 8
  ti% = ti% + 1

  x% = x% + 8
  IF x% > sw% - 8 THEN x% = x% - sw%: y% = y% + 8
NEXT i%
END SUB

SUB ENprint2x (x%, y%, t$)
SHARED ENglyphs%()
SHARED sw%

FOR i% = 1 TO LEN(t$)
  ch% = ASC(MID$(t$, i%, 1))

  DrawGlyph2x x%, y%, ENglyphs%(), ch% * 8
  ti% = ti% + 1

  x% = x% + 16
  IF x% > sw% - 16 THEN x% = x% - sw%: y% = y% + 16
NEXT i%
END SUB

SUB InitializeC64Japanese (screenwidth%)
SHARED sw%

LoadENGlyphs
LoadJPGlyphs

sw% = screenwidth%
END SUB

SUB JPprint (x%, y%, t$)
JPprintImpl x%, y%, 1, t$
END SUB

SUB JPprint2x (x%, y%, t$)
JPprintImpl x%, y%, 2, t$
END SUB

SUB JPprintImpl (x%, y%, scale%, t$)
SHARED JPmappings$(), JPglyphs%(), JPcharcount%
SHARED sw%

ti% = 1

WHILE ti% <= LEN(t$)
  c$ = MID$(t$, ti%, 1)
  IF ti% + 1 <= LEN(t$) THEN c2$ = MID$(t$, ti%, 2) ELSE c2$ = ""
  IF ti% + 3 <= LEN(t$) THEN c4$ = MID$(t$, ti%, 4) ELSE c4$ = ""
  IF ti% + 5 <= LEN(t$) THEN c6$ = MID$(t$, ti%, 6) ELSE c6$ = ""

  ch% = ASC(c$)

  IF (ch% < 65) AND (ch% <> 44) THEN
    havechmatch% = 0
  ELSE
    FOR ch% = 0 TO JPcharcount% - 1
      SELECT CASE LEN(JPmappings$(ch%))
        CASE 1: havechmatch% = JPmappings$(ch%) = c$
        CASE 2: havechmatch% = JPmappings$(ch%) = c2$
        CASE 4: havechmatch% = JPmappings$(ch%) = c4$
        CASE 6: havechmatch% = JPmappings$(ch%) = c6$
      END SELECT

      IF havechmatch% THEN EXIT FOR
    NEXT ch%
  END IF

  IF havechmatch% THEN
    IF scale% = 1 THEN
      DrawGlyph x%, y%, JPglyphs%(), ch% * 8
    ELSE
      DrawGlyph2x x%, y%, JPglyphs%(), ch% * 8
    END IF

    ti% = ti% + LEN(JPmappings$(ch%))
  ELSE
    IF cc% <> 32 THEN
      IF scale% = 1 THEN
        ENprint (x%), (y%), c$
      ELSE
        ENprint2x (x%), (y%), c$
      END IF
    END IF

    ti% = ti% + 1
  END IF

  x% = x% + 8 * scale%
  IF x% > sw% - 8 * scale% THEN x% = x% - sw%: y% = y% + 8 * scale%
WEND
END SUB

SUB LoadENGlyphs
SHARED ENglyphs%()

COLOR 255
FOR ch% = 1 TO 255
  IF ch% <> 7 THEN
    LOCATE 1, 1
    PRINT CHR$(ch%)

    o% = ch% * 8

    FOR y% = 0 TO 7
      b% = 128
      v% = 0
      FOR x% = 0 TO 7
        IF POINT(x%, y%) THEN v% = v% OR b%
        b% = b% \ 2
      NEXT x%

      ENglyphs%(o% + y%) = v%
    NEXT y%
  END IF
NEXT ch%

CLS
COLOR 15
END SUB

SUB LoadJPGlyphs
SHARED JPmappings$(), JPglyphs%(), JPcharcount%

ch% = 0

DO
  READ JPmappings$(ch%)

  IF JPmappings$(ch%) = "" THEN EXIT DO

  o% = ch% * 8

  FOR i% = 0 TO 7
    READ l$

    b% = 128
    v% = 0

    FOR j% = 1 TO LEN(l$)
      IF MID$(l$, j%, 1) <> "." THEN v% = v% OR b%
      b% = b% \ 2
    NEXT j%

    JPglyphs%(o% + i%) = v%
  NEXT i%

  ch% = ch% + 1
LOOP

JPcharcount% = ch%
END SUB

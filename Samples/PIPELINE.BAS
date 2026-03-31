CONST NumColumns% = 40
CONST NumRows% = 25

CONST DirRight% = 0
CONST DirUp% = 1
CONST DirLeft% = 2
CONST DirDown% = 3

CONST AverageStraightLength% = 20
CONST AverageColourLength% = 100

DIM SHARED CharacterMap%(0 TO 3, 0 TO 3)

InitializeScreen
InitializeDrawingCharacters

PickRandomStartingPoint x%, y%, direction%

DO
  DrawRandomPipe x%, y%, direction%
  Wrap x%, y%

  Delay
LOOP WHILE INKEY$ = ""

WIDTH 80, 25

SUB Advance (x%, y%, direction%)
SELECT CASE direction%
  CASE DirRight%: x% = x% + 1
  CASE DirUp%: y% = y% - 1
  CASE DirLeft%: x% = x% - 1
  CASE DirDown%: y% = y% + 1
END SELECT
END SUB

SUB Delay
st! = TIMER
WHILE st! = TIMER: WEND
END SUB

SUB DrawRandomPipe (x%, y%, direction%)
STATIC c%

IF c% = 0 THEN c% = 1: COLOR c%:

WHILE IsOnScreen%(x%, y%)
  MaybePickNewColour c%

  newdirection% = MaybePickNewDirection%(direction%)
  reversedirection% = (direction% + 2) MOD 4

  LOCATE y%, x%
  PRINT CHR$(CharacterMap%(reversedirection%, newdirection%));

  direction% = newdirection%

  Advance x%, y%, direction%
WEND
END SUB

SUB FillIn (pixels%())
' Bridge
FOR x% = 0 TO 7
  FOR y% = 0 TO 15
    IF (x% > 0) AND (x% < 6) THEN
      IF pixels%(x% - 1, y%) <> 0 AND pixels%(x% + 2, y%) <> 0 THEN
        pixels%(x%, y%) = 1
      END IF
    END IF

    IF (y% > 0) AND (y% < 14) THEN
      IF pixels%(x%, y% - 1) <> 0 AND pixels%(x%, y% + 2) <> 0 THEN
        pixels%(x%, y%) = 1
      END IF
    END IF
  NEXT y%
NEXT x%

' Round
FOR x% = 0 TO 6
  FOR y% = 0 TO 14
    s% = 0

    FOR xx% = 0 TO 1
      FOR yy% = 0 TO 1
        s% = s% + pixels%(x% + xx%, y% + yy%)
      NEXT yy%
    NEXT xx%

    IF s% = 3 THEN
      FOR xx% = 0 TO 1
        FOR yy% = 0 TO 1
          IF pixels%(x% + xx%, y% + yy%) = 0 THEN
            pixels%(x% + xx%, y% + yy%) = -1
          END IF
        NEXT yy%
      NEXT xx%
    END IF
  NEXT y%
NEXT x%

FOR x% = 0 TO 7
  FOR y% = 0 TO 15
    pixels%(x%, y%) = ABS(pixels%(x%, y%))
  NEXT y%
NEXT x%
END SUB

SUB FillRectangle (pixels%(), x1%, y1%, x2%, y2%)
FOR y% = y1% TO y2%
  FOR x% = x1% TO x2%
    pixels%(x%, y%) = 1
  NEXT x%
NEXT y%
END SUB

SUB HueToRGB (hue%, r%, g%, b%)
x% = 64 * (1! - ABS((hue% MOD 120) / 60! - 1))

IF x% = 64 THEN x% = 63

SELECT CASE hue%
  CASE 0 TO 60: r% = 63: g% = x%: b% = 0
  CASE 60 TO 120: r% = x%: g% = 63: b% = 0
  CASE 120 TO 180: r% = 0: g% = 63: b% = x%
  CASE 180 TO 240: r% = 0: g% = x%: b% = 63
  CASE 240 TO 300: r% = x%: g% = 0: b% = 63
  CASE 300 TO 360: r% = 63: g% = 0: b% = x%
END SELECT
END SUB

SUB InitializeDrawingCharacters
nextchar% = 128

FOR i% = 0 TO 3
  FOR j% = 0 TO 3
    IF i% <> j% THEN
      IF CharacterMap%(j%, i%) <> 0 THEN
        CharacterMap%(i%, j%) = CharacterMap%(j%, i%)
      ELSE
        RenderCharacter i%, j%, nextchar%
        CharacterMap%(i%, j%) = nextchar%
        nextchar% = nextchar% + 1
      END IF
    END IF
  NEXT j%
NEXT i%
END SUB

SUB InitializePalette
FOR i% = 1 TO 15
  hue% = i% * 360 \ 15

  HueToRGB hue%, r%, g%, b%

  PALETTE i%, i%

  OUT &H3C8, i%
  OUT &H3C9, r%
  OUT &H3C9, g%
  OUT &H3C9, b%
NEXT i%
END SUB

SUB InitializeScreen
WIDTH NumColumns%, NumRows%

CONST MiscOutputReadPort% = &H3CC
CONST MiscOutputWritePort% = &H3C2

CONST ClockMask% = 12
CONST Clock25MHz% = 0

mode% = INP(MiscOutputReadPort%)

mode% = mode% AND NOT ClockMask%
mode% = mode% OR Clock25MHz%

OUT MiscOutputWritePort%, mode%

CONST SequencerIndexPort% = &H3C4
CONST SequencerDataPort% = &H3C5

CONST ClockingModeRegister% = 1
CONST ClockingModeCharacterWidth8% = 1

OUT SequencerIndexPort%, ClockingModeRegister%
mode% = INP(SequencerDataPort%)

OUT SequencerIndexPort%, ClockingModeRegister%
OUT SequencerDataPort%, mode% OR ClockingModeCharacterWidth8%

InitializePalette
END SUB

FUNCTION IsOnScreen% (x%, y%)
IF x% >= 1 THEN
  IF x% <= NumColumns% THEN
    IF y% >= 1 THEN
      IF y% <= NumRows% THEN
        IsOnScreen% = 1
      END IF
    END IF
  END IF
END IF
END FUNCTION

SUB MaybePickNewColour (c%)
IF INT(RND * AverageColourLength%) = 0 THEN
  c% = c% + 1
  IF c% = 16 THEN c% = 1
  COLOR c%
END IF
END SUB

FUNCTION MaybePickNewDirection% (direction%)
IF INT(RND * AverageStraightLength%) = 0 THEN
  ' turn left (+1) or right (-1)
  ' these are the same as +5 or +3 in MOD 4 space
  MaybePickNewDirection% = (direction% + 3 + 2 * INT(RND * 2)) MOD 4
ELSE
  MaybePickNewDirection% = direction%
END IF
END FUNCTION

SUB PickRandomStartingPoint (x%, y%, direction%)
CONST HorizontalEdgeChars% = 40
CONST VerticalEdgeChars% = 25

CONST PerimeterChars% = 2 * HorizontalEdgeChars% + 2 * VerticalEdgeChars%

r% = INT(RND * PerimeterChars%)

IF r% < HorizontalEdgeChars% THEN
  ' top edge
  x% = r% + 1
  y% = 1
  direction% = DirDown%
  EXIT SUB
END IF

r% = r% - HorizontalEdgeChars%

IF r% < HorizontalEdgeChars% THEN
  ' bottom edge
  x% = r% + 1
  y% = VerticalEdgeChars%
  direction% = DirUp%
  EXIT SUB
END IF

r% = r% - HorizontalEdgeChars%

IF r% < VerticalEdgeChars% THEN
  ' left edge
  x% = 1
  y% = r% + 1
  direction% = DirRight%
ELSE
  r% = r% - VerticalEdgeChars%

  ' right edge
  x% = HorizontalEdgeChars%
  y% = r% + 1
  direction% = DirLeft%
END IF
END SUB

SUB RenderCharacter (fromdir%, todir%, charcode%)
DIM pixels%(0 TO 7, 0 TO 15)

RenderLine pixels%(), fromdir%
RenderLine pixels%(), todir%

FillIn pixels%()

SetFontCharacter charcode%, pixels%()
END SUB

SUB RenderLine (pixels%(), direction%)
SELECT CASE direction%
  CASE DirRight%: FillRectangle pixels%(), 5, 6, 7, 9
  CASE DirUp%: FillRectangle pixels%(), 3, 0, 4, 7
  CASE DirLeft%: FillRectangle pixels%(), 0, 6, 2, 9
  CASE DirDown%: FillRectangle pixels%(), 3, 8, 4, 15
END SELECT
END SUB

SUB SetFontCharacter (charcode%, pixels%())
CONST SequencerIndexPort% = &H3C4
CONST SequencerDataPort% = &H3C5

CONST MapMaskRegister% = 2
CONST MapMaskPlane0% = 1
CONST MapMaskPlane1% = 2
CONST MapMaskPlane2% = 4
CONST MapMaskPlane3% = 8

CONST MemoryModeRegister% = 4
CONST MemoryModeOddEvenDisable% = 4

CONST GraphicsIndexPort% = &H3CE
CONST GraphicsDataPort% = &H3CF

CONST MiscGraphicsRegister% = 6
CONST MiscGraphicsChainOddEven% = 2

OUT SequencerIndexPort%, MapMaskRegister%
savedmask% = INP(SequencerDataPort%)
OUT SequencerIndexPort%, MemoryModeRegister%
savedmode% = INP(SequencerDataPort%)
OUT GraphicsIndexPort%, MiscGraphicsRegister%
savedmisc% = INP(GraphicsDataPort%)

OUT SequencerIndexPort%, MapMaskRegister%
OUT SequencerDataPort%, MapMaskPlane2%

OUT SequencerIndexPort%, MemoryModeRegister%
OUT SequencerDataPort%, savedmode% OR MemoryModeOddEvenDisable%

OUT GraphicsIndexPort%, MiscGraphicsRegister%
OUT GraphicsDataPort%, savedmisc% AND NOT MiscGraphicsChainOddEven%

DEF SEG = &HB800

baseoffset% = charcode% * 32

FOR y% = 0 TO 15
  scan% = 0
  FOR x% = 0 TO 7
    scan% = scan% * 2 + pixels%(x%, y%)
  NEXT x%

  POKE baseoffset% + y%, scan%
NEXT y%

OUT SequencerIndexPort%, MapMaskRegister%
OUT SequencerDataPort%, savedmask%

OUT SequencerIndexPort%, MemoryModeRegister%
OUT SequencerDataPort%, savedmode%

OUT GraphicsIndexPort%, MiscGraphicsRegister%
OUT GraphicsDataPort%, savedmisc%
END SUB

SUB Wrap (x%, y%)
IF x% = 0 THEN x% = NumColumns%
IF x% > NumColumns% THEN x% = 1

IF y% = 0 THEN y% = NumRows%
IF y% > NumRows% THEN y% = 1
END SUB


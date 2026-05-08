DECLARE SUB DisableBlink ()
DECLARE SUB DisableSpacingPixel ()
DECLARE SUB SetFont ()
DECLARE SUB SetFontCharacter (charcode%, pixels%())
DECLARE SUB SetPalette ()

CONST NumFontChars% = 15
CONST FirstChar% = 128

FontChars$ = SPACE$(NumFontChars%)

FOR i% = 1 TO NumFontChars%
  MID$(FontChars$, i%, 1) = CHR$(FirstChar% + i% - 1)
NEXT i%

WIDTH 80, 50

DisableBlink
DisableSpacingPixel
SetPalette
SetFont

T! = 0!
running% = -1

WHILE INKEY$ = ""
  T! = T! + .1

  FOR y% = 0 TO 399 STEP 8
    LOCATE y% \ 8 + 1, 1

    FOR x% = 0 TO 639 STEP 8
      v1! = SIN(x% / 50! + T!)
      v2! = SIN(y% / 30! - T!)
      v3! = SIN((x% + y%) / 60! + T! * 1.5)

      c! = (v1! + v2! + v3! + 3!) * 42!

      c1% = INT(c! * 15! / 255!)
      c2% = c1% + 1

      IF c2% > 15 THEN c2% = 15

      cp! = (c! - c1% * (255! / 15!)) / (255! / 15!)

      IF c1% > 7 THEN
        c2% = c2% + 16
        c1% = c1% - 8
      END IF

      cf% = cp! * NumFontChars%
      IF cf% = NumFontChars% THEN cf% = NumFontChars% - 1

      COLOR c2%, c1%
      PRINT MID$(FontChars$, cf% + 1, 1);
    NEXT x%
  NEXT y%
WEND

SUB DisableBlink
CONST InputStatus1Port% = &H3DA

CONST AttributeControllerWritePort% = &H3C0
CONST AttributeControllerReadPort% = &H3C1

CONST ModeControlRegister% = &H10
CONST ModeControlBlinkEnable% = 8

' Reset Attribute Controller Write Port to Index
ignored% = INP(InputStatus1Port%)

OUT AttributeControllerWritePort%, ModeControlRegister%

modecontrol% = INP(AttributeControllerReadPort%)
modecontrol% = modecontrol% AND NOT ModeControlBlinkEnable%

' Reset Attribute Controller Write Port to Index
ignored% = INP(InputStatus1Port%)

OUT AttributeControllerWritePort%, ModeControlRegister%
OUT AttributeControllerWritePort%, modecontrol%
END SUB

SUB DisableSpacingPixel
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
END SUB

SUB SetFont
DIM pixels%(7, 7)

FOR i% = 1 TO NumFontChars%
  charcode% = i% + FirstChar% - 1

  p! = (i% - 1) / (NumFontChars% - 1)

  FOR y% = 0 TO 7
    FOR x% = 0 TO 7
      IF RND <= p! THEN pixels%(x%, y%) = 1 ELSE pixels%(x%, y%) = 0
    NEXT x%
  NEXT y%

  SetFontCharacter charcode%, pixels%()
NEXT i%
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

FOR y% = 0 TO 7
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

SUB SetPalette
FOR i% = 0 TO 15
  c% = i% * 255 \ 15

  rr% = c%
  gg% = 255 \ 5
  bb% = 1 - c

  OUT &H3C8, i%
  OUT &H3C9, rr% * 63 \ 255
  OUT &H3C9, gg% * 63 \ 255
  OUT &H3C9, bb% * 63 \ 255

  PALETTE i%, i%
NEXT i%
END SUB

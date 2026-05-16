DECLARE SUB DisableSpacingPixel ()
DECLARE SUB SetFontCharacterString (charcode%, pixelstring$)
DECLARE SUB SetFontCharacter (charcode%, pixels%())

' Test code with 8x8 character
' (to do 8x16 characters, simply supply 16 8-character scans instead of 8 in the character data)
' (should work with 8x14 characters in 80x43 mode too)
WIDTH 80, 50 ' Switch to a mode with 8x8 characters

' Build up character data
ch$ = ""
ch$ = ch$ + "........"
ch$ = ch$ + "........"
ch$ = ch$ + "..XXXX.."
ch$ = ch$ + ".XX  XX."
ch$ = ch$ + ".XX  XX."
ch$ = ch$ + "..XXXX.."
ch$ = ch$ + "........"
ch$ = ch$ + "........"

' Optional: DisableSpacingPixel
' (without this, even though there's only 8x8 data, the VGA adapter displays 9x8 character boxes)

' Do the actual font update
SetFontCharacterString 1, ch$

' Try it out
PRINT "Sonic the Hedgehog's rings: " + STRING$(25, 1)

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

SUB SetFontCharacter (charcode%, pixels%())
CONST CRTControllerIndexPort% = &H3D4
CONST CRTControllerDataPort% = &H3D5

CONST MaximumScanLineRegister% = 9
CONST MaximumScanLineMask% = 31

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
CONST MiscGraphicsDisableText% = 1
CONST MiscGraphicsChainOddEven% = 2

OUT CRTControllerIndexPort%, MaximumScanLineRegister%
characterheight% = INP(CRTControllerDataPort%) AND MaximumScanLineMask%
OUT SequencerIndexPort%, MapMaskRegister%
savedmask% = INP(SequencerDataPort%)
OUT SequencerIndexPort%, MemoryModeRegister%
savedmode% = INP(SequencerDataPort%)
OUT GraphicsIndexPort%, MiscGraphicsRegister%
savedmisc% = INP(GraphicsDataPort%)

' Ensure we're not in a graphics mode
IF (savedmisc% AND MiscGraphicsDisableText%) <> 0 THEN ERROR 5

' Ensure supplied character data matches the current mode's character box
IF LBOUND(pixels%, 1) <> 0 OR UBOUND(pixels%, 1) <> 7 THEN ERROR 5
IF LBOUND(pixels%, 2) <> 0 OR UBOUND(pixels%, 2) <> characterheight% THEN ERROR 5

' Reconfigure memory mapping so that we can access the VGA text font
OUT SequencerIndexPort%, MapMaskRegister%
OUT SequencerDataPort%, MapMaskPlane2%

OUT SequencerIndexPort%, MemoryModeRegister%
OUT SequencerDataPort%, savedmode% OR MemoryModeOddEvenDisable%

OUT GraphicsIndexPort%, MiscGraphicsRegister%
OUT GraphicsDataPort%, savedmisc% AND NOT MiscGraphicsChainOddEven%

' Rewrite the specified character
DEF SEG = &HB800

baseoffset% = charcode% * 32

FOR y% = 0 TO characterheight%
  scan% = 0
  FOR x% = 0 TO 7
    scan% = scan% * 2 + pixels%(x%, y%)
  NEXT x%

  POKE baseoffset% + y%, scan%
NEXT y%

' Restore memory mapping
OUT SequencerIndexPort%, MapMaskRegister%
OUT SequencerDataPort%, savedmask%

OUT SequencerIndexPort%, MemoryModeRegister%
OUT SequencerDataPort%, savedmode%

OUT GraphicsIndexPort%, MiscGraphicsRegister%
OUT GraphicsDataPort%, savedmisc%
END SUB

SUB SetFontCharacterString (charcode%, pixelstring$)
' Make sure pixelstring$ is a supported length (we let SetFontCharacter validate the actual height)
height% = LEN(pixelstring$) \ 8

IF height% < 1 OR height% > 32 THEN ERROR 5
IF LEN(pixelstring$) <> height% * 8 THEN ERROR 5

' Convert the supplied string into an array for SetFontCharacter
DIM pixels%(0 TO 7, 0 TO height% - 1)

x% = 0
y% = 0

FOR i% = 1 TO LEN(pixelstring$)
  SELECT CASE ASC(MID$(pixelstring$, i%, 1))
    CASE 9: ' tab
    CASE 32: ' space
    CASE 46: ' dot
      ' treat these characters as "off" pixels
    CASE ELSE:
      pixels%(x%, y%) = 1
  END SELECT

  x% = x% + 1
  IF x% = 8 THEN
    x% = 0
    y% = y% + 1
  END IF
NEXT i%

' Have SetFontCharacter do the actual work
SetFontCharacter charcode%, pixels%()
END SUB

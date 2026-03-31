DECLARE SUB ClearGlyph (pixels%())
DECLARE SUB DrawGlyphLine (pixels%(), x1%, y1%, x2%, y2%)
DECLARE SUB InitializeFont ()
DECLARE SUB Set8x16Characters ()
DECLARE SUB SetFontCharacter (charcode%, pixels%())

1 REM TRANSLATED FROM THE BOOK: 10 PRINT CHR$(205.5 + RND(1)); : GOTO 10
2 REM CHAPTER 65: REM MAZE WALKER IN BASIC
3 REM https://10print.org/10_PRINT_121114.pdf
4 COLOR 11, 1
5 WIDTH 40, 25
6 DEF SEG = &HB800
7 InitializeFont

10 REM PRODUCE A STABLE MAZE
20 CLS
30 FOR i = 0 TO 999
40 POKE i * 2, INT(77.5 + RND)
50 NEXT

100 REM SET INITIAL X AND Y WALKER LOCATION AND DIRECTION
110 REM DIRECTION IS EITHER 0 LEFT, 1 RIGHT, 2 UP, 3 DOWN
120 x = INT(RND * 39): xold = -1
130 y = INT(RND * 24): yold = -1
140 dd = INT(RND * 3)
150 wold = -1
160 GOSUB 500

200 REM START WALKING MAZE USING RULES FOR BOUNCING OFF WALLS
210 REM COMPUTER NEW LOCATION BASED ON INITIAL DIRECTION
220 IF dd = 0 THEN x = x - 1: GOTO 270
230 IF dd = 1 THEN x = x + 1: GOTO 270
240 IF dd = 2 THEN y = y - 1: GOTO 270
250 IF dd = 3 THEN y = y + 1

260 REM DETERMINE IF THE WALKER IS OFF THE SCREEN
270 IF x >= 0 AND x <= 39 AND y >= 0 AND y <= 24 THEN GOTO 300
280 GOSUB 600: GOSUB 650
290 GOTO 10

300 REM BOUNCE OFF WALL AS FUNCTION OF DIRECTION
310 REM 77 IS \, 78 IS /
320 wall = PEEK(2 * (x + (y * 40)))
330 IF wall = 78 THEN GOTO 380
340 IF dd = 0 THEN dd = 2: GOTO 420
350 IF dd = 1 THEN dd = 3: GOTO 420
360 IF dd = 2 THEN dd = 0: GOTO 420
370 IF dd = 3 THEN dd = 1: GOTO 420
380 IF dd = 0 THEN dd = 3: GOTO 420
390 IF dd = 1 THEN dd = 2: GOTO 420
400 IF dd = 2 THEN dd = 1: GOTO 420
410 IF dd = 3 THEN dd = 0
420 GOTO 160

500 REM DRAW WALKER, RESTORING PREVIOUS WALL CHARACTER
510 GOSUB 600
520 xold = x: yold = y
530 m = 2 * (x + (y * 40))
540 wold = PEEK(m)
550 c = 1 + 2 * (x + (y * 40))
560 POKE c, 31: POKE m, 9
570 GOSUB 650
580 RETURN

600 REM RESTORE WALL AT PREVIOUS WALKER LOCATION
610 IF xold = -1 THEN RETURN
620 POKE 2 * (xold + (yold * 40)), wold
630 RETURN

650 REM PAUSE FOR 500 LOOPS
651 SELECT CASE INKEY$
      CASE " ": RETURN 10
      CASE IS <> "": END
    END SELECT
660 st = TIMER: WHILE st = TIMER: WEND
670 RETURN

SUB ClearGlyph (pixels%())
FOR y% = 0 TO 15
  FOR x% = 0 TO 7
    pixels%(x%, y%) = 0
  NEXT x%
NEXT y%
END SUB

SUB DrawGlyphLine (pixels%(), x1%, y1%, x2%, y2%)
dx% = ABS(x2% - x1%)
dy% = ABS(y2% - y1%)

IF dx% > dy% THEN
  IF x1% > x2% THEN SWAP x1%, x2%: SWAP y1%, y2%
  FOR x% = x1% TO x2%
    y% = (x% - x1%) * (y2% - y1%) \ (x2% - x1%) + y1%
    pixels%(x%, y%) = 1
  NEXT x%
ELSE
  IF y1% > y2% THEN SWAP x1%, x2%: SWAP y1%, y2%
  FOR y% = y1% TO y2%
    x% = (y% - y1%) * (x2% - x1%) \ (y2% - y1%) + x1%
    pixels%(x%, y%) = 1
  NEXT y%
END IF
END SUB

SUB InitializeFont
DIM pixels%(0 TO 7, 0 TO 15)

ClearGlyph pixels%()
DrawGlyphLine pixels%(), 0, 0, 7, 15
DrawGlyphLine pixels%(), 1, 0, 7, 14
DrawGlyphLine pixels%(), 1, 0, 7, 13
DrawGlyphLine pixels%(), 0, 1, 6, 15
DrawGlyphLine pixels%(), 0, 2, 6, 15

SetFontCharacter 77, pixels%()

ClearGlyph pixels%()
DrawGlyphLine pixels%(), 7, 0, 0, 15
DrawGlyphLine pixels%(), 6, 0, 0, 14
DrawGlyphLine pixels%(), 6, 0, 0, 13
DrawGlyphLine pixels%(), 7, 1, 1, 15
DrawGlyphLine pixels%(), 7, 2, 1, 15

SetFontCharacter 78, pixels%()

Set8x16Characters
END SUB

SUB Set8x16Characters
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
CONST SequencerIndexPort% = &H3C4
CONST SequencerDataPort% = &H3C5

CONST MapMaskRegister% = 2
CONST MapMaskPlane0% = 1
CONST MapMaskPlane1% = 2
CONST MapMaskPlane2% = 4
CONST MapMaskPlane3% = 8

CONST GraphicsIndexPort% = &H3CE
CONST GraphicsDataPort% = &H3CF

CONST MiscGraphicsRegister% = 6
CONST MiscGraphicsChainOddEven% = 2

OUT SequencerIndexPort%, MapMaskRegister%
savedmask% = INP(SequencerDataPort%)
OUT GraphicsIndexPort%, MiscGraphicsRegister%
savedmode% = INP(GraphicsDataPort%)

OUT SequencerIndexPort%, MapMaskRegister%
OUT SequencerDataPort%, MapMaskPlane2%

OUT GraphicsIndexPort%, MiscGraphicsRegister%
OUT GraphicsDataPort%, savedmode% AND NOT MiscGraphicsChainOddEven%

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

OUT GraphicsIndexPort%, MiscGraphicsRegister%
OUT GraphicsDataPort%, savedmode%
END SUB


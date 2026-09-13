DECLARE SUB epilogue ()
DECLARE SUB gameover ()
DECLARE SUB initvideo ()
DECLARE SUB render (board%())
DECLARE SUB uninitvideo ()
DECLARE SUB gameloop ()
DECLARE SUB initpieces ()
DECLARE SUB addpiece (board%(), x%, y%, piecetype%, rotation%)
DECLARE FUNCTION testpiece% (board%(), x%, y%, piecetype%, rotation%)
DECLARE SUB delpiece (board%(), x%, y%, piecetype%, rotation%)
DECLARE SUB deletefulllines (board%())
CONST BOARDWIDTH% = 13
CONST BOARDHEIGHT% = 25

CONST TICKSPERDROP% = 10
CONST TICKSWAITDELETE% = 5

CONST ROTnone% = 0
CONST ROT90% = 1
CONST ROT180% = 2
CONST ROT270% = 3
CONST ROT360% = 4

CONST DELETEDPIECE% = 15

DIM SHARED piece%(500), pieceofs%(50), numpieces%

initpieces

initvideo
gameloop
uninitvideo

DATA 7

DATA 2,2
DATA 1,1
DATA 1,1

DATA 3,2
DATA 2,2,2
DATA 2,0,0

DATA 3,2
DATA 3,3,3
DATA 0,3,0

DATA 3,2
DATA 4,4,4
DATA 0,0,4

DATA 3,2
DATA 5,5,0
DATA 0,5,5

DATA 3,2
DATA 0,6,6
DATA 6,6,0

DATA 4,1
DATA 7,7,7,7

SUB addpiece (board%(), x%, y%, piecetype%, rotation%)
idx% = pieceofs%(piecetype%)
w% = piece%(idx%)
h% = piece%(idx% + 1)
idx% = idx% + 2
SELECT CASE rotation%
 CASE ROTnone%, ROT360%:
  FOR yy% = 1 TO h%
   FOR xx% = 1 TO w%
    rx% = xx% + (x% - 1) - 1
    ry% = yy% + (y% - 1) - 1
    IF (rx% > 0) AND (rx% <= BOARDWIDTH%) THEN
     IF (ry% > 0) AND (ry% <= BOARDHEIGHT%) THEN
      IF piece%(idx%) THEN board%(rx%, ry%) = piece%(idx%)
     END IF
    END IF
    idx% = idx% + 1
   NEXT xx%
  NEXT yy%
 CASE ROT90%:
  FOR yy% = 1 TO h%
   FOR xx% = 1 TO w%
    rx% = x% + (yy% - 1) - 1
    ry% = y% + (1 - xx%) - 1
    IF (rx% > 0) AND (rx% <= BOARDWIDTH%) THEN
     IF (ry% > 0) AND (ry% <= BOARDHEIGHT%) THEN
      IF piece%(idx%) THEN board%(rx%, ry%) = piece%(idx%)
     END IF
    END IF
    idx% = idx% + 1
   NEXT xx%
  NEXT yy%
 CASE ROT180%:
  FOR yy% = 1 TO h%
   FOR xx% = 1 TO w%
    rx% = x% + (1 - xx%) - 1
    ry% = y% + (1 - yy%) - 1
    IF (rx% > 0) AND (rx% <= BOARDWIDTH%) THEN
     IF (ry% > 0) AND (ry% <= BOARDHEIGHT%) THEN
      IF piece%(idx%) THEN board%(rx%, ry%) = piece%(idx%)
     END IF
    END IF
    idx% = idx% + 1
   NEXT xx%
  NEXT yy%
 CASE ROT270%:
  FOR yy% = 1 TO h%
   FOR xx% = 1 TO w%
    rx% = x% + (1 - yy%) - 1
    ry% = y% + (xx% - 1) - 1
    IF (rx% > 0) AND (rx% <= BOARDWIDTH%) THEN
     IF (ry% > 0) AND (ry% <= BOARDHEIGHT%) THEN
      IF piece%(idx%) THEN board%(rx%, ry%) = piece%(idx%)
     END IF
    END IF
    idx% = idx% + 1
   NEXT xx%
  NEXT yy%
END SELECT
END SUB

SUB deletefulllines (board%())
DIM deletedrow%(BOARDHEIGHT%)
deletedlines% = 0
FOR y% = 1 TO BOARDHEIGHT%
 FOR x% = 1 TO BOARDWIDTH%
  IF board%(x%, y%) = 0 THEN EXIT FOR
 NEXT x%
 IF x% > BOARDWIDTH% THEN
  FOR x% = 1 TO BOARDWIDTH%
   board%(x%, y%) = DELETEDPIECE%
  NEXT x%
  deletedrow%(y%) = -1
  deletedlines% = -1
 END IF
NEXT y%
IF deletedlines% THEN
 render board%()
 FOR i% = 1 TO TICKSWAITDELETE%
  st# = TIMER
  WHILE st# = TIMER: WEND
 NEXT i%
 FOR y% = 1 TO BOARDHEIGHT%
  IF deletedrow%(y%) THEN
   FOR x% = 1 TO BOARDWIDTH%
    board%(x%, y%) = 0
   NEXT x%
  END IF
 NEXT y%
 render board%()
 FOR i% = 1 TO TICKSWAITDELETE%
  st# = TIMER
  WHILE st# = TIMER: WEND
 NEXT i%
 FOR y% = BOARDHEIGHT% TO 1 STEP -1
  IF deletedrow%(y%) THEN
   FOR yy% = y% - 1 TO 1 STEP -1
    FOR x% = 1 TO BOARDWIDTH%
     board%(x%, yy% + 1) = board%(x%, yy%)
    NEXT x%
    deletedrow%(yy% + 1) = deletedrow%(yy%)
   NEXT yy%
   st# = TIMER
   render board%()
   WHILE st# = TIMER: WEND
   y% = y% + 1
  END IF
 NEXT y%
END IF
END SUB

SUB delpiece (board%(), x%, y%, piecetype%, rotation%)
idx% = pieceofs%(piecetype%)
w% = piece%(idx%)
h% = piece%(idx% + 1)
idx% = idx% + 2
SELECT CASE rotation%
 CASE ROTnone%, ROT360%:
  FOR yy% = 1 TO h%
   FOR xx% = 1 TO w%
    rx% = xx% + (x% - 1) - 1
    ry% = yy% + (y% - 1) - 1
    IF (rx% > 0) AND (rx% <= BOARDWIDTH%) THEN
     IF (ry% > 0) AND (ry% <= BOARDHEIGHT%) THEN
      IF piece%(idx%) THEN board%(rx%, ry%) = 0
     END IF
    END IF
    idx% = idx% + 1
   NEXT xx%
  NEXT yy%
 CASE ROT90%:
  FOR yy% = 1 TO h%
   FOR xx% = 1 TO w%
    rx% = x% + (yy% - 1) - 1
    ry% = y% + (1 - xx%) - 1
    IF (rx% > 0) AND (rx% <= BOARDWIDTH%) THEN
     IF (ry% > 0) AND (ry% <= BOARDHEIGHT%) THEN
      IF piece%(idx%) THEN board%(rx%, ry%) = 0
     END IF
    END IF
    idx% = idx% + 1
   NEXT xx%
  NEXT yy%
 CASE ROT180%:
  FOR yy% = 1 TO h%
   FOR xx% = 1 TO w%
    rx% = x% + (1 - xx%) - 1
    ry% = y% + (1 - yy%) - 1
    IF (rx% > 0) AND (rx% <= BOARDWIDTH%) THEN
     IF (ry% > 0) AND (ry% <= BOARDHEIGHT%) THEN
      IF piece%(idx%) THEN board%(rx%, ry%) = 0
     END IF
    END IF
    idx% = idx% + 1
   NEXT xx%
  NEXT yy%
 CASE ROT270%:
  FOR yy% = 1 TO h%
   FOR xx% = 1 TO w%
    rx% = x% + (1 - yy%) - 1
    ry% = y% + (xx% - 1) - 1
    IF (rx% > 0) AND (rx% <= BOARDWIDTH%) THEN
     IF (ry% > 0) AND (ry% <= BOARDHEIGHT%) THEN
      IF piece%(idx%) THEN board%(rx%, ry%) = 0
     END IF
    END IF
    idx% = idx% + 1
   NEXT xx%
  NEXT yy%
END SELECT
END SUB

SUB epilogue
gameover
SLEEP
END SUB

SUB gameloop
DIM board%(BOARDWIDTH%, BOARDHEIGHT%)
pieceX% = BOARDWIDTH% \ 2
pieceY% = 2
'piecetype% = INT(RND * numpieces%) + 1

piecetype% = 7
FOR i% = 1 TO BOARDWIDTH% - 1
 FOR j% = BOARDHEIGHT% TO BOARDHEIGHT% - 5 STEP -1
  board%(i%, j%) = ((j% AND 1) <> 0) AND INT(RND * piecetype%) + 1
 NEXT j%
NEXT i%

piecerotation% = ROTnone%
quit% = 0
redraw% = -1
drop% = 0
autodrop% = 0
DO
 addpiece board%(), pieceX%, pieceY%, piecetype%, piecerotation%
 render board%()
 delpiece board%(), pieceX%, pieceY%, piecetype%, piecerotation%
 redraw% = 0
 DO
  st# = TIMER
  DO
   a$ = INKEY$
   IF LEN(a$) THEN
    SELECT CASE a$
     CASE CHR$(0) + "H":
      FOR i% = ROT90% TO ROT270%
       IF testpiece%(board%(), pieceX%, pieceY%, piecetype%, piecerotation% + i%) THEN
        piecerotation% = (piecerotation% + i%) MOD ROT360%
        redraw% = -1
        EXIT FOR
       END IF
      NEXT i%
     CASE CHR$(0) + "K":
      IF testpiece%(board%(), pieceX% - 1, pieceY%, piecetype%, piecerotation%) THEN
       pieceX% = pieceX% - 1
       redraw% = -1
      END IF
     CASE CHR$(0) + "M":
      IF testpiece%(board%(), pieceX% + 1, pieceY%, piecetype%, piecerotation%) THEN
       pieceX% = pieceX% + 1
       redraw% = -1
      END IF
     CASE CHR$(0) + "P":
      drop% = -1
     CASE " ":
      autodrop% = -1
     CASE CHR$(27):
      quit% = -1
    END SELECT
   END IF
   IF autodrop% THEN drop% = -1
  LOOP UNTIL (TIMER <> st#) OR drop%
  ticks% = ticks% + 1
  IF ticks% >= TICKSPERDROP% THEN drop% = -1
  IF drop% THEN
   drop% = 0
   ticks% = 0
   pieceY% = pieceY% + 1
   redraw% = -1
   IF testpiece%(board%(), pieceX%, pieceY%, piecetype%, piecerotation%) = 0 THEN
    addpiece board%(), pieceX%, pieceY% - 1, piecetype%, piecerotation%
    deletefulllines board%()
    pieceX% = BOARDWIDTH% \ 2
    pieceY% = 2
    piecetype% = INT(RND * numpieces%) + 1
    piecerotation% = ROTnone%
    autodrop% = 0
    IF testpiece%(board%(), pieceX%, pieceY%, piecetype%, piecerotation%) = 0 THEN
     quit% = -1
     EXIT DO
    END IF
   END IF
  END IF
 LOOP UNTIL redraw%
LOOP UNTIL quit%
epilogue
END SUB

SUB gameover
LOCATE 45, 35
PRINT "Game Over."
END SUB

SUB initpieces
idx% = 0
READ numpieces%
FOR i% = 1 TO numpieces%
 pieceofs%(i%) = idx%
 READ piecewidth%, pieceheight%
 piece%(idx%) = piecewidth%
 piece%(idx% + 1) = pieceheight%
 idx% = idx% + 2
 FOR y% = 1 TO pieceheight%
  FOR x% = 1 TO piecewidth%
   READ piece%(idx%)
   idx% = idx% + 1
  NEXT x%
 NEXT y%
NEXT i%
END SUB

SUB initvideo
SCREEN 0
WIDTH 80, 50
CLS
LOCATE , , 0
END SUB

SUB render (board%())
FOR y% = 1 TO BOARDHEIGHT%
 LOCATE y%, 39 - BOARDWIDTH% \ 2
 COLOR 15
 PRINT CHR$(219);
 FOR x% = 1 TO BOARDWIDTH%
  COLOR board%(x%, y%)
  PRINT CHR$(219);
 NEXT x%
 COLOR 15
 PRINT CHR$(219);
NEXT y%
LOCATE y%, 39 - BOARDWIDTH% \ 2
PRINT STRING$(BOARDWIDTH% + 2, 219);
END SUB

FUNCTION testpiece% (board%(), x%, y%, piecetype%, rotation%)
idx% = pieceofs%(piecetype%)
w% = piece%(idx%)
h% = piece%(idx% + 1)
idx% = idx% + 2
SELECT CASE rotation%
 CASE ROTnone%, ROT360%:
  FOR yy% = 1 TO h%
   FOR xx% = 1 TO w%
    rx% = xx% + (x% - 1) - 1
    ry% = yy% + (y% - 1) - 1
    IF (rx% <= 0) OR (rx% > BOARDWIDTH%) THEN EXIT FUNCTION
    IF (ry% > BOARDHEIGHT%) THEN EXIT FUNCTION
    IF ry% > 0 THEN
     IF (board%(rx%, ry%) <> 0) AND (piece%(idx%) <> 0) THEN EXIT FUNCTION
    END IF
    idx% = idx% + 1
   NEXT xx%
  NEXT yy%
 CASE ROT90%:
  FOR yy% = 1 TO h%
   FOR xx% = 1 TO w%
    rx% = x% + (yy% - 1) - 1
    ry% = y% + (1 - xx%) - 1
    IF (rx% <= 0) OR (rx% > BOARDWIDTH%) THEN EXIT FUNCTION
    IF (ry% > BOARDHEIGHT%) THEN EXIT FUNCTION
    IF ry% > 0 THEN
     IF (board%(rx%, ry%) <> 0) AND (piece%(idx%) <> 0) THEN EXIT FUNCTION
    END IF
    idx% = idx% + 1
   NEXT xx%
  NEXT yy%
 CASE ROT180%:
  FOR yy% = 1 TO h%
   FOR xx% = 1 TO w%
    rx% = x% + (1 - xx%) - 1
    ry% = y% + (1 - yy%) - 1
    IF (rx% <= 0) OR (rx% > BOARDWIDTH%) THEN EXIT FUNCTION
    IF (ry% > BOARDHEIGHT%) THEN EXIT FUNCTION
    IF ry% > 0 THEN
     IF (board%(rx%, ry%) <> 0) AND (piece%(idx%) <> 0) THEN EXIT FUNCTION
    END IF
    idx% = idx% + 1
   NEXT xx%
  NEXT yy%
 CASE ROT270%:
  FOR yy% = 1 TO h%
   FOR xx% = 1 TO w%
    rx% = x% + (1 - yy%) - 1
    ry% = y% + (xx% - 1) - 1
    IF (rx% <= 0) OR (rx% > BOARDWIDTH%) THEN EXIT FUNCTION
    IF (ry% > BOARDHEIGHT%) THEN EXIT FUNCTION
    IF ry% > 0 THEN
     IF (board%(rx%, ry%) <> 0) AND (piece%(idx%) <> 0) THEN EXIT FUNCTION
    END IF
    idx% = idx% + 1
   NEXT xx%
  NEXT yy%
END SELECT
testpiece% = -1
END FUNCTION

SUB uninitvideo
SCREEN 0
WIDTH 80, 25
END SUB


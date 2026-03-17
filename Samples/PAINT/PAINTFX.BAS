DECLARE SUB puttextxy (x%, y%, a$, c%)
DECLARE SUB drawbutton (x%, y%, x2%, y2%, t$)
DECLARE SUB setuser (method%, reverse%(), array%(), user%(), divide%, userdivide%, bias%, userbias%)
DECLARE SUB editnumber (x%, y%, num%, char%())
DECLARE SUB setarray (method%, array%(), divider%, bias%, user%(), userdivide%, userbias%)
TYPE regtypex
 ax AS INTEGER
 bx AS INTEGER
 cx AS INTEGER
 dx AS INTEGER
 bp AS INTEGER
 si AS INTEGER
 di AS INTEGER
 flags AS INTEGER
 ds AS INTEGER
 es AS INTEGER
END TYPE
TYPE rgbtype
 r AS INTEGER
 g AS INTEGER
 b AS INTEGER
END TYPE

SUB drawbutton (x%, y%, x2%, y2%, t$)
LINE (x%, y%)-(x% + 3, y% + 3), 7
LINE -(x% + 3, y2% - 3), 7
LINE -(x%, y2%), 7
LINE -(x%, y%), 7
PAINT (x% + 1, y% + 2), 7, 7
LINE (x%, y%)-(x% + 3, y% + 3), 15
LINE -(x2% - 3, y% + 3), 15
LINE -(x2%, y%), 15
LINE -(x%, y%), 15
PAINT (x% + 3, y% + 2), 15, 15
LINE (x%, y2%)-(x% + 3, y2% - 3), 8
LINE -(x2% - 3, y2% - 3), 8
LINE -(x2% - 3, y% + 3), 8
LINE -(x2%, y%), 8
LINE -(x2%, y2%), 8
LINE -(x%, y2%), 8
PAINT (x2% - 2, y2% - 3), 8, 8
LINE (x%, y%)-(x% + 3, y% + 3), 0
LINE -(x% + 3, y2% - 3), 8
LINE (x2%, y2%)-(x2% - 3, y2% - 3), 0
LINE (x% + 4, y% + 4)-(x2% - 4, y2% - 4), 7, BF
puttextxy x% + 6, y% + 5, t$, 15
LINE (x%, y%)-(x2%, y2%), 0, B
END SUB

SUB effects
DIM scr&(16000), scr2&(16000), reverse%(962), freecolors%(255), highlight%(52)
DIM editmask%(194), array%(5, 5), user%(5, 5), regs AS regtypex
DIM total(5, 5) AS rgbtype, pal(255) AS rgbtype
DO: regs.ax = 3: CALL interrupt(&H33, regs, regs): LOOP UNTIL regs.bx = 0
DEF SEG = &HA000
GET (0, 0)-(319, 199), scr&
FOR i% = 0 TO 255: freecolors%(i%) = -1: NEXT i%
FOR i& = 0 TO 63999: freecolors%(PEEK(i&)) = 0: NEXT i&
FOR i% = 0 TO 255: colorsleft% = colorsleft% - freecolors%(i%): NEXT i%
CLS : COLOR 15
PRINT STRING$(30, 219);
GET (0, 0)-(239, 7), reverse%
GET (0, 0)-(47, 7), editmask%
LINE (0, 0)-(10, 10), 15, BF
GET (1, 1)-(10, 10), highlight%
CLS
PRINT "Smooth"
PRINT "Smooth +"
PRINT "Blur"
PRINT "Contour"
PRINT "Detail"
PRINT "Smear"
PRINT "User Defined"
PUT (0, 0), reverse%, XOR
LINE (260, 10)-(271, 21), 15, B
LINE (271, 10)-(282, 21), 15, B
LINE (282, 10)-(293, 21), 15, B
LINE (260, 21)-(271, 32), 15, B
LINE (282, 21)-(293, 32), 15, B
LINE (260, 32)-(271, 43), 15, B
LINE (271, 32)-(282, 43), 15, B
LINE (282, 32)-(293, 43), 15, B

LINE (268, 18)-(263, 13), 15
LINE (263, 13)-(263, 17), 15
LINE (263, 13)-(267, 13), 15

LINE (276, 19)-(276, 12), 15
LINE (276, 12)-(273, 15), 15
LINE (276, 12)-(279, 15), 15

LINE (285, 18)-(290, 13), 15
LINE (290, 13)-(290, 17), 15
LINE (290, 13)-(286, 13), 15

FOR i% = 1 TO 10
 FOR j% = 1 TO 10
  PSET (i% + 260, j% + 32), POINT(i% + 260, 21 - j%)
  PSET (i% + 271, j% + 32), POINT(i% + 271, 21 - j%)
  PSET (i% + 282, j% + 32), POINT(i% + 282, 21 - j%)
  PSET (i% + 260, j% + 21), POINT(j% + 271, i% + 10)
  PSET (i% + 282, j% + 21), POINT(j% + 271, 21 - i%)
 NEXT j%
NEXT i%

drawbutton 20, 70, 66, 86, "Exec"
drawbutton 237, 70, 300, 86, "Cancel"

method% = 1
setarray 1, array%(), divide%, bias%, user%(), userdivide%, userbias%

direction% = 0: PUT (272, 22), highlight%, XOR
DO
 FOR i% = 1 TO 5
  FOR j% = 1 TO 5
   LOCATE j% + 15, i% * 6 - 4
   PRINT USING "#####"; array%(i%, j%);
  NEXT j%
 NEXT i%
 LOCATE 22, 5
 PRINT "Divider:"; divide%
 LOCATE 22, 25
 PRINT "Bias:"; bias%
 regs.ax = 1: CALL interrupt(&H33, regs, regs)
 DO
  regs.ax = 3
  CALL interrupt(&H33, regs, regs)
 LOOP UNTIL regs.bx
 IF regs.bx = 2 THEN EXIT DO
 DO: regs.ax = 3: CALL interrupt(&H33, regs, regs): LOOP WHILE regs.bx
 x% = regs.cx \ 2
 y% = regs.dx
 regs.ax = 2: CALL interrupt(&H33, regs, regs)
 IF (x% < 240) AND (y% < 56) THEN
  PUT (0, method% * 8 - 8), reverse%, XOR
  method% = y% \ 8 + 1
  setarray method%, array%(), divide%, bias%, user%(), userdivide%, userbias%
  PUT (0, method% * 8 - 8), reverse%, XOR
 END IF
 IF (x% > 260) AND (y% > 10) THEN
  IF (x% < 293) AND (y% < 43) THEN
   SELECT CASE direction%
    CASE 0: PUT (272, 22), highlight%, XOR
    CASE 1: PUT (272, 11), highlight%, XOR
    CASE 2: PUT (283, 11), highlight%, XOR
    CASE 3: PUT (283, 22), highlight%, XOR
    CASE 4: PUT (283, 33), highlight%, XOR
    CASE 5: PUT (272, 33), highlight%, XOR
    CASE 6: PUT (261, 33), highlight%, XOR
    CASE 7: PUT (261, 22), highlight%, XOR
    CASE 8: PUT (261, 11), highlight%, XOR
   END SELECT
   x% = (x% - 250) \ 11
   y% = y% \ 11
   IF (x% = 1) AND (y% = 1) THEN direction% = 8
   IF (x% = 2) AND (y% = 2) THEN direction% = 0
   IF (x% = 2) AND (y% = 1) THEN direction% = 1
   IF (x% = 3) AND (y% = 1) THEN direction% = 2
   IF (x% = 3) AND (y% = 2) THEN direction% = 3
   IF (x% = 3) AND (y% = 3) THEN direction% = 4
   IF (x% = 2) AND (y% = 3) THEN direction% = 5
   IF (x% = 1) AND (y% = 3) THEN direction% = 6
   IF (x% = 1) AND (y% = 2) THEN direction% = 7
   SELECT CASE direction%
    CASE 0: PUT (272, 22), highlight%, XOR
    CASE 1: PUT (272, 11), highlight%, XOR
    CASE 2: PUT (283, 11), highlight%, XOR
    CASE 3: PUT (283, 22), highlight%, XOR
    CASE 4: PUT (283, 33), highlight%, XOR
    CASE 5: PUT (272, 33), highlight%, XOR
    CASE 6: PUT (261, 33), highlight%, XOR
    CASE 7: PUT (261, 22), highlight%, XOR
    CASE 8: PUT (261, 11), highlight%, XOR
   END SELECT
  END IF
 END IF
 IF (y% > 119) AND (y% < 160) THEN
  IF (x% < 232) THEN
   setuser method%, reverse%(), array%(), user%(), divide%, userdivide%, bias%, userbias%
   x% = x% \ 48 + 1
   y% = (y% - 112) \ 8
   LOCATE y% + 15, x% * 6 - 4: PRINT SPACE$(5); : LOCATE y% + 15, x% * 6 - 4
   LINE INPUT a$
   user%(x%, y%) = VAL(a$)
   array%(x%, y%) = user%(x%, y%)
  END IF
 END IF
 IF (y% > 167) AND (y% < 176) THEN
  IF (x% > 103) AND (x% < 192) THEN
   setuser method%, reverse%(), array%(), user%(), divide%, userdivide%, bias%, userbias%
   LOCATE 22, 14: PRINT SPACE$(10); : LOCATE 22, 14
   LINE INPUT a$
   divide% = VAL(a$)
   userdivide% = divide%
  END IF
  IF (x% > 240) THEN
   setuser method%, reverse%(), array%(), user%(), divide%, userdivide%, bias%, userbias%
   LOCATE 22, 31: PRINT SPACE$(10); : LOCATE 22, 31
   LINE INPUT a$
   bias% = VAL(a$)
   userbias% = bias%
  END IF
 END IF
 IF (y% > 70) AND (y% < 86) THEN
  IF (x% > 20) AND (x% < 66) THEN f% = -1: EXIT DO
  IF (x% > 237) AND (x% < 300) THEN f% = 0: EXIT DO
 END IF
LOOP
PUT (0, 0), scr&, PSET
IF f% THEN
 DEF SEG = &HA000
 p% = 0
 IF direction% = 0 THEN
  direction% = 1
  DEF SEG = VARSEG(scr&(1))
  p% = VARPTR(scr&(1))
 END IF
 FOR i% = 0 TO 255
  OUT &H3C7, i%
  pal(i%).r = INP(&H3C9)
  pal(i%).g = INP(&H3C9)
  pal(i%).b = INP(&H3C9)
 NEXT i%
 SELECT CASE direction%
  CASE 1:
   FOR j% = 199 TO 0 STEP -1
    FOR i% = 0 TO 319
     GOSUB iterate
    NEXT i%
   NEXT j%
  CASE 2:
   FOR i% = 0 TO 319
    FOR j% = 199 TO 0 STEP -1
     IF i% - (199 - j%) < 0 THEN EXIT FOR
     i% = i% - (199 - j%)
     GOSUB iterate
     i% = i% + (199 - j%)
    NEXT j%
   NEXT i%
   FOR j% = 198 TO 0 STEP -1
    FOR i% = 319 - j% TO 319
     j% = j% - (319 - i%)
     GOSUB iterate
     j% = j% + (319 - i%)
    NEXT i%
   NEXT j%
  CASE 3:
   FOR i% = 0 TO 319
    FOR j% = 0 TO 199
     GOSUB iterate
    NEXT j%
   NEXT i%
  CASE 4:
   FOR i% = 0 TO 319
    FOR j% = 0 TO 199
     IF i% - j% < 0 THEN EXIT FOR
     i% = i% - j%
     GOSUB iterate
     i% = i% + j%
    NEXT j%
   NEXT i%
   FOR j% = 1 TO 199
    FOR i% = 319 - (199 - j%) TO 319
     j% = j% + (319 - i%)
     GOSUB iterate
     j% = j% - (319 - i%)
    NEXT i%
   NEXT j%
  CASE 5:
   FOR j% = 0 TO 199
    FOR i% = 0 TO 319
     GOSUB iterate
    NEXT i%
   NEXT j%
  CASE 6:
   FOR i% = 319 TO 0 STEP -1
    FOR j% = 0 TO 199
     IF i% + j% = 320 THEN EXIT FOR
     i% = i% + j%
     GOSUB iterate
     i% = i% - j%
    NEXT j%
   NEXT i%
   FOR j% = 1 TO 199
    FOR i% = 0 TO 199 - j%
     j% = j% + i%
     GOSUB iterate
     j% = j% - i%
    NEXT i%
   NEXT j%
  CASE 7:
   FOR i% = 319 TO 0 STEP -1
    FOR j% = 0 TO 199
     GOSUB iterate
    NEXT j%
   NEXT i%
  CASE 8:
   FOR i% = 319 TO 0 STEP -1
    FOR j% = 199 TO 0 STEP -1
     IF i% + (199 - j%) = 320 THEN EXIT FOR
     i% = i% + (199 - j%)
     GOSUB iterate
     i% = i% - (199 - j%)
    NEXT j%
   NEXT i%
   FOR j% = 198 TO 0 STEP -1
    FOR i% = 0 TO j%
     j% = j% - i%
     GOSUB iterate
     j% = j% + i%
    NEXT i%
   NEXT j%
 END SELECT
 GET (0, 0)-(319, 199), scr2&
 LOCATE 13, 10
 PRINT "Keep changes?"
 DO
  a$ = UCASE$(INPUT$(1))
 LOOP UNTIL INSTR("YN", a$)
 SELECT CASE a$
  CASE "Y": PUT (0, 0), scr2&, PSET
  CASE "N": PUT (0, 0), scr&, PSET
 END SELECT
END IF
EXIT SUB
iterate:
 PSET (i%, j%), 15
 IF (i% > 1) AND (j% > 1) THEN total(1, 1) = pal(PEEK(p% + j% * 320& + i% - 642))
 IF (i% > 0) AND (j% > 1) THEN total(2, 1) = pal(PEEK(p% + j% * 320& + i% - 641))
 IF (j% > 1) THEN total(3, 1) = pal(PEEK(p% + j% * 320& + i% - 640))
 IF (i% < 319) AND (j% > 1) THEN total(4, 1) = pal(PEEK(p% + j% * 320& + i% - 639))
 IF (i% < 318) AND (j% > 1) THEN total(5, 1) = pal(PEEK(p% + j% * 320& + i% - 638))
 IF (i% > 1) AND (j% > 0) THEN total(1, 2) = pal(PEEK(p% + j% * 320& + i% - 322))
 IF (i% > 0) AND (j% > 0) THEN total(2, 2) = pal(PEEK(p% + j% * 320& + i% - 321))
 IF (j% > 0) THEN total(3, 2) = pal(PEEK(p% + j% * 320& + i% - 320))
 IF (i% < 319) AND (j% > 0) THEN total(4, 2) = pal(PEEK(p% + j% * 320& + i% - 319))
 IF (i% < 318) AND (j% > 0) THEN total(5, 2) = pal(PEEK(p% + j% * 320& + i% - 318))
 IF (i% > 1) THEN total(1, 3) = pal(PEEK(p% + j% * 320& + i% - 2))
 IF (i% > 0) THEN total(2, 3) = pal(PEEK(p% + j% * 320& + i% - 1))
 total(3, 3) = pal(PEEK(p% + j% * 320& + i%))
 IF (i% < 319) THEN total(4, 3) = pal(PEEK(p% + j% * 320& + i% + 1))
 IF (i% < 318) THEN total(5, 3) = pal(PEEK(p% + j% * 320& + i% + 2))
 IF (i% > 1) AND (j% < 199) THEN total(1, 4) = pal(PEEK(p% + j% * 320& + i% + 318))
 IF (i% > 0) AND (j% < 199) THEN total(2, 4) = pal(PEEK(p% + j% * 320& + i% + 319))
 IF (j% < 199) THEN total(3, 4) = pal(PEEK(p% + j% * 320& + i% + 320))
 IF (i% < 319) AND (j% < 199) THEN total(4, 4) = pal(PEEK(p% + j% * 320& + i% + 321))
 IF (i% < 318) AND (j% < 199) THEN total(5, 4) = pal(PEEK(p% + j% * 320& + i% + 322))
 IF (i% > 1) AND (j% < 198) THEN total(1, 5) = pal(PEEK(p% + j% * 320& + i% + 638))
 IF (i% > 0) AND (j% < 198) THEN total(2, 5) = pal(PEEK(p% + j% * 320& + i% + 639))
 IF (j% < 198) THEN total(3, 5) = pal(PEEK(p% + j% * 320& + i% + 640))
 IF (i% < 319) AND (j% < 198) THEN total(4, 5) = pal(PEEK(p% + j% * 320& + i% + 641))
 IF (i% < 318) AND (j% < 198) THEN total(5, 5) = pal(PEEK(p% + j% * 320& + i% + 642))
 tr& = 0: tg& = 0: tb& = 0
 FOR k% = 1 TO 5
  FOR l% = 1 TO 5
   tr& = tr& + total(k%, l%).r * array%(k%, l%)
   tg& = tg& + total(k%, l%).g * array%(k%, l%)
   tb& = tb& + total(k%, l%).b * array%(k%, l%)
  NEXT l%
 NEXT k%
 tr& = tr& \ divide%: tg& = tg& \ divide%: tb& = tb& \ divide%
 tr& = tr& + bias%: tg& = tg& + bias%: tb& = tb& + bias%
 IF (tr& < 0) THEN tr& = 0 ELSE IF (tr& > 63) THEN tr& = 63
 IF (tg& < 0) THEN tg& = 0 ELSE IF (tg& > 63) THEN tg& = 63
 IF (tb& < 0) THEN tb& = 0 ELSE IF (tb& > 63) THEN tb& = 63
 FOR k% = 0 TO 255
  IF (pal(k%).r = tr&) AND (pal(k%).g = tg&) AND (pal(k%).b = tb&) THEN di% = k%: EXIT FOR
 NEXT k%
 IF k% = 256 THEN
  IF colorsleft% THEN
   colorsleft% = colorsleft% - 1
   FOR k% = 0 TO 255
    IF freecolors%(k%) THEN freecolors%(k%) = 0: EXIT FOR
   NEXT k%
   IF k% = 256 THEN GOTO normal
   di% = k%
   OUT &H3C8, k%: OUT &H3C9, tr&: OUT &H3C9, tg&: OUT &H3C9, tb&
   pal(k%).r = tr&: pal(k%).g = tg&: pal(k%).b = tb&
  ELSE
normal:
   dd% = 500
   FOR k% = 0 TO 255
    d% = ABS(pal(k%).r - tr&) + ABS(pal(k%).g - tg&) + ABS(pal(k%).b - tb&)
    IF dd% > d% THEN di% = k%: dd% = d%
   NEXT k%
  END IF
 ELSE
  freecolors%(k%) = 0
 END IF
 PSET (i%, j%), di%
 RETURN
END SUB

SUB setarray (method%, array%(), divider%, bias%, user%(), userdivide%, userbias%)
SELECT CASE method%
 CASE 1:
  array%(1, 1) = 0: array%(2, 1) = 0: array%(3, 1) = 0: array%(4, 1) = 0: array%(5, 1) = 0
  array%(1, 2) = 0: array%(2, 2) = 1: array%(3, 2) = 1: array%(4, 2) = 1: array%(5, 2) = 0
  array%(1, 3) = 0: array%(2, 3) = 1: array%(3, 3) = 2: array%(4, 3) = 1: array%(5, 3) = 0
  array%(1, 4) = 0: array%(2, 4) = 1: array%(3, 4) = 1: array%(4, 4) = 1: array%(5, 4) = 0
  array%(1, 5) = 0: array%(2, 5) = 0: array%(3, 5) = 0: array%(4, 5) = 0: array%(5, 5) = 0
  divider% = 10: bias% = 0
 CASE 2:
  array%(1, 1) = 1: array%(2, 1) = 1: array%(3, 1) = 1: array%(4, 1) = 1: array%(5, 1) = 1
  array%(1, 2) = 1: array%(2, 2) = 2: array%(3, 2) = 2: array%(4, 2) = 2: array%(5, 2) = 1
  array%(1, 3) = 1: array%(2, 3) = 2: array%(3, 3) = 3: array%(4, 3) = 2: array%(5, 3) = 1
  array%(1, 4) = 1: array%(2, 4) = 2: array%(3, 4) = 2: array%(4, 4) = 2: array%(5, 4) = 1
  array%(1, 5) = 1: array%(2, 5) = 1: array%(3, 5) = 1: array%(4, 5) = 1: array%(5, 5) = 1
  divider% = 35: bias% = 0
 CASE 3:
  array%(1, 1) = 1: array%(2, 1) = 1: array%(3, 1) = 1: array%(4, 1) = 1: array%(5, 1) = 1
  array%(1, 2) = 1: array%(2, 2) = 0: array%(3, 2) = 0: array%(4, 2) = 0: array%(5, 2) = 1
  array%(1, 3) = 1: array%(2, 3) = 0: array%(3, 3) = 0: array%(4, 3) = 0: array%(5, 3) = 1
  array%(1, 4) = 1: array%(2, 4) = 0: array%(3, 4) = 0: array%(4, 4) = 0: array%(5, 4) = 1
  array%(1, 5) = 1: array%(2, 5) = 1: array%(3, 5) = 1: array%(4, 5) = 1: array%(5, 5) = 1
  divider% = 16: bias% = 0
 CASE 4:
  array%(1, 1) = 0: array%(2, 1) = 0: array%(3, 1) = 0: array%(4, 1) = 0: array%(5, 1) = 0
  array%(1, 2) = 0: array%(2, 2) = -1: array%(3, 2) = -1: array%(4, 2) = -1: array%(5, 2) = 0
  array%(1, 3) = 0: array%(2, 3) = -1: array%(3, 3) = 8: array%(4, 3) = -1: array%(5, 3) = 0
  array%(1, 4) = 0: array%(2, 4) = -1: array%(3, 4) = -1: array%(4, 4) = -1: array%(5, 4) = 0
  array%(1, 5) = 0: array%(2, 5) = 0: array%(3, 5) = 0: array%(4, 5) = 0: array%(5, 5) = 0
  divider% = 1: bias% = 0
 CASE 5:
  array%(1, 1) = 0: array%(2, 1) = 0: array%(3, 1) = 0: array%(4, 1) = 0: array%(5, 1) = 0
  array%(1, 2) = 0: array%(2, 2) = -2: array%(3, 2) = -2: array%(4, 2) = -2: array%(5, 2) = 0
  array%(1, 3) = 0: array%(2, 3) = -2: array%(3, 3) = 32: array%(4, 3) = -2: array%(5, 3) = 0
  array%(1, 4) = 0: array%(2, 4) = -2: array%(3, 4) = -2: array%(4, 4) = -2: array%(5, 4) = 0
  array%(1, 5) = 0: array%(2, 5) = 0: array%(3, 5) = 0: array%(4, 5) = 0: array%(5, 5) = 0
  divider% = 16: bias% = 0
 CASE 6:
  array%(1, 1) = 0: array%(2, 1) = 0: array%(3, 1) = 0: array%(4, 1) = 0: array%(5, 1) = 0
  array%(1, 2) = 0: array%(2, 2) = 4: array%(3, 2) = 4: array%(4, 2) = 4: array%(5, 2) = 0
  array%(1, 3) = 0: array%(2, 3) = 4: array%(3, 3) = 1: array%(4, 3) = 4: array%(5, 3) = 0
  array%(1, 4) = 0: array%(2, 4) = 4: array%(3, 4) = 4: array%(4, 4) = 4: array%(5, 4) = 0
  array%(1, 5) = 0: array%(2, 5) = 0: array%(3, 5) = 0: array%(4, 5) = 0: array%(5, 5) = 0
  divider% = 33: bias% = 0
 CASE 7:
  FOR i% = 1 TO 5
   FOR j% = 1 TO 5
    array%(i%, j%) = user%(i%, j%)
   NEXT j%
  NEXT i%
  divider% = userdivide%: bias% = userbias%
END SELECT
END SUB

SUB setuser (method%, reverse%(), array%(), user%(), divide%, userdivide%, bias%, userbias%)
 PUT (0, method% * 8 - 8), reverse%, XOR
 method% = 7
 FOR i% = 1 TO 5: FOR j% = 1 TO 5: user%(i%, j%) = array%(i%, j%): NEXT j%, i%
 userdivide% = divide%
 userbias% = bias%
 PUT (0, method% * 8 - 8), reverse%, XOR
END SUB


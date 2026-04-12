DECLARE SUB testmode (mode%, w%, h%, s$)
r$ = "Report:" + CHR$(13) + CHR$(10)
SCREEN 1

testmode 1, 320, 200, r$
testmode 2, 640, 200, r$
testmode 7, 320, 200, r$
testmode 8, 640, 200, r$
testmode 9, 640, 350, r$
testmode 12, 640, 480, r$
testmode 13, 320, 200, r$

SCREEN 0
WIDTH 80
PRINT r$

SUB testmode (mode%, w%, h%, s$)
SCREEN mode%

'CIRCLE (0, 0), 50
CIRCLE (0, 0), 50, , , , .5

FOR x% = w% - 1 TO 0 STEP -1
  IF POINT(x%, 0) THEN EXIT FOR
NEXT x%

FOR y% = h% - 1 TO 0 STEP -1
  IF POINT(0, y%) THEN EXIT FOR
NEXT y%

s$ = s$ + "SCREEN " + STR$(mode%) + " " + STR$(w%) + "x" + STR$(h%) + ": radius 50 = (" + STR$(x%) + " x " + STR$(y%) + ")" + CHR$(13)
END SUB


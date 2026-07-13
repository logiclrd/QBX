SCREEN 7
FOR i% = 255 TO 0 STEP -1
  PSET (i%, 0), 0
  DRAW "C" + STR$(i%) + "D10"
NEXT i%

OPEN "TESTOUT$" FOR OUTPUT AS #1

c% = 0

FOR i% = 0 TO 255
  IF POINT(i%, 0) <> c% THEN PRINT #1, "FAIL"
  IF c% < 15 THEN c% = c% + 1
NEXT i%

PRINT #1, "PASS OTHERWISE"

CLOSE #1

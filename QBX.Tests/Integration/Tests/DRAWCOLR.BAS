SCREEN 13

COLOR 2

DIM expected%(10)

expected%(0) = 15
expected%(1) = 4
expected%(2) = 4 ' from 1 LINE
expected%(3) = 2 ' from COLOR
expected%(4) = 2 ' from 3 LINE
expected%(5) = 0
expected%(6) = 0 ' from 5 PRESET
expected%(7) = 11
expected%(8) = 11 ' from 7 PRESET
expected%(9) = 2
expected%(10) = 2 ' from 9 PSET

0 DRAW "bm 1,0 m 6,0"
1 LINE (1, 1)-(6, 1), 4
2 DRAW "bm 1,2 m 6,2"
3 LINE (1, 3)-(6, 3)
4 DRAW "bm 1,4 m 6,4"
5 FOR x% = 1 TO 6: PRESET (x%, 5): NEXT x%
6 DRAW "bm 1,6 m 6,6"
7 FOR x% = 1 TO 6: PRESET (x%, 7), 11: NEXT x%
8 DRAW "bm 1,8 m 6,8"
9 FOR x% = 1 TO 6: PSET (x%, 9): NEXT x%
10 DRAW "bm 1,10 m 6,10"

FOR i% = 0 TO 10
  LINE (9, i%)-(14, i%), expected%(i%)
NEXT i%

LOCATE 3, 1
PRINT "AE"

OPEN "TESTOUT$" FOR OUTPUT AS #1

FOR i% = 0 TO 10
  IF POINT(2, i%) <> expected%(i%) THEN PRINT #1, "FAIL"
NEXT i%

PRINT #1, "PASS OTHERWISE"

CLOSE #1


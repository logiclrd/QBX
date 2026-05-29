' Regression test: paint border in column 0

SCREEN 13

LINE (0, 0)-(0, 199), 8

PAINT (160, 100), 1, 8

OPEN "TESTOUT$" FOR OUTPUT AS #1

FOR y% = 0 TO 199
  IF POINT(0, y%) <> 8 THEN
    PRINT #1, "FAIL"
    EXIT FOR
  END IF

  FOR x% = 1 TO 319
    IF POINT(x%, y%) <> 1 THEN
      PRINT #1, "FAIL"
      EXIT FOR
    END IF
  NEXT x%
NEXT y%

PRINT #1, "PASS" ' if no FAILs

CLOSE #1


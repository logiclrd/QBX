SCREEN 13

x1% = 150
y1% = 90
x2% = 160
y2% = 95

VIEW (x1%, y1%)-(x2%, y2%), 1, 2
DRAW "u5"
VIEW

mx% = (x1% + x2%) \ 2
my% = (y1% + y2%) \ 2

' Expand to include border
x1% = x1% - 1: x2% = x2% + 1
y1% = y1% - 1: y2% = y2% + 1

FOR x% = x1% TO x2%
  FOR y% = y1% TO y2%
    IF x% = x1% OR x% = x2% OR y% = y1% OR y% = y2% THEN
      expectedc% = 2
    ELSEIF x% = mx% AND y% <= my% THEN
      expectedc% = 15
    ELSE
      expectedc% = 1
    END IF

    IF POINT(x%, y%) <> expectedc% THEN END
  NEXT y%
NEXT x%

OPEN "TESTOUT$" FOR OUTPUT AS #1
PRINT #1, "PASS"
CLOSE #1

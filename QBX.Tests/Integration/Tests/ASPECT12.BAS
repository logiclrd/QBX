DECLARE SUB MeasureCircle (x%, y%, r%, a#, mx%, my%)

SCREEN 12

' Aspect value of -1.57 taken from GORILLA.BAS.
MeasureCircle 160, 100, 50, -1.57, x%, y%

IF x% = 101 AND y% = 43 THEN
  OPEN "TESTOUT$" FOR OUTPUT AS #1
  PRINT #1, "PASS"
  CLOSE #1
END IF

SUB MeasureCircle (x%, y%, r%, a#, mx%, my%)
CLS
CIRCLE (x%, y%), r%, , , , a#

mx% = 0
my% = 0

FOR i% = 0 TO 319
  found% = 0
  FOR j% = 0 TO 199
    IF POINT(i%, j%) THEN found% = 1: EXIT FOR
  NEXT j%

  IF found% THEN
    mx% = 320 - i%
    EXIT FOR
  END IF
NEXT i%

IF mx% > 0 THEN
  FOR i% = 319 TO 0 STEP -1
    found% = 0
    FOR j% = 0 TO 199
      IF POINT(i%, j%) THEN found% = 1: EXIT FOR
    NEXT j%

    IF found% THEN EXIT FOR ELSE mx% = mx% - 1
  NEXT i%
END IF

FOR j% = 0 TO 199
  found% = 0
  FOR i% = 0 TO 319
    IF POINT(i%, j%) THEN found% = 1: EXIT FOR
  NEXT i%

  IF found% THEN
    my% = 200 - j%
    EXIT FOR
  END IF
NEXT j%

IF my% > 0 THEN
  FOR j% = 199 TO 0 STEP -1
    found% = 0
    FOR i% = 0 TO 319
      IF POINT(i%, j%) THEN found% = 1: EXIT FOR
    NEXT i%

    IF found% THEN EXIT FOR ELSE my% = my% - 1
  NEXT j%
END IF
END SUB


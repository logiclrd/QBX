sequence$ = ""

FOR i = 1 TO 10
  SELECT CASE i
    CASE 1: sequence$ = sequence$ + "1"
    CASE 2 TO 4, 7: sequence$ = sequence$ + "2"
    CASE IS >= 8: sequence$ = sequence$ + "3"
    CASE ELSE: sequence$ = sequence$ + "4"
  END SELECT
NEXT i

IF sequence$ = "1222442333" THEN
  OPEN "TESTOUT$" FOR OUTPUT AS #1
  PRINT #1, "PASS"
  CLOSE #1
END IF


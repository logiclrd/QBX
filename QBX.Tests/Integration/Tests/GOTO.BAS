trace$ = "1"

n% = -1

top: 
IF n% AND 1 = 0 THEN
  label1: trace$ = trace$ + "2"
  GOTO label2
ELSE
  trace$ = trace$ + "3"
  GOTO label3
END IF

label2: 
  n% = n% + 1
  trace$ = trace$ + "4"
  GOTO label4

label3: 
  n% = n% + 1
  trace$ = trace$ + "5"
  IF n% >= 25 THEN
    IF trace$ = "13524635246352463524635246352463524635246352463524635246352463524635" THEN
      OPEN "TESTOUT$" FOR OUTPUT AS #1
      PRINT #1, "PASS"
      CLOSE #1
    END IF
    SYSTEM
  END IF
  GOTO label1

label4: 
  trace$ = trace$ + "6"
  GOTO top

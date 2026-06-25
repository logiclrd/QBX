TYPE testtype
  items(1 TO 5) AS LONG
END TYPE

DIM struct AS testtype

OPEN "TESTOUT$" FOR OUTPUT AS #1

IF LBOUND(struct.items) <> 1 THEN PRINT #1, "FAIL"
IF UBOUND(struct.items) <> 5 THEN PRINT #1, "FAIL"

PRINT #1, "PASS OTHERWISE"

CLOSE #1

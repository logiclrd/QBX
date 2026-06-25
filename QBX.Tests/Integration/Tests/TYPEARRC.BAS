CONST n% = 3
CONST m% = 5
CONST o% = 7

TYPE testtype
  items(n% TO m% + o%) AS LONG
END TYPE

DIM struct AS testtype

OPEN "TESTOUT$" FOR OUTPUT AS #1

IF LBOUND(struct.items) <> n% THEN PRINT #1, "FAIL"
IF UBOUND(struct.items) <> m% + o% THEN PRINT #1, "FAIL"

PRINT #1, "PASS OTHERWISE"

CLOSE #1

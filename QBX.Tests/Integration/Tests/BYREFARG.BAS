DECLARE SUB test (a$)

t$ = "old value"

test t$

IF t$ = "new value" THEN
  OPEN "TESTOUT$" FOR OUTPUT AS #1
  PRINT #1, "PASS"
  CLOSE #1
END IF

SUB test (a$)
a$ = "new value"
END SUB
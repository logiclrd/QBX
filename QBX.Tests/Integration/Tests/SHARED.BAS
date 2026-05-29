a% = 35
before% = a%
foo
after% = a%

IF before% = 35 AND after% = 42 THEN
  OPEN "TESTOUT$" FOR OUTPUT AS #1
  PRINT #1, "PASS"
  CLOSE #1
END IF

SUB foo
SHARED a%
a% = 42
END SUB

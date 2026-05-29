DECLARE SUB foo (r%)

foo t%
foo u%
foo v%

IF t% = 1 AND u% = 2 AND v% = 3 THEN
  OPEN "TESTOUT$" FOR OUTPUT AS #1
  PRINT #1, "PASS"
  CLOSE #1
END IF

SUB foo (r%)
STATIC a%

a% = a% + 1
r% = a%
END SUB

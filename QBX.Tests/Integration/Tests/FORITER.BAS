' Verify that FOR loop iterators can be modified while the loop is running

OPEN "TESTOUT$" FOR OUTPUT AS #1

FOR i = 1 TO 10
  IF i = 3 THEN i = 7
  IF i > 3 AND i < 7 THEN PRINT #1, "FAIL"
NEXT i

PRINT #1, "PASS OTHERWISE"

CLOSE #1

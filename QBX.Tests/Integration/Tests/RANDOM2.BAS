' Arrange
OPEN "random2.tmp" FOR OUTPUT AS #1
PRINT #1, CHR$(3) + CHR$(0) + "abc";
CLOSE #1

OPEN "random2.tmp" FOR RANDOM AS #1 LEN = 5

lb% = LOC(1)
GET #1, , a$
la% = LOC(1)

CLOSE #1

OPEN "TESTOUT$" FOR OUTPUT AS #1
IF a$ <> "abc" THEN PRINT #1, "FAIL because a$ does not contain the expected string"
IF lb% <> 0 THEN PRINT #1, "FAIL because LOC(1) did not return 0 in the initial state"
IF la% <> 1 THEN PRINT #1, "FAIL because LOC(1) did not return 1 after reading one record"
PRINT #1, "PASS OTHERWISE"
CLOSE #1


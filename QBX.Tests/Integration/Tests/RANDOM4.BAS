' Arrange
OPEN "random4.tmp" FOR OUTPUT AS #1
PRINT #1, CHR$(3) + CHR$(0) + "abc";
CLOSE #1

OPEN "random4.tmp" FOR RANDOM AS #1 LEN = 5
FIELD #1, 3 AS fa$, 2 AS fb$
GET #1
a$ = fa$
b$ = fb$
CLOSE #1

OPEN "TESTOUT$" FOR OUTPUT AS #1
IF a$ <> CHR$(3) + CHR$(0) + "a" THEN PRINT #1, "FAIL"
IF b$ <> "bc" THEN PRINT #1, "FAIL"
PRINT #1, "PASS OTHERWISE"
CLOSE #1


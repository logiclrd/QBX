READ a$
READ b$

OPEN "TESTOUT$" FOR OUTPUT AS #1
IF a$ <> "a" + CHR$(34) + "test1," + CHR$(34) + "c" THEN PRINT #1, "FAIL"
IF b$ <> "b" THEN PRINT #1, "FAIL"
PRINT #1, "PASS OTHERWISE"
CLOSE #1

DATA a"test1,"c,b

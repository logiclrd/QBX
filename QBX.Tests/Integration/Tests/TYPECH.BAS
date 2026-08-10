z = 1
z% = 2
z& = 3
z! = 4
z# = 5
z@ = 6
z$ = "7"

OPEN "TESTOUT$" FOR OUTPUT AS #1

IF z% <> 2 THEN PRINT #1, "FAIL"
IF z& <> 3 THEN PRINT #1, "FAIL"
IF z! <> 4 THEN PRINT #1, "FAIL"
IF z# <> 5 THEN PRINT #1, "FAIL"
IF z@ <> 6 THEN PRINT #1, "FAIL"
IF z$ <> "7" THEN PRINT #1, "FAIL"

IF z <> 4 THEN PRINT #1, "FAIL"

PRINT #1, "PASS OTHERWISE"

CLOSE #1


' explicit failure
OPEN "TESTOUT$" FOR OUTPUT AS #1
PRINT #1, "FAIL"
CLOSE #1

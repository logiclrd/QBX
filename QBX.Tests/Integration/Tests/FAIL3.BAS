' FAIL signal even when PASS signal is also present
OPEN "TESTOUT$" FOR OUTPUT AS #1
PRINT #1, "PASS"
PRINT #1, "FAIL"
PRINT #1, "PASS"
CLOSE #1

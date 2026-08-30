DIM a%(5)

test a%()

OPEN "TESTOUT$" FOR OUTPUT AS #1
IF a%(3) = 42 THEN PRINT #1, "PASS"
CLOSE #1

SUB test (x%())
x%(3) = 42
END SUB

DECLARE SUB foo (map%())

DIM SHARED map%(5), map#(10)
DIM othermap%(5)

map%(0) = 1
map#(0) = 2
othermap%(0) = 42

foo othermap%()


SUB foo (map%())
DEFINT M
IF map(0) = 42 THEN
  OPEN "TESTOUT$" FOR OUTPUT AS #1
  PRINT #1, "PASS"
  CLOSE #1
END IF
END SUB

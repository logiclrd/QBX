ON ERROR GOTO foo

caughterror% = 0

ERROR 5

IF caughterror% THEN
  OPEN "TESTOUT$" FOR OUTPUT AS #1
  PRINT #1, "PASS"
  CLOSE #1
END IF

SYSTEM

foo:
  caughterror% = 1
  PRINT "hi"
  RESUME NEXT

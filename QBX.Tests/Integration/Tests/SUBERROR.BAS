DECLARE SUB RaiseError ()

ON ERROR GOTO foo

caughterror% = 0

RaiseError

IF caughterror% THEN
  OPEN "TESTOUT$" FOR OUTPUT AS #1
  PRINT #1, "PASS"
  CLOSE #1
END IF

SYSTEM

foo:
  IF ERR = 2 THEN caughterror% = 1
  PRINT "hi"
  RESUME NEXT

SUB RaiseError
ERROR 2
END SUB
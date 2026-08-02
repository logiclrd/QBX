DECLARE SUB triggerandcatcherror ()

OPEN "TESTOUT$" FOR OUTPUT AS #1

triggerandcatcherror

' ERR should persist even though the local error handler scope ended
IF ERR <> 5 THEN PRINT #1, "FAIL"

CLOSE #1

SUB triggerandcatcherror
ON LOCAL ERROR RESUME NEXT

a$ = SPACE$(-1)

IF ERR = 5 THEN
  PRINT #1, "PASS"
END IF
END SUB

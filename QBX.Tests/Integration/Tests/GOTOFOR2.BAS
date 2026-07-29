' Test 2: Skipping FOR with the loop iterator
' before the end of the bounds but greater than 0.

l = 0
i = 3
GOTO in1

FOR i = 1 TO 5
in1:
  l = l + 1
  IF l > 1 THEN END
  IF i <> 3 THEN END
  ' Expected: will hit this point exactly once,
  ' i = 3, FOR loop's captured parameters are
  ' from = 0, to = 0, step = 0, NEXT terminates
  ' on the first go
NEXT i

' Expected: i = 3, because we skipped the FOR
' line, so the parameters were never captured.
' As such, the step of 0 was added to the
' initial i = 10 and then the loop terminated.
IF i <> 3 THEN END

OPEN "TESTOUT$" FOR OUTPUT AS #1
PRINT #1, "PASS"
CLOSE #1

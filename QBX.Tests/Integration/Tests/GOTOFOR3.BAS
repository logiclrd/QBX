' Test 3: Skipping FOR with the loop iterator
' equal to 0.

l = 0
i = 0
GOTO in1

FOR i = 1 TO 5
in1:
  IF i <> 0 THEN END
  l = l + 1
  IF l > 5 THEN
    OPEN "TESTOUT$" FOR OUTPUT AS #1
    PRINT #1, "PASS"
    CLOSE #1
    END
  END IF
    
  ' Expected: will loop forever, i never changes.
  ' FOR loop's captured parameters are from = 0,
  ' to = 0, step = 0, step of 0 is added to loop
  ' iterator and loop iterator is never greater
  ' than to.
NEXT i

' If we get here, the test failed.
DECLARE SUB Test (a$)

a$ = "456789"

Test LEFT$(a$, 2)

IF a$ = "456789" THEN
  OPEN "TESTOUT$" FOR OUTPUT AS #1
  PRINT #1, "PASS"
  CLOSE #1
END IF

SUB Test (a$)
a$ = "123"
END SUB

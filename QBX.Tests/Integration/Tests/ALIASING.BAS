DECLARE SUB foo (a$, b$, c$, d$, e$, f$)
a$ = "spoo"
foo MID$(a$, 2, 2), a$, a$, r1$, r2$, r3$

OPEN "TESTOUT$" FOR OUTPUT AS #1

IF r1$ <> "spoo" THEN
  PRINT #1, "FAIL"
ELSEIF r2$ <> "fork" THEN
  PRINT #1, "FAIL"
ELSEIF r3$ <> "knif" THEN
  PRINT #1, "FAIL"
ELSE
  PRINT #1, "PASS"
END IF

CLOSE #1

SUB foo (a$, b$, c$, d$, e$, f$)
' Assign to the result of MID$(a$, 2, 2), but passed as an argument
' (should not alter the underlying string)
a$ = "tr"
d$ = b$

' Assign to the caller's a$ through one argument
b$ = "fork"
e$ = c$

' Assign to the caller's a$ through a second argument
c$ = "knif"
f$ = b$
END SUB

a$ = "testing"

DEF SEG = SSEG(a$)

POKE SADD(a$), ASC("j")

IF a$ = "jesting" THEN
  OPEN "TESTOUT$" FOR OUTPUT AS #1
  PRINT #1, "PASS"
  CLOSE #1
END IF

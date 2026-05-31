TYPE RegTypeX
  AX AS INTEGER
  BX AS INTEGER
  CX AS INTEGER
  DX AS INTEGER
  BP AS INTEGER
  SI AS INTEGER
  DI AS INTEGER
  FLAGS AS INTEGER
  DS AS INTEGER
  ES AS INTEGER
END TYPE

DECLARE SUB InterruptX (i%, rin AS RegTypeX, rout AS RegTypeX)

DIM rin AS RegTypeX, rout AS RegTypeX

buf$ = "a" + CHR$(8) + "b$"

rin.AX = &H900
rin.DS = SSEG(buf$)
rin.DX = SADD(buf$)

PRINT LEFT$(buf$, INSTR(buf$, "$") - 1)

CALL InterruptX(&H21, rin, rout)

OPEN "TESTOUT$" FOR OUTPUT AS #1

' Expected results:
' - On line 1, the QuickBASIC PRINT statement has output characters 'a', 8 and 'b'
' - On line 2, the DOS INT 21h call has output character 'a', then backspaced the cursor and overwritten it with 'b'

IF SCREEN(1, 1) <> ASC("a") THEN PRINT #1, "FAIL"
IF SCREEN(1, 2) <> 8 THEN PRINT #1, "FAIL"
IF SCREEN(1, 3) <> ASC("b") THEN PRINT #1, "FAIL"

IF SCREEN(2, 1) <> ASC("b") THEN PRINT #1, "FAIL"
IF SCREEN(2, 2) <> 32 THEN PRINT #1, "FAIL"

PRINT #1, "PASS OTHERWISE"

CLOSE #1

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

DECLARE SUB InterruptX (r%, rin AS RegTypeX, rout AS RegTypeX)

DIM rin AS RegTypeX, rout AS RegTypeX

buffer$ = SPACE$(200)

rin.DS = SSEG(buffer$)
rin.SI = SADD(buffer$)

rin.AX = &H4700 ' Get current directory (unrooted)

CALL InterruptX(&H21, rin, rout)

n% = INSTR(buffer$, CHR$(0))

IF n% = 0 THEN n% = LEN(buffer$)

buffer$ = LEFT$(buffer$, n% - 1)

IF buffer$ = MID$(CURDIR$, 4) THEN
  OPEN "TESTOUT$" FOR OUTPUT AS #1
  PRINT #1, "PASS"
  CLOSE #1
END IF


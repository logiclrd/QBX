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
rin.ES = SADD(buffer$)

rin.AX = &H4700

CALL InterruptX(&H21, rin, rout)

CLS 

n% = INSTR(buffer$, CHR$(0))

IF n% = 0 THEN n% = LEN(buffer$)

PRINT LEFT$(buffer$, n%)

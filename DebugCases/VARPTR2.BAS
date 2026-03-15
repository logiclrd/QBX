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

DIM buffer%(100) AS INTEGER

rin.DS = VARSEG(buffer%(0))
rin.ES = VARPTR(buffer%(0))

rin.AX = &H4700

CALL InterruptX(&H21, rin, rout)

CLS 

FOR i% = 0 TO UBOUND(buffer%)
  d$ = MKI$(buffer%(i%))

  n% = INSTR(d$, CHR$(0))

  IF n% = 0 THEN
    PRINT d$;
  ELSE
    PRINT LEFT$(d$, n%)
    EXIT FOR
  END IF
NEXT i%

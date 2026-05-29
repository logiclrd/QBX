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

DIM buffer%(100)

rin.DS = VARSEG(buffer%(0))
rin.SI = VARPTR(buffer%(0))

rin.AX = &H4700

CALL InterruptX(&H21, rin, rout)

result$ = ""

FOR i% = 0 TO UBOUND(buffer%)
  d$ = MKI$(buffer%(i%))

  n% = INSTR(d$, CHR$(0))

  IF n% = 0 THEN
    result$ = result$ + d$
  ELSE
    result$ = result$ + LEFT$(d$, n% - 1)
    EXIT FOR
  END IF
NEXT i%

IF result$ = MID$(CURDIR$, 4) THEN
  OPEN "TESTOUT$" FOR OUTPUT AS #1
  PRINT #1, "PASS"
  CLOSE #1
END IF

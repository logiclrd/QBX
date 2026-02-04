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

rin.AX = 1
rin.BX = 2
rin.CX = 3
rin.DX = 4
rin.BP = 5
rin.SI = 6
rin.DI = 7
rin.FLAGS = 8
rin.DS = 9
rin.ES = 10

CALL InterruptX(&H33, rin, rout)

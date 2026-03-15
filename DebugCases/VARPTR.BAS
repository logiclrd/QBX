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

DIM rin AS RegTypeX

rin.AX = &H201
rin.BX = &H403
rin.CX = &H605
rin.DX = &H807
rin.BP = &HA09
rin.SI = &HC0B
rin.DI = &HE0D
rin.FLAGS = &H100F
rin.DS = &H1211
rin.ES = &H1413

DEF SEG = VARSEG(rin)

CLS 

FOR i% = 1 TO &H14
  a& = VARPTR(rin) + i% - 1

  b% = PEEK(a&)
  PRINT b%;
  POKE a&, NOT b%
NEXT i%

PRINT 

DEF FNh$ (n%) = RIGHT$("000" + HEX$(n%), 4)

PRINT FNh$(rin.AX)
PRINT FNh$(rin.BX)
PRINT FNh$(rin.CX)
PRINT FNh$(rin.DX)
PRINT FNh$(rin.BP)
PRINT FNh$(rin.SI)
PRINT FNh$(rin.DI)
PRINT FNh$(rin.FLAGS)
PRINT FNh$(rin.DS)
PRINT FNh$(rin.ES)

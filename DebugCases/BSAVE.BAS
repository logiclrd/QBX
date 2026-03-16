DIM buffer%(10)

FOR i% = 1 TO 10
  buffer%(i%) = i%
NEXT i%

DEF SEG = VARSEG(buffer%(1))

BSAVE "spoons.cag", VARPTR(buffer%(1)), 20


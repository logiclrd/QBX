DIM buffer%(10)

DEF SEG = VARSEG(buffer%(1))

BLOAD "spoons.cag", VARPTR(buffer%(1))

FOR i% = 1 TO 10
  PRINT buffer%(i%)
NEXT i%


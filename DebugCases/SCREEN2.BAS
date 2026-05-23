SCREEN 2

DEF SEG = &HB800

FOR y% = 16 TO 31
  FOR x% = 16 TO 31 STEP 8
    POKE y% * 80 + x% \ 8, 255
  NEXT x%
NEXT y%

SLEEP

OUT &H3C7, 0

FOR i% = 0 TO 15
  PRINT RIGHT$("0" + HEX$(INP(&H3C9)), 2);
  PRINT RIGHT$("0" + HEX$(INP(&H3C9)), 2);
  PRINT RIGHT$("0" + HEX$(INP(&H3C9)), 2);
  PRINT "  ";
NEXT i%

SLEEP

PRINT "--"

FOR i% = 0 TO 15
  a% = INP(&H3DA)

  OUT &H3C0, i%

  PRINT INP(&H3C1);
NEXT i%

SLEEP


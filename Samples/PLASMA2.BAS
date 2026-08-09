SCREEN 13
DIM monkey&(16000)
DEF SEG = &HA000
DO
 o% = 0
 x% = x% + 1
 y% = y% + 1
 FOR j% = 0 TO 199
  FOR i% = 0 TO 319
   POKE o%, (SIN(COS((i% ^ 1.9 + x% ^ .8) / 5) * ATN(x%) / 15) + ATN(j% ^ 1.5) - COS(j% / 12)) * 15
   IF o% = 32767 THEN o% = -32768 ELSE o% = o% + 1
  NEXT i%
 NEXT j%
 GET (0, 0)-(319, 199), monkey&
 f% = f% + 1
 'DEF SEG = VARSEG(monkey&(0))
 'BSAVE "frame" + LTRIM$(RTRIM$(STR$(f%))) + ".bsv", VARPTR(monkey&(0)), 64004
 DEF SEG = &HA000
LOOP UNTIL INKEY$ <> ""
SCREEN 0: WIDTH 80, 25


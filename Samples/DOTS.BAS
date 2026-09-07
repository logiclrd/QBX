TYPE dtype
 x AS INTEGER  'base point
 y AS INTEGER
 px AS INTEGER 'phase
 py AS INTEGER
 v AS LONG     'current on-screen point
END TYPE
'$DYNAMIC
CONST maxdots% = 4000
DIM dots(maxdots%) AS dtype
DIM sintab%(3600), costab%(3600)
FOR i% = 0 TO 3600
 sintab%(i%) = SIN(i% / 57.295779#) * 20
 costab%(i%) = COS(i% / 57.295779#) * 20
NEXT i%
FOR i% = 1 TO maxdots%
 dots(i%).x = INT(RND * 360)
 dots(i%).y = INT(RND * 240)
 dots(i%).px = dots(i%).x * 2
 dots(i%).py = dots(i%).y * 2

 dots(i%).x = dots(i%).x - 20
 dots(i%).y = dots(i%).y - 20
NEXT i%
SCREEN 13
DEF SEG = 0
FOR i% = 0 TO 255
 OUT &H3C8, i%
 OUT &H3C9, i% \ 4
 OUT &H3C9, i% \ 7
 OUT &H3C9, i% \ 13
NEXT i%
vretrace% = VAL(COMMAND$)
DO
 t% = PEEK(1132)
 DEF SEG = &HA000
 ft% = (ft% + 4) MOD 360
 FOR i% = 1 TO maxdots%
  POKE dots(i%).v, 0
  vx% = dots(i%).x + sintab%(dots(i%).py + ft%)
  vy% = dots(i%).y + costab%(dots(i%).px + ft%)
  dots(i%).v = vy% * 320& + vx%
  IF (dots(i%).v < 0) OR (dots(i%).v > 64001) OR (vx% < 0) OR (vx% > 319) THEN dots(i%).v = 64001
  POKE dots(i%).v, 255
 NEXT i%
 DEF SEG = 0
 IF vretrace% THEN WHILE (t% = PEEK(1132)): WEND
LOOP UNTIL INKEY$ <> ""
SCREEN 0
WIDTH 80, 25


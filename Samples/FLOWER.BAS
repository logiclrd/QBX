DEFINT A-Z
SCREEN 7, 0, 1, 0
DO
toadd = toadd + 10
FOR x = 0 TO 720
eenp = (SIN((x + toadd) * 3.14 / 360) + SIN((x * 3 + toadd) * 3.14 / 270)) * 30
xloc = SIN(x * 3.14 / 360) * 30
yloc = COS(x * 3.14 / 360) * 30
curx = 160 + xloc * eenp / 30
cury = 100 + yloc * eenp / 30
IF x > 0 THEN LINE (curx, cury)-(lastx, lasty), 2 ELSE PSET (curx, cury), 2
lastx = curx
lasty = cury
NEXT
PCOPY 1, 0
CLS
LOOP UNTIL INKEY$ > ""


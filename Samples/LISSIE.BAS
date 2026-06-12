rows% = 50

WIDTH 80, rows%
CLS

dot$ = CHR$(249)

LOCATE 1, 38
PRINT "Lissie"

FOR i% = 2 TO rows% - 1
  LOCATE i%, 40
  PRINT dot$;
NEXT i%

LOCATE (rows% + 1) \ 2, 1
PRINT STRING$(80, 249)

a# = 12
b# = 17

DO
  t# = t# + .005

  x% = INT(41 + 38.5 * SIN(a# * t#))
  y% = INT((rows% + 2) / 2 + (rows% - 2) * .5 * SIN(b# * t#))

  LOCATE y%, x%
  PRINT "*";

  st! = TIMER: WHILE st! = TIMER: WEND
LOOP WHILE INKEY$ = ""

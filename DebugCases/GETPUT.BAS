mode% = 10 ' 2, 7, 8, 9, 10, 11, 12 exercise packed 1 bit per plane

colours% = (mode% <> 2) AND (mode% <> 10)

SCREEN mode%

FOR i% = 1 TO 15
  LINE (0, 0)-(i% * 15, 15), i%
NEXT i%

DIM a%(500)

IF colours% THEN c% = 4 ELSE c% = 1

FOR i% = 0 TO 20
  FOR j% = 1 TO 20
    LINE (0, 30)-(639, 45), c%, BF
    GET (i%, 0)-(i% + j% - 1, 15), a%
    PUT (i%, 30), a%
    SLEEP
  NEXT j%
NEXT i%

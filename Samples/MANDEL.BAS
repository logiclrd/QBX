'mandelbrot red-dark nuBasic => QuickBASIC
'Based on the micro(A) port
CONST MAXITERATION% = 223

SCREEN 13

FOR i% = 0 TO 255
  OUT &H3C8, i%
  OUT &H3C9, i% * 63 \ 255
  OUT &H3C9, 0
  OUT &H3C9, 0
NEXT i%

FOR x% = 0 TO 319
  x0 = (x% - 200) / 79.5

  FOR y% = 0 TO 199
    y0 = (y% - 99.5) / (99.5 / 1.5)

    x = 0
    y = 0

    FOR iteration% = 0 TO MAXITERATION%
      xtemp = x * x - y * y + x0
      y = 2 * x * y + y0
      x = xtemp
      xxyy = x * x + y * y
      IF xxyy >= 4 THEN EXIT FOR
    NEXT iteration%

    IF iteration% <= MAXITERATION% THEN
      c% = iteration% * 16
      c% = c% AND 511
      IF c% >= 256 THEN c% = 512 - c%
      PSET (x%, y%), c%
    END IF
  NEXT y%
NEXT x%

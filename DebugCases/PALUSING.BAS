SCREEN 0
WIDTH 80, 25

DIM p%(63)

FOR i% = 0 TO 63
  p%(i%) = i%
NEXT i%

FOR i% = 0 TO 15
  COLOR i%
  PRINT CHR$(219); CHR$(219);
NEXT i%

SLEEP

PALETTE USING p%(10)

SLEEP

SCREEN 13

DIM p&(255)

c% = 0

FOR r% = 0 TO 5
  FOR g% = 0 TO 5
    FOR b% = 0 TO 5
      rr% = r% * 63 \ 5
      gg% = g% * 63 \ 5
      bb% = b% * 63 \ 5
      p&(c%) = rr% * 65536 + gg% * 256& + bb%
      c% = c% + 1
    NEXT b%
  NEXT g%
NEXT r%

FOR y% = 0 TO 5
  FOR x% = 0 TO 35
    xx% = x% * 6
    yy% = y% * 6
    LINE (xx%, yy%)-STEP(5, 5), x% + y% * 36, BF
  NEXT x%
NEXT y%

SLEEP

PALETTE USING p&

SLEEP

SCREEN 12

' This looks better with a subtler red, because it's used as a glow.
OUT &H3C8, 4 ' Reprogram VGA DAC entry 4
OUT &H3C9, 28 ' Red: 28 (out of 63) (default is 42)
OUT &H3C9, 0  ' Green
OUT &H3C9, 0  ' Blue

CONST Interval% = 9
CONST DotSize% = 3

CONST PI# = 3.14159265358979

DIM sieve%(640 \ Interval% + 1)

' The drawing is top-heavy, so offset it down a bit from the middle.
y% = 300

FOR i% = 2 TO UBOUND(sieve%)
  IF sieve%(i%) = 0 THEN
    ' We found a prime, mark all multiples of it as composite.
    FOR j% = i% + i% TO UBOUND(sieve%) STEP i%
      sieve%(j%) = 1
    NEXT j%

    ' Draw a dot for this prime on the number line.
    x% = (i% - 2) * Interval% + Interval% \ 2

    CIRCLE (x%, y%), DotSize% + 2, 5
    PAINT (x%, y%), 5, 5
    CIRCLE (x%, y%), DotSize% + 1, 13
    PAINT (x%, y%), 13, 13
    CIRCLE (x%, y%), DotSize%, 14
    PAINT (x%, y%), 14, 14

    ' Draw a curve that goes through every multiple of this prime.
    polarity% = 0 ' Alternate top and bottom curve segments.

    FOR j% = i% TO UBOUND(sieve%) STEP i%
      cx% = (2 * j% + i% - 3) * Interval% \ 2
      CIRCLE (cx%, y%), Interval% * i% \ 2, 14, PI# * polarity%, PI# * (polarity% + 1)
      polarity% = polarity% XOR 1
    NEXT j%

    ' Hollow out the prime's dot
    CIRCLE (x%, y%), 1, 0
    PAINT (x%, y%), 0, 0
  END IF
NEXT i%

' Waste some CPU cycles for aesthetic appearance: Give all
' yellow pixels a red glow. :-)
FOR i% = 0 TO 639
  FOR j% = 0 TO 479
    IF POINT(i%, j%) = 14 THEN
      FOR k% = -1 TO 1
        FOR l% = -1 TO 1
          IF k% OR l% THEN
            IF POINT(i% + k%, j% + l%) = 0 THEN PSET (i% + k%, j% + l%), 4
          END IF
        NEXT l%
      NEXT k%
    END IF
  NEXT j%
NEXT i%

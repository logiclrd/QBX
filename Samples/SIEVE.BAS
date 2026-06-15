SCREEN 12

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

    CIRCLE (x%, y%), DotSize%, 14
    PAINT (x%, y%), 14, 14

    ' Draw a curve that goes through every multiple of this prime.
    polarity% = 0 ' Alternate top and bottom curve segments.

    FOR j% = i% TO UBOUND(sieve%) STEP i%
      x% = (2 * j% + i% - 3) * Interval% \ 2
      CIRCLE (x%, y%), Interval% * i% \ 2, 14, PI# * polarity%, PI# * (polarity% + 1)
      polarity% = polarity% XOR 1
    NEXT j%
  END IF
NEXT i%

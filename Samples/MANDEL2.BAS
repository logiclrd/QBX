' Mandelbrot fractal variant. In this presentation, the Mandelbrot set
' is sliced down the middle and then the two halves are swapped.
'
' Inspired by a ZX Spectrum demo that drew the same structure.

' Set up video mode and palette
SCREEN 13

OUT &H3C8, 1

FOR i% = 63 TO 0 STEP -1
  n# = i% / 63
  n# = n# ^ 12
  j% = n# * 63
  OUT &H3C9, j%
  OUT &H3C9, j%
  OUT &H3C9, j%
NEXT i%

OUT &H3C9, 4
OUT &H3C9, 6
OUT &H3C9, 20

' Set viewing window
du# = 2.25 / 320
dv# = 2.3 / 200

u1# = -1.6
v1# = dv#

' Render
u# = u1#

FOR x% = 0 TO 319
  v# = v1#

  FOR y% = 0 TO 99
    uu# = 0
    vv# = 0

    FOR k% = 0 TO 63
      u2# = uu# * uu#
      v2# = vv# * vv#

      q# = u2# - v2# + u#
      w# = 2 * uu# * vv# + v#

      uu# = q#
      vv# = w#

      IF u2# + v2# > 4 THEN EXIT FOR
    NEXT k%

    COLOR k% + 1

    PSET (x%, y%)
    PSET (x%, 199 - y%)

    v# = v# + dv#
  NEXT y%

  LET u# = u# + du#
NEXT x%

SLEEP

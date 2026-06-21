CONST PI# = 3.14159265358979

CONST RR# = 2 * PI# / 235

ESC$ = CHR$(27)

SCREEN 13

FOR a% = 0 TO 63
  h% = a% * 384 \ 64
  o% = h% AND 63

  SELECT CASE h%
    CASE 0 TO 63: r% = o%: g% = 0: b% = 63
    CASE 64 TO 127: r% = 63: g% = 0: b% = 63 - o%
    CASE 128 TO 191: r% = 63: g% = o%: b% = 0
    CASE 192 TO 255: r% = 63 - o%: g% = 63: b% = 0
    CASE 256 TO 319: r% = 0: g% = 63: b% = o%
    CASE 320 TO 383: r% = 0: g% = 63 - o%: b% = 63
  END SELECT

  FOR i% = 0 TO 3
    OUT &H3C9, r% * i% \ 3
    OUT &H3C9, g% * i% \ 3
    OUT &H3C9, b% * i% \ 3
  NEXT i%
NEXT a%

DIM buf&(16001)

GET (0, 0)-(319, 199), buf&

DEF SEG = VARSEG(buf&(0))
bufofs% = VARPTR(buf&(0))

w# = 320: h# = 200

xc# = w# / 2: yc# = h# / 2
xs# = w# / 4: ys# = h# / 4

IF xs# < ys# THEN ys# = xs# ELSE xs# = ys#

v# = 0
x# = 0

t# = RND * 10

rotatec% = -1
clearbuffer% = -1

DO
  IF clearbuffer% THEN
    FOR i% = 1 TO 16000
      buf&(i%) = 0
    NEXT i%
  END IF

  t# = t# + .035

  IF rotatec% THEN c% = 0

  FOR i% = 60 TO 255 STEP 4
    b# = RR# * i% + t#

    FOR j% = 0 TO 65
      iv# = i% + v#

      u# = SIN(iv#) + SIN(x#)
      v# = COS(iv#) + COS(x#)

      x# = u# + b#

      x% = u# * xs# + xc#
      y% = v# * ys# + yc#

      o& = y% * 320& + x% + bufofs%

      p% = PEEK(o&) AND 3

      IF p% < 3 THEN POKE o&, c% + p% + 1
    NEXT j%

    IF rotatec% THEN c% = c% + 4
  NEXT i%

  PUT (0, 0), buf&, PSET

  k$ = INKEY$

  SELECT CASE k$
    CASE "r":
      rotatec% = NOT rotatec%
      c% = INT(RND * 64) * 4
    CASE "c":
      clearbuffer% = NOT clearbuffer%
  END SELECT
LOOP UNTIL k$ = ESC$

SUB BubbleRow (iang#, b#, v#, x#, hp#, xs#, ys#, xc#, yc#, c%(), d%())
FOR j% = 0 TO 65
  iv# = iang# + v#

  u# = SIN(iv#) + SIN(x#)
  v# = COS(iv#) + COS(x#)

  x# = u# + b#

  c%(j%) = u# * xs# + xc#
  d%(j%) = v# * ys# + yc#
NEXT j%
END SUB

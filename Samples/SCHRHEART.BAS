' Based on the Julia set rendering created by Eric Schraf

DECLARE SUB CreatePalette (bc%, n%)
DECLARE SUB HSV2RGB (h#, s#, v#, r%, g%, b%)
DECLARE SUB SelectJuliaPoint (cr#, ci#)

SCREEN 12

basecolour% = 0

CreatePalette basecolour%, 8

w% = 640
h% = 480

cx# = (w% - 1) / 2
cy# = (h% - 1) / 2

zoom# = 2 / w%

dmin# = .006

DIM invpixels%(65)

LINE (0, 0)-(7, 15), 7, BF
GET (0, 0)-(7, 15), invpixels%

DO
  SelectJuliaPoint cr#, ci#

  FOR a% = 0 TO w% \ 2 - 1
    FOR b% = 0 TO h% - 1
      zr# = (a% - cx#) * zoom#
      zi# = (b% - cy#) * zoom#

      i% = 0
      d# = 100

      DO WHILE d# >= dmin# AND i% <= 30
        u# = zr# * zr#
        v# = zi# * zi#

        IF u# + v# > 4.8 THEN EXIT DO

        zi# = 2 * zr# * zi# + ci#
        zr# = u# - v# + cr#

        i% = i% + 1

        ' Quadrants:
        '
        '   .-.|.-.
        '  :   |   |
        '--|---+---|--
        '  `.  |  .'
        '    `.|.'
        '      |

        ' Symmetrical; only have to process one half.
        IF zr# < 0 THEN zzr# = -zr# ELSE zzr# = zr#

        IF zi# > 0 THEN
          ' Upper half: half-circle
          zzr# = zzr# - .05
          zzi# = zi#

          n# = ABS(SQR(zzr# * zzr# + zzi# * zzi#) - .05)
        ELSE
          ' Bottom half: cosine wave
          ' => goes from 1 to -1 with the desired shape as the
          '    input goes from 0 to pi (or -pi)
          ' => actual correct distance to cosine wave is
          '    expensive and complicated to estimate, so we
          '    use a very poor approximation here of just
          '    testing one axis :-)

          zzi# = -zi#

          angle# = zzi# * 3.14159265358979# / .15
          targetoffset# = (COS(angle#) * .5 + .5) * .1

          n# = ABS(zzr# - targetoffset#)

          IF zzi# > .15 THEN n# = n# + zzi# - .15
        END IF

        IF n# < d# THEN d# = n#
      LOOP

      IF d# >= dmin# THEN
        coul% = basecolour%
      ELSE
        coul% = 7 - INT(1250 * d#)
        IF coul% < 1 THEN coul% = 1
        IF coul% > 7 THEN coul% = 7

        coul% = coul% + basecolour%
      END IF

      PSET (a%, b%), coul%
      PSET (w% - a% - 1, h% - b% - 1), coul%
    NEXT b%
  NEXT a%

  COLOR basecolour% + 7
  LOCATE 30, 1
  PRINT USING "R: ##.############    I:##.############    "; cr#; ci#;

  IF basecolour% THEN
    ' PRINT has used colour 0 for the background, but the black
    ' for the current figure is colour basecolour%
    FOR y% = 464 TO 479
      FOR x% = 0 TO 639
        IF POINT(x%, y%) = 0 THEN PSET (x%, y%), basecolour%
      NEXT x%
    NEXT y%
  END IF

  st# = TIMER
  FOR ticks% = 1 TO 80
    WHILE st# = TIMER: WEND
    st# = TIMER

    IF INKEY$ <> "" THEN EXIT DO

    PUT (ticks% * 8 - 8, 464), invpixels%, XOR
  NEXT ticks%

  basecolour% = basecolour% XOR 8

  CreatePalette basecolour%, 8
LOOP

SUB CreatePalette (bc%, n%)
OUT &H3C8, bc%

mc% = n% - 1

IF RND < .5 THEN
  ' From black
  hue# = RND * 360
  saturation# = RND * .25 + .75
  value# = 1

  HSV2RGB hue#, saturation#, value#, r%, g%, b%

  FOR i% = 0 TO mc%
    OUT &H3C9, r% * i% \ mc%
    OUT &H3C9, g% * i% \ mc%
    OUT &H3C9, b% * i% \ mc%
  NEXT i%
ELSE
  ' To white
  hue# = RND * 360
  saturation# = RND * .1 + .9
  value# = RND * .3 + .4

  HSV2RGB hue#, saturation#, value#, r%, g%, b%

  dr% = 63 - r%
  dg% = 63 - g%
  db% = 63 - b%

  ' We still need a black
  OUT &H3C9, 0
  OUT &H3C9, 0
  OUT &H3C9, 0

  mc% = mc% - 1

  FOR i% = 0 TO mc%
    OUT &H3C9, r% + dr% * i% \ mc%
    OUT &H3C9, g% + dg% * i% \ mc%
    OUT &H3C9, b% + db% * i% \ mc%
  NEXT i%
END IF
END SUB

SUB HSV2RGB (h#, s#, v#, r%, g%, b%)
' Input: h# in 0..360, s# and v# in 0..1
' Output: r%, g%, b% in 0..63

' Just in case
WHILE h# < 0: h# = h# + 360: WEND
WHILE h# >= 360: h# = h# - 360: WEND

chroma# = s# * v#

x# = h# / 60#

WHILE x# >= 2
  x# = x# - 2
WEND

x# = chroma# * (1 - ABS(x# - 1))

SELECT CASE h#
  CASE 0 TO 60: r# = chroma#: g# = x#: b# = 0
  CASE 60 TO 120: r# = x#: g# = chroma#: b# = 0
  CASE 120 TO 180: r# = 0: g# = chroma#: b# = x#
  CASE 180 TO 240: r# = 0: g# = x#: b# = chroma#
  CASE 240 TO 300: r# = x#: g# = 0: b# = chroma#
  CASE 300 TO 360: r# = chroma#: g# = 0: b# = x#
END SELECT

match# = v# - chroma#

r% = (r# + match#) * 64
g% = (g# + match#) * 64
b% = (b# + match#) * 64

IF r% > 63 THEN r% = 63
IF g% > 63 THEN g% = 63
IF b% > 63 THEN b% = 63
END SUB

SUB SelectJuliaPoint (cr#, ci#)
CONST MaxIters% = 300

DO
  cr# = RND * 3.5 - 2.5
  ci# = RND * 2 - 1

  zr# = 0
  zi# = 0

  FOR i% = 1 TO MaxIters%
    newzr# = zr# * zr# - zi# * zi# + cr#
    newzi# = 2 * zr# * zi# + ci#

    zr# = newzr#
    zi# = newzi#

    IF zr# * zr# + zi# * zi# > 4 THEN EXIT FOR
  NEXT i%
LOOP UNTIL i% >= 150 AND i% <= MaxIters%

' cr + i ci is now a promising Julia parameter c!
END SUB

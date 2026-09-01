DECLARE SUB SelectJuliaPoint (cr#, ci#)
SCREEN 12

OUT &H3C8, 0

FOR i% = 0 TO 15
  OUT &H3C9, i% * 63 \ 15
  OUT &H3C9, i% * 63 \ 15
  OUT &H3C9, 0
NEXT i%

w% = 640
h% = 480

cx# = (w% - 1) / 2
cy# = (h% - 1) / 2

zoom# = 2 / w%

dmin# = .05

DIM invpixels%(65)

LINE (0, 0)-(7, 15), 15, BF
GET (0, 0)-(7, 15), invpixels%

DO
  SelectJuliaPoint cr#, ci#

  LINE (0, 0)-(639, 479), 8, BF

  FOR a% = 0 TO w% \ 2 - 1
    FOR b% = 0 TO h% - 1
      zr# = (a% - cx#) * zoom#
      zi# = (b% - cy#) * zoom#

      i% = 0
      d# = 100

      DO
        u# = zr# * zr#
        v# = zi# * zi#

        IF u# + v# > 4.8 OR i% > 30 OR d# < dmin# THEN EXIT DO

        zi# = 2 * x# * y# + ci#
        zr# = u# - v# - cr#

        i% = i% + 1

        n# = ABS(u# + v# - 1)

        IF n# < d# THEN d# = n#
      LOOP

      IF d# >= dmin# THEN
        coul% = 0
      ELSE
        coul% = 15 - INT(250 * d#)
        IF coul% < 0 THEN coul% = 0
        IF coul% > 15 THEN coul% = 15
      END IF

      PSET (a%, b%), coul%
      PSET (w% - a% - 1, h% - b% - 1), coul%
    NEXT b%
  NEXT a%

  LOCATE 30, 1
  PRINT USING "R: ##.############    I:##.############"; cr#; ci#;

  st# = TIMER
  FOR ticks% = 1 TO 80
    WHILE st# = TIMER: WEND
    st# = TIMER

    IF INKEY$ <> "" THEN EXIT DO

    PUT (ticks% * 8 - 8, 464), invpixels%, XOR
  NEXT ticks%
LOOP

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
LOOP UNTIL i% > 20 AND i% <= MaxIters%

' cr + i ci is now a promising Julia parameter c!
END SUB

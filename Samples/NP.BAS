' Non-periodic  K. Moerman 2026
' QuickBASIC translation by Jonathan Gilbert

DECLARE FUNCTION m# (x#)
DECLARE FUNCTION f# (x#)

CONST PI# = 3.14159265358979#

CONST Fade% = 15

SCREEN 13

DEF SEG = &HA000

offset# = 1
dt# = .03

cs& = 0
ce& = 251 * (Fade% + 1) - 1

DO
  pb% = (pb% MOD 255) + 1

  FOR i% = 0 TO Fade%
    bt# = i% / (Fade% - 1)
    IF bt# >= 1 THEN b% = 63 ELSE b% = (bt# ^ 1.5) * 63
    OUT &H3C8, ((pb% + i% - 1) MOD 255) + 1
    OUT &H3C9, b%
    OUT &H3C9, b%
    OUT &H3C9, b%
  NEXT i%

  ipb% = ((pb% + Fade% - 1) MOD 255 + 1)

  IF pb% < ipb% THEN
    IF cs& < ce& THEN
      FOR i& = cs& TO ce&
        c% = PEEK(i&)
        IF c% <= pb% OR c% > ipb% THEN POKE i&, 0
      NEXT i&
    ELSE
      FOR i& = cs& TO 63999
        c% = PEEK(i&)
        IF c% <= pb% OR c% > ipb% THEN POKE i&, 0
      NEXT i&

      FOR i& = 0 TO ce&
        c% = PEEK(i&)
        IF c% <= pb% OR c% > ipb% THEN POKE i&, 0
      NEXT i&
    END IF
  ELSE
    IF cs& < ce& THEN
      FOR i& = cs& TO ce&
        c% = PEEK(i&)
        IF c% <= pb% AND c% > ipb% THEN POKE i&, 0
      NEXT i&
    ELSE
      FOR i& = cs& TO 63999
        c% = PEEK(i&)
        IF c% <= pb% AND c% > ipb% THEN POKE i&, 0
      NEXT i&

      FOR i& = 0 TO ce&
        c% = PEEK(i&)
        IF c% <= pb% AND c% > ipb% THEN POKE i&, 0
      NEXT i&
    END IF
  END IF

  cs& = cs& + 251
  ce& = ce& + 251

  IF cs& > 63999 THEN cs& = cs& - 64000
  IF ce& > 63999 THEN ce& = ce& - 64000

  t2# = offset#
  FOR t1# = 0 TO 75 STEP dt#
    x% = 160 + 45 * f#(t2#)
    y% = 100 - 45 * f#(t1#)

    POKE y% * 320& + x%, ipb%

    t2# = t2# + dt#
  NEXT t1#

  offset# = offset# + .0015#
LOOP WHILE INKEY$ = ""

FUNCTION f# (x#) ' Non-periodic function
f# = SIN(PI# * x#) + SIN(3.5 * x#)
END FUNCTION

FUNCTION m# (x#) ' Mapping value to pixel coord.
m# = 100 + 45 * x#
END FUNCTION

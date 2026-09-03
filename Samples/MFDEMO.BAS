' Modular flow demo created to demonstrate specifically how, with this particular lattice
' configuration, there is a point in time t where the deformation has moved every lattice
' point to exactly the position of another lattice point, so looping back to t == 0 is not
' discontinuous.

SCREEN 7, , 0, 1

t# = 0

phi# = (1 + SQR(5)) / 2

t# = 0

CycleLength# = LOG(1 + phi#)

DO
  CLS

  f# = EXP(t#)

  FOR a% = -15 TO 15
    FOR b% = -15 TO 15
      x# = a% * phi# + b%
      y# = a% - b% * phi#

      x# = x# * f#
      y# = y# / f#

      x% = 160 + 4 * x#
      y% = 100 - 4 * y#

      IF (x% > 120) AND (x% < 200) AND (y% > 70) AND (y% < 130) THEN
        COLOR 7
      ELSE
        COLOR 8
      END IF

      PSET (160 + 4 * x#, 100 - 4 * y#)
    NEXT b%
  NEXT a%

  t# = t# + .01

  IF t# > CycleLength# THEN t# = t# - CycleLength#

  PCOPY 0, 1
LOOP WHILE INKEY$ = ""


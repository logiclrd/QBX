' Universo de color

CONST Nshapes% = 60
CONST Npoints% = 60

CONST Colour1Start% = Nshapes% * 0 \ 3
CONST Colour2Start% = Nshapes% * 1 \ 3
CONST Colour3Start% = Nshapes% * 2 \ 3

CONST DT! = .003
CONST PI! = 3.141593
CONST R! = 2 * PI! / Nshapes%

t! = 0

SCREEN 13

DIM buf&(16001)

GET (0, 0)-(319, 199), buf&

DEF SEG = VARSEG(buf&(1))
bufofs% = VARPTR(buf&(1))

DO
  ' Clear frame buffer
  FOR i% = 1 TO 16000
    buf&(i%) = 0
  NEXT i%

  ' Calculate frame parameters
  a! = COS(3 * t!)
  zoom! = ABS(a!) + 1.1
  scale! = 100! / zoom!

  FOR shape% = 0 TO Nshapes% - 1
    FOR pnt% = 0 TO Npoints% - 1
      a1! = shape% + y!

      x! = SIN(a1!) + a! * SIN(R! * shape% + t! + x!)
      y! = a! * COS(a1!) + COS(R! * shape% + t! + x!)

      px% = 160 + x! * scale!
      py% = 100 + y! * scale!

      SELECT CASE shape%
        CASE Colour1Start%: c% = 12
        CASE Colour2Start%: c% = 11
        CASE Colour3Start%: c% = 14
      END SELECT

      IF px% >= 0 AND px% < 320 THEN
        IF py% >= 0 AND py% < 200 THEN
          POKE py% * 320& + px% + bufofs%, c%
        END IF
      END IF
    NEXT pnt%
  NEXT shape%

  ' Blit frame buffer
  PUT (0, 0), buf&, PSET

  ' Advance to next frame
  t! = t! + DT!
LOOP WHILE INKEY$ = ""

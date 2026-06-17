DECLARE SUB DrawLSystem (t$, rules$(), x#, y#, d#, angle#, angledelta#, depth%, charcount&)

SCREEN 12

DIM rules$(255)

rules$(ASC("F")) = "F+F-F-FF+F+F-F"
initial$ = "F+F+F+F"

FOR i% = 1 TO 5
  CLS
  charcount& = 0
  DrawLSystem initial$, rules$(), 200, 380, (5.5 - i%) * .5, 0, 90, i%, charcount&
  LOCATE 1, 1
  PRINT "Virtual string length:"; charcount&
  SLEEP
NEXT i%

SUB DrawLSystem (t$, rules$(), x#, y#, d#, angle#, angledelta#, depth%, charcount&)
nextdepth% = depth% - 1

FOR i% = 1 TO LEN(t$)
  c$ = MID$(t$, i%, 1)

  SELECT CASE c$
    CASE "-": angle# = angle# - angledelta#
    CASE "+": angle# = angle# + angledelta#

    CASE ELSE:
      IF depth% > 0 THEN
        c% = ASC(c$)

        DrawLSystem rules$(c%), rules$(), x#, y#, d#, angle#, angledelta#, nextdepth%, charcount&
      ELSE
        nx# = x# + d# * COS(angle# / 57.295779)
        ny# = y# - d# * SIN(angle# / 57.295779)

        LINE (x#, y#)-(nx#, ny#)

        x# = nx#
        y# = ny#
      END IF
  END SELECT

  charcount& = charcount& + 1
NEXT i%
END SUB

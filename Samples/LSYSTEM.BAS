DECLARE SUB RunDemo (shape%)
DECLARE SUB DrawLSystem (t$, rules$(), x#, y#, d#, angle#, angledelta#, depth%, charcount&)

SCREEN 12

DO
  CLS
  PRINT "1. Square"
  PRINT "2. Tree"
  PRINT "3. Bush"
  PRINT "Select: ";
  LINE INPUT a$

  IF a$ = "" THEN EXIT DO

  RunDemo VAL(a$)

  ' Flush keyboard buffer in case user pressed keys while we were SLEEPing.
  WHILE INKEY$ <> "": WEND
LOOP

SUB DrawLSystem (t$, rules$(), x#, y#, d#, angle#, angledelta#, depth%, charcount&)
nextdepth% = depth% - 1

i% = 1

DO WHILE i% <= LEN(t$)
  c$ = MID$(t$, i%, 1)

  SELECT CASE c$
    CASE "-": angle# = angle# - angledelta#
    CASE "+": angle# = angle# + angledelta#

    CASE "[":
      bracketstart% = i% + 1
      bracketlevel% = 1
      i% = i% + 1
      DO WHILE i% <= LEN(t$)
        SELECT CASE MID$(t$, i%, 1)
          CASE "[":
            bracketlevel% = bracketlevel% + 1
          CASE "]":
            bracketlevel% = bracketlevel% - 1
            IF bracketlevel% = 0 THEN EXIT DO
        END SELECT

        i% = i% + 1
      LOOP

      IF i% - bracketstart% > 0 THEN
        scoped$ = MID$(t$, bracketstart%, i% - bracketstart%)
        ' Pass state in by value
        tx# = x#
        ty# = y#
        tangle# = angle#

        DrawLSystem scoped$, rules$(), tx#, ty#, d#, tangle#, angledelta#, depth%, charcount&
      END IF
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

  i% = i% + 1
LOOP
END SUB

SUB RunDemo (shape%)
DIM rules$(255)

SELECT CASE shape%
  CASE 1: ' Square
    rules$(ASC("F")) = "F+F-F-FF+F+F-F"
    initial$ = "F+F+F+F"
    initialangle# = 0
    angledelta# = 90
    basesize# = 60
    scalefactor# = .25
    levels% = 5
  CASE 2: ' Tree
    rules$(ASC("F")) = "FF"
    rules$(ASC("X")) = "F-[[X]+X]+F[+FX]-X"
    initial$ = "X"
    initialangle# = 90
    angledelta# = 22.5
    basesize# = 10
    scalefactor# = .67
    levels% = 7
  CASE 3: ' Bush
    rules$(ASC("F")) = "FF+[+F-F-F]-[-F+F+F]"
    initial$ = "F"
    initialangle# = 90
    angledelta# = 22.5
    basesize# = 20
    scalefactor# = .58
    levels% = 6
END SELECT

FOR i% = 1 TO levels%
  CLS
  charcount& = 0
  DrawLSystem initial$, rules$(), 200, 380, basesize# * EXP(LOG(scalefactor#) * (i% - 1)), initialangle# + 0, angledelta#, i%, charcount&
  LOCATE 1, 1
  PRINT "Virtual string length:"; charcount&
  SLEEP
NEXT i%
END SUB

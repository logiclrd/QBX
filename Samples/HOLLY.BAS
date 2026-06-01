DECLARE SUB Holly (x!, y!, c!)
DECLARE SUB ZXDraw (dx!, dy!, angle!)
DECLARE FUNCTION ATAN2! (y!, x!)

' *** XMAS HOLLY ***
' by E.R. Duncan-Dunlop
' Translated from Sinclair Spectrum by Jonathan Gilbert

DIM SHARED ZXDrawDebug%

SCREEN 12

COLOR 10

' Show how the sausage is made
ZXDrawDebug% = 1
Holly 200, 350, 200
ZXDrawDebug% = 0

SLEEP

CLS

DO
  c! = RND * 40 + 10

  x! = RND * (640 - c!)
  y! = RND * (480 - c!) + c!

  Holly x!, y!, c!

  st! = TIMER: WHILE st! = TIMER: WEND
LOOP WHILE INKEY$ = ""

FUNCTION ATAN2! (y!, x!)
CONST PI! = 3.141593

slope! = y! / x!

IF slope! = slope! THEN
  angle! = ATN(slope!)

  IF x! < 0 THEN angle! = angle! + PI!
  IF angle! < 0 THEN angle! = angle! + 2 * PI!

  ATAN2! = angle!
ELSEIF y! > 0 THEN
  ATAN2! = PI! * .5
ELSEIF y! < 0 THEN
  ATAN2! = -PI! * .5
ELSE
  ATAN2! = slope!
END IF
END FUNCTION

SUB Holly (x!, y!, c!)
PSET (x!, y!)

ZXDraw c!, c!, 1
ZXDraw -c! / 3, -c! / 5, -2
ZXDraw -c! / 2.5, -c! / 5, -2
ZXDraw -c! / 5, -c! / 3.4, -2
ZXDraw -c! / 10, -c! / 3, -2
ZXDraw c! / 2.5, -c! / 10, -2
ZXDraw c! / 3.4, c! / 5, -2
ZXDraw c! / 3.4, c! / 3.4, -2
ZXDraw 0, c! / 2, -1
END SUB

SUB ZXDraw (dx!, dy!, angle!)
' ZX Spectrum's Y axis points up, QuickBASIC's points down

curx! = POINT(2)
cury! = POINT(3)

nextx! = curx! + dx!
nexty! = cury! - dy!

IF ABS(angle!) < .001 THEN
  LINE (curx!, cury!)-(nextx!, nexty!)
ELSE
  chordlength! = SQR(dx! ^ 2 + dy! ^ 2)
  radius! = chordlength! / (2 * SIN(ABS(angle!) / 2))

  chordmidx! = curx! + dx! * .5
  chordmidy! = cury! - dy! * .5

  chordtriangleheight! = SQR(radius! ^ 2 - (chordlength! / 2) ^ 2)

  chordnormalx! = dy! / chordlength!
  chordnormaly! = dx! / chordlength!

  IF angle! > 0 THEN chordtriangleheight! = -chordtriangleheight!

  circlecenterx! = chordmidx! + chordnormalx! * chordtriangleheight!
  circlecentery! = chordmidy! + chordnormaly! * chordtriangleheight!

  startangle! = ATAN2(circlecentery! - cury!, curx! - circlecenterx!)
  endangle! = ATAN2(circlecentery! - nexty!, nextx! - circlecenterx!)

  IF angle! < 0 THEN SWAP startangle!, endangle!

  IF ZXDrawDebug% THEN
    CIRCLE (circlecenterx!, circlecentery!), radius!, 1, , , 1
  END IF

  CIRCLE (circlecenterx!, circlecentery!), radius!, , startangle!, endangle!, 1

  IF ZXDrawDebug% THEN
    CIRCLE (nextx!, nexty!), 2, 12
    CIRCLE (nextx!, nexty!), 1, 12
  END IF
END IF

' Update last point
PSET (nextx!, nexty!)
END SUB

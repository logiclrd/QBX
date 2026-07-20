DECLARE FUNCTION QCOS! (xx!)
DECLARE FUNCTION QSIN! (xx!)

CONST PI! = 3.141593

SCREEN 12

COLOR 8

CONST SCALE! = 60!

ticks% = 320 \ SCALE! + 1

FOR i% = -ticks% TO ticks%
  x% = i% * SCALE!

  LINE (320 + x%, 230)-STEP(0, 20)
  LINE (310, 240 + x%)-STEP(20, 0)
NEXT i%

COLOR 7

LINE (0, 240)-(639, 240)
LINE (320, 0)-(320, 479)

COLOR 14

FOR x% = 0 TO 639
  xx! = (x% - 320) / SCALE!
  y% = 240 - SIN(xx!) * SCALE! - 5
  IF x% = 0 THEN PSET (x%, y%) ELSE LINE -(x%, y%)
NEXT x%

COLOR 15

FOR x% = 0 TO 639
  xx! = (x% - 320) / SCALE!
  y% = 240 - QSIN(xx!) * SCALE! + 5
  IF x% = 0 THEN PSET (x%, y%) ELSE LINE -(x%, y%)
NEXT x%


FUNCTION QCOS (xx!)
QCOS! = QSIN!(x + 90)
END FUNCTION

FUNCTION QSIN! (xx!) ' Quick trigonometry estimation
x! = xx! ' Decouple

WHILE x! > 2 * PI!
  x! = x! - 2 * PI!
WEND

WHILE x! < 0
  x! = x! + 2 * PI!
WEND

IF x! > PI! THEN x! = PI! - x!

' If we want the input to be in degrees rather than radians:
' x! = x! * PI! / 180!

x! = 1.2732 * x! - .4053 * x! * ABS(x!)
x! = .225 * (x! * ABS(x!) - x!) + x!

QSIN! = x!
END FUNCTION

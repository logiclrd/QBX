DECLARE SUB SolidCircle (x, y, r)
DECLARE SUB Lines (x, y, s, angle, b%)

'Based on fracTRee micro(A) sample
SCREEN 12

PAINT (0, 0), 15

CONST PI = 3.14159

DO
  x = 300
  y = 600
  s = 290
  angle = 270

  COLOR FIX(RND * 15)

  FOR b% = 1 TO 5
    Lines x, y, s, angle, b%
    s = s * .5
    angle = angle + INT(RND * 5) * 45 - 90
  NEXT b%

  st = TIMER: WHILE st = TIMER: WEND
LOOP WHILE INKEY$ = ""

SUB Lines (x, y, s, angle, b%)
r = (angle / 180) * PI
y2 = y + SIN(r) * s
x2 = x - COS(r) * s
radius! = 6 / b% + 2
FOR n = 1 TO 30
  nx = (x2 * n + x * (30 - n)) / 30
  ny = (y2 * n + y * (30 - n)) / 30
  SolidCircle nx, ny, radius!
NEXT n

x = x2
y = y2
END SUB

SUB SolidCircle (x, y, r)
FOR yy = -r TO r
  xx = SQR(r * r - yy * yy)
  LINE (x - xx, y + yy)-(x + xx, y + yy)
NEXT yy
END SUB

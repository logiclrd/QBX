SCREEN 7

w2 = 2

' Draw ring of 5 circles with a 6th circle in the middle
cx = 128
cy = 88
r = 30
rc = 20

FOR i = 0 TO 4
  AA = i * (6.283185 / 5)
  px = cx + r * COS(AA)
  py = cy + r * SIN(AA)
  CIRCLE (px, py), r, , , , 1
NEXT i

CIRCLE (cx, cy), rc, , , , 1

' Begin painting in the first circle (angle 0, to the right of the origin)
DEF FNM (a, b) = a - b * INT(a / b)

COLOR 4

px = cx + r
py = cy

PAINT (px, py), CHR$(0) + CHR$(0) + CHR$(&HAA) + CHR$(0) + CHR$(0) + CHR$(0) + CHR$(&H55), 15

END

FOR y = py - r TO py + r
  h = r ^ 2 - (y - py) ^ 2
  IF h >= 0 THEN
    w = INT(SQR(h))
    FOR x = px - w TO px + w
      dd = (x - cx) ^ 2 + (y - cy) ^ 2
      IF dd >= rc ^ 2 THEN
        IF FNM(x, w2) = FNM(y, w2) THEN
          PSET (x, y)
        END IF
      END IF
    NEXT x
  END IF
NEXT y

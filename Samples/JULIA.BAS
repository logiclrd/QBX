' Based on julia.bas sample from micro(A)

CONST PI = 3.14159265

SCREEN 12

COLOR 9

rc = 2
DO
  WAIT &H3DA, 1
  CLS

  c1 = SIN(rc * 11)
  c2 = SIN(rc * PI * 2)

  FOR i% = 1 TO 600
    w1 = y - c2
    w0 = x - c1

    IF w0 = 0 THEN
      theta = PI / 1.41
    END IF

    IF w0 > 0 THEN
      theta = ATN(w1 / w0)
    END IF

    IF w0 < 0 THEN
      theta = PI + ATN(w1 / w0) + .1
    END IF

    r = SQR(w0 * w0 + w1 * w1)
    theta = (theta / 2) + INT(RND * 3) * PI
    r = SQR(r)
    x = r * COS(theta)
    y = r * SIN(theta)

    IF i < 220 THEN
      c2 = c2 - .0005
      i = i + 1
    END IF

    m = 80 + (x + 2) * 480 / 4
    n = (2 - y) * 480 / 4

    PSET (m, n)
  NEXT

  rc = rc + .001
LOOP WHILE INKEY$ = ""

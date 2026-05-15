DECLARE SUB BlackFilledCircle (x, y, r, c%)
'full_circle by [B+=MGA]2017->QuickBASIC by logic
'everything was supposed to be turning around another circle

CONST PI = 3.141592

SCREEN 12

xc = 640 / 2
yc = 480 / 2

DO
  r = r + PI / 180
  w = w + PI / 72

  FOR i = 1 TO 11
    x = xc + 200 * COS(w + i * PI / 6) * SIN(r)
    y = yc + 200 * SIN(w + i * PI / 6) * SIN(r)

    BlackFilledCircle x, y, 40, 10

    ww = ww + PI / 80

    IF i = 1 THEN
      FOR j = 1 TO 11
        xx = x + 100 * COS(ww + j * PI / 6) * SIN(r)
        yy = y + 100 * SIN(ww + j * PI / 6) * SIN(r)

        BlackFilledCircle xx, yy, 20, 12

        w3 = w3 + PI / 80

        IF j = 1 THEN
          FOR k = 1 TO 11
            x3 = xx + 50 * COS(w3 + k * PI / 6) * SIN(r)
            y3 = yy + 50 * SIN(w3 + k * PI / 6) * SIN(r)

            BlackFilledCircle x3, y3, r, 9
          NEXT k
        END IF
      NEXT j
    END IF
  NEXT i
LOOP WHILE INKEY$ = ""

SUB BlackFilledCircle (x, y, r, c%)
FOR yy = -r TO r
  xx = SQR(r * r - yy * yy)
  LINE (x - xx, y + yy)-(x + xx, y + yy), 0
NEXT yy

CIRCLE (x, y), r, c%
END SUB

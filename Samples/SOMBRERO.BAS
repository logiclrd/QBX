SCREEN 12

OX = 22
OY = -22

DOX = -.5
DOY = 0

DO
  OX = OX + DOX
  OY = OY + DOY

  X = OX
  Y = OY

  DX = .5
  DY = 0

  FIRST = -1

  DO
    D = SQR(X * X + Y * Y)
    Z = 50

    IF D > 0 THEN Z = Z * SIN(D) / D

    SX = INT(320 + (X + Y) * 7)
    SY = INT(240 - (X - Y) * 3 - Z * 4)

    IF FIRST THEN
      FIRST = 0
    ELSE
      LINE (PX, PY)-(SX, SY), 3

      FOR XX = PX + 1 TO SX
        YY = (XX - PX) * (SY - PY) / (SX - PX) + PY
        LINE (XX, YY)-(XX, 479), 0
      NEXT XX

      LINE (PX, PY)-(SX, SY), 2
    END IF

    PX = SX
    PY = SY

    X = X + DX
    Y = Y + DY

    SWAP DX, DY
  LOOP UNTIL X > 22 OR Y > 22

  IF OX = -22 THEN DOX = 0: DOY = .5
LOOP UNTIL OY = 22

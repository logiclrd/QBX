SCREEN 12

WINDOW (-2, -1.2)-(2, 1.2)

PI2 = 6.283185
CAMDIST = 6

' Presentation angle: 30 degrees
A3 = -30 * PI2 / 360
C3 = COS(A3): S3 = SIN(A3)

' Presentation offset: move up a bit
OY = .75

DO
  A1 = A1 + .1
  IF A1 >= PI2 THEN A1 = A1 - PI2
  A2 = A2 + (COS(A1) + 1.2) * .02
  IF A2 >= PI2 THEN A2 = A2 - PI2

  ' Generate point on surface of tube at angle 0
  X1 = COS(A1) + 3: Y = SIN(A1)

  ' Rotate (top-down) to correct position in tube. Y is unchanged.
  C2 = COS(A2): S2 = SIN(A2)

  X = X1 * C2
  Z = X1 * S2

  ' Rotate (from side) for presentation. X is unchanged.
  YY = Y * C3 - Z * S3
  ZZ = Y * S3 + Z * C3

  ' Vertical offset.
  YY = YY + OY

  ' Project
  SZ = ZZ + CAMDIST

  SX = X / SZ: SY = YY / SZ

  PSET (SX, SY)
LOOP WHILE INKEY$ = ""

SCREEN 13
PAINT (0, 0), 4

M = 313
P = 50
J = 1

FOR I = 0 TO 1.57 STEP .00055
  J = -J

  X = P * (J * I + 3)
  Y = 25 + P * (2 + (I ^ .01 * ABS(COS(I)) ^ .5 * COS(M * I)) - I ^ .3)

  PSET (X, Y), 15
NEXT I

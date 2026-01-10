SCREEN 12
COLOR 2
DEFSNG X-Y
DEFLNG I, N
c% = 1
FOR j! = .65 TO .95 STEP .01
  COLOR c%
  c% = (c% MOD 15) + 1
  FOR i = 1 TO 100000
    n = RND * 100
    IF n < 1 THEN
      Xp = 0
      Yp = .16 * Y
    ELSEIF n >= 1 AND n <= 8 THEN
      Xp = .2 * X - .26 * Y
      Yp = .23 * X + .22 * Y + 1.6
    ELSEIF n >= 8 AND n <= 15 THEN
      Xp = -.15 * X + .28 * Y
      Yp = .26 * X + .24 * Y + .44
    ELSE
      Xp = j! * X + .04 * Y
      Yp = -.04 * X + .85 * Y + 1.6
    END IF

    X = Xp
    Y = Yp
    xx = X * 45
    yy = Y * 45 - 225
    PSET (xx + 320, -yy + 240)
  NEXT i
NEXT j!

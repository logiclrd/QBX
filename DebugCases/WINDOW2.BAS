SCREEN 13

DEF FNRNDR% (l%, h%) = INT(RND * (h% - l%)) + l%

FOR j% = 0 TO 50 STEP 10
  CLS

  ' Comment these out in different combinations
  WINDOW (80, 50)-(240, 150)
  VIEW (j% * 8 \ 5, j%)-(160 + j% * 8 \ 5, 100 + j%)
  WINDOW

  FOR i% = 1 TO 1000
    PSET (FNRNDR%(80, 240), FNRNDR%(50, 150))
  NEXT i%

  PSET (120, 75)

  FOR i% = 1 TO 50
    PSET (120 + i%, 85)
    PSET (130, 75 + i%)
  NEXT i%

  SLEEP
NEXT j%

SCREEN 13

x% = 320
dx% = 5

y% = 100
dy% = 20

DO
  x% = x% + dx%

  IF (x% = 220) OR (x% = 420) THEN dx% = -dx%

  y% = y% + dy%

  IF (y% = 100) OR (y% = 1000) THEN dy% = -dy%

  WINDOW SCREEN (0, 0)-(x%, y%)

  CLS

  LINE (0, 0)-(319, 199), 1, BF

  LINE (10, 10)-(50, 50), , B

  LINE (70, 20)-(20, 70)
  LINE (70, 20)-(120, 70)
  LINE (70, 120)-(20, 70)
  LINE (70, 120)-(120, 70)

  CIRCLE (120, 50), 25

  st# = TIMER: WHILE st# = TIMER: WEND
LOOP WHILE INKEY$ = ""


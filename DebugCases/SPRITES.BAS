SCREEN 13

FOR i% = 1 TO 50
  CIRCLE (RND * 320, RND * 200), RND * 50 + 10, RND * 200
NEXT i%

DIM a%(10000)

FOR i% = 1 TO 20
  w% = RND * 30 + 10
  h% = RND * 30 + 10

  x% = RND * (320 - w%)
  y% = RND * (200 - h%)

  GET (x%, y%)-(x% + w% - 1, y% + h% - 1), a%

  x% = RND * (320 - w%)
  y% = RND * (200 - h%)

  PUT (x%, y%), a%

  FOR j% = 1 TO 5
    st! = TIMER
    WHILE TIMER = st!: WEND
  NEXT j%
NEXT i%

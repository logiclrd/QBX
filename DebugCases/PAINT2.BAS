SCREEN 13

DO
  LINE (0, 0)-(319, 199), 7, BF

  FOR i% = 1 TO 50
    x% = RND * 320
    y% = RND * 200

    w% = RND * 160
    h% = RND * 100

    IF RND < .5 THEN
      w% = w% \ 15
    ELSE
      h% = h% \ 15
    END IF

    LINE (x% - w%, y% - h%)-(x% + w%, y% + h%), 0, BF
  NEXT i%

  IF RND < .5 THEN LINE (0, 0)-(0, 199), 7

  LINE (150, 90)-(170, 110), 0, BF

  SLEEP

  PAINT (160, 100), 4, 7

  SLEEP
LOOP WHILE INKEY$ = ""

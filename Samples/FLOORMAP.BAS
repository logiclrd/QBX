'Based on Floormaper by Antoni Gual (micro(A) version)
'for Rel's 9 LINER contest at QBASICNEWS.COM 1/2003
'---------------------------------------------------
SCREEN 12

FOR i% = 0 TO 15
  OUT &H3C8, i%
  OUT &H3C9, i% * 63 \ 15
  OUT &H3C9, i% * 63 \ 15
  OUT &H3C9, i% * 63 \ 15
NEXT i%

DO
  px! = RND * 1000
  py! = RND * 1000
  rx! = RND * 5 + .1
  ry! = RND * 5 + .1
  FOR y% = 0 TO 479
    y2! = y% * ry! + py!
    FOR x% = 0 TO 639
      x2! = x% * rx! + px!
      rc& = x2! * y2!
      rc& = (rc& AND 255) * 15 \ 255
      IF rc& < 0 THEN rc& = 0
      IF rc& > 15 THEN rc& = 15
      PSET (x%, y%), rc&
    NEXT x%
  NEXT y%
LOOP WHILE INKEY$ = ""


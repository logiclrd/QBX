SCREEN 13

DEF SEG = &HA000

FOR i% = 1 TO 5000
  x1% = FIX(RND * 320)
  y1% = FIX(RND * 200)
  x2% = FIX(RND * 320)
  y2% = FIX(RND * 200)
  IF x1% = x2% THEN x2% = x2% XOR 1
  IF y1% = y2% THEN y2% = y2% XOR 1

  IF x1% < x2% THEN SWAP x1%, x2%
  IF y1% < y2% THEN SWAP y1%, y2%

  CLS
  VIEW (x1%, y1%)-(x2%, y2%), 1, 2
  DRAW "ta" + STR$(INT(RND * 360)) + " u5"
  f% = 0
  FOR i& = 0 TO 63999
    IF PEEK(i&) = 2 THEN f% = 1: EXIT FOR
  NEXT i&
  LOCATE 23, 1
  PRINT "                         "
  PRINT "                         "
  LOCATE 23, 1
  PRINT x1%, y1%
  PRINT x2%, y2%
  IF f% = 0 THEN SLEEP
  IF INKEY$ <> "" THEN END
NEXT i%

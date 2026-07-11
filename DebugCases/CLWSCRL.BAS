' Character Line Window Scroll

CLS

VIEW PRINT 3 TO 7

LOCATE 25, 40, 1

SLEEP

PRINT "test";

SLEEP

FOR i% = 1 TO 10
  PRINT CHR$(i% + 64)
  SLEEP
NEXT i%

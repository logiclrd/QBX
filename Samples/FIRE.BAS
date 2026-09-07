DECLARE SUB add (i%)
DIM fire%(80, 25), r%(63), g%(63), b%(63)
DEF SEG = &HB800
FOR i% = 0 TO 63
 OUT &H3C7, i%
 r%(i%) = INP(&H3C9)
 g%(i%) = INP(&H3C9)
 b%(i%) = INP(&H3C9)
NEXT i%
FOR i% = 0 TO 7
 IF i% = 6 THEN OUT &H3C8, 20 ELSE OUT &H3C8, i%
 OUT &H3C9, 63
 OUT &H3C9, i% * 8
 OUT &H3C9, 0
NEXT i%
FOR i% = 8 TO 15
 OUT &H3C8, i% + 48
 OUT &H3C9, 63
 OUT &H3C9, 63
 OUT &H3C9, (i% - 8) * 9
NEXT i%
LOCATE 1, 1
PRINT STRING$(4000, 219);
DO
 st# = TIMER
 FOR i% = 1 TO 80
  fire%(i%, 25) = INT(RND * 2) * 255
 NEXT i%
 FOR i% = 1 TO 80
  FOR j% = 3 TO 25
   t% = fire%(i%, j%) * 24
   n% = 25
   IF i% > 1 THEN
    t% = t% + fire%(i% - 1, j% - 1) + fire%(i% - 1, j%)
    n% = n% + 2
    IF j% < 25 THEN t% = t% + fire%(i% - 1, j% + 1): n% = n% + 1
   END IF
   t% = t% + fire%(i%, j% - 1): n% = n% + 1
   IF j% < 25 THEN t% = t% + fire%(i%, j% + 1): n% = n% + 1
   IF i% < 80 THEN
    t% = t% + fire%(i% + 1, j% - 1) + fire%(i% + 1, j%)
    n% = n% + 2
    IF j% < 25 THEN t% = t% + fire%(i% + 1, j% + 1): n% = n% + 1
   END IF
   fire%(i%, j% - 1) = t% \ n%
  NEXT j%
 NEXT i%
 FOR i% = 1 TO 80
  FOR j% = 1 TO 25
   p% = (j% - 1) * 160 + (i% - 1) * 2 + 1
   POKE p%, fire%(i%, j%) \ 16
  NEXT j%
 NEXT i%
 WHILE ABS(TIMER - st#) < .01: WEND
LOOP UNTIL INKEY$ <> ""
FOR i% = 0 TO 63
 OUT &H3C8, i%
 OUT &H3C9, r%(i%)
 OUT &H3C9, g%(i%)
 OUT &H3C9, b%(i%)
NEXT i%
CLS

SUB add (i%)
SHARED t%, n%
t% = t% + i%
n% = n% + 1
END SUB


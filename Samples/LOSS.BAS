'$DYNAMIC
DIM order%(319, 99), buff&(16003)
'$STATIC
DIM yt&(199)
yt&(0) = VARPTR(buff&(1))
FOR i% = 1 TO 199
 yt&(i%) = yt&(i% - 1) + 320
NEXT i%
FOR i% = 0 TO 99
 FOR j% = 0 TO 319
  order%(j%, i%) = j%
 NEXT j%
 FOR j% = 0 TO 319
  SWAP order%(j%, i%), order%(INT(RND * 320), i%)
 NEXT j%
NEXT i%
SCREEN 13
GET (0, 0)-(319, 199), buff&
j% = 64
FOR i% = 0 TO 127
 k% = i% < 64: l% = NOT k%
 OUT &H3C9, l% * j%
 OUT &H3C9, l% * j%
 OUT &H3C9, k% * -i% - 63 * l%
 j% = j% - 1
NEXT i%
FOR i% = 0 TO 127
 j% = j% + 1
 k% = i% < 64: l% = NOT k%
 OUT &H3C9, k% * j%
 OUT &H3C9, k% * j%
 OUT &H3C9, l% * j% - 63 * k%
NEXT i%
tseg% = VARSEG(buff&(0))
DEF SEG = tseg%
y% = 50
DO
 y% = y% + 1
 IF y% = 148 THEN
  PUT (0, 0), buff&, PSET
  WHILE TIMER = st#: WEND
  st# = TIMER
  y% = 50
 END IF
 z% = INT(RND * 100)
 po& = yt&(y%)
 FOR i% = 0 TO 319
  o& = po& + order%(i%, z%)
  POKE o&, (PEEK(o& - 1) + PEEK(o& + 1) + PEEK(o&) + PEEK(o& - 320)) \ 4 - 4
 NEXT i%
LOOP UNTIL INKEY$ <> ""
SCREEN 0: WIDTH 80


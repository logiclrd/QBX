DECLARE SUB updatefire ()
DECLARE SUB poly3 (c%, x1%, y1%, x2%, y2%, x3%, y3%)
TYPE trishapetype
 a1 AS SINGLE
 a2 AS SINGLE
 a3 AS SINGLE
 r1 AS SINGLE
 r2 AS SINGLE
 r3 AS SINGLE
END TYPE
DIM trishape(8) AS trishapetype, firearea%(63, 63), sprite%(15, 15)
SCREEN 13
OUT &H3C8, 0
FOR i% = 0 TO 767
 OUT &H3C9, 0
NEXT i%
FOR y% = -100 TO 99
 DEF SEG = &HA000 + (y% + 100) * 20
 FOR x% = -160 TO 159
  r! = SQR(1& * x% * x% + 1& * y% * y%)
  IF x% THEN
   a! = ATN(y% / x%)
   IF x% < 0 THEN a! = a! + 3.141593
  ELSE
   IF y% < 0 THEN a! = 4.712389 ELSE a! = 1.570796
  END IF
  spritex% = INT((a! * 320) / 6.283185) AND 15
  spritey% = INT(r! ^ .9) AND 15
  IF (spritex% + spritey%) = 0 THEN spritey% = 1
  POKE x% + 160, spritex% * 16 + spritey%
 NEXT x%
NEXT y%
FOR iter% = 0 TO 200
 st# = TIMER
 iofs% = SIN(iter% / 9) * 20
 jofs% = COS(iter% / 5) * 25
 OUT &H3C8, 1
 FOR i% = 0 TO 15
  FOR j% = 0 TO 15
   IF (i% + j%) THEN
    intens% = 63 * (((i% + iofs%) XOR (j% + jofs%)) AND 15) \ 15
    IF iter% < 63 THEN intens% = intens% * iter% \ 63
    OUT &H3C9, intens%
    OUT &H3C9, intens%
    OUT &H3C9, intens%
   END IF
  NEXT j%
 NEXT i%
 WHILE TIMER = st#: WEND
NEXT iter%
FOR radius% = 189 TO 10 STEP -1
 st# = TIMER
 FOR y% = -100 TO 99
  delta& = 1& * radius% * radius% - y% * y%
  IF delta& >= 0 THEN
   xdif% = SQR(delta&)
   sy% = y% + 100
   IF xdif% < 160 THEN
    LINE (0, sy%)-(160 - xdif%, sy%), 0
    LINE (160 + xdif%, sy%)-(319, sy%), 0
   END IF
  END IF
 NEXT y%
 iofs% = SIN(iter% / 9) * 20
 jofs% = (jofs% - 3) AND 15
 OUT &H3C8, 1
 FOR i% = 0 TO 15
  FOR j% = 0 TO 15
   IF (i% + j%) THEN
    intens% = 63 * (((i% + iofs%) XOR (j% + jofs%)) AND 15) \ 15
    IF iter% < 63 THEN intens% = intens% * iter% \ 63
    OUT &H3C9, intens%
    OUT &H3C9, intens%
    OUT &H3C9, intens%
   END IF
  NEXT j%
 NEXT i%
 WHILE TIMER = st#: WEND
 iter% = iter% + 1
NEXT radius%
FOR fadeout% = 63 TO 0 STEP -1
 st# = TIMER
 iofs% = SIN(iter% / 9) * 20
 jofs% = (jofs% - 3) AND 15
 OUT &H3C8, 1
 FOR i% = 0 TO 15
  FOR j% = 0 TO 15
   IF (i% + j%) THEN
    intens% = 63 * (((i% + iofs%) XOR (j% + jofs%)) AND 15) \ 15
    intens% = 63 - (63 - intens%) * fadeout% \ 63
    OUT &H3C9, intens%
    OUT &H3C9, intens%
    OUT &H3C9, intens%
   END IF
  NEXT j%
 NEXT i%
 WHILE TIMER = st#: WEND
 iter% = iter% + 1
NEXT fadeout%
RANDOMIZE TIMER
FOR i% = 1 TO 8
 trishape(i%).a1 = (i% / 8) * 6.283185 + RND - .5
 trishape(i%).a2 = (i% / 8) * 6.283185 + RND - .5
 trishape(i%).a3 = (i% / 8) * 6.283185 + RND - .5
 trishape(i%).r1 = 6 + 4 * RND - 2
 trishape(i%).r2 = 6 + 4 * RND
 trishape(i%).r3 = 6 + 4 * RND - 2
NEXT i%
FOR scale% = 1 TO 30
 st# = TIMER
 t% = scale% * 8
 i% = 63
 DO
  OUT &H3C8, t% + 1
  FOR j% = t% + 1 TO t% + 8
   OUT &H3C9, i%
   OUT &H3C9, i%
   OUT &H3C9, i%
  NEXT j%
  t% = t% - 8
  i% = i% - 10
  IF (i% < 0) AND (i% > -10) THEN i% = 0
 LOOP WHILE (i% >= 0) AND (t% >= 0)
 t% = scale% * 8
 FOR i% = 1 TO 8
  x1% = 160 + scale% * trishape(i%).r1 * COS(trishape(i%).a1)
  y1% = 100 - scale% * trishape(i%).r1 * SIN(trishape(i%).a1)
  x2% = 160 + scale% * trishape(i%).r2 * COS(trishape(i%).a2)
  trishape(i%).a1 = trishape(i%).a1 + .1
  trishape(i%).a2 = trishape(i%).a2 + .1
  trishape(i%).a3 = trishape(i%).a3 + .1
  y2% = 100 - scale% * trishape(i%).r2 * SIN(trishape(i%).a2)
  x3% = 160 + scale% * trishape(i%).r3 * COS(trishape(i%).a3)
  y3% = 100 - scale% * trishape(i%).r3 * SIN(trishape(i%).a3)
  poly3 t% + i%, x1%, y1%, x2%, y2%, x3%, y3%
 NEXT i%
 WHILE TIMER = st#: WEND
NEXT scale%
FOR i% = 0 TO 255 STEP 8
 st# = TIMER
 intens% = i% * 63 \ 255 - 31
 IF intens% < 0 THEN intens% = 0
 OUT &H3C8, 0
 OUT &H3C9, intens%
 OUT &H3C9, intens%
 OUT &H3C9, intens%
 FOR j% = i% TO 1 STEP -1
  intens% = (i% - j%) \ 4
  IF intens% > 63 THEN intens% = 63
  OUT &H3C8, j%
  OUT &H3C9, intens%
  OUT &H3C9, intens%
  OUT &H3C9, intens%
 NEXT j%
 FOR j% = 247 TO i% + 1 STEP -1
  OUT &H3C7, j%
  r% = INP(&H3C9)
  g% = INP(&H3C9)
  b% = INP(&H3C9)
  OUT &H3C8, j% + 8
  OUT &H3C9, r%
  OUT &H3C9, g%
  OUT &H3C9, b%
 NEXT j%
 WHILE st# = TIMER: WEND
NEXT i%
FOR i% = 256 TO 512 STEP 8
 OUT &H3C8, 0
 intens% = i% * 63 \ 255 - 31
 IF intens% > 63 THEN intens% = 63
 OUT &H3C9, intens%
 OUT &H3C9, intens%
 OUT &H3C9, intens%
 FOR j% = 1 TO i% - 248
  OUT &H3C9, 63
  OUT &H3C9, 63
  OUT &H3C9, 63
 NEXT j%
 ub% = i%
 IF ub% > 255 THEN ub% = 255
 FOR j% = j% TO ub%
  intens% = 63 - (j% - i% + 248) * 63 \ 255
  OUT &H3C9, intens%
  OUT &H3C9, intens%
  OUT &H3C9, intens%
 NEXT j%
 st# = TIMER
 WHILE TIMER = st#: WEND
NEXT i%
st# = TIMER
skipstart:
OUT &H3C8, 0
OUT &H3C9, 63
OUT &H3C9, 63
OUT &H3C9, 63
OUT &H3C9, 63
OUT &H3C9, 63
OUT &H3C9, 63
LINE (0, 0)-(319, 199), 1, BF
WHILE st# = TIMER: WEND
FOR i% = 63 TO 32 STEP -1
 st# = TIMER
 OUT &H3C8, 0
 OUT &H3C9, 63
 OUT &H3C9, (i% - 32) * 2
 OUT &H3C9, (i% - 32) * 2
 OUT &H3C9, 63
 OUT &H3C9, i%
 OUT &H3C9, i%
 WHILE st# = TIMER: WEND
NEXT i%
FOR i% = 31 TO 0 STEP -1
 st# = TIMER
 OUT &H3C8, 0
 OUT &H3C9, i% * 2
 OUT &H3C9, 0
 OUT &H3C9, 0
 OUT &H3C9, 63
 OUT &H3C9, i%
 OUT &H3C9, i%
 WHILE st# = TIMER: WEND
NEXT i%
OUT &H3C8, 32
FOR i% = 64 TO 191 STEP 2
 OUT &H3C9, 63
 OUT &H3C9, (i% - 64) \ 2
 OUT &H3C9, 0
NEXT i%
FOR i% = 192 TO 255 STEP 2
 OUT &H3C9, 63
 OUT &H3C9, 63
 OUT &H3C9, i% - 192
NEXT i%
FOR i% = 63 TO 0 STEP -1
 st# = TIMER
 OUT &H3C8, 1
 FOR j% = 1 TO i% STEP 2
  OUT &H3C9, i%
  OUT &H3C9, 0
  OUT &H3C9, 0
 NEXT j%
 bgi% = i%
 updatefire
 FOR x% = 0 TO 135
  DEF SEG = &HA000 + 20 * x%
  FOR y% = 0 TO 63
   b% = firearea%(y%, 0) * (x% \ 2) \ 102
   IF b% > 255 THEN b% = 255
   IF b% > bgi% THEN POKE y% + 128, b% \ 2
  NEXT y%
 NEXT x%
 WHILE st# = TIMER: WEND
NEXT i%
FOR i% = 1 TO 30
 st# = TIMER
 updatefire
 FOR x% = 0 TO 135
  DEF SEG = &HA000 + 20 * x%
  FOR y% = 0 TO 63
   b% = firearea%(y%, 0) * (x% \ 2) \ 102
   IF b% > 255 THEN b% = 255
   IF b% > bgi% THEN POKE y% + 128, b% \ 2
  NEXT y%
 NEXT x%
 WHILE st# = TIMER: WEND
NEXT i%
FOR i% = 128 TO 255
 OUT &H3C7, i% - 128
 r% = INP(&H3C9)
 g% = INP(&H3C9)
 b% = INP(&H3C9)
 b% = b% + 40
 IF b% > 63 THEN
  g% = g% + b% - 63
  b% = 63
  IF g% > 63 THEN
   r% = r% + g% - 63
   g% = 63
   IF r% > 63 THEN r% = 63
  END IF
 END IF
 OUT &H3C8, i%
 OUT &H3C9, r%
 OUT &H3C9, g%
 OUT &H3C9, b%
NEXT i%
' The original scrollstring$ was too cringy for words.
scrollstring$ = "doors and corners, kid. that's where they get you."
DIM font%(7, 7, LEN(scrollstring$))
DEF SEG = &HA000
COLOR 1
FOR i% = 1 TO LEN(scrollstring$)
 LOCATE 1, 1
 PRINT MID$(scrollstring$, i%, 1);
 FOR j% = 0 TO 7
  FOR k% = 0 TO 7
   font%(j%, k%, i%) = PEEK(j% + k% * 320)
  NEXT k%
 NEXT j%
NEXT i%
LINE (0, 0)-(7, 7), 1, BF
FOR i% = 319 TO -224 STEP -1
 st# = TIMER
 updatefire
 FOR x% = 0 TO 135
  DEF SEG = &HA000 + 20 * x%
  IF (x% >= 70) AND (x% < 78) THEN
   whichrow% = x% - 70
   FOR y% = 0 TO 319
    IF (y% >= 128) AND (y% < 192) THEN
     b% = firearea%(y% - 128, 0) * (x% \ 2) \ 102
     IF b% > 255 THEN b% = 255
     b% = b% \ 2
    ELSE
     b% = 1
    END IF
    whichpixel% = y% - i%
    IF (whichpixel% >= 0) AND (whichpixel% < 224) THEN
     whichchar% = whichpixel% \ 8 + 1
     whichcol% = whichpixel% AND 7
     IF font%(whichcol%, whichrow%, whichchar%) THEN b% = b% OR 128
    END IF
    POKE y%, b%
   NEXT y%
  ELSE
   FOR y% = 0 TO 63
    b% = firearea%(y%, 0) * (x% \ 2) \ 102
    IF b% > 255 THEN b% = 255
    IF b% > bgi% THEN POKE y% + 128, b% \ 2
   NEXT y%
  END IF
 NEXT x%
 WHILE st# = TIMER: WEND
NEXT i%
nonewfire% = -1
FOR i% = 1 TO 200
 st# = TIMER
 updatefire
 FOR x% = 0 TO 135
  DEF SEG = &HA000 + 20 * x%
  FOR y% = 0 TO 63
   b% = firearea%(y%, 0) * (x% \ 2) \ 102
   IF b% > 255 THEN b% = 255
   IF b% > bgi% THEN POKE y% + 128, b% \ 2
  NEXT y%
 NEXT x%
 IF i% > 137 THEN
  intens% = 200 - i%
  OUT &H3C8, 0
  FOR j% = 0 TO 63 STEP 2
   OUT &H3C9, j% * intens% \ 63
   OUT &H3C9, 0
   OUT &H3C9, 0
  NEXT j%
  FOR j% = 64 TO 191 STEP 2
   OUT &H3C9, 63
   OUT &H3C9, (j% - 64) * intens% \ 127
   OUT &H3C9, 0
  NEXT j%
  FOR j% = 192 TO 255 STEP 2
   OUT &H3C9, 63
   OUT &H3C9, 63
   OUT &H3C9, (j% - 192) * intens% \ 63
  NEXT j%
 END IF
 WHILE st# = TIMER: WEND
NEXT i%
SCREEN 0: WIDTH 80
PRINT "sillydemo by logiclrd (november 2000)"
PRINT "framerate is 1193181/65536"

SUB poly3 (c%, x1%, y1%, x2%, y2%, x3%, y3%)
ON LOCAL ERROR RESUME NEXT
IF (y1% > y2%) THEN SWAP x1%, x2%: SWAP y1%, y2%
IF (y2% > y3%) THEN SWAP x2%, x3%: SWAP y2%, y3%
IF (y1% > y2%) THEN SWAP x1%, x2%: SWAP y1%, y2%
IF y3% = y1% THEN
 IF x1% < x2% THEN
  IF x2% < x3% THEN
   LINE (x1%, y1%)-(x3%, y1%), c%
  ELSE
   LINE (x1%, y1%)-(x2%, y1%), c%
  END IF
 ELSEIF x2% < x3% THEN
  IF x1% < x3% THEN
   LINE (x2%, y1%)-(x3%, y1%), c%
  ELSE
   LINE (x2%, y1%)-(x1%, y1%), c%
  END IF
 END IF
ELSE
 xl% = x1%
 xr% = x1%
 y21% = y2% - y1%
 y31% = y3% - y1%
 y32% = y3% - y2%
 tx% = (y21% * 1& * (x3% - x1%) + y31% \ 2) \ y31% + x1%
 IF y1% = y2% THEN
  LINE (x1%, y1%)-(x2%, y1%), c%
  IF x1% < x2% THEN
   xr% = x2%
  ELSE
   xl% = x2%
  END IF
  sxl% = 0
  sxr% = 0
  IF tx% < x2% THEN
   dxl% = ((x3% - x1%) * 256& + y31% \ 2) \ (y3% - y1%)
   dsxl% = dxl% AND 255
   IF dxl% >= 0 THEN
    dxl% = dxl% \ 256
    sxl% = 0
   ELSE
    dxl% = dxl% \ 256 - 1
    sxl% = 256
   END IF
  ELSE
   dxr% = ((x3% - x1%) * 256& + y31% \ 2) \ (y3% - y1%)
   dsxr% = dxr% AND 255
   IF dxr% >= 0 THEN
    dxr% = dxr% \ 256
    sxr% = 0
   ELSE
    dxr% = dxr% \ 256 - 1
    sxr% = 256
   END IF
  END IF
 ELSE
  IF tx% > x2% THEN
   dxl% = ((x2% - x1%) * 256& + y21% \ 2) \ (y2% - y1%)
   dxr% = ((x3% - x1%) * 256& + y31% \ 2) \ (y3% - y1%)
  ELSE
   dxl% = ((x3% - x1%) * 256& + y31% \ 2) \ (y3% - y1%)
   dxr% = ((x2% - x1%) * 256& + y21% \ 2) \ (y2% - y1%)
  END IF
  dsxl% = dxl% AND 255
  IF dxl% >= 0 THEN
   dxl% = dxl% \ 256
   sxl% = 0
  ELSE
   dxl% = dxl% \ 256 - 1
   sxl% = 256
  END IF
  dsxr% = dxr% AND 255
  IF dxr% >= 0 THEN
   dxr% = dxr% \ 256
   sxr% = 0
  ELSE
   dxr% = dxr% \ 256 - 1
   sxr% = 256
  END IF
  FOR i% = y1% TO y2% - 1
   LINE (xl%, i%)-(xr%, i%), c%
   xl% = xl% + dxl%
   xr% = xr% + dxr%
   sxl% = sxl% + dsxl%
   sxr% = sxr% + dsxr%
   IF sxl% > 255 THEN
    xl% = xl% + 1
    sxl% = sxl% - 256
   END IF
   IF sxr% > 255 THEN
    xr% = xr% + 1
    sxr% = sxr% - 256
   END IF
  NEXT i%
  LINE (x2%, y2%)-(tx%, y2%), c%
 END IF
 IF y2% <> y3% THEN
  IF tx% > x2% THEN
   xl% = x2%
   dxl% = ((x3% - x2%) * 256& + y32% \ 2) \ (y3% - y2%)
   dsxl% = dxl% AND 255
   IF dxl% >= 0 THEN
    dxl% = dxl% \ 256
    sxl% = 0
   ELSE
    dxl% = dxl% \ 256 - 1
    sxl% = 256
   END IF
  ELSE
   xr% = x2%
   dxr% = ((x3% - x2%) * 256& + y32% \ 2) \ (y3% - y2%)
   dsxr% = dxr% AND 255
   IF dxr% >= 0 THEN
    dxr% = dxr% \ 256
    sxr% = 0
   ELSE
    dxr% = dxr% \ 256 - 1
    sxr% = 256
   END IF
  END IF
  FOR i% = y2% + 1 TO y3%
   xl% = xl% + dxl%
   xr% = xr% + dxr%
   sxl% = sxl% + dsxl%
   sxr% = sxr% + dsxr%
   IF sxl% > 255 THEN
    xl% = xl% + 1
    sxl% = sxl% - 256
   END IF
   IF sxr% > 255 THEN
    xr% = xr% + 1
    sxr% = sxr% - 256
   END IF
   LINE (xl%, i%)-(xr%, i%), c%
  NEXT i%
 END IF
END IF
END SUB

SUB updatefire
SHARED firearea%(), x1%, x2%, bgi%, nonewfire%
IF nonewfire% THEN
 FOR i% = 0 TO 63
  b% = firearea%(i%, 63) - 1
  IF b% >= 0 THEN firearea%(i%, 63) = b%
 NEXT i%
ELSE
 FOR i% = 0 TO 63
  p! = 1 / ABS(i% - 31.5)
  IF RND < p! THEN firearea%(i%, 63) = 1020 ELSE firearea%(i%, 63) = 0
 NEXT i%
END IF
FOR i% = 0 TO 63
 im% = (i% - 1) AND 63
 ip% = (i% + 1) AND 63
 FOR j% = 0 TO 63
  jm% = (j% - 1) AND 63
  jp% = (j% + 1) AND 63
  firearea%(i%, jm%) = (firearea%(i%, j%) * 3 + firearea%(im%, j%) + firearea%(ip%, j%) + firearea%(i%, jm%) + firearea%(i%, jp%)) \ 7
 NEXT j%
NEXT i%
DEF SEG = &HA000
o% = -22208
FOR i% = 0 TO 63
 FOR j% = 0 TO 63
  c% = firearea%(j%, i%) * 2 \ 3
  IF c% > 255 THEN c% = 127 ELSE c% = c% \ 2
  IF c% = 0 THEN c% = 1
  POKE o%, c%
  o% = o% + 1
 NEXT j%
 o% = o% + 256
NEXT i%
END SUB

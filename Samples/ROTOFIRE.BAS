CONST SPRITESIZE% = 64
CONST SPRITEMAX% = SPRITESIZE% - 1
CONST SCALE% = 16384 \ SPRITESIZE%
DIM fire%(SPRITEMAX%, SPRITEMAX%, 1), lut%(16383)
'$DYNAMIC
DIM blurlut%(8192)
'$STATIC
FOR i% = SCALE% TO 16383
 lut%(i%) = i% \ SCALE%
NEXT i%
FOR i% = 32 TO 8192
 blurlut%(i%) = i% \ 32
NEXT i%
SCREEN 13
FOR i% = 0 TO 255
 SELECT CASE i%
  CASE 0 TO 85:
   OUT &H3C9, i% * 63 \ 85
   OUT &H3C9, 0
   OUT &H3C9, 0
  CASE 86 TO 170:
   OUT &H3C9, 63
   OUT &H3C9, (i% - 85) * 63 \ 85
   OUT &H3C9, 0
  CASE 171 TO 255:
   OUT &H3C9, 63
   OUT &H3C9, 63
   OUT &H3C9, (i% - 170) * 63 \ 85
 END SELECT
NEXT i%
DEF SEG = &HA000
apage% = 1
DO
 SWAP apage%, vpage%
 FOR i% = 0 TO SPRITESIZE% + SPRITEMAX%
  fire%(INT(RND * SPRITESIZE%), INT(RND * SPRITESIZE%), vpage%) = INT(RND * 2) * 255
 NEXT i%
 jm1% = SPRITEMAX%
 jp1% = 1
 jp2% = 2
 FOR j% = 0 TO SPRITEMAX%
  im1% = SPRITEMAX%
  ip1% = 1
  FOR i% = 0 TO SPRITEMAX%
   fire%(i%, j%, apage%) = blurlut%(fire%(im1%, j%, vpage%) + fire%(ip1%, j%, vpage%) + fire%(im1%, jp2%, vpage%) + fire%(ip1%, jp2%, vpage%) + 2 * (fire%(im1%, jp1%, vpage%) + fire%(i%, j%, vpage%) + fire%(ip1%, jp1%, vpage%) + fire%(i%, jp2%,  _
vpage%)) + 20 * fire%(i%, jp1%, vpage%))
   'PSET (i%, j%), fire%(i%, j%, apage%)
   im1% = (im1% + 1) AND SPRITEMAX%
   ip1% = (ip1% + 1) AND SPRITEMAX%
  NEXT i%
  jm1% = (jm1% + 1) AND SPRITEMAX%
  jp1% = (jp1% + 1) AND SPRITEMAX%
  jp2% = (jp2% + 1) AND SPRITEMAX%
 NEXT j%
 o% = 0
 a! = a! + .01
 lx% = INT(188.679622641132# * SCALE% * COS(a! + 2.58299336637912#)) AND 16383
 ly% = INT(188.679622641132# * SCALE% * SIN(a! + 2.58299336637912#)) AND 16383
 dx% = INT(SCALE% * COS(a!)) AND 16383
 dy% = INT(SCALE% * SIN(a!)) AND 16383
 dlx% = dy%
 dly% = -dx%
 FOR j% = 0 TO 101
  x% = lx%
  y% = ly%
  FOR o% = o% TO o% + 319
   POKE o%, fire%(lut%(x%), lut%(y%), apage%)
   x% = (x% + dx%) AND 16383
   y% = (y% + dy%) AND 16383
  NEXT o%
  lx% = (lx% + dlx%) AND 16383
  ly% = (ly% + dly%) AND 16383
 NEXT j%
 x% = lx%
 y% = ly%
 FOR o% = o% TO 32766
  POKE o%, fire%(lut%(x%), lut%(y%), apage%)
  x% = (x% + dx%) AND 16383
  y% = (y% + dy%) AND 16383
 NEXT o%
 POKE o%, fire%(lut%(x%), lut%(y%), apage%)
 x% = (x% + dx%) AND 16383
 y% = (y% + dy%) AND 16383
 FOR o% = -32768 TO -32577
  POKE o%, fire%(lut%(x%), lut%(y%), apage%)
  x% = (x% + dx%) AND 16383
  y% = (y% + dy%) AND 16383
 NEXT o%
 lx% = (lx% + dlx%) AND 16383
 ly% = (ly% + dly%) AND 16383
 FOR j% = 103 TO 199
  x% = lx%
  y% = ly%
  FOR o% = o% TO o% + 319
   POKE o%, fire%(lut%(x%), lut%(y%), apage%)
   x% = (x% + dx%) AND 16383
   y% = (y% + dy%) AND 16383
  NEXT o%
  lx% = (lx% + dlx%) AND 16383
  ly% = (ly% + dly%) AND 16383
 NEXT j%
monkey:
LOOP UNTIL INKEY$ <> ""
SCREEN 0: WIDTH 80, 25


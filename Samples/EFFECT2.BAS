SCREEN 13
DIM buffer%(32, 20), sbuffer%(32, 20), xlattab%(255), wc&(10)
FOR i% = 0 TO 10
 wc&(i%) = (COS(i% * 3.141593 / 10 - 3.141593) + 1) * 50
NEXT i%
FOR i% = 0 TO 255
 OUT &H3C8, i%
 OUT &H3C9, i% \ 16
 OUT &H3C9, i% \ 8
 OUT &H3C9, i% \ 4
 xlattab%(i%) = i% * SQR(i%) / 16
NEXT i%
DEF SEG = &HA000
FOR i% = 0 TO 32
 FOR j% = 0 TO 20
  buffer%(i%, j%) = INT(RND * 512)
 NEXT j%
NEXT i%
DO
 FOR i% = 0 TO 32
  FOR j% = 0 TO 20
   buffer%(i%, j%) = (buffer%(i%, j%) + INT(RND * 10) + 4) AND 511
   IF buffer%(i%, j%) > 255 THEN
    sbuffer%(i%, j%) = 511 - buffer%(i%, j%)
   ELSE
    sbuffer%(i%, j%) = buffer%(i%, j%)
   END IF
  NEXT j%
 NEXT i%
 x% = 0
 FOR i% = 0 TO 31
  y% = 0
  FOR j% = 0 TO 19
   xw% = 0
   FOR xx% = x% TO x% + 9
    yw% = 0
    FOR yy% = y% TO y% + 9
     PSET (xx%, yy%), ((sbuffer%(i%, j%) * wc&(10 - xw%) + sbuffer%(i% + 1, j%) * wc&(xw%)) * wc&(10 - yw%) + (sbuffer%(i%, j% + 1) * wc&(10 - xw%) + sbuffer%(i% + 1, j% + 1) * wc&(xw%)) * wc&(yw%)) \ 10000
     yw% = yw% + 1
    NEXT yy%
    xw% = xw% + 1
   NEXT xx%
   y% = y% + 10
  NEXT j%
  x% = x% + 10
 NEXT i%
LOOP WHILE INKEY$ = ""
SCREEN 0: WIDTH 80, 25


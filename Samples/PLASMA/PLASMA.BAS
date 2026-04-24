DECLARE FUNCTION hyp! (a%, b%)
DECLARE SUB ar ()
'$DYNAMIC
SCREEN 13: DEF SEG = &HA000
FOR i% = 0 TO 15
 FOR j% = 0 TO 7
  OUT &H3C8, i% + j% * 32
  OUT &H3C9, i% * 4
  OUT &H3C9, i% * 4
  OUT &H3C9, i% * 4
  OUT &H3C8, (31 - i%) + j% * 32
  OUT &H3C9, i% * 4
  OUT &H3C9, i% * 4
  OUT &H3C9, i% * 4
 NEXT j%
NEXT i%
'FOR i% = 0 TO 255
' LINE (i%, 0)-(i%, 199), i%
'NEXT i%
RANDOMIZE TIMER
DIM SHARED a, b, c
DIM SHARED sintab(16000), costab(16000)
FOR i% = 0 TO 16000
 sintab(i%) = SIN(i% / 57) * 16
 costab(i%) = COS(i% / 57) * 16
NEXT i%
a = 57 / (INT(RND * 95) + 1)
b = 57 / (INT(RND * 95) + 1)
c = 57 / (INT(RND * 95) + 1)
da = (RND + .1) / 50
db = (RND + .1) / 50
dc = (RND + .1) / 50
st# = TIMER
DO
' ar
 z& = 16080
 FOR y% = 50 TO 149 STEP 2
  FOR x% = 80 TO 239 STEP 2
   cl% = 24 + 8 * sintab((150 + sintab((x% + y%) * a) + sintab(y% * b) + costab(SQR((x% - 160) * (x% - 160) + (y% - 100) * (y% - 100)) * c)) * 57)
   cl% = cl% \ 16
   POKE z&, cl%: POKE z& + 1, cl%: POKE z& + 320, cl%: POKE z& + 321, cl%: z& = z& + 2
  NEXT x%
  z& = z& + 480
  IF INKEY$ <> "" THEN SCREEN 0: WIDTH 80: PRINT "fps: "; (fr% / (TIMER - st#)): END
 NEXT y%
 fr% = fr% + 1
 a = a + da
 IF (a * 57) < 5 THEN da = (RND + .1) / 50
 IF (a * 57) > 95 THEN da = -(RND + .1) / 50
 b = b + db
 IF (b * 57) < 5 THEN db = (RND + .1) / 50
 IF (b * 57) > 95 THEN db = -(RND + .1) / 50
 c = c + dc
 IF (c * 57) < 5 THEN dc = (RND + .1) / 50
 IF (c * 57) > 95 THEN dc = -(RND + .1) / 50
LOOP
'detail% = 32
'DO
' ar detail%
' detail% = detail% \ 2
'LOOP WHILE detail%

REM $STATIC
SUB ar
 SHARED fr%, st#
 z& = 16080
 'FOR y% = 0 TO 199
 ' FOR x% = 0 TO 319
 FOR y% = 50 TO 149 STEP 2
  FOR x% = 80 TO 239 STEP 2
   cl% = 24 + 8 * sintab((150 + sintab((x% + y%) * a) + sintab(y% * b) + costab(SQR((x% - 160) * (x% - 160) + (y% - 100) * (y% - 100)) * c)) * 57)
   'cl% = 16 + 8 * ((1 + SIN((SIN((x% + y%) / a * 57)) + (SIN(y% / b * 57)) + (COS(SQR((x% - 160) * (x% - 160) + (y% - 100) * (y% - 100)) / c * 57)))))
   'cl% = 16 + 8 * ((1 + sintab(150 + (sintab(150 + (x% + y%) / a)) + (sintab(150 + y% / b)) + (costab(150 + hyp(x% - 319 \ 2, y% - 199 \ 2) / c)))))
   'LINE (x%, y%)-(x% + 1, y% + 1), cl%, B
   cl% = cl% \ 16
   POKE z&, cl%
   POKE z& + 1, cl%
   POKE z& + 320, cl%
   POKE z& + 321, cl%
   z& = z& + 2
  NEXT x%
  z& = z& + 480
  IF INKEY$ <> "" THEN SCREEN 0: WIDTH 80: PRINT "fps: "; (fr% / (TIMER - st#)): END
 NEXT y%
END SUB

FUNCTION hyp (a%, b%)
hyp = SQR(a% ^ 2 + b% ^ 2)
END FUNCTION


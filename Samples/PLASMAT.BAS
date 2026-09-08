TYPE xytype
 x AS INTEGER: dx AS INTEGER
 y AS INTEGER: dy AS INTEGER
END TYPE
DIM charge(5) AS xytype
FOR i% = 1 TO 5
 charge(i%).x = INT(RND * 80) + 1: charge(i%).dx = INT(RND * 2) * 2 - 1
 charge(i%).y = INT(RND * 25) + 1: charge(i%).dy = INT(RND * 2) * 2 - 1
NEXT i%
update$ = SPACE$(2000)
DO
 FOR i% = 1 TO 5
  charge(i%).x = charge(i%).x + charge(i%).dx
  charge(i%).y = charge(i%).y + charge(i%).dy
  IF (charge(i%).x = 1) OR (charge(i%).x = 80) THEN charge(i%).dx = -charge(i%).dx
  IF (charge(i%).y = 1) OR (charge(i%).y = 25) THEN charge(i%).dy = -charge(i%).dy
 NEXT i%
 z% = 0
 FOR y% = 1 TO 25
  FOR x% = 1 TO 80
   z% = z% + 1
   c! = 0
   FOR i% = 1 TO 5
    d! = SQR((charge(i%).x - x%) ^ 2 + (charge(i%).y - y%) ^ 2)
    IF d! < 1 THEN c! = c! + 15 ELSE c! = c! + 15 / d!
   NEXT i%
   c! = c! / 5
   SELECT CASE CINT(c!)
    CASE 0: MID$(update$, z%, 1) = " "
    CASE 1: MID$(update$, z%, 1) = "°"
    CASE 2: MID$(update$, z%, 1) = "±"
    CASE 3: MID$(update$, z%, 1) = "²"
    CASE ELSE: MID$(update$, z%, 1) = "Û"
   END SELECT
  NEXT x%
 NEXT y%
 LOCATE 1, 1: PRINT LEFT$(update$, 1920);
 LOCATE 25, 1: PRINT MID$(update$, 1921, 80);
LOOP UNTIL INKEY$ <> ""


DECLARE SUB UpdateFire ()
DEFINT A-Z
'                              |        __--        ______
' *-----------------*          |    __--______------
'  \   \   |   /   /           | *--------------------------
'    \  \  |  /  /             |   \  --__    ------______
'      \ \ | / /               |     \    --__            ------______
'        \\|//                 | ______\______--______________________------_
'          *                   |
'    top-down view             |  side view of rays (most of them skipped)
'             +----------------+----------------------------+
'             | the angle between any two adjacent rays is  |
'             | always the same -- namely, the (vertical or |
'             | horizontal) field of vision (a measure of   |
'             | angle) divided by the (vertical or          |
'             | horizontal) resolution (320,200)            |
'             +----------------+----------------------------+
' if the character is facing   |  if you measure the angles in radians
' "upwards, as depicted here,  |  starting at 0 being directly down, the
' then the coordinates of each |  intersection of a given angle with the
' side of the screen for a     |  floor is going to be:
' given row are going to be    |     (eyeheight - floorheight) * TAN(angle)
'                              +-+--------------------------------------------
' (-sin(HFOV/2)*d,cos(HFOV/2)*d) |
'            and                 |
'  (sin(HFOV/2)*d,cos(HFOV/2)*d) |
'                              +-+
' the distance between these   |
' is:                          |
'   x2 - x1                    +-----+
' = d*sin(HFOV/2) - (-d*sin(HFOV/2)) |
' = 2*d*sin(HFOV/2)                  :
' and thus, the texture              .
' distance between two pixels is
' 2*d*sin(HFOV/2), and d is equal to .
'  (eyeheight - floorheight) * TAN(angle)
'
' getting the offset is also quite easy: you simply add the relative angle
' to the HFOV/2 in the first of the two points 15 lines up, and evaluate it
' to get your point. you can then multiply this by 256, truncate it with AND,
' and store it in an integer to get your fixed-point values. the angle of
' the trace along the floor is (viewangle - 90ø), and the amplitude of this
' is:
'   2 * (eyeheight - floorheight) * TAN(angle) * sin(HFOV/2) / HResolution
'
' so your initial x and y values are:
'  d = eyeheight * TAN(thisYangle)
'  x = d * -SIN(viewXangle + HorizontalFOV / 2)
'  y = d * COS(viewXangle + HorizontalFOV / 2)
'
' and your deltax and deltay are:
'  dx = d * SIN(HorizontalFOV / 2) / 160 * COS(viewXangle)
'  dy = d * SIN(HorizontalFOV / 2) / 160 * SIN(viewXangle)
'
'this implementation uses floating-point. I'll leave itto you to convert
'it to fixed-point =D

'COMMON SHARED Flame(), Flame2()
'^-- why do you need this? COMMON is for multi-module projects (ie, more
'    than one .BAS file)

DIM SHARED E AS SINGLE
SCREEN 13
DIM SHARED Flame(-1 TO 64, -1 TO 64), Flame2(-1 TO 64, -1 TO 64), Div5(635)
FOR F = 0 TO 31: OUT &H3C8, F: OUT &H3C9, F * 2: OUT &H3C9, 0: OUT &H3C9, 0: NEXT F
FOR F = 32 TO 63: OUT &H3C8, F: OUT &H3C9, 63: OUT &H3C9, (F - 32) * 2: OUT &H3C9, 0: NEXT F
FOR F = 0 TO 635: Div5(F) = CINT(F / 5.7): NEXT F
CLS
Ang = 0
DEF SEG = &HA000
HorizontalFOV! = 45 'degrees
VerticalFOV! = 45
eyeheight! = 15
viewXangle! = 0'radians
DO
   UpdateFire
   FPC& = FPC& + 1
   FOR sy% = 0 TO 199
    thisYangle! = VerticalFOV! / 2 - sy% * VerticalFOV! / 199
    IF thisYangle! < 0 THEN 'it's part of the floor
      thisYangle! = thisYangle! / 57.295779# + 1.570796
      d! = eyeheight! * TAN(thisYangle!) '^-- it's counted in rad. from 0=down
      x! = d! * -SIN(viewXangle! + HorizontalFOV! / (2 * 57.295779#)) 'conv.
      y! = d! * COS(viewXangle! + HorizontalFOV! / (2 * 57.295779#)) 'dg->rd

      dx! = d! * SIN(HorizontalFOV! / (2 * 57.295779#)) * COS(viewXangle!) / 160
      dy! = d! * SIN(HorizontalFOV! / (2 * 57.295779#)) * SIN(viewXangle!) / 160
      FOR ox& = sy% * 320& TO sy% * 320& + 319
        POKE ox&, Flame(INT(x!) AND 63, INT(y!) AND 63)
        x! = x! + dx!
        y! = y! + dy!
      NEXT ox&
    END IF
   NEXT sy%
   ' 'Ang = (Ang + 1) MOD 360
   'the analog would be:
   viewXangle! = viewXangle! + .017453
LOOP WHILE INKEY$ = ""
DEF SEG

SUB UpdateFire
   FOR F = 0 TO 63
      Flame(F, 0) = INT(RND * 64)
      Flame(0, F) = INT(RND * 64)
      Flame(F, 63) = INT(RND * 64)
      Flame(63, F) = INT(RND * 64)
   NEXT F
   FOR G = 0 TO 63
      FOR F = 0 TO 63
         Avg = Div5(Flame(F, G) + Flame(F + 1, G) + Flame(F - 1, G) + Flame(F, G + 1) + Flame(F, G - 1))
         Flame2(F + SGN(32 - F), G + SGN(32 - G)) = Avg
      NEXT F
   NEXT G
   FOR G = 1 TO 62
      FOR F = 1 TO 62
         Flame(F, G) = Flame2(F, G)
      NEXT F
   NEXT G
END SUB


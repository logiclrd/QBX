DECLARE SUB showpoints3d ()
DECLARE SUB newvector (p1%, p2%)
DECLARE SUB rotatez (x!, y!, a!)
DECLARE SUB rotatey (x!, z!, a!)
DECLARE SUB rotatex (y!, z!, a!)
DECLARE SUB showvectors3d ()
DECLARE FUNCTION newpoint% (x!, y!, z!, c%)
DECLARE SUB transform3d ()
TYPE xytype
 x AS INTEGER
 y AS INTEGER
END TYPE
TYPE xyztype
 x AS SINGLE
 y AS SINGLE
 z AS SINGLE
 c AS INTEGER
 sx AS INTEGER
 sy AS INTEGER
END TYPE
TYPE vectortype
 p1 AS INTEGER
 p2 AS INTEGER
 c AS INTEGER
 a AS INTEGER
END TYPE
DIM SHARED numpoints%, points(400) AS xyztype, screenpoints(400) AS xytype
DIM SHARED vector(400) AS vectortype, numvectors%, avgz!
numpoints% = 0
numvectors% = 0
points(0).x = 0 'camera
points(0).y = -400
points(0).z = 0
FOR i% = 1 TO 40
 r! = i% / 3
 a% = newpoint%(i% * 10 - 196, SIN(r!) * 20 + 160, COS(r!) * 20, 15)
 b% = newpoint%(i% * 10 - 194, SIN(r!) * 20 + 160, COS(r!) * 20, 15)
 c% = newpoint%(i% * 10 - 196, -SIN(r!) * 20 + 160, -COS(r!) * 20, 15)
 d% = newpoint%(i% * 10 - 194, -SIN(r!) * 20 + 160, -COS(r!) * 20, 15)
 e% = newpoint%(i% * 10 - 195, SIN(r! - .1) * 20 + 160, COS(r! - .1) * 20, 15)
 f% = newpoint%(i% * 10 - 195, SIN(r! + .1) * 20 + 160, COS(r! + .1) * 20, 15)
 g% = newpoint%(i% * 10 - 195, -SIN(r! - .1) * 20 + 160, -COS(r! - .1) * 20, 15)
 h% = newpoint%(i% * 10 - 195, -SIN(r! + .1) * 20 + 160, -COS(r! + .1) * 20, 15)
 newvector a%, c%
 newvector b%, d%
 newvector e%, g%
 newvector f%, h%
 IF i% > 1 THEN
  e2% = e% - 8: f2% = f% - 8: g2% = g% - 8: h2% = h% - 8
  newvector e%, e2%
  newvector f%, f2%
  newvector g%, g2%
  newvector h%, h2%
 END IF
NEXT i%
SCREEN 13
DO
 'SLEEP
 st# = TIMER
 rotatez 0, 160, -rz!
 rotatey 0, 0, -ry!
 rotatex 160, 0, 10
 ry! = SIN(ryr!) * 180
 ryr! = ryr! + .01
 IF ryr! > 6.283185 THEN ryr! = ryr! - 6.283185
 rotatey 0, 0, ry!
 rz! = SIN(rzr!) * 50
 rzr! = rzr! + .05
 IF rzr! > 6.283185 THEN rzr! = rzr! - 6.283185
 rotatez 0, 160, rz!
showpoints3d
' showvectors3d
 WHILE TIMER = st#: WEND
LOOP UNTIL INKEY$ <> ""
SCREEN 0: WIDTH 80, 25
PRINT "3D7.EXE by logiclrd - DNA model with depth cueing"

FUNCTION newpoint% (x!, y!, z!, c%)
numpoints% = numpoints% + 1
points(numpoints%).x = x!
points(numpoints%).y = y!
points(numpoints%).z = z!
points(numpoints%).c = c%
newpoint% = numpoints%
END FUNCTION

SUB newvector (p1%, p2%)
numvectors% = numvectors% + 1
vector(numvectors%).p1 = p1%
vector(numvectors%).p2 = p2%
END SUB

SUB rotatex (y!, z!, a!)
'rule of rotating around x axis: x value never changes
sintheta! = SIN(a! / 57.295779#): costheta! = COS(a! / 57.295779#)
FOR i% = 1 TO numpoints%
 ny! = y! + (points(i%).y - y!) * costheta! - (points(i%).z - z!) * sintheta!
 points(i%).z = z! + (points(i%).y - y!) * sintheta! + (points(i%).z - z!) * costheta!
 points(i%).y = ny!
NEXT i%
END SUB

SUB rotatey (x!, z!, a!)
'rule of rotating around y axis: y value never changes
sintheta! = SIN(a! / 57.295779#): costheta! = COS(a! / 57.295779#)
FOR i% = 1 TO numpoints%
 nx! = x! + (points(i%).x - x!) * costheta! - (points(i%).z - z!) * sintheta!
 points(i%).z = z! + (points(i%).x - x!) * sintheta! + (points(i%).z - z!) * costheta!
 points(i%).x = nx!
NEXT i%
END SUB

SUB rotatez (x!, y!, a!)
'rule of rotating around z axis: z value never changes
sintheta! = SIN(a! / 57.295779#): costheta! = COS(a! / 57.295779#)
FOR i% = 1 TO numpoints%
 nx! = x! + (points(i%).x - x!) * costheta! - (points(i%).y - y!) * sintheta!
 points(i%).y = y! + (points(i%).x - x!) * sintheta! + (points(i%).y - y!) * costheta!
 points(i%).x = nx!
NEXT i%
END SUB

SUB showpoints
scalen% = 1
scaled% = 8
LOCATE 1, 1: PRINT "Top-down:"
l% = 10000
b% = -10000
FOR i% = 1 TO numpoints%
 IF points(i%).z < l% THEN l% = points(i%).z
 IF points(i%).z > b% THEN b% = points(i%).z
NEXT i%
r% = b% - l%
IF r% = 0 THEN r% = 1
FOR i% = 1 TO numpoints%
 PSET (screenpoints(i%).x, screenpoints(i%).y), 0
 screenpoints(i%).x = 160 + points(i%).x * scalen% \ scaled%
 screenpoints(i%).y = 100 - points(i%).y * scalen% \ scaled%
 PSET (screenpoints(i%).x, screenpoints(i%).y), points(i%).c '(points(i%).z - l%) * 15 \ r% + 16
NEXT i%
END SUB

SUB showpoints3d
distcamera! = ABS(points(0).y)
FOR i% = 1 TO numpoints%
 IF points(i%).y > 0 THEN
  distpoint! = SQR((points(0).x - points(i%).x) ^ 2 + (points(0).y - points(i%).y) ^ 2 + (points(0).z - points(i%).z) ^ 2)
  distplane! = ABS(points(0).y - points(i%).y)
  distinter! = distcamera! * distpoint! / distplane!
  PSET (screenpoints(i%).x, screenpoints(i%).y), 0
  IF (distinter! >= 0) AND (distinter! <= distpoint) THEN
   screenpoints(i%).x = 160 + ((points(i%).x - points(0).x) / distpoint!) * distinter! + points(0).x
   screenpoints(i%).y = 100 - ((points(i%).z - points(0).z) / distpoint!) * distinter! + points(0).z
   PSET (screenpoints(i%).x, screenpoints(i%).y), points(i%).c
  END IF
 END IF
NEXT i%
END SUB

SUB showvectors3d
'specialized code contained within!
transform3d
FOR i% = 1 TO numvectors%
 LINE (screenpoints(vector(i%).p1).x, screenpoints(vector(i%).p1).y)-(screenpoints(vector(i%).p2).x, screenpoints(vector(i%).p2).y), 0
 y! = (points(vector(i%).p1).y + points(vector(i%).p2).y) / 2
 vector(i%).c = 31 - y! * 15.49 / 400
NEXT i%
FOR i% = 1 TO numvectors%
 d! = SQR((points(vector(i%).p1).x - points(vector(i%).p2).x) ^ 2 + (points(vector(i%).p1).y - points(vector(i%).p2).y) ^ 2 + (points(vector(i%).p1).z - points(vector(i%).p2).z) ^ 2)
 LINE (points(vector(i%).p1).sx, points(vector(i%).p1).sy)-(points(vector(i%).p2).sx, points(vector(i%).p2).sy), vector(i%).c
NEXT i%
FOR i% = 1 TO numpoints%
 screenpoints(i%).x = points(i%).sx
 screenpoints(i%).y = points(i%).sy
NEXT i%
END SUB

SUB transform3d
distcamera! = ABS(points(0).y)
FOR i% = 1 TO numpoints%
 IF points(i%).y > 0 THEN
  distpoint! = SQR((points(0).x - points(i%).x) ^ 2 + (points(0).y - points(i%).y) ^ 2 + (points(0).z - points(i%).z) ^ 2)
  distplane! = ABS(points(0).y - points(i%).y)
  distinter! = distpoint! * distcamera! / distplane!
  IF (distinter! >= 0) AND (distinter! <= distpoint!) THEN
   points(i%).sx = 160 + ((points(i%).x - points(0).x) / distpoint!) * distinter! + points(0).x
   points(i%).sy = 100 - ((points(i%).z - points(0).z) / distpoint!) * distinter! + points(0).z
  END IF
 END IF
NEXT i%
END SUB

SUB transformx (t!)
FOR i% = 1 TO numpoints%
 points(i%).x = points(i%).x + t!
NEXT i%
END SUB

SUB transformy (t!)
FOR i% = 1 TO numpoints%
 points(i%).y = points(i%).y + t!
NEXT i%
END SUB


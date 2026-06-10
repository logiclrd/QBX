'$DYNAMIC

TYPE PointType
  X AS DOUBLE
  Y AS DOUBLE
  Z AS DOUBLE
END TYPE

TYPE VertexType
  VertexIndex AS INTEGER
  NormalIndex AS INTEGER
END TYPE

TYPE FaceType
  P1 AS VertexType
  P2 AS VertexType
  P3 AS VertexType
  Centre AS PointType
  Normal AS PointType
END TYPE

TYPE ShotType
  Active AS INTEGER
  ShotX AS DOUBLE
  ShotY AS DOUBLE
  ShotAngle AS DOUBLE
END TYPE

TYPE DustType
  Position AS PointType
  Colour AS INTEGER
END TYPE

DECLARE SUB AddPoint (t AS PointType, s AS PointType)
DECLARE SUB ApplyTransform (i AS PointType, matrix#(), o AS PointType)
DECLARE SUB DrawReticle (CX%, cy%)
DECLARE SUB FillTriangle (x1%, y1%, x2%, y2%, x3%, y3%, Colour%)
DECLARE SUB GetObjectCounts (objfilename$, numvertices%, numnormals%, numfaces%)
DECLARE SUB HorizontalLine (scanofs&, x1%, x2%, Colour%)
DECLARE SUB InitializeDust (Dusts() AS DustType)
DECLARE SUB LoadObject (objfilename$, vertices() AS PointType, normals() AS PointType, faces() AS FaceType)
DECLARE SUB MultiplyMatrix (subject#(), transform#())
DECLARE SUB MultiplyPoint (t AS PointType, Scale#)
DECLARE SUB NormalizePoint (t AS PointType)
DECLARE SUB ParseTriple (a$, P AS PointType)
DECLARE SUB ParseVertex (a$, v AS VertexType)
DECLARE SUB ParseVertices (a$, f AS FaceType)
DECLARE SUB RenderObject (sourcevertices() AS PointType, sourcenormals() AS PointType, faces() AS FaceType, transform#(), rotation#(), lightposition AS PointType, LightIntensity#, AmbientLightIntensity#, ox%, oy%)
DECLARE SUB RenderShot (shot AS ShotType)
DECLARE SUB RotateTransformX (matrix#(), angle#)
DECLARE SUB RotateTransformY (matrix#(), angle#)
DECLARE SUB RotateTransformZ (matrix#(), angle#)
DECLARE SUB ScaleTransform (matrix#(), factor#)
DECLARE SUB SetIdentityTransform (transform#())
DECLARE SUB SortFaces (faces() AS FaceType, facez#(), first%, last%)
DECLARE SUB SubtractPoint (t AS PointType, s AS PointType)
DECLARE SUB UpdateAndRenderDust (Dusts() AS DustType)
DECLARE SUB UpdateAndRenderShot (shot AS ShotType)
DECLARE FUNCTION Distance# (t AS PointType)
DECLARE FUNCTION DotProduct# (a AS PointType, b AS PointType)
DECLARE FUNCTION RandomDustColour% ()

TYPE RegTypeX
  AX AS INTEGER
  BX AS INTEGER
  CX AS INTEGER
  dx AS INTEGER
  BP AS INTEGER
  SI AS INTEGER
  DI AS INTEGER
  FLAGS AS INTEGER
  DS AS INTEGER
  ES AS INTEGER
END TYPE

DECLARE SUB InterruptX (intnum%, rin AS RegTypeX, rout AS RegTypeX)

DIM rin AS RegTypeX, rout AS RegTypeX

CONST PI# = 3.14159265358979

CONST FighterMomentum# = .8
CONST FighterReticleAttraction# = .6
CONST FighterTiltSpeed# = .06
CONST FighterTiltMax# = .8

CONST ReticleColour% = 14

CONST MaxShots% = 40
CONST ShotStartOffset# = 5
CONST ShotSpeed# = 35
CONST ShotWidth# = 2.2
CONST ShotTailLength# = 15
CONST ShotColour% = 35

CONST CameraZ# = -1000
CONST Shininess# = 4
CONST LightIntensity# = .6
CONST AmbientLightIntensity# = .3

CONST MaxDusts% = 100
CONST DustSpeed# = 435
CONST DustZMin# = -5
CONST DustZMax# = 45
CONST DustXMin# = -160 * (DustZMax# - CameraZ#) / 10
CONST DustXMax# = 160 * (DustZMax# - CameraZ#) / 10
CONST DustYMin# = -100 * (DustZMax# - CameraZ#) / 10
CONST DustYMax# = 100 * (DustZMax# - CameraZ#) / 10 + DustSpeed#
CONST DustYSpawnMin# = 100 * (DustZMax# - CameraZ#) / 10

CONST MarginX% = 18
CONST FighterY% = 180

GetObjectCounts "FIGHTER.OBJ", numvertices%, numnormals%, numfaces%

DIM FighterVertices(1 TO numvertices%) AS PointType
DIM FighterNormals(1 TO numnormals%) AS PointType
DIM FighterFaces(1 TO numfaces%) AS FaceType

DIM FighterLightPosition AS PointType

LoadObject "FIGHTER.OBJ", FighterVertices(), FighterNormals(), FighterFaces()

DIM Shots(1 TO MaxShots%) AS ShotType
DIM Dusts(1 TO MaxDusts%) AS DustType

InitializeDust Dusts()

guncooldown% = 3:  ' frames
gunheat% = 0

DIM transform#(0 TO 3, 0 TO 3)
DIM rotation#(0 TO 3, 0 TO 3) ' same as transform, but without translation

SCREEN 13

DIM buf&(16001)

GET (0, 0)-(319, 199), buf&

DEF SEG = VARSEG(buf&(1))

DIM SHARED bufofs%

bufofs% = VARPTR(buf&(1))

FighterLightPosition.X = -30
FighterLightPosition.Y = -90
FighterLightPosition.Z = CameraZ#

' Reset mouse driver
rin.AX = 0
CALL InterruptX(&H33, rin, rout)

fighterx# = 160
fighterdx# = 0
fightertiltangle# = 0

DO
  ' Get mouse
  rin.AX = 3
  CALL InterruptX(&H33, rin, rout)

  mousebutton% = rout.BX
  mousex% = rout.CX \ 2: ' Mouse driver always reports 0..639
  mousey% = rout.dx

  ' Fighter movement
  targetx% = mousex%
  IF targetx% < MarginX% THEN targetx% = MarginX%
  IF targetx% > 319 - MarginX% THEN targetx% = 319 - MarginX%

  delta# = targetx% - fighterx#

  fighterdx# = fighterdx# * FighterMomentum# + ATN(delta#) * FighterReticleAttraction#
  fighterx# = fighterx# + fighterdx#
  fightertiltangle# = ATN(delta# * FighterTiltSpeed#) * FighterTiltMax#

  ' Shooting
  IF gunheat% > 0 THEN gunheat% = gunheat% - 1

  IF mousebutton% THEN
    FOR shotindex% = 1 TO MaxShots%
      IF Shots(shotindex%).Active = 0 THEN EXIT FOR
    NEXT shotindex%

    IF shotindex% <= MaxShots% THEN
      IF gunheat% = 0 THEN
        gunheat% = guncooldown%

        angle# = PI# * .5 - 12 * SIN(fightertiltangle#) / 57.295779
        cosa# = COS(angle#)
        sina# = SIN(angle#)

        Shots(shotindex%).Active = 1
        Shots(shotindex%).ShotX = fighterx# + cosa# * ShotStartOffset#
        Shots(shotindex%).ShotY = FighterY% - sina# * ShotStartOffset#
        Shots(shotindex%).ShotAngle = angle#
      END IF
    END IF
  END IF

  ' Fighter transform
  SetIdentityTransform transform#()
  SetIdentityTransform rotation#()

  ScaleTransform transform#(), 44

  RotateTransformZ transform#(), -10 * SIN(fightertiltangle#) / 57.295779
  RotateTransformY transform#(), 10 * SIN(fightertiltangle#) / 57.295779
  RotateTransformX transform#(), -100 / 57.295779

  ' Clear buffer
  FOR i% = 1 TO 16000
    buf&(i%) = 0
  NEXT i%

  UpdateAndRenderDust Dusts()


  RenderObject FighterVertices(), FighterNormals(), FighterFaces(), transform#(), rotation#(), FighterLightPosition, LightIntensity#, AmbientLightIntensity#, INT(fighterx#), FighterY%

  FOR i% = 1 TO MaxShots%
    UpdateAndRenderShot Shots(i%)
  NEXT i%

  DrawReticle mousex%, mousey%

  ' Blit
  PUT (0, 0), buf&, PSET
LOOP WHILE INKEY$ = ""

SUB AddPoint (t AS PointType, s AS PointType)
t.X = t.X + s.X
t.Y = t.Y + s.Y
t.Z = t.Z + s.Z
END SUB

SUB ApplyTransform (i AS PointType, matrix#(), o AS PointType)
o.X = i.X * matrix#(0, 0) + i.Y * matrix#(1, 0) + i.Z * matrix#(2, 0) + matrix#(3, 0)
o.Y = i.X * matrix#(0, 1) + i.Y * matrix#(1, 1) + i.Z * matrix#(2, 1) + matrix#(3, 1)
o.Z = i.X * matrix#(0, 2) + i.Y * matrix#(1, 2) + i.Z * matrix#(2, 2) + matrix#(3, 2)
END SUB

FUNCTION Distance# (t AS PointType)
Distance# = SQR(t.X * t.X + t.Y * t.Y + t.Z * t.Z)
END FUNCTION

FUNCTION DotProduct# (a AS PointType, b AS PointType)
DotProduct# = a.X * b.X + a.Y * b.Y + a.Z * b.Z
END FUNCTION

SUB DrawReticle (ox%, oy%)
' ** **
'*     *
'*  *  *
'  ***
'*  *  *
'*     *
' ** **
scanofs& = oy% * 320& + bufofs%

IF oy% >= 3 AND oy% < 203 THEN
  HorizontalLine scanofs& - 3 * 320, ox% - 2, ox% - 1, ReticleColour%
  HorizontalLine scanofs& - 3 * 320, ox% + 1, ox% + 2, ReticleColour%
END IF

IF oy% >= 2 AND oy% < 202 THEN
  HorizontalLine scanofs& - 2 * 320, ox% - 3, ox% - 3, ReticleColour%
  HorizontalLine scanofs& - 2 * 320, ox% + 3, ox% + 3, ReticleColour%
END IF

IF oy% >= 1 AND oy% < 201 THEN
  HorizontalLine scanofs& - 1 * 320, ox% - 3, ox% - 3, ReticleColour%
  HorizontalLine scanofs& - 1 * 320, ox%, ox%, ReticleColour%
  HorizontalLine scanofs& - 1 * 320, ox% + 3, ox% + 3, ReticleColour%
END IF

IF oy% >= 0 AND oy% < 200 THEN
  HorizontalLine scanofs&, ox% - 1, ox% + 1, ReticleColour%
END IF

IF oy% >= -1 AND oy% < 199 THEN
  HorizontalLine scanofs& + 1 * 320, ox% - 3, ox% - 3, ReticleColour%
  HorizontalLine scanofs& + 1 * 320, ox%, ox%, ReticleColour%
  HorizontalLine scanofs& + 1 * 320, ox% + 3, ox% + 3, ReticleColour%
END IF

IF oy% >= -2 AND oy% < 198 THEN
  HorizontalLine scanofs& + 2 * 320, ox% - 3, ox% - 3, ReticleColour%
  HorizontalLine scanofs& + 2 * 320, ox% + 3, ox% + 3, ReticleColour%
END IF

IF oy% >= -3 AND oy% < 197 THEN
  HorizontalLine scanofs& + 3 * 320, ox% - 2, ox% - 1, ReticleColour%
  HorizontalLine scanofs& + 3 * 320, ox% + 1, ox% + 2, ReticleColour%
END IF
END SUB

SUB FillTriangle (x1%, y1%, x2%, y2%, x3%, y3%, Colour%)
' Sort coordinates
IF y1% > y2% THEN
  SWAP x1%, x2%
  SWAP y1%, y2%
END IF
IF y2% > y3% THEN
  SWAP x2%, x3%
  SWAP y2%, y3%
END IF
IF y1% > y2% THEN
  SWAP x1%, x2%
  SWAP y1%, y2%
END IF

o& = y1% * 320& + bufofs%

IF y1% = y3% THEN
  IF y1% >= 0 AND y1% < 200 THEN
    mx% = x1%
    IF x2% < mx% THEN mx% = x2%
    IF x3% < mx% THEN mx% = x3%

    nx% = x1%
    IF x2% > nx% THEN nx% = x2%
    IF x3% > nx% THEN nx% = x3%

    HorizontalLine y1% * 320&, mx%, nx%, Colour%
  END IF
ELSE
  xfp% = 0
  dxfp% = (x3% - x1%) * 256& \ (y3% - y1% + 1)
  xx% = x1%

  IF (y2% > y1%) AND (y2% >= 0) AND (y1% < 200) THEN
    x2fp% = 0
    dx2fp% = (x2% - x1%) * 256& \ (y2% - y1%)
    xx2% = x1%

    FOR yy% = y1% TO y2%
      IF yy% >= 200 THEN EXIT FOR
      IF yy% >= 0 THEN HorizontalLine o&, xx%, xx2%, Colour%

      xfp% = xfp% + dxfp%
      WHILE xfp% < 0: xx% = xx% - 1: xfp% = xfp% + 256: WEND
      WHILE xfp% >= 256: xx% = xx% + 1: xfp% = xfp% - 256: WEND

      x2fp% = x2fp% + dx2fp%
      WHILE x2fp% < 0: xx2% = xx2% - 1: x2fp% = x2fp% + 256: WEND
      WHILE x2fp% >= 256: xx2% = xx2% + 1: x2fp% = x2fp% - 256: WEND

      o& = o& + 320&
    NEXT yy%
  ELSEIF y1% >= 0 AND y1% < 200 THEN
    HorizontalLine o&, xx%, x2%, Colour%

    xfp% = xfp% + dxfp%
    WHILE xfp% < 0: xx% = xx% - 1: xfp% = xfp% + 256: WEND
    WHILE xfp% >= 256: xx% = xx% + 1: xfp% = xfp% - 256: WEND

    o& = o& + 320&
  ELSE
    o& = y2% * 320& + bufofs%
  END IF

  IF y3% > y2% THEN
    x2fp% = 0
    dx2fp% = (x3% - x2%) * 256& \ (y3% - y2% + 1)
    xx2% = x2%

    FOR yy% = y2% + 1 TO y3% - 1
      IF yy% >= 200 THEN EXIT FOR

      x2fp% = x2fp% + dx2fp%
      WHILE x2fp% < 0: xx2% = xx2% - 1: x2fp% = x2fp% + 256: WEND
      WHILE x2fp% >= 256: xx2% = xx2% + 1: x2fp% = x2fp% - 256: WEND

      IF yy% >= 0 THEN HorizontalLine o&, xx%, xx2%, Colour%

      xfp% = xfp% + dxfp%
      WHILE xfp% < 0: xx% = xx% - 1: xfp% = xfp% + 256: WEND
      WHILE xfp% >= 256: xx% = xx% + 1: xfp% = xfp% - 256: WEND

      o& = o& + 320&
    NEXT yy%

    x2fp% = x2fp% + dx2fp%
    WHILE x2fp% < 0: xx2% = xx2% - 1: x2fp% = x2fp% + 256: WEND
    WHILE x2fp% >= 256: xx2% = xx2% + 1: x2fp% = x2fp% - 256: WEND

    IF yy% >= 0 AND yy% < 200 THEN HorizontalLine o&, xx%, xx2%, Colour%
  END IF
END IF
END SUB

SUB GetObjectCounts (objfilename$, numvertices%, numnormals%, numfaces%)
ff% = FREEFILE

OPEN objfilename$ FOR INPUT AS #ff%

numvertices% = 0
numnormals% = 0
numfaces% = 0

DO
  LINE INPUT #ff%, a$

  a$ = LTRIM$(a$)

  IF LEN(a$) >= 2 THEN
    SELECT CASE LEFT$(a$, 2)
      CASE "v ": numvertices% = numvertices% + 1
      CASE "vn": numnormals% = numnormals% + 1
      CASE "f ": numfaces% = numfaces% + 1
    END SELECT
  END IF
LOOP UNTIL EOF(ff%)

CLOSE #ff%
END SUB

SUB HorizontalLine (scanofs&, x1%, x2%, Colour%)
IF x1% <= x2% THEN
  IF x1% < 0 THEN x1% = 0
  IF x2% > 319 THEN x2% = 319
  FOR xx% = x1% TO x2%
    POKE scanofs& + xx%, Colour%
  NEXT xx%
ELSE
  IF x2% < 0 THEN x2% = 0
  IF x1% > 319 THEN x1% = 319
  FOR xx% = x2% TO x1%
    POKE scanofs& + xx%, Colour%
  NEXT xx%
END IF
END SUB

SUB InitializeDust (Dusts() AS DustType)
FOR i% = 1 TO MaxDusts%
  Dusts(i%).Position.X = RND * (DustXMax# - DustXMin#) + DustXMin#
  Dusts(i%).Position.Y = RND * (DustYMax# - DustYMin#) + DustYMin#
  Dusts(i%).Position.Z = RND * (DustZMax# - DustZMin#) + DustZMin#
  Dusts(i%).Colour = RandomDustColour%
NEXT i%
END SUB

REM $STATIC
SUB LoadObject (objfilename$, vertices() AS PointType, normals() AS PointType, faces() AS FaceType)
ff% = FREEFILE

OPEN objfilename$ FOR INPUT AS #ff%

numvertices% = 0
numnormals% = 0
numfaces% = 0

DO
  LINE INPUT #ff%, a$

  a$ = LTRIM$(a$)

  IF LEN(a$) >= 2 THEN
    SELECT CASE LEFT$(a$, 2)
      CASE "v ":
        numvertices% = numvertices% + 1
        ParseTriple MID$(a$, 3), vertices(numvertices%)
      CASE "vn":
        numnormals% = numnormals% + 1
        ParseTriple MID$(a$, 4), normals(numnormals%)
      CASE "f ":
        numfaces% = numfaces% + 1
        ParseVertices MID$(a$, 3), faces(numfaces%)
    END SELECT
  END IF
LOOP UNTIL EOF(ff%)

CLOSE #ff%
END SUB

REM $DYNAMIC
SUB MultiplyMatrix (subject#(), transform#())
DIM result#(0 TO 3, 0 TO 3)

FOR i% = 0 TO 3
  FOR j% = 0 TO 3
    FOR k% = 0 TO 3
      result#(i%, j%) = result#(i%, j%) + subject#(i%, k%) * transform#(k%, j%)
    NEXT k%
  NEXT j%
NEXT i%

FOR i% = 0 TO 3
  FOR j% = 0 TO 3
    subject#(i%, j%) = result#(i%, j%)
  NEXT j%
NEXT i%
END SUB

SUB MultiplyPoint (t AS PointType, Scale#)
t.X = t.X * Scale#
t.Y = t.Y * Scale#
t.Z = t.Z * Scale#
END SUB

SUB NormalizePoint (t AS PointType)
d# = Distance#(t)

t.X = t.X / d#
t.Y = t.Y / d#
t.Z = t.Z / d#
END SUB

SUB ParseTriple (a$, P AS PointType)
a$ = LTRIM$(a$)
i% = INSTR(a$, " ")

IF i% = 0 THEN
  P.X = VAL(a$)
ELSE
  P.X = VAL(LEFT$(a$, i% - 1))

  a$ = LTRIM$(MID$(a$, i% + 1))
  i% = INSTR(a$, " ")

  IF i% = 0 THEN
    P.Y = VAL(a$)
  ELSE
    P.Y = VAL(LEFT$(a$, i% - 1))

    a$ = LTRIM$(MID$(a$, i% + 1))
    i% = INSTR(a$, " ")

    IF i% = 0 THEN
      P.Z = VAL(a$)
    ELSE
      P.Z = VAL(LEFT$(a$, i% - 1))
    END IF
  END IF
END IF
END SUB

SUB ParseVertex (a$, v AS VertexType)
v.NormalIndex = 0

i% = INSTR(a$, "/")

IF i% = 0 THEN
  v.VertexIndex = VAL(a$)
ELSE
  v.VertexIndex = VAL(LEFT$(a$, i% - 1))

  a$ = MID$(a$, i% + 1)

  IF ASC(a$) = 47 THEN
    a$ = MID$(a$, 2)
    v.NormalIndex = VAL(a$)
  END IF
END IF
END SUB

SUB ParseVertices (a$, f AS FaceType)
a$ = LTRIM$(a$)
i% = INSTR(a$, " ")

IF i% = 0 THEN
  ParseVertex a$, f.P1
ELSE
  ParseVertex LEFT$(a$, i% - 1), f.P1

  a$ = LTRIM$(MID$(a$, i% + 1))
  i% = INSTR(a$, " ")

  IF i% = 0 THEN
    ParseVertex a$, f.P2
  ELSE
    ParseVertex LEFT$(a$, i% - 1), f.P2

    a$ = LTRIM$(MID$(a$, i% + 1))
    i% = INSTR(a$, " ")

    IF i% = 0 THEN
      ParseVertex a$, f.P3
    ELSE
      ParseVertex LEFT$(a$, i% - 1), f.P3
    END IF
  END IF
END IF
END SUB

FUNCTION RandomDustColour%
r# = RND

IF r# < .05 THEN
  RandomDustColour% = 6
ELSEIF r# < .1 THEN
  RandomDustColour% = 4
ELSEIF r# < .15 THEN
  RandomDustColour% = 11
ELSE
  c% = r# * 16
  IF c% > 15 THEN c% = 15
  RandomDustColour% = 16 + c%
END IF
END FUNCTION

SUB RenderObject (sourcevertices() AS PointType, sourcenormals() AS PointType, faces() AS FaceType, transform#(), rotation#(), lightposition AS PointType, LightIntensity#, AmbientLightIntensity#, ox%, oy%)
' Transform vertices into place
DIM vertices(LBOUND(sourcevertices) TO UBOUND(sourcevertices)) AS PointType

FOR i% = LBOUND(sourcevertices) TO UBOUND(sourcevertices)
  ApplyTransform sourcevertices(i%), transform#(), vertices(i%)
NEXT i%

' Transform normals
DIM normals(LBOUND(sourcenormals) TO UBOUND(sourcenormals)) AS PointType

FOR i% = LBOUND(sourcenormals) TO UBOUND(sourcenormals)
  ApplyTransform sourcenormals(i%), rotation#(), normals(i%)
NEXT i%

' Sort faces
DIM facez#(LBOUND(faces) TO UBOUND(faces))

FOR i% = LBOUND(faces) TO UBOUND(faces)
  z1# = vertices(faces(i%).P1.VertexIndex).Z
  z2# = vertices(faces(i%).P2.VertexIndex).Z
  z3# = vertices(faces(i%).P3.VertexIndex).Z

  IF z1# < z2# THEN
    IF z1# < z3# THEN facez#(i%) = z1# ELSE facez#(i%) = z3#
  ELSE
    IF z2# < z3# THEN facez#(i%) = z2# ELSE facez#(i%) = z3#
  END IF

  faces(i%).Centre = vertices(faces(i%).P1.VertexIndex)
  AddPoint faces(i%).Centre, normals(faces(i%).P2.VertexIndex)
  AddPoint faces(i%).Centre, normals(faces(i%).P3.VertexIndex)

  faces(i%).Normal = normals(faces(i%).P1.NormalIndex)
  AddPoint faces(i%).Normal, normals(faces(i%).P2.NormalIndex)
  AddPoint faces(i%).Normal, normals(faces(i%).P3.NormalIndex)

  NormalizePoint faces(i%).Normal
NEXT i%

SortFaces faces(), facez#(), LBOUND(faces), UBOUND(faces)

' Project vertices - from this point on, vertex (X, Y) is screen
FOR i% = LBOUND(vertices) TO UBOUND(vertices)
  dz# = vertices(i%).Z - CameraZ#
  dzf# = 10 / dz#

  vertices(i%).X = ox% + vertices(i%).X * dzf#
  vertices(i%).Y = oy% - vertices(i%).Y * dzf#
NEXT i%

' Render
DIM cameraposition AS PointType

cameraposition.X = 0
cameraposition.Y = 0
cameraposition.Z = -CameraZ#

DIM tolight AS PointType
DIM tocamera AS PointType
DIM reflection AS PointType

FOR i% = UBOUND(faces) TO LBOUND(faces) STEP -1
  p1i% = faces(i%).P1.VertexIndex
  p2i% = faces(i%).P2.VertexIndex
  p3i% = faces(i%).P3.VertexIndex

  x1# = vertices(p1i%).X
  y1# = vertices(p1i%).Y
  x2# = vertices(p2i%).X
  y2# = vertices(p2i%).Y
  x3# = vertices(p3i%).X
  y3# = vertices(p3i%).Y

  cross1# = x1# * y2# - y1# * x2#
  cross2# = x2# * y3# - y2# * x3#
  cross3# = x3# * y1# - y3# * x1#

  testwinding# = cross1# + cross2# + cross3#

  ' Only draw triangles that are facing the screen
  IF testwinding# > 0 THEN
    ' Lighting
    tolight = lightposition
    SubtractPoint tolight, faces(i%).Centre
    NormalizePoint tolight

    tocamera = cameraposition
    SubtractPoint tocamera, faces(i%).Centre
    NormalizePoint tocamera

    reflection = faces(i%).Normal
    MultiplyPoint reflection, 2 * DotProduct#(faces(i%).Normal, tolight)
    SubtractPoint reflection, tolight
    NormalizePoint reflection

    diffuse# = DotProduct(tolight, faces(i%).Normal)
    specular# = DotProduct(reflection, tocamera)

    IF diffuse# < 0 THEN diffuse# = 0
    IF specular# < 0 THEN specular# = 0 ELSE specular# = specular# ^ Shininess#

    light# = AmbientLightIntensity# + LightIntensity# * (diffuse# + specular#)

    ' Paint
    x1% = INT(x1#)
    y1% = INT(y1#)
    x2% = INT(x2#)
    y2% = INT(y2#)
    x3% = INT(x3#)
    y3% = INT(y3#)
    c% = INT(light# * 16) + 16

    IF c% < 16 THEN c% = 16
    IF c% > 31 THEN c% = 31

    FillTriangle x1%, y1%, x2%, y2%, x3%, y3%, c%
  END IF
NEXT i%
END SUB

REM $STATIC
SUB RotateTransformX (matrix#(), angle#)
DIM transform#(0 TO 3, 0 TO 3)

ca# = COS(angle#)
sa# = SIN(angle#)

transform#(0, 0) = 1
transform#(1, 1) = ca#
transform#(1, 2) = sa#
transform#(2, 1) = -sa#
transform#(2, 2) = ca#
transform#(3, 3) = 1

MultiplyMatrix matrix#(), transform#()
END SUB

SUB RotateTransformY (matrix#(), angle#)
DIM transform#(0 TO 3, 0 TO 3)

ca# = COS(angle#)
sa# = SIN(angle#)

transform#(1, 1) = 1
transform#(0, 0) = ca#
transform#(2, 0) = sa#
transform#(0, 2) = -sa#
transform#(2, 2) = ca#
transform#(3, 3) = 1

MultiplyMatrix matrix#(), transform#()
END SUB

SUB RotateTransformZ (matrix#(), angle#)
DIM transform#(0 TO 3, 0 TO 3)

ca# = COS(angle#)
sa# = SIN(angle#)

transform#(2, 2) = 1
transform#(0, 0) = ca#
transform#(0, 1) = sa#
transform#(1, 0) = -sa#
transform#(1, 1) = ca#
transform#(3, 3) = 1

MultiplyMatrix matrix#(), transform#()
END SUB

SUB ScaleTransform (matrix#(), factor#)
FOR i% = 0 TO 3
  FOR j% = 0 TO 3
    matrix#(i%, j%) = matrix#(i%, j%) * factor#
  NEXT j%
NEXT i%
END SUB

SUB SetIdentityTransform (transform#())
FOR i% = 0 TO 2
  FOR j% = 0 TO 2
    transform#(i%, j%) = ABS(i% = j%)
  NEXT j%
NEXT i%
END SUB

REM $DYNAMIC
SUB SortFaces (faces() AS FaceType, facez#(), first%, last%)
' Try to avoid pathological cases
r% = last% - first% + 1

s1% = INT(RND * r%) + first%
s2% = INT(RND * r%) + first%
s3% = INT(RND * r%) + first%

IF facez#(s1%) > facez#(s2%) THEN
  SWAP facez#(s1%), facez#(s2%)
  SWAP faces(s1%), faces(s2%)
END IF
IF facez#(s2%) > facez#(s3%) THEN
  SWAP facez#(s2%), facez#(s3%)
  SWAP faces(s2%), faces(s3%)
END IF
IF facez#(s1%) > facez#(s2%) THEN
  SWAP facez#(s1%), facez#(s2%)
  SWAP faces(s1%), faces(s2%)
END IF

' Put the median of the 3 randomly selected values at the
' start as the initial pivot.
SWAP facez#(s2%), facez#(first%)
SWAP faces(s2%), faces(first%)

' Pivot
pivot% = first%
FOR i% = first% + 1 TO last%
  IF facez#(i%) < facez#(pivot%) THEN
    SWAP facez#(pivot%), facez#(pivot% + 1)
    SWAP faces(pivot%), faces(pivot% + 1)

    IF i% <> pivot% + 1 THEN
      SWAP facez#(pivot%), facez#(i%)
      SWAP faces(pivot%), faces(i%)

      i% = i% - 1
    END IF

    pivot% = pivot% + 1
  END IF
NEXT i%

' Recurse
IF pivot% - first% >= 2 THEN SortFaces faces(), facez#(), first%, pivot% - 1
IF last% - pivot% >= 2 THEN SortFaces faces(), facez#(), pivot% + 1, last%
END SUB

SUB SubtractPoint (t AS PointType, s AS PointType)
t.X = t.X - s.X
t.Y = t.Y - s.Y
t.Z = t.Z - s.Z
END SUB

SUB Translate (matrix#(), dx#, dy#, dz#)
matrix#(3, 0) = matrix#(3, 0) + dx#
matrix#(3, 1) = matrix#(3, 1) + dy#
matrix#(3, 2) = matrix#(3, 2) + dz#
END SUB

SUB UpdateAndRenderDust (Dusts() AS DustType)
FOR i% = 1 TO MaxDusts%
  Dusts(i%).Position.Y = Dusts(i%).Position.Y - DustSpeed#

  IF Dusts(i%).Position.Y < DustYMin# THEN
    Dusts(i%).Position.X = RND * (DustXMax# - DustXMin#) + DustXMin#
    Dusts(i%).Position.Y = RND * (DustYMax# - DustYSpawnMin#) + DustYSpawnMin#
    Dusts(i%).Position.Z = RND * (DustZMax# - DustZMin#) + DustZMin#
    Dusts(i%).Colour = RandomDustColour%
  END IF

  dz# = Dusts(i%).Position.Z - CameraZ#
  idz# = 10 / dz#

  sx% = 160 + Dusts(i%).Position.X * idz#
  sy% = 100 - Dusts(i%).Position.Y * idz#

  IF sx% >= 0 AND sx% < 320 THEN
    IF sy% >= 0 AND sy% < 200 THEN
      o& = sy% * 320& + sx%
      POKE o& + bufofs%, Dusts(i%).Colour
    END IF
  END IF
NEXT i%
END SUB

SUB UpdateAndRenderShot (shot AS ShotType)
IF shot.Active THEN
  dx# = COS(shot.ShotAngle)
  dy# = SIN(shot.ShotAngle)

  shot.ShotX = shot.ShotX + dx# * ShotSpeed#
  shot.ShotY = shot.ShotY - dy# * ShotSpeed#

  x1# = shot.ShotX - dy# * (ShotWidth# * .5)
  y1# = shot.ShotY - dx# * (ShotWidth# * .5)

  x2# = shot.ShotX + dy# * (ShotWidth# * .5)
  y2# = shot.ShotY + dx# * (ShotWidth# * .5)

  x3# = shot.ShotX - dx# * ShotTailLength#
  y3# = shot.ShotY + dy# * ShotTailLength#

  x1% = INT(x1#)
  y1% = INT(y1#)
  x2% = INT(x2#)
  y2% = INT(y2#)
  x3% = INT(x3#)
  y3% = INT(y3#)

  IF y3% < 0 THEN
    shot.Active = 0
  ELSE
    FillTriangle x1%, y1%, x2%, y2%, x3%, y3%, ShotColour%
  END IF
END IF
END SUB

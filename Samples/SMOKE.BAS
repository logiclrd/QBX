'$DYNAMIC

TYPE LogParameters
  CentreX AS INTEGER
  CentreY AS INTEGER
  LogWidth AS INTEGER
  LogRadius AS INTEGER
END TYPE

TYPE SmokeParticle
  PX AS DOUBLE
  PY AS DOUBLE
  PZ AS DOUBLE
  DPX AS DOUBLE
  DPY AS DOUBLE
  DPZ AS DOUBLE
  ProjectedX AS INTEGER
  ProjectedY AS INTEGER
  IsActive AS INTEGER
END TYPE

CONST SmokeCount% = 100

DECLARE SUB InitializeMesh (meshx#(), meshy#(), meshz#())
DECLARE SUB SetPalette ()

DECLARE SUB Interp (mesh#(), x#, y#, z#, result#)

DECLARE SUB DrawLog (params AS LogParameters, meshx#(), meshy#(), meshz#())
DECLARE SUB SparkEmber (params AS LogParameters, meshx#(), meshy#(), meshz#())
DECLARE SUB SpreadEmber (params AS LogParameters)

DECLARE SUB PutSmoke (smoke AS SmokeParticle)
DECLARE SUB UnputSmoke (smoke AS SmokeParticle)
DECLARE SUB MoveSmoke (smoke() AS SmokeParticle, meshx#(), meshy#(), meshz#(), phase#)
DECLARE SUB GenerateSmoke (wood AS LogParameters, smoke() AS SmokeParticle)

CONST PI# = 3.14159265358979#

DIM meshx#(-15 TO 15, -15 TO 15, -10 TO 10)
DIM meshy#(-15 TO 15, -15 TO 15, -10 TO 10)
DIM meshz#(-15 TO 15, -15 TO 15, -10 TO 10)

InitializeMesh meshx#(), meshy#(), meshz#()

DIM wood AS LogParameters

wood.CentreX = 160
wood.CentreY = 180
wood.LogWidth = 75
wood.LogRadius = 6

DIM smoke(SmokeCount%) AS SmokeParticle

SCREEN 13

DIM UnputTable%(255), PutTable%(255)

FOR i% = 0 TO 255
  smork% = i% \ 32
  behind% = i% AND 31

  unputsmork% = smork% - 1
  putsmork% = smork% + 1

  IF unputsmork% < 0 THEN unputsmork% = 0
  IF putsmork% > 7 THEN putsmork% = 7

  UnputTable%(i%) = unputsmork% * 32 + behind%
  PutTable%(i%) = putsmork% * 32 + behind%
NEXT i%

SetPalette

DrawLog wood, meshx#(), meshy#(), meshz#()

SparkEmber wood, meshx#(), meshy#(), meshz#()

DO
  SpreadEmber wood

  GenerateSmoke wood, smoke()

  MoveSmoke smoke(), meshx#(), meshy#(), meshz#(), phase#

  phase# = phase# + .1
LOOP WHILE INKEY$ = ""


SUB DrawLog (params AS LogParameters, meshx#(), meshy#(), meshz#())
x% = params.CentreX
y% = params.CentreY
w% = params.LogWidth
radius% = params.LogRadius

hw# = w% * .5

' Back
FOR rr% = radius% TO 0 STEP -1
  sliceheight% = SQR(radius% * radius% - rr% * rr%)

  z# = -rr%

  FOR yy% = -sliceheight% TO sliceheight%
    x1# = -hw#
    x2# = x1# + w%

    bc% = INT(RND * 5 + 1)

    FOR t% = 0 TO 10
      t# = t% / 10
      it# = 1 - t#

      x# = x1# * it# + x2# * t#

      dx# = x# * .2
      dy# = yy% * .2
      dz# = z# * .2

      Interp meshx#(), dx#, dy#, dz#, dispx#
      Interp meshy#(), dx#, dy#, dz#, dispy#
      Interp meshz#(), dx#, dy#, dz#, dispz#

      sx% = x% + x# + dispx# + (z# + dispz#) * .33
      sy% = y% + yy% + dispy#

      IF t% = 0 THEN
        PSET (sx%, sy%), bc%
      ELSE
        LINE -(sx%, sy%), bc% + INT(RND * 3)
      END IF
    NEXT t%
  NEXT yy%
NEXT rr%

' Front
FOR rr% = 0 TO radius%
  sliceheight% = SQR(radius% * radius% - rr% * rr%)

  z# = rr%

  FOR yy% = -sliceheight% TO sliceheight%
    x1# = -hw# + rr% * .33
    x2# = x1# + w%

    bc% = INT(RND * 5 + 1)
    abc% = 0

    FOR t% = 0 TO 30
      t# = t% / 30
      it# = 1 - t#

      x# = x1# * it# + x2# * t#

      dx# = x# * .2
      dy# = yy% * .2
      dz# = z# * .2

      Interp meshx#(), dx#, dy#, dz#, dispx#
      Interp meshy#(), dx#, dy#, dz#, dispy#
      Interp meshz#(), dx#, dy#, dz#, dispz#

      sx% = x% + x# + dispx# + (z# + dispz#) * .33
      sy% = y% + yy% + dispy#

      IF t% = 0 THEN
        PSET (sx%, sy%), bc%
      ELSE
        LINE -(sx%, sy%), bc% + abc%
      END IF

      IF RND < .1 THEN abc% = INT(RND * 3)
    NEXT t%
  NEXT yy%
NEXT rr%

' Exposed wood
xc# = -hw#
yc# = 0

x# = xc#

FOR rr# = .1 TO radius% - 1.5 STEP .2
  a1# = RND * (PI# * 2)
  a2# = a1# + RND * (PI# * 2)

  circumference% = 2 * PI# * rr#

  icircumference# = 1 / circumference%

  c% = 15 - (rr# / radius%) * 4 - INT(RND * RND * 4)

  FOR i% = 0 TO circumference% * (a2# - a1#) / (PI# * 2)
    a# = (a1# * (circumference% - i%) + a2# * i%) * icircumference#

    y# = yc# + rr# * SIN(a#)
    z# = rr# * COS(a#)

    dx# = x# * .2
    dy# = y# * .2
    dz# = z# * .2

    Interp meshx#(), dx#, dy#, dz#, dispx#
    Interp meshy#(), dx#, dy#, dz#, dispy#
    Interp meshz#(), dx#, dy#, dz#, dispz#

    sx% = x% + x# + dispx# + (z# + dispz# * .33)
    sy% = y% + y# + dispy#

    PSET (sx%, sy%), c%
  NEXT i%
NEXT rr#
END SUB

SUB GenerateSmoke (wood AS LogParameters, smoke() AS SmokeParticle)
STATIC lastscanindex%

IF lastscanindex% < LBOUND(smoke) THEN lastscanindex% = LBOUND(smoke)

FOR i% = LBOUND(smoke) TO UBOUND(smoke)
  IF smoke(lastscanindex%).IsActive = 0 THEN i% = lastscanindex%: EXIT FOR

  lastscanindex% = lastscanindex% + 1
  IF lastscanindex% > UBOUND(smoke) THEN lastscanindex% = LBOUND(smoke)
NEXT i%

IF i% <= UBOUND(smoke) THEN
  x% = wood.CentreX
  y% = wood.CentreY
  w% = wood.LogWidth
  radius% = wood.LogRadius

  hw% = w% \ 2 + 2

  x1% = x% - hw% - radius% * .5: y1% = y% - radius% - 1
  x2% = x% + hw% + radius% * .5: y2% = y% + radius% + 1

  x% = x1% + (x2% - x1%) * RND
  y% = y1% + (y2% - y1%) * RND

  IF (POINT(x%, y%) AND 31) > 18 THEN
    smoke(i%).PX = x%
    smoke(i%).PY = y%
    smoke(i%).PZ = (RND - .5) * wood.LogRadius * 2
    smoke(i%).DPX = (RND - .5) * .2
    smoke(i%).DPY = -.1 * (RND + .3)
    smoke(i%).DPZ = (RND - .2) * .1
    smoke(i%).ProjectedX = -1
    smoke(i%).IsActive = -1
  END IF
END IF
END SUB

SUB InitializeMesh (meshx#(), meshy#(), meshz#())
FOR xx% = LBOUND(meshx#, 1) TO UBOUND(meshx#, 1)
  FOR yy% = LBOUND(meshx#, 2) TO UBOUND(meshx#, 2)
    FOR zz% = LBOUND(meshx#, 3) TO UBOUND(meshx#, 3)
      meshx#(xx%, yy%, zz%) = RND * 3 - 1.5
      meshy#(xx%, yy%, zz%) = RND * 3 - 1.5
      meshz#(xx%, yy%, zz%) = RND * 3 - 1.5
    NEXT zz%
  NEXT yy%
NEXT xx%
END SUB

SUB Interp (mesh#(), x#, y#, z#, result#)
x1% = INT(x#)
y1% = INT(y#)
z1% = INT(z#)
x2% = x1% + 1
y2% = y1% + 1
z2% = z1% + 1

dx# = x# - x1%: idx# = 1 - dx#
dy# = y# - y1%: idy# = 1 - dy#
dz# = z# - z1%: idz# = 1 - dz#

result# = 0

result# = result# + mesh#(x1%, y1%, z1%) * idx# * idy# * idz#
result# = result# + mesh#(x1%, y1%, z2%) * idx# * idy# * dz#
result# = result# + mesh#(x1%, y2%, z1%) * idx# * dy# * idz#
result# = result# + mesh#(x1%, y2%, z2%) * idx# * dy# * dz#
result# = result# + mesh#(x2%, y1%, z1%) * dx# * idy# * idz#
result# = result# + mesh#(x2%, y1%, z2%) * dx# * idy# * dz#
result# = result# + mesh#(x2%, y2%, z1%) * dx# * dy# * idz#
result# = result# + mesh#(x2%, y2%, z2%) * dx# * dy# * dz#
END SUB

SUB MoveSmoke (smoke() AS SmokeParticle, meshx#(), meshy#(), meshz#(), phase#)
FOR i% = LBOUND(smoke) TO UBOUND(smoke)
  UnputSmoke smoke(i%)

  IF smoke(i%).IsActive THEN
    x# = (smoke(i%).PX - 160) * (1 / 10) + SIN(phase#)
    y# = (smoke(i%).PY - 100) * (1 / 10) + COS(phase#)
    z# = smoke(i%).PZ + SIN(phase# * 2) + COS(phase# * 2)

    IF x# < -10 THEN x# = -10
    IF x# > 10 THEN x# = 10

    IF y# < -10 THEN y# = -10
    IF y# > 10 THEN y# = 10

    IF z# < -10 THEN z# = -10
    IF z# > 10 THEN z# = 10

    Interp meshx#(), dx#, dy#, dz#, dispx#
    Interp meshy#(), dx#, dy#, dz#, dispy#
    Interp meshz#(), dx#, dy#, dz#, dispz#

    dispx# = ((dispx# * .4) ^ 4) * .005
    dispy# = ((dispy# * .4) ^ 4) * .005
    dispz# = ((dispz# * .4) ^ 4) * .005

    IF dispy# > .0009 THEN dispy# = .0009

    smoke(i%).DPX = smoke(i%).DPX + dispx#
    smoke(i%).DPY = smoke(i%).DPY + dispy# - .001
    smoke(i%).DPZ = smoke(i%).DPZ + dispz#

    smoke(i%).PX = smoke(i%).PX + smoke(i%).DPX
    smoke(i%).PY = smoke(i%).PY + smoke(i%).DPY
    smoke(i%).PZ = smoke(i%).PZ + smoke(i%).DPZ

    projx% = smoke(i%).PX + smoke(i%).PZ * .33
    projy% = smoke(i%).PY

    IF (projy% < -8) OR (projy% > 300) THEN
      smoke(i%).IsActive = 0
    ELSEIF (projx% < -50) OR (projx% > 370) THEN
      smoke(i%).IsActive = 0
    ELSE
      smoke(i%).ProjectedX = projx%
      smoke(i%).ProjectedY = projy%

      PutSmoke smoke(i%)
    END IF
  END IF
NEXT i%
END SUB

SUB PutSmoke (smoke AS SmokeParticle)
SHARED PutTable%()

x% = smoke.ProjectedX
y% = smoke.ProjectedY

'   ###
'  #####
'  #####
'  #####
'   ###

IF x% >= 0 THEN
  PSET (x% - 1, y% - 2), PutTable%(POINT(x% - 1, y% - 2))
  PSET (x%, y% - 2), PutTable%(POINT(x%, y% - 2))
  PSET (x% + 1, y% - 2), PutTable%(POINT(x% + 1, y% - 2))

  PSET (x% - 2, y% - 1), PutTable%(POINT(x% - 2, y% - 1))
  PSET (x% - 1, y% - 1), PutTable%(POINT(x% - 1, y% - 1))
  PSET (x%, y% - 1), PutTable%(POINT(x%, y% - 1))
  PSET (x% + 1, y% - 1), PutTable%(POINT(x% + 1, y% - 1))
  PSET (x% + 2, y% - 1), PutTable%(POINT(x% + 2, y% - 1))

  PSET (x% - 2, y%), PutTable%(POINT(x% - 2, y%))
  PSET (x% - 1, y%), PutTable%(POINT(x% - 1, y%))
  PSET (x%, y%), PutTable%(POINT(x%, y%))
  PSET (x% + 1, y%), PutTable%(POINT(x% + 1, y%))
  PSET (x% + 2, y%), PutTable%(POINT(x% + 2, y%))

  PSET (x% - 2, y% + 1), PutTable%(POINT(x% - 2, y% + 1))
  PSET (x% - 1, y% + 1), PutTable%(POINT(x% - 1, y% + 1))
  PSET (x%, y% + 1), PutTable%(POINT(x%, y% + 1))
  PSET (x% + 1, y% + 1), PutTable%(POINT(x% + 1, y% + 1))
  PSET (x% + 2, y% + 1), PutTable%(POINT(x% + 2, y% + 1))

  PSET (x% - 1, y% + 2), PutTable%(POINT(x% - 1, y% + 2))
  PSET (x%, y% + 2), PutTable%(POINT(x%, y% + 2))
  PSET (x% + 1, y% + 2), PutTable%(POINT(x% + 1, y% + 2))
END IF
END SUB

SUB SetPalette
DIM basepaletter%(31)
DIM basepaletteg%(31)
DIM basepaletteb%(31)

' Bark

' Gray to dark brown
r1% = 45: g1% = 45: b1% = 45
r2% = 24: g2% = 12: b2% = 10

FOR t% = 1 TO 4
  t# = (t% - 1) / 3
  it# = 1 - t#

  basepaletter%(t%) = r1% * it# + r2% * t#
  basepaletteg%(t%) = g1% * it# + g2% * t#
  basepaletteb%(t%) = b1% * it# + b2% * t#
NEXT t%

' Dark brown to medium brown
r1% = r2%: g1% = g2%: b1% = b2%
r2% = 42: g2% = 23: b2% = 14

FOR t% = 5 TO 7
  t# = (t% - 4) / 3
  it# = 1 - t#

  basepaletter%(t%) = r1% * it# + r2% * t#
  basepaletteg%(t%) = g1% * it# + g2% * t#
  basepaletteb%(t%) = b1% * it# + b2% * t#
NEXT t%

' Bare wood

' Slightly gray to rich brown
r1% = 45: g1% = 28: b1% = 17
r2% = 54: g2% = 30: b2% = 13

FOR t% = 8 TO 11
  t# = (t% - 8) / 3
  it# = 1 - t#

  basepaletter%(t%) = r1% * it# + r2% * t#
  basepaletteg%(t%) = g1% * it# + g2% * t#
  basepaletteb%(t%) = b1% * it# + b2% * t#
NEXT t%

' Rich brown to pale beige
r1% = r2%: g1% = g2%: b1% = b2%
r2% = 54: g2% = 44: b2% = 38

FOR t% = 12 TO 15
  t# = (t% - 11) / 4
  it# = 1 - t#

  basepaletter%(t%) = r1% * it# + r2% * t#
  basepaletteg%(t%) = g1% * it# + g2% * t#
  basepaletteb%(t%) = b1% * it# + b2% * t#
NEXT t%

' Embers
r2% = 42: g2% = 17: b2% = 14

FOR t% = 16 TO 20
  r% = g2% * (18.5 - t%) / 2.5
  g% = g2% * (18.5 - t%) / 2.5
  b% = b2% * (18.5 - t%) / 2.5

  IF r% < 0 THEN r% = 0
  IF g% < 0 THEN g% = 0
  IF b% < 0 THEN b% = 0

  r% = r% + (t% - 16) * 12

  basepaletter%(t%) = r%
  basepaletteg%(t%) = g%
  basepaletteb%(t%) = b%
NEXT t%

FOR t% = 21 TO 25
  basepaletter%(t%) = (25 - t%) * 10
  basepaletteg%(t%) = 0
  basepaletteb%(t%) = 0
NEXT t%

FOR t% = 26 TO 31
  i% = (t% - 26) * 5
  basepaletter%(t%) = i%
  basepaletteg%(t%) = i%
  basepaletteb%(t%) = i%
NEXT t%

OUT &H3C8, 0

FOR smork% = 0 TO 7
  smorkt# = smork% / 8
  smorkit# = 1 - smorkt#

  FOR t% = 0 TO 31
    OUT &H3C9, basepaletter%(t%) * smorkit# + 40 * smorkt#
    OUT &H3C9, basepaletteg%(t%) * smorkit# + 40 * smorkt#
    OUT &H3C9, basepaletteb%(t%) * smorkit# + 40 * smorkt#
  NEXT t%
NEXT smork%
END SUB

SUB SparkEmber (params AS LogParameters, meshx#(), meshy#(), meshz#())
x% = params.CentreX
y% = params.CentreY
w% = params.LogWidth
radius% = params.LogRadius

hw# = w% * .5

xc# = -hw#
yc# = 0

r# = RND * RND * radius%
a# = RND * (PI# * 2)

x# = xc#
y# = yc# + rr# * SIN(a#)
z# = rr# * COS(a#)

dx# = x# * .2
dy# = y# * .2
dz# = z# * .2

Interp meshx#(), dx#, dy#, dz#, dispx#
Interp meshy#(), dx#, dy#, dz#, dispy#
Interp meshz#(), dx#, dy#, dz#, dispz#

sx% = x% + x# + dispx# + (z# + dispz# * .33)
sy% = y% + y# + dispy#

PSET (sx%, sy%), 16
END SUB

SUB SpreadEmber (params AS LogParameters)
x% = params.CentreX
y% = params.CentreY
w% = params.LogWidth
radius% = params.LogRadius

hw% = w% \ 2 + 2

x1% = x% - hw% - radius% * .5: y1% = y% - radius% - 1
x2% = x% + hw% + radius% * .5: y2% = y% + radius% + 1

scanw% = x2% - x1% + 1
scanh% = y2% - y1% + 1

DIM scanc%(scanw% - 1, scanh% - 1)
DIM scansmork%(scanw% - 1, scanh% - 1)

FOR xx% = x1% TO x2%
  FOR yy% = y1% TO y2%
    c% = POINT(xx%, yy%)

    scanc%(xx% - x1%, yy% - y1%) = c% AND 31
    scansmork%(xx% - x1%, yy% - y1%) = c% AND 224
  NEXT yy%
NEXT xx%

FOR xx% = x1% TO x2%
  sx% = xx% - x1%

  FOR yy% = y1% TO y2%
    sy% = yy% - y1%

    IF scanc%(sx%, sy%) >= 16 THEN
      IF scanc%(sx%, sy%) < 25 THEN
        IF RND < .01 THEN PSET (xx%, yy%), scansmork%(sx%, sy%) + scanc%(sx%, sy%) + 1
      ELSEIF scanc%(sx%, sy%) < 27 THEN
        IF RND < .0005 THEN PSET (xx%, yy%), scansmork%(sx%, sy%) + scanc%(sx%, sy%) + 1
      ELSEIF scanc%(sx%, sy%) < 30 THEN
        IF RND < .002 THEN PSET (xx%, yy%), scansmork%(sx%, sy%) + scanc%(sx%, sy%) + 1
      END IF

      spread% = (scanc%(sx%, sy%) - 16) \ 3

      FOR i% = 1 TO spread%
        SELECT CASE INT(RND * 8)
          CASE 0: dx% = 1: dy% = 0
          CASE 1: dx% = 1: dy% = 1
          CASE 2: dx% = 0: dy% = 1
          CASE 3: dx% = -1: dy% = 1
          CASE 4: dx% = -1: dy% = 0
          CASE 5: dx% = -1: dy% = -1
          CASE 6: dx% = 0: dy% = -1
          CASE 7: dx% = 1: dy% = -1
        END SELECT

        tx% = sx% + dx%
        ty% = sy% + dy%

        IF tx% >= 0 AND tx% < scanw% THEN
          IF ty% >= 0 AND ty% < scanh% THEN
            c% = scanc%(tx%, ty%)

            IF c% AND (c% < 16) THEN PSET (xx% + dx%, yy% + dy%), scansmork%(sx%, sy%) + 16
          END IF
        END IF
      NEXT i%
    END IF
  NEXT yy%
NEXT xx%
END SUB

SUB UnputSmoke (smoke AS SmokeParticle)
SHARED UnputTable%()

x% = smoke.ProjectedX
y% = smoke.ProjectedY

'   ###
'  #####
'  #####
'  #####
'   ###

IF x% >= 0 THEN
  PSET (x% - 1, y% - 2), UnputTable%(POINT(x% - 1, y% - 2))
  PSET (x%, y% - 2), UnputTable%(POINT(x%, y% - 2))
  PSET (x% + 1, y% - 2), UnputTable%(POINT(x% + 1, y% - 2))

  PSET (x% - 2, y% - 1), UnputTable%(POINT(x% - 2, y% - 1))
  PSET (x% - 1, y% - 1), UnputTable%(POINT(x% - 1, y% - 1))
  PSET (x%, y% - 1), UnputTable%(POINT(x%, y% - 1))
  PSET (x% + 1, y% - 1), UnputTable%(POINT(x% + 1, y% - 1))
  PSET (x% + 2, y% - 1), UnputTable%(POINT(x% + 2, y% - 1))

  PSET (x% - 2, y%), UnputTable%(POINT(x% - 2, y%))
  PSET (x% - 1, y%), UnputTable%(POINT(x% - 1, y%))
  PSET (x%, y%), UnputTable%(POINT(x%, y%))
  PSET (x% + 1, y%), UnputTable%(POINT(x% + 1, y%))
  PSET (x% + 2, y%), UnputTable%(POINT(x% + 2, y%))

  PSET (x% - 2, y% + 1), UnputTable%(POINT(x% - 2, y% + 1))
  PSET (x% - 1, y% + 1), UnputTable%(POINT(x% - 1, y% + 1))
  PSET (x%, y% + 1), UnputTable%(POINT(x%, y% + 1))
  PSET (x% + 1, y% + 1), UnputTable%(POINT(x% + 1, y% + 1))
  PSET (x% + 2, y% + 1), UnputTable%(POINT(x% + 2, y% + 1))

  PSET (x% - 1, y% + 2), UnputTable%(POINT(x% - 1, y% + 2))
  PSET (x%, y% + 2), UnputTable%(POINT(x%, y% + 2))
  PSET (x% + 1, y% + 2), UnputTable%(POINT(x% + 1, y% + 2))
END IF
END SUB


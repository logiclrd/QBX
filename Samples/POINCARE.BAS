'-----------------------------------------------------------------------.
'   Title:  POINCARE.BAS                                                '
'                                                                       '
'   Translated by: Jonathan Gilbert                                     '
'                                                                       '
'   Author: David E. Joyce                                              '
'           Department of Mathematics and Computer Science              '
'           Clark University                                            '
'           Worcester, MA 01610-1477                                    '
'           U.S.A.                                                      '
'           http://aleph0.clarku.edu/~djoyce/                           '
'                                                                       '
'   Date:   April, 1987.  Pascal version for tektronix terminal         '
'           December, 2002.  Java applet version (PoincareDisk.java)    '
'           August, 2026.  Translated to QuickBASIC                     '
'-----------------------------------------------------------------------'

DECLARE SUB AllocatePolygon (p AS PolygonType, n%, k%)
DECLARE SUB ComplexConjugateInPlace (this AS PointType)
DECLARE SUB ComplexDivide (numerator AS PointType, denominator AS PointType, result AS PointType)
DECLARE SUB ComplexMultiplyInPlace (this AS PointType, w AS PointType)
DECLARE SUB ConstructCentrePolygon (par AS TileParametersType, this AS PolygonType, vertices() AS PointType, k%)
DECLARE SUB CountPolygons (par AS TileParametersType, totalPolygons%, innerPolygons%)
DECLARE SUB CreateNextPolygon (par AS TileParametersType, p AS PolygonType, s%, q AS PolygonType, vertices() AS PointType, k%)
DECLARE SUB DrawCurve (a AS PointType, b AS PointType, directtoscreen%, edge%)
DECLARE SUB DrawLineToPolyBuffer (x1%, y1%, x2%, y2%)
DECLARE SUB DrawPolygon (this AS PolygonType, vertices() AS PointType)
DECLARE SUB DrawTiling (par AS TileParametersType)
DECLARE SUB FillPolygon (this AS PolygonType, vertices() AS PointType, xorfill%)
DECLARE SUB HSV2RGB (h#, s#, v#, r%, g%, b%)
DECLARE SUB ParametersCheck (this AS TileParametersType)
DECLARE SUB PointToScreen (x#, y#)
DECLARE SUB RandomizeArray (a%())
DECLARE SUB ReflectPointAcrossLine (a AS PointType, b AS PointType, r AS PointType)
DECLARE SUB ReflectPointAcrossPoint (this AS PointType, a AS PointType)
DECLARE SUB SetRandomPalette ()
DECLARE SUB Sort (keys%(), values%(), lb%, ub%)
DECLARE FUNCTION ApplyRule% (par AS TileParametersType, i%, j%, k%, polygons() AS PolygonType, vertices() AS PointType, rule%(), colour%(), numcolours%)
DECLARE FUNCTION ATAN2# (y#, x#)
DECLARE FUNCTION ComplexNormalizedSquared# (this AS PointType)
DECLARE FUNCTION GCD% (m%, n%)
DECLARE FUNCTION RandomColour% (avoid%)

CONST PI# = 3.14159265358979

CONST FillTypeUnfilled% = 0
CONST FillTypeOddEven% = 1
CONST FillTypeNonzero% = 2

TYPE TileParametersType
  NumPolygonSides AS INTEGER
  VertexValence AS INTEGER ' the number of polygons that meet at each vertex
  IsQuasiregular AS INTEGER
  NumLayers AS INTEGER
  SkipNumber AS INTEGER
  DiscColour AS INTEGER
  FillType AS INTEGER
  IsOutlined AS INTEGER
  ColourSetCount AS INTEGER
END TYPE

TYPE PolygonType
  NumSides AS INTEGER
  FirstVertexIndex AS INTEGER
END TYPE

TYPE PointType
  XValue AS DOUBLE
  YValue AS DOUBLE
END TYPE

TYPE ScreenPointType
  XValue AS INTEGER
  YValue AS INTEGER
END TYPE

DIM FPvertices(1000) AS ScreenPointType
DIM FPcrossingx%(200)
DIM FPwinding%(200)

SCREEN 12

DIM par AS TileParametersType

DO
  par.NumPolygonSides = 3 + INT(RND * 3)
  par.VertexValence = 15 - par.NumPolygonSides - RND * 10
  par.IsQuasiregular = RND < .8
  par.NumLayers = INT(2.5 + RND)
  par.SkipNumber = INT(1 + RND * RND)
  par.DiscColour = INT(RND * 16)
  par.FillType = INT(RND + RND + RND * RND)
  par.IsOutlined = RND < .1
  par.ColourSetCount = INT(RND * 4)

  ParametersCheck par

  SetRandomPalette

  DrawTiling par

  SLEEP 1
  k$ = INKEY$
  IF k$ = "s" THEN STOP
LOOP WHILE k$ <> CHR$(27)

SUB AllocatePolygon (p AS PolygonType, n%, k%)
p.NumSides = n%
p.FirstVertexIndex = k%
k% = k% + n%
END SUB

FUNCTION ApplyRule% (par AS TileParametersType, i%, j%, k%, polygons() AS PolygonType, vertices() AS PointType, rule%(), colour%(), numcolours%)
r% = rule%(i%)

isspecial% = (r% = 1)
IF isspecial% THEN r% = 2

IF r% = 4 THEN start% = 3 ELSE start% = 2

quantity% = par.NumPolygonSides - r%
IF par.VertexValence = 3 AND r% <> 0 THEN quantity% = quantity% - 1

FOR s% = start% TO start% + quantity% - 1
  ' Create a polygon adjacent to P[i]
  CreateNextPolygon par, polygons(i%), s% MOD par.NumPolygonSides, polygons(j%), vertices(), k%

  IF par.VertexValence = 3 AND s% = start% AND r% <> 0 THEN
    rule%(j%) = 4
  ELSE
    rule%(j%) = 3
  END IF

  IF numcolours% THEN
    colour%(j%) = (colour%(i%) + 1) MOD numcolours%
  ELSE
    colour%(j%) = RandomColour%(par.DiscColour)
  END IF

  j% = j% + 1

  IF isspecial% THEN
    m% = 2
  ELSEIF s% = 2 AND r% <> 0 THEN
    m% = 1
  ELSE
    m% = 0
  END IF

  FOR m% = m% TO par.VertexValence - 4
    ' Create a polygon adjacent to P[j - 1]
    CreateNextPolygon par, polygons(j% - 1), 1, polygons(j%), vertices(), k%

    IF par.NumPolygonSides = 3 AND m% = par.VertexValence - 4 THEN
      rule%(j%) = 1
    ELSE
      rule%(j%) = 2
    END IF

    IF numcolours% THEN
      colour%(j%) = (colour%(i%) + 1) MOD numcolours%
    ELSE
      colour%(j%) = RandomColour%(par.DiscColour)
    END IF

    j% = j% + 1
  NEXT m%
NEXT s%

ApplyRule% = j%
END FUNCTION

FUNCTION ATAN2# (y#, x#)
IF x# = y# THEN
  IF y# > 0 THEN
    ATAN2# = PI# * .25
  ELSE
    ATAN2# = PI# * 1.25
  END IF
ELSEIF x# = -y# THEN
  IF y# > 0 THEN
    ATAN2# = PI# * .75
  ELSE
    ATAN2# = PI# * 1.75
  END IF
ELSE
  isusable% = -1

  ON LOCAL ERROR RESUME NEXT

  slope# = y# / x#
  IF ERR THEN isusable% = 0

  ON LOCAL ERROR GOTO 0

  IF isusable% THEN
    angle# = ATN(slope#)

    IF x# < 0 THEN angle# = angle# + PI#
    IF angle# < 0 THEN angle# = angle# + 2 * PI#

    ATAN2# = angle#
  ELSEIF y# <> 0 THEN
    IF (y# > 0) EQV (x# > 0) THEN
      ATAN2# = PI# * .5
    ELSE
      ATAN2# = -PI# * .5
    END IF
  END IF
END IF
END FUNCTION

SUB ComplexConjugateInPlace (this AS PointType)
this.YValue = -this.YValue
END SUB

SUB ComplexDivide (numerator AS PointType, denominator AS PointType, result AS PointType)
den# = ComplexNormalizedSquared(denominator)

result.XValue = (numerator.XValue * denominator.XValue + numerator.YValue * denominator.YValue) / den#
result.YValue = (numerator.YValue * denominator.XValue - numerator.XValue * denominator.YValue) / den#
END SUB

SUB ComplexMultiplyInPlace (this AS PointType, w AS PointType)
tx# = this.XValue
ty# = this.YValue

this.XValue = tx# * w.XValue - ty# * w.YValue
this.YValue = ty# * w.XValue + tx# * w.YValue
END SUB

FUNCTION ComplexNormalizedSquared# (this AS PointType)
ComplexNormalizedSquared# = this.XValue * this.XValue + this.YValue * this.YValue
END FUNCTION

SUB ConstructCentrePolygon (par AS TileParametersType, this AS PolygonType, vertices() AS PointType, k%)
' Initialize P as the center polygon in an n-k regular or quasiregular tiling.
' Let ABC be a triangle in a regular (n,k0-tiling, where
'    A is the center of an n-gon (also center of the disk),
'    B is a vertex of the n-gon, and
'    C is the midpoint of a side of the n-gon adjacent to B.

angleA# = PI# / par.NumPolygonSides
angleB# = PI# / par.VertexValence
angleC# = PI# * .5

' For a regular tiling, we need to compute the distance s from A to B.
sinA# = SIN(angleA#)
sinB# = SIN(angleB#)

s# = SIN(angleC# - angleB# - angleA#) / SQR(1 - sinB# * sinB# - sinA# * sinA#)

' But for a quasiregular tiling, we need the distance s from A to C.
IF par.IsQuasiregular THEN
  s# = (s# * s# + 1) / (2 * s# * COS(angleA#))
  s# = s# - SQR(s# * s# - 1)
END IF

' Now determine the coordinates of the n vertices of the n-gon.
' They're all at distance s from the center of the Poincare disc.
AllocatePolygon this, par.NumPolygonSides, k%

FOR i% = 0 TO par.NumPolygonSides - 1
  vertices(this.FirstVertexIndex + i%).XValue = s# * COS((3 + 2 * i%) * angleA#)
  vertices(this.FirstVertexIndex + i%).YValue = s# * SIN((3 + 2 * i%) * angleA#)
NEXT i%
END SUB

SUB CountPolygons% (par AS TileParametersType, totalPolygons%, innerPolygons%)
' Determine
'   totalPolygons:  the number of polygons there are through that many layers
'   innerPolygons:  the number through one less layer

totalPolygons% = 1 ' count the central polygon
innerPolygons% = 0

a% = par.NumPolygonSides * (par.VertexValence - 3) ' polygons in first layer joined by a vertex
b% = par.NumPolygonSides                           ' polygons in first layer joined by an edge

IF par.VertexValence = 3 THEN
  FOR l% = 1 TO par.NumLayers
    innerPolygons% = totalPolygons%
    nextA& = a% + b%
    nextB& = (par.NumPolygonSides - 6) * a% + (par.NumPolygonSides - 5) * b%
    totalPolygons% = totalPolygons% + a% + b%

    IF totalPolygons% > 1000 OR nextA& > 1000 OR nextB& > 1000 THEN EXIT FOR

    a% = nextA&
    b% = nextB&
  NEXT l%
ELSE 'k >= 4
  FOR l% = 1 TO par.NumLayers
    innerPolygons% = totalPolygons%
    nextA& = ((par.NumPolygonSides - 2) * (par.VertexValence - 3) - 1) * a%
    nextA& = nextA& + ((par.NumPolygonSides - 3) * (par.VertexValence - 3) - 1) * b%
    nextB& = (par.NumPolygonSides - 2) * a% + (par.NumPolygonSides - 3) * b%
    totalPolygons% = totalPolygons% + a% + b%

    IF totalPolygons% > 1000 OR nextA& > 1000 OR nextB& > 1000 THEN EXIT FOR

    a% = nextA&
    b% = nextB&
  NEXT l%
END IF
END SUB

SUB CreateNextPolygon (par AS TileParametersType, p AS PolygonType, s%, q AS PolygonType, vertices() AS PointType, k%)
' reflect P across the point or the side indicated by the side s
'  to produce the resulting polygon Q
AllocatePolygon q, par.NumPolygonSides, k%

DIM u AS PointType, v AS PointType, w AS PointType

IF par.IsQuasiregular THEN
  v = vertices(p.FirstVertexIndex + s%)

  FOR i% = 0 TO par.NumPolygonSides - 1 ' reflect P[i] across P[s] to get Q[j]
    j% = (par.NumPolygonSides + i% - s%) MOD par.NumPolygonSides

    w = vertices(p.FirstVertexIndex + i%)

    ReflectPointAcrossPoint v, w

    vertices(q.FirstVertexIndex + j%) = w
  NEXT i%
ELSE ' regular
  ' the endpoints of line C
  u = vertices(p.FirstVertexIndex + s%)
  v = vertices(p.FirstVertexIndex + ((s% + 1) MOD par.NumPolygonSides))

  FOR i% = 0 TO par.NumPolygonSides - 1 ' reflect P[i] across C to get Q[j]
    j% = (par.NumPolygonSides + s% - i% + 1) MOD par.NumPolygonSides

    w = vertices(p.FirstVertexIndex + i%)

    ReflectPointAcrossLine u, v, w

    vertices(q.FirstVertexIndex + j%) = w
  NEXT i%
END IF
END SUB

SUB DrawCurve (a AS PointType, b AS PointType, directtoscreen%, edge%)
'xcentre# = (a.XValue + b.XValue) * .5
'ycentre# = (a.YValue + b.YValue) * .5

'IF xcentre# < ycentre# THEN radius# = xcentre# ELSE radius# = ycentre#

x1# = a.XValue' * radius# + xcentre#
y1# = a.YValue' * radius# + ycentre#

' first determine if its a line or a circle
den# = a.XValue * b.YValue - b.XValue * a.YValue

isstraight% = ABS(den#) < 1E-14

IF isstraight% THEN
  ' go directly to terminal point B
  x2# = b.XValue' * radius# + xcentre#
  y2# = b.YValue' * radius# + ycentre#

  PointToScreen x1#, y1#
  PointToScreen x2#, y2#

  IF directtoscreen# THEN
    LINE (x1#, y1#)-(x2#, y2#)
  ELSE
    DrawLineToPolyBuffer (x1#), (y1#), (x2#), (y2#)', edge%
  END IF
ELSE ' it's a circle
  DIM c AS PointType

  ' find the centre of the circle through these points
  s1# = (1 + a.XValue * a.XValue + a.YValue * a.YValue) * .5
  s2# = (1 + b.XValue * b.XValue + b.YValue * b.YValue) * .5
  c.XValue = (s1# * b.YValue - s2# * a.YValue) / den#
  c.YValue = (a.XValue * s2# - b.XValue * s1#) / den#
  radius# = SQR(c.XValue * c.XValue + c.YValue * c.YValue - 1)

  ' determine starting and ending angles
  alpha# = ATAN2(a.YValue - c.YValue, a.XValue - c.XValue)
  beta# = ATAN2(b.YValue - c.YValue, b.XValue - c.XValue)

  IF ERR THEN EXIT SUB

  IF ABS(beta# - alpha#) > PI# THEN
    IF beta# < alpha# THEN
      beta# = beta# + 2 * PI#
    ELSE
      alpha# = alpha# + 2 * PI#
    END IF
  END IF

  aspan# = beta# - alpha#

  segments% = ABS(radius# * aspan# * (230 * .35))

  IF segments% < 1 THEN segments% = 1

  x1# = c.XValue + radius# * COS(alpha#)
  y1# = c.YValue + radius# * SIN(alpha#)

  IF ERR THEN EXIT SUB

  PointToScreen x1#, y1#

  FOR i% = 1 TO segments%
    t# = i% / segments%

    a# = alpha# + aspan# * t#

    x2# = c.XValue + radius# * COS(a#)
    y2# = c.YValue + radius# * SIN(a#)

    PointToScreen x2#, y2#

    IF directtoscreen% THEN
      LINE (x1#, y1#)-(x2#, y2#)
    ELSE
      DrawLineToPolyBuffer (x1#), (y1#), (x2#), (y2#)', edge%
    END IF

    x1# = x2#
    y1# = y2#
  NEXT i%
END IF
END SUB

SUB DrawLineToPolyBuffer (x1%, y1%, x2%, y2%)
SHARED FPvertices() AS ScreenPointType, FPvertexcount%

IF FPvertexcount% = 0 THEN
  FPvertices(0).XValue = x1%
  FPvertices(0).YValue = y1%

  FPvertexcount% = FPvertexcount% + 1
END IF

FPvertices(FPvertexcount%).XValue = x2%
FPvertices(FPvertexcount%).YValue = y2%

FPvertexcount% = FPvertexcount% + 1
END SUB

SUB DrawPolygon (this AS PolygonType, vertices() AS PointType)
FOR i% = 0 TO this.NumSides - 1
  DrawCurve vertices(this.FirstVertexIndex + i%), vertices(this.FirstVertexIndex + ((i% + 1) MOD this.NumSides)), -1, i%
NEXT i%
END SUB

SUB DrawTiling (par AS TileParametersType)
numcolours% = par.ColourSetCount AND ((par.VertexValence MOD 2) = 0)

' totalPolygons%: the total number of polygons in all the layers
' innerpolygons%: the number through one less layer

CountPolygons par, totalPolygons%, innerPolygons%

IF totalPolygons% <= 0 THEN EXIT SUB

' rule codes
'   0:  initial polygon.  Needs neighbors on all n sides
'   1:  polygon already has 2 neighbors, but one less around corner needed
'   2:  polygon already has 2 neighbors
'   3:  polygon already has 3 neighbors
'   4:  polygon already has 4 neighbors

DIM polygons(0 TO totalPolygons% - 1) AS PolygonType ' the list of polygons
DIM vertices(0 TO (totalPolygons% + 1) * par.NumPolygonSides) AS PointType
DIM rule%(0 TO totalPolygons% - 1) ' previously-created neighbours for the polygons
DIM colour%(0 TO totalPolygons% - 1) ' the list of colours for the polygons

k% = 0 ' index of the next vertex to allocate

ConstructCentrePolygon par, polygons(0), vertices(), k%

rule%(0) = 0

IF numcolours% = 0 THEN colour%(0) = RandomColour%(par.DiscColour)

j% = 1 ' index of the next polygon to create

FOR i% = 0 TO innerPolygons% - 1
  j% = ApplyRule(par, i%, j%, k%, polygons(), vertices(), rule%(), colour%(), numcolours%)
NEXT i%

IF numcolours% THEN
  DIM colourset%(14)

  FOR i% = 0 TO 14
    colourset%(i%) = i% + 1
  NEXT i%

  RandomizeArray colourset%()

  FOR i% = 0 TO totalPolygons% - 1
    colour%(i%) = colourset%(colour%(i%))
  NEXT i%
END IF

' Now draw

SCREEN 12

CLS

IF par.DiscColour = 0 THEN
  PAINT (0, 0), RandomColour%(0)
END IF

CIRCLE (320, 240), 230, par.DiscColour
PAINT (320, 240), par.DiscColour, par.DiscColour

numstars% = GCD(par.SkipNumber, par.NumPolygonSides)
pointsperstar% = par.NumPolygonSides / numstars%

DIM q AS PolygonType

AllocatePolygon q, pointsperstar%, k%

FOR i% = 0 TO totalPolygons% - 1
  IF par.FillType <> 0 THEN
    COLOR colour%(i%)

    FOR s% = 0 TO numstars% - 1
      FOR j% = 0 TO pointsperstar% - 1
        vertices(q.FirstVertexIndex + j%) = vertices(polygons(i%).FirstVertexIndex + ((j% * par.SkipNumber) MOD par.NumPolygonSides) + s%)
      NEXT j%

      FillPolygon q, vertices(), (par.FillType = 1)
    NEXT s%
  END IF

  IF par.IsOutlined OR (par.FillType = 0) THEN
    IF par.FillType = 0 THEN
      COLOR colour%(i%)
    ELSE
      COLOR 0
    END IF

    FOR s% = 0 TO numstars% - 1
      FOR j% = 0 TO pointsperstar% - 1
        vertices(q.FirstVertexIndex + j%) = vertices(polygons(i%).FirstVertexIndex + ((j% * par.SkipNumber) MOD par.NumPolygonSides) + s%)
      NEXT j%

      DrawPolygon q, vertices()
    NEXT s%
  END IF
NEXT i%
END SUB

SUB FillPolygon (this AS PolygonType, vertices() AS PointType, xorfill%)
SHARED FPvertices() AS ScreenPointType, FPvertexcount%
SHARED FPcrossingx%()
SHARED FPwinding%()

FPvertexcount% = 0

FOR i% = 0 TO this.NumSides - 1
  DrawCurve vertices(this.FirstVertexIndex + i%), vertices(this.FirstVertexIndex + ((i% + 1) MOD this.NumSides)), 0, i%
NEXT i%

IF FPvertexcount% < 3 THEN EXIT SUB

y% = FPvertices(0).YValue

y1% = y%
y2% = y%

FOR i% = 1 TO FPvertexcount% - 1
  y% = FPvertices(i%).YValue

  IF y% < y1% THEN y1% = y%
  IF y% > y2% THEN y2% = y%
NEXT i%

yy% = y1% + y1% - 1

FOR y% = y1% TO y2% - 1
  yy% = yy% + 2

  count% = 0

  x1% = FPvertices(FPvertexcount% - 1).XValue
  y1% = FPvertices(FPvertexcount% - 1).YValue
  yy1% = y1% + y1%

  FOR i% = 0 TO FPvertexcount% - 1
    x2% = FPvertices(i%).XValue
    y2% = FPvertices(i%).YValue
    yy2% = y2% + y2%

    IF y1% <> y2% THEN
      IF yy% >= yy1% AND yy% <= yy2% THEN
        FPcrossingx%(count%) = x1% + (yy% - yy1%) * (x2% - x1%) \ (yy2% - yy1%)
        FPwinding%(count%) = 1
        count% = count% + 1
      ELSEIF yy% >= yy2% AND yy% <= yy1% THEN
        FPcrossingx%(count%) = x1% + (yy% - yy1%) * (x2% - x1%) \ (yy2% - yy1%)
        FPwinding%(count%) = -1
        count% = count% + 1
      END IF
    END IF

    x1% = x2%
    y1% = y2%
    yy1% = yy2%
  NEXT i%

  IF count% > 1 THEN
    Sort FPcrossingx%(), FPwinding%(), 0, count% - 1

    IF xorfill% THEN
      FOR i% = 0 TO count% - 1 STEP 2
        LINE (FPcrossingx%(i%), y%)-(FPcrossingx%(i% + 1), y%)
      NEXT i%
    ELSE
      x1% = FPcrossingx%(0)
      w% = FPwinding%(0)

      FOR i% = 1 TO count% - 1
        IF w% = 0 THEN x1% = FPcrossingx%(i%)

        w% = w% + FPwinding%(i%)

        IF w% = 0 THEN
          x2% = FPcrossingx%(i%)
          LINE (x1%, y%)-(x2%, y%)
        END IF
      NEXT i%
    END IF
  END IF
NEXT y%

x1% = FPvertices(FPvertexcount% - 1).XValue
y1% = FPvertices(FPvertexcount% - 1).YValue

FOR i% = 0 TO FPvertexcount% - 1
  x2% = FPvertices(i%).XValue
  y2% = FPvertices(i%).YValue

  IF y1% = y2% THEN
    LINE (x1%, y1%)-(x2%, y1%)
  END IF

  x1% = x2%
  y1% = y2%
NEXT i%
END SUB

FUNCTION GCD% (mm%, nn%) ' greatest common divisor
' Make sure m and n are nonnegative.
m% = ABS(mm%)
n% = ABS(nn%)

IF m% > n% THEN SWAP m%, n% ' Make sure m <= n.

WHILE m% <> 0
  temp% = n%
  n% = m%
  m% = temp% MOD m%
WEND

GCD% = n%
END FUNCTION

SUB HSV2RGB (h#, s#, v#, r%, g%, b%)
' Input: h# in 0..360, s# and v# in 0..1
' Output: r%, g%, b% in 0..63

' Just in case
WHILE h# < 0: h# = h# + 360: WEND
WHILE h# >= 360: h# = h# - 360: WEND

chroma# = s# * v#

x# = h# / 60#

WHILE x# >= 2
  x# = x# - 2
WEND

x# = chroma# * (1 - ABS(x# - 1))

SELECT CASE h#
  CASE 0 TO 60: r# = chroma#: g# = x#: b# = 0
  CASE 60 TO 120: r# = x#: g# = chroma#: b# = 0
  CASE 120 TO 180: r# = 0: g# = chroma#: b# = x#
  CASE 180 TO 240: r# = 0: g# = x#: b# = chroma#
  CASE 240 TO 300: r# = x#: g# = 0: b# = chroma#
  CASE 300 TO 360: r# = chroma#: g# = 0: b# = x#
END SELECT

match# = v# - chroma#

r% = (r# + match#) * 64
g% = (g# + match#) * 64
b% = (b# + match#) * 64

IF r% > 63 THEN r% = 63
IF g% > 63 THEN g% = 63
IF b% > 63 THEN b% = 63
END SUB

SUB ParametersCheck (this AS TileParametersType)
' n should be between 3 and 20
IF this.NumPolygonSides < 3 THEN this.NumPolygonSides = 3
IF this.NumPolygonSides > 20 THEN this.NumPolygonSides = 20

' k should be large enough, but no larger than 20
SELECT CASE this.NumPolygonSides
  CASE 3: IF this.VertexValence < 7 THEN this.VertexValence = 7
  CASE 4: IF this.VertexValence < 5 THEN this.VertexValence = 5
  CASE IS < 7: IF this.VertexValence < 4 THEN this.VertexValence = 4
  CASE ELSE: IF this.VertexValence < 3 THEN this.VertexValence = 3
END SELECT

IF this.VertexValence > 20 THEN this.VertexValence = 20

' skipNumber should be between 1 and n/2
IF this.SkipNumber * 2 >= this.NumPolygonSides THEN this.SkipNumber = 1

' layers shouldn't be too big
IF this.NumPolygonSides = 3 OR this.VertexValence = 3 THEN
  IF this.NumLayers > 5 THEN this.NumLayers = 5
ELSE
  IF this.NumLayers > 4 THEN this.NumLayers = 4
END IF

IF this.IsQuasiregular = 0 AND this.ColourSetCount = 1 THEN
  this.ColourSetCount = (RND < .5) * -2
END IF
END SUB

SUB PointToScreen (x#, y#)
x# = 320 + 230 * x#
y# = 240 - 230 * y#
END SUB

FUNCTION RandomColour% (avoid%)
' It is assumed that the palette has been arranged so that 1..15 are the desired set of colours.
IF avoid% > 0 AND avoid% < 16 THEN
  r% = INT(RND * 14) + 1
  IF r% >= avoid% THEN r% = r% + 1
  RandomColour% = r%
ELSE
  RandomColour% = INT(RND * 15) + 1
END IF
END FUNCTION

SUB RandomizeArray (a%())
ub% = UBOUND(a%)
lb% = LBOUND(a%)

count% = ub% - lb% + 1

FOR i% = ub% TO lb% + 1
  j% = INT(RND * (count% + i% - ub%)) + lb%

  IF j% <> i% THEN SWAP a%(i%), a%(j%)
NEXT i%
END SUB

SUB ReflectPointAcrossLine (a AS PointType, b AS PointType, r AS PointType)
' first determine if its a line or a circle
den# = a.XValue * b.YValue - b.XValue * a.YValue

isstraight% = ABS(den#) < 1E-14

DIM cd AS PointType
DIM q AS PointType

IF isstraight% THEN
  ' find a unit vector D in the direction of the line
  dx# = a.XValue - b.XValue
  dy# = a.YValue - b.YValue
  den# = SQR(dx# * dx# + dy# * dy#)

  cd.XValue = (b.XValue - a.XValue) / den#
  cd.YValue = (b.YValue - a.YValue) / den#

  ' Reflect the point R across the this line to get Q the returned point}
  factor# = 2 * ((r.XValue - a.XValue) * cd.XValue + (r.YValue - a.YValue) * cd.YValue)

  q.XValue = 2 * a.XValue + factor# * cd.XValue - r.XValue
  q.YValue = 2 * a.YValue + factor# * cd.YValue - r.YValue
ELSE ' it's a circle
  ' find the centre of the circle through these points
  s1# = (1 + a.XValue * a.XValue + a.YValue * a.YValue) * .5
  s2# = (1 + b.XValue * b.XValue + b.YValue * b.YValue) * .5
  cd.XValue = (s1# * b.YValue - s2# * a.YValue) / den#
  cd.YValue = (a.XValue * s2# - b.XValue * s1#) / den#
  radius# = SQR(cd.XValue * cd.XValue + cd.YValue * cd.YValue - 1)

  ' Reflect the point R across the this line to get Q the returned point}
  dx# = r.XValue - cd.XValue
  dy# = r.YValue - cd.YValue

  factor# = radius# * radius# / (dx# * dx# + dy# * dy#)

  q.XValue = cd.XValue + factor# * (r.XValue - cd.XValue)
  q.YValue = cd.YValue + factor# * (r.YValue - cd.YValue)
END IF

r = q
END SUB

SUB ReflectPointAcrossPoint (this AS PointType, a AS PointType)
' Reflect the point A through this point B to get the returned point C.
' The rule for computing A thru B (as complex numbers) is:
'
'            B - t A             where t = (1+BB')/2, and
' A |> B = -----------               B' is the complex
'           t -  A B'                conjugate of B

t# = (1 + ComplexNormalizedSquared(this)) * .5

DIM numerator AS PointType, denominator AS PointType

' compute the numerator as  B - t * A
numerator.XValue = this.XValue - a.XValue * t#
numerator.YValue = this.YValue - a.YValue * t#

' compute the denominator as  t - A * B'
denominator = this
ComplexConjugateInPlace denominator
ComplexMultiplyInPlace denominator, a

denominator.XValue = t# - denominator.XValue
denominator.YValue = -denominator.YValue

ComplexDivide numerator, denominator, a
END SUB

SUB SetRandomPalette
DO
  h1# = RND * 360: s1# = RND * .5 + .5: v1# = RND * .5 + .5
  h2# = h1% + (RND * 360 - 180): s2# = RND * .5 + .5: v2# = RND * .5 + .5

  dh# = h2# - h1#
  ds# = s2# - s1#
  dv# = v2# - v1#

  difference# = SQR(dh# * dh# / (360# * 360#) + ds# * ds# + dv# * dv#)
LOOP WHILE difference# < .2

FOR i% = 1 TO 15
  t# = (i% - 1) / 14#

  h# = h1# + t# * dh#
  s# = s1# + t# * ds#
  v# = v1# + t# * dv#

  HSV2RGB h#, s#, v#, r%, g%, b%

  OUT &H3C8, i%
  OUT &H3C9, r%
  OUT &H3C9, g%
  OUT &H3C9, b%
NEXT i%
END SUB

SUB Sort (keys%(), values%(), lb%, ub%)
IF ub% = lb% + 1 THEN
  IF keys%(lb%) > keys%(ub%) THEN
    SWAP keys%(lb%), keys%(ub%)
    SWAP values%(lb%), values%(ub%)
  END IF
ELSEIF ub% > lb% THEN
  ' Try to avoid pathological cases; test 3 possible pivots and use
  ' the median.
  mb% = (lb% + ub%) \ 2

  pivot1% = keys%(lb%)
  pivot2% = keys%(mb%)
  pivot3% = keys%(ub%)

  IF pivot1% < pivot2% THEN
    IF pivot3% > pivot2% THEN
      SWAP keys%(lb%), keys%(mb%)
      SWAP values%(lb%), values%(mb%)
    ELSEIF pivot3% > pivot1% THEN
      SWAP keys%(lb%), keys%(ub%)
      SWAP values%(lb%), values%(ub%)
    END IF
  ELSEIF pivot3% < pivot1% THEN
    SWAP keys%(lb%), keys%(ub%)
    SWAP values%(lb%), values%(ub%)
  END IF

  pivoti% = lb%
  pivot% = keys%(pivoti%)

  FOR i% = lb% + 1 TO ub%
    IF keys%(i%) < pivot% THEN
      IF i% > pivoti% + 1 THEN
        ' We have at least one value larger than the pivot after
        ' it; move that known-larger value to i% and i% to be
        ' adjacent to the pivot.
        SWAP keys%(i%), keys%(pivoti% + 1)
        SWAP values%(i%), values%(pivoti% + 1)
      END IF

      SWAP keys%(pivoti%), keys%(pivoti% + 1)
      SWAP values%(pivoti%), values%(pivoti% + 1)
      pivoti% = pivoti% + 1
    END IF
  NEXT i%

  Sort keys%(), values%(), lb%, pivoti% - 1
  Sort keys%(), values%(), pivoti% + 1, ub%
END IF
END SUB

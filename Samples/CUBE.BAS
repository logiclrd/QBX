TYPE Pt
  X AS DOUBLE
  Y AS DOUBLE
  Z AS DOUBLE
END TYPE

TYPE PixelPt
  X AS DOUBLE
  Y AS DOUBLE
END TYPE

TYPE Tri
  P1 AS INTEGER
  P2 AS INTEGER
  P3 AS INTEGER
  C AS INTEGER
END TYPE

DECLARE SUB ApplyTransform (i AS Pt, matrix#(), o AS Pt)
DECLARE SUB CreateTextureTris (facebase%, spritepixelcolour%)
DECLARE SUB HorizontalLine (scanofs&, x1%, x2%, colour%)
DECLARE SUB FillTriangle (x1%, y1%, x2%, y2%, x3%, y3%, colour%)
DECLARE SUB MultiplyMatrix (subject#(), transform#())
DECLARE SUB RotateX (matrix#(), angle#)
DECLARE SUB RotateY (matrix#(), angle#)
DECLARE SUB RotateZ (matrix#(), angle#)
DECLARE SUB SetIdentity (transform#())

CONST CameraZ = -10

DIM SHARED Pts(200) AS Pt
DIM SHARED Tris(100) AS Tri

DIM SHARED NumPts%, NumTris%

FOR X% = -1 TO 1 STEP 2
  FOR Y% = -1 TO 1 STEP 2
    FOR Z% = -1 TO 1 STEP 2
      Pts(NumPts%).X = X%
      Pts(NumPts%).Y = Y%
      Pts(NumPts%).Z = Z%
      NumPts% = NumPts% + 1
    NEXT Z%
  NEXT Y%
NEXT X%

' Base cube
Tris(0).P1 = 2: Tris(0).P2 = 6: Tris(0).P3 = 4: Tris(0).C = 1
Tris(1).P1 = 2: Tris(1).P2 = 4: Tris(1).P3 = 0: Tris(1).C = 1

Tris(2).P1 = 6: Tris(2).P2 = 7: Tris(2).P3 = 5: Tris(2).C = 10
Tris(3).P1 = 6: Tris(3).P2 = 5: Tris(3).P3 = 4: Tris(3).C = 10

Tris(4).P1 = 7: Tris(4).P2 = 3: Tris(4).P3 = 1: Tris(4).C = 11
Tris(5).P1 = 7: Tris(5).P2 = 1: Tris(5).P3 = 5: Tris(5).C = 11

Tris(6).P1 = 3: Tris(6).P2 = 2: Tris(6).P3 = 0: Tris(6).C = 12
Tris(7).P1 = 3: Tris(7).P2 = 0: Tris(7).P3 = 1: Tris(7).C = 12

Tris(8).P1 = 3: Tris(8).P2 = 7: Tris(8).P3 = 6: Tris(8).C = 13
Tris(9).P1 = 3: Tris(9).P2 = 6: Tris(9).P3 = 2: Tris(9).C = 13

Tris(10).P1 = 0: Tris(10).P2 = 4: Tris(10).P3 = 5: Tris(10).C = 14
Tris(11).P1 = 0: Tris(11).P2 = 5: Tris(11).P3 = 1: Tris(11).C = 14

NumTris% = 12

FOR i% = 0 TO 11 STEP 2
  IF i% = 0 THEN spritepixelcolour% = 14 ELSE spritepixelcolour% = 0
  CreateTextureTris i%, spritepixelcolour%
NEXT i%

DATA ........
DATA ........
DATA ..#..#..
DATA ........
DATA ..#..#..
DATA ...##...
DATA ........
DATA ........

DIM transform(0 TO 2, 0 TO 2) AS DOUBLE
DIM transformedPts(200) AS Pt

DIM paintOrder(100) AS INTEGER

DIM screenPts(200) AS PixelPt

a# = 0

SCREEN 13

DIM buf&(16001)

GET (0, 0)-(319, 199), buf&

DEF SEG = VARSEG(buf&(1))

DIM SHARED bufofs%

bufofs% = VARPTR(buf&(1))

DO
  FOR i% = 1 TO 16000
    buf&(i%) = 0
  NEXT i%

  SetIdentity transform()

  RotateX transform(), a#
  RotateY transform(), a# * .7
  'RotateZ transform(), a#

  FOR i% = 0 TO NumPts% - 1
    ApplyTransform Pts(i%), transform(), transformedPts(i%)

    dz# = transformedPts(i%).Z - CameraZ

    screenPts(i%).X = 160 + 350 * transformedPts(i%).X / dz#
    screenPts(i%).Y = 100 - 350 * transformedPts(i%).Y / dz#
  NEXT i%

  FOR i% = 0 TO NumTris% - 1
    p1i% = Tris(i%).P1
    p2i% = Tris(i%).P2
    p3i% = Tris(i%).P3

    x1# = screenPts(p1i%).X
    y1# = screenPts(p1i%).Y
    x2# = screenPts(p2i%).X
    y2# = screenPts(p2i%).Y
    x3# = screenPts(p3i%).X
    y3# = screenPts(p3i%).Y

    cross1# = x1# * y2# - y1# * x2#
    cross2# = x2# * y3# - y2# * x3#
    cross3# = x3# * y1# - y3# * x1#

    testwinding# = cross1# + cross2# + cross3#

    ' Only draw triangles that are facing the screen
    IF testwinding# > 0 THEN
      x1% = INT(x1#)
      y1% = INT(y1#)
      x2% = INT(x2#)
      y2% = INT(y2#)
      x3% = INT(x3#)
      y3% = INT(y3#)

      FillTriangle x1%, y1%, x2%, y2%, x3%, y3%, Tris(i%).C
    END IF
  NEXT i%

  PUT (0, 0), buf&, PSET

  a# = a# + .03
LOOP WHILE INKEY$ = ""


SUB ApplyTransform (i AS Pt, matrix#(), o AS Pt)
o.X = i.X * matrix#(0, 0) + i.Y * matrix#(1, 0) + i.Z * matrix#(2, 0)
o.Y = i.X * matrix#(0, 1) + i.Y * matrix#(1, 1) + i.Z * matrix#(2, 1)
o.Z = i.X * matrix#(0, 2) + i.Y * matrix#(1, 2) + i.Z * matrix#(2, 2)
END SUB

SUB CreateTextureTris (facebase%, spritepixelcolour%)
DIM topleft AS Pt
DIM topright AS Pt
DIM bottomright AS Pt

DIM positiveTX AS Pt
DIM positiveTY AS Pt

topleft = Pts(Tris(facebase%).P1)
topright = Pts(Tris(facebase%).P2)
bottomright = Pts(Tris(facebase%).P3)

IF topleft.X < topright.X THEN
  positiveTX.X = 2
ELSEIF topleft.X > topright.X THEN
  positiveTX.X = -2
ELSEIF topleft.Y < topright.Y THEN
  positiveTX.Y = 2
ELSEIF topleft.Y > topright.Y THEN
  positiveTX.Y = -2
ELSEIF topleft.Z < topright.Z THEN
  positiveTX.Z = 2
ELSEIF topleft.Z > topright.Z THEN
  positiveTX.Z = -2
END IF

IF topright.X < bottomright.X THEN
  positiveTY.X = 2
ELSEIF topright.X > bottomright.X THEN
  positiveTY.X = -2
ELSEIF topright.Y < bottomright.Y THEN
  positiveTY.Y = 2
ELSEIF topright.Y > bottomright.Y THEN
  positiveTY.Y = -2
ELSEIF topright.Z < bottomright.Z THEN
  positiveTY.Z = 2
ELSEIF topright.Z > bottomright.Z THEN
  positiveTY.Z = -2
END IF

RESTORE

FOR Y% = 1 TO 8
  READ scan$
  FOR X% = 1 TO 8
    IF MID$(scan$, X%, 1) = "#" THEN
      pixeltopleft% = NumPts%: NumPts% = NumPts% + 1
      pixeltopright% = NumPts%: NumPts% = NumPts% + 1
      pixelbottomleft% = NumPts%: NumPts% = NumPts% + 1
      pixelbottomright% = NumPts%: NumPts% = NumPts% + 1

      Pts(pixeltopleft%).X = topleft.X + positiveTX.X * (X% - 1) / 8 + positiveTY.X * (Y% - 1) / 8
      Pts(pixeltopleft%).Y = topleft.Y + positiveTX.Y * (X% - 1) / 8 + positiveTY.Y * (Y% - 1) / 8
      Pts(pixeltopleft%).Z = topleft.Z + positiveTX.Z * (X% - 1) / 8 + positiveTY.Z * (Y% - 1) / 8

      Pts(pixeltopright%).X = topleft.X + positiveTX.X * X% / 8 + positiveTY.X * (Y% - 1) / 8
      Pts(pixeltopright%).Y = topleft.Y + positiveTX.Y * X% / 8 + positiveTY.Y * (Y% - 1) / 8
      Pts(pixeltopright%).Z = topleft.Z + positiveTX.Z * X% / 8 + positiveTY.Z * (Y% - 1) / 8

      Pts(pixelbottomleft%).X = topleft.X + positiveTX.X * (X% - 1) / 8 + positiveTY.X * Y% / 8
      Pts(pixelbottomleft%).Y = topleft.Y + positiveTX.Y * (X% - 1) / 8 + positiveTY.Y * Y% / 8
      Pts(pixelbottomleft%).Z = topleft.Z + positiveTX.Z * (X% - 1) / 8 + positiveTY.Z * Y% / 8

      Pts(pixelbottomright%).X = topleft.X + positiveTX.X * X% / 8 + positiveTY.X * Y% / 8
      Pts(pixelbottomright%).Y = topleft.Y + positiveTX.Y * X% / 8 + positiveTY.Y * Y% / 8
      Pts(pixelbottomright%).Z = topleft.Z + positiveTX.Z * X% / 8 + positiveTY.Z * Y% / 8

      Tris(NumTris%).P1 = pixeltopleft%
      Tris(NumTris%).P2 = pixeltopright%
      Tris(NumTris%).P3 = pixelbottomright%
      Tris(NumTris%).C = spritepixelcolour%
      NumTris% = NumTris% + 1

      Tris(NumTris%).P1 = pixeltopleft%
      Tris(NumTris%).P2 = pixelbottomright%
      Tris(NumTris%).P3 = pixelbottomleft%
      Tris(NumTris%).C = spritepixelcolour%
      NumTris% = NumTris% + 1
    END IF
  NEXT X%
NEXT Y%
END SUB

SUB FillTriangle (x1%, y1%, x2%, y2%, x3%, y3%, colour%)
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
  mx% = x1%
  IF x2% < mx% THEN mx% = x2%
  IF x3% < mx% THEN mx% = x3%

  nx% = x1%
  IF x2% > nx% THEN nx% = x2%
  IF x3% > nx% THEN nx% = x3%

  HorizontalLine y1% * 320&, mx%, nx%, colour%
ELSE
  xfp% = 0
  dxfp% = (x3% - x1%) * 256& \ (y3% - y1% + 1)
  xx% = x1%

  IF y2% > y1% THEN
    x2fp% = 0
    dx2fp% = (x2% - x1%) * 256& \ (y2% - y1%)
    xx2% = x1%

    FOR yy% = y1% TO y2%
      HorizontalLine o&, xx%, xx2%, colour%

      xfp% = xfp% + dxfp%
      WHILE xfp% < 0: xx% = xx% - 1: xfp% = xfp% + 256: WEND
      WHILE xfp% >= 256: xx% = xx% + 1: xfp% = xfp% - 256: WEND

      x2fp% = x2fp% + dx2fp%
      WHILE x2fp% < 0: xx2% = xx2% - 1: x2fp% = x2fp% + 256: WEND
      WHILE x2fp% >= 256: xx2% = xx2% + 1: x2fp% = x2fp% - 256: WEND

      o& = o& + 320&
    NEXT yy%
  ELSE
    HorizontalLine o&, xx%, x2%, colour%

    xfp% = xfp% + dxfp%
    WHILE xfp% < 0: xx% = xx% - 1: xfp% = xfp% + 256: WEND
    WHILE xfp% >= 256: xx% = xx% + 1: xfp% = xfp% - 256: WEND

    o& = o& + 320&
  END IF

  IF y3% > y2% THEN
    x2fp% = 0
    dx2fp% = (x3% - x2%) * 256& \ (y3% - y2% + 1)
    xx2% = x2%

    FOR yy% = y2% + 1 TO y3% - 1
      x2fp% = x2fp% + dx2fp%
      WHILE x2fp% < 0: xx2% = xx2% - 1: x2fp% = x2fp% + 256: WEND
      WHILE x2fp% >= 256: xx2% = xx2% + 1: x2fp% = x2fp% - 256: WEND

      HorizontalLine o&, xx%, xx2%, colour%

      xfp% = xfp% + dxfp%
      WHILE xfp% < 0: xx% = xx% - 1: xfp% = xfp% + 256: WEND
      WHILE xfp% >= 256: xx% = xx% + 1: xfp% = xfp% - 256: WEND

      o& = o& + 320&
    NEXT yy%

    x2fp% = x2fp% + dx2fp%
    WHILE x2fp% < 0: xx2% = xx2% - 1: x2fp% = x2fp% + 256: WEND
    WHILE x2fp% >= 256: xx2% = xx2% + 1: x2fp% = x2fp% - 256: WEND

    HorizontalLine o&, xx%, xx2%, colour%
  END IF
END IF
END SUB

SUB HorizontalLine (scanofs&, x1%, x2%, colour%)
IF x1% <= x2% THEN
  FOR X% = x1% TO x2%
    POKE scanofs& + X%, colour%
  NEXT X%
ELSE
  FOR X% = x2% TO x1%
    POKE scanofs& + X%, colour%
  NEXT X%
END IF
END SUB

SUB MultiplyMatrix (subject#(), transform#())
DIM result#(0 TO 2, 0 TO 2)

FOR i% = 0 TO 2
  FOR j% = 0 TO 2
    FOR k% = 0 TO 2
      result#(i%, j%) = result#(i%, j%) + subject#(i%, k%) * transform#(k%, j%)
    NEXT k%
  NEXT j%
NEXT i%

FOR i% = 0 TO 2
  FOR j% = 0 TO 2
    subject#(i%, j%) = result#(i%, j%)
  NEXT j%
NEXT i%
END SUB

SUB RotateX (matrix#(), angle#)
DIM transform#(0 TO 2, 0 TO 2)

ca# = COS(angle#)
sa# = SIN(angle#)

transform#(0, 0) = 1
transform#(1, 1) = ca#
transform#(1, 2) = sa#
transform#(2, 1) = -sa#
transform#(2, 2) = ca#

MultiplyMatrix matrix#(), transform#()
END SUB

SUB RotateY (matrix#(), angle#)
DIM transform#(0 TO 2, 0 TO 2)

ca# = COS(angle#)
sa# = SIN(angle#)

transform#(1, 1) = 1
transform#(0, 0) = ca#
transform#(2, 0) = sa#
transform#(0, 2) = -sa#
transform#(2, 2) = ca#

MultiplyMatrix matrix#(), transform#()
END SUB

SUB RotateZ (matrix#(), angle#)
DIM transform#(0 TO 2, 0 TO 2)

ca# = COS(angle#)
sa# = SIN(angle#)

transform#(2, 2) = 1
transform#(0, 0) = ca#
transform#(0, 1) = sa#
transform#(1, 0) = -sa#
transform#(1, 1) = ca#

MultiplyMatrix matrix#(), transform#()
END SUB

SUB SetIdentity (transform#())
FOR i% = 0 TO 2
  FOR j% = 0 TO 2
    transform#(i%, j%) = ABS(i% = j%)
  NEXT j%
NEXT i%
END SUB

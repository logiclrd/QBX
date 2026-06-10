'$DYNAMIC
TYPE PointType
  X AS DOUBLE
  Y AS DOUBLE
END TYPE

TYPE NodeType
  Position AS PointType
  ControlLeft AS PointType
  ControlRight AS PointType
END TYPE

DECLARE SUB AddPoint (p AS PointType, addend AS PointType)
DECLARE SUB FindClosestCurveAndOffset (Nodes() AS NodeType, findpoint AS PointType, curveindex%, curveoffset#)
DECLARE SUB FindClosestPointOnCurve (p0 AS PointType, p1 AS PointType, p2 AS PointType, p3 AS PointType, findpoint AS PointType, closestpoint AS PointType, closestpointoffset#)
DECLARE SUB InterpolatePoint (frompoint AS PointType, topoint AS PointType, t#, resultpoint AS PointType)
DECLARE SUB RenderCurve (p0 AS PointType, p1 AS PointType, p2 AS PointType, p3 AS PointType)
DECLARE SUB RenderPath (Nodes() AS NodeType)
DECLARE SUB RenderUI (Nodes() AS NodeType, forcesmooth%)
DECLARE SUB SplitCurve (Nodes() AS NodeType, splitindex%, splitt#)
DECLARE SUB SubtractPoint (p AS PointType, subtrahend AS PointType)
DECLARE SUB UpdateUI (Nodes() AS NodeType, forcesmooth%)
DECLARE FUNCTION AngleDifference# (a#, b#)
DECLARE FUNCTION ATAN2# (Y#, X#)
DECLARE FUNCTION Distance# (p0 AS PointType, p1 AS PointType)
DECLARE FUNCTION VectorLength# (v AS PointType)

CONST pi# = 3.14159265358979

CONST HandleSize# = 3
CONST DoubleClickInterval! = .4

TYPE RegTypeX
  AX AS INTEGER
  BX AS INTEGER
  CX AS INTEGER
  DX AS INTEGER
  BP AS INTEGER
  SI AS INTEGER
  DI AS INTEGER
  FLAGS AS INTEGER
  DS AS INTEGER
  ES AS INTEGER
END TYPE

DECLARE SUB InterruptX (intnum%, rin AS RegTypeX, rout AS RegTypeX)

DIM rin AS RegTypeX, rout AS RegTypeX

DIM Nodes(1 TO 2) AS NodeType

Nodes(1).Position.X = 320
Nodes(1).Position.Y = 240
Nodes(1).ControlRight.X = 40

Nodes(2).Position.X = 400
Nodes(2).Position.Y = 400
Nodes(2).ControlLeft.X = -40

SCREEN 12

DIM p0 AS PointType, p1 AS PointType, p2 AS PointType
DIM clickp AS PointType, dragp AS PointType
DIM delta AS PointType

' Sane initial value that won't trigger a false positive
clicktime! = TIMER - 2 * DoubleClickInterval!

dragnodeindex% = 0 ' non-zero = dragging
dragpointtype% = 0 ' 0 = node, 1 = control left, 2 = control right

forcesmooth% = -1

RenderUI Nodes(), forcesmooth%

' Reset mouse driver & show the mouse
rin.AX = 0: CALL InterruptX(&H33, rin, rout)
rin.AX = 1: CALL InterruptX(&H33, rin, rout)

DO
  rin.AX = 5: CALL InterruptX(&H33, rin, rout)

  buttonpresscount% = rout.BX

  IF buttonpresscount% <> 0 THEN
    clickp.X = rout.CX
    clickp.Y = rout.DX

    timesincelastclick! = TIMER - clicktime!

    IF timesincelastclick! < -43200 THEN
      ' Midnight passed since the last click
      timesincelastclick! = timesincelastclick! + 86400
    END IF

    IF timesincelastclick! < DoubleClickInterval! THEN
      ' Prevent accidental double-trigger on third click
      timesincelastclick! = 0

      FindClosestCurveAndOffset Nodes(), clickp, insertafter%, insertt#

      lastnodeindex% = UBOUND(Nodes)

      IF insertafter% = 1 THEN
        angletoclick# = ATAN2(clickp.Y - Nodes(1).Position.Y, clickp.X - Nodes(1).Position.X)
        angletocurve# = ATAN2(Nodes(1).ControlRight.Y, Nodes(1).ControlRight.X)

        IF ABS(AngleDifference(angletoclick#, angletocurve#)) >= pi# THEN
          ' Click before start
          REDIM PRESERVE Nodes(1 TO UBOUND(Nodes) + 1) AS NodeType

          FOR i% = UBOUND(Nodes) TO 2 STEP -1
            Nodes(i%) = Nodes(i% - 1)
          NEXT i%

          Nodes(1).Position = clickp

          Nodes(1).ControlRight = Nodes(2).Position
          SubtractPoint Nodes(1).ControlRight, Nodes(1).Position

          Nodes(1).ControlRight.X = Nodes(1).ControlRight.X * .5
          Nodes(1).ControlRight.Y = Nodes(1).ControlRight.Y * .5

          Nodes(1).ControlLeft.X = -Nodes(1).ControlRight.X
          Nodes(1).ControlLeft.Y = -Nodes(1).ControlRight.Y

          insertafter% = -1
        END IF
      ELSEIF insertafter% = lastnodeindex% - 1 THEN
        angletoclick# = ATAN2(clickp.Y - Nodes(lastnodeindex%).Position.Y, clickp.X - Nodes(lastnodeindex%).Position.X)
        angletocurve# = ATAN2(Nodes(lastnodeindex%).ControlRight.Y, Nodes(lastnodeindex%).ControlRight.X)

        IF ABS(AngleDifference(angletoclick#, angletocurve#)) >= pi# THEN
          ' Click after end
          REDIM PRESERVE Nodes(1 TO UBOUND(Nodes) + 1) AS NodeType

          newnodeindex% = UBOUND(Nodes)

          Nodes(newnodeindex%).Position = clickp

          Nodes(newnodeindex%).ControlRight = Nodes(newnodeindex% - 1).Position
          SubtractPoint Nodes(newnodeindex%).ControlRight, Nodes(newnodeindex%).Position

          Nodes(newnodeindex%).ControlLeft.X = Nodes(newnodeindex%).ControlLeft.X * .5
          Nodes(newnodeindex%).ControlLeft.Y = Nodes(newnodeindex%).ControlLeft.Y * .5

          Nodes(newnodeindex%).ControlRight.X = -Nodes(newnodeindex%).ControlLeft.X
          Nodes(newnodeindex%).ControlRight.Y = -Nodes(newnodeindex%).ControlLeft.Y
        END IF
      END IF
     
      IF insertafter% > 0 THEN
        SplitCurve Nodes(), insertafter%, insertt#

        newnodeindex% = insertafter% + 1

        Nodes(newnodeindex%).Position = clickp
      END IF

      UpdateUI Nodes(), forcesmooth%
    END IF

    clicktime! = TIMER

    dragnodeindex% = -1 ' still track the drag even if dragging nothing

    FOR i% = 1 TO UBOUND(Nodes)
      p0 = Nodes(i%).Position
      p1 = Nodes(i%).Position
      p2 = Nodes(i%).Position

      AddPoint p0, Nodes(i%).ControlLeft
      AddPoint p2, Nodes(i%).ControlRight

      IF Distance(p1, clickp) <= HandleSize# THEN
        dragnodeindex% = i%: dragpointtype% = 0
      ELSEIF Distance(p0, clickp) <= HandleSize# THEN
        dragnodeindex% = i%: dragpointtype% = 1
      ELSEIF Distance(p2, clickp) <= HandleSize# THEN
        dragnodeindex% = i%: dragpointtype% = 2
      END IF

      IF dragnodeindex% > 0 THEN
        dragp = clickp
        EXIT FOR
      END IF
    NEXT i%
  ELSEIF dragnodeindex% <> 0 THEN
    ' Query position and check for release
    rin.AX = 6: CALL InterruptX(&H33, rin, rout)

    buttonreleasecount% = rout.BX

    IF buttonreleasecount% <> 0 THEN
      dragnodeindex% = 0
    ELSE
      rin.AX = 3: CALL InterruptX(&H33, rin, rout)
    END IF

    IF dragnodeindex% > 0 THEN ' could be dragging nothing
      clickp.X = rout.CX
      clickp.Y = rout.DX

      p0 = Nodes(i%).Position
      p1 = Nodes(i%).Position

      SELECT CASE dragpointtype%
        CASE 1: AddPoint p0, Nodes(i%).ControlLeft
        CASE 2: AddPoint p0, Nodes(i%).ControlRight
      END SELECT

      delta = clickp
      SubtractPoint delta, dragp

      AddPoint p0, delta

      IF dragpointtype% <> 0 THEN
        p2 = p0
        SubtractPoint p0, p1
      END IF

      SELECT CASE dragpointtype%
        CASE 0: Nodes(i%).Position = p0
        CASE 1: Nodes(i%).ControlLeft = p0
        CASE 2: Nodes(i%).ControlRight = p0
      END SELECT

      IF forcesmooth% AND (dragpointtype% <> 0) THEN
        ' Update the other control point to match the angle
        SELECT CASE dragpointtype%
          CASE 1: p2 = Nodes(i%).ControlRight
          CASE 2: p2 = Nodes(i%).ControlLeft
        END SELECT

        angle# = ATAN2#(-p0.Y, -p0.X)
        magnitude# = VectorLength(p2)

        p2.X = magnitude# * COS(angle#)
        p2.Y = magnitude# * SIN(angle#)

        SELECT CASE dragpointtype%
          CASE 1: Nodes(i%).ControlRight = p2
          CASE 2: Nodes(i%).ControlLeft = p2
        END SELECT
      END IF

      dragp = clickp

      UpdateUI Nodes(), forcesmooth%
    END IF
  END IF

  k$ = INKEY$

  SELECT CASE k$
    CASE "s", "S": forcesmooth% = NOT forcesmooth%: UpdateUI Nodes(), forcesmooth%
    CASE IS <> "": EXIT DO
  END SELECT
LOOP

END

' QBASIC supports neither ON ERROR RESUME NEXT nor ON LOCAL ERROR ...
' This catches math errors in FUNCTION ATAN2!
ATAN2HandleMathError:
  ATAN2CaughtError% = 0
  RESUME NEXT

REM $STATIC
SUB AddPoint (p AS PointType, addend AS PointType)
p.X = p.X + addend.X
p.Y = p.Y + addend.Y
END SUB

FUNCTION AngleDifference# (a#, b#)
aa# = a#
bb# = b#

WHILE aa# > 2 * pi#: aa# = aa# - 2 * pi#: WEND
WHILE bb# > 2 * pi#: bb# = bb# - 2 * pi#: WEND

d# = aa# - bb#

IF d# > pi# THEN d# = d# - 2 * pi#
IF d# < -pi# THEN d# = d# + 2 * pi#

AngleDifference# = d#
END FUNCTION

FUNCTION ATAN2# (Y#, X#)
IF X# = Y# THEN
  IF Y# > 0 THEN
    ATAN2# = pi# * .25
  ELSE
    ATAN2# = pi# * 1.25
  END IF
ELSEIF X# = -Y# THEN
  IF Y# > 0 THEN
    ATAN2# = pi# * .75
  ELSE
    ATAN2# = pi# * 1.75
  END IF
ELSE
  isusable% = -1

  ON ERROR GOTO ATAN2HandleMathError

  ATAN2CaughtError% = 0
  slope# = Y# / X#

  ON ERROR GOTO 0

  IF ATAN2CaughtError% THEN isusable% = 0

  IF isusable% THEN
    angle# = ATN(slope#)

    IF X# < 0 THEN angle# = angle# + pi#
    IF angle# < 0 THEN angle# = angle# + 2 * pi#

    ATAN2# = angle#
  ELSEIF Y# <> 0 THEN
    IF (Y# > 0) EQV (X# > 0) THEN
      ATAN2# = pi# * .5
    ELSE
      ATAN2# = -pi# * .5
    END IF
  END IF
END IF
END FUNCTION

FUNCTION Distance# (p0 AS PointType, p1 AS PointType)
Distance# = SQR((p1.X - p0.X) ^ 2 + (p1.Y - p0.Y) ^ 2)
END FUNCTION

SUB FindClosestCurveAndOffset (Nodes() AS NodeType, findpoint AS PointType, curveindex%, curveoffset#)
DIM p0 AS PointType
DIM p1 AS PointType
DIM p2 AS PointType
DIM p3 AS PointType

DIM closestpoint AS PointType
DIM closestpointtangent AS PointType

bestmatchd# = Distance(Nodes(1).Position, findpoint)
curveindex% = 1
curveoffset# = 0

FOR i% = 1 TO UBOUND(Nodes) - 1
  p0 = Nodes(i%).Position
  p1 = Nodes(i%).Position
  p2 = Nodes(i% + 1).Position
  p3 = Nodes(i% + 1).Position

  AddPoint p1, Nodes(i%).ControlRight
  AddPoint p2, Nodes(i% + 1).ControlLeft

  FindClosestPointOnCurve p0, p1, p2, p3, findpoint, closestpoint, closestpointoffset#

  d# = Distance(closestpoint, findpoint)

  IF d# < bestmatchd# THEN
    bestmatchd# = d#
    curveindex% = i%
    curveoffset# = closestpointoffset#
  END IF
NEXT i%
END SUB

SUB FindClosestPointOnCurve (p0 AS PointType, p1 AS PointType, p2 AS PointType, p3 AS PointType, findpoint AS PointType, closestpoint AS PointType, closestpointoffset#)
a01# = ATAN2#(p1.Y - p0.Y, p1.X - p0.X)
a03# = ATAN2#(p3.Y - p0.Y, p3.X - p0.X)
a23# = ATAN2#(p3.Y - p2.Y, p3.Y - p2.Y)

d01# = Distance(p0, p1)
d03# = Distance(p0, p3)
d23# = Distance(p2, p3)

d# = d03# + d01# * ABS(AngleDifference(a01#, a03#)) + d23# * ABS(AngleDifference(a23#, a03#))

steps% = FIX(d#) + 1

DIM p10 AS PointType, p21 AS PointType, p32 AS PointType

p10 = p1: SubtractPoint p10, p0
p21 = p2: SubtractPoint p21, p1
p32 = p3: SubtractPoint p32, p2

DIM p AS PointType
DIM besttangent AS PointType

bestpd# = Distance(p0, findpoint)

closestpoint = p0
closestpointoffset# = 0

FOR i% = 0 TO steps%
  t# = i% / steps%
  it# = 1 - t#

  t2# = t# * t#
  it2# = it# * it#

  c0# = it2# * it#
  c1# = it2# * t# * 3
  c2# = it# * t2# * 3
  c3# = t# * t2#

  p.X = c0# * p0.X + c1# * p1.X + c2# * p2.X + c3# * p3.X
  p.Y = c0# * p0.Y + c1# * p1.Y + c2# * p2.Y + c3# * p3.Y

  pd# = Distance(p, findpoint)

  IF pd# < bestpd# THEN
    bestpd# = pd#

    closestpoint = p
    closestpointoffset# = t#
  END IF
NEXT i%
END SUB

SUB InterpolatePoint (frompoint AS PointType, topoint AS PointType, t#, resultpoint AS PointType)
it# = 1 - t#

resultpoint.X = it# * frompoint.X + t# * topoint.X
resultpoint.Y = it# * frompoint.Y + t# * topoint.Y
END SUB

SUB RenderCurve (p0 AS PointType, p1 AS PointType, p2 AS PointType, p3 AS PointType)
a01# = ATAN2#(p1.Y - p0.Y, p1.X - p0.X)
a03# = ATAN2#(p3.Y - p0.Y, p3.X - p0.X)
a23# = ATAN2#(p3.Y - p2.Y, p3.Y - p2.Y)

d01# = Distance(p0, p1)
d03# = Distance(p0, p3)
d23# = Distance(p2, p3)

d# = d03# + d01# * ABS(AngleDifference(a01#, a03#)) + d23# * ABS(AngleDifference(a23#, a03#))

steps% = FIX(d#) + 1

FOR i% = 0 TO steps%
  t# = i% / steps%
  it# = 1 - t#

  t2# = t# * t#
  it2# = it# * it#

  c0# = it2# * it#
  c1# = it2# * t# * 3
  c2# = it# * t2# * 3
  c3# = t# * t2#

  px# = c0# * p0.X + c1# * p1.X + c2# * p2.X + c3# * p3.X
  py# = c0# * p0.Y + c1# * p1.Y + c2# * p2.Y + c3# * p3.Y

  IF i% = 0 THEN PSET (px#, py#) ELSE LINE -(px#, py#)
NEXT i%
END SUB

SUB RenderPath (Nodes() AS NodeType)
DIM p0 AS PointType
DIM p1 AS PointType
DIM p2 AS PointType
DIM p3 AS PointType

FOR i% = 1 TO UBOUND(Nodes)
  p0 = Nodes(i%).Position
  p1 = Nodes(i%).Position
  p2 = Nodes(i%).Position

  AddPoint p0, Nodes(i%).ControlLeft
  AddPoint p2, Nodes(i%).ControlRight

  LINE (p1.X, p1.Y)-(p0.X, p0.Y), 8
  LINE (p1.X, p1.Y)-(p2.X, p2.Y), 8
NEXT i%

FOR i% = 1 TO UBOUND(Nodes) - 1
  p0 = Nodes(i%).Position
  p1 = Nodes(i%).Position
  p2 = Nodes(i% + 1).Position
  p3 = Nodes(i% + 1).Position

  AddPoint p1, Nodes(i%).ControlRight
  AddPoint p2, Nodes(i% + 1).ControlLeft

  RenderCurve p0, p1, p2, p3
NEXT i%

FOR i% = 1 TO UBOUND(Nodes)
  p0 = Nodes(i%).Position
  p2 = Nodes(i%).Position

  AddPoint p0, Nodes(i%).ControlLeft
  AddPoint p2, Nodes(i%).ControlRight

  CIRCLE (p0.X, p0.Y), 3, 14
  CIRCLE (p2.X, p2.Y), 3, 14

  CIRCLE (Nodes(i%).Position.X, Nodes(i%).Position.Y), 3, 10
NEXT i%
END SUB

SUB RenderUI (Nodes() AS NodeType, forcesmooth%)
CLS
RenderPath Nodes()
IF forcesmooth% THEN
  LOCATE 1, 1
  PRINT "SMOOTH"
END IF
END SUB

SUB SplitCurve (Nodes() AS NodeType, splitindex%, splitt#)
REDIM PRESERVE Nodes(1 TO UBOUND(Nodes) + 1) AS NodeType

FOR i% = UBOUND(Nodes) TO splitindex% + 2 STEP -1
  Nodes(i%) = Nodes(i% - 1)
NEXT i%

' De Casteljau algorithm
DIM p0 AS PointType, p1 AS PointType, p2 AS PointType, p3 AS PointType

p0 = Nodes(splitindex%).Position
p1 = Nodes(splitindex%).Position
p2 = Nodes(splitindex% + 1).Position
p3 = Nodes(splitindex% + 1).Position

AddPoint p1, Nodes(splitindex%).ControlRight
AddPoint p2, Nodes(splitindex% + 1).ControlLeft

DIM p01 AS PointType, p12 AS PointType, p23 AS PointType

InterpolatePoint p0, p1, splitt#, p01
InterpolatePoint p1, p2, splitt#, p12
InterpolatePoint p2, p3, splitt#, p23

DIM p02 AS PointType, p13 AS PointType

InterpolatePoint p01, p12, splitt#, p02
InterpolatePoint p12, p23, splitt#, p13

' Final interpolation:
InterpolatePoint p02, p13, splitt#, Nodes(splitindex% + 1).Position

' Fix up control points
Nodes(splitindex%).ControlRight = p01
Nodes(splitindex% + 1).ControlLeft = p02
Nodes(splitindex% + 1).ControlRight = p13
Nodes(splitindex% + 2).ControlLeft = p23

SubtractPoint Nodes(splitindex%).ControlRight, Nodes(splitindex%).Position
SubtractPoint Nodes(splitindex% + 1).ControlLeft, Nodes(splitindex% + 1).Position
SubtractPoint Nodes(splitindex% + 1).ControlRight, Nodes(splitindex% + 1).Position
SubtractPoint Nodes(splitindex% + 2).ControlLeft, Nodes(splitindex% + 2).Position
END SUB

SUB SubtractPoint (p AS PointType, subtrahend AS PointType)
p.X = p.X - subtrahend.X
p.Y = p.Y - subtrahend.Y
END SUB

SUB UpdateUI (Nodes() AS NodeType, forcesmooth%)
DIM rin AS RegTypeX, rout AS RegTypeX

rin.AX = 2: CALL InterruptX(&H33, rin, rout)
RenderUI Nodes(), forcesmooth%
rin.AX = 1: CALL InterruptX(&H33, rin, rout)
END SUB

FUNCTION VectorLength# (v AS PointType)
VectorLength# = SQR(v.X * v.X + v.Y * v.Y)
END FUNCTION


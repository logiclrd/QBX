DECLARE SUB SetTileColour (tilex%, tiley%)
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

TYPE TileConfiguration
  CentreX AS SINGLE
  CentreY AS SINGLE

  Path1 AS INTEGER
  Path2 AS INTEGER
  Path3 AS INTEGER
END TYPE

DECLARE SUB RotateTile (tiledata AS TileConfiguration)
DECLARE SUB DrawHextile (r!, tiledata AS TileConfiguration)
DECLARE SUB ComputePathParameters (r!, p%, CentreX!, CentreY!, radius!)

CONST COS30! = .8660254  ' COS 30 degrees = sqrt(3) / 2
CONST SIN30! = .5        ' SIN 30 degrees =      1  / 2

' Hextile Parameters
CONST PathThicknessProportion! = .3      ' Proportion of edge length
CONST PathWallThicknessProportion! = .07 ' Proportion of edge length

DIM SHARED BackColour%, PathColour%, WallColour%

BackColour% = 7
PathColour% = 15
WallColour% = 0

CONST TileRadius! = 30

CONST TileColumnWidth! = TileRadius! * (2 * COS30!)
CONST TileRowHeight! = TileRadius! * (2 - SIN30!)

tilecolumns% = 1 + FIX((320 + TileColumnWidth! * .5) / TileColumnWidth!)
tilerows% = 1 + FIX((200 + TileRowHeight!) / TileRowHeight!)

DIM rin AS RegTypeX, rout AS RegTypeX

SCREEN 13

' Generate initial tile data
DIM tiledata(tilecolumns% - 1, tilerows% - 1) AS TileConfiguration

rowx! = 0
rowy! = 0

tiley% = 0

DO
  x! = rowx!
  y! = rowy!

  tilex% = 0

  DO
    flip% = -2 * (RND < .5)
    rotation% = RND * 6

    tiledata(tilex%, tiley%).CentreX = x!
    tiledata(tilex%, tiley%).CentreY = y!

    tiledata(tilex%, tiley%).Path1 = (flip% + 2 * rotation%) MOD 12
    tiledata(tilex%, tiley%).Path2 = (2 - flip% + 2 * rotation%) MOD 12
    tiledata(tilex%, tiley%).Path3 = (7 + 2 * rotation%) MOD 12

    SetTileColour tilex%, tiley%

    DrawHextile TileRadius!, tiledata(tilex%, tiley%)

    x! = x! + TileRadius! * (2 * COS30!)
    tilex% = tilex% + 1
  LOOP UNTIL x! > 320 + TileRadius!

  IF tiley% AND 1 THEN
    rowx! = rowx! - TileColumnWidth! * .5
  ELSE
    rowx! = rowx! + TileColumnWidth! * .5
  END IF

  rowy! = rowy! + TileRowHeight!
  tiley% = tiley% + 1
LOOP UNTIL tiley% = tilerows%

' Reset mouse driver & show the mouse
rin.AX = 0: CALL InterruptX(&H33, rin, rout)
rin.AX = 1: CALL InterruptX(&H33, rin, rout)

' Wait for a click
DO
  rin.AX = 5: CALL InterruptX(&H33, rin, rout)

  buttonpresscount% = rout.BX

  IF buttonpresscount% <> 0 THEN
    clickx% = rout.CX \ 2 ' Mouse driver always reports 0..639
    clicky% = rout.DX

    ' I'm sure there's a faster way to do this
    bestd! = 1000000
    closesttilex% = -1
    closesttiley% = -1

    FOR x% = 0 TO tilecolumns% - 1
      FOR y% = 0 TO tilerows% - 1
        deltax! = clickx% - tiledata(x%, y%).CentreX
        deltay! = clicky% - tiledata(x%, y%).CentreY

        d! = deltax! * deltax! + deltay! * deltay!

        IF d! < bestd! THEN
          closesttilex% = x%
          closesttiley% = y%

          bestd! = d!
        END IF
      NEXT y%
    NEXT x%

    IF closesttilex% >= 0 THEN
      RotateTile tiledata(closesttilex%, closesttiley%)

      ' Hide mouse, redraw, show mouse
      rin.AX = 2: CALL InterruptX(&H33, rin, rout)

      SetTileColour closesttilex%, closesttiley%

      DrawHextile TileRadius!, tiledata(closesttilex%, closesttiley%)

      rin.AX = 1: CALL InterruptX(&H33, rin, rout)
    END IF
  END IF
LOOP WHILE INKEY$ = ""

SUB ComputePathParameters (r!, p%, CentreX!, CentreY!, radius!)
' The circle representing a path segment is always at the
' intersection of the two edges being bridged.
'
' The radius of the circle is the distance to the midpoint of
' either one of those edges.

CONST WEDGERADIANS! = 60 / 57.295779

IF (p% AND 1) = 0 THEN
  ' This path is joining two edges spaced out by another edge.
  edge1A! = (p% \ 2 - 1) * WEDGERADIANS!
  edge2A! = (p% \ 2 + 1) * WEDGERADIANS!
ELSE
  ' This path is joinig two adjacent edges.
  edge1A! = (p% \ 2) * WEDGERADIANS!
  edge2A! = (p% \ 2 + 1) * WEDGERADIANS!
END IF

edge1x1! = r! * COS(edge1A! - .5 * WEDGERADIANS!)
edge1y1! = r! * SIN(edge1A! - .5 * WEDGERADIANS!)
edge1x2! = r! * COS(edge1A! + .5 * WEDGERADIANS!)
edge1y2! = r! * SIN(edge1A! + .5 * WEDGERADIANS!)

edge2x1! = r! * COS(edge2A! - .5 * WEDGERADIANS!)
edge2y1! = r! * SIN(edge2A! - .5 * WEDGERADIANS!)
edge2x2! = r! * COS(edge2A! + .5 * WEDGERADIANS!)
edge2y2! = r! * SIN(edge2A! + .5 * WEDGERADIANS!)

' We need the lines to be represented in the form
' Ax + By = C.
edge1A! = edge1y2! - edge1y1!
edge1B! = edge1x1! - edge1x2!
edge1C! = edge1A! * edge1x1! + edge1B! * edge1y1!

edge2A! = edge2y2! - edge2y1!
edge2B! = edge2x1! - edge2x2!
edge2C! = edge2A! * edge2x1! + edge2B! * edge2y1!

' Now the intersection is fairly straightforward to
' calculate. The derivation of this formula is left
' as an exercise to the reader.
denominator! = edge2A! * edge1B! - edge1A! * edge2B!

CentreX! = (edge1B! * edge2C! - edge2B! * edge1C!) / denominator!
CentreY! = (edge2A! * edge1C! - edge1A! * edge2C!) / denominator!

' Finally, to get the radius, we just need to pick one of the nidpoints.
entryx! = (edge1x1! + edge1x2!) * .5
entryy! = (edge1y1! + edge1y2!) * .5

DX! = entryx! - CentreX!
dy! = entryy! - CentreY!

radius! = SQR(DX! * DX! + dy! * dy!)
END SUB

SUB DrawHextile (r!, tiledata AS TileConfiguration)
' Calculate the parameters of the hexagon
topy% = tiledata.CentreY - r!
bottomy% = tiledata.CentreY + r!

leftx% = tiledata.CentreX - r! * COS30!
rightx% = tiledata.CentreX + r! * COS30!

sidetopy% = tiledata.CentreY - r! * SIN30!
sidebottomy% = tiledata.CentreY + r! * SIN30!

wedgeheight% = sidetopy% - topy%
middler! = (rightx% - leftx%) * .5

' The path pieces are specified with an index. Even indices
' bridge across two edges with a gap between them; the index
' identifies the edge in between. The edges are 0-2-4-6-8-10.
' Odd indices bridge across two adjacent edges; the index
' identifies the vertex in between. The vertices are 1-3-5-7-9-11.
ComputePathParameters r!, tiledata.Path1, path1x!, path1y!, path1r!
ComputePathParameters r!, tiledata.Path2, path2x!, path2y!, path2r!
ComputePathParameters r!, tiledata.Path3, path3x!, path3y!, path3r!

halfpaththickness! = r! * PathThicknessProportion! * .5
halfwallthickness! = r! * PathWallThicknessProportion! * .5

path1startr! = path1r! - halfpaththickness!
path1endr! = path1r! + halfpaththickness!
path1innerstartr! = path1startr! + halfwallthickness!
path1innerendr! = path1endr! - halfwallthickness!

path2startr! = path2r! - halfpaththickness!
path2endr! = path2r! + halfpaththickness!
path2innerstartr! = path2startr! + halfwallthickness!
path2innerendr! = path2endr! - halfwallthickness!

path3startr! = path3r! - halfpaththickness!
path3endr! = path3r! + halfpaththickness!
path3innerstartr! = path3startr! + halfwallthickness!
path3innerendr! = path3endr! - halfwallthickness!

' Trick: Since we're just testing greater than/less than,
' we can avoid square roots by comparing the square of the
' distance (easier to calculate) against the square of the
' threshold.
path1startr! = path1startr! ^ 2
path1endr! = path1endr! ^ 2
path1innerstartr! = path1innerstartr! ^ 2
path1innerendr! = path1innerendr! ^ 2

path2startr! = path2startr! ^ 2
path2endr! = path2endr! ^ 2
path2innerstartr! = path2innerstartr! ^ 2
path2innerendr! = path2innerendr! ^ 2

path3startr! = path3startr! ^ 2
path3endr! = path3endr! ^ 2
path3innerstartr! = path3innerstartr! ^ 2
path3innerendr! = path3innerendr! ^ 2

' Now scan through the pixels. For each pixel, figure out its
' (non-integer) position in tile space, and use that to figure
' out if we're on one of the links.
y1% = topy%
y2% = bottomy%

IF y1% < 0 THEN y1% = 0
IF y2% > 199 THEN y2% = 199

tiley! = tiledata.CentreY - y1%

FOR pixely% = y1% TO y2%
  IF pixely% < sidetopy% THEN
    r2! = (pixely% - topy%) * middler! / wedgeheight%
  ELSEIF pixely% > sidebottomy% THEN
    r2! = (bottomy% - pixely%) * middler! / wedgeheight%
  ELSE
    r2! = middler!
  END IF

  pixelx1% = tiledata.CentreX - r2!
  pixelx2% = tiledata.CentreX + r2!

  tilex! = pixelx1% - tiledata.CentreX

  FOR pixelx% = pixelx1% TO pixelx2%
    c% = BackColour%

    deltax! = tilex! - path1x!
    deltay! = tiley! - path1y!

    d! = deltax! * deltax! + deltay! * deltay!

    IF d! > path1startr! AND d! < path1endr! THEN
      IF d! > path1innerstartr! AND d! < path1innerendr! THEN
        c% = PathColour%
      ELSE
        c% = WallColour%
      END IF
    ELSE
      deltax! = tilex! - path2x!
      deltay! = tiley! - path2y!

      d! = deltax! * deltax! + deltay! * deltay!

      IF d! > path2startr! AND d! < path2endr! THEN
        IF d! > path2innerstartr! AND d! < path2innerendr! THEN
          c% = PathColour%
        ELSE
          c% = WallColour%
        END IF
      ELSE
        deltax! = tilex! - path3x!
        deltay! = tiley! - path3y!

        d! = deltax! * deltax! + deltay! * deltay!

        IF d! > path3startr! AND d! < path3endr! THEN
          IF d! > path3innerstartr! AND d! < path3innerendr! THEN
            c% = PathColour%
          ELSE
            c% = WallColour%
          END IF
        END IF
      END IF
    END IF

    PSET (pixelx%, pixely%), c%

    tilex! = tilex! + 1
  NEXT pixelx%

  tiley! = tiley! - 1
NEXT pixely%
END SUB

SUB RotateTile (tiledata AS TileConfiguration)
tiledata.Path1 = (tiledata.Path1 + 2) MOD 12
tiledata.Path2 = (tiledata.Path2 + 2) MOD 12
tiledata.Path3 = (tiledata.Path3 + 2) MOD 12
END SUB

SUB SetTileColour (tilex%, tiley%)
BackColour% = 20 + 3 * ((tilex% + 3 - (tiley% AND 1)) MOD 3)
END SUB

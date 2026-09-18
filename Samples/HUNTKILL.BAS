DECLARE SUB LoadImage (filename$, image%())
DECLARE SUB GenerateMazeHuntAndKillImage (maze%(), image%())
DECLARE SUB GenerateMazeHuntAndKill (maze%(), straightlinebias%, axisbias%)
DECLARE SUB DrawMaze (maze%())

CONST MazeWidth% = 45
CONST MazeHeight% = 45

CONST OpenRight% = 1
CONST OpenDown% = 2
CONST Connected% = 4
CONST ActivePath% = 8 ' For visualization; strip this out for production

' maze%(x%, y%) is a bit mask:
'
'   Connected%: Indicates that this cell is connected to the maze.
'   OpenRight%: Indicates that the wall to the right has been removed
'   OpenDown%: Indicates that the wall to the bottom has been removed
'
' What about the walls to the left and the top? They are specified by
' maze%(x% - 1, y%) and maze%(x%, y% - 1).
'
' Cells with x% = 1 or y% = 1 always implicitly have walls to the left
' and the top.
'
' Cells with x% = MazeWidth% or y% = MazeHeight% always explicitly have
' walls to the right and the bottom. They still have array entries to
' track Connected%.

DIM maze%(1 TO MazeWidth%, 1 TO MazeHeight%)
DIM image%(1 TO MazeWidth%, 1 TO MazeHeight%)

SCREEN 12

DO
  CLS

  PRINT "1 - Basic maze"
  PRINT "2 - Bias toward straight lines"
  PRINT "3 - Bias toward vertical lines"
  PRINT "4 - Bias toward horizontal lines"
  PRINT "5 - Image: BOXES"
  PRINT "6 - Image: SIMON"
  PRINT "ESC - Exit"

  DO
    a$ = INKEY$
  LOOP WHILE a$ = ""

  sel% = VAL(a$)

  IF sel% >= 1 AND sel% <= 6 THEN
    FOR x% = 0 TO 639
      FOR y% = (sel% - 1) * 16 TO sel% * 16 - 1
        PSET (x%, y%), POINT(x%, y%) XOR 15
      NEXT y%
    NEXT x%

    st# = TIMER
    FOR delay% = 1 TO 5
      WHILE st# = TIMER: WEND
      st# = TIMER
    NEXT delay%

    CLS

    SELECT CASE sel%
      CASE 1:
        GenerateMazeHuntAndKill maze%(), 0, 0 ' no bias to straight lines
      CASE 2:
        GenerateMazeHuntAndKill maze%(), 5, 0 ' prefer straight lines
      CASE 3:
        GenerateMazeHuntAndKill maze%(), 0, 1 ' prefer vertical axis
      CASE 4:
        GenerateMazeHuntAndKill maze%(), 0, 2 ' prefer horizontal axis
      CASE 5:
        LoadImage "BOXES.RAW", image%()
        GenerateMazeHuntAndKillImage maze%(), image%()
      CASE 6:
        LoadImage "SIMON.RAW", image%()
        GenerateMazeHuntAndKillImage maze%(), image%()
    END SELECT

    DrawMaze maze%()
    WHILE INKEY$ = "": WEND
  END IF
LOOP UNTIL ASC(a$) = 27

SCREEN 0


SUB DrawMaze (maze%())
x1% = LBOUND(maze%, 1)
y1% = LBOUND(maze%, 2)

x2% = UBOUND(maze%, 1)
y2% = UBOUND(maze%, 2)

w% = x2% - x1% + 1
h% = y2% - y1% + 1

cellsize% = 479 \ h%

pw% = w% * cellsize%
ph% = h% * cellsize%

px% = 320 - pw% \ 2
py% = 240 - ph% \ 2

' Top and left borders
LINE (px%, py%)-STEP(pw%, 0)
LINE (px%, py%)-STEP(0, ph%)

yy% = py%

FOR y% = y1% TO y2%
  xx% = px%

  FOR x% = x1% TO x2%
    IF maze%(x%, y%) AND ActivePath% THEN
      fill% = 5
    ELSEIF maze%(x%, y%) AND Connected% THEN
      fill% = 0
    ELSE
      fill% = 8
    END IF

    IF (maze%(x%, y%) AND OpenRight%) = 0 THEN rightcol% = 15 ELSE rightcol% = fill%
    IF (maze%(x%, y%) AND OpenDown%) = 0 THEN downcol% = 15 ELSE downcol% = fill%

    LINE (xx% + 1, yy% + 1)-STEP(cellsize% - 2, cellsize% - 2), fill%, BF
    LINE (xx% + cellsize%, yy% + 1)-STEP(0, cellsize% - 2), rightcol%
    LINE (xx% + 1, yy% + cellsize%)-STEP(cellsize% - 2, 0), downcol%
    PSET (xx% + cellsize%, yy% + cellsize%), 15

    xx% = xx% + cellsize%
  NEXT x%

  yy% = yy% + cellsize%
NEXT y%
END SUB

SUB GenerateMazeHuntAndKill (maze%(), straightlinebias%, axisbias%)
IF (straightlinebias% <> 0) AND (axisbias% <> 0) THEN ERROR 5
IF (straightlinebias% < 0) OR (axisbias% < 0) OR (axisbias% > 2) THEN ERROR 5

x1% = LBOUND(maze%, 1)
y1% = LBOUND(maze%, 2)

x2% = UBOUND(maze%, 1)
y2% = UBOUND(maze%, 2)

' Reset maze
FOR y% = y1% TO y2%
  FOR x% = x1% TO x2%
    maze%(x%, y%) = 0
  NEXT x%
NEXT y%

DIM scan%(0 TO 3 + straightlinebias% + axisbias% * 8)

' Kick things off at a random cell
tx% = INT(RND * (x2% - x1% + 1)) + x1%
ty% = INT(RND * (y2% - y1% + 1)) + y1%

x% = tx%
y% = ty%

dir% = 0

axis% = axisbias% AND 2

DO
  ' Connect tx%, ty% to x%, y%, then set x%, y% to tx%, ty%
  ' and find a new tx%, ty%, until we hit a dead end.
  DO
    maze%(tx%, ty%) = maze%(tx%, ty%) OR (Connected% OR ActivePath%)

    ' Seed cell won't match any of these. That's okay. :-)
    IF tx% > x% THEN
      maze%(x%, y%) = maze%(x%, y%) OR OpenRight%
    ELSEIF ty% > y% THEN
      maze%(x%, y%) = maze%(x%, y%) OR OpenDown%
    ELSEIF tx% < x% THEN
      maze%(tx%, ty%) = maze%(tx%, ty%) OR OpenRight%
    ELSEIF ty% < y% THEN
      maze%(tx%, ty%) = maze%(tx%, ty%) OR OpenDown%
    END IF

    DrawMaze maze%()

    ' Check for adjacent cells in a random order
    maxscan% = 3

    FOR i% = 0 TO 3
      scan%(i%) = i%
    NEXT i%

    ' Do not go backward.
    scan%(dir% XOR 1) = dir%

    IF dir% = -1 THEN
      maxscan% = 3
    ELSEIF straightlinebias% <> 0 THEN
      ' Prioritize going straight
      maxscan% = UBOUND(scan%)

      FOR i% = 4 TO maxscan%
        scan%(i%) = dir%
      NEXT i%
    ELSEIF axisbias% THEN
      ' Prefer one axis or the other. First load up scan%(4) and scan%(5)
      ' with the direction(s) matching the selected axis, then fill the
      ' remainder of the buffer with copies of these two slots.
      maxscan% = UBOUND(scan%)

      FOR i% = 0 TO maxscan%
        IF (scan%(i%) AND 2) = axis% THEN
          scan%(4) = scan%(i%)
          scan%(5) = scan%(i%)
          EXIT FOR
        END IF
      NEXT i%

      FOR i% = i% + 1 TO maxscan%
        IF (scan%(i%) AND 2) = axis% THEN
          scan%(5) = scan%(i%)
          EXIT FOR
        END IF
      NEXT i%

      FOR i% = 6 TO maxscan%
        scan%(i%) = scan%(4 + (i% AND 1))
      NEXT i%
    END IF

    FOR i% = maxscan% TO 1 STEP -1
      SWAP scan%(i%), scan%(INT(RND * (i% + 1)))
    NEXT i%

    x% = tx%
    y% = ty%

    FOR i% = 0 TO maxscan%
      dir% = scan%(i%)

      SELECT CASE dir%
        CASE 0: tx% = x%: ty% = y% - 1 ' Up
        CASE 1: tx% = x%: ty% = y% + 1 ' Down
        CASE 2: tx% = x% - 1: ty% = y% ' Left
        CASE 3: tx% = x% + 1: ty% = y% ' Right
      END SELECT

      IF tx% >= x1% AND tx% <= x2% THEN
        IF ty% >= y1% AND ty% <= y2% THEN
          IF maze%(tx%, ty%) = 0 THEN EXIT FOR
        END IF
      END IF
    NEXT i%
  LOOP WHILE i% <= maxscan%

  ' Clear active path from visualization
  FOR y% = y1% TO y2%
    FOR x% = x1% TO x2%
      maze%(x%, y%) = maze%(x%, y%) AND NOT ActivePath%
    NEXT x%
  NEXT y%

  DrawMaze maze%()

  ' Find first connectible cell
  FOR ty% = y1% TO y2%
    FOR tx% = x1% TO x2%
      IF (maze%(tx%, ty%) AND Connected%) = 0 THEN
        IF ty% < y2% THEN
          IF maze%(tx%, ty% + 1) AND Connected% THEN x% = tx%: y% = ty% + 1: dir% = 0: GOTO GotOne
        END IF

        IF tx% < x2% THEN
          IF maze%(tx% + 1, ty%) AND Connected% THEN x% = tx% + 1: y% = ty%: dir% = 2: GOTO GotOne
        END IF

        ' We have to check backwards too, to handle the
        ' rightmost column and bottom row.

        IF tx% > x1% THEN
          IF maze%(tx% - 1, ty%) AND Connected% THEN x% = tx% - 1: y% = ty%: dir% = 3: GOTO GotOne
        END IF

        IF ty% > y1% THEN
          IF maze%(tx%, ty% - 1) AND Connected% THEN x% = tx%: y% = ty% - 1: dir% = 1: GOTO GotOne
        END IF
      END IF
    NEXT tx%
  NEXT ty%

GotOne:

  ' If we fell out of the scan loop, we didn't find any candidate.
  ' Every cell is connected.
LOOP WHILE ty% <= y2%
END SUB

SUB GenerateMazeHuntAndKillImage (maze%(), image%())
IF (straightlinebias% <> 0) AND (axisbias% <> 0) THEN ERROR 5
IF (straightlinebias% < 0) OR (axisbias% < 0) OR (axisbias% > 2) THEN ERROR 5

x1% = LBOUND(maze%, 1)
y1% = LBOUND(maze%, 2)

x2% = UBOUND(maze%, 1)
y2% = UBOUND(maze%, 2)

' Reset maze
FOR y% = y1% TO y2%
  FOR x% = x1% TO x2%
    maze%(x%, y%) = 0
  NEXT x%
NEXT y%

' First, connect most image region boundary cells
FOR y% = y1% TO y2%
  FOR x% = x1% TO x2%
    IF image%(x%, y%) <> 0 AND (maze%(x%, y%) AND Connected%) = 0 THEN
      IF RND < .95 THEN
        IF x% > x1% THEN
          IF image%(x%, y%) = image%(x% - 1, y%) THEN
            maze%(x%, y%) = Connected%
            maze%(x% - 1, y%) = maze%(x% - 1, y%) OR (Connected% OR OpenRight%)
          END IF
        END IF

        IF y% > y1% THEN
          IF image%(x%, y%) = image%(x%, y% - 1) THEN
            maze%(x%, y%) = maze%(x%, y%) OR (Connected% OR OpenDown%)
            maze%(x%, y% - 1) = maze%(x%, y% - 1) OR (Connected% OR OpenDown%)
          END IF
        END IF

        IF x% < x2% THEN
          IF image%(x%, y%) = image%(x% + 1, y%) THEN
            maze%(x%, y%) = Connected% OR OpenRight%
            maze%(x% + 1, y%) = Connected%
          END IF
        END IF

        IF y% < y2% THEN
          IF image%(x%, y%) = image%(x%, y% + 1) THEN
            maze%(x%, y%) = maze%(x%, y%) OR (Connected% OR OpenDown%)
            maze%(x%, y% + 1) = Connected%
          END IF
        END IF
      END IF
    END IF
  NEXT x%
NEXT y%

CONST BoundaryWeight% = 20

' Allow each direction to prioritized from up to BoundaryWeight%.
' The seed cell will typically have four neighbours, so we need
' BoundaryWeight% * 4 slots, even though for the rest of the maze
' there will be at most 3 options.
DIM scan%(0 TO BoundaryWeight% * 4 - 1)

' Kick things off at a random cell
DO
  tx% = INT(RND * (x2% - x1% + 1)) + x1%
  ty% = INT(RND * (y2% - y1% + 1)) + y1%
LOOP WHILE maze%(tx%, ty%) AND Connected%

x% = tx%
y% = ty%

dir% = 0

axis% = axisbias% AND 2

DO
  ' Connect tx%, ty% to x%, y%, then set x%, y% to tx%, ty%
  ' and find a new tx%, ty%, until we hit a dead end.
  DO
    maze%(tx%, ty%) = maze%(tx%, ty%) OR (Connected% OR ActivePath%)

    ' Seed cell won't match any of these. That's okay. :-)
    IF tx% > x% THEN
      maze%(x%, y%) = maze%(x%, y%) OR OpenRight%
    ELSEIF ty% > y% THEN
      maze%(x%, y%) = maze%(x%, y%) OR OpenDown%
    ELSEIF tx% < x% THEN
      maze%(tx%, ty%) = maze%(tx%, ty%) OR OpenRight%
    ELSEIF ty% < y% THEN
      maze%(tx%, ty%) = maze%(tx%, ty%) OR OpenDown%
    END IF

    DrawMaze maze%()

    ' Check for adjacent cells in a random order
    px% = tx% - 1: nx% = tx% + 1
    py% = ty% - 1: ny% = ty% + 1

    d0weight% = 0
    d1weight% = 0
    d2weight% = 0
    d3weight% = 0

    IF (tx% <> x% OR py% <> y%) AND (py% >= y1%) THEN
      IF (image%(tx%, ty%) <> 0) AND (image%(tx%, ty%) <> image%(tx%, py%)) THEN d0weight% = BoundaryWeight% ELSE d0weight% = 1
    END IF
    IF (tx% <> x% OR py% <> y%) AND (ny% <= y2%) THEN
      IF (image%(tx%, ty%) <> 0) AND (image%(tx%, ty%) <> image%(tx%, ny%)) THEN d1weight% = BoundaryWeight% ELSE d1weight% = 1
    END IF
    IF (px% <> x% OR ty% <> y%) AND (px% >= x1%) THEN
      IF (image%(tx%, ty%) <> 0) AND (image%(tx%, ty%) <> image%(px%, ty%)) THEN d2weight% = BoundaryWeight% ELSE d2weight% = 1
    END IF
    IF (nx% <> x% OR ty% <> y%) AND (nx% <= x2%) THEN
      IF (image%(tx%, ty%) <> 0) AND (image%(tx%, ty%) <> image%(nx%, ty%)) THEN d3weight% = BoundaryWeight% ELSE d3weight% = 1
    END IF

    maxscan% = -1

    FOR i% = 1 TO d0weight%
      maxscan% = maxscan% + 1
      scan%(maxscan%) = 0
    NEXT i%
    FOR i% = 1 TO d1weight%
      maxscan% = maxscan% + 1
      scan%(maxscan%) = 1
    NEXT i%
    FOR i% = 1 TO d2weight%
      maxscan% = maxscan% + 1
      scan%(maxscan%) = 2
    NEXT i%
    FOR i% = 1 TO d3weight%
      maxscan% = maxscan% + 1
      scan%(maxscan%) = 3
    NEXT i%

    FOR i% = maxscan% TO 1 STEP -1
      SWAP scan%(i%), scan%(INT(RND * (i% + 1)))
    NEXT i%

    x% = tx%
    y% = ty%

    FOR i% = 0 TO maxscan%
      dir% = scan%(i%)

      SELECT CASE dir%
        CASE 0: tx% = x%: ty% = y% - 1 ' Up
        CASE 1: tx% = x%: ty% = y% + 1 ' Down
        CASE 2: tx% = x% - 1: ty% = y% ' Left
        CASE 3: tx% = x% + 1: ty% = y% ' Right
      END SELECT

      IF tx% >= x1% AND tx% <= x2% THEN
        IF ty% >= y1% AND ty% <= y2% THEN
          IF maze%(tx%, ty%) = 0 THEN EXIT FOR
        END IF
      END IF
    NEXT i%
  LOOP WHILE i% <= maxscan%

  ' Clear active path from visualization
  FOR y% = y1% TO y2%
    FOR x% = x1% TO x2%
      maze%(x%, y%) = maze%(x%, y%) AND NOT ActivePath%
    NEXT x%
  NEXT y%

  DrawMaze maze%()

  ' Find first connectible cell
  FOR ty% = y1% TO y2%
    FOR tx% = x1% TO x2%
      IF (maze%(tx%, ty%) AND Connected%) = 0 THEN
        IF ty% < y2% THEN
          IF maze%(tx%, ty% + 1) AND Connected% THEN x% = tx%: y% = ty% + 1: dir% = 0: GOTO GotOne
        END IF

        IF tx% < x2% THEN
          IF maze%(tx% + 1, ty%) AND Connected% THEN x% = tx% + 1: y% = ty%: dir% = 2: GOTO GotOne
        END IF

        ' We have to check backwards too, to handle the
        ' rightmost column and bottom row.

        IF tx% > x1% THEN
          IF maze%(tx% - 1, ty%) AND Connected% THEN x% = tx% - 1: y% = ty%: dir% = 3: GOTO GotOne
        END IF

        IF ty% > y1% THEN
          IF maze%(tx%, ty% - 1) AND Connected% THEN x% = tx%: y% = ty% - 1: dir% = 1: GOTO GotOne
        END IF
      END IF
    NEXT tx%
  NEXT ty%

GotOne:

  ' If we fell out of the scan loop, we didn't find any candidate.
  ' Every cell is connected.
LOOP WHILE ty% <= y2%
END SUB

SUB LoadImage (filename$, image%())
x1% = LBOUND(image%, 1)
y1% = LBOUND(image%, 2)

x2% = UBOUND(image%, 1)
y2% = UBOUND(image%, 2)

OPEN filename$ FOR BINARY AS #1

pel$ = " "

FOR y% = y1% TO y2%
  FOR x% = x1% TO x2%
    GET #1, , pel$
    image%(x%, y%) = ASC(pel$)
  NEXT x%
NEXT y%

CLOSE #1

' Clear pixels that aren't at a region boundary.
DIM boundary%(x1% TO x2%, y1% TO y2%)

FOR y% = y1% TO y2%
  FOR x% = x1% TO x2%
    boundary% = 0

    IF y% > y1% THEN
      IF image%(x%, y%) <> image%(x%, y% - 1) THEN boundary% = -1
    END IF
    IF y% < y2% THEN
      IF image%(x%, y%) <> image%(x%, y% + 1) THEN boundary% = -1
    END IF
    IF x% > x1% THEN
      IF image%(x%, y%) <> image%(x% - 1, y%) THEN boundary% = -1
    END IF
    IF x% < x2% THEN
      IF image%(x%, y%) <> image%(x% + 1, y%) THEN boundary% = -1
    END IF

    boundary%(x%, y%) = boundary%
  NEXT x%
NEXT y%

FOR y% = y1% TO y2%
  FOR x% = x1% TO x2%
    IF NOT boundary%(x%, y%) THEN image%(x%, y%) = 0
  NEXT x%
NEXT y%

' Make orthogonal connections with single pixels where possible
DIM d%(0 TO 3)

FOR y% = y1% TO y2%
  FOR x% = x1% TO x2%
    boundary%(x%, y%) = 0

    IF image%(x%, y%) = 0 THEN
      numd% = 0

      IF y% > y1% THEN
        IF image%(x%, y% - 1) THEN d%(numd%) = image%(x%, y% - 1): numd% = numd% + 1
      END IF
      IF y% < y2% THEN
        IF image%(x%, y% + 1) THEN d%(numd%) = image%(x%, y% + 1): numd% = numd% + 1
      END IF
      IF x% > x1% THEN
        IF image%(x% - 1, y%) THEN d%(numd%) = image%(x% - 1, y%): numd% = numd% + 1
      END IF
      IF x% < x2% THEN
        IF image%(x% + 1, y%) THEN d%(numd%) = image%(x% + 1, y%): numd% = numd% + 1
      END IF

      IF numd% > 1 THEN
        havedifferent% = 0

        FOR i% = 0 TO numd% - 2
          FOR j% = i% + 1 TO numd% - 1
            IF d%(i%) <> d%(j%) THEN havedifferent% = -1
          NEXT j%
        NEXT i%

        IF havedifferent% = 0 THEN
          boundary%(x%, y%) = d%(0)
        END IF
      END IF
    END IF
  NEXT x%
NEXT y%

FOR y% = y1% TO y2%
  FOR x% = x1% TO x2%
    IF boundary%(x%, y%) THEN image%(x%, y%) = boundary%(x%, y%)
  NEXT x%
NEXT y%
END SUB

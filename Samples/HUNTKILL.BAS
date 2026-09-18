DECLARE SUB DrawMaze (maze%())
DECLARE SUB GenerateMazeHuntAndKill (maze%(), straightlinebias%)

CONST MazeWidth% = 35
CONST MazeHeight% = 35

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

SCREEN 12

GenerateMazeHuntAndKill maze%(), 0 ' no bias to straight lines

DrawMaze maze%()

WHILE INKEY$ = "": WEND

GenerateMazeHuntAndKill maze%(), 5 ' heavy bias to straight lines

DrawMaze maze%()

WHILE INKEY$ = "": WEND

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

SUB GenerateMazeHuntAndKill (maze%(), straightlinebias%)
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

DIM scan%(0 TO 3 + straightlinebias%)

' Kick things off at a random cell
tx% = INT(RND * (x2% - x1% + 1)) + x1%
ty% = INT(RND * (y2% - y1% + 1)) + y1%

x% = tx%
y% = ty%

dir% = 0

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
    FOR i% = 0 TO 3
      scan%(i%) = i%
    NEXT i%

    ' Do not go backward.
    scan%(dir% XOR 1) = dir%

    IF dir% = -1 THEN
      maxscan% = 3
    ELSE
      ' Prioritize going straight
      maxscan% = UBOUND(scan%)

      FOR i% = 4 TO maxscan%
        scan%(i%) = dir%
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

' Stress test: paint with lots of obstacles

CONST NumObstacles% = 1000
CONST ObstacleColour% = 8
CONST PaintColour% = 1
CONST TemporaryBlockColour% = 4

DIM ox%(1 TO NumObstacles%), oy%(1 TO NumObstacles%)

SCREEN 13

' Block pixel for paint start
PSET (160, 100), TemporaryBlockColour%

' Generate random points. Never put two points adjacent or overlapping.
FOR i% = 1 TO NumObstacles%
  DO
    px% = INT(RND * 320)
    py% = INT(RND * 200)

    good% = -1

    FOR dx% = -1 - (px% = 0) TO 1 + (px% = 199)
      FOR dy% = -1 - (py% = 0) TO 1 + (py% = 199)
        IF POINT(px% + dx%, py% + dy%) THEN good% = 0: EXIT FOR
      NEXT dy%
      IF NOT good% THEN EXIT FOR
    NEXT dx%
  LOOP UNTIL good%

  PSET (px%, py%), ObstacleColour%

  ox%(i%) = px%
  oy%(i%) = py%
NEXT i%

' Clear paint start point and paint
PSET (160, 100), 0
PAINT (160, 100), PaintColour%, ObstacleColour%

OPEN "TESTOUT$" FOR OUTPUT AS #1

' Make sure none of the obstacles were overdrawn
FOR i% = 1 TO NumObstacles%
  IF POINT(ox%(i%), oy%(i%)) <> ObstacleColour% THEN
    PRINT #1, "FAIL"
    EXIT FOR
  END IF

  ' Merge obstacle into paint to simplify paint test
  PSET (ox%(i%), oy%(i%)), PaintColour%
NEXT i%

' Make sure paint got everywhere
FOR y% = 0 TO 199
  FOR x% = 0 TO 319
    IF POINT(x%, y%) <> PaintColour% THEN
      PRINT #1, "FAIL"
      EXIT FOR
    END IF
  NEXT x%
NEXT y%

PRINT #1, "PASS" ' if no FAILs

CLOSE #1


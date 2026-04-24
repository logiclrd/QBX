TYPE PointType
  X AS INTEGER
  Y AS INTEGER
END TYPE

SCREEN 12

CONST Colour% = 5
CONST MinSpeed% = 2
CONST MaxSpeed% = 5
CONST NumPoints% = 4
CONST HistoryLength% = 40
CONST FramesPerDelay% = 2

CONST SpeedRange% = MaxSpeed% - MinSpeed% + 1

DIM Points(0 TO NumPoints% - 1, HistoryLength%) AS PointType
DIM Direction(0 TO NumPoints% - 1) AS PointType

FOR i% = 0 TO NumPoints% - 1
  Points(i%).X = RND * 640
  Points(i%).Y = RND * 480
  Direction(i%).X = INT(RND * SpeedRange% + MinSpeed%) * (INT(RND * 2) * 2 - 1)
  Direction(i%).Y = INT(RND * SpeedRange% + MinSpeed%) * (INT(RND * 2) * 2 - 1)
NEXT i%

trailindex% = 0

DIM p1 AS PointType
DIM p2 AS PointType

framecounter% = 0

DO
  nexttrailindex% = (trailindex% + 1) MOD HistoryLength%

  FOR i% = 0 TO NumPoints% - 1
    j% = (i% + 1) MOD NumPoints%

    p1 = Points(i%, nexttrailindex%)
    p2 = Points(j%, nexttrailindex%)

    LINE (p1.X, p1.Y)-(p2.X, p2.Y), 0
  NEXT i%

  FOR i% = 0 TO NumPoints% - 1
    p1 = Points(i%, trailindex%)
    p2 = Direction(i%)

    p1.X = p1.X + p2.X
    p1.Y = p1.Y + p2.Y

    IF p1.X < 0 OR p1.X > 639 THEN
      p1.X = p1.X - p2.X

      Direction(i%).X = INT(RND * SpeedRange% + MinSpeed%) * -SGN(Direction(i%).X)

      p1.X = p1.X + Direction(i%).X
    END IF

    IF p1.Y < 0 OR p1.Y > 479 THEN
      p1.Y = p1.Y - p2.Y

      Direction(i%).Y = INT(RND * SpeedRange% + MinSpeed%) * -SGN(Direction(i%).Y)

      p1.Y = p1.Y + Direction(i%).Y
    END IF

    Points(i%, nexttrailindex%) = p1
  NEXT i%

  FOR k% = 0 TO HistoryLength%
    FOR i% = 0 TO NumPoints% - 1
      j% = (i% + 1) MOD NumPoints%

      p1 = Points(i%, k%)
      p2 = Points(j%, k%)

      LINE (p1.X, p1.Y)-(p2.X, p2.Y), Colour%
    NEXT i%
  NEXT k%

  trailindex% = nexttrailindex%

  framecounter% = framecounter% + 1

  IF framecounter% = FramesPerDelay% THEN
    framecounter% = 0
    st! = TIMER: WHILE TIMER = st!: WEND
  END IF
LOOP WHILE INKEY$ = ""

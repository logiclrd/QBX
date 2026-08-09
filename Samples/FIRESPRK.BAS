TYPE sparktype
  x AS DOUBLE
  y AS DOUBLE
  dx AS DOUBLE
  dy AS DOUBLE
  c AS INTEGER
END TYPE
DIM NewBuffer(0 TO 99, 0 TO 99)
DIM OldBuffer(0 TO 99, 0 TO 99)
DIM spark(0 TO 99) AS sparktype

'Initializing:
SCREEN 13
FOR x = 0 TO 255
  OUT &H3C8, x
  SELECT CASE x
    CASE 0 TO 63:
      OUT &H3C9, x
      OUT &H3C9, 0
      OUT &H3C9, 0
    CASE 64 TO 191:
      OUT &H3C9, 63
      OUT &H3C9, (x - 64) \ 2
      OUT &H3C9, 0
    CASE 192 TO 255
      OUT &H3C9, 63
      OUT &H3C9, 63
      OUT &H3C9, x - 192
  END SELECT
NEXT x

DO WHILE INKEY$ = ""
  'Setting Base
  'FOR x = 1 TO 98
  '  NewBuffer(x, 99) = 255 * INT(RND * 2)
  'NEXT x
  FOR x = 0 TO 43
    NewBuffer(x, 99) = 0
  NEXT x
  FOR x = 56 TO 99
    NewBuffer(x, 99) = 0
  NEXT x
  FOR x = 0 TO 99
    spark(x).x = spark(x).x + spark(x).dx
    spark(x).y = spark(x).y + spark(x).dy
    spark(x).dy = spark(x).dy + .1
    sx = INT(spark(x).x)
    sy = INT(spark(x).y)
    spark(x).c = spark(x).c - 1
    IF (sy > 99) OR (spark(x).c <= 0) THEN
      spark(x).x = RND * 10 + 45
      spark(x).y = 99
      spark(x).dx = RND * 2 - 1
      spark(x).dy = -RND - RND - 1
      spark(x).c = 255
      sx = INT(spark(x).x)
      sy = INT(spark(x).y)
    END IF
    subx# = spark(x).x - sx
    suby# = spark(x).y - sy
    contribution = spark(x).c
    contribUpperLeft = contribution * (1 - subx#) * (1 - suby#)
    contribUpperRight = contribution * subx# * (1 - suby#)
    contribLowerLeft = contribution * (1 - subx#) * suby#
    contribLowerRight = contribution * subx# * suby#
    IF (sx >= 0) AND (sx <= 99) THEN
      IF (sy >= 0) AND (sy <= 99) THEN
        NewBuffer(sx, sy) = NewBuffer(sx, sy) + contribUpperLeft
      END IF
      IF (sy >= -1) AND (sy <= 98) THEN
        NewBuffer(sx, sy + 1) = NewBuffer(sx, sy + 1) + contribLowerLeft
      END IF
    END IF
    IF (sx >= -1) AND (sx <= 98) THEN
      IF (sy >= 0) AND (sy <= 99) THEN
        NewBuffer(sx + 1, sy) = NewBuffer(sx + 1, sy) + contribUpperLeft
      END IF
      IF (sy >= -1) AND (sy <= 98) THEN
        NewBuffer(sx + 1, sy + 1) = NewBuffer(sx + 1, sy + 1) + contribLowerLeft
      END IF
    END IF
  NEXT x

  'Printing Buffer
  FOR x = 1 TO 98
    FOR y = 0 TO 99
      c = NewBuffer(x, y) \ 5
      IF c <= 255 THEN
        PSET (x, y), c
      ELSE
        PSET (x, y), 255
      END IF
      IF y THEN OldBuffer(x, y - 1) = NewBuffer(x, y)
    NEXT y
  NEXT x

  FOR y = 98 TO 1 STEP -1
    FOR x = 1 TO 98
      NewBuffer(x, y) = (OldBuffer(x, y) * 10 + OldBuffer(x - 1, y) + OldBuffer(x + 1, y) + OldBuffer(x, y + 1) * 2) \ 16
    NEXT x
  NEXT y
LOOP


'Simple 2.5d engine that raycasts only to a textured floor.
'Zack F. -- v2001.08.25
'I get ~13fps at default res (150x240) on my p3 900.
'Can this be sped up within the limits of QB?????

SCREEN 13
CONST pi = 3.14159, blur% = 3 'for creating tile texture
DIM c2%(-1 TO 64, -1 TO 64), c%(0 TO 63, 0 TO 63)
vish% = 150: visw% = 240: h% = 25 'vis = rendering, h% = height from floor
px = 0: py = 0: z = 0: peh% = 90 'peh% = fov in deg, fov = fov in rad. px,py,z = player coord & angle

FOR x% = 0 TO 63: FOR y% = 0 TO 63
    c2%(x%, y%) = INT(RND * 16) * 4   'random floor texture
  NEXT y%                             '
                                      '
  c2%(x%, -1) = c2%(x%, 63)           '
  c2%(x%, 64) = c2%(x%, 0)            '
NEXT x%                               '
                                      '
FOR y% = -1 TO 64                     '
  c2%(-1, y%) = c2%(63, y%)           '
  c2%(64, y%) = c2%(0, y%)            '
NEXT y%                               '

OUT 968, 1                            'set pal: 1-63 grn-white, 64-127 blu-white
FOR x% = 0 TO 62: OUT 969, x%: OUT 969, 32 + (x% \ 2): OUT 969, x%: NEXT x%
FOR x% = 0 TO 63: OUT 969, x%: OUT 969, x%: OUT 969, 32 + (x% \ 2): NEXT x%

FOR hehe% = 1 TO blur%                           'blurring texture
  FOR x% = 0 TO 63: FOR y% = 0 TO 63             '
      a% = 0                                     'two arrays so there is no
      FOR xx% = -1 TO 1: FOR yy% = -1 TO 1       '"wind" effect
          a% = a% + c2%(x% + xx%, y% + yy%)      '
      NEXT yy%, xx%                              '
      c%(x%, y%) = a% \ 9
  NEXT y%, x%
  FOR x% = 0 TO 63: FOR y% = 0 TO 63
      c2%(x%, y%) = c%(x%, y%)
    NEXT y%

    c2%(x%, -1) = c%(x%, 63)
    c2%(x%, 64) = c%(x%, 0)
  NEXT x%

  FOR y% = 0 TO 63
    c2%(-1, y%) = c%(63, y%)
    c2%(64, y%) = c%(0, y%)
  NEXT y%
NEXT hehe%


CLS
COLOR 63
PRINT "Use: A/Z  1/2  +/-  /  "; CHR$(27); "/"
PRINT "     hght fov  size move turn"

DIM p%(32002)
p%(0) = visw% * 8: p%(1) = vish%  'initialize GET array for screen

e = TIMER: fov = pi * peh% / 360  'set fov in rad
DO
  q% = 1: fps% = fps% + 1
  IF TIMER >= e + 1 THEN e = TIMER: curfps% = fps%: fps% = 0

  FOR y% = 100 - vish% / 2 TO 99 + vish% / 2
    heh = (y% - 99) * fov / 100
    IF y% = 99 THEN d = 32767 ELSE d = h% / SIN(heh)
   
    FOR r% = q% + 1 TO q% + visw% / 2       'sky incorporated to array
    p%(r%) = 25700                          'solid color to maintain speed
    NEXT r%                                 '25700 = 100 for two pixels in p%(0)
   
    IF d < 0 THEN q% = q% + visw% / 2: GOTO nexty 'if cannot cast toward floor, skip the math

    dx1 = COS(z - fov) * d + px: dy1 = SIN(z - fov) * d + py 'on screen, left to right
    dx2 = COS(z + fov) * d + px: dy2 = SIN(z + fov) * d + py

    IF ABS(dx1) > 32767 OR ABS(dx2) > 32767 OR ABS(dy1) > 32767 OR ABS(dy2) > 32767 THEN q% = q% + visw% / 2: GOTO nexty
    sx = (dx2 - dx1) / 320: sy = (dy2 - dy1) / 320    '^ if too far away, fuck it and skip the math to avoid overflow (use sky col) ^

    FOR x% = 160 - visw% / 2 TO 159 + visw% / 2 STEP 2
                              'core: rendels two pixels at a time for each int
      FOR z% = 0 TO 1         'sprites, linedefs, lighting would all be placed within the walls of FOR z% ...NEXT
        l% = x% + z%          'at a very high price in QB =)
        lox% = (dx1 + l% * sx) MOD 64: IF lox% < 0 THEN lox% = lox% + 64
        loy% = (dy1 + l% * sy) MOD 64: IF loy% < 0 THEN loy% = loy% + 64
        c1%(z%) = c%(lox%, loy%)
      NEXT z%

      q% = q% + 1
      IF c1%(1) >= 128 THEN c1%(1) = c1%(1) - 256
      p%(q%) = c1%(1) * 256 + c1%(0)  'write 2 pixels to PUT array

    NEXT x%

nexty:
  NEXT y%  'end of renderer
                                    'since not in realtime, movement speed is adjusted to comply with fps and the user is happy
  IF curfps% > 0 THEN spd% = 35 / curfps% ELSE spd% = 1
  spd% = spd% + 1 - SGN(spd%)       'formula to make spd% 1 = if spd% = 0 (no movement)

  hit% = INP(96): SELECT CASE hit%  '"compressed" key detection--notice the pairs of cases
    CASE 1: END
    CASE 2, 3: IF ABS(peh% - 91) < 89 THEN peh% = peh% + hit% * 2 - 5: fov = pi * peh% / 360 'inc/decrease FOV
    CASE 12: IF sch% = 0 AND visw% > 16 THEN sch% = 1: visw% = visw% - 16: vish% = vish% - 10: p%(0) = visw% * 8: p%(1) = vish%: CLS 'shrink image size for speed, and clear screen of debris
    CASE 13: IF sch% = 0 AND visw% < 305 THEN sch% = 1: visw% = visw% + 16: vish% = vish% + 10: p%(0) = visw% * 8: p%(1) = vish% 'enlarge image for quality
    CASE 30, 44: h% = INT(h% + spd% * (39 - hit%) / 7)  'fly up/down
    CASE 72, 80: py = py - SIN(z) * spd% * ((hit% - 76) / 2): px = px - COS(z) * spd% * ((hit% - 76) / 2)'move forward/back
    CASE 75, 77: z = z + (pi / 90) * spd% * (hit% - 76): IF ABS(z) > pi THEN z = z - SGN(z) * 2 * pi 'turn left/right
    CASE 140, 141: sch% = 0
  END SELECT
  IF ABS(px) > 16384 THEN px = px - 32768 * SGN(px)
  IF ABS(py) > 16384 THEN py = py - 32768 * SGN(py)
  DO: LOOP WHILE INKEY$ <> ""

  PUT (160 - (visw% / 2), 100 - (vish% / 2)), p%(0), PSET 'finally, get that image up
  LOCATE , 1: PRINT LTRIM$(STR$(curfps%)); " FPS";
LOOP

END


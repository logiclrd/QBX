CONST EGA% = 0 ' if not EGA then VGA

TYPE LatticePoint
  Colour AS INTEGER
  HasCentrePoint AS INTEGER
  OrbitPointCount AS INTEGER
  OrbitRadius AS DOUBLE
  OrbitSpeed AS DOUBLE
  OrbitPhase AS DOUBLE
  Filled AS INTEGER
END TYPE

TYPE HSV
  Hue AS DOUBLE
  Value AS DOUBLE
  Saturation AS DOUBLE
END TYPE

DECLARE SUB UpdatePalette (colours() AS HSV, hueoffset#)
DECLARE SUB HSV2RGB (h#, s#, v#, r#, g#, b#)

DECLARE SUB GenerateLatticePoint (pt AS LatticePoint)

SCREEN 7, , 0, 1

DIM colours(7) AS HSV

IF EGA% THEN
  PALETTE 1, 1
  PALETTE 2, 3
  PALETTE 3, 5
  PALETTE 4, 57
  PALETTE 5, 59
  PALETTE 6, 61
  PALETTE 7, 63
ELSE
  colours(1).Hue = 240: colours(1).Saturation = 1: colours(1).Value = .67
  colours(2).Hue = 180: colours(2).Saturation = 1: colours(2).Value = .67
  colours(3).Hue = 300: colours(3).Saturation = 1: colours(3).Value = .67
  colours(4).Hue = 240: colours(4).Saturation = .67: colours(4).Value = 1
  colours(5).Hue = 180: colours(5).Saturation = .67: colours(5).Value = 1
  colours(6).Hue = 300: colours(6).Saturation = .67: colours(6).Value = 1
  colours(7).Hue = 0: colours(7).Saturation = 0: colours(7).Value = 1

  FOR i% = 1 TO 7
    PALETTE i%, i%
  NEXT i%
END IF

CONST PI# = 3.14159265358979#
CONST PHI# = 1.61803398874989#

CONST LatticeScale# = 7

CycleLength# = LOG(1 + PHI#)

lata# = PHI#
latb# = 1
latc# = 1
latd# = -PHI#

DIM lattice(-20 TO 20, -30 TO 30) AS LatticePoint
DIM oldlattice(-20 TO 20, -30 TO 30) AS LatticePoint

FOR i% = -20 TO 20
  FOR j% = -30 TO 30
    GenerateLatticePoint lattice(i%, j%)
  NEXT j%
NEXT i%

palettehueoffset# = 0

DO
  ex# = EXP(t#)
  ey# = EXP(-t#)

  iterlata# = lata# * ex#
  iterlatb# = latc# * ey#
  iterlatc# = latb# * ex#
  iterlatd# = latd# * ey#

  CLS

  IF NOT EGA% THEN
    palettehueoffset# = palettehueoffset# + COS(u#) ^ 2 - .3

    IF palettehueoffset# >= 360 THEN palettehueoffset# = palettehueoffset# - 360

    UpdatePalette colours(), palettehueoffset#
  END IF

  FOR i% = -20 TO 20
    FOR j% = -30 TO 30
      x# = LatticeScale# * (lata# * i% + latb# * j%)
      y# = LatticeScale# * (latc# * i% + latd# * j%)

      x# = x# * ex#
      y# = y# * ey#

      IF lattice(i%, j%).HasCentrePoint THEN
        PSET (160 + x#, 100 - y#), lattice(i%, j%).Colour
      END IF

      IF lattice(i%, j%).OrbitPointCount THEN
        lattice(i%, j%).OrbitPhase = lattice(i%, j%).OrbitPhase + (COS(u# * lattice(i%, j%).OrbitSpeed) - .2) * .2

        da# = 2 * PI# / lattice(i%, j%).OrbitPointCount

        FOR k% = 1 TO lattice(i%, j%).OrbitPointCount
          a# = k% * da# + lattice(i%, j%).OrbitPhase

          xx# = lattice(i%, j%).OrbitRadius * COS(a#)
          yy# = lattice(i%, j%).OrbitRadius * SIN(a#)

          PSET (160 + x# + xx#, 100 - y# - yy#), lattice(i%, j%).Colour
        NEXT k%
      END IF
    NEXT j%
  NEXT i%

  PCOPY 0, 1

  u# = u# + .01

  t# = t# + .02

  IF t# > CycleLength# THEN
    t# = t# - CycleLength#

    FOR i% = -20 TO 20
      FOR j% = -30 TO 30
        oldlattice(i%, j%) = lattice(i%, j%)
        lattice(i%, j%).Filled = 0
      NEXT j%
    NEXT i%

    ex# = EXP(CycleLength#)
    ey# = EXP(-CycleLength#)

    FOR i% = -20 TO 20
      FOR j% = -30 TO 30
        ii% = 2 * i% + j%
        jj% = i% + j%

        IF ii% >= -20 AND ii% <= 20 THEN
          IF jj% >= -30 AND jj% <= 30 THEN
            lattice(ii%, jj%) = oldlattice(i%, j%)
          END IF
        END IF
      NEXT j%
    NEXT i%

    FOR i% = -20 TO 20
      FOR j% = -30 TO 30
        IF NOT lattice(i%, j%).Filled THEN
          GenerateLatticePoint lattice(i%, j%)
        END IF
      NEXT j%
    NEXT i%
  END IF

  st# = TIMER: WHILE st# = TIMER: WEND
LOOP WHILE INKEY$ = ""

SUB GenerateLatticePoint (pt AS LatticePoint)
pt.Colour = INT(RND * 7) + 1
pt.OrbitPointCount = INT(RND * 6)

IF pt.OrbitPointCount < 2 THEN
  pt.OrbitPointCount = 0
  pt.HasCentrePoint = -1
ELSE
  pt.HasCentrePoint = RND < .5
  pt.OrbitRadius = RND * 3 + 1
  pt.OrbitSpeed = RND * 5 + 3
END IF

pt.Filled = -1
END SUB

SUB HSV2RGB (h#, s#, v#, r#, g#, b#)
' Input: h# in 0..360, s# and v# in 0..1
' Output: r3, g#, b# in 0..1

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

r# = r# + match#
g# = g# + match#
b# = b# + match#
END SUB

SUB UpdatePalette (colours() AS HSV, hueoffset#)
OUT &H3C8, 1

FOR i% = 1 TO UBOUND(colours)
  HSV2RGB colours(i%).Hue + hueoffset#, colours(i%).Saturation, colours(i%).Value, r#, g#, b#

  r% = r# * 64
  g% = g# * 64
  b% = b# * 64

  IF r% > 63 THEN r% = 63
  IF g% > 63 THEN g% = 63
  IF b% > 63 THEN b% = 63

  OUT &H3C9, r%
  OUT &H3C9, g%
  OUT &H3C9, b%
NEXT i%
END SUB

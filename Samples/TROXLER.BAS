CONST GridSpacing% = 14
CONST GridPointExtent% = 6
CONST CornerRadius! = 1.41428 * GridPointExtent% * GridSpacing%
CONST RadiusFadeWidth% = 2 * GridSpacing%
CONST DotSize! = 2
CONST FadeDotSize! = 1
CONST VanishingDotRadius! = 60
CONST RotationPerFrame! = .05

CONST PI# = 3.14159265358979

SCREEN 7, , 1, 0

DO
  framestarttime! = TIMER

  a# = a# + RotationPerFrame!

  IF a# > 2 * PI# THEN a# = a# - 2 * PI#

  ca# = COS(a#)
  sa# = SIN(a#)

  CLS

  FOR x% = -GridPointExtent% TO GridPointExtent%
    FOR y% = -GridPointExtent% TO GridPointExtent%
      xx% = x% * GridSpacing%
      yy% = y% * GridSpacing%

      rx! = xx% * ca# - yy% * sa#
      ry! = xx% * sa# + yy% * ca#

      sx! = 160 + rx!
      sy! = 100 - ry!

      radius! = CornerRadius! - SQR(rx! * rx! + ry! * ry!)

      IF radius! > RadiusFadeWidth! THEN radius! = RadiusFadeWidth!

      radius! = radius! * (DotSize! - FadeDotSize!) / RadiusFadeWidth! + FadeDotSize!

      CIRCLE (sx!, sy!), radius!, 4
      PAINT (sx!, sy!), 4, 4
    NEXT y%
  NEXT x%

  CIRCLE (160, 100), DotSize!, 10
  PAINT (160, 100), 10, 10

  FOR i% = 90 TO 360 STEP 120
    angle# = i% * (PI# / 180#)

    ca# = COS(angle#)
    sa# = SIN(angle#)

    sx! = 160 + VanishingDotRadius! * ca#
    sy! = 100 - VanishingDotRadius! * sa#

    CIRCLE (sx!, sy!), DotSize!, 14
    PAINT (sx!, sy!), 14, 14
  NEXT i%

  PCOPY 1, 0

  WHILE TIMER = framestarttime!: WEND
LOOP WHILE INKEY$ = ""


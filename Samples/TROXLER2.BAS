CONST DotSize! = 15
CONST VanishingDotRadius! = 75
CONST DotCount% = 12
CONST CrossHairSize% = 4
CONST FrameDuration! = .2

CONST PI# = 3.14159265358979

SCREEN 13

OUT &H3C8, 0

FOR i% = 40 TO 20 STEP -1
  OUT &H3C9, 40
  OUT &H3C9, i%
  OUT &H3C9, 40
NEXT i%

dotextent% = -FIX(-DotSize!)

FOR x% = -dotextent% TO dotextent%
  FOR y% = -dotextent% TO dotextent%
    r! = SQR(x% * x% + y% * y%)
    IF r! <= DotSize! THEN
      PSET (160 + x%, 100 + y%), (1 - r! / DotSize!) * 20
    END IF
  NEXT y%
NEXT x%

numpixels% = (dotextent% * 2 + 1) ^ 2

numintegers% = (numpixels% + 1) \ 2

DIM dot%(0 TO numintegers% + 1)

GET (160 - dotextent%, 100 - dotextent%)-(160 + dotextent%, 100 + dotextent%), dot%

CLS

FOR i% = 1 TO DotCount%
  angle# = i% * (2# * PI# / DotCount%)

  ca# = COS(angle#)
  sa# = SIN(angle#)

  x% = 160 + ca# * VanishingDotRadius!
  y% = 100 - sa# * VanishingDotRadius!

  PUT (x% - dotextent%, y% - dotextent%), dot%, PSET
NEXT i%

LINE (160 - CrossHairSize%, 100)-(160 + CrossHairSize%, 100), 255
LINE (160, 100 - CrossHairSize%)-(160, 100 + CrossHairSize%), 255

nextframetime! = TIMER

missingdotindex% = 0

DO
  WHILE TIMER < nextframetime!: WEND

  nextframetime! = nextframetime! + FrameDuration!
  IF nextframetime! >= 86400 THEN nextframetime! = nextframetime! - 86400

  PUT (x% - dotextent%, y% - dotextent%), dot%, PSET

  missingdotindex% = (missingdotindex% + 1) MOD DotCount%

  angle# = missingdotindex% * (2# * PI# / DotCount%)

  ca# = COS(angle#)
  sa# = SIN(angle#)

  x% = 160 + ca# * VanishingDotRadius!
  y% = 100 - sa# * VanishingDotRadius!

  LINE (x% - dotextent%, y% - dotextent%)-(x% + dotextent%, y% + dotextent%), 0, BF
LOOP WHILE INKEY$ = ""


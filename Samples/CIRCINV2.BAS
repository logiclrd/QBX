' Recreation of an image by Xah Lee
' http://xahlee.info/SpecialPlaneCurves_dir/InversionGallery_dir/InversionGallery.html
' Expressed in QuickBASIC by Jonathan Gilbert 2026-08-26

TYPE CircleType
  CX AS SINGLE
  CY AS SINGLE
  CR AS SINGLE
  CC AS INTEGER
END TYPE

DECLARE SUB DrawCircles (circles() AS CircleType)
DECLARE SUB DrawFigure
DECLARE SUB InvertCircle (original AS CircleType, across AS CircleType, reflection AS CircleType)
DECLARE SUB InvertCircles (this AS CircleType, circles() AS CircleType, depth%)
DECLARE SUB InvertPoint (xc!, yc!, rc!, xp!, yp!)
DECLARE SUB OutputVGAColourCMYK (c!, m!, y!, k!)
DECLARE SUB SetPalette ()

CONST PI! = 3.141593

CONST MaxDepth% = 7 ' depth of iterations > 2 (0-based)

CONST Delta! = .00001  ' underflow

SCREEN 12

SetPalette

DrawFigure

SLEEP

SUB DrawFigure
scale! = 240 / 2.2

DIM circles(0 TO 6) AS CircleType

'   O
' O   O
'   O
' O   O
'   O

mx! = 320
my! = 240

circles(0).CX = mx!
circles(0).CY = my!
circles(0).CR = scale! ' Central circle
circles(0).CC = 1

FOR i% = 1 TO 6
  a! = PI! / 2 - (i% - 1) * (PI! / 3)

  circles(i%).CX = mx! + 2.02 * scale! * COS(a!)
  circles(i%).CY = my! - 2.02 * scale! * SIN(a!) + d!
  circles(i%).CR = scale!
  circles(i%).CC = i% + 1
NEXT i%

FOR i% = 0 TO 6
  InvertCircles circles(i%), circles(), 2
NEXT i%

WINDOW
END SUB

SUB InvertCircle (across AS CircleType, original AS CircleType, reflection AS CircleType)
' Invert circle original in circle across
' Writes result to reflection

d! = SQR((original.CX - across.CX) ^ 2 + (original.CY - across.CY) ^ 2)

IF d! < Delta! THEN d! = 1

ir! = original.CR / d!

ix! = (original.CX - across.CX) * ir!
iy! = (original.CY - across.CY) * ir!

xa! = original.CX + ix!
xb! = original.CX - ix!

ya! = original.CY + iy!
yb! = original.CY - iy!

InvertPoint across.CX, across.CY, across.CR, xa!, ya!
InvertPoint across.CX, across.CY, across.CR, xb!, yb!

reflection.CX = (xa! + xb!) * .5 ' xinvcircle
reflection.CY = (ya! + yb!) * .5 ' yinvcircle
reflection.CR = SQR((xa! - xb!) ^ 2 + (ya! - yb!) ^ 2) * .5 ' rinvcircle
reflection.CC = original.CC
END SUB

SUB InvertCircles (this AS CircleType, circles() AS CircleType, depth%)
' Recursively inverts this circle into each of the others
' Will only recurse while depth% < MaxDepth%

DIM reflections(LBOUND(circles) TO UBOUND(circles)) AS CircleType

nextdepth% = depth% + 1

' Draw this circle
IF this.CR >= .5 THEN CIRCLE (this.CX, this.CY), this.CR, this.CC

' Do the inversion
FOR i% = LBOUND(reflections) TO UBOUND(reflections)
  InvertCircle this, circles(i%), reflections(i%)
NEXT i%

' Do the recursion
IF depth% < MaxDepth% THEN
  FOR i% = LBOUND(circles) TO UBOUND(circles)
    InvertCircles circles(i%), reflections(), nextdepth%
  NEXT i%
END IF
END SUB

SUB InvertPoint (xc!, yc!, rc!, xp!, yp!)
' Invert point xp yp in circle xc yc rc
' Modifies xp, yp in place
xp! = xp! - xc!
yp! = yp! - yc!

d! = xp! * xp! + yp! * yp!

IF d! < Delta! THEN d! = 1

ir! = rc! * rc! / d! 'rc^2/(xp-xc)^2+(yp-yc)^2

xp! = xc! + xp! * ir!
yp! = yc! + yp! * ir!
END SUB

SUB SetPalette
OUT &H3C8, 1

' 7-colour palette evenly spaced in hue with saturation and value locked at 1

FOR hueidx% = 0 TO 6
  hue! = hueidx% * 360 / 7
  relativehue! = hue! - 60 * (hue! \ 60)
  cv! = relativehue! / 60

  r! = 0
  g! = 0
  b! = 0

  SELECT CASE hue!
    CASE 0 TO 60: r! = 1: g! = cv!
    CASE 60 TO 120: r! = 1 - cv!: g! = 1
    CASE 120 TO 180: g! = 1: b! = cv!
    CASE 180 TO 240: g! = 1 - cv!: b! = 1
    CASE 240 TO 300: b! = 1: r! = cv!
    CASE 300 TO 360: b! = 1 - cv!: r! = 1
  END SELECT

  rr% = INT(r! * 64)
  gg% = INT(g! * 64)
  bb% = INT(b! * 64)

  IF rr% > 63 THEN rr% = 63
  IF gg% > 63 THEN gg% = 63
  IF bb% > 63 THEN bb% = 63

  OUT &H3C9, rr%
  OUT &H3C9, gg%
  OUT &H3C9, bb%
NEXT hueidx%
END SUB

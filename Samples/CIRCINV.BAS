' Creator: William Gilbert <wgilbert@fatou.UWaterloo.ca>
' Date: July 1990, modified January 1993
' Translated to QuickBASIC by Jonathan Gilbert 2026-08-15

DECLARE SUB DrawFigure ()
DECLARE SUB FillCircle (x!, y!, r!, outlinewidth!, BYVAL col%)
DECLARE SUB InvertCircle (x!, y!, r!, xx!, yy!, rr!, xi!, yi!, ri!)
DECLARE SUB InvertPoint (xc!, yc!, rc!, xp!, yp!)
DECLARE SUB InvertThreeCircles (x0!, y0!, r0!, x1!, y1!, r1!, x2!, y2!, r2!, x3!, y3!, r3!, BYVAL depth%)
DECLARE SUB OutputVGAColourCMYK (c!, m!, y!, k!)
DECLARE SUB SetPalette ()

CONST MaxDepth% = 6 ' depth of iterations > 2 (0-based)

CONST Delta! = .00001  ' underflow

CONST BorderColour% = 7

SCREEN 12

SetPalette

DIM Colours(5) AS INTEGER

Colours(0) = 6 ' /dkred
Colours(1) = 2 ' /ltcream
Colours(2) = 3 ' /ltbrown
Colours(3) = 4 ' /dkbrown
Colours(4) = 5 ' /browngreen
Colours(5) = 1 ' /cream

DrawFigure

SUB DrawFigure
LINE (0, 0)-(639, 479), 15, BF

' Create a centred 500x500 coordinate system
VIEW (320 - 240, 240 - 240)-(320 + 239, 240 + 239)
WINDOW (0, 0)-(500, 500)

' Outer circle
x0! = 250: y0! = 250: r0! = 200

' Inner circles
x1! = 300: y1! = 190: r1! = 121.898
x2! = 219.85: y2! = 370.6: r2! = 75.6884
x3! = 121.358: y3! = 262.1941: r3! = 70.7811

FillCircle x0!, y0!, r0!, 2, 0

FillCircle x1!, y1!, r1!, 1.5, 1
FillCircle x2!, y2!, r2!, 1.5, 1
FillCircle x3!, y3!, r3!, 1.5, 1

InvertThreeCircles x0!, y0!, r0!, x1!, y1!, r1!, x2!, y2!, r2!, x3!, y3!, r3!, 2

WINDOW
VIEW
END SUB

SUB FillCircle (x!, y!, r!, outlinewidth!, BYVAL col%)
SHARED Colours() AS INTEGER

col% = Colours(col% MOD UBOUND(Colours))

IF outlinewidth! > 1 THEN
  rr! = r! + outlinewidth!

  CIRCLE (x!, y!), rr!, BorderColour%
  PAINT (x!, y!), 0, BorderColour%
  CIRCLE (x!, y!), rr!, 0
END IF

CIRCLE (x!, y!), r!, BorderColour%
PAINT (x!, y!), col%, BorderColour%

IF outlinewidth! > 1 THEN
  CIRCLE (x!, y!), r!, col%
ELSE
  CIRCLE (x!, y!), r!, 0
END IF
END SUB

SUB InvertCircle (x!, y!, r!, xx!, yy!, rr!, xi!, yi!, ri!)
' Invert circle xx yy rr in circle x y r
' Modifies xx!, yy!, rr! in place

d! = SQR((xx! - x!) ^ 2 + (yy! - y!) ^ 2)

IF d! < Delta! THEN d! = 1

ir! = rr! / d!

ix! = (xx! - x!) * ir!
iy! = (yy! - y!) * ir!

xa! = xx! + ix!
xb! = xx! - ix!

ya! = yy! + iy!
yb! = yy! - iy!

InvertPoint x!, y!, r!, xa!, ya!
InvertPoint x!, y!, r!, xb!, yb!

xi! = (xa! + xb!) * .5 ' xinvcircle
yi! = (ya! + yb!) * .5 ' yinvcircle
ri! = SQR((xa! - xb!) ^ 2 + (ya! - yb!) ^ 2) * .5 ' rinvcircle
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

SUB InvertThreeCircles (x0!, y0!, r0!, x1!, y1!, r1!, x2!, y2!, r2!, x3!, y3!, r3!, BYVAL depth%)
' Recursively inverts each circle in 3 others
' Outside circle is x0 y0 r0
' Will only recurse while depth% < MaxDepth%
InvertCircle x1!, y1!, r1!, x0!, y0!, r0!, x10!, y10!, r10!: FillCircle x10!, y10!, r10!, .5, depth%
InvertCircle x1!, y1!, r1!, x2!, y2!, r2!, x12!, y12!, r12!: FillCircle x12!, y12!, r12!, .5, depth%
InvertCircle x1!, y1!, r1!, x3!, y3!, r3!, x13!, y13!, r13!: FillCircle x13!, y13!, r13!, .5, depth%

InvertCircle x2!, y2!, r2!, x0!, y0!, r0!, x20!, y20!, r20!: FillCircle x20!, y20!, r20!, .5, depth%
InvertCircle x2!, y2!, r2!, x1!, y1!, r1!, x21!, y21!, r21!: FillCircle x21!, y21!, r21!, .5, depth%
InvertCircle x2!, y2!, r2!, x3!, y3!, r3!, x23!, y23!, r23!: FillCircle x23!, y23!, r23!, .5, depth%

InvertCircle x3!, y3!, r3!, x0!, y0!, r0!, x30!, y30!, r30!: FillCircle x30!, y30!, r30!, .5, depth%
InvertCircle x3!, y3!, r3!, x1!, y1!, r1!, x31!, y31!, r31!: FillCircle x31!, y31!, r31!, .5, depth%
InvertCircle x3!, y3!, r3!, x2!, y2!, r2!, x32!, y32!, r32!: FillCircle x32!, y32!, r32!, .5, depth%

IF depth% < MaxDepth% THEN
  depth% = depth% + 1

  InvertThreeCircles x1!, y1!, r1!, x10!, y10!, r10!, x12!, y12!, r12!, x13!, y13!, r13!, depth%
  InvertThreeCircles x2!, y2!, r2!, x20!, y20!, r20!, x21!, y21!, r21!, x23!, y23!, r23!, depth%
  InvertThreeCircles x3!, y3!, r3!, x30!, y30!, r30!, x31!, y31!, r31!, x32!, y32!, r32!, depth%
END IF
END SUB

SUB OutputVGAColourCMYK (c!, m!, y!, k!)
' Converts supplied CMYK to VGA RAMDAC 6:6:6-bit RGB and
' sends it to port &H3C9 (index assumed already set).
ci! = 1 - c!
mi! = 1 - m!
yi! = 1 - y!
ki! = 1 - k!

r! = ci! * ki!
g! = mi! * ki!
b! = yi! * ki!

rr% = INT(r! * 64)
gg% = INT(g! * 64)
bb% = INT(b! * 64)

IF rr% > 63 THEN rr% = 63
IF gg% > 63 THEN gg% = 63
IF bb% > 63 THEN bb% = 63

OUT &H3C9, rr%
OUT &H3C9, gg%
OUT &H3C9, bb%
END SUB

SUB SetPalette
' COLOR 15: /white {0 0 0 0} def
' COLOR 0: /black {0 0 0 1} def
' COLOR 12: /red {0 1 1 0} def
' COLOR 11: /cyan {1 0 0 0} def
' COLOR 13: /magenta {0 1 0 0} def
' COLOR 10: /green {1 0 1 0} def
' COLOR 14: /yellow {0 0 1 0} def
' COLOR 9: /blue {1 1 0 0} def

' COLOR 1: /cream {.09 .26 .49 0} def
' COLOR 2: /ltcream {0 .18 .44 0} def
' COLOR 3: /ltbrown {.24 .69 .86 0} def
' COLOR 4: /dkbrown {.75 .92 .99 0} def
' COLOR 5: /browngreen {.78 .88 .97 0} def
' COLOR 6: /dkred {.50 1 .99 0} def

OUT &H3C8, 1

OutputVGAColourCMYK .09#, .26, .49, 0
OutputVGAColourCMYK 0, .18, .44, 0
OutputVGAColourCMYK .24, .69, .86, 0
OutputVGAColourCMYK .75, .92, .99, 0
OutputVGAColourCMYK .78, .88, .97, 0
OutputVGAColourCMYK .5, 1, .99, 0

OUT &H3C8, 9

OutputVGAColourCMYK 1, 1, 0, 0
OutputVGAColourCMYK 1, 0, 1, 0
OutputVGAColourCMYK 1, 0, 0, 0
OutputVGAColourCMYK 0, 1, 1, 0
OutputVGAColourCMYK 0, 1, 0, 0
OutputVGAColourCMYK 0, 0, 1, 0
END SUB


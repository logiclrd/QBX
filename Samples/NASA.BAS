' This collection of SUBs allows an SVG curve to be
' translated directly to a series of CALLs.

DECLARE SUB DrawBezierCurve (dx1!, dy1!, dx2!, dy2!, dx!, dy!)
DECLARE SUB DrawBezierCurveTo (x1!, y1!, x2!, y2!, x!, y!)
DECLARE SUB DrawHorizontal (dx!)
DECLARE SUB DrawHorizontalTo (x!)
DECLARE SUB DrawLine (dx!, dy!)
DECLARE SUB DrawLineTo (x!, y!)
DECLARE SUB DrawVertical (dy!)
DECLARE SUB DrawVerticalTo (y!)
DECLARE SUB MoveTo (x!, y!)

DIM SHARED DrawX!, DrawY!

SCREEN 12

PAINT (0, 0), 15

COLOR 4

' NASAWorm.svg

' Curve 1
MoveTo 49.604729#, 312.00587#
DrawVerticalTo 203.99759#
DrawBezierCurve 0, -15.22331, 1.976073, -21.82086, 9.695206, -29.54212
DrawBezierCurve 5.561097, -5.56216, 15.823479#, -9.86547, 28.781281#, -9.86547
DrawBezierCurve 10.467758#, 0, 20.484324#, 3.46584, 26.872244#, 9.8644
DrawBezierCurve 5.60047, 5.61111, 8.67685, 11.37439, 13.48454, 29.54426
DrawLine 19.05628, 72.00375
DrawBezierCurve 1.67706, 6.34322, 4.38312, 8.79602, 9.16528, 8.79602
DrawBezierCurve 4.95774, 0, 7.55738, -2.97848, 7.55738, -8.79602
DrawVerticalTo 167.99519#
DrawHorizontal 31.05531
DrawVertical 108.00722#
DrawBezierCurve 0, 15.22437, -1.97501, 21.82086, -9.69414, 29.54212
DrawBezierCurve -5.56323, 5.56429, -15.82454, 9.86547, -28.78128, 9.86547
DrawBezierCurve -10.46776, 0, -20.48433, -3.46478, -26.87225, -9.8644
DrawBezierCurve -5.60047, -5.61111, -8.67684, -11.37439, -13.48454, -29.54426
DrawLineTo 97.383761#, 203.99759#
DrawBezierCurve -1.678118, -6.34322, -4.383114, -8.79496, -9.165274, -8.79496
DrawBezierCurve -4.958803, 0, -7.558451, 2.97742, -7.558451, 8.79496
DrawVertical 108.00828#
DrawLineTo 49.604729#, 312.00587#

' Curve 2
MoveTo 223.40548#, 312.00474#
DrawLine 35.86514, -112.19772#
DrawBezierCurve .39585, -1.23864, .76935, -2.28999, 1.34398, -2.99125
DrawBezierCurve .98538, -1.19926, 2.34958, -1.82922, 4.18306, -1.82922
DrawBezierCurve 1.83348, 0, 3.19662, .62996, 4.18306, 1.82922
DrawBezierCurve .57462, .70019, .94813, 1.75155, 1.34292, 2.99125
DrawLine 35.86513, 112.19772#
DrawHorizontal 93.38194
DrawVertical .001
DrawBezierCurve 20.34599, 0, 30.26999, -4.32245, 38.11043, -12.11927
DrawBezierCurve 8.67578, -8.6247, 12.82585, -18.06132, 12.82585, -32.15029
DrawBezierCurve 0, -12.44915, -4.55763, -23.97996, -11.49676, -30.87972
DrawBezierCurve -9.0727, -9.02269, -20.34174, -11.86602, -38.71698, -11.86602
DrawLine -25.7379, -.001
DrawBezierCurve -9.8346, 0, -13.75376, -1.18862, -16.85461, -4.2352
DrawBezierCurve -2.1293, -2.08994, -3.12745, -5.19823, -3.12745, -8.862
DrawBezierCurve 0, -3.78721, .91195, -7.52759, 3.51479, -10.08255
DrawBezierCurve 2.31021, -2.26764, 5.45894, -3.30835, 11.50954, -3.30835
DrawHorizontal 74.38524
DrawVertical -30.50622
DrawHorizontal -68.7039
DrawBezierCurve -20.346, 0, -30.26892, 4.32352, -38.11043, 12.11927
DrawBezierCurve -8.67472, 8.6247, -12.82585, 18.06026, -12.82585, 32.14923
DrawBezierCurve 0, 12.45128, 4.55763, 23.98103, 11.49676, 30.87972
DrawBezierCurve 9.07376, 9.02269, 20.34174, 11.86708, 38.71698, 11.86708
DrawLine 25.73896, .002
DrawBezierCurve 9.8346, 0, 13.75376, 1.18649, 16.85461, 4.23414
DrawBezierCurve 2.12931, 2.09099, 3.12745, 5.19823, 3.12745, 8.862
DrawBezierCurve 0, 3.7872, -.91195, 7.52759, -3.51373, 10.08254
DrawBezierCurve -2.31127, 2.26764, -5.46, 3.30835, -11.51059, 3.30835
DrawHorizontal -76.71674
DrawLine -29.42401, -92.05604
DrawBezierCurve -2.04098, -6.38472, -4.9588, -11.80536, -8.66088, -15.41698
DrawBezierCurve -6.35387, -6.19425, -13.84102, -9.43663, -25.65596, -9.43663
DrawBezierCurve -11.81493, 0, -19.30315, 3.24238, -25.65383, 9.43556
DrawBezierCurve -3.70314, 3.61269, -6.61989, 9.03226, -8.66088, 15.41699
DrawLine -39.17561, 122.56226#
DrawLineTo 223.40548#, 312.00474#

' Curve 3
MoveTo 443.41143#, 312.00587#
DrawLine 39.17562, -122.56226#
DrawHorizontal .5 ' HAX: Without this, the Bezier spline ends up making the final 'A' look like it has a helmet :-P
DrawBezierCurve 2.04098, -6.38472, 4.9588, -11.80536, 8.66088, -15.41698
DrawBezierCurve 6.35386, -6.19425, 13.84102, -9.43663, 25.65595, -9.43663
DrawBezierCurve 11.81494, 0, 19.30316, 3.24238, 25.6549, 9.43556
DrawBezierCurve 3.70207, 3.61269, 6.61989, 9.03226, 8.66194, 15.41699
DrawLine 39.17455, 122.56226#
DrawHorizontalTo 558.295
DrawVertical .001
DrawLineTo 522.42986#, 199.80815#
DrawBezierCurve -.39585, -1.23864, -.76936, -2.28999, -1.34398, -2.99124
DrawBezierCurve -.98538, -1.19927, -2.34852, -1.82923, -4.18306, -1.82923
DrawBezierCurve -1.83348, 0, -3.19662, .62996, -4.18306, 1.82923
DrawBezierCurve -.57356, .70019, -.94813, 1.75154, -1.34292, 2.99124
DrawLine -35.86407, 112.19772#
DrawLineTo 443.41143#, 312.00587#

' Fill them in! (Comment these out for outlines.)
PAINT (51, 311), 4, 4
PAINT (223, 311), 4, 4
PAINT (445, 311), 4, 4

SUB DrawBezierCurve (dx1!, dy1!, dx2!, dy2!, dx!, dy!)
DrawBezierCurveTo DrawX! + dx1!, DrawY! + dy1!, DrawX! + dx2!, DrawY! + dy2!, DrawX! + dx!, DrawY! + dy!
END SUB

SUB DrawBezierCurveTo (x2!, y2!, x3!, y3!, x4!, y4!)
' Strategy: See if we can just draw a straight line, and it'll be close enough to the actual curve.
' If not, then use De Casteljau's algorithm to seamlessly split the curve in two (t=0.5), and then
' recursively draw each half. As we keep zooming in on the curve, the bit we're looking at gets
' straighter and straighter, until we can just draw a line.

x1! = DrawX!
y1! = DrawY!

dx! = x4! - x1!
dy! = y4! - y1!

d2! = ABS((x2! - x4!) * dy! - (y2! - y4!) * dx!)
d3! = ABS((x3! - x4!) * dy! - (y3! - y4!) * dx!)

controld! = d2! + d3!
controld! = controld! * controld!

segmentd! = dx! * dx! + dy! * dy!

CONST Tolerance! = .001

IF controld! < Tolerance! * segmentd! THEN
  ' The curve is very close to a straight line; we can draw it
  ' as one with no consequences.

  LINE (DrawX!, DrawY!)-(x4!, y4!)

  DrawX! = x4!
  DrawY! = y4!
ELSE
  ' Not close enough to a straight line; subdivide the curve.

  ' Calculate control points for the two halves.

  x12! = (x1! + x2!) * .5
  y12! = (y1! + y2!) * .5
  x23! = (x2! + x3!) * .5
  y23! = (y2! + y3!) * .5
  x34! = (x3! + x4!) * .5
  y34! = (y3! + y4!) * .5

  x123! = (x12! + x23!) * .5
  y123! = (y12! + y23!) * .5
  x234! = (x23! + x34!) * .5
  y234! = (y23! + y34!) * .5

  ' This is the exact midpoint (t = .5) of the curve. This
  ' becomes the end of the first half and the start of the
  ' second half.

  mx! = (x123! + x234!) * .5
  my! = (y123! + y234!) * .5

  ' Draw each half as its own Bezier curve.
  DrawBezierCurveTo x12!, y12!, x123!, y123!, mx!, my!
  DrawBezierCurveTo x234!, y234!, x34!, y34!, x4!, y4!
END IF
END SUB

SUB DrawHorizontal (dx!)
DrawLineTo DrawX! + dx!, DrawY!
END SUB

SUB DrawHorizontalTo (x!)
DrawLineTo x!, DrawY!
END SUB

SUB DrawLine (dx!, dy!)
DrawLineTo DrawX! + dx!, DrawY! + dy!
END SUB

SUB DrawLineTo (x!, y!)
LINE (DrawX!, DrawY!)-(x!, y!)

DrawX! = x!
DrawY! = y!
END SUB

SUB DrawVertical (dy!)
DrawLineTo DrawX!, DrawY! + dy!
END SUB

SUB DrawVerticalTo (y!)
DrawLineTo DrawX!, y!
END SUB

SUB Move (dx!, dy!)
MoveTo DrawX! + dx!, DrawY + dy!
END SUB

SUB MoveTo (x!, y!)
DrawX! = x!
DrawY! = y!
END SUB


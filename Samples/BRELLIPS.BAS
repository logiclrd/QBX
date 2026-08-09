' Bresenham Ellipse Algorithm
' ---------------------------
'
' This code was written with future translation to TRS-80 BASIC in mind.
'
' Required changes:
' - Add line numbers. Map subroutine entrypoints to line numbers.
' - Change PSET (x, y) to SET(x, y).
' - Remove SCREEN statement.

SCREEN 13

XC = 160
YC = 100
R = 15

GOSUB TRS80Circle

END

TRS80Circle:
REM Parameters: (XC, YC) = centre, R = radius Y
XR = R * 2
YR = R
GOSUB BresenhamEllipse
RETURN

BresenhamEllipse:
REM Parameters: (XC, YC) = centre, (XR, YR) = radius for each axis
A2 = 2 * XR * XR
B2 = 2 * YR * YR
X = XR
Y = 0
XD = YR * YR * (1 - 2 * XR)
YD = XR * XR
EE = 0
XS = B2 * XR
YS = 0
REM 1st set of points, y' > -1
TopOfLoop1:
IF XS < YS THEN GOTO EndOfLoop1
PSET (XC - X, YC - Y): PSET (XC + X, YC - Y)
PSET (XC - X, YC + Y): PSET (XC + X, YC + Y)
Y = Y + 1
YS = YS + A2
EE = EE + YD
YD = YD + A2
IF 2 * EE + XD <= 0 THEN GOTO TopOfLoop1
X = X - 1
XS = XS - B2
EE = EE + XD
XD = XD + B2
GOTO TopOfLoop1
EndOfLoop1:
REM 1st point set is done; start the 2nd set of points
X = 0
Y = YR
XD = YR * YR
YD = XR * XR * (1 - 2 * YR)
EE = 0
XS = 0
YS = A2 * YR
REM 2nd set of points, y' < 1
TopOfLoop2:
IF XS > YS THEN RETURN
PSET (XC - X, YC - Y): PSET (XC + X, YC - Y)
PSET (XC - X, YC + Y): PSET (XC + X, YC + Y)
X = X + 1
XS = XS + B2
EE = EE + XD
XD = XD + B2
IF 2 * EE + YD <= 0 THEN GOTO TopOfLoop2
Y = Y - 1
YS = YS - A2
EE = EE + YD
YD = YD + A2
GOTO TopOfLoop2


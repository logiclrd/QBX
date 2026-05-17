' Interference by K Moerman 2026 for QB64 Phoenix Edition
' Ported to Basic256 by Michel Vilain
' Ported to QuickBASIC by Jonathan Gilbert

SCREEN 13

DIM buf&(16001)

GET (0, 0)-(319, 199), buf&

DEF SEG = VARSEG(buf&(1))

bufofs% = VARPTR(buf&(1))

OUT &H3C8, 0
FOR i% = 0 TO 127
  OUT &H3C9, i% * 2
  OUT &H3C9, i% * 4
  OUT &H3C9, i% * 8
NEXT i%
FOR i% = 0 TO 127
  OUT &H3C9, i% * 8
  OUT &H3C9, i% * 4
  OUT &H3C9, i% * 2
NEXT i%

CONST PI# = 3.14159265358979

hw% = 160
hh% = 100

' wavelength related to point a and b
lambda# = 56
lambdadiv2pi# = lambda# / (PI# * 2)

' time period of point a and b
Tper# = .45
Tdiv2PI# = Tper# / (PI# * 2)

' number of frames in one revolution of point a and b
Na% = 1000
Nb% = 550

' radius of circular motion of point a and b
Ra# = 6 * hh% / 7
Rb# = Ra# - 4 * lambdadiv2pi#

' increment step for ta and tb
dta# = 2 * PI# / Na%
dtb# = 2 * PI# / Nb%

' time step
dt# = 1 / 60

' turn division into multiplication faster
invlambda# = 1 / lambdadiv2pi#
ta# = 0
tb# = 0
t# = 0

DO
  xa# = Ra# * COS(ta#)
  ya# = Ra# * SIN(ta#)
  xb# = -Rb# * COS(tb#)
  yb# = -Rb# * SIN(tb#)

  tphase# = t# / Tdiv2PI#

  o& = bufofs%

  cya# = -99.5 - ya#
  cyb# = -99.5 - yb#

  FOR y% = 0 TO 199 STEP 2
    cxa# = -159.5 - xa#
    cxb# = -159.5 - xb#

    FOR x% = 0 TO 319 STEP 2
      da# = SQR(cxa# * cxa# + cya# * cya#)
      db# = SQR(cxb# * cxb# + cyb# * cyb#)

      wave# = SIN(da# * invlambda# - tphase#) + SIN(db# * invlambda# - tphase#)

      abswave% = (ABS(wave#) * 15.5) AND 127

      IF wave# < 0 THEN abswave% = abswave% + 128

      POKE o&, abswave%
      POKE o& + 320, abswave%
      o& = o& + 1
      POKE o&, abswave%
      POKE o& + 320, abswave%
      o& = o& + 1
      cxa# = cxa# + 2
      cxb# = cxb# + 2
    NEXT x%

    o& = o& + 320

    cya# = cya# + 2
    cyb# = cyb# + 2
  NEXT y%

  PUT (0, 0), buf&, PSET

  ' update angles for movement of point a and b
  ta# = ta# + dta#
  IF ta# >= 2 * PI# THEN ta# = 0
  tb# = tb# + dtb#
  IF tb# >= 2 * PI# THEN tb# = 0
  t# = t# + dt#
LOOP WHILE INKEY$ = ""

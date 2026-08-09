CONST PixelsFromEyeToCentreOfScreen# = 750

CONST SphereStripes% = 32
CONST SphereDotsPerStripe% = 24
CONST NumDots% = SphereStripes% * SphereDotsPerStripe%

CONST SphereRadius# = 1
CONST SphereCentreX# = 0
CONST SphereCentreY# = 0
CONST SphereCentreZ# = 8
TYPE pointType
 x AS DOUBLE
 y AS DOUBLE
 z AS DOUBLE
 oldScreenX AS INTEGER
 oldScreenY AS INTEGER
END TYPE
DECLARE SUB makeSphere (dot() AS pointType)
DIM dot(1 TO NumDots%) AS pointType
makeSphere dot()
SCREEN 13
DO
 '0.003 radians == 17.2 degrees
 rotation# = rotation# + .003
 IF rotation# > 6.283185 THEN rotation# = rotation# - 6.283185

 FOR i% = 1 TO NumDots%
  thisX# = dot(i%).x
  thisY# = dot(i%).y
  thisZ# = dot(i%).z

  'rotate point around Y axis at Sphere Center (X,Y,Z)
  rotatedX# = COS(rotation#) * (thisX# - SphereCentreX#) - SIN(rotation#) * (thisZ# - SphereCentreZ#) + SphereCentreX#
  rotatedY# = thisY#
  rotatedZ# = SIN(rotation#) * (thisX# - SphereCentreX#) + COS(rotation#) * (thisZ# - SphereCentreZ#) + SphereCentreZ#

  'translate to screen coordinates
  pointSX# = rotatedX# / rotatedZ#
  pointSY# = rotatedY# / rotatedZ#

  screenX% = 160 + pointSX# * PixelsFromEyeToCentreOfScreen#
  screenY% = 100 - pointSY# * PixelsFromEyeToCentreOfScreen#

  'do a little bit of depth queuing (very simple =D)
  IF rotatedZ# > SphereCentreZ# THEN
   colour% = 8 'dark gray
  ELSEIF rotatedZ# > SphereCentreZ# - SphereRadius# / 2 THEN
   colour% = 7 'light gray
  ELSE
   colour% = 15 'white
  END IF

  'draw pixel
  PSET (screenX%, screenY%), colour%

  'erase old pixel in a fairly flicker-free way
  IF (dot(i%).oldScreenX <> screenX%) OR (dot(i%).oldScreenY <> screenY%) THEN
   PSET (dot(i%).oldScreenX, dot(i%).oldScreenY), 0
  END IF

  'store new pixel's position for next time
  dot(i%).oldScreenX = screenX%
  dot(i%).oldScreenY = screenY%
 NEXT i%
LOOP UNTIL INKEY$ <> ""

SUB makeSphere (dot() AS pointType)
FOR i% = 1 TO SphereStripes%
 heading# = 6.283185 * i% / SphereStripes%
 FOR j% = 1 TO SphereDotsPerStripe%
  pitch# = 3.141592 * (j% - 1) / (SphereDotsPerStripe% - 1) - 1.570796
  thisDotX# = SphereRadius# * SIN(heading#) * COS(pitch#)
  thisDotY# = SphereRadius# * SIN(pitch#)
  thisDotZ# = SphereRadius# * COS(heading#) * COS(pitch#)
  thisDotIndex% = (i% - 1) * SphereDotsPerStripe% + j%
  dot(thisDotIndex%).x = thisDotX# + SphereCentreX#
  dot(thisDotIndex%).y = thisDotY# + SphereCentreY#
  dot(thisDotIndex%).z = thisDotZ# + SphereCentreZ#
 NEXT j%
NEXT i%
END SUB


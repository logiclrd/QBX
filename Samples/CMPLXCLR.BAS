DECLARE FUNCTION MapComplexToColourDoublePole% (zr#, zi#)
DECLARE SUB SetComplexColourPalette ()
DECLARE FUNCTION ATAN2# (y#, x#)
DECLARE FUNCTION MapComplexToColour% (zr#, zi#)

CONST PI# = 3.14159265358979#

SCREEN 13

SetComplexColourPalette

FOR y% = 0 TO 199
  FOR x% = 0 TO 319
    zr# = (x% - 159.5) / 12.5
    zi# = (99.5 - y%) / 12.5

    PSET (x%, y%), MapComplexToColour%(zr#, zi#)
  NEXT x%
NEXT y%

FUNCTION ATAN2# (y#, x#)
IF x# = y# THEN
  IF y# > 0 THEN
    ATAN2# = PI# * .25
  ELSE
    ATAN2# = PI# * 1.25
  END IF
ELSEIF x# = -y# THEN
  IF y# > 0 THEN
    ATAN2# = PI# * .75
  ELSE
    ATAN2# = PI# * 1.75
  END IF
ELSE
  ON LOCAL ERROR RESUME NEXT

  isusable% = -1

  slope# = y# / x#
  IF ERR THEN isusable% = 0

  IF isusable% THEN
    angle# = ATN(slope#)

    IF x# < 0 THEN angle# = angle# + PI#
    IF angle# < 0 THEN angle# = angle# + 2 * PI#

    ATAN2# = angle#
  ELSEIF y# <> 0 THEN
    IF (y# > 0) EQV (x# > 0) THEN
      ATAN2# = PI# * .5
    ELSE
      ATAN2# = -PI# * .5
    END IF
  END IF
END IF
END FUNCTION

FUNCTION MapComplexToColour% (zr#, zi#)
arg# = ATAN2#(zi#, zr#)

t# = arg# / PI#

IF t# >= 1 THEN t# = t# - 1: basecolour% = 128

d# = zr# * zr# + zi# * zi#

mag# = SQR(zr# * zr# + zi# * zi#)

' Check for the intermediate value being unrepresentable
IF (d# > 0) AND (mag# >= d#) THEN
  mag# = 1
END IF

octave# = LOG(mag#) / LOG(2)

octavesize# = 2 ^ INT(octave#)

intensity# = (mag# - octavesize#) / octavesize#

IF t# < .5 THEN
  r1# = 0
  r2# = t# * 2
ELSE
  r1# = (t# - .5) * 2
  r2# = 1
END IF

c% = basecolour% + (r1# + (r2# - r1#) * intensity#) * 128

IF c% < 0 THEN c% = 0
IF c% > 255 THEN c% = 255

MapComplexToColour% = c%
END FUNCTION

SUB SetComplexColourPalette
OUT &H3C8, 0

FOR i% = 0 TO 127
  OUT &H3C9, i% \ 2
  OUT &H3C9, 0
  OUT &H3C9, 0
NEXT i%

FOR i% = 128 TO 255
  OUT &H3C9, 63
  OUT &H3C9, (i% - 128) * 63 \ 127
  OUT &H3C9, 0
NEXT i%
END SUB

DECLARE SUB ComplexSin (BYVAL zr#, BYVAL zi#, sr#, si#)
DECLARE SUB ComplexDivide (BYVAL ar#, BYVAL ai#, BYVAL br#, BYVAL bi#, cr#, ci#)
DECLARE SUB ComplexMultiply (BYVAL ar#, BYVAL ai#, BYVAL br#, BYVAL bi#, cr#, ci#)
DECLARE SUB SetComplexColourPalette ()
DECLARE FUNCTION MapComplexToColour% (zr#, zi#)

SCREEN 13

SetComplexColourPalette

' Function:
'
'   f(z) = (1 + i) sin z
'
' Iterated multiple times

FOR y% = 0 TO 199
  FOR x% = 0 TO 319
    zr# = (x% - 159.5) / 32
    zi# = (99.5 - y%) / 32

    FOR iter% = 1 TO 5
      IF ABS(zi#) > 500 THEN
        ' Result likely to not be representable; just paint black.
        zr# = 1
        zi# = 0
        EXIT FOR
      END IF

      ComplexSin zr#, zi#, zr#, zi#

      ComplexMultiply 1, 1, zr#, zi#, zr#, zi#
    NEXT iter%

    PSET (x%, y%), MapComplexToColour%(zr#, zi#)
  NEXT x%
NEXT y%

WHILE INKEY$ = "": WEND


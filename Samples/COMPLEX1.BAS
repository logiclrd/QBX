DECLARE SUB ComplexMultiply (BYVAL ar#, BYVAL ai#, BYVAL br#, BYVAL bi#, cr#, ci#)
DECLARE SUB SetComplexColourPalette ()
DECLARE FUNCTION MapComplexToColour% (zr#, zi#)

SCREEN 13

SetComplexColourPalette

' Fourth-degree polynomial:
'
'   f(z) = (z + 2)^2 (z - 1 - 2i) (z + i)

FOR y% = 0 TO 199
  FOR x% = 0 TO 319
    zr# = (x% - 159.5) / 32
    zi# = (99.5 - y%) / 32

    f1r# = zr# + 2
    f1i# = zi#
    ComplexMultiply f1r#, f1i#, f1r#, f1i#, f1r#, f1i#

    f2r# = zr# - 1
    f2i# = zi# - 2

    f3r# = zr#
    f3i# = zi# + 1

    ComplexMultiply f1r#, f1i#, f2r#, f2i#, fr#, fi#
    ComplexMultiply fr#, fi#, f3r#, f3i#, fr#, fi#

    PSET (x%, y%), MapComplexToColour%(fr#, fi#)
  NEXT x%
NEXT y%

WHILE INKEY$ = "": WEND


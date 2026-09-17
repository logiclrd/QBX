DECLARE FUNCTION GCD% (BYVAL a%, BYVAL b%)

' Thomae's function is defined as:
'
'   f(x) := 1/q if x = p/q (reduced)
'
'   f(x) := 0 if x = 0 or x is irrational
'
' This function is explicitly discontinuous over the rationals
' but continuous over the irrationals. Even though the rationals
' are dense in R, this function manages to break continuity only
' on those specific points.
'
' We plot it here by noting that if p/q is not reduced, then it
' can be reduced by dividing both p and q by their GCD. This
' means that the function's value 1/q becomes 1/(q/GCD(p,q)),
' which is simply GCD(p,q) / q.

SCREEN 12

DIM origin%(385)
DIM oneone%(385)

LOCATE 1, 1: PRINT "(0, 0)"
GET (0, 0)-(47, 15), origin%
LOCATE 1, 1: PRINT "(1, 1)"
GET (0, 0)-(47, 15), oneone%

CLS

WINDOW (-.1, -.1)-(1.1, 1.1)

PUT (0, 0), origin%, PSET
PUT (1, 1), oneone%, PSET

LINE (1, -1)-(1, 2), 8
LINE (-1, 1)-(2, 1), 8

LINE (0, -1)-(0, 2)
LINE (-1, 0)-(2, 0)

FOR i% = 1 TO 400
  FOR j% = 1 TO i% - 1
    x# = j% / i%
    y# = GCD%(i%, j%) / i%

    PSET (x#, y#)
  NEXT j%
NEXT i%

WHILE INKEY$ = "": WEND

FUNCTION GCD% (BYVAL a%, BYVAL b%)
IF a% < b% THEN SWAP a%, b%

r% = a% MOD b%

GCD% = b%

WHILE r% <> 0
  GCD% = r%

  a% = b%
  b% = r%

  r% = a% MOD b%
WEND
END FUNCTION

DECLARE FUNCTION CountFriends% (n%)
DECLARE FUNCTION GCD% (BYVAL A%, BYVAL b%)

' This program visualizes Euler's "totient" phi(n) for n = 1..640.
' The totient of a number is the count of numbers below n that are
' coprime to n, sometimes called the "friends" of n.
'
' This relationship has a curious property: Its running average
' converges on a specific value, 6 / pi^2, most commonly referred to
' as the asymptotic density of coprime pairs -- the probability that
' two randomly-chosen integers are coprime.

CONST PI# = 3.14159265358979#

CONST Density# = 6 / (PI# * PI#)

SCREEN 12

sum& = 0

convy% = 479 * (1 - Density#)

FOR i% = 0 TO 639
  n% = i% + 1

  f% = CountFriends%(n%)

  phi# = f% / n%

  LINE (i%, 479)-(i%, 479 * (1 - phi#))

  sum# = sum# + phi#

  ' Plot the running average
  y% = 479 * (1 - sum# / n%)
  PSET (i%, y%), 12

  ' Plot the limit value as a constant line
  PSET (i%, convy%), 9
NEXT i%

WHILE INKEY$ = "": WEND

FUNCTION CountFriends% (n%)
IF n% < 2 THEN
  CountFriends% = 1
ELSE
  FOR i% = 1 TO n% - 1
    IF GCD%(i%, n%) = 1 THEN c% = c% + 1
  NEXT i%

  CountFriends% = c%
END IF
END FUNCTION

FUNCTION GCD% (BYVAL A%, BYVAL b%)
IF A% < b% THEN SWAP A%, b%

r% = A% MOD b%

GCD% = b%

WHILE r% <> 0
  GCD% = r%

  A% = b%
  b% = r%

  r% = A% MOD b%
WEND
END FUNCTION

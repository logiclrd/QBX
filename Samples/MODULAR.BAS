' This program draws the Modular Discriminant (Dedekind eta to the 24th)
'  as a function of the square of the nome q = e ^ (i pi tau).
'
' I have no idea what this means, but this is the math behind two of the
' images at:
'
'   https://linas.org/art-gallery/numberetic/numberetic.html
'
' The specific steps being graphed:
' - Points within the unit circle are identified.
' - They are interpreted as a nome (I'm not sure what that is :-), and
'   converted back to an underlying complex number tau.
' - The modular discriminant of tau is then computed.
' - The graph is drawn wherever the value is positive and the colour
'   drawn is based on the magnitude of the value.
'   => The left graph draws wherever the real part is positive.
'   => The right graph draws wherever the imaginary part is positive.

DECLARE SUB ComplexDivide (BYVAL ar#, BYVAL ai#, BYVAL br#, BYVAL bi#, cr#, ci#)
DECLARE SUB ComplexMultiply (BYVAL ar#, BYVAL ai#, BYVAL br#, BYVAL bi#, cr#, ci#)
DECLARE SUB ModularDiscriminant (BYVAL tr#, BYVAL ti#, dr#, di#)
DECLARE SUB ReduceTau (tr#, ti#, c#, d#)
DECLARE FUNCTION ATAN2# (y#, x#)
DECLARE FUNCTION NomeToTau% (x#, y#, tr#, ti#)

CONST PI# = 3.14159265358979#

SCREEN 13

OUT &H3C8, 1

FOR i% = 1 TO 255
  IF i% < 64 THEN
    r! = 0
    g! = i% / 64!
    b! = 1 - g!
  ELSE
    r! = ((i% - 20) / (255 - 20)) ^ 2
    g! = (1 - r!) ^ .5
    b! = 0
  END IF

  r! = r! ^ .25
  g! = g! ^ .5
  b! = b! ^ .5

  OUT &H3C9, r! * 63
  OUT &H3C9, g! * 63
  OUT &H3C9, b! * 63
NEXT i%

radius! = 75

FOR y% = 0 TO 199
  y! = (y% - 99.5) / radius!

  IF ABS(y!) <= 1 THEN
    xr! = SQR(1 - y! * y!) * radius!

    LINE (80 - xr!, y%)-(79 + xr!, y%), 15

    FOR x% = 79.5 - xr! TO 79.5 + xr!
      x! = (x% - 79.5) / radius!

      IF NomeToTau(BYVAL x!, BYVAL y!, taur#, taui#) THEN
        ModularDiscriminant taur#, taui#, dr#, di#

        d# = dr# * dr# + di# * di#
        IF d# > 1D+115 THEN
          c% = 255
        ELSEIF d# < 1 THEN
          c% = 1
        ELSE
          c% = 5 * LOG(d#) + 1
          IF c% > 255 THEN c% = 255
        END IF

        IF dr# > 0 THEN
          PSET (x%, y%), c%
        ELSE
          PSET (x%, y%), 0
        END IF

        IF di# > 0 THEN
          PSET (320 - x%, y%), c%
        END IF
      END IF
    NEXT x%
  END IF
NEXT y%

SLEEP


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

SUB ComplexDivide (BYVAL ar#, BYVAL ai#, BYVAL br#, BYVAL bi#, cr#, ci#)
id# = 1# / (br# * br# + bi# * bi#)

cr# = (ar# * br# + ai# * bi#) * id#
ci# = (ai# * br# - ar# * bi#) * id#
END SUB

SUB ComplexMultiply (BYVAL ar#, BYVAL ai#, BYVAL br#, BYVAL bi#, cr#, ci#)
cr# = ar# * br# - ai# * bi#
ci# = ar# * bi# + ai# * br#
END SUB

SUB ModularDiscriminant (BYVAL tr#, BYVAL ti#, dr#, di#)
t0r# = tr#
t0i# = ti#

ReduceTau tr#, ti#, c#, d#

' The modular discriminant is a modular form of weight 12. The
' reduction of tau thus introduces a complex twelfth-power scale/rotation
' with each inversion:
'
'    D(-1 / tau) = tau^12 D(tau)
'
' The reduction has been tracked in the form:
'
'                      a tau + b
'       tau_reduced = -----------
'                      c tau + d
'
' This gives us:
'
'    D(tau_reduced) = (c tau + d)^12 D(tau)
'
' Hence:
'
'              D(tau_reduced)
'    D(tau) = ----------------
'              (c tau + d)^12
'
' We calculate this denominator as f12.

fr# = c# * t0r# + d#
fi# = c# * t0i#

ComplexMultiply fr#, fi#, fr#, fi#, fr2#, fi2#
ComplexMultiply fr2#, fi2#, fr2#, fi2#, fr4#, fi4#
ComplexMultiply fr4#, fi4#, fr4#, fi4#, fr8#, fi8#

ComplexMultiply fr8#, fi8#, fr4#, fi4#, fr12#, fi12#

' q = exp(2*pi*i*tau)

er# = EXP(-2 * PI# * ti#)
ang# = 2# * PI# * tr#

qr# = er# * COS(ang#)
qi# = er# * SIN(ang#)

e4r# = 1#
e4i# = 0#

e6r# = 1#
e6i# = 0#

' q^1
qnr# = qr#
qni# = qi#

FOR n% = 1 TO 12 ' for more terms calculate more coefficients!
  ' Coefficient of q^n in E4 and E6
  SELECT CASE n%
    CASE 1: c4# = 240: c6# = -504
    CASE 2: c4# = 2160: c6# = -16632
    CASE 3: c4# = 6720: c6# = -122976
    CASE 4: c4# = 17520: c6# = -532728
    CASE 5: c4# = 30240: c6# = -1575504
    CASE 6: c4# = 60480: c6# = -4058208
    CASE 7: c4# = 82560: c6# = -8471232
    CASE 8: c4# = 140400: c6# = -17047800
    CASE 9: c4# = 181680: c6# = -29883672
    CASE 10: c4# = 272160: c6# = -51991632
    CASE 11: c4# = 319680: c6# = -81170208
    CASE 12: c4# = 490560: c6# = -129985632
  END SELECT

  e4r# = e4r# + c4# * qnr#
  e4i# = e4i# + c4# * qni#

  e6r# = e6r# + c6# * qnr#
  e6i# = e6i# + c6# * qni#

  ' q^(n + 1) = q^n * q
  ComplexMultiply qnr#, qni#, qr#, qi#, qnr#, qni#
NEXT n%

' A = E4^3

ComplexMultiply e4r#, e4i#, e4r#, e4i#, ar#, ai#
ComplexMultiply ar#, ai#, e4r#, e4i#, ar#, ai#

' B = E6^2

ComplexMultiply e6r#, e6i#, e6r#, e6i#, br#, bi#

' Modular discriminant:
'
' delta = (E4^3 - E6^2) / 1728

i1728# = 1 / 1728#

dr# = (ar# - br#) * i1728#
di# = (ai# - bi#) * i1728#

' Undo the reduction transformation
ComplexDivide dr#, di#, fr12#, fi12#, dr#, di#
END SUB

FUNCTION NomeToTau% (x#, y#, tr#, ti#)
r2# = x# * x# + y# * y#

IF r2# < 1 THEN
  ' |Q| = sqrt(r2)

  ' Im(tau) = -log(|Q|) / 2pi
  '
  ' log(|Q|) = log(r^2) / 2#, and we already have r^2

  ti# = -LOG(r2#) / (4 * PI#)

  ' Re(tau) = arg(Q) / 2pi

  tr# = ATAN2#(y#, x#) / (2 * PI#)

  NomeToTau% = -1
END IF
END FUNCTION

SUB ReduceTau (tr#, ti#, c#, d#)
' We need to be able to reverse the reduction later. We can do that
' by tracking the reduction. The final result will be of the form:
'
'           a tau + b
'  tau_R = -----------
'           c tau + d
'
' The initial values represent the identity:
'
'         1 tau + 0
'  tau = -----------
'         0 tau + 1

t0r# = tr#
t0i# = ti#

a# = 1
b# = 0
c# = 0
d# = 1

DO
  ' Shift real part into approximately [-1/2, 1/2].

  ' INT(+.5) rounds .5 toward negative infinity
  ' FIX(+.5) rounds .5 rounds toward zero
  ' We want to round .5 away from zero
  IF tr# > 0 THEN
    n& = INT(tr# + .5)
  ELSE
    n& = -INT(-tr# + .5)
  END IF

  tr# = tr# - n&

  ' Matrix update:
  ' [ 1 -N ] [ A B ]
  ' [ 0  1 ] [ C D ]

  a# = a# - n& * c#
  b# = b# - n& * d#

  ' If we're outside the unit circle, we're done.
  den# = tr# * tr# + ti# * ti#

  IF den# >= 1 THEN EXIT DO

  ' tau = -1 / tau
  tr# = -tr# / den#
  ti# = ti# / den#

  ' Matrix update:
  ' [ 0 -1 ] [ A B ]
  ' [ 1  0 ] [ C D ]
  SWAP a#, c#: a# = -a#
  SWAP b#, d#: b# = -b#
LOOP

test# = a# * d# - b# * c#
ComplexDivide a# * t0r# + b#, a# * t0i#, c# * t0r# + d#, c# * t0i#, testr#, testi#
END SUB

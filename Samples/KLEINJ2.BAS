' This program draws the real part of Klein's J-invariant the Modular Discriminant (Dedekind eta
' to the 24th) over the unit complex numbers. This configuration was reached while trying to
' produce KLEINJ.BAS, and was interesting enough that I decided to keep it. It's missing a key
' step in the computation: the nome-to-tau transformation.
'
' The specific steps being graphed:
' - Points within the unit circle are identified.
' - They are interpreted as complex number tau directly.
' - The Klein J-invariant g_2^3 / delta is then computed.
' - The graph is drawn wherever the real value is positive and the colour
'   drawn is based on the magnitude of the value.

DECLARE SUB KleinJ (BYVAL tr#, BYVAL ti#, jr#, ji#)
DECLARE SUB ComplexDivide (BYVAL ar#, BYVAL ai#, BYVAL br#, BYVAL bi#, cr#, ci#)
DECLARE SUB ComplexMultiply (BYVAL ar#, BYVAL ai#, BYVAL br#, BYVAL bi#, cr#, ci#)
DECLARE SUB ReduceTau (tr#, ti#)
DECLARE FUNCTION DiskToUpperHalf% (x#, y#, taur#, taui#)

CONST PI# = 3.14159265358979#

SCREEN 13

OUT &H3C8, 1

FOR i% = 1 TO 255
  IF i% < 127 THEN
    r% = 0
    g% = i% \ 2
    b% = 63 - g%
  ELSE
    r% = (i% - 128) \ 2
    g% = 63 - r%
    b% = 0
  END IF

  OUT &H3C9, r%
  OUT &H3C9, g%
  OUT &H3C9, b%
NEXT i%

radius# = 95

offsetx# = -.2
offsety# = 0

FOR y% = 0 TO 199
  y# = (y% - 99.5) / radius#

  IF ABS(y#) <= 1 THEN
    xr# = SQR(1 - y# * y#) * radius#

    LINE (160 - xr#, y%)-(159 + xr#, y%), 15

    FOR x% = 159.5 - xr# TO 159.5 + xr#
      x# = (x% - 159.5) / radius#

      ' Translation
      numr# = x# - offsetx#
      numi# = y# - offsety#

      denr# = 1 - offsetx# * x# - offsety# * y#
      deni# = offsety# * x# - offsetx# * y#

      ComplexDivide numr#, numi#, denr#, deni#, tx#, ty#

      IF DiskToUpperHalf(tx#, ty#, taur#, taui#) THEN
        KleinJ taur#, taui#, jr#, ji#

        IF jr# > 0 THEN
          d# = jr# * jr# + ji# * ji#
          IF d# > 1D+115 THEN
            c% = 255
          ELSE
            c% = LOG(d#) + 12
            IF c% < 1 THEN c% = 1
            IF c% > 255 THEN c% = 255
          END IF
          PSET (x%, y%), c%
        ELSE
          PSET (x%, y%), 0
        END IF
      END IF
    NEXT x%
  END IF
NEXT y%

SLEEP

SUB ComplexDivide (BYVAL ar#, BYVAL ai#, BYVAL br#, BYVAL bi#, cr#, ci#)
id# = 1# / (br# * br# + bi# * bi#)

cr# = (ar# * br# + ai# * bi#) * id#
ci# = (ai# * br# - ar# * bi#) * id#
END SUB

SUB ComplexMultiply (BYVAL ar#, BYVAL ai#, BYVAL br#, BYVAL bi#, cr#, ci#)
cr# = ar# * br# - ai# * bi#
ci# = ar# * bi# + ai# * br#
END SUB

FUNCTION DiskToUpperHalf% (x#, y#, taur#, taui#)
IF x# * x# + y# * y# < 1 THEN
  d# = (1 - x#) ^ 2 + y# ^ 2

  taur# = -2# * y# / d#
  taui# = (1# - x# * x# - y# * y#) / d#

  DiskToUpperHalf% = -1
END IF
END FUNCTION

SUB KleinJ (BYVAL tr#, BYVAL ti#, jr#, ji#)
ReduceTau tr#, ti#

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

' denominator = E4^3 - E6^2 = A - B

dr# = ar# - br#
di# = ai# - bi#

' J = E4^3 / (E4^3 - E6^2)

ComplexDivide ar#, ai#, dr#, di#, jr#, ji#
END SUB

SUB ReduceTau (tr#, ti#)
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

  ' If we're outside the unit circle, we're done.
  d# = tr# * tr# + ti# * ti#

  IF d# >= 1 THEN EXIT DO

  ' tau = -1 / tau
  tr# = -tr# / d#
  ti# = ti# / d#
LOOP
END SUB

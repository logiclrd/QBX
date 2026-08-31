DECLARE SUB ComplexExp (zr#, zi#, er#, ei#)
DECLARE SUB ComplexDivide (BYVAL ar#, BYVAL ai#, BYVAL br#, BYVAL bi#, cr#, ci#)
DECLARE SUB ComplexMultiply (BYVAL ar#, BYVAL ai#, BYVAL br#, BYVAL bi#, cr#, ci#)

SUB ComplexDivide (BYVAL ar#, BYVAL ai#, BYVAL br#, BYVAL bi#, cr#, ci#)
id# = 1# / (br# * br# + bi# * bi#)

cr# = (ar# * br# + ai# * bi#) * id#
ci# = (ai# * br# - ar# * bi#) * id#
END SUB

SUB ComplexExp (zr#, zi#, er#, ei#)
mag# = EXP(zr#)

er# = mag# * COS(zi#)
ei# = mag# * SIN(zi#)
END SUB

SUB ComplexMultiply (BYVAL ar#, BYVAL ai#, BYVAL br#, BYVAL bi#, cr#, ci#)
cr# = ar# * br# - ai# * bi#
ci# = ar# * bi# + ai# * br#
END SUB

SUB ComplexSin (BYVAL zr#, BYVAL zi#, sr#, si#)
'          e^iz - e^-iz
' sin z = --------------
'               2i

ComplexMultiply 0, 1, zr#, zi#, ar#, ai#
ComplexExp ar#, ai#, ar#, ai#

ComplexMultiply 0, -1, zr#, zi#, br#, bi#
ComplexExp br#, bi#, br#, bi#

numr# = ar# - br#
numi# = ai# - bi#

ComplexDivide numr#, numi#, 0, 2, sr#, si#
END SUB

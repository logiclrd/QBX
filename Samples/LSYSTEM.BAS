DECLARE FUNCTION ConvertToDRAWCommand$ (lsystem$)
DECLARE FUNCTION Iterate$ (s$, rules$())

DIM rules$(127)

rules$(ASC("X")) = "X+YF+"
rules$(ASC("Y")) = "-FX-Y"

axiom$ = "FX"

numlevels% = 10

lsystem$ = axiom$

FOR i% = 1 TO numlevels%
  lsystem$ = Iterate$(lsystem$, rules$())
NEXT i%

drawcommand$ = ConvertToDRAWCommand$(lsystem$)

SCREEN 12

PSET (450, 300)
DRAW drawcommand$

FUNCTION ConvertToDRAWCommand$ (lsystem$)
result$ = "s43"

a% = 0
lasta% = 0

FOR i% = 1 TO LEN(lsystem$)
  SELECT CASE MID$(lsystem$, i%, 1)
    CASE "+": a% = (a% + 1) MOD 4
    CASE "-": a% = (a% + 3) MOD 4
    CASE "F":
      IF a% <> lasta% THEN
        result$ = result$ + "a" + LTRIM$(STR$(a%))
        lasta% = a%
      END IF
      result$ = result$ + "u"
  END SELECT
NEXT i%

ConvertToDRAWCommand$ = result$
END FUNCTION

FUNCTION Iterate$ (s$, rules$())
result$ = ""

FOR i% = 1 TO LEN(s$)
  c% = ASC(MID$(s$, i%, 1))

  IF rules$(c%) <> "" THEN
    result$ = result$ + rules$(c%)
  ELSE
    result$ = result$ + MID$(s$, i%, 1)
  END IF
NEXT i%

Iterate$ = result$
END FUNCTION


DECLARE SUB drawlist ()
DECLARE SUB postfix (subhead%)
DECLARE SUB drawtree (head%, pprefix$)
DECLARE SUB prunt (node AS ANY)
DECLARE SUB maketree (subhead%, eq$)
DECLARE FUNCTION isalpha% (a$)
DECLARE FUNCTION isnumeric% (a$)
TYPE treetype
 left AS INTEGER
 right AS INTEGER
 dat AS STRING * 1
 val AS DOUBLE
 var AS STRING * 32
END TYPE
TYPE linearlisttype
 dat AS STRING * 1
 val AS DOUBLE
 var AS STRING * 32
END TYPE
'$DYNAMIC
CONST MAXTREE% = 500
CONST MAXLIST% = 500
DIM SHARED tree(MAXTREE%) AS treetype, numnodes%
DIM SHARED linearlist(MAXLIST%) AS linearlisttype, numlist%
'maketree 0, "x^5+5*x^4+x"
'maketree 0, "5*x+x*x+2*x+x"
'maketree 0, "(x+5)*(x+3)+5*x"
'maketree 0, "4*sin(5*(x+4))+4"
CLS
PRINT "Expression must be in 'screen expression' format. In other words,"
PRINT "multiplications cannot be implied, and functions must have their parameters"
PRINT "enclosed in brackets."
PRINT
PRINT " eg."
PRINT "    sin(3*n)"
PRINT " instead of"
PRINT "    sin 3n"
PRINT
LINE INPUT "Enter expression: ", a$
x% = INSTR(a$, " ")
DO WHILE x%
 a$ = LEFT$(a$, x% - 1) + MID$(a$, x% + 1)
 x% = INSTR(a$, " ")
LOOP
maketree 0, a$
PRINT
PRINT "Expression tree:"
drawtree 0, ""
PRINT
numlist% = -1 'empty list
postfix 0
PRINT "RPN list:"
drawlist
PRINT

REM $STATIC
SUB drawlist
FOR i% = 0 TO numlist%
 IF i% THEN PRINT ",";
 SELECT CASE linearlist(i%).dat
  CASE "f", "x": t$ = RTRIM$(linearlist(i%).var)
  CASE "n": t$ = LTRIM$(RTRIM$(STR$(linearlist(i%).val)))
  CASE ELSE: t$ = linearlist(i%).dat
 END SELECT
 IF POS(0) + LEN(t$) > 77 THEN PRINT
 COLOR 15
 PRINT t$;
 COLOR 7
NEXT i%
END SUB

SUB drawtree (head%, pprefix$)
prefix$ = pprefix$
IF prefix$ <> "" THEN PRINT prefix$ + "-";  ELSE PRINT " ";
COLOR 15
SELECT CASE tree(head%).dat
 CASE "f", "x": PRINT tree(head%).var
 CASE "n": PRINT LTRIM$(RTRIM$(STR$(tree(head%).val)))
 CASE ELSE: PRINT tree(head%).dat
END SELECT
COLOR 7
x% = INSTR(prefix$, "`")
IF x% THEN MID$(prefix$, x%, 1) = " "
IF tree(head%).right <> 0 THEN
 IF tree(head%).left <> 0 THEN drawtree tree(head%).left, prefix$ + " |"
 drawtree tree(head%).right, prefix$ + " `"
ELSEIF tree(head%).left <> 0 THEN
 drawtree tree(head%).left, prefix$ + " `"
END IF
END SUB

FUNCTION isalpha% (a$)
aa% = VAL(UCASE$(a$))
isalpha% = ((aa% > 64) AND (aa% < 91))
END FUNCTION

FUNCTION isnumeric% (a$)
aa% = ASC(a$)
isnumeric% = (((aa% > 47) AND (aa% < 58)) OR (a% = 46))
END FUNCTION

SUB maketree (subhead%, eq$)
IF (LEFT$(eq$, 1) = "(") AND (RIGHT$(eq$, 1) = ")") THEN
 bracketlevel% = 1
 FOR i% = 2 TO LEN(eq$) - 1
  c$ = MID$(eq$, i%, 1)
  IF c$ = "(" THEN bracketlevel% = bracketlevel% + 1
  IF c$ = ")" THEN bracketlevel% = bracketlevel% - 1
  IF bracketlevel% = 0 THEN EXIT FOR
 NEXT i%
 IF i% = LEN(eq$) THEN eq$ = MID$(eq$, 2, LEN(eq$) - 2)
END IF
found$ = "": foundi% = 0: foundlevel% = 5
FOR i% = LEN(eq$) TO 1 STEP -1
 c$ = MID$(eq$, i%, 1)
 SELECT CASE c$
  CASE ")":
   bracketlevel% = 1
   FOR j% = i% - 1 TO 1 STEP -1
    c$ = MID$(eq$, j%, 1)
    IF c$ = ")" THEN bracketlevel% = bracketlevel% + 1
    IF c$ = "(" THEN bracketlevel% = bracketlevel% - 1
    IF bracketlevel% = 0 THEN EXIT FOR
   NEXT j%
   i% = j%
  CASE "^":
   IF foundlevel% >= 3 THEN
    found$ = "^"
    foundi% = i%
    foundlevel% = 3
   END IF
  CASE "*", "/", "\", "%":
   IF foundlevel% > 2 THEN
    found$ = c$
    foundi% = i%
    foundlevel% = 2
   END IF
  CASE "+", "-":
   IF foundlevel% > 1 THEN
    found$ = c$
    foundi% = i%
    foundlevel% = 1
   END IF
 END SELECT
NEXT i%
IF foundi% = 0 THEN
 c$ = LEFT$(eq$, 1)
 IF (c$ <> "0") AND (VAL(c$) = 0) THEN
  x% = INSTR(eq$, "(")
  IF x% THEN
   tree(subhead%).dat = "f"
   tree(subhead%).var = LEFT$(eq$, x% - 1)
   numnodes% = numnodes% + 1
   tree(subhead%).left = numnodes%
   maketree (numnodes%), MID$(eq$, x%)
  ELSE
   tree(subhead%).dat = "x"
   tree(subhead%).var = eq$
  END IF
 ELSE
  FOR i% = 1 TO LEN(eq$)
   IF isnumeric%(MID$(eq$, i%, 1)) = 0 THEN EXIT FOR
  NEXT i%
  IF i% > LEN(eq$) THEN
   tree(subhead%).dat = "n"
   tree(subhead%).val = VAL(eq$)
  ELSE
   tree(subhead%).dat = "*"
   numnodes% = numnodes% + 1
   tree(subhead%).left = numnodes%
   tree(numnodes%).dat = "n"
   tree(numnodes%).val = VAL(LEFT$(eq$, i% - 1))
   numnodes% = numnodes% + 1
   tree(subhead%).right = numnodes%
   maketree (numnodes%), MID$(eq$, i%)
  END IF
 END IF
ELSE
 lefteq$ = LEFT$(eq$, foundi% - 1)
 righteq$ = MID$(eq$, foundi% + 1)
 tree(subhead%).dat = found$
 numnodes% = numnodes% + 1
 tree(subhead%).left = numnodes%
 maketree (numnodes%), lefteq$
 numnodes% = numnodes% + 1
 tree(subhead%).right = numnodes%
 maketree (numnodes%), righteq$
END IF
END SUB

SUB postfix (subhead%)
IF tree(subhead%).left <> 0 THEN postfix tree(subhead%).left
IF tree(subhead%).right <> 0 THEN postfix tree(subhead%).right
numlist% = numlist% + 1
linearlist(numlist%).dat = tree(subhead%).dat
linearlist(numlist%).val = tree(subhead%).val
linearlist(numlist%).var = tree(subhead%).var
END SUB


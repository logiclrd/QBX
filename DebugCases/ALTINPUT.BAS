LINE INPUT a$

PRINT "Len: "; LEN(a$)
PRINT "'" + a$ + "'"
FOR i% = 1 TO LEN(a$)
  PRINT ASC(MID$(a$, i%, 1));
NEXT i%
PRINT

DEF SEG = &H40

DO
  LOCATE , 1
  PRINT PEEK(&H19);
LOOP WHILE INKEY$ <> CHR$(27)


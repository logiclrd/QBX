DIM ks%(127)

CLS
PRINT " 0"
PRINT "64"
PRINT "   |'''''''|'''''''|'''''''|'''''''|'''''''|'''''''|'''''''|'''''''"
PRINT "   0,64    8,72    16,80   24,88   32,96   40,104  48,112  56,120"

DO
  a% = INP(&H60)

  ks%(a% AND 127) = ((a% AND 128) = 0)

  LOCATE 1, 1

  FOR y% = 0 TO 64 STEP 64
    PRINT TAB(4);
    FOR x% = 0 TO 63
      IF ks%(x% + y%) THEN PRINT "O";  ELSE PRINT "-";
    NEXT x%
    PRINT
  NEXT y%
LOOP


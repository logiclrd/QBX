DIM ks%(127)
DO
  a% = INP(&H60)

  ks%(a% AND 127) = ((a% AND 128) = 0)

  LOCATE 1, 1

  FOR y% = 0 TO 64 STEP 64
    FOR x% = 0 TO 63
      IF ks%(x% + y%) THEN PRINT "O";  ELSE PRINT "-";
    NEXT x%
    PRINT
  NEXT y%
LOOP


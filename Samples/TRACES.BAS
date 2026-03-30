DIM c$(2)

c$(0) = " "
c$(1) = CHR$(220)
c$(2) = CHR$(223)

FOR y% = 1 TO 24 STEP 3
  FOR x% = 1 TO 78 STEP 6
    IF RND < .5 THEN RESTORE down ELSE RESTORE up
    FOR q% = 0 TO 2
      LOCATE y% + q%, x%
      FOR r% = 1 TO 6
        READ n%
        PRINT c$(n%);
      NEXT r%
    NEXT q%
  NEXT x%
NEXT y%

down:
  DATA 2,1,0,2,1,0
  DATA 1,0,2,1,0,2
  DATA 0,2,1,0,2,1
up:
  DATA 0,1,2,0,1,2
  DATA 2,0,1,2,0,1
  DATA 1,2,0,1,2,0

DECLARE SUB Test (CharSet() AS INTEGER)

DIM SHARED CharSet(1, 2) AS INTEGER
DIM OtherCS(1, 2) AS INTEGER

CharSet(1, 1) = 42
OtherCS(1, 1) = 69

Test OtherCS()

SUB Test (CharSet%())
IF CharSet(1, 1) = 42 AND CharSet%(1, 1) = 69 THEN
  ' DIM SHARED CharSet() AS INTEGER from main module seen here
  ' CharSet%() parameter did not occlude it, but is also accessible
  OPEN "TESTOUT$" FOR OUTPUT AS #1
  PRINT #1, "PASS"
  CLOSE #1
END IF
END SUB

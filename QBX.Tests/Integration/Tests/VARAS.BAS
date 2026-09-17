DECLARE SUB Test (CharSet AS INTEGER)

DIM SHARED CharSet AS INTEGER

CharSet = 42
OtherCS% = 69

Test OtherCS%

SUB Test (CharSet%)
IF CharSet = 42 AND CharSet% = 69 THEN
  ' DIM SHARED CharSet AS INTEGER from main module seen here
  ' CharSet% parameter did not occlude it, but is also accessible
  OPEN "TESTOUT$" FOR OUTPUT AS #1
  PRINT #1, "PASS"
  CLOSE #1
END IF
END SUB

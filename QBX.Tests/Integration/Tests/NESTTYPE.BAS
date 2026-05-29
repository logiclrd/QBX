TYPE testing2
  x AS INTEGER
  y AS INTEGER
END TYPE

TYPE testing
  a(3 TO 5) AS testing2
  b(2 TO 4) AS STRING * 3
END TYPE

DIM r AS testing

FOR i% = 3 TO 5
  r.a(i%).y = i% * 4
NEXT i%

r.b(2) = "testing"
r.b(3) = "banana"
r.b(4) = "is"

OPEN "TESTOUT$" FOR OUTPUT AS #1

IF r.a(3).y <> 12 THEN PRINT #1, "FAIL"
IF r.a(4).y <> 16 THEN PRINT #1, "FAIL"
IF r.a(5).y <> 20 THEN PRINT #1, "FAIL"

IF r.b(2) <> "tes" THEN PRINT #1, "FAIL"
IF r.b(3) <> "ban" THEN PRINT #1, "FAIL"
IF r.b(4) <> "is " THEN PRINT #1, "FAIL"

PRINT #1, "PASS" ' if no FAILs

CLOSE #1

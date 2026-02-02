TYPE testing2
  x AS INTEGER
  y AS INTEGER
END TYPE

TYPE testing
  a(3 TO 5) AS testing2
  b(2 TO 4) AS STRING * 3
END TYPE

DIM r AS testing

PRINT r.a(3).y

r.b(2) = "testing"

PRINT r.b(2)


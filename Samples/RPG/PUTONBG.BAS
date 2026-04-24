DIM spt%(8, 10, 10)
OPEN "game.spt" FOR BINARY AS #1
SEEK #1, 3101
FOR i% = 1 TO 8
 FOR j% = 1 TO 10
  FOR k% = 1 TO 10
   spt%(i%, k%, j%) = ASC(INPUT$(1, 1))
  NEXT k%
 NEXT j%
NEXT i%
SEEK #1, ((LOF(1) - 50) \ 100) * 100 + 1
FOR i% = 1 TO 10
 FOR j% = 1 TO 10
  spt%(0, j%, i%) = ASC(INPUT$(1, 1))
 NEXT j%
NEXT i%
CLOSE #1
FOR i% = 1 TO 8
 FOR j% = 1 TO 10
  FOR k% = 1 TO 10
   IF spt%(i%, j%, k%) = 0 THEN spt%(i%, j%, k%) = spt%(0, j%, k%)
  NEXT k%
 NEXT j%
NEXT i%
OPEN "game.spt" FOR APPEND AS #1
FOR i% = 1 TO 8
 FOR j% = 1 TO 10
  FOR k% = 1 TO 10
   PRINT #1, CHR$(spt%(i%, k%, j%));
  NEXT k%
 NEXT j%
NEXT i%
CLOSE #1


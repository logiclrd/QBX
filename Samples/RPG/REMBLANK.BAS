DIM spt%(10, 10)
OPEN "gam.spt" FOR BINARY AS #1
OPEN "gamm.spt" FOR OUTPUT AS #2
FOR i% = 1 TO LOF(1) \ 100
 d% = 0
 FOR j% = 1 TO 10
  FOR k% = 1 TO 10
   spt%(k%, j%) = ASC(INPUT$(1, 1))
   d% = d% + spt%(k%, j%)
  NEXT k%
 NEXT j%
 IF d% <> 0 THEN
  FOR j% = 1 TO 10
   FOR k% = 1 TO 10
    PRINT #2, CHR$(spt%(k%, j%));
   NEXT k%
  NEXT j%
 END IF
NEXT i%
CLOSE #1, #2


DECLARE SUB addval (b%)
DECLARE SUB out9bit (v%)
DIM patterns$(2500), outbuff%(128)
ON TIMER(1) GOSUB drawstats
TIMER ON
CLS
oldy% = 1: oldx% = 1
OPEN "game.map" FOR BINARY AS #1
OPEN "tmp.$$$" FOR OUTPUT AS #2
OPEN "game.lzw" FOR OUTPUT AS #3
DO
 a$ = INPUT$(1, 1)
 inbits& = inbits& + 8
 b$ = b$ + a$
 FOR i% = 1 TO numpatterns%
  IF patterns$(i%) = b$ THEN EXIT FOR
 NEXT i%
 IF i% > numpatterns% THEN
  IF i% = 512 THEN
   FOR i% = 1 TO numpatterns%
    IF MID$(patterns$(i%), 1, 4) = MID$(b$, 1, 4) THEN EXIT FOR
   NEXT i%
   IF i% > numpatterns% THEN
    FOR i% = 1 TO numpatterns%
     IF MID$(patterns$(i%), 1, 3) = MID$(b$, 1, 3) THEN EXIT FOR
    NEXT i%
    IF i% > numpatterns% THEN
     FOR i% = 1 TO numpatterns%
      IF MID$(patterns$(i%), 1, 2) = MID$(b$, 1, 2) THEN EXIT FOR
     NEXT i%
     IF i% > numpatterns% THEN
      FOR i% = 1 TO numpatterns%
       IF MID$(patterns$(i%), 1, 1) = MID$(b$, 1, 1) THEN EXIT FOR
      NEXT i%
      IF i% > numpatterns% THEN BEEP: PRINT "Program error": END
      match% = i%
      GOTO normalmatch
     ELSE
      match% = i%
      GOTO normalmatch
     END IF
    ELSE
     match% = i%
     GOTO normalmatch
    END IF
   ELSE
    match% = i%
    GOTO normalmatch
   END IF
  END IF
  IF LEN(b$) < 5 THEN numpatterns% = i%: patterns$(i%) = b$ ELSE b$ = RIGHT$(b$, 1)
  FOR i% = 1 TO numpatterns% - 1
   IF patterns$(i%) = patterns$(numpatterns%) THEN PRINT "Bug": END
  NEXT i%
  outbits& = outbits& + 9
 ELSE
normalmatch:
  match% = i%
  IF LEN(b$) = 4 THEN
   b$ = ""
  ' VIEW PRINT 1 TO 4
  ' LOCATE oldy%, oldx%, 1, 14, 15
  ' IF POS(0) > 70 THEN PRINT match% ELSE PRINT match%;
  ' oldy% = CSRLIN: oldx% = POS(0)
  ' VIEW PRINT
   outbits& = outbits& + 9
   out9bit match%
  END IF
 END IF
LOOP UNTIL LOC(1) = LOF(1)
FOR i% = 1 TO numpatterns%
 PRINT #3, CHR$(LEN(patterns$(i%))) + patterns$(i%);
NEXT i%
PRINT #3, CHR$(0);
CLOSE #2: OPEN "tmp.$$$" FOR BINARY AS #2
transbuff$ = SPACE$(8192)
DO
 IF (LOF(2) - LOC(2) < 8192) THEN
  transbuff$ = ""
  transbuff$ = SPACE$(LOF(2) - LOC(2))
 END IF
 GET #2, , transbuff$
 PRINT #3, transbuff$;
LOOP UNTIL LOF(2) = LOC(2)
CLOSE #2
KILL "tmp.$$$"
CLOSE #1, #3
CLS
END

drawstats:
LOCATE 6, 1
PRINT "Bits in: "; inbits&
PRINT "Bits out: "; outbits&
inbytes& = inbits& \ 8 + ABS((inbits& MOD 8) <> 0)
PRINT "Bytes in: "; inbytes&
outbytes& = outbits& \ 8 + ABS((outbits& MOD 8) <> 0)
PRINT "Bytes out: "; outbytes&
PRINT "Patterns: "; numpatterns%
PRINT "Overall compression: "; outbytes& * 100 \ inbytes&
PRINT "Percent done: "; LOC(1) * 100& \ LOF(1)
RETURN

SUB addval (b%)
SHARED outbuff%(), outindex%
outindex% = outindex% + 1
outbuff%(outindex%) = b%
END SUB

SUB out9bit (v%)
SHARED outbuff%(), outindex%
FOR i% = 1 TO 9
 addval ABS((v% AND (2 ^ (9 - i%))) <> 0)
NEXT i%
IF outindex% > 119 THEN
 out$ = ""
 FOR i% = 1 TO (outindex% \ 8) * 8
  outv% = 0
  FOR j% = i% TO i% + 7
   outv% = outv% + outbuff%(j%) * (2 ^ ((i% + 7) - j%))
  NEXT j%
  out$ = out$ + CHR$(outv%)
 NEXT i%
 PRINT #2, out$;
 FOR i% = 1 TO (outindex% MOD 8)
  outbuff%(i%) = outbuff%(i% + ((outindex% \ 8) * 8) - 1)
 NEXT i%
 outindex% = outindex% MOD 8
END IF
END SUB


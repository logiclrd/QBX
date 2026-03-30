' Translated from Oneliner64 -- not one line
' any more, but still pretty compact :-)

WIDTH 40, 25: DEF SEG = &HB800: CLS

1 PRINT TAB(RND * 39); "*": k = VAL("0" + INKEY$): GOSUB 3
p = p + (k = 1) - (k = 9): POKE 2 * (260 + p), 86: GOSUB 3
ON (2 + (p < 420)) AND (PEEK(600 + 2 * p) <> 42) GOTO 1, 2
2 IF p < 420 THEN PRINT "DIED": END ELSE PRINT "WIN!": END
3 st = TIMER: WHILE TIMER = st: WEND: RETURN

DECLARE SUB putall ()
DECLARE FUNCTION readfile% (a$)
TYPE paltype
 r AS INTEGER
 g AS INTEGER
 b AS INTEGER
END TYPE
COMMON SHARED pal() AS paltype

SUB circl (x%, y%, r!, a!, c%)
thisx% = x%
thisy% = y% + (r! * a!)
PSET (thisx%, thisy%), c%
FOR i! = 0 TO 6.2831 STEP .05
 thisx% = x% + (SIN(i!) * r!)
 thisy% = y% + (COS(i!) * r! * a!)
 LINE -(thisx%, thisy%), c%
NEXT i!
thisx% = x%
thisy% = y% + (r! * a!)
LINE -(thisx%, thisy%), c%
END SUB

SUB drawcolorframe
 LINE (0, 0)-(319, 7), 0, BF
 LINE (0, 160)-(319, 199), 0, BF
 LINE (0, 160)-(319, 199), 15, B
 FOR i% = 20 TO 299 STEP 40
  LINE (i%, 160)-(i% + 40, 199), 15, B
  LINE (i%, 160)-(i% + 40, 180), 15, B
 NEXT i%
 PAINT (10, 170), 7, 15
 PAINT (310, 170), 7, 15
 PSET (8, 180), 15
 DRAW "NE5NF5"
 PSET (310, 180), 15
 DRAW "NG5NH5"
END SUB

SUB putfontxy (x%, y%, a$, c%, f$)
curchr% = 0
IF LEN(f$) > 2048 THEN bwfont% = 0 ELSE bwfont% = -1
FOR i% = x% TO x% + LEN(a$) * 8 STEP 8
 curchr% = curchr% + 1
 IF curchr% > LEN(a$) THEN EXIT FOR
 FOR j% = y% TO y% + 7
  IF bwfont% THEN
   b% = ASC(MID$(f$, ASC(MID$(a$, curchr%, 1)) * 8 + (j% - y%) + 1))
   FOR k% = i% TO i% + 7
    m% = k% - i%
    IF (b% AND (2 ^ (7 - m%))) <> 0 THEN PSET (k%, j%), c%
   NEXT k%
  ELSE
   FOR k% = i% TO i% + 7
    b% = ASC(MID$(f$, ASC(MID$(a$, curchr%, 1)) * 64 + (j% - y%) * 8 + (k% - i%), 1))
    IF b% <> 0 THEN PSET (k%, j%), b%
   NEXT k%
  END IF
 NEXT j%
NEXT i%
END SUB

SUB puttextxy (x%, y%, a$, c%)
DEF SEG = &HF000
curchr% = 0
FOR i% = x% TO x% + LEN(a$) * 8 - 1 STEP 8
 curchr% = curchr% + 1
 IF curchr% > LEN(a$) THEN EXIT FOR
 FOR j% = y% TO y% + 7
  b% = PEEK(&HFA6E + ASC(MID$(a$, curchr%, 1)) * 8 + (j% - y%))
  FOR k% = i% TO i% + 7
   m% = 7 - (k% - i%)
   IF (b% AND (2 ^ m%)) <> 0 THEN PSET (k% + curchr%, j%), c%
  NEXT k%
 NEXT j%
NEXT i%
DEF SEG
END SUB

SUB quit
SCREEN 0: WIDTH 80, 25
COLOR , 0
CLS
COLOR 2: PRINT "‹‹‹‹‹‹‹‹‹‹‹‹‹": COLOR , 1
PRINT "€€€€€ﬂ ﬂ€€€€€"; : COLOR 9, 0: PRINT " ‹ ‹‹  ‹‹   ‹‹   ‹‹‹  ‹‹   ‹ﬂﬂ  €": COLOR 2, 1
PRINT "€€€ﬂ     ﬂ€€€"; : COLOR 9, 0: PRINT " €ﬂ   €  ﬂ €  € €  ﬂ €  € ‹€‹  ‹€‹": COLOR 2, 1
PRINT "€ﬂ         ﬂ€"; : COLOR 9, 0: PRINT " €    €    €  € ﬂﬂﬂ€ €  €  €    €": COLOR 2
PRINT "ﬂﬂﬂﬂﬂﬂﬂﬂﬂﬂﬂﬂﬂ"; : COLOR 9, 0: PRINT " ﬂ     ﬂﬂﬂ  ﬂﬂ  ﬂﬂﬂ   ﬂﬂ   €     ﬂﬂ"
                        PRINT "                                      ﬂﬂ": COLOR 12
PRINT "€ﬂﬂﬂ€       ‹ﬂﬂ‹  €                        "; : COLOR 13: PRINT "   €€ €ﬁ €€ €€ €ﬂ  €ﬂ€ﬂ€ €€": COLOR 12
PRINT "€‹‹‹‹ ‹‹‹‹ ‹€‹   ‹€‹   ‹ ‹ ‹ ‹‹‹  ‹ ‹‹  ‹‹‹"; : COLOR 13: PRINT "   ›  €€ ›  ﬁ› €‹  › ≥ ﬁ ›": COLOR 12
PRINT "‹   € €  €  €     €  ‹ € € € ‹‹€  €ﬂ  ﬂ €ﬂﬂ"; : COLOR 13: PRINT "   ﬂ›  ﬁ ﬂ› ﬁ› €   › ≥ ﬁ ﬂ›": COLOR 12
PRINT "ﬂﬂﬂﬂﬂ ﬂﬂﬂﬂ  ﬂ      ﬂﬂ   ﬂ ﬂ  ﬂﬂﬂﬂ ﬂ     ﬂﬂﬂ"; : COLOR 13: PRINT "   ﬂ   ﬂ ﬂ  ﬂ  ﬂﬂﬂ ﬂ ﬂ ﬂ ﬂ": COLOR 7
SYSTEM
END SUB


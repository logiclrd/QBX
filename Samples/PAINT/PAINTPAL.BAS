DECLARE SUB tomemory (mymem&)
DECLARE SUB frommemory (mymem&)
TYPE paltype
 r AS INTEGER
 g AS INTEGER
 B AS INTEGER
END TYPE
DECLARE SUB downval (x%, y%, v%, display%)
DECLARE SUB getpal (pal() AS paltype)
DECLARE SUB getrgb (c%, r%, g%, B%)
DECLARE SUB localmouseint ()
DECLARE SUB paledit ()
DECLARE SUB puttextxy (x%, y%, a$, c%)
DECLARE SUB setrgb (c%, r%, g%, B%)
DECLARE SUB upval (x%, y%, v%, display%)
TYPE regtype
 ax AS INTEGER
 bx AS INTEGER
 cx AS INTEGER
 dx AS INTEGER
 bp AS INTEGER
 si AS INTEGER
 di AS INTEGER
 flags AS INTEGER
 ds AS INTEGER
 es AS INTEGER
END TYPE
DIM SHARED regs AS regtype

SUB downval (x%, y%, v%, display%)
IF display% THEN
 v$ = LTRIM$(RTRIM$(STR$(v%))): IF LEN(v$) = 1 THEN v$ = "0" + v$
 puttextxy x%, y%, v$, 0
END IF
v% = v% - 1
IF v% < 0 THEN v% = 0
IF display% THEN
 v$ = LTRIM$(RTRIM$(STR$(v%))): IF LEN(v$) = 1 THEN v$ = "0" + v$
 puttextxy x%, y%, v$, 15
END IF
END SUB

SUB getpal (pal() AS paltype)
FOR i% = 0 TO 255
 getrgb i%, pal(i%).r, pal(i%).g, pal(i%).B
NEXT i%
END SUB

SUB getrgb (c%, r%, g%, B%)
OUT &H3C7, c%
r% = INP(&H3C9)
g% = INP(&H3C9)
B% = INP(&H3C9)
END SUB

SUB localmouseint
CALL interrupt(&H33, regs, regs)
END SUB

SUB paledit
DO: regs.ax = 3: localmouseint: LOOP UNTIL regs.bx = 0
REDIM pal(255) AS paltype, scr&(16000)
getpal pal()
regs.ax = 7: regs.cx = 0: regs.dx = 383: localmouseint
regs.ax = 8: regs.cx = 0: regs.dx = 39: localmouseint
regs.ax = 2: localmouseint
GET (0, 0)-(319, 199), scr&
DO
 regs.ax = 3
 localmouseint
 basex% = regs.cx \ 2
 basey% = regs.dx
 LINE (basex%, basey%)-(basex% + 128, basey% + 160), 15, B, &H5555
 DO
  regs.ax = 3
  localmouseint
 LOOP UNTIL (basex% <> regs.cx \ 2) OR (basey% <> regs.dx) OR (regs.bx <> 0)
 PUT (0, 0), scr&, PSET
LOOP UNTIL regs.bx <> 0
regs.ax = 7: regs.cx = 0: regs.dx = 639: localmouseint
regs.ax = 8: regs.cx = 0: regs.dx = 199: localmouseint
regs.ax = 1: localmouseint
IF (regs.bx AND 2) = 2 THEN EXIT SUB
LINE (basex%, basey%)-(basex% + 128, basey% + 160), 0, BF
regs.ax = 2: localmouseint
FOR i% = 0 TO 128 STEP 8
 LINE (basex% + i%, basey% + 32)-(basex% + i%, basey% + 160), 15
 LINE (basex%, basey% + i% + 32)-(basex% + 128, basey% + i% + 32), 15
NEXT i%
LINE (basex%, basey%)-(basex% + 128, basey%), 15
LINE (basex%, basey%)-(basex%, basey% + 32), 15
LINE (basex% + 64, basey%)-(basex% + 64, basey% + 32), 15
LINE (basex% + 128, basey%)-(basex% + 128, basey% + 32), 15
puttextxy basex% + 8, basey% + 5, "R: 00", 15
puttextxy basex% + 72, basey% + 5, "R: 00", 15
puttextxy basex% + 8, basey% + 13, "G: 00", 7
puttextxy basex% + 72, basey% + 13, "G: 00", 7
puttextxy basex% + 8, basey% + 21, "B: 00", 7
puttextxy basex% + 72, basey% + 21, "B: 00", 7
LINE (basex% + 4, basey% + 3)-(basex% + 60, basey% + 29), 15, B
FOR i% = 0 TO 15
 FOR j% = 0 TO 15
  PAINT (basex% + i% * 8 + 4, basey% + j% * 8 + 36), i% + j% * 16, 15
 NEXT j%
NEXT i%
regs.ax = 1: localmouseint
x% = basex% + 4
y% = basey% + 36
regs.ax = 4
regs.cx = basex% + 4
regs.dx = basey% + 36
localmouseint
selector1% = 1
selector2% = 1
selector% = 1
register% = 1
display% = 1
setrgb 7, 42, 42, 42
setrgb 15, 63, 63, 63
DO
 IF display% THEN
  regs.ax = 2
  localmouseint
  selection% = POINT(x%, y%)
  SELECT CASE selector%
   CASE 1: selector1% = selection%
   CASE 2: selector2% = selection%
  END SELECT
  regs.ax = 1
  localmouseint
  r1% = pal(selector1%).r: g1% = pal(selector1%).g: b1% = pal(selector1%).B
  IF (r1% <> or1%) OR (g1% <> og1%) OR (b1% <> ob1%) THEN
   or1% = r1%: og1% = g1%: ob1% = b1%
   r$ = LTRIM$(RTRIM$(STR$(r1%))): IF LEN(r$) = 1 THEN r$ = "0" + r$
   g$ = LTRIM$(RTRIM$(STR$(g1%))): IF LEN(g$) = 1 THEN g$ = "0" + g$
   B$ = LTRIM$(RTRIM$(STR$(b1%))): IF LEN(B$) = 1 THEN B$ = "0" + B$
   LINE (basex% + 32, basey% + 5)-(basex% + 59, basey% + 28), 0, BF
   puttextxy basex% + 32, basey% + 5, r$, 15
   puttextxy basex% + 32, basey% + 13, g$, 15
   puttextxy basex% + 32, basey% + 21, B$, 15
  END IF
  r2% = pal(selector2%).r: g2% = pal(selector2%).g: b2% = pal(selector2%).B
  IF (r2% <> or2%) OR (g2% <> og2%) OR (b2% <> ob2%) THEN
   or2% = r2%: og2% = g2%: ob2% = b2%
   r$ = LTRIM$(RTRIM$(STR$(r2%))): IF LEN(r$) = 1 THEN r$ = "0" + r$
   g$ = LTRIM$(RTRIM$(STR$(g2%))): IF LEN(g$) = 1 THEN g$ = "0" + g$
   B$ = LTRIM$(RTRIM$(STR$(b2%))): IF LEN(B$) = 1 THEN B$ = "0" + B$
   LINE (basex% + 96, basey% + 5)-(basex% + 123, basey% + 28), 0, BF
   puttextxy basex% + 96, basey% + 5, r$, 15
   puttextxy basex% + 96, basey% + 13, g$, 15
   puttextxy basex% + 96, basey% + 21, B$, 15
  END IF
  SELECT CASE register%
   CASE 1:
    puttextxy basex% + 8, basey% + 5, "R:", 15
    puttextxy basex% + 72, basey% + 5, "R:", 15
    puttextxy basex% + 8, basey% + 13, "G:", 7
    puttextxy basex% + 72, basey% + 13, "G:", 7
    puttextxy basex% + 8, basey% + 21, "B:", 7
    puttextxy basex% + 72, basey% + 21, "B:", 7
   CASE 2:
    puttextxy basex% + 8, basey% + 5, "R:", 7
    puttextxy basex% + 72, basey% + 5, "R:", 7
    puttextxy basex% + 8, basey% + 13, "G:", 15
    puttextxy basex% + 72, basey% + 13, "G:", 15
    puttextxy basex% + 8, basey% + 21, "B:", 7
    puttextxy basex% + 72, basey% + 21, "B:", 7
   CASE 3:
    puttextxy basex% + 8, basey% + 5, "R:", 7
    puttextxy basex% + 72, basey% + 5, "R:", 7
    puttextxy basex% + 8, basey% + 13, "G:", 7
    puttextxy basex% + 72, basey% + 13, "G:", 7
    puttextxy basex% + 8, basey% + 21, "B:", 15
    puttextxy basex% + 72, basey% + 21, "B:", 15
  END SELECT
  regs.ax = 1
  localmouseint
 END IF
continue:
 ox% = x%
 oy% = y%
 DO
  a$ = INKEY$
  regs.ax = 3
  localmouseint
  x% = regs.cx \ 2
  y% = regs.dx
 LOOP UNTIL (a$ <> "") OR (regs.bx <> 0) OR (ox% <> x%) OR (oy% <> y%)
 SELECT CASE a$
  CASE CHR$(0) + "K":
   x% = x% - 5: newpos% = -1: IF x% < 0 THEN x% = 0
  CASE CHR$(0) + "M":
   x% = x% + 5: newpos% = -1: IF x% > 319 THEN x% = 319
  CASE CHR$(0) + "H":
   y% = y% - 5: newpos% = -1: IF y% < 0 THEN y% = 0
  CASE CHR$(0) + "P":
   y% = y% + 5: newpos% = -1: IF y% > 199 THEN y% = 199
  CASE CHR$(9), CHR$(0) + CHR$(15):
   display% = (display% + 1) MOD 2
   regs.ax = 2
   localmouseint
   SELECT CASE display%
    CASE 0:
     PUT (0, 0), scr&, PSET
     setrgb 7, pal(7).r, pal(7).g, pal(7).B
     setrgb 15, pal(15).r, pal(15).g, pal(15).B
    CASE 1:
     FOR i% = 0 TO 128 STEP 8
      LINE (basex% + i%, basey% + 32)-(basex% + i%, basey% + 160), 15
      LINE (basex%, basey% + i% + 32)-(basex% + 128, basey% + i% + 32), 15
     NEXT i%
     LINE (basex%, basey%)-(basex% + 128, basey%), 15
     LINE (basex%, basey%)-(basex%, basey% + 32), 15
     LINE (basex% + 64, basey%)-(basex% + 64, basey% + 32), 15
     LINE (basex% + 128, basey%)-(basex% + 128, basey% + 32), 15
     puttextxy basex% + 8, basey% + 5, "R:", 15
     puttextxy basex% + 72, basey% + 5, "R:", 15
     puttextxy basex% + 8, basey% + 13, "G:", 7
     puttextxy basex% + 72, basey% + 13, "G:", 7
     puttextxy basex% + 8, basey% + 21, "B:", 7
     puttextxy basex% + 72, basey% + 21, "B:", 7
     LINE (basex% + (selector% - 1) * 64 + 4, basey% + 3)-(basex% + (selector% - 1) * 64 + 60, basey% + 29), 15, B
     FOR i% = 0 TO 15
      FOR j% = 0 TO 15
       PAINT (basex% + i% * 8 + 4, basey% + j% * 8 + 36), i% + j% * 16, 15
      NEXT j%
     NEXT i%
   END SELECT
   regs.ax = 1
   localmouseint
  CASE " ":
   IF display% THEN LINE (basex% + (selector% - 1) * 64 + 4, basey% + 3)-(basex% + (selector% - 1) * 64 + 60, basey% + 29), 0, B
   selector% = (selector% MOD 2) + 1
   IF display% THEN LINE (basex% + (selector% - 1) * 64 + 4, basey% + 3)-(basex% + (selector% - 1) * 64 + 60, basey% + 29), 15, B
  CASE "r", "R":
   register% = 1
  CASE "g", "G":
   register% = 2
  CASE "b", "B":
   register% = 3
  CASE "+":
   SELECT CASE selector%
    CASE 1:
     SELECT CASE register%
      CASE 1: upval basex% + 32, basey% + 5, pal(selector1%).r, display%
      CASE 2: upval basex% + 32, basey% + 13, pal(selector1%).g, display%
      CASE 3: upval basex% + 32, basey% + 21, pal(selector1%).B, display%
     END SELECT
     IF INSTR(" 7 15 ", STR$(selector1%) + " ") = 0 THEN setrgb selector1%, pal(selector1%).r, pal(selector1%).g, pal(selector1%).B
    CASE 2:
     SELECT CASE register%
      CASE 1: upval basex% + 96, basey% + 5, pal(selector2%).r, display%
      CASE 2: upval basex% + 96, basey% + 13, pal(selector2%).g, display%
      CASE 3: upval basex% + 96, basey% + 21, pal(selector2%).B, display%
     END SELECT
     IF INSTR(" 7 15 ", STR$(selector2%) + " ") = 0 THEN setrgb selector2%, pal(selector2%).r, pal(selector2%).g, pal(selector2%).B
   END SELECT
   GOTO continue
  CASE "-":
   SELECT CASE selector%
    CASE 1:
     SELECT CASE register%
      CASE 1: downval basex% + 32, basey% + 5, pal(selector1%).r, display%
      CASE 2: downval basex% + 32, basey% + 13, pal(selector1%).g, display%
      CASE 3: downval basex% + 32, basey% + 21, pal(selector1%).B, display%
     END SELECT
     IF INSTR(" 7 15 ", STR$(selector1%) + " ") = 0 THEN setrgb selector1%, pal(selector1%).r, pal(selector1%).g, pal(selector1%).B
    CASE 2:
     SELECT CASE register%
      CASE 1: downval basex% + 96, basey% + 5, pal(selector2%).r, display%
      CASE 2: downval basex% + 96, basey% + 13, pal(selector2%).g, display%
      CASE 3: downval basex% + 96, basey% + 21, pal(selector2%).B, display%
     END SELECT
     IF INSTR(" 7 15 ", STR$(selector2%) + " ") = 0 THEN setrgb selector2%, pal(selector2%).r, pal(selector2%).g, pal(selector2%).B
   END SELECT
   GOTO continue
  CASE ",", "<":
   tr% = pal(0).r
   tg% = pal(0).g
   tb% = pal(0).B
   FOR i% = 0 TO 254
    pal(i%).r = pal(i% + 1).r
    pal(i%).g = pal(i% + 1).g
    pal(i%).B = pal(i% + 1).B
   NEXT i%
   pal(255).r = tr%
   pal(255).g = tg%
   pal(255).B = tb%
   newpal% = -1
  CASE ".", ">":
   tr% = pal(255).r
   tg% = pal(255).g
   tb% = pal(255).B
   FOR i% = 254 TO 0 STEP -1
    pal(i% + 1).r = pal(i%).r
    pal(i% + 1).g = pal(i%).g
    pal(i% + 1).B = pal(i%).B
   NEXT i%
   pal(0).r = tr%
   pal(0).g = tg%
   pal(0).B = tb%
   newpal% = -1
  CASE CHR$(27): EXIT DO
  CASE "=":
   IF selector1% <> selector2% THEN
    IF selector1% > selector2% THEN SWAP selector1%, selector2%: swapped% = -1
    addr% = pal(selector1%).r
    addg% = pal(selector1%).g
    addb% = pal(selector1%).B
    difr% = (pal(selector2%).r - pal(selector1%).r)
    difg% = (pal(selector2%).g - pal(selector1%).g)
    difb% = (pal(selector2%).B - pal(selector1%).B)
    difs% = selector2% - selector1%
    FOR i% = selector1% TO selector2%
     difi% = i% - selector1%
     pal(i%).r = difr% * (difi% / difs%) + addr%
     pal(i%).g = difg% * (difi% / difs%) + addg%
     pal(i%).B = difb% * (difi% / difs%) + addb%
    NEXT i%
    IF swapped% THEN SWAP selector1%, selector2%: swapped% = 0
    newpal% = -1
   END IF
  CASE "d", "D":
   SELECT CASE selector%
    CASE 1:
     pal(selector1%).r = pal(selector2%).r
     pal(selector1%).g = pal(selector2%).g
     pal(selector1%).B = pal(selector2%).B
     IF INSTR(" 7 15 ", STR$(selector1%) + " ") = 0 THEN setrgb selector1%, pal(selector1%).r, pal(selector1%).g, pal(selector1%).B
    CASE 2:
     pal(selector2%).r = pal(selector1%).r
     pal(selector2%).g = pal(selector1%).g
     pal(selector2%).B = pal(selector1%).B
     IF INSTR(" 7 15 ", STR$(selector2%) + " ") = 0 THEN setrgb selector2%, pal(selector2%).r, pal(selector2%).g, pal(selector2%).B
   END SELECT
 END SELECT
 IF regs.bx THEN
  IF (regs.bx AND 1) = 1 THEN
   IF display% THEN LINE (basex% + (selector% - 1) * 64 + 4, basey% + 3)-(basex% + (selector% - 1) * 64 + 60, basey% + 29), 0, B
   selector% = (selector% MOD 2) + 1
   IF display% THEN LINE (basex% + (selector% - 1) * 64 + 4, basey% + 3)-(basex% + (selector% - 1) * 64 + 60, basey% + 29), 15, B
  END IF
  IF (regs.bx AND 2) = 2 THEN
   display% = (display% + 1) MOD 2
   regs.ax = 2
   localmouseint
   SELECT CASE display%
    CASE 0:
     PUT (0, 0), scr&, PSET
     setrgb 7, pal(7).r, pal(7).g, pal(7).B
     setrgb 15, pal(15).r, pal(15).g, pal(15).B
    CASE 1:
     FOR i% = 0 TO 128 STEP 8
      LINE (basex% + i%, basey% + 32)-(basex% + i%, basey% + 160), 15
      LINE (basex%, basey% + i% + 32)-(basex% + 128, basey% + i% + 32), 15
     NEXT i%
     LINE (basex%, basey%)-(basex% + 128, basey%), 15
     LINE (basex%, basey%)-(basex%, basey% + 32), 15
     LINE (basex% + 64, basey%)-(basex% + 64, basey% + 32), 15
     LINE (basex% + 128, basey%)-(basex% + 128, basey% + 32), 15
     puttextxy basex% + 8, basey% + 5, "R:", 15
     puttextxy basex% + 72, basey% + 5, "R:", 15
     puttextxy basex% + 8, basey% + 13, "G:", 7
     puttextxy basex% + 72, basey% + 13, "G:", 7
     puttextxy basex% + 8, basey% + 21, "B:", 7
     puttextxy basex% + 72, basey% + 21, "B:", 7
     LINE (basex% + (selector% - 1) * 64 + 4, basey% + 3)-(basex% + (selector% - 1) * 64 + 60, basey% + 29), 15, B
     FOR i% = 0 TO 15
      FOR j% = 0 TO 15
       PAINT (basex% + i% * 8 + 4, basey% + j% * 8 + 36), i% + j% * 16, 15
      NEXT j%
     NEXT i%
   END SELECT
   regs.ax = 1
   localmouseint
  END IF
  DO
   regs.ax = 3
   localmouseint
  LOOP UNTIL regs.bx = 0
 END IF
 IF newpos% THEN
  newpos% = 0
  regs.ax = 4
  regs.cx = x%
  regs.dx = y%
  localmouseint
 END IF
 IF newpal% THEN
  newpal% = 0
  FOR i% = 0 TO 255
   IF (i% <> 15) AND (i% <> 7) THEN
    IF pal(i%).r < 0 THEN pal(i%).r = 0
    IF pal(i%).r > 63 THEN pal(i%).r = 63
    IF pal(i%).g < 0 THEN pal(i%).g = 0
    IF pal(i%).g > 63 THEN pal(i%).g = 63
    IF pal(i%).B < 0 THEN pal(i%).B = 0
    IF pal(i%).B > 63 THEN pal(i%).B = 63
    setrgb i%, pal(i%).r, pal(i%).g, pal(i%).B
   END IF
  NEXT i%
 END IF
LOOP
setrgb 15, pal(15).r, pal(15).g, pal(15).B
setrgb 7, pal(7).r, pal(7).g, pal(7).B
regs.ax = 2: localmouseint
PUT (0, 0), scr&, PSET
regs.ax = 1: localmouseint
ERASE pal, scr&
END SUB

SUB setrgb (c%, r%, g%, B%)
OUT &H3C8, c%
OUT &H3C9, r%
OUT &H3C9, g%
OUT &H3C9, B%
END SUB

SUB upval (x%, y%, v%, display%)
IF display% THEN
 v$ = LTRIM$(RTRIM$(STR$(v%))): IF LEN(v$) = 1 THEN v$ = "0" + v$
 puttextxy x%, y%, v$, 0
END IF
v% = v% + 1
IF v% > 63 THEN v% = 63
IF display% THEN
 v$ = LTRIM$(RTRIM$(STR$(v%))): IF LEN(v$) = 1 THEN v$ = "0" + v$
 puttextxy x%, y%, v$, 15
END IF
END SUB


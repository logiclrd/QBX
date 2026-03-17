DECLARE SUB effects ()
DECLARE SUB paledit ()
DECLARE SUB putall ()
DECLARE SUB drawcolorframe ()
DECLARE SUB quit ()
DECLARE SUB getall ()
DECLARE SUB paste (x%, y%, buffer%())
DECLARE SUB fill (xp%, yp%, fc%, bc%)
DECLARE SUB copy (x1%, y1%, x2%, y2%, buffer%())
DECLARE SUB hidetool (num%)
DECLARE SUB showtool (num%)
DECLARE SUB getlen ()
DECLARE SUB writefile (a$)
DECLARE SUB getregion (x1%, y1%, x2%, y2%)
DECLARE SUB circl (x%, y%, r!, a!, c%)
DECLARE SUB hidemouse ()
DECLARE SUB showmouse ()
DECLARE SUB puttextxy (x%, y%, a$, c%)
DECLARE SUB putfontxy (x%, y%, a$, c%, f$)
DECLARE SUB button (x%, y%, x2%, y2%, t$, l%, r$)
DECLARE SUB clrscr ()
DECLARE SUB mouseint ()
DECLARE SUB define (x%, y%, x2%, y2%, c$)
DECLARE SUB toolleft ()
DECLARE SUB toolright ()
DECLARE SUB toolbar ()
DECLARE SUB colorbar ()
DECLARE FUNCTION readfile% (a$)
DECLARE FUNCTION mousepress% ()
DECLARE FUNCTION getresponse$ ()
DECLARE FUNCTION resetmouse% ()
DECLARE FUNCTION getinput1$ (x%, y%, f$, text$)
'$DYNAMIC
CLEAR , , 16384
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
TYPE xytype
 x AS INTEGER
 y AS INTEGER
 x2 AS INTEGER
 y2 AS INTEGER
END TYPE
TYPE buttontype
 selectarea AS xytype
 selectchar AS STRING * 1
 selectcharx AS STRING * 2
 l AS INTEGER
END TYPE
DIM SHARED buttons(50) AS buttontype, numbuttons%
DIM SHARED inregs AS regtype, outregs AS regtype
DIM SHARED toolbuffer%(1 TO 23, 1 TO 32, 1 TO 8)
DIM SHARED tempbuffer%(135, 80), toolstart%, colorstart%
DIM SHARED saved1%(160, 7), saved2%(160, 40), saved3%(160, 151)
DIM SHARED clipbrd11%(80, 100), clipbrd12%(80, 100), clipbrd21%(80, 100), clipbrd22%(80, 100)
DIM listing%(110), xs%(1500), ys%(1500)
IF resetmouse% THEN
 PRINT "Mouse driver not installed!"
 SYSTEM
END IF
SCREEN 13
getall
IF resetmouse% THEN
 SCREEN 0: WIDTH 80, 25
 PRINT "Error initializing mouse!"
 SYSTEM
END IF
ON KEY(10) GOSUB F10
KEY(10) ON
button 110, 60, 210, 76, "PAINT V1.1", 0, CHR$(13)
LINE (40, 80)-(280, 105), 7, BF
LINE (40, 80)-(280, 105), 15, B
puttextxy 49, 85, "Paint is Copyright 1996/7", 15
puttextxy 46, 93, "by ArCoSoft Unincorporated", 15
define 40, 80, 280, 105, CHR$(13)
a$ = getresponse$
OUT &H3C8, 1: OUT &H3C9, 0: OUT &H3C9, 0: OUT &H3C9, 0
COLOR 1
colorstart% = 0
toolstart% = 1
FOR i% = 1 TO 23
 READ a$
 LOCATE 1, 1
 PRINT a$ + SPACE$(10)
 FOR j% = 0 TO 31
  FOR k% = 0 TO 7
   toolbuffer%(i%, j% + 1, k% + 1) = POINT(j% * 2, k%) * 15
  NEXT k%
 NEXT j%
 FOR k% = 1 TO 8
  toolbuffer%(i%, 31, k%) = 15
 NEXT k%
NEXT i%
OUT &H3C8, 1: OUT &H3C9, 0: OUT &H3C9, 0: OUT &H3C9, 42
clrscr
drawcolorframe
colorchosen% = 7
tool% = 1: showtool tool%
sprayradius% = 5
toolsandcolorson% = -1
default% = 1
OPEN "SYSTEM.FNT" FOR BINARY AS #1
font$ = INPUT$(LOF(1), 1)
sysfont$ = font$
fontfile$ = "SYSTEM"
CLOSE #1
DO
 numbuttons% = 0
 define -1, -1, -1, -1, CHR$(27)
 IF toolsandcolorson% = -1 THEN
  toolbar
  colorbar
  IF (colorstart% > 239) AND (colorchosen% < 16) THEN colorstart% = colorstart% - 256
  IF (colorchosen% - colorstart% < 14) AND (colorchosen% - colorstart% >= 0) THEN
   col% = (colorchosen% - colorstart%) \ 2 + 1
   row% = ((colorchosen% - colorstart%) MOD 2) + 1
   col% = col% * 40
   row% = row% * 20 + 150
   IF colorchosen% = 31 THEN
    PSET (col%, row%), 30
    CIRCLE (col%, row%), 3, 30
    PSET (col%, row%), 0
    PAINT (col%, row%), 0, 30
   ELSE
    PSET (col%, row%), 31
    CIRCLE (col%, row%), 3, 31
    PSET (col%, row%), 0
    PAINT (col%, row%), 0, 31
   END IF
  END IF
  IF colorstart% < 0 THEN colorstart% = colorstart% + 256
  define 0, 8, 319, 159, CHR$(250)
 ELSE
  define 0, 0, 319, 199, CHR$(250)
 END IF
 a$ = getresponse$
 SELECT CASE a$
  CASE CHR$(252): toolleft
  CASE CHR$(253): toolright
  CASE CHR$(255) + "K":
   colorstart% = colorstart% - 2
   IF colorstart% < 0 THEN colorstart% = colorstart% + 256
  CASE CHR$(255) + "M":
   colorstart% = colorstart% + 2
   IF colorstart% > 255 THEN colorstart% = colorstart% - 256
  CASE CHR$(255) + CHR$(0): colorchosen% = colorstart%
  CASE CHR$(255) + CHR$(1): colorchosen% = colorstart% + 1
  CASE CHR$(255) + CHR$(2): colorchosen% = colorstart% + 2
  CASE CHR$(255) + CHR$(3): colorchosen% = colorstart% + 3
  CASE CHR$(255) + CHR$(4): colorchosen% = colorstart% + 4
  CASE CHR$(255) + CHR$(5): colorchosen% = colorstart% + 5
  CASE CHR$(255) + CHR$(6): colorchosen% = colorstart% + 6
  CASE CHR$(255) + CHR$(7): colorchosen% = colorstart% + 7
  CASE CHR$(255) + CHR$(8): colorchosen% = colorstart% + 8
  CASE CHR$(255) + CHR$(9): colorchosen% = colorstart% + 9
  CASE CHR$(255) + CHR$(10): colorchosen% = colorstart% + 10
  CASE CHR$(255) + CHR$(11): colorchosen% = colorstart% + 11
  CASE CHR$(255) + CHR$(12): colorchosen% = colorstart% + 12
  CASE CHR$(255) + CHR$(13): colorchosen% = colorstart% + 13
  CASE CHR$(254) + CHR$(1): quit
  CASE CHR$(254) + CHR$(2): hidetool tool%: tool% = 1: showtool tool%
  CASE CHR$(254) + CHR$(3): hidetool tool%: tool% = 2: showtool tool%
  CASE CHR$(254) + CHR$(4): hidetool tool%: tool% = 3: showtool tool%
  CASE CHR$(254) + CHR$(5): hidetool tool%: tool% = 4: showtool tool%
  CASE CHR$(254) + CHR$(6): hidetool tool%: tool% = 5: showtool tool%
  CASE CHR$(254) + CHR$(7): hidetool tool%: tool% = 6: showtool tool%
  CASE CHR$(254) + CHR$(8): hidetool tool%: tool% = 7: showtool tool%
  CASE CHR$(254) + CHR$(9): hidetool tool%: tool% = 8: showtool tool%
  CASE CHR$(254) + CHR$(10): hidetool tool%: tool% = 9: showtool tool%
  CASE CHR$(254) + CHR$(11): hidetool tool%: tool% = 10: showtool tool%
  CASE CHR$(254) + CHR$(12): hidetool tool%: tool% = 11: showtool tool%
  CASE CHR$(254) + CHR$(13):
   DO: LOOP WHILE mousepress%
   getregion x1%, y1%, x2%, y2%
   IF x1% > x2% THEN SWAP x1%, x2%
   IF y1% > y2% THEN SWAP y1%, y2%
   putall
   IF (x2% - x1% > 78) OR (y2% - y1% > 133) THEN
    LINE (0, 8)-(319, 159), 0, BF
    LINE (41, 43)-(279, 63), 7, BF
    LINE (41, 43)-(279, 63), 15, B
    puttextxy 56, 45, "Resolution insufficient", 15
    puttextxy 43, 53, "for that size of zoom box!", 15
    showmouse
    WHILE mousepress% = 0: WEND
    WHILE mousepress%: WEND
    hidemouse
    putall
   ELSE
    numbuttons% = 0
    DEF SEG = &HA000
    FOR i% = x1% TO x2%
     FOR j% = y1% TO y2%
      tempbuffer%(i% - x1% + 1, j% - y1% + 1) = PEEK(j% * 320! + i%)
     NEXT j%
    NEXT i%
    xs% = x2% - x1% + 1
    ys% = y2% - y1% + 1
    xb! = 267 / xs%
    yb! = 157 / ys%
    LINE (0, 0)-(319, 159), 0, BF
    define 0, 0, xs% * xb! - 1, ys% * yb! - 1, CHR$(250)
    button 275, 40, 318, 56, "Grid", 1, "G"
    button 275, 60, 318, 76, "Save", 1, "S"
    button 275, 80, 318, 96, "Quit", 1, "Q"
    gridon% = -1
    drawcolorframe
    FOR i% = 1 TO xs%
     FOR j% = 1 TO ys%
      LINE (i% * xb! - xb!, j% * yb! - yb!)-(i% * xb! - 1, j% * yb! - 1), tempbuffer%(i%, j%), BF
      LINE (i% * xb! - xb!, j% * yb! - yb!)-(i% * xb!, j% * yb!), 15, B
     NEXT j%
    NEXT i%
    DO
     numbuttons% = 4
     hidemouse
     colorbar
     IF (colorstart% > 239) AND (colorchosen% < 16) THEN colorstart% = colorstart% - 256
     IF (colorchosen% - colorstart% < 14) AND (colorchosen% - colorstart% >= 0) THEN
      col% = (colorchosen% - colorstart%) \ 2 + 1
      row% = ((colorchosen% - colorstart%) MOD 2) + 1
      col% = col% * 40
      row% = row% * 20 + 150
      IF colorchosen% = 31 THEN
       PSET (col%, row%), 30
       CIRCLE (col%, row%), 3, 30
       PSET (col%, row%), 0
       PAINT (col%, row%), 0, 30
      ELSE
       PSET (col%, row%), 31
       CIRCLE (col%, row%), 3, 31
       PSET (col%, row%), 0
       PAINT (col%, row%), 0, 31
      END IF
     END IF
     IF colorstart% < 0 THEN colorstart% = colorstart% + 256
     FOR i% = 1 TO xs%
      FOR j% = 1 TO ys%
       LINE (i% * xb! - xb!, j% * yb! - yb!)-(i% * xb! - 1, j% * yb! - 1), tempbuffer%(i%, j%), BF
       IF gridon% THEN LINE (i% * xb! - xb!, j% * yb! - yb!)-(i% * xb!, j% * yb!), 15, B
      NEXT j%
     NEXT i%
     IF gridon% = 0 THEN LINE (xs% * xb!, 0)-(xs% * xb!, ys% * yb!), 0: LINE (0, ys% * yb!)-(xs% * xb!, ys% * yb!), 0
     showmouse
     a$ = getresponse$
     SELECT CASE a$
      CASE CHR$(255) + "K":
       colorstart% = colorstart% - 2
       IF colorstart% < 0 THEN colorstart% = colorstart% + 256
      CASE CHR$(255) + "M":
       colorstart% = colorstart% + 2
       IF colorstart% > 255 THEN colorstart% = colorstart% - 256
      CASE CHR$(255) + CHR$(0): colorchosen% = colorstart%
      CASE CHR$(255) + CHR$(1): colorchosen% = colorstart% + 1
      CASE CHR$(255) + CHR$(2): colorchosen% = colorstart% + 2
      CASE CHR$(255) + CHR$(3): colorchosen% = colorstart% + 3
      CASE CHR$(255) + CHR$(4): colorchosen% = colorstart% + 4
      CASE CHR$(255) + CHR$(5): colorchosen% = colorstart% + 5
      CASE CHR$(255) + CHR$(6): colorchosen% = colorstart% + 6
      CASE CHR$(255) + CHR$(7): colorchosen% = colorstart% + 7
      CASE CHR$(255) + CHR$(8): colorchosen% = colorstart% + 8
      CASE CHR$(255) + CHR$(9): colorchosen% = colorstart% + 9
      CASE CHR$(255) + CHR$(10): colorchosen% = colorstart% + 10
      CASE CHR$(255) + CHR$(11): colorchosen% = colorstart% + 11
      CASE CHR$(255) + CHR$(12): colorchosen% = colorstart% + 12
      CASE CHR$(255) + CHR$(13): colorchosen% = colorstart% + 13
      CASE CHR$(250):
       thisx% = (x% - (xb! / 2)) / xb! + 1
       thisy% = (y% - (yb! / 2)) / yb! + 1
       tempbuffer%(thisx%, thisy%) = colorchosen%
      CASE "G":
       WHILE mousepress%: WEND
       IF gridon% = 0 THEN gridon% = -1 ELSE gridon% = 0
       FOR i% = 1 TO xs%
        FOR j% = 1 TO ys%
         IF NOT gridon% THEN LINE (i% * xb! - xb!, j% * yb! - yb!)-(i% * xb!, j% * yb!), 0, B
         LINE (i% * xb! - xb!, j% * yb! - yb!)-(i% * xb! - 1, j% * yb! - 1), tempbuffer%(i%, j%), BF
         IF gridon% THEN LINE (i% * xb! - xb!, j% * yb! - yb!)-(i% * xb!, j% * yb!), 15, B
        NEXT j%
       NEXT i%
      CASE "S": save% = 1: EXIT DO
      CASE "Q": save% = 0: EXIT DO
     END SELECT
    LOOP
    putall
    IF save% THEN
     FOR i% = x1% TO x2%
      FOR j% = y1% TO y2%
       PSET (i%, j%), tempbuffer%(i% - x1% + 1, j% - y1% + 1)
      NEXT j%
     NEXT i%
     getall
    END IF
    DEF SEG
   END IF
   WHILE mousepress%: WEND
   drawcolorframe
  CASE CHR$(254) + CHR$(14):
   DO: LOOP WHILE mousepress%
   getregion x1%, y1%, x2%, y2%
   putall
   LINE (x1%, y1%)-(x2%, y2%), 0, BF
   getall
   drawcolorframe
  CASE CHR$(254) + CHR$(15):
   DO: LOOP WHILE mousepress%
   GET (0, 8)-(319, 159), saved3%
   putall
   showmouse
   DO: LOOP UNTIL mousepress%
   hidemouse
   colorchosen% = POINT(x%, y%)
   colorstart% = (colorchosen% - (colorchosen% MOD 2)) - 6
   IF colorstart% < 0 THEN colorstart% = colorstart% + 256
   drawcolorframe
  CASE CHR$(254) + CHR$(16), CHR$(254) + CHR$(17):
   DO: LOOP WHILE mousepress%
   getregion x1%, y1%, x2%, y2%
   putall
   clip11used% = 1
   IF x2% - x1% > 160 THEN clip12used% = 1 ELSE clip12used% = 0
   IF y2% - y1% > 100 THEN
    clip21used% = 1
    IF clip12used% THEN clip22used% = 1 ELSE clip22used% = 0
   ELSE
    clip21used% = 0
   END IF
   IF clip12used% THEN
    IF clip22used% THEN
     copy x1%, y1%, x1% + 159, y1% + 99, clipbrd11%()
     copy x1% + 160, y1%, x2%, y1% + 99, clipbrd12%()
     copy x1%, y1% + 100, x1% + 159, y2%, clipbrd21%()
     copy x1% + 160, y1% + 100, x2%, y2%, clipbrd22%()
    ELSE
     copy x1%, y1%, x1% + 159, y2%, clipbrd11%()
     copy x1% + 160, y1%, x2%, y2%, clipbrd12%()
    END IF
   ELSE
    IF clip22used% THEN
     copy x1%, y1%, x2%, y1% + 99, clipbrd11%()
     copy x1%, y1% + 100, x2%, y2%, clipbrd21%()
    ELSE
     copy x1%, y1%, x2%, y2%, clipbrd11%()
    END IF
   END IF
   IF a$ = CHR$(254) + CHR$(16) THEN LINE (x1%, y1%)-(x2%, y2%), 0, BF
   getall
   drawcolorframe
  CASE CHR$(254) + CHR$(18):
   DO: LOOP WHILE mousepress%
   putall
   hidemouse
   GET (0, 8)-(319, 159), saved3%
   paste x%, y%, clipbrd11%()
   IF clip12used% THEN paste x% + 160, y%, clipbrd12%()
   IF clip21used% THEN paste x%, y% + 100, clipbrd21%()
   IF clip22used% THEN paste x% + 160, y% + 100, clipbrd22%()
   a% = mousepress%
   DO UNTIL a% <> 0
    ox% = x%: oy% = y%
    DO
     a% = mousepress%
    LOOP UNTIL (x% <> ox%) OR (y% <> oy%) OR (a% <> 0)
    putall
    paste x%, y%, clipbrd11%()
    IF clip12used% THEN paste x% + 160, y%, clipbrd12%()
    IF clip21used% THEN paste x%, y% + 100, clipbrd21%()
    IF clip22used% THEN paste x% + 160, y% + 100, clipbrd22%()
    x1% = x%
    y1% = y%
    x2% = x% + clipbrd11%(0, 0) + clipbrd12%(0, 0) * clip12used%
    y2% = y% + clipbrd11%(0, 1) + clipbrd21%(0, 1) * clip21used%
    LINE (x1%, y1%)-(x2%, y2%), 15, B, &H5555
   LOOP
   putall
   paste x%, y%, clipbrd11%()
   IF clip12used% THEN paste x% + 160, y%, clipbrd12%()
   IF clip21used% THEN paste x%, y% + 100, clipbrd21%()
   IF clip22used% THEN paste x% + 160, y% + 100, clipbrd22%()
   getall
   drawcolorframe
   showmouse
   DO: LOOP WHILE mousepress%
  CASE CHR$(254) + CHR$(19):
   putall
   paledit
   drawcolorframe
  CASE CHR$(254) + CHR$(20):
   putall
   effects
   getall
   drawcolorframe
  CASE CHR$(254) + CHR$(21):
savepic:
   sd% = default%
   IF saveflag% THEN IF default% = 2 THEN default% = 6
   GET (0, 8)-(319, 159), saved3%
   LINE (0, 8)-(319, 159), 0, BF
   numbuttons% = 0
   puttextxy 80, 60, "Output file format:", 15
   button 80, 70, 122, 86, "BHP", 0, CHR$(3)
   button 139, 70, 181, 86, "ACG", 0, CHR$(4)
   button 198, 70, 240, 86, "BAS", 0, CHR$(5)
   button 80, 90, 122, 106, "BSV", 0, CHR$(6)
   button 139, 90, 181, 106, "PAS", 0, CHR$(7)
   button 198, 90, 240, 106, "RAW", 0, CHR$(10)
   SELECT CASE default%
    CASE 1: LINE (79, 69)-(123, 87), 15, B
    CASE 2: LINE (138, 69)-(182, 87), 15, B
    CASE 3: LINE (197, 69)-(241, 87), 15, B
    CASE 4: LINE (79, 89)-(123, 107), 15, B
    CASE 5: LINE (138, 89)-(182, 107), 15, B
    CASE 6: LINE (197, 89)-(241, 107), 15, B
   END SELECT
   puttextxy 42, 40, "Filename:", 15
   LINE (124, 38)-(198, 50), 7, BF
   LINE (124, 38)-(198, 50), 15, B
   define 124, 38, 198, 50, CHR$(9)
   button 101, 125, 145, 141, "Save", 1, "S"
   button 149, 125, 211, 141, "Cancel", 0, CHR$(27)
   text$ = ""
   DO
    a$ = getresponse$
    SELECT CASE a$
     CASE CHR$(3), CHR$(4), CHR$(5), CHR$(6), CHR$(7), CHR$(10):
      SELECT CASE default%
       CASE 1: LINE (79, 69)-(123, 87), 0, B
       CASE 2: LINE (138, 69)-(182, 87), 0, B
       CASE 3: LINE (197, 69)-(241, 87), 0, B
       CASE 4: LINE (79, 89)-(123, 107), 0, B
       CASE 5: LINE (138, 89)-(182, 107), 0, B
       CASE 6: LINE (197, 89)-(241, 107), 0, B
      END SELECT
      SELECT CASE a$
       CASE CHR$(3): default% = 1
       CASE CHR$(4): IF NOT saveflag% THEN default% = 2
       CASE CHR$(5): default% = 3
       CASE CHR$(6): default% = 4
       CASE CHR$(7): default% = 5
       CASE CHR$(10): default% = 6
      END SELECT
      SELECT CASE default%
       CASE 1: LINE (79, 69)-(123, 87), 15, B
       CASE 2: LINE (138, 69)-(182, 87), 15, B
       CASE 3: LINE (197, 69)-(241, 87), 15, B
       CASE 4: LINE (79, 89)-(123, 107), 15, B
       CASE 5: LINE (138, 89)-(182, 107), 15, B
       CASE 6: LINE (197, 89)-(241, 107), 15, B
      END SELECT
     CASE CHR$(9):
      hidemouse
      st# = TIMER - 1
      DO
       blink% = 0
       DO
        a$ = INKEY$
        IF ABS(TIMER - st#) > .5 THEN
         st# = TIMER
         IF blink% = -1 THEN
          blink% = 0
          IF LEN(text$) < 8 THEN LINE (LEN(text$) * 9 + 126, 40)-(LEN(text$) * 9 + 133, 47), 7, BF
         ELSE
          blink% = -1
          IF LEN(text$) < 8 THEN LINE (LEN(text$) * 9 + 126, 40)-(LEN(text$) * 9 + 133, 47), 15, BF
         END IF
        END IF
       LOOP UNTIL a$ <> ""
       IF LEN(text$) < 8 THEN LINE (LEN(text$) * 9 + 126, 40)-(LEN(text$) * 9 + 133, 47), 7, BF
       SELECT CASE a$
        CASE CHR$(8):
         IF LEN(text$) > 0 THEN
          text$ = LEFT$(text$, LEN(text$) - 1)
          LINE (LEN(text$) * 9 + 126, 40)-(LEN(text$) * 9 + 133, 47), 7, BF
         END IF
        CASE CHR$(13):
         EXIT DO
        CASE IS < CHR$(31):
        CASE ELSE:
         IF (LEN(a$) = 1) AND (LEN(text$) < 8) THEN
          puttextxy LEN(text$) * 9 + 126, 40, a$, 15
          text$ = text$ + a$
         END IF
       END SELECT
      LOOP
      showmouse
     CASE "S", CHR$(27): EXIT DO
    END SELECT
   LOOP
   WHILE mousepress%: WEND
   IF a$ = "S" THEN
    putall
    DEF SEG = &HA000
    SELECT CASE default%
     CASE 1:
      getregion x1%, y1%, x2%, y2%
      putall
      IF x1% > x2% THEN SWAP x1%, x2%
      IF y1% > y2% THEN SWAP y1%, y2%
      x1% = x1% + 1
      y1% = y1% + 1
      x2% = x2% - 1
      y2% = y2% - 1
      OPEN text$ + ".BHP" FOR OUTPUT AS #1
      PRINT #1, CHR$(13) + "BHP" + MKI$(x1%) + MKI$(y1%) + MKI$(x2%) + MKI$(y2%);
      FOR i% = x1% TO x2%
       LINE (x1% - 1, y1% - 1)-(x2% + 1, y2% + 1), 0, B
       stage% = (stage% MOD 4) + 1
       SELECT CASE stage%
        CASE 1: LINE (x1% - 1, y1% - 1)-(x2% + 1, y2% + 1), 15, B, &H3333
        CASE 2: LINE (x1% - 1, y1% - 1)-(x2% + 1, y2% + 1), 15, B, &H6666
        CASE 3: LINE (x1% - 1, y1% - 1)-(x2% + 1, y2% + 1), 15, B, &H9999
        CASE 4: LINE (x1% - 1, y1% - 1)-(x2% + 1, y2% + 1), 15, B, &HCCCC
       END SELECT
       a$ = ""
       FOR j% = y1% TO y2%
        a$ = a$ + CHR$(POINT(i%, j%))
       NEXT j%
       PRINT #1, a$;
      NEXT i%
      CLOSE #1
      putall
     CASE 2:
      writefile text$ + ".ACG"
      putall
     CASE 3:
      getregion x1%, y1%, x2%, y2%
      putall
      IF x1% > x2% THEN SWAP x1%, x2%
      IF y1% > y2% THEN SWAP y1%, y2%
      x1% = x1% + 1
      y1% = y1% + 1
      x2% = x2% - 1
      y2% = y2% - 1
      OPEN text$ + ".BAS" FOR APPEND AS #1
      FOR i% = y1% TO y2%
       a$ = "DATA "
       FOR j% = x1% TO x2%
        a$ = a$ + LTRIM$(RTRIM$(STR$(PEEK(i% * 320! + j%))))
        IF j% < x2% THEN a$ = a$ + ","
       NEXT j%
       PRINT #1, a$
      NEXT i%
      PRINT #1, "DIM picture%(" + LTRIM$(RTRIM$(STR$(x2% - x1% + 1))) + "," + LTRIM$(RTRIM$(STR$(y2% - y1% + 1))) + ")"
      PRINT #1, "FOR i%=1 TO " + LTRIM$(RTRIM$(STR$(y2% - y1% + 1)))
      PRINT #1, " FOR j%=1 TO " + LTRIM$(RTRIM$(STR$(x2% - x1% + 1)))
      PRINT #1, "  READ picture%(j%,i%)"
      PRINT #1, " NEXT j%"
      PRINT #1, "NEXT i%"
      CLOSE #1
     CASE 4:
      BSAVE text$ + ".BSV", 0, 64000
     CASE 5:
      getregion x1%, y1%, x2%, y2%
      putall
      IF x1% > x2% THEN SWAP x1%, x2%
      IF y1% > y2% THEN SWAP y1%, y2%
      x1% = x1% + 1
      y1% = y1% + 1
      x2% = x2% - 1
      y2% = y2% - 1
      OPEN text$ + ".PAS" FOR APPEND AS #1
      PRINT #1, " {ACSPAINT picture - put this at the beginning of your program}"
      PRINT #1, "const picture:array[1.." + LTRIM$(RTRIM$(STR$(x2% - x1% + 1))) + ",1.." + LTRIM$(RTRIM$(STR$(y2% - y1% + 1))) + "]="
      FOR i% = y1% TO y2%
       IF i% = y1% THEN a$ = "   ((" ELSE a$ = "    ("
       FOR j% = x1% TO x2%
        a$ = a$ + LTRIM$(RTRIM$(STR$(PEEK(i% * 320! + j%))))
        IF j% < x2% THEN a$ = a$ + "," ELSE a$ = a$ + ")"
       NEXT j%
       IF i% = y2% THEN a$ = a$ + ");"
       PRINT #1, a$
      NEXT i%
      PRINT #1, " {end of ACSPAINT-generated picture: access image with picture[x][y]}"
      CLOSE #1
     CASE 6:
      getregion x1%, y1%, x2%, y2%
      IF x1% > x2% THEN SWAP x1%, x2%
      IF y1% > y2% THEN SWAP y1%, y2%
      putall
      OPEN text$ + ".RAW" FOR OUTPUT AS #1
      PRINT #1, MKI$(x2% - x1% + 1) + MKI$(y2% - y1% + 1);
      FOR i% = 0 TO 255
       OUT &H3C7, i%
       PRINT #1, CHR$(INP(&H3C9)); CHR$(INP(&H3C9)); CHR$(INP(&H3C9));
      NEXT i%
      FOR i% = y1% TO y2%
       a$ = ""
       FOR j% = x1% TO x2%
        a$ = a$ + CHR$(PEEK(i% * 320! + j%))
       NEXT j%
       PRINT #1, a$;
       LINE (x1%, i%)-(x1% + 4, i%), 15
       LINE (x2% - 4, i%)-(x2%, i%), 15
      NEXT i%
      putall
      CLOSE #1
    END SELECT
    DEF SEG
   ELSE
    PUT (0, 8), saved3%, PSET
   END IF
   default% = sd%
   IF saveflag% THEN saveflag% = 0: RETURN
   drawcolorframe
  CASE CHR$(254) + CHR$(22):
   sd% = default%
   GET (0, 8)-(319, 159), saved3%
   LINE (0, 8)-(319, 159), 0, BF
   numbuttons% = 0
   hidemouse
   puttextxy 80, 60, "Input file format:", 15
   button 80, 70, 122, 86, "BHP", 0, CHR$(3)
   button 139, 70, 181, 86, "ACG", 0, CHR$(4)
   button 198, 70, 240, 86, "BAS", 0, CHR$(5)
   button 80, 90, 122, 106, "BSV", 0, CHR$(6)
   button 139, 90, 181, 106, "PAS", 0, CHR$(7)
   button 198, 90, 240, 106, "RAW", 0, CHR$(10)
   button 101, 125, 145, 141, "Load", 1, "L"
   button 149, 125, 211, 141, "Cancel", 0, CHR$(27)
   SELECT CASE default%
    CASE 1: LINE (79, 69)-(123, 87), 15, B
    CASE 2: LINE (138, 69)-(182, 87), 15, B
    CASE 3: LINE (197, 69)-(241, 87), 15, B
    CASE 4: LINE (79, 89)-(123, 107), 15, B
    CASE 5: LINE (138, 89)-(182, 107), 15, B
    CASE 6: LINE (197, 89)-(241, 107), 15, B
   END SELECT
   puttextxy 30, 111, "Filename:", 15
   LINE (111, 109)-(292, 120), 7, BF
   LINE (111, 109)-(292, 120), 15, B
   define 111, 109, 292, 120, CHR$(9)
   text$ = ""
   showmouse
   DO
    a$ = getresponse$
    SELECT CASE a$
     CASE CHR$(3), CHR$(4), CHR$(5), CHR$(6), CHR$(7), CHR$(10):
      SELECT CASE default%
       CASE 1: LINE (79, 69)-(123, 87), 0, B
       CASE 2: LINE (138, 69)-(182, 87), 0, B
       CASE 3: LINE (197, 69)-(241, 87), 0, B
       CASE 4: LINE (79, 89)-(123, 107), 0, B
       CASE 5: LINE (138, 89)-(182, 107), 0, B
       CASE 6: LINE (197, 89)-(241, 107), 0, B
      END SELECT
      SELECT CASE a$
       CASE CHR$(3): default% = 1
       CASE CHR$(4): default% = 2
       CASE CHR$(5): default% = 3
       CASE CHR$(6): default% = 4
       CASE CHR$(7): default% = 5
       CASE CHR$(10): default% = 6
      END SELECT
      SELECT CASE default%
       CASE 1: LINE (79, 69)-(123, 87), 15, B
       CASE 2: LINE (138, 69)-(182, 87), 15, B
       CASE 3: LINE (197, 69)-(241, 87), 15, B
       CASE 4: LINE (79, 89)-(123, 107), 15, B
       CASE 5: LINE (138, 89)-(182, 107), 15, B
       CASE 6: LINE (197, 89)-(241, 107), 15, B
      END SELECT
     CASE CHR$(9):
      hidemouse
      st# = TIMER - 1
      DO
       blink% = 0
       DO
        a$ = INKEY$
        IF ABS(TIMER - st#) > .5 THEN
         st# = TIMER
         IF blink% = -1 THEN
          blink% = 0
          IF LEN(text$) < 8 THEN LINE (LEN(text$) * 9 + 113, 111)-(LEN(text$) * 9 + 120, 118), 7, BF
         ELSE
          blink% = -1
          IF LEN(text$) < 8 THEN LINE (LEN(text$) * 9 + 113, 111)-(LEN(text$) * 9 + 120, 118), 15, BF
         END IF
        END IF
       LOOP UNTIL a$ <> ""
       IF LEN(text$) < 8 THEN LINE (LEN(text$) * 9 + 113, 111)-(LEN(text$) * 9 + 120, 118), 7, BF
       SELECT CASE a$
        CASE CHR$(8):
         IF LEN(text$) > 0 THEN
          text$ = LEFT$(text$, LEN(text$) - 1)
          LINE (LEN(text$) * 9 + 113, 111)-(LEN(text$) * 9 + 120, 118), 7, BF
         END IF
        CASE CHR$(13):
         EXIT DO
        CASE IS < CHR$(31):
        CASE ELSE:
         IF (LEN(a$) = 1) AND (LEN(text$) < 20) THEN
          puttextxy LEN(text$) * 9 + 113, 111, a$, 15
          text$ = text$ + a$
         END IF
       END SELECT
      LOOP
      showmouse
     CASE "L", CHR$(27): EXIT DO
    END SELECT
   LOOP
   WHILE mousepress%: WEND
   IF a$ = "L" THEN
    IF INSTR(text$, ".") = 0 THEN
     ON ERROR GOTO setflag
     flag% = 0
     OPEN text$ + ".BHP" FOR INPUT AS #1
     IF flag% THEN
      flag% = 0
      OPEN text$ + ".ACG" FOR INPUT AS #1
      IF flag% THEN
       flag% = 0
       OPEN text$ + ".BAS" FOR INPUT AS #1
       IF flag% THEN
        flag% = 0
        OPEN text$ + ".BSV" FOR INPUT AS #1
        IF flag% THEN
         flag% = 0
         OPEN text$ + ".PAS" FOR INPUT AS #1
         IF flag% THEN
          flag% = 0
          OPEN text$ + ".RAW" FOR INPUT AS #1
          IF NOT flag% THEN CLOSE #1: text$ = text$ + ".RAW": default% = 6
         ELSE
          CLOSE #1: text$ = text$ + ".PAS": default% = 5
         END IF
        ELSE
         CLOSE #1: text$ = text$ + ".BSV": default% = 4
        END IF
       ELSE
        CLOSE #1, text$ = text$ + ".BAS": default% = 3
       END IF
      ELSE
       CLOSE #1: text$ = text$ + ".ACG": default% = 2
      END IF
     ELSE
      CLOSE #1: text$ = text$ + ".BHP": default% = 1
     END IF
    ELSE
     IF INSTR(text$, ".") < LEN(text$) THEN
      SELECT CASE UCASE$(MID$(text$, INSTR(text$, ".") + 1))
       CASE "BHP": default% = 1
       CASE "ACG": default% = 2
       CASE "BAS": default% = 3
       CASE "BSV": default% = 4
       CASE "PAS": default% = 5
       CASE "RAW": default% = 6
      END SELECT
     END IF
    END IF
    OPEN text$ FOR INPUT AS #1
    IF flag% THEN
     LINE (0, 8)-(319, 159), 0, BF
     LINE (90, 78)-(229, 90), 7, BF
     LINE (90, 78)-(229, 90), 15, B
     puttextxy 92, 80, "File not found!", 15
     define 0, 0, 319, 199, CHR$(13)
     a$ = getresponse$
    ELSE
     CLOSE #1
     putall
     DEF SEG = &HA000
     SELECT CASE default%
      CASE 1:
       OPEN text$ FOR BINARY AS #1
       header$ = INPUT$(4, 1)
       IF ASC(header$) <> 13 THEN
        CLOSE #1
        LINE (0, 8)-(319, 159), 0, BF
        LINE (81, 78)-(238, 90), 7, BF
        LINE (81, 78)-(238, 90), 15, B
        puttextxy 83, 80, "Wrong video mode!", 15
        define 0, 0, 319, 199, CHR$(13)
        a$ = getresponse$
        putall
       ELSEIF MID$(header$, 2) <> "BHP" THEN
        CLOSE #1
        LINE (0, 8)-(319, 159), 0, BF
        LINE (70, 78)-(250, 90), 7, BF
        LINE (70, 78)-(250, 90), 15, B
        puttextxy 72, 80, "Incorrect file format!", 15
        define 0, 0, 319, 199, CHR$(13)
        a$ = getresponse$
        putall
       ELSE
        x1% = CVI(INPUT$(2, 1))
        y1% = CVI(INPUT$(2, 1))
        x2% = CVI(INPUT$(2, 1))
        y2% = CVI(INPUT$(2, 1))
        FOR i% = x1% TO x2%
         a$ = INPUT$(y2% - y1% + 1, 1)
         FOR j% = y1% TO y2%
          PSET (i%, j%), ASC(MID$(a$, j% - y1% + 1, 1))
         NEXT j%
        NEXT i%
        getall
        CLOSE #1
       END IF
      CASE 2:
       a% = readfile(text$)
       IF a% <> 0 THEN
        LINE (0, 8)-(319, 159), 0, BF
        LINE (76, 78)-(243, 90), 7, BF
        LINE (76, 78)-(243, 90), 15, B
        puttextxy 78, 80, "Error reading file!", 15
        define 0, 0, 319, 199, CHR$(13)
        a$ = getresponse$
        putall
       ELSE
        getall
       END IF
      CASE 3:
       LINE (0, 8)-(319, 159), 0, BF
       LINE (70, 78)-(250, 90), 7, BF
       LINE (70, 78)-(250, 90), 15, B
       puttextxy 72, 80, "Cannot load BAS files!", 15
       define 0, 0, 319, 199, CHR$(13)
       a$ = getresponse$
       putall
      CASE 4:
       BLOAD text$, 0
       getall
      CASE 5:
       LINE (0, 8)-(319, 159), 0, BF
       LINE (70, 78)-(250, 90), 7, BF
       LINE (70, 78)-(250, 90), 15, B
       puttextxy 72, 80, "Cannot load PAS files!", 15
       define 0, 0, 319, 199, CHR$(13)
       a$ = getresponse$
       putall
      CASE 6:
       numbuttons% = 0
       LINE (5, 82)-(315, 122), 7, BF
       LINE (5, 82)-(315, 122), 15, B
       puttextxy 25, 84, "Click on OK, then click on the", 15
       puttextxy 7, 96, "upperleft corner of where to place", 15
       puttextxy 106, 108, "the image", 15
       button 202, 104, 228, 121, "Ok", 0, CHR$(13)
       a$ = getresponse$
       putall
       showmouse
       DO: LOOP WHILE mousepress%
       DO: LOOP UNTIL mousepress%
       hidemouse
       OPEN text$ FOR BINARY AS #1
       a$ = INPUT$(4, 1)
       xs% = CVI(LEFT$(a$, 2))
       ys% = CVI(RIGHT$(a$, 2))
       a$ = INPUT$(768, 1)
       FOR i% = 0 TO 767 STEP 3
        OUT &H3C8, i% \ 3
        OUT &H3C9, ASC(MID$(a$, i% + 1, 1))
        OUT &H3C9, ASC(MID$(a$, i% + 2, 1))
        OUT &H3C9, ASC(MID$(a$, i% + 3, 1))
       NEXT i%
       FOR i% = 0 TO ys% - 1
        a$ = INPUT$(xs%, 1)
        FOR j% = 0 TO xs% - 1
         PSET (j% + x% - 1, i% + y% - 1), ASC(MID$(a$, j% + 1, 1))
        NEXT j%
       NEXT i%
       getall
       showmouse
       CLOSE #1
     END SELECT
     DEF SEG
     drawcolorframe
    END IF
   ELSE
    PUT (0, 8), saved3%, PSET
   END IF
   default% = sd%
  CASE CHR$(0) + "D":
   GET (0, 8)-(319, 159), saved3%
   LINE (0, 8)-(319, 159), 0, BF
   numbuttons% = 0
   puttextxy 70, 30, "Spray paint brush size:", 15
   button 80, 40, 130, 56, " <", 0, CHR$(1)
   button 190, 40, 240, 56, "  >", 0, CHR$(2)
   puttextxy 80, 60, "Default file format:", 15
   button 80, 70, 122, 86, "BHP", 0, CHR$(3)
   button 139, 70, 181, 86, "ACG", 0, CHR$(4)
   button 198, 70, 240, 86, "BAS", 0, CHR$(5)
   button 80, 90, 122, 106, "BSV", 0, CHR$(6)
   button 139, 90, 181, 106, "PAS", 0, CHR$(7)
   button 198, 90, 240, 106, "RAW", 0, CHR$(10)
   button 138, 120, 182, 136, "Done", 0, CHR$(13)
   puttextxy 145, 44, STR$(sprayradius%), 15
   SELECT CASE default%
    CASE 1: LINE (79, 69)-(123, 87), 15, B
    CASE 2: LINE (138, 69)-(182, 87), 15, B
    CASE 3: LINE (197, 69)-(241, 87), 15, B
    CASE 4: LINE (79, 89)-(123, 107), 15, B
    CASE 5: LINE (138, 89)-(182, 107), 15, B
    CASE 6: LINE (197, 89)-(241, 107), 15, B
   END SELECT
   DO
    a$ = getresponse$
    SELECT CASE a$
     CASE CHR$(1):
      sprayradius% = sprayradius% - 1
      IF sprayradius% <= 0 THEN sprayradius% = 1
      LINE (150, 40)-(190, 59), 0, BF
      puttextxy 145, 44, STR$(sprayradius%), 15
      DO: LOOP UNTIL mousepress% = 0
     CASE CHR$(2):
      sprayradius% = sprayradius% + 1
      IF sprayradius% = 16 THEN sprayradius% = 15
      LINE (150, 40)-(190, 59), 0, BF
      puttextxy 145, 44, STR$(sprayradius%), 15
      DO: LOOP UNTIL mousepress% = 0
     CASE CHR$(3), CHR$(4), CHR$(5), CHR$(6), CHR$(7), CHR$(10):
      SELECT CASE default%
       CASE 1: LINE (79, 69)-(123, 87), 0, B
       CASE 2: LINE (138, 69)-(182, 87), 0, B
       CASE 3: LINE (197, 69)-(241, 87), 0, B
       CASE 4: LINE (79, 89)-(123, 107), 0, B
       CASE 5: LINE (138, 89)-(182, 107), 0, B
       CASE 6: LINE (197, 89)-(241, 107), 0, B
      END SELECT
      SELECT CASE a$
       CASE CHR$(3): default% = 1
       CASE CHR$(4): default% = 2
       CASE CHR$(5): default% = 3
       CASE CHR$(6): default% = 4
       CASE CHR$(7): default% = 5
       CASE CHR$(10): default% = 6
      END SELECT
      SELECT CASE default%
       CASE 1: LINE (79, 69)-(123, 87), 15, B
       CASE 2: LINE (138, 69)-(182, 87), 15, B
       CASE 3: LINE (197, 69)-(241, 87), 15, B
       CASE 4: LINE (79, 89)-(123, 107), 15, B
       CASE 5: LINE (138, 89)-(182, 107), 15, B
       CASE 6: LINE (197, 89)-(241, 107), 15, B
      END SELECT
     CASE CHR$(13): EXIT DO
    END SELECT
   LOOP
   PUT (0, 8), saved3%, PSET
   WHILE mousepress%: WEND
  CASE CHR$(27):
   WHILE mousepress%: WEND
   IF toolsandcolorson% = 0 THEN
    getall
    drawcolorframe
    toolsandcolorson% = -1
   ELSE
    putall
    toolsandcolorson% = 0
   END IF
  CASE CHR$(250):
   IF toolsandcolorson% = -1 THEN
    hidemouse
    PUT (0, 0), saved1%, PSET
    PUT (0, 160), saved2%, PSET
    showmouse
   END IF
   SELECT CASE tool%
    CASE 1:
     DO UNTIL mousepress% = 0
      hidemouse
      PSET (x%, y%), colorchosen%
      showmouse
     LOOP
    CASE 2:
     hidemouse
     PSET (x%, y%), colorchosen%
     showmouse
     DO UNTIL mousepress% = 0
      hidemouse
      FOR i% = 1 TO 10
       a% = mousepress%
       LINE -(x%, y%), colorchosen%
      NEXT i%
      showmouse
     LOOP
    CASE 3:
     GET (0, 8)-(319, 159), saved3%
     xx% = x%: yy% = y%
     DO
      ox% = x%: oy% = y%
      showmouse
      DO
       a% = mousepress%
      LOOP UNTIL (x% <> ox%) OR (y% <> oy%) OR (a% = 0)
      hidemouse
      putall
      LINE (xx%, yy%)-(x%, y%), colorchosen%
     LOOP UNTIL mousepress% = 0
    CASE 4:
     GET (0, 8)-(319, 159), saved3%
     xx% = x%: yy% = y%
     DO
      ox% = x%: oy% = y%
      showmouse
      DO
       a% = mousepress%
      LOOP UNTIL (x% <> ox%) OR (y% <> oy%) OR (a% = 0)
      hidemouse
      putall
      IF ABS(x% - xx%) < ABS(y% - yy%) THEN
       IF y% - yy% < 0 THEN y% = yy% - ABS(x% - xx%) ELSE y% = yy% + ABS(x% - xx%)
      ELSEIF ABS(y% - yy%) < ABS(x% - xx%) THEN
       IF x% - xx% < 0 THEN x% = xx% - ABS(y% - yy%) ELSE x% = xx% + ABS(y% - yy%)
      END IF
      LINE (xx%, yy%)-(x%, y%), colorchosen%, B
     LOOP UNTIL mousepress% = 0
    CASE 5:
     GET (0, 8)-(319, 159), saved3%
     xx% = x%: yy% = y%
     DO
      ox% = x%: oy% = y%
      showmouse
      DO
       a% = mousepress%
      LOOP UNTIL (x% <> ox%) OR (y% <> oy%)
      hidemouse
      putall
      LINE (xx%, yy%)-(x%, y%), colorchosen%, B
     LOOP UNTIL mousepress% = 0
    CASE 6:
     GET (0, 8)-(319, 159), saved3%
     xx% = x%: yy% = y%
     DO
      ox% = x%: oy% = y%
      showmouse
      DO
       a% = mousepress%
      LOOP UNTIL (x% <> ox%) OR (y% <> oy%) OR (a% = 0)
      hidemouse
      putall
      IF ABS(x% - xx%) < ABS(y% - yy%) THEN
       IF y% - yy% < 0 THEN y% = yy% - ABS(x% - xx%) ELSE y% = yy% + ABS(x% - xx%)
      ELSEIF ABS(y% - yy%) < ABS(x% - xx%) THEN
       IF x% - xx% < 0 THEN x% = xx% - ABS(y% - yy%) ELSE x% = xx% + ABS(y% - yy%)
      END IF
      IF x% = xx% THEN
       circl .5 * (xx% + x%), (yy% + y%) / 2, ABS(x% - xx%) / 2, ABS(y% - yy%), colorchosen%
      ELSE
       circl .5 * (xx% + x%), (yy% + y%) / 2, ABS(x% - xx%) / 2, ABS(y% - yy%) / ABS(x% - xx%), colorchosen%
      END IF
      LINE (xx%, yy%)-(x%, y%), 7, B, &HAAAA
     LOOP UNTIL mousepress% = 0
     putall
     IF ABS(x% - xx%) < ABS(y% - yy%) THEN
      IF y% - yy% < 0 THEN y% = yy% - ABS(x% - xx%) ELSE y% = yy% + ABS(x% - xx%)
     ELSEIF ABS(y% - yy%) < ABS(x% - xx%) THEN
      IF x% - xx% < 0 THEN x% = xx% - ABS(y% - yy%) ELSE x% = xx% + ABS(y% - yy%)
     END IF
     IF x% = xx% THEN
      circl .5 * (xx% + x%), (yy% + y%) / 2, ABS(x% - xx%) / 2, 0, colorchosen%
     ELSE
      circl .5 * (xx% + x%), (yy% + y%) / 2, ABS(x% - xx%) / 2, ABS(y% - yy%) / ABS(x% - xx%), colorchosen%
     END IF
    CASE 7:
     GET (0, 8)-(319, 159), saved3%
     xx% = x%: yy% = y%
     DO
      ox% = x%: oy% = y%
      showmouse
      DO
       a% = mousepress%
      LOOP UNTIL (x% <> ox%) OR (y% <> oy%) OR (a% = 0)
      hidemouse
      putall
      IF x% = xx% THEN
       circl .5 * (xx% + x%), (yy% + y%) / 2, ABS(x% - xx%) / 2, ABS(y% - yy%), colorchosen%
      ELSE
       circl .5 * (xx% + x%), (yy% + y%) / 2, ABS(x% - xx%) / 2, ABS(y% - yy%) / ABS(x% - xx%), colorchosen%
      END IF
      LINE (xx%, yy%)-(x%, y%), 7, B, &HAAAA
     LOOP UNTIL mousepress% = 0
     putall
     IF x% = xx% THEN
      circl .5 * (xx% + x%), (yy% + y%) / 2, ABS(x% - xx%) / 2, 0, colorchosen%
     ELSE
      circl .5 * (xx% + x%), (yy% + y%) / 2, ABS(x% - xx%) / 2, ABS(y% - yy%) / ABS(x% - xx%), colorchosen%
     END IF
    CASE 8:
     DO UNTIL mousepress% = 0
      hidemouse
      FOR i% = 1 TO 10
       a# = RND * 6.2831
       x% = x% + SIN(a#) * sprayradius%
       y% = y% + COS(a#) * sprayradius%
       PSET (x%, y%), colorchosen%
      NEXT i%
      showmouse
     LOOP
    CASE 9:
     numbuttons% = 0
     GET (0, 8)-(319, 159), saved3%
     LINE (0, 8)-(319, 159), 0, BF
     puttextxy 5, 12, "Text:", 15
     LINE (55, 10)-(319, 22), 7, BF
     LINE (55, 10)-(319, 22), 15, B
     define 55, 10, 319, 22, CHR$(9)
     button 117, 40, 143, 56, "Ok", 0, CHR$(13)
     button 145, 40, 203, 56, "Cancel", 0, CHR$(27)
     puttextxy 5, 82, "Font:", 15
     LINE (55, 80)-(319, 92), 7, BF
     LINE (55, 80)-(319, 92), 15, B
     putfontxy 57, 82, fontfile$, 15, sysfont$
     define 55, 80, 319, 92, "F"
     text$ = ""
     csron% = 0
     DO
      showmouse
      a$ = getresponse$
      hidemouse
      SELECT CASE a$
       CASE CHR$(9):
        text$ = getinput1$(57, 12, font$, text$)
       CASE "F":
        ffile$ = fontfile$
        fontfile$ = getinput1$(57, 82, sysfont$, fontfile$)
        OPEN fontfile$ + ".fnt" FOR BINARY AS #1
        IF LOF(1) = 0 THEN CLOSE #1: KILL fontfile$: fontfile$ = ffile$ ELSE font$ = INPUT$(LOF(1), 1): CLOSE #1
        LINE (56, 11)-(318, 21), 7, BF
        putfontxy 57, 12, text$, 15, font$
       CASE CHR$(13): writetext% = -1: EXIT DO
       CASE CHR$(27): writetext% = 0: EXIT DO
      END SELECT
     LOOP
     PUT (0, 8), saved3%, PSET
     IF writetext% THEN
      WHILE mousepress%: WEND
      DO
       ox% = x%
       oy% = y%
       DO
        a% = mousepress%
       LOOP UNTIL (ox% <> x%) OR (oy% <> y%) OR (a% <> 0)
       putall
       putfontxy x%, y%, text$, colorchosen%, font$
      LOOP UNTIL mousepress%
      getall
     END IF
     putall
     showmouse
    CASE 10:
     hidemouse
     LINE (x%, y%)-(x% + 15, y% + 15), 0, BF
     LINE (x%, y%)-(x% + 15, y% + 15), 15, B
     DO
      ox% = x%: oy% = y%
      DO
       a% = mousepress%
      LOOP UNTIL (x% <> ox%) OR (y% <> oy%)
      LINE (ox%, oy%)-(ox% + 15, oy% + 15), 0, B
      LINE (x%, y%)-(x% + 15, y% + 15), 0, BF
      LINE (x%, y%)-(x% + 15, y% + 15), 15, B
     LOOP UNTIL mousepress% = 0
     LINE (x%, y%)-(x% + 15, y% + 15), 0, B
     showmouse
    CASE 11:
     hidemouse
     back% = POINT(x%, y%)
     fill x%, y%, colorchosen%, back%
     showmouse
   END SELECT
   getall
   IF toolsandcolorson% THEN
    hidemouse
    drawcolorframe
    showmouse
   END IF
 END SELECT
LOOP

DATA "EXIT","DRAW","PAINT","LINE","SQUARE","BOX","CIRCLE","ELLIPSE","SPRAY"
DATA "TEXT","ERASER","FILL","ZOOM","CLEAR","PICK UP","CUT","COPY","PASTE"
DATA "PALETTE","EFFECTS","SAVE","LOAD","CONFIG"

setflag:
flag% = -1
RESUME NEXT

F10:
saveflag% = -1
GOSUB savepic
RETURN

REM $STATIC
SUB button (x%, y%, x2%, y2%, t$, l%, r$)
LINE (x%, y%)-(x% + 3, y% + 3), 7
LINE -(x% + 3, y2% - 3), 7
LINE -(x%, y2%), 7
LINE -(x%, y%), 7
PAINT (x% + 1, y% + 2), 7, 7
LINE (x%, y%)-(x% + 3, y% + 3), 15
LINE -(x2% - 3, y% + 3), 15
LINE -(x2%, y%), 15
LINE -(x%, y%), 15
PAINT (x% + 3, y% + 2), 15, 15
LINE (x%, y2%)-(x% + 3, y2% - 3), 8
LINE -(x2% - 3, y2% - 3), 8
LINE -(x2% - 3, y% + 3), 8
LINE -(x2%, y%), 8
LINE -(x2%, y2%), 8
LINE -(x%, y2%), 8
PAINT (x2% - 2, y2% - 3), 8, 8
LINE (x%, y%)-(x% + 3, y% + 3), 0
LINE -(x% + 3, y2% - 3), 8
LINE (x2%, y2%)-(x2% - 3, y2% - 3), 0
LINE (x% + 4, y% + 4)-(x2% - 4, y2% - 4), 7, BF
puttextxy x% + 6, y% + 5, t$, 15
IF l% <> 0 THEN
 puttextxy x% + 6 + (l% - 1) * 9, y% + 5, MID$(t$, l%, 1), 8
END IF
LINE (x%, y%)-(x2%, y2%), 0, B
define x% + 2, y% + 2, x2% - 2, y2% - 2, r$
END SUB

SUB clrscr
CLS
numbuttons% = 0
END SUB

SUB colorbar
FOR i% = colorstart% TO colorstart% + 13 STEP 2
 x% = (i% - colorstart%) * 20 + 25
 y% = 170
 PAINT (x%, y%), i% MOD 256, 15
 define x% - 5, 160, x% + 35, 180, CHR$(255) + CHR$(i% - colorstart%)
 y% = 190
 define x% - 5, 181, x% + 35, 199, CHR$(255) + CHR$(i% - colorstart% + 1)
 IF (i% + 1) = 15 THEN PAINT (x%, y%), 31, 15 ELSE PAINT (x%, y%), (i% + 1) MOD 256, 15
NEXT i%
define 0, 160, 20, 199, CHR$(255) + "K"
define 300, 160, 319, 199, CHR$(255) + "M"
END SUB

SUB copy (x1%, y1%, x2%, y2%, buffer%())
buffer%(0, 0) = x2% - x1% + 1
buffer%(0, 1) = y2% - y1% + 1
DEF SEG = &HA000
FOR i% = y1% TO y2%
 FOR j% = x1% TO x2% STEP 2
  buffer%((j% - x1%) / 2 + 1, i% - y1% + 1) = PEEK(i% * 320! + j%) * 256! + PEEK(i% * 320! + j% + 1)
 NEXT j%
 IF (x2% - x1%) MOD 2 = 0 THEN buffer%((x2% - x1%) / 2 + 1, i% - y1% + 1) = PEEK(i% * 320! + j%) * 256!
NEXT i%
DEF SEG
END SUB

SUB define (x%, y%, x2%, y2%, c$)
numbuttons% = numbuttons% + 1
buttons(numbuttons%).selectarea.x = x%
buttons(numbuttons%).selectarea.y = y%
buttons(numbuttons%).selectarea.x2 = x2%
buttons(numbuttons%).selectarea.y2 = y2%
buttons(numbuttons%).selectchar = c$
buttons(numbuttons%).selectcharx = c$
buttons(numbuttons%).l = LEN(c$)
END SUB

SUB fill (xp%, yp%, fc%, bc%)
 SHARED xs%(), ys%(), stk%
 xs%(0) = xp%: ys%(0) = yp%: stk% = 1
 WHILE stk% > 0
  stk% = stk% - 1
  x1% = xs%(stk%): fx1% = 0: y1% = ys%(stk%): fy1% = 0
  x2% = xs%(stk%): fx2% = 0: y2% = ys%(stk%): fy2% = 0
  WHILE (fx1% + fy1% + fx2% + fy2%) <> 4
   IF fx1% = 0 THEN
    IF x1% = 0 THEN
     fx1% = 1
    ELSE
     x1% = x1% - 1
     FOR i% = y1% TO y2%
      nc% = POINT(x1%, i%)
      IF nc% = fc% OR nc% <> bc% THEN fx1% = 1: x1% = x1% + 1: EXIT FOR
     NEXT
    END IF
   END IF
   IF fx2% = 0 THEN
    IF x2% = 320 THEN
     fx2% = 1
    ELSE
     x2% = x2% + 1
     FOR i% = y1% TO y2%
      nc% = POINT(x2%, i%)
      IF nc% = fc% OR nc% <> bc% THEN fx2% = 1: x2% = x2% - 1: EXIT FOR
     NEXT
    END IF
   END IF
   IF fy1% = 0 THEN
    IF y1% = 0 THEN
     fy1% = 1
    ELSE
     y1% = y1% - 1
     FOR i% = x1% TO x2%
      nc% = POINT(i%, y1%)
      IF nc% = fc% OR nc% <> bc% THEN fy1% = 1: y1% = y1% + 1: EXIT FOR
     NEXT
    END IF
   END IF
   IF fy2% = 0 THEN
    IF y2% = 200 THEN
     fy2% = 1
    ELSE
     y2% = y2% + 1
     FOR i% = x1% TO x2%
      nc% = POINT(i%, y2%)
      IF nc% = fc% OR nc% <> bc% THEN fy2% = 1: y2% = y2% - 1: EXIT FOR
     NEXT
    END IF
   END IF
  WEND
  LINE (x1%, y1%)-(x2%, y2%), fc%, BF
  m1% = -1: m2% = -1
  FOR i% = x1% TO x2%
   WHILE stk% >= 1499
    fill xs%(stk% - 1), ys%(stk% - 1), fc%, bc%: stk% = stk% - 1
   WEND
   IF y1% > 0 THEN
    nc% = POINT(i%, y1% - 1)
    IF nc% = bc% AND nc% <> fc% THEN
     IF m1% = -1 THEN m1% = i%
     nc% = POINT(i% + 1, y1% - 1)
     IF i% = x2% OR nc% <> bc% OR nc% = fc% THEN
      xs%(stk%) = (i% - m1%) \ 2 + m1%: ys%(stk%) = y1% - 1: stk% = stk% + 1
      m1% = -1
     END IF
    END IF
   END IF
   IF y2% < 200 THEN
    nc% = POINT(i%, y2% + 1)
    IF nc% = bc% AND nc% <> fc% THEN
     IF m2% = -1 THEN m2% = i%
     nc% = POINT(i% + 1, y2% + 1)
     IF i% = x2% OR nc% <> bc% OR nc% = fc% THEN
      xs%(stk%) = (i% - m2%) \ 2 + m2%: ys%(stk%) = y2% + 1: stk% = stk% + 1
      m2% = -1
     END IF
    END IF
   END IF
  NEXT i%
  m1 = -1: m2 = -1
  FOR i% = y1% TO y2%
   WHILE stk >= 1499
    fill xs%(stk% - 1), ys%(stk% - 1), fc%, bc%: stk% = stk% - 1
   WEND
   IF x1% > 0 THEN
    nc% = POINT(x1% - 1, i%)
    IF nc% = bc% AND nc% <> fc% THEN
     IF m1% = -1 THEN m1% = i%
     nc% = POINT(x1% - 1, i% + 1)
     IF i% = y2% OR nc% <> bc% OR nc% = fc% THEN
      xs%(stk%) = x1% - 1: ys%(stk%) = (i% - m1%) \ 2 + m1%: stk% = stk% + 1
      m1% = -1
     END IF
    END IF
   END IF
   IF x2% < 320 THEN
    nc% = POINT(x2% + 1, i%)
    IF nc% = bc% AND nc% <> fc% THEN
     IF m2% = -1 THEN m2% = i%
     nc% = POINT(x2% + 1, i% + 1)
     IF i% = y2% OR nc% <> bc% OR nc% = fc% THEN
      xs%(stk%) = x2% + 1: ys%(stk%) = (i% - m2%) \ 2 + m2%: stk% = stk% + 1
      m2% = -1
     END IF
    END IF
   END IF
  NEXT i%
 WEND
END SUB

SUB getall
 SHARED c15r%, c15b%, c15g%, c8r%, c8g%, c8b%, c7r%, c7g%, c7b%
 GET (0, 0)-(319, 7), saved1%
 GET (0, 160)-(319, 199), saved2%
 GET (0, 8)-(319, 159), saved3%
 OUT &H3C7, 15: c15r% = INP(&H3C9): c15g% = INP(&H3C9): c15b% = INP(&H3C9)
 OUT &H3C7, 8: c8r% = INP(&H3C9): c8g% = INP(&H3C9): c8b% = INP(&H3C9)
 OUT &H3C7, 7: c7r% = INP(&H3C9): c7g% = INP(&H3C9): c7b% = INP(&H3C9)
 OUT &H3C8, 15: OUT &H3C9, 63: OUT &H3C9, 63: OUT &H3C9, 63
 OUT &H3C8, 8: OUT &H3C9, 21: OUT &H3C9, 21: OUT &H3C9, 21
 OUT &H3C8, 7: OUT &H3C9, 42: OUT &H3C9, 42: OUT &H3C9, 42
END SUB

FUNCTION getinput1$ (x%, y%, f$, text$)
 DO
  DO
   a$ = INKEY$
   IF ABS(st# - TIMER) > .5 THEN
    st# = TIMER
    IF csron% THEN
     csron% = 0
     LINE (x% + LEN(text$) * 8, y%)-(x% + 8 + LEN(text$) * 8, y% + 8), 7, BF
    ELSE
     csron% = -1
     LINE (x% + LEN(text$) * 8, y%)-(x% + 8 + LEN(text$) * 8, y% + 8), 15, BF
    END IF
   END IF
  LOOP UNTIL a$ <> ""
  st# = TIMER - 1
  csron% = 0
  LINE (x% + LEN(text$) * 8, y%)-(x% + 8 + LEN(text$) * 8, y% + 8), 7, BF
  SELECT CASE a$
   CASE CHR$(8):
    IF LEN(text$) > 0 THEN text$ = LEFT$(text$, LEN(text$) - 1)
   CASE CHR$(13):
    EXIT DO
   CASE CHR$(0) TO CHR$(31):
   CASE ELSE:
    IF (LEN(a$) = 1) AND (LEN(text$) < 31) THEN
     putfontxy x% + LEN(text$) * 8, y%, a$, 15, f$
     text$ = text$ + a$
    END IF
  END SELECT
 LOOP
 getinput1$ = text$
END FUNCTION

SUB getlen
 SHARED i%, j%, thiscolor%, listing%(), k%
 k% = k% + 1
 thiscolor% = POINT(j%, i%)
 FOR j% = j% TO 319
  IF POINT(j%, i%) <> thiscolor% THEN EXIT FOR
 NEXT j%
 IF j% = 320 THEN j% = 319
 PRINT #25, CHR$(thiscolor%); MKI$(j% - listing%(k% - 1) - 1);
 IF j% <> 319 THEN
  listing%(k%) = j% - 1
  getlen
 END IF
 listing%(k%) = 0
 k% = k% - 1
END SUB

SUB getregion (x1%, y1%, x2%, y2%)
SHARED saved1%(), saved2%(), saved3%(), x%, y%
hidemouse
GET (0, 8)-(319, 159), saved3%
a% = mousepress%
DO
 ox% = x%
 oy% = y%
 DO
  a% = mousepress%
 LOOP UNTIL (ox% <> x%) OR (oy% <> y%) OR (a% <> 0)
 putall
 LINE (x%, 0)-(x%, 199), 15
 LINE (0, y%)-(319, y%), 15
LOOP UNTIL mousepress% <> 0
x1% = x%
y1% = y%
DO: LOOP UNTIL mousepress% = 0
a% = mousepress%
DO
 ox% = x%
 oy% = y%
 DO
  a% = mousepress%
 LOOP UNTIL (ox% <> x%) OR (oy% <> y%) OR (a% <> 0)
 putall
 LINE (x%, 0)-(x%, 199), 15
 LINE (0, y%)-(319, y%), 15
 LINE (x1%, 0)-(x1%, 199), 15
 LINE (0, y1%)-(319, y1%), 15
LOOP UNTIL mousepress% <> 0
x2% = x%
y2% = y%
DO: LOOP UNTIL mousepress% = 0
showmouse
END SUB

FUNCTION getresponse$
SHARED x%, y%
showmouse
DO
 inregs.ax = 3
 mouseint
 IF outregs.bx = 0 THEN
  a$ = UCASE$(INKEY$)
  IF a$ <> "" THEN
   FOR i% = 1 TO numbuttons%
    IF (buttons(i%).selectchar = a$) OR (buttons(i%).selectcharx = a$) THEN
     getresponse$ = a$
     hidemouse
     EXIT FUNCTION
    END IF
   NEXT i%
  END IF
 ELSEIF (outregs.bx AND 2) = 2 THEN
  getresponse$ = CHR$(27)
  hidemouse
  EXIT FUNCTION
 ELSE
  y% = outregs.dx
  x% = outregs.cx \ 2
  FOR i% = 1 TO numbuttons%
   IF x% >= buttons(i%).selectarea.x THEN
    IF x% <= buttons(i%).selectarea.x2 THEN
     IF y% >= buttons(i%).selectarea.y THEN
      IF y% <= buttons(i%).selectarea.y2 THEN
       IF buttons(i%).l = 1 THEN
        getresponse$ = buttons(i%).selectchar
       ELSE
        getresponse$ = buttons(i%).selectcharx
       END IF
       hidemouse
       EXIT FUNCTION
      END IF
     END IF
    END IF
   END IF
  NEXT i%
 END IF
LOOP
END FUNCTION

SUB hidemouse
inregs.ax = 2
mouseint
END SUB

SUB hidetool (num%) STATIC
 num% = num% + 1
 FOR i% = 1 TO 29
  FOR j% = 1 TO 8
   IF toolbuffer%(num%, i%, j%) = 15 THEN toolbuffer%(num%, i%, j%) = 4
   IF toolbuffer%(num%, i%, j%) = 0 THEN toolbuffer%(num%, i%, j%) = 15
   IF toolbuffer%(num%, i%, j%) = 4 THEN toolbuffer%(num%, i%, j%) = 0
  NEXT j%
 NEXT i%
 num% = num% - 1
END SUB

SUB mouseint
CALL interrupt(&H33, inregs, outregs)
END SUB

FUNCTION mousepress%
SHARED x%, y%
inregs.ax = 3
mouseint
mousepress% = outregs.bx
x% = outregs.cx / 2
y% = outregs.dx
END FUNCTION

SUB paste (x%, y%, buffer%())
DEF SEG = &HA000
FOR i% = y% TO y% + buffer%(0, 1) - 1
 FOR j% = x% TO x% + buffer%(0, 0) - 1 STEP 2
  IF (i% >= 0) AND (i% < 200) THEN
   IF (j% >= 0) AND (j% < 320) THEN
    POKE i% * 320! + j%, (buffer%((j% - x%) / 2 + 1, i% - y% + 1) AND &HFF00) \ 256
    IF (buffer%(0, 0) - 1) MOD 2 = 0 THEN
     IF j% <> x% + buffer%(0, 0) - 1 THEN
      POKE i% * 320! + j% + 1, buffer%((j% - x%) / 2 + 1, i% - y% + 1) AND &HFF
     END IF
    ELSE
     POKE i% * 320! + j% + 1, buffer%((j% - x%) / 2 + 1, i% - y% + 1) AND &HFF
    END IF
   END IF
  END IF
 NEXT j%
NEXT i%
DEF SEG
END SUB

SUB putall
 SHARED c15r%, c15b%, c15g%, c8r%, c8g%, c8b%, c7r%, c7g%, c7b%
 PUT (0, 0), saved1%, PSET
 PUT (0, 160), saved2%, PSET
 PUT (0, 8), saved3%, PSET
 OUT &H3C8, 15: OUT &H3C9, c15r%: OUT &H3C9, c15g%: OUT &H3C9, c15b%
 OUT &H3C8, 8: OUT &H3C9, c8r%: OUT &H3C9, c8g%: OUT &H3C9, c8b%
 OUT &H3C8, 7: OUT &H3C9, c7r%: OUT &H3C9, c7g%: OUT &H3C9, c7b%
END SUB

FUNCTION readfile% (a$)
OPEN a$ FOR BINARY AS #25
IF INPUT$(35, 25) <> "ArCoSoft Compressed Graphics Format" THEN
 CLOSE #1
 readfile% = 1
 EXIT FUNCTION
ELSE
 garbage$ = INPUT$(1, 25)
 gmode% = ASC(INPUT$(1, 25))
 IF gmode% <> 13 THEN
  BEEP
  putall
  EXIT FUNCTION
 END IF
 cols% = ASC(INPUT$(1, 25))
END IF
FOR i% = 0 TO cols%
 OUT &H3C8, i%
 a$ = INPUT$(3, 25)
 OUT &H3C9, ASC(LEFT$(a$, 1))
 OUT &H3C9, ASC(MID$(a$, 2, 1))
 OUT &H3C9, ASC(RIGHT$(a$, 1))
NEXT i%
FOR i% = 0 TO 199
 linecount% = ASC(INPUT$(1, 25))
 IF linecount% = 1 THEN
  j% = 1
  thiscol% = ASC(INPUT$(1, 25))
  PSET (0, i%), thiscol%
  thislen% = CVI(INPUT$(2, 25))
  DO
   LINE (j%, i%)-(j% + thislen% - 1, i%), thiscol%
   j% = j% + thislen%
   thiscol% = ASC(INPUT$(1, 25))
   thislen% = CVI(INPUT$(2, 25))
  LOOP UNTIL thiscol% + thislen% = 0
 ELSEIF linecount% = 2 THEN
  a$ = INPUT$(320, 25)
  FOR j% = 1 TO 320
   PSET (j% - 1, i%), ASC(MID$(a$, j%, 1))
  NEXT j%
 ELSEIF linecount% = 3 THEN
  thiscol% = ASC(INPUT$(1, 25))
  LINE (0, i%)-(319, i%), thiscol%
 ELSE
  LINE (0, i%)-(319, i%), 0
 END IF
NEXT i%
CLOSE #25
END FUNCTION

FUNCTION resetmouse%
inregs.ax = 0
mouseint
IF outregs.ax = 0 THEN resetmouse% = &HFFFF ELSE resetmouse% = 0
END FUNCTION

SUB showmouse
inregs.ax = 1
mouseint
END SUB

SUB showtool (num%) STATIC
 num% = num% + 1
 FOR i% = 1 TO 29
  FOR j% = 1 TO 8
   IF toolbuffer%(num%, i%, j%) = 15 THEN toolbuffer%(num%, i%, j%) = 4
   IF toolbuffer%(num%, i%, j%) = 0 THEN toolbuffer%(num%, i%, j%) = 15
   IF toolbuffer%(num%, i%, j%) = 4 THEN toolbuffer%(num%, i%, j%) = 0
  NEXT j%
 NEXT i%
 num% = num% - 1
END SUB

SUB toolbar
PSET (6, 4), 15
DRAW "ne3nf3"
PSET (314, 4), 15
DRAW "ng3nh3"
FOR i% = toolstart% TO toolstart% + 8
 FOR j% = 1 TO 32
  FOR k% = 1 TO 8
   PSET (j% - 1 + (i% - toolstart%) * 32 + 16, k% - 1), toolbuffer%(i%, j%, k%)
  NEXT k%
 NEXT j%
 IF i% = 23 THEN
  define 32 * (i% - toolstart%) + 16, 0, (i% - toolstart% + 1) * 32 + 15, 7, CHR$(0) + "D"
 ELSE
  define 32 * (i% - toolstart%) + 16, 0, (i% - toolstart% + 1) * 32 + 15, 7, CHR$(254) + CHR$(i%)
 END IF
NEXT i%
define 0, 0, 15, 7, CHR$(252)
define 303, 0, 319, 7, CHR$(253)
END SUB

SUB toolleft
toolstart% = toolstart% - 1
IF toolstart% = 0 THEN
 toolstart% = 1
ELSE
 FOR i% = 1 TO 32
  GET (16, 0)-(302, 7), tempbuffer%
  PUT (17, 0), tempbuffer%, PSET
  FOR j% = 0 TO 7
   PSET (16, j%), toolbuffer%(toolstart%, 33 - i%, j% + 1)
  NEXT j%
 NEXT i%
END IF
END SUB

SUB toolright
toolstart% = toolstart% + 1
IF toolstart% = 16 THEN
 toolstart% = 15
ELSE
 FOR i% = 1 TO 32
  GET (17, 0)-(303, 7), tempbuffer%
  PUT (16, 0), tempbuffer%, PSET
  FOR j% = 0 TO 7
   PSET (303, j%), toolbuffer%(toolstart% + 8, i%, j% + 1)
  NEXT j%
 NEXT i%
END IF
END SUB

SUB writefile (a$)
SHARED i%, j%
OPEN a$ FOR OUTPUT AS #25
PRINT #25, "ArCoSoft Compressed Graphics Format" + CHR$(26) + CHR$(13) + CHR$(255);
FOR i% = 0 TO 255
 OUT &H3C7, i%
 PRINT #25, CHR$(INP(&H3C9)) + CHR$(INP(&H3C9)) + CHR$(INP(&H3C9));
NEXT i%
FOR i% = 0 TO 199
 linecount% = 0
 FOR j% = 0 TO 319
  linecount% = linecount% + ABS(POINT(j%, i%) <> 0)
 NEXT j%
 IF linecount% = 0 THEN
  PRINT #25, CHR$(0);
 ELSE
  thiscolor% = POINT(0, i%)
  difcolors% = 1
  FOR j% = 1 TO 319
   IF POINT(j%, i%) <> thiscolor% THEN
    thiscolor% = POINT(j%, i%)
    difcolors% = difcolors% + 1
   END IF
  NEXT j%
  IF difcolors% = 1 THEN
   PRINT #25, CHR$(3) + CHR$(thiscolor%);
  ELSEIF difcolors% > 107 THEN
   PRINT #25, CHR$(2);
   out$ = ""
   FOR j% = 0 TO 319
    out$ = out$ + CHR$(POINT(j%, i%))
   NEXT j%
   PRINT #25, out$;
  ELSE
   PRINT #25, CHR$(1);
   j% = 0
   getlen
   PRINT #25, CHR$(0) + MKI$(0);
  END IF
 END IF
 LINE (0, i%)-(5, i%), 15
 LINE (314, i%)-(319, i%), 15
NEXT i%
CLOSE #25
END SUB


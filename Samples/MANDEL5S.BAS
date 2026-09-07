DECLARE SUB fixaspect (r1#, i1#, r2#, i2#)
DECLARE SUB setpalette ()
DECLARE SUB dojulia (r1#, i1#, r2#, i2#, jr#, ji#)
DECLARE SUB showmouse ()
DECLARE SUB hidemouse ()
DECLARE SUB domandel (r1#, i1#, r2#, i2#)
DECLARE SUB mouseint ()
TYPE regtype
 ax AS INTEGER
 bx AS INTEGER
 cx AS INTEGER
 dx AS INTEGER
 bp AS INTEGER
 si AS INTEGER
 di AS INTEGER
 flags AS INTEGER
END TYPE
'$DYNAMIC
DIM SHARED regs AS regtype, save&(16001), maxiter%, mode%, method%
IF INSTR(COMMAND$, "/?") THEN
 PRINT "Controls:"
 PRINT
 PRINT " Left-drag:    Define zoom box"
 PRINT " Middle-click: Improve border quality (increases drawing time)"
 PRINT " Right-click:  Exit"
 PRINT " M:            Toggle prediction mode (predictor sucks, but it essentially"
 PRINT "               quarters the drawing time)"
 PRINT " J:            Switch to Julia mode (note: for every dot inside the mandel-"
 PRINT "               brot set, there is a unique connected (ie, all the points"
 PRINT "               touch another point) Julia set; for every dot outside the"
 PRINT "               mandelbrot, there is a unique disconnected Julia set"
 PRINT
 PRINT "Command-line options:"
 PRINT
 PRINT "/2 - third-button emulation - if your mouse has only two buttons, press both"
 PRINT "     buttons simultaneously to simulate a middle-click"
 PRINT "  --mandelbrot variations--"
 PRINT "     default: outside when     |z.r| > 2"
 PRINT "/R - soft: outside when        |atn(z.r)| > 2"
 PRINT "/O - thorns out: outside when  |z.i| > 2"
 PRINT "/S - smoothed: outside when    |atn(z.i)| > 2"
 PRINT
 PRINT "Press any key ..."
 WHILE INKEY$ <> "": WEND
 PRINT
 PRINT "MANDEL5S.EXE - Mandelbrot and Julia set display program by logiclrd"
 END
END IF
IF INSTR(COMMAND$, "/2") THEN twobutton% = -1
IF INSTR(COMMAND$, "/R") THEN method% = 1
IF INSTR(COMMAND$, "/O") THEN method% = 2
IF INSTR(COMMAND$, "/S") THEN method% = 3
regs.ax = 0: mouseint
IF regs.ax = 0 THEN PRINT "Error: this program requires a mouse!"
mode% = 1
maxiter% = 30
r1# = -3
r2# = 1
i1# = -1.5
i2# = 1.5
SCREEN 13
setpalette
regs.ax = 0: mouseint
DO
 IF discard% THEN
  discard% = 0
 ELSE
  IF julia% THEN
   dojulia jr1#, ji1#, jr2#, ji2#, jr#, ji#
  ELSE
   domandel r1#, i1#, r2#, i2#
  END IF
  GET (0, 0)-(319, 199), save&
 END IF
 showmouse
 stage% = 0
 DO
  a$ = INKEY$
  regs.ax = 3: mouseint
  IF twobutton% THEN
   IF regs.bx THEN
    SELECT CASE stage%
     CASE 0:
      st# = TIMER
      stage% = 1
      oldbx% = regs.bx
      IF regs.bx = 1 THEN x1% = regs.cx \ 2: y1% = regs.dx
      regs.bx = 0
     CASE 1:
      IF st# <> TIMER THEN st# = TIMER: stage% = 2
      regs.bx = 0
     CASE 2:
      IF st# = TIMER THEN regs.bx = 0 ELSE IF regs.bx = 0 THEN regs.bx = oldbx%
    END SELECT
   END IF
  END IF
 LOOP UNTIL (regs.bx <> 0) OR (a$ <> "")
 SELECT CASE a$
  CASE "m", "M": hidemouse: mode% = (mode% MOD 2) + 1
  CASE "j", "J":
   IF julia% = 0 THEN
    julia% = 1: discard% = -1
    hidemouse
    LOCATE 1, 1
    PRINT "julia"
   ELSE
    hidemouse
    IF julia% = 1 THEN
     PUT (0, 0), save&, PSET
     discard% = -1
    END IF
    julia% = 0
   END IF
  CASE CHR$(27): hidemouse: EXIT DO
  CASE "":
  CASE ELSE: discard% = -1
 END SELECT
 SELECT CASE regs.bx
  CASE 1:
   IF julia% = 1 THEN
    DO: regs.ax = 3: mouseint: LOOP WHILE regs.bx
    hidemouse
    x% = regs.cx \ 2: y% = regs.dx
    jr# = x% * (r2# - r1#) / 319 + r1#
    ji# = (199 - y%) * (i2# - i1#) / 199 + i1#
    jr1# = -2: ji1# = -1.5
    jr2# = 2: ji2# = 1.5
    julia% = 2
   ELSE
    x1% = regs.cx \ 2: y1% = regs.dx
    DO
     ox% = x%: oy% = y%
     regs.ax = 3: mouseint
     x% = regs.cx \ 2: y% = regs.dx
     IF (ox% <> x%) OR (oy% <> y%) THEN
      hidemouse
      PUT (0, 0), save&, PSET
      LINE (x1%, y1%)-(x%, y%), 15, B
      showmouse
     END IF
    LOOP UNTIL regs.bx = 0
    x2% = regs.cx \ 2: y2% = regs.dx
    hidemouse
    IF julia% THEN
     nr1# = x1% * (jr2# - jr1#) / 319 + jr1#
     ni1# = (199 - y1%) * (ji2# - ji1#) / 199 + ji1#
     nr2# = x2% * (jr2# - jr1#) / 319 + jr1#
     ni2# = (199 - y2%) * (ji2# - ji1#) / 199 + ji1#
     fixaspect nr1#, ni1#, nr2#, ni2#
     jr1# = nr1#: ji1# = ni2#: jr2# = nr2#: ji2# = ni1#
    ELSE
     nr1# = x1% * (r2# - r1#) / 319 + r1#
     ni1# = (199 - y1%) * (i2# - i1#) / 199 + i1#
     nr2# = x2% * (r2# - r1#) / 319 + r1#
     ni2# = (199 - y2%) * (i2# - i1#) / 199 + i1#
     fixaspect nr1#, ni1#, nr2#, ni2#
     r1# = nr1#: i1# = ni2#: r2# = nr2#: i2# = ni1#
    END IF
   END IF
  CASE 2: hidemouse: EXIT DO
  CASE 3, 4: maxiter% = maxiter% + 20: hidemouse
 END SELECT
LOOP
SCREEN 0: WIDTH 80
PRINT "MANDEL5S.EXE - Mandelbrot and Julia set display program by logiclrd"

REM $STATIC
SUB dojulia (r1#, i1#, r2#, i2#, jr#, ji#)
ON LOCAL ERROR RESUME NEXT
dr# = r2# - r1#: di# = i2# - i1#
DEF SEG = &HA000
FOR x% = 0 TO 319 STEP mode%
 FOR y% = 0 TO 199 STEP mode%
  r# = x% * dr# / 319 + r1#: i# = (199 - y%) * di# / 199 + i1#
  FOR j% = 1 TO maxiter%
   nr# = r# * r# - i# * i# + jr#
   IF ERR = 6 THEN EXIT FOR
   i# = 2 * r# * i# + ji#
   IF ERR = 6 THEN EXIT FOR
   r# = nr#
   SELECT CASE method%
    CASE 0: IF ABS(r#) > 2 THEN EXIT FOR
    CASE 1: IF ABS(ATN(r#)) > 2 THEN EXIT FOR
    CASE 2: IF ABS(i#) > 2 THEN EXIT FOR
    CASE 3: IF ABS(ATN(i#)) > 2 THEN EXIT FOR
   END SELECT
  NEXT j%
  ERR = 0
  thisc% = (j% - 1) * 255& \ maxiter%
  o& = y% * 320& + x%
  POKE o&, thisc%
  IF (mode% = 2) THEN
   IF (y% > 0) THEN
    c1% = PEEK(o& - 640)
    c2% = thisc%
    IF c1% < c2% THEN
     POKE o& - 320, c1%
    ELSE
     POKE o& - 320, c2%
    END IF
   END IF
   IF (x% > 0) THEN
    c1% = PEEK(o& - 2)
    c2% = thisc%
    IF c1% < c2% THEN
     POKE o& - 1, c1%
    ELSE
     POKE o& - 1, c2%
    END IF
    IF (y% > 0) THEN
     c1% = PEEK(o& - 322)
     c2% = PEEK(o& - 320)
     c3% = PEEK(o& - 641)
     c4% = PEEK(o& - 1)
     n1% = 0: n2% = 0: n3% = 0: n4% = 0
     IF c1% = c2% THEN n1% = 1
     IF c1% = c3% THEN n1% = n1% + 1
     IF c1% = c4% THEN n1% = n1% + 1
     IF c2% = c1% THEN n2% = 1
     IF c2% = c3% THEN n2% = n2% + 1
     IF c2% = c4% THEN n2% = n2% + 1
     IF c3% = c1% THEN n3% = 1
     IF c3% = c2% THEN n3% = n3% + 1
     IF c3% = c4% THEN n3% = n3% + 1
     IF c4% = c1% THEN n4% = 1
     IF c4% = c2% THEN n4% = n4% + 1
     IF c4% = c3% THEN n4% = n4% + 1
     d1% = 1: d2% = 2: d3% = 3: d4% = 4
     IF n1% < n2% THEN SWAP n1%, n2%: SWAP d1%, d2%
     IF n2% < n3% THEN SWAP n2%, n3%: SWAP d2%, d3%
     IF n3% < n4% THEN SWAP n3%, n4%: SWAP d3%, d4%
     IF n2% < n3% THEN SWAP n2%, n3%: SWAP d2%, d3%
     IF n1% < n2% THEN SWAP n1%, n2%: SWAP d1%, d2%
     IF n2% < n3% THEN SWAP n2%, n3%: SWAP d2%, d3%
     SELECT CASE d1%
      CASE 1: POKE o& - 321, c1%
      CASE 2: POKE o& - 321, c2%
      CASE 3: POKE o& - 321, c3%
      CASE 4: POKE o& - 321, c4%
     END SELECT
    END IF
   END IF
  END IF
 NEXT y%
NEXT x%
END SUB

SUB domandel (r1#, i1#, r2#, i2#)
ON LOCAL ERROR RESUME NEXT
dr# = r2# - r1#: di# = i2# - i1#
DEF SEG = &HA000
FOR x% = 0 TO 319 STEP mode%
 FOR y% = 0 TO 199 STEP mode%
  orgr# = x% * dr# / 319 + r1#
  orgi# = (199 - y%) * di# / 199 + i1#
  r# = orgr#: i# = orgi#
  FOR j% = 1 TO maxiter%
   nr# = r# * r# - i# * i# + orgr#
   IF ERR = 6 THEN EXIT FOR
   i# = 2 * r# * i# + orgi#
   IF ERR = 6 THEN EXIT FOR
   r# = nr#
   SELECT CASE method%
    CASE 0: IF ABS(r#) > 2 THEN EXIT FOR
    CASE 1: IF ABS(ATN(r#)) > 2 THEN EXIT FOR
    CASE 2: IF ABS(i#) > 2 THEN EXIT FOR
    CASE 3: IF ABS(ATN(i#)) > 2 THEN EXIT FOR
   END SELECT
  NEXT j%
  ERR = 0
  thisc% = (j% - 1) * 255& \ maxiter%
  o& = y% * 320& + x%
  POKE o&, thisc%
  IF (mode% = 2) THEN
   IF (y% > 0) THEN
    c1% = PEEK(o& - 640)
    c2% = thisc%
    IF c1% < c2% THEN
     POKE o& - 320, c1%
    ELSE
     POKE o& - 320, c2%
    END IF
   END IF
   IF (x% > 0) THEN
    c1% = PEEK(o& - 2)
    c2% = thisc%
    IF c1% < c2% THEN
     POKE o& - 1, c1%
    ELSE
     POKE o& - 1, c2%
    END IF
    IF (y% > 0) THEN
     c1% = PEEK(o& - 322)
     c2% = PEEK(o& - 320)
     c3% = PEEK(o& - 641)
     c4% = PEEK(o& - 1)
     n1% = 0: n2% = 0: n3% = 0: n4% = 0
     IF c1% = c2% THEN n1% = 1
     IF c1% = c3% THEN n1% = n1% + 1
     IF c1% = c4% THEN n1% = n1% + 1
     IF c2% = c1% THEN n2% = 1
     IF c2% = c3% THEN n2% = n2% + 1
     IF c2% = c4% THEN n2% = n2% + 1
     IF c3% = c1% THEN n3% = 1
     IF c3% = c2% THEN n3% = n3% + 1
     IF c3% = c4% THEN n3% = n3% + 1
     IF c4% = c1% THEN n4% = 1
     IF c4% = c2% THEN n4% = n4% + 1
     IF c4% = c3% THEN n4% = n4% + 1
     d1% = 1: d2% = 2: d3% = 3: d4% = 4
     IF n1% < n2% THEN SWAP n1%, n2%: SWAP d1%, d2%
     IF n2% < n3% THEN SWAP n2%, n3%: SWAP d2%, d3%
     IF n3% < n4% THEN SWAP n3%, n4%: SWAP d3%, d4%
     IF n2% < n3% THEN SWAP n2%, n3%: SWAP d2%, d3%
     IF n1% < n2% THEN SWAP n1%, n2%: SWAP d1%, d2%
     IF n2% < n3% THEN SWAP n2%, n3%: SWAP d2%, d3%
     SELECT CASE d1%
      CASE 1: POKE o& - 321, c1%
      CASE 2: POKE o& - 321, c2%
      CASE 3: POKE o& - 321, c3%
      CASE 4: POKE o& - 321, c4%
     END SELECT
    END IF
   END IF
  END IF
 NEXT y%
NEXT x%
END SUB

SUB fixaspect (r1#, i1#, r2#, i2#)
IF r1# > r2# THEN SWAP r1#, r2#
IF i1# < i2# THEN SWAP i1#, i2#

cr# = (r1# + r2#) * .5
ci# = (i1# + i2#) * .5

dr# = r2# - r1#
di# = i1# - i2#

area# = dr# * di#

' Initial range:
' - Mandelbrot: (-3 - 1.5i) to (1 + 1.5i)
' - Julia: (-2 - 1.5i) to (2 + 1.5i)
'
' In both cases, the aspect ratio is 4:3.

square# = SQR(4 * 3)
deviation# = 4 / square#

fixeddr# = SQR(area#) * deviation#
fixeddi# = SQR(area#) / deviation#

r1# = cr# - fixeddr# * .5
r2# = cr# + fixeddr# * .5

i1# = ci# + fixeddi# * .5
i2# = ci# - fixeddi# * .5
END SUB

SUB hidemouse
regs.ax = 2
mouseint
END SUB

SUB mouseint
CALL interrupt(&H33, regs, regs)
END SUB

SUB setpalette
FOR i% = 1 TO 255
 OUT &H3C8, i%
 OUT &H3C9, SIN(i% / 255 * 6.2831) * 31 + 32
 OUT &H3C9, SIN((i% + 85) / 255 * 6.2831) * 31 + 32
 OUT &H3C9, SIN((i% + 170) / 255 * 6.2831) * 31 + 32
NEXT i%
END SUB

SUB showmouse
regs.ax = 1
mouseint
END SUB

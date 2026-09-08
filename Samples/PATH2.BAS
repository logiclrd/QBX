DECLARE SUB unsetwindow ()
DECLARE SUB dosleep (s%)
DECLARE SUB setwindow ()
DECLARE SUB processkeys ()
DECLARE SUB plot (x%, y%, c%)
DECLARE SUB plotlineto (x%, y%, c%)
DECLARE SUB order (a$)
DECLARE SUB process (a$)
DECLARE SUB optimize ()
DECLARE SUB optimize2 ()
DECLARE SUB dooptimize2 (n1%, n2%, n3%)
DECLARE SUB deletenode (n%)
DECLARE FUNCTION newnode% (n1%, n2%)
DECLARE FUNCTION solve% (n1%, n2%)
DECLARE FUNCTION checkline% (n1%, n2%)
'$DYNAMIC
DIM SHARED map%(100, 100), tmap%(101, 101, 1)
ON ERROR GOTO catch
TYPE nodetype
 x AS INTEGER
 y AS INTEGER
 next AS INTEGER
 prev AS INTEGER
 a AS INTEGER
END TYPE
DIM SHARED node(150) AS nodetype, display%
SCREEN 13
RANDOMIZE TIMER
DO
 CLS
 setwindow
 FOR i% = 0 TO 100
  FOR j% = 0 TO 100
   tmap%(i%, j%, 0) = 0
  NEXT j%
 NEXT i%
 FOR i% = 1 TO 25
  tmap%(INT(RND * 102), INT(RND * 102), 0) = 1
 NEXT i%
 a% = 0: v% = 1
 FOR k% = 1 TO 20
  a% = v%
  v% = v% XOR 1
  FOR i% = 1 TO 100
   FOR j% = 1 TO 100
    tmap%(i%, j%, a%) = tmap%(i%, j%, v%)
    IF tmap%(i% - 1, j%, v%) OR tmap%(i% + 1, j%, v%) OR tmap%(i%, j% - 1, v%) OR tmap%(i%, j% + 1, v%) THEN
     IF INT(RND * 3) = 1 THEN tmap%(i%, j%, a%) = 1
    END IF
   NEXT j%
  NEXT i%
  FOR i% = 1 TO 100
   FOR j% = 1 TO 100
    map%(i%, j%) = tmap%(i%, j%, a%)
    PSET (i%, j%), map%(i%, j%)
   NEXT j%
  NEXT i%
 NEXT k%
 order "select start & end points"
 DO
  fromx% = INT(RND * 100) + 1
  fromy% = INT(RND * 100) + 1
 LOOP WHILE map%(fromx%, fromy%)
 DO
  tox% = INT(RND * 100) + 1
  toy% = INT(RND * 100) + 1
 LOOP WHILE map%(tox%, toy%)
 FOR i% = 1 TO 3
  PSET (fromx%, fromy%), 15
  PSET (tox%, toy%), 15
  st# = TIMER: WHILE TIMER = st#: WEND
  st# = TIMER: WHILE TIMER = st#: WEND
  st# = TIMER: WHILE TIMER = st#: WEND
  PSET (fromx%, fromy%), 4
  PSET (tox%, toy%), 4
  st# = TIMER: WHILE TIMER = st#: WEND
  st# = TIMER: WHILE TIMER = st#: WEND
  st# = TIMER: WHILE TIMER = st#: WEND
 NEXT i%
 order "path finding"
 FOR i% = 0 TO 150
  node(i%).a = 0
 NEXT i%
 node(1).a = -1: node(1).x = fromx%: node(1).y = fromy%
 node(2).a = -1: node(2).x = tox%: node(2).y = toy%
 node(1).next = 2: node(2).prev = 1
 IF solve%(1, 2) THEN
  IF display% THEN
   PSET (node(1).x, node(1).y), 4
   i% = node(1).next
   DO WHILE i%
    LINE -(node(i%).x, node(i%).y), 4
    i% = node(i%).next
   LOOP
   order "optimize (1/3)"
  END IF
  optimize2
  IF display% THEN
   PSET (node(1).x, node(1).y), 5
   i% = node(1).next
   DO WHILE i%
    LINE -(node(i%).x, node(i%).y), 5
    i% = node(i%).next
   LOOP
   order "optimize (2/3)"
  END IF
  optimize
  IF display% THEN
   PSET (node(1).x, node(1).y), 13
   i% = node(1).next
   DO WHILE i%
    LINE -(node(i%).x, node(i%).y), 13
    i% = node(i%).next
   LOOP
   order "optimize (3/3)"
  END IF
  optimize2
  optimize
  PSET (node(1).x, node(1).y), 15
  i% = node(1).next
  count% = 0
  DO WHILE i%
   LINE -(node(i%).x, node(i%).y), 15
   i% = node(i%).next
   count% = count% + 1
  LOOP
  unsetwindow
  LOCATE 21, 1
  PRINT "solution is"; count%; "nodes long"
 ELSE
  unsetwindow
  LOCATE 21, 1
  PRINT "no solution"
 END IF
 dosleep 4
LOOP
SCREEN 0: WIDTH 80
END

catch:
LOCATE 25, 1
PRINT "runtime error! press a key to restart";
SLEEP: processkeys
RUN
END

REM $STATIC
FUNCTION checkline% (n1%, n2%)
x1% = node(n1%).x
y1% = node(n1%).y
x2% = node(n2%).x
y2% = node(n2%).y
adx% = ABS(x2% - x1%)
ady% = ABS(y2% - y1%)
IF (adx% = 0) AND (ady% = 0) THEN checkline% = -1: EXIT FUNCTION
IF adx% > ady% THEN
 IF x1% > x2% THEN
  SWAP x1%, x2%
  SWAP y1%, y2%
 END IF
 dx% = x2% - x1%
 dy% = y2% - y1%
 oy% = y1%
 FOR i% = x1% TO x2%
  ty% = (i% - x1%) * dy% \ dx% + y1%
  IF ty% <> oy% THEN
   IF map%(i%, oy%) THEN EXIT FUNCTION
   oy% = ty%
  END IF
  IF map%(i%, ty%) THEN EXIT FUNCTION
 NEXT i%
ELSE
 IF y1% > y2% THEN
  SWAP x1%, x2%
  SWAP y1%, y2%
 END IF
 dx% = x2% - x1%
 dy% = y2% - y1%
 ox% = x1%
 FOR i% = y1% TO y2%
  tx% = (i% - y1%) * dx% \ dy% + x1%
  IF tx% <> ox% THEN
   IF map%(ox%, i%) THEN
    IF map%(tx%, i% - 1) THEN
     EXIT FUNCTION
    END IF
   END IF
   ox% = tx%
  END IF
  IF map%(tx%, i%) THEN EXIT FUNCTION
 NEXT i%
END IF
checkline% = -1
END FUNCTION

SUB deletenode (n%)
IF node(n%).prev THEN node(node(n%).prev).next = node(n%).next
IF node(n%).next THEN node(node(n%).next).prev = node(n%).prev
node(n%).a = 0
END SUB

SUB dooptimize2 (n1%, n2%, n3%)
updated% = 0
c% = 0
x1% = node(n1%).x: y1% = node(n1%).y
x2% = node(n2%).x: y2% = node(n2%).y
x3% = node(n3%).x: y3% = node(n3%).y
IF checkline%(n1%, n3%) THEN
 EXIT SUB
END IF
x1% = x1% - SGN(x2% - x1%): y1% = y1% - SGN(y2% - y1%)
x3% = x3% - SGN(x2% - x3%): y3% = y3% - SGN(y2% - y3%)
count% = 0
DO
 dx% = x2% - x1%: dy% = y2% - y1%
 sx% = SGN(dx%): sy% = SGN(dy%)
 updated% = 0
 IF sx% = 0 THEN
  FOR i% = y1% + sy% TO y2% - sy% STEP sy%
   node(n2%).y = i%
   IF checkline%(n2%, n3%) THEN updated% = -1: EXIT FOR
  NEXT i%
  IF updated% THEN y2% = node(n2%).y ELSE node(n2%).y = y2%
 ELSEIF sy% = 0 THEN
  FOR i% = x1% + sx% TO x2% - sx% STEP sx%
   node(n2%).x = i%
   IF checkline%(n2%, n3%) THEN updated% = -1: EXIT FOR
  NEXT i%
  IF updated% THEN x2% = node(n2%).x ELSE node(n2%).x = x2%
 ELSE
  IF ABS(dx%) > ABS(dy%) THEN
   FOR i% = x1% + sx% TO x2% - sx% STEP sx%
    node(n2%).x = i%
    node(n2%).y = (i% - x1%) * dy% \ dx% + y1%
    IF checkline%(n3%, n2%) THEN updated% = -1: EXIT FOR
   NEXT i%
  ELSE
   FOR i% = y1% + sy% TO y2% - sx% STEP sy%
    node(n2%).x = (i% - y1%) * dx% \ dy% + x1%
    node(n2%).y = i%
    IF checkline%(n3%, n2%) THEN updated% = -1: EXIT FOR
   NEXT i%
  END IF
  IF updated% THEN
   x2% = node(n2%).x
   y2% = node(n2%).y
  ELSE
   node(n2%).x = x2%
   node(n2%).y = y2%
  END IF
 END IF
 c% = c% XOR 1
 SWAP n1%, n3%
 SWAP x1%, x3%
 SWAP y1%, y3%
 count% = count% + 1
 IF count% = 6 THEN EXIT DO
LOOP WHILE updated% OR (c% = 1)
END SUB

SUB dosleep (s%)
SLEEP s%
processkeys
END SUB

FUNCTION newnode% (n1%, n2%)
FOR i% = 1 TO 150
 IF node(i%).a = 0 THEN
  node(i%).a = -1
  node(i%).x = (node(n1%).x + node(n2%).x) \ 2
  node(i%).y = (node(n1%).y + node(n2%).y) \ 2
  node(i%).next = n2%
  node(i%).prev = n1%
  node(n1%).next = i%
  node(n2%).prev = i%
  newnode% = i%
  EXIT FOR
 END IF
NEXT i%
END FUNCTION

SUB optimize
n% = 1
DO WHILE node(node(n%).next).next
 IF checkline(n%, node(node(n%).next).next) THEN
  x% = node(n%).next
  deletenode x%
 ELSE
  n% = node(n%).next
 END IF
LOOP
END SUB

SUB optimize2
n% = 1
DO WHILE node(node(n%).next).next
 dooptimize2 n%, node(n%).next, node(node(n%).next).next
 n% = node(n%).next
LOOP
END SUB

SUB order (a$)
unsetwindow

LOCATE 21, 1
PRINT SPACE$(40);
LOCATE 21, 1
PRINT a$

SLEEP 1
processkeys

LOCATE 21, 1
PRINT SPACE$(40)

setwindow
END SUB

SUB processkeys ()
DO
 a$ = INKEY$
 SELECT CASE a$
  CASE "d", "D": display% = display% XOR -1
  CASE "":
  CASE ELSE: SCREEN 0: WIDTH 80: END
 END SELECT
LOOP WHILE LEN(a$)
END SUB

SUB setwindow
VIEW (160 - 50, 100 - 50)-(160 + 50, 100 + 50)
END SUB

FUNCTION solve% (n1%, n2%)
DIM pointx%(201, 1), pointy%(201, 1)
DIM strength%(100, 100)
FOR i% = 1 TO 100
 FOR j% = 1 TO 100
  IF map%(i%, j%) THEN strength%(i%, j%) = 32767
 NEXT j%
NEXT i%
nextleft% = 1
x1% = node(n1%).x: y1% = node(n1%).y
x2% = node(n2%).x: y2% = node(n2%).y
pointx%(1, 0) = x1%
pointy%(1, 0) = y1%
strength%(x1%, y1%) = 1
s% = 1
v% = 1
DO WHILE nextleft%
 left% = nextleft%
 s% = s% + 1
 nextleft% = 0
 SWAP a%, v%
 FOR i% = 1 TO left%
  px% = pointx%(i%, v%)
  py% = pointy%(i%, v%)
  IF px% > 1 THEN
   IF strength%(px% - 1, py%) = 0 THEN
    strength%(px% - 1, py%) = s%
    IF ((px% - 1) = x2%) AND (py% = y2%) THEN
     goodness% = -1
     nextleft% = 0
     EXIT FOR
    END IF
    nextleft% = nextleft% + 1
    pointx%(nextleft%, a%) = px% - 1
    pointy%(nextleft%, a%) = py%
   END IF
  END IF
  IF px% < 100 THEN
   IF strength%(px% + 1, py%) = 0 THEN
    strength%(px% + 1, py%) = s%
    IF ((px% + 1) = x2%) AND (py% = y2%) THEN
     goodness% = -1
     nextleft% = 0
     EXIT FOR
    END IF
    nextleft% = nextleft% + 1
    pointx%(nextleft%, a%) = px% + 1
    pointy%(nextleft%, a%) = py%
   END IF
  END IF
  IF py% > 1 THEN
   IF strength%(px%, py% - 1) = 0 THEN
    strength%(px%, py% - 1) = s%
    IF (px% = x2%) AND ((py% - 1) = y2%) THEN
     goodness% = -1
     nextleft% = 0
     EXIT FOR
    END IF
    nextleft% = nextleft% + 1
    pointx%(nextleft%, a%) = px%
    pointy%(nextleft%, a%) = py% - 1
   END IF
  END IF
  IF py% < 100 THEN
   IF strength%(px%, py% + 1) = 0 THEN
    strength%(px%, py% + 1) = s%
    IF (px% = x2%) AND ((py% + 1) = y2%) THEN
     goodness% = -1
     nextleft% = 0
     EXIT FOR
    END IF
    nextleft% = nextleft% + 1
    pointx%(nextleft%, a%) = px%
    pointy%(nextleft%, a%) = py% + 1
   END IF
  END IF
 NEXT i%
 IF display% THEN
  FOR i% = 1 TO 100
   FOR j% = 1 TO 100
    SELECT CASE strength%(i%, j%)
     CASE 32767: PSET (i%, j%), 1
     CASE 0:
     CASE ELSE: PSET (i%, j%), 14
    END SELECT
   NEXT j%
  NEXT i%
 END IF
LOOP
IF display% THEN dosleep 1
IF goodness% THEN
 IF display% THEN
  FOR i% = 1 TO s%
   it% = i% * 63 \ s%
   OUT &H3C8, i%
   OUT &H3C9, it%
   OUT &H3C9, it%
   OUT &H3C9, it%
  NEXT i%
  FOR i% = 1 TO 100
   FOR j% = 1 TO 100
    IF strength%(i%, j%) = 0 THEN strength%(i%, j%) = 32767
    SELECT CASE strength%(i%, j%)
     CASE 32767: PSET (i%, j%), s%
     CASE 0:
     CASE ELSE: PSET (i%, j%), strength%(i%, j%)
    END SELECT
   NEXT j%
  NEXT i%
  dosleep 2
  PALETTE
  FOR i% = 1 TO 100
   FOR j% = 1 TO 100
    PSET (i%, j%), map%(i%, j%)
   NEXT j%
  NEXT i%
  PALETTE
 ELSE
  FOR i% = 1 TO 100
   FOR j% = 1 TO 100
    IF strength%(i%, j%) = 0 THEN strength%(i%, j%) = 32767
   NEXT j%
  NEXT i%
 END IF
 n3% = n2%
 lastdir% = 0
 findnext% = -1
 tx% = x2%
 ty% = y2%
 DO
  IF display% THEN PSET (tx%, ty%), 8
  SELECT CASE lastdir%
   CASE 1:
    IF tx% > 1 THEN
     IF strength%(tx% - 1, ty%) < strength%(tx%, ty%) THEN
      tx% = tx% - 1
     ELSE
      n3% = newnode%(n1%, n3%)
      node(n3%).x = tx%
      node(n3%).y = ty%
      findnext% = -1
     END IF
    ELSE
     findnext% = -1
    END IF
   CASE 2:
    IF tx% < 100 THEN
     IF strength%(tx% + 1, ty%) < strength%(tx%, ty%) THEN
      tx% = tx% + 1
     ELSE
      n3% = newnode%(n1%, n3%)
      node(n3%).x = tx%
      node(n3%).y = ty%
      findnext% = -1
     END IF
    ELSE
     findnext% = -1
    END IF
   CASE 3:
    IF ty% > 1 THEN
     IF strength%(tx%, ty% - 1) < strength%(tx%, ty%) THEN
      ty% = ty% - 1
     ELSE
      n3% = newnode%(n1%, n3%)
      node(n3%).x = tx%
      node(n3%).y = ty%
      findnext% = -1
     END IF
    ELSE
     findnext% = -1
    END IF
   CASE 4:
    IF ty% < 100 THEN
     IF strength%(tx%, ty% + 1) < strength%(tx%, ty%) THEN
      ty% = ty% + 1
     ELSE
      n3% = newnode%(n1%, n3%)
      node(n3%).x = tx%
      node(n3%).y = ty%
      findnext% = -1
     END IF
    ELSE
     findnext% = -1
    END IF
  END SELECT
  IF findnext% THEN
   IF tx% > 1 THEN
    IF strength%(tx% - 1, ty%) < strength%(tx%, ty%) THEN
     tx% = tx% - 1
     lastdir% = 1
     findnext% = 0
    END IF
   END IF
   IF findnext% AND (tx% < 100) THEN
    IF strength%(tx% + 1, ty%) < strength%(tx%, ty%) THEN
     tx% = tx% + 1
     lastdir% = 2
     findnext% = 0
    END IF
   END IF
   IF findnext% AND (ty% > 1) THEN
    IF strength%(tx%, ty% - 1) < strength%(tx%, ty%) THEN
     ty% = ty% - 1
     lastdir% = 3
     findnext% = 0
    END IF
   END IF
   IF findnext% AND (ty% < 100) THEN
    IF strength%(tx%, ty% + 1) < strength%(tx%, ty%) THEN
     ty% = ty% + 1
     lastdir% = 4
     findnext% = 0
    END IF
   END IF
   IF findnext% THEN EXIT DO
  END IF
 LOOP UNTIL findnext% OR ((tx% = x1%) AND (ty% = y1%))
 IF display% THEN dosleep 3
 solve% = findnext% XOR -1
END IF
END FUNCTION

SUB unsetwindow
VIEW
END SUB

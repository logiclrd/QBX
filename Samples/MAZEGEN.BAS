'$DYNAMIC
DECLARE SUB MakeMaze (pjmaze%(), pjexact%)
DECLARE SUB SolveMaze (pjmaze%(), startx%, starty%, endx%, endy%)
DECLARE SUB DrawMaze (maze%(), startx%, starty%, endx%, endy%)
DECLARE SUB DrawText (t$, x%, y%)

''__''__''__''__''__''__''__''__''__''__''__''__''__''__''__''__''__''__''
''                                                                      ''
'' Project: Random Maze Generator Function for QuickBASIC               ''
''          Pathfinding Function for QuickBASIC                         ''
''                                                                      ''
'' Ported from GLBasic code written by Andrew Canham                    ''
''                                                                      ''
''__''__''__''__''__''__''__''__''__''__''__''__''__''__''__''__''__''__''

TYPE RegTypeX
  AX AS INTEGER
  BX AS INTEGER
  CX AS INTEGER
  DX AS INTEGER
  BP AS INTEGER
  SI AS INTEGER
  DI AS INTEGER
  FLAGS AS INTEGER
  DS AS INTEGER
  ES AS INTEGER
END TYPE

DECLARE SUB InterruptX (interrupt%, rin AS RegTypeX, rout AS RegTypeX)

SCREEN 12

' Set some palette entries so we can fake an alpha overlay.
DIM calpha%(15)

' calpha%(USED COLOUR) = UNUSED COLOUR

calpha%(0) = 5
calpha%(1) = 6
calpha%(2) = 7
calpha%(4) = 8
calpha%(14) = 9

FOR i% = 0 TO 15
  IF calpha%(i%) <> 0 THEN
    OUT &H3C7, i%
    r% = INP(&H3C9)
    g% = INP(&H3C9)
    b% = INP(&H3C9)

    OUT &H3C8, calpha%(i%)
    OUT &H3C9, (r% * 3 + 32) \ 4
    OUT &H3C9, (g% * 3 + 32) \ 4
    OUT &H3C9, (b% * 3 + 32) \ 4
  END IF
NEXT i%

' maze dimensions
CONST mazex% = 159
CONST mazey% = 119

DIM maze%(mazex%, mazey%)

' start point in passage
startx% = 1
starty% = 117

' end point in wall
endx% = 156
endy% = 2

DO
  ERASE maze% ' Reset maze
  REDIM maze%(mazex%, mazey%)

  time1! = TIMER

  MakeMaze maze%(), 1

  time2! = TIMER

  SolveMaze maze%(), startx%, starty%, endx%, endy%

  time3! = TIMER

  DrawMaze maze%(), startx%, starty%, endx%, endy%

  FOR y% = 0 TO 44
    FOR x% = 0 TO 259
      c% = POINT(x%, y%)
      IF calpha%(c%) THEN PSET (x%, y%), calpha%(c%)
    NEXT x%
  NEXT y%

  COLOR 15

  DrawText "Generated in " + LTRIM$(RTRIM$(STR$(INT((time2! - time1!) * 1000)))) + " milliseconds", 10, 10
  DrawText "Solved in " + LTRIM$(RTRIM$(STR$(INT((time3! - time2!) * 1000)))) + " milliseconds", 10, 20
  DrawText "Key to Regenerate, Esc Quits", 10, 30

  DO
    k$ = INKEY$
  LOOP WHILE k$ = ""
LOOP WHILE k$ <> CHR$(27)

END


SUB DrawMaze (maze%(), startx%, starty%, endx%, endy%)
mx% = UBOUND(maze%(), 1)
my% = UBOUND(maze%(), 2)

FOR x% = 0 TO mx% - 1
  FOR y% = 0 TO my% - 1
    SELECT CASE maze%(x%, y%)
      CASE 0: COLOR 0
      CASE 1: COLOR 14 ' yellow path (solution)
      CASE 2: COLOR 4  ' red walls
    END SELECT

    LINE (4 + x% * 4, 4 + y% * 4)-STEP(3, 3), , BF
  NEXT y%
NEXT x%

LINE (4 + startx% * 4, 4 + starty% * 4)-STEP(4, 4), 2, BF ' green start point
LINE (4 + endx% * 4, 4 + endy% * 4)-STEP(4, 4), 1, BF ' blue end point
END SUB

SUB DrawText (t$, x%, y%)
DIM rin AS RegTypeX, rout AS RegTypeX

rin.AX = &H1130 ' get pointer to font
rin.BX = &H300 ' 8x8 font

CALL InterruptX(&H10, rin, rout)

DEF SEG = rout.ES

FOR i% = 1 TO LEN(t$)
  ch% = ASC(MID$(t$, i%, 1))

  ofs% = ch% * 8

  FOR yy% = 0 TO 7
    bits% = PEEK(rout.BP + ofs% + yy%)
    bit% = 1
    FOR xx% = 0 TO 7
      IF bits% AND bit% THEN PSET (x% + 7 - xx%, y% + yy%)
      bit% = bit% + bit%
    NEXT xx%
  NEXT yy%

  x% = x% + 8
NEXT i%
END SUB

SUB MakeMaze (pjmaze%(), pjexact%)
pjmx% = UBOUND(pjmaze%, 1) - 1
pjmy% = UBOUND(pjmaze%, 2) - 1

DIM pjcos%(4), pjsin%(4)

pjcos%(0) = 1
pjcos%(2) = -1
pjsin%(1) = 1
pjsin%(3) = -1

FOR pjfx% = 0 TO pjmx%
  pjmaze%(pjfx%, 0) = 2
  pjmaze%(pjfx%, pjmy%) = 2
NEXT

FOR pjfy% = 0 TO pjmy%
  pjmaze%(0, pjfy%) = 2
  pjmaze%(pjmx%, pjfy%) = 2
NEXT

FOR pjloop% = 1 TO pjmy% \ 6
  pjypos% = INT(RND * (1 + (pjmy% - 6) / 2)) * 2
  pjxpos% = INT(RND * 3) * 2

  FOR pjfx% = 1 TO pjxpos%
    pjmaze%(pjfx%, pjypos%) = 2
  NEXT

  pjypos% = INT(RND * (1 + (pjmy% - 6) / 2)) * 2
  pjxpos% = pjmx% - INT(RND * 3) * 2
  FOR pjfx% = pjmx% TO pjxpos% STEP -1
    pjmaze%(pjfx%, pjypos%) = 2
  NEXT
NEXT

FOR pjloop% = 1 TO pjmx% \ 6
  pjxpos% = 4 + INT(RND * (1 + (pjmx% - 8) / 2)) * 2

  pjmaze%(pjxpos%, pjmy% - 1) = 2
  pjmaze%(pjxpos%, pjmy% - 2) = 2
NEXT

pjfarx% = pjmx%
pjfary% = pjmy%
pjallx% = 0
pjally% = 0
pjexact% = pjexact% + 1

DO
  DO
    pjallx% = pjallx% + pjcos%(pjalldir%) * pjexact%
    pjally% = pjally% + pjsin%(pjalldir%) * pjexact%

    pjgo% = pjgo% + pjexact%

    IF pjgo% >= pjfarx% - pjexact% AND (pjalldir% = 0 OR pjalldir% = 2) THEN
      pjgo% = 0
      pjfarx% = pjfarx% - pjexact%
      pjtemp% = pjalldir% + 1
      pjalldir% = pjtemp% + 4 * (pjtemp% > 3) - 4 * (pjtemp% < 0)
    END IF

    IF pjgo% >= pjfary% - pjexact% AND (pjalldir% = 1 OR pjalldir% = 3) THEN
      pjgo% = 0
      pjfary% = pjfary% - pjexact%
      pjtemp% = pjalldir% + 1
      pjalldir% = pjtemp% + 4 * (pjtemp% > 3) - 4 * (pjtemp% < 0)
    END IF

    IF pjfarx% <= 0 OR pjfary% <= 0 THEN EXIT DO

    IF pjmaze%(pjallx%, pjally%) = 2 AND (pjallx% < pjmx% - 1 OR pjally% < pjmy% - 1) THEN EXIT DO
  LOOP

  IF pjfarx% <= 0 OR pjfary% <= 0 THEN EXIT DO
  IF pjallx% >= pjmx% OR pjally% >= pjmy% THEN EXIT DO

  pjx% = pjallx%
  pjy% = pjally%
  pjdir% = INT(RND * 4)
  DO
    FOR pjloop% = 0 TO 3
      pjresult% = 1
      FOR pjlop% = 1 TO pjexact% + 1
        pjx2% = pjx% + pjcos%(pjloop%) * pjlop%
        pjy2% = pjy% + pjsin%(pjloop%) * pjlop%

        IF pjx2% <= 0 OR pjy2% <= 0 OR pjx2% > pjmx% OR pjy2% > pjmy% THEN pjresult% = 0
        IF pjresult% = 1 THEN
          IF pjmaze%(pjx2%, pjy2%) = 2 THEN pjresult% = 0
        END IF
        IF pjresult% = 1 THEN
          pjtemp% = pjloop% + 1
          pjtemp2% = pjtemp% + 4 * (pjtemp% > 3) - 4 * (pjtemp% < 0)
          IF pjmaze%(pjx2% + pjcos%(pjtemp2%), pjy2% + pjsin%(pjtemp2%)) = 2 THEN pjresult% = 0
        END IF
        IF pjresult% = 1 THEN
          pjtemp% = pjloop% - 1
          pjtemp2% = pjtemp% + 4 * (pjtemp% > 3) - 4 * (pjtemp% < 0)
          IF pjmaze%(pjx2% + pjcos%(pjtemp2%), pjy2% + pjsin%(pjtemp2%)) = 2 THEN pjresult% = 0
        END IF
      NEXT

      IF pjresult% = 1 THEN EXIT FOR
    NEXT

    IF pjloop% = 4 THEN EXIT DO

    pjtemp% = pjdir% + 1 - INT(RND * 3)
    pjdir% = pjtemp% + 4 * (pjtemp% > 3) - 4 * (pjtemp% < 0)
    pjresult% = 1

    FOR pjlop% = 1 TO pjexact% + 1
      pjx2% = pjx% + pjcos%(pjdir%) * pjlop%
      pjy2% = pjy% + pjsin%(pjdir%) * pjlop%

      IF pjx2% <= 0 OR pjy2% <= 0 OR pjx2% > pjmx% OR pjy2% > pjmy% THEN pjresult% = 0

      IF pjresult% = 1 THEN
        IF pjmaze%(pjx2%, pjy2%) = 2 THEN pjresult% = 0
      END IF
      IF pjresult% = 1 THEN
        pjtemp% = pjdir% + 1
        pjtemp2% = pjtemp% + 4 * (pjtemp% > 3) - 4 * (pjtemp% < 0)
        IF pjmaze%(pjx2% + pjcos%(pjtemp2%), pjy2% + pjsin%(pjtemp2%)) = 2 THEN pjresult% = 0
      END IF
      IF pjresult% = 1 THEN
        pjtemp% = pjdir% - 1
        pjtemp2% = pjtemp% + 4 * (pjtemp% > 3) - 4 * (pjtemp% < 0)
        IF pjmaze%(pjx2% + pjcos%(pjtemp2%), pjy2% + pjsin%(pjtemp2%)) = 2 THEN pjresult% = 0
      END IF
    NEXT
    IF pjresult% = 1 THEN
      FOR pjloop% = 1 TO pjexact%
        pjx% = pjx% + pjcos%(pjdir%)
        pjy% = pjy% + pjsin%(pjdir%)
        pjmaze%(pjx%, pjy%) = 2
      NEXT
    END IF
  LOOP
LOOP

IF pjexact% = 2 THEN
  FOR pjfx% = 2 TO pjmx% - 2 STEP 2
    IF pjmaze%(pjfx%, 2) = 0 THEN
      pjmaze%(pjfx%, 2) = 2: pjmaze%(pjfx%, 1) = 2
    END IF
    IF pjmaze%(pjfx%, pjmy% - 2) = 0 THEN
      pjmaze%(pjfx%, pjmy% - 2) = 2: pjmaze%(pjfx%, pjmy% - 1) = 2
    END IF
  NEXT

  FOR pjfy% = 2 TO pjmy% - 2 STEP 2
    IF pjmaze%(2, pjfy%) = 0 THEN
      pjmaze%(2, pjfy%) = 2: pjmaze%(1, pjfy%) = 2
    END IF
    IF pjmaze%(pjmx% - 2, pjfy%) = 0 THEN
      pjmaze%(pjmx% - 2, pjfy%) = 2: pjmaze%(pjmx% - 1, pjfy%) = 2
    END IF
  NEXT
END IF
END SUB

SUB SolveMaze (pjmaze%(), startx%, starty%, endx%, endy%)
' A* pathfinding from (startx,starty) to (endx,endy)
' Walls = 2, passages = 0, solution marked as 1
' 4-directional movement, Manhattan distance heuristic
DIM pjcos%(4), pjsin%(4)

pjcos%(0) = 1
pjcos%(2) = -1
pjsin%(1) = 1
pjsin%(3) = -1

endpoint% = pjmaze%(endx%, endy%)

pjmaze%(endx%, endy%) = 0  ' Blank end position

pjmx% = UBOUND(pjmaze%, 1)
pjmy% = UBOUND(pjmaze%, 2)

' Per-cell score and parent arrays, indexed [x][y]
DIM asg&(pjmx%, pjmy%)         ' g: cost from start
DIM asf&(pjmx%, pjmy%)         ' f: g + heuristic
DIM aspx%(pjmx%, pjmy%)        ' parent x (-1 = none)
DIM aspy%(pjmx%, pjmy%)        ' parent y (-1 = none)
DIM asinopen%(pjmx%, pjmy%)    ' 1 = currently in open list
DIM asinclosed%(pjmx%, pjmy%)  ' 1 = already evaluated

' Open list stored as parallel flat arrays of x,y coordinates
DIM asopenx%(pjmx% * pjmy%)
DIM asopeny%(pjmx% * pjmy%)

asopencount% = 0

' Initialise all cells to large cost, no parent
FOR ix% = 0 TO pjmx% - 1
  FOR iy% = 0 TO pjmy% - 1
    asg&(ix%, iy%) = 999999
    asf&(ix%, iy%) = 999999
    aspx%(ix%, iy%) = -1
    aspy%(ix%, iy%) = -1
    asinopen%(ix%, iy%) = 0
    asinclosed%(ix%, iy%) = 0
  NEXT
NEXT

' Add start node to open list
asg&(startx%, starty%) = 0
asf&(startx%, starty%) = ABS(endx% - startx%) + ABS(endy% - starty%)
asinopen%(startx%, starty%) = 1
asopenx%(0) = startx%
asopeny%(0) = starty%
asopencount% = 1

' 4 directions: right, down, left, up
DIM asdx%(4)
DIM asdy%(4)

asdx%(0) = 1: asdy%(0) = 0
asdx%(1) = 0: asdy%(1) = 1
asdx%(2) = -1: asdy%(2) = 0
asdx%(3) = 0: asdy%(3) = -1

asfound% = 0

DO WHILE asopencount% > 0 AND asfound% = 0
  ' Pick the open node with the lowest f score
  asbestf& = 999999
  asbestidx% = 0

  FOR asloop% = 0 TO asopencount% - 1
    aslx% = asopenx%(asloop%)
    asly% = asopeny%(asloop%)
    IF asf&(aslx%, asly%) < asbestf& THEN
      asbestf& = asf&(aslx%, asly%)
      asbestidx% = asloop%
    END IF
  NEXT

  ascx% = asopenx%(asbestidx%)
  ascy% = asopeny%(asbestidx%)

  ' Reached the goal?
  IF ascx% = endx% AND ascy% = endy% THEN
    asfound% = -1
    EXIT DO
  END IF

  ' Move this node from open to closed
  asinopen%(ascx%, ascy%) = 0
  asinclosed%(ascx%, ascy%) = 1

  ' Swap with last entry to remove from open list
  asopenx%(asbestidx%) = asopenx%(asopencount% - 1)
  asopeny%(asbestidx%) = asopeny%(asopencount% - 1)
  asopencount% = asopencount% - 1

  ' Check each of the 4 neighbours
  FOR asdir% = 0 TO 3
    asnx% = ascx% + asdx%(asdir%)
    asny% = ascy% + asdy%(asdir%)

    ' Bounds check
    IF asnx% >= 0 AND asnx% < pjmx% AND asny% >= 0 AND asny% < pjmy% THEN
      ' Skip walls (value=2) and already-evaluated nodes
      IF pjmaze%(asnx%, asny%) <> 2 THEN
        IF asinclosed%(asnx%, asny%) <> 1 THEN
          ' Cost to reach this neighbour via current node
          astentg% = asg&(ascx%, ascy%) + 1
          IF astentg% < asg&(asnx%, asny%) THEN
            ' Better path found - record it
            aspx%(asnx%, asny%) = ascx%
            aspy%(asnx%, asny%) = ascy%
            asg&(asnx%, asny%) = astentg%
            asf&(asnx%, asny%) = astentg% + ABS(endx% - asnx%) + ABS(endy% - asny%)

            ' Add to open list if not already there
            IF asinopen%(asnx%, asny%) = 0 THEN
              asopenx%(asopencount%) = asnx%
              asopeny%(asopencount%) = asny%
              asopencount% = asopencount% + 1
              asinopen%(asnx%, asny%) = 1
            END IF
          END IF
        END IF
      END IF
    END IF
  NEXT
LOOP

' Walk back through parent pointers, marking solution cells as 1
IF asfound% THEN
  astx% = endx%
  asty% = endy%

  WHILE astx% <> -1 AND asty% <> -1
    pjmaze%(astx%, asty%) = 1

    asnextx% = aspx%(astx%, asty%)
    asnexty% = aspy%(astx%, asty%)

    astx% = asnextx%
    asty% = asnexty%
  WEND
END IF

pjmaze%(endx%, endy%) = endpoint% ' Reset endpoint
END SUB

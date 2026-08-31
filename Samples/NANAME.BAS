CONST DIFFEasy% = 1
CONST DIFFHard% = 2

CONST BOARDEmpty% = 1
CONST BOARDForwardSlash% = 2
CONST BOARDBackwardSlash% = 0
CONST BOARDBorder% = 4
CONST BOARDSlashError% = 8
CONST BOARDVertexError% = 16
CONST BOARDVertexComplete% = 32

CONST BOARDSlashMask% = BOARDEmpty% OR BOARDForwardSlash%
CONST BOARDErrorMask% = BOARDSlashError% OR BOARDVertexError%

TYPE DSFEntry
  ' If this is the canonical element of an equivalent class,
  ' ParentOrSize holds the number of elements in that class,
  ' with the DSFCanonical% bit set.
  '
  ' If it is not the canonical element, ParentOrSize holds
  ' the index of another element nearer to the root of the
  ' tree for that class.
  ParentOrSize AS INTEGER

  ' If this is a canonical element, FlipOrMin holds the index
  ' of the smallest value in n's equivalent class.
  '
  ' If it is not a canonical element, FlipOrMin holds a flag
  ' indicating whether the sense of this element is flipped
  ' relative to ParentOrSize.
  FlipOrMin AS INTEGER
END TYPE

TYPE FindLoopState
  Depth AS INTEGER
  ShallowestReachable AS INTEGER
  SubtreeSize AS INTEGER
  Parent AS INTEGER
  ComponentRoot AS INTEGER
  LoopPrev AS INTEGER
  LoopNext AS INTEGER
END TYPE

DECLARE SUB DSFCanonify2D (xc%, yc%, dsf() AS DSFEntry, x%, y%)
DECLARE SUB DSFCompressPath (dsf() AS DSFEntry, BYVAL n%, root%)
DECLARE SUB DSFInitialize (dsf() AS DSFEntry)
DECLARE SUB DSFMerge (dsf() AS DSFEntry, n1%, n2%)
DECLARE SUB DSFMerge2D (xc%, yc%, dsf() AS DSFEntry, x1%, y1%, x2%, y2%)
DECLARE FUNCTION DSFCanonify% (dsf() AS DSFEntry, n%)
DECLARE FUNCTION DSFEquivalent% (dsf() AS DSFEntry, n1%, n2%)
DECLARE FUNCTION DSFEquivalent2D% (xc%, yc%, dsf() AS DSFEntry, n1x%, n1y%, n2x%, n2y%)
DECLARE FUNCTION DSFFindRoot% (dsf() AS DSFEntry, BYVAL n%)
DECLARE FUNCTION DSFFindRootFlip% (dsf() AS DSFEntry, n%, flip%)
DECLARE FUNCTION DSFSize% (dsf() AS DSFEntry, n%)

DECLARE SUB DecrementExits (xc%, yc%, connected() AS DSFEntry, exits%(), clues%(), x%, y%)
DECLARE SUB FindNeighbours (xc%, yc%, board%(), x%, y%, neighbourx%(), neighhboury%(), neighbourcount%)
DECLARE SUB GenerateGokigenNanamePuzzle (xc%, yc%, difficulty%, solution%(), clues%())
DECLARE SUB GenerateValidSolution (xc%, yc%, solution%())
DECLARE SUB Shuffle (a%())
DECLARE SUB SolveMergeVertices (xc%, yc%, connected() AS DSFEntry, exits%(), border%(), ix%, iy%, jx%, jy%)
DECLARE FUNCTION CheckCompletion% (xc%, yc%, solution%(), clues%(), board%())
DECLARE FUNCTION FillSquare% (xc%, yc%, solution%(), connected() AS DSFEntry, x%, y%, v%)
DECLARE FUNCTION FillSquareAndTrackExits% (xc%, yc%, solution%(), connected() AS DSFEntry, x%, y%, v%, sc%, exits%(), border%(), equiv() AS DSFEntry, slashval%(), clues%())
DECLARE FUNCTION FindLoopIsBridge% (pv() AS FindLoopState, u%, v%, uvertices%, vvertices%)
DECLARE FUNCTION FindLoopIsBridgeOneWay% (pv() AS FindLoopState, u%, v%, uvertices%, vvertices%)
DECLARE FUNCTION FindLoopIsLoopEdge% (pv() AS FindLoopState, u%, v%)
DECLARE FUNCTION FindLoopRun% (xc%, yc%, board%(), pv() AS FindLoopState)
DECLARE FUNCTION IIF% (c%, a%, b%)
DECLARE FUNCTION Solve% (xc%, yc%, clues%(), solution%(), difficulty%, connected() AS DSFEntry, exits%(), border%(), equiv() AS DSFEntry, slashval%(), vbitmap%())
DECLARE FUNCTION UpdateVBitmap% (vbitmap%(), x%, y%, vbits%)
DECLARE FUNCTION VertexDegree% (xc%, yc%, board%(), x%, y%, anti%, sx%, sy%)

DECLARE SUB PlaceBoard (xc%, yc%, gx%, gy%, gw%, gh%)
DECLARE SUB DrawBoard (xc%, yc%, clues%(), board%(), showgridlines%, hidecompleteclues%)

DECLARE SUB InitializeClueGlyphs ()
DECLARE SUB DrawCluePlaceholders (xc%, yc%)
DECLARE SUB RemoveCluePlaceholder (xc%, yc%, x%, y%)
DECLARE SUB ExposeClues (xc%, yc%, clues%())

TYPE RegTypeX
  AX AS INTEGER
  BX AS INTEGER
  cx AS INTEGER
  dx AS INTEGER
  BP AS INTEGER
  SI AS INTEGER
  DI AS INTEGER
  FLAGS AS INTEGER
  DS AS INTEGER
  ES AS INTEGER
END TYPE

DECLARE SUB InterruptX (r%, rin AS RegTypeX, rout AS RegTypeX)

DECLARE SUB ResetMouse ()
DECLARE SUB ShowMouse ()
DECLARE SUB HideMouse ()
DECLARE SUB GetMouseState (mx%, my%, mb%)

CONST DSFCanonical% = -32768
CONST DSFIndexMask% = 32767

DECLARE SUB InitializeSlashGlyphs ()

DECLARE SUB FancyJapaneseMessage (message$, messagechars%, messagey%, colours$)

' JAPANESE.BAS
DECLARE SUB InitializeC64Japanese (screenwidth%)
DECLARE SUB JPprint (x%, y%, t$)
DECLARE SUB JPprint2x (x%, y%, t$)
DECLARE SUB DrawGlyph (x%, y%, glyphs%(), BYVAL o%)
DECLARE SUB DrawGlyph2x (x%, y%, glyphs%(), BYVAL o%)

SCREEN 13

DIM slashglyphs%(16)
DIM clueglyphs%(40)

InitializeC64Japanese 640 ' SCREEN 9
InitializeSlashGlyphs
InitializeClueGlyphs

SCREEN 9, , 0, 0

FancyJapaneseMessage "KOtentenKIKEtentenN NANAME", 10, 25, "E4"

' Vertices
xc% = 9
yc% = 9

' Cells
w% = xc% - 1
h% = yc% - 1

difficulty% = DIFFEasy%

DIM solution%(w% - 1, h% - 1)
DIM clues%(xc% - 1, yc% - 1)

GenerateGokigenNanamePuzzle xc%, yc%, difficulty%, solution%(), clues%()

ExposeClues xc%, yc%, clues%()

DIM board%(xc% - 1, yc% - 1)

FOR y% = 0 TO yc% - 1
  FOR x% = 0 TO xc% - 1
    board%(x%, y%) = BOARDEmpty%
  NEXT x%
NEXT y%

PlaceBoard xc%, yc%, gx%, gy%, gw%, gh%

ResetMouse
ShowMouse

DO
  DO
    GetMouseState mx%, my%, mb%

    hit% = -1

    IF mx% >= gx% THEN mcx% = (mx% - gx%) \ 16 ELSE hit% = 0
    IF my% >= gy% THEN mcy% = (my% - gy%) \ 16 ELSE hit% = 0

    IF mcx% >= xc% THEN hit% = 0
    IF mcy% >= yc% THEN hit% = 0

    IF INKEY$ = CHR$(27) THEN END
  LOOP WHILE (mb% AND 3) = 0 OR NOT hit%

  IF (mb% AND 2) <> 0 THEN
    ' Clear
    board%(mcx%, mcy%) = board%(mcx%, mcy%) OR BOARDEmpty%
  ELSE
    ' Toggle
    IF board%(mcx%, mcy%) AND BOARDEmpty% THEN
      board%(mcx%, mcy%) = board%(mcx%, mcy%) AND NOT BOARDSlashMask%
    END IF

    board%(mcx%, mcy%) = board%(mcx%, mcy%) XOR BOARDForwardSlash%
  END IF

  complete% = CheckCompletion%(xc%, yc%, solution%(), clues%(), board%())

  HideMouse

  SCREEN 9, , 1, 0
  DrawBoard xc%, yc%, clues%(), board%(), -1, 0 ' grid lines, show clues
  PCOPY 1, 0
  SCREEN 9, , 0, 0

  ShowMouse

  DO
    GetMouseState mx%, my%, mb%
  LOOP UNTIL (mb% AND 3) = 0
LOOP UNTIL complete%

HideMouse

SCREEN 9, , 1, 0
DrawBoard xc%, yc%, clues%(), board%(), 0, -1 ' no grid lines, hide clues
PCOPY 1, 0
SCREEN 9, , 0, 0

FancyJapaneseMessage "TEtentenKIMASITA!! YATUTA!!", 14, 315, "FBD591"

SLEEP

DATA XXX
DATA X.X
DATA X.X
DATA X.X
DATA XXX

DATA .X.
DATA XX.
DATA .X.
DATA .X.
DATA XXX

DATA XX.
DATA ..X
DATA .X.
DATA X..
DATA XXX

DATA XX.
DATA ..X
DATA .X.
DATA ..X
DATA XX.

DATA X.X
DATA X.X
DATA XXX
DATA ..X
DATA ..X



FUNCTION CheckCompletion% (xc%, yc%, solution%(), clues%(), board%())
w% = xc% - 1
h% = yc% - 1

maxvertex% = xc% * yc% - 1

haveerror% = 0

clearstatebits% = BOARDErrorMask% OR BOARDVertexComplete% OR BOARDEdge%
keepbits% = NOT clearstatebits%

FOR x% = 0 TO xc% - 1
  FOR y% = 0 TO yc% - 1
    board%(x%, y%) = board%(x%, y%) AND keepbits%
  NEXT y%
NEXT x%

' Detect and grounded-highlight edge-connected components in the grid.

DIM connected(0 TO maxvertex%) AS DSFEntry

DSFInitialize connected()

FOR x% = 0 TO w%
  DSFMerge2D xc%, yc%, connected(), x%, 0, 0, 0
  DSFMerge2D xc%, yc%, connected(), x%, h%, 0, 0
NEXT x%

FOR y% = 0 TO h%
  DSFMerge2D xc%, yc%, connected(), 0, y%, 0, 0
  DSFMerge2D xc%, yc%, connected(), w%, y%, 0, 0
NEXT y%

FOR y% = 0 TO h% - 1
  FOR x% = 0 TO w% - 1
    SELECT CASE board%(x%, y%) AND BOARDSlashMask%
      CASE BOARDBackSlash%: DSFMerge2D xc%, yc%, connected(), x%, y%, x% + 1, y% + 1
      CASE BOARDForwardSlash%: DSFMerge2D xc%, yc%, connected(), x% + 1, y%, x%, y% + 1
    END SELECT
  NEXT x%
NEXT y%

rootNW% = DSFCanonify%(connected(), 0)

FOR y% = 0 TO h% - 1
  FOR x% = 0 TO w% - 1
    IF (board%(x%, y%) AND BOARDEmpty%) = 0 THEN
      idx% = y% * xc% + x%

      IF (board%(x%, y%) AND BOARDSlashMask%) = BOARDForwardSlash% THEN
        idx% = idx% + 1
      END IF

      IF DSFCanonify%(connected(), idx%) = rootNW% THEN
        board%(x%, y%) = board%(x%, y%) OR BOARDEdge%
      END IF
    END IF
  NEXT x%
NEXT y%

' Detect and error-highlight loops in the grid.
DIM fls(xc% * yc% - 1) AS FindLoopState

IF FindLoopRun%(xc%, yc%, board%(), fls()) THEN
  haveerror% = -1
END IF

FOR y% = 0 TO h% - 1
  FOR x% = 0 TO w% - 1
    IF (board%(x%, y%) AND BOARDEmpty%) = 0 THEN
      SELECT CASE board%(x%, y%) AND BOARDSlashMask%
        CASE BOARDForwardSlash%:
          u% = y% * xc% + (x% + 1)
          v% = (y% + 1) * xc% + x%
        CASE BOARDBackwardSlash%:
          u% = (y% + 1) * xc% + (x% + 1)
          v% = y% * xc% + x%
      END SELECT

      IF FindLoopIsLoopEdge(fls(), u%, v%) THEN
        board%(x%, y%) = board%(x%, y%) OR BOARDSlashError%
      END IF
    END IF
  NEXT x%
NEXT y%

' Now go through and check the degree of each clue vertex, and
' mark it with BOARDVertexComplete% if it is finished or
' BOARDVertexError% if it cannot be fulfilled.

FOR y% = 0 TO yc% - 1
  FOR x% = 0 TO xc% - 1
    c% = clues%(x%, y%)

    IF c% >= 0 THEN
      ' Check to see if there are too many connections to
      ' this vertex _or_ too many non-connections. Either is
      ' grounds for marking the vertex as erroneous.

      connections% = VertexDegree%(xc%, yc%, board%(), x%, y%, 0, tx%, ty%)
      nonconnections% = VertexDegree%(xc%, yc%, board%(), x%, y%, -1, tx%, ty%)

      haveadjacentslash% = 0

      IF x% > 0 THEN dx1% = -1 ELSE dx1% = 0
      IF y% > 0 THEN dy1% = -1 ELSE dy1% = 0

      FOR dx% = dx1% TO 0
        FOR dy% = dy1% TO 0
          IF (board%(x% + dx%, y% + dy%) AND BOARDEmpty%) = 0 THEN
            haveadjacentslash% = -1
          END IF
        NEXT dy%
      NEXT dx%

      havecompletevertex% = (connections% = c%)

      IF havecompletevertex% AND (c% = 0) THEN
        ' Only complete 0 clues if there is at least one adjacent slash.
        IF NOT haveadjacentslash% THEN havecompletevertex% = 0
      END IF

      havevertexerror% = (connections% > c%)

      IF haveadjacentslash% AND NOT havevertexerror% THEN
        havevertexerror% = (nonconnections% > 4 - c%)
      END IF

      IF havecompletevertex% THEN
        board%(x%, y%) = board%(x%, y%) OR BOARDVertexComplete%
      END IF

      IF havevertexerror% THEN
        board%(x%, y%) = board%(x%, y%) OR BOARDVertexError%
        haveerror% = -1
      END IF
    END IF
  NEXT x%
NEXT y%

' Now our actual victory condition is that (a) none of the
' above code marked anything as erroneous, and (b) every
' square has an edge in it.
IF haveerror% = 0 THEN
  FOR y% = 0 TO h% - 1
    FOR x% = 0 TO w% - 1
      IF board%(x%, y%) AND BOARDEmpty% THEN EXIT FUNCTION
    NEXT x%
  NEXT y%

  ' Success!
  CheckCompletion% = -1
END IF
END FUNCTION

' Called when we have just blocked one way out of a particular
' point. If that point is a non-clue point (thus has a variable
' number of exits), we have therefore decreased its potential exit
' count, so we must decrement the exit count for the group as a
' whole.
SUB DecrementExits (xc%, yc%, connected() AS DSFEntry, exits%(), clues%(), x%, y%)
IF clues%(x%, y%) < 0 THEN
  DSFCanonify2D xc%, yc%, connected(), x%, y%
  exits%(x%, y%) = exits%(x%, y%) - 1
END IF
END SUB

SUB DrawBoard (xc%, yc%, clues%(), board%(), showgridlines%, hidecompleteclues%)
SHARED slashglyphs%()
SHARED clueglyphs%()

w% = xc% - 1
h% = yc% - 1

PlaceBoard xc%, yc%, gx%, gy%, gw%, gh%

LINE (gx% - 5, gy% - 5)-STEP(gw% + 10, gh% + 10), 0, BF

IF showgridlines% THEN
  xx% = gx%
  FOR x% = 0 TO xc% - 1
    LINE (xx%, gy%)-(xx%, gy% + gh% - 1), 8
    xx% = xx% + 16
  NEXT x%

  yy% = gy%
  FOR y% = 0 TO yc% - 1
    LINE (gx%, yy%)-(gx% + gw% - 1, yy%), 8
    yy% = yy% + 16
  NEXT y%
END IF

yy% = gy%

FOR y% = 0 TO h% - 1
  xx% = gx%

  FOR x% = 0 TO w% - 1
    IF (board%(x%, y%) AND BOARDEmpty%) = 0 THEN
      IF board%(x%, y%) AND BOARDForwardSlash% THEN o% = 8 ELSE o% = 0

      IF board%(x%, y%) AND BOARDSlashError% THEN
        COLOR 4
      ELSEIF board%(x%, y%) AND BOARDBorder% THEN
        COLOR 7
      ELSE
        COLOR 15
      END IF

      DrawGlyph2x xx%, yy%, slashglyphs%(), o%
    END IF

    xx% = xx% + 16
  NEXT x%

  yy% = yy% + 16
NEXT y%

yy% = gy%

FOR y% = 0 TO yc% - 1
  xx% = gx%

  FOR x% = 0 TO xc% - 1
    haveclue% = clues%(x%, y%) >= 0
    iscomplete% = (board%(x%, y%) AND BOARDVertexComplete%) <> 0

    IF haveclue% AND NOT (iscomplete% AND hidecompleteclues%) THEN
      CIRCLE (xx%, yy%), 4, 7, , , 1
      PAINT (xx%, yy%), 0, 7

      IF board%(x%, y%) AND BOARDVertexError% THEN
        COLOR 12
      ELSE
        COLOR 15
      END IF

      DrawGlyph xx% - 1, yy% - 2, clueglyphs%(), clues%(x%, y%) * 8
    END IF

    xx% = xx% + 16
  NEXT x%

  yy% = yy% + 16
NEXT y%

VIEW
END SUB

SUB DrawCluePlaceholders (xc%, yc%)
PlaceBoard xc%, yc%, gx%, gy%, gw%, gh%

FOR x% = 0 TO xc% - 1
  LINE (gx% + x% * 16, gy%)-(gx% + x% * 16, gy% + gh% - 1), 8
NEXT x%

FOR y% = 0 TO yc% - 1
  LINE (gx%, gy% + y% * 16)-(gx% + gw% - 1, gy% + y% * 16), 8
NEXT y%

FOR y% = 0 TO yc% - 1
  yy% = gy% + y% * 16

  FOR x% = 0 TO xc% - 1
    xx% = gx% + x% * 16

    CIRCLE (xx%, yy%), 4, 7, , , 1
    PAINT (xx%, yy%), 0, 7
  NEXT x%
NEXT y%
END SUB

FUNCTION DSFCanonify% (dsf() AS DSFEntry, n%)
IF (n% < 0) OR (n% > UBOUND(dsf)) THEN ERROR 9

root% = DSFFindRoot%(dsf(), n%)
DSFCompressPath dsf(), n%, root%

DSFCanonify% = root%
END FUNCTION

SUB DSFCanonify2D (xc%, yc%, dsf() AS DSFEntry, x%, y%)
i% = y% * xc% + x%

i% = DSFCanonify%(dsf(), i%)

y% = i% \ xc%
x% = i% MOD xc%
END SUB

SUB DSFCompressPath (dsf() AS DSFEntry, BYVAL n%, root%)
WHILE (dsf(n%).ParentOrSize AND DSFCanonical%) = 0
  nextn% = dsf(n%).ParentOrSize
  dsf(n%).ParentOrSize = root%
  n% = nextn%
WEND
END SUB

SUB DSFCompressPathFlip (dsf() AS DSFEntry, n%, root%, flip%)
WHILE (dsf(n%).ParentOrSize AND DSFCanonical%) = 0
  nextn% = dsf(n%).ParentOrSize
  nextflip% = flip% XOR dsf(n%).FlipOrMin

  dsf(n%).FlipOrMin = flip%
  dsf(n%).ParentOrSize = root%

  n% = nextn%
  flip% = nextflip%
WEND

IF n% <> root% THEN ERROR 5
END SUB

FUNCTION DSFEquivalent% (dsf() AS DSFEntry, n1%, n2%)
DSFEquivalent% = (DSFCanonify%(dsf(), n1%) = DSFCanonify%(dsf(), n2%))
END FUNCTION

FUNCTION DSFEquivalent2D% (xc%, yc%, dsf() AS DSFEntry, n1x%, n1y%, n2x%, n2y%)
n1% = n1y% * xc% + n1x%
n2% = n2y% * xc% + n2x%

DSFEquivalent2D% = DSFEquivalent%(dsf(), n1%, n2%)
END FUNCTION

FUNCTION DSFFindRoot% (dsf() AS DSFEntry, BYVAL n%)
WHILE (dsf(n%).ParentOrSize AND DSFCanonical%) = 0
  n% = dsf(n%).ParentOrSize
WEND

DSFFindRoot% = n%
END FUNCTION

FUNCTION DSFFindRootFlip% (dsf() AS DSFEntry, n%, flip%)
flip% = 0

WHILE (dsf(n%).ParentOrSize AND DSFCanonical%) = 0
  flip% = flip% XOR dsf(n%).FlipOrMin
  n% = dsf(n%).ParentOrSize
WEND

DSFFindRootFlip% = n%
END FUNCTION

SUB DSFInitialize (dsf() AS DSFEntry)
' Every element starts as the root of an equivalent class of size 1
FOR i% = 0 TO UBOUND(dsf)
  dsf(i%).ParentOrSize = 1 OR DSFCanonical%
  dsf(i%).FlipOrMin = i% ' Every element is also its own min
NEXT i%
END SUB

SUB DSFMerge (dsf() AS DSFEntry, n1%, n2%)
IF (n1% < 0) OR (n1% > UBOUND(dsf)) THEN ERROR 9
IF (n2% < 0) OR (n2% > UBOUND(dsf)) THEN ERROR 9

' Find the root elements
r1% = DSFFindRoot%(dsf(), n1%)
r2% = DSFFindRoot%(dsf(), n2%)

IF r1% = r2% THEN
  ' Classes are already the same, so we have a common root
  root% = r1%
ELSE
  ' Classes must be merged

  ' Decide which one to use as the overall root, based on size
  s1% = dsf(r1%).ParentOrSize AND DSFIndexMask%
  s2% = dsf(r2%).ParentOrSize AND DSFIndexMask%

  IF s1% > s2% THEN
    dsf(r2%).ParentOrSize = r1%
    root% = r1%
  ELSE
    dsf(r1%).ParentOrSize = r2%
    root% = r2%
  END IF

  dsf(root%).ParentOrSize = (s1% + s2%) OR DSFCanonical%

  ' Update the min of the merged class
  m1% = dsf(r1%).FlipOrMin
  m2% = dsf(r2%).FlipOrMin

  IF m1% < m2% THEN m% = m1% ELSE m% = m2%

  dsf(root%).FlipOrMin = m%
END IF

' Path-compress both paths from n1 and n2 so they point at the new root
DSFCompressPath dsf(), n1%, root%
DSFCompressPath dsf(), n2%, root%
END SUB

SUB DSFMerge2D (xc%, yc%, dsf() AS DSFEntry, x1%, y1%, x2%, y2%)
n1% = y1% * xc% + x1%
n2% = y2% * xc% + x2%

DSFMerge dsf(), n1%, n2%
END SUB

FUNCTION DSFSize% (dsf() AS DSFEntry, n%)
root% = DSFCanonify%(dsf(), n%)
DSFSize% = dsf(root%).ParentOrSize AND DSFIndexMask%
END FUNCTION

SUB ExposeClues (xc%, yc%, clues%())
SHARED clueglyphs%()

PlaceBoard xc%, yc%, gx%, gy%, gw%, gh%

COLOR 15

FOR y% = 0 TO yc% - 1
  yy% = gy% + y% * 16

  FOR x% = 0 TO xc% - 1
    xx% = gx% + x% * 16

    IF clues%(x%, y%) >= 0 THEN
      DrawGlyph xx% - 1, yy% - 2, clueglyphs%(), clues%(x%, y%) * 8
    END IF
  NEXT x%
NEXT y%
END SUB

SUB FancyJapaneseMessage (message$, messagechars%, messagey%, colours$)
messagex% = 320 - 8 * messagechars%

FOR i% = LEN(colours$) TO 1 STEP -1
  COLOR VAL("&H" + MID$(colours$, i%, 1))
  JPprint2x messagex% + i%, messagey% + i%, message$
NEXT i%
END SUB

FUNCTION FillSquare% (xc%, yc%, solution%(), connected() AS DSFEntry, x%, y%, v%)
STATIC dummy%(), dummydsf() AS DSFEntry, dummyinitialized%

IF dummyinitialized% = 0 THEN
  DIM dummy%(1), dummydsf(1) AS DSFEntry
  dummyinitialized% = -1
END IF

FillSquare% = FillSquareAndTrackExits%(xc%, yc%, solution%(), connected(), x%, y%, v%, 0, dummy%(), dummy%(), dummydsf(), dummy%(), dummy%())
END FUNCTION

FUNCTION FillSquareAndTrackExits% (xc%, yc%, solution%(), connected() AS DSFEntry, x%, y%, v%, sc%, exits%(), border%(), equiv() AS DSFEntry, slashval%(), clues%())
IF solution%(x%, y%) = v% THEN FillSquareAndTrackExits% = -1: EXIT FUNCTION
IF solution%(x%, y%) <> 0 THEN FillSquareAndTrackExits% = 0: EXIT FUNCTION ' Already filled

IF v% < 0 THEN
  ci1x% = x%: ci1y% = y%
  ci2x% = x% + 1: ci2y% = y% + 1

  di1x% = x% + 1: di1y% = y%
  di2x% = x%: di2y% = y% + 1
ELSE
  ci1x% = x% + 1: ci1y% = y%
  ci2x% = x%: ci2y% = y% + 1

  di1x% = x%: di1y% = y%
  di2x% = x% + 1: di2y% = y% + 1
END IF

ci1% = ci1y% * xc% + ci1x%
ci2% = ci2y% * xc% + ci2x%

IF DSFEquivalent%(connected(), ci1%, ci2%) THEN FillSquareAndTrackExits% = 0: EXIT FUNCTION ' Would make a loop

' DEBUG
'IF sc% THEN
'  correct% = POINT((x% + 1) * 8, (y% + 1) * 8)
'  IF correct% = 0 THEN correctv% = 1 ELSE correctv% = -1
'  IF v% = correctv% THEN COLOR 2 ELSE COLOR 4
'  LOCATE y% + 2, x% + 2
'  IF v% > 0 THEN PRINT "/";  ELSE PRINT "\";
'END IF

solution%(x%, y%) = v%

IF sc% = 0 THEN
  DSFMerge connected(), ci1%, ci2%
ELSE
  w% = xc% - 1
  h% = yc% - 1

  DSFCanonify2D w%, h%, equiv(), x%, y%

  slashval%(x%, y%) = v%

  SolveMergeVertices xc%, yc%, connected(), exits%(), border%(), ci1x%, ci1y%, ci2x%, ci2y%

  DecrementExits xc%, yc%, connected(), exits%(), clues%(), di1x%, di1y%
  DecrementExits xc%, yc%, connected(), exits%(), clues%(), di2x%, di2y%
END IF

FillSquareAndTrackExits% = -1
END FUNCTION

FUNCTION FindLoopIsBridge% (pv() AS FindLoopState, u%, v%, uvertices%, vvertices%)
IF FindLoopIsBridgeOneWay%(pv(), u%, v%, uvertices%, vvertices%) THEN
  FindLoopIsBridge% = -1
ELSEIF FindLoopIsBridgeOneWay%(pv(), v%, u%, vvertices%, uvertices%) THEN
  FindLoopIsBridge% = -1
END IF
END FUNCTION

FUNCTION FindLoopIsBridgeOneWay% (pv() AS FindLoopState, u%, v%, uvertices%, vvertices%)
IF pv(u%).Parent <> v% THEN
  FindLoopIsBridgeOneWay% = 0
ELSEIF pv(u%).ShallowestReachable < pv(u%).Depth THEN
  FindLoopIsBridgeOneWay% = 0
ELSE
  uvertices% = pv(u%).SubtreeSize

  r% = pv(u%).ComponentRoot
  vvertices% = pv(r%).SubtreeSize - pv(u).SubtreeSize

  FindLoopIsBridgeOneWay% = -1
END IF
END FUNCTION

FUNCTION FindLoopIsLoopEdge% (pv() AS FindLoopState, u%, v%)
' In the DSF-built forest, all edges are either from parent
' to child or from child to ancestor.
'
' Back-edges to ancestors must be parts of loops. In order to
' detect whether a parent-to-child edge is part of a loop, we
' check if any ancestor is reachable from that child's subtree.

IF pv(u%).Parent = v% AND pv(u%).ShallowestReachable >= pv(u%).Depth THEN
  FindLoopIsLoopEdge% = 0
ELSEIF pv(v%).Parent = u% AND pv(v%).ShallowestReachable >= pv(v%).Depth THEN
  FindLoopIsLoopEdge% = 0
ELSE
  FindLoopIsLoopEdge% = -1
END IF
END FUNCTION

FUNCTION FindLoopRun% (xc%, yc%, board%(), pv() AS FindLoopState)
' We run a DSF to split the graph into disjoint spanning trees.
' This construction guarantees that every edge either descends
' to a new child never visited before or goes to an ancestor.
' Tracking which ancestors are linked from which subtrees lets
' us detect all loops efficiently.
'
' Here we don't use recursion, instead holding the entire DSF
' state in the findloopstate struct. The loop below visits each
' node exactly twice: before and after visiting its subtree.
'
' The first time we visit a node, we take care of marking its
' children with their position in the tree and when they're
' scheduled to be visited. The second time we update the parent
' with statistics about the subtree.
'
' The order of nodes is managed using a doubly-linked list.
' The first time a node is visited, we add its children before it
' in the list and set the pointer to go through them first. The
' second time we move to the next node in the list, which is a
' sibling or a parent, or if we're at the root of the connected
' component will be a node in the next component. (All the nodes
' of the graph always appear in the list)
' A linked list is used to handle the case where the same child
' appears in two levels, to allow us to efficiently remove it from
' its previous position.
'
' The algorithm tracks whether we're at the first or the second
' visit at a node using the depth property. It's set to a negative
' value on initialization, and to the depth in the connected
' component's tree on the first visit.
' It detects a new connected component using the parent pointer,
' it's always set to a real node in the search, and is negative for
' new trees.
'
' In the first visit, we go over the node's children, moving them
' in the list and setting their parent pointer. Edges going to
' ancestors are noted in the shallowest_reachable field.
' In the second visit, we adjust the subtree_size and
' shallowest_reachable fields of the parent.
'
' Variables:
'   u = the current node under examination
'   v = the node to go to in the next iteration
'   w = neighbour iterator
maxvertex% = UBOUND(pv)
numvertices% = maxvertex% + 1

FOR u% = 0 TO maxvertex%
  pv(u%).Depth = -1
  pv(u%).ShallowestReachable = numvertices%
  pv(u%).SubtreeSize = 1
  pv(u%).Parent = -1
  pv(u%).ComponentRoot = u%
  pv(u%).LoopPrev = u% - 1
  pv(u%).LoopNext = u% + 1

  IF pv(u%).LoopNext = numvertices% THEN pv(u%).LoopNext = -1
NEXT u%

anyloop% = 0

v% = 0

DIM neighbourx%(0 TO 3)
DIM neighboury%(0 TO 3)

WHILE v% <> -1
  u% = v%

  IF pv(u%).Depth < 0 THEN
    ' Our first visit to the node (on the way down the search)
    IF pv(u%).Parent < 0 THEN
      pv(u%).Depth = 0
      pv(u%).ComponentRoot = u%
    ELSE
      pv(u%).Depth = pv(pv(u%).Parent).Depth + 1
      pv(u%).ComponentRoot = pv(pv(u%).Parent).ComponentRoot
    END IF

    ' Schedule visits to the neighbors, and then back here
    v% = u%

    y% = u% \ xc%
    x% = u% MOD xc%

    FindNeighbours xc%, yc%, board%(), x%, y%, neighbourx%(), neighboury%(), neighbourcount%

    FOR wi% = 0 TO neighbourcount% - 1
      wx% = neighbourx%(wi%)
      wy% = neighboury%(wi%)

      w% = wy% * xc% + wx%

      IF w% <> pv(u%).Parent THEN
        IF pv(w%).Depth < 0 THEN
          pv(w%).Parent = u%

          ' Remove the neighbour from the linked list
          IF pv(w%).LoopPrev >= 0 THEN pv(pv(w%).LoopPrev).LoopNext = pv(w%).LoopNext
          IF pv(w%).LoopNext >= 0 THEN pv(pv(w%).LoopNext).LoopPrev = pv(w%).LoopPrev

          ' Add it to the start of the list
          pv(w%).LoopPrev = pv(v%).LoopPrev
          pv(w%).LoopNext = v%

          IF pv(v%).LoopPrev >= 0 THEN pv(pv(v%).LoopPrev).LoopNext = w%

          pv(v%).LoopPrev = w%

          ' Mark this as the next node to visit
          v% = w%
        ELSE
          IF pv(w%).Depth < pv(u%).ShallowestReachable THEN
            pv(u%).ShallowestReachable = pv(w%).Depth
          END IF

          anyloop% = -1
        END IF
      END IF
    NEXT wi%
  ELSE
    IF pv(u%).Parent >= 0 THEN
      pv(pv(u%).Parent).SubtreeSize = pv(pv(u%).Parent).SubtreeSize + pv(u%).SubtreeSize
      IF pv(u%).ShallowestReachable < pv(pv(u%).Parent).ShallowestReachable THEN
        pv(pv(u%).Parent).ShallowestReachable = pv(u%).ShallowestReachable
      END IF
    END IF

    v% = pv(u%).LoopNext
  END IF
WEND

FindLoopRun% = anyloop%
END FUNCTION

SUB FindNeighbours (xc%, yc%, board%(), x%, y%, neighbourx%(), neighboury%(), neighbourcount%)
w% = xc% - 1
h% = yc% - 1

n% = 0

IF x% < w% AND y% < h% THEN
  IF (board%(x%, y%) AND BOARDSlashMask%) = BOARDBackwardSlash% THEN
    neighbourx%(n%) = x% + 1
    neighboury%(n%) = y% + 1
    n% = n% + 1
  END IF
END IF

IF x% > 0 AND y% > 0 THEN
  IF (board%(x% - 1, y% - 1) AND BOARDSlashMask%) = BOARDBackwardSlash% THEN
    neighbourx%(n%) = x% - 1
    neighboury%(n%) = y% - 1
    n% = n% + 1
  END IF
END IF

IF x% > 0 AND y% < h% THEN
  IF (board%(x% - 1, y%) AND BOARDSlashMask%) = BOARDForwardSlash% THEN
    neighbourx%(n%) = x% - 1
    neighboury%(n%) = y% + 1
    n% = n% + 1
  END IF
END IF

IF x% < w% AND y% > 0 THEN
  IF (board%(x%, y% - 1) AND BOARDSlashMask%) = BOARDForwardSlash% THEN
    neighbourx%(n%) = x% + 1
    neighboury%(n%) = y% - 1
    n% = n% + 1
  END IF
END IF

neighbourcount% = n%
END SUB

SUB GenerateGokigenNanamePuzzle (xc%, yc%, difficulty%, solution%(), clues%())
COLOR 11
JPprint 320 - 11 * 4, 320, "SEISEITIYUU,OMATI"

IF xc% > 30 THEN xc% = 30
IF yc% > 20 THEN yc% = 20

w% = xc% - 1
h% = yc% - 1

maxvertex% = xc% * yc% - 1
maxcell% = w% * h% - 1

DIM tempsolution%(0 TO w% - 1, 0 TO h% - 1)

DIM clueindices%(0 TO maxvertex%)

' Disjoint set forest which tracks the connected sets of points.
DIM connected(0 TO maxvertex%) AS DSFEntry

' Counts the number of possible exits from each connected set
' of points. (That is, the number of possible _simultaneous_
' exits: an unconnected point albelled 2 has an exit count of
' 2 even if all four possible edges are still under
' consideration.)
DIM exits%(0 TO xc% - 1, 0 TO yc% - 1)

' Tracks whether each connected set of points includes a
' border point.
DIM border%(0 TO xc% - 1, 0 TO yc% - 1)

' Another disjoint set forest. This one tracks _squares_ which
' are known to slant in the same direction.
DIM equiv(0 TO maxcell%) AS DSFEntry

' Stores slash values which we known for an equivalence class.
' When we fill in a square, we set slashval(canonify(x)) to
' the same value as solution(x), so that we can then spot other
' squares equivalent to it and fill them in immediately via
' their known equivalence.
DIM slashval%(0 TO w% - 1, 0 TO h% - 1)

' Stores possible v-shapes. This array is w by h in size, but
' not every bit of every entry is meaningful. The bits mean:
'
'  - bit 0 for a square means that that square and the one to
'    its right might form a v-shape between them
'  - bit 1 for a square means that that square and the one to
'    its right might form a ^-shape between them
'  - bit 2 for a square means that that square and the one
'    below it might form a >-shape between them
'  - bit 3 for a square means that that square and the one
'    below it might form a <-shape between them
'
' Any starting 1 or 3 clue rules out four bits in this array
' immediately; a 2 clue propagates any ruled-out bit past it
' (if the two squares on one side of a 2 cannot be a v-shape,
' then neither can the two on the other side be the same
' v-shape); we can rule out further bits during play using
' partially filled 2 clues; whenever a pair of squares is
' known not to be _either_ kind of v-shape, we can mark them
' as equivalent.
DIM vbitmap%(0 TO w% - 1, 0 TO h% - 1)

DO
  ' Create the filled grid.
  GenerateValidSolution xc%, yc%, solution%()

  ' Fill in the complete set of clues.
  FOR y% = 0 TO yc% - 1
    FOR x% = 0 TO xc% - 1
      v% = 0

      IF x% > 0 THEN
        IF y% > 0 THEN IF solution%(x% - 1, y% - 1) = -1 THEN v% = v% + 1
        IF y% < h% THEN IF solution%(x% - 1, y%) = 1 THEN v% = v% + 1
      END IF

      IF x% < w% THEN
        IF y% > 0 THEN IF solution%(x%, y% - 1) = 1 THEN v% = v% + 1
        IF y% < h% THEN IF solution%(x%, y%) = -1 THEN v% = v% + 1
      END IF

      clues%(x%, y%) = v%
    NEXT x%
  NEXT y%

  ' Draw placeholders for the full set of clues as part of progress
  ' feedback to the user.
  DrawCluePlaceholders xc%, yc%

  ' With all clue points filled in, all puzzles are easy: we can
  ' simply process the clue points in lexicographic order, and
  ' at each clue point, we will always have at most one square
  ' undecided, which we can then fill in uniquely.
  sanitycheck% = Solve%(xc%, yc%, clues%(), solution%(), difficulty%, connected(), exits%(), border%(), equiv(), slashval%(), vbitmap%())

  IF sanitycheck% <> 1 THEN ERROR 5

  ' Remove as many clues as possible while retaining solubility.
  '
  ' In DIFFHard% mode, we prioritise the removal of obvious
  ' starting points (4s, 0s, border 2s and corner 1s), on
  ' the grounds that having as few of these as possible
  ' seems like a good thing. In particular, we can often get
  ' away without _any_ completely obvious starting points,
  ' which is even better.

  FOR i% = 0 TO maxvertex%
    clueindices%(i%) = i%
  NEXT i%

  Shuffle clueindices%()

  FOR j% = 0 TO 1
    FOR i% = 0 TO maxvertex%
      IF INKEY$ = CHR$(27) THEN END

      y% = clueindices%(i%) \ xc%
      x% = clueindices%(i%) MOD xc%

      v% = clues%(x%, y%)

      ' Identify which pass we should process this point
      ' in. If it's an obvious start point, _or_ we're
      ' in DIFFEasy%, then it goes in pass 0; otherwise
      ' pass 1.
      xb% = (x% = 0 OR x% = xc% - 1)
      yb% = (y% = 0 OR y% = yc% - 1)

      IF difficulty% = DIFFEasy% OR v% = 4 OR v% = 0 OR (v% = 2 AND (xb% OR yb%)) OR (v% = 1 AND xb% AND yb%) THEN
        pass% = 0
      ELSE
        pass% = 1
      END IF

      IF pass% = j% THEN
        ' Try taking it out
        clues%(x%, y%) = -1

        IF Solve%(xc%, yc%, clues%(), tempsolution%(), difficulty%, connected(), exits%(), border%(), equiv(), slashval%(), vbitmap%()) <> 1 THEN
          clues%(x%, y%) = v% ' put it back
        ELSE
          RemoveCluePlaceholder xc%, yc%, x%, y%
        END IF
      END IF
    NEXT i%
  NEXT j%
LOOP WHILE Solve%(xc%, yc%, clues%(), tempsolution%(), difficulty% - 1, connected(), exits%(), border%(), equiv(), slashval%(), vbitmap%()) <= 1

LINE (0, 320)-(639, 328), 0, BF
END SUB

SUB GenerateValidSolution (xc%, yc%, solution%())
' xc% and yc% count vertices, w% and h% count cells
w% = xc% - 1
h% = yc% - 1

' Clear the output.
FOR y% = 0 TO w% - 1
  FOR x% = 0 TO h% - 1
    solution%(x%, y%) = 0
  NEXT x%
NEXT y%

' Establish a disjoint set forest for tracking connectedness between grid points.
DIM connected(xc% * yc% - 1) AS DSFEntry

DSFInitialize connected()

' Prepare a list of the squares in the grid, and fill them in a random order.
DIM indices%(w% * h% - 1)

FOR i% = 0 TO UBOUND(indices%)
  indices%(i%) = i%
NEXT i%

Shuffle indices%()

' Fill in each one in turn
FOR i% = 0 TO UBOUND(indices%)
  y% = indices%(i%) \ w%
  x% = indices%(i%) MOD w%

  ' DEBUG
  'LOCATE 1, 1
  'FOR ddx% = -1 TO w%
  '  IF ddx% AND 1 THEN PRINT "+";  ELSE PRINT "-";
  'NEXT ddx%
  'PRINT
  'FOR ddy% = 0 TO h% - 1
  '  PRINT "|";
  '  FOR ddx% = 0 TO w% - 1
  '    IF ddx% = x% AND ddy% = y% THEN
  '      PRINT "?";
  '    ELSEIF solution%(ddx%, ddy%) < 0 THEN
  '      PRINT "\";
  '    ELSEIF solution%(ddx%, ddy%) > 0 THEN
  '      PRINT "/";
  '    ELSE
  '      PRINT " ";
  '    END IF
  '  NEXT ddx%
  '  PRINT "|"
  'NEXT ddy%
  'FOR ddx% = -1 TO w%
  '  IF ddx% AND 1 THEN PRINT "+";  ELSE PRINT "-";
  'NEXT ddx%

  topleft% = y% * xc% + x%
  topright% = y% * xc% + x% + 1
  bottomleft% = (y% + 1) * xc% + x%
  bottomright% = (y% + 1) * xc% + x% + 1

  IF bottomright% > UBOUND(connected) THEN STOP

  fs% = (DSFCanonify%(connected(), topleft%) = DSFCanonify%(connected(), bottomright%))
  bs% = (DSFCanonify%(connected(), bottomleft%) = DSFCanonify%(connected(), topright%))

  ' It isn't possible to get into a situation where we
  ' aren't allowed to place _either_ type of slash in a
  ' square. Thus, filled-grid generation never has to
  ' backtrack.
  '
  ' Proof (thanks to Gareth Taylor):
  '
  ' If it were possible, it would have to be because there
  ' was an existing path (not using this square) between the
  ' top-left and bottom-right corners of this square, and
  ' another between the other two. These two paths would
  ' have to cross at some point.
  '
  ' Obviously they can't cross in the middle of a square, so
  ' they must cross by sharing a point in common. But this
  ' isn't possible either: if you chessboard-colour all the
  ' points on the grid, you find that any continuous
  ' diagonal path is entirely composed of points of the same
  ' colour. And one of our two hypothetical paths is between
  ' two black points, and the other is between two white
  ' points - therefore they can have no point in common.
  IF fs% AND bs% THEN ERROR 5

  IF fs% THEN
    v% = 1
  ELSEIF bs% THEN
    v% = -1
  ELSE
    v% = INT(RND * 2) * 2 - 1
  END IF

  ok% = FillSquare%(xc%, yc%, solution%(), connected(), x%, y%, v%)

  IF NOT ok% THEN ERROR 5
NEXT i%
END SUB

SUB GetMouseState (mx%, my%, mb%)
DIM rin AS RegTypeX, rout AS RegTypeX

rin.AX = 3

CALL InterruptX(&H33, rin, rout)

mb% = rout.BX
mx% = rout.cx
my% = rout.dx
END SUB

SUB HideMouse
DIM rin AS RegTypeX, rout AS RegTypeX

rin.AX = 2

CALL InterruptX(&H33, rin, rout)
END SUB

FUNCTION IIF% (c%, a%, b%)
IF c% THEN IIF% = a% ELSE IIF% = b%
END FUNCTION

SUB InitializeClueGlyphs
SHARED clueglyphs%()

FOR i% = 0 TO 4
  o% = i% * 8

  FOR y% = 0 TO 4
    READ l$

    bit% = 32
    FOR x% = 3 TO 1 STEP -1
      IF MID$(l$, x%, 1) <> "." THEN clueglyphs%(o%) = clueglyphs%(o%) OR bit%
      bit% = bit% + bit%
    NEXT x%

    o% = o% + 1
  NEXT y%
NEXT i%
END SUB

SUB InitializeSlashGlyphs
SHARED slashglyphs%()

bits% = 64 OR 128 OR 256

FOR y% = 0 TO 7
  slashglyphs%(y%) = bits% AND 255
  slashglyphs%(15 - y%) = bits% AND 255

  bits% = bits% \ 2
NEXT y%
END SUB

SUB PlaceBoard (xc%, yc%, gx%, gy%, gw%, gh%)
gw% = xc% * 16 - 15
gh% = yc% * 16 - 15

gx% = (640 - gw%) \ 2
gy% = (350 - gh%) \ 2
END SUB

SUB RemoveCluePlaceholder (xc%, yc%, x%, y%)
PlaceBoard xc%, yc%, gx%, gy%, gw%, gh%

xx% = gx% + x% * 16
yy% = gy% + y% * 16

FOR dx% = -1 TO 1 STEP 2
  FOR dy% = -1 TO 1 STEP 2
    LINE (xx% + dx% * 4, yy% + dy% * 4)-(xx% + dx%, yy% + dy%), 0, BF
  NEXT dy%
NEXT dx%

x1% = xx% - 4: IF x1% < gx% THEN LINE (x1%, yy%)-(gx% - 1, yy%), 0: x1% = gx%
x2% = xx% + 4: IF x2% >= gx% + gw% THEN LINE (gx% + gw%, yy%)-(x2%, yy%), 0: x2% = gx% + gw% - 1
y1% = yy% - 4: IF y1% < gy% THEN LINE (xx%, y1%)-(xx%, gy% - 1), 0: y1% = gy%
y2% = yy% + 4: IF y2% >= gy% + gh% THEN LINE (xx%, gy% + gh%)-(xx%, y2%), 0: y2% = gy% + gh% - 1

LINE (xx%, y1%)-(xx%, y2%), 8
LINE (x1%, yy%)-(x2%, yy%), 8
END SUB

SUB ResetMouse
DIM rin AS RegTypeX, rout AS RegTypeX

rin.AX = 0

CALL InterruptX(&H33, rin, rout)
END SUB

SUB ShowMouse
DIM rin AS RegTypeX, rout AS RegTypeX

rin.AX = 1

CALL InterruptX(&H33, rin, rout)
END SUB

SUB Shuffle (a%())
l% = LBOUND(a%)
u% = UBOUND(a%)

FOR i% = u% TO l% + 1 STEP -1
  n% = INT(RND * (i% - l% + 1)) + l%

  IF n% <> i% THEN SWAP a%(i%), a%(n%)
NEXT i%
END SUB

' Solver. Returns 0 for impossibility, 1 for success, 2 for
' ambiguity or failure to converge.
FUNCTION Solve% (xc%, yc%, clues%(), solution%(), difficulty%, connected() AS DSFEntry, exits%(), border%(), equiv() AS DSFEntry, slashval%(), vbitmap%())
IF difficulty% <= 0 THEN Solve% = 3: EXIT FUNCTION ' short-circuit if we've been told to not even try

w% = xc% - 1
h% = yc% - 1

FOR x% = 0 TO w% - 1
  FOR y% = 0 TO h% - 1
    solution%(x%, y%) = 0
  NEXT y%
NEXT x%

' Establish a disjoint set forest for tracking connectedness
' between grid points.
DSFInitialize connected()

' Establish a disjoint set forest for tracking which squares
' are known to slant in the same direction.
DSFInitialize equiv()

' Clear the slashval array
' Set up the vbitmap array. Initially, all types of v are possible.
FOR x% = 0 TO w% - 1
  FOR y% = 0 TO h% - 1
    slashval%(x%, y%) = 0
    vbitmap%(x%, y%) = &HF
  NEXT y%
NEXT x%

' Initialise the `exits' and `border' arrays. These are used
' to do second-order loop avoidance: the dual of the no loops
' constraint is that every point must be somehow connected to
' the border of the grid (otherwise there would be a solid
' loop around it which prevented this).
'
' I define a `dead end' to be a connected group of points
' which contains no border point, and which can form at most
' one new connection outside itself. Then I forbid placing an
' edge so that it connects together two dead-end groups, since
' this would yield a non-border-connected isolated subgraph
' with no further scope to extend it.

FOR y% = 0 TO yc% - 1
  FOR x% = 0 TO xc% - 1
    border%(x%, y%) = (y% = 0) OR (y% = yc% - 1) OR (x% = 0) OR (x% = xc% - 1)

    IF clues%(x%, y%) < 0 THEN
      exits%(x%, y%) = 4
    ELSE
      exits%(x%, y%) = clues%(x%, y%)
    END IF
  NEXT x%
NEXT y%

' Repeatedly try to deduce something until we can't.
DIM neighbourx%(0 TO 3)
DIM neighboury%(0 TO 3)
DIM neighbourslash%(0 TO 3)

DO
  donesomething% = 0

  ' Any clue point with the number of remaining lines equal
  ' to zero or to the number of remaining undecided
  ' neighbouring squares can be filled in completely.
  FOR y% = 0 TO yc% - 1
    FOR x% = 0 TO xc% - 1
      c% = clues%(x%, y%)

      IF c% >= 0 THEN
        ' We have a clue point. Start by listing its
        ' neighbouring squares, in order around the point,
        ' together with the type of slash that would be
        ' required in that square to connect to the point.

        numneighbours% = 0

        IF x% > 0 THEN
          IF y% > 0 THEN
            neighbourx%(numneighbours%) = x% - 1
            neighboury%(numneighbours%) = y% - 1
            neighbourslash%(numneighbours%) = -1
            numneighbours% = numneighbours% + 1
          END IF

          IF y% < h% THEN
            neighbourx%(numneighbours%) = x% - 1
            neighboury%(numneighbours%) = y%
            neighbourslash%(numneighbours%) = 1
            numneighbours% = numneighbours% + 1
          END IF
        END IF

        IF x% < w% THEN
          IF y% < h% THEN
            neighbourx%(numneighbours%) = x%
            neighboury%(numneighbours%) = y%
            neighbourslash%(numneighbours%) = -1
            numneighbours% = numneighbours% + 1
          END IF

          IF y% > 0 THEN
            neighbourx%(numneighbours%) = x%
            neighboury%(numneighbours%) = y% - 1
            neighbourslash%(numneighbours%) = 1
            numneighbours% = numneighbours% + 1
          END IF
        END IF

        ' Count up the number of undecided neighbours, and
        ' also the number of lines already present.
        '
        ' If we're not on DIFFEasy%, then in this loop we
        ' also track whether we've seen two adjacent empty
        ' squares belonging to the same equivalence class
        ' (meaning they havet he same type of slash). If
        ' so, we count them jointly as one line.
        nu% = 0
        nl% = c%

        lastx% = neighbourx%(numneighbours% - 1)
        lasty% = neighboury%(numneighbours% - 1)

        IF solution%(lastx%, lasty%) = 0 THEN
          eqx% = lastx%: eqy% = lasty%
          DSFCanonify2D w%, h%, equiv(), eqx%, eqy%
        ELSE
          eqx% = -1: eqy% = -1
        END IF

        meqx% = -1: meqy% = -1
        mjx1% = -1: mjy1% = -1
        mjx2% = -1: mjy2% = -1

        FOR i% = 0 TO numneighbours% - 1
          jx% = neighbourx%(i%)
          jy% = neighboury%(i%)
          s% = neighbourslash%(i%)

          IF solution%(jx%, jy%) = 0 THEN
            nu% = nu% + 1 ' undecided
            IF meqx% < 0 AND difficulty% > DIFFEasy% THEN
              eq2x% = jx%: eq2y% = jy%
              DSFCanonify2D w%, h%, equiv(), eq2x%, eq2y%

              IF eqx% = eq2x% AND eqy% = eq2y% AND (lastx% <> jx% OR lasty% <> jy%) THEN
                ' We've found an equivalent pair.
                ' Mark it. This also inhibits any
                ' further equivalence tracking
                ' around this square, since we can
                ' only handle one pair (and in
                ' particular we want to avoid
                ' being misled by two overlapping
                ' equivalence pairs).

                meqx% = eqx%: meqy% = eqy%
                mjx1% = lastx%: mjy1% = lasty%
                mjx2% = jx%: mjy2% = jy%

                nl% = nl% - 1 ' count one line
                nu% = nu% - 2 ' and lose two undecideds
              ELSE
                eqx% = eq2x%
                eqy% = eq2y%
              END IF
            END IF
          ELSE
            eqx% = -1
            eqy% = -1

            IF solution%(jx%, jy%) = s% THEN
              nl% = nl% - 1 ' here's a line
            END IF
          END IF

          lastx% = jx%
          lasty% = jy%
        NEXT i%

        ' Check the counts
        IF nl% < 0 OR nl% > nu% THEN
          ' No consistent value for this at all!
          Solve% = 0
          EXIT FUNCTION
        END IF

        IF nu% > 0 AND (nl% = 0 OR nl% = nu%) THEN
          FOR i% = 0 TO numneighbours% - 1
            jx% = neighbourx%(i%)
            jy% = neighboury%(i%)
            s% = neighbourslash%(i%)

            IF solution%(jx%, jy%) = 0 AND (jx% <> mjx1% OR jy% <> mjy1%) AND (jx% <> mjx2% OR jy% <> mjy2%) THEN
              IF nl% = 0 THEN s% = -s%

              IF NOT FillSquareAndTrackExits%(xc%, yc%, solution%(), connected(), jx%, jy%, s%, -1, exits%(), border%(), equiv(), slashval%(), clues%()) THEN
                retry% = FillSquareAndTrackExits%(xc%, yc%, solution%(), connected(), jx%, jy%, s%, -1, exits%(), border%(), equiv(), slashval%(), clues%())
                ' Impossible
                Solve% = 0
                EXIT FUNCTION
              END IF
            END IF
          NEXT i%

          donesomething% = -1
        ELSEIF nu% = 2 AND nl% = 1 AND difficulty% > DIFFEasy% THEN
          ' If we have precisely two undecided squares
          ' and precisely one line to place between
          ' them, _and_ those squares are adjacent, then
          ' we can mark them as equivalent to one
          ' another.
          '
          ' This even applies if meq >= 0: if we have a
          ' 2 clue point and two of its neighbours are
          ' already marked equivalent, we can indeed
          ' mark the other two as equivalent.
          '
          ' We don't bother with this on DIFFEasy,
          ' since we wouldn't have used the results
          ' anyway.
          last% = -1

          FOR i% = 0 TO numneighbours% - 1
            jx% = neighbourx%(i%)
            jy% = neighboury%(i%)

            IF solution%(jx%, jy%) = 0 AND (jx% <> mjx1% OR jy% <> mjy1%) AND (jx% <> mjx2% OR jy% <> mjy2%) THEN
              IF last% < 0 THEN
                last% = i%
              ELSEIF last% = i% - 1 OR last% = 0 AND i% = 3 THEN
                EXIT FOR ' found a pair
              END IF
            END IF
          NEXT i%

          IF i% < numneighbours% THEN
            IF last% < 0 THEN ERROR 5

            ' neighbours(last) and neighbours(i) are the pair. Mark them equivalent.
            mjx1% = neighbourx%(last%)
            mjy1% = neighboury%(last%)

            mjx2% = neighbourx%(i%)
            mjy2% = neighboury%(i%)

            DSFCanonify2D w%, h%, equiv(), mjx1%, mjy1%
            sv1% = slashval%(mjx1%, mjy1%)

            DSFCanonify2D w%, h%, equiv(), mjx2%, mjy2%
            sv2% = slashval%(mjx2%, mjy2%)

            IF sv1% <> 0 AND sv2% <> 0 AND sv1% <> sv2% THEN
              ' Bug: Merged two equivalence classes with different slash values!
              Solve% = 0
              EXIT FUNCTION
            END IF

            IF sv1% <> 0 THEN sv% = sv1% ELSE sv% = sv2%

            DSFMerge2D w%, h%, equiv(), mjx1%, mjy1%, mjx2%, mjy2%
            DSFCanonify2D w%, h%, equiv(), mjx1%, mjy1%

            slashval%(mjx1%, mjy1%) = sv%
          END IF
        END IF
      END IF
    NEXT x%
  NEXT y%

  IF NOT donesomething% THEN
    ' Failing that, we now apply the second condition, which
    ' is that no square may be filled in such a way as to form
    ' a loop. Also in this loop (since it's over squares
    ' rather than points), we check slashval to see if we've
    ' already filled in another square in the same equivalence
    ' class.
    '
    ' The slashval check is disabled on DIFFEasy%, as is dead
    ' end avoidance. Only _immediate_ loop avoidance remains.

    FOR y% = 0 TO h% - 1
      FOR x% = 0 TO w% - 1
        IF solution%(x%, y%) = 0 THEN
          fs% = 0
          bs% = 0

          IF difficulty% > DIFFEasy% THEN
            sx% = x%: sy% = y%

            DSFCanonify2D w%, h%, equiv(), sx%, sy%

            v% = slashval%(sx%, sy%)
          ELSE
            v% = 0
          END IF

          ' Try to rule out connectivity between (x, y) and
          ' (x+1, y+1); if successful, we will deduce that we
          ' must have a forward slash.
          c1x% = x%: c1y% = y%
          DSFCanonify2D xc%, yc%, connected(), c1x%, c1y%

          c2x% = x% + 1: c2y% = y% + 1
          DSFCanonify2D xc%, yc%, connected(), c2x%, c2y%

          IF c1x% = c2x% AND c1y% = c2y% THEN
            fs% = -1 ' simple loop avoidance
          ELSEIF difficulty% > DIFFEasy% AND (border%(c1x%, c1y%) = 0) AND (border%(c2x%, c2y%) = 0) AND (exits%(c1x%, c1y%) <= 1) AND (exits%(c2x%, c2y%) <= 1) THEN
            fs% = -1 ' dead end avoidance
          ELSEIF v% = 1 THEN
            fs% = -1 ' equivalence to an already filled square
          END IF

          ' Now do the same between (x+1, y) and (x, y+1), to
          ' see if we are required to have a backslash.
          c1x% = x% + 1: c1y% = y%
          DSFCanonify2D xc%, yc%, connected(), c1x%, c1y%

          c2x% = x%: c2y% = y% + 1
          DSFCanonify2D xc%, yc%, connected(), c2x%, c2y%

          IF c1x% = c2x% AND c1y% = c2y% THEN
            bs% = -1 ' simple loop avoidance
          ELSEIF difficulty% > DIFFEasy% AND (border%(c1x%, c1y%) = 0) AND (border%(c2x%, c2y%) = 0) AND (exits%(c1x%, c1y%) <= 1) AND (exits%(c2x%, c2y%) <= 1) THEN
            bs% = -1 ' dead end avoidance
          ELSEIF v% = -1 THEN
            bs% = -1 ' equivalence to an already filled square
          END IF

          IF fs% AND bs% THEN
            ' No consistent value for this at all!
            Solve% = 0
            EXIT FUNCTION
          END IF

          IF fs% THEN
            IF NOT FillSquareAndTrackExits%(xc%, yc%, solution%(), connected(), x%, y%, 1, -1, exits%(), border%(), equiv(), slashval%(), clues%()) THEN
              ' impossible
              Solve% = 0
              EXIT FUNCTION
            END IF

            donesomething% = -1
          ELSEIF bs% THEN
            IF NOT FillSquareAndTrackExits%(xc%, yc%, solution%(), connected(), x%, y%, -1, -1, exits%(), border%(), equiv(), slashval%(), clues%()) THEN
              ' impossible
              Solve% = 0
              EXIT FUNCTION
            END IF

            donesomething% = -1
          END IF
        END IF
      NEXT x%
    NEXT y%

    IF NOT donesomething% AND (difficulty% > DIFFEasy%) THEN
      ' Now see what we can do with the vbitmap array. All
      ' vbitmap deductions are disabled at Easy level.

      FOR y% = 0 TO h% - 1
        FOR x% = 0 TO w% - 1
          ' Any line already placed in a square must rule
          ' out any type of v which contradicts it.
          s% = solution%(x%, y%)
          IF s% <> 0 THEN
            IF x% > 0 THEN donesomething% = donesomething% OR UpdateVBitmap%(vbitmap%(), x% - 1, y%, IIF(s% < 0, 1, 2))
            IF x% + 1 < w% THEN donesomething% = donesomething% OR UpdateVBitmap%(vbitmap%(), x%, y%, IIF(s% < 0, 1, 2))
            IF y% > 0 THEN donesomething% = donesomething% OR UpdateVBitmap%(vbitmap%(), x%, y% - 1, IIF(s% < 0, 1, 2))
            IF y% + 1 < h% THEN donesomething% = donesomething% OR UpdateVBitmap%(vbitmap%(), x%, y%, IIF(s% < 0, 1, 2))
          END IF

          ' If both types of v are ruled out for a pair of
          ' adjacent squares, mark them as equivalent.
          IF x% + 1 < w% AND (vbitmap%(x%, y%) AND 3) = 0 THEN
            n1% = y% * w% + x%: n2% = y% * w% + (x% + 1)
            IF DSFCanonify%(equiv(), n1%) <> DSFCanonify%(equiv(), n2%) THEN
              DSFMerge equiv(), n1%, n2%
              donesomething% = -1
            END IF
          END IF

          IF y% + 1 < h% AND (vbitmap%(x%, y%) AND &HC) = 0 THEN
            n1% = y% * w% + x%: n2% = (y% + 1) * w% + x%
            IF DSFCanonify%(equiv(), n1%) <> DSFCanonify%(equiv(), n2%) THEN
              DSFMerge equiv(), n1%, n2%
              donesomething% = -1
            END IF
          END IF

          ' The remaining work in this loop only works
          ' around non-edge clue points.

          IF x% > 0 AND y% > 0 THEN
            c% = clues%(x%, y%)

            IF c% >= 0 THEN
              ' x,y marks a clue point not on the grid edge. See
              ' if this clue point allows us to rule out any v
              ' shapes.

              SELECT CASE c%
                CASE 1:
                  ' A 1 clue can never have any v shape pointing
                  ' at it.

                  donesomething% = donesomething% OR UpdateVBitmap(vbitmap%(), x% - 1, y% - 1, &H5)
                  donesomething% = donesomething% OR UpdateVBitmap(vbitmap%(), x% - 1, y%, &H2)
                  donesomething% = donesomething% OR UpdateVBitmap(vbitmap%(), x%, y% - 1, &H8)
                CASE 3:
                  ' A 3 clue can never have any v shape pointing
                  ' away from it.
                  donesomething% = donesomething% OR UpdateVBitmap(vbitmap%(), x% - 1, y% - 1, &HA)
                  donesomething% = donesomething% OR UpdateVBitmap(vbitmap%(), x% - 1, y%, &H1)
                  donesomething% = donesomething% OR UpdateVBitmap(vbitmap%(), x%, y% - 1, &H4)
                CASE 2:
                  ' If a 2 clue has any kind of v ruled out on
                  ' one side of it, the same v is ruled out on
                  ' the other side.
                  donesomething% = donesomething% OR UpdateVBitmap(vbitmap%(), x% - 1, y% - 1, (vbitmap%(x% - 1, y%) AND &H3) XOR &H3)
                  donesomething% = donesomething% OR UpdateVBitmap(vbitmap%(), x% - 1, y% - 1, (vbitmap%(x%, y% - 1) AND &HC) XOR &HC)
                  donesomething% = donesomething% OR UpdateVBitmap(vbitmap%(), x% - 1, y%, (vbitmap%(x% - 1, y% - 1) AND &H3) XOR &H3)
                  donesomething% = donesomething% OR UpdateVBitmap(vbitmap%(), x%, y% - 1, (vbitmap%(x% - 1, y% - 1) AND &HC) XOR &HC)
              END SELECT
            END IF
          END IF
        NEXT x%
      NEXT y%
    END IF
  END IF
LOOP WHILE donesomething%

' Solver can make no more progress. See if the grid is full.
FOR y% = 0 TO h% - 1
  FOR x% = 0 TO w% - 1
    IF solution%(x%, y%) = 0 THEN
      Solve% = 2 ' failed to converge
      EXIT FUNCTION
    END IF
  NEXT x%
NEXT y%

Solve% = 1
END FUNCTION

' Wrapper on DSFMerge which updates the `exits` and `border` arrays.
SUB SolveMergeVertices (xc%, yc%, connected() AS DSFEntry, exits%(), border%(), ix%, iy%, jx%, jy%)
DSFCanonify2D xc%, yc%, connected(), ix%, iy%
DSFCanonify2D xc%, yc%, connected(), jx%, jy%

' We have used one possible exit from each of the two
' classes. Thus, the viable exit count of the new class is
' the sum of the old exit counts minus two.

numexits% = exits%(ix%, iy%) + exits%(jx%, jy%) - 2
isborder% = border%(ix%, iy%) OR border%(jx%, jy%)

DSFMerge2D xc%, yc%, connected(), ix%, iy%, jx%, jy%

DSFCanonify2D xc%, yc%, connected(), ix%, iy%

exits%(ix%, iy%) = numexits%
border%(ix%, iy%) = isborder%
END SUB

FUNCTION UpdateVBitmap% (vbitmap%(), x%, y%, vbits%)
donesomething% = 0

vbit% = 1
DO WHILE vbit% <= 8
  IF vbits% AND vbitmap%(x%, y%) AND vbit% THEN
    donesomething% = -1

    vbitmap%(x%, y%) = vbitmap%(x%, y%) AND NOT vbit%
  END IF

  vbit% = vbit% + vbit%
LOOP

UpdateVBitmap% = donesomething%
END FUNCTION

' Utility function to return the current degree of a vertex. If
' `anti' is set, it returns the number of filled-in edges
' surrounding the point which _don't_ connect to it; thus 4 minus
' its anti-degree is the maximum degree it could have if all the
' empty spaces around it were filled in.
'
' (Yes, _4_ minus its anti-degree even if it's a border vertex.)
'
' If ret > 0, *sx and *sy are set to the coordinates of one of the
' squares that contributed to it.
FUNCTION VertexDegree% (xc%, yc%, board%(), x%, y%, anti%, sx%, sy%)
ret% = 0

' F/B  E   anti   slash   sl-ant    should count
'  0   0    0      -1      -1         1
'  0   1    0       0       0         0
'  1   0    0       1       1         0
'  1   1    0       0       0         0
'  0   0    1      -1      -2         1
'  0   1    1       0      -1         1
'  1   0    1       1       0         0
'  1   1    1       0      -1         1

IF anti% THEN anti% = -1

slashmask% = BOARDForwardSlash% OR BOARDBackwardSlash%

IF x% > 0 THEN dx1% = -1 ELSE dx1% = 0
IF x% < xc% - 1 THEN dx2% = 0 ELSE dx2% = -1

IF y% > 0 THEN dy1% = -1 ELSE dy1% = 0
IF y% < yc% - 1 THEN dy2% = 0 ELSE dy2% = -1

FOR dx% = dx1% TO dx2%
  xx% = x% + dx%

  FOR dy% = dy1% TO dy2%
    yy% = y% + dy%

    IF (dx% + dy%) AND 1 THEN
      connectingslash% = BOARDForwardSlash%
    ELSE
      connectingslash% = BOARDBackwardSlash%
    END IF

    b% = board%(xx%, yy%)

    isempty% = (b% AND BOARDEmpty%) <> 0
    slash% = b% AND slashmask%

    countbecauseempty% = isempty% AND anti%
    countbecauseslash% = (slash% = connectingslash%) AND NOT isempty%

    IF countbecauseempty% OR countbecauseslash% THEN
      sx% = xx%
      sy% = yy%
      ret% = ret% + 1
    END IF
  NEXT dy%
NEXT dx%

IF anti% THEN VertexDegree% = 4 - ret% ELSE VertexDegree% = ret%
END FUNCTION

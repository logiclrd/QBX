TYPE PathEnd
  PathX AS INTEGER
  PathY AS INTEGER
  PathDX AS INTEGER
  PathDY AS INTEGER
END TYPE

TYPE HexPart
  BaseShape AS INTEGER
  OffsetX AS INTEGER
  OffsetY AS INTEGER
  MergeWithPart AS INTEGER
  MergeDX AS INTEGER
  MergeDY AS INTEGER
  PathPoints(-2 TO 2) AS PathEnd
END TYPE

TYPE PathInfo
  FromEdge AS INTEGER
  ToEdge AS INTEGER
END TYPE

TYPE PathSet
  Path(1 TO 3) AS PathInfo
END TYPE

DECLARE SUB DrawPath (thispath AS PathInfo, otherpaths AS PathSet)
DECLARE SUB DrawPathPart (drawpart%, frompartrelative%, topartrelative%, otherpaths AS ANY)
DECLARE SUB GenerateBaseShapes ()
DECLARE SUB GenerateCharacters ()
DECLARE SUB InitializePathSet (paths AS PathSet)
DECLARE SUB InitializeScreen ()
DECLARE SUB InitializeTiles ()
DECLARE SUB LoadHexParts ()
DECLARE FUNCTION MapCharacter% (glyph%())
DECLARE SUB RenderTiles (cmap%())
DECLARE SUB RotatePathInfo (thispath AS PathInfo)
DECLARE SUB RotatePathSet (paths AS PathSet)
DECLARE SUB SetFontCharacter (charcode%, pixels%())
DECLARE SUB TransferCharactersToFont ()

CONST NumColumns% = 80
CONST NumRows% = 50

DIM SHARED BaseShape%(33, 3)
DIM SHARED HexParts(0 TO 5) AS HexPart

CONST FirstCharacter% = 33
DIM SHARED Characters%(33, 255)
DIM SHARED NextCharacter%

NextCharacter% = FirstCharacter%

DIM SHARED CharacterMap%(0 TO 5, 0 TO 5, 0 TO 5)
DIM SHARED CharacterMapWithGrid%(0 TO 5, 0 TO 5, 0 TO 5)

' Each hex tile is two columns wide, three rows tall, and overlaps
' with the next row by 1. Every other row is offset by half a hex tile.
'
' For 80 columns, one row is filled with the middles of 40 tiles. The
' rows above and below are filled with 39 tiles, plus
' half one to left and half one to the right, So, we need to store 41, but
' every other grid row only has 40, one is entirely off screen. If we
' offset the odd rows to the right by 1/2 tile, then the even rows have one
' tile that starts to the left of the screen.
'
' Every other character row combines data from two different grid rows,
' interleaving the bottom corners of tiles above with the top corners of
' tiles below. With an even number of character rows, it is then impossible
' to make both the first row and the last row be ones that do not mix two
' rows. Thus, we need to store one extra row of tile data. If we make row
' 1 the interleaving row, then we have an extra row that starts before the
' top of the screen. This is consistent with the even rows starting before
' the left of the screen.
'
' With the array from 0 TO 40, 0 TO 25, it is row 0 that begins above the
' top of the screen and column 0 that is at least half off to the left,
' fully off-screen on even rows.

DIM SHARED Tiles%(40, 25)

SCREEN 13

LoadHexParts
GenerateBaseShapes
GenerateCharacters

InitializeTiles

InitializeScreen

TransferCharactersToFont

showgrid% = 0

DO
  IF showgrid% THEN
    RenderTiles CharacterMapWithGrid%()
  ELSE
    RenderTiles CharacterMap%()
  END IF

  DO: a$ = INKEY$: LOOP WHILE a$ = ""

  SELECT CASE LCASE$(a$)
    CASE "g": showgrid% = NOT showgrid%
    CASE "r": InitializeTiles
  END SELECT
LOOP UNTIL a$ = CHR$(27) ' Esc

SCREEN 13: SCREEN 0


'''''''''''''''''''''''''''''''''''''''''''''''''''''''
' Hex parts (follows Cartesian angles):
'
'    2  / \  1
'    3 |   | 0
'    4  \ /  5
'
' Path options
' Part 0
DATA 3,8,8: ' Base shape
DATA -1,0,0: ' Does not merge with another part
DATA 0,0, 1,1: ' Connection to part 2
DATA 3,0, 0,1: ' Connection to part 1
DATA 7,3, -1,0: ' Self connect out
DATA 3,7, 0,-1: ' Connection to part 5
DATA 0,7, 1,-1: ' Connection to part 4
' Part 1
DATA 1,8,0: ' Base shape
DATA 4,0,-1: ' Merges with part 4, found at relative tile (0,-1)
DATA 0,7, 1,-1: ' Connection to part 3
DATA 0,3, 1,0: ' Connection to part 2
DATA 0,0, 0,0: ' Connect out is through merge
DATA 3,7, 0,-1: ' Connection to part 0
DATA 3,7, 0,-1: ' Connection to part 5
' Part 2
DATA 0,0,0: ' Base shape
DATA 5,-1,-1: ' Merges with part 5, found at relative tile (-1,-1)
DATA 5,7, 0,-1: ' Connection to part 4
DATA 4,7, 0,-1: ' Connection to part 3
DATA 0,0, 0,0: ' Connect out is through merge
DATA 7,3, -1,0: ' Connection to part 1
DATA 7,7, -1,-1: ' Connection to part 0
' Part 3
DATA 2,0,8: ' Base shape
DATA -1,0,0: ' Does not merge with another part
DATA 7,7, 1,1: ' Connection to part 5
DATA 5,7, 0,-1: ' Connection to part 4
DATA 0,3, 1,0: ' Self connect out
DATA 5,0, 0,1: ' Connection to part 2
DATA 7,0, -1,1: ' Connection to part 1
' Part 4
DATA 1,0,16: ' Base shape
DATA 1,-1,1: ' Merges with part 1, found at relative tile (-1,1)
DATA 7,0, -1,1: ' Connection to part 0
DATA 7,3, -1,0: ' Connection to part 5
DATA 0,0, 0,0: ' Connect out is through merge
DATA 4,0, 0,1: ' Connection to part 3
DATA 5,0, 0,1: ' Connection to part 2
' Part 5
DATA 0,8,16: ' Base shape
DATA 2,0,1: ' Merges with part 2, found at relative tile (0,1)
DATA 3,0, 0,1: ' Connection to part 1
DATA 3,0, 0,1: ' Connection to part 0
DATA 0,0, 0,0: ' Connect out is through merge
DATA 0,3, 1,0: ' Connection to part 4
DATA 0,0, 1,1: ' Connection to part 3

SUB DrawPath (thispath AS PathInfo, otherpaths AS PathSet)
drawpart% = thispath.FromEdge
DO
  IF (drawpart% = 0) OR (drawpart% = 3) OR (drawpart% = thispath.FromEdge) OR (drawpart% = thispath.ToEdge) THEN
    DrawPathPart drawpart%, thispath.FromEdge - drawpart%, thispath.ToEdge - drawpart%, otherpaths
  END IF

  IF drawpart% = thispath.ToEdge THEN EXIT DO
  drawpart% = (drawpart% + 1) MOD 6
LOOP
END SUB

SUB DrawPathPart (drawpart%, frompartrelative%, topartrelative%, otherpaths AS PathSet)
frompart% = drawpart%
topart% = drawpart%

IF (drawpart% <> 0) AND (drawpart% <> 3) THEN
  IF frompartrelative% = 0 THEN
    frompart% = HexParts(frompart%).MergeWithPart

    FOR i% = 1 TO 3
      IF otherpaths.Path(i%).FromEdge = frompart% THEN frompartrelative% = otherpaths.Path(i%).ToEdge - frompart%: EXIT FOR
      IF otherpaths.Path(i%).ToEdge = frompart% THEN frompartrelative% = otherpaths.Path(i%).FromEdge - frompart%: EXIT FOR
    NEXT i%

    IF frompartrelative% = 0 THEN STOP
  END IF

  IF topartrelative% = 0 THEN
    topart% = HexParts(topart%).MergeWithPart

    FOR i% = 1 TO 3
      IF otherpaths.Path(i%).FromEdge = topart% THEN topartrelative% = otherpaths.Path(i%).ToEdge - topart%: EXIT FOR
      IF otherpaths.Path(i%).ToEdge = topart% THEN topartrelative% = otherpaths.Path(i%).FromEdge - topart%: EXIT FOR
    NEXT i%

    IF topartrelative% = 0 THEN STOP
  END IF
END IF

IF frompartrelative% < -2 THEN frompartrelative% = frompartrelative% + 6
IF frompartrelative% > 2 THEN frompartrelative% = frompartrelative% - 6
IF topartrelative% < -2 THEN topartrelative% = topartrelative% + 6
IF topartrelative% > 2 THEN topartrelative% = topartrelative% - 6

DIM fromend AS PathEnd
DIM toend AS PathEnd

fromend = HexParts(frompart%).PathPoints(frompartrelative%)
toend = HexParts(topart%).PathPoints(topartrelative%)

basex% = HexParts(drawpart%).OffsetX
basey% = HexParts(drawpart%).OffsetY

FOR i% = 0 TO 10
  invi% = 10 - i%

  t! = i% / 10
  invt! = 1 - t!

  x1% = fromend.PathX + i% * fromend.PathDX / 3
  y1% = fromend.PathY + i% * fromend.PathDY / 3

  x2% = toend.PathX + invi% * toend.PathDX / 5
  y2% = toend.PathY + invi% * toend.PathDY / 5

  x% = x1% * invt! + x2% * t!
  y% = y1% * invt! + y2% * t!

  IF i% = 0 THEN
    PSET (basex% + x%, basey% + y%)
  ELSE
    LINE -(basex% + x%, basey% + y%)
  END IF
NEXT i%
END SUB

SUB GenerateBaseShapes
LINE (0, 7)-(7, 0), 8
LINE (9, 0)-(15, 6), 8
LINE (16, 0)-(16, 7), 8

GET (0, 0)-(7, 7), BaseShape%(0, 0)
GET (8, 0)-(15, 7), BaseShape%(0, 1)
GET (16, 0)-(23, 7), BaseShape%(0, 2)
GET (24, 0)-(31, 7), BaseShape%(0, 3)

CLS
END SUB

SUB GenerateCharacters
' We are only considering one type of tile: The type that has two
' connections that skip one edge interleaved, and then a connection
' joining the remaining two edges. Technically, this is two configurations
' because the interleaved edges can overlap in two different orders,
' but we're drawing paths as a single pixel, so the overlap is not
' distinguishable.
'
' We are drawing the tiles using 6 characters:
'
' 2  / \  1
' 3 |   | 0   (corresponding with the order of Cartesian angles)
' 4  \ /  5
'
' Part 3 contains the vertical wall. Part 0 does not include a wall, because
' this would make the wall 2 pixels wide here.
'
' Parts 1, 2, 4 and 4 all contain bits of two different tiles.
'
' So, for parts 0 and 3, we need 12 characters, one per part per rotation.
' For parts 1, 2, 4, and 5, we need 144 characters, six per part per
' rotation.
'
' To simplify, we store every possible mapping of
' (part, rotation, otherpartrotation). There are some duplicates; for
' instance, as parts 0 and 3 don't have an other part at all, they
' have the same mapped character for every otherpartrotation.

DIM glyph%(33)

DIM paths AS PathSet, otherpaths AS PathSet

FOR showgrid% = -1 TO 0
  InitializePathSet paths

  FOR rotation% = 0 TO 5
    InitializePathSet otherpaths

    FOR otherpartrotation% = 0 TO 5
      IF showgrid% THEN
        FOR part% = 0 TO 5
          PUT (HexParts(part%).OffsetX, HexParts(part%).OffsetY), BaseShape%(0, HexParts(part%).BaseShape), PSET
        NEXT part%
      ELSE
        LINE (0, 0)-(15, 23), 0, BF
      END IF

      FOR pathindex% = 1 TO 3
        DrawPath paths.Path(pathindex%), otherpaths
      NEXT pathindex%

      FOR part% = 0 TO 5
        GET (HexParts(part%).OffsetX, HexParts(part%).OffsetY)-STEP(7, 7), glyph%
        CharacterMap%(part%, rotation%, otherpartrotation%) = MapCharacter(glyph%())
      NEXT part%

      RotatePathSet otherpaths
    NEXT otherpartrotation%

    RotatePathSet paths
  NEXT rotation%

  IF showgrid% THEN
    FOR part% = 0 TO 5
      FOR rotation% = 0 TO 5
        FOR otherpartrotation% = 0 TO 5
          CharacterMapWithGrid%(part%, rotation%, otherpartrotation%) = CharacterMap%(part%, rotation%, otherpartrotation%)
        NEXT otherpartrotation%
      NEXT rotation%
    NEXT part%
  END IF
NEXT showgrid%
END SUB

SUB InitializePathSet (paths AS PathSet)
paths.Path(1).FromEdge = 0: paths.Path(1).ToEdge = 2
paths.Path(2).FromEdge = 1: paths.Path(2).ToEdge = 3
paths.Path(3).FromEdge = 4: paths.Path(3).ToEdge = 5
END SUB

SUB InitializeScreen
SCREEN 0
WIDTH NumColumns%, NumRows%

CONST MiscOutputReadPort% = &H3CC
CONST MiscOutputWritePort% = &H3C2

CONST ClockMask% = 12
CONST Clock25MHz% = 0

mode% = INP(MiscOutputReadPort%)

mode% = mode% AND NOT ClockMask%
mode% = mode% OR Clock25MHz%

OUT MiscOutputWritePort%, mode%

CONST SequencerIndexPort% = &H3C4
CONST SequencerDataPort% = &H3C5

CONST ClockingModeRegister% = 1
CONST ClockingModeCharacterWidth8% = 1

OUT SequencerIndexPort%, ClockingModeRegister%
mode% = INP(SequencerDataPort%)

OUT SequencerIndexPort%, ClockingModeRegister%
OUT SequencerDataPort%, mode% OR ClockingModeCharacterWidth8%
END SUB

SUB InitializeTiles
FOR i% = 0 TO 40
  FOR j% = 0 TO 25
    Tiles%(i%, j%) = INT(RND * 6)
  NEXT j%
NEXT i%
END SUB

SUB LoadHexParts
FOR i% = 0 TO 5
  READ HexParts(i%).BaseShape
  READ HexParts(i%).OffsetX, HexParts(i%).OffsetY
  READ HexParts(i%).MergeWithPart
  READ HexParts(i%).MergeDX, HexParts(i%).MergeDY

  FOR j% = 2 TO -2 STEP -1
    READ HexParts(i%).PathPoints(j%).PathX
    READ HexParts(i%).PathPoints(j%).PathY
    READ HexParts(i%).PathPoints(j%).PathDX
    READ HexParts(i%).PathPoints(j%).PathDY
  NEXT j%
NEXT i%
END SUB

FUNCTION MapCharacter% (glyph%())
FOR i% = 0 TO NextCharacter% - 1
  ismatch% = -1

  FOR j% = 0 TO 33
    IF glyph%(j%) <> Characters%(j%, i%) THEN
      ismatch% = 0
      EXIT FOR
    END IF
  NEXT j%

  IF ismatch% THEN
    MapCharacter% = i%
    EXIT FUNCTION
  END IF
NEXT i%

DIM pixels%(7, 7)

FOR j% = 0 TO 33
  Characters%(j%, NextCharacter%) = glyph%(j%)
NEXT j%

MapCharacter% = NextCharacter%
NextCharacter% = NextCharacter% + 1
END FUNCTION

SUB RenderTiles (cmap%())
DEF SEG = &HB800

o% = 0

tiley% = 0

FOR y% = 0 TO 49
  tilex% = 1

  advancex% = 1

  SELECT CASE y% AND 3
    CASE 0: evenpart% = 4: oddpart% = 5
    CASE 1: evenpart% = 0: oddpart% = 3: advancex% = 0: tilex% = tilex% - 1
    CASE 2: evenpart% = 5: oddpart% = 4: advancex% = 0: tilex% = tilex% - 1
    CASE 3: evenpart% = 3: oddpart% = 0
  END SELECT

  FOR x% = 0 TO 79
    IF (x% AND 1) = 0 THEN
      part% = evenpart%
    ELSE
      part% = oddpart%
    END IF

    r% = Tiles%(tilex%, tiley%)

    IF (y% AND 1) = 0 THEN
      othertiley% = tiley% + 1

      SELECT CASE y% AND 3
        CASE 0:
          othertilex% = tilex% - 1 + (x% AND 1)
        CASE 2:
          othertilex% = tilex% + 1 - (x% AND 1)
      END SELECT

      otherr% = Tiles%(othertilex%, othertiley%)
    END IF

    ch% = cmap%(part%, r%, otherr%)

    POKE o%, ch%
    o% = o% + 2

    IF (x% AND 1) = advancex% THEN tilex% = tilex% + 1
  NEXT x%

  tilex% = 0

  FOR x% = 0 TO 79
    IF (x% AND 1) = 0 THEN tilex% = tilex% + 1

    r% = Tiles%(tilex%, tiley%)
  NEXT x%

  IF (y% AND 1) = 0 THEN tiley% = tiley% + 1
NEXT y%
END SUB

SUB RotatePathInfo (thispath AS PathInfo)
thispath.FromEdge = (thispath.FromEdge + 1) MOD 6
thispath.ToEdge = (thispath.ToEdge + 1) MOD 6
END SUB

SUB RotatePathSet (paths AS PathSet)
FOR i% = 1 TO 3
  RotatePathInfo paths.Path(i%)
NEXT i%
END SUB

SUB SetFontCharacter (charcode%, pixels%())
CONST SequencerIndexPort% = &H3C4
CONST SequencerDataPort% = &H3C5

CONST MapMaskRegister% = 2
CONST MapMaskPlane0% = 1
CONST MapMaskPlane1% = 2
CONST MapMaskPlane2% = 4
CONST MapMaskPlane3% = 8

CONST MemoryModeRegister% = 4
CONST MemoryModeOddEvenDisable% = 4

CONST GraphicsIndexPort% = &H3CE
CONST GraphicsDataPort% = &H3CF

CONST MiscGraphicsRegister% = 6
CONST MiscGraphicsChainOddEven% = 2

OUT SequencerIndexPort%, MapMaskRegister%
savedmask% = INP(SequencerDataPort%)
OUT SequencerIndexPort%, MemoryModeRegister%
savedmode% = INP(SequencerDataPort%)
OUT GraphicsIndexPort%, MiscGraphicsRegister%
savedmisc% = INP(GraphicsDataPort%)

OUT SequencerIndexPort%, MapMaskRegister%
OUT SequencerDataPort%, MapMaskPlane2%

OUT SequencerIndexPort%, MemoryModeRegister%
OUT SequencerDataPort%, savedmode% OR MemoryModeOddEvenDisable%

OUT GraphicsIndexPort%, MiscGraphicsRegister%
OUT GraphicsDataPort%, savedmisc% AND NOT MiscGraphicsChainOddEven%

DEF SEG = &HB800

baseoffset% = charcode% * 32

FOR y% = 0 TO 7
  scan% = 0
  FOR x% = 0 TO 7
    scan% = scan% * 2 + pixels%(x%, y%)
  NEXT x%

  POKE baseoffset% + y%, scan%
NEXT y%

OUT SequencerIndexPort%, MapMaskRegister%
OUT SequencerDataPort%, savedmask%

OUT SequencerIndexPort%, MemoryModeRegister%
OUT SequencerDataPort%, savedmode%

OUT GraphicsIndexPort%, MiscGraphicsRegister%
OUT GraphicsDataPort%, savedmisc%
END SUB

SUB TransferCharactersToFont
DIM pixels%(7, 7)
FOR ch% = FirstCharacter% TO NextCharacter% - 1
  o% = 2
  d% = &HFF
  e% = &HFF00

  FOR y% = 0 TO 7
    FOR x% = 0 TO 7
      IF Characters%(o%, ch%) AND d% THEN pixels%(x%, y%) = 1 ELSE pixels%(x%, y%) = 0

      SWAP d%, e%
      IF d% > 0 THEN o% = o% + 1
    NEXT x%
  NEXT y%

  SetFontCharacter ch%, pixels%()
NEXT ch%
END SUB


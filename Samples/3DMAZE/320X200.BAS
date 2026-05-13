DECLARE SUB DrawSprite (x%, y%, sprite%())
DECLARE SUB CaptureStatusSprite (txt$, fore%, back%, buffer%())
DECLARE SUB PreRenderStatusSprites ()
DECLARE SUB CastRay (pX!, pY!, pXI%, pYI%, fA!, sXI%, sYI%, dst!, keyDst!, exitDst!, texnum%, texX!, keyTexX!, exitTexX!)
DECLARE SUB CorrectPerspective (fRelAngle!, dst!)
DECLARE SUB DrawFrame (pX!, pY!, pXI%, pYI%, pA!, flashing%, exitopen%)
DECLARE SUB DrawMiniMap (pXI%, pYI%)
DECLARE SUB DrawStatus (exitopen%)
DECLARE SUB GenerateTextures ()
DECLARE SUB InitializeLabyrinth ()
DECLARE SUB PaintSolidSlice (col%, dst!, colour%)
DECLARE SUB PaintTextureSlice (col%, dst!, texnum%, texX!, col0%, col1%)
DECLARE SUB PaintTransparentTextureSlice (col%, dst!, texColour%, texnum%, texX!)
DECLARE SUB SetUpPalette ()
DECLARE FUNCTION HandleInput% (pX!, pY!, pXI%, pYI%, pA!, flashing%, exitopen%, victory%)
DECLARE FUNCTION InputNumber% (prompt$, minvalue%, maxvalue%)

' Portions of this code are based on code written by AI under the direction of Elisson Fabiano.

CONST DistanceToScreen! = 200
CONST MaximumDepth! = 20
CONST NumGradientTextures% = 6
CONST NumFixedTextures% = 3 ' Key, Closed Door, Opened Door
CONST NumTextures% = NumGradientTextures% + NumFixedTextures%
CONST TextureWidth% = 16
CONST TextureHeight% = 16
CONST FirstSolidBlockType% = 10
CONST FirstFixedTexture% = NumGradientTextures%
CONST KeyTexture% = FirstFixedTexture% + 0
CONST ClosedDoorTexture% = FirstFixedTexture% + 1
CONST OpenDoorTexture% = FirstFixedTexture% + 2

CONST CeilingColour% = 0
CONST FlashColour% = 7
CONST FloorColour% = 8

CONST TextureCloseColour0% = 2
CONST TextureCloseColour1% = 4
CONST TextureFarColour0% = 5
CONST TextureFarColour1% = 6

CONST KeyColour% = 14
CONST ClosedDoorColour% = 12
CONST OpenDoorColour% = 3

DIM SHARED LabW%, LabH%
DIM SHARED kUP$, kDOWN$, kLEFT$, kRIGHT$, kESCAPE$
DIM SHARED HalfScreen$, Shade75$, Shade100$

kUP$ = CHR$(0) + "H"
kDOWN$ = CHR$(0) + "P"
kLEFT$ = CHR$(0) + "K"
kRIGHT$ = CHR$(0) + "M"
kESCAPE$ = CHR$(27)

HalfScreen$ = STRING$(80 * 25, 219)
Shade75$ = CHR$(178)
Shade100$ = CHR$(219)

SCREEN 0
COLOR 15
PRINT "Labyrinth configuration"
PRINT "======================="

LabW% = InputNumber%("Width (11-51)", 11, 51)
LabH% = InputNumber%("Height (11-41)", 11, 41)

IF LabW% MOD 2 = 0 THEN LabW% = LabW% + 1
IF LabH% MOD 2 = 0 THEN LabH% = LabH% + 1

RANDOMIZE TIMER

DIM SHARED Lab(LabW%, LabH%) AS INTEGER
DIM SHARED Texture(0 TO NumTextures% - 1, 0 TO TextureWidth% - 1, 0 TO TextureHeight% - 1) AS INTEGER
DIM SHARED StatusClosedSprite%(1600)
DIM SHARED StatusOpenSprite%(1600)
DIM SHARED VictorySprite%(1600)

VictorySprite%(0) = 42

DIM RenderBuffer&(16100)

DIM SHARED RenderOffset%

DEF SEG = VARSEG(RenderBuffer&(1))

RenderOffset% = VARPTR(RenderBuffer&(1))

SCREEN 13

GET (0, 0)-(319, 199), RenderBuffer&

PreRenderStatusSprites
SetUpPalette
InitializeLabyrinth
GenerateTextures

pX = 1.5
pY = 1.5
pA = 0

pXI% = FIX(pX)
pYI% = FIX(pY)

' Make sure we don't start out staring at a wall
IF Lab(pXI% + 1, pYI%) >= FirstSolidBlockType% THEN pA = 1.570796

exitopen% = 0
victory% = 0

DO
  DrawFrame pX, pY, pXI%, pYI%, pA, flashing%, exitopen%
  DrawMiniMap pXI%, pYI%
  DrawStatus exitopen%

  IF victory% THEN
    DrawSprite 120, 44, VictorySprite%()
  END IF

  WAIT &H3DA, 1
  PUT (0, 0), RenderBuffer&

  exitloop% = HandleInput%(pX, pY, pXI%, pYI%, pA, flashing%, exitopen%, victory%)
LOOP UNTIL exitloop%

SCREEN 0

SUB CaptureStatusSprite (txt$, fore%, back%, buffer%())
CLS
PRINT txt$

w% = LEN(txt$) * 8
h% = 8

buffer%(0) = w%
buffer%(1) = h%

o% = 2

FOR y% = scan% TO h% - 1
  FOR x% = 0 TO w% - 1
    IF POINT(x%, y%) THEN c% = fore% ELSE c% = back%
    buffer%(o%) = c%
    o% = o% + 1
  NEXT x%
NEXT y%
END SUB

SUB CastRay (pX, pY, pXI%, pYI%, fA, sXI%, sYI%, dst, keyDst, exitDst, texnum%, texX, keyTexX, exitTexX)
vX = COS(fA)
vY = SIN(fA)

sX = pX
sY = pY

sXI% = pXI%
sYI% = pYI%

dXI% = SGN(vX)
dYI% = SGN(vY)

probeDXI% = (vX < 0)
probeDYI% = (vY < 0)

nextXI% = sXI% - (vX > 0)
nextYI% = sYI% - (vY > 0)

hit% = 0
dst = 0
keyDst = 0
exitDst = 0
texnum% = 0
WHILE hit% = 0 AND dst < MaximumDepth!
  nextYFromXI = sY + (nextXI% - sX) * vY / vX
  nextXFromYI = sX + (nextYI% - sY) * vX / vY

  d1 = (nextXI% - pX) ^ 2 + (nextYFromXI - pY) ^ 2
  d2 = (nextXFromYI - pX) ^ 2 + (nextYI% - pY) ^ 2

  IF d1 < d2 THEN
    dst = SQR(d1)
    sX = nextXI%: sY = nextYFromXI
    sXI% = nextXI% + probeDXI%
    nextXI% = nextXI% + dXI%
    texX = (sY - sYI%) * dXI%
  ELSE
    dst = SQR(d2)
    sX = nextXFromYI: sY = nextYI%
    sYI% = nextYI% + probeDYI%
    nextYI% = nextYI% + dYI%
    texX = (sX - sXI%) * dYI%
  END IF

  IF sXI% >= 0 AND sXI% < LabW% AND sYI% >= 0 AND sYI% < LabH% THEN
    IF Lab(sXI%, sYI%) = 3 THEN keyDst = dst: keyTexX = texX
    IF Lab(sXI%, sYI%) = 2 THEN exitDst = dst: exitTexX = texX
    IF Lab(sXI%, sYI%) >= FirstSolidBlockType% THEN texnum% = Lab(sXI%, sYI%) - FirstSolidBlockType%: hit% = 1
  ELSE
    hit% = 1
    dst = MaximumDepth!
  END IF
WEND

IF texX < 0 THEN texX = texX + 1
IF keyTexX < 0 THEN keyTexX = keyTexX + 1
IF exitTexX < 0 THEN exitTexX = exitTexX + 1
END SUB

SUB DrawFrame (pX, pY, pXI%, pYI%, pA, flashing%, exitopen%)
IF flashing% AND 1 THEN topcolour% = FlashColour% ELSE topcolour% = CeilingColour%
IF flashing% > 0 THEN flashing% = flashing% - 1

FOR o& = RenderOffset% TO RenderOffset + 31999&
  POKE o&, topcolour%
NEXT o&
FOR o& = RenderOffset% + 32000& TO RenderOffset + 63999
  POKE o&, FloorColour%
NEXT o&

FOR col% = 0 TO 319
  fRelAngle = ATN((col% - 159.5) / DistanceToScreen!)
  fA = pA + fRelAngle

  CastRay pX, pY, pXI%, pYI%, fA, sXI%, sYI%, dst, keyDst, exitDst, texnum%, texX, keyTexX, exitTexX

  perspective = COS(fRelAngle)

  dst = dst * perspective
  keyDst = keyDst * perspective
  exitDst = exitDst * perspective

  IF dst > 8 THEN
    PaintSolidSlice col%, dst, TextureFarColour0%
  ELSEIF dst > 4 THEN
    PaintTextureSlice col%, dst, texnum%, texX, TextureFarColour0%, TextureFarColour1%
  ELSE
    PaintTextureSlice col%, dst, texnum%, texX, TextureCloseColour0%, TextureCloseColour1%
  END IF

  keyvisible% = (keyDst > 0) AND (keyDst < dst)
  exitvisible% = (exitDst > 0) AND (exitDst < dst)

  IF keyvisible% OR exitvisible% THEN
    IF keyvisible% AND (keyDst > exitDst) THEN
      PaintTransparentTextureSlice col%, keyDst, KeyColour%, KeyTexture%, keyTexX
    END IF

    IF exitvisible% AND (keyDst <= exitDst) THEN
      IF exitopen% THEN
        exittex% = OpenDoorTexture%
        exitcolour% = OpenDoorColour%
      ELSE
        exittex% = ClosedDoorTexture%
        exitcolour% = ClosedDoorColour%
      END IF

      PaintTransparentTextureSlice col%, exitDst, exitcolour%, exittex%, exitTexX
    END IF

    IF keyvisible% AND (keyDst < exitDst) THEN
      PaintTransparentTextureSlice col%, keyDst, KeyColour%, KeyTexture%, keyTexX
    END IF
  END IF
NEXT col%
END SUB

SUB DrawMiniMap (pXI%, pYI%)
FOR my% = -5 TO 4
  offX% = 272
  offY% = 28 + my% * 4
  FOR mx% = -5 TO 4
    cx% = pXI% + mx%
    cy% = pYI% + my%

    IF cx% < 0 OR cx% > LabW% OR cy% < 0 OR cy% > LabH% THEN
      c% = 0
    ELSEIF mx% = 0 AND my% = 0 THEN
      c% = 13
    ELSEIF Lab(cx%, cy%) >= FirstSolidBlockType% THEN
      c% = 9
    ELSEIF Lab(cx%, cy%) = 2 THEN
      IF exitopen% THEN c% = OpenDoorColour% ELSE c% = ClosedDoorColour%
    ELSEIF Lab(cx%, cy%) = 3 THEN
      c% = 14
    ELSE
      c% = 0
    END IF

    FOR yy% = 0 TO 3
      o& = offX% + (offY% + yy%) * 320& + RenderOffset%
      FOR xx% = 0 TO 3
        POKE o& + xx%, c%
      NEXT xx%
    NEXT yy%

    offX% = offX% + 4
  NEXT mx%
NEXT my%
END SUB

SUB DrawSprite (x%, y%, sprite%())
w% = sprite%(0)
h% = sprite%(1)

o% = 2

FOR yy% = y% TO y% + h% - 1
  o& = x% + yy% * 320& + RenderOffset%

  FOR xx% = x% TO x% + w% - 1
    POKE o&, sprite%(o%)
    o% = o% + 1
    o& = o& + 1
  NEXT xx%
NEXT yy%
END SUB

SUB DrawStatus (exitopen%)
IF exitopen% = 0 THEN
  DrawSprite 0, 0, StatusClosedSprite%()
ELSE
  DrawSprite 0, 0, StatusOpenSprite%()
END IF
END SUB

SUB GenerateTextures
FOR t% = 0 TO NumGradientTextures% - 1
  density = t% / (NumGradientTextures% - 1)

  FOR i% = 0 TO TextureWidth% - 1
    FOR j% = 0 TO TextureHeight% - 1
      Texture(t%, i%, j%) = ABS(RND < density)
    NEXT j%
  NEXT i%
NEXT t%

' Key
raise% = 3

FOR i% = 1 TO 3
  Texture(KeyTexture%, i%, TextureHeight% - 4 - raise%) = 1
  Texture(KeyTexture%, i%, TextureHeight% - raise%) = 1
  Texture(KeyTexture%, 0, TextureHeight% - i% - raise%) = 1
  Texture(KeyTexture%, 4, TextureHeight% - i% - raise%) = 1
NEXT i%

FOR i% = 4 TO TextureWidth% - 1
  Texture(KeyTexture%, i%, TextureHeight% - 2 - raise%) = 1
NEXT i%

FOR i% = 9 TO TextureWidth% - 2
  FOR j% = 1 TO RND * 3
    Texture(KeyTexture%, i%, TextureHeight% - 2 - j% - raise%) = 1
  NEXT j%
NEXT i%

' Open Door
FOR i% = 0 TO TextureWidth% - 1
  Texture(OpenDoorTexture%, i%, 0) = 1
  Texture(OpenDoorTexture%, i%, TextureHeight% - 1) = 1
NEXT i%

FOR i% = 0 TO TextureHeight% - 1
  Texture(OpenDoorTexture%, 0, i%) = 1
  Texture(OpenDoorTexture%, TextureWidth% - 1, i%) = 1
NEXT i%

mr! = TextureWidth%
IF TextureHeight% > mr! THEN mr! = TextureHeight%
mr! = mr! * .5
nr! = mr! - 1.5

mr! = mr! * mr!
nr! = nr! * nr!

FOR y% = 0 TO TextureHeight% - 1
  yy! = y% - (TextureHeight% - 1) * .5
  FOR x% = 0 TO TextureWidth% - 1
    xx! = x% - (TextureWidth% - 1) * .5
    r! = xx! * xx! + yy! * yy!
    IF r! > nr! AND r! <= mr! THEN Texture(OpenDoorTexture%, x%, y%) = 1
  NEXT x%
NEXT y%

' Closed Door
FOR y% = 0 TO TextureHeight% - 1
  FOR x% = 0 TO TextureWidth% - 1
    Texture(ClosedDoorTexture%, x%, y%) = Texture(OpenDoorTexture%, x%, y%)
  NEXT x%
NEXT y%

FOR x% = 4 TO TextureWidth% - 5
  FOR y% = TextureHeight% \ 2 - 2 TO TextureHeight% \ 2 + 2
    Texture(ClosedDoorTexture%, x%, y%) = 1
  NEXT y%
NEXT x%

x% = TextureWidth% \ 2

FOR y% = TextureHeight% \ 2 - 1 TO TextureHeight% \ 2 + 1
  Texture(ClosedDoorTexture%, x%, y%) = 0
NEXT y%
END SUB

FUNCTION HandleInput% (pX, pY, pXI%, pYI%, pA, flashing%, exitopen%, victory%)
k$ = INKEY$

IF k$ = kESCAPE$ THEN
  HandleInput% = -1
ELSEIF victory% = 0 THEN
  SELECT CASE k$
    CASE "w", "W", kUP$:
      nx = pX + COS(pA) * .3: ny = pY + SIN(pA) * .3
      IF Lab(FIX(nx), FIX(ny)) < FirstSolidBlockType% THEN pX = nx: pY = ny
    CASE "s", "S", kDOWN$:
      nx = pX - COS(pA) * .3: ny = pY - SIN(pA) * .3
      IF Lab(FIX(nx), FIX(ny)) < FirstSolidBlockType% THEN pX = nx: pY = ny
    CASE "a", "A", kLEFT$:
      pA = pA - .2
    CASE "d", "D", kRIGHT$:
      pA = pA + .2
  END SELECT

  pXI% = FIX(pX)
  pYI% = FIX(pY)

  IF Lab(pXI%, pYI%) = 3 THEN
    exitopen% = 1
    Lab(pXI%, pYI%) = 0
    flashing% = 3
  END IF

  IF Lab(pXI%, pYI%) = 2 AND exitopen% = 1 THEN victory% = 1
END IF
END FUNCTION

SUB InitializeLabyrinth
' Fill Lab with random noise
FOR y% = 0 TO LabH%
  FOR x% = 0 TO LabW%
    Lab(x%, y%) = RND * 100
  NEXT x%
NEXT y%

' Smooth the random noise (simple box blur)
DIM LabTemp(60, 60) AS INTEGER

FOR i% = 1 TO 10
  FOR y% = 0 TO LabH%
    FOR x% = 0 TO LabW%
      sum% = Lab(x%, y%)
      count% = 1

      IF x% > 0 THEN
        IF y% > 0 THEN sum% = sum% + Lab(x% - 1, y% - 1): count% = count% + 1
        sum% = sum% + Lab(x% - 1, y%): count% = count% + 1
        IF y% < LabH% THEN sum% = sum% + Lab(x% - 1, y% + 1): count% = count% + 1
      END IF

      IF y% > 0 THEN sum% = sum% + Lab(x%, y% - 1): count% = count% + 1
      IF y% < LabH% THEN sum% = sum% + Lab(x%, y% + 1): count% = count% + 1

      IF x% < LabW% THEN
        IF y% > 0 THEN sum% = sum% + Lab(x% + 1, y% - 1): count% = count% + 1
        sum% = sum% + Lab(x% + 1, y%): count% = count% + 1
        IF y% < LabH% THEN sum% = sum% + Lab(x% + 1, y% + 1): count% = count% + 1
      END IF

      LabTemp(x%, y%) = sum% \ count%
    NEXT x%
  NEXT y%

  FOR y% = 0 TO LabH%
    FOR x% = 0 TO LabW%
      Lab(x%, y%) = LabTemp(x%, y%)
    NEXT x%
  NEXT y%
NEXT i%

' Normalize Lab
min% = 100
max% = 0

FOR y = 0 TO LabH%
  FOR x = 0 TO LabW%
    IF Lab(x, y) < min% THEN min% = Lab(x, y)
    IF Lab(x, y) > max% THEN max% = Lab(x, y)
  NEXT x
NEXT y

r% = max% - min% + 1

FOR y% = 0 TO LabH%
  FOR x% = 0 TO LabW%
    Lab(x%, y%) = FirstSolidBlockType% + (Lab(x%, y%) - min%) * NumGradientTextures% \ r%
  NEXT x%
NEXT y%

' Carve labyrinth out
DIM stkX(3000) AS INTEGER, stkY(3000) AS INTEGER
ptr% = 1
stkX(1) = 1: stkY(1) = 1
Lab(1, 1) = 0
WHILE ptr% > 0
  cx% = stkX(ptr%): cy% = stkY(ptr%)
  found% = 0
  r% = INT(RND * 4)
  FOR i% = 0 TO 3
    dr% = (r% + i%) MOD 4
    dx% = 0: dy% = 0
    IF dr% = 0 THEN dy% = -2
    IF dr% = 1 THEN dy% = 2
    IF dr% = 2 THEN dx% = 2
    IF dr% = 3 THEN dx% = -2
    nx% = cx% + dx%: ny% = cy% + dy%
    IF nx% > 0 AND nx% < LabW% AND ny% > 0 AND ny% < LabH% THEN
      IF Lab(nx%, ny%) >= FirstSolidBlockType% THEN
        Lab(cx% + dx% \ 2, cy% + dy% \ 2) = 0
        Lab(nx%, ny%) = 0
        ptr% = ptr% + 1
        stkX(ptr%) = nx%: stkY(ptr%) = ny%
        found% = 1
        EXIT FOR
      END IF
    END IF
  NEXT i%
  IF found% = 0 THEN ptr% = ptr% - 1
WEND

Lab(LabW% - 2, LabH% - 2) = 2
DO
  kx% = INT(RND * (LabW% - 1)) + 1
  ky% = INT(RND * (LabH% - 1)) + 1
LOOP WHILE Lab(kx%, ky%) <> 0 OR (kx% < 3 AND ky% < 3)

Lab(kx%, ky%) = 3
END SUB

FUNCTION InputNumber% (prompt$, minvalue%, maxvalue%)
PRINT prompt$; ": ";
LINE INPUT a$
n% = VAL(a$)

IF n% > maxvalue% THEN n% = maxvalue%
IF n% < minvalue% THEN n% = minvalue%

InputNumber% = n%
END FUNCTION

SUB PaintSolidSlice (col%, dst, colour%)
h = INT(100 / dst)

startrow% = 100 - h
stoprow% = 100 + h
wallheight% = h * 2 + 1

textureDTY% = TextureHeight% * 256 \ wallheight%

textureTYAccum% = 0
textureTY% = 0

IF startrow% < 0 THEN startrow% = 0
IF stoprow% > 199 THEN stoprow% = 199

o& = col% + startrow% * 320& + RenderOffset%

FOR y% = startrow% TO stoprow%
  POKE o&, colour%
  o& = o& + 320
NEXT y%
END SUB

SUB PaintTextureSlice (col%, dst, texnum%, texX, col0%, col1%)
CONST FixedPointScale% = 8192

texXI% = texX * TextureWidth%
IF texXI% < 0 THEN texXI% = 0
IF texXI% >= TextureWidth% THEN texXI% = TextureWidth% - 1

IF dst < .01 THEN
  h% = 16000
ELSE
  h% = INT(100 / dst)
END IF

startrow% = 100 - h%
stoprow% = 100 + h%
wallheight% = h% * 2 + 1

texYI% = 0

texYAccum% = 0
texDYAccum% = TextureHeight% * 1& * FixedPointScale% \ wallheight%

IF startrow% < 0 THEN
  offScreenAccum& = texDYAccum% * -1& * startrow%
  texYI% = offScreenAccum& \ FixedPointScale%
  texYAccum% = offScreenAccum& AND (FixedPointScale% - 1)
  startrow% = 0
END IF

IF stoprow% > 199 THEN stoprow% = 199

o& = col% + startrow% * 320& + RenderOffset%

FOR row% = startrow% TO stoprow%
  IF Texture(texnum%, texXI%, texYI%) = 0 THEN
    POKE o&, col0%
  ELSE
    POKE o&, col1%
  END IF

  o& = o& + 320

  texYAccum% = texYAccum% + texDYAccum%

  WHILE texYAccum% >= FixedPointScale%
    texYI% = texYI% + 1
    texYAccum% = texYAccum% - FixedPointScale%

    IF texYI% >= TextureHeight% THEN EXIT FOR
  WEND
NEXT row%
END SUB

SUB PaintTransparentTextureSlice (col%, dst, texColour%, texnum%, texX)
CONST FixedPointScale% = 8192

texXI% = texX * TextureWidth%
IF texXI% < 0 THEN texXI% = 0
IF texXI% >= TextureWidth% THEN texXI% = TextureWidth% - 1

IF dst < .01 THEN
  h% = 16000
ELSE
  h% = INT(100 / dst)
END IF

startrow% = 100 - h%
stoprow% = 100 + h%
wallheight% = h% * 2 + 1

texYI% = 0

texYAccum% = 0
texDYAccum% = TextureHeight% * 1& * FixedPointScale% \ wallheight%

IF startrow% < 0 THEN
  offScreenAccum& = texDYAccum% * -1& * startrow%
  texYI% = offScreenAccum& \ FixedPointScale%
  texYAccum% = offScreenAccum& AND (FixedPointScale% - 1)
  startrow% = 0
END IF

IF stoprow% > 199 THEN stoprow% = 199

o& = col% + startrow% * 320& + RenderOffset%

FOR row% = startrow% TO stoprow%
  IF Texture(texnum%, texXI%, texYI%) THEN
    POKE o&, texColour%
  END IF

  o& = o& + 320

  texYAccum% = texYAccum% + texDYAccum%

  WHILE texYAccum% >= FixedPointScale%
    texYI% = texYI% + 1
    texYAccum% = texYAccum% - FixedPointScale%

    IF texYI% >= TextureHeight% THEN EXIT FOR
  WEND
NEXT row%
END SUB

SUB PreRenderStatusSprites
SCREEN 13

CaptureStatusSprite "TAKE THE KEY", 15, 0, StatusClosedSprite%()
CaptureStatusSprite "ESCAPE", 15, 0, StatusOpenSprite%()
CaptureStatusSprite " VICTORY! ", 0, 7, VictorySprite%()

CLS
END SUB

SUB SetUpPalette
OUT &H3C8, TextureFarColour0%
OUT &H3C9, 5
OUT &H3C9, 0
OUT &H3C9, 25

OUT &H3C8, TextureFarColour1%
OUT &H3C9, 10
OUT &H3C9, 0
OUT &H3C9, 50

OUT &H3C8, TextureCloseColour0%
OUT &H3C9, 3
OUT &H3C9, 2
OUT &H3C9, 29

OUT &H3C8, TextureCloseColour1%
OUT &H3C9, 10
OUT &H3C9, 7
OUT &H3C9, 55
END SUB

'$DYNAMIC

TYPE PointType
  X AS DOUBLE
  Y AS DOUBLE
  Z AS DOUBLE
END TYPE

TYPE VertexType
  VertexIndex AS INTEGER
  NormalIndex AS INTEGER
END TYPE

TYPE FaceType
  P1 AS VertexType
  P2 AS VertexType
  P3 AS VertexType
  SortZ AS DOUBLE
  Centre AS PointType
  Normal AS PointType
END TYPE

TYPE ShotType
  Active AS INTEGER
  ShotX AS DOUBLE
  ShotY AS DOUBLE
  ShotAngle AS DOUBLE
END TYPE

TYPE DustType
  Position AS PointType
  Colour AS INTEGER
END TYPE

TYPE RockType
  Active AS INTEGER
  X AS INTEGER
  Y AS INTEGER
  DX AS INTEGER
  SpriteIndex AS INTEGER
  SpriteIndexDelta AS INTEGER
END TYPE

TYPE AlienSpeciesType
  Radius AS INTEGER
  Value AS INTEGER
  Squishy AS INTEGER
END TYPE

TYPE AlienType
  Active AS INTEGER
  Species AS INTEGER
  Argument1 AS DOUBLE
  Argument2 AS DOUBLE
  Argument3 AS DOUBLE
  Argument4 AS INTEGER
  FrameNumber AS INTEGER
  HealthPoints AS INTEGER
  Damaged AS INTEGER
  SpriteIndex AS INTEGER
  X AS DOUBLE
  Y AS DOUBLE
  DX AS DOUBLE
  DY AS DOUBLE
  ActualX AS INTEGER
  ActualY AS INTEGER
END TYPE

TYPE BoomType
  Active AS INTEGER
  X AS INTEGER
  Y AS INTEGER
  DX AS INTEGER
  DY AS INTEGER
  SpriteIndex AS INTEGER
END TYPE

TYPE SplatType
  ActiveForFrames AS INTEGER
  X AS DOUBLE
  Y AS DOUBLE
  SplatAngle AS DOUBLE
END TYPE

DECLARE SUB AddPoint (t AS PointType, s AS PointType)
DECLARE SUB ApplyTransform (i AS PointType, matrix#(), o AS PointType)
DECLARE SUB CascadeFighterBoom (fx%, fy%, fdx%, Booms() AS BoomType)
DECLARE SUB CentreText (tx%, ty%, text$)
DECLARE SUB CheckForHits (Shots() AS ShotType, Rocks() AS RockType, Aliens() AS AlienType, Booms() AS BoomType, Splats() AS SplatType, fx%, fy%, nextround%, score&)
DECLARE SUB CreateKamikaze (Aliens() AS AlienType, ox#, oy#, odx#, ody#, acceleration#, activatey%, dropy%)
DECLARE SUB CreateMine (Aliens() AS AlienType, ox#, oy#)
DECLARE SUB CreateMineLayer (Aliens() AS AlienType, ox#, oy#)
DECLARE SUB CreateOrbitCluster (Aliens() AS AlienType, clustersize%, ox#, oy#, odx#, ody#)
DECLARE SUB DrawAlien (alien AS AlienType, ox%, oy%)
DECLARE SUB DrawAlien0 (xx#, yy#)
DECLARE SUB DrawAlien1 (xx#, yy#, alien AS AlienType)
DECLARE SUB DrawAlien2 (xx#, yy#, alien AS AlienType)
DECLARE SUB DrawAlien2Mine (xx#, yy#, alien AS AlienType)
DECLARE SUB DrawBoom (boom AS BoomType)
DECLARE SUB DrawReticle (ox%, oy%)
DECLARE SUB DrawRock (rock AS RockType)
DECLARE SUB DrawScore (score&)
DECLARE SUB DrawSplat (splatinfo AS SplatType)
DECLARE SUB FillTriangle (x1%, y1%, x2%, y2%, x3%, y3%, c%)
DECLARE SUB GetArtDimensions (filename$, w%, h%)
DECLARE SUB GetObjectCounts (objfilename$, numvertices%, numnormals%, numfaces%)
DECLARE SUB HorizontalLine (scanofs&, x1%, x2%, c%)
DECLARE SUB InitializeDust (Dusts() AS DustType)
DECLARE SUB InitializeFont ()
DECLARE SUB LoadArt (filename$, pixels%(), mask%())
DECLARE SUB LoadObject (objfilename$, vertices() AS PointType, normals() AS PointType, faces() AS FaceType)
DECLARE SUB MakeBoom (Booms() AS BoomType, ox%, oy%, odx%, ody%)
DECLARE SUB MakeSplat (Splats() AS SplatType, ox#, oy#)
DECLARE SUB MultiplyMatrix (subject#(), transform#())
DECLARE SUB MultiplyPoint (t AS PointType, scale#)
DECLARE SUB NormalizePoint (t AS PointType)
DECLARE SUB ParseTriple (a$, p AS PointType)
DECLARE SUB ParseVertex (a$, v AS VertexType)
DECLARE SUB ParseVertices (a$, f AS FaceType)
DECLARE SUB ProgramTimerRate (tickspersecond#)
DECLARE SUB PutArt (ox#, oy#, angle#, pixels%(), mask%())
DECLARE SUB PutGlyph (gx%, gy%, c%, w%)
DECLARE SUB RenderObject (sourcevertices() AS PointType, sourcenormals() AS PointType, faces() AS FaceType, transform#(), rotation#(), lightposition AS PointType, LightIntensity#, AmbientLightIntensity#, ox%, oy%)
DECLARE SUB RenderShot (shot AS ShotType)
DECLARE SUB RotateTransformX (matrix#(), angle#)
DECLARE SUB RotateTransformY (matrix#(), angle#)
DECLARE SUB RotateTransformZ (matrix#(), angle#)
DECLARE SUB ScaleTransform (matrix#(), factor#)
DECLARE SUB SetIdentityTransform (transform#())
DECLARE SUB SortFaces (faces() AS FaceType, first%, last%)
DECLARE SUB StartRound (Aliens() AS AlienType, roundnumber%, nextround%)
DECLARE SUB SubtractPoint (t AS PointType, s AS PointType)
DECLARE SUB TitleScreen (FighterVertices() AS PointType, FighterNormals() AS PointType, FighterFaces() AS FaceType, FighterLightPosition AS PointType)
DECLARE SUB Translate (matrix#(), deltax#, deltay#, deltaz#)
DECLARE SUB UpdateAndRenderAliens (Aliens() AS AlienType, fx%, fy%, fdx%, Booms() AS BoomType)
DECLARE SUB UpdateAndRenderBooms (Booms() AS BoomType)
DECLARE SUB UpdateAndRenderDust (Dusts() AS DustType)
DECLARE SUB UpdateAndRenderRocks (Rocks() AS RockType, fx%, fy%, fdx%, Booms() AS BoomType)
DECLARE SUB UpdateAndRenderShot (shot AS ShotType)
DECLARE SUB UpdateAndRenderSplats (Splats() AS SplatType)
DECLARE FUNCTION Distance# (t AS PointType)
DECLARE FUNCTION DotProduct# (a AS PointType, b AS PointType)
DECLARE FUNCTION IsNaNOrInf% (d#)
DECLARE FUNCTION MeasureText% (text$)
DECLARE FUNCTION RandomDustColour% ()

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

DECLARE SUB InterruptX (intnum%, rin AS RegTypeX, rout AS RegTypeX)

DIM rin AS RegTypeX, rout AS RegTypeX

CONST FPSLimit% = 32

CONST PI# = 3.14159265358979

CONST FighterMomentum# = .8
CONST FighterReticleAttraction# = .6
CONST FighterTiltSpeed# = .06
CONST FighterTiltMax# = .8
CONST FighterRadius% = 10

CONST ReticleColour% = 14

CONST MaxShots% = 40
CONST ShotStartOffset# = 5
CONST ShotSpeed# = 35
CONST ShotWidth# = 2.2
CONST ShotTailLength# = 15
CONST ShotColour% = 35

CONST CameraZ# = -1000
CONST Shininess# = 4
CONST LightIntensity# = .6
CONST AmbientLightIntensity# = .3

CONST MaxAliens% = 25
CONST AlienActiveMargin% = 40
CONST AlienDamageVisibleFrames% = 6

CONST OrbitClusterAlienHealthPoints% = 1
CONST OrbitClusterAlienVeerOffY% = 90
CONST OrbitClusterAlienVeerOffDX# = 6

CONST KamikazeAlienHealthPoints% = 22
CONST KamikazeAlienDragFactor# = .95
CONST KamikazeAlienActivateY% = 75
CONST KamikazeAlienAttractionFactor# = .05
CONST KamikazeAlienMaxSpeed# = 1.5
CONST KamikazeAlienDropY% = 120

CONST MineLayerHealthPoints% = 75
CONST MineLayerDropSpeed# = .3
CONST MineLayerDecelerationFactor# = .95
CONST MineLayerRotationSpeed# = .04
CONST MineLayerMaxSpeed# = 3.25

CONST MineHealthPoints% = 2
CONST MineSpeed# = 1.5

CONST RoundInterval% = 200

CONST MaxDusts% = 100
CONST DustSpeed# = 435
CONST DustZMin# = -5
CONST DustZMax# = 45
CONST DustXMin# = -160 * (DustZMax# - CameraZ#) / 10
CONST DustXMax# = 160 * (DustZMax# - CameraZ#) / 10
CONST DustYMin# = -100 * (DustZMax# - CameraZ#) / 10
CONST DustYMax# = 100 * (DustZMax# - CameraZ#) / 10 + DustSpeed#
CONST DustYSpawnMin# = 100 * (DustZMax# - CameraZ#) / 10

CONST MaxRocks% = 5
CONST RockSpeed% = 8
CONST RockSpawnRate# = 1 / 100 ' Try once every 100 frames

CONST MaxBooms% = 10
CONST BoomFrames% = 10

CONST MaxSplats% = 10
CONST SplatFrames% = 3

CONST MarginX% = 18
CONST FighterY% = 180

CONST HitGridWidth% = 80
CONST HitGridHeight% = 50
CONST HitGridRock% = 1000
CONST HitGridAlien% = 2000

' Load art. Unfortunately, we have to repeat the pattern like
' this because we can't set the array sizes within a SUB on
' a parameter.

GetArtDimensions "ALIEN0.ART", w%, h%
DIM Alien0%(1 TO w%, 1 TO h%), Alien0Mask%(1 TO w%, 1 TO h%)
LoadArt "ALIEN0.ART", Alien0%(), Alien0Mask%()

GetArtDimensions "ALIEN1_A.ART", w%, h%
DIM Alien1A%(1 TO w%, 1 TO h%), Alien1AMask%(1 TO w%, 1 TO h%)
LoadArt "ALIEN1_A.ART", Alien1A%(), Alien1AMask%()

GetArtDimensions "ALIEN1_B.ART", w%, h%
DIM Alien1B%(1 TO w%, 1 TO h%), Alien1BMask%(1 TO w%, 1 TO h%)
LoadArt "ALIEN1_B.ART", Alien1B%(), Alien1BMask%()

GetArtDimensions "ALIEN1_D.ART", w%, h%
DIM Alien1D%(1 TO w%, 1 TO h%), Alien1DMask%(1 TO w%, 1 TO h%)
LoadArt "ALIEN1_D.ART", Alien1D%(), Alien1DMask%()

GetArtDimensions "ALIEN2_A.ART", w%, h%
DIM Alien2A%(1 TO w%, 1 TO h%), Alien2AMask%(1 TO w%, 1 TO h%)
LoadArt "ALIEN2_A.ART", Alien2A%(), Alien2AMask%()

GetArtDimensions "ALIEN2_B.ART", w%, h%
DIM Alien2B%(1 TO w%, 1 TO h%), Alien2BMask%(1 TO w%, 1 TO h%)
LoadArt "ALIEN2_B.ART", Alien2B%(), Alien2BMask%()

GetArtDimensions "ALIEN2_D.ART", w%, h%
DIM Alien2D%(1 TO w%, 1 TO h%), Alien2DMask%(1 TO w%, 1 TO h%)
LoadArt "ALIEN2_D.ART", Alien2D%(), Alien2DMask%()

GetArtDimensions "ALIEN3_A.ART", w%, h%
DIM Alien3A%(1 TO w%, 1 TO h%), Alien3AMask%(1 TO w%, 1 TO h%)
LoadArt "ALIEN3_A.ART", Alien3A%(), Alien3AMask%()

GetArtDimensions "ALIEN3_B.ART", w%, h%
DIM Alien3B%(1 TO w%, 1 TO h%), Alien3BMask%(1 TO w%, 1 TO h%)
LoadArt "ALIEN3_B.ART", Alien3B%(), Alien3BMask%()

GetArtDimensions "ALIEN4.ART", w%, h%
DIM Alien4%(1 TO w%, 1 TO h%), Alien4Mask%(1 TO w%, 1 TO h%)
LoadArt "ALIEN4.ART", Alien4%(), Alien4Mask%()

GetArtDimensions "ALIEN5_A.ART", w%, h%
DIM Alien5A%(1 TO w%, 1 TO h%), Alien5AMask%(1 TO w%, 1 TO h%)
LoadArt "ALIEN5_A.ART", Alien5A%(), Alien5AMask%()

GetArtDimensions "ALIEN5_B.ART", w%, h%
DIM Alien5B%(1 TO w%, 1 TO h%), Alien5BMask%(1 TO w%, 1 TO h%)
LoadArt "ALIEN5_B.ART", Alien5B%(), Alien5BMask%()

GetArtDimensions "ROCK01.ART", w%, h%
DIM Rock01%(1 TO w%, 1 TO h%), Rock01Mask%(1 TO w%, 1 TO h%)
LoadArt "ROCK01.ART", Rock01%(), Rock01Mask%()

GetArtDimensions "ROCK02.ART", w%, h%
DIM Rock02%(1 TO w%, 1 TO h%), Rock02Mask%(1 TO w%, 1 TO h%)
LoadArt "ROCK02.ART", Rock02%(), Rock02Mask%()

GetArtDimensions "ROCK03.ART", w%, h%
DIM Rock03%(1 TO w%, 1 TO h%), Rock03Mask%(1 TO w%, 1 TO h%)
LoadArt "ROCK03.ART", Rock03%(), Rock03Mask%()

GetArtDimensions "ROCK04.ART", w%, h%
DIM Rock04%(1 TO w%, 1 TO h%), Rock04Mask%(1 TO w%, 1 TO h%)
LoadArt "ROCK04.ART", Rock04%(), Rock04Mask%()

GetArtDimensions "ROCK05.ART", w%, h%
DIM Rock05%(1 TO w%, 1 TO h%), Rock05Mask%(1 TO w%, 1 TO h%)
LoadArt "ROCK05.ART", Rock05%(), Rock05Mask%()

GetArtDimensions "ROCK06.ART", w%, h%
DIM Rock06%(1 TO w%, 1 TO h%), Rock06Mask%(1 TO w%, 1 TO h%)
LoadArt "ROCK06.ART", Rock06%(), Rock06Mask%()

GetArtDimensions "ROCK07.ART", w%, h%
DIM Rock07%(1 TO w%, 1 TO h%), Rock07Mask%(1 TO w%, 1 TO h%)
LoadArt "ROCK07.ART", Rock07%(), Rock07Mask%()

GetArtDimensions "ROCK08.ART", w%, h%
DIM Rock08%(1 TO w%, 1 TO h%), Rock08Mask%(1 TO w%, 1 TO h%)
LoadArt "ROCK08.ART", Rock08%(), Rock08Mask%()

GetArtDimensions "ROCK09.ART", w%, h%
DIM Rock09%(1 TO w%, 1 TO h%), Rock09Mask%(1 TO w%, 1 TO h%)
LoadArt "ROCK09.ART", Rock09%(), Rock09Mask%()

GetArtDimensions "ROCK10.ART", w%, h%
DIM Rock10%(1 TO w%, 1 TO h%), Rock10Mask%(1 TO w%, 1 TO h%)
LoadArt "ROCK10.ART", Rock10%(), Rock10Mask%()

GetArtDimensions "ROCK11.ART", w%, h%
DIM Rock11%(1 TO w%, 1 TO h%), Rock11Mask%(1 TO w%, 1 TO h%)
LoadArt "ROCK11.ART", Rock11%(), Rock11Mask%()

GetArtDimensions "ROCK12.ART", w%, h%
DIM Rock12%(1 TO w%, 1 TO h%), Rock12Mask%(1 TO w%, 1 TO h%)
LoadArt "ROCK12.ART", Rock12%(), Rock12Mask%()

GetArtDimensions "ROCK13.ART", w%, h%
DIM Rock13%(1 TO w%, 1 TO h%), Rock13Mask%(1 TO w%, 1 TO h%)
LoadArt "ROCK13.ART", Rock13%(), Rock13Mask%()

GetArtDimensions "ROCK14.ART", w%, h%
DIM Rock14%(1 TO w%, 1 TO h%), Rock14Mask%(1 TO w%, 1 TO h%)
LoadArt "ROCK14.ART", Rock14%(), Rock14Mask%()

GetArtDimensions "ROCK15.ART", w%, h%
DIM Rock15%(1 TO w%, 1 TO h%), Rock15Mask%(1 TO w%, 1 TO h%)
LoadArt "ROCK15.ART", Rock15%(), Rock15Mask%()

GetArtDimensions "ROCK16.ART", w%, h%
DIM Rock16%(1 TO w%, 1 TO h%), Rock16Mask%(1 TO w%, 1 TO h%)
LoadArt "ROCK16.ART", Rock16%(), Rock16Mask%()

GetArtDimensions "BOOM01.ART", w%, h%
DIM Boom01%(1 TO w%, 1 TO h%), Boom01Mask%(1 TO w%, 1 TO h%)
LoadArt "BOOM01.ART", Boom01%(), Boom01Mask%()

GetArtDimensions "BOOM02.ART", w%, h%
DIM Boom02%(1 TO w%, 1 TO h%), Boom02Mask%(1 TO w%, 1 TO h%)
LoadArt "BOOM02.ART", Boom02%(), Boom02Mask%()

GetArtDimensions "BOOM03.ART", w%, h%
DIM Boom03%(1 TO w%, 1 TO h%), Boom03Mask%(1 TO w%, 1 TO h%)
LoadArt "BOOM03.ART", Boom03%(), Boom03Mask%()

GetArtDimensions "BOOM04.ART", w%, h%
DIM Boom04%(1 TO w%, 1 TO h%), Boom04Mask%(1 TO w%, 1 TO h%)
LoadArt "BOOM04.ART", Boom04%(), Boom04Mask%()

GetArtDimensions "BOOM05.ART", w%, h%
DIM Boom05%(1 TO w%, 1 TO h%), Boom05Mask%(1 TO w%, 1 TO h%)
LoadArt "BOOM05.ART", Boom05%(), Boom05Mask%()

GetArtDimensions "BOOM06.ART", w%, h%
DIM Boom06%(1 TO w%, 1 TO h%), Boom06Mask%(1 TO w%, 1 TO h%)
LoadArt "BOOM06.ART", Boom06%(), Boom06Mask%()

GetArtDimensions "BOOM07.ART", w%, h%
DIM Boom07%(1 TO w%, 1 TO h%), Boom07Mask%(1 TO w%, 1 TO h%)
LoadArt "BOOM07.ART", Boom07%(), Boom07Mask%()

GetArtDimensions "BOOM08.ART", w%, h%
DIM Boom08%(1 TO w%, 1 TO h%), Boom08Mask%(1 TO w%, 1 TO h%)
LoadArt "BOOM08.ART", Boom08%(), Boom08Mask%()

GetArtDimensions "BOOM09.ART", w%, h%
DIM Boom09%(1 TO w%, 1 TO h%), Boom09Mask%(1 TO w%, 1 TO h%)
LoadArt "BOOM09.ART", Boom09%(), Boom09Mask%()

GetArtDimensions "BOOM10.ART", w%, h%
DIM Boom10%(1 TO w%, 1 TO h%), Boom10Mask%(1 TO w%, 1 TO h%)
LoadArt "BOOM10.ART", Boom10%(), Boom10Mask%()

GetArtDimensions "SPLAT.ART", w%, h%
DIM Splat%(1 TO w%, 1 TO h%), SplatMask%(1 TO w%, 1 TO h%)
LoadArt "SPLAT.ART", Splat%(), SplatMask%()

GetArtDimensions "SPACEMAN.ART", w%, h%
DIM SHARED SpaceMan%(1 TO w%, 1 TO h%), SpaceManMask%(1 TO w%, 1 TO h%)
LoadArt "SPACEMAN.ART", SpaceMan%(), SpaceManMask%()

GetArtDimensions "DOG.ART", w%, h%
DIM SHARED Dog%(1 TO w%, 1 TO h%), DogMask%(1 TO w%, 1 TO h%)
LoadArt "DOG.ART", Dog%(), DogMask%()

GetArtDimensions "FONT.ART", w%, h%
DIM Font%(1 TO w%, 1 TO h%), FontMask%(1 TO w%, 1 TO h%)
LoadArt "FONT.ART", Font%(), FontMask%()

' Phew!

' Get set up to draw text
DIM FontWidths%(0 TO 127)

InitializeFont

' Get set up to render the fighter
GetObjectCounts "FIGHTER.OBJ", numvertices%, numnormals%, numfaces%

DIM FighterVertices(1 TO numvertices%) AS PointType
DIM FighterNormals(1 TO numnormals%) AS PointType
DIM FighterFaces(1 TO numfaces%) AS FaceType

DIM FighterLightPosition AS PointType

LoadObject "FIGHTER.OBJ", FighterVertices(), FighterNormals(), FighterFaces()

' Get set up to draw aliens
DIM AlienSpecies(0 TO 4) AS AlienSpeciesType

' Species:
'   0 - Little guy
'   1 - Kamikaze (medium)
'   2 - Mine layer (big)
'   3 - Mine from alien 1
'   4 - Mothership
'   5 - Floater

AlienSpecies(0).Radius = 6: AlienSpecies(0).Value = 5
AlienSpecies(1).Radius = 35: AlienSpecies(1).Value = 25
AlienSpecies(2).Radius = 17: AlienSpecies(2).Value = 70
AlienSpecies(3).Radius = 6: AlienSpecies(3).Value = 2
AlienSpecies(4).Radius = 4: AlienSpecies(4).Value = 1: AlienSpecies(4).Squishy = 1

' State

DIM Shots(1 TO MaxShots%) AS ShotType
DIM Dusts(1 TO MaxDusts%) AS DustType
DIM Aliens(1 TO MaxAliens%) AS AlienType
DIM Rocks(1 TO MaxRocks%) AS RockType
DIM Booms(1 TO MaxBooms%) AS BoomType
DIM Splats(1 TO MaxSplats%) AS SplatType

InitializeDust Dusts()

' Rate limit the gun

guncooldown% = 3 ' frames
gunheat% = 0

nextround% = RoundInterval%
roundnumber% = 0

' 3D transfom

DIM transform#(0 TO 3, 0 TO 3)
DIM rotation#(0 TO 3, 0 TO 3) ' same as transform, but without translation

' Hit testing

DIM SHARED HitGrid%(0 TO HitGridWidth% - 1, 0 TO HitGridHeight% - 1)
DIM SHARED FighterHit%, DeathFrames%, FramesSinceDeath%

SCREEN 13

DIM SHARED buf&(16001)

GET (0, 0)-(319, 199), buf&

DEF SEG = VARSEG(buf&(1))

DIM SHARED bufofs%

bufofs% = VARPTR(buf&(1))

FighterLightPosition.X = -30
FighterLightPosition.Y = -90
FighterLightPosition.Z = CameraZ#

TitleScreen FighterVertices(), FighterNormals(), FighterFaces(), FighterLightPosition

' Reset mouse driver
rin.AX = 0
CALL InterruptX(&H33, rin, rout)

fighterx# = 160
fighterdx# = 0
fightertiltangle# = 0

score& = 0

ProgramTimerRate FPSLimit%

DO
  framestarttime! = TIMER

  ' Get mouse
  rin.AX = 3
  CALL InterruptX(&H33, rin, rout)

  mousebutton% = rout.BX
  mousex% = rout.CX \ 2 ' Mouse driver always reports 0..639
  mousey% = rout.DX

  ' Fighter movement
  targetx% = mousex%
  IF targetx% < MarginX% THEN targetx% = MarginX%
  IF targetx% > 319 - MarginX% THEN targetx% = 319 - MarginX%

  delta# = targetx% - fighterx#

  IF NOT FighterHit% THEN
    fighterdx# = fighterdx# * FighterMomentum# + ATN(delta#) * FighterReticleAttraction#
  END IF

  fighterx# = fighterx# + fighterdx#
  fightertiltangle# = ATN(delta# * FighterTiltSpeed#) * FighterTiltMax#

  ' Shooting
  IF gunheat% > 0 THEN gunheat% = gunheat% - 1

  IF mousebutton% AND (FighterHit% = 0) THEN
    FOR shotindex% = 1 TO MaxShots%
      IF Shots(shotindex%).Active = 0 THEN EXIT FOR
    NEXT shotindex%

    IF shotindex% <= MaxShots% THEN
      IF gunheat% = 0 THEN
        gunheat% = guncooldown%

        angle# = PI# * .5 - 12 * SIN(fightertiltangle#) / 57.295779
        cosa# = COS(angle#)
        sina# = SIN(angle#)

        Shots(shotindex%).Active = 1
        Shots(shotindex%).ShotX = fighterx# + cosa# * ShotStartOffset#
        Shots(shotindex%).ShotY = FighterY% - sina# * ShotStartOffset#
        Shots(shotindex%).ShotAngle = angle#
      END IF
    END IF
  END IF

  ' Alien round
  nextround% = nextround% - 1

  IF nextround% <= 0 THEN
    nextround% = RoundInterval%
    roundnumber% = roundnumber% + 1

    StartRound Aliens(), roundnumber%, nextround%
  END IF

  ' Fighter transform
  SetIdentityTransform transform#()
  SetIdentityTransform rotation#()

  ScaleTransform transform#(), 3

  RotateTransformZ transform#(), -10 * SIN(fightertiltangle#) / 57.295779
  RotateTransformY transform#(), 10 * SIN(fightertiltangle#) / 57.295779
  RotateTransformX transform#(), -100 / 57.295779

  ' Clear buffer
  FOR i% = 1 TO 16000
    buf&(i%) = 0
  NEXT i%

  ' Hamburgers
  UpdateAndRenderDust Dusts()

  CheckForHits Shots(), Rocks(), Aliens(), Booms(), Splats(), (fighterx#), FighterY%, nextround%, score&

  UpdateAndRenderAliens Aliens(), (fighterx#), FighterY%, (fighterdx#), Booms()

  UpdateAndRenderRocks Rocks(), (fighterx#), FighterY%, (fighterdx#), Booms()

  UpdateAndRenderSplats Splats()
  UpdateAndRenderBooms Booms()

  IF FighterHit% = 0 THEN
    RenderObject FighterVertices(), FighterNormals(), FighterFaces(), transform#(), rotation#(), FighterLightPosition, LightIntensity#, AmbientLightIntensity#, INT(fighterx#), FighterY%
  END IF

  FOR i% = 1 TO MaxShots%
    UpdateAndRenderShot Shots(i%)
  NEXT i%

  IF FighterHit% = 0 THEN
    DrawReticle mousex%, mousey%
  END IF

  DrawScore score&

  ' Blit
  PUT (0, 0), buf&, PSET

  IF DeathFrames% > 0 THEN
    DeathFrames% = DeathFrames% - 1
    FramesSinceDeath% = FramesSinceDeath% + 1
    CascadeFighterBoom (fighterx#), FighterY%, (fighterdx#), Booms()
  END IF

  ' Framerate limit
  WHILE TIMER = framestarttime!: WEND
LOOP WHILE (FighterHit% = 0) OR (DeathFrames% > 0)

SUB AddPoint (t AS PointType, s AS PointType)
t.X = t.X + s.X
t.Y = t.Y + s.Y
t.Z = t.Z + s.Z
END SUB

SUB ApplyTransform (i AS PointType, matrix#(), o AS PointType)
o.X = i.X * matrix#(0, 0) + i.Y * matrix#(1, 0) + i.Z * matrix#(2, 0) + matrix#(3, 0)
o.Y = i.X * matrix#(0, 1) + i.Y * matrix#(1, 1) + i.Z * matrix#(2, 1) + matrix#(3, 1)
o.Z = i.X * matrix#(0, 2) + i.Y * matrix#(1, 2) + i.Z * matrix#(2, 2) + matrix#(3, 2)
END SUB

SUB CascadeFighterBoom (fx%, fy%, fdx%, Booms() AS BoomType)
IF FramesSinceDeath% = 13 OR FramesSinceDeath% = 18 OR FramesSinceDeath% = 27 THEN
  MakeBoom Booms(), fx%, fy%, fdx%, 0
END IF
END SUB

SUB CentreText (tx%, ty%, text$)
SHARED FontWidths%()

w% = MeasureText%(text$)

tx% = tx% - w% \ 2

FOR i% = 1 TO LEN(text$)
  ch% = ASC(MID$(text$, i%, 1))

  glyphwidth% = FontWidths%(ch%)

  PutGlyph tx%, ty%, ch%, glyphwidth%

  tx% = tx% + glyphwidth% + 1
NEXT i%
END SUB

SUB CheckForHits (Shots() AS ShotType, Rocks() AS RockType, Aliens() AS AlienType, Booms() AS BoomType, Splats() AS SplatType, fx%, fy%, nextround%, score&)
SHARED Rock01%()
SHARED AlienSpecies() AS AlienSpeciesType

' Render a hit grid for quick collision tests
FOR xx% = 0 TO HitGridWidth% - 1
  FOR yy% = 0 TO HitGridHeight% - 1
    HitGrid%(xx%, yy%) = 0
  NEXT yy%
NEXT xx%

rocksize% = UBOUND(Rock01%, 1) - LBOUND(Rock01%, 1)

rocksizex% = rocksize% * HitGridWidth% \ 800
rocksizey% = rocksize% * HitGridHeight% \ 600

FOR i% = 1 TO MaxRocks%
  IF Rocks(i%).Active THEN
    xx% = Rocks(i%).X * HitGridWidth% \ 320
    yy% = Rocks(i%).Y * HitGridHeight% \ 200

    x1% = xx% - rocksizex%
    x2% = xx% + rocksizex%
    y1% = yy% - rocksizey%
    y2% = yy% + rocksizey%

    IF x1% < 0 THEN x1% = 0
    IF y1% < 0 THEN y1% = 0
    IF x2% >= HitGridWidth% THEN x2% = HitGridWidth% - 1
    IF y2% >= HitGridHeight% THEN y2% = HitGridHeight% - 1

    FOR xx% = x1% TO x2%
      FOR yy% = y1% TO y2%
        HitGrid%(xx%, yy%) = HitGridRock% + i%
      NEXT yy%
    NEXT xx%
  END IF
NEXT i%

activealiens% = 0

FOR i% = 1 TO MaxAliens%
  IF Aliens(i%).Active THEN
    activealiens% = activealiens% + 1

    xx% = Aliens(i%).ActualX * HitGridWidth% \ 320
    yy% = Aliens(i%).ActualY * HitGridHeight% \ 200

    aliensize% = AlienSpecies(Aliens(i%).Species).Radius

    aliensizex% = aliensize% * HitGridWidth% \ 320
    aliensizey% = aliensize% * HitGridHeight% \ 200

    x1% = xx% - aliensizex%
    x2% = xx% + aliensizey%
    y1% = yy% - aliensizex%
    y2% = yy% + aliensizey%

    IF x1% < 0 THEN x1% = 0
    IF y1% < 0 THEN y1% = 0
    IF x2% >= HitGridWidth% THEN x2% = HitGridWidth% - 1
    IF y2% >= HitGridHeight% THEN y2% = HitGridHeight% - 1

    aliensize2% = aliensize% * aliensize%

    FOR px% = x1% TO x2%
      FOR py% = y1% TO y2%
        pdx% = (xx% - px%) * 320 \ HitGridWidth%
        pdy% = (yy% - py%) * 200 \ HitGridHeight%

        IF pdx% * pdx% + pdy% * pdy% <= aliensize2% THEN
          HitGrid%(px%, py%) = HitGridAlien% + i%
        END IF
      NEXT py%
    NEXT px%
  END IF
NEXT i%

' Check for fighter collisions
IF NOT FighterHit% THEN
  fightery1% = fy% - FighterRadius%
  fightery2% = fy% + FighterRadius%

  hyy% = fightery1% * HitGridHeight% \ 200

  IF fightery2% > 199 THEN fightery2% = 199

  DO
    syy% = hyy% * 200 \ HitGridHeight%

    IF syy% >= fightery2% THEN EXIT DO

    ' Maximize chord width
    IF syy% < fy% THEN syy% = syy% + HitGridHeight% - 1

    dsyy% = ABS(syy% - fy%)

    chordwidth% = SQR(FighterRadius% * FighterRadius% - dsyy% * dsyy%)

    fighterx1% = fx% - chordwidth%
    fighterx2% = fx% + chordwidth%

    FOR hxx% = fighterx1% * HitGridWidth% \ 320 TO fighterx2% * HitGridWidth% \ 320 + 1
      IF hxx% >= HitGridWidth% THEN EXIT FOR
      IF hxx% >= 0 THEN
        hit% = HitGrid%(hxx%, hyy%)

        IF hit% THEN
          squishyhit% = 0

          IF hit% > HitGridAlien% THEN
            hit% = hit% - HitGridAlien%

            IF AlienSpecies(Aliens(hit%).Species).Squishy THEN
              squishyhit% = -1
            END IF
          END IF

          IF squishyhit% THEN
            Aliens(hit%).Active = 0
            score& = score& + AlienSpecies(Aliens(hit%).Species).Value
            MakeSplat Splats(), Aliens(hit%).X, Aliens(hit%).Y
          ELSE
            FighterHit% = -1
            MakeBoom Booms(), fx%, fy%, fdx%, 0
            DeathFrames% = 15 * BoomFrames%
            EXIT DO
          END IF
        END IF
      END IF
    NEXT hxx%

    hyy% = hyy% + 1
  LOOP
END IF

' Check for shot hits
shotlength% = ShotSpeed# * HitGridHeight% \ 200

FOR i% = 1 TO MaxShots%
  IF Shots(i%).Active THEN
    xx% = Shots(i%).ShotX * HitGridWidth% \ 320
    yy% = Shots(i%).ShotY * HitGridHeight% \ 200

    IF xx% >= 0 AND xx% < HitGridWidth% THEN
      IF yy% >= 0 AND yy% < HitGridHeight% THEN
        ' Quick hack: test the boxes below the shot because that's
        ' roughly where it was previously.
        FOR yyy% = yy% + shotlength% - 1 TO yy% STEP -1
          IF yyy% < HitGridHeight% THEN
            hit% = HitGrid%(xx%, yyy%)
            IF hit% THEN EXIT FOR
          END IF
        NEXT yyy%

        IF hit% THEN
          Shots(i%).Active = 0

          SELECT CASE hit%
            CASE HitGridRock% TO HitGridAlien%:
              hit% = hit% - HitGridRock%

              Rocks(hit%).Active = 0

              MakeBoom Booms(), Rocks(hit%).X, Rocks(hit%).Y, Rocks(hit%).DX, RockSpeed%
            CASE IS > HitGridAlien%:
              hit% = hit% - HitGridAlien%

              IF Aliens(hit%).Active THEN
                Aliens(hit%).HealthPoints = Aliens(hit%).HealthPoints - 1
                Aliens(hit%).Damaged = AlienDamageVisibleFrames%

                IF Aliens(hit%).HealthPoints <= 0 THEN
                  Aliens(hit%).Active = 0

                  score& = score& + AlienSpecies(Aliens(hit%).Species).Value

                  activealiens% = activealiens% - 1

                  ' TODO: ejected aliens

                  IF AlienSpecies(killedspecies%).Squishy THEN
                    MakeSplat Splats(), Aliens(hit%).X, Aliens(hit%).Y
                  ELSE
                    MakeBoom Booms(), Aliens(hit%).ActualX, Aliens(hit%).ActualY, 0, 0
                  END IF
                END IF
              END IF
          END SELECT
        END IF
      END IF
    END IF
  END IF
NEXT i%

IF (activealiens% = 0) AND (nextround% > RoundInterval% \ 2) THEN
  nextround% = RoundInterval% \ 2
END IF
END SUB

SUB CreateKamikaze (Aliens() AS AlienType, ox#, oy#, odx#, ody#, acceleration#, activatey%, dropy%)
FOR i% = 1 TO MaxAliens%
  IF Aliens(i%).Active = 0 THEN
    Aliens(i%).Active = -1
    Aliens(i%).Species = 1 ' Kamikaze - Medium Alien (ALIEN2)
    Aliens(i%).HealthPoints = KamikazeAlienHealthPoints%

    ' Argument1: Acceleration
    ' Argument2: Activate Y
    ' Argument3: Drop Y

    Aliens(i%).Argument1 = acceleration#
    Aliens(i%).Argument2 = activatey%
    Aliens(i%).Argument3 = dropy%

    Aliens(i%).X = ox#
    Aliens(i%).Y = oy#
    Aliens(i%).DX = odx#
    Aliens(i%).DY = ody#

    EXIT FOR
  END IF
NEXT i%
END SUB

SUB CreateMine (Aliens() AS AlienType, ox#, oy#)
FOR i% = 1 TO MaxAliens%
  IF Aliens(i%).Active = 0 THEN
    Aliens(i%).Active = -1
    Aliens(i%).Species = 3 ' Mine (ALIEN3)
    Aliens(i%).HealthPoints = MineHealthPoints%

    Aliens(i%).X = ox#
    Aliens(i%).Y = oy#
    Aliens(i%).DX = 0
    Aliens(i%).DY = MineSpeed#

    EXIT FOR
  END IF
NEXT i%
END SUB

SUB CreateMineLayer (Aliens() AS AlienType, ox#, oy#)
SHARED AlienSpecies() AS AlienSpeciesType
STATIC lastslot%

hh% = AlienSpecies(1).Radius * 2 + 3

FOR i% = 1 TO MaxAliens%
  IF Aliens(i%).Active AND (Aliens(i%).Species = 1) THEN
    existingtargety% = Aliens(i%).Argument1
    IF existingtargety% < hh% THEN
      slot0% = 1
    ELSEIF existingtargety% < 2 * hh% THEN
      slot1% = 1
    ELSEIF existingtargety% < 3 * hh% THEN
      slot2% = 1
    END IF

    IF slot0% AND slot1% AND slot2% THEN EXIT FOR
  END IF
NEXT i%

IF slot0% = 0 THEN
  slot% = 0
ELSEIF slot1% = 0 THEN
  slot% = 1
ELSEIF slot2% = 0 THEN
  slot% = 2
ELSE
  slot% = (lastslot% + 1) MOD 3
END IF

lastslot% = slot%

targety% = slot% * hh% + AlienSpecies(1).Radius

FOR i% = 1 TO MaxAliens%
  IF Aliens(i%).Active = 0 THEN
    Aliens(i%).Active = -1
    Aliens(i%).Species = 2 ' Large Alien (ALIEN2)
    Aliens(i%).HealthPoints = MineLayerHealthPoints%

    ' Argument1: Target Y
    ' Argument2: State
    ' Argument3: Rotation
    ' Argument4: (Move state) Target X

    Aliens(i%).Argument1 = targety%
    Aliens(i%).Argument2 = 0
    Aliens(i%).Argument3 = 0

    Aliens(i%).X = ox#
    Aliens(i%).Y = oy#
    Aliens(i%).DX = 0
    Aliens(i%).DY = MineLayerDropSpeed#

    EXIT FOR
  END IF
NEXT i%
END SUB

SUB CreateOrbitCluster (Aliens() AS AlienType, clustersize%, ox#, oy#, odx#, ody#)
n% = 1
a# = 2 * PI# * n% / clustersize%

FOR i% = 1 TO MaxAliens%
  IF Aliens(i%).Active = 0 THEN
    Aliens(i%).Active = -1
    Aliens(i%).Species = 0 ' Orbit Cluster - Tiny Alien (ALIEN0)
    Aliens(i%).HealthPoints = OrbitClusterAlienHealthPoints%

    ' Argument1: Time
    ' Argument2: Phase
    ' Argument3: Radius

    Aliens(i%).Argument1 = a#
    Aliens(i%).Argument2 = a#
    Aliens(i%).Argument3 = clustersize% * 2 + 6

    Aliens(i%).X = ox#
    Aliens(i%).Y = oy#
    Aliens(i%).DX = odx#
    Aliens(i%).DY = ody#

    n% = n% + 1

    IF n% > clustersize% THEN EXIT FOR

    a# = 2 * PI# * n% / clustersize%
  END IF
NEXT i%
END SUB

FUNCTION Distance# (t AS PointType)
Distance# = SQR(t.X * t.X + t.Y * t.Y + t.Z * t.Z)
END FUNCTION

FUNCTION DotProduct# (a AS PointType, b AS PointType)
DotProduct# = a.X * b.X + a.Y * b.Y + a.Z * b.Z
END FUNCTION

SUB DrawAlien (alien AS AlienType, ox%, oy%)
xx# = alien.X + ox%
yy# = alien.Y + oy%

alien.ActualX = xx#
alien.ActualY = yy#

SELECT CASE alien.Species
  CASE 0: ' Little guy
    DrawAlien0 xx#, yy#
  CASE 1: ' Medium fighter
    DrawAlien1 xx#, yy#, alien
  CASE 2: ' Big mine layer
    DrawAlien2 xx#, yy#, alien
  CASE 3: ' Mine
    DrawAlien2Mine xx#, yy#, alien
END SELECT
END SUB

SUB DrawAlien0 (xx#, yy#)
SHARED Alien0%(), Alien0Mask%()
PutArt xx#, yy#, 0, Alien0%(), Alien0Mask%()
END SUB

SUB DrawAlien1 (xx#, yy#, alien AS AlienType)
SHARED Alien1A%(), Alien1AMask%()
SHARED Alien1B%(), Alien1BMask%()
SHARED Alien1D%(), Alien1DMask%()

a# = alien.DX * -.1

IF alien.Damaged THEN
  PutArt xx#, yy#, a#, Alien1D%(), Alien1DMask%()
ELSE
  SELECT CASE alien.SpriteIndex
    CASE 0: PutArt xx#, yy#, a#, Alien1A%(), Alien1AMask%()
    CASE 1: PutArt xx#, yy#, a#, Alien1B%(), Alien1BMask%()
  END SELECT
END IF
END SUB

SUB DrawAlien2 (xx#, yy#, alien AS AlienType)
SHARED Alien2A%(), Alien2AMask%()
SHARED Alien2B%(), Alien2BMask%()
SHARED Alien2D%(), Alien2DMask%()

a# = alien.Argument3

IF alien.Damaged > 0 THEN
  PutArt xx#, yy#, a#, Alien2D%(), Alien2DMask%()
ELSE
  SELECT CASE alien.SpriteIndex
    CASE 0: PutArt xx#, yy#, a#, Alien2A%(), Alien2AMask%()
    CASE 1: PutArt xx#, yy#, a#, Alien2B%(), Alien2BMask%()
  END SELECT
END IF
END SUB

SUB DrawAlien2Mine (xx#, yy#, alien AS AlienType)
SHARED Alien3A%(), Alien3AMask%()
SHARED Alien3B%(), Alien3BMask%()

a# = alien.FrameNumber / 30

SELECT CASE alien.SpriteIndex
  CASE 0: PutArt xx#, yy#, a#, Alien3A%(), Alien3AMask%()
  CASE 1: PutArt xx#, yy#, a#, Alien3B%(), Alien3BMask%()
END SELECT
END SUB

SUB DrawBoom (boom AS BoomType)
SHARED Boom01%(), Boom01Mask%()
SHARED Boom02%(), Boom02Mask%()
SHARED Boom03%(), Boom03Mask%()
SHARED Boom04%(), Boom04Mask%()
SHARED Boom05%(), Boom05Mask%()
SHARED Boom06%(), Boom06Mask%()
SHARED Boom07%(), Boom07Mask%()
SHARED Boom08%(), Boom08Mask%()
SHARED Boom09%(), Boom09Mask%()
SHARED Boom10%(), Boom10Mask%()

SELECT CASE boom.SpriteIndex
  CASE 1: PutArt (boom.X), (boom.Y), 0, Boom01%(), Boom01Mask%()
  CASE 2: PutArt (boom.X), (boom.Y), 0, Boom02%(), Boom02Mask%()
  CASE 3: PutArt (boom.X), (boom.Y), 0, Boom03%(), Boom03Mask%()
  CASE 4: PutArt (boom.X), (boom.Y), 0, Boom04%(), Boom04Mask%()
  CASE 5: PutArt (boom.X), (boom.Y), 0, Boom05%(), Boom05Mask%()
  CASE 6: PutArt (boom.X), (boom.Y), 0, Boom06%(), Boom06Mask%()
  CASE 7: PutArt (boom.X), (boom.Y), 0, Boom07%(), Boom07Mask%()
  CASE 8: PutArt (boom.X), (boom.Y), 0, Boom08%(), Boom08Mask%()
  CASE 9: PutArt (boom.X), (boom.Y), 0, Boom09%(), Boom09Mask%()
  CASE 10: PutArt (boom.X), (boom.Y), 0, Boom10%(), Boom10Mask%()
END SELECT
END SUB

SUB DrawReticle (ox%, oy%)
' ** **
'*     *
'*  *  *
'  ***
'*  *  *
'*     *
' ** **
scanofs& = oy% * 320& + bufofs%

IF oy% >= 3 AND oy% < 203 THEN
  HorizontalLine scanofs& - 3 * 320, ox% - 2, ox% - 1, ReticleColour%
  HorizontalLine scanofs& - 3 * 320, ox% + 1, ox% + 2, ReticleColour%
END IF

IF oy% >= 2 AND oy% < 202 THEN
  HorizontalLine scanofs& - 2 * 320, ox% - 3, ox% - 3, ReticleColour%
  HorizontalLine scanofs& - 2 * 320, ox% + 3, ox% + 3, ReticleColour%
END IF

IF oy% >= 1 AND oy% < 201 THEN
  HorizontalLine scanofs& - 1 * 320, ox% - 3, ox% - 3, ReticleColour%
  HorizontalLine scanofs& - 1 * 320, ox%, ox%, ReticleColour%
  HorizontalLine scanofs& - 1 * 320, ox% + 3, ox% + 3, ReticleColour%
END IF

IF oy% >= 0 AND oy% < 200 THEN
  HorizontalLine scanofs&, ox% - 1, ox% + 1, ReticleColour%
END IF

IF oy% >= -1 AND oy% < 199 THEN
  HorizontalLine scanofs& + 1 * 320, ox% - 3, ox% - 3, ReticleColour%
  HorizontalLine scanofs& + 1 * 320, ox%, ox%, ReticleColour%
  HorizontalLine scanofs& + 1 * 320, ox% + 3, ox% + 3, ReticleColour%
END IF

IF oy% >= -2 AND oy% < 198 THEN
  HorizontalLine scanofs& + 2 * 320, ox% - 3, ox% - 3, ReticleColour%
  HorizontalLine scanofs& + 2 * 320, ox% + 3, ox% + 3, ReticleColour%
END IF

IF oy% >= -3 AND oy% < 197 THEN
  HorizontalLine scanofs& + 3 * 320, ox% - 2, ox% - 1, ReticleColour%
  HorizontalLine scanofs& + 3 * 320, ox% + 1, ox% + 2, ReticleColour%
END IF
END SUB

SUB DrawRock (rock AS RockType)
SHARED Rock01%(), Rock01Mask%()
SHARED Rock02%(), Rock02Mask%()
SHARED Rock03%(), Rock03Mask%()
SHARED Rock04%(), Rock04Mask%()
SHARED Rock05%(), Rock05Mask%()
SHARED Rock06%(), Rock06Mask%()
SHARED Rock07%(), Rock07Mask%()
SHARED Rock08%(), Rock08Mask%()
SHARED Rock09%(), Rock09Mask%()
SHARED Rock10%(), Rock10Mask%()
SHARED Rock11%(), Rock11Mask%()
SHARED Rock12%(), Rock12Mask%()
SHARED Rock13%(), Rock13Mask%()
SHARED Rock14%(), Rock14Mask%()
SHARED Rock15%(), Rock15Mask%()
SHARED Rock16%(), Rock16Mask%()

WHILE rock.SpriteIndex < 1
  rock.SpriteIndex = rock.SpriteIndex + 16
WEND

WHILE rock.SpriteIndex > 16
  rock.SpriteIndex = rock.SpriteIndex - 16
WEND

SELECT CASE rock.SpriteIndex
  CASE 1: PutArt (rock.X), (rock.Y), 0, Rock01%(), Rock01Mask%()
  CASE 2: PutArt (rock.X), (rock.Y), 0, Rock02%(), Rock02Mask%()
  CASE 3: PutArt (rock.X), (rock.Y), 0, Rock03%(), Rock03Mask%()
  CASE 4: PutArt (rock.X), (rock.Y), 0, Rock04%(), Rock04Mask%()
  CASE 5: PutArt (rock.X), (rock.Y), 0, Rock05%(), Rock05Mask%()
  CASE 6: PutArt (rock.X), (rock.Y), 0, Rock06%(), Rock06Mask%()
  CASE 7: PutArt (rock.X), (rock.Y), 0, Rock07%(), Rock07Mask%()
  CASE 8: PutArt (rock.X), (rock.Y), 0, Rock08%(), Rock08Mask%()
  CASE 9: PutArt (rock.X), (rock.Y), 0, Rock09%(), Rock09Mask%()
  CASE 10: PutArt (rock.X), (rock.Y), 0, Rock10%(), Rock10Mask%()
  CASE 11: PutArt (rock.X), (rock.Y), 0, Rock11%(), Rock11Mask%()
  CASE 12: PutArt (rock.X), (rock.Y), 0, Rock12%(), Rock12Mask%()
  CASE 13: PutArt (rock.X), (rock.Y), 0, Rock13%(), Rock13Mask%()
  CASE 14: PutArt (rock.X), (rock.Y), 0, Rock14%(), Rock14Mask%()
  CASE 15: PutArt (rock.X), (rock.Y), 0, Rock15%(), Rock15Mask%()
  CASE 16: PutArt (rock.X), (rock.Y), 0, Rock16%(), Rock16Mask%()
END SELECT
END SUB

SUB DrawScore (score&)
CentreText 255, 0, "SCORE:"
CentreText 300, 0, RIGHT$("00000" + LTRIM$(STR$(score&)), 5)
END SUB

SUB DrawSplat (splatinfo AS SplatType)
SHARED Splat%(), SplatMask%()

PutArt (splatinfo.X), (splatinfo.Y), splatinfo.SplatAngle, Splat%(), SplatMask%()
END SUB

SUB FillTriangle (x1%, y1%, x2%, y2%, x3%, y3%, c%)
' Sort coordinates
IF y1% > y2% THEN
  SWAP x1%, x2%
  SWAP y1%, y2%
END IF
IF y2% > y3% THEN
  SWAP x2%, x3%
  SWAP y2%, y3%
END IF
IF y1% > y2% THEN
  SWAP x1%, x2%
  SWAP y1%, y2%
END IF

o& = y1% * 320& + bufofs%

IF y1% = y3% THEN
  IF y1% >= 0 AND y1% < 200 THEN
    mx% = x1%
    IF x2% < mx% THEN mx% = x2%
    IF x3% < mx% THEN mx% = x3%

    nx% = x1%
    IF x2% > nx% THEN nx% = x2%
    IF x3% > nx% THEN nx% = x3%

    HorizontalLine o&, mx%, nx%, c%
  END IF
ELSE
  xfp% = 0
  dxfp% = (x3% - x1%) * 256& \ (y3% - y1% + 1)
  xx% = x1%

  IF (y2% > y1%) AND (y2% >= 0) AND (y1% < 200) THEN
    x2fp% = 0
    dx2fp% = (x2% - x1%) * 256& \ (y2% - y1%)
    xx2% = x1%

    FOR yy% = y1% TO y2%
      IF yy% >= 200 THEN EXIT FOR
      IF yy% >= 0 THEN HorizontalLine o&, xx%, xx2%, c%

      xfp% = xfp% + dxfp%
      WHILE xfp% < 0: xx% = xx% - 1: xfp% = xfp% + 256: WEND
      WHILE xfp% >= 256: xx% = xx% + 1: xfp% = xfp% - 256: WEND

      x2fp% = x2fp% + dx2fp%
      WHILE x2fp% < 0: xx2% = xx2% - 1: x2fp% = x2fp% + 256: WEND
      WHILE x2fp% >= 256: xx2% = xx2% + 1: x2fp% = x2fp% - 256: WEND

      o& = o& + 320&
    NEXT yy%
  ELSEIF y1% >= 0 AND y1% < 200 THEN
    HorizontalLine o&, xx%, x2%, c%

    xfp% = xfp% + dxfp%
    WHILE xfp% < 0: xx% = xx% - 1: xfp% = xfp% + 256: WEND
    WHILE xfp% >= 256: xx% = xx% + 1: xfp% = xfp% - 256: WEND

    o& = o& + 320&
  ELSE
    o& = (y2% + 1) * 320& + bufofs%
  END IF

  IF y3% > y2% THEN
    x2fp% = 0
    dx2fp% = (x3% - x2%) * 256& \ (y3% - y2% + 1)
    xx2% = x2%

    FOR yy% = y2% + 1 TO y3% - 1
      IF yy% >= 200 THEN EXIT FOR

      x2fp% = x2fp% + dx2fp%
      WHILE x2fp% < 0: xx2% = xx2% - 1: x2fp% = x2fp% + 256: WEND
      WHILE x2fp% >= 256: xx2% = xx2% + 1: x2fp% = x2fp% - 256: WEND

      IF yy% >= 0 THEN HorizontalLine o&, xx%, xx2%, c%

      xfp% = xfp% + dxfp%
      WHILE xfp% < 0: xx% = xx% - 1: xfp% = xfp% + 256: WEND
      WHILE xfp% >= 256: xx% = xx% + 1: xfp% = xfp% - 256: WEND

      o& = o& + 320&
    NEXT yy%

    x2fp% = x2fp% + dx2fp%
    WHILE x2fp% < 0: xx2% = xx2% - 1: x2fp% = x2fp% + 256: WEND
    WHILE x2fp% >= 256: xx2% = xx2% + 1: x2fp% = x2fp% - 256: WEND

    IF yy% >= 0 AND yy% < 200 THEN HorizontalLine o&, xx%, xx2%, c%
  END IF
END IF
END SUB

SUB GetArtDimensions (filename$, w%, h%)
ff% = FREEFILE

OPEN filename$ FOR BINARY AS #ff%
w% = ASC(INPUT$(1, ff%))
h% = ASC(INPUT$(1, ff%))
CLOSE #ff%
END SUB

SUB GetObjectCounts (objfilename$, numvertices%, numnormals%, numfaces%)
ff% = FREEFILE

OPEN objfilename$ FOR INPUT AS #ff%

numvertices% = 0
numnormals% = 0
numfaces% = 0

DO
  LINE INPUT #ff%, a$

  a$ = LTRIM$(a$)

  IF LEN(a$) >= 2 THEN
    SELECT CASE LEFT$(a$, 2)
      CASE "v ": numvertices% = numvertices% + 1
      CASE "vn": numnormals% = numnormals% + 1
      CASE "f ": numfaces% = numfaces% + 1
    END SELECT
  END IF
LOOP UNTIL EOF(ff%)

CLOSE #ff%
END SUB

SUB HorizontalLine (scanofs&, x1%, x2%, c%)
IF x1% <= x2% THEN
  IF x1% < 0 THEN x1% = 0
  IF x2% > 319 THEN x2% = 319
  FOR xx% = x1% TO x2%
    POKE scanofs& + xx%, c%
  NEXT xx%
ELSE
  IF x2% < 0 THEN x2% = 0
  IF x1% > 319 THEN x1% = 319
  FOR xx% = x2% TO x1%
    POKE scanofs& + xx%, c%
  NEXT xx%
END IF
END SUB

SUB InitializeDust (Dusts() AS DustType)
FOR i% = 1 TO MaxDusts%
  Dusts(i%).Position.X = RND * (DustXMax# - DustXMin#) + DustXMin#
  Dusts(i%).Position.Y = RND * (DustYMax# - DustYMin#) + DustYMin#
  Dusts(i%).Position.Z = RND * (DustZMax# - DustZMin#) + DustZMin#
  Dusts(i%).Colour = RandomDustColour%
NEXT i%
END SUB

SUB InitializeFont ()
SHARED Font%(), FontMask%(), FontWidths%()

gw% = (UBOUND(Font%, 1) - LBOUND(Font%, 1) + 1) \ 16
gh% = (UBOUND(Font%, 2) - LBOUND(Font%, 2) + 1) \ 8

FOR ch% = 0 TO 127
  chx% = (ch% AND 15) * gw% + 1
  chy% = (ch% \ 16) * gh% + 1

  FontWidths%(ch%) = 0

  FOR xx% = gw% - 1 TO 0 STEP -1
    FOR yy% = 0 TO gh% - 1
      IF Font%(chx% + xx%, chy% + yy%) THEN
        FontWidths%(ch%) = xx% + 1
        IF Font%(chx% + xx%, chy% + yy%) = 13 THEN
          FontMask%(chx% + xx%, chy% + yy%) = 0
        END IF
        EXIT FOR
      END IF
    NEXT yy%

    IF FontWidths%(ch%) > 0 THEN EXIT FOR
  NEXT xx%

  IF FontWidths%(ch%) = 0 THEN FontWidths%(ch%) = gw%
NEXT ch%
END SUB

FUNCTION IsNaNOrInf% (d#)
e# = d# / d#
IsNaNOrInf% = e# <> e#
END FUNCTION

SUB LoadArt (filename$, pixels%(), mask%())
ff% = FREEFILE

OPEN filename$ FOR BINARY AS #ff%

w% = ASC(INPUT$(1, ff%))
h% = ASC(INPUT$(1, ff%))

FOR yy% = 1 TO h%
  FOR xx% = 1 TO w%
    pixels%(xx%, yy%) = ASC(INPUT$(1, ff%))
  NEXT xx%
NEXT yy%

FOR yy% = 1 TO h%
  FOR xx% = 1 TO w%
    mask%(xx%, yy%) = ASC(INPUT$(1, ff%))
  NEXT xx%
NEXT yy%

CLOSE #ff%
END SUB

REM $STATIC
SUB LoadObject (objfilename$, vertices() AS PointType, normals() AS PointType, faces() AS FaceType)
ff% = FREEFILE

OPEN objfilename$ FOR INPUT AS #ff%

numvertices% = 0
numnormals% = 0
numfaces% = 0

DO
  LINE INPUT #ff%, a$

  a$ = LTRIM$(a$)

  IF LEN(a$) >= 2 THEN
    SELECT CASE LEFT$(a$, 2)
      CASE "v ":
        numvertices% = numvertices% + 1
        ParseTriple MID$(a$, 3), vertices(numvertices%)
      CASE "vn":
        numnormals% = numnormals% + 1
        ParseTriple MID$(a$, 4), normals(numnormals%)
      CASE "f ":
        numfaces% = numfaces% + 1
        ParseVertices MID$(a$, 3), faces(numfaces%)
    END SELECT
  END IF
LOOP UNTIL EOF(ff%)

CLOSE #ff%
END SUB

SUB MakeBoom (Booms() AS BoomType, ox%, oy%, odx%, ody%)
FOR i% = 1 TO MaxBooms%
  IF Booms(i%).Active = 0 THEN
    Booms(i%).Active = 1
    Booms(i%).X = ox%
    Booms(i%).Y = oy%
    Booms(i%).DX = odx%
    Booms(i%).DY = ody%
    Booms(i%).SpriteIndex = 0

    EXIT FOR
  END IF
NEXT i%
END SUB

SUB MakeSplat (Splats() AS SplatType, ox#, oy#)
FOR i% = 1 TO MaxSplats%
  IF Splats(i%).ActiveForFrames = 0 THEN
    Splats(i%).ActiveForFrames = SplatFrames%
    Splats(i%).X = ox#
    Splats(i%).Y = oy#
    Splats(i%).SplatAngle = RND * (2 * PI#)

    EXIT FOR
  END IF
NEXT i%
END SUB

FUNCTION MeasureText% (text$)
SHARED FontWidths%()

w% = 0

FOR i% = 1 TO LEN(text$)
  w% = w% + FontWidths%(ASC(MID$(text$, i%, 1))) + 1
NEXT i%

MeasureText% = w% - 1
END FUNCTION

REM $DYNAMIC
SUB MultiplyMatrix (subject#(), transform#())
DIM result#(0 TO 3, 0 TO 3)

FOR i% = 0 TO 3
  FOR j% = 0 TO 3
    FOR k% = 0 TO 3
      result#(i%, j%) = result#(i%, j%) + subject#(i%, k%) * transform#(k%, j%)
    NEXT k%
  NEXT j%
NEXT i%

FOR i% = 0 TO 3
  FOR j% = 0 TO 3
    subject#(i%, j%) = result#(i%, j%)
  NEXT j%
NEXT i%
END SUB

SUB MultiplyPoint (t AS PointType, scale#)
t.X = t.X * scale#
t.Y = t.Y * scale#
t.Z = t.Z * scale#
END SUB

SUB NormalizePoint (t AS PointType)
id# = 1# / Distance#(t)

t.X = t.X * id#
t.Y = t.Y * id#
t.Z = t.Z * id#
END SUB

SUB ParseTriple (a$, p AS PointType)
a$ = LTRIM$(a$)
i% = INSTR(a$, " ")

IF i% = 0 THEN
  p.X = VAL(a$)
ELSE
  p.X = VAL(LEFT$(a$, i% - 1))

  a$ = LTRIM$(MID$(a$, i% + 1))
  i% = INSTR(a$, " ")

  IF i% = 0 THEN
    p.Y = VAL(a$)
  ELSE
    p.Y = VAL(LEFT$(a$, i% - 1))

    a$ = LTRIM$(MID$(a$, i% + 1))
    i% = INSTR(a$, " ")

    IF i% = 0 THEN
      p.Z = VAL(a$)
    ELSE
      p.Z = VAL(LEFT$(a$, i% - 1))
    END IF
  END IF
END IF
END SUB

SUB ParseVertex (a$, v AS VertexType)
v.NormalIndex = 0

i% = INSTR(a$, "/")

IF i% = 0 THEN
  v.VertexIndex = VAL(a$)
ELSE
  v.VertexIndex = VAL(LEFT$(a$, i% - 1))

  a$ = MID$(a$, i% + 1)

  IF ASC(a$) = 47 THEN
    a$ = MID$(a$, 2)
    v.NormalIndex = VAL(a$)
  END IF
END IF
END SUB

SUB ParseVertices (a$, f AS FaceType)
a$ = LTRIM$(a$)
i% = INSTR(a$, " ")

IF i% = 0 THEN
  ParseVertex a$, f.P1
ELSE
  ParseVertex LEFT$(a$, i% - 1), f.P1

  a$ = LTRIM$(MID$(a$, i% + 1))
  i% = INSTR(a$, " ")

  IF i% = 0 THEN
    ParseVertex a$, f.P2
  ELSE
    ParseVertex LEFT$(a$, i% - 1), f.P2

    a$ = LTRIM$(MID$(a$, i% + 1))
    i% = INSTR(a$, " ")

    IF i% = 0 THEN
      ParseVertex a$, f.P3
    ELSE
      ParseVertex LEFT$(a$, i% - 1), f.P3
    END IF
  END IF
END IF
END SUB

SUB ProgramTimerRate (tickspersecond#)
CONST BaseFrequency& = 1193181

interval& = BaseFrequency& / tickspersecond#

OUT &H43, &H36
OUT &H40, interval&
OUT &H40, interval& \ 256
END SUB

SUB PutArt (ox#, oy#, angle#, pixels%(), mask%())
w% = UBOUND(pixels%, 1) - LBOUND(pixels%, 1) + 1
h% = UBOUND(pixels%, 2) - LBOUND(pixels%, 2) + 1

l# = (w% - 1) * -.5: r# = (w% - 1) * .5
t# = (h% - 1) * -.5: b# = (h% - 1) * .5

ca# = COS(angle#): sa# = -SIN(angle#)

x1# = ox# + l# * ca# - t# * sa#
y1# = oy# + l# * sa# + t# * ca#

x2# = ox# + r# * ca# - t# * sa#
y2# = oy# + r# * sa# + t# * ca#

x3# = ox# + l# * ca# - b# * sa#
y3# = oy# + l# * sa# + b# * ca#

x4# = ox# + r# * ca# - b# * sa#
y4# = oy# + r# * sa# + b# * ca#

sx1% = 1: sy1% = 1
sx2% = w%: sy2% = 1
sx3% = 1: sy3% = h%
sx4% = w%: sy4% = h%

IF y1# > y2# THEN SWAP x1#, x2#: SWAP y1#, y2#: SWAP sx1%, sx2%: SWAP sy1%, sy2%: :
IF y1# > y3# THEN SWAP x1#, x3#: SWAP y1#, y3#: SWAP sx1%, sx3%: SWAP sy1%, sy3%:
IF y1# > y4# THEN SWAP x1#, x4#: SWAP y1#, y4#: SWAP sx1%, sx4%: SWAP sy1%, sy4%: :
IF y2# > y4# THEN SWAP x2#, x4#: SWAP y2#, y4#: SWAP sx2%, sx4%: SWAP sy2%, sy4%:
IF y3# > y4# THEN SWAP x3#, x4#: SWAP y3#, y4#: SWAP sx3%, sx4%: SWAP sy3%, sy4%:

IF x2# > x3# THEN SWAP x2#, x3#: SWAP y2#, y3#: SWAP sx2%, sx3%: SWAP sy2%, sy3%:

x2mx1# = x2# - x1#: sx2msx1% = sx2% - sx1%
x3mx1# = x3# - x1#: sx3msx1% = sx3% - sx1%
x4mx2# = x4# - x2#: sx4msx2% = sx4% - sx2%
x4mx3# = x4# - x3#: sx4msx3% = sx4% - sx3%

y2my1# = y2# - y1#: sy2msy1% = sy2% - sy1%
y3my1# = y3# - y1#: sy3msy1% = sy3% - sy1%
y4my2# = y4# - y2#: sy4msy2% = sy4% - sy2%
y4my3# = y4# - y3#: sy4msy3% = sy4% - sy3%

r21# = x2mx1# / y2my1#
r31# = x3mx1# / y3my1#
r42# = x4mx2# / y4my2#
r43# = x4mx3# / y4my3#

srx21# = sx2msx1% / y2my1#: sry21# = sy2msy1% / y2my1#
srx31# = sx3msx1% / y3my1#: sry31# = sy3msy1% / y3my1#
srx42# = sx4msx2% / y4my2#: sry42# = sy4msy2% / y4my2#
srx43# = sx4msx3% / y4my3#: sry43# = sy4msy3% / y4my3#

' Collapse NaN
IF IsNaNOrInf%(r21#) THEN r21# = 0
IF IsNaNOrInf%(r31#) THEN r31# = 0: br31% = 1:
IF IsNaNOrInf%(r42#) THEN r42# = 0
IF IsNaNOrInf%(r43#) THEN r43# = 0: br43% = 1:

IF IsNaNOrInf%(srx21#) THEN srx21# = 0
IF IsNaNOrInf%(srx31#) THEN srx31# = 0: bsrx31% = 1: : : :
IF IsNaNOrInf%(srx42#) THEN srx42# = 0
IF IsNaNOrInf%(srx43#) THEN srx43# = 0: bsrx43% = 1: : : :

IF IsNaNOrInf%(sry21#) THEN sry21# = 0
IF IsNaNOrInf%(sry31#) THEN sry31# = 0: bsry31% = 1: : :
IF IsNaNOrInf%(sry42#) THEN sry42# = 0
IF IsNaNOrInf%(sry43#) THEN sry43# = 0: bsry43% = 1: : :

x1# = x1# - .5
x2# = x2# - .5
x3# = x3# - .5
x4# = x4# - .5

y1# = y1# - .5
y2# = y2# - .5
y3# = y3# - .5
y4# = y4# - .5

yy1% = FIX(y1#): IF yy1% <> y1# THEN yy1% = yy1% + 1
yy2% = FIX(y4#)

IF yy1% < 0 THEN yy1% = 0
IF yy2% > 199 THEN yy2% = 199

so& = yy1% * 320& + bufofs%

FOR yy% = yy1% TO yy2%
  IF yy% < y2# THEN
    yyp# = yy% - y1#
    xx1# = yyp# * r21# + x1#
    sxx1# = yyp# * srx21# + sx1%
    syy1# = yyp# * sry21# + sy1%
  ELSE
    yyp# = yy% - y2#
    xx1# = yyp# * r42# + x2#
    sxx1# = yyp# * srx42# + sx2%
    syy1# = yyp# * sry42# + sy2%
  END IF

  IF yy% < y3# THEN
    yyp# = yy% - y1#
    xx2# = yyp# * r31# + x1# + br31% * x3mx1#
    sxx2# = yyp# * srx31# + sx1% + bsrx31% * sx3msx1%
    syy2# = yyp# * sry31# + sy1% + bsry31% * sy3msy1%
  ELSE
    yyp# = yy% - y3#
    xx2# = yyp# * r43# + x3# + br43% * x4mx3#
    sxx2# = yyp# * srx43# + sx3% + bsrx43% * sx4msx3%
    syy2# = yyp# * sry43# + sy3% + bsry43% * sy4msy3%
  END IF

  ww# = xx2# - xx1#
  srxx# = (sxx2# - sxx1#) / ww#
  sryy# = (syy2# - syy1#) / ww#

  IF IsNaNOrInf%(srxx#) THEN srxx# = 0
  IF IsNaNOrInf%(sryy#) THEN sryy# = 0

  xx1% = FIX(xx1#): IF xx1% <> xx1# THEN xx1% = xx1% + 1
  xx2% = FIX(xx2#)

  IF xx1% < 0 THEN xx1% = 0
  IF xx2% > 319 THEN xx2% = 319

  o& = so& + xx1%

  FOR xx% = xx1% TO xx2%
    xxp# = xx% - xx1#

    sx% = FIX(xxp# * srxx# + sxx1#)
    sy% = FIX(xxp# * sryy# + syy1#)

    IF sx% >= 1 AND sx% <= w% AND sy% >= 1 AND sy% <= h% THEN
      IF mask%(sx%, sy%) THEN POKE o&, pixels%(sx%, sy%)
      'IF mask%(sx%, sy%) THEN PSET (xx%, yy%), pixels%(sx%, sy%)
    END IF

    o& = o& + 1
  NEXT xx%

  so& = so& + 320
NEXT yy%
END SUB

SUB PutGlyph (gx%, gy%, ch%, w%)
SHARED Font%(), FontMask%(), FontWidths%()

gw% = (UBOUND(Font%, 1) - LBOUND(Font%, 1) + 1) \ 16
gh% = (UBOUND(Font%, 2) - LBOUND(Font%, 2) + 1) \ 8

chx% = (ch% AND 15) * gw% + 1
chy% = (ch% \ 16) * gh% + 1

o& = gy% * 320& + gx% + bufofs%

x1% = 0
x2% = w% - 1

IF gx% + x1% < 0 THEN x1% = -gx%
IF gx% + x2% >= 319 THEN x2% = 319 - gx%

FOR yy% = 0 TO gh% - 1
  IF yy% >= 200 THEN EXIT FOR

  IF yy% >= 0 THEN
    FOR xx% = x1% TO x2%
      IF FontMask%(chx% + xx%, chy% + yy%) THEN
        POKE o& + xx%, Font%(chx% + xx%, chy% + yy%)
      END IF
    NEXT xx%
  END IF

  o& = o& + 320
NEXT yy%
END SUB

FUNCTION RandomDustColour%
r# = RND

IF r# < .05 THEN
  RandomDustColour% = 6
ELSEIF r# < .1 THEN
  RandomDustColour% = 4
ELSEIF r# < .15 THEN
  RandomDustColour% = 11
ELSE
  c% = r# * 16
  IF c% > 15 THEN c% = 15
  RandomDustColour% = 16 + c%
END IF
END FUNCTION

SUB RenderObject (sourcevertices() AS PointType, sourcenormals() AS PointType, faces() AS FaceType, transform#(), rotation#(), lightposition AS PointType, LightIntensity#, AmbientLightIntensity#, ox%, oy%)
' Transform vertices into place
DIM vertices(LBOUND(sourcevertices) TO UBOUND(sourcevertices)) AS PointType

FOR i% = LBOUND(sourcevertices) TO UBOUND(sourcevertices)
  ApplyTransform sourcevertices(i%), transform#(), vertices(i%)
NEXT i%

' Transform normals
DIM normals(LBOUND(sourcenormals) TO UBOUND(sourcenormals)) AS PointType

FOR i% = LBOUND(sourcenormals) TO UBOUND(sourcenormals)
  ApplyTransform sourcenormals(i%), rotation#(), normals(i%)
NEXT i%

' Sort faces
DIM faceP1 AS VertexType
DIM faceP2 AS VertexType
DIM faceP3 AS VertexType
DIM faceCentre AS PointType
DIM faceNormal AS PointType

FOR i% = LBOUND(faces) TO UBOUND(faces)
  faceP1 = faces(i%).P1
  faceP2 = faces(i%).P2
  faceP3 = faces(i%).P3

  z1# = vertices(faceP1.VertexIndex).Z
  z2# = vertices(faceP2.VertexIndex).Z
  z3# = vertices(faceP3.VertexIndex).Z

  IF z1# < z2# THEN
    IF z1# < z3# THEN faces(i%).SortZ = z1# ELSE faces(i%).SortZ = z3#
  ELSE
    IF z2# < z3# THEN faces(i%).SortZ = z2# ELSE faces(i%).SortZ = z3#
  END IF

  faceCentre = vertices(faceP1.VertexIndex)
  AddPoint faceCentre, vertices(faceP2.VertexIndex)
  AddPoint faceCentre, vertices(faceP3.VertexIndex)
  MultiplyPoint faceCentre, 1# / 3#

  faceNormal = normals(faceP1.NormalIndex)
  AddPoint faceNormal, normals(faceP2.NormalIndex)
  AddPoint faceNormal, normals(faceP3.NormalIndex)
  NormalizePoint faceNormal

  faces(i%).Centre = faceCentre
  faces(i%).Normal = faceNormal
NEXT i%

SortFaces faces(), LBOUND(faces), UBOUND(faces)

' Project vertices - from this point on, vertex (X, Y) is screen
FOR i% = LBOUND(vertices) TO UBOUND(vertices)
  dz# = vertices(i%).Z - CameraZ#
  dzf# = 160 / dz#

  vertices(i%).X = ox% + vertices(i%).X * dzf#
  vertices(i%).Y = oy% - vertices(i%).Y * dzf#
NEXT i%

' Render
DIM cameraposition AS PointType

cameraposition.X = 0
cameraposition.Y = 0
cameraposition.Z = CameraZ#

DIM tolight AS PointType
DIM tocamera AS PointType
DIM reflection AS PointType

FOR i% = UBOUND(faces) TO LBOUND(faces) STEP -1
  p1i% = faces(i%).P1.VertexIndex
  p2i% = faces(i%).P2.VertexIndex
  p3i% = faces(i%).P3.VertexIndex

  x1# = vertices(p1i%).X
  y1# = vertices(p1i%).Y
  x2# = vertices(p2i%).X
  y2# = vertices(p2i%).Y
  x3# = vertices(p3i%).X
  y3# = vertices(p3i%).Y

  cross1# = x1# * y2# - y1# * x2#
  cross2# = x2# * y3# - y2# * x3#
  cross3# = x3# * y1# - y3# * x1#

  testwinding# = cross1# + cross2# + cross3#

  ' Only draw triangles that are facing the screen
  IF testwinding# > 0 THEN
    ' Lighting
    tolight = lightposition
    SubtractPoint tolight, faces(i%).Centre
    NormalizePoint tolight

    tocamera = cameraposition
    SubtractPoint tocamera, faces(i%).Centre
    NormalizePoint tocamera

    reflection = faces(i%).Normal
    MultiplyPoint reflection, 2 * DotProduct#(faces(i%).Normal, tolight)
    SubtractPoint reflection, tolight
    NormalizePoint reflection

    diffuse# = DotProduct(tolight, faces(i%).Normal)
    specular# = DotProduct(reflection, tocamera)

    IF diffuse# < 0 THEN diffuse# = 0
    IF specular# < 0 THEN specular# = 0 ELSE specular# = specular# ^ Shininess#

    light# = AmbientLightIntensity# + LightIntensity# * (diffuse# + specular#)

    ' Paint
    x1% = INT(x1#)
    y1% = INT(y1#)
    x2% = INT(x2#)
    y2% = INT(y2#)
    x3% = INT(x3#)
    y3% = INT(y3#)
    c% = INT(light# * 16) + 16

    IF c% < 16 THEN c% = 16
    IF c% > 31 THEN c% = 31

    FillTriangle x1%, y1%, x2%, y2%, x3%, y3%, c%
  END IF
NEXT i%
END SUB

REM $STATIC
SUB RotateTransformX (matrix#(), angle#)
DIM transform#(0 TO 3, 0 TO 3)

ca# = COS(angle#)
sa# = SIN(angle#)

transform#(0, 0) = 1
transform#(1, 1) = ca#
transform#(1, 2) = sa#
transform#(2, 1) = -sa#
transform#(2, 2) = ca#
transform#(3, 3) = 1

MultiplyMatrix matrix#(), transform#()
END SUB

SUB RotateTransformY (matrix#(), angle#)
DIM transform#(0 TO 3, 0 TO 3)

ca# = COS(angle#)
sa# = SIN(angle#)

transform#(1, 1) = 1
transform#(0, 0) = ca#
transform#(2, 0) = sa#
transform#(0, 2) = -sa#
transform#(2, 2) = ca#
transform#(3, 3) = 1

MultiplyMatrix matrix#(), transform#()
END SUB

SUB RotateTransformZ (matrix#(), angle#)
DIM transform#(0 TO 3, 0 TO 3)

ca# = COS(angle#)
sa# = SIN(angle#)

transform#(2, 2) = 1
transform#(0, 0) = ca#
transform#(0, 1) = sa#
transform#(1, 0) = -sa#
transform#(1, 1) = ca#
transform#(3, 3) = 1

MultiplyMatrix matrix#(), transform#()
END SUB

SUB ScaleTransform (matrix#(), factor#)
FOR i% = 0 TO 3
  FOR j% = 0 TO 3
    matrix#(i%, j%) = matrix#(i%, j%) * factor#
  NEXT j%
NEXT i%
END SUB

SUB SetIdentityTransform (transform#())
FOR i% = 0 TO 3
  FOR j% = 0 TO 3
    transform#(i%, j%) = ABS(i% = j%)
  NEXT j%
NEXT i%
END SUB

SUB SortFaces (faces() AS FaceType, first%, last%)
' Try to avoid pathological cases
r% = last% - first% + 1

s1% = INT(RND * r%) + first%
s2% = INT(RND * r%) + first%
s3% = INT(RND * r%) + first%

IF faces(s1%).SortZ > faces(s2%).SortZ THEN
  SWAP faces(s1%), faces(s2%)
END IF
IF faces(s2%).SortZ > faces(s3%).SortZ THEN
  SWAP faces(s2%), faces(s3%)
END IF
IF faces(s1%).SortZ > faces(s2%).SortZ THEN
  SWAP faces(s1%), faces(s2%)
END IF

' Put the median of the 3 randomly selected values at the
' start as the initial pivot.
SWAP faces(s2%), faces(first%)

' Pivot
pivot% = first%
pivotz# = faces(pivot%).SortZ
FOR i% = first% + 1 TO last%
  IF faces(i%).SortZ < pivotz# THEN
    SWAP faces(pivot%), faces(pivot% + 1)

    IF i% <> pivot% + 1 THEN
      SWAP faces(pivot%), faces(i%)

      i% = i% - 1
    END IF

    pivot% = pivot% + 1
  END IF
NEXT i%

' Recurse
IF pivot% - first% >= 2 THEN SortFaces faces(), first%, pivot% - 1
IF last% - pivot% >= 2 THEN SortFaces faces(), pivot% + 1, last%
END SUB

SUB StartRound (Aliens() AS AlienType, roundnumber%, nextround%)
SHARED AlienSpecies() AS AlienSpeciesType

STATIC lastroundtype%, secondlastroundtype%

' Try to reduce repetitiveness
minroundtype% = roundnumber% \ 6 - 2
maxroundtype% = minroundtype% + 2

IF minroundtype% < 0 THEN minroundtype% = 0
IF maxroundtype% > 2 THEN maxroundtype% = 2

DO
  roundtype% = INT(RND * (maxroundtype% - minroundtype% + 1)) + minroundtype%

  IF roundtype% < 0 THEN roundtype% = 0
  IF roundtype% > 2 THEN roundtype% = 2
LOOP WHILE (minroundtype% <> maxroundtype%) AND (roundtype% = lastroundtype%) AND (roundtype% = secondlastroundtype%)

secondlastroundtype% = lastroundtype%
lastroundtype% = roundtype%

maxroundtype% = roundtype%
IF roundnumber% = 15 THEN minroundtype% = 1 ELSE minroundtype% = roundtype%

FOR roundtype% = minroundtype% TO maxroundtype%
  SELECT CASE roundtype%
    CASE 0: ' Orbit Clusters
      clustersize% = (roundnumber% + 2) \ 4 + 3
      IF clustersize% > 6 THEN clustersize% = 6

      FOR i% = 0 TO 3
        CreateOrbitCluster Aliens(), clustersize%, (i% AND 1) * 100 + 110, -20 - 40 * i%, 0, 2
      NEXT i%
    CASE 1: ' Kamikaze Fighters
      numaliens% = INT(RND * 3) + roundnumber% \ 4 + 1
      acceleration# = roundnumber% / 16
      activatey% = KamikazeAlienActivateY%
      dropy% = KamikazeAlienDropY%
      IF roundnumber% > 6 THEN
        activatey% = activatey% - roundnumber% * 3
        dropy% = dropy% - roundnumber% * 5

        IF dropy% < 60 THEN dropy% = 60
      END IF

      FOR i% = 1 TO numaliens%
        CreateKamikaze Aliens(), INT(RND * 320), -AlienSpecies(2).Radius - 20 * (i% - 1), 0, .3, acceleration#, activatey%, dropy%
      NEXT i%

      nextround% = nextround% + 5 * RoundInterval%
    CASE 2: ' Mine Layers
      CreateMineLayer Aliens(), INT(RND * 320), -2 * AlienSpecies(1).Radius - 20 * (i% - 1)
      nextround% = nextround% + 15 * RoundInterval%
  END SELECT
NEXT roundtype%
END SUB

SUB SubtractPoint (t AS PointType, s AS PointType)
t.X = t.X - s.X
t.Y = t.Y - s.Y
t.Z = t.Z - s.Z
END SUB

SUB TitleScreen (FighterVertices() AS PointType, FighterNormals() AS PointType, FighterFaces() AS FaceType, FighterLightPosition AS PointType)
GetArtDimensions "TITLE.ART", w%, h%
DIM Title%(1 TO w%, 1 TO h%), TitleMask%(1 TO w%, 1 TO h%)
LoadArt "TITLE.ART", Title%(), TitleMask%()

DIM transform#(0 TO 3, 0 TO 3)
DIM rotation#(0 TO 3, 0 TO 3) ' same as transform, but without translation

float# = 0

DO
  FOR i% = 1 TO 16000
    buf&(i%) = 0
  NEXT i%

  SetIdentityTransform transform#()
  SetIdentityTransform rotation#()

  ScaleTransform transform#(), 20

  RotateTransformX transform#(), .3
  RotateTransformX rotation#(), .3

  RotateTransformY transform#(), 3
  RotateTransformY rotation#(), 3

  Translate transform#(), 0, 20 * SIN(float#), 80

  float# = float# + .15

  RenderObject FighterVertices(), FighterNormals(), FighterFaces(), transform#(), rotation#(), FighterLightPosition, LightIntensity#, AmbientLightIntensity#, 50, 60

  PutArt 200, 60, 0, Title%(), TitleMask%()

  CentreText 200, 100, "They came to earth."
  CentreText 200, 109, "They killed your dog."
  CentreText 200, 118, "They're going to pay."

  CentreText 160, 150, "Your Shrike is faster than anything"
  CentreText 160, 159, "they've got. Pursue them back to"
  CentreText 160, 168, "their homeworld."

  CentreText 160, 181, "Press any key"

  PUT (0, 0), buf&, PSET
LOOP WHILE INKEY$ = ""
END SUB

SUB Translate (matrix#(), deltax#, deltay#, deltaz#)
matrix#(3, 0) = matrix#(3, 0) + deltax#
matrix#(3, 1) = matrix#(3, 1) + deltay#
matrix#(3, 2) = matrix#(3, 2) + deltaz#
END SUB

SUB UpdateAndRenderAliens (Aliens() AS AlienType, fx%, fy%, fdx%, Booms() AS BoomType)
SHARED AlienSpecies() AS AlienSpeciesType

FOR i% = 1 TO MaxAliens%
  IF Aliens(i%).Active THEN
    ox% = 0
    oy% = 0

    SELECT CASE Aliens(i%).Species
      CASE 0: ' Orbit Cluster
        ' Argument1: Time
        ' Argument2: Phase
        ' Argument3: Radius
        r# = (COS(Aliens(i%).Argument1) + 3) / 2
        Aliens(i%).Argument1 = Aliens(i%).Argument1 + .1 / (r# - .9)

        t# = Aliens(i%).Argument1 * 1.3 + Aliens(i%).Argument2
        r# = r# * Aliens(i%).Argument3

        ox% = r# * COS(t#)
        oy% = r# * SIN(t#)

        IF Aliens(i%).Y > OrbitClusterAlienVeerOffY% THEN
          Aliens(i%).DY = Aliens(i%).DY * .96

          IF Aliens(i%).X < 160 THEN
            Aliens(i%).DX = -OrbitClusterAlienVeerOffDX# + (Aliens(i%).DX + OrbitClusterAlienVeerOffDX#) * .92
          ELSE
            Aliens(i%).DX = OrbitClusterAlienVeerOffDX# + (Aliens(i%).DX - OrbitClusterAlienVeerOffDX#) * .92
          END IF
        END IF
      CASE 1: ' Kamikaze
        ' Argument1: Acceleration (after drop)
        ' Argument2: Activate Y
        ' Argument3: Drop Y

        Aliens(i%).DX = Aliens(i%).DX * KamikazeAlienDragFactor#

        IF Aliens(i%).Y > Aliens(i%).Argument2 THEN
          dfx# = fx% - Aliens(i%).X
          IF ABS(dfx#) > 1 THEN
            dfx# = dfx# - SGN(dfx#)
            Aliens(i%).DX = Aliens(i%).DX + dfx# * KamikazeAlienAttractionFactor#
          END IF
        END IF

        IF Aliens(i%).DX < -KamikazeAlienMaxSpeed# THEN Aliens(i%).DX = -KamikazeAlienMaxSpeed#
        IF Aliens(i%).DX > KamikazeAlienMaxSpeed# THEN Aliens(i%).DX = KamikazeAlienMaxSpeed#

        IF Aliens(i%).Y >= Aliens(i%).Argument3 THEN Aliens(i%).DY = Aliens(i%).DY + Aliens(i%).Argument1

        IF (Aliens(i%).FrameNumber AND 7) = 0 THEN
          Aliens(i%).SpriteIndex = 1 - Aliens(i%).SpriteIndex
        END IF
      CASE 2: ' Mine Layer
        ' Argument1: Target Y
        ' Argument2: State (  Drop to target Y
        '                        |
        '                        v
        '               .-> Rotate to scan->Scan->Rotate from scan->Move->Mines-.
        '               `-------------------------------------------------------'  )
        ' Argument3: Rotation
        ' Argument4: (Move state) Target X

        state% = INT(Aliens(i%).Argument2)

        SELECT CASE state%
          CASE 0: ' Drop to target Y
            IF Aliens(i%).Y >= Aliens(i%).Argument1 THEN
              Aliens(i%).DY = Aliens(i%).DY * MineLayerDecelerationFactor# - .1
              IF Aliens(i%).DY < 0 THEN
                Aliens(i%).DY = 0
                Aliens(i%).Argument2 = 1 ' Rotate to scan
                Aliens(i%).Argument3 = 0
                Aliens(i%).FrameNumber = 0
              END IF
            END IF
          CASE 1: ' Rotate to scan
            IF Aliens(i%).FrameNumber > 40 THEN
              Aliens(i%).Argument3 = Aliens(i%).Argument3 + MineLayerRotationSpeed#

              IF Aliens(i%).Argument3 > PI# THEN
                Aliens(i%).Argument3 = PI#
                Aliens(i%).Argument2 = 2 ' Scan
                Aliens(i%).FrameNumber = 0
              END IF
            END IF
          CASE 2: ' Scan
            IF (Aliens(i%).FrameNumber AND 7) = 0 THEN Aliens(i%).SpriteIndex = 1 - Aliens(i%).SpriteIndex

            IF Aliens(i%).FrameNumber > 53 THEN
              Aliens(i%).Argument4 = fx% ' Latch fighter position at this time

              Aliens(i%).Argument2 = 3 ' Rotate from scan
              Aliens(i%).Argument3 = -PI#
            END IF
          CASE 3: ' Rotate from scan
            Aliens(i%).Argument3 = Aliens(i%).Argument3 + MineLayerRotationSpeed#

            IF Aliens(i%).Argument3 > 0# THEN
              Aliens(i%).Argument3 = 0#
              Aliens(i%).Argument2 = 4 ' Move
              Aliens(i%).FrameNumber = 0
            END IF
          CASE 4: ' Move
            IF Aliens(i%).FrameNumber > 40 THEN
              targetdx# = Aliens(i%).Argument4 - Aliens(i%).X

              Aliens(i%).DX = targetdx# * .2 + SGN(targetdx#) * .1

              IF ABS(Aliens(i%).DX) > MineLayerMaxSpeed# THEN
                Aliens(i%).DX = MineLayerMaxSpeed# * SGN(Aliens(i%).DX)
              END IF

              IF ABS(Aliens(i%).DX) > ABS(targetdx#) THEN
                Aliens(i%).DX = targetdx#
                Aliens(i%).Argument2 = 5 ' Lay mines
                Aliens(i%).FrameNumber = 0
              END IF
            END IF
          CASE 5: ' Mine
            Aliens(i%).DX = 0

            IF (Aliens(i%).FrameNumber AND 31) = 0 THEN
              CreateMine Aliens(), Aliens(i%).X, Aliens(i%).Y + AlienSpecies(1).Radius
            END IF

            IF Aliens(i%).FrameNumber > 120 THEN
              Aliens(i%).Argument2 = 1 ' Rotate to scan
              Aliens(i%).Argument3 = 0
              Aliens(i%).FrameNumber = 0
            END IF
        END SELECT
      CASE 3: ' Mine
        IF (Aliens(i%).FrameNumber AND 7) = 0 THEN
          Aliens(i%).SpriteIndex = 1 - Aliens(i%).SpriteIndex
        END IF
    END SELECT

    Aliens(i%).X = Aliens(i%).X + Aliens(i%).DX
    Aliens(i%).Y = Aliens(i%).Y + Aliens(i%).DY

    margin% = AlienSpecies(Aliens(i%).Species).Radius + AlienActiveMargin%

    IF (Aliens(i%).X < -margin%) OR (Aliens(i%).X > 319 + margin%) OR (Aliens(i%).Y > 199 + margin%) THEN
      Aliens(i%).Active = 0
    ELSE
      Aliens(i%).FrameNumber = Aliens(i%).FrameNumber + 1

      IF Aliens(i%).FrameNumber = 32767 THEN Aliens(i%).FrameNumber = 0

      dd% = Aliens(i%).Damaged - 1
      IF dd% >= 0 THEN Aliens(i%).Damaged = dd%

      DrawAlien Aliens(i%), ox%, oy%
    END IF
  END IF
NEXT i%
END SUB

SUB UpdateAndRenderBooms (Booms() AS BoomType)
FOR i% = LBOUND(Booms) TO UBOUND(Booms)
  IF Booms(i%).Active THEN
    Booms(i%).SpriteIndex = Booms(i%).SpriteIndex + 1

    IF Booms(i%).SpriteIndex > BoomFrames% THEN
      Booms(i%).Active = 0
    ELSE
      Booms(i%).X = Booms(i%).X + Booms(i%).DX
      Booms(i%).Y = Booms(i%).Y + Booms(i%).DY

      DrawBoom Booms(i%)
    END IF
  END IF
NEXT i%
END SUB

SUB UpdateAndRenderDust (Dusts() AS DustType)
FOR i% = 1 TO MaxDusts%
  Dusts(i%).Position.Y = Dusts(i%).Position.Y - DustSpeed#

  IF Dusts(i%).Position.Y < DustYMin# THEN
    Dusts(i%).Position.X = RND * (DustXMax# - DustXMin#) + DustXMin#
    Dusts(i%).Position.Y = RND * (DustYMax# - DustYSpawnMin#) + DustYSpawnMin#
    Dusts(i%).Position.Z = RND * (DustZMax# - DustZMin#) + DustZMin#
    Dusts(i%).Colour = RandomDustColour%
  END IF

  dz# = Dusts(i%).Position.Z - CameraZ#
  idz# = 10 / dz#

  sx% = 160 + Dusts(i%).Position.X * idz#
  sy% = 100 - Dusts(i%).Position.Y * idz#

  IF sx% >= 0 AND sx% < 320 THEN
    IF sy% >= 0 AND sy% < 200 THEN
      o& = sy% * 320& + sx%
      POKE o& + bufofs%, Dusts(i%).Colour
    END IF
  END IF
NEXT i%
END SUB

SUB UpdateAndRenderRocks (Rocks() AS RockType, fx%, fy%, fdx%, Booms() AS BoomType)
IF RND < RockSpawnRate# THEN
  FOR i% = LBOUND(Rocks) TO UBOUND(Rocks)
    IF Rocks(i%).Active = 0 THEN
      Rocks(i%).Active = 1
      Rocks(i%).X = RND * 320
      Rocks(i%).DX = INT(RND * 5) - 2
      Rocks(i%).Y = -30

      Rocks(i%).SpriteIndex = INT(RND * 16)

      SELECT CASE RND
        CASE IS < .2: Rocks(i%).SpriteIndexDelta = -1
        CASE IS > .8: Rocks(i%).SpriteIndexDelta = 1
        CASE ELSE: Rocks(i%).SpriteIndexDelta = 0
      END SELECT

      EXIT FOR
    END IF
  NEXT i%
END IF

FOR i% = LBOUND(Rocks) TO UBOUND(Rocks)
  IF Rocks(i%).Active THEN
    Rocks(i%).Y = Rocks(i%).Y + RockSpeed%

    IF Rocks(i%).Y > 230 THEN
      Rocks(i%).Active = 0
    ELSE
      Rocks(i%).X = Rocks(i%).X + Rocks(i%).DX
      Rocks(i%).SpriteIndex = Rocks(i%).SpriteIndex + Rocks(i%).SpriteIndexDelta

      DrawRock Rocks(i%)
    END IF
  END IF
NEXT i%
END SUB

SUB UpdateAndRenderShot (shot AS ShotType)
IF shot.Active THEN
  deltax# = COS(shot.ShotAngle)
  deltay# = SIN(shot.ShotAngle)

  shot.ShotX = shot.ShotX + deltax# * ShotSpeed#
  shot.ShotY = shot.ShotY - deltay# * ShotSpeed#

  x1# = shot.ShotX - deltay# * (ShotWidth# * .5)
  y1# = shot.ShotY - deltax# * (ShotWidth# * .5)

  x2# = shot.ShotX + deltay# * (ShotWidth# * .5)
  y2# = shot.ShotY + deltax# * (ShotWidth# * .5)

  x3# = shot.ShotX - deltax# * ShotTailLength#
  y3# = shot.ShotY + deltay# * ShotTailLength#

  x1% = INT(x1#)
  y1% = INT(y1#)
  x2% = INT(x2#)
  y2% = INT(y2#)
  x3% = INT(x3#)
  y3% = INT(y3#)

  IF y3% < 0 THEN
    shot.Active = 0
  ELSE
    FillTriangle x1%, y1%, x2%, y2%, x3%, y3%, ShotColour%
  END IF
END IF
END SUB

SUB UpdateAndRenderSplats (Splats() AS SplatType)
FOR i% = LBOUND(Splats) TO UBOUND(Splats)
  IF Splats(i%).ActiveForFrames THEN
    Splats(i%).ActiveForFrames = Splats(i%).ActiveForFrames - 1
    DrawSplat Splats(i%)
  END IF
NEXT i%
END SUB

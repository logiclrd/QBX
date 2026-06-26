' Translated from the Chromium T-Rex runner game by the Chromium Authors
'

CONST NightModeFadeSpeed# = .035
CONST NightModeMoonSpeed# = .25
CONST NightModeNumStars% = 2
CONST NightModeStarSpeed# = .3
CONST NightModeStarMaxY% = 70
CONST NightModeNumPhases% = 7
CONST NightModeWidth% = 20

CONST NightModeMaxStarIndex% = NightModeNumStars% - 1

TYPE RunnerType
  CurrentSpeed AS DOUBLE
  HighestScore AS LONG
  TimeValue AS DOUBLE
  RunningTime AS DOUBLE
  PlayCount AS INTEGER
  IsActivated AS INTEGER
  IsCrashed AS INTEGER
  IsInverted AS INTEGER
  InvertTimer AS DOUBLE
END TYPE

TYPE LoopStateType
  ShowInstructions AS INTEGER
  InitialFreeze AS INTEGER
  GameOver AS INTEGER
  LastFrameTime AS SINGLE
  DistanceRan AS DOUBLE
  InvertTrigger AS INTEGER
  IsPaused AS INTEGER
  IsPlaying AS INTEGER
  IsJumpPressed AS INTEGER
  IsDuckPressed AS INTEGER
END TYPE

TYPE DistanceMeterType
  XPos AS INTEGER
  YPos AS INTEGER
  CurrentDistance AS DOUBLE
  CurrentScore AS LONG
  MaxScore AS LONG
  HighScore AS LONG
  ShowAchievement AS INTEGER
  FlashTimer AS DOUBLE
  FlashIterations AS INTEGER
  InvertTrigger AS INTEGER
  MaxScoreUnits AS INTEGER
END TYPE

' Horizon Line.
TYPE HorizonLineType
  XPos AS DOUBLE
  RegionTypes(0 TO 2) AS INTEGER
  BumpThreshold AS DOUBLE
END TYPE

' Cloud background item.
' Similar to an obstacle object but without collision boxes.
TYPE CloudType
  XPos AS INTEGER
  YPos AS INTEGER
  IsActive AS INTEGER
  CloudGap AS INTEGER
END TYPE

TYPE StarType
  XPos AS DOUBLE
  YPos AS INTEGER
END TYPE

' Nightmode shows a moon and stars on the horizon.
TYPE NightModeType
  XPos AS INTEGER
  YPos AS INTEGER
  CurrentPhase AS INTEGER
  Opacity AS DOUBLE
  Stars(0 TO NightModeMaxStarIndex%) AS StarType
  DrawStars AS INTEGER
END TYPE

TYPE TRexAnimationType
  ImageIndices(0 TO 1) AS INTEGER
  NumFrames AS INTEGER
  FrameDuration AS DOUBLE
END TYPE

TYPE TRexType
  TRexStatus AS INTEGER
  CurrentSpeed AS DOUBLE
  IsJumping AS INTEGER
  IsDucking AS INTEGER
  XPos AS INTEGER
  YPos AS INTEGER
  GroundYPos AS INTEGER
  JumpVelocity AS DOUBLE
  MinJumpHeight AS INTEGER
  ReachedMinHeight AS INTEGER
  SpeedDrop AS INTEGER
  ImageIndex AS INTEGER
  ImageWidth AS INTEGER
  ImageHeight AS INTEGER
  CurrentFrame AS INTEGER
  TimeValue AS DOUBLE
  IntroTimeValue AS DOUBLE
  JumpTimeValue AS DOUBLE
  NumFrames AS INTEGER
  BlinkDelay AS INTEGER
  BlinkCount AS INTEGER
  AnimStartTime AS DOUBLE
  PlayingIntro AS INTEGER
END TYPE

TYPE CollisionBoxType
  BoxX AS INTEGER
  BoxY AS INTEGER
  BoxWidth AS INTEGER
  BoxHeight AS INTEGER
END TYPE

TYPE ObstacleVariantType
  ImageIndex AS INTEGER
  ImageWidth AS INTEGER
  ImageHeight AS INTEGER
  MultipleSpeed AS DOUBLE
  MinimumGap AS INTEGER
  MinimumSpeed AS DOUBLE
  SpeedOffset AS DOUBLE
  NumFrames AS INTEGER
  FrameDuration AS DOUBLE
  CollisionBoxes(0 TO 4) AS CollisionBoxType
  CollisionBoxCount AS INTEGER
  YPos(0 TO 5) AS INTEGER
  YPosCount AS INTEGER
END TYPE

TYPE ObstacleType
  Variant AS INTEGER
  GapCoefficient AS DOUBLE
  XPos AS DOUBLE
  YPos AS INTEGER
  Size AS INTEGER
  IsActive AS INTEGER
  ObstacleWidth AS INTEGER
  Gap AS INTEGER
  SpeedOffset AS DOUBLE
  CurrentFrame AS INTEGER
  TimeValue AS DOUBLE
  CollisionBoxes(0 TO 4) AS CollisionBoxType
  CollisionBoxCount AS INTEGER
  FollowingObstacleCreated AS INTEGER
END TYPE

DECLARE SUB AddCloud (clouds() AS CloudType)
DECLARE SUB AddNewObstacle (obstacles() AS ObstacleType, firstactiveobstacle%, nextinactiveobstacle%, speed#)
DECLARE SUB CreateAdjustedCollisionBox (box AS CollisionBoxType, adjustment AS CollisionBoxType, result AS CollisionBoxType)
DECLARE SUB DrawCloud (cloud AS CloudType)
DECLARE SUB DrawDigit (distancemeter AS DistanceMeterType, digitpos%, value%, opthighscore%)
DECLARE SUB DrawGameOver (runner AS RunnerType, loopstate AS LoopStateType, distancemeter AS DistanceMeterType, horizonline AS HorizonLineType, nightmode AS NightModeType, clouds() AS CloudType, trex AS TRexType, obstacles() AS ObstacleType,  _
firstactiveobstacle%, nextinactiveobstacle%)
DECLARE SUB DrawHorizonLine (horizonline AS HorizonLineType)
DECLARE SUB DrawNightMode (runner AS RunnerType, nightmode AS NightModeType)
DECLARE SUB DrawObstacle (obstacle AS ObstacleType)
DECLARE SUB DrawTRex (trex AS TRexType, imageidx%)
DECLARE SUB GameOver (runner AS RunnerType, loopstate AS LoopStateType, distancemeter AS DistanceMeterType, trex AS TRexType)
DECLARE SUB HandleKeys (runner AS RunnerType, loopstate AS LoopStateType, distancemeter AS DistanceMeterType, horizonline AS HorizonLineType, nightmode AS NightModeType, clouds() AS CloudType, trex AS TRexType, obstacles() AS ObstacleType,  _
firstactiveobstacle%, nextinactiveobstacle%, KeyStates%())
DECLARE SUB InitializeCloud (cloud AS CloudType)
DECLARE SUB InitializeCollisionBox (collisionbox AS CollisionBoxType, XPos%, YPos%, widthval%, heightval%)
DECLARE SUB InitializeDistanceMeter (distancemeter AS DistanceMeterType)
DECLARE SUB InitializeGame (runner AS RunnerType, loopstate AS LoopStateType, horizonline AS HorizonLineType, clouds() AS CloudType, nightmode AS NightModeType, obstacles() AS ObstacleType, firstactiveobstacle%, nextinactiveobstacle%, distancemeter  _
AS DistanceMeterType, trex AS TRexType)
DECLARE SUB InitializeHorizonLine (horizonline AS HorizonLineType)
DECLARE SUB InitializeNightMode (nightmode AS NightModeType)
DECLARE SUB InitializeObstacle (obstacle AS ObstacleType, var%, gapcoeff#, speed#, xoffset#)
DECLARE SUB InitializeTRex (trex AS TRexType)
DECLARE SUB LoadImages ()
DECLARE SUB PlaceNightModeStars (nightmode AS NightModeType)
DECLARE SUB PlayGame (runner AS RunnerType, loopstate AS LoopStateType, distancemeter AS DistanceMeterType, horizonline AS HorizonLineType, nightmode AS NightModeType, clouds() AS CloudType, trex AS TRexType, obstacles() AS ObstacleType,  _
firstactiveobstacle%, nextinactiveobstacle%)
DECLARE SUB ProgramTimerRate (tickspersecond#)
DECLARE SUB PutImage (imagex%, imagey%, imageidx%)
DECLARE SUB PutImageOpacity (imagex%, imagey%, imageidx%, imageo#, invertimage%)
DECLARE SUB ResetDistanceMeter (distancemeter AS DistanceMeterType)
DECLARE SUB ResetHorizon (runner AS RunnerType, horizonline AS HorizonLineType, nightmode AS NightModeType, firstactiveobstacle%, nextinactiveobstacle%)
DECLARE SUB ResetHorizonLine (horizonline AS HorizonLineType)
DECLARE SUB ResetNightMode (runner AS RunnerType, nightmode AS NightModeType)
DECLARE SUB ResetTRex (trex AS TRexType)
DECLARE SUB RestartGame (runner AS RunnerType, loopstate AS LoopStateType, distancemeter AS DistanceMeterType, horizonline AS HorizonLineType, nightmode AS NightModeType, clouds() AS CloudType, obstacles() AS ObstacleType, firstactiveobstacle%,  _
nextinactiveobstacle%, trex AS TRexType)
DECLARE SUB SetTRexBlinkDelay (trex AS TRexType)
DECLARE SUB SetTRexIsDucking (trex AS TRexType, newvalue%)
DECLARE SUB SetTRexSpeedDrop (trex AS TRexType)
DECLARE SUB TRexBlink (trex AS TRexType, blinktime#)
DECLARE SUB TRexEndJump (trex AS TRexType)
DECLARE SUB TRexStartJump (trex AS TRexType, speed#)
DECLARE SUB TRexUpdateJump (trex AS TRexType, deltatime#, loopstate AS LoopStateType)
DECLARE SUB UpdateCloud (cloud AS CloudType, speed#)
DECLARE SUB UpdateClouds (clouds() AS CloudType, deltatime#, speed#)
DECLARE SUB UpdateGame (runner AS RunnerType, loopstate AS LoopStateType, distancemeter AS DistanceMeterType, horizonline AS HorizonLineType, nightmode AS NightModeType, clouds() AS CloudType, trex AS TRexType, obstacles() AS ObstacleType,  _
firstactiveobstacle%, nextinactiveobstacle%)
DECLARE SUB UpdateHorizon (runner AS RunnerType, loopstate AS LoopStateType, horizonline AS HorizonLineType, nightmode AS NightModeType, clouds() AS CloudType, obstacles() AS ObstacleType, firstactiveobstacle%, nextinactiveobstacle%, deltatime#,  _
hasobstacles%, ShowNightMode%)
DECLARE SUB UpdateHorizonLine (runner AS RunnerType, horizonline AS HorizonLineType, deltatime#)
DECLARE SUB UpdateHorizonLineXPos (horizonline AS HorizonLineType, increment#)
DECLARE SUB UpdateInvert (runner AS RunnerType, resetinvert%)
DECLARE SUB UpdateNightMode (runner AS RunnerType, nightmode AS NightModeType, activated%, paused%)
DECLARE SUB UpdateObstacle (obstacle AS ObstacleType, deltatime#, speed#)
DECLARE SUB UpdateObstacles (obstacles() AS ObstacleType, firstactiveobstacle%, nextinactiveobstacle%, deltatime#, speed#)
DECLARE SUB UpdateTRex (trex AS TRexType, deltatime#, optstatus%)
DECLARE FUNCTION BoxCompare% (trexbox AS CollisionBoxType, obstaclebox AS CollisionBoxType)
DECLARE FUNCTION CheckForCollision% (obstacle AS ObstacleType, trex AS TRexType)
DECLARE FUNCTION CheckForCollisionB% (obstaclebox AS CollisionBoxType, obstacleboxes() AS CollisionBoxType, trex AS TRexType, trexbox AS CollisionBoxType)
DECLARE FUNCTION CheckForCollisionC% (obstaclebox AS CollisionBoxType, obstacleboxes() AS CollisionBoxType, trexbox AS CollisionBoxType, trexboxes() AS CollisionBoxType)
DECLARE FUNCTION DuplicateObstacleCheck% (nextobstaclevariant%)
DECLARE FUNCTION GetActualDistance& (distance#)
DECLARE FUNCTION GetRandomNum% (minvalue%, maxvalue%)
DECLARE FUNCTION UpdateDistanceMeter% (distancemeter AS DistanceMeterType, deltatime#, distance#)
DECLARE FUNCTION UpdateNightModeXPos# (XPos#, delta#)

' Nominal FPS is 60 fps. BUT, we reprogram the timer so that its
' value changes 60 times per second instead of 18.2 times per
' second. The catch is that the increment doesn't change, so the
' value of TIMER starts going up more quickly. The timer starts
' going up faster by a factor of (60 / 18.2), which means that
' we need to do the math for frame durations and such scaled by
' the inverse amount, (18.2 / 60). So, we tell the game it's
' running at 18.2 fps, and the TIMER value it's working with is
' in fast forward by exactly the amount to produce 60 FPS. :-)

CONST NominalFPS% = 240

CONST TimerFPS# = 1193181# / 65536

CONST FrameDuration# = 1# / TimerFPS#

CONST Acceleration# = .001
CONST BGCloudSpeed# = .2
CONST BottomPad% = 10
CONST ClearTime# = 3
CONST CloudFrequency# = .5
CONST GameOverClearTime# = .75
CONST GapCoefficient# = .6
CONST Gravity# = .6
CONST InitialJumpVelocity# = 12
CONST InvertFadeDuration# = 12
CONST InvertDistance% = 700
CONST MaxBlinkCount% = 3
CONST MaxClouds% = 6
CONST MaxObstacleLength% = 3
CONST MaxObstacleDuplication% = 2
CONST MaxSpeed# = 13
CONST MinJumpHeight% = 35
CONST DefaultSpeed# = 6
CONST SpeedDropCoefficient# = 3
CONST ArcadeModeInitialTopPosition% = 35
CONST ArcadeModeTopPositionPercent# = .1

CONST ObstacleMaxGapCoefficient# = 1.5 ' Coefficient for calculating the maximum gap.
CONST ObstacleMaxLength% = 3 ' Maximum obstacle grouping count.

' Number of digits.
CONST DistanceMeterMaxDistanceUnits% = 5
' Distance that causes achievement animation.
CONST DistanceMeterAchievementDistance% = 100
' Used for conversion from pixel distance to a scaled unit.
CONST DistanceMeterCoefficient# = .025
' Flash duration in seconds.
CONST DistanceMeterFlashDuration# = .25
' Flash iterations for achievement animation.
CONST DistanceMeterFlashIterations% = 3

CONST CloudMaxGap% = 400
CONST CloudMaxSkyLevel% = 30
CONST CloudMinGap% = 100
CONST CloudMinSkyLevel% = 71

CONST CloudWidth% = 46
CONST CloudHeight% = 14

CONST ScreenWidth% = 640
CONST ScreenHeight% = 350

CONST GameWidth% = 546
CONST GameHeight% = 150

CONST GameX% = (ScreenWidth% - GameWidth%) \ 2
CONST GameY% = (ScreenHeight% - GameHeight%) \ 2

CONST GameBottomPad% = 10

' Scan codes
CONST KeySTART% = 28 ' return
CONST KeyJUMP% = 57 ' space
CONST KeyDUCK1% = 42 ' left shift
CONST KeyDUCK2% = 54 ' right shift

CONST KeyStateMaskDUCK% = 3

CONST ImageCACTUSLARGE% = 0 ' x6
CONST ImageCACTUSSMALL% = 6 ' x6
CONST ImageCLOUD% = 12 ' x1
CONST ImageHORIZON% = 13 ' x60
CONST ImageMOON% = 73 ' x8
CONST ImagePTERODACTYL% = 81 ' x2
CONST ImageRESTART% = 83 ' x1
CONST ImageTEXT% = 84 ' x12
CONST ImageGAMEOVER% = 96 ' x4
CONST ImageTREX% = 100 ' x6
CONST ImageDUCKING% = 106 ' x2
CONST ImageSTAR% = 108 ' x3, vertical
CONST ImageINTROTREX% = 111 ' x1

CONST ImageLAST% = ImageINTROTREX%

CONST ImageGAMEOVERPieceWidth% = 48

CONST ImageTREXWidth% = 44
CONST ImageTREXHeight% = 47

CONST ImageDUCKINGWidth% = 59
CONST ImageDUCKINGHeight% = 47

CONST ImageHORIZONPieceWidth% = 20
CONST ImageHORIZONPieceHeight% = 12
CONST ImageHORIZONPieceCount% = 30 ' 30 flat, 30 bumpy, bumpiness is selected by a separate parameter
CONST ImageHORIZONPieceY% = 127

CONST DigitWidth% = 10 ' DistanceMeter.dimensions.WIDTH
CONST DigitHeight% = 13 ' DistanceMeter.dimensions.HEIGHT
CONST DigitSpacing% = 11 ' DistanceMeter.dimensions.DEST_WIDTH

CONST HorizonWidth% = ImageHORIZONPieceWidth% * ImageHORIZONPieceCount%
CONST MaxObstacles% = 20

CONST AchievementSoundPlayString$ = "O3C24G10"
CONST ButtonPressSoundPlayString$ = "O3C40"
CONST HitSoundPlayString$ = "O0L36C.C"

CONST ImageBankCount% = 43
CONST MaxImageWidth% = 59
CONST MaxImageHeight% = 50
CONST MaxImageSize% = ((MaxImageWidth% + 1) \ 2) * MaxImageHeight% + 4
CONST MaxImageWords% = (MaxImageSize% + 1) \ 2

CONST TRexStatusCRASHED% = 1
CONST TRexStatusDUCKING% = 2
CONST TRexStatusJUMPING% = 3
CONST TRexStatusRUNNING% = 4
CONST TRexStatusWAITING% = 5

CONST TRexDropVelocity# = -5
CONST TRexGravity# = .6
CONST TRexInitialJumpVelocity# = -10
CONST TRexIntroDuration# = 1.5
CONST TRexMaxJumpHeight# = 30
CONST TRexMinJumpHeight# = 30
CONST TRexSpeedDropCoefficient# = 3
CONST TRexStartXPos% = 50
CONST TRexBlinkTiming# = 7 ' Blinking coefficient.

DIM TRexDUCKINGCollisionBoxes(0 TO 0) AS CollisionBoxType
DIM TRexRUNNINGCollisionBoxes(0 TO 5) AS CollisionBoxType

InitializeCollisionBox TRexDUCKINGCollisionBoxes(0), 1, 18, 55, 25

InitializeCollisionBox TRexRUNNINGCollisionBoxes(0), 22, 0, 17, 16
InitializeCollisionBox TRexRUNNINGCollisionBoxes(1), 1, 18, 30, 9
InitializeCollisionBox TRexRUNNINGCollisionBoxes(2), 10, 35, 14, 8
InitializeCollisionBox TRexRUNNINGCollisionBoxes(3), 1, 24, 29, 5
InitializeCollisionBox TRexRUNNINGCollisionBoxes(4), 5, 30, 21, 4
InitializeCollisionBox TRexRUNNINGCollisionBoxes(5), 9, 34, 15, 4

' Animation config for different T-Rex states.
DIM trexanimframes(1 TO 5) AS TRexAnimationType

trexanimframes(TRexStatusWAITING%).ImageIndices(0) = ImageTREX% + 1
trexanimframes(TRexStatusWAITING%).ImageIndices(1) = ImageTREX% + 0
trexanimframes(TRexStatusWAITING%).NumFrames = 2
trexanimframes(TRexStatusWAITING%).FrameDuration = 1# / 3#

trexanimframes(TRexStatusRUNNING%).ImageIndices(0) = ImageTREX% + 2
trexanimframes(TRexStatusRUNNING%).ImageIndices(1) = ImageTREX% + 3
trexanimframes(TRexStatusRUNNING%).NumFrames = 2
trexanimframes(TRexStatusRUNNING%).FrameDuration = 1# / 12#

trexanimframes(TRexStatusCRASHED%).ImageIndices(0) = ImageTREX% + 5
trexanimframes(TRexStatusCRASHED%).NumFrames = 1
trexanimframes(TRexStatusCRASHED%).FrameDuration = 1# / TimerFPS#

trexanimframes(TRexStatusJUMPING%).ImageIndices(0) = ImageTREX% + 0
trexanimframes(TRexStatusJUMPING%).NumFrames = 1
trexanimframes(TRexStatusJUMPING%).FrameDuration = 1# / TimerFPS#

trexanimframes(TRexStatusDUCKING%).ImageIndices(0) = ImageDUCKING% + 0
trexanimframes(TRexStatusDUCKING%).ImageIndices(1) = ImageDUCKING% + 1
trexanimframes(TRexStatusDUCKING%).NumFrames = 2
trexanimframes(TRexStatusDUCKING%).FrameDuration = 1# / 8#

' Obstacle definitions.
' minGap: minimum pixel space betweeen obstacles.
' multipleSpeed: Speed at which multiples are allowed.
' speedOffset: speed faster / slower than the horizon.
' minSpeed: Minimum speed which the obstacle can make an appearance.
DIM obstaclevariants(0 TO 2) AS ObstacleVariantType

obstaclevariants(0).ImageIndex = ImageCACTUSSMALL%
obstaclevariants(0).ImageWidth = 17
obstaclevariants(0).ImageHeight = 35
obstaclevariants(0).YPos(0) = 105
obstaclevariants(0).YPosCount = 1
obstaclevariants(0).MultipleSpeed = 4
obstaclevariants(0).MinimumGap = 120
obstaclevariants(0).MinimumSpeed = 0

InitializeCollisionBox obstaclevariants(0).CollisionBoxes(0), 0, 7, 5, 27
InitializeCollisionBox obstaclevariants(0).CollisionBoxes(1), 4, 0, 6, 34
InitializeCollisionBox obstaclevariants(0).CollisionBoxes(2), 10, 4, 7, 14

obstaclevariants(0).CollisionBoxCount = 3

obstaclevariants(1).ImageIndex = ImageCACTUSLARGE%
obstaclevariants(1).ImageWidth = 25
obstaclevariants(1).ImageHeight = 50
obstaclevariants(1).YPos(0) = 90
obstaclevariants(1).YPosCount = 1
obstaclevariants(1).MultipleSpeed = 7
obstaclevariants(1).MinimumGap = 120
obstaclevariants(1).MinimumSpeed = 0

InitializeCollisionBox obstaclevariants(1).CollisionBoxes(0), 0, 12, 7, 38
InitializeCollisionBox obstaclevariants(1).CollisionBoxes(1), 8, 0, 7, 49
InitializeCollisionBox obstaclevariants(1).CollisionBoxes(2), 13, 10, 10, 38

obstaclevariants(1).CollisionBoxCount = 3

obstaclevariants(2).ImageIndex = ImagePTERODACTYL%
obstaclevariants(2).ImageWidth = 46
obstaclevariants(2).ImageHeight = 40
obstaclevariants(2).YPos(0) = 100 ' Variable height.
obstaclevariants(2).YPos(1) = 75
obstaclevariants(2).YPos(2) = 50
obstaclevariants(2).YPosCount = 3
obstaclevariants(2).MultipleSpeed = 999
obstaclevariants(2).MinimumSpeed = 8.5
obstaclevariants(2).MinimumGap = 150

InitializeCollisionBox obstaclevariants(2).CollisionBoxes(0), 15, 15, 16, 5
InitializeCollisionBox obstaclevariants(2).CollisionBoxes(1), 18, 21, 24, 6
InitializeCollisionBox obstaclevariants(2).CollisionBoxes(2), 2, 14, 4, 3
InitializeCollisionBox obstaclevariants(2).CollisionBoxes(3), 6, 10, 4, 7
InitializeCollisionBox obstaclevariants(2).CollisionBoxes(4), 10, 8, 6, 9

obstaclevariants(2).CollisionBoxCount = 5
obstaclevariants(2).NumFrames = 2
obstaclevariants(2).FrameDuration = 1# / 6#
obstaclevariants(2).SpeedOffset = .8

SCREEN 9, , 0, 1

OUT &H3C8, 0

FOR i% = 0 TO 15
  PALETTE i%, i%

  j% = i% * 63 \ 15

  OUT &H3C9, j%
  OUT &H3C9, j%
  OUT &H3C9, j%
NEXT i%

ProgramTimerRate NominalFPS%

' Two banks, because we have ~118 KB of image data (due to making the sprites
' all the same size), and QuickBASIC maxes out at 64KB per array.

DIM images1%(0 TO MaxImageWords% - 1, 0 TO ImageBankCount% - 1)
DIM imagemasks1%(0 TO MaxImageWords% - 1, 0 TO ImageBankCount% - 1)

DIM images2%(0 TO MaxImageWords% - 1, ImageBankCount% TO ImageBankCount% * 2 - 1)
DIM imagemasks2%(0 TO MaxImageWords% - 1, ImageBankCount% TO ImageBankCount% * 2 - 1)

DIM images3%(0 TO MaxImageWords% - 1, ImageBankCount% * 2 TO ImageLAST%)
DIM imagemasks3%(0 TO MaxImageWords% - 1, ImageBankCount% * 2 TO ImageLAST%)

DIM KeyStates%(127)

HighestScore& = 0

LoadImages

LINE (0, 0)-(639, 349), 14, BF

DIM runner AS RunnerType
DIM loopstate AS LoopStateType
DIM distancemeter AS DistanceMeterType
DIM trex AS TRexType
DIM horizonline AS HorizonLineType
DIM clouds(0 TO MaxClouds% - 1) AS CloudType
DIM nightmode AS NightModeType
DIM obstacles(0 TO MaxObstacles% - 1) AS ObstacleType
DIM firstactiveobstacle%, nextinactiveobstacle%
DIM obstaclehistory%(0 TO MaxObstacleDuplication% - 1)

runner.CurrentSpeed = DefaultSpeed#

loopstate.ShowInstructions = -1
loopstate.InitialFreeze = -1

InitializeGame runner, loopstate, horizonline, clouds(), nightmode, obstacles(), firstactiveobstacle%, nextinactiveobstacle%, distancemeter, trex

' Process PLAY strings in the background.
PLAY "MB"

DO
  framestarttime! = TIMER

  HandleKeys runner, loopstate, distancemeter, horizonline, nightmode, clouds(), trex, obstacles(), firstactiveobstacle%, nextinactiveobstacle%, KeyStates%()

  IF loopstate.GameOver = 0 THEN
    UpdateGame runner, loopstate, distancemeter, horizonline, nightmode, clouds(), trex, obstacles(), firstactiveobstacle%, nextinactiveobstacle%
  ELSE
    DrawGameOver runner, loopstate, distancemeter, horizonline, nightmode, clouds(), trex, obstacles(), firstactiveobstacle%, nextinactiveobstacle%
  END IF

  PCOPY 0, 1

  DO
    ks% = INP(&H60)
    KeyStates%(ks% AND 127) = ((ks% AND 128) = 0)
  LOOP WHILE TIMER = framestarttime!
LOOP WHILE KeyStates%(1) = 0 ' Exit on ESC.

SUB AddCloud (clouds() AS CloudType)
FOR i% = 0 TO MaxClouds% - 1
  IF clouds(i%).IsActive = 0 THEN
    InitializeCloud clouds(i%)
    EXIT SUB
  END IF
NEXT i%
END SUB

SUB AddNewObstacle (obstacles() AS ObstacleType, firstactiveobstacle%, nextinactiveobstacle%, speed#)
SHARED obstaclevariants() AS ObstacleVariantType
SHARED obstaclehistory%(), nextobstaclehistory%

newnext% = (nextinactiveobstacle% + 1) MOD MaxObstacles%

IF newnext% <> firstactiveobstacle% THEN
  newobstacle% = nextinactiveobstacle%
  nextinactiveobstacle% = newnext%

  DO
    newobstaclevariant% = GetRandomNum%(LBOUND(obstaclevariants), UBOUND(obstaclevariants))

    ' Check for multiples of the same type of obstacle.
    ' Also check obstacle is available at current speed.
  LOOP WHILE DuplicateObstacleCheck%(newobstaclevariant%) OR (speed# < obstaclevariants(newobstaclevariant%).MinimumSpeed)

  InitializeObstacle obstacles(newobstacle%), newobstaclevariant%, GapCoefficient#, speed#, (obstaclevariants(newobstaclevariant).ImageWidth)

  obstaclehistory%(nextobstaclehistory%) = newobstaclevariant%
  nextobstaclehistory% = nextobstaclehistory% + 1
  IF nextobstaclehistory% > UBOUND(obstaclehistory%) THEN nextobstaclehistory% = LBOUND(obstaclehistory%)
END IF
END SUB

FUNCTION BoxCompare% (trexbox AS CollisionBoxType, obstaclebox AS CollisionBoxType)
' Axis-Aligned Bounding Box method.
IF trexbox.BoxX < obstaclebox.BoxX + obstaclebox.BoxWidth THEN
  IF trexbox.BoxX + trexbox.BoxWidth > obstaclebox.BoxX THEN
    IF trexbox.BoxY < obstaclebox.BoxY + obstaclebox.BoxHeight THEN
      IF trexbox.BoxY + trexbox.BoxHeight > obstaclebox.BoxY THEN
        BoxCompare% = -1
      END IF
    END IF
  END IF
END IF
END FUNCTION

FUNCTION CheckForCollision% (obstacle AS ObstacleType, trex AS TRexType)
SHARED obstaclevariants() AS ObstacleVariantType

DIM trexbox AS CollisionBoxType
DIM obstaclebox AS CollisionBoxType

' Adjustments are made to the bounding box as there is a 1 pixel white
' border around the t-rex and obstacles.
trexbox.BoxX = trex.XPos + 1
trexbox.BoxY = trex.YPos + 1
trexbox.BoxWidth = trex.ImageWidth - 2
trexbox.BoxHeight = trex.ImageHeight - 2

obstaclebox.BoxX = obstacle.XPos + 1
obstaclebox.BoxY = obstacle.YPos + 1
obstaclebox.BoxWidth = obstaclevariants(obstacle.Variant).ImageWidth * obstacle.Size - 2
obstaclebox.BoxHeight = obstaclevariants(obstacle.Variant).ImageHeight - 2

' Simple outer bounds check.
IF BoxCompare%(trexbox, obstaclebox) THEN
  CheckForCollision% = CheckForCollisionB%(obstaclebox, obstaclevariants(obstacle.Variant).CollisionBoxes(), trex, trexbox)
END IF
END FUNCTION

FUNCTION CheckForCollisionB% (obstaclebox AS CollisionBoxType, obstacleboxes() AS CollisionBoxType, trex AS TRexType, trexbox AS CollisionBoxType)
SHARED TRexDUCKINGCollisionBoxes() AS CollisionBoxType
SHARED TRexRUNNINGCollisionBoxes() AS CollisionBoxType

IF trex.IsDucking THEN
  CheckForCollisionB% = CheckForCollisionC%(obstaclebox, obstacleboxes(), trexbox, TRexDUCKINGCollisionBoxes())
ELSE
  CheckForCollisionB% = CheckForCollisionC%(obstaclebox, obstacleboxes(), trexbox, TRexRUNNINGCollisionBoxes())
END IF
END FUNCTION

FUNCTION CheckForCollisionC% (obstaclebox AS CollisionBoxType, obstacleboxes() AS CollisionBoxType, trexbox AS CollisionBoxType, trexboxes() AS CollisionBoxType)
' Detailed axis aligned box check.
maxo% = UBOUND(obstacleboxes)
maxt% = UBOUND(trexboxes)

DIM adjtrexbox AS CollisionBoxType
DIM adjobstaclebox AS CollisionBoxType

FOR t% = 0 TO maxt%
  CreateAdjustedCollisionBox trexbox, trexboxes(t%), adjtrexbox

  FOR o% = 0 TO maxo%
    ' Adjust the box to actual positions.
    CreateAdjustedCollisionBox obstaclebox, obstacleboxes(o%), adjobstaclebox

    IF BoxCompare(adjtrexbox, adjobstaclebox) THEN
      CheckForCollisionC% = -1
      EXIT FUNCTION
    END IF
  NEXT o%
NEXT t%
END FUNCTION

SUB CreateAdjustedCollisionBox (box AS CollisionBoxType, adjustment AS CollisionBoxType, result AS CollisionBoxType)
result.BoxX = box.BoxX + adjustment.BoxX
result.BoxY = box.BoxY + adjustment.BoxY
result.BoxWidth = box.BoxWidth
result.BoxHeight = box.BoxHeight
END SUB

SUB DrawCloud (cloud AS CloudType)
PutImage cloud.XPos, cloud.YPos, ImageCLOUD%
END SUB

SUB DrawDigit (distancemeter AS DistanceMeterType, digitpos%, value%, opthighscore%)
targetx% = distancemeter.XPos + digitpos% * DigitSpacing%
targety% = distancemeter.YPos

IF opthighscore% THEN
  ' Left of the current score.
  targetx% = targetx% - distancemeter.MaxScoreUnits * 2 * DigitWidth%
END IF

PutImage targetx%, targety%, ImageTEXT% + value%
END SUB

SUB DrawGameOver (runner AS RunnerType, loopstate AS LoopStateType, distancemeter AS DistanceMeterType, horizonline AS HorizonLineType, nightmode AS NightModeType, clouds() AS CloudType, trex AS TRexType, obstacles() AS ObstacleType,  _
firstactiveobstacle%, nextinactiveobstacle%)
' We need to draw the entire Game Over screen. The original has persistence
' in the canvas, but we start from a blank frame every time.
zerotime# = 0

UpdateHorizon runner, loopstate, horizonline, nightmode, clouds(), obstacles(), firstactiveobstacle%, nextinactiveobstacle%, zerotime#, hasobstacles%, 0
ignored% = UpdateDistanceMeter(distancemeter, zerotime#, -INT(-loopstate.DistanceRan))
UpdateTRex trex, zerotime#, 0

' Overlay GAME OVER text and restart icon
textwidth% = 191
restartwidth% = 36

centrex% = GameWidth% \ 2

textx% = centrex% - textwidth% \ 2
texty% = (GameHeight% - 25) \ 3

restartx% = centrex% - restartwidth% \ 2
restarty% = GameHeight% \ 2

FOR i% = 0 TO 3
  PutImage textx%, texty%, ImageGAMEOVER% + i%
  textx% = textx% + ImageGAMEOVERPieceWidth%
NEXT i%

PutImage restartx%, restarty%, ImageRESTART%

' Mask off areas where sprites are drawn past the end.
LINE (0, 0)-(GameX% - 1, ScreenHeight%), 14, BF
LINE (ScreenWidth% - GameX%, 0)-(ScreenWidth%, ScreenHeight%), 14, BF
LINE (GameX%, GameY% + GameHeight%)-(GameX% + GameWidth% - 1, ScreenHeight%), 14, BF
END SUB

SUB DrawHorizonLine (horizonline AS HorizonLineType)
terrainx% = horizonline.XPos

spriteindex% = ImageHORIZON% + terrainx% \ ImageHORIZONPieceWidth%
offsetx% = -(terrainx% MOD ImageHORIZONPieceWidth%)

region% = 0
regiontype% = horizonline.RegionTypes(region%) * ImageHORIZONPieceCount%

WHILE offsetx% < GameWidth%
  PutImage offsetx%, ImageHORIZONPieceY%, spriteindex% + regiontype%

  offsetx% = offsetx% + ImageHORIZONPieceWidth%
  spriteindex% = spriteindex% + 1

  IF spriteindex% = ImageHORIZON% + ImageHORIZONPieceCount% THEN
    spriteindex% = ImageHORIZON%
    region% = region% + 1
    regiontype% = horizonline.RegionTypes(region%) * ImageHORIZONPieceCount%
  END IF
WEND
END SUB

SUB DrawNightMode (runner AS RunnerType, nightmode AS NightModeType)
IF nightmode.CurrentPhase = 3 THEN
  moonwidth% = NightModeWidth% * 2
ELSE
  moonwidth% = NightModeWidth%
END IF

' Stars.
IF nightmode.DrawStars THEN
  FOR i% = 0 TO NightModeNumStars% - 1
    PutImageOpacity (nightmode.Stars(i%).XPos), nightmode.Stars(i%).YPos, ImageSTAR% + i%, nightmode.Opacity, NOT runner.IsInverted
  NEXT i%
END IF

' Moon.
PutImageOpacity nightmode.XPos, nightmode.YPos, ImageMOON% + nightmode.CurrentPhase, nightmode.Opacity, NOT runner.IsInverted
END SUB

SUB DrawObstacle (obstacle AS ObstacleType)
SHARED obstaclevariants() AS ObstacleVariantType

actualx% = GameX% + obstacle.XPos
localx% = obstacle.XPos

imagew% = obstaclevariants(obstacle.Variant).ImageWidth

imagebaseidx% = obstaclevariants(obstacle.Variant).ImageIndex
imageidx% = obstacle.CurrentFrame
maximageidx% = obstaclevariants(obstacle.Variant).NumFrames

FOR i% = 1 TO obstacle.Size
  IF actualx% >= 0 THEN
    PutImage localx%, obstacle.YPos, imagebaseidx% + imageidx%
  END IF

  actualx% = actualx% + imagew%
  localx% = localx% + imagew%

  IF actualx% + imagew% > ScreenWidth% THEN EXIT FOR

  imageidx% = imageidx% + 1
  IF imageidx% = maximageidx% THEN imageidx% = 0
NEXT i%
END SUB

SUB DrawTRex (trex AS TRexType, imageidx%)
IF trex.IsDucking AND (trex.TRexStatus = TRexStatusCRASHED%) THEN
  ' Crashed whilst ducking. Trex is standing up so needs adjustment.
  trex.XPos = trex.XPos + 1
END IF

PutImage trex.XPos, trex.YPos, imageidx%
END SUB

' Returns whether the previous two obstacles are the same as the next one.
' Maximum duplication is set in constant config value MaxObstacleDuplication%.
FUNCTION DuplicateObstacleCheck% (nextobstaclevariant%)
SHARED obstaclehistory%()

FOR i% = LBOUND(obstaclehistory%) TO UBOUND(obstaclehistory%)
  IF obstaclehistory%(i%) <> nextobstaclevariant% THEN EXIT FUNCTION
NEXT i%

DuplicateObstacleCheck% = -1
END FUNCTION

' Game over state.
SUB GameOver (runner AS RunnerType, loopstate AS LoopStateType, distancemeter AS DistanceMeterType, trex AS TRexType)
PLAY HitSoundPlayString$

loopstate.IsPlaying = 0
loopstate.IsPaused = -1
loopstate.GameOver = -1

runner.IsCrashed = -1

distancemeter.ShowAchievement = 0

UpdateTRex trex, .1, TRexStatusCRASHED%

' Update the high score
IF loopstate.DistanceRan > runner.HighestScore THEN
  runner.HighestScore = -INT(-loopstate.DistanceRan)
  distancemeter.HighScore = runner.HighestScore
END IF

' Reset the time clock.
loopstate.LastFrameTime = TIMER
END SUB

' Convert pixel distance to a 'real' distance.
FUNCTION GetActualDistance& (distance#)
GetActualDistance& = INT(.5 * distance# * DistanceMeterCoefficient#)
END FUNCTION

' Get random number.
FUNCTION GetRandomNum% (minvalue%, maxvalue%)
GetRandomNum% = FIX(RND * (maxvalue% - minvalue% + 1)) + minvalue%
END FUNCTION

SUB HandleKeys (runner AS RunnerType, loopstate AS LoopStateType, distancemeter AS DistanceMeterType, horizonline AS HorizonLineType, nightmode AS NightModeType, clouds() AS CloudType, trex AS TRexType, obstacles() AS ObstacleType,  _
firstactiveobstacle%, nextinactiveobstacle%, KeyStates%())
IF loopstate.IsJumpPressed THEN
  IF NOT KeyStates%(KeyJUMP%) THEN
    loopstate.IsJumpPressed = 0

    TRexEndJump trex

    IF runner.IsCrashed THEN
      ' Check that enough time has elapsed before allowing jump key to restart.
      deltatime# = TIMER - loopstate.LastFrameTime
      IF deltatime# < 0 THEN deltatime# = deltatime# + 86400#

      deltatime# = deltatime# * (TimerFPS# / NominalFPS%)

      IF deltatime# >= GameOverClearTime# THEN
        ' Restart the game using the jump key but without actually jumping
        RestartGame runner, loopstate, distancemeter, horizonline, nightmode, clouds(), obstacles(), firstactiveobstacle%, nextinactiveobstacle%, trex
      END IF
    ELSEIF loopstate.IsPaused THEN
      ' Reset the jump state
      ResetTRex trex
      PlayGame runner, loopstate, distancemeter, horizonline, nightmode, clouds(), trex, obstacles(), firstactiveobstacle%, nextinactiveobstacle%
    END IF
  END IF
ELSE
  IF KeyStates%(KeyJUMP%) THEN
    loopstate.IsJumpPressed = -1

    IF NOT runner.IsCrashed THEN
      IF NOT loopstate.IsPlaying THEN
        ' Jump to start the game.
        loopstate.IsPlaying = -1
        loopstate.LastFrameTime = TIMER
        UpdateGame runner, loopstate, distancemeter, horizonline, nightmode, clouds(), trex, obstacles(), firstactiveobstacle%, nextinactiveobstacle%
      END IF

      IF NOT trex.IsJumping AND NOT trex.IsDucking THEN
        PLAY ButtonPressSoundPlayString$
        TRexStartJump trex, runner.CurrentSpeed
      END IF
    END IF
  END IF
END IF

IF runner.IsCrashed AND KeyStates%(KeySTART%) THEN
  RestartGame runner, loopstate, distancemeter, horizonline, nightmode, clouds(), obstacles(), firstactiveobstacle%, nextinactiveobstacle%, trex
END IF

IF loopstate.IsDuckPressed THEN
  IF NOT (KeyStates%(KeyDUCK1%) OR KeyStates%(KeyDUCK2%)) THEN
    loopstate.IsDuckPressed = 0

    trex.SpeedDrop = 0

    SetTRexIsDucking trex, 0
  END IF
ELSEIF loopstate.IsPlaying AND NOT runner.IsCrashed AND (KeyStates%(KeyDUCK1%) OR KeyStates%(KeyDUCK2%)) THEN
  loopstate.IsDuckPressed = -1

  IF trex.IsJumping THEN
    ' Speed drop, activated only when jump key is not pressed.
    SetTRexSpeedDrop trex
  ELSEIF NOT trex.IsDucking THEN
    ' Duck.
    SetTRexIsDucking trex, -1
  END IF
END IF
END SUB

SUB InitializeCloud (cloud AS CloudType)
cloud.XPos = GameWidth%
cloud.YPos = 0
cloud.IsActive = -1
cloud.CloudGap = GetRandomNum%(CloudMinGap%, CloudMaxGap%)

' Initialise the cloud. Sets the Cloud height.
cloud.YPos = GetRandomNum%(CloudMaxSkyLevel%, CloudMinSkyLevel%)

DrawCloud cloud
END SUB

SUB InitializeCollisionBox (collisionbox AS CollisionBoxType, XPos%, YPos%, widthval%, heightval%)
collisionbox.BoxX = XPos%
collisionbox.BoxY = YPos%
collisionbox.BoxWidth = widthval%
collisionbox.BoxHeight = heightval%
END SUB

SUB InitializeDistanceMeter (distancemeter AS DistanceMeterType)
distancemeter.XPos = 0
distancemeter.YPos = 5

distancemeter.CurrentDistance = 0
distancemeter.MaxScore = 0
distancemeter.HighScore = 0

distancemeter.ShowAchievement = 0
distancemeter.FlashTimer = 0
distancemeter.FlashIterations = 0
distancemeter.InvertTrigger = 0

distancemeter.MaxScoreUnits = DistanceMeterMaxDistanceUnits%

distancemeter.XPos = GameWidth% - DigitSpacing% * (distancemeter.MaxScoreUnits + 1)

distancemeter.MaxScore = 10 ^ DistanceMeterMaxDistanceUnits% - 1
END SUB

' Game initialiser.
SUB InitializeGame (runner AS RunnerType, loopstate AS LoopStateType, horizonline AS HorizonLineType, clouds() AS CloudType, nightmode AS NightModeType, obstacles() AS ObstacleType, firstactiveobstacle%, nextinactiveobstacle%, distancemeter AS  _
DistanceMeterType, trex AS TRexType)
loopstate.LastFrameTime = TIMER

' Horizon contains clouds, obstacles and the ground.
AddCloud clouds()
InitializeHorizonLine horizonline
InitializeNightMode nightmode

' Distance meter
InitializeDistanceMeter distancemeter

' Draw t-rex
InitializeTRex trex

UpdateGame runner, loopstate, distancemeter, horizonline, nightmode, clouds(), trex, obstacles(), firstactiveobstacle%, nextinactiveobstacle%
END SUB

SUB InitializeHorizon (horizonline AS HorizonLineType, nightmode AS NightModeType)
InitializeHorizonLine horizonline
InitializeNightMode nightmode
END SUB

' Horizon Line.
' Consists of two connecting lines. Randomly assigns a flat / bumpy horizon.
SUB InitializeHorizonLine (horizonline AS HorizonLineType)
horizonline.XPos = RND * HorizonWidth%
horizonline.BumpThreshold = .5

DrawHorizonLine horizonline
END SUB

SUB InitializeNightMode (nightmode AS NightModeType)
nightmode.XPos = GameWidth% - 50
nightmode.YPos = 30
nightmode.CurrentPhase = 0
nightmode.Opacity = 0
nightmode.DrawStars = 0

PlaceNightModeStars nightmode
END SUB

SUB InitializeObstacle (obstacle AS ObstacleType, var%, gapcoeff#, speed#, xoffset#)
SHARED obstaclevariants() AS ObstacleVariantType

obstacle.Variant = var%
obstacle.GapCoefficient = gapcoeff#
obstacle.Size = GetRandomNum%(1, MaxObstacleLength%)
obstacle.IsActive = -1
obstacle.XPos = GameWidth% + xoffset#
obstacle.Gap = 0
obstacle.SpeedOffset = 0

' For animated obstacles
obstacle.CurrentFrame = 0
obstacle.TimeValue = 0

obstacle.CollisionBoxCount = obstaclevariants(var%).CollisionBoxCount
FOR i% = 0 TO obstacle.CollisionBoxCount - 1
  obstacle.CollisionBoxes(i%) = obstaclevariants(var%).CollisionBoxes(i%)
NEXT i%

' Only allow sizing if we're at the right speed.
IF obstacle.Size > 1 AND obstaclevariants(var%).MultipleSpeed > speed# THEN
  obstacle.Size = 1
END IF

obstacle.ObstacleWidth = obstaclevariants(var%).ImageWidth * obstacle.Size

' Check if obstacle can be positioned at various heights.
obstacle.YPos = obstaclevariants(var%).YPos(GetRandomNum%(0, obstaclevariants(var%).YPosCount - 1))

DrawObstacle obstacle

' Make collision box adjustments,
' Central box is adjusted to the size as one box.
'      ____        ______        ________
'    _|   |-|    _|     |-|    _|       |-|
'   | |<->| |   | |<--->| |   | |<----->| |
'   | | 1 | |   | |  2  | |   | |   3   | |
'   |_|___|_|   |_|_____|_|   |_|_______|_|
'
IF obstacle.Size > 1 THEN
  obstacle.CollisionBoxes(1).BoxWidth = obstacle.ObstacleWidth - obstacle.CollisionBoxes(0).BoxWidth - obstacle.CollisionBoxes(2).BoxWidth
  obstacle.CollisionBoxes(2).BoxX = obstacle.ObstacleWidth - obstacle.CollisionBoxes(2).BoxWidth
END IF

' For obstacles that go at a different speed from the horizon.
IF obstaclevariants(var%).SpeedOffset <> 0 THEN
  IF RND > .5 THEN
    obstacle.SpeedOffset = obstaclevariants(var%).SpeedOffset
  ELSE
    obstacle.SpeedOffset = -obstaclevariants(var%).SpeedOffset
  END IF
END IF

mingap% = INT(.5 + obstacle.ObstacleWidth * speed# + obstaclevariants(var%).MinimumGap * obstacle.GapCoefficient)
maxgap% = INT(.5 + mingap% * ObstacleMaxGapCoefficient#)

obstacle.Gap = GetRandomNum%(mingap%, maxgap%)

obstacle.FollowingObstacleCreated = 0
END SUB

' T-rex game character.
SUB InitializeTRex (trex AS TRexType)
SHARED trexanimframes() AS TRexAnimationType

trex.XPos = 0
trex.CurrentFrame = 0
trex.BlinkDelay = 0
trex.BlinkCount = 0
trex.AnimStartTime = 0
trex.TimeValue = 0
' Current status.
trex.TRexStatus = TRexStatusWAITING%

trex.IsJumping = 0
trex.IsDucking = 0
trex.JumpVelocity = 0
trex.ReachedMinHeight = 0
trex.SpeedDrop = 0

' Position when on the ground.
trex.GroundYPos = GameHeight% - ImageTREXHeight% - GameBottomPad%
trex.YPos = trex.GroundYPos

trex.MinJumpHeight = trex.GroundYPos - MinJumpHeight%

DrawTRex trex, trexanimframes(TRexStatusWAITING%).ImageIndices(0)

UpdateTRex trex, 0, TRexStatusWAITING%
END SUB

SUB LoadImages
SHARED images1%(), imagemasks1%()
SHARED images2%(), imagemasks2%()
SHARED images3%(), imagemasks3%()

ff% = FREEFILE

OPEN "TREX.ART" FOR BINARY AS #ff%

GET #ff%, , artw%
GET #ff%, , arth%

pixelsstart& = LOC(ff%)
maskstart& = pixelsstart& + artw% * 1& * arth%

FOR spriteindex% = 0 TO ImageLAST%
  SELECT CASE spriteindex%
    CASE ImageCACTUSLARGE%: x% = 332: y% = 2: w% = 25: h% = 50
    CASE ImageCACTUSSMALL%: x% = 228: y% = 2: w% = 17: h% = 35
    CASE ImageCLOUD%: x% = 86: y% = 2: w% = 46: h% = 14
    CASE ImageHORIZON%: x% = 2: y% = 54: w% = ImageHORIZONPieceWidth%: h% = ImageHORIZONPieceHeight%
    CASE ImageMOON%: x% = 484: y% = 2: w% = 20: h% = 40
    CASE ImagePTERODACTYL%: x% = 134: y% = 2: w% = 46: h% = 40
    CASE ImageRESTART%: x% = 2: y% = 2: w% = 36: h% = 32
    CASE ImageTEXT%: x% = 655: y% = 2: w% = DigitWidth%: h% = DigitHeight%
    CASE ImageGAMEOVER%: x% = 655: y% = 15: w% = 48: h% = 11
    CASE ImageTREX%: x% = 848: y% = 2: w% = ImageTREXWidth%: h% = ImageTREXHeight%
    CASE ImageDUCKING%: x% = 1112: y% = 2: w% = ImageDUCKINGWidth%: h% = ImageDUCKINGHeight%
    CASE ImageSTAR% + 0: x% = 644: y% = 2: w% = 9: h% = 9
    CASE ImageSTAR% + 1: x% = 644: y% = 11: w% = 9: h% = 9
    CASE ImageSTAR% + 2: x% = 644: y% = 20: w% = 9: h% = 9
    CASE ImageINTROTREX%: x% = 40: y% = 4: w% = 44: h% = 45
  END SELECT

  ' Generate two images:
  ' - A mask, which is ANDed against the target area. It turns all pixels that will be painted (that are opaque) black.
  ' - The image itself, which is ORed against the target area. All transparent pixels need to be 0s so that the OR leaves them alone.

  FOR yy% = 0 TO h% - 1
    sy& = y% + yy%

    scanofs& = sy& * artw% + x%

    SEEK #ff%, pixelsstart& + scanofs&
    pixels$ = INPUT$(w%, ff%)
    SEEK #ff%, maskstart& + scanofs&
    mask$ = INPUT$(w%, ff%)

    FOR xx% = 0 TO w% - 1
      IF ASC(MID$(mask$, xx% + 1, 1)) THEN PSET (xx%, yy%), ASC(MID$(pixels$, xx% + 1, 1)) ELSE PRESET (xx%, yy%)
      IF ASC(MID$(mask$, xx% + 1, 1)) THEN PRESET (100 + xx%, yy%) ELSE PSET (100 + xx%, yy%), 15
    NEXT xx%
  NEXT yy%

  IF spriteindex% < ImageBankCount% THEN
    GET (0, 0)-STEP(w% - 1, h% - 1), images1%(0, spriteindex%)
    GET (100, 0)-STEP(w% - 1, h% - 1), imagemasks1%(0, spriteindex%)
  ELSEIF spriteindex% < 2 * ImageBankCount% THEN
    GET (0, 0)-STEP(w% - 1, h% - 1), images2%(0, spriteindex%)
    GET (100, 0)-STEP(w% - 1, h% - 1), imagemasks2%(0, spriteindex%)
  ELSE
    GET (0, 0)-STEP(w% - 1, h% - 1), images3%(0, spriteindex%)
    GET (100, 0)-STEP(w% - 1, h% - 1), imagemasks3%(0, spriteindex%)
  END IF

  x% = x% + w%
NEXT spriteindex%

CLOSE #1
END SUB

SUB PlaceNightModeStars (nightmode AS NightModeType)
segmentsize% = GameWidth% \ NightModeNumStars%

FOR i% = 0 TO NightModeNumStars% - 1
  nightmode.Stars(i%).XPos = GetRandomNum%(segmentsize% * i%, segmentsize% * (i% + 1))
  nightmode.Stars(i%).YPos = GetRandomNum%(0, NightModeStarMaxY%)
NEXT i%
END SUB

SUB PlayGame (runner AS RunnerType, loopstate AS LoopStateType, distancemeter AS DistanceMeterType, horizonline AS HorizonLineType, nightmode AS NightModeType, clouds() AS CloudType, trex AS TRexType, obstacles() AS ObstacleType,  _
firstactiveobstacle%, nextinactiveobstacle%)
IF NOT runner.IsCrashed THEN
  loopstate.IsPlaying = -1
  loopstate.IsPaused = 0

  loopstate.LastFrameTime = TIMER

  UpdateTRex trex, 0, TRexStatusRUNNING%

  UpdateGame runner, loopstate, distancemeter, horizonline, nightmode, clouds(), trex, obstacles(), firstactiveobstacle%, nextinactiveobstacle%
END IF
END SUB

SUB ProgramTimerRate (tickspersecond#)
CONST BaseFrequency& = 1193181

interval& = BaseFrequency& / tickspersecond#

OUT &H43, &H36
OUT &H40, interval&
OUT &H40, interval& \ 256
END SUB

SUB PutImage (imagex%, imagey%, imageidx%)
SHARED images1%(), imagemasks1%()
SHARED images2%(), imagemasks2%()
SHARED images3%(), imagemasks3%()

IF imageidx% < ImageBankCount% THEN
  imagew% = images1%(0, imageidx%)
ELSEIF imageidx% < 2 * ImageBankCount% THEN
  imagew% = images2%(0, imageidx%)
ELSE
  imagew% = images3%(0, imageidx%)
END IF

IF GameX% + imagex% < 0 THEN EXIT SUB
IF GameX% + imagex% + imagew% > ScreenWidth% THEN EXIT SUB

IF imageidx% < ImageBankCount% THEN
  PUT (GameX% + imagex%, GameY% + imagey%), imagemasks1%(0, imageidx%), AND
  PUT (GameX% + imagex%, GameY% + imagey%), images1%(0, imageidx%), OR
ELSEIF imageidx% < 2 * ImageBankCount% THEN
  PUT (GameX% + imagex%, GameY% + imagey%), imagemasks2%(0, imageidx%), AND
  PUT (GameX% + imagex%, GameY% + imagey%), images2%(0, imageidx%), OR
ELSE
  PUT (GameX% + imagex%, GameY% + imagey%), imagemasks3%(0, imageidx%), AND
  PUT (GameX% + imagex%, GameY% + imagey%), images3%(0, imageidx%), OR
END IF
END SUB

SUB PutImageOpacity (imagex%, imagey%, imageidx%, imageo#, invertimage%)
SHARED images1%(), imagemasks1%()
SHARED images2%(), imagemasks2%()
SHARED images3%(), imagemasks3%()

IF imageidx% < ImageBankCount% THEN
  w% = images1%(0, imageidx%)
  h% = images1%(1, imageidx%)
  IF invertimage% THEN
    LINE (0, 0)-(w% - 1, h% - 1), 15, BF
    PUT (0, 0), images1%(0, imageidx%), XOR
  ELSE
    PUT (0, 0), images1%(0, imageidx%), PSET
  END IF
  PUT (w%, 0), imagemasks1%(0, imageidx%), PSET
ELSEIF imageidx% < 2 * ImageBankCount% THEN
  w% = images2%(0, imageidx%)
  h% = images2%(1, imageidx%)
  IF invertimage% THEN
    LINE (0, 0)-(w% - 1, h% - 1), 15, BF
    PUT (0, 0), images2%(0, imageidx%), XOR
  ELSE
    PUT (0, 0), images2%(0, imageidx%), PSET
  END IF
  PUT (w%, 0), imagemasks2%(0, imageidx%), PSET
ELSE
  w% = images3%(0, imageidx%)
  h% = images3%(1, imageidx%)
  IF invertimage% THEN
    LINE (0, 0)-(w% - 1, h% - 1), 15, BF
    PUT (0, 0), images3%(0, imageidx%), XOR
  ELSE
    PUT (0, 0), images3%(0, imageidx%), PSET
  END IF
  PUT (w%, 0), imagemasks3%(0, imageidx%), PSET
END IF

opacityscale% = imageo# * 16

tx% = GameX% + imagex%
ty% = GameY% + imagey%

FOR y% = 0 TO h% - 1
  FOR x% = 0 TO w% - 1
    mask% = POINT(x% + w%, y%)

    IF mask% = 0 THEN
      src% = POINT(x%, y%)
      dst% = POINT(tx% + x%, ty% + y%)

      blend% = (src% * opacityscale% + dst% * (16 - opacityscale%)) \ 16

      PSET (tx% + x%, ty% + y%), blend%
    END IF
  NEXT x%
NEXT y%
END SUB

' Reset the distance meter back to '00000'.
SUB ResetDistanceMeter (distancemeter AS DistanceMeterType)
a% = UpdateDistanceMeter(distancemeter, 0, 0)

distancemeter.ShowAchievement = 0
END SUB

' Reset the horizon layer.
' Remove existing obstacles and reposition the horizon line.
SUB ResetHorizon (runner AS RunnerType, horizonline AS HorizonLineType, nightmode AS NightModeType, firstactiveobstacle%, nextinactiveobstacle%)
firstactiveobstacle% = nextinactiveobstacle%
ResetHorizonLine horizonline
ResetNightMode runner, nightmode
END SUB

' Reset horizon to the starting position.
SUB ResetHorizonLine (horizonline AS HorizonLineType)
horizonline.XPos = 0

horizonline.RegionTypes(0) = 0
horizonline.RegionTypes(1) = 0
horizonline.RegionTypes(2) = 0
END SUB

SUB ResetNightMode (runner AS RunnerType, nightmode AS NightModeType)
nightmode.CurrentPhase = 0
nightmode.Opacity = 0
UpdateNightMode runner, nightmode, 0, -1
END SUB

' Reset the t-rex to running.
SUB ResetTRex (trex AS TRexType)
trex.YPos = trex.GroundYPos
trex.JumpVelocity = 0
trex.IsJumping = 0
trex.IsDucking = 0

UpdateTRex trex, 0, TRexStatusRUNNING%

trex.SpeedDrop = 0
END SUB

SUB RestartGame (runner AS RunnerType, loopstate AS LoopStateType, distancemeter AS DistanceMeterType, horizonline AS HorizonLineType, nightmode AS NightModeType, clouds() AS CloudType, obstacles() AS ObstacleType, firstactiveobstacle%,  _
nextinactiveobstacle%, trex AS TRexType)
IF loopstate.GameOver THEN
  runner.PlayCount = runner.PlayCount + 1

  runner.RunningTime = 0
  runner.IsCrashed = 0
  runner.CurrentSpeed = DefaultSpeed#

  loopstate.IsPlaying = -1
  loopstate.IsPaused = 0
  loopstate.GameOver = 0
  loopstate.DistanceRan = 0
  loopstate.LastFrameTime = TIMER

  ResetDistanceMeter distancemeter
  ResetHorizon runner, horizonline, nightmode, firstactiveobstacle%, nextinactiveobstacle%
  ResetTRex trex

  PLAY ButtonPressSoundPlayString$

  UpdateInvert runner, -1 ' reset

  UpdateGame runner, loopstate, distancemeter, horizonline, nightmode, clouds(), trex, obstacles(), firstactiveobstacle%, nextinactiveobstacle%
END IF
END SUB

' Sets a random time for the blink to happen.
SUB SetTRexBlinkDelay (trex AS TRexType)
trex.BlinkDelay = -INT(-(RND + .5) * TRexBlinkTiming#)
END SUB

SUB SetTRexIsDucking (trex AS TRexType, newvalue%)
IF newvalue% AND (trex.TRexStatus <> TRexStatusDUCKING%) THEN
  UpdateTRex trex, 0, TRexStatusDUCKING%
  trex.IsDucking = -1
ELSEIF trex.TRexStatus = TRexStatusDUCKING% THEN
  UpdateTRex trex, 0, TRexStatusRUNNING%
  trex.IsDucking = 0
END IF
END SUB

' Set the speed drop. Immediately cancels the current jump.
SUB SetTRexSpeedDrop (trex AS TRexType)
trex.SpeedDrop = -1
trex.JumpVelocity = 1
END SUB

' Make t-rex blink at random intervals.
SUB TRexBlink (trex AS TRexType, blinktime#)
SHARED trexanimframes() AS TRexAnimationType
STATIC lastblinkdelay#

deltatime# = blinktime# - trex.AnimStartTime
IF deltatime# < 0 THEN deltatime# = deltatime# + 86400#

IF deltatime# > trex.BlinkDelay THEN
  trex.CurrentFrame = (trex.CurrentFrame + 1) MOD trexanimframes(trex.TRexStatus).NumFrames

  IF trex.CurrentFrame = 1 THEN
    SetTRexBlinkDelay trex
    lastblinkdelay# = trex.BlinkDelay
    trex.AnimStartTime = TIMER
    trex.BlinkCount = trex.BlinkCount + 1
  END IF
END IF

DrawTRex trex, trexanimframes(trex.TRexStatus).ImageIndices(trex.CurrentFrame)
END SUB

' Jump is complete, falling down.
SUB TRexEndJump (trex AS TRexType)
IF trex.ReachedMinHeight AND (trex.JumpVelocity < TRexDropVelocity#) THEN
  trex.JumpVelocity = TRexDropVelocity#
END IF
END SUB

' Initialise a jump.
SUB TRexStartJump (trex AS TRexType, speed#)
IF NOT trex.IsJumping THEN
  UpdateTRex trex, 0, TRexStatusJUMPING%
  ' Tweak the jump velocity based on the speed.
  trex.JumpVelocity = TRexInitialJumpVelocity# - speed# / 10#
  trex.JumpTimeValue = 0
  trex.IsJumping = -1
  trex.ReachedMinHeight = 0
  trex.SpeedDrop = 0
END IF
END SUB

' Update frame for a jump.
SUB TRexUpdateJump (trex AS TRexType, deltatime#, loopstate AS LoopStateType)
' The original does a jump update every 1000/FPS ms. This means that if
' you double the FPS, you jump twice as fast. This has been corrected to
' be based on a fixed 60 FPS regardless of the actual game FPS.

CONST FrameDuration60Hz# = 1# / 60#

trex.JumpTimeValue = trex.JumpTimeValue + deltatime#

DO WHILE trex.JumpTimeValue > FrameDuration60Hz#
  trex.JumpTimeValue = trex.JumpTimeValue - FrameDuration60Hz#

  dy# = trex.JumpVelocity

  ' Speed drop makes Trex fall faster.
  IF trex.SpeedDrop THEN dy# = dy# * TRexSpeedDropCoefficient#

  trex.YPos = trex.YPos + dy#
  trex.JumpVelocity = trex.JumpVelocity + TRexGravity#

  ' Minimum height has been reached.
  IF trex.YPos < trex.MinJumpHeight OR trex.SpeedDrop THEN
    trex.ReachedMinHeight = -1
  END IF

  ' Reached max height
  IF trex.YPos < TRexMaxJumpHeight# OR trex.SpeedDrop THEN
    TRexEndJump trex
  END IF

  ' Back down at ground level. Jump completed.
  IF trex.YPos > trex.GroundYPos THEN
    ResetTRex trex

    IF loopstate.InitialFreeze THEN
      loopstate.InitialFreeze = 0
      trex.PlayingIntro = -1
      trex.IntroTimeValue = 0
    END IF

    EXIT DO
  END IF
LOOP

UpdateTRex trex, deltatime#, 0
END SUB

' Update the cloud position.
SUB UpdateCloud (cloud AS CloudType, speed#)
IF cloud.IsActive THEN
  speedceil# = -INT(-speed#)

  cloud.XPos = cloud.XPos - speedceil#

  DrawCloud cloud

  ' Mark as removeable if no longer in the canvas.
  IF cloud.XPos + CloudWidth% <= 0 THEN cloud.IsActive = 0
END IF
END SUB

' Update the cloud positions.
SUB UpdateClouds (clouds() AS CloudType, deltatime#, speed#)
cloudspeed# = BGCloudSpeed# / 1000 * deltatime# * speed#

CONST NoClouds% = -1000

couldaddcloud% = 0
lastcloudx% = NoClouds%
lastcloudgap% = 0

FOR i% = 0 TO MaxClouds% - 1
  UpdateCloud clouds(i%), cloudspeed#

  IF clouds(i%).IsActive THEN
    IF clouds(i%).XPos > lastcloudx% THEN lastcloudx% = clouds(i%).XPos: lastcloudgap% = clouds(i%).CloudGap
  ELSE
    couldaddcloud% = -1
  END IF
NEXT i%

' Check for adding a new cloud.
IF lastcloudx% = NoClouds% THEN
  AddCloud clouds()
ELSEIF couldaddcloud% AND (GameWidth% - lastcloudx%) > lastcloudgap% AND CloudFrequency# > RND THEN
  AddCloud clouds()
END IF
END SUB

' Update the distance meter.
FUNCTION UpdateDistanceMeter% (distancemeter AS DistanceMeterType, deltatime#, distance#)
shouldpaint% = -1
shouldplaysound% = 0

IF NOT distancemeter.ShowAchievement THEN
  distancemeter.CurrentScore = GetActualDistance&(distance#)

  ' Score has gone beyond the initial digit count.
  IF distancemeter.CurrentScore > distancemeter.MaxScore AND distancemeter.MaxScoreUnits = DistanceMeterMaxDistanceUnits% THEN
    distancemeter.MaxScoreUnits = distancemeter.MaxScoreUnits + 1
    distancemeter.MaxScore = distancemeter.MaxScore * 10 + 9
  END IF

  IF distancemeter.CurrentScore > 0 THEN
    ' Achievement unlocked
    IF distancemeter.CurrentScore MOD DistanceMeterAchievementDistance% = 0 THEN
      distancemeter.ShowAchievement = -1
      distancemeter.FlashTimer = 0
      shouldplaysound% = -1
    END IF
  END IF
ELSE
  ' Control flashing of the score on reaching achievement.
  IF distancemeter.FlashIterations <= DistanceMeterFlashIterations% THEN
    distancemeter.FlashTimer = distancemeter.FlashTimer + deltatime#

    IF distancemeter.FlashTimer < DistanceMeterFlashDuration# THEN
      shouldpaint% = 0
    ELSEIF distancemeter.FlashTimer > DistanceMeterFlashDuration# * 2 THEN
      distancemeter.FlashTimer = distancemeter.FlashTimer - DistanceMeterFlashDuration# * 2
      distancemeter.FlashIterations = distancemeter.FlashIterations + 1
    END IF
  ELSE
    distancemeter.ShowAchievement = 0
    distancemeter.FlashIterations = 0
    distancemeter.FlashTimer = 0
  END IF
END IF

' Draw the digits if not flashing.
IF shouldpaint% THEN
  score& = distancemeter.CurrentScore

  FOR i% = distancemeter.MaxScoreUnits TO 1 STEP -1
    DrawDigit distancemeter, i%, score& MOD 10, 0 ' 0 = regular score
    score& = score& \ 10
  NEXT i%
END IF

' Draw the high score.
score& = distancemeter.HighScore

FOR i% = distancemeter.MaxScoreUnits TO 1 STEP -1
  DrawDigit distancemeter, i% + 3, score& MOD 10, 1 ' 1 = high score
  score& = score& \ 10
NEXT i%

DrawDigit distancemeter, 0, 10, 1 ' 'H'
DrawDigit distancemeter, 1, 11, 1 ' 'I'

UpdateDistanceMeter% = shouldplaysound%
END FUNCTION

SUB UpdateGame (runner AS RunnerType, loopstate AS LoopStateType, distancemeter AS DistanceMeterType, horizonline AS HorizonLineType, nightmode AS NightModeType, clouds() AS CloudType, trex AS TRexType, obstacles() AS ObstacleType,  _
firstactiveobstacle%, nextinactiveobstacle%)
thisframetime! = TIMER

IF NOT runner.IsCrashed THEN
  deltatime# = thisframetime! - loopstate.LastFrameTime
  IF deltatime# < 0 THEN deltatime# = deltatime# + 86400#

  deltatime# = deltatime# * (TimerFPS# / NominalFPS%)
END IF

loopstate.LastFrameTime = thisframetime!

IF runner.IsInverted THEN
  bgcolor% = 0
ELSE
  bgcolor% = 14
END IF

LINE (GameX%, GameY%)-STEP(GameWidth%, GameHeight%), bgcolor%, BF

IF trex.IsJumping THEN
  TRexUpdateJump trex, deltatime#, loopstate
END IF

runner.RunningTime = runner.RunningTime + deltatime#

hasobstacles% = runner.RunningTime > ClearTime#

' First jump triggers the intro.
IF trex.IsJumping THEN
  runner.IsActivated = -1

  loopstate.ShowInstructions = 0
  loopstate.IsPlaying = -1
END IF

' The horizon doesn't move until the intro is over.
IF loopstate.InitialFreeze THEN deltatime# = 0

UpdateHorizon runner, loopstate, horizonline, nightmode, clouds(), obstacles(), firstactiveobstacle%, nextinactiveobstacle%, deltatime#, hasobstacles%, runner.IsInverted

' Check for collisions
collision% = 0

IF hasobstacles% THEN
  collision% = CheckForCollision%(obstacles(firstactiveobstacle%), trex)
END IF

IF collision% = 0 THEN
  ' The original JavaScript code divides deltaTime by msPerFrame, which
  ' roughly counts 60 distance units per second -- one per frame. However,
  ' this calculation is buggy, because if FPS is ever changed, then the
  ' wall clock rate of distance accumulated changes too. Our deltatime# is
  ' seconds in wall clock time, so we just hardcode the 60 so that the
  ' framerate is irrelevant; it will always count 60 distance per second.
  ' (All scaled by CurrentSpeed.)

  loopstate.DistanceRan = loopstate.DistanceRan + runner.CurrentSpeed * deltatime# * 60

  IF runner.CurrentSpeed < MaxSpeed# THEN
    runner.CurrentSpeed = runner.CurrentSpeed + Acceleration#
  END IF
ELSE
  GameOver runner, loopstate, distancemeter, trex
END IF

playachievementsound% = UpdateDistanceMeter(distancemeter, deltatime#, -INT(-loopstate.DistanceRan))

IF playachievementsound% THEN PLAY AchievementSoundPlayString$

' Night mode.
IF runner.InvertTimer > InvertFadeDuration# THEN
  runner.InvertTimer = 0
  loopstate.InvertTrigger = 0
  UpdateInvert runner, 0
ELSEIF runner.InvertTimer <> 0 THEN
  runner.InvertTimer = runner.InvertTimer + deltatime#
ELSE
  actualdistance& = -INT(-loopstate.DistanceRan * DistanceMeterCoefficient#)

  IF actualdistance& > 0 THEN
    loopstate.InvertTrigger = (actualdistance& MOD InvertDistance%) = 0

    IF loopstate.InvertTrigger AND (runner.InvertTimer = 0) THEN
      runner.InvertTimer = runner.InvertTimer + deltatime#
      UpdateInvert runner, 0
    END IF
  END IF
END IF

IF loopstate.IsPlaying OR NOT runner.IsActivated THEN
  UpdateTRex trex, deltatime#, 0
END IF

' Mask off areas where sprites are drawn past the end.
LINE (0, 0)-(GameX% - 1, ScreenHeight%), bgcolor%, BF
LINE (ScreenWidth% - GameX%, 0)-(ScreenWidth%, ScreenHeight%), bgcolor%, BF
LINE (GameX%, 0)-(GameX% + GameWidth% - 1, GameY% - 1), bgcolor%, BF
LINE (GameX%, GameY% + GameHeight%)-(GameX% + GameWidth% - 1, ScreenHeight%), bgcolor%, BF

IF loopstate.ShowInstructions THEN
  messagerow% = 1 + (GameY% + GameHeight% + 13) \ 14
  messagecol% = 3
  message$ = "Press space to play"

  LOCATE messagerow%, messagecol%
  PRINT message$

  ' We only do this while paused at the initial freeze, so if this
  ' tanks the framerate, nobody will notice.
  x1% = (messagecol% - 1) * 8
  y1% = (messagerow% - 1) * 14

  x2% = x1% + LEN(message$) * 8 - 1
  y2% = y1% + 13

  FOR yy% = y1% TO y2%
    FOR xx% = x1% TO x2%
      PSET (xx%, yy%), POINT(xx%, yy%) XOR 14
    NEXT xx%
  NEXT yy%
END IF
END SUB

SUB UpdateHorizon (runner AS RunnerType, loopstate AS LoopStateType, horizonline AS HorizonLineType, nightmode AS NightModeType, clouds() AS CloudType, obstacles() AS ObstacleType, firstactiveobstacle%, nextinactiveobstacle%, deltatime#,  _
hasobstacles%, ShowNightMode%)
UpdateHorizonLine runner, horizonline, deltatime#
UpdateNightMode runner, nightmode, ShowNightMode%, loopstate.IsPaused OR loopstate.InitialFreeze
UpdateClouds clouds(), deltatime#, runner.CurrentSpeed
UpdateObstacles obstacles(), firstactiveobstacle%, nextinactiveobstacle%, deltatime#, runner.CurrentSpeed
END SUB

SUB UpdateHorizonLine (runner AS RunnerType, horizonline AS HorizonLineType, deltatime#)
' The original multiplies by (FPS / 1000), which means that if you double
' the FPS, it actually starts moving _more_ per frame, not less. Oops.
' Really, we just want the same amount of movement per unit time.
' So, we hardcode the ratio that the original uses with its FPS setting.

increment# = runner.CurrentSpeed * 60 * deltatime#

UpdateHorizonLineXPos horizonline, increment#
DrawHorizonLine horizonline
END SUB

SUB UpdateHorizonLineXPos (horizonline AS HorizonLineType, increment#)
horizonline.XPos = horizonline.XPos + increment#

WHILE horizonline.XPos >= HorizonWidth%
  horizonline.XPos = horizonline.XPos - HorizonWidth%

  horizonline.RegionTypes(0) = horizonline.RegionTypes(1)
  horizonline.RegionTypes(1) = horizonline.RegionTypes(2)
  horizonline.RegionTypes(2) = FIX(RND + 1 - horizonline.BumpThreshold)
WEND
END SUB

' Inverts the current colors.
SUB UpdateInvert (runner AS RunnerType, resetinvert%)
IF resetinvert% THEN
  runner.IsInverted = 0
ELSE
  runner.IsInverted = NOT runner.IsInverted
END IF
END SUB

' Update moving moon, changing phases.
SUB UpdateNightMode (runner AS RunnerType, nightmode AS NightModeType, activated%, paused%)
IF NOT paused% THEN
  ' Moon phase.
  IF activated% AND (nightmode.Opacity = 0) THEN
    nightmode.CurrentPhase = nightmode.CurrentPhase + 1

    IF nightmode.CurrentPhase >= NightModeNumPhases% THEN
      nightmode.CurrentPhase = 0
    END IF
  END IF

  ' Fade in / out.
  IF activated% AND (nightmode.Opacity < 1) THEN
    nightmode.Opacity = nightmode.Opacity + NightModeFadeSpeed#
  ELSEIF nightmode.Opacity > 0 THEN
    nightmode.Opacity = nightmode.Opacity - NightModeFadeSpeed#
  END IF
END IF

' Set moon positioning
IF nightmode.Opacity > 0 THEN
  IF NOT paused% THEN
    nightmode.XPos = UpdateNightModeXPos#((nightmode.XPos), NightModeMoonSpeed#)

    ' Update stars.
    IF nightmode.DrawStars THEN
      FOR i% = 0 TO NightModeNumStars% - 1
        nightmode.Stars(i%).XPos = UpdateNightModeXPos#(nightmode.Stars(i%).XPos, NightModeStarSpeed#)
      NEXT i%
    END IF
  END IF

  DrawNightMode runner, nightmode
ELSE
  nightmode.Opacity = 0
  PlaceNightModeStars nightmode
END IF

nightmode.DrawStars = -1
END SUB

FUNCTION UpdateNightModeXPos# (XPos#, delta#)
IF XPos# < -NightModeWidth% THEN
  UpdateNightModeXPos# = GameWidth%
ELSE
  UpdateNightModeXPos# = XPos# - delta#
END IF
END FUNCTION

' Obstacle frame update.
SUB UpdateObstacle (obstacle AS ObstacleType, deltatime#, speed#)
SHARED obstaclevariants() AS ObstacleVariantType

IF obstacle.IsActive THEN
  effectivespeed# = speed# + obstacle.SpeedOffset

  obstacle.XPos = obstacle.XPos - effectivespeed# * 60 * deltatime#

  ' Update frame
  IF obstaclevariants(obstacle.Variant).NumFrames <> 0 THEN
    obstacle.TimeValue = obstacle.TimeValue + deltatime#
    WHILE obstacle.TimeValue > obstaclevariants(obstacle.Variant).FrameDuration
      obstacle.CurrentFrame = ostacle.CurrentFrame + 1
      IF obstacle.CurrentFrame = obstaclevariants(obstacle.Variant).NumFrames THEN
        obstacle.CurrentFrame = 0
      END IF
      obstacle.TimeValue = obstacle.TimeValue - obstaclevariants(obstacle.Variant).FrameDuration
    WEND
  END IF

  IF obstacle.XPos + obstacle.ObstacleWidth > 0 THEN
    DrawObstacle obstacle
  ELSE
    obstacle.IsActive = 0
  END IF
END IF
END SUB

' Update the obstacle positions.
SUB UpdateObstacles (obstacles() AS ObstacleType, firstactiveobstacle%, nextinactiveobstacle%, deltatime#, speed#)
' Obstacles, move to Horizon layer.
i% = firstactiveobstacle%

WHILE i% <> nextinactiveobstacle%
  UpdateObstacle obstacles(i%), deltatime#, speed#

  ' Clean up existing obstacles.
  ' This can only happen on the leftmost obstacle, which is the first one.
  IF obstacles(i%).IsActive = 0 THEN
    firstactiveobstacle% = firstactiveobstacle% + 1
    IF firstactiveobstacle% >= MaxObstacles% THEN firstactiveobstacle% = 0
  END IF

  i% = i% + 1
  IF i% >= MaxObstacles% THEN i% = 0
WEND

shouldcreateobstacle% = 0

IF firstactiveobstacle% = nextinactiveobstacle% THEN
  ' No active obstacles
  shouldcreateobstacle% = -1
ELSE
  lastobstacle% = (nextinactiveobstacle% + MaxObstacles% - 1) MOD MaxObstacles%

  obstacleexists% = obstacles(lastobstacle%).IsActive
  obstacleislast% = NOT obstacles(lastobstacle%).FollowingObstacleCreated
  obstacleisvisible% = (obstacles(lastobstacle%).XPos + obstacles(lastobstacle%).ObstacleWidth > 0)
  obstaclegaphaspassed% = (obstacles(lastobstacle%).XPos + obstacles(lastobstacle%).ObstacleWidth + obstacles(lastobstacle%).Gap < GameWidth%)

  IF obstacleexists% AND obstacleislast% AND obstacleisvisible% AND obstaclegaphaspassed% THEN
    shouldcreateobstacle% = -1
    obstacles(lastobstacle%).FollowingObstacleCreated = -1
  END IF
END IF

IF shouldcreateobstacle% THEN
  ' Create new obstacles.
  AddNewObstacle obstacles(), firstactiveobstacle%, nextinactiveobstacle%, speed#
END IF
END SUB

' Set the animation status.
SUB UpdateTRex (trex AS TRexType, deltatime#, optstatus%)
SHARED trexanimframes() AS TRexAnimationType

trex.TimeValue = trex.TimeValue + deltatime#

' Update the status.
IF optstatus% <> 0 THEN
  trex.TRexStatus = optstatus%
  trex.CurrentFrame = 0

  IF optstatus% = TRexStatusWAITING% THEN
    trex.AnimStartTime = TIMER
    trex.CurrentFrame = 1
    SetTRexBlinkDelay trex
  ELSE
    trex.BlinkDelay = 0
  END IF

  IF optstatus% = TRexStatusDUCKING% THEN
    trex.ImageWidth = ImageDUCKINGWidth%
    trex.ImageHeight = ImageDUCKINGHeight%
  ELSE
    trex.ImageWidth = ImageTREXWidth%
    trex.ImageHeight = ImageTREXHeight%
  END IF
END IF

' Game intro animation, T-rex moves in from the left.
IF trex.PlayingIntro AND (trex.XPos < TRexStartXPos%) THEN
  trex.IntroTimeValue = trex.IntroTimeValue + deltatime#

  t# = trex.IntroTimeValue / TRexIntroDuration#

  IF t# > 1 THEN
    t# = 1
    trex.PlayingIntro = 0
  END IF

  trex.XPos = TRexStartXPos% * t#
END IF

IF trex.TRexStatus = TRexStatusWAITING% THEN
  TRexBlink trex, TIMER
ELSE
  DrawTRex trex, trexanimframes(trex.TRexStatus).ImageIndices(trex.CurrentFrame)
END IF

advanceframe% = -1

IF trex.BlinkDelay = 0 THEN
  ' Update the frame position.
  framedur# = trexanimframes(trex.TRexStatus).FrameDuration

  WHILE trex.TimeValue > framedur#
    trex.CurrentFrame = (trex.CurrentFrame + 1) MOD trexanimframes(trex.TRexStatus).NumFrames
    trex.TimeValue = trex.TimeValue - framedur#
  WEND
END IF

' Speed drop becomes duck if the down key is still being pressed.
IF trex.SpeedDrop AND (trex.YPos = trex.GroundYPos) THEN
  trex.SpeedDrop = 0
  SetTRexIsDucking trex, -1
END IF
END SUB

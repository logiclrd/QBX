'       Lianne in... The Dark Crown
'       Created by: DarkDread
'       Please see the INFO.TXT file for credits and thanks.
'       Please see README.TXT for help.
'
'       darkdread@geocities.com
'       http://www.geocities.com/SiliconValley/Pines/1732/dds.htm
'
DECLARE SUB AttemptRun ()
DECLARE SUB BeginGame ()
DECLARE SUB BossDialog ()
DECLARE SUB BoxOut ()
DECLARE SUB CheckSetUp ()
DECLARE SUB ClearDialog ()
DECLARE SUB ClearPickBox ()
DECLARE SUB CycleAnimation ()
DECLARE SUB CycleWater ()
DECLARE SUB Delay (Secs%)
DECLARE SUB DialogBox ()
DECLARE SUB Died ()
DECLARE SUB Ending ()
DECLARE SUB EnterName ()
DECLARE SUB FightBoss ()
DECLARE SUB FightEnemy ()
DECLARE SUB FireBall (Colors%, Colore%, X%, Y%, Siz%, Thng%)
DECLARE SUB FloatCrown ()
DECLARE SUB Help ()
DECLARE SUB InitLevel (StartLevel%)
DECLARE SUB LoadBossStats ()
DECLARE SUB LoadCells ()
DECLARE SUB LoadData ()
DECLARE SUB LoadEnemyStats ()
DECLARE SUB LoadFont ()
DECLARE SUB LoadSaveBox ()
DECLARE SUB LoadSaved ()
DECLARE SUB MoveCain ()
DECLARE SUB MoveDown ()
DECLARE SUB MoveLeft ()
DECLARE SUB MoveRight ()
DECLARE SUB MoveUp ()
DECLARE SUB OpenChest ()
DECLARE SUB Options ()
DECLARE SUB PutBack ()
DECLARE SUB PutLong (Value&, PosX%, PosY%, Digits%)
DECLARE SUB PutNumber (Value%, PosX%, PosY%, Digits%)
DECLARE SUB PutText (PosX%, PosY%, Sentence$)
DECLARE SUB Rain ()
DECLARE SUB RestorePickBox ()
DECLARE SUB SaveStats ()
DECLARE SUB SetUpBoss ()
DECLARE SUB SetUpEnemy ()
DECLARE SUB Shake (Duration%)
DECLARE SUB Shop ()
DECLARE SUB ShowHPMP ()
DECLARE SUB ShowPlayer ()
DECLARE SUB ShowStats ()
DECLARE SUB ShowStory ()
DECLARE SUB ShowStory2 ()
DECLARE SUB ShowTiles ()
DECLARE SUB Stance ()
DECLARE SUB UpdateX ()
DECLARE SUB VisiblePlane ()

'$DYNAMIC
DEFINT A-Z

ON ERROR GOTO ErrorTrap

DIM SHARED MaxX AS INTEGER
DIM SHARED MaxY AS INTEGER
DIM SHARED Saved(5) AS STRING
DIM SHARED Visible(16, 10) AS INTEGER
DIM SHARED Chest(60) AS INTEGER
DIM SHARED ChestCounter AS INTEGER
DIM SHARED LevelUp(40) AS LONG
DIM SHARED VisibleX AS INTEGER
DIM SHARED VisibleY AS INTEGER
DIM SHARED WaterCycle AS INTEGER
DIM SHARED AnimeCycle AS INTEGER
DIM SHARED Move AS INTEGER
DIM SHARED PlayerDead AS INTEGER
DIM SHARED EnemyDead AS INTEGER
DIM SHARED RunAway AS INTEGER
DIM SHARED Animation AS INTEGER
DIM SHARED ChrSet(33 TO 122, 1 TO 8, 1 TO 8) AS INTEGER
DIM SHARED PlayerDir AS STRING * 1
DIM SHARED Choice AS STRING * 1
DIM SHARED Name$
DIM SHARED BossBeat AS INTEGER
DIM SHARED Main AS INTEGER
DIM SHARED TextScroll AS INTEGER
DIM SHARED Speed(5) AS INTEGER
DIM SHARED NoConfig AS INTEGER

DIM SHARED Counter AS INTEGER
DIM SHARED HP(2) AS INTEGER
DIM SHARED MP(2) AS INTEGER
DIM SHARED CatFood AS LONG
DIM SHARED Item(4) AS INTEGER
DIM SHARED Weapon AS INTEGER
DIM SHARED Armour AS INTEGER
DIM SHARED PlayerLevel AS INTEGER
DIM SHARED Ex AS LONG
DIM SHARED Strength AS INTEGER
DIM SHARED Defense AS INTEGER
DIM SHARED MagicStrength AS INTEGER
DIM SHARED NextLevel AS INTEGER
DIM SHARED ForNextLevel AS INTEGER
DIM SHARED Cat AS INTEGER
DIM SHARED Quit AS INTEGER
DIM SHARED TempX AS INTEGER
DIM SHARED TempY AS INTEGER
DIM SHARED TempOn AS INTEGER
DIM SHARED AnimateBoss AS INTEGER
DIM SHARED WeaponType AS STRING
DIM SHARED ArmourType AS STRING
DIM SHARED Magic(5) AS STRING
DIM SHARED Talk AS STRING

DIM SHARED Enemy(23) AS STRING
DIM SHARED EnemyHP(23) AS INTEGER
DIM SHARED EnemyCatFood(23) AS INTEGER
DIM SHARED EnemyWeapon(23) AS STRING
DIM SHARED EnemyLevel(23) AS INTEGER
DIM SHARED EnemyEx(23) AS INTEGER
DIM SHARED EnemyStrength(23) AS INTEGER
DIM SHARED EnemyDefense(23) AS INTEGER
DIM SHARED EnemyMagic(23) AS INTEGER
DIM SHARED EnemyMagicType(23) AS STRING
DIM SHARED EnemyType AS INTEGER

DIM SHARED Boss(5) AS STRING
DIM SHARED BossHP(5) AS INTEGER
DIM SHARED BossCatFood(5) AS INTEGER
DIM SHARED BossWeapon(5, 5) AS STRING
DIM SHARED BossLevel(5) AS INTEGER
DIM SHARED BossEx(5) AS INTEGER
DIM SHARED BossStrength(5) AS INTEGER
DIM SHARED BossDefense(5) AS INTEGER
DIM SHARED BossMagic(5, 5) AS STRING
DIM SHARED BossType AS INTEGER

DIM SHARED CatX AS INTEGER
DIM SHARED CatY AS INTEGER
DIM SHARED BossX AS INTEGER
DIM SHARED BossY AS INTEGER
DIM SHARED StairsUpX AS INTEGER
DIM SHARED StairsUpY AS INTEGER
DIM SHARED StairsDownX AS INTEGER
DIM SHARED StairsDownY AS INTEGER
DIM SHARED PlayerX AS INTEGER
DIM SHARED PlayerY AS INTEGER
DIM SHARED CatXPos AS INTEGER
DIM SHARED CatYPos AS INTEGER
DIM SHARED BossXPos AS INTEGER
DIM SHARED BossYPos AS INTEGER

DIM SHARED G01(201)
DIM SHARED G02(201)
DIM SHARED G03(201)
DIM SHARED G04(201)
DIM SHARED G11(201)
DIM SHARED G12(201)
DIM SHARED G13(201)
DIM SHARED G14(201)
DIM SHARED G15(201)
DIM SHARED G26(201)
DIM SHARED G27(201)
DIM SHARED G29A1(201)
DIM SHARED G29A2(201)
DIM SHARED G38(201)
DIM SHARED G39(201)
DIM SHARED M02(201)
DIM SHARED M11(201)
DIM SHARED M26(201)
DIM SHARED M27(201)
DIM SHARED M29A1(201)
DIM SHARED M29A2(201)

DIM SHARED Down1(201)
DIM SHARED Down2(201)
DIM SHARED Up1(201)
DIM SHARED Up2(201)
DIM SHARED Right1(201)
DIM SHARED Right2(201)
DIM SHARED Left1(201)
DIM SHARED Left2(201)
DIM SHARED Down1Mask(201)
DIM SHARED Down2Mask(201)
DIM SHARED Up1Mask(201)
DIM SHARED Up2Mask(201)
DIM SHARED Right1Mask(201)
DIM SHARED Right2Mask(201)
DIM SHARED Left1Mask(201)
DIM SHARED Left2Mask(201)

DIM SHARED EnemyPic(9901) AS INTEGER
DIM SHARED EnemyMask(5300) AS INTEGER
DIM SHARED Boss2Mask(800) AS INTEGER
DIM SHARED Boss2(800) AS INTEGER
DIM SHARED Boss1Mask(201) AS INTEGER
DIM SHARED Boss1A(201) AS INTEGER
DIM SHARED Boss1B(201) AS INTEGER
DIM SHARED Boss1C(201) AS INTEGER

DIM SHARED StoryMap(16, 10) AS INTEGER

DIM SHARED ST01(201) AS INTEGER
DIM SHARED ST02(201) AS INTEGER
DIM SHARED ST10(201) AS INTEGER
DIM SHARED ST11(201) AS INTEGER
DIM SHARED ST12(201) AS INTEGER
DIM SHARED ST13(201) AS INTEGER
DIM SHARED ST14(201) AS INTEGER
DIM SHARED SM13(201) AS INTEGER
DIM SHARED SM14(201) AS INTEGER
DIM SHARED Character1(201) AS INTEGER
DIM SHARED Character2(201) AS INTEGER
DIM SHARED Mask1(201) AS INTEGER
DIM SHARED Mask2(201) AS INTEGER
DIM SHARED CrownX AS INTEGER
DIM SHARED CrownY AS INTEGER
DIM SHARED Float AS INTEGER
DIM SHARED K1X AS INTEGER
DIM SHARED K1Y AS INTEGER
DIM SHARED K2X AS INTEGER
DIM SHARED K2Y AS INTEGER
DIM SHARED LordX AS INTEGER
DIM SHARED LordY AS INTEGER

CONST True = -1
CONST False = 0
CONST InitPosX = 8
CONST InitPosY = 5
CONST StartLevel = 1
CONST EnemyMeet = 10
CONST Number = 5
CONST BackGround = 240
CONST NumDrops = 25

DIM SHARED DropType(1 TO NumDrops * 2)
DIM SHARED BackColour(1 TO NumDrops * 2)
DIM SHARED DropX(1 TO NumDrops * 2)
DIM SHARED DropY(1 TO NumDrops * 2)

MaxX = 50: MaxY = 50
DIM SHARED Maze(MaxX, MaxY) AS INTEGER

CLS
COLOR 14
LOCATE 2, 31
PRINT "Lianne in... The Dark Crown"
LOCATE 4, 34
PRINT "Created By: Darkdread"

CALL CheckSetUp
IF NoConfig = 4 THEN SYSTEM

CALL LoadCells
CALL LoadData
CALL LoadEnemyStats
CALL LoadFont
CALL LoadBossStats

PRINT : PRINT : PRINT "Free array space: "; FRE(-1)
WHILE INKEY$ = "": WEND

CALL InitLevel(StartLevel)
RANDOMIZE TIMER
SCREEN 13

DEF SEG = &HA000
BLOAD "DDLOGO.FSI"
CALL PutText(112, 150, "PRESENTS...")
SLEEP 8
CALL BoxOut
WHILE INKEY$ <> "": WEND

DO
  BLOAD "TITLE.FSI"
  CALL PutText(112, 0, "Lianne in...")
  CALL PutText(144, 16, "THE")
  CALL PutText(88, 188, "DarkDreams Software")
  CALL PutText(128, 132, "1 Begin")
  CALL PutText(128, 140, "2 Continue")
  CALL PutText(128, 148, "3 Quit")
  CALL PutText(96, 172, "Press H for help")
  DO
    SELECT CASE INKEY$
    CASE IS = "1"
      Main = 1
      KeyPressed = True
    CASE IS = "2"
      Main = 2
      KeyPressed = True
    CASE IS = "3"
      Main = 3
      KeyPressed = True
      QuitProgram = True
    CASE IS = "H", "h"
      Main = 4
      KeyPressed = True
    END SELECT
  LOOP UNTIL KeyPressed
  KeyPressed = False
  CALL BoxOut
 
  IF Main = 4 THEN CALL Help
  IF Main = 1 THEN
    CALL BeginGame
    CALL ShowStory
  END IF
  IF Main = 2 THEN
    CALL BeginGame
    CALL LoadSaved
  END IF

  IF Main < 3 THEN
    CALL CycleWater
    CALL VisiblePlane
    CALL ShowPlayer
    CALL Stance
    IF CatFood = 0 THEN CALL ShowStory2
  END IF
  IF Main < 3 THEN
    DO
      SELECT CASE INKEY$
        CASE CHR$(0) + CHR$(72)
          CALL MoveUp
          CALL CycleWater
          CALL CycleAnimation
        CASE CHR$(0) + CHR$(80)
          CALL MoveDown
          CALL CycleWater
          CALL CycleAnimation
        CASE CHR$(0) + CHR$(75)
          CALL MoveLeft
          CALL CycleWater
          CALL CycleAnimation
        CASE CHR$(0) + CHR$(77)
          CALL MoveRight
          CALL CycleWater
          CALL CycleAnimation
        CASE CHR$(32)
          IF PlayerDir = "N" AND Maze(PlayerX, PlayerY - 1) = 38 THEN
              Maze(PlayerX, PlayerY - 1) = 39
              Chest(ChestCounter) = PlayerX
              Chest(ChestCounter + 1) = PlayerY - 1
              ChestCounter = ChestCounter + 2
              CALL OpenChest
            ELSEIF PlayerDir = "E" AND Maze(PlayerX + 1, PlayerY) = 38 THEN
              Maze(PlayerX + 1, PlayerY) = 39
              Chest(ChestCounter) = PlayerX + 1
              Chest(ChestCounter + 1) = PlayerY
              ChestCounter = ChestCounter + 2
              CALL OpenChest
            ELSEIF PlayerDir = "S" AND Maze(PlayerX, PlayerY + 1) = 38 THEN
              Maze(PlayerX, PlayerY + 1) = 39
              Chest(ChestCounter) = PlayerX
              Chest(ChestCounter + 1) = PlayerY + 1
              ChestCounter = ChestCounter + 2
              CALL OpenChest
            ELSEIF PlayerDir = "W" AND Maze(PlayerX - 1, PlayerY) = 38 THEN
              Maze(PlayerX - 1, PlayerY) = 39
              Chest(ChestCounter) = PlayerX - 1
              Chest(ChestCounter + 1) = PlayerY
              ChestCounter = ChestCounter + 2
              CALL OpenChest
            ELSE
              CALL Options
          END IF
      END SELECT
      IF Quit OR PlayerDead OR BossBeat = 2 THEN EXIT DO
      IF WaterCycle = Speed(4) THEN CALL CycleWater
      IF AnimeCycle = Speed(3) THEN CALL CycleAnimation
      WaterCycle = WaterCycle + 1
      AnimeCycle = AnimeCycle + 1
      IF Talk = "cat" THEN CALL Shop
      IF Speed(1) > 0 THEN CALL Delay(Speed(1))
    LOOP
    IF BossBeat = 2 THEN CALL Ending
    IF PlayerDead THEN CALL Died
    Quit = False
  END IF
LOOP UNTIL QuitProgram

CALL UpdateX
CALL BoxOut

SCREEN 0: WIDTH 80
CLS
COLOR 14
PRINT "Copyright (c) 1997 DarkDreams Software"
PRINT "Lianne in... The Dark Crown"
PRINT "Created by: DarkDread"
SYSTEM

ErrorTrap:
CLS
SCREEN 0: WIDTH 80
SELECT CASE ERR
CASE IS = 7
  PRINT "Oh oh.  An out of memory error has happened."
  PRINT "There is not enough conventional memory to run this program."
CASE IS = 53
  PRINT "Oh oh.  The program could not find one of its files."
  PRINT "Make sure you are in the directory where all the files are located."
CASE ELSE
  PRINT "Oh oh.  A program error has happened."
  PRINT "The error code was:"; ERR
END SELECT
WHILE INKEY$ = "": WEND
PRINT
PRINT "Returning to system."
SYSTEM

REM $STATIC
SUB AttemptRun
  Player = PlayerLevel * (INT(RND * 10) + 1)
  Baddie = EnemyLevel(EnemyType) * (INT(RND * 10) + 1)
  IF Player >= Baddie THEN
      RunAway = True
    ELSE
      CALL ClearDialog
      IF Speed(2) > 0 THEN TextScroll = True
      CALL PutText(8, 128, "You tried to run but")
      CALL PutText(8, 136, "the enemy blocked you.")
      WHILE INKEY$ = "": WEND
      TextScroll = False
      CALL ClearDialog
  END IF
END SUB

SUB BeginGame
  CALL PutText(104, 88, "Please wait..")

  PlayerDead = False
  HP(1) = 25
  HP(2) = 25
  MP(1) = 4
  MP(2) = 4
  Weapon = 1
  Armour = 1
  PlayerLevel = 1
  Strength = 2
  Defense = 1
  MagicStrength = 1
  Ex = 0
  NextLevel = 10
  ForNextLevel = NextLevel - Ex
  TempX = 7
  TempY = 4
  TempOn = True
  WeaponType = "plastic"
  ArmourType = "cloth"
  CatFood = 0
  Cat = 0
  AnimateBoss = 0
  CurrentLevel = 1
  BossBeat = 0

  Magic(1) = "Fireball"
  FOR I = 2 TO 5
    Magic(I) = ""
  NEXT I
  FOR I = 1 TO 4
    Item(I) = 0
  NEXT I
  FOR I = 0 TO 60
    Chest(I) = 0
  NEXT I
  OPEN "LEVEL1.MAP" FOR INPUT AS #1
  FOR Y = 1 TO 50
    FOR X = 1 TO 50
      INPUT #1, MazeData
      Maze(X, Y) = MazeData
    NEXT X
  NEXT Y
  CLOSE #1
  OPEN "STORY.DAT" FOR INPUT AS #1
  FOR Y = 1 TO 10
    FOR X = 1 TO 16
      INPUT #1, StoryMap(X, Y)
    NEXT X
  NEXT Y
  CLOSE #1
 
  CALL InitLevel(StartLevel)
  LINE (104, 88)-(208, 96), 0, BF
END SUB

SUB BossDialog
  LINE (3, 123)-(196, 188), 205, B
  LINE (4, 124)-(195, 187), 109, B
  LINE (5, 125)-(194, 186), 35, B
  LINE (6, 126)-(193, 185), 109, B
  CALL ClearDialog
  IF Speed(2) > 0 THEN TextScroll = True
  IF BossType = 1 THEN
      CALL PutText(8, 128, "EVIL TREE: I warn you  ")
      CALL PutText(8, 136, "right now.  Do not step")
      CALL PutText(8, 144, "any closer and force me")
      CALL PutText(8, 152, "to deal with you!      ")
      WHILE INKEY$ = "": WEND
    ELSEIF BossType = 2 THEN
      CALL PutText(8, 128, "LIANNE: Who are you?   ")
      WHILE INKEY$ = "": WEND
      CALL PutText(8, 136, "MAGNUS: I am Magnus.  I")
      CALL PutText(8, 144, "guard the enterance to ")
      CALL PutText(8, 152, "Baron Cain's castle    ")
      CALL PutText(8, 160, "against intruders like ")
      CALL PutText(8, 168, "you.                   ")
      WHILE INKEY$ = "": WEND
      CALL PutText(8, 176, "LIANNE: Let me pass!   ")
      WHILE INKEY$ = "": WEND
      CALL ClearDialog
      CALL PutText(8, 128, "MAGNUS: Now, now.  That")
      CALL PutText(8, 136, "would be too easy.  No ")
      CALL PutText(8, 144, "excitement in that is  ")
      CALL PutText(8, 152, "there?                 ")
      WHILE INKEY$ = "": WEND
      CALL PutText(8, 160, "LIANNE: Then how do I  ")
      CALL PutText(8, 168, "get in this castle?    ")
      WHILE INKEY$ = "": WEND
      CALL PutText(8, 176, "MAGNUS: Simple.  A duel")
      WHILE INKEY$ = "": WEND
      CALL ClearDialog
      CALL PutText(8, 128, "between you and me.  If")
      CALL PutText(8, 136, "you win, then I let you")
      CALL PutText(8, 144, "pass.                  ")
      WHILE INKEY$ = "": WEND
      CALL PutText(8, 152, "LIANNE: And if I lose? ")
      WHILE INKEY$ = "": WEND
      CALL PutText(8, 160, "MAGNUS: HA, HA, HA, HA!")
      WHILE INKEY$ = "": WEND
  END IF
  TextScroll = False
  CALL VisiblePlane
END SUB

SUB BoxOut
  X2 = 319: Y2 = 199
  FOR I = 1 TO 159
    LINE (X1, Y1)-(X2, Y2), 0, B
    X1 = X1 + 1
    Y1 = Y1 + 1
    X2 = X2 - 1
    Y2 = Y2 - 1
    IF TextScroll THEN Delay (Speed(2))
  NEXT I
  WHILE INKEY$ <> "": WEND
END SUB

SUB CheckSetUp
  OPEN "SETUP.DAT" FOR INPUT AS #1
  FOR I = 1 TO 4
    INPUT #1, Check
    IF Check = 0 THEN NoConfig = NoConfig + 1
  NEXT I
  CLOSE #1
  IF NoConfig = 4 THEN
    LOCATE 6
    PRINT "The program has not been properly configured.  Please run the 'SETUP.BAS' file"
    PRINT "before playing."
    PRINT
    PRINT "Press any key to exit."
    WHILE INKEY$ = "": WEND
  END IF
END SUB

SUB ClearDialog
  LINE (7, 127)-(192, 184), BackGround, BF
END SUB

SUB ClearPickBox
  LINE (203, 123)-(315, 188), 205, B
  LINE (204, 124)-(314, 187), 109, B
  LINE (205, 125)-(313, 186), 35, B
  LINE (206, 126)-(312, 185), 109, B
  LINE (207, 127)-(311, 184), BackGround, BF
END SUB

SUB CycleAnimation
  AnimeCycle = 0
  IF Cat = 1 THEN
      IF Animation = 0 THEN
          PUT (CatXPos, CatYPos), G01, PSET
          PUT (CatXPos, CatYPos), M29A1, AND
          PUT (CatXPos, CatYPos), G29A1, XOR
          Animation = 1
        ELSE
          PUT (CatXPos, CatYPos), G01, PSET
          PUT (CatXPos, CatYPos), M29A2, AND
          PUT (CatXPos, CatYPos), G29A2, XOR
          Animation = 0
      END IF
  END IF
  IF AnimateBoss = 1 THEN
      IF Animation = 0 THEN
          PUT (BossXPos, BossYPos), Boss1Mask, AND
          PUT (BossXPos, BossYPos), Boss1A, XOR
          Animation = 1
        ELSEIF Animation = 1 THEN
          PUT (BossXPos, BossYPos), Boss1Mask, AND
          PUT (BossXPos, BossYPos), Boss1B, XOR
          Animation = 2
        ELSEIF Animation = 2 THEN
          PUT (BossXPos, BossYPos), Boss1Mask, AND
          PUT (BossXPos, BossYPos), Boss1C, XOR
          Animation = 3
        ELSEIF Animation = 3 THEN
          PUT (BossXPos, BossYPos), Boss1Mask, AND
          PUT (BossXPos, BossYPos), Boss1B, XOR
          Animation = 0
      END IF
  END IF
END SUB

SUB CycleWater STATIC
  WaterCycle = 0
  IF Water = 0 THEN
        OUT &H3C8, 250: OUT &H3C9, 0: OUT &H3C9, 0: OUT &H3C9, 50
        OUT &H3C8, 248: OUT &H3C9, 0: OUT &H3C9, 0: OUT &H3C9, 40
        OUT &H3C8, 249: OUT &H3C9, 0: OUT &H3C9, 0: OUT &H3C9, 30
      Water = 1
    ELSEIF Water = 1 THEN
        OUT &H3C8, 250: OUT &H3C9, 0: OUT &H3C9, 0: OUT &H3C9, 30
        OUT &H3C8, 248: OUT &H3C9, 0: OUT &H3C9, 0: OUT &H3C9, 50
        OUT &H3C8, 249: OUT &H3C9, 0: OUT &H3C9, 0: OUT &H3C9, 40
      Water = 2
    ELSEIF Water = 2 THEN
        OUT &H3C8, 250: OUT &H3C9, 0: OUT &H3C9, 0: OUT &H3C9, 40
        OUT &H3C8, 248: OUT &H3C9, 0: OUT &H3C9, 0: OUT &H3C9, 30
        OUT &H3C8, 249: OUT &H3C9, 0: OUT &H3C9, 0: OUT &H3C9, 50
      Water = 0
  END IF
END SUB

SUB Delay (Secs) STATIC
    IF SysFact& THEN
        IF Secs THEN
            Count& = (SysFact& * Secs) \ -54
            DO
            Count& = Count& + 1
                IF Count& = Z THEN EXIT DO
            LOOP UNTIL t2 = PEEK(&H6C)
        END IF
      ELSE
        DEF SEG = &H40
        t1 = PEEK(&H6C)
        DO
            t2 = PEEK(&H6C)
        LOOP UNTIL t2 <> t1
        DO
            SysFact& = SysFact& + 1
            IF SysFact& = Z THEN EXIT DO
        LOOP UNTIL t2 <> PEEK(&H6C)
        t2 = 256
    END IF
END SUB

SUB DialogBox
  LINE (50, 130)-(269, 190), BackGround, BF
  LINE (50, 130)-(269, 190), 205, B
  LINE (51, 131)-(268, 189), 105, B
  LINE (52, 132)-(267, 188), 35, B
  LINE (53, 133)-(266, 187), 105, B
END SUB

SUB Died
  CALL BoxOut
  CALL DialogBox
  IF Speed(2) > 0 THEN TextScroll = True
  CALL PutText(56, 136, "You have been defeated.   ")
  WHILE INKEY$ = "": WEND
  CALL PutText(56, 144, "Now Kissworld shall surely")
  CALL PutText(56, 152, "fall into darkness...")
  WHILE INKEY$ = "": WEND
  TextScroll = False
END SUB

SUB Ending
  CALL BoxOut
  CALL DialogBox
  IF Speed(2) > 0 THEN TextScroll = True
  WHILE INKEY$ <> "": WEND
  CALL PutText(56, 136, "Magnus was gone.  Only    ")
  CALL PutText(56, 144, "steps ahead lay the stairs")
  CALL PutText(56, 152, "into Baron Cain's castle. ")
  CALL PutText(56, 160, "Now the adventure would   ")
  CALL PutText(56, 168, "truly begin!              ")
  WHILE INKEY$ = "": WEND
  CALL BoxOut
  DEF SEG = &HA000
  BLOAD "BACK.FSI"
  CALL PutText(0, 64, "     Thanks for trying the demo version ")
  CALL PutText(0, 72, "of Lianne in... The Dark Crown.  I hope ")
  CALL PutText(0, 80, "you've enjoyed the game.  Why not drop  ")
  CALL PutText(0, 88, "an e-mail to: darkdread@geocities.com to")
  CALL PutText(0, 96, "let me know what you think so far.  The ")
  CALL PutText(0, 104, "game will be continued of course.       ")
  CALL PutText(0, 112, "                             - DarkDread")
  WHILE INKEY$ = "": WEND
  TextScroll = False
  CALL BoxOut
END SUB

SUB EnterName
  LINE (112, 128)-(208, 144), BackGround, BF
  CALL PutText(112, 128, "Enter a name")
  CALL PutText(112, 136, "........")
  Name$ = ""
  DO
    N$ = INPUT$(1)
    IF N$ = CHR$(8) THEN
      IF Length = 9 THEN Length = 8
      IF Length <> 0 THEN Length = Length - 1
      Name$ = LEFT$(Name$, Length)
      LINE (112, 136)-(208, 144), BackGround, BF
      CALL PutText(112, 136, Name$)
    END IF
    IF Length < 9 AND N$ <> CHR$(8) THEN Length = Length + 1
    IF N$ = CHR$(13) THEN EXIT DO
    IF N$ <> CHR$(8) THEN
      IF ASC(N$) > 47 AND ASC(N$) < 58 OR ASC(N$) > 64 AND ASC(N$) < 91 OR ASC(N$) > 96 AND ASC(N$) < 123 THEN Name$ = Name$ + N$
    END IF
    IF Length = 9 THEN Name$ = LEFT$(Name$, 8)
    CALL PutText(112, 136, Name$)
  LOOP
END SUB

SUB FightBoss
  CALL ShowStats
  LINE (3, 123)-(196, 188), 205, B
  LINE (4, 124)-(195, 187), 109, B
  LINE (5, 125)-(194, 186), 35, B
  LINE (6, 126)-(193, 185), 109, B
  CALL SetUpBoss
  EnHP = BossHP(BossType)
    RunAway = False
    PlayerDead = False
    EnemyDead = False
    Choice = ""
  CALL ClearPickBox
  CALL ClearDialog
  IF Speed(2) > 0 THEN TextScroll = True
  IF BossType = 1 THEN
      CALL PutText(8, 128, "EVIL TREE: I warned you")
    ELSEIF BossType = 2 THEN
      CALL PutText(8, 128, "MAGNUS: Then I get to  ")
      CALL PutText(8, 136, "bury your body wherever")
      CALL PutText(8, 144, "I see fit!             ")
  END IF
  TextScroll = False
  DO
    CALL PutText(232, 136, "(f)ight")
    CALL PutText(232, 144, "(r)un")
    DO WHILE NOT MadeChoice
      SELECT CASE LCASE$(INKEY$)
      CASE IS = "r"
        Choice = "r"
        MadeChoice = True
      CASE IS = "f"
        Choice = "f"
        MadeChoice = True
      END SELECT
    LOOP
    CALL ClearPickBox
    MadeChoice = False
    IF Choice = "r" THEN
      CALL ClearDialog
      IF Speed(2) > 0 THEN TextScroll = True
      IF BossType = 1 THEN
          CALL PutText(8, 128, "EVIL TREE: There is  ")
          CALL PutText(8, 136, "no escape.           ")
        ELSEIF BossType = 2 THEN
          CALL PutText(8, 128, "MAGNUS: You can't    ")
          CALL PutText(8, 136, "leave now!           ")
      END IF
      TextScroll = False
      WHILE INKEY$ = "": WEND
    END IF
    IF NOT RunAway THEN
      IF Stunned = 0 THEN
        CALL ClearDialog
        CALL PutText(8, 128, "Choose your course of")
        CALL PutText(8, 136, "action:              ")
        CALL PutText(232, 136, "(a)ttack")
        CALL PutText(232, 144, "(m)agic")
        DO WHILE NOT MadeChoice
          SELECT CASE LCASE$(INKEY$)
          CASE IS = "a"
            Choice = "a"
            MadeChoice = True
          CASE IS = "m"
            Choice = "m"
            MadeChoice = True
          END SELECT
        LOOP
        CALL ClearPickBox
        MadeChoice = False
        CALL ClearDialog
        IF Choice = "a" THEN
            Hit = (Strength * Weapon * (INT(RND * 5) + 1)) - (BossDefense(BossType) * (INT(RND * 5) + 1))
            IF Hit < 1 THEN Hit = 1
            EnHP = EnHP - Hit
            IF Speed(2) > 0 THEN TextScroll = True
            CALL PutText(8, 128, "You attack " + Boss(BossType))
            CALL PutText(8, 136, "with a " + WeaponType + " CD and")
            CALL PutText(8, 144, "do" + STR$(Hit) + " damage.")
            TextScroll = False
            IF EnHP < 1 THEN EnemyDead = True
            WHILE INKEY$ = "": WEND
          ELSEIF Choice = "m" THEN
            CALL PutText(8, 128, "Select the magic you   ")
            CALL PutText(8, 136, "wish to use.           ")
            CALL PutText(224, 136, "1 " + Magic(1))
            CALL PutText(224, 144, "2 " + Magic(2))
            CALL PutText(224, 152, "3 " + Magic(3))
            MagicUse = 0
            DO
              SELECT CASE INKEY$
              CASE IS = "1"
                IF Magic(1) = "Fireball" THEN
                  MagicUse = 1
                  MPNeed = 2
                END IF
              CASE IS = "2"
                IF Magic(2) = "Stunner" THEN
                  MagicUse = 2
                  MPNeed = 3
                END IF
              CASE IS = "3"
                IF Magic(3) = "RainStorm" THEN
                  MagicUse = 3
                  MPNeed = 7
                END IF
              END SELECT
              IF MagicUse > 0 THEN EXIT DO
            LOOP
            CALL ClearDialog
            CALL ClearPickBox
            IF MP(1) >= MPNeed AND MagicUse <> 2 THEN
                IF MagicUse = 1 THEN
                    CALL FireBall(39, 43, 100, 56, 50, 2)
                  ELSEIF MagicUse = 3 THEN
                    CALL Rain
                END IF
                CALL SetUpBoss
                Hit = MagicStrength * (INT(RND * 5) + MagicUse * 2)
                EnHP = EnHP - Hit
                IF EnHP < 0 THEN EnemyDead = True
                MP(1) = MP(1) - MPNeed
                CALL ShowHPMP
                IF Speed(2) > 0 THEN TextScroll = True
                CALL PutText(8, 128, "You cast the spell of")
                CALL PutText(8, 136, Magic(MagicUse) + " and do" + STR$(Hit))
                CALL PutText(8, 144, "damage.")
                TextScroll = False
                WHILE INKEY$ = "": WEND
              ELSEIF MP(1) >= MPNeed AND MagicUse = 2 THEN
                CALL Shake(2000)
                CALL SetUpBoss
                StunEnemy = INT(RND * 5) + 1
                Hit = MagicStrength * (INT(RND * 5) + MagicUse)
                EnHP = EnHP - Hit
                IF EnHP < 0 THEN EnemyDead = True
                MP(1) = MP(1) - MPNeed
                CALL ShowHPMP
                IF Speed(2) > 0 THEN TextScroll = True
                CALL PutText(8, 128, "You cast the spell of")
                CALL PutText(8, 136, Magic(MagicUse) + " and do" + STR$(Hit))
                CALL PutText(8, 144, "damage.")
                IF StunEnemy = 1 THEN
                  CALL PutText(8, 152, Boss(BossType) + " stunned.")
                  EnemyStunned = 3
                END IF
                TextScroll = False
                WHILE INKEY$ = "": WEND
              ELSEIF MP(1) < MPNeed THEN
                IF Speed(2) > 0 THEN TextScroll = True
                CALL PutText(8, 128, "You attempt to cast  ")
                CALL PutText(8, 136, "the spell of " + Magic(MagicUse))
                CALL PutText(8, 144, "and fail.")
                TextScroll = False
                WHILE INKEY$ = "": WEND
            END IF
        END IF
      END IF
      IF NOT EnemyDead AND EnemyStunned = 0 THEN
        IF BossType = 1 THEN
            AttackType = INT(RND * 2) + 1
          ELSEIF BossType = 2 THEN
            AttackType = INT(RND * 2) + 1
        END IF
        IF AttackType = 1 AND BossType = 1 THEN
            Hit = BossStrength(BossType) * (INT(RND * 5) + 1) - (Defense * (INT(RND * 5) + 1))
          ELSEIF AttackType = 2 AND BossType = 1 THEN
            Hit = BossStrength(BossType) * (INT(RND * 3) + 5) - (Defense * (INT(RND * 5) + 1))
          ELSEIF AttackType = 1 AND BossType = 2 THEN
            Times = INT(RND * 2) + 1
            Hit = BossStrength(BossType) * (INT(RND * 3) + 5) * Times - (Defense * (INT(RND * 5) + 1))
        END IF
        IF Hit < 1 THEN Hit = 1
        HP(1) = HP(1) - Hit
        CALL ShowHPMP
        IF BossType = 1 THEN
            IF Speed(2) > 0 THEN TextScroll = True
            CALL PutText(8, 160, Boss(BossType) + " attacks by")
            CALL PutText(8, 168, BossWeapon(BossType, AttackType) + " for" + STR$(Hit) + " damage.")
            TextScroll = False
          ELSEIF BossType = 2 AND AttackType = 1 THEN
            IF Speed(2) > 0 THEN TextScroll = True
            CALL PutText(8, 160, Boss(BossType) + " attacks by")
            CALL PutText(8, 168, BossWeapon(BossType, AttackType) + ", hitting" + STR$(Times))
            CALL PutText(8, 176, "times for" + STR$(Hit) + " damage.")
            TextScroll = False
          ELSEIF BossType = 2 AND AttackType = 2 THEN
            HP(1) = HP(1) + Hit
            CALL ShowHPMP
            IF Speed(2) > 0 THEN TextScroll = True
            CALL PutText(8, 160, Boss(BossType) + " misses!")
            TextScroll = False
        END IF
        IF HP(1) < 1 THEN PlayerDead = True
        WHILE INKEY$ = "": WEND
      END IF
      CALL ClearDialog
      IF Stunned AND NOT PlayerDead THEN
          IF Speed(2) > 0 THEN TextScroll = True
          Stunned = Stunned - 1
          CALL PutText(8, 128, "You are stunned.")
          TextScroll = False
          WHILE INKEY$ = "": WEND
        ELSEIF EnemyStunned AND NOT EnemyDead THEN
          EnemyStunned = EnemyStunned - 1
          IF Speed(2) > 0 THEN TextScroll = True
          CALL PutText(8, 128, Boss(BossType) + " stunned.")
          TextScroll = False
          WHILE INKEY$ = "": WEND
      END IF
    END IF
  LOOP UNTIL PlayerDead OR EnemyDead OR RunAway
  IF EnemyDead THEN
    IF BossType = 1 THEN
        DEF SEG = VARSEG(EnemyPic(0)): BLOAD "GRASS.BSG", VARPTR(EnemyPic(0))
      ELSEIF BossType = 2 THEN
        DEF SEG = VARSEG(EnemyPic(0)): BLOAD "CASTLE.BSG", VARPTR(EnemyPic(0))
    END IF
    PUT (10, 3), EnemyPic, PSET
    LINE (10, 3)-(189, 115), 205, B
    LINE (11, 4)-(188, 114), 109, B
    LINE (12, 5)-(187, 113), 35, B
    LINE (13, 6)-(186, 112), 109, B
    CALL ClearDialog
    CatFood = CatFood + BossCatFood(BossType)
    Ex = Ex + BossEx(BossType)
    ForNextLevel = NextLevel - Ex
    IF Speed(2) > 0 THEN TextScroll = True
    CALL PutText(8, 128, "Yeah! You've defeated")
    CALL PutText(8, 136, Boss(BossType) + " gaining")
    CALL PutText(8, 144, STR$(BossCatFood(BossType)) + " pieces of cat food")
    CALL PutText(8, 152, "and" + STR$(BossEx(BossType)) + " experience.")
    TextScroll = False
    CALL ShowStats
    WHILE INKEY$ = "": WEND
    IF Ex >= NextLevel THEN
      CALL ClearDialog
      IF Speed(2) > 0 THEN TextScroll = True
      CALL PutText(8, 128, "Yeah! You've gained")
      CALL PutText(8, 136, "a level!")
      PlayerLevel = PlayerLevel + 1
      IF PlayerLevel = 8 THEN
          Magic(2) = "Stunner"
          CALL PutText(8, 144, "You learn Stunner.")
        ELSEIF PlayerLevel = 12 THEN
          Magic(3) = "RainStorm"
          CALL PutText(8, 144, "You learn RainStorm.")
      END IF
      TextScroll = False
      IF PlayerLevel < 41 THEN
          NextLevel = LevelUp(PlayerLevel)
        ELSE
          NextLevel = NextLevel + 25000
      END IF
      ForNextLevel = NextLevel - Ex
      Increase = INT(RND * 5) + 5
      HP(2) = HP(2) + Increase
      HP(1) = HP(1) + Increase
      MP(2) = MP(2) + (INT(Increase / 2))
      MP(1) = MP(1) + (INT(Increase / 2))
      Increase = INT(RND * 6) + 1
      IF Increase = 1 THEN
          Strength = Strength + 1
        ELSEIF Increase = 2 THEN
          Defense = Defense + 1
        ELSEIF Increase = 3 THEN
          MagicStrength = MagicStrength + 1
      END IF
      CALL ShowStats
      WHILE INKEY$ = "": WEND
    END IF
  END IF
  IF BossType = 1 THEN
      Maze(BossX, BossY) = 1
      BossBeat = 1
    ELSEIF BossType = 2 THEN
      BossBeat = 2
  END IF
  CALL VisiblePlane
  IF TempOn THEN CALL ShowPlayer
  OUT &H3C8, 36
  OUT &H3C9, 63
  OUT &H3C9, 0
  OUT &H3C9, 63
  OUT &H3C8, 37
  OUT &H3C9, 63
  OUT &H3C9, 0
  OUT &H3C9, 47
END SUB

SUB FightEnemy
  CALL ShowStats
  LINE (3, 123)-(196, 188), 205, B
  LINE (4, 124)-(195, 187), 109, B
  LINE (5, 125)-(194, 186), 35, B
  LINE (6, 126)-(193, 185), 109, B
    RunAway = False
    PlayerDead = False
    EnemyDead = False
    Choice = ""
  IF PlayerY < 21 THEN
      EnemyType = INT(RND * 2) + 1
    ELSE
      EnemyType = INT(RND * 6) + 1
  END IF
  CALL SetUpEnemy
  EnHP = EnemyHP(EnemyType)
  CALL ClearPickBox
  CALL ClearDialog
  IF Speed(2) > 0 THEN TextScroll = True
  CALL PutText(8, 128, "Uh oh, a " + Enemy(EnemyType))
  CALL PutText(8, 136, "confronts you.  What do")
  CALL PutText(8, 144, "you do?                ")
  TextScroll = False
  DO
    CALL PutText(232, 136, "(f)ight")
    CALL PutText(232, 144, "(r)un")
    DO WHILE NOT MadeChoice
      SELECT CASE LCASE$(INKEY$)
      CASE IS = "r"
        Choice = "r"
        MadeChoice = True
      CASE IS = "f"
        Choice = "f"
        MadeChoice = True
      END SELECT
    LOOP
    CALL ClearPickBox
    MadeChoice = False
    IF Choice = "r" THEN CALL AttemptRun
    IF NOT RunAway THEN
      IF Stunned = 0 THEN
        CALL ClearDialog
        CALL PutText(8, 128, "Choose your course of")
        CALL PutText(8, 136, "action:              ")
        CALL PutText(232, 136, "(a)ttack")
        CALL PutText(232, 144, "(m)agic")
        DO WHILE NOT MadeChoice
          SELECT CASE LCASE$(INKEY$)
          CASE IS = "a"
            Choice = "a"
            MadeChoice = True
          CASE IS = "m"
            Choice = "m"
            MadeChoice = True
          END SELECT
        LOOP
        CALL ClearPickBox
        MadeChoice = False
        CALL ClearDialog
        IF Choice = "a" THEN
            Hit = (Strength * Weapon * (INT(RND * 5) + 1)) - (EnemyDefense(EnemyType) * (INT(RND * 5) + 1))
            IF Hit < 1 THEN Hit = 1
            EnHP = EnHP - Hit
            IF Speed(2) > 0 THEN TextScroll = True
            CALL PutText(8, 128, "You attack " + Enemy(EnemyType))
            CALL PutText(8, 136, "with a " + WeaponType + " CD and")
            CALL PutText(8, 144, "do" + STR$(Hit) + " damage.")
            TextScroll = False
            IF EnHP < 1 THEN EnemyDead = True
            WHILE INKEY$ = "": WEND
          ELSEIF Choice = "m" THEN
            CALL PutText(8, 128, "Select the magic you   ")
            CALL PutText(8, 136, "wish to use.           ")
            CALL PutText(224, 136, "1 " + Magic(1))
            CALL PutText(224, 144, "2 " + Magic(2))
            CALL PutText(224, 152, "3 " + Magic(3))
            MagicUse = 0
            DO
              SELECT CASE INKEY$
              CASE IS = "1"
                IF Magic(1) = "Fireball" THEN
                  MagicUse = 1
                  MPNeed = 2
                END IF
              CASE IS = "2"
                IF Magic(2) = "Stunner" THEN
                  MagicUse = 2
                  MPNeed = 3
                END IF
              CASE IS = "3"
                IF Magic(3) = "RainStorm" THEN
                  MagicUse = 3
                  MPNeed = 7
                END IF
              END SELECT
              IF MagicUse > 0 THEN EXIT DO
            LOOP
            CALL ClearDialog
            CALL ClearPickBox
            IF MP(1) >= MPNeed AND MagicUse <> 2 THEN
                IF MagicUse = 1 THEN
                    CALL FireBall(39, 43, 100, 56, 50, 2)
                  ELSEIF MagicUse = 3 THEN
                    CALL Rain
                END IF
                CALL SetUpEnemy
                Hit = (MagicStrength + 1) * (INT(RND * 5) + MagicUse * 2)
                EnHP = EnHP - Hit
                IF EnHP < 0 THEN EnemyDead = True
                MP(1) = MP(1) - MPNeed
                CALL ShowHPMP
                IF Speed(2) > 0 THEN TextScroll = True
                CALL PutText(8, 128, "You cast the spell of")
                CALL PutText(8, 136, Magic(MagicUse) + " and do" + STR$(Hit))
                CALL PutText(8, 144, "damage.")
                TextScroll = False
                WHILE INKEY$ = "": WEND
              ELSEIF MP(1) >= MPNeed AND MagicUse = 2 THEN
                CALL Shake(2000)
                StunEnemy = INT(RND * 5) + 1
                Hit = (MagicStrength + 1) * (INT(RND * 5) + MagicUse)
                EnHP = EnHP - Hit
                IF EnHP < 0 THEN EnemyDead = True
                MP(1) = MP(1) - MPNeed
                CALL ShowHPMP
                IF Speed(2) > 0 THEN TextScroll = True
                CALL PutText(8, 128, "You cast the spell of")
                CALL PutText(8, 136, Magic(MagicUse) + " and do" + STR$(Hit))
                CALL PutText(8, 144, "damage.")
                IF StunEnemy = 1 THEN
                  CALL PutText(8, 152, Enemy(EnemyType) + " stunned.")
                  EnemyStunned = 3
                END IF
                TextScroll = False
                WHILE INKEY$ = "": WEND
              ELSEIF MP(1) < MPNeed THEN
                IF Speed(2) > 0 THEN TextScroll = True
                CALL PutText(8, 128, "You attempt to cast  ")
                CALL PutText(8, 136, "the spell of " + Magic(MagicUse))
                CALL PutText(8, 144, "and fail.")
                TextScroll = False
                WHILE INKEY$ = "": WEND
            END IF
        END IF
      END IF
      IF NOT EnemyDead AND EnemyStunned = 0 THEN
        AttackType = INT(RND * 4) + 1
        IF AttackType = 1 AND EnemyMagicType(EnemyType) <> "" THEN
            IF EnemyMagicType(EnemyType) <> "Death" THEN
                Hit = EnemyMagic(EnemyType) * (INT(RND * 3) + 3) - (MagicStrength * INT(RND * 5) + 1)
                IF Hit < 1 THEN Hit = 1
                HP(1) = HP(1) - Hit
                CALL ShowHPMP
                IF Speed(2) > 0 THEN TextScroll = True
                CALL PutText(8, 160, Enemy(EnemyType) + " casts the")
                CALL PutText(8, 168, "spell of " + EnemyMagicType(EnemyType))
                CALL PutText(8, 176, "doing" + STR$(Hit) + " damage.")
                TextScroll = False
                IF EnemyMagicType(EnemyType) = "Stunner" THEN
                  StunPlayer = INT(RND * 5) + 1
                  IF StunPlayer = 1 THEN Stunned = 3
                END IF
                IF HP(1) < 1 THEN PlayerDead = True
                WHILE INKEY$ = "": WEND
            END IF
          ELSEIF AttackType <> 1 OR EnemyMagicType(EnemyMagicType) = "" THEN
            Hit = EnemyStrength(EnemyType) * (INT(RND * 5) + 1) - (Defense * (INT(RND * 5) + 1))
            IF Hit < 1 THEN Hit = 1
            HP(1) = HP(1) - Hit
            CALL ShowHPMP
            IF Speed(2) > 0 THEN TextScroll = True
            CALL PutText(8, 160, Enemy(EnemyType) + " attacks by")
            CALL PutText(8, 168, EnemyWeapon(EnemyType) + " for" + STR$(Hit) + " damage.")
            TextScroll = False
            IF HP(1) < 1 THEN PlayerDead = True
            WHILE INKEY$ = "": WEND
        END IF
      END IF
      CALL ClearDialog
      IF Speed(2) > 0 THEN TextScroll = True
      IF Stunned AND NOT PlayerDead THEN
          Stunned = Stunned - 1
          CALL PutText(8, 128, "You are stunned.")
          WHILE INKEY$ = "": WEND
        ELSEIF EnemyStunned AND NOT EnemyDead THEN
          EnemyStunned = EnemyStunned - 1
          CALL PutText(8, 128, Enemy(EnemyType) + " stunned.")
          WHILE INKEY$ = "": WEND
      END IF
      TextScroll = False
    END IF
  LOOP UNTIL PlayerDead OR EnemyDead OR RunAway
  IF EnemyDead THEN
    DEF SEG = VARSEG(EnemyPic(0)): BLOAD "GRASS.BSG", VARPTR(EnemyPic(0))
    PUT (10, 3), EnemyPic, PSET
    LINE (10, 3)-(189, 115), 205, B
    LINE (11, 4)-(188, 114), 109, B
    LINE (12, 5)-(187, 113), 35, B
    LINE (13, 6)-(186, 112), 109, B
    CALL ClearDialog
    CatFood = CatFood + EnemyCatFood(EnemyType)
    Ex = Ex + EnemyEx(EnemyType)
    ForNextLevel = NextLevel - Ex
    IF Speed(2) > 0 THEN TextScroll = True
    CALL PutText(8, 128, "Yeah! You've defeated")
    CALL PutText(8, 136, Enemy(EnemyType) + " gaining")
    CALL PutText(8, 144, STR$(EnemyCatFood(EnemyType)) + " pieces of cat food")
    CALL PutText(8, 152, "and" + STR$(EnemyEx(EnemyType)) + " experience.")
    TextScroll = False
    CALL ShowStats
    WHILE INKEY$ = "": WEND
    IF Ex >= NextLevel THEN
      CALL ClearDialog
      IF Speed(2) > 0 THEN TextScroll = True
      CALL PutText(8, 128, "Yeah! You've gained")
      CALL PutText(8, 136, "a level!")
      PlayerLevel = PlayerLevel + 1
      IF PlayerLevel = 8 THEN
          Magic(2) = "Stunner"
          CALL PutText(8, 144, "You learn Stunner.")
        ELSEIF PlayerLevel = 12 THEN
          Magic(3) = "RainStorm"
          CALL PutText(8, 144, "You learn RainStorm.")
      END IF
      TextScroll = False
      IF PlayerLevel < 41 THEN
          NextLevel = LevelUp(PlayerLevel)
        ELSE
          NextLevel = NextLevel + 25000
      END IF
      ForNextLevel = NextLevel - Ex
      Increase = INT(RND * 5) + 5
      HP(2) = HP(2) + Increase
      HP(1) = HP(1) + Increase
      MP(2) = MP(2) + (INT(Increase / 2))
      MP(1) = MP(1) + (INT(Increase / 2))
      Increase = INT(RND * 6) + 1
      IF Increase = 1 THEN
          Strength = Strength + 1
        ELSEIF Increase = 2 THEN
          Defense = Defense + 1
        ELSEIF Increase = 3 THEN
          MagicStrength = MagicStrength + 1
      END IF
      CALL ShowStats
      WHILE INKEY$ = "": WEND
    END IF
  END IF
  CALL VisiblePlane
  IF TempOn THEN CALL ShowPlayer
  OUT &H3C8, 36: OUT &H3C9, 63: OUT &H3C9, 0: OUT &H3C9, 63
  OUT &H3C8, 37: OUT &H3C9, 63: OUT &H3C9, 0: OUT &H3C9, 47
END SUB

SUB FireBall (Colors%, Colore%, X%, Y%, Siz%, Thng%)
  C% = Colors%

  DO
    Col = C%
    CIRCLE (X%, Y%), Siz%, Col
    PAINT (X% + Siz% - 1, Y% + Siz% / 100), Col
    C% = C% + 1
    IF C% > Colore% THEN C% = Colors%
    Siz% = Siz% - Thng%
    CALL Delay(10)
  LOOP UNTIL Siz% <= 1
END SUB

SUB FloatCrown
  IF Float = 0 THEN
      PUT (CrownX * 20, CrownY * 20), ST02, PSET
      PUT (CrownX * 20, CrownY * 20), SM13, AND
      PUT (CrownX * 20, CrownY * 20), ST13, XOR
      PUT (CrownX * 20, (CrownY * 20) - 1), SM14, AND
      PUT (CrownX * 20, (CrownY * 20) - 1), ST14, XOR
      Float = 1
    ELSEIF Float = 1 THEN
      PUT (CrownX * 20, CrownY * 20), ST02, PSET
      PUT (CrownX * 20, CrownY * 20), SM13, AND
      PUT (CrownX * 20, CrownY * 20), ST13, XOR
      PUT (CrownX * 20, (CrownY * 20) - 2), SM14, AND
      PUT (CrownX * 20, (CrownY * 20) - 2), ST14, XOR
      Float = 2
    ELSEIF Float = 2 THEN
      PUT (CrownX * 20, CrownY * 20), ST02, PSET
      PUT (CrownX * 20, CrownY * 20), SM13, AND
      PUT (CrownX * 20, CrownY * 20), ST13, XOR
      PUT (CrownX * 20, (CrownY * 20) - 3), SM14, AND
      PUT (CrownX * 20, (CrownY * 20) - 3), ST14, XOR
      Float = 3
    ELSEIF Float = 3 THEN
      PUT (CrownX * 20, CrownY * 20), ST02, PSET
      PUT (CrownX * 20, CrownY * 20), SM13, AND
      PUT (CrownX * 20, CrownY * 20), ST13, XOR
      PUT (CrownX * 20, (CrownY * 20) - 2), SM14, AND
      PUT (CrownX * 20, (CrownY * 20) - 2), ST14, XOR
      Float = 4
    ELSEIF Float = 4 THEN
      PUT (CrownX * 20, CrownY * 20), ST02, PSET
      PUT (CrownX * 20, CrownY * 20), SM13, AND
      PUT (CrownX * 20, CrownY * 20), ST13, XOR
      PUT (CrownX * 20, (CrownY * 20) - 1), SM14, AND
      PUT (CrownX * 20, (CrownY * 20) - 1), ST14, XOR
      Float = 0
  END IF
END SUB

SUB Help
  WHILE INKEY$ <> "": WEND
  DEF SEG = &HA000
  IF Speed(2) > 0 THEN TextScroll = True
  BLOAD "BACK.FSI"
  CALL PutText(120, 0, "Help File")
  CALL PutText(0, 16, "Welcome to Lianne in... The Dark Crown:")
  CALL PutText(0, 24, "An exciting, top-down RPG( role playing")
  CALL PutText(0, 32, "game), written entirely in Qbasic.  In")
  CALL PutText(0, 40, "this game, you will assume the role of")
  CALL PutText(0, 48, "Lianne and explore an island filled with")
  CALL PutText(0, 56, "treasure and monsters.")
  WHILE INKEY$ = "": WEND
  CALL PutText(112, 72, "How to Play")
  CALL PutText(0, 88, "Here are the basic keys for controlling")
  CALL PutText(0, 96, "Lianne:")
  CALL PutText(0, 104, "Left arrow key  - walk left")
  CALL PutText(0, 112, "Right arrow key - walk right")
  CALL PutText(0, 120, "Up arrow key    - walk up")
  CALL PutText(0, 128, "Down arrow key  - walk down")
  CALL PutText(0, 136, "Space bar       - options, open chest")
  CALL PutText(0, 152, "When pressing keys do not hold them down")
  CALL PutText(0, 160, "as this will make game play akward.")
  CALL PutText(272, 192, "1 of 3")
  WHILE INKEY$ = "": WEND
  BLOAD "BACK.FSI"
  CALL PutText(136, 0, "Menus")
  CALL PutText(0, 16, "Using menus in The Dark Crown is easy.")
  CALL PutText(0, 24, "Whenever you are given a menu, there will")
  CALL PutText(0, 32, "be one of two ways to navigate it.  If")
  CALL PutText(0, 40, "the choices have numbers, then press the")
  CALL PutText(0, 48, "number of the menu choice you wish.")
  CALL PutText(0, 56, "Otherwise, press the bracketed letter of")
  CALL PutText(0, 64, "the choice you wish.")
  WHILE INKEY$ = "": WEND
  CALL PutText(80, 80, "Object of the Game")
  CALL PutText(0, 96, "The object of The Dark Crown is to find")
  CALL PutText(0, 104, "the stairs which lead down into Baron")
  CALL PutText(0, 112, "Cain's castle.  This is easier said than")
  CALL PutText(0, 120, "done because many monsters will stop you")
  CALL PutText(0, 128, "along the way.")
  WHILE INKEY$ = "": WEND
  CALL PutText(120, 144, "Enemies")
  CALL PutText(0, 160, "Fighting enemies is easy.  Just let the")
  CALL PutText(0, 168, "menus guide you along but don't forget")
  CALL PutText(0, 176, "to keep an eye on your hit points.  Once")
  CALL PutText(0, 184, "they are down to zero, the game is over.")
  CALL PutText(272, 192, "2 of 3")
  WHILE INKEY$ = "": WEND
  BLOAD "BACK.FSI"
  CALL PutText(0, 0, "That's all of the help you will need for")
  CALL PutText(0, 8, "now.  You should be able to figure out")
  CALL PutText(0, 16, "the rest of the game by yourself.")
  WHILE INKEY$ = "": WEND
  CALL PutText(120, 32, "Thanks To")
  CALL PutText(0, 52, "Lianne - For being a great inspiration")
  CALL PutText(0, 60, "         and for always being there.")
  CALL PutText(0, 72, "Rich Geldreich - for the GIF viewer.  I")
  CALL PutText(0, 80, "         use this to import graphics and")
  CALL PutText(0, 88, "         change them to binary files.")
  CALL PutText(0, 96, "Nick Kochakian- for the 'perfect circle'")
  CALL PutText(0, 104, "         sub.")
  CALL PutText(0, 112, "Chris Sequeria - for the CPU independent")
  CALL PutText(0, 120, "         delay from Lunar Lander.")
  CALL PutText(0, 128, "William Yu - for the earth shaking")
  CALL PutText(0, 136, "         effect.")
  WHILE INKEY$ = "": WEND
  CALL PutText(120, 152, " Hail To")
  CALL PutText(0, 168, "My Dying Bride, Garnet, NeoStar, Brian ")
  CALL PutText(0, 176, "Hogan, Elias Holmlind, Mallard, William")
  CALL PutText(0, 184, "Yu, J1, Type O Negative, and anyone I  ")
  CALL PutText(0, 192, "forgot.")
  CALL PutText(272, 192, "3 of 3")
  WHILE INKEY$ = "": WEND
  TextScroll = False
END SUB

SUB InitLevel (StartLevel)
  SELECT CASE StartLevel
    CASE IS = 1
      VisibleX = 1: VisibleY = 1
      PlayerX = 7: PlayerY = 4
      PlayerDir = "W"
      CatX = 26: CatY = 14
      BossX = 27: BossY = 33
      StairsUpX = 24: StairsDownX = 23
  END SELECT
  Maze(CatX, CatY) = 29
  Maze(BossX, BossY) = 30
  Maze(StarisUpX, StairsUpY) = 26
END SUB

SUB LoadBossStats
  COLOR 14
  LOCATE 18, 1
  PRINT "LOADING BOSS     ";
  COLOR 4
  FOR I = 1 TO 2
    PRINT CHR$(219);
  NEXT I
  COLOR 14
  LOCATE 18, 18
  OPEN "BOSS.DAT" FOR INPUT AS #1
  FOR I = 1 TO 2
    INPUT #1, Boss(I)
    INPUT #1, BossHP(I)
    INPUT #1, BossDefense(I)
    INPUT #1, BossStrength(I)
    FOR J = 1 TO 5
      INPUT #1, BossWeapon(I, J)
    NEXT J
    INPUT #1, BossCatFood(I)
    INPUT #1, BossEx(I)
    INPUT #1, BossLevel(I)
    FOR J = 1 TO 5
      INPUT #1, BossMagic(I, J)
    NEXT J
    PRINT CHR$(219);
  NEXT I
  CLOSE #1
END SUB

SUB LoadCells
  COLOR 14
  LOCATE 10, 1
  PRINT "LOADING CELLS    ";
  COLOR 4
  FOR I = 1 TO 52
    PRINT CHR$(219);
  NEXT I
  COLOR 14
  LOCATE 10, 18
  DEF SEG = VARSEG(G01(0)): BLOAD "01.TIL", VARPTR(G01(0))
  PRINT CHR$(219);
  DEF SEG = VARSEG(G02(0)): BLOAD "02.TIL", VARPTR(G02(0))
  PRINT CHR$(219);
  DEF SEG = VARSEG(G03(0)): BLOAD "03.TIL", VARPTR(G03(0))
  PRINT CHR$(219);
  DEF SEG = VARSEG(G04(0)): BLOAD "04.TIL", VARPTR(G04(0))
  PRINT CHR$(219);
  DEF SEG = VARSEG(G11(0)): BLOAD "11.TIL", VARPTR(G11(0))
  PRINT CHR$(219);
  DEF SEG = VARSEG(G12(0)): BLOAD "12.TIL", VARPTR(G12(0))
  PRINT CHR$(219);
  DEF SEG = VARSEG(G13(0)): BLOAD "13.TIL", VARPTR(G13(0))
  PRINT CHR$(219);
  DEF SEG = VARSEG(G14(0)): BLOAD "14.TIL", VARPTR(G14(0))
  PRINT CHR$(219);
  DEF SEG = VARSEG(G15(0)): BLOAD "15.TIL", VARPTR(G15(0))
  PRINT CHR$(219);
  DEF SEG = VARSEG(G26(0)): BLOAD "BOAT.TIL", VARPTR(G26(0))
  PRINT CHR$(219);
  DEF SEG = VARSEG(G27(0)): BLOAD "27.TIL", VARPTR(G27(0))
  PRINT CHR$(219);
  DEF SEG = VARSEG(G29A1(0)): BLOAD "MANNIE1.TIL", VARPTR(G29A1(0))
  PRINT CHR$(219);
  DEF SEG = VARSEG(G29A2(0)): BLOAD "MANNIE2.TIL", VARPTR(G29A2(0))
  PRINT CHR$(219);
  DEF SEG = VARSEG(G38(0)): BLOAD "38.TIL", VARPTR(G38(0))
  PRINT CHR$(219);
  DEF SEG = VARSEG(G39(0)): BLOAD "39.TIL", VARPTR(G39(0))
  PRINT CHR$(219);
  DEF SEG = VARSEG(M02(0)): BLOAD "02.MSK", VARPTR(M02(0))
  PRINT CHR$(219);
  DEF SEG = VARSEG(M11(0)): BLOAD "11.MSK", VARPTR(M11(0))
  PRINT CHR$(219);
  DEF SEG = VARSEG(M26(0)): BLOAD "BOAT.MSK", VARPTR(M26(0))
  PRINT CHR$(219);
  DEF SEG = VARSEG(M27(0)): BLOAD "27.MSK", VARPTR(M27(0))
  PRINT CHR$(219);
  DEF SEG = VARSEG(M29A1(0)): BLOAD "MANNIE1.MSK", VARPTR(M29A1(0))
  PRINT CHR$(219);
  DEF SEG = VARSEG(M29A2(0)): BLOAD "MANNIE2.MSK", VARPTR(M29A2(0))
  PRINT CHR$(219);
  DEF SEG = VARSEG(Down1(0)): BLOAD "LIANNED1.TIL", VARPTR(Down1(0))
  PRINT CHR$(219);
  DEF SEG = VARSEG(Down2(0)): BLOAD "LIANNED2.TIL", VARPTR(Down2(0))
  PRINT CHR$(219);
  DEF SEG = VARSEG(Up1(0)): BLOAD "LIANNEU1.TIL", VARPTR(Up1(0))
  PRINT CHR$(219);
  DEF SEG = VARSEG(Up2(0)): BLOAD "LIANNEU2.TIL", VARPTR(Up2(0))
  PRINT CHR$(219);
  DEF SEG = VARSEG(Right1(0)): BLOAD "LIANNER1.TIL", VARPTR(Right1(0))
  PRINT CHR$(219);
  DEF SEG = VARSEG(Right2(0)): BLOAD "LIANNER2.TIL", VARPTR(Right2(0))
  PRINT CHR$(219);
  DEF SEG = VARSEG(Left1(0)): BLOAD "LIANNEL1.TIL", VARPTR(Left1(0))
  PRINT CHR$(219);
  DEF SEG = VARSEG(Left2(0)): BLOAD "LIANNEL2.TIL", VARPTR(Left2(0))
  PRINT CHR$(219);
  DEF SEG = VARSEG(Down1Mask(0)): BLOAD "LIANNED1.MSK", VARPTR(Down1Mask(0))
  PRINT CHR$(219);
  DEF SEG = VARSEG(Down2Mask(0)): BLOAD "LIANNED2.MSK", VARPTR(Down2Mask(0))
  PRINT CHR$(219);
  DEF SEG = VARSEG(Up1Mask(0)): BLOAD "LIANNEU1.MSK", VARPTR(Up1Mask(0))
  PRINT CHR$(219);
  DEF SEG = VARSEG(Up2Mask(0)): BLOAD "LIANNEU2.MSK", VARPTR(Up2Mask(0))
  PRINT CHR$(219);
  DEF SEG = VARSEG(Right1Mask(0)): BLOAD "LIANNER1.MSK", VARPTR(Right1Mask(0))
  PRINT CHR$(219);
  DEF SEG = VARSEG(Right2Mask(0)): BLOAD "LIANNER2.MSK", VARPTR(Right2Mask(0))
  PRINT CHR$(219);
  DEF SEG = VARSEG(Left1Mask(0)): BLOAD "LIANNEL1.MSK", VARPTR(Left1Mask(0))
  PRINT CHR$(219);
  DEF SEG = VARSEG(Left2Mask(0)): BLOAD "LIANNEL2.MSK", VARPTR(Left2Mask(0))
  PRINT CHR$(219);
  DEF SEG = VARSEG(ST01(0)): BLOAD "ST01.TIL", VARPTR(ST01(0))
  PRINT CHR$(219);
  DEF SEG = VARSEG(ST02(0)): BLOAD "ST02.TIL", VARPTR(ST02(0))
  PRINT CHR$(219);
  DEF SEG = VARSEG(ST10(0)): BLOAD "ST10.TIL", VARPTR(ST10(0))
  PRINT CHR$(219);
  DEF SEG = VARSEG(ST11(0)): BLOAD "ST11.TIL", VARPTR(ST11(0))
  PRINT CHR$(219);
  DEF SEG = VARSEG(ST12(0)): BLOAD "ST12.TIL", VARPTR(ST12(0))
  PRINT CHR$(219);
  DEF SEG = VARSEG(ST13(0)): BLOAD "ST13.TIL", VARPTR(SM13(0))
  PRINT CHR$(219);
  DEF SEG = VARSEG(ST14(0)): BLOAD "ST14.TIL", VARPTR(SM14(0))
  PRINT CHR$(219);
  DEF SEG = VARSEG(SM13(0)): BLOAD "ST13.MSK", VARPTR(SM13(0))
  PRINT CHR$(219);
  DEF SEG = VARSEG(SM14(0)): BLOAD "ST14.MSK", VARPTR(SM14(0))
  PRINT CHR$(219);
  DEF SEG = VARSEG(Boss1A(0)): BLOAD "BOSS1_1.TIL", VARPTR(Boss1A(0))
  PRINT CHR$(219);
  DEF SEG = VARSEG(Boss1B(0)): BLOAD "BOSS1_2.TIL", VARPTR(Boss1B(0))
  PRINT CHR$(219);
  DEF SEG = VARSEG(Boss1C(0)): BLOAD "BOSS1_3.TIL", VARPTR(Boss1C(0))
  PRINT CHR$(219);
  DEF SEG = VARSEG(Boss1Mask(0)): BLOAD "BOSS1_1.MSK", VARPTR(Boss1Mask(0))
  PRINT CHR$(219);
  DEF SEG = VARSEG(Boss2(0)): BLOAD "BOSS2.BSG", VARPTR(Boss2(0))
  PRINT CHR$(219);
  DEF SEG = VARSEG(Boss2Mask(0)): BLOAD "BOSS2.MSK", VARPTR(Boss2Mask(0))
  PRINT CHR$(219);
END SUB

SUB LoadData
  COLOR 14
  LOCATE 12, 1
  PRINT "LOADING DATA     ";
  COLOR 4
  FOR I = 1 TO 29
    PRINT CHR$(219);
  NEXT I
  COLOR 14
  LOCATE 12, 18
  OPEN "LEVEL1.MAP" FOR INPUT AS #1
  FOR Y = 1 TO 50
    FOR X = 1 TO 50
      INPUT #1, MazeData
      Maze(X, Y) = MazeData
    NEXT X
    IF Y / 2 = INT(Y / 2) THEN PRINT CHR$(219);
  NEXT Y
  CLOSE #1

  OPEN "LEVELUP.DAT" FOR INPUT AS #1
  FOR I = 2 TO 40
    INPUT #1, LevelUp(I)
  NEXT I
  CLOSE #1
  PRINT CHR$(219);
 
  OPEN "STORY.DAT" FOR INPUT AS #1
  FOR Y = 1 TO 10
    FOR X = 1 TO 16
      INPUT #1, StoryMap(X, Y)
    NEXT X
  NEXT Y
  CLOSE #1
  PRINT CHR$(219);

  OPEN "SAVES.DAT" FOR INPUT AS #1
  FOR I = 1 TO 5
    INPUT #1, Saved(I)
  NEXT I
  PRINT CHR$(219);
  CLOSE #1

  OPEN "SETUP.DAT" FOR INPUT AS #1
  FOR I = 1 TO 4
    INPUT #1, Speed(I)
  NEXT I
  PRINT CHR$(219);
  CLOSE #1
 
  COLOR 14
  CLOSE #1
END SUB

SUB LoadEnemyStats
  COLOR 14
  LOCATE 14, 1
  PRINT "LOADING ENEMY    ";
  COLOR 4
  FOR I = 1 TO 6
    PRINT CHR$(219);
  NEXT I
  COLOR 14
  LOCATE 14, 18
  OPEN "ENEMY.DAT" FOR INPUT AS #1
  FOR I = 1 TO 6
    INPUT #1, Enemy(I)
    INPUT #1, EnemyHP(I)
    INPUT #1, EnemyLevel(I)
    INPUT #1, EnemyEx(I)
    INPUT #1, EnemyCatFood(I)
    INPUT #1, EnemyWeapon(I)
    INPUT #1, EnemyStrength(I)
    INPUT #1, EnemyDefense(I)
    INPUT #1, EnemyMagic(I)
    IF EnemyMagic(I) > 0 THEN
      INPUT #1, EnemyMagicType(I)
    END IF
    PRINT CHR$(219);
  NEXT I
  CLOSE #1
END SUB

SUB LoadFont
  SHARED Font AS INTEGER, X AS INTEGER, Y AS INTEGER
  LOCATE 16, 1
  PRINT "LOADING FONTS    ";
  COLOR 4
  FOR I = 1 TO 30
    PRINT CHR$(219);
  NEXT I
  COLOR 14
  LOCATE 16, 18
  OPEN "FONTS.DAT" FOR INPUT AS #1
  Font = 32
  DO
    Font = Font + 1
    FOR Y = 1 TO 8
      FOR X = 1 TO 8
        INPUT #1, ChrSet(Font, X, Y)
      NEXT X
    NEXT Y
    Status = Status + 1
    IF Status = 3 THEN
      PRINT CHR$(219);
      Status = 0
    END IF
  LOOP UNTIL Font = 122
  CLOSE #1
  COLOR 7
END SUB

SUB LoadSaveBox
  LINE (110, 51)-(208, 154), BackGround, BF
  LINE (109, 50)-(209, 155), 109, B
  LINE (108, 49)-(210, 156), 35, B
  LINE (107, 48)-(211, 157), 109, B
  LINE (106, 47)-(212, 158), 205, B
  CALL PutText(112, 64, "  Save Game ")
  CALL PutText(112, 80, "  1 " + Saved(1))
  CALL PutText(112, 88, "  2 " + Saved(2))
  CALL PutText(112, 96, "  3 " + Saved(3))
  CALL PutText(112, 104, "  4 " + Saved(4))
  CALL PutText(112, 112, "  5 " + Saved(5))
  CALL PutText(112, 128, " Save [1-5] ")
  CALL PutText(112, 136, "Esc cancels.")
END SUB

SUB LoadSaved
  LINE (110, 51)-(208, 154), BackGround, BF
  LINE (109, 50)-(209, 155), 109, B
  LINE (108, 49)-(210, 156), 35, B
  LINE (107, 48)-(211, 157), 109, B
  LINE (106, 47)-(212, 158), 205, B
  CALL PutText(112, 64, "  Load Game ")
  CALL PutText(112, 80, "  1 " + Saved(1))
  CALL PutText(112, 88, "  2 " + Saved(2))
  CALL PutText(112, 96, "  3 " + Saved(3))
  CALL PutText(112, 104, "  4 " + Saved(4))
  CALL PutText(112, 112, "  5 " + Saved(5))
  CALL PutText(112, 128, " Load [1-5] ")
  CALL PutText(112, 136, "Esc cancels.")
  DO
    SELECT CASE INKEY$
    CASE IS = "1"
      IF Saved(1) <> "" THEN
        FileName$ = Saved(1)
        KeyPressed = True
      END IF
    CASE IS = "2"
      IF Saved(2) <> "" THEN
        FileName$ = Saved(2)
        KeyPressed = True
      END IF
    CASE IS = "3"
      IF Saved(3) <> "" THEN
        FileName$ = Saved(3)
        KeyPressed = True
      END IF
    CASE IS = "4"
      IF Saved(4) <> "" THEN
        FileName$ = Saved(4)
        KeyPressed = True
      END IF
    CASE IS = "5"
      IF Saved(5) <> "" THEN
        FileName$ = Saved(5)
        KeyPressed = True
      END IF
    CASE CHR$(27)
      QuitLoad = True
      KeyPressed = True
      Main = 4
    END SELECT
  LOOP UNTIL KeyPressed
  KeyPressed = False
  IF QuitLoad THEN EXIT SUB
  OPEN FileName$ FOR INPUT AS #1
    INPUT #1, VisibleX, VisibleY, PlayerDir, HP(1), HP(2), MP(1), MP(2), CatFood, Item(1), Item(2)
    INPUT #1, Item(3), Item(4), Weapon, Armour, PlayerLevel, Ex, Strength, Defense, MagicStrength, NextLevel
    INPUT #1, ForNextLevel, Cat, TempX, TempY, TempOn, AnimateBoss, WeaponType, ArmourType, Magic(1), Magic(2)
    INPUT #1, Magic(3), Magic(4), Magic(5), MusicType, PlayerX, PlayerY, CurrentLevel, BossBeat
  CLOSE #1
  IF BossBeat > 0 THEN
      Maze(BossX, BossY) = 1
  END IF
  OPEN FileName$ + ".DAT" FOR INPUT AS #1
  FOR I = 0 TO 59 STEP 2
    INPUT #1, Chest(I)
    INPUT #1, Chest(I + 1)
    IF Chest(I) > 0 AND Chest(I + 1) > 0 THEN Maze(Chest(I), Chest(I + 1)) = 39
  NEXT I
  CLOSE #1
END SUB

SUB MoveCain
  DEF SEG = VARSEG(Character1(0)): BLOAD "CAIN1.TIL", VARPTR(Character1(0))
  DEF SEG = VARSEG(Character2(0)): BLOAD "CAIN2.TIL", VARPTR(Character2(0))
  DEF SEG = VARSEG(Mask1(0)): BLOAD "CAIN1.MSK", VARPTR(Mask1(0))
  DEF SEG = VARSEG(Mask2(0)): BLOAD "CAIN2.MSK", VARPTR(Mask2(0))
  FOR Y = 20 TO 60 STEP 20
    FOR I = 1 TO 10
      IF CainFloat = 0 THEN
          IF Y > 20 THEN
              PUT (140, Y), ST02, PSET
            ELSE
              LINE (140, Y)-(159, Y + 19), 0, BF
          END IF
          PUT (140, Y), Mask1, AND
          PUT (140, Y), Character1, XOR
          CALL Delay(200)
          CainFloat = 1
        ELSEIF CainFloat = 1 THEN
          IF Y > 20 THEN
              PUT (140, Y), ST02, PSET
            ELSE
              LINE (140, Y)-(159, Y + 19), 0, BF
          END IF
          PUT (140, Y), Mask2, AND
          PUT (140, Y), Character2, XOR
          CALL Delay(200)
          CainFloat = 0
       END IF
    NEXT I
    IF Y > 20 AND Y < 60 THEN
        PUT (140, Y), ST02, PSET
      ELSEIF Y < 21 THEN
        LINE (140, Y)-(159, Y + 19), 0, BF
    END IF
  NEXT Y
END SUB

SUB MoveDown
  IF Maze(PlayerX, PlayerY + 1) < 11 THEN
    IF PlayerY + 1 >= MaxY - 4 OR PlayerY + 1 <= 5 THEN
        TempOn = True
      ELSEIF PlayerY + 1 >= 6 AND PlayerY + 1 < MaxY - 4 THEN
        TempOn = Flase
    END IF
    IF NOT TempOn THEN
        VisibleY = VisibleY + 1
        PlayerY = PlayerY + 1
        PlayerDir = "S"
        CALL VisiblePlane
      ELSEIF TempOn THEN
        CALL PutBack
        TempY = TempY + 1
        PlayerY = PlayerY + 1
        PlayerDir = "S"
        CALL ShowPlayer
    END IF
    IF INT(RND * EnemyMeet) + 1 = Number THEN CALL FightEnemy
    CALL Stance
  END IF
END SUB

SUB MoveLeft
  IF Maze(PlayerX - 1, PlayerY) < 11 THEN
    IF PlayerX - 1 <= 7 OR PlayerX - 1 >= MaxX - 8 THEN
        TempOn = True
      ELSEIF PlayerX - 1 <= MaxX - 9 AND PlayerX - 1 > 7 THEN
        TempOn = Flase
    END IF
    IF NOT TempOn THEN
        VisibleX = VisibleX - 1
        PlayerX = PlayerX - 1
        PlayerDir = "W"
        CALL VisiblePlane
      ELSEIF TempOn THEN
        CALL PutBack
        TempX = TempX - 1
        PlayerX = PlayerX - 1
        PlayerDir = "W"
        CALL ShowPlayer
    END IF
    IF INT(RND * EnemyMeet) + 1 = Number THEN CALL FightEnemy
    IF PlayerY >= 46 THEN CALL ShowPlayer
    CALL Stance
  END IF
END SUB

SUB MoveRight
  IF Maze(PlayerX + 1, PlayerY) < 11 THEN
    IF PlayerX + 1 >= MaxX - 7 OR PlayerX + 1 <= 8 THEN
        TempOn = True
      ELSEIF PlayerX + 1 >= 9 AND PlayerX + 1 < MaxX - 7 THEN
        TempOn = Flase
    END IF
    IF NOT TempOn THEN
        VisibleX = VisibleX + 1
        PlayerX = PlayerX + 1
        PlayerDir = "E"
        CALL VisiblePlane
      ELSEIF TempOn THEN
        CALL PutBack
        TempX = TempX + 1
        PlayerX = PlayerX + 1
        PlayerDir = "E"
        CALL ShowPlayer
    END IF
    IF INT(RND * EnemyMeet) + 1 = Number THEN CALL FightEnemy
    IF PlayerY >= 46 THEN CALL ShowPlayer
    CALL Stance
  END IF
END SUB

SUB MoveUp
  IF Maze(PlayerX, PlayerY - 1) < 11 THEN
    IF PlayerY - 1 <= 4 OR PlayerY - 1 >= MaxY - 5 THEN
        TempOn = True
      ELSEIF PlayerY - 1 <= MaxY - 6 AND PlayerY - 1 > 4 THEN
        TempOn = False
    END IF
    IF NOT TempOn THEN
        VisibleY = VisibleY - 1
        PlayerY = PlayerY - 1
        PlayerDir = "N"
        CALL VisiblePlane
      ELSEIF TempOn THEN
        CALL PutBack
        TempY = TempY - 1
        PlayerY = PlayerY - 1
        PlayerDir = "N"
        CALL ShowPlayer
    END IF
    IF INT(RND * EnemyMeet) + 1 = Number AND PlayerX <> 27 AND PlayerY <> 36 AND PlayerY <> 35 AND PlayerX <> 24 AND PlayerY <> 24 THEN CALL FightEnemy
    IF PlayerY = 36 AND PlayerX = 27 AND BossBeat < 1 THEN
        BossType = 1
        CALL BossDialog
      ELSEIF PlayerY = 35 AND PlayerX = 27 AND BossBeat < 1 THEN
        CALL FightBoss
      ELSEIF PlayerY = 24 AND PlayerX = 24 AND BossBeat < 2 THEN
        BossType = 2
        PUT (130, 38), Boss2Mask, AND
        PUT (130, 38), Boss2, XOR
        CALL BossDialog
        CALL FightBoss
    END IF
    CALL Stance
  END IF
END SUB

SUB OpenChest
  CALL VisiblePlane
  CALL ShowPlayer
  CALL DialogBox
  ChestType = INT(RND * 5)
  IF Speed(2) > 0 THEN TextScroll = True
  IF ChestType = 1 THEN
      CALL PutText(56, 136, "You open the chest and    ")
      CALL PutText(56, 144, "find some ice cream!      ")
      Item(2) = Item(2) + 1
    ELSEIF ChestType = 2 THEN
      CALL PutText(56, 136, "You open the chest and    ")
      CALL PutText(56, 144, "find some lemon cream pie!")
      Item(3) = Item(3) + 1
    ELSE
      Treasure = INT((RND * 5) + 1) * 10
      CatFood = CatFood + Treasure
      CALL PutText(56, 136, "You open the chest and    ")
      CALL PutText(56, 144, "find" + STR$(Treasure) + " pieces of")
      CALL PutText(56, 152, "cat food!")
  END IF
  TextScroll = False
  WHILE INKEY$ = "": WEND
  CALL VisiblePlane
  CALL ShowPlayer
END SUB

SUB Options
  LINE (3, 123)-(196, 188), 205, B
  LINE (4, 124)-(195, 187), 109, B
  LINE (5, 125)-(194, 186), 35, B
  LINE (6, 126)-(193, 185), 109, B
  CALL ClearDialog
  CALL ClearPickBox
  CALL ShowStats
  DO UNTIL Done
    CALL PutText(232, 128, "(T)ALK")
    CALL PutText(232, 136, "(U)SE")
    CALL PutText(232, 144, "(I)TEM")
    CALL PutText(232, 152, "(O)PTIONS")
    CALL PutText(232, 160, "(S)AVE")
    CALL PutText(232, 168, "(Q)UIT")
    CALL PutText(8, 136, "Please choose an option")
    CALL PutText(8, 144, "Press space to cancel. ")
    DO WHILE NOT MadeChoice
      Choice = LCASE$(INPUT$(1))
      SELECT CASE Choice
      CASE IS = "t", "u", "i", "o", "s", "q", " "
        MadeChoice = True
      END SELECT
    LOOP
    MadeChoice = False
    CALL ClearDialog
    CALL ClearPickBox
    IF Choice = "t" THEN
        Done = True
        IF PlayerX = CatX AND PlayerY - 1 = CatY OR PlayerY = CatY AND PlayerX + 1 = CatX THEN
            Talk = "cat"
          ELSE
            CALL PutText(8, 136, "There is no one there.")
            WHILE INKEY$ = "": WEND
        END IF
      ELSEIF Choice = "u" THEN
        IF Item(1) > 0 THEN CALL PutText(208, 128, "1 BAR " + STR$(Item(1)))
        IF Item(2) > 0 THEN CALL PutText(208, 136, "2 CONE" + STR$(Item(2)))
        IF Item(3) > 0 THEN CALL PutText(208, 144, "3 PIE " + STR$(Item(3)))
        IF Item(4) > 0 THEN CALL PutText(208, 152, "4 CAKE" + STR$(Item(4)))
        CALL PutText(8, 136, "Select an item to use.")
        CALL PutText(8, 144, "Press 0 to cancel.    ")
        DO WHILE NOT MadeChoice
          Choice = LCASE$(INPUT$(1))
          SELECT CASE Choice
          CASE IS = "1"
            IF Item(1) > 0 THEN
              Heal$ = "Chocolate Bar"
              MadeChoice = True
              HP = 10
              Item(1) = Item(1) - 1
            END IF
          CASE IS = "2"
            IF Item(2) > 0 THEN
              Heal$ = "Ice Cream Cone"
              MadeChoice = True
              HP = 25
              Item(2) = Item(2) - 1
             END IF
          CASE IS = "3"
            IF Item(3) > 0 THEN
              Heal$ = "Apple Pie"
              MadeChoice = True
              HP = 50
              Item(3) = Item(3) - 1
            END IF
          CASE IS = "4"
            IF Item(4) > 0 THEN
              Heal$ = "Chocolate Cake"
              MadeChoice = True
              HP = 100
              Item(4) = Item(4) - 1
            END IF
          CASE IS = "0"
            MadeChoice = True
          END SELECT
        LOOP
        MadeChoice = False
        IF HP > 0 THEN
          HP(1) = HP(1) + HP
          IF HP(1) > HP(2) THEN HP(1) = HP(2)
          CALL ClearDialog
          CALL PutText(8, 136, "Lianne ate the")
          CALL PutText(8, 144, Heal$ + " and")
          CALL PutText(8, 152, "regained" + STR$(HP) + " hit")
          CALL PutText(8, 160, "points!")
          CALL ShowHPMP
          WHILE INKEY$ = "": WEND
        END IF
        HP = 0
      ELSEIF Choice = "i" THEN
        CALL PutText(8, 136, "Items have not yet been")
        CALL PutText(8, 144, "programmed.       ")
        WHILE INKEY$ = "": WEND
      ELSEIF Choice = "o" THEN
        CALL PutText(8, 136, "Options have not yet   ")
        CALL PutText(8, 144, "been programmed")
        WHILE INKEY$ = "": WEND
      ELSEIF Choice = "s" THEN
        CALL LoadSaveBox
        DO WHILE NOT MadeChoice
          Choice = LCASE$(INPUT$(1))
          SELECT CASE Choice
          CASE IS = "1"
            CALL EnterName
            CALL SaveStats
            Saved(1) = Name$
            MadeChoice = True
          CASE IS = "2"
            CALL EnterName
            CALL SaveStats
            Saved(2) = Name$
            MadeChoice = True
          CASE IS = "3"
            CALL EnterName
            CALL SaveStats
            Saved(3) = Name$
            MadeChoice = True
          CASE IS = "4"
            CALL EnterName
            CALL SaveStats
            Saved(4) = Name$
            MadeChoice = True
          CASE IS = "5"
            CALL EnterName
            CALL SaveStats
            Saved(5) = Name$
            MadeChoice = True
          CASE CHR$(27)
            MadeChoice = True
          END SELECT
        LOOP
        MadeChoice = False
        Done = True
      ELSEIF Choice = "q" THEN
        Done = True
        Quit = True
      ELSEIF Choice = " " THEN
        Done = True
    END IF
    CALL ClearPickBox
    CALL ClearDialog
  LOOP
  CALL VisiblePlane
  IF TempOn THEN CALL ShowPlayer
END SUB

SUB PutBack
  Visible(TempX, TempY) = Maze(PlayerX, PlayerY)
  IF Visible(TempX, TempY) = 1 THEN
      PUT ((TempX - 1) * 20, (TempY - 1) * 20), G01, PSET
    ELSEIF Visible(TempX, TempY) = 2 THEN
      PUT ((TempX - 1) * 20, (TempY - 1) * 20), G12, PSET
      PUT ((TempX - 1) * 20, (TempY - 1) * 20), M02, AND
      PUT ((TempX - 1) * 20, (TempY - 1) * 20), G02, XOR
    ELSEIF Visible(TempX, TempY) = 3 THEN
      PUT ((TempX - 1) * 20, (TempY - 1) * 20), G03, PSET
    ELSEIF Visible(TempX, TempY) = 4 THEN
      PUT ((TempX - 1) * 20, (TempY - 1) * 20), G04, PSET
  END IF
END SUB

SUB PutLong (Value&, PosX%, PosY%, Digits%)
  Sentence$ = RIGHT$(STR$(Value&), LEN(STR$(Value&)) - 1)
  IF Digits% - LEN(Sentence$) > 0 THEN
    Sentence$ = SPACE$(Digits% - LEN(Sentence$)) + Sentence$
  END IF
  FOR Csr = 1 TO LEN(Sentence$)
    ChrNumber = ASC(MID$(Sentence$, Csr, 1))
    IF ChrNumber > 32 THEN
      FOR Y = 1 TO 8
        FOR X = 1 TO 8
          IF ChrSet(ChrNumber, X, Y) <> 0 THEN PSET (PosX + (X - 1), PosY + (Y - 1)), ChrSet(ChrNumber, X, Y)
        NEXT X
      NEXT Y
    END IF
    PosX = PosX + 8
    IF PosX = 320 THEN
      PosX = 0
      PosY = PosY + 8
    END IF
  NEXT Csr
END SUB

SUB PutNumber (Value%, PosX%, PosY%, Digits%)
  Sentence$ = RIGHT$(STR$(Value%), LEN(STR$(Value%)) - 1)
  IF Digits% - LEN(Sentence$) > 0 THEN
    Sentence$ = SPACE$(Digits% - LEN(Sentence$)) + Sentence$
  END IF
  FOR Csr = 1 TO LEN(Sentence$)
    ChrNumber = ASC(MID$(Sentence$, Csr, 1))
    IF ChrNumber > 32 THEN
      FOR Y = 1 TO 8
        FOR X = 1 TO 8
          IF ChrSet(ChrNumber, X, Y) <> 0 THEN PSET (PosX + (X - 1), PosY + (Y - 1)), ChrSet(ChrNumber, X, Y)
        NEXT X
      NEXT Y
    END IF
    PosX = PosX + 8
    IF PosX = 320 THEN
      PosX = 0
      PosY = PosY + 8
    END IF
    IF TextScroll THEN Delay (Speed(2))
  NEXT Csr
END SUB

SUB PutText (PosX%, PosY%, Sentence$)
  FOR Csr = 1 TO LEN(Sentence$)
    ChrNumber = ASC(MID$(Sentence$, Csr, 1))
    IF ChrNumber > 32 THEN
      FOR Y = 1 TO 8
        FOR X = 1 TO 8
          IF ChrSet(ChrNumber, X, Y) <> 0 THEN PSET (PosX + (X - 1), PosY + (Y - 1)), ChrSet(ChrNumber, X, Y)
        NEXT X
      NEXT Y
    END IF
    PosX = PosX + 8
    IF PosX = 320 THEN
      PosX = 0
      PosY = PosY + 8
    END IF
    IF TextScroll THEN Delay (Speed(2))
  NEXT Csr
END SUB

SUB Rain
  FOR I = 1 TO NumDrops * 2
    IF I < NumDrops THEN
        DropType(I) = 11
      ELSE
        DropType(I) = 27
    END IF
    DropX(I) = INT(RND * 171) + 14
    DropY(I) = INT(RND * 104) + 7
    BackColour(I) = POINT(DropX(I), DropY(I))
    PSET (DropX(I), DropY(I)), DropType(I)
  NEXT I

  FOR J = 1 TO 50
    Counter = Counter + 1
    FOR I = 1 TO NumDrops * 2
      PSET (DropX(I), DropY(I)), BackColour(I)
      IF I < NumDrops OR Counter = 3 THEN
        DropY(I) = DropY(I) + 4
        IF DropY(I) >= 100 AND DropType(I) = 27 THEN
            DropX(I) = INT(RND * 171) + 14
            DropY(I) = 7
          ELSEIF DropY(I) >= 108 AND DropType(I) = 11 THEN
            DropX(I) = INT(RND * 171) + 14
            DropY(I) = 7
        END IF
      END IF
      BackColour(I) = POINT(DropX(I), DropY(I))
      PSET (DropX(I), DropY(I)), DropType(I)
    NEXT I
    IF Counter = 3 THEN Counter = 0
  NEXT J
END SUB

SUB SaveStats
  OPEN Name$ FOR OUTPUT AS #1
    WRITE #1, VisibleX, VisibleY, PlayerDir, HP(1), HP(2), MP(1), MP(2), CatFood, Item(1), Item(2)
    WRITE #1, Item(3), Item(4), Weapon, Armour, PlayerLevel, Ex, Strength, Defense, MagicStrength, NextLevel
    WRITE #1, ForNextLevel, Cat, TempX, TempY, TempOn, AnimateBoss, WeaponType, ArmourType, Magic(1), Magic(2)
    WRITE #1, Magic(3), Magic(4), Magic(5), MusicType, PlayerX, PlayerY, CurrentLevel, BossBeat
  CLOSE #1
  OPEN Name$ + ".DAT" FOR OUTPUT AS #1
  FOR I = 0 TO 60
    WRITE #1, Chest(I)
  NEXT I
  CLOSE #1
END SUB

SUB SetUpBoss
  IF BossType = 1 THEN
      DEF SEG = VARSEG(EnemyPic(0)): BLOAD "GRASS.BSG", VARPTR(EnemyPic(0))
      PUT (10, 3), EnemyPic, PSET
      LINE (10, 3)-(189, 115), 205, B
      LINE (11, 4)-(188, 114), 109, B
      LINE (12, 5)-(187, 113), 35, B
      LINE (13, 6)-(186, 112), 109, B
      DEF SEG = VARSEG(EnemyMask(0)): BLOAD "BOSS1.MSK", VARPTR(EnemyMask(0))
      DEF SEG = VARSEG(EnemyPic(0)): BLOAD "BOSS1.BSG", VARPTR(EnemyPic(0))
      PUT (40, 20), EnemyMask, AND
      PUT (40, 20), EnemyPic, XOR
    ELSEIF BossType = 2 THEN
      DEF SEG = VARSEG(EnemyPic(0)): BLOAD "CASTLE.BSG", VARPTR(EnemyPic(0))
      PUT (10, 3), EnemyPic, PSET
      LINE (10, 3)-(189, 115), 205, B
      LINE (11, 4)-(188, 114), 109, B
      LINE (12, 5)-(187, 113), 35, B
      LINE (13, 6)-(186, 112), 109, B
      DEF SEG = VARSEG(EnemyMask(0)): BLOAD "MAGNUS.MSK", VARPTR(EnemyMask(0))
      DEF SEG = VARSEG(EnemyPic(0)): BLOAD "MAGNUS.BSG", VARPTR(EnemyPic(0))
      PUT (45, 30), EnemyMask, AND
      PUT (45, 30), EnemyPic, XOR
  END IF
END SUB

SUB SetUpEnemy
  DEF SEG = VARSEG(EnemyPic(0)): BLOAD "GRASS.BSG", VARPTR(EnemyPic(0))
  PUT (10, 3), EnemyPic, PSET
  LINE (10, 3)-(189, 115), 205, B
  LINE (11, 4)-(188, 114), 109, B
  LINE (12, 5)-(187, 113), 35, B
  LINE (13, 6)-(186, 112), 109, B
  IF EnemyType = 1 THEN
      DEF SEG = VARSEG(EnemyMask(0)): BLOAD "BUNNY.MSK", VARPTR(EnemyMask(0))
      DEF SEG = VARSEG(EnemyPic(0)): BLOAD "BUNNY.BSG", VARPTR(EnemyPic(0))
      PUT (60, 25), EnemyMask, AND
      PUT (60, 25), EnemyPic, XOR
    ELSEIF EnemyType = 2 THEN
      OUT &H3C8, 36: OUT &H3C9, 31: OUT &H3C9, 31: OUT &H3C9, 63
      OUT &H3C8, 37: OUT &H3C9, 0: OUT &H3C9, 16: OUT &H3C9, 63
      DEF SEG = VARSEG(EnemyMask(0)): BLOAD "BUNNY.MSK", VARPTR(EnemyMask(0))
      DEF SEG = VARSEG(EnemyPic(0)): BLOAD "BUNNY.BSG", VARPTR(EnemyPic(0))
      PUT (60, 25), EnemyMask, AND
      PUT (60, 25), EnemyPic, XOR
    ELSEIF EnemyType = 3 THEN
      OUT &H3C8, 36: OUT &H3C9, 63: OUT &H3C9, 63: OUT &H3C9, 0
      OUT &H3C8, 37: OUT &H3C9, 63: OUT &H3C9, 47: OUT &H3C9, 0
      DEF SEG = VARSEG(EnemyMask(0)): BLOAD "BUNNY.MSK", VARPTR(EnemyMask(0))
      DEF SEG = VARSEG(EnemyPic(0)): BLOAD "BUNNY.BSG", VARPTR(EnemyPic(0))
      PUT (60, 25), EnemyMask, AND
      PUT (60, 25), EnemyPic, XOR
    ELSEIF EnemyType = 4 THEN
      DEF SEG = VARSEG(EnemyMask(0)): BLOAD "PORCUPIN.MSK", VARPTR(EnemyMask(0))
      DEF SEG = VARSEG(EnemyPic(0)): BLOAD "PORCUPIN.BSG", VARPTR(EnemyPic(0))
      PUT (50, 45), EnemyMask, AND
      PUT (50, 45), EnemyPic, XOR
    ELSEIF EnemyType = 5 THEN
      DEF SEG = VARSEG(EnemyMask(0)): BLOAD "TURTLE.MSK", VARPTR(EnemyMask(0))
      DEF SEG = VARSEG(EnemyPic(0)): BLOAD "TURTLE.BSG", VARPTR(EnemyPic(0))
      PUT (45, 50), EnemyMask, AND
      PUT (45, 50), EnemyPic, XOR
    ELSEIF EnemyType = 6 THEN
      DEF SEG = VARSEG(EnemyMask(0)): BLOAD "SPIDER.MSK", VARPTR(EnemyMask(0))
      DEF SEG = VARSEG(EnemyPic(0)): BLOAD "SPIDER.BSG", VARPTR(EnemyPic(0))
      PUT (25, 35), EnemyMask, AND
      PUT (25, 35), EnemyPic, XOR
  END IF
END SUB

SUB Shake (Duration)
  ' Reprogram for 1ms timer ticks
  BaseFrequency& = 1193181
  Interval = BaseFrequency& / 1000

  OUT &H43, &H36
  OUT &H40, Interval
  OUT &H40, Interval \ 256

  ' Switch from scan doubling to an explicit repeat count, allowing individual repetitions to skipped
  OUT &H3D4, 9: OUT &H3D5, 1

  FOR X = 1 TO Duration
    OUT &H3D4, 8: OUT &H3D5, X AND 127
    T! = TIMER: WHILE T! = TIMER: WEND
  NEXT X
  OUT &H3D4, 8: OUT &H3D5, 0

  ' Restore the 18.2Hz timer rate
  OUT &H43, &H36
  OUT &H40, 0
  OUT &H40, 0
END SUB

SUB Shop
  LINE (3, 123)-(196, 188), 205, B
  LINE (4, 124)-(195, 187), 109, B
  LINE (5, 125)-(194, 186), 35, B
  LINE (6, 126)-(193, 185), 109, B
  CALL ClearDialog
  CALL ClearPickBox
  CALL ShowStats
  IF Speed(2) > 0 THEN TextScroll = True
  CALL PutText(8, 136, "MANNIE: Mew, hi Lianne!")
  CALL PutText(8, 144, "Let me heal you, meow. ")
  TextScroll = False
  HP(1) = HP(2)
  MP(1) = MP(2)
  WHILE INKEY$ = "": WEND
  CALL ShowHPMP
  IF Speed(2) > 0 THEN TextScroll = True
  CALL PutText(8, 160, "MANNIE: Meow, all done!")
  CALL PutText(8, 168, "What would you like?   ")
  TextScroll = False
  DO UNTIL Leave
    CALL PutText(232, 136, "(B)UY")
    CALL PutText(232, 144, "(S)ELL")
    CALL PutText(232, 152, "(L)EAVE")
    DO WHILE NOT MadeChoice
      Choice = LCASE$(INPUT$(1))
      SELECT CASE Choice
      CASE IS = "b", "s", "l"
        MadeChoice = True
      END SELECT
    LOOP
    MadeChoice = False
    IF Choice = "b" THEN
        CALL ClearDialog
        CALL ClearPickBox
        IF Speed(2) > 0 THEN TextScroll = True
        CALL PutText(8, 136, "MANNIE: Meow, what     ")
        CALL PutText(8, 144, "would you like to buy? ")
        TextScroll = False
        CALL PutText(208, 136, "1 Choc    10")
        CALL PutText(208, 144, "2 Br CD  100")
        CALL PutText(208, 152, "3 Br Vest 75")
        CALL PutText(208, 168, "0 No Thanks ")
        DO WHILE NOT MadeChoice
          Choice = LCASE$(INPUT$(1))
          SELECT CASE Choice
          CASE IS = "0"
            MadeChoice = True
          CASE IS = "1"
            NeedCatFood = 10
            MadeChoice = True
          CASE IS = "2"
            NeedCatFood = 100
            MadeChoice = True
          CASE IS = "3"
            NeedCatFood = 75
            MadeChoice = True
          END SELECT
        LOOP
        MadeChoice = False
        IF Choice = "2" AND Weapon = 2 THEN
            CALL ClearDialog
            IF Speed(2) > 0 THEN TextScroll = True
            CALL PutText(8, 136, "MANNIE: You already   ")
            CALL PutText(8, 144, "have that!            ")
            TextScroll = False
            HaveIt = True
            WHILE INKEY$ = "": WEND
          ELSEIF Choice = "3" AND Armour = 2 THEN
            CALL ClearDialog
            IF Speed(2) > 0 THEN TextScroll = True
            CALL PutText(8, 136, "MANNIE: You already   ")
            CALL PutText(8, 144, "have that!            ")
            TextScroll = False
            HaveIt = True
            WHILE INKEY$ = "": WEND
        END IF
        IF CatFood < NeedCatFood AND NOT HaveIt THEN
          CALL ClearDialog
          IF Speed(2) > 0 THEN TextScroll = True
          CALL PutText(8, 136, "MANNIE: You don't have")
          CALL PutText(8, 144, "enough cat food for   ")
          CALL PutText(8, 152, "that!                 ")
          TextScroll = False
          WHILE INKEY$ = "": WEND
        END IF
        IF Choice = "1" AND CatFood > NeedCatFood THEN
            Item(1) = Item(1) + 1
            CatFood = CatFood - 10
            CALL ShowStats
          ELSEIF Choice = "2" AND CatFood > NeedCatFood AND NOT HaveIt THEN
            WeaponType = "bronze"
            Weapon = 2
            Strength = Strength + 2
            CatFood = CatFood - 100
            CALL ShowStats
          ELSEIF Choice = "3" AND CatFood > NeedCatFood AND NOT HaveIt THEN
            ArmourType = "bronze"
            Armour = 2
            Defense = Defense + 2
            CatFood = CatFood - 75
            CALL ShowStats
        END IF
      ELSEIF Choice = "s" THEN
        IF Speed(2) > 0 THEN TextScroll = True
        CALL PutText(8, 136, "MANNIE: I'm sorry, but ")
        CALL PutText(8, 144, "this feature has not   ")
        CALL PutText(8, 152, "yet been programmmed.  ")
        TextScroll = False
        WHILE INKEY$ = "": WEND
      ELSEIF Choice = "l" THEN
        Leave = True
    END IF
    CALL ClearDialog
    CALL ClearPickBox
    IF Speed(2) > 0 THEN TextScroll = True
    IF NOT Leave THEN CALL PutText(8, 136, "MANNIE: Anything else?")
    TextScroll = False
    HaveIt = False
  LOOP
  Talk = ""
  CALL VisiblePlane
  IF TempOn THEN CALL ShowPlayer
END SUB

SUB ShowHPMP
  LINE (256, 24)-(280, 40), BackGround, BF
  IF HP(1) > 0 THEN
      CALL PutNumber(HP(1), 256, 24, 3)
    ELSE
      HP(1) = 0
      CALL PutNumber(HP(1), 256, 24, 3)
  END IF
  CALL PutNumber(MP(1), 256, 32, 3)
END SUB

SUB ShowPlayer
  IF NOT TempOn AND PlayerX > 7 AND PlayerY > 4 AND PlayerX < 43 AND PlayerY < 46 THEN
    TempX = InitPosX
    TempY = InitPosY
  END IF
  IF Move = 0 THEN
      IF PlayerDir$ = "S" THEN
          PUT ((TempX - 1) * 20, (TempY - 1) * 20), Down1Mask, AND
          PUT ((TempX - 1) * 20, (TempY - 1) * 20), Down1, XOR
        ELSEIF PlayerDir$ = "N" THEN
          PUT ((TempX - 1) * 20, (TempY - 1) * 20), Up1Mask, AND
          PUT ((TempX - 1) * 20, (TempY - 1) * 20), Up1, XOR
        ELSEIF PlayerDir$ = "E" THEN
          PUT ((TempX - 1) * 20, (TempY - 1) * 20), Right1Mask, AND
          PUT ((TempX - 1) * 20, (TempY - 1) * 20), Right1, XOR
        ELSEIF PlayerDir$ = "W" THEN
          PUT ((TempX - 1) * 20, (TempY - 1) * 20), Left1Mask, AND
          PUT ((TempX - 1) * 20, (TempY - 1) * 20), Left1, XOR
      END IF
    ELSE
      IF PlayerDir$ = "S" THEN
          PUT ((TempX - 1) * 20, (TempY - 1) * 20), Down2Mask, AND
          PUT ((TempX - 1) * 20, (TempY - 1) * 20), Down2, XOR
        ELSEIF PlayerDir$ = "N" THEN
          PUT ((TempX - 1) * 20, (TempY - 1) * 20), Up2Mask, AND
          PUT ((TempX - 1) * 20, (TempY - 1) * 20), Up2, XOR
        ELSEIF PlayerDir$ = "E" THEN
          PUT ((TempX - 1) * 20, (TempY - 1) * 20), Right2Mask, AND
          PUT ((TempX - 1) * 20, (TempY - 1) * 20), Right2, XOR
        ELSEIF PlayerDir$ = "W" THEN
          PUT ((TempX - 1) * 20, (TempY - 1) * 20), Left2Mask, AND
          PUT ((TempX - 1) * 20, (TempY - 1) * 20), Left2, XOR
      END IF
  END IF
END SUB

SUB ShowStats
  LINE (207, 7)-(311, 111), BackGround, BF
  LINE (203, 3)-(315, 115), 205, B
  LINE (204, 4)-(314, 114), 109, B
  LINE (205, 5)-(313, 113), 35, B
  LINE (206, 6)-(312, 112), 109, B
  CALL PutText(232, 8, "LIANNE")
  CALL PutText(208, 24, "HP")
  CALL PutText(208, 32, "MP")
  CALL PutText(208, 48, "ST")
  CALL PutText(208, 56, "DF")
  CALL PutText(208, 64, "MG")
  CALL PutText(208, 72, "LV")
  CALL PutText(208, 88, "EX")
  CALL PutText(208, 96, "Next")
  CALL PutText(208, 104, "CatFood")
  CALL PutNumber(HP(1), 256, 24, 3)
  CALL PutText(280, 24, "/")
  CALL PutNumber(HP(2), 288, 24, 3)
  CALL PutNumber(MP(1), 256, 32, 3)
  CALL PutText(280, 32, "/")
  CALL PutNumber(MP(2), 288, 32, 3)
  CALL PutNumber(Strength, 296, 48, 2)
  CALL PutNumber(Defense, 296, 56, 2)
  CALL PutNumber(MagicStrength, 296, 64, 2)
  CALL PutNumber(PlayerLevel, 296, 72, 2)
  CALL PutLong(Ex, 264, 88, 6)
  CALL PutNumber(ForNextLevel, 264, 96, 6)
  CALL PutLong(CatFood, 272, 104, 5)
  LINE (203, 123)-(315, 188), 205, B
  LINE (204, 124)-(314, 187), 109, B
  LINE (205, 125)-(313, 186), 35, B
  LINE (206, 126)-(312, 185), 109, B
END SUB

SUB ShowStory
  K1X = 6: K1Y = 2
  K2X = 9: K2Y = 2
  LordX = 7: LordY = 4
 
  IF Speed(2) > 0 THEN TextScroll = True
  CALL DialogBox
  CALL PutText(56, 136, "The Crown of Kissworld is ")
  CALL PutText(56, 144, "said to have great powers.")
  WHILE INKEY$ = "": WEND
  CALL PutText(56, 152, "Whomever wears it, rules  ")
  CALL PutText(56, 160, "the land.                 ")
  WHILE INKEY$ = "": WEND
  CALL PutText(56, 168, "A great temptation for    ")
  CALL PutText(56, 176, "evil.  But Kissworld has  ")
  WHILE INKEY$ = "": WEND
  CALL DialogBox
  CALL PutText(56, 136, "been at peace since the   ")
  CALL PutText(56, 144, "Wizard Vincent fell at the")
  CALL PutText(56, 152, "hands of Lianne.          ")
  WHILE INKEY$ = "": WEND
  CALL PutText(56, 160, "Until now...              ")
  WHILE INKEY$ = "": WEND
 
  CALL ShowTiles
  DEF SEG = VARSEG(Character1(0)): BLOAD "KNIGHT.TIL", VARPTR(Character1(0))
  DEF SEG = VARSEG(Mask1(0)): BLOAD "KNIGHT.MSK", VARPTR(Mask1(0))
  PUT (K1X * 20, K1Y * 20), Mask1, AND
  PUT (K1X * 20, K1Y * 20), Character1, XOR

  PUT (K2X * 20, K2Y * 20), Mask1, AND
  PUT (K2X * 20, K2Y * 20), Character1, XOR

  DEF SEG = VARSEG(Character1(0)): BLOAD "LORD1.TIL", VARPTR(Character1(0))
  DEF SEG = VARSEG(Mask1(0)): BLOAD "LORD1.MSK", VARPTR(Mask1(0))
  PUT (LordX * 20, LordY * 20), Mask1, AND
  PUT (LordX * 20, LordY * 20), Character1, XOR

  FOR I = 1 TO 5
    CALL FloatCrown
    CALL Delay(200)
  NEXT I

  CALL Shake(2000)

  LINE (140, 20)-(179, 39), 0, BF
  StoryMap(8, 2) = 0: StoryMap(9, 2) = 0
  PUT (K1X * 20, K1Y * 20), ST01, PSET
  PUT (K2X * 20, K2Y * 20), ST01, PSET
  K1X = K1X - 1: K2X = K2X + 1
  DEF SEG = VARSEG(Character1(0)): BLOAD "KNIGHT1.TIL", VARPTR(Character1(0))
  DEF SEG = VARSEG(Mask1(0)): BLOAD "KNIGHT1.MSK", VARPTR(Mask1(0))
  PUT (K1X * 20, K1Y * 20), Mask1, AND
  PUT (K1X * 20, K1Y * 20), Character1, XOR
  PUT (K2X * 20, K2Y * 20), Mask1, AND
  PUT (K2X * 20, K2Y * 20), Character1, XOR
  CALL MoveCain

  CALL DialogBox
  CALL PutText(56, 136, "CAIN: Ah, good day my dear")
  CALL PutText(56, 144, "Lord Frehley.             ")
  WHILE INKEY$ = "": WEND
  CALL PutText(56, 152, "FREHLEY: Who are you?     ")
  WHILE INKEY$ = "": WEND
  CALL PutText(56, 160, "CAIN: My name is Baron Von")
  CALL PutText(56, 168, "Cain and I want the crown.")
  WHILE INKEY$ = "": WEND
  CALL PutText(56, 176, "FREHLEY: Stop! You cannot ")
  WHILE INKEY$ = "": WEND
  CALL DialogBox
  CALL PutText(56, 136, "take the crown!           ")
  WHILE INKEY$ = "": WEND
  CALL PutText(56, 144, "CAIN: Step aside and let  ")
  CALL PutText(56, 152, "me have it.               ")
  WHILE INKEY$ = "": WEND
  CALL PutText(56, 160, "FREHLEY: No! Never!       ")
  WHILE INKEY$ = "": WEND
  PUT (LordX * 20, LordY * 20), ST02, PSET
  LordX = 8
  DEF SEG = VARSEG(Character1(0)): BLOAD "LORD2.TIL", VARPTR(Character1(0))
  DEF SEG = VARSEG(Mask1(0)): BLOAD "LORD2.MSK", VARPTR(Mask1(0))
  PUT (LordX * 20, LordY * 20), Mask1, AND
  PUT (LordX * 20, LordY * 20), Character1, XOR
  CALL PutText(56, 168, "AAAAAARRRRRGGGGHHHHHHHHH! ")
  WHILE INKEY$ = "": WEND
  CALL PutText(56, 176, "CAIN: Then die old man!   ")
  WHILE INKEY$ = "": WEND
  DEF SEG = VARSEG(Character1(0)): BLOAD "CAIN1.TIL", VARPTR(Character1(0))
  DEF SEG = VARSEG(Character2(0)): BLOAD "CAIN2.TIL", VARPTR(Character2(0))
  DEF SEG = VARSEG(Mask1(0)): BLOAD "CAIN1.MSK", VARPTR(Mask1(0))
  DEF SEG = VARSEG(Mask2(0)): BLOAD "CAIN2.MSK", VARPTR(Mask2(0))
  PUT (140, 60), ST02, PSET
  PUT (140, 80), Mask1, AND
  PUT (140, 80), Character1, XOR
  CALL Delay(200)
  PUT (140, 80), ST02, PSET
  PUT (140, 80), Mask2, AND
  PUT (140, 80), Character2, XOR
  CALL Delay(200)
  PUT (140, 80), ST02, PSET
  PUT (140, 80), Mask1, AND
  PUT (140, 80), Character1, XOR
  CALL Delay(200)
  PUT (140, 80), ST02, PSET
  PUT (140, 80), Mask2, AND
  PUT (140, 80), Character2, XOR
  CALL Delay(200)
  PUT (140, 100), ST02, PSET
  CALL DialogBox
  CALL PutText(56, 136, "CAIN: At last.  The Crown ")
  CALL PutText(56, 144, "of Kissworld is mine! Let ")
  CALL PutText(56, 152, "the coronation begin!     ")
  WHILE INKEY$ = "": WEND
  CALL PutText(56, 160, "No.  I should return to my")
  CALL PutText(56, 168, "island.  I shall have a   ")
  CALL PutText(56, 176, "proper ceremony there! HA!")
  WHILE INKEY$ = "": WEND
  CALL DialogBox
  CALL PutText(56, 136, "CAIN: Kissworld is mine!  ")
  PUT (140, 80), ST02, PSET
  PUT (140, 80), Mask2, AND
  PUT (140, 80), Character2, XOR
  CALL Delay(200)
  PUT (140, 80), ST02, PSET
  CALL Delay(200)
  PUT (140, 80), ST02, PSET
  PUT (140, 80), Mask2, AND
  PUT (140, 80), Character2, XOR
  CALL Delay(200)
  PUT (140, 80), ST02, PSET
  CALL Delay(200)
  PUT (140, 80), ST02, PSET
  PUT (140, 80), Mask2, AND
  PUT (140, 80), Character2, XOR
  CALL Delay(200)
  PUT (140, 80), ST02, PSET
  CALL Delay(200)
  CALL DialogBox
  CALL PutText(56, 136, "...:Father! What's going  ")
  CALL PutText(56, 144, "on?                       ")
  WHILE INKEY$ = "": WEND
  CALL PutText(56, 156, "FREHLEY: Son... Miles?    ")
  WHILE INKEY$ = "": WEND
  DEF SEG = VARSEG(Character1(0)): BLOAD "MILESD1.TIL", VARPTR(Character1(0))
  DEF SEG = VARSEG(Character2(0)): BLOAD "MILESD2.TIL", VARPTR(Character2(0))
  DEF SEG = VARSEG(Mask1(0)): BLOAD "MILESD1.MSK", VARPTR(Mask1(0))
  DEF SEG = VARSEG(Mask2(0)): BLOAD "MILESD2.MSK", VARPTR(Mask2(0))
  MilesY = 1
  PUT (LordX * 20, MilesY * 20), Mask1, AND
  PUT (LordX * 20, MilesY * 20), Character1, XOR
  Delay (150)
  LINE (LordX * 20, MilesY * 20)-(LordX * 20 + 19, MilesY * 20 + 19), 0, BF
  MilesY = 2
  PUT (LordX * 20, MilesY * 20), Mask2, AND
  PUT (LordX * 20, MilesY * 20), Character2, XOR
  Delay (150)
  PUT (LordX * 20, MilesY * 20), ST02, PSET
  MilesY = 3
  PUT (LordX * 20, MilesY * 20), Mask1, AND
  PUT (LordX * 20, MilesY * 20), Character1, XOR
  CALL DialogBox
  CALL PutText(56, 136, "MILES: Father! Are you... ")
  CALL PutText(56, 144, "FREHLEY: I'm dying son.   ")
  CALL PutText(56, 152, "You must find Lianne and  ")
  CALL PutText(56, 160, "ask for her help.  She is ")
  CALL PutText(56, 168, "the only one who can get  ")
  CALL PutText(56, 176, "the Crown back from Cain. ")
  WHILE INKEY$ = "": WEND
  CALL DialogBox
  CALL PutText(56, 136, "MILES: Baron Cain took the")
  CALL PutText(56, 144, "Crown?                    ")
  WHILE INKEY$ = "": WEND
  CALL PutText(56, 152, "FREHLEY: You know of him? ")
  WHILE INKEY$ = "": WEND
  CALL PutText(56, 160, "MILES: Well, yes.  The    ")
  CALL PutText(56, 168, "Kiss Army has been keeping")
  CALL PutText(56, 176, "an eye on him for the past")
  WHILE INKEY$ = "": WEND
  CALL DialogBox
  CALL PutText(56, 136, "year or so.  He's been    ")
  CALL PutText(56, 144, "causing some trouble but  ")
  CALL PutText(56, 152, "we didn't think he would  ")
  CALL PutText(56, 160, "do this!                  ")
  WHILE INKEY$ = "": WEND
  CALL PutText(56, 168, "FREHLEY: It's... not your ")
  CALL PutText(56, 176, "... fault son.            ")
  WHILE INKEY$ = "": WEND
  CALL DialogBox
  CALL PutText(56, 136, "MILES: Father! Do you know")
  CALL PutText(56, 144, "where he could be?        ")
  WHILE INKEY$ = "": WEND
  CALL PutText(56, 152, "FREHLEY: His island... He ")
  CALL PutText(56, 160, "said... coronation...     ")
  WHILE INKEY$ = "": WEND
  CALL PutText(56, 168, "MILES: Father no!         ")
  WHILE INKEY$ = "": WEND
  CALL PutText(56, 176, "FREHLEY: ...              ")
  WHILE INKEY$ = "": WEND
  CALL DialogBox
  CALL PutText(56, 136, "MILES: No! Cain, I will   ")
  CALL PutText(56, 144, "make you pay!             ")
  WHILE INKEY$ = "": WEND
  CALL BoxOut
  CALL DialogBox
  CALL PutText(56, 136, "Upon hearing what had     ")
  CALL PutText(56, 144, "happened, Lianne agreed to")
  CALL PutText(56, 152, "help defeat Cain and get  ")
  CALL PutText(56, 160, "the crown back.           ")
  WHILE INKEY$ = "": WEND
  CALL PutText(56, 168, "The next day, a ship set  ")
  CALL PutText(56, 176, "sail from Kissworld to    ")
  WHILE INKEY$ = "": WEND
  CALL DialogBox
  CALL PutText(56, 136, "Baron Cain's island, with ")
  CALL PutText(56, 144, "Lianne, Miles, and some of")
  CALL PutText(56, 152, "the Kiss Army on board.   ")
  WHILE INKEY$ = "": WEND
  TextScroll = False
END SUB

SUB ShowStory2
  DEF SEG = VARSEG(Character1(0)): BLOAD "MILESD1.TIL", VARPTR(Character1(0))
  DEF SEG = VARSEG(Character2(0)): BLOAD "MILESD2.TIL", VARPTR(Character2(0))
  DEF SEG = VARSEG(Mask1(0)): BLOAD "MILESD1.MSK", VARPTR(Mask1(0))
  DEF SEG = VARSEG(Mask2(0)): BLOAD "MILESD2.MSK", VARPTR(Mask2(0))
  PUT ((PlayerX - 2) * 20, (PlayerY - 1) * 20), Mask1, AND
  PUT ((PlayerX - 2) * 20, (PlayerY - 1) * 20), Character1, XOR
  CALL DialogBox
  IF Speed(2) > 0 THEN TextScroll = True
  CALL PutText(56, 136, "MILES: Some of the Knights")
  CALL PutText(56, 144, "have already gone into the")
  CALL PutText(56, 152, "castle.  There are some   ")
  CALL PutText(56, 160, "things I need to tell you ")
  CALL PutText(56, 168, "before I go ahead as well.")
  WHILE INKEY$ = "": WEND
  CALL PutText(56, 176, "You will find cats on all ")
  WHILE INKEY$ = "": WEND
  CALL DialogBox
  CALL PutText(56, 136, "of the levels.  They will ")
  CALL PutText(56, 144, "help you out, but only if ")
  CALL PutText(56, 152, "you have cat food.  The   ")
  CALL PutText(56, 160, "enemies tend to drop some ")
  CALL PutText(56, 168, "when defeated, remember   ")
  CALL PutText(56, 176, "that.                     ")
  WHILE INKEY$ = "": WEND
  CALL DialogBox
  CALL PutText(56, 136, "Cain also has lots of     ")
  CALL PutText(56, 144, "treasure so if you see a  ")
  CALL PutText(56, 152, "wooden chest, try to open ")
  CALL PutText(56, 160, "it.  There should be some ")
  CALL PutText(56, 168, "goodies inside.           ")
  WHILE INKEY$ = "": WEND
  CALL PutText(56, 176, "I guess that's about it.  ")
  WHILE INKEY$ = "": WEND
  CALL DialogBox
  CALL PutText(56, 136, "MILES: Good luck!         ")
  WHILE INKEY$ = "": WEND
  CALL PutText(56, 144, "LIANNE: Thanks.  Let's go!")
  WHILE INKEY$ = "": WEND
  CALL VisiblePlane
  CALL ShowPlayer
  PUT ((PlayerX - 2) * 20, (PlayerY - 1) * 20), Mask1, AND
  PUT ((PlayerX - 2) * 20, (PlayerY - 1) * 20), Character1, XOR
  Delay (150)
  PUT ((PlayerX - 2) * 20, (PlayerY - 1) * 20), G01, PSET
  PUT ((PlayerX - 2) * 20, PlayerY * 20), Mask2, AND
  PUT ((PlayerX - 2) * 20, PlayerY * 20), Character2, XOR
  Delay (150)
  PUT ((PlayerX - 2) * 20, PlayerY * 20), G01, PSET
  PUT ((PlayerX - 2) * 20, (PlayerY + 1) * 20), Mask1, AND
  PUT ((PlayerX - 2) * 20, (PlayerY + 1) * 20), Character1, XOR
  Delay (150)
  PUT ((PlayerX - 2) * 20, (PlayerY + 1) * 20), G01, PSET
  PUT ((PlayerX - 2) * 20, (PlayerY + 2) * 20), Mask2, AND
  PUT ((PlayerX - 2) * 20, (PlayerY + 2) * 20), Character2, XOR
  Delay (150)
  PUT ((PlayerX - 2) * 20, (PlayerY + 2) * 20), G01, PSET
  PUT ((PlayerX - 2) * 20, (PlayerY + 3) * 20), Mask1, AND
  PUT ((PlayerX - 2) * 20, (PlayerY + 3) * 20), Character1, XOR
  Delay (150)
  PUT ((PlayerX - 2) * 20, (PlayerY + 3) * 20), G04, PSET
  PUT ((PlayerX - 2) * 20, (PlayerY + 4) * 20), Mask2, AND
  PUT ((PlayerX - 2) * 20, (PlayerY + 4) * 20), Character2, XOR
  Delay (150)
  PUT ((PlayerX - 2) * 20, (PlayerY + 4) * 20), G04, PSET
  PUT ((PlayerX - 2) * 20, (PlayerY + 5) * 20), Mask2, AND
  PUT ((PlayerX - 2) * 20, (PlayerY + 5) * 20), Character2, XOR
  Delay (150)
  PUT ((PlayerX - 2) * 20, (PlayerY + 5) * 20), G04, PSET
  TextScroll = False
END SUB

SUB ShowTiles
  FOR X = 0 TO 15
    FOR Y = 0 TO 9
      IF StoryMap(X + 1, Y + 1) = 1 THEN
          PUT (X * 20, Y * 20), ST01, PSET
        ELSEIF StoryMap(X + 1, Y + 1) = 2 THEN
          PUT (X * 20, Y * 20), ST02, PSET
        ELSEIF StoryMap(X + 1, Y + 1) = 10 THEN
          PUT (X * 20, Y * 20), ST10, PSET
        ELSEIF StoryMap(X + 1, Y + 1) = 11 THEN
          PUT (X * 20, Y * 20), ST11, PSET
        ELSEIF StoryMap(X + 1, Y + 1) = 12 THEN
          PUT (X * 20, Y * 20), ST12, PSET
        ELSEIF StoryMap(X + 1, Y + 1) = 13 THEN
          PUT (X * 20, Y * 20), ST02, PSET
          PUT (X * 20, Y * 20), SM13, AND
          PUT (X * 20, Y * 20), ST13, XOR
          PUT (X * 20, Y * 20), SM14, AND
          PUT (X * 20, Y * 20), ST14, XOR
          CrownX = X: CrownY = Y
        END IF
    NEXT Y
  NEXT X
END SUB

SUB Stance
  IF Move = 0 THEN
      Move = 1
    ELSE
      Move = 0
  END IF
END SUB

SUB UpdateX
  OPEN "SAVES.DAT" FOR OUTPUT AS #1
  FOR I = 1 TO 5
    WRITE #1, Saved(I)
  NEXT I
  CLOSE #1
END SUB

SUB VisiblePlane
  A = -1: B = -1: Cat = 0: AnimateBoss = 0
  FOR Y = VisibleY TO VisibleY + 9
    B = B + 1
    IF B = 10 THEN B = 0
    FOR X = VisibleX TO VisibleX + 15
      A = A + 1
      IF A = 16 THEN A = 0
      Visible(A, B) = Maze(X, Y)
      IF Visible(A, B) = 1 THEN
          PUT (A * 20, B * 20), G01, PSET
        ELSEIF Visible(A, B) = 2 THEN
          PUT (A * 20, B * 20), G12, PSET
          PUT (A * 20, B * 20), M02, AND
          PUT (A * 20, B * 20), G02, XOR
        ELSEIF Visible(A, B) = 3 THEN
          PUT (A * 20, B * 20), G03, PSET
        ELSEIF Visible(A, B) = 4 THEN
          PUT (A * 20, B * 20), G04, PSET
        ELSEIF Visible(A, B) = 11 THEN
          PUT (A * 20, B * 20), G01, PSET
          PUT (A * 20, B * 20), M11, AND
          PUT (A * 20, B * 20), G11, XOR
        ELSEIF Visible(A, B) = 12 THEN
          PUT (A * 20, B * 20), G12, PSET
        ELSEIF Visible(A, B) = 13 THEN
          PUT (A * 20, B * 20), G13, PSET
        ELSEIF Visible(A, B) = 14 THEN
          PUT (A * 20, B * 20), G14, PSET
        ELSEIF Visible(A, B) = 15 THEN
          PUT (A * 20, B * 20), G15, PSET
        ELSEIF Visible(A, B) = 26 THEN
          PUT (A * 20, B * 20), G12, PSET
          PUT (A * 20, B * 20), M26, AND
          PUT (A * 20, B * 20), G26, XOR
        ELSEIF Visible(A, B) = 27 THEN
          PUT (A * 20, B * 20), G03, PSET
          PUT (A * 20, B * 20), M27, AND
          PUT (A * 20, B * 20), G27, XOR
        ELSEIF Visible(A, B) = 29 THEN
          Cat = 1
          PUT (A * 20, B * 20), G01, PSET
          CatXPos = A * 20: CatYPos = B * 20
          IF Animation = 0 THEN
              PUT (A * 20, B * 20), M29A1, AND
              PUT (A * 20, B * 20), G29A1, XOR
            ELSE
              PUT (A * 20, B * 20), M29A2, AND
              PUT (A * 20, B * 20), G29A2, XOR
          END IF
        ELSEIF Visible(A, B) = 30 THEN
          AnimateBoss = 1
          PUT (A * 20, B * 20), G04, PSET
          BossXPos = A * 20: BossYPos = B * 20
          IF Animation = 0 THEN
              PUT (A * 20, B * 20), Boss1Mask, AND
              PUT (A * 20, B * 20), Boss1A, XOR
            ELSEIF Animation = 1 THEN
              PUT (A * 20, B * 20), Boss1Mask, AND
              PUT (A * 20, B * 20), Boss1B, XOR
            ELSEIF Animation = 2 THEN
              PUT (A * 20, B * 20), Boss1Mask, AND
              PUT (A * 20, B * 20), Boss1C, XOR
            ELSEIF Animation = 3 THEN
              PUT (A * 20, B * 20), Boss1Mask, AND
              PUT (A * 20, B * 20), Boss1B, XOR
          END IF
        ELSEIF Visible(A, B) = 38 THEN
          PUT (A * 20, B * 20), G38, PSET
        ELSEIF Visible(A, B) = 39 THEN
          PUT (A * 20, B * 20), G39, PSET
      END IF
    NEXT X
    IF B = 4 AND NOT TempOn THEN CALL ShowPlayer
  NEXT Y
END SUB


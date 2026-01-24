'Set default data type to integer for faster game play
DEFINT A-Z

'Constants
CONST SPEEDCONST = 500
CONST TRUE = -1
CONST FALSE = NOT TRUE
CONST HITSELF = 1
CONST BACKATTR = 0
CONST OBJECTCOLOR = 1
CONST WINDOWCOLOR = 14
CONST SUNATTR = 3
CONST SUNHAPPY = FALSE
CONST SUNSHOCK = TRUE
CONST RIGHTUP = 1
CONST LEFTUP = 2
CONST ARMSDOWN = 3

'Global Variables
DIM SHARED pi#
DIM SHARED LBan&(x), RBan&(x), UBan&(x), DBan&(x) 'Graphical picture of banana

  GOSUB InitVars

  SCREEN Mode
  SetScreen


  COLOR 15

  i& = 7

  DO
    RANDOMIZE i&

    x# = RND * 600 + 20
    y# = RND * 300 + 20
    r% = (RND * 10000) AND 3

    LOCATE 1, 1
    PRINT i&; x#; y#; r%

    DrawBan x#, y#, r%, TRUE
    DrawBan x#, y#, r%, FALSE

    WHILE INKEY$ = "": WEND

    i& = i& + 1
  LOOP
 
END


CGABanana:
  'BananaLeft
  DATA 327686, -252645316, 60
  'BananaDown
  DATA 196618, -1057030081, 49344
  'BananaUp
  DATA 196618, -1056980800, 63
  'BananaRight
  DATA 327686,  1010580720, 240

EGABanana:
  'BananaLeft
  DATA 458758,202116096,471604224,943208448,943208448,943208448,471604224,202116096,0
  'BananaDown
  DATA 262153, -2134835200, -2134802239, -2130771968, -2130738945,8323072, 8323199, 4063232, 4063294
  'BananaUp
  DATA 262153, 4063232, 4063294, 8323072, 8323199, -2130771968, -2130738945, -2134835200,-2134802239
  'BananaRight
  DATA 458758, -1061109760, -522133504, 1886416896, 1886416896, 1886416896,-522133504,-1061109760,0

InitVars:
  pi# = 4 * ATN(1#)

  'This is a clever way to pick the best graphics mode available
  ON ERROR GOTO ScreenModeError
  Mode = 9
  SCREEN Mode
  ON ERROR GOTO PaletteError
  IF Mode = 9 THEN PALETTE 4, 0   'Check for 64K EGA
  ON ERROR GOTO 0

  MachSpeed = 1

  IF Mode = 9 THEN
    ScrWidth = 640
    ScrHeight = 350
    GHeight = 25
    RESTORE EGABanana
    REDIM LBan&(8), RBan&(8), UBan&(8), DBan&(8)

    FOR i = 0 TO 8
      READ LBan&(i)
    NEXT i

    FOR i = 0 TO 8
      READ DBan&(i)
    NEXT i

    FOR i = 0 TO 8
      READ UBan&(i)
    NEXT i

    FOR i = 0 TO 8
      READ RBan&(i)
    NEXT i

    SunHt = 39

  ELSE

    ScrWidth = 320
    ScrHeight = 200
    GHeight = 12
    RESTORE CGABanana
    REDIM LBan&(2), RBan&(2), UBan&(2), DBan&(2)
    REDIM GorL&(20), GorD&(20), GorR&(20)

    FOR i = 0 TO 2
      READ LBan&(i)
    NEXT i
    FOR i = 0 TO 2
      READ DBan&(i)
    NEXT i
    FOR i = 0 TO 2
      READ UBan&(i)
    NEXT i
    FOR i = 0 TO 2
      READ RBan&(i)
    NEXT i

    MachSpeed = MachSpeed * 1.3
    SunHt = 20
  END IF
RETURN

ScreenModeError:
  IF Mode = 1 THEN
    CLS
    LOCATE 10, 5
    PRINT "Sorry, you must have CGA, EGA color, or VGA graphics to play GORILLA.BAS"
    END
  ELSE
    Mode = 1
    RESUME
  END IF

PaletteError:
  Mode = 1            '64K EGA cards will run in CGA mode.
  RESUME NEXT

'DrawBan:
'  Draws the banana
'Parameters:
'  xc# - Horizontal Coordinate
'  yc# - Vertical Coordinate
'  r - rotation position (0-3). (  \_/  ) /-\
'  bc - if TRUE then DrawBan draws the banana ELSE it erases the banana
SUB DrawBan (xc#, yc#, r, bc)

SELECT CASE r
  CASE 0
    IF bc THEN PUT (xc#, yc#), LBan&, PSET ELSE PUT (xc#, yc#), LBan&, XOR
  CASE 1
    IF bc THEN PUT (xc#, yc#), UBan&, PSET ELSE PUT (xc#, yc#), UBan&, XOR
  CASE 2
    IF bc THEN PUT (xc#, yc#), DBan&, PSET ELSE PUT (xc#, yc#), DBan&, XOR
  CASE 3
    IF bc THEN PUT (xc#, yc#), RBan&, PSET ELSE PUT (xc#, yc#), RBan&, XOR
END SELECT

END SUB

'SetScreen:
'  Sets the appropriate color statements
SUB SetScreen

  IF Mode = 9 THEN
    ExplosionColor = 2
    BackColor = 1
    PALETTE 0, 1
    PALETTE 1, 46
    PALETTE 2, 44
    PALETTE 3, 54
    PALETTE 5, 7
    PALETTE 6, 4
    PALETTE 7, 3
    PALETTE 9, 63       'Display Color
  ELSE
    ExplosionColor = 2
    BackColor = 0
    COLOR BackColor, 2

  END IF

END SUB

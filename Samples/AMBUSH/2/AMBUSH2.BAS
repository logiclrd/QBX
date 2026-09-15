
'                         Ambush at South Range II
        
'          (c) copyright 1995 by Molnar \ Kucalaba Productions
       
'     Let us know what you think about this game: Blood 225@aol.com

' If you do send us email, we will tell you about some of the numerous secret
'                        codes, areas, features.

'   We left all of the directory paths open, so all you need to do is:

'              1) Have all of the files in one directory.

'   2) Go down to line 52 and put that directory in the CHDIR statement.

'                             Good Luck!




DECLARE SUB SBPlay (zreg%, zuration%, zoctave%, zaveform%)
DECLARE SUB WriteReg (zeg%, zalue%)
DECLARE SUB Detectcard ()
DECLARE SUB SBinit ()
DECLARE SUB Difficulty (HARD!)
DECLARE SUB Back (sped, orw, words$)
DECLARE SUB Slide (speed!, row!, Tex$)
DECLARE SUB Legal ()
DECLARE SUB Display ()
DECLARE SUB SBPlay2 (zreg%, zuration%, zoctave%, zaveform%)
DECLARE SUB Fx ()
DECLARE SUB SBPlay3 (zreg%, zuration%, zoctave%, zaveform%)
DECLARE SUB Logo ()
DECLARE SUB Titlescreen ()
DECLARE SUB Sond (snd!)
DECLARE SUB Direct ()
DECLARE SUB Text (functnum)
DECLARE SUB Footer (Word$)
DECLARE SUB Bullethole (A!, B!)
COMMON BEGINNINGTIME, Q, A, B, BLOCKX, BLOCKY, ELAPSEDTIME, STAIRWAY, HEALTH
COMMON HEALTHMAX, ENEMIES, ROOM, ARCADE, DEATHS, snd, DETECTOR, STRENGTH
COMMON SHELLS, LIGHT, BOSS, BOSSES, ORANGE, GRINDER, GRANADE, GRENADES, FLOOR
COMMON Placeholder, Names, Loaded, Turbo, Invince, name$, LOAD, BOMB, SUIT
COMMON SHOVEL, KEE, OPEN1, AMMOA, AMMOB, FOODA, FOODB, AMMOGET, FOODGET, NOTE
COMMON LIGHTON, SUITON, ITEMUSE, Ending, SpecialCredits, Warping, OPEN2, SWORD
COMMON ENDGAME, HARD


'   To make this work, change the directory path to the one you have all of


'        the Ambush 2 programs in. Then take the ' away from CHDIR.

'         You will need every little file for this to be the
'                     ultimate QBasic game experience.

PALETTE
IF Placeholder = 1 THEN GOTO back4more
IF Placeholder = 2 THEN Placeholder = 0: GOTO reloop
IF Placeholder = 4 THEN GOTO POOPY
Detectcard
IF snd = 4 THEN SBinit
Notagain:
Legal
Logo
GOTO Titlescreen
POOPY:
Placeholder = 0
Q = 1
HEALTH = 20
HEALTHMAX = 19.5
STRENGTH = .6
SHELLS = 20
BOSS = 1
HARD = 2
A = 155
B = 100
reloop:
CLS
SCREEN 13
COLOR 56
LOCATE 2, 10: PRINT "Ambush at South Range II"
highlight = 1
VA = 45
VB = 140
cov = 2
 FOR times = 1 TO 4
   FOR n = 1 TO cov
   Bullethole VA, VB
   IF times = 1 THEN VA = VA + 60
   IF times = 2 THEN VB = VB - 60: cov = 3
   IF times = 3 THEN VA = VA - 60
   IF times = 4 THEN VB = VB + 60
   NEXT
 NEXT
pete:
COLOR 4
IF highlight = 1 THEN COLOR 31
LOCATE 14, 15
PRINT "Kick Butt!"
COLOR 4
IF highlight = 2 THEN COLOR 31
LOCATE 15, 15
PRINT "Read Me "
COLOR 4
IF highlight = 3 THEN COLOR 31
LOCATE 16, 15
PRINT "Sound"
COLOR 4
IF highlight = 4 THEN COLOR 31
LOCATE 17, 15
PRINT "Controls "
COLOR 4
IF highlight = 5 THEN COLOR 31
LOCATE 18, 15
PRINT "Difficulty"
COLOR 4
IF highlight = 6 THEN COLOR 31
LOCATE 19, 15
PRINT "Load Saved Game"
pleasehit: aa$ = INKEY$: IF aa$ = "" THEN GOTO pleasehit
IF aa$ = CHR$(13) AND highlight = 1 THEN
TIMER ON
BEGINNINGTIME = TIMER
FOR i = 14 TO 19
LOCATE i, 15: PRINT STRING$(13, " ")
NEXT
LOCATE 19, 15: PRINT STRING$(15, " ")
COLOR 48
LOCATE 13, 15: PRINT "Name: "
LOCATE 20, 14: PRINT "(limit 6 letters)"
COLOR 40
LOCATE 15, 15
i = 0
DO
aa$ = INKEY$
IF aa$ = CHR$(13) THEN GOTO SNAME
IF aa$ > CHR$(47) AND aa$ < CHR$(123) THEN LOCATE 15, 15 + i: PRINT aa$: W = 1
IF aa$ = " " THEN LOCATE 15, 15 + i: PRINT aa$: W = 1
IF W = 1 THEN i = i + 1
IF i < 5 THEN LOCATE 15, 15 + i: PRINT "_"
IF W = 1 THEN
W = 0
IF i = 1 THEN I1$ = aa$
IF i = 2 THEN I2$ = aa$
IF i = 3 THEN I3$ = aa$
IF i = 4 THEN I4$ = aa$
IF i = 5 THEN I5$ = aa$
IF i = 6 THEN I6$ = aa$
END IF
IF aa$ = CHR$(8) THEN
i = i - 1: LOCATE 15, 15 + i: PRINT "  "
IF i = 0 THEN I1$ = " "
IF i = 1 THEN I2$ = " "
IF i = 2 THEN I3$ = " "
IF i = 3 THEN I4$ = " "
IF i = 4 THEN I5$ = " "
IF i = 5 THEN I6$ = " "
END IF
IF i = 6 THEN i = i - 1
IF i = -1 THEN i = i + 1
LOOP
SNAME:
name$ = I1$ + I2$ + I3$ + I4$ + I5$ + I6$
IF UCASE$(name$) = "SBTEST" THEN GOTO sbtest
FOR i = 14 TO 19
LOCATE i, 15: PRINT STRING$(13, " ")
NEXT
LOCATE 18, 15: PRINT STRING$(15, " ")
LOCATE 13, 15: PRINT "     "
LOCATE 20, 14: PRINT "                 "
LOCATE 15, 15: PRINT "Please wait"
LOCATE 16, 15: PRINT "while Ambush"
LOCATE 17, 15: PRINT "is loading..."
CHAIN "FLOOR1.mk!"
END IF
IF aa$ = CHR$(13) AND highlight = 2 THEN Text 1: GOTO reloop
IF aa$ = CHR$(13) AND highlight = 3 THEN Sond snd: GOTO reloop
IF aa$ = CHR$(13) AND highlight = 4 THEN Text 2: GOTO reloop
IF aa$ = CHR$(13) AND highlight = 5 THEN Difficulty HARD: GOTO reloop
IF aa$ = CHR$(13) AND highlight = 6 THEN CHAIN "ARCADE.mk!"
IF aa$ = CHR$(0) + "H" AND highlight = 1 THEN highlight = 6:  GOTO pete
IF aa$ = CHR$(0) + "P" AND highlight = 6 THEN highlight = 1: : GOTO pete
IF aa$ = CHR$(0) + "H" THEN highlight = highlight - 1: GOTO pete
IF aa$ = CHR$(0) + "P" THEN highlight = highlight + 1: GOTO pete
GOTO pete
Titlescreen:
op = 100
po = 100
CLS
SCREEN 13
LINE (5, 118)-(3, 139), 16
LINE (5, 118)-(15, 113), 16
LINE (15, 113)-(25, 111), 16
LINE (25, 111)-(39, 114), 16
LINE (39, 114)-(46, 119), 16
LINE (46, 119)-(80, 122), 16
LINE (80, 122)-(85, 122), 16
LINE (85, 122)-(90, 118), 16
LINE (90, 118)-(89, 115), 16
LINE (89, 115)-(88, 112), 16
LINE (88, 112)-(91, 111), 16
LINE (91, 111)-(93, 117), 16
LINE (92, 112)-(146, 112), 16
CIRCLE (146, 115), 3, 16, , , 1
LINE (146, 118)-(120, 118), 16
LINE (120, 118)-(120, 123), 16
LINE (120, 123)-(115, 123), 16
LINE (115, 123)-(115, 118), 16
LINE (93, 117)-(93, 125), 16
LINE (93, 125)-(99, 125), 16
LINE (99, 125)-(99, 118), 16
LINE (99, 118)-(115, 118), 16
LINE (99, 121)-(102, 121), 16
LINE (102, 121)-(103, 118), 16
LINE (95, 125)-(88, 134), 16
LINE (88, 134)-(59, 137), 16
LINE (59, 137)-(45, 138), 16
LINE (45, 138)-(22, 141), 16
LINE (22, 141)-(3, 139), 16
LINE (26, 123)-(22, 126), 16
LINE (22, 126)-(22, 129), 16
LINE (22, 129)-(24, 132), 16
LINE (3, 139)-(13, 137), 16
LINE (52, 134)-(81, 131), 16
LINE (52, 125)-(78, 126), 16
LINE (16, 46)-(28, 26), 16
LINE (28, 26)-(36, 46), 16
LINE (17, 46)-(21, 46), 16
LINE (21, 46)-(26, 41), 16
LINE (26, 41)-(28, 41), 16
LINE (28, 41)-(32, 46), 16
LINE (32, 46)-(36, 46), 16
LINE (42, 46)-(47, 29), 16
LINE (47, 29)-(52, 38), 16
LINE (52, 38)-(58, 29), 16
LINE (58, 29)-(62, 46), 16
LINE (62, 46)-(57, 39), 16
LINE (57, 39)-(52, 43), 16
LINE (52, 43)-(48, 38), 16
LINE (48, 38)-(43, 46), 16
LINE (68, 46)-(66, 30), 16
LINE (66, 30)-(76, 33), 16
LINE (76, 33)-(72, 39), 16
LINE (72, 39)-(77, 45), 16
LINE (77, 45)-(68, 46), 16
LINE (81, 34)-(84, 45), 16
LINE (84, 45)-(90, 45), 16
LINE (90, 45)-(92, 34), 16
LINE (92, 34)-(87, 41), 16
LINE (87, 41)-(81, 34), 16
LINE (105, 34)-(100, 38), 16
LINE (100, 38)-(106, 40), 16
LINE (106, 40)-(102, 44), 16
LINE (102, 44)-(110, 41), 16
LINE (110, 41)-(106, 37), 16
LINE (106, 37)-(105, 34), 16
LINE (115, 34)-(117, 39), 16
LINE (117, 39)-(119, 39), 16
LINE (119, 39)-(121, 34), 16
LINE (121, 34)-(123, 45), 16
LINE (123, 45)-(120, 42), 16
LINE (120, 42)-(117, 46), 16
LINE (118, 45)-(115, 45), 16
LINE (115, 45)-(116, 35), 16
LINE (140, 42)-(137, 42), 43
LINE (137, 42)-(137, 45), 43
LINE (137, 45)-(140, 45), 43
LINE (140, 42)-(140, 46), 43
LINE (146, 46)-(146, 39), 43
LINE (144, 41)-(148, 41), 43
LINE (25, 54)-(16, 64), 43
LINE (16, 64)-(30, 69), 43
LINE (30, 69)-(15, 75), 43
CIRCLE (40, 62), 8, 43, , , 1.4
LINE (54, 55)-(58, 69), 43
LINE (58, 69)-(64, 55), 43
LINE (69, 57)-(81, 56), 43
LINE (76, 56)-(74, 70), 43
LINE (92, 70)-(90, 56), 43
LINE (91, 63)-(99, 62), 43
LINE (98, 55)-(101, 70), 43
LINE (48, 95)-(49, 81), 43
LINE (49, 81)-(57, 85), 43
LINE (57, 85)-(49, 87), 43
LINE (49, 87)-(56, 95), 43
LINE (61, 95)-(66, 82), 43
LINE (66, 82)-(72, 95), 43
LINE (69, 89)-(64, 89), 43
LINE (76, 95)-(79, 82), 43
LINE (79, 82)-(83, 94), 43
LINE (83, 94)-(87, 82), 43
LINE (98, 82)-(91, 86), 43
LINE (91, 86)-(92, 94), 43
LINE (92, 94)-(101, 92), 43
LINE (101, 92)-(100, 88), 43
LINE (100, 88)-(96, 89), 43
LINE (107, 93)-(105, 83), 43
LINE (105, 83)-(112, 82), 43
LINE (112, 87)-(106, 88), 43
LINE (107, 93)-(115, 95), 43
LINE (148, 112)-(146, 103), 16
LINE (146, 103)-(154, 108), 16
LINE (154, 108)-(161, 103), 16
LINE (161, 103)-(157, 115), 16
LINE (157, 115)-(168, 111), 16
LINE (168, 111)-(156, 121), 16
LINE (156, 121)-(163, 130), 16
LINE (163, 130)-(150, 124), 16
LINE (150, 124)-(147, 118), 16
LINE (152, 115)-(151, 111), 16
LINE (151, 111)-(155, 112), 16
LINE (155, 112)-(155, 117), 16
LINE (155, 117)-(154, 119), 16
LINE (154, 119)-(155, 124), 16
LINE (155, 124)-(151, 118), 16
LINE (151, 118)-(152, 115), 16
LINE (211, 35)-(211, 127), 16
LINE (211, 127)-(181, 127), 16
LINE (181, 127)-(181, 137), 16
LINE (181, 137)-(275, 137), 16
LINE (275, 137)-(275, 127), 16
LINE (275, 127)-(245, 127), 16
LINE (245, 127)-(245, 35), 16
LINE (245, 35)-(275, 35), 16
LINE (275, 35)-(275, 25), 16
LINE (275, 25)-(181, 25), 16
LINE (181, 25)-(181, 35), 16
LINE (181, 35)-(211, 35), 16
LINE (221, 36)-(234, 127), 16, B
PAINT (88, 123), 65, 16
PAINT (100, 119), 65, 16
PAINT (119, 117), 23, 16
PAINT (151, 121), 4, 16
PAINT (153, 117), 41, 16
PAINT (154, 121), 41, 16
PAINT (220, 116), 237, 16
PAINT (29, 38), 112, 16
PAINT (52, 39), 112, 16
PAINT (70, 40), 112, 16
PAINT (86, 42), 112, 16
PAINT (104, 38), 112, 16
PAINT (118, 41), 112, 16
FOR i = 1 TO 2
LINE (op - 55, po + 96)-(op - 55, po + 68), 16
LINE (op - 55, po + 68)-(op - 45, po + 68), 16
LINE (op - 45, po + 68)-(op - 45, po + 62), 16
LINE (op - 45, po + 62)-(op - 55, po + 62), 16
LINE (op - 55, po + 62)-(op - 55, po + 55), 16
LINE (op - 55, po + 55)-(op - 63, po + 55), 16
LINE (op - 63, po + 55)-(op - 63, po + 62), 16
LINE (op - 63, po + 62)-(op - 72, po + 62), 16
LINE (op - 72, po + 62)-(op - 72, po + 68), 16
LINE (op - 72, po + 68)-(op - 63, po + 68), 16
LINE (op - 63, po + 68)-(op - 63, po + 96), 16
LINE (op - 63, po + 96)-(op - 55, po + 96), 16
LINE (op - 35, po + 96)-(op - 34, po + 67), 16
LINE (op - 34, po + 67)-(op - 24, po + 57), 16
LINE (op - 24, po + 57)-(op - 15, po + 55), 16
LINE (op - 15, po + 55)-(op - 6, po + 57), 16
LINE (op - 6, po + 57)-(op - 1, po + 67), 16
LINE (op - 1, po + 67)-(op - 3, po + 96), 16
LINE (op - 3, po + 96)-(op - 35, po + 96), 16
LINE (op + 39, po + 95)-(op + 25, po + 66), 16
LINE (op + 25, po + 66)-(op + 17, po + 72), 16
LINE (op + 17, po + 72)-(op + 13, po + 68), 16
LINE (op + 13, po + 68)-(op + 22, po + 60), 16
LINE (op + 22, po + 60)-(op + 19, po + 55), 16
LINE (op + 19, po + 55)-(op + 26, po + 51), 16
LINE (op + 26, po + 51)-(op + 30, po + 58), 16
LINE (op + 30, po + 58)-(op + 38, po + 52), 16
LINE (op + 38, po + 52)-(op + 42, po + 58), 16
LINE (op + 42, po + 58)-(op + 33, po + 64), 16
LINE (op + 33, po + 64)-(op + 47, po + 94), 16
LINE (op + 47, po + 94)-(op + 39, po + 95), 16
PAINT (op + 40, po + 90), 42, 16
PAINT (op - 18, po + 90), 26, 16
PAINT (op - 59, po + 90), 20, 16
op = op + 120
NEXT
COLOR 47
LOCATE 19, 10: PRINT "The Chaos Continues"
CHAIN "hoFAME."
WHILE INKEY$ = "": WEND
END
back4more:
aa$ = INKEY$
aa$ = ""
IF SpecialCredits = 500 THEN CHAIN "ENDING.mk!"
Placeholder = 0
FOR i = 1 TO 63
PALETTE 0, 65536 * i
FOR ASDFASDF = 1 TO 25: NEXT
aa$ = INKEY$
aa$ = ""
NEXT
FOR i = 63 TO 1 STEP -1
PALETTE 0, 65536 * i + i
FOR ASDFASDF = 1 TO 25: NEXT
aa$ = INKEY$
aa$ = ""
NEXT
aa$ = INKEY$
aa$ = ""
WHILE INKEY$ = "": WEND
GOTO POOPY
sbtest:
SCREEN 0
WIDTH 80
menu:
CLS
LOCATE 2, 20: PRINT "Sound Test: Ambush at South Range II"
LOCATE 10, 37: PRINT choi
j: aa$ = INKEY$: IF aa$ = "" THEN GOTO j
Display
IF aa$ = CHR$(0) + "H" THEN
IF choi < 10 THEN choi = choi + 1
Display
GOTO j
END IF
IF aa$ = CHR$(0) + "P" THEN
IF choi > 1 THEN choi = choi - 1
Display
GOTO j
END IF
IF aa$ = CHR$(13) THEN
 SELECT CASE choi
  CASE 1: GOSUB newboss
  CASE 2: GOSUB items
  CASE 3: GOTO huh
  CASE 4: GOTO gameovr
  CASE 5: GOTO bossmusic
  CASE 6: GOTO bully
  CASE 7: GOTO krivan
  CASE 8: GOTO Queer
  CASE 9: GOTO dada
  CASE 10: GOTO Notagain
 END SELECT
GOTO j
END IF
GOTO j
newboss:
RESTORE bossmusi
FOR i = 1 TO 16
READ sund%
SBPlay2 sund%, 9, &H31, &HF10: CALL WriteReg(&HB0, &H0)
NEXT
bossmusi:
DATA 1000, 1200, 1000, 1200, 100, 300, 100, 300,1000,1200,1000,1200,100,300
DATA 100,300
FOR i = 1 TO 16
READ sund%
SBPlay2 sund%, 9, &HF29, &HF9: CALL WriteReg(&HB0, &H0)
NEXT
DATA 1000, 1200, 1000, 1200, 100, 300, 100, 300,1000,1200,1000,1200,100,300
DATA 100, 300
GOTO menu
items:
x = 21
 FOR i = 1 TO 12
 FOR sund% = 1 TO 90 STEP 30
SBPlay2 sund%, 1, &HF + x, &H9: CALL WriteReg(&HB0, &H0)
IF INKEY$ <> "" THEN EXIT FOR
NEXT
 x = x + 1
NEXT
GOTO menu
huh:
RESTORE newdata
SBPlay2 600, 10, &HF27, &HF1: CALL WriteReg(&HB0, &H0)
SBPlay2 1000, 20, &HF27, &HF1: CALL WriteReg(&HB0, &H0)
FOR i = 1 TO 6
READ sund%
SBPlay2 sund%, 8, &HF26, &HF1: CALL WriteReg(&HB0, &H0)
NEXT
newdata:
DATA 100,150,100,150,100,150
GOTO menu
gameovr:
RESTORE gmeov
FOR i = 1 TO 8
READ sund%
SBPlay2 sund%, 10, &HA30, &HF1: CALL WriteReg(&HB0, &H0)
NEXT
gmeov:
DATA 1200, 1200,1200,1200, 1175, 1150,1125,1100
RESTORE gmeov
FOR i = 1 TO 8
READ sund%
SBPlay2 sund%, 10, &HA26, &HF1: CALL WriteReg(&HB0, &H0)
NEXT
SBPlay2 1150, 10, &HA26, &HA9: CALL WriteReg(&HB0, &H0)
SBPlay2 1190, 10, &HA26, &HA9: CALL WriteReg(&HB0, &H0)
SBPlay2 1200, 13, &HA26, &HA9: CALL WriteReg(&HB0, &H0)
GOTO menu
bossmusic:
FOR sund% = 120 TO 220 STEP 10
SBPlay2 sund%, 1, &H26, &HF9: CALL WriteReg(&HB0, &H0)
NEXT
FOR sund% = 220 TO 10 STEP -10
SBPlay2 sund%, 1, &H26, &HF9: CALL WriteReg(&HB0, &H0)
NEXT
FOR sund% = 10 TO 250 STEP 10
SBPlay2 sund%, 1, &H26, &HF9: CALL WriteReg(&HB0, &H0)
NEXT
SBPlay sund%, 10, &H39, &HF9: CALL WriteReg(&HB0, &H0)
SBPlay sund%, 10, &H39, &HF9: CALL WriteReg(&HB0, &H0)
GOTO menu
bully:
FOR sund% = 160 TO 100 STEP -10
SBPlay2 sund%, 3, &H25, &HC: CALL WriteReg(&HB0, &H0)
NEXT
FOR sund% = 100 TO 200 STEP 10
SBPlay2 sund%, 2, &H25, &HC: CALL WriteReg(&HB0, &H0)
NEXT
FOR i = 1 TO 8
SBPlay2 100, 2, &H34, &HC: CALL WriteReg(&HB0, &H0)
NEXT
GOTO menu
krivan:
RESTORE bossmusi
FOR i = 1 TO 16
READ sund%
SBPlay3 sund%, 9, &HC39, &HF19: CALL WriteReg(&HB0, &H0)
NEXT
FOR i = 1 TO 16
READ sund%
SBPlay3 sund%, 9, &HF33, &HFC1: CALL WriteReg(&HB0, &H0)
NEXT
CALL WriteReg(&HB0, &H0)
GOTO menu
dada:
Fx
GOTO menu
Queer:
FOR x% = 1000 TO 800 STEP -30
SBPlay3 x%, 1, &H30, &H2: CALL WriteReg(&HB0, &H0)
NEXT
FOR x% = 800 TO 1000 STEP 30
SBPlay3 x%, 1, &H30, &H2: CALL WriteReg(&HB0, &H0)
NEXT
GOTO menu

SUB Back (sped, orw, words$)
mi = LEN(words$)
cent = 41 - INT(LEN(words$) / 2) + (mi) - 1
FOR im = mi TO 1 STEP -1
FOR delay = 1 TO sped: NEXT
LOCATE orw, cent
IF im > 1 THEN PRINT MID$(words$, im, 1)
IF im = 1 THEN PRINT LEFT$(words$, 1)
cent = cent - 1
NEXT
END SUB

SUB Bullethole (VA, VB)
CIRCLE (VA + 81, VB + 37), 3, 25, , , 1
PAINT (VA + 81, VB + 37), 25, 25
LINE (VA + 78, VB + 38)-(VA + 72, VB + 36), 25
LINE (VA + 72, VB + 36)-(VA + 63, VB + 38), 25
LINE (VA + 63, VB + 38)-(VA + 72, VB + 51), 25
LINE (VA + 68, VB + 44)-(VA + 58, VB + 49), 25
LINE (VA + 82, VB + 40)-(VA + 78, VB + 46), 25
LINE (VA + 78, VB + 46)-(VA + 87, VB + 58), 25
LINE (VA + 87, VB + 58)-(VA + 71, VB + 64), 25
LINE (VA + 83, VB + 35)-(VA + 93, VB + 29), 25
LINE (VA + 89, VB + 31)-(VA + 98, VB + 36), 25
LINE (VA + 98, VB + 36)-(VA + 106, VB + 26), 25
LINE (VA + 83, VB + 60)-(VA + 99, VB + 65), 25
LINE (VA + 85, VB + 38)-(VA + 95, VB + 50), 25
LINE (VA + 94, VB + 49)-(VA + 115, VB + 45), 25
LINE (VA + 82, VB + 31)-(VA + 77, VB + 25), 25
END SUB

SUB Detectcard
SHARED snd
   CALL WriteReg(&H4, &H60)
   CALL WriteReg(&H4, &H80)
   B = INP(&H388)
   CALL WriteReg(&H2, &HFF)
   CALL WriteReg(&H4, &H21)
   FOR x = 0 TO 130
      A = INP(&H388)
   NEXT x
   c = INP(&H388)
   CALL WriteReg(&H4, &H60)
   CALL WriteReg(&H4, &H80)
   success = 0
   IF (B AND &HE0) = &H0 THEN
     IF (c AND &HE0) = &HC0 THEN
       success = -1
     END IF
   END IF
  IF success = -1 THEN snd = 4
  IF success = 0 THEN snd = 1
END SUB

SUB Difficulty (HARD)
CLS
f:
LOCATE 5, 1: PRINT "             "
COLOR 14
LOCATE 5, 1
COLOR 42
PRINT "1) Easy"
PRINT "2) Normal"
PRINT "3) Difficult"
PRINT "4) Exit"
PRINT STRING$(80, " ")
PRINT STRING$(80, " ")
COLOR 93
LOCATE 4 + HARD, 20: PRINT CHR$(27)
kn: aa$ = INKEY$: IF aa$ = "" THEN GOTO kn
IF VAL(aa$) < 1 OR VAL(aa$) > 5 THEN GOTO kn
LOCATE 4 + HARD, 20: PRINT " "
IF VAL(aa$) <> 4 THEN
HARD = VAL(aa$)
END IF
IF VAL(aa$) = 4 THEN EXIT SUB
GOTO f
END SUB

SUB Direct
LOCATE 15, 10
PRINT "  Esc"
END SUB

DEFINT Z
SUB Display
SHARED choi
LOCATE 10, 1: PRINT SPACE$(55)
IF choi <> 10 THEN LOCATE 10, 37: PRINT choi
IF choi = 10 THEN LOCATE 10, 21: PRINT "Go back to Ambush at South Range II"
END SUB

DEFSNG Z
SUB Footer (Word$)
LOCATE 23, 41 - LEN(Word$)
PRINT Word$
END SUB

SUB Fx
 FOR x% = 1030 TO 1260 STEP 15
  SBPlay x%, 1, &H31, &H2: CALL WriteReg(&HB0, &H0)
 NEXT
 FOR x% = 1260 TO 1030 STEP -15
  SBPlay x%, 1, &H31, &H2: CALL WriteReg(&HB0, &H0)
 NEXT
 FOR x% = 1000 TO 800 STEP -15
  SBPlay x%, 1, &H30, &H2: CALL WriteReg(&HB0, &H0)
 NEXT
END SUB

SUB Legal
SHARED snd
CLS
SCREEN 12
WIDTH 80
COLOR 15
Slide 100, 3, "Ambush at South Range II"
Slide 100, 4, "(c) Copyright 1995 by Molnar \ Kucalaba Productions"
Slide 20, 6, "All characters and events in this game are completely"
Back 20, 7, "ficticious. Any resemblence to any living or non-living"
Slide 20, 8, "person is purely coincidental."
Back 20, 10, "Due to some graphic scenes of violence, we recommend children under"
Slide 20, 11, "the age of 10 to abstain from playing. M/K Productions does not"
Back 20, 12, "support such behavior and encourages everyone to be kind to one another."
Slide 20, 13, "This product is a registered trademark of Molnar \ Kucalaba"
Back 20, 14, "Productions. Any reproduction with the intent to sell is strictly"
Slide 20, 15, "forbidden. This is Shareware. Please distribute it to all your"
Back 20, 16, "friends, relatives, enemies, and assorted other individuals."
Slide 20, 17, "All of the Sound Blaster SUBs were made entirely by Bret Levin."
Back 20, 18, "We have permission to use these SUBs."
Slide 20, 21, "Ambush at South Range and it's distinctive likenesses are "
Back 20, 22, "registered trademarks of Molnar \ Kucalaba Productions."
Slide 20, 24, "Molnar \ Kucalaba Productions can be reached on the internet and AOL:"
Back 20, 25, "Blood 225@aol.com"
IF snd = 4 THEN Fx
WHILE INKEY$ = "": WEND
H = 80
g = 1
FOR ASI = 1 TO 40
FOR i = 1 TO 25
LOCATE i, g: PRINT " "
LOCATE i, H: PRINT " "
NEXT
FOR delay = 1 TO 5: NEXT
g = g + 1
H = H - 1
NEXT
END SUB

SUB Logo
CLS
SCREEN 13
CC = 15
c = 31
i = 15
S = 32
SI = 62
PALETTE 1, 20
FOR r = 2 TO 63
SI = SI - 1
PALETTE r, r
CIRCLE (157, 100), SI, r, , , -.5
NEXT
FOR i = 26 TO 41
LINE (1, 1)-(319, 199), 1, B: LINE (60, 50)-(70, 50), 1
LINE (60, 50)-(60, 150), 1: LINE (60, 150)-(70, 150), 1
LINE (70, 150)-(70, 60), 1: LINE (70, 50)-(120, 100), 1
LINE (70, 60)-(120, 110), 1: LINE (120, 110)-(130, 110), 1
LINE (120, 100)-(130, 100), 1: LINE (130, 100)-(180, 50), 1
LINE (130, 110)-(180, 60), 1: LINE (180, 50)-(190, 50), 1
LINE (180, 60)-(180, 150), 1: LINE (180, 150)-(190, 150), 1
LINE (190, 50)-(190, 95), 1: LINE (190, 150)-(190, 105), 1
LINE (190, 105)-(235, 150), 1: LINE (190, 95)-(235, 50), 1
LINE (235, 150)-(245, 150), 1: LINE (235, 50)-(245, 50), 1
LINE (245, 150)-(195, 100), 1: LINE (245, 50)-(195, 100), 1
PAINT (65, 55), CC, 1
CC = CC + .9
LOCATE 2, 5
COLOR i: PRINT " Molnar \ Kucalaba Productions"
LOCATE 23, 12
COLOR i: PRINT "Proudly Present"
NEXT
AGAIN:
FOR j = 0 TO 63
PALETTE 0, j
FOR W = 1 TO 50
aa$ = INKEY$
IF aa$ <> "" THEN GOTO NXTA
NEXT
NEXT
FOR j = 63 TO 0 STEP -1
PALETTE 0, j
FOR W = 1 TO 50
aa$ = INKEY$
IF aa$ <> "" THEN GOTO NXTA
NEXT
NEXT
GOTO AGAIN
NXTA:
FOR j = 0 TO 63
PALETTE 0, j
FOR W = 1 TO 50
aa$ = INKEY$
NEXT
NEXT
FOR j = 63 TO 0 STEP -1
PALETTE 0, j
FOR W = 1 TO 50
NEXT
NEXT
FOR c = 31 TO 2 STEP -1
LINE (1, 1)-(319, 199), c, B: LINE (60, 50)-(70, 50), c
LINE (60, 50)-(60, 150), c: LINE (60, 150)-(70, 150), c
LINE (70, 150)-(70, 60), c: LINE (70, 50)-(120, 100), c
LINE (70, 60)-(120, 110), c: LINE (120, 110)-(130, 110), c
LINE (120, 100)-(130, 100), c: LINE (130, 100)-(180, 50), c
LINE (130, 110)-(180, 60), c: LINE (180, 50)-(190, 50), c
LINE (180, 60)-(180, 150), c: LINE (180, 150)-(190, 150), c
LINE (190, 50)-(190, 95), c: LINE (190, 150)-(190, 105), c
LINE (190, 105)-(235, 150), c: LINE (190, 95)-(235, 50), c
LINE (235, 150)-(245, 150), c: LINE (235, 50)-(245, 50), c
LINE (245, 150)-(195, 100), c: LINE (245, 50)-(195, 100), c
LOCATE 2, 5
COLOR c: PRINT " Molnar \ Kucalaba Productions"
LOCATE 23, 12
COLOR c: PRINT "Proudly Present"
c = c - 1
PLAY "MFT255P16o3l6"
NEXT
WHILE INKEY$ = "": WEND
COLOR 15
CLS
PALETTE
END SUB

DEFINT Z
SUB SBinit
   FOR z% = 1 TO &HF5
     CALL WriteReg(z%, 0)
   NEXT z%
END SUB

SUB SBPlay (zreg%, zuration%, zoctave%, zaveform%)
CALL WriteReg(&H20, &HB15)
CALL WriteReg(&H90, &HF999)
CALL WriteReg(&H60, &HFA8)
CALL WriteReg(&H80, &H1)
CALL WriteReg(&HA0, zreg%)
CALL WriteReg(&H23, &H3)
CALL WriteReg(&H43, &H1)
CALL WriteReg(&H63, &HF99)
CALL WriteReg(&H83, &H9)
CALL WriteReg(&HB0, zoctave%)
CALL WriteReg(&HE0, zaveform%)
Timeup! = TIMER + zuration% / 64
WHILE Timeup! > TIMER: WEND
END SUB

SUB SBPlay2 (zreg%, zuration%, zoctave%, zaveform%)
CALL WriteReg(&H20, &HA39)
CALL WriteReg(&H40, &HFA90)
CALL WriteReg(&H60, &HA9)
CALL WriteReg(&H80, &HD1)
CALL WriteReg(&HA0, zreg%)
CALL WriteReg(&H23, &HA3)
CALL WriteReg(&H43, &H1)
CALL WriteReg(&H63, &HFA1)
CALL WriteReg(&H83, &HFD)
CALL WriteReg(&HB0, zoctave%)
CALL WriteReg(&HE0, zaveform%)
Timeup! = TIMER + zuration% / 64
WHILE Timeup! > TIMER: WEND
END SUB

SUB SBPlay3 (zreg%, zuration%, zoctave%, zaveform%)
CALL WriteReg(&H20, &HB15)
CALL WriteReg(&H40, &HF900)
CALL WriteReg(&H60, &HFA8)
CALL WriteReg(&H80, &HF9)
CALL WriteReg(&HA0, zreg%)
CALL WriteReg(&H23, &HEF30)
CALL WriteReg(&H43, &H1)
CALL WriteReg(&H63, &HF5)
CALL WriteReg(&H83, &HF1)
CALL WriteReg(&HB0, zoctave%)
CALL WriteReg(&HE0, zaveform%)
Timeup! = TIMER + zuration% / 64
WHILE Timeup! > TIMER: WEND
END SUB

DEFSNG Z
SUB Slide (speed, row, txet$)
mi = LEN(txet$)
cent = 41 - INT(LEN(txet$) / 2)
FOR im = 1 TO mi
FOR delay = 1 TO speed: NEXT
LOCATE row, cent
IF im > 1 THEN PRINT MID$(txet$, im, 1)
IF im = 1 THEN PRINT LEFT$(txet$, 1)
cent = cent + 1
NEXT
aa$ = INKEY$
aa$ = ""
END SUB

SUB Sond (snd)
CLS
er = snd + 4
t:
LOCATE 1, 1
COLOR 41: PRINT " Ambush at South Range II Sound Options"
COLOR 67
LOCATE 4, 1
PRINT "1) Sound FX"
PRINT "2) Music"
PRINT "3) No sound"
PRINT "4) Sound Blaster"
PRINT "5) Exit Sound Option"
PRINT STRING$(80, " ")
PRINT STRING$(80, " ")
col = snd + 3
COLOR 4
LOCATE col, 25: PRINT CHR$(27)
sd: aa$ = INKEY$: IF aa$ = "" THEN GOTO sd
IF VAL(aa$) < 1 OR VAL(aa$) > 5 THEN GOTO sd
LOCATE col, 25: PRINT " "
IF aa$ = "5" THEN EXIT SUB
snd = VAL(aa$)
GOTO t
END SUB

SUB Text (functnum)
SHARED Names
IF functnum = 1 THEN textsc = 1
IF functnum = 2 THEN textsc = 10
CLS
LINE (-4, 26)-(320, 85), 171, B
lm = 100
lk = 100
SIZ = 5
FOR BLOOD = 1 TO 3
FOR SIZ = 5 TO .5 STEP -.09
CIRCLE (lm + 50, lk + 21), SIZ, 4, , , 1
PAINT (lm + 50, lk + 21), 4, 4
lk = lk + 1
NEXT
FOR S = 1 TO 68
PSET (lm + 50, lk + 21), 4
lk = lk + 1
NEXT
lm = lm + 40
lk = 100 - lm * .2
NEXT
COLOR 67: LOCATE 2, 10: PRINT "Ambush at South Range II"
newpage:
SCREEN 13
COLOR 29
IF textsc = 1 THEN
FOR i = 16 TO 31 STEP 1.5
LOCATE 5, 1
COLOR i
PRINT "  Twenty years ago, your father put an  "
IF Names = 500 THEN PRINT "end to the reign of terror of Mr. Krivan"
IF Names = 0 THEN PRINT " end to the reign of terror of Mr. Wuss "
PRINT " Now South Range has been rebuilt, and  "
IF Names = 500 THEN PRINT "Mr. Krivans remarkably similar son is in"
IF Names = 0 THEN PRINT "Mr. Wuss's remarkably similar son is in "
PRINT "           control again.              "
PRINT "                                        "
Direct
Footer "page 1 of 9"
NEXT
END IF
IF textsc = 2 THEN
FOR i = 16 TO 31 STEP 1.5
LOCATE 5, 1
COLOR i
PRINT " Many innocent people have died. Prison "
PRINT " camps, or schools, are being built as  "
IF Names = 500 THEN PRINT "we speak. Mr. Krivan has set up torture "
IF Names = 0 THEN PRINT " we speak. Mr. Wuss has set up torture  "
PRINT " chambers for people who put down his   "
PRINT "         barbaric methods.              "
PRINT "                                        "
NEXT
Direct
Footer "page 2 of 9"
END IF
IF textsc = 3 THEN
FOR i = 16 TO 31 STEP 1.5
LOCATE 5, 1
COLOR i
PRINT "  You are our last hope. Your mission   "
IF Names = 500 THEN PRINT " is to kill Mr. Krivan's henchemen, or  "
IF Names = 0 THEN PRINT "  is to kill Mr. Wuss's henchemen, or   "
IF Names = 500 THEN PRINT " teachers, kill Mr. Krivan himself, and "
IF Names = 0 THEN PRINT "  teachers, kill Mr. Wuss himself, and  "
PRINT " blow up the building back to the Stone "
PRINT "                 Age.                   "
PRINT "              Good Luck!                "
NEXT
Direct
Footer "page 3 of 9"
END IF
IF textsc = 4 THEN
FOR i = 16 TO 31 STEP 1.5
LOCATE 5, 1
COLOR i
PRINT "           About the Makers             "
PRINT "                                        "
PRINT "Molnar \ Kucalaba Productions is a fine "
PRINT "company devoted to your total, complete "
PRINT "satisfaction. Founded in August of 1994,"
PRINT "we are the symbol of quality in QBasic. "
NEXT
Direct
Footer "page 4 of 9"
END IF
IF textsc = 5 THEN
FOR i = 16 TO 31 STEP 1.5
LOCATE 5, 1
COLOR i
PRINT "          Other Programs on AOL:        "
PRINT "              QBasic Maze               "
PRINT "        Ambush at South Range I         "
PRINT "       QBasic Animation Factory         "
PRINT "         QBasic Game Profiler           "
PRINT "          QBasic Music Maker            "
NEXT
Direct
Footer "page 5 of 9"
END IF
IF textsc = 6 THEN
FOR i = 16 TO 31 STEP 1.5
LOCATE 5, 1
COLOR i
PRINT "          Other Programs on AOL:        "
PRINT "           QBasic Jumble Buster         "
PRINT "              File Viewer               "
PRINT "         QBasic Crossword Puzzle?       "
PRINT "             QBasic Pieces?             "
PRINT "              PsychaBASIC?              "
NEXT
Direct
Footer "page 6 of 9"
END IF
IF textsc = 7 THEN
FOR i = 16 TO 31 STEP 1.5
LOCATE 5, 1
COLOR i
PRINT "      Please send all comments to:      "; ""
PRINT "                                        "
PRINT "       Blood 225 (AOL screen name)      "
PRINT "       Internet: Blood 225@aol.com      "
PRINT "                                        "
PRINT "              Thank You                 "
NEXT
Direct
Footer "page 7 of 9"
END IF
IF textsc = 8 THEN
FOR i = 16 TO 31 STEP 1.5
LOCATE 5, 1
COLOR i
PRINT "If you are interested in other programs "
PRINT " we have made then drop me a line. What "
PRINT "we really need more than a programmer is"
PRINT "a beta tester. If you are interested, I "
PRINT " would like it if you would contact me. "
PRINT "      AOL Screen name: Blood 225.       "
NEXT
Direct
Footer "page 8 of 9"
END IF
IF textsc = 9 THEN
FOR i = 16 TO 31 STEP 1.5
LOCATE 5, 1
COLOR i
PRINT "      Have you downloaded Ambush I?     "
PRINT "If not, you are missing out on the only "
PRINT "REAL game for QBasic. (Besides this game"
PRINT "of course) This ain't no stupid guessing"
PRINT " game either. And it's massive too. 11  "
PRINT "programs full of death and destruction! "
NEXT
Direct
Footer "page 9 of 9"
END IF
IF textsc = 10 THEN
FOR i = 16 TO 31 STEP 1.5
LOCATE 5, 1
COLOR i
PRINT "          How to Play the Game          "
PRINT "                                        "
PRINT "  Use the arrow keys to move your guy,  "
PRINT " the space bar to open and take things, "
PRINT "  and escape to leave places. To visit  "
PRINT "  the vital screen, tap F1. Read on...  "
NEXT
Direct
Footer "page 1 of 3"
END IF
IF textsc = 11 THEN
FOR i = 16 TO 31 STEP 1.5
LOCATE 5, 1
COLOR i
PRINT "      Keys when in the Hallway...       "
PRINT " The circles represent the exits of the "
PRINT "school. The brown boxes are doors. Press"
PRINT "  1 to walk, 2 to jog, or 3 to run. Hit "
PRINT "enter to stop walking. To use an item in"
PRINT "      the school, hit 0 or Insert."
NEXT
Direct
Footer "page 2 of 3"
END IF
IF textsc = 12 THEN
FOR i = 16 TO 31 STEP 1.5
LOCATE 5, 1
COLOR i
PRINT "      Keys when in the Hallway...       "
PRINT "                                        "
PRINT " The skull in rooms is a boss. The most "
PRINT "important thing to remember is that the "
PRINT "space bar has many uses. For example you"
PRINT "  need to open the lockers with space.  "
NEXT
Direct
Footer "page 3 of 3"
END IF
inky: aa$ = INKEY$: IF aa$ = "" THEN GOTO inky
IF aa$ = CHR$(27) THEN EXIT SUB
IF aa$ = CHR$(0) + "P" AND textsc = 10 THEN GOTO newpage
IF aa$ = CHR$(0) + "H" AND textsc = 13 THEN GOTO newpage
IF aa$ = CHR$(0) + "P" AND textsc = 1 THEN GOTO newpage
IF aa$ = CHR$(0) + "P" THEN textsc = textsc - 1: GOTO newpage
IF aa$ = CHR$(0) + "H" AND textsc = 9 THEN GOTO newpage
IF aa$ = CHR$(0) + "H" THEN textsc = textsc + 1: GOTO newpage
GOTO inky
END SUB

DEFINT Z
SUB WriteReg (zeg%, zalue%)
OUT &H388, zeg

  FOR ze = 0 TO 5
  z = INP(&H388)
  NEXT ze
                                                     
OUT &H389, zalue
  FOR ze = 0 TO 34
   z = INP(&H388)
  NEXT ze
END SUB


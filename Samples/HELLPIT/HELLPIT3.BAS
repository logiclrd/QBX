'       лллллллллллллл        лллллллллллл         лллллллллллллл
'       ллББББББББББББ         ББББллББББББ        ллББББББББББллБ
'       ллБ                        ллБ             ллБ         ллБ    '97
'       ллллллллл                  ллБ             ллБ          ББ
'       ллББББББББ                 ллБ             ллБ     лллллл
'       ллБ                        ллБ             ллБ      БББллБ
'       ллБ                        ллБ             ллБ         ллБ
'       ллБ                   лллллллллллл         ллллллллллллллБ
'        ББ                    ББББББББББББ         ББББББББББББББ

'     The new wave of computer game technology!

'     Presents... HELLPIT III: Final Flame




'         Press Page Down to read more...

'ллллллллллллллллллллллллллллллллллллллллллллллллллллллллллллллллллллллллллллл
'л      Fig Presents Hellpit III: Final Flame                                л
'л                                                                           л
'л      Welcome to the final of the three HELLPITS.  This game may not be as л
'л long as HELLPIT II, but it's longer than HELLPIT, and it has many new     л
'л improvements to the game.  One major addition to the game is the use of   л
'л fading in/out, new colors, etc. in many moves.  Another improvement that  л                
'л I have made is the minor graphical improvements that have been made.  I   л
'л know that there are many of the same graphics as HELLPIT II, but I have   л
'л made better looking enemies, attacks, and magics.  There are also quite a л
'л few secrets which will be revealed shortly when I release the HELLPIT     л
'л trilogy.  The HELLPIT trilogy will have all three games with MORE added   л
'л to each, a strategy guide, and even MORE SECRETS added!  It should be     л
'л complete VERY SOON!                                                       л
'ллллллллллллллллллллллллллллллллллллллллллллллллллллллллллллллллллллллллллллл

' PLEASE use HELLPIT3.bat to run this properly!!!                                                                                                                                            

DECLARE SUB es ()
DECLARE SUB about ()
DECLARE SUB instructions ()
DECLARE SUB setrgb (Nr!, r!, g!, B!)
DECLARE SUB dl ()
DECLARE SUB fastexplosions (x!, y!)
DECLARE SUB cross ()
DECLARE SUB explosions (x!, y!)
DECLARE SUB intro ()
DECLARE SUB sounds ()
DECLARE SUB options ()
DECLARE SUB word (x!, y!, l$, c!)
DECLARE SUB nicecrystal (x!, y!)
DECLARE SUB crystal (x!, y!)
DECLARE SUB writing ()
DECLARE SUB fp ()
DECLARE SUB border ()
DECLARE SUB title ()

RANDOMIZE TIMER

SCREEN 12                              '
                                       '
LINE (0, 0)-(640, 480), 1, BF          '
LINE (10, 10)-(630, 470), 0, BF        '
                                       '
                                       '
fp                                     '
                                       '
FOR p = 0 TO 63 STEP .5                '
                                       '
fp                                     '
                                       '
setrgb 1, 0, 0, -p                     '
setrgb 0, 0, 0, p                      '
setrgb 4, -p, 0, 0                     '
                                       '
NEXT p                                 '   All of this is for the FIG logo
                                       '
FOR s = 0 TO 320                       '
                                       '
CIRCLE (320, 240), s, 0, , , .2        '
                                       '
setrgb 4, -64, 0, 0                    '
setrgb 0, s / 10, 0, 0                 '
                                       '
LINE (s, 0)-(s, 480), 0                '
LINE (640 - s, 0)-(640 - s, 480), 0    '
SOUND 100 + s, .04                     '
                                       '
NEXT s                                 '







title

FOR d = 1 TO 3
PLAY "mbl11o1eegeegeegeeg"
PLAY "mbl7o1cdd#"
NEXT d

PLAY "mbl11o1eegeegeegeeg"
PLAY "mbl7o3cdd#"

writing

c = 78

options

0

crystal 160, c

DO
a$ = INKEY$
IF a$ = CHR$(0) + "P" THEN IF c < 140 THEN c = c + 20: sounds: GOTO 0
IF a$ = CHR$(0) + "H" THEN IF c > 78 THEN c = c - 20: sounds: GOTO 0
IF a$ = CHR$(13) THEN GOTO 0.1
IF a$ = CHR$(27) THEN CHAIN "bye.hp3"
IF a$ = CHR$(0) + ";" THEN GOTO 10
IF a$ = CHR$(0) + CHR$(64) THEN IF c = 78 THEN GOTO 125
LOOP

0.1
IF c = 78 THEN GOTO 1
IF c = 98 THEN GOTO 2
IF c = 118 THEN GOTO 3
IF c = 138 THEN GOTO 4
IF c = 158 THEN GOTO 5

1
CLS
PAINT (1, 1), 1
COLOR 9, 1
PRINT "Which file would you like to use?"
PRINT "File (1), File (2), File (3), or (N)one ?"
PRINT "(You can view each file before you use it)"

DO
a$ = INKEY$
IF a$ = "1" THEN file = 1: GOTO 1.1
IF a$ = "2" THEN file = 2: GOTO 1.1
IF a$ = "3" THEN file = 3: GOTO 1.1
IF UCASE$(a$) = "N" THEN CLS : options: GOTO 0
LOOP

1.1
IF file = 1 THEN OPEN "save1.dat" FOR INPUT AS #1
IF file = 2 THEN OPEN "save2.dat" FOR INPUT AS #1
IF file = 3 THEN OPEN "save3.dat" FOR INPUT AS #1
        INPUT #1, characters, bro, savefile
        INPUT #1, dhp, dhpmax, dmp, dmpmax
        INPUT #1, dlev, dex, dweapon, darmor
        INPUT #1, nhp, nhpmax, nmp, nmpmax
        INPUT #1, nlev, nex, nweapon, narmor
        INPUT #1, thp, thpmax, tmp, tmpmax
        INPUT #1, tlev, tex, tweapon, tarmor
        INPUT #1, portal, money
        CLOSE #1
       
CLS
COLOR 4, 12
LOCATE 1, 37
PRINT "FILE #", file
IF dweapon = 1 THEN dwname$ = "EXO-STARS" ELSE dwname$ = "ENTITY STARS"
IF darmor = 1 THEN daname$ = "EXO-VEST" ELSE daname$ = "ENTITY MAIL"
IF characters = 3 THEN
IF nweapon = 1 THEN nwname$ = "EXO-STARS" ELSE nwname$ = "ENTITY STARS"
IF narmor = 1 THEN naname$ = "EXO-VEST" ELSE naname$ = "ENTITY MAIL"
IF tweapon = 1 THEN twname$ = "EXO-SWORD" ELSE twname$ = "ENTITY BLADE"
IF tarmor = 1 THEN taname$ = "FIRE CAPE" ELSE taname$ = "ENTITY DRESS"
END IF

LOCATE 2, 5
PRINT "DAVID   HP:", dhp, "/", dhpmax
LOCATE 3, 13
PRINT "MP:", dmp, "/", dmpmax
LOCATE 4, 5
PRINT "WEAPON: ", dwname$, " ARMOR: ", daname$
LOCATE 5, 5
PRINT "LEVEL:"; dlev; "    EXPERIENCE POINTS:"; dex
IF characters = 3 THEN
LOCATE 6, 5
PRINT "NEON    HP:", nhp, "/", nhpmax
LOCATE 7, 13
PRINT "MP:", nmp, "/", nmpmax
LOCATE 8, 5
PRINT "WEAPON:", nwname$, " ARMOR: ", naname$
LOCATE 9, 5
PRINT "LEVEL:"; nlev; "    EXPERIENCE POINTS:"; nex
LOCATE 10, 5
PRINT "THESSE  HP:", thp, "/", thpmax
LOCATE 11, 13
PRINT "MP:", tmp, "/", tmpmax
LOCATE 12, 5
PRINT "WEAPON:", twname$, " ARMOR: ", taname$
LOCATE 13, 5
PRINT "LEVEL:"; tlev; "    EXPERIENCE POINTS:"; tex
END IF

LOCATE 15, 5
PRINT "MONEY:", money
LOCATE 16, 5
PRINT "BLOOD RAIN ORBS:", bro
IF portal > 1 THEN COLOR 14 ELSE COLOR 8
LOCATE 18, 5
PRINT "      STAFF OF BALANCE - є"
IF portal > 2 THEN COLOR 14 ELSE COLOR 8
LOCATE 19, 5
PRINT "      RING OF CHAOS    - o"
COLOR 8
LOCATE 20, 5
PRINT "      ALPHA CHAIN      - р"

LOCATE 21, 38
PRINT "USE?"

DO
a$ = INKEY$
IF UCASE$(a$) = "Y" THEN GOTO 1.5
IF UCASE$(a$) = "N" THEN GOTO 1
LOOP

1.5
CLS
IF file = 1 THEN OPEN "save1.dat" FOR OUTPUT AS #1
IF file = 2 THEN OPEN "save2.dat" FOR OUTPUT AS #1
IF file = 3 THEN OPEN "save3.dat" FOR OUTPUT AS #1
        WRITE #1, 1, 0, file
        WRITE #1, 500, 500, 225, 225
        WRITE #1, 25, 5000, 1, 1
        WRITE #1, 450, 450, 250, 250
        WRITE #1, 25, 5000, 1, 1
        WRITE #1, 550, 550, 225, 225
        WRITE #1, 25, 5000, 1, 1
        WRITE #1, portal, money
        CLOSE #1
       
OPEN "data.dat" FOR OUTPUT AS #1
        WRITE #1, 1, 0, file
        WRITE #1, 500, 500, 225, 225
        WRITE #1, 25, 5000, 1, 1
        WRITE #1, 450, 450, 250, 250
        WRITE #1, 25, 5000, 1, 1
        WRITE #1, 550, 550, 225, 225
        WRITE #1, 25, 5000, 1, 1
        WRITE #1, 1, 250
        CLOSE #1

CHAIN "town.hp3"


2
CLS
PAINT (1, 1), 1
COLOR 9, 1
PRINT "Which file would you like to load?"
PRINT "File (1), File (2), File (3), or (N)one ?"
PRINT "(You can view each file before you use it)"

DO
a$ = INKEY$
IF a$ = "1" THEN file = 1: GOTO 2.1
IF a$ = "2" THEN file = 2: GOTO 2.1
IF a$ = "3" THEN file = 3: GOTO 2.1
IF UCASE$(a$) = "N" THEN CLS : options: GOTO 0
LOOP

2.1
IF file = 1 THEN OPEN "save1.dat" FOR INPUT AS #1
IF file = 2 THEN OPEN "save2.dat" FOR INPUT AS #1
IF file = 3 THEN OPEN "save3.dat" FOR INPUT AS #1
        INPUT #1, characters, bro, savefile
        INPUT #1, dhp, dhpmax, dmp, dmpmax
        INPUT #1, dlev, dex, dweapon, darmor
        INPUT #1, nhp, nhpmax, nmp, nmpmax
        INPUT #1, nlev, nex, nweapon, narmor
        INPUT #1, thp, thpmax, tmp, tmpmax
        INPUT #1, tlev, tex, tweapon, tarmor
        INPUT #1, portal, money
        CLOSE #1
      
CLS
COLOR 4, 12
LOCATE 1, 37
PRINT "FILE #", file
IF dweapon = 1 THEN dwname$ = "EXO-STARS" ELSE dwname$ = "ENTITY STARS"
IF darmor = 1 THEN daname$ = "EXO-VEST" ELSE daname$ = "ENTITY MAIL"
IF characters = 3 THEN
IF nweapon = 1 THEN nwname$ = "EXO-STARS" ELSE nwname$ = "ENTITY STARS"
IF narmor = 1 THEN naname$ = "EXO-VEST" ELSE naname$ = "ENTITY MAIL"
IF tweapon = 1 THEN twname$ = "EXO-SWORD"
IF tweapon = 2 THEN twname$ = "ENTITY BLADE"
IF tweapon = 3 THEN twname$ = "DREAM SWORD"
IF tarmor = 1 THEN taname$ = "FIRE CAPE" ELSE taname$ = "ENTITY DRESS"
END IF

LOCATE 2, 5
COLOR 15, 2
PRINT "DAVID   HP:", dhp, "/", dhpmax
LOCATE 3, 13
PRINT "MP:", dmp, "/", dmpmax
LOCATE 4, 5
PRINT "WEAPON: ", dwname$, " ARMOR: ", daname$
LOCATE 5, 5
PRINT "LEVEL:"; dlev; "    EXPERIENCE POINTS:"; dex

IF characters = 3 THEN
LOCATE 6, 5
PRINT "NEON    HP:", nhp, "/", nhpmax
LOCATE 7, 13
PRINT "MP:", nmp, "/", nmpmax
LOCATE 8, 5
PRINT "WEAPON:", nwname$, " ARMOR: ", naname$
LOCATE 9, 5
PRINT "LEVEL:"; nlev; "    EXPERIENCE POINTS:"; nex
LOCATE 10, 5
PRINT "THESSE  HP:", thp, "/", thpmax
LOCATE 11, 13
PRINT "MP:", tmp, "/", tmpmax
LOCATE 12, 5
PRINT "WEAPON:", twname$, " ARMOR: ", taname$
LOCATE 13, 5
PRINT "LEVEL:"; tlev; "    EXPERIENCE POINTS:"; tex
END IF

LOCATE 14, 5
PRINT "MONEY:", money
LOCATE 15, 5
PRINT "BLOOD RAIN ORBS:", bro
IF portal > 1 THEN COLOR 14 ELSE COLOR 8
LOCATE 17, 5
PRINT "      STAFF OF BALANCE - є"
IF portal > 2 THEN COLOR 14 ELSE COLOR 8
LOCATE 18, 5
PRINT "      RING OF CHAOS    - o"
COLOR 8
LOCATE 19, 5
PRINT "      ALPHA CHAIN      - р"
LOCATE 21, 38
PRINT "LOAD?"

DO
a$ = INKEY$
IF UCASE$(a$) = "Y" THEN GOTO 2.5
IF UCASE$(a$) = "N" THEN GOTO 2
LOOP

2.5
IF file = 1 THEN OPEN "save1.dat" FOR INPUT AS #1
IF file = 2 THEN OPEN "save2.dat" FOR INPUT AS #1
IF file = 3 THEN OPEN "save3.dat" FOR INPUT AS #1
        INPUT #1, characters, bro, savefile
        INPUT #1, dhp, dhpmax, dmp, dmpmax
        INPUT #1, dlev, dex, dweapon, darmor
        INPUT #1, nhp, nhpmax, nmp, nmpmax
        INPUT #1, nlev, nex, nweapon, narmor
        INPUT #1, thp, thpmax, tmp, tmpmax
        INPUT #1, tlev, tex, tweapon, tarmor
        INPUT #1, portal, money
        CLOSE #1

OPEN "data.dat" FOR OUTPUT AS #1
        WRITE #1, characters, bro, savefile
        WRITE #1, dhp, dhpmax, dmp, dmpmax
        WRITE #1, dlev, dex, dweapon, darmor
        WRITE #1, nhp, nhpmax, nmp, nmpmax
        WRITE #1, nlev, nex, nweapon, narmor
        WRITE #1, thp, thpmax, tmp, tmpmax
        WRITE #1, tlev, tex, tweapon, tarmor
        WRITE #1, portal, money
        CLOSE #1

COLOR 0, 0
CHAIN "town.hp3"

3
intro
options
c = 78
GOTO 0

4
instructions
options
GOTO 0
5
about
options
GOTO 0
10
END



125
CLS
COLOR 15, 1
PRINT "     Welcome to the ultimate room.  You are a great detective (unless you got"
PRINT "this from the strategy guide!), and you shall now know the secret production"
PRINT "that will soon come to you, courtesy of FIG:"
PRINT
PRINT "     STEALTH BOMBER, a flight simulator game similar to Starfox.  Three levels"
PRINT "of amazing plane fighting, all using different planes and different enemies"
PRINT "for each level.  And YES, I have proposed for a sequel with MUCH better "
PRINT "graphics, control, enemies, weapons, and MORE!"
PRINT
PRINT "     Well, did you expect something else?"
PRINT
PRINT "     Be happy about this!  This is a fun game!"
PRINT
PRINT "     For those of you who did not enjoy that, I'll try my hardest to make you"
PRINT "happy.  All you have to do is press ENTER!"

DO
a$ = INKEY$
IF a$ = CHR$(13) THEN es: CLS : COLOR 0, 0: options: GOTO 0
LOOP

SUB about
SCREEN 8
CLS
COLOR 10, 2
PRINT "     FIG Inc. was founded in Mid '96.  Anthony Figliuzzi founded it.  In"
PRINT "Early '97, FIG Inc. completed it's first game, DUNGEON MASTER, and then went"
PRINT "on to create other QBasic games.  So far, FIG has never asked for any money"
PRINT "for their productions.  There are now multiple members.  Anyone can join"
PRINT "FIG.  All you have to do is send me a sample of your QBasic work, and tell"
PRINT "me what type of games you like to play/program.  FIG Inc. also likes e-mail"
PRINT "dealing with our games.  Tell us what you like/hate about any/all of our"
PRINT "games."
PRINT "     FIG will continue to grow in members.  There are so many benefits to"
PRINT "becoming a member of FIG.  You will get members only QBasic drivers, and "
PRINT "other basic stuff.  You can be the first to try out any FIG game.  You get"
PRINT "the NEW FIG logo with permission to change the palettes.  A character creator,"
PRINT "FIG's 3D driver (used for DOOM type games), graphics, animations, and so"
PRINT "much more!  All you have to do is join (and it's FREE to join, too.)"
PRINT "     I know that most of you are interested in FIG's 3D Driver.  Let me "
PRINT "expand on it: no flashing screen, multiple rooms, items, hallways, enemies,"
PRINT "and much more!  You may have seen 3D drivers before, but they probably were"
PRINT "always flashing.  Well, mine doesn't!  Crisp, 3D background.  You can be the"
PRINT "first to preview it as soon as BIRTH OF THE DEVIL 2 is done!  In BIRTH OF THE"
PRINT "DEVIL 2, I didn't feel like making so many commands, so it's just three big"
PRINT "battle fields, each with tons of enemies."
PRINT "     So, now do you want to join?"
SLEEP
CLS
COLOR 0, 0
END SUB

SUB border
PAINT (1, 1), 4
LINE (10, 5)-(630, 195), 12, BF
LINE (15, 10)-(625, 190), 2, BF

END SUB

SUB cross
COLOR 4
LINE (250, 160)-(390, 160)
LINE -(350, 120)
LINE -(290, 120)
LINE -(250, 160)
PAINT (300, 122), 4
COLOR 12
LINE (250, 160)-(390, 160)
LINE -(350, 120)
LINE -(290, 120)
LINE -(250, 160)
COLOR 14
LINE (290, 120)-(350, 120)
LINE -(350, 60)
LINE -(420, 60)
LINE -(420, 40)
LINE -(350, 40)
LINE -(350, 10)
LINE -(290, 10)
LINE -(290, 40)
LINE -(220, 40)
LINE -(220, 60)
LINE -(290, 60)
LINE -(290, 120)
PAINT (320, 60), 14
COLOR 1
LINE (290, 120)-(350, 120)
LINE -(350, 60)
LINE -(420, 60)
LINE -(420, 40)
LINE -(350, 40)
LINE -(350, 10)
LINE -(290, 10)
LINE -(290, 40)
LINE -(220, 40)
LINE -(220, 60)
LINE -(290, 60)
LINE -(290, 120)
END SUB

SUB crystal (x, y)

COLOR 1
LINE (x, y)-(x + 15, y + 7)
LINE -(x, y + 15)
LINE -(x - 15, y + 7)
LINE -(x, y)
PAINT (x, y + 7), 1
COLOR 9
LINE (x, y)-(x + 15, y + 7)
LINE -(x, y + 15)
LINE -(x - 5, y + 7)
LINE -(x, y)
LINE (x - 15, y + 7)-(x + 15, y + 7)
COLOR 11
LINE (x + 1, y)-(x + 16, y + 7)
LINE -(x + 1, y + 15)
END SUB

SUB dl
FOR d = 1 TO 2000
NEXT d
END SUB

SUB es

PLAY "l9o2el6o1el8el10o2eo1bl6a#"
PLAY "l9o2el6o1el8el10o2eo1bl6a#"
PLAY "l9o2el6o1el8el10o2eo1bl6a#"
PLAY "l7o1egl9ef#gf#gf#l5eL9e"
PLAY "l10o2eo1bl6a#"
PLAY "l9o2el6o1el8el10o2eo1bl6a#"
PLAY "l9o2el6o1el8el10o2eo1bl6a#"
PLAY "l7o1egl9ef#gf#gf#l5eL9e"

PRINT "     Enter Sandman by Metallica!  That BETTER have made you happy!"
SLEEP
END SUB

SUB explosions (x, y)
FOR a = 1 TO 46
c = INT(RND * 2) + 1
IF c = 1 THEN c = 4 ELSE c = 12
CIRCLE (x, y), a, c
SOUND 50 + a, .06
NEXT a
END SUB

SUB fastexplosions (x, y)
FOR a = 1 TO 22
CIRCLE (x, y), a, 4
SOUND 50 + a, .04
NEXT a
END SUB

SUB fp
COLOR 1
LINE (100, 100)-(130, 250), , BF
LINE (100, 100)-(200, 130), , BF
LINE (100, 160)-(180, 190), , BF
LINE (270, 100)-(370, 130), , BF
LINE (305, 100)-(335, 250), , BF
LINE (270, 220)-(370, 250), , BF
LINE (440, 100)-(540, 130), , BF
LINE (440, 100)-(470, 250), , BF
LINE (470, 250)-(540, 220), , BF
LINE (540, 250)-(510, 200), , BF
LINE (540, 200)-(490, 170), , BF
COLOR 15
LOCATE 23, 37
PRINT "PRESENTS"
END SUB

SUB instructions
CLS
COLOR 9, 1
PRINT "     If you still need to read instructions after playing all three of the"
PRINT "HELLPIT games, then I mock you.  If you are playing HELLPIT III before HELLPIT"
PRINT "II or I, then I suggest that you get on AOL and download HELLPIT I and II."
PRINT "go to SOFTWARE SEARCH: PANTERA55 or FIG, or else, just e-mail me at"
PRINT "PANTERA55@aol.com.  I'll tell you how to play anyways..."
PRINT "     The object of the game is to get the staff of balance to open the second"
PRINT "portal, the ring of chaos to open the third portal, and the alpha chain to"
PRINT "cast away the HELLPIT."
PRINT "     Use the arrow keys to move.  Enter brings up the status screen.  To   "
PRINT "do someting in battle, press the key corresponding to the move that you"
PRINT "would like to do.  Press ESCAPE to exit.  Don't die.  That's it."
PRINT "     P.S There are 5 secrets in this game!"
SLEEP
CLS
COLOR 0, 0
END SUB

SUB intro
SCREEN 8

CLS

PAINT (1, 1), 4
COLOR 15, 4
PRINT "     After Neon, Thesse and David got all of the crystals, they thought that "
PRINT "the evil was over.  Now, disturbances all over their town have brought "
PRINT "suspicions.  Perhaps the evil is not dead.  Perhaps it never will be..."
PRINT "     After Neon and Thesse decided to take a vacation to a nearby town, David"
PRINT "was noticing that very wierd things were happening.  The castle would flash"
PRINT "with bright red lights, there were illusions of the town on fire, and some "
PRINT "reported blood dripping down the sacred cross located in the middle if the "
PRINT "castle.  Neon and Thesse were going to be back soon, so David thought that it"
PRINT "would be no harm to check it out for himself.  The night before Neon and "
PRINT "Thesse were to arrive, David went into the castle to examine these "
PRINT "occurances..."
SLEEP

COLOR 0, 0

CLS

x = -120
xx = 760
y = -80
yy = 200

3.1
CLS
WINDOW SCREEN (x, y)-(xx, yy)
cross

dl

x = x + 7.8
xx = xx - 7.8
y = y + 2.5
yy = yy - 2.5

IF y > 20 THEN GOTO 3.2
GOTO 3.1

3.2
explosions 220, 50
explosions 420, 50
explosions 320, 50

FOR n = 1 TO 20
e = 220 + INT(RND * 210) + 1
f = 20 + INT(RND * 80) + 1
fastexplosions e, f
NEXT n

FOR n = 1 TO 9
explosions 320, 50
NEXT n

FOR a = 1 TO 150
CIRCLE (320, 50), a, 4
SOUND 50 + a, .04
NEXT a

WINDOW SCREEN (0, 0)-(640, 200)

PAINT (1, 1), 4
COLOR 15, 4
PRINT "     A new portal opened inside the castle.  A spectrum of bright red lights"
PRINT "shone upon David's eyes.  David ran to his hut to see if the crystals were"
PRINT "affected by the portal..."
SLEEP

COLOR 0, 0

CLS

PAINT (1, 1), 4

FOR e = 1 TO 12
crystal 320, 40
crystal 200, 100
crystal 320, 160
crystal 440, 100
crystal 280, 100
crystal 340, 100
dl
LINE (0, 0)-(640, 200), 4, BF
NEXT e

FOR e = 1 TO 30
crystal 320, 40
crystal 200, 100
crystal 320, 160
crystal 440, 100
crystal 280, 100
crystal 340, 100
LINE (0, 0)-(640, 200), 4, BF
NEXT e

SCREEN 13

FOR a = 63 TO 0 STEP -.5
COLOR 0
PALETTE 0, a
NEXT a

SCREEN 8
CLS
PAINT (1, 1), 4
COLOR 15, 4
PRINT "     The crystals mysteriously disappeared.  David was unharmed, but he knew"
PRINT "that a new challenge was awaiting them.  Evil was raging with power.  The"
PRINT "skies filled with total darkness.  A total eclipse was sent to block the"
PRINT "moon.  The sun evapourized in the sky.  Drops of blood were raining down"
PRINT "on the town.  The people panicked, and ran for shelter.  Bolts of lightning"
PRINT "blasted down from the sky, and engulfed many small fires.  David decided to"
PRINT "take a look inside the portal to see what kind of deadly challenge awaited."
PRINT "While there was no one to lead the people, the Wise Man casted a shield on "
PRINT "the town to prevent further damage, but the shield was wearing down.  The "
PRINT "Wise Man was weak in power, and unless David quickly solves this problem, "
PRINT "evil will be victorious, and the town will engulf in flames...  "
SLEEP
COLOR 0, 0

CLS
END SUB

SUB nicecrystal (x, y)

COLOR 3
LINE (x, y)-(x + 25, y + 10)
LINE -(x, y + 20)
LINE -(x - 25, y + 10)
LINE -(x, y)
PAINT (x, y + 10), 3
COLOR 10
LINE (x, y)-(x + 25, y + 10)
LINE -(x, y + 20)
LINE -(x - 7, y + 10)
LINE -(x, y)
LINE (x - 25, y + 10)-(x + 25, y + 10)
LINE (x, y)-(x - 25, y + 10)
LINE -(x, y + 20)
LINE (x + 1, y)-(x + 26, y + 10)
LINE -(x + 1, y + 20)
END SUB

SUB options
SCREEN 8
CLS

border

word 200, 10, "H", 14
word 220, 10, "E", 14
word 240, 10, "L", 14
word 260, 10, "L", 14
word 280, 10, "P", 14
word 300, 10, "I", 14
word 320, 10, "T", 14

word 360, 10, "I", 14
word 375, 10, "I", 14
word 390, 10, "I", 14

word 240, 30, "O", 14
word 260, 30, "P", 14
word 280, 30, "T", 14
word 300, 30, "I", 14
word 320, 30, "O", 14
word 340, 30, "N", 14
word 360, 30, "S", 14

word 200, 80, "N", 9
word 220, 80, "E", 9
word 240, 80, "W", 9
word 280, 80, "G", 9
word 300, 80, "A", 9
word 320, 80, "M", 9
word 340, 80, "E", 9

word 200, 100, "L", 9
word 220, 100, "O", 9
word 240, 100, "A", 9
word 260, 100, "D", 9
word 300, 100, "G", 9
word 320, 100, "A", 9
word 340, 100, "M", 9
word 360, 100, "E", 9

word 200, 120, "I", 9
word 220, 120, "N", 9
word 240, 120, "T", 9
word 260, 120, "R", 9
word 280, 120, "O", 9
word 300, 120, "D", 9
word 320, 120, "U", 9
word 340, 120, "C", 9
word 360, 120, "T", 9
word 380, 120, "I", 9
word 400, 120, "O", 9
word 420, 120, "N", 9

word 200, 140, "I", 9
word 220, 140, "N", 9
word 240, 140, "S", 9
word 260, 140, "T", 9
word 280, 140, "R", 9
word 300, 140, "U", 9
word 320, 140, "C", 9
word 340, 140, "T", 9
word 360, 140, "I", 9
word 380, 140, "O", 9
word 400, 140, "N", 9
word 420, 140, "S", 9
         
word 200, 160, "A", 9
word 220, 160, "B", 9
word 240, 160, "O", 9
word 260, 160, "U", 9
word 280, 160, "T", 9
word 320, 160, "F", 9
word 340, 160, "I", 9
word 360, 160, "G", 9

END SUB

SUB setrgb (Nr, r, g, B)
  OUT &H3C8, Nr
  OUT &H3C9, r
  OUT &H3C9, g
  OUT &H3C9, B
END SUB

SUB sounds
FOR s = 200 TO 160 STEP -.5
SOUND s, .04
NEXT s
LINE (140, 60)-(180, 175), 2, BF
END SUB

SUB title

SCREEN 8

border

DRAW "bm200,50c4f20d60g20r240h20u60e20l240"
DRAW "bm240,60r40d80l40e5u70h5"
DRAW "bm400,60l40d80r40h5u70e5"
DRAW "bm300,60r40d80l40u80"
PAINT (210, 55), 4

DRAW "bm200,50c9f20d60g20r240h20u60e20l240"
DRAW "bm240,60r40d80l40e5u70h5"
DRAW "bm400,60l40d80r40h5u70e5"
DRAW "bm300,60r40d80l40u80"

word 220, 20, "H", 4
word 240, 20, "E", 4
word 260, 20, "L", 4
word 280, 20, "L", 4
word 300, 20, "P", 4
word 320, 20, "I", 4
word 340, 20, "T", 4

word 380, 20, "I", 14
word 395, 20, "I", 14
word 410, 20, "I", 14

word 210, 160, "F", 4
word 230, 160, "I", 4
word 250, 160, "N", 4
word 270, 160, "A", 4
word 290, 160, "L", 4

word 330, 160, "F", 4
word 350, 160, "L", 4
word 370, 160, "A", 4
word 390, 160, "M", 4
word 410, 160, "E", 4


crystal 100, 100
crystal 150, 30
crystal 150, 170
crystal 540, 100
crystal 490, 30
crystal 490, 170
nicecrystal 320, 90
END SUB

SUB word (x, y, l$, c)

SELECT CASE l$
        CASE IS = "A"
        LINE (x, y + 2)-(x, y), c
        LINE -(x + 15, y), c
        LINE -(x + 15, y + 10), c
        LINE -(x + 15, y + 6), c
        LINE -(x + 3, y + 6), c
        LINE (x + 3, y)-(x + 3, y + 10), c
        CASE IS = "B"
        LINE (x, y + 3)-(x + 5, y), c
        LINE (x + 5, y)-(x + 18, y + 5), c, B
        LINE -(x + 5, y + 10), c, B
        CASE IS = "C"
        LINE (x, y)-(x + 15, y), c
        LINE (x, y)-(x, y + 10), c
        LINE -(x + 15, y + 10), c
        CASE IS = "D"
        LINE (x, y)-(x + 13, y), c
        LINE -(x + 15, y + 2), c
        LINE -(x + 15, y + 8), c
        LINE -(x + 13, y + 10), c
        LINE -(x + 2, y + 10), c
        LINE -(x + 2, y), c
        CASE IS = "E"
        LINE (x, y)-(x + 15, y), c
        LINE (x, y + 10)-(x + 15, y + 10), c
        LINE (x + 3, y)-(x + 3, y + 10), c
        LINE (x + 3, y + 5)-(x + 12, y + 5), c
        CASE IS = "F"
        LINE (x, y)-(x + 15, y), c
        LINE (x + 3, y)-(x + 3, y + 10), c
        LINE (x + 3, y + 5)-(x + 12, y + 5), c
        CASE IS = "G"
        LINE (x, y)-(x + 15, y), c
        LINE -(x + 15, y + 3), c
        LINE (x, y)-(x, y + 10), c
        LINE -(x + 15, y + 10), c
        LINE -(x + 15, y + 6), c
        LINE -(x + 6, y + 6), c
        CASE IS = "H"
        LINE (x, y)-(x, y + 10), c
        LINE (x, y + 4)-(x + 15, y + 4), c
        LINE (x + 15, y)-(x + 15, y + 10), c
        CASE IS = "I"
        LINE (x, y + 3)-(x, y), c
        LINE -(x + 15, y), c
        LINE (x + 7, y)-(x + 7, y + 10), c
        LINE (x, y + 10)-(x + 15, y + 10), c
        CASE IS = "J"
        LINE (x, y + 3)-(x, y), c
        LINE -(x + 15, y), c
        LINE (x + 9, y)-(x + 9, y + 8), c
        LINE -(x + 5, y + 10), c
        LINE -(x + 2, y + 10), c
        LINE -(x, y + 7), c
        CASE IS = "K"
        LINE (x, y)-(x + 3, y), c
        LINE -(x + 3, y + 10), c
        LINE (x + 3, y + 5)-(x + 13, y), c
        LINE -(x + 15, y), c
        LINE (x + 3, y + 5)-(x + 15, y + 10), c
        CASE IS = "L"
        LINE (x, y)-(x + 3, y), c
        LINE -(x + 3, y + 10), c
        LINE (x, y + 10)-(x + 15, y + 10), c
        LINE -(x + 15, y + 7), c
        CASE IS = "M"
        LINE (x, y)-(x + 2, y), c
        LINE -(x + 2, y + 10), c
        LINE (x + 2, y)-(x + 9, y + 4), c
        LINE -(x + 15, y), c
        LINE -(x + 15, y + 10), c
        CASE IS = "N"
        LINE (x, y)-(x + 2, y), c
        LINE -(x + 2, y + 10), c
        LINE (x + 2, y)-(x + 15, y + 10), c
        LINE -(x + 15, y), c
        CASE IS = "O"
        LINE (x, y + 2)-(x, y), c
        LINE -(x + 2, y), c
        LINE (x + 2, y)-(x + 15, y + 10), c, B
        CASE IS = "P"
        LINE (x, y)-(x + 2, y), c
        LINE -(x + 15, y + 4), c, B
        LINE (x + 2, y)-(x + 2, y + 10), c
        CASE IS = "Q"
        LINE (x, y + 2)-(x, y), c
        LINE -(x + 2, y), c
        LINE (x + 2, y)-(x + 15, y + 10), c, B
        LINE (x + 12, y + 7)-(x + 15, y + 10), c
        CASE IS = "R"
        LINE (x, y)-(x + 2, y), c
        LINE -(x + 15, y + 4), c, B
        LINE (x + 2, y)-(x + 2, y + 10), c
        LINE (x + 5, y + 4)-(x + 15, y + 10), c
        CASE IS = "S"
        LINE (x + 15, y)-(x, y + 5), c
        LINE -(x + 15, y + 5), c
        LINE -(x, y + 10), c
        CASE IS = "T"
        LINE (x, y + 2)-(x, y), c
        LINE -(x + 15, y), c
        LINE (x + 7, y)-(x + 7, y + 10), c
        CASE IS = "U"
        LINE (x, y)-(x, y + 10), c
        LINE -(x + 15, y + 10), c
        LINE -(x + 15, y), c
        CASE IS = "V"
        LINE (x, y)-(x + 3, y), c
        LINE -(x + 9, y + 10), c
        LINE -(x + 15, y), c
        CASE IS = "W"
        LINE (x, y)-(x + 1, y), c
        LINE -(x + 1, y + 10), c
        LINE -(x + 8, y), c
        LINE -(x + 15, y + 10), c
        LINE -(x + 15, y), c
        CASE IS = "X"
        LINE (x, y)-(x + 1, y), c
        LINE -(x + 15, y + 10), c
        LINE (x, y + 10)-(x + 1, y + 10), c
        LINE -(x + 15, y), c
        CASE IS = "Y"
        LINE (x, y + 2)-(x, y), c
        LINE -(x + 1, y), c
        LINE -(x + 8, y + 4), c
        LINE -(x + 15, y), c
        LINE (x + 8, y + 4)-(x + 8, y + 10), c
        CASE IS = "Z"
        LINE (x, y + 2)-(x, y), c
        LINE -(x + 15, y), c
        LINE -(x, y + 10), c
        LINE -(x + 15, y + 10), c
        LINE -(x + 15, y + 8), c
END SELECT
END SUB

SUB writing

SCREEN 8
CLS
PAINT (1, 1), 9

DO
FOR c = 10 TO 14 STEP 4
COLOR c, 9
LOCATE 2, 22
PRINT "FIG Inc. proudly present HELLPIT III."
LOCATE 4, 5
PRINT "1997 / 1998 FIG Inc. presents HELLPIT III: Final Flame.  This game may not"
PRINT "be edited in any way, so please do not because I spent a lot of time on this"
PRINT "game.  I appreciate any feedback you may have.  Please send comments, questions,"
PRINT "etc. to PANTERA55@aol.com.  This game is free, so please do not edit the files."
PRINT
PRINT "     I hope that you are visiting my Web Site.  If you do not already know,"
PRINT "the address is"
LOCATE 10, 16
COLOR 1, 9
PRINT "http://members.aol.com/PANTERA55."
LOCATE 12, 1
COLOR c, 9
PRINT "     I am always open to new members of FIG.  Currently, there are many members"
PRINT "of FIG, and I hope to expand on FIG Inc.  I am a frequent programmer of RPG's,"
PRINT "but I program other games as well.  If you want to become a member, then"
PRINT "please e-mail me with a sample of your work.  I will not reject anyone."
PRINT
PRINT "     My plans for HELLPIT are to put all three of the games into one zip file,"
PRINT "and upload it to AOL, and also attach it to my Web Page.  This is the final "
PRINT "game of the HELLPIT trilogy, but soon I will be making ELYSIAN FIELDS, another"
PRINT "RPG.  ELYSIAN FIELDS will be bigger than all three HELLPIT games combined."

NEXT c
LOOP UNTIL INKEY$ <> ""

END SUB


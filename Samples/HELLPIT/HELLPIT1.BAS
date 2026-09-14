'Please use HELLPIT1.BAT to run this game or you will get errors.
'
'   Hellpit Version 1.0 By Fig     CapsLock ON         Numlock ON           
'   RPG with graphics, animations, and much more.  Try to get all of the    
'   's to close the portal connecting hell and earth before it's too late  
'   Neon - Mage (Uses magic)       Thesse - Cleric (Heals, magics, power)   
'   Use the numeric pad to move   Enter to see status    Esc to exit        
'   * You MUST buy weapons first.  You start out with only 250 gold!        
'   * Some areas do not allow the use of certain magics.                    
'   ARMORS               WEAPONS(Thesse)           WEAPONS(Neon)            
'   Radical Armor        Thrust Blade              Sess Sais                
'   Tri Plate            Stealth Sword             Soul Stars               
'   Mystic Armor         Z Sword                   Z Sais                   
'                                                                           
'   PORTALS:  RUINS (1 <-), SPIRIT LANDS (2 ->), THE DEPTHS (3 )           
'



DECLARE SUB satan ()
DECLARE SUB flames ()
DECLARE SUB zheal ()
DECLARE SUB zvolt ()
DECLARE SUB boss3 ()
DECLARE SUB ultra ()
DECLARE SUB darkangel ()
DECLARE SUB firebat ()
DECLARE SUB depths ()
DECLARE SUB birdhead ()
DECLARE SUB omega ()
DECLARE SUB dragonfly ()
DECLARE SUB spirit ()
DECLARE SUB spiritland ()
DECLARE SUB blast ()
DECLARE SUB boss1 ()
DECLARE SUB tattack ()
DECLARE SUB cover ()
DECLARE SUB stars ()
DECLARE SUB sai ()
DECLARE SUB fire ()
DECLARE SUB shot ()
DECLARE SUB wounds ()
DECLARE SUB heal ()
DECLARE SUB nthrow ()
DECLARE SUB tundra ()
DECLARE SUB fury ()
DECLARE SUB cecilslime ()
DECLARE SUB undead ()
DECLARE SUB resurrect ()
DECLARE SUB bump ()
DECLARE SUB ruins ()
DECLARE SUB desk ()
DECLARE SUB town ()
DECLARE SUB thesse ()
DECLARE SUB neon ()
DECLARE SUB border ()
DECLARE SUB quit ()
DECLARE SUB intro ()
DECLARE SUB instructions ()
DECLARE SUB main ()
DECLARE SUB fp ()
DECLARE SUB title ()
nhpmax = 23: thpmax = 32: nmpmax = 4: tmpmax = 4: gold = 250
nlev = 1: tlev = 1: hearts = 0: nweapon = 0: tweapon = 0
narmor = 0: tarmor = 0: nex = 0: tex = 0: nabil1 = 0: nabil2 = 0: nabil3 = 0
tabil1 = 0: tabil2 = 0: tabil3 = 0:
nhp = nhpmax: thp = thpmax: nmp = nmpmax: tmp = tmpmax
cecilslimehp = 4: undeadhp = 6: resurrecthp = 15: boss1hp = 65
dragonflyhp = 24: spirithp = 32: boss2hp = 610
darkangelhp = 92: firebathp = 107: boss3hp = 2000: satanhp = 3500
RANDOMIZE TIMER

fp
title
main
B = 9

0.1
main
LOCATE B, 31
PRINT CHR$(16)
SOUND 100, .62
SOUND 39, .46

0.11
a$ = INKEY$
IF a$ = "2" THEN B = B + 2: GOTO 0.12 ELSE
IF a$ = "8" THEN B = B - 2: GOTO 0.12 ELSE
IF a$ = CHR$(13) THEN GOTO 0.13 ELSE
GOTO 0.11

0.12
IF B = 19 THEN SOUND 155, 1: B = B - 2: GOTO 0.11 ELSE
IF B = 7 THEN SOUND 155, 1: B = B + 2: GOTO 0.11 ELSE
GOTO 0.1

0.13
IF B = 9 THEN GOTO 0.99 ELSE
IF B = 11 THEN GOTO 0.2 ELSE
IF B = 13 THEN GOTO 0.3 ELSE
IF B = 15 THEN GOTO 0.4 ELSE
IF B = 17 THEN GOTO 0.5 ELSE

0.2
instructions
B = 9
GOTO 0.1

0.3
intro
B = 9
GOTO 0.1
                                                                                                                                                            
0.4
OPEN "I", 1, "WHO.inf"
INPUT #1, nhpmax, thpmax, nmpmax, tmpmax, nabil1, nabil2, nabil3, tabil1, tabil2, tabil3, gold, nweapon, tweapon, narmor, tarmor, nex, tex, nlev, tlev, hearts: CLOSE
thp = thpmax: nhp = nhpmax: nmp = nmpmax: tmp = tmpmax
GOTO 0.99

0.5
CALL quit

0.99
SCREEN 8, 0
CLS
PRINT "     SOME NOTES BEFORE PLAYING:"
PRINT "BUY WEAPONS FIRST, OR YOU CANNOT DEFEND YOURSELVES!!!!!"
PRINT "GO INTO THE LANDS IN ORDER OF NUMBER SO YOU DON'T DIE!"
PRINT "  GOES TO RUINS (1) (YOU SHOULD GO THERE FIRST)"
PRINT "  GOES TO SPIRITLANDS (2) (PRETTY HARD)"
PRINT "  GOES TO THE DEPTHS (3) (DON'T GO UNLESS YOU'RE BUFFED!)"
PRINT " For a real challenge, try beating the second portal first!"
PRINT "NOW PRESS THE SPACE BAR THREE TIMES TO START!"
SLEEP
SLEEP
SLEEP
CLS
mx = 40
my = 12

1
town
LOCATE my, mx
PRINT ""
GOTO 1.1

1.1
a$ = INKEY$
IF a$ = "8" THEN my = my - 1: GOTO 1.2 ELSE
IF a$ = "2" THEN my = my + 1: GOTO 1.2 ELSE
IF a$ = "4" THEN mx = mx - 1: GOTO 1.2 ELSE
IF a$ = "6" THEN mx = mx + 1: GOTO 1.2 ELSE
IF a$ = CHR$(0) + ";" THEN GOTO 1.15
IF a$ = CHR$(27) THEN GOSUB sure: GOTO 1 ELSE
IF a$ = CHR$(13) THEN GOSUB 900: GOTO 1 ELSE
GOTO 1.1

1.15
CLS
desk
COLOR 15
LOCATE 3, 50
PRINT "*FIG'S SECRET ROOM*"
LOCATE 6, 35
PRINT "How may I help you?"
LOCATE 8, 35
PRINT "(R)estore HP and MP for both"
LOCATE 9, 35
PRINT "(B)est Weapons and armors"
LOCATE 10, 35
PRINT "(S)ave game"
LOCATE 11, 35
PRINT "(L)eave"
LOCATE 12, 35
PRINT "Well?"

DO: a$ = UCASE$(INKEY$)
IF a$ = "R" THEN thp = thpmax: tmp = tmpmax: nhp = nhpmax: nmp = nmpmax: GOTO 1
IF a$ = "B" THEN tweapon = 3: nweapon = 3: tarmor = 3: narmor = 3: GOTO 1
IF a$ = "S" THEN
        OPEN "O", 1, "WHO.inf"
        WRITE #1, nhpmax, thpmax, nmpmax, tmpmax, nabil1, nabil2, nabil3, tabil1, tabil2, tabil3, gold, nweapon, tweapon, narmor, tarmor, nex, tex, nlev, tlev, hearts: CLOSE
        GOTO 1
        END IF
IF a$ = "L" THEN GOTO 1
LOOP

1.2
IF mx < 3 THEN bump: mx = mx + 1
IF mx > 78 THEN bump: mx = mx - 1
IF my < 2 THEN bump: my = my + 1
IF my > 23 THEN bump: my = my - 1
IF my = 20 THEN IF mx < 36 THEN IF mx > 33 THEN GOTO 1.3
IF my = 14 THEN IF mx < 16 THEN IF mx > 13 THEN GOTO 1.4
IF my = 13 THEN IF mx < 72 THEN IF mx > 69 THEN GOTO 1.5
IF my = 8 THEN IF mx > 36 THEN IF mx < 45 THEN GOTO 1.6
IF my = 13 THEN IF mx < 42 THEN IF mx > 39 THEN GOTO 2
GOTO 1

1.3
border
desk
COLOR 15
LINE (270, 40)-(560, 86), , B
LOCATE 8, 42
PRINT "WOULD YOU LIKE TO SPEND"
LOCATE 9, 42
PRINT "  THE NIGHT FOR $10?   "
LOCATE 10, 45
PRINT "(Y) or (N)?"
DO
AS$ = UCASE$(INKEY$)
IF AS$ = "N" THEN my = my + 1: GOTO 1 ELSE
IF AS$ = "Y" THEN nhp = nhpmax: nmp = nmpmax: thp = thpmax: tmp = tmpmax: gold = gold - 10: GOTO 1.31 ELSE
LOOP

1.31
OPEN "O", 1, "WHO.inf"
WRITE #1, nhpmax, thpmax, nmpmax, tmpmax, nabil1, nabil2, nabil3, tabil1, tabil2, tabil3, gold, nweapon, tweapon, narmor, tarmor, nex, tex, nlev, tlev, hearts: CLOSE
LOCATE 10, 38
PRINT "YOUR GAME IS ALSO SAVED"
SLEEP 8
my = my + 1
GOTO 1

1.4
border
desk
COLOR 15
LINE (270, 40)-(565, 120), , B
LOCATE 7, 37
PRINT "WE DEAL IN MANY WEAPONS AND ARMORS"
LOCATE 8, 37
PRINT "(R)adical Armor  (BOTH)   $100"
LOCATE 9, 37
PRINT "(T)hrust Blade  (THESSE)   $90"
LOCATE 10, 37
PRINT "(S)tealth Blade (THESSE) $ 200"
LOCATE 11, 37
PRINT "S(E)ss Sais      (NEON)    $85"
LOCATE 12, 37
PRINT "S(O)ul Stars     (NEON)  $ 190"
LOCATE 13, 37
PRINT "OR (N) IF YOU DON'T WANT ANYTHING"
DO
ax$ = UCASE$(INKEY$)
IF ax$ = "N" THEN GOTO 1.42 ELSE
IF ax$ = "R" THEN GOTO 1.41 ELSE
IF ax$ = "T" THEN IF gold > 89 THEN IF tweapon = 0 THEN COLOR 15: LOCATE 14, 42: PRINT "THANK YOU": gold = gold - 90: tweapon = 1: SLEEP 3: GOTO 1.42 ELSE LOCATE 14, 37: PRINT "I'M AFRAID YOU CAN'T DO THAT": SLEEP 3: GOTO 1.42 ELSE
IF ax$ = "S" THEN IF gold > 199 THEN IF tweapon <= 1 THEN COLOR 15: LOCATE 14, 42: PRINT "THANK YOU": gold = gold - 200: tweapon = 2: SLEEP 3: GOTO 1.42 ELSE LOCATE 14, 37: PRINT "I'M AFRAID YOU CAN'T DO THAT": SLEEP 3: GOTO 1.42 ELSE
IF ax$ = "E" THEN IF gold > 84 THEN IF nweapon = 0 THEN COLOR 15: LOCATE 14, 42: PRINT "THANK TOU": gold = gold - 85: nweapon = 1: SLEEP 3: GOTO 1.42 ELSE LOCATE 14, 37: PRINT "I'M AFRAID YOU CAN'T DO THAT": SLEEP 3: GOTO 1.42 ELSE
IF ax$ = "O" THEN IF gold > 189 THEN IF nweapon <= 1 THEN COLOR 15: LOCATE 14, 42: PRINT "THANK YOU": gold = gold - 190: nweapon = 2: SLEEP 3: GOTO 1.42 ELSE LOCATE 14, 37: PRINT "I'M AFRAID YOU CAN'T DO THAT": SLEEP 3: GOTO 1.42 ELSE
LOOP

1.41
CLS
border
desk
COLOR 15
LINE (270, 40)-(565, 120), , B
IF ax$ = "R" THEN IF gold >= 100 THEN LOCATE 7, 37: COLOR 15: PRINT "WHO WILL TAKE THIS?": LOCATE 8, 40: PRINT "(N)eon or (T)hesse?" ELSE GOTO 1.4
DO
ac$ = UCASE$(INKEY$)
IF ac$ = "N" THEN IF narmor = 0 THEN LOCATE 9, 42: PRINT "THANK YOU": gold = gold - 100: narmor = 1: SLEEP 3: GOTO 1.42 ELSE LOCATE 9, 40: PRINT "HIS ARMOR IS ALREADY BETTER": SLEEP 3: GOTO 1.42
IF ac$ = "T" THEN IF tarmor = 0 THEN LOCATE 9, 42: PRINT "THANK YOU": gold = gold - 100: tarmor = 1: SLEEP 3: GOTO 1.42 ELSE LOCATE 9, 40: PRINT "HER ARMOR IS ALREADY BETTER": SLEEP 3: GOTO 1.42
LOOP

1.42
my = my + 1
GOTO 1

1.5
border
desk
COLOR 15
LINE (270, 40)-(565, 120), , B
LOCATE 7, 38
PRINT "I AM A TEACHER OF ABILITIES"
IF nabil1 = 0 THEN LOCATE 8, 38: PRINT "I CAN TEACH NEON FURY": yes = 1
IF nabil1 <> 0 THEN IF nlev > 5 THEN IF nabil2 = 0 THEN LOCATE 8, 38: PRINT "I CAN TEACH NEON TUNDRA": yes = 2
IF nabil2 <> 0 THEN IF nlev > 11 THEN IF nabil3 = 0 THEN LOCATE 8, 38: PRINT "I CAN TEACH NEON Z-VOLT": yes = 3
IF tabil1 = 0 THEN LOCATE 9, 38: PRINT "I CAN TEACH THESSE HEAL": yess = 1
IF tabil1 <> 0 THEN IF tlev > 4 THEN IF tabil2 = 0 THEN LOCATE 9, 38: PRINT "I CAN TEACH THESSE WOUNDS": yess = 2
IF tabil2 <> 0 THEN IF tlev > 12 THEN IF tabil3 = 0 THEN LOCATE 9, 38: PRINT "I CAN TEACH THESSE Z-HEAL": yess = 3
IF tlev = 15 THEN IF nlev = 15 THEN LOCATE 9, 38: PRINT "YOU TWO ARE FULLY TRAINED": LOCATE 11, 38: PRINT "I HAVE NOTHING MORE TO TEACH YOU": SLEEP: my = my + 1: GOTO 1
IF yes = 0 THEN IF yess = 0 THEN LOCATE 10, 38: PRINT "I CAN'T TEACH ANYONE RIGHT NOW": LOCATE 11, 38: PRINT "COME BACK WHEN YOUR LEVELS ARE UP": SLEEP 15: my = my + 1: GOTO 1
LOCATE 10, 39
PRINT "(Y) or (N)?"
DO
ab$ = UCASE$(INKEY$)
IF ab$ = "Y" THEN GOTO 1.51
IF ab$ = "N" THEN my = my + 1: GOTO 1
LOOP

1.51
IF yes = 1 THEN nabil1 = 1
IF yes = 2 THEN nabil2 = 1
IF yes = 3 THEN nabil3 = 1
IF yess = 1 THEN tabil1 = 1
IF yess = 2 THEN tabil2 = 1
IF yess = 3 THEN tabil3 = 1
yes = 0: yess = 0: my = my + 1: GOTO 1

1.6
border
COLOR 15
LINE (30, 20)-(610, 180), , B
LOCATE 5, 11
PRINT "A NOTE TO ALL:"
LOCATE 7, 6
PRINT "I HAVE SEALED OFF THE CASTLE AND IT'S SURROUNDINGS FROM THE"
LOCATE 8, 6
PRINT "MAJOR PARTS OF TOWN.  WE DO NOT WANT THE DEMONS TO GET INTO"
LOCATE 9, 6
PRINT "OUR TOWN.  I, PRINCE NEON, ALONG WITH THESSE WILL PUT AND END"
LOCATE 10, 6
PRINT "TO THIS HELL PORTAL AND ONCE AGAIN RESTORE PEACE TO OUR TOWN"
LOCATE 17, 55
PRINT "PRINCE NEON"
SLEEP 50
my = my + 1
GOTO 1

sure:
COLOR 15
LOCATE 13, 37
PRINT "REALLY QUIT?"
DO
xz$ = UCASE$(INKEY$)
IF xz$ = "Y" THEN CALL quit ELSE
IF xz$ = "N" THEN RETURN ELSE
LOOP

2       
CLS
COLOR 15
PRINT "     Left is easy, right is medium, up is hard!"
LOCATE 15, 40
PRINT ""
LOCATE 16, 40
PRINT ""
COLOR 4
LOCATE 15, 39
PRINT ""
LOCATE 14, 40
PRINT ""
LOCATE 15, 41
PRINT ""
IF hearts = 3 THEN LOCATE 15, 39: PRINT "": LOCATE 1, 1: PRINT "Step into the left portal if you wish to oppose me!"
DO
g$ = INKEY$
IF g$ = "4" THEN IF hearts < 3 THEN place = 1: GOTO 2.001 ELSE GOTO 6
IF g$ = "6" THEN place = 2: GOTO 2.001
IF g$ = "8" THEN place = 3: GOTO 2.001
IF g$ = "2" THEN my = my + 2: GOTO 1
LOOP

2.001
IF place = 1 THEN CLS : mx = 77: my = 23: GOTO 3
IF place = 2 THEN CLS : mx = 34: my = 23: GOTO 4
IF place = 3 THEN CLS : mx = 34: my = 23: GOTO 5

3
ruins
IF hearts = 0 THEN COLOR 12: LOCATE 2, 4: PRINT ""
LOCATE my, mx
COLOR 15
PRINT ""
GOTO 3.01

3.01
B$ = INKEY$
IF B$ = "2" THEN my = my + 1: GOTO 3.05 ELSE
IF B$ = "4" THEN mx = mx - 1: GOTO 3.05 ELSE
IF B$ = "6" THEN mx = mx + 1: GOTO 3.05 ELSE
IF B$ = "8" THEN my = my - 1: GOTO 3.05 ELSE
IF B$ = CHR$(27) THEN GOSUB sure: GOTO 3 ELSE
IF B$ = CHR$(13) THEN GOSUB 900: GOTO 3 ELSE
GOTO 3.01


3.05
IF my = 22 THEN IF mx < 77 THEN IF mx > 74 THEN mx = 40: my = 17: GOTO 1
IF mx > 78 THEN bump: mx = mx - 1
IF my > 23 THEN bump: my = my - 1
IF mx < 3 THEN bump: mx = mx + 1
IF my < 2 THEN bump: my = my + 1
IF my = 2 THEN IF mx = 4 THEN GOTO 3.59
IF INT(RND * 8) + 1 = 6 THEN enemy = 1: GOTO 3.2
IF INT(RND * 10) + 1 = 7 THEN enemy = 2: GOTO 3.2
IF INT(RND * 25) + 1 = 19 THEN enemy = 3: GOTO 3.2
GOTO 3

3.2
border
neon
IF thp > 0 THEN thesse
IF enemy = 1 THEN cecilslime
IF enemy = 2 THEN undead
IF enemy = 3 THEN resurrect
IF nhp <= 0 THEN IF thp <= 0 THEN GOSUB 1000: mx = 40: my = 15: GOTO 1
IF nhp <= 0 THEN GOTO 3.3
COLOR 15
LOCATE 2, 5
PRINT "NEON     (A)ttack"
LOCATE 3, 5
PRINT "         (R)un   "
IF nabil1 = 1 THEN LOCATE 4, 5: PRINT " Mp - 2  (F)ury  "
IF nabil2 = 1 THEN LOCATE 5, 5: PRINT " Mp - 4  (T)undra"
COLOR 15
LOCATE 22, 5
PRINT "NEON    HP:"; nhp; "/"; nhpmax; "MP", nmp; "/"; nmpmax
LOCATE 23, 5
PRINT "THESSE  HP:"; thp; "/"; thpmax; "MP"; tmp; "/"; tmpmax

3.21
ba$ = UCASE$(INKEY$)
IF ba$ = "A" THEN nthrow: IF nweapon = 0 THEN GOTO 3.22 ELSE IF nweapon = 2 THEN stars: GOTO 3.22 ELSE sai: GOTO 3.22
IF ba$ = "R" THEN IF nlev + INT(RND * 3) > 3 THEN GOTO 3
IF ba$ = "F" THEN IF nabil1 = 1 THEN IF nmp > 1 THEN GOTO 3.23
IF ba$ = "T" THEN IF nabil2 = 1 THEN IF nmp > 3 THEN GOTO 3.24
GOTO 3.21

3.22
IF enemy = 1 THEN cecilslimehp = cecilslimehp - (nweapon * nlev): IF cecilslimehp < 0 THEN GOTO 3.5 ELSE
IF enemy = 2 THEN undeadhp = undeadhp - (nweapon * nlev): IF undeadhp < 0 THEN GOTO 3.5 ELSE
IF enemy = 3 THEN resurrecthp = resurrecthp - (nweapon * nlev): IF resurrecthp < 0 THEN GOTO 3.5 ELSE
GOTO 3.3

3.23
fury
nmp = nmp - 2
IF enemy = 1 THEN cecilslimehp = cecilslimehp - (4 * nlev): IF cecilslimehp < 0 THEN GOTO 3.5 ELSE
IF enemy = 2 THEN undeadhp = undeadhp - (4 * nlev): IF undeadhp < 0 THEN GOTO 3.5 ELSE
IF enemy = 3 THEN resurrecthp = resurrecthp - (4 * nlev): IF resurrecthp < 0 THEN GOTO 3.5 ELSE
GOTO 3.3

3.24
tundra
nmp = nmp - 4
IF enemy = 1 THEN cecilslimehp = cecilslimehp - (7 * nlev): IF cecilslimehp < 0 THEN GOTO 3.5 ELSE
IF enemy = 2 THEN undeadhp = undeadhp - (7 * nlev): IF undeadhp < 0 THEN GOTO 3.5 ELSE
IF enemy = 3 THEN resurrecthp = resurrecthp - (7 * nlev): IF resurrecthp < 0 THEN GOTO 3.5 ELSE
GOTO 3.3

3.3
border
IF nhp > 0 THEN neon
thesse
IF enemy = 1 THEN cecilslime
IF enemy = 2 THEN undead
IF enemy = 3 THEN resurrect
IF thp <= 0 THEN GOTO 3.4
COLOR 14
LOCATE 2, 5
PRINT "THESSE   (A)ttack"
LOCATE 3, 5
PRINT "         (R)un   "
IF tabil1 = 1 THEN LOCATE 4, 5: PRINT " Mp - 2  (H)eal  "
IF tabil2 = 1 THEN LOCATE 5, 5: PRINT " Mp - 3  (W)ounds"
COLOR 15
LOCATE 22, 5
PRINT "NEON    HP:"; nhp; "/"; nhpmax; "MP", nmp; "/"; nmpmax
LOCATE 23, 5
PRINT "THESSE  HP:"; thp; "/"; thpmax; "MP"; tmp; "/"; tmpmax
DO
bb$ = UCASE$(INKEY$)
IF bb$ = "A" THEN cover: tattack: thesse: GOTO 3.32 ELSE
IF bb$ = "R" THEN IF tlev + INT(RND * 3) > 3 THEN GOTO 3 ELSE
IF bb$ = "H" THEN IF tabil1 = 1 THEN IF tmp > 1 THEN GOTO 3.33 ELSE
IF bb$ = "W" THEN IF tabil2 = 1 THEN IF tmp > 2 THEN GOTO 3.34 ELSE
LOOP

3.32
IF enemy = 1 THEN cecilslime: cecilslimehp = cecilslimehp - (tlev * tweapon): IF cecilslimehp < 0 THEN GOTO 3.5 ELSE
IF enemy = 2 THEN undead: undeadhp = undeadhp - (tlev * tweapon): IF undeadhp < 0 THEN GOTO 3.5 ELSE
IF enemy = 3 THEN resurrect: resurrecthp = resurrecthp - (tlev * tweapon): IF resurrecthp < 0 THEN GOTO 3.5 ELSE
GOTO 3.4

3.33
heal
tmp = tmp - 2
hp = hp + (tlev * 4) + INT(RND * 4) + 1: thp = thp + hp: nhp = nhp + hp: IF thp > thpmax THEN thp = thpmax: IF nhp > nhpmax THEN nhp = nhpmax: hp = 0
GOTO 3.4

3.34
wounds
tmp = tmp - 3
IF enemy = 1 THEN cecilslimehp = cecilslimehp - (tlev * 5): IF cecilslimehp < 0 THEN GOTO 3.5 ELSE
IF enemy = 2 THEN undeadhp = undeadhp - (tlev * 5): IF undeadhp < 0 THEN GOTO 3.5 ELSE
IF enemy = 3 THEN resurrecthp = resurrecthp - (tlev * 5): IF resurrecthp < 0 THEN GOTO 3.5 ELSE
GOTO 3.5

3.4
IF enemy = 1 THEN shot: IF INT(RND * 2) + 1 = 1 THEN thp = thp - 4 + tarmor ELSE nhp = nhp - 4 + narmor ELSE
IF enemy = 2 THEN shot: IF INT(RND * 2) + 1 = 2 THEN thp = thp - 5 + tarmor ELSE nhp = nhp - 5 + narmor ELSE
IF enemy = 3 THEN fire: IF INT(RND * 2) + 1 = 2 THEN thp = thp - 7 + tarmor ELSE nhp = nhp - 7 + narmor ELSE
GOTO 3.2

3.5
CLS
COLOR 15
LOCATE 4, 25
PRINT "You have gained", enemy * 4, "gold and"
LOCATE 5, 27
PRINT "", enemy * 3, "experience points"
PLAY "l12o3CEAG#L7DDl4AEF#"
IF thp > 0 THEN tex = tex + enemy * 3
IF nhp > 0 THEN nex = nex + enemy * 3
gold = gold + (enemy * 4)
IF enemy = 1 THEN cecilslimehp = 3 + INT(RND * 2) + 1
IF enemy = 2 THEN undeadhp = 5 + INT(RND * 2) + 1
IF enemy = 3 THEN resurrecthp = 13 + INT(RND * 3) + 1
GOSUB 1500
GOTO 3

3.59
IF hearts = 1 THEN GOTO 3 ELSE

3.6
border
neon
IF thp > 0 THEN thesse
boss1
IF nhp <= 0 THEN IF thp <= 0 THEN boss1hp = 65: GOSUB 1000
IF nhp <= 0 THEN GOTO 3.7
COLOR 15
LOCATE 2, 5
PRINT "NEON     (A)ttack"
LOCATE 3, 5
PRINT "         (R)un   "
IF nabil1 = 1 THEN LOCATE 4, 5: PRINT " Mp - 2  (F)ury  "
IF nabil2 = 1 THEN LOCATE 5, 5: PRINT " Mp - 4  (T)undra"
LOCATE 22, 5
PRINT "NEON    HP:"; nhp; "/"; nhpmax; "MP", nmp; "/"; nmpmax
LOCATE 23, 5
PRINT "THESSE  HP:"; thp; "/"; thpmax; "MP"; tmp; "/"; tmpmax
3.61
ba$ = UCASE$(INKEY$)
IF ba$ = "A" THEN nthrow: IF nweapon = 0 THEN GOTO 3.62 ELSE IF nweapon = 2 THEN stars ELSE sai: GOTO 3.62 ELSE
IF ba$ = "R" THEN GOTO 3.8 ELSE
IF ba$ = "F" THEN IF nabil1 = 1 THEN IF nmp > 1 THEN GOTO 3.63 ELSE
IF ba$ = "T" THEN IF nabil2 = 1 THEN IF nmp > 3 THEN GOTO 3.64 ELSE
GOTO 3.61

3.62
boss1hp = boss1hp - (nweapon * nlev): IF boss1hp < .1 THEN GOTO 3.9
GOTO 3.7

3.63
fury
nmp = nmp - 2
boss1hp = boss1hp - (2 * nlev): IF boss1hp < .1 THEN GOTO 3.9
GOTO 3.7

3.64
tundra
nmp = nmp - 4
boss1hp = boss1hp - (3 * nlev): IF boss1hp < .1 THEN GOTO 3.9
GOTO 3.7

3.7
border
IF nhp > 0 THEN neon
thesse
boss1
IF thp <= 0 THEN GOTO 3.8
COLOR 14
LOCATE 2, 5
PRINT "THESSE   (A)ttack"
LOCATE 3, 5
PRINT "         (R)un   "
IF tabil1 = 1 THEN LOCATE 4, 5: PRINT " Mp - 2  (H)eal  "
IF tabil2 = 1 THEN LOCATE 5, 5: PRINT " Mp - 3  (W)ounds"
COLOR 15
LOCATE 22, 5
PRINT "NEON    HP:"; nhp; "/"; nhpmax; "MP", nmp; "/"; nmpmax
LOCATE 23, 5
PRINT "THESSE  HP:"; thp; "/"; thpmax; "MP"; tmp; "/"; tmpmax
DO
bb$ = UCASE$(INKEY$)
IF bb$ = "A" THEN cover: tattack: thesse: GOTO 3.72 ELSE
IF bb$ = "R" THEN GOTO 3.8 ELSE
IF bb$ = "H" THEN IF nabil1 = 1 THEN IF tmp > 1 THEN GOTO 3.73 ELSE
IF bb$ = "W" THEN IF nabil2 = 1 THEN IF tmp > 2 THEN GOTO 3.74 ELSE
LOOP

3.72
boss1
boss1hp = boss1hp - (tweapon * tlev): IF boss1hp < .1 THEN GOTO 3.9
GOTO 3.8

3.73
heal
tmp = tmp - 2
hp = hp + (tlev * 4) + INT(RND * 4) + 1: thp = thp + hp: nhp = nhp + hp: IF thp > thpmax THEN thp = thpmax: IF nhp > nhpmax THEN nhp = nhpmax: hp = 0
GOTO 3.8

3.74
wounds
tmp = tmp - 3
boss1hp = boss1hp - (2 * tlev): IF boss1hp < .1 THEN GOTO 3.9
GOTO 3.8

3.8
border
neon
thesse
boss1
IF INT(RND * 3) = 1 THEN fire: nhp = nhp - (9 + narmor): thp = thp - (9 * tarmor): GOTO 3.6 ELSE blast: nhp = nhp - (10 + narmor * 2): thp = thp - (10 + tarmor * 2)
GOTO 3.6

3.9
PAINT (1, 1), 15
border
COLOR 15
LOCATE 5, 29
PRINT "YOU HAVE DEFEATED THE FIRST BOSS!"
LOCATE 7, 33
PRINT "YOU GOT THE FIRST HEART!"
PLAY "o1l9BFG#EFL14ACD#L10EE"
hearts = 1
place = 2
SLEEP 10
my = 16: mx = 40
GOTO 1

4
spiritland
IF hearts = 1 THEN COLOR 4: LOCATE 3, 71: PRINT ""
LOCATE my, mx
COLOR 15
PRINT ""
GOTO 4.01

4.01
B$ = INKEY$
IF B$ = "2" THEN my = my + 1: GOTO 4.05 ELSE
IF B$ = "4" THEN mx = mx - 1: GOTO 4.05 ELSE
IF B$ = "6" THEN mx = mx + 1: GOTO 4.05 ELSE
IF B$ = "8" THEN my = my - 1: GOTO 4.05 ELSE
IF B$ = CHR$(27) THEN GOSUB sure: GOTO 4 ELSE
IF B$ = CHR$(13) THEN GOSUB 900: GOTO 4 ELSE
GOTO 4.01

4.05
IF my = 22 THEN IF mx < 42 THEN IF mx > 39 THEN mx = 40: my = 17: GOTO 1
IF mx > 78 THEN bump: mx = mx - 1
IF my > 23 THEN bump: my = my - 1
IF mx < 3 THEN bump: mx = mx + 1
IF my < 2 THEN bump: my = my + 1
IF my = 3 THEN IF mx = 71 THEN GOTO 4.6
IF INT(RND * 8) + 1 = 7 THEN enemy = 4: GOTO 4.2
IF INT(RND * 10) + 1 = 10 THEN enemy = 5: GOTO 4.2
GOTO 4

4.2
border
neon
IF thp > 0 THEN thesse
IF enemy = 4 THEN dragonfly
IF enemy = 5 THEN spirit
IF nhp <= 0 THEN IF thp <= 0 THEN GOSUB 1000: mx = 40: my = 15: GOTO 1
IF nhp <= 0 THEN GOTO 4.3
COLOR 15
LOCATE 2, 5
PRINT "NEON     (A)ttack"
LOCATE 3, 5
PRINT "         (R)un   "
IF nabil1 = 1 THEN LOCATE 4, 5: PRINT " Mp - 2  (F)ury  "
IF nabil2 = 1 THEN LOCATE 5, 5: PRINT " Mp - 4  (T)undra"
COLOR 15
LOCATE 22, 5
PRINT "NEON    HP:"; nhp; "/"; nhpmax; "MP", nmp; "/"; nmpmax
LOCATE 23, 5
PRINT "THESSE  HP:"; thp; "/"; thpmax; "MP"; tmp; "/"; tmpmax

4.21
ba$ = UCASE$(INKEY$)
IF ba$ = "A" THEN nthrow: IF nweapon = 0 THEN GOTO 4.22 ELSE IF nweapon = 2 THEN stars: GOTO 4.22 ELSE sai: GOTO 4.22 ELSE
IF ba$ = "R" THEN IF nlev + INT(RND * 3) > 8 THEN GOTO 4 ELSE
IF ba$ = "F" THEN IF nabil1 = 1 THEN IF nmp > 1 THEN GOTO 4.23 ELSE
IF ba$ = "T" THEN IF nabil2 = 1 THEN IF nmp > 3 THEN GOTO 4.24 ELSE
GOTO 4.21

4.22                                                                      
IF enemy = 4 THEN dragonflyhp = dragonflyhp - (nweapon * nlev): IF dragonflyhp < 0 THEN GOTO 4.5 ELSE
IF enemy = 5 THEN spirithp = spirithp - (nweapon * nlev): IF spirithp < 0 THEN GOTO 4.5 ELSE
GOTO 4.3

4.23
fury
nmp = nmp - 2
IF enemy = 4 THEN dragonflyhp = dragonflyhp - (2.5 * nlev): IF dragonflyhp < 0 THEN GOTO 4.5 ELSE
IF enemy = 5 THEN spirithp = spirithp - (3 * nlev): IF spirithp < 0 THEN GOTO 4.5 ELSE
GOTO 4.3

4.24
tundra
nmp = nmp - 4
IF enemy = 4 THEN dragonflyhp = dragonflyhp - (5 * nlev): IF dragonflyhp < 0 THEN GOTO 4.5 ELSE
IF enemy = 5 THEN spirithp = spirithp - (3 * nlev): IF spirithp < 0 THEN GOTO 4.5 ELSE
GOTO 4.3

4.3
border
IF nhp > 0 THEN neon
thesse
IF enemy = 4 THEN dragonfly
IF enemy = 5 THEN spirit
IF thp <= 0 THEN GOTO 4.4
COLOR 14
LOCATE 2, 5
PRINT "THESSE   (A)ttack"
LOCATE 3, 5
PRINT "         (R)un   "
IF tabil1 = 1 THEN LOCATE 4, 5: PRINT " Mp - 2  (H)eal  "
IF tabil2 = 1 THEN LOCATE 5, 5: PRINT " Mp - 3  (W)ounds"
COLOR 15
LOCATE 22, 5
PRINT "NEON    HP:"; nhp; "/"; nhpmax; "MP", nmp; "/"; nmpmax
LOCATE 23, 5
PRINT "THESSE  HP:"; thp; "/"; thpmax; "MP"; tmp; "/"; tmpmax
DO
bb$ = UCASE$(INKEY$)
IF bb$ = "A" THEN cover: tattack: thesse: GOTO 4.32 ELSE
IF bb$ = "R" THEN IF tlev + INT(RND * 3) > 8 THEN GOTO 4 ELSE
IF bb$ = "H" THEN IF tabil1 = 1 THEN IF tmp > 1 THEN GOTO 4.33 ELSE
IF bb$ = "W" THEN IF tabil2 = 1 THEN IF tmp > 2 THEN GOTO 4.34 ELSE
LOOP

4.32                                                                                  
IF enemy = 4 THEN dragonfly: dragonflyhp = dragonflyhp - (tlev * tweapon): IF dragonflyhp < 0 THEN GOTO 4.5 ELSE
IF enemy = 5 THEN spirit: spirithp = spirithp - (tlev * tweapon): IF spirithp < 0 THEN GOTO 4.5 ELSE
GOTO 4.4

4.33
heal
tmp = tmp - 2
hp = hp + (tlev * 4) + INT(RND * 4) + 1: thp = thp + hp: nhp = nhp + hp: IF thp > thpmax THEN thp = thpmax: IF nhp > nhpmax THEN nhp = nhpmax: hp = 0
GOTO 4.4

4.34
wounds
tmp = tmp - 3
IF enemy = 4 THEN dragonflyhp = dragonflyhp - (tlev * 4): IF dragonflyhp < 0 THEN GOTO 4.5 ELSE
IF enemy = 5 THEN spirithp = spirithp - (tlev * 4): IF spirithp < 0 THEN GOTO 4.5 ELSE
GOTO 4.4

4.4                                                                    
IF enemy = 4 THEN fire: IF INT(RND * 2) + 1 = 1 THEN thp = thp - 5 + tarmor ELSE nhp = nhp - 5 * narmor
IF enemy = 5 THEN omega: thp = thp - 7 + tarmor: nhp = nhp - 6 + narmor
GOTO 4.2

4.5
CLS
COLOR 15
LOCATE 4, 25
PRINT "You have gained", enemy * 4, "gold and"
LOCATE 5, 27
PRINT "", enemy * 3, "experience points"
PLAY "l12o3CEAG#L7DDl4AEF#"
IF thp > 0 THEN tex = tex + enemy * 3
IF nhp > 0 THEN nex = nex + enemy * 3
gold = gold + (enemy * 4)
IF enemy = 4 THEN dragonflyhp = 23 + INT(RND * 4) + 1
IF enemy = 5 THEN spirithp = 30 + INT(RND * 6) + 1
GOSUB 1500
GOTO 4

4.59
IF hearts > 1 THEN GOTO 4 ELSE

4.6
border
neon
IF thp > 0 THEN thesse
birdhead
IF nhp <= 0 THEN IF thp <= 0 THEN boss2hp = 610: GOSUB 1000
IF nhp <= 0 THEN GOTO 4.7
COLOR 15
LOCATE 2, 5
PRINT "NEON     (A)ttack"
LOCATE 3, 5
PRINT "         (R)un   "
IF nabil1 = 1 THEN LOCATE 4, 5: PRINT " Mp - 2  (F)ury  "
IF nabil2 = 1 THEN LOCATE 5, 5: PRINT " Mp - 4  (T)undra"
LOCATE 22, 5
PRINT "NEON    HP:"; nhp; "/"; nhpmax; "MP", nmp; "/"; nmpmax
LOCATE 23, 5
PRINT "THESSE  HP:"; thp; "/"; thpmax; "MP"; tmp; "/"; tmpmax
4.61
ba$ = UCASE$(INKEY$)
IF ba$ = "A" THEN nthrow: IF nweapon = 0 THEN GOTO 4.62 ELSE IF nweapon = 2 THEN stars: GOTO 4.62 ELSE sai: GOTO 4.62 ELSE
IF ba$ = "R" THEN GOTO 4.8 ELSE
IF ba$ = "F" THEN IF nabil1 = 1 THEN IF nmp > 1 THEN GOTO 4.63 ELSE
IF ba$ = "T" THEN IF nabil2 = 1 THEN IF nmp > 3 THEN GOTO 4.64 ELSE
GOTO 4.61

4.62                      
boss2hp = boss2hp - (nweapon * nlev): IF boss2hp < .1 THEN GOTO 4.9
GOTO 4.7

4.63
fury
nmp = nmp - 2
boss2hp = boss2hp - (3 * nlev): IF boss2hp < .1 THEN GOTO 4.9
GOTO 4.7

4.64
tundra
nmp = nmp - 4
boss2hp = boss2hp - (3.5 * nlev): IF boss2hp < .1 THEN GOTO 4.9
GOTO 4.7

4.7
border
IF nhp > 0 THEN neon
thesse
birdhead
IF thp <= 0 THEN GOTO 4.8
COLOR 14
LOCATE 2, 5
PRINT "THESSE   (A)ttack"
LOCATE 3, 5
PRINT "         (R)un   "
IF tabil1 = 1 THEN LOCATE 4, 5: PRINT " Mp - 2  (H)eal  "
IF tabil2 = 1 THEN LOCATE 5, 5: PRINT " Mp - 3  (W)ounds"
COLOR 15
LOCATE 22, 5
PRINT "NEON    HP:"; nhp; "/"; nhpmax; "MP", nmp; "/"; nmpmax
LOCATE 23, 5
PRINT "THESSE  HP:"; thp; "/"; thpmax; "MP"; tmp; "/"; tmpmax
DO
bb$ = UCASE$(INKEY$)
IF bb$ = "A" THEN cover: tattack: thesse: GOTO 4.72 ELSE
IF bb$ = "R" THEN GOTO 4.8 ELSE
IF bb$ = "H" THEN IF nabil1 = 1 THEN IF tmp > 1 THEN GOTO 4.73 ELSE
IF bb$ = "W" THEN IF nabil2 = 1 THEN IF tmp > 2 THEN GOTO 4.74 ELSE
LOOP

4.72
birdhead
boss2hp = boss2hp - (tweapon * tlev): IF boss2hp < .1 THEN GOTO 4.9
GOTO 4.8

4.73
heal
tmp = tmp - 2
hp = hp + (tlev * 4) + INT(RND * 4) + 1: thp = thp + hp: nhp = nhp + hp: IF thp > thpmax THEN thp = thpmax: IF nhp > nhpmax THEN nhp = nhpmax: hp = 0
GOTO 4.8

4.74
wounds
tmp = tmp - 3
boss2hp = boss2hp - (3 * tlev): IF boss2hp < .1 THEN GOTO 4.9
GOTO 4.8

4.8
border
neon
thesse
birdhead
IF INT(RND * 3) = 2 THEN omega: nhp = nhp - (14 + narmor): thp = thp - (15 * tarmor): GOTO 4.6 ELSE blast: nhp = nhp - (10 + narmor * 2): thp = thp - (10 + tarmor * 2)
GOTO 4.6

4.9
PAINT (1, 1), 15
border
COLOR 15
LOCATE 5, 29
PRINT "YOU HAVE DEFEATED THE SECOND BOSS!"
LOCATE 7, 33
PRINT "YOU GOT THE SECOND HEART!"
LOCATE 8, 31
PRINT "NEON HAS RECIEVED A TRI PLATE!"
LOCATE 9, 30
PRINT "THESSE HAS RECIEVED A TRI PLATE!"
PLAY "o3l13EGAADl18EGGo2CDD#o1l11EEEEEGE"
hearts = 2
tarmor = 2
narmor = 2
SLEEP 10
my = 16: mx = 40
GOTO 1

5
depths
IF hearts = 2 THEN COLOR 4: LOCATE 3, 9: PRINT ""
LOCATE my, mx
COLOR 15
PRINT ""
GOTO 5.01

5.01
B$ = INKEY$
IF B$ = "2" THEN my = my + 1: GOTO 5.05 ELSE
IF B$ = "4" THEN mx = mx - 1: GOTO 5.05 ELSE
IF B$ = "6" THEN mx = mx + 1: GOTO 5.05 ELSE
IF B$ = "8" THEN my = my - 1: GOTO 5.05 ELSE
IF B$ = CHR$(27) THEN GOSUB sure: GOTO 5 ELSE
IF B$ = CHR$(13) THEN GOSUB 900: GOTO 5 ELSE
GOTO 5.01

5.05
IF my = 22 THEN IF mx < 42 THEN IF mx > 39 THEN mx = 40: my = 17: GOTO 1
IF mx > 78 THEN bump: mx = mx - 1
IF my > 23 THEN bump: my = my - 1
IF mx < 3 THEN bump: mx = mx + 1
IF my < 2 THEN bump: my = my + 1
IF mx = 9 THEN IF my = 3 THEN GOTO 5.6
IF INT(RND * 8) + 1 = 7 THEN enemy = 6: GOTO 5.2
IF INT(RND * 10) + 1 = 10 THEN enemy = 7: GOTO 5.2
GOTO 5

5.2
border
neon
IF thp > 0 THEN thesse
IF enemy = 6 THEN darkangel
IF enemy = 7 THEN firebat
IF nhp <= 0 THEN IF thp <= 0 THEN GOSUB 1000: mx = 40: my = 15: GOTO 1
IF nhp <= 0 THEN GOTO 5.3
COLOR 15
LOCATE 2, 5
PRINT "NEON     (A)ttack"
LOCATE 3, 5
PRINT "         (R)un   "
IF nabil1 = 1 THEN LOCATE 4, 5: PRINT " Mp - 2  (F)ury  "
IF nabil2 = 1 THEN LOCATE 5, 5: PRINT " Mp - 4  (T)undra"
IF nabil3 = 1 THEN LOCATE 6, 5: PRINT " Mp - 6  (Z)-Volt"
COLOR 15
LOCATE 22, 5
PRINT "NEON    HP:"; nhp; "/"; nhpmax; "MP", nmp; "/"; nmpmax
LOCATE 23, 5
PRINT "THESSE  HP:"; thp; "/"; thpmax; "MP"; tmp; "/"; tmpmax

5.21
ba$ = UCASE$(INKEY$)
IF ba$ = "A" THEN nthrow: IF nweapon = 0 THEN GOTO 5.22 ELSE IF nweapon = 2 THEN stars: GOTO 5.22 ELSE sai: GOTO 5.22 ELSE
IF ba$ = "R" THEN IF nlev + INT(RND * 3) >= 13 THEN GOTO 5 ELSE
IF ba$ = "F" THEN IF nabil1 = 1 THEN IF nmp > 1 THEN GOTO 5.23 ELSE
IF ba$ = "T" THEN IF nabil2 = 1 THEN IF nmp > 3 THEN GOTO 5.24 ELSE
IF ba$ = "Z" THEN IF nabil3 = 1 THEN IF nmp > 5 THEN GOTO 5.25 ELSE
GOTO 5.21

5.22                                                                                                                                        
IF enemy = 6 THEN darkangelhp = darkangelhp - (nweapon * nlev): IF darkangelhp < 0 THEN GOTO 5.5 ELSE
IF enemy = 7 THEN firebathp = firebathp - (nweapon * nlev): IF firebathp < 0 THEN GOTO 5.5 ELSE
GOTO 5.3

5.23
fury
nmp = nmp - 2
IF enemy = 6 THEN darkangelhp = darkangelhp - (2.5 * nlev): IF darkangelhp < 0 THEN GOTO 5.5 ELSE
IF enemy = 7 THEN firebathp = firebathp - (3 * nlev): IF firabathp < 0 THEN GOTO 5.5 ELSE
GOTO 5.3

5.24
tundra
nmp = nmp - 4
IF enemy = 6 THEN darkangelhp = darkangelhp - (3 * nlev): IF darkangelhp < 0 THEN GOTO 5.5 ELSE
IF enemy = 7 THEN firebathp = firebathp - (4 * nlev): IF firebathp < 0 THEN GOTO 5.5 ELSE
GOTO 5.3

5.25
zvolt
nmp = nmp - 6
IF enemy = 6 THEN darkangelhp = darkangelhp - (5 * nlev): IF darkangelhp < 0 THEN GOTO 5.5 ELSE
IF enemy = 7 THEN firebathp = firebathp - (5 * nlev): IF firebathp < 0 THEN GOTO 5.5 ELSE
GOTO 5.3

5.3
border
IF nhp > 0 THEN neon
thesse
IF enemy = 6 THEN darkangel
IF enemy = 7 THEN firebat
IF thp <= 0 THEN GOTO 5.4
COLOR 14
LOCATE 2, 5
PRINT "THESSE   (A)ttack"
LOCATE 3, 5
PRINT "         (R)un   "
IF tabil1 = 1 THEN LOCATE 4, 5: PRINT " Mp - 2  (H)eal  "
IF tabil2 = 1 THEN LOCATE 5, 5: PRINT " Mp - 3  (W)ounds"
IF tabil3 = 1 THEN LOCATE 6, 5: PRINT " Mp - 5  (Z)-Heal"
COLOR 15
LOCATE 22, 5
PRINT "NEON    HP:"; nhp; "/"; nhpmax; "MP", nmp; "/"; nmpmax
LOCATE 23, 5
PRINT "THESSE  HP:"; thp; "/"; thpmax; "MP"; tmp; "/"; tmpmax
DO
bb$ = UCASE$(INKEY$)
IF bb$ = "A" THEN cover: tattack: thesse: GOTO 5.32 ELSE
IF bb$ = "R" THEN IF tlev + INT(RND * 3) > 12 THEN GOTO 5 ELSE
IF bb$ = "H" THEN IF tabil1 = 1 THEN IF tmp > 1 THEN GOTO 5.33 ELSE
IF bb$ = "W" THEN IF tabil2 = 1 THEN IF tmp > 2 THEN GOTO 5.34 ELSE
IF bb$ = "Z" THEN IF tabil3 = 1 THEN IF tmp > 4 THEN GOTO 5.35 ELSE
LOOP

5.32                                                                                
IF enemy = 6 THEN darkangel: darkangelhp = darkangelhp - (tlev * tweapon): IF darkangelhp < 0 THEN GOTO 5.5 ELSE
IF enemy = 7 THEN firebat: firebathp = firebathp - (tlev * tweapon): IF firebathp < 0 THEN GOTO 5.5 ELSE
GOTO 5.4

5.33
heal
tmp = tmp - 2
hp = hp + (tlev * 4) + INT(RND * 4) + 1: thp = thp + hp: nhp = nhp + hp: IF thp > thpmax THEN thp = thpmax: IF nhp > nhpmax THEN nhp = nhpmax: hp = 0
GOTO 5.4

5.34
wounds
tmp = tmp - 3
IF enemy = 6 THEN darkangelhp = darkangelhp - (tlev * 3): IF darkangelhp < 0 THEN GOTO 5.5 ELSE
IF enemy = 7 THEN firebathp = firebathp - (tlev * 4): IF firebathp < 0 THEN GOTO 5.5 ELSE
GOTO 5.4

5.35
zheal
tmp = tmp - 5: thp = thpmax: nhp = nhpmax
GOTO 5.4

5.4                                                          
IF enemy = 6 THEN ultra: thp = thp - 9 + (tarmor * 2): nhp = nhp - 9 + (narmor * 2)
IF enemy = 7 THEN omega: thp = thp - 14 + (tarmor * 2): nhp = nhp - 14 + (narmor * 2)
GOTO 5.2

5.5                 
CLS
COLOR 15
LOCATE 4, 25
PRINT "You have gained", enemy * 6, "gold and"
LOCATE 5, 27
PRINT "", enemy * 5, "experience points"
PLAY "l12o3CEAG#L7DDl4AEF#"
IF thp > 0 THEN tex = tex + enemy * 5
IF nhp > 0 THEN nex = nex + enemy * 5
gold = gold + (enemy * 6)
IF enemy = 6 THEN darkangelhp = 90 + INT(RND * 23) + 1
IF enemy = 7 THEN firebathp = 100 + INT(RND * 27) + 1
GOSUB 1500
GOTO 5

5.59
IF hearts > 2 THEN GOTO 5 ELSE

5.6
border
neon
IF thp > 0 THEN thesse
boss3
IF nhp <= 0 THEN IF thp <= 0 THEN boss3hp = 2400: GOSUB 1000
IF nhp <= 0 THEN GOTO 5.7
COLOR 15
LOCATE 2, 5
PRINT "NEON     (A)ttack"
LOCATE 3, 5
PRINT "         (R)un   "
IF nabil1 = 1 THEN LOCATE 4, 5: PRINT " Mp - 2  (F)ury  "
IF nabil2 = 1 THEN LOCATE 5, 5: PRINT " Mp - 4  (T)undra"
IF nabil3 = 1 THEN LOCATE 6, 5: PRINT " Mp - 6  (Z)-Volt"
LOCATE 22, 5
PRINT "NEON    HP:"; nhp; "/"; nhpmax; "MP", nmp; "/"; nmpmax
LOCATE 23, 5
PRINT "THESSE  HP:"; thp; "/"; thpmax; "MP"; tmp; "/"; tmpmax
LOCATE 23, 57

5.61
ba$ = UCASE$(INKEY$)
IF ba$ = "A" THEN nthrow: IF nweapon = 0 THEN GOTO 5.62 ELSE IF nweapon = 2 THEN stars: GOTO 5.62 ELSE sai: GOTO 5.62 ELSE
IF ba$ = "R" THEN GOTO 5.8 ELSE
IF ba$ = "F" THEN IF nabil1 = 1 THEN IF nmp > 1 THEN GOTO 5.63 ELSE
IF ba$ = "T" THEN IF nabil2 = 1 THEN IF nmp > 3 THEN GOTO 5.64 ELSE
IF ba$ = "Z" THEN IF nabil3 = 1 THEN IF nmp > 5 THEN GOTO 5.65 ELSE
GOTO 5.61

5.62                                                   
boss3hp = boss3hp - (nweapon * nlev): IF boss3hp < .1 THEN GOTO 5.9
GOTO 5.7

5.63
fury
nmp = nmp - 2
boss3hp = boss3hp - (3 * nlev): IF boss3hp < .1 THEN GOTO 5.9
GOTO 5.7

5.64
tundra
nmp = nmp - 4
boss3hp = boss3hp - (4.25 * nlev): IF boss3hp < .1 THEN GOTO 5.9
GOTO 5.7

5.65
zvolt
nmp = nmp - 6
boss3hp = boss3hp - (4.5 * nlev): IF boss3hp < .1 THEN GOTO 5.9
GOTO 5.7

5.7
border
IF nhp > 0 THEN neon
thesse
boss3
IF thp <= 0 THEN GOTO 5.8
COLOR 14
LOCATE 2, 5
PRINT "THESSE   (A)ttack"
LOCATE 3, 5
PRINT "         (R)un   "
IF tabil1 = 1 THEN LOCATE 4, 5: PRINT " Mp - 2  (H)eal  "
IF tabil2 = 1 THEN LOCATE 5, 5: PRINT " Mp - 3  (W)ounds"
IF tabil3 = 1 THEN LOCATE 6, 5: PRINT " Mp - 5  (Z)-Heal"
COLOR 15
LOCATE 22, 5
PRINT "NEON    HP:"; nhp; "/"; nhpmax; "MP", nmp; "/"; nmpmax
LOCATE 23, 5
PRINT "THESSE  HP:"; thp; "/"; thpmax; "MP"; tmp; "/"; tmpmax
DO
bb$ = UCASE$(INKEY$)
IF bb$ = "A" THEN cover: tattack: thesse: GOTO 5.72 ELSE
IF bb$ = "R" THEN GOTO 5.8 ELSE
IF bb$ = "H" THEN IF nabil1 = 1 THEN IF tmp > 1 THEN GOTO 5.73 ELSE
IF bb$ = "W" THEN IF nabil2 = 1 THEN IF tmp > 2 THEN GOTO 5.74 ELSE
IF bb$ = "Z" THEN IF nabil3 = 1 THEN IF tmp > 4 THEN GOTO 5.75 ELSE
LOOP

5.72
boss3
boss3hp = boss3hp - (tweapon * tlev): IF boss3hp < .1 THEN GOTO 5.9
GOTO 5.8

5.73
heal
tmp = tmp - 2
hp = hp + (tlev * 4) + INT(RND * 4) + 1: thp = thp + hp: nhp = nhp + hp: IF thp > thpmax THEN thp = thpmax: IF nhp > nhpmax THEN nhp = nhpmax: hp = 0
GOTO 5.8

5.74
wounds
tmp = tmp - 3
boss3hp = boss3hp - (3.5 * tlev): IF boss3hp < .1 THEN GOTO 5.9
GOTO 5.8

5.75
zheal
tmp = tmp - 5: thp = thpmax: nhp = nhpmax
GOTO 5.8

5.8
border
neon
thesse
boss3
IF INT(RND * 3) = 2 THEN ultra: nhp = nhp - 23 + (narmor * 2): thp = thp - 23 + (tarmor * 2): GOTO 5.6 ELSE blast: nhp = nhp - 16 + (narmor * 2): thp = thp - 16 + (tarmor * 2)
GOTO 5.6

5.9
PAINT (1, 1), 15
border
COLOR 15
LOCATE 5, 29
PRINT "YOU HAVE DEFEATED THE THIRD BOSS!"
LOCATE 7, 33
PRINT "YOU GOT THE THIRD HEART!"
LOCATE 8, 32
PRINT "NEON HAS RECIEVED Z-SAIS!"
LOCATE 9, 29
PRINT "THESSE HAS RECIEVED A Z-SWORD!"
PLAY "o3l12EGABCl17EFDo4edao3edo2cdo1EEEl1El31egafdceg#a#"
hearts = 3
nweapon = 3
tweapon = 3
SLEEP 10
my = 16: mx = 40
GOTO 1

6
CLS
COLOR 15
PRINT "SATAN : NOW YOU WILL SEE WHO IS MORE POWERFUL!"
SLEEP 2
PRINT "     I WILL PUNISH THOSE WHO DARE TO OPPOSE ME!"
SLEEP 2
PRINT "..."
SLEEP 1
PRINT "YOU HEAR A VOICE IN YOUR HEAD:"
SLEEP 2
PRINT "VOICE : TAKE THESE, FOR YOU WILL NEED THEM!"
SLEEP 2
PRINT "Neon and Thesse both recieved Mystic Armor!"
SLEEP 2
PRINT "VOICE : YOU'LL NEED THIS ARMOR TO SURVIVE SATAN'S FLAMES"
SLEEP 2
PRINT "SATAN : YOU'LL NEED ALL THE HELP IN THE WORLD TO SURVIVE!"
SLEEP
tarmor = 3
narmor = 3
GOTO 6.1

6.1
border
neon
IF thp > 0 THEN thesse
satan
IF nhp <= 0 THEN IF thp <= 0 THEN satanhp = 3500: GOSUB 1000
IF nhp <= 0 THEN GOTO 6.2
COLOR 15
LOCATE 2, 5
PRINT "NEON     (A)ttack"
IF nabil1 = 1 THEN LOCATE 4, 5: PRINT " Mp - 2  (F)ury  "
IF nabil2 = 1 THEN LOCATE 5, 5: PRINT " Mp - 4  (T)undra"
IF nabil3 = 1 THEN LOCATE 6, 5: PRINT " Mp - 6  (Z)-Volt"
LOCATE 22, 5
PRINT "NEON    HP:"; nhp; "/"; nhpmax; "MP", nmp; "/"; nmpmax
LOCATE 23, 5
PRINT "THESSE  HP:"; thp; "/"; thpmax; "MP"; tmp; "/"; tmpmax
DO
ba$ = UCASE$(INKEY$)
SOUND 100 + INT(RND * 760), .03
IF ba$ = "A" THEN nthrow: IF nweapon = 0 THEN GOTO 6.11 ELSE IF nweapon = 2 THEN stars ELSE sai: GOTO 6.11 ELSE
IF ba$ = "F" THEN IF nabil1 = 1 THEN IF nmp > 1 THEN GOTO 6.12
IF ba$ = "T" THEN IF nabil2 = 1 THEN IF nmp > 3 THEN GOTO 6.13
IF ba$ = "Z" THEN IF nabil3 = 1 THEN IF nmp > 5 THEN GOTO 6.14
LOOP

6.11
satanhp = satanhp - (nweapon * nlev): IF satanhp < .1 THEN GOTO 7
GOTO 6.2

6.12
fury
nmp = nmp - 2
satanhp = satanhp - (2.5 * nlev): IF satanhp < .1 THEN GOTO 7
GOTO 6.2

6.13
tundra
nmp = nmp - 4
satanhp = satanhp - (3.5 * nlev): IF satanhp < .1 THEN GOTO 7
GOTO 6.2

6.14
zvolt
nmp = nmp - 6
satanhp = satanhp - (4.5 * nlev): IF satanhp < .1 THEN GOTO 7
GOTO 6.2

6.2
border
IF nhp > 0 THEN neon
thesse
satan
IF thp <= 0 THEN GOTO 6.3
COLOR 14
LOCATE 2, 5
PRINT "THESSE   (A)ttack"
IF tabil1 = 1 THEN LOCATE 4, 5: PRINT " Mp - 2  (H)eal  "
IF tabil2 = 1 THEN LOCATE 5, 5: PRINT " Mp - 3  (W)ounds"
IF tabil3 = 1 THEN LOCATE 6, 5: PRINT " Mp - 5  (Z)-Heal"
COLOR 15
LOCATE 22, 5
PRINT "NEON    HP:"; nhp; "/"; nhpmax; "MP", nmp; "/"; nmpmax
LOCATE 23, 5
PRINT "THESSE  HP:"; thp; "/"; thpmax; "MP"; tmp; "/"; tmpmax

DO
bb$ = UCASE$(INKEY$)
SOUND 100 + INT(RND * 760), .03
IF bb$ = "A" THEN cover: tattack: thesse: GOTO 6.21 ELSE
IF bb$ = "H" THEN IF nabil1 = 1 THEN IF tmp > 1 THEN GOTO 6.22
IF bb$ = "W" THEN IF nabil2 = 1 THEN IF tmp > 2 THEN GOTO 6.23
IF bb$ = "Z" THEN IF nabil3 = 1 THEN IF tmp > 4 THEN GOTO 6.24
LOOP

6.21
satanhp = satanhp - (tweapon * tlev): IF satanhp < .1 THEN GOTO 7
GOTO 6.3

6.22
heal
 tmp = tmp - 2
 hp = hp + (tlev * 4) + INT(RND * 4) + 1: thp = thp + hp: nhp = nhp + hp: IF thp > thpmax THEN thp = thpmax: IF nhp > nhpmax THEN nhp = nhpmax: hp = 0
GOTO 6.3

6.23
wounds
tmp = tmp - 3
satanhp = satanhp - (2.5 * tlev): IF satanhp < .1 THEN GOTO 7
GOTO 6.3

6.24
zheal
tmp = tmp - 5: thp = thpmax: nhp = nhpmax
GOTO 6.3

6.3
border
neon
thesse
satan
IF INT(RND * 3) = 1 THEN flames: nhp = nhp - 29: thp = thp - 29: GOTO 6.1 ELSE ultra: nhp = nhp - 22: thp = thp - 22
GOTO 6.1

7
PAINT (1, 1), 15
border
COLOR 15
LOCATE 5, 33
PRINT "YOU HAVE DEFEATED SATAN"
LOCATE 7, 23
PRINT "YOU HAVE THE ABILITIES OF THE HEARTS!"
PLAY "l12egd#o4l16eefel19egabbo3l12egafel19ggagfedl14eef#ccba#l25abcdefgl16eefgel12gfgfeadl18edcbf#o6l16efgabcdo5efgabcdo4efgabcd"
SLEEP 10
GOTO 7.5

7.5
CLS
border
COLOR 15
LOCATE 3, 7
PRINT "NEON : Yes!  What?  Thesse?  What's wrong!"
PLAY "o2l5eegee"
SLEEP
LOCATE 4, 7
PRINT "THESSE : No!  I can't be!  I'm being taken over by Satan's soul!"
SLEEP
LOCATE 5, 7
PRINT "NEON : No!  Fight it!"
SLEEP
thesse
SLEEP 1
flames
CLS
border
COLOR 15
LOCATE 3, 7
PRINT "It was too late.  Thesse has been posessed.  A new challenge"
LOCATE 4, 7
PRINT "awaits Neon.  He must find a partner to help him get back the"
LOCATE 5, 7
PRINT "soul of Thesse.  When he destroyed Satan, he created an imbalance"
LOCATE 6, 7
PRINT "in the universe.  He must now get back Thesse's soul, and return "
LOCATE 7, 7
PRINT "to be King of his town."
PLAY "l13o1egggal16defgabc"
PLAY "l13o1egggal16defgabc"
PLAY "l13o1egggal16defgabc"
PLAY "l13o1egggal16defgabc"
PLAY "l13o1egggal16defgabc"
SLEEP
PRINT "Thanks for playing (and passing) HELLPIT 1.  Wait for HELLPIT 2"
PRINT "coming soon by FIG.  Goodbye!"
SLEEP
CALL quit


900
CLS
PAINT (1, 1), 10
border
COLOR 15
LOCATE 3, 6
PRINT "NEON   HP:", nhp, "/", nhpmax
LOCATE 4, 6
PRINT "       MP:", nmp, "/", nmpmax
LOCATE 5, 6
PRINT "    LEVEL:", nlev
LOCATE 6, 6
PRINT "       EX:", nex
IF nweapon = 1 THEN nwname$ = "Sess Sai"
IF nweapon = 2 THEN nwname$ = "Soul Stars"
IF nweapon = 3 THEN nwname$ = "Z Sais"
LOCATE 7, 6
PRINT "   WEAPON:", nwname$
IF narmor = 1 THEN naname$ = "Radical Armor"
IF narmor = 2 THEN naname$ = "Tri Plate"
IF narmor = 3 THEN naname$ = "Mystic Armor"
LOCATE 8, 6
PRINT "    ARMOR:", naname$
COLOR 14
LOCATE 10, 6
PRINT "THESSE HP:", thp, "/", thpmax
LOCATE 11, 6
PRINT "       MP:", tmp, "/", tmpmax
LOCATE 12, 6
PRINT "    LEVEL:", tlev
LOCATE 13, 6
PRINT "       EX:", tex
IF tweapon = 1 THEN twname$ = "Thrust Blade"
IF tweapon = 2 THEN twname$ = "Stealth Sword"
IF tweapon = 3 THEN twname$ = "Z Sword"
LOCATE 14, 6
PRINT "   WEAPON:", twname$
IF tarmor = 1 THEN taname$ = "Radical Armor"
IF tarmor = 2 THEN taname$ = "Tri Plate"
IF tarmor = 3 THEN taname$ = "Mystic Armor"
LOCATE 15, 6
PRINT "    ARMOR:", taname$
COLOR 15
LOCATE 17, 6
PRINT "GOLD:", gold
LOCATE 18, 6
IF hearts = 0 THEN COLOR 8: PRINT "HEARTS: "
IF hearts = 1 THEN COLOR 4: PRINT "HEARTS: ": COLOR 8: LOCATE 18, 15: PRINT ""
IF hearts = 2 THEN COLOR 4: PRINT "HEARTS: ": COLOR 8: LOCATE 18, 16: PRINT ""
IF hearts = 3 THEN COLOR 4: PRINT "HEARTS: "
DO: LOOP UNTIL INKEY$ <> ""
RETURN

1000
CLS
border
PAINT (1, 1), 12
COLOR 15
LOCATE 5, 8
PRINT "You have died.  The monster took half of your money.  Try again!"
SLEEP
gold = gold / 2: gold = INT(gold): mx = 40: my = 16: GOTO 1

1500
IF nex > 14 THEN IF nlev = 1 THEN nlev = 2: GOSUB 1550
IF nex > 35 THEN IF nlev = 2 THEN nlev = 3: GOSUB 1550
IF nex > 88 THEN IF nlev = 3 THEN nlev = 4: GOSUB 1550
IF nex > 145 THEN IF nlev = 4 THEN nlev = 5: GOSUB 1550
IF nex > 195 THEN IF nlev = 5 THEN nlev = 6: GOSUB 1550
IF nex > 282 THEN IF nlev = 6 THEN nlev = 7: GOSUB 1550
IF nex > 407 THEN IF nlev = 7 THEN nlev = 8: GOSUB 1550
IF nex > 803 THEN IF nlev = 8 THEN nlev = 9: GOSUB 1550
IF nex > 1235 THEN IF nlev = 9 THEN nlev = 10: GOSUB 1550
IF nex > 1500 THEN IF nlev = 10 THEN nlev = 11: GOSUB 1550
IF nex > 1935 THEN IF nlev = 11 THEN nlev = 12: GOSUB 1550
IF nex > 2200 THEN IF nlev = 12 THEN nlev = 13: GOSUB 1550
IF nex > 2400 THEN IF nlev = 13 THEN nlev = 14: GOSUB 1550
IF nex > 2575 THEN IF nlev = 14 THEN nlev = 15: GOSUB 1550
IF tex > 12 THEN IF tlev = 1 THEN tlev = 2: GOSUB 1560
IF tex > 30 THEN IF tlev = 2 THEN tlev = 3: GOSUB 1560
IF tex > 80 THEN IF tlev = 3 THEN tlev = 4: GOSUB 1560
IF tex > 125 THEN IF tlev = 4 THEN tlev = 5: GOSUB 1560
IF tex > 165 THEN IF tlev = 5 THEN tlev = 6: GOSUB 1560
IF tex > 222 THEN IF tlev = 6 THEN tlev = 7: GOSUB 1560
IF tex > 377 THEN IF tlev = 7 THEN tlev = 8: GOSUB 1560
IF tex > 733 THEN IF tlev = 8 THEN tlev = 9: GOSUB 1560
IF tex > 1175 THEN IF tlev = 9 THEN tlev = 10: GOSUB 1560
IF tex > 1450 THEN IF tlev = 10 THEN tlev = 11: GOSUB 1560
IF tex > 1539 THEN IF tlev = 11 THEN tlev = 12: GOSUB 1560
IF tex > 1850 THEN IF tlev = 12 THEN tlev = 13: GOSUB 1560
IF tex > 2031 THEN IF tlev = 13 THEN tlev = 14: GOSUB 1560
IF tex > 2300 THEN IF tlev = 14 THEN tlev = 15: GOSUB 1560
RETURN

1550
CLS
PAINT (1, 1), 10
border
COLOR 15
LOCATE 6, 12
PRINT "PRINCE NEON'S LEVEL WENT TO", nlev, "!"
IF nlev = 15 THEN LOCATE 7, 38: PRINT "HE IS FULLY TRAINED!"
PLAY "l9AGEEF#o1EDAo4L13AEF#"
nhpmax = nhpmax + 11
nmpmax = nmpmax + 5
SLEEP 7
RETURN

1560
CLS
PAINT (1, 1), 13
border
COLOR 15
LOCATE 6, 20
PRINT "THESSE'S LEVEL WENT TO", tlev, "!"
IF tlev = 15 THEN LOCATE 7, 38: PRINT "SHE IS FULLY TRAINED!"
PLAY "l11o5D#FFo3ABGF#l8EE"
thpmax = thpmax + 13
tmpmax = tmpmax + 3
SLEEP 7
RETURN

SUB birdhead
COLOR 14
LINE (460, 70)-(450, 90)
LINE -(430, 80)
LINE -(420, 50)
LINE -(460, 70)
LINE (500, 70)-(510, 90)
LINE -(530, 80)
LINE -(540, 50)
LINE -(500, 70)
PAINT (430, 70), 14
PAINT (520, 70), 14
LINE (540, 50)-(480, 30)
LINE -(420, 50)
LINE (430, 80)-(480, 130)
LINE -(530, 80)
COLOR 4
LINE (460, 70)-(500, 70)
LINE -(510, 90)
LINE -(480, 140)
LINE -(450, 90)
LINE -(460, 70)
PAINT (470, 100), 4
END SUB

SUB blast
FOR rt = 550 TO 100 STEP -3.7
CIRCLE (rt, 100), 38, 14
PAINT (rt, 100), 14
CIRCLE (rt, 100), 16, 15
PAINT (rt, 100), 15
SOUND 100 + (rt + INT(RND * 98) + 1), .12
NEXT rt
END SUB

SUB border
CLS
PAINT (1, 1), 1
COLOR 0
LINE (20, 10)-(620, 190), , BF
END SUB

SUB boss1
COLOR 3
LINE (490, 80)-(520, 100), , BF
CIRCLE (505, 72), 11, 13, , , .65
PAINT (505, 72), 13
COLOR 4
LINE (490, 100)-(520, 100)
LINE -(520, 127)
LINE -(496, 127)
LINE -(490, 100)
PAINT (500, 120), 4
LINE (507, 70)-(498, 72)
LINE -(498, 74)
LINE -(507, 70)
COLOR 14
LINE (500, 83)-(515, 103), , BF
END SUB

SUB boss3
DRAW "bm510, 100c4d4f15u19d4f15u19d4f15u49h6l27g12d23"
LINE (535, 75)-(480, 84), 15, BF
LINE (485, 75)-(512, 73), 15, BF
LINE (465, 75)-(480, 84), 4, BF
LINE (485, 74)-(475, 73), 4, BF
CIRCLE (535, 53), 16, 4, , , .69
COLOR 4
LINE (527, 50)-(527, 53)
LINE -(534, 47)
LINE -(527, 50)
END SUB

SUB bump
SOUND 155, 1
END SUB

SUB cecilslime
CIRCLE (500, 100), 25, 12, , , .34
PAINT (500, 100), 12
CIRCLE (490, 98), 4, 0
PAINT (490, 98), 0
COLOR 0
LINE (482, 106)-(494, 106)
END SUB

SUB cover
END SUB

SUB darkangel
DRAW "bm530, 65c1l4g6d9r4d14r2d4r1d13f23r4u6h15u46h2l9"
DRAW "bm532, 79c4d4e5f19u42g23e7u3e19d45h2"
DRAW "bm525, 72c4d2e4g2l2"
END SUB

SUB depths
PAINT (1, 1), 4
COLOR 12
LINE (10, 5)-(630, 185), , BF
CIRCLE (320, 170), 20, 0
PAINT (320, 170), 0
END SUB

SUB desk
COLOR 6
LINE (20, 120)-(210, 190), , BF
LINE (20, 110)-(230, 120), , BF
COLOR 1
LINE (60, 109)-(170, 80), , BF
CIRCLE (115, 70), 22, 13, , , .44
PAINT (115, 70), 13
CIRCLE (111, 67), 3, 1
CIRCLE (119, 67), 3, 1
PAINT (111, 67), 1
PAINT (119, 67), 1
COLOR 0
LINE (20, 120)-(210, 190), , B
LINE (20, 110)-(230, 120), , B
LINE (60, 109)-(170, 80), , B
LINE (90, 109)-(90, 91)
LINE (140, 109)-(140, 91)
LINE (111, 76)-(119, 76)

END SUB

SUB dragonfly
DRAW "bm560, 78c14e8f8f8d8g8l16h7u10e15c4l6u2l6u2l6u2l6u2l6u2l6u2l6g23r53bm567, 70r6u2r6u2r6u2r6u2r6u2r6u2r6ff22l52bm552, 95g6f14d6e7u6d6f7u6e14h6"
CIRCLE (568, 62), 12, 4, , , .67
PAINT (568, 62), 4
DRAW "bm568, 60c0h3l2d3f5e5u3l3g3"
PAINT (568, 63), 0
END SUB

SUB fire
PCOPY 0, 1
FOR km = 520 TO 50 STEP -5.4
CIRCLE (km, 100), 20, 4
PAINT (km, 100), 4
CIRCLE (km - 7, 100), 4, 13
PAINT (km - 7, 100), 13
COLOR 4
LINE (km, 94)-(km + 45, 100)
LINE -(km, 106)
PAINT (km + 25, 100), 4
PCOPY 1, 0
NEXT km
END SUB

SUB firebat
DRAW "bm520, 88c4e9f9l18"
PAINT (524, 86), 4
CIRCLE (530, 95), 24, 15
PAINT (530, 95), 15
DRAW "bm 520, 92c4d5e9l2d2l2d2l4"
PAINT (521, 94), 4
DRAW "bm530, 95c4e14f29l7u2l7u2l7u2l7u2l7u2l7u2l2u2"
PAINT (535, 95), 4
END SUB

SUB flames
0.148
wer = INT(RND * 380) + 1
ert = INT(RND * 190) + 1
IF wer < 20 THEN GOTO 0.148
LINE (wer, 190)-(wer, ert), 4
LINE (wer, 190)-(wer - 3, ert), 4
LINE (wer, 190)-(wer + 3, ert), 4
SOUND 40 + ert, .07
rty = rty + 1
IF rty < 400 THEN GOTO 0.148
rty = 0
END SUB

SUB fp
SCREEN 12
PAINT (1, 1), 1
COLOR 14
LINE (20, 20)-(620, 460), , BF
COLOR 4
LINE (0, 0)-(660, 480)
PAINT (1, 470), 4
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
SLEEP 1
FOR sizers = 1 TO 270 STEP 1
CIRCLE (320, 240), sizers, 0, , , .9
NEXT sizers
END SUB

SUB fury
PCOPY 0, 1   'Small Fireball
FOR i = 1 TO 600 STEP 7
COLOR 4
LINE (i, 98)-(i - 100, 100)
LINE (i, 102)-(i - 100, 100)
CIRCLE (i, 100), 4, 4
PAINT (i, 100), 4
PAINT (i - 20, 100), 4
CIRCLE (i, 100), 3, 12
PAINT (i, 100), 12
PCOPY 1, 0
SOUND 130, .05
NEXT i
END SUB

SUB heal
0.45
qq = INT(RND * 250) + 1
qr = INT(RND * 170) + 1
IF qq < 30 THEN GOTO 0.45 ELSE
COLOR 14
LINE (qq, qr)-(qq, qr + 3), , BF
SOUND 7000, .05
howmany = howmany + 1
IF howmany < 100 THEN GOTO 0.45 ELSE
howmany = 0
SLEEP 1
END SUB

SUB instructions
SCREEN 8, 0
CLS
PAINT (1, 1), 4
LOCATE 1, 33
PRINT " INSTRUCTIONS "
LOCATE 3, 5
PRINT "   The object of this game is to conquer all three worlds and get all  "
LOCATE 4, 5
PRINT "of the 's to restore peace to the world and to make the portal of the "
LOCATE 5, 5
PRINT "                      hellpit disappear forever                        "
LOCATE 6, 5
PRINT "  HP = Current Hit Points (losing all of these will result in death)   "
LOCATE 7, 5
PRINT "    SP = Special Points (these allow you to preform special moves)     "
LOCATE 8, 5
PRINT "     EX = Experience Points (gaining these will increase levels)       "
LOCATE 9, 5
PRINT "                $$ = Money (used to pay for many things)               "
LOCATE 10, 5
PRINT "                  LEVEL = How experienced a player is                  "
DO
PCOPY 0, 1
FOR xx = 1 TO 640 STEP 4
CIRCLE (xx, 100), 20, 1
PAINT (xx, 100), 20, 1
COLOR 1
LINE (xx, 92)-(xx - 60, 100)
LINE -(xx, 108)
PAINT (xx - 30, 100), 1
IF INKEY$ <> "" THEN GOTO 0.134
PCOPY 1, 0
NEXT xx
PCOPY 0, 1
COLOR 1
FOR xxx = 640 TO 1 STEP -4
CIRCLE (xxx, 100), 20, 1
PAINT (xxx, 100), 1
COLOR 1
LINE (xxx, 92)-(xxx + 60, 100)
LINE -(xxx, 108)
PAINT (xxx + 30, 100), 1
PCOPY 1, 0
IF INKEY$ <> "" THEN GOTO 0.134
NEXT xxx
LOOP UNTIL INKEY$ <> ""
0.134
CLS
END SUB

SUB intro
CLS
SCREEN 8, 0
PAINT (1, 1), 4
COLOR 15
LOCATE 1, 9
PRINT "  The world was a silent, peaceful place until a demonic man   "
LOCATE 2, 9
PRINT "opened the portal of hell.  When he realized what he had done, "
LOCATE 3, 9
PRINT "           he quickly told prince Neon of his fault            "
DO
FOR ai = 1 TO 150 STEP 1
CIRCLE (320, 120), ai, 12, , , .43
CIRCLE (320, 120), ai * .85, 13, , , .43
CIRCLE (320, 120), ai * .8, 0, , , .43
SOUND 80 + (ai / ai), .04
NEXT ai
FOR ab = 150 TO 1 STEP -1
CIRCLE (320, 120), ab, 4, , , .43
SOUND 130 + (ab + 13 / ab + 13), .04
NEXT ab
LOOP UNTIL INKEY$ <> ""
COLOR 15
LOCATE 1, 9
PRINT "     Prince Neon then called forth his most noble maiden,      "
LOCATE 2, 9
PRINT " Thesse, to join him in a quest to put and end to the demonic  "
LOCATE 3, 9
PRINT " portal, and to once again prove the courage of a true Prince  "
DO
PCOPY 0, 1
FOR aj = 1 TO 640 STEP 6
CIRCLE (aj, 100), 4, 1
PAINT (aj, 100), 1
CIRCLE (aj + 1, 100), 3, 9
PAINT (aj + 1, 100), 9
SOUND 650 + (aj / aj), .05
COLOR 1
LINE (aj, 98)-(aj - 45, 100)
LINE -(aj, 102)
PAINT (aj - 7, 100), 1
PCOPY 1, 0
NEXT aj
PCOPY 0, 2
FOR ak = 640 TO 1 STEP -6
CIRCLE (ak, 100), 4, 1
PAINT (ak, 100), 1
CIRCLE (ak, 100), 3, 9
PAINT (ak, 100), 9
COLOR 1
LINE (ak, 98)-(ak + 45, 100)
LINE -(ak, 102)
PAINT (ak + 7, 100), 1
IF INKEY$ <> "" THEN GOTO 0.135
PCOPY 2, 0
SOUND 750 + (ak / ak), .06
NEXT ak
LOOP UNTIL INKEY$ <> ""
0.135
CLS
END SUB

SUB main
PAINT (1, 1), 4
COLOR 13
LINE (20, 10)-(620, 190), , BF
COLOR 15
LOCATE 9, 34
PRINT "PLAY"
LOCATE 11, 34
PRINT "INSTRUCTIONS"
LOCATE 13, 34
PRINT "INTRO"
LOCATE 15, 34
PRINT "LOAD GAME"
LOCATE 17, 34
PRINT "QUIT"
END SUB

SUB neon
COLOR 1
LINE (70, 110)-(83, 118), , BF
COLOR 6
LINE (70, 118)-(82, 126), , BF
COLOR 14
LINE (73, 112)-(78, 120), , BF
LINE (71, 127)-(80, 127)
LINE -(83, 129)
LINE -(71, 129)
LINE -(71, 127): PAINT (74, 128), 14
COLOR 12
CIRCLE (77, 106), 4.4, 12, , , .84
PAINT (77, 106), 12
COLOR 0
LINE (78, 105)-(78, 105)
END SUB

SUB nthrow
COLOR 1
LINE (73, 112)-(78, 120), , BF
COLOR 14
LINE (73, 112)-(91, 114), , BF
COLOR 6
LINE (73, 118)-(78, 120), , BF
SOUND 147, .31
END SUB

SUB omega
FOR qoi = 1 TO 160 STEP 1.2
od = INT(RND * 3) + 1
IF od = 1 THEN od = 1
IF od = 2 THEN od = 4
IF od = 3 THEN od = 15
CIRCLE (320, 100), qoi, od
CIRCLE (320, 100), qoi, od
CIRCLE (320, 100), qoi, od
SOUND 100 + qoi - qoi, .1
NEXT qoi
END SUB

SUB quit
CLS
PAINT (1, 1), 4
DO
FOR yyyy = 2 TO 23 STEP 1
zz = INT(RND * 16) + 1
IF zz = 16 THEN zz = 0
COLOR zz
LOCATE yyyy, 29
PRINT "THANKS FOR PLAYING!!!!"
LOCATE yyyy, 3
PRINT "THANKS FOR PLAYING!!!!"
LOCATE yyyy, 55
PRINT "THANKS FOR PLAYING!!!!"
NEXT yyyy
LOOP UNTIL INKEY$ <> ""
FOR zwxy = 1 TO 340 STEP 1
CIRCLE (320, 100), zwxy, 1, , , .34
SOUND 100 + (zwxy / zwxy), .03
NEXT zwxy
FOR zwy = 340 TO 1 STEP -2
COLOR 0
CIRCLE (320, 100), zwy, 0, , , .34
SOUND 300 + (zwy + 6 / zwy + 6), .06
NEXT zwy
END
END SUB

SUB resurrect
COLOR 4
CIRCLE (480, 100), 15, 4
PAINT (480, 100), 4
COLOR 4
LINE (478, 89)-(482, 110), , BF
LINE (482, 110)-(460, 112), , BF
LINE (482, 89)-(467, 90), , BF
LINE (483, 96)-(540, 100)
LINE -(483, 104)
PAINT (520, 100), 4
CIRCLE (476, 97), 3, 15
PAINT (476, 97), 15
LINE (523, 100)-(570, 100)
COLOR 15
LINE (467, 103)-(479, 103)
END SUB

SUB ruins
PAINT (1, 1), 12
COLOR 13
LINE (10, 5)-(630, 185), , BF
COLOR 4
LINE (10, 5)-(50, 25), , BF
COLOR 0
LINE (20, 10)-(40, 20), , B
CIRCLE (600, 170), 17, 0
PAINT (600, 170), 0
END SUB

SUB sai
PCOPY 0, 1
FOR tw = 140 TO 580 STEP 4.5
COLOR 4
LINE (tw - 25, 125)-(tw, 127), , BF
COLOR 1
LINE (tw, 125)-(tw, 127)
LINE -(tw + 50, 126)
LINE -(tw, 125)
PAINT (tw + 10, 126), 1
LINE (tw, 127)-(tw + 20, 130)
LINE (tw, 125)-(tw + 20, 122)
PCOPY 1, 0
SOUND 120 + tw, .04
NEXT tw
END SUB

SUB satan
LINE (473, 58)-(479, 141), 13, BF
DRAW "bm498, 65c15g2d34r39u34h2l34"
PAINT (515, 100), 15
DRAW "bm496, 102c4d13e4d6r2e4f6d4r1e4u2r4f6u7r3e4u3r1u7l39"
PAINT (515, 120), 4
DRAW "bm530, 70c12d7g9d2l35g2l16h2u9e2r16f2r14e4u6e2r21f2"
PAINT (516, 87), 12
CIRCLE (514, 54), 16, 4, , , .66
PAINT (514, 54), 4
COLOR 15
LINE (506, 51)-(506, 54)
LINE -(514, 48)
LINE -(506, 51)
LINE (461, 58)-(491, 58)
LINE -(493, 56)
LINE -(493, 42)
LINE -(483, 54)
LINE -(476, 31)
LINE -(471, 54)
LINE -(459, 42)
LINE -(459, 56)
LINE -(461, 58)
PAINT (476, 38), 15
END SUB

SUB shot
PCOPY 0, 1
FOR mile = 500 TO 60 STEP -5.1
CIRCLE (mile, 100), 9.6, 12
PAINT (mile, 100), 12
COLOR 12
LINE (mile, 100)-(mile + 40, 100)
PCOPY 1, 0
NEXT mile
END SUB

SUB spirit
DRAW "bm 550, 85c15f15d12g23u13h9u13bm540, 100u9l8g13l6f3g6r3e5f3u3e8r10bm550, 85u6h6l12g5d6f6bm539, 75g6u2bm533, 84r3f4"
END SUB

SUB spiritland
PAINT (1, 1), 14
COLOR 15
LINE (10, 5)-(630, 185), , BF
CIRCLE (320, 170), 20, 0
PAINT (320, 170), 0
END SUB

SUB stars
PCOPY 0, 1
FOR ed = 70 TO 570 STEP 6.6
COLOR 8
LINE (ed + 1, 118)-(ed - 2, 121)
LINE -(ed - 10, 123)
LINE -(ed - 2, 125)
LINE -(ed + 1, 128)
LINE -(ed + 4, 125)
LINE -(ed + 12, 123)
LINE -(ed + 4, 121)
LINE -(ed + 1, 118)
PAINT (ed + 1, 121), 8
CIRCLE (ed + 1, 123), 6.5, 4
PAINT (ed + 1, 123), 4
SOUND 1000 + ed - ed, .2
PCOPY 1, 0
NEXT ed
END SUB

SUB tattack
COLOR 0
LINE (110, 60)-(143, 100), , BF
COLOR 12
LINE (430, 100)-(437, 103), , BF
COLOR 4
LINE (430, 100)-(437, 100)
LINE -(443, 96)
LINE -(425, 96)
LINE -(430, 100)
PAINT (433, 98), 4
LINE (430, 103)-(437, 103)
LINE -(441, 110)
LINE -(426, 110)
LINE -(430, 103)
PAINT (434, 105), 4
COLOR 15
LINE (432, 97)-(450, 98), , BF
LINE (431, 111)-(431, 113)
LINE -(438, 113)
LINE -(433, 111)
LINE -(431, 111)
PAINT (432, 112), 15
COLOR 12
CIRCLE (435, 92), 4.2, 12, , , .82
PAINT (435, 92), 12
COLOR 0
LINE (436, 91)-(436, 91), , BF
COLOR 14
LINE (434, 89)-(436, 89)
LINE (434, 89)-(427, 95)
LINE -(429, 95)
LINE -(434, 89)
COLOR 1
LINE (450, 97)-(450, 98)
LINE -(480, 97)
LINE -(450, 97)
SOUND 170, .5
SOUND 230, .7
SLEEP 1
COLOR 0
LINE (410, 80)-(443, 120), , BF
LINE (430, 97)-(481, 98), , BF
END SUB

SUB thesse
COLOR 12
LINE (130, 80)-(137, 83), , BF
COLOR 4
LINE (130, 80)-(137, 80)
LINE -(143, 76)
LINE -(125, 76)
LINE -(130, 80)
PAINT (133, 78), 4
LINE (130, 83)-(137, 83)
LINE -(141, 90)
LINE -(126, 90)
LINE -(130, 83)
PAINT (134, 85), 4
COLOR 15
LINE (132, 77)-(135, 85), , BF
LINE (131, 91)-(131, 93)
LINE -(138, 93)
LINE -(133, 91)
LINE -(131, 91)
PAINT (132, 92), 15
COLOR 12
CIRCLE (135, 72), 4.2, 12, , , .82
PAINT (135, 72), 12
COLOR 0
LINE (136, 71)-(136, 71), , BF
COLOR 14
LINE (134, 69)-(136, 69)
LINE (134, 69)-(127, 75)
LINE -(129, 75)
LINE -(134, 69)

END SUB

SUB title
SCREEN 8, 0
PAINT (1, 1), 12
DO
ie = INT(RND * 15) + 1
IF ie = 12 THEN ie = 0
COLOR ie
LOCATE 3, 34
PRINT "THE HELLPIT"
PCOPY 0, 1
FOR a = 1 TO 600 STEP 12
COLOR 0
LINE (a - 35, 99)-(a, 101), , BF
COLOR ie
LINE (a, 95)-(a, 105)
LINE -(a + 10, 103)
LINE -(a, 101)
LINE -(a + 55, 100)
LINE -(a, 99)
LINE -(a + 10, 97)
LINE -(a, 95)
PAINT (a + 3, 100), ie
SOUND 100 + (a / a), .04
PCOPY 1, 0
NEXT a
LOOP UNTIL INKEY$ <> ""
END SUB

SUB town
PAINT (1, 1), 8
COLOR 10
LINE (10, 5)-(630, 185), , BF
CIRCLE (320, 100), 18, 0
PAINT (320, 100), 0
CIRCLE (320, 100), 14, 12
PAINT (320, 100), 12
CIRCLE (320, 100), 10, 4
PAINT (320, 100), 4
COLOR 3
LINE (80, 110)-(140, 86), , BF
COLOR 2
LINE (530, 100)-(590, 76), , BF
COLOR 1
LINE (240, 160)-(300, 136), , BF
COLOR 8
LINE (240, 60)-(400, 0), , BF
COLOR 6
LINE (290, 60)-(350, 40), , BF
COLOR 0
LINE (240, 60)-(400, -1), , B
LINE (240, 60)-(270, -1), , B
LINE (400, 60)-(370, -1), , B
LINE (320, 60)-(320, 40)
COLOR 15
LINE (100, 100)-(120, 110), , BF
LINE (550, 100)-(570, 90), , BF
LINE (260, 160)-(280, 150), , BF

END SUB

SUB tundra
FOR qw = 1 TO 140 STEP 1
CIRCLE (190, 100), qw, 9, , , .67
SOUND (1000 + (qw * 2.5)), .11
NEXT qw
COLOR 1
LINE (190, 100)-(620, 40)
LINE -(620, 160)
LINE -(190, 100)
PAINT (250, 100), 1
COLOR 15
LINE (190, 100)-(620, 80)
LINE -(620, 120)
LINE -(190, 100)
PAINT (260, 100), 15
1000.1
SOUND 1000, .6
SOUND 2000, .5
SOUND 3000, .4
wow = wow + 1
IF wow < 11 THEN GOTO 1000.1 ELSE
END SUB

SUB ultra
FOR up = 100 TO 1 STEP -1
fgh = INT(RND * 30) + 1
IF fgh < 10 THEN fgh = 4
IF fgh > 9 THEN IF fgh < 21 THEN fgh = 12
IF fgh > 20 THEN fgh = 13
LINE (21, up)-(619, up), fgh
ghi = INT(RND * 200) + 1
LINE (21, ghi)-(619, ghi), 14
LINE (21, 200 - up)-(619, 200 - up), fgh
SOUND 3000 + up - up, .25
NEXT up
END SUB

SUB undead
COLOR 8
LINE (490, 80)-(500, 120)
LINE -(510, 80)
CIRCLE (500, 75), 16, 8, , , .43
PAINT (500, 75), 8
PAINT (500, 105), 8
LINE (500, 86)-(460, 92), , BF
CIRCLE (458, 89), 4, 8
PAINT (458, 89), 8
CIRCLE (495, 73), 4, 15
PAINT (495, 73), 15
END SUB

SUB wounds
0.956
re = INT(RND * 16) + 1
IF re = 16 THEN re = 0
que = INT(RND * 590) + 1
qur = INT(RND * 170) + 1
IF que < 400 THEN GOTO 0.956
IF qur < 50 THEN GOTO 0.956
COLOR re
LINE (que, qur)-(que + 30, qur + 4)
LINE -(que + 34, qur)
LINE -(que + 30, qur - 4)
LINE -(que, qur)
PAINT (que + 20, qur), re
SOUND 200 + (whatap * 4), .06
whatap = whatap + 1
IF whatap < 120 THEN GOTO 0.956 ELSE
whatap = 0
END SUB

SUB zheal
1976
xs = INT(RND * 270) + 1
ys = INT(RND * 180) + 1
IF xs < 21 THEN GOTO 1976
IF ys < 11 THEN GOTO 1976
LINE (xs, ys)-(xs, ys), 14
SOUND 3000, .05
frse = frse + 1
IF frse < 900 THEN GOTO 1976 ELSE
END SUB

SUB zvolt
FOR getit = 100 TO 10 STEP -.1
qwe = INT(RND * 2) + 1
IF qwe = 1 THEN qwe = 14
IF qwe = 2 THEN qwe = 15
LINE (21, getit)-(619, getit), qwe, BF
LINE (21, 200 - getit)-(619, 200 - getit), qwe, BF
SOUND 2000 + (-getit * 19), .1
NEXT getit
END SUB


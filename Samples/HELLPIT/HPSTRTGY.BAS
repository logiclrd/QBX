DECLARE SUB fp ()

fp

0

SCREEN 8
CLS
PAINT (1, 1), 4
COLOR 15, 4
LOCATE 1, 24
PRINT "HELLPIT I, II and III STRATEGY GUIDE"

LOCATE 4, 1
PRINT "     Welcome to FIG's SUPER HELLPIT strategy guide.  I am FIG, and I"
PRINT "programmed the game, so everything here is completely true.  In this strategy"
PRINT "guide, I will reveal everything about every HELLPIT game."
PRINT "     HELLPIT is an RPG that started out as a side project while I was working"
PRINT "on other games, but I found that HELLPIT was a good game, so I continued it, "
PRINT "making HELLPIT II.  After HELLPIT II was done, I recieved tons of e-mail"
PRINT "from HELLPIT fans who were anxious to play HPIII, so I started to work on it"
PRINT "right away.  After HPIII was done, I first decided that I would make no more"
PRINT "HELLPIT games, but now I changed my mind (hint, hint)"
PRINT "     And as always, for those of you who are programmers, or plan to become "
PRINT "one, read 'aboutfig.txt'."
LOCATE 23, 46
PRINT "Press  to go to next screen"

DO
a$ = INKEY$
IF a$ = CHR$(0) + "M" THEN GOTO 1
LOOP

1
CLS
PAINT (1, 1), 4
COLOR 15, 4
LOCATE 4, 1
PRINT "     Back to HELLPIT.  Anyways, Neon was supposed to be a bad guy for my other"
PRINT "planned RPG, BAD OMENS, but when I decided to cancel BAD OMENS, I thought that"
PRINT "NEON was a pretty good character.  Thesse was originally named VISION, and she"
PRINT "was also to be in BAD OMENS as the main evil, but I thought she was a pretty "
PRINT "good character herself, so I renamed her, and changed her around a bit.  David"
PRINT "was a third character in because I wanted to put the guy who opened the portal"
PRINT "in the game.  I got the name from a friend who I call Brother David."
PRINT "     I thought it was a good idea to make Thesse turn evil, so that I could "
PRINT "get another character in the game, and so it had a twist and a reason for "
PRINT "returning to hell."
PRINT "     This has definitely been the best, and longest game I have ever made, and"
PRINT "I won't make an RPG as long as this for a while.  I have had a couple of"
PRINT "people test the game, and I have debugged it.  There may still be some bugs,"
PRINT "but unfortunately, I am not making any more versions of any HELLPITS.  It's"
PRINT "really boring, and it takes too much time.  I have planned many RPG's, and I "
PRINT "cancel more than 50% of them.  I have cancelled THE CONJURING, BAD OMENS, and "
PRINT "THE ETERNAL WARS.  All of these were planned to be good, long RPG's, but there"
PRINT "was always something that got in the way."

LOCATE 23, 5
PRINT "Press  to go to previous screen"
LOCATE 23, 46
PRINT "Press  to go to next screen"

DO
a$ = INKEY$
IF a$ = CHR$(0) + "M" THEN GOTO 2
IF a$ = CHR$(0) + "K" THEN GOTO 0
LOOP


2
CLS
PAINT (1, 1), 4
COLOR 15, 4
LOCATE 4, 1
PRINT "     You may view the strategy guides for HELLPIT I, II or III."
LOCATE 9, 25
PRINT "Press 1 to view HELLPIT I"
LOCATE 10, 25
PRINT "Press 2 to view HELLPIT II"
LOCATE 11, 25
PRINT "Press 3 to view HELLPIT III"
LOCATE 23, 5
PRINT "Press  to go to previous screen"

DO
a$ = INKEY$
IF a$ = CHR$(0) + "K" THEN GOTO 1
IF a$ = "1" THEN GOTO 10
IF a$ = "2" THEN GOTO 20
IF a$ = "3" THEN GOTO 30
LOOP


10
CLS
LOCATE 1, 36
PRINT "HELLPIT"
LOCATE 3, 1
PRINT "     Hellpit was my first RPG that was made by FIG.  I had another RPG, but it"
PRINT "was by DELFIGALLO.  It was called THE CONJURING.  It was really short, but it"
PRINT "wasn't that bad.  Anyways, Hellpit turned out to be a really fun game, and I "
PRINT "recieved many good comments about the game.  If you do not have Hellpit,"
PRINT "check the AOL files (BASIC), or e-mail me at PANTERA55@aol.com.  It is a fun"
PRINT "game, but Hellpit II is much better."
PRINT
PRINT "THE CHARACTERS"
PRINT
PRINT "     There were two characters in Hellpit.  Their names were NEON and THESSE."
PRINT "Neon was a death mage who is able to use projectiles as weapons, such as sais"
PRINT "and stars.  He goes up in levels slower than THESSE, and his Hpmax was lower"
PRINT "than THESSE, but his Mpmax was higher.  Thesse is a cleric with some healing"
PRINT "abilities, as well as some attack techniques.  She is very quick to raise her"
PRINT "levels, and is quite powerful.  She uses swords."
LOCATE 23, 5
PRINT "Press  to go to previous screen"
LOCATE 23, 46
PRINT "Press  to go to next screen"

DO
a$ = INKEY$
IF a$ = CHR$(0) + "K" THEN GOTO 2
IF a$ = CHR$(0) + "M" THEN GOTO 11
LOOP

11
CLS
LOCATE 3, 1
PRINT "WEAPONS AND ARMORS"
PRINT
PRINT "     Each fighter is able to hold different weapons, but they are also able"
PRINT "to use the same armors.  Here is a list of Weapons and Armors:"
PRINT
PRINT "   ARMORS               WEAPONS(Thesse)           WEAPONS(Neon) "
PRINT "   Radical Armor        Thrust Blade              Sess Sais     "
PRINT "   Tri Plate            Stealth Sword             Soul Stars    "
PRINT "   Mystic Armor         Z Sword                   Z Sais        "
PRINT
PRINT "     Here is a list of magic moves for both:"
PRINT
PRINT "   NEON                                  THESSE"
PRINT
PRINT "   Fury   (Mp - 2)                       Heal   (Mp - 2)"
PRINT "   Tundra (Mp - 3)                       Wounds (Mp - 3)"
PRINT "   Z-Bolt (Mp - 5)                       Z-Heal (Mp - 5)"
PRINT
PRINT "     The peak levels for both warriors is 15.  Their levels cannot go any"
PRINT "higher.  That also means that experience becomes worthless at level 15.  "

LOCATE 23, 5
PRINT "Press  to go to previous screen"
LOCATE 23, 46
PRINT "Press  to go to next screen"

DO
a$ = INKEY$
IF a$ = CHR$(0) + "K" THEN GOTO 10
IF a$ = CHR$(0) + "M" THEN GOTO 12
LOOP

12
CLS
LOCATE 3, 1
PRINT "YOUR ADVENTURE"
PRINT
PRINT "     Your adventure becomes tough when you decide to try to save your Kingdom."
PRINT "Neon and Thesse must grow strong in order to attempt to save their Kingdom.  To"
PRINT "become stronger, they must possess the three 's of strength.  Then, they will"
PRINT "be able to oppose the master of evil, Satan.  Satan is not an easy boss.  He has"
PRINT "a lot of power, and he can destroy you with just a few shots.  Anyways, here is"
PRINT "a step by step guide on how to defeat each level."
PRINT
PRINT "LEVEL 1 - THE RUINS (FIRST )"
PRINT
PRINT "     The ruins are pretty easy.  It contains three enemies, and one boss.  This"
PRINT "allows you to get your levels up, I think that by the time you are out of there,"
PRINT "you will have levels 6 - 8.  Here is a breakdown of the enemies:"
PRINT
PRINT "CECILSLIME - Weak, about 4 hp, any magic will wipe him out."
PRINT "UNDEAD - Also very weak, about 6 hp, any level 2 magic will kill it."
PRINT "RESURRECT - Medium power, about 23 hp, use magics on it."
PRINT "BOSS - Medium power, about 65 hp, use all magics and heals on him."

LOCATE 23, 5
PRINT "Press  to go to previous screen"
LOCATE 23, 46
PRINT "Press  to go to next screen"

DO
a$ = INKEY$
IF a$ = CHR$(0) + "K" THEN GOTO 11
IF a$ = CHR$(0) + "M" THEN GOTO 13
LOOP

13
CLS
LOCATE 3, 1
PRINT "LEVEL 2 - SPIRITLAND (SECOND )"
PRINT
PRINT "     The spiritlands are a medium challenge.  There are two enemies, spirits"
PRINT "and Dragonflies.  Each of them are reasonably powerful.  This is a great place"
PRINT "to raise some levels."
PRINT
PRINT "DRAGONFLY - O.K Power, 24 hp, use attacks or magics."
PRINT "SPIRIT - O.K Power, 32 hp, magics or attacks combined are good."
PRINT "BOSS - Powerful, 610 hp, use a lot of ice on him to defeat him."
PRINT
PRINT "LEVEL 3 - THE DEPTHS (THIRD )"
PRINT
PRINT "     This is the hardest place.  There are darkangels and firebats inside this"
PRINT "portal.  They are all very powerful.  This is the place that will get your"
PRINT "levels maxed out!"
PRINT
PRINT "DARKANGEL - Strong, 92 hp, use attacks and magics to destroy them."
PRINT "FIREBAT - Strong, 107 hp, use ice and attacks to destroy them."
PRINT "BOSS - Very strong, 2000 hp, use magics and heals to be victorious."

LOCATE 23, 5
PRINT "Press  to go to previous screen"
LOCATE 23, 46
PRINT "Press  to go to next screen"

DO
a$ = INKEY$
IF a$ = CHR$(0) + "K" THEN GOTO 12
IF a$ = CHR$(0) + "M" THEN GOTO 14
LOOP

14
CLS
LOCATE 3, 1
PRINT "FINAL BOSS"
PRINT
PRINT "     The final boss of the game is Satan.  He is the most powerful enemy in"
PRINT "the game.  You must use all of your power to defeat him."
PRINT
PRINT "SATAN - 3500 hp, Extremely Strong, use any magics against him.  Be sure to"
PRINT "heal at all times.  Satan's fire is very strong."
PRINT
PRINT "SKILLS"
PRINT
PRINT "     The most powerful tactics in the game are:"
PRINT
PRINT "     Don't use Z-Heal because you will have no MP left"
PRINT "     Use Tundra against all of the bosses, and attack and heal with Thesse"
PRINT "     Have about level 14 before you fight Satan"
PRINT
PRINT "     That's it for HELLPIT.  Now, you should go view the HELLPIT II guide."

LOCATE 23, 5
PRINT "Press  to go to previous screen"
LOCATE 23, 46
PRINT "Press  to go to options screen"

DO
a$ = INKEY$
IF a$ = CHR$(0) + "K" THEN GOTO 13
IF a$ = CHR$(0) + "M" THEN GOTO 2
LOOP


20
CLS
LOCATE 3, 1
PRINT "HELLPIT II: INTO THE LUNGS OF HELL"
PRINT
PRINT "     This Hellpit was definitely the longest of the three, and may be the"
PRINT "funnest, but the graphics of Hellpit III blow this game away.  This is still"
PRINT "a great game.  In HELLPIT I, Thesse is taken over by the devil's spirit, so"
PRINT "the mission for HELLPIT II is to attempt to get Thesse back.  You also get"
PRINT "some help from a new character, David, who originally opened the first portal."
PRINT "This just may be the most challenging HELLPIT game."
PRINT
PRINT "WEAPONS AND ARMORS"
PRINT
PRINT "     There are five weapons and three armors.  Neon and David each use the"
PRINT "same weapons and armors."
PRINT
PRINT "     WEAPONS                     ARMORS"
PRINT "     X-Stars                     X-Armor"
PRINT "     Ravage Stars                Z-Armor"
PRINT "     Hybrid Stars                Exo-Vest"
PRINT "     Z-Stars"
PRINT "     Exo-Stars"
PRINT

LOCATE 23, 5
PRINT "Press  to go to previous screen"
LOCATE 23, 46
PRINT "Press  to go to next screen"

DO
a$ = INKEY$
IF a$ = CHR$(0) + "K" THEN GOTO 2
IF a$ = CHR$(0) + "M" THEN GOTO 21
LOOP

21
CLS
LOCATE 3, 1
PRINT "     Thesse uses an Exo-Sword, and a Fire Cape, and you can get an Ultimate"
PRINT "sword for her, too!"
PRINT
PRINT "PORTAL 1"
PRINT
PRINT "     Portal 1 is reasonably easy, but I don't recommend just walking in, and"
PRINT "trying to go straight to the Crystal.  I recommend that you kick butt in there"
PRINT "until your characters are about level four or five, going to the inn, and"
PRINT "returning to get the crystal.  The bats are easy; use the opposite magic of"
PRINT "what they are.  For example, use lava on an icebat.  The boss is easy, too."
PRINT "Just use lots of magic with Neon, and Attack and heal with David."
PRINT
PRINT "PORTAL 2"
PRINT
PRINT "     Portal 2 is a big step from portal 1.  The enemies are much harder, and"
PRINT "you may find yourself wasting a lot of magic.  I suggest that you go into the"
PRINT "portal, kick butt until your MP is wasted, leave, and then go for the cystal."
PRINT "The enemies are skulls, and as always, jsut use the opposite magics of what "
PRINT "types of skulls they are.  The boss is a darkskull, so use any magic on it."
PRINT

LOCATE 23, 5
PRINT "Press  to go to previous screen"
LOCATE 23, 46
PRINT "Press  to go to next screen"

DO
a$ = INKEY$
IF a$ = CHR$(0) + "K" THEN GOTO 20
IF a$ = CHR$(0) + "M" THEN GOTO 22
LOOP

22
CLS
LOCATE 3, 1
PRINT "PORTAL 3"
PRINT
PRINT "     This portal is inhabited by zombies, and the boss is Thesse.  This is"
PRINT "a very difficult portal, so I think that you may have to go in there twice."
PRINT "Use anything but fire on Thesse, and magic works well on the zombies."
PRINT
PRINT "PORTAL 4"
PRINT
PRINT "     You may be able to go right into portal 4, kick butt, and leave, but your"
PRINT "levels better be high!  These mages resist magic, but the boss doesn't.  Just"
PRINT "keep blasting it with Neon, while attacking and healing with David.  Oh, and"
PRINT "after you defeat the fourth portal, you can cheat your way to victory!"
PRINT
PRINT "PORTAL 5"
PRINT
PRINT "     This portal has deadly skeletons, but now your magics will work.  I"
PRINT "think that you might be able to survive by just walking into the portal, and"
PRINT "killing everything in site.  The boss is reasonably strong, but use the same"
PRINT "methods to defeat the boss."

LOCATE 23, 5
PRINT "Press  to go to previous screen"
LOCATE 23, 46
PRINT "Press  to go to next screen"

DO
a$ = INKEY$
IF a$ = CHR$(0) + "K" THEN GOTO 21
IF a$ = CHR$(0) + "M" THEN GOTO 23
LOOP

23
CLS
LOCATE 3, 1
PRINT "PORTAL 6"
PRINT
PRINT "     This portal is deadly.  The enemies are entities, and the boss is really"
PRINT "deadly.  You're gonna have to get in and out as quick as possible.  Use a"
PRINT "lot of magics by Neon, and only heal with David.  At the boss, use a lot of"
PRINT "heals, and only use Restores when absolutely nescessary.  When you beat the"
PRINT "last boss, the final portal will open."
PRINT
PRINT "PORTAL 7"
PRINT
PRINT "     The final portal will open in the middle of Hell, and it will be blue."
PRINT "Inside the portal, you'll have to fight Thesse.  She'll take long to defeat, "
PRINT "but then you'll have her on your team.  Then, you must go kill the entity "
PRINT "that is always putting those damn signs in front of the portals.  After you"
PRINT "kill him, you win."

LOCATE 23, 5
PRINT "Press  to go to previous screen"
LOCATE 23, 46
PRINT "Press  to go to next screen"

DO
a$ = INKEY$
IF a$ = CHR$(0) + "K" THEN GOTO 22
IF a$ = CHR$(0) + "M" THEN GOTO 24
LOOP

24
CLS
LOCATE 3, 1
PRINT "SECRETS"
PRINT
PRINT "     The first secret is: go to the complete top left of Hell to enter FIG'S"
PRINT "SECRET ROOM.  In FIG'S SECRET ROOM, you can do almost anything for a small"
PRINT "fee of $125."
PRINT
PRINT "     The second secret is for a TRIPLE TECH MAGIC MOVE!  When Thesse is on"
PRINT "your team, press BACKSPACE to activate it.  It takes 6MP from each character,"
PRINT "but it is SO Deadly.  Damage varies from about 2000 to 5000!"
PRINT
PRINT "     In the end, Neon and Thesse take a vacation, and David hides the six"
PRINT "crystals.  That's it!"

LOCATE 23, 5
PRINT "Press  to go to previous screen"
LOCATE 23, 46
PRINT "Press  to go to options screen"

DO
a$ = INKEY$
IF a$ = CHR$(0) + "K" THEN GOTO 23
IF a$ = CHR$(0) + "M" THEN GOTO 2
LOOP

30
CLS
LOCATE 3, 1
PRINT "HELLPIT III: FINAL FLAME"
PRINT
PRINT "     After you defeat the Essence of Evil, a new evil begins to terrorize"
PRINT "earth.  The town is nearly destroyed, and Neon and Thesse are still away.  A"
PRINT "new portal opens inside the castle.  David decides to go in by himself.  This"
PRINT "HELLPIT does have the best graphics, but is shorter than HELLPIT II."
PRINT
PRINT "WEAPONS AND ARMORS"
PRINT
PRINT "WEAPONS (N and D)   WEAPONS(T)          ARMORS(N and D)    ARMORS(T)"
PRINT
PRINT "EXO-STARS           EXO-SWORD           EXO-VEST           FIRE CAPE"
PRINT "ENTITY STARS        ENTITY BLADE        ENTITY MAIL        ENTITY DRESS"
PRINT "                    DREAM SWORD"
PRINT
PRINT "A NEW ITEM"
PRINT
PRINT "     In HELLPIT III, you can purchase Blood Rain Orbs from the blacksmith."
PRINT "Blood Rain Orbs are $125, and give a guaranteed 400+ hit.  Save these for the"
PRINT "bosses."

LOCATE 23, 5
PRINT "Press  to go to previous screen"
LOCATE 23, 46
PRINT "Press  to go to next screen"

DO
a$ = INKEY$
IF a$ = CHR$(0) + "M" THEN GOTO 31
IF a$ = CHR$(0) + "K" THEN GOTO 2
LOOP

31
CLS
LOCATE 3, 1
PRINT "PORTAL 1"
PRINT
PRINT "     Portal 1 may be the hardest portal in the game, but I suggest that you"
PRINT "just try to go straight to the boss without exiting the portal.  Trust me on"
PRINT "this one.  This portal's enemies are skulls, but not the same as the ones"
PRINT "from HELLPIT II.  The boss is quite difficult, but once you are victorious, you"
PRINT "get Neon and Thesse back."
PRINT
PRINT "PORTAL 2"
PRINT
PRINT "     Portal 2 is a bit easier 'cause you can use all three of your players."
PRINT "Shrooms are the enemies, but using opposite magic will put them away easily."
PRINT "The boss isn't too difficult.  Use magics with Neon and Thesse, and attack"
PRINT "and heal with David."
PRINT
PRINT "PORTAL 3"
PRINT
PRINT "     This is where the action takes place.  No enemies, just one boss- The"
PRINT "Essence of Evil.  Once you defeat him (it takes long), the Essence will blow"
PRINT "up, and kill you.  Then, you're in for a little surprise!"

LOCATE 23, 5
PRINT "Press  to go to previous screen"
LOCATE 23, 46
PRINT "Press  to go to next screen"

DO
a$ = INKEY$
IF a$ = CHR$(0) + "M" THEN GOTO 32
IF a$ = CHR$(0) + "K" THEN GOTO 30
LOOP

32
CLS
LOCATE 3, 1
PRINT "FINAL SHOWDOWN"
PRINT
PRINT "     In this FINAL SHOWDOWN, you control the Wiseman, and you must try to"
PRINT "destroy the world.  You have deadly magics, and after winning, you have a"
PRINT "choice of one wish."
PRINT
PRINT "YOUR WISH IS MY COMMAND"
PRINT
PRINT "     When you pass the game, you can either 1) save Neon, David and Thesse,"
PRINT "2) become the most powerful being ever, or 3) destroy the world.  I'll let"
PRINT "you figure out what happens for each."
PRINT
PRINT "SECRETS"
PRINT
PRINT "     1.  In the first portal, go in between the wall to access a secret!"
PRINT "     ex: ÛÛ (Through the wall from the bottom)"
PRINT "     2.  At the very last boss, with the Wise Man, press 'F12' for the secret"
PRINT "     move!  It takes no mp, and it's DAMN strong!"
PRINT "     3.  Go to the bottom left corner in the town for FIG'S SECRET ROOM II!"
PRINT "     4.  In the town, press 'F11' for THESSE'S secret weapon!"
PRINT "     5.  In the town, press 'F2' for THESSE'S secret armor!"
PRINT
PRINT "     And that's it!"

LOCATE 23, 5
PRINT "Press  to go to previous screen"
LOCATE 23, 46
PRINT "Press  to go to options screen"

DO
a$ = INKEY$
IF a$ = CHR$(0) + "M" THEN GOTO 2
IF a$ = CHR$(0) + "K" THEN GOTO 31
LOOP

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
FOR sizers = 0 TO 320 STEP 1
CIRCLE (320, 240), sizers, 0, , , .9
SOUND 100 + (sizers * .8), .04
LINE (sizers, 0)-(sizers, 480), 0, B
LINE (640 - sizers, 0)-(640 - sizers, 480), 0, B
NEXT sizers
END SUB


1 REM Originally published in Spectra View magazine, year 2 issues 1-3.
2 REM https://www.samdal.com/svdocuments.htm

10 CLS : CLEAR 3000: DIM R$(9), N%(9), S%(9), O%(9), V%(9), S$(7), P%(7), K$(7), V$(19)
20 REM RESTORE 7000: REM Originalsvenska
30 RESTORE 8000: REM English translation
40 FOR I = 1 TO 5
50   READ A$: PRINT A$
60 NEXT I
70 DEFINT A-Z
80 FOR R = 1 TO 9
90   READ R$(R), N%(R), S%(R), O%(R), V%(R)
100 NEXT R
110 FOR S = 1 TO 7
120   READ S$(S), P%(S), K$(S)
130 NEXT S
140 FOR V = 1 TO 19
150   READ V$(V)
160 NEXT V

900 DIM W$(16), X$(5), Y$(4), Z$(31)
910 FOR W = 1 TO 16
920   READ W$(W): W$(W) = W$(W) + " "
930 NEXT W
940 FOR X = 1 TO 5: READ X$(X): NEXT X
950 FOR Y = 1 TO 4: READ Y$(Y): NEXT Y
960 FOR Z = 1 TO 31: READ Z$(Z): NEXT Z

1000 PRINT "-----------": READ A$: PRINT A$
1010 R = 1: T = 0: S = 0
1020 COLOR 15: PRINT : L$ = W$(1) + R$(R): GOSUB 9000
1022 PRINT : F = 0
1023 FOR C = 1 TO 7
1024   IF P%(C) = R% THEN L$ = W$(2) + S$(C): GOSUB 9000: F = 1
1025 NEXT C
1026 IF F = 0 THEN L$ = W$(2) + W$(3): GOSUB 9000
1030 F = 0: L$ = W$(4)
1040 IF N%(R) <> 0 THEN L$ = L$ + X$(1) + ", ": F = 1
1050 IF S%(R) <> 0 THEN L$ = L$ + X$(2) + ", ": F = 1
1060 IF V%(R) <> 0 THEN L$ = L$ + X$(3) + ", ": F = 1
1070 IF O%(R) <> 0 THEN L$ = L$ + X$(4) + ", ": F = 1
1080 IF F% = 0 THEN L$ = L$ + X$(5) + "!" ELSE L$ = LEFT$(L$, LEN(L$) - 2)
1090 GOSUB 9000

1100 ' Top of loop, bottom is 5000

1190 IF TM = 40 THEN 20000 ' Dead ?
1200 COLOR 7: PRINT : PRINT : PRINT W$(5); : LINE INPUT "? ", K$
1210 K$ = UCASE$(LTRIM$(RTRIM$(K$))) + " "
1215 IF K$ = "? " THEN GOSUB 6000: GOTO 1200
1220 K1$ = LEFT$(K$, INSTR(K$, " ") - 1)
1230 K2$ = LTRIM$(MID$(K$, LEN(K1$) + 1))
1235 IF INSTR(K$, V$(19)) THEN PRINT "Ok.": COLOR 7: END
1240 F = 0: FOR C = 1 TO 18
1250   IF INSTR(K$, V$(C)) = 1 THEN F = C: K1$ = V$(C): K2$ = LTRIM$(MID$(K$, LEN(K1$) + 1)): EXIT FOR
1260 NEXT C
1265 COLOR 10: PRINT K1$; " "; K2$
1267 COLOR 9
1270 ON F GOSUB 10100, 10200, 10300, 10400, 10500, 10600, 10700, 10800, 10900, 11000, 11050, 11100, 11200, 11300, 11400, 11500, 11600, 11700
1280 IF F = 0 THEN PRINT : PRINT W$(6); LCASE$(K$)

4000 TM = TM + 1' The pleasant passage of time
4100 IF P%(4) = 0 THEN KF = KF + 1 ELSE KF = 0
4110 IF KF = 4 THEN PRINT : FOR Y = 1 TO 4: PRINT Y$(Y): NEXT Y: GOTO 30000

5000 GOTO 1100

6000 REM List verbs
6010 FOR C = 1 TO 19
6020   COLOR 9 + (C AND 1)
6030   PRINT V$(C) + " ";
6040 NEXT C
6050 COLOR 7
6060 PRINT
6070 RETURN

7000 DATA Välkommen till skoladventure !
7010 DATA Du har 40 minuter på dig att ta dig
7020 DATA ut ur skolan innan den galne kemi-
7030 DATA läraren spränger dig !
7040 DATA Vänta ett tag ...

7170 DATA i norra delen av tekniksalen. Det är skrapigt här. Du kan se ett väggurtag och en låst dörr österut., 0, 5, 0, 0
7180 DATA i korridoren vid tekniksalen. Det ligger borrskräp här., 0, 0, 3, 1
7190 DATA i en krok i korridoren., 0, 7, 0, 2
7200 DATA i ett tomt och öde klassrum. Det är helt tomt pa bänkar., 0, 8, 0, 0
7210 DATA i södra delen av tekniksalen. Du ser ett skåp här. En stor låst dörr leder ut., 1, 0, 0, 0
7220 DATA i lärarnas rökrum. Här är kvavt., 0, 0, 7, 0
7230 DATA i en korridor med en dörr västerut. Här finns en massa förvaringsskåp varav ett utan las., 3, 9, 8, 0
7240 DATA i korridorens östra ände., 4, 0, 0, 7
7250 DATA i ett dammigt klassrum. Katedern är till hälften uppbrunnen., 7, 0, 0, 0

7260 DATA en elektrisk borr, 133, BORREN
7270 DATA en liten nyckel, 135, NYCKELN
7280 DATA en stor nykel, 6, NYCKELN
7290 DATA en rostig kniv, 2, KNIVEN
7300 DATA en krokig dyrk, 1, DYRKEN
7310 DATA en halvfull flaska rödvin, 135, VINET
7320 DATA en skrynklig lapp, 0, LAPPEN

7330 DATA GÅ,TA,SLÄPP,TITTA,SAKER,HJÄLP,BORRA,ANSLUT,UNDERSÖK,LÅS,LOCK,ÖPPNA,STÄNG,DRICK,DYRKA,ANVÄND,SKÄR,LETA,SLUT

7400 DATA Du är,Du ser,ingenting speciellt,Du kan gå,Vad vill du göra,Jag kan inte,Ditåt kan du inte gå,Du kan inte se,Du har tagit,Du har släppt,Du bär på,Ingenting,Jag ser ingen,Det är,Det går inte,Du hittar
7410 DATA Norrut,Söderut,Västerut,Österut,Ingenstans

7500 DATA "Oops. Jag snubblade och föll på kniven. Nar jag försöker"
7510 DATA "ta emot mig körde DATA jag den genom magen. Jag skriker"
7520 DATA "på hjälp, wen den en da som kommer är kemiläraren,"
7530 DATA "med ett hånflin sparker mig i ansiktet."

7600 DATA Ouch ! Du ryckte ur sladden !,Tyvärr kan jag inte säga mer än detta:,Har du letat överallt?,Med vad då ?
7601 DATA "Du trycker på knappen, men ingenting händer.",LÅSET,DÖRREN,Du borrar vilt omkring dig.
7602 DATA Du kan gå österut.,Vad skall jag ansluta ?,Var då ?,BORR,Eh... Vad da sade du ?,En styck borr ansluten.
7603 DATA Attans !! Nyckeln gick av och fastnade i låset.,Hurra !!!,Det funkar inte riktigt..
7604 DATA SKÅPET,Vad skall det vara för nytta med det ??,Du har inget att dricka !!
7605 DATA Yeach ! ! ! Det här lantvinet smakar  arsenik ! !  Det bör jar plötsligt gå
7606 DATA runt for dig. Du tumlar och faller dod ner på golvet ...
7607 DATA Jag har ristat in mina initialer har.
7608 DATA Uuungh??? Hur gör man det ??
7609 DATA Piotsligt störtar skolan in bver dig. Du traffas av en balk i huvudet och
7610 DATA kastas handlöst till golvet. Det sista du ser innan du blir med vetslös ären
7611 DATA röd växande pöl framför dig.
7612 DATA "Det är bara att inse, du är död. Det var trevligt att lär."
7613 DATA Grattis !! Du springer för ditt liv ochhar tagit dig ut ur
7614 DATA skolan levande aftar,minuter.

7700 DATA DU HAR BLIVIT INLÅST I EN SKOLA OCH HAR BARA EN VISS TID PÅ DIG ATT TA DIG UT   INNAN SKOLAN SPRÄNGS. LYCKA TILL !!

8000 DATA Welcome to SchoolAdventure !
8010 DATA You have 40 minutes to get out of
8020 DATA school before the crazy chemistry
8030 DATA teacher blows you up !
8040 DATA Wait a minute ...

8170 DATA in the northern part of the computer lab. It's messy here. You can see an electrical outlet and a locked door to the east., 0, 5, 0, 0
8180 DATA in the hall outside the computer lab. There's a pile of sawdust here., 0, 0, 3, 1
8190 DATA in the corner of the hall., 0, 7, 0, 2
8200 DATA in deserted classroom. The benches are completely empty., 0, 8, 0, 0
8210 DATA in the southern part of the computer lab. You see a cupboard here. There is an exit door but it is locked., 1, 0, 0, 0
8220 DATA in the teacher's lounge. It's stuffy here., 0, 0, 7, 0
8230 DATA in a hall with a door to the west. There are a bunch of lockers here. One of them is open., 3, 9, 8, 0
8240 DATA at the eastern end of the hall., 4, 0, 0, 7
8250 DATA in a dustry classroom. Half the teacher's desk is completely burned away., 7, 0, 0, 0

8260 DATA an electric drill, 133, DRILL
8270 DATA a small key, 135, KEY
8280 DATA a large key, 6, KEY
8290 DATA a rusty knife, 2, KNIFE
8300 DATA a crooked thorn, 2, THORN
8310 DATA a half-empty bottle of red wine, 135, WINE
8320 DATA a crumpled note, 0, NOTE

8330 DATA GO,TAKE,DROP,LOOK,THING,HELP,DRILL,PLUG IN,EXAMINE,UNLOCK,LOCK,OPEN,CLOSE,DRINK,PICK,USE,CUT,SEARCH,END

8400 DATA You are,You see,nothing special,You can go,What do you want to do,I can't,You can't go there,You can't see,You've taken,You've dropped,You are carrying,Nothing,I don't see a,It's,That's not possible,You find
8410 DATA North,South,West,East,Nowhere

8500 DATA "Oops. You trip and fall on the knife. Flailing around as you fall, you"
8510 DATA "somehow manage to drive it into your stomach. You cry out for help, but the"
8520 DATA "only person who hears your cries is the crazy chemistry teacher. With a"
8530 DATA "sneer, he kicks you in the face."

8600 DATA Ouch ! You pulled the plug !,All I can say is:,Have you looked everywhere?,With what ?
8601 DATA You press the button but nothing happens.,KEY,DOOR,You drill wildly all over the place.
8602 DATA You can go east.,What should I plug in ?,Where ?,DRILL,Er... what ?,One drill plugged in.
8603 DATA Oh no !! The key broke off and got stuck in the lock.,Hooray !!!,It doesn't really work...
8604 DATA CABINET,How would that help ??,You have nothing to drink !!
8605 DATA Yech ! ! ! This country wine tastes like arsenic ! !  You start to get
8606 DATA dizzy then tumble to the floor dead ...
8607 DATA You find something to engrave your initials on.
8608 DATA Uuungh??? How do you do that ??
8609 DATA "Without warning, there's a blast and the school collapses around you. A"
8610 DATA "metal beam lands on you, pinning you to the floor. As your vision fades,"
8611 DATA "you see a growing puddle of red."
8612 DATA You are now dead. It was nice getting to know you.
8613 DATA Congratulations !! You managed to escape
8614 DATA school alive in,minutes.

8700 DATA YOU ARE TRAPPED IN A SCHOOL THAT'S ABOUT TO EXPLODE AND HAVE TO ESCAPE BEFORE   TIME RUNS OUT. GOOD LUCK!

9000 REM Word wrap text from L$
9010 C = INSTR(L$, " ")
9020 IF C = 0 THEN C = LEN(L$)
9030 PRINT LEFT$(L$, C);
9040 IF C >= LEN(L$) THEN PRINT : RETURN
9050 L$ = LTRIM$(MID$(L$, C + 1))
9060 GOTO 9010

10000 REM Verbs

10100 K2$ = LEFT$(K2$ + " ", 1)
10110 IF K2$ = "N" AND N%(R) THEN R = N%(R): GOTO 10190
10120 IF K2$ = "S" AND S%(R) THEN R = S%(R): GOTO 10190
10130 IF (K2$ = "V" OR K2$ = "W") AND V%(R) THEN R = V%(R): GOTO 10190
10140 IF (K2$ = "Ö" OR K2$ = "E") AND O%(R) THEN R = O%(R): GOTO 10190
10150 PRINT : PRINT W$(6); "!!": BEEP
10160 GOTO 10195
10190 PRINT : PRINT "Ok."
10191 IF AF = 1 AND P%(1) = 0 THEN AF = 0: PRINT Z$(1): BEEP
10195 RETURN

10200 K2$ = RTRIM$(K2$)
10210 Q = 0: FOR T = 1 TO 7
10220   IF K$(T) = K2$ AND P%(T) = R THEN Q = T
10230 NEXT T
10240 IF Q = 0 THEN PRINT : PRINT W$(8); LCASE$(K2$): BEEP: GOTO 10295
10250 P%(Q) = 0
10260 PRINT : PRINT "Ok. "; W$(9); S$(Q)
10295 RETURN

10300 K2$ = RTRIM$(K2$)
10310 Q = 0: FOR T = 1 TO 7
10320   IF K$(T) = K2$ AND P%(T) = 0 THEN Q = T
10330 NEXT T
10340 IF Q = 0 THEN PRINT : PRINT W$(7); LCASE$(K2$): BEEP: GOTO 10295
10350 P%(Q) = R
10360 PRINT : PRINT "Ok. "; W$(10); S$(Q)
10395 RETURN

10400 PRINT : PRINT Z$(2): PRINT : RETURN 1020 ' Do not advance the clock

10500 PRINT : PRINT W$(11)
10510 Q = 0: FOR T = 1 TO 7
10520   IF P%(T) = 0 THEN PRINT S$(T): Q = 1
10530 NEXT T
10540 IF Q = 0 THEN PRINT W$(12); "!"
10550 RETURN 1100 ' Do not advance the clock

10600 PRINT : PRINT Z$(3); "?": RETURN

10700 IF P%(1) <> 0 THEN PRINT : PRINT Z$(4): BEEP: GOTO 10795
10710 IF AF = 0 THEN PRINT : PRINT Z$(5): BEEP: GOTO 10795
10720 IF R <> 1 OR ((INSTR(K2$, Z$(6)) = 0) AND (INSTR(K2$, Z$(7)) <> 0)) THEN PRINT Z$(8): GOTO 10795
10730 O%(1) = 2: PRINT "Ok. "; Z$(9)
10795 RETURN

10800 IF P%(1) <> 0 THEN PRINT : PRINT Z$(10): BEEP: GOTO 10895
10810 IF R <> 1 THEN PRINT : PRINT Z$(11): BEEP: GOTO 10895
10820 IF INSTR(K2$, Z$(12)) = 0 THEN PRINT : PRINT Z$(13): BEEP: GOTO 10895
10830 PRINT : PRINT "Ok. "; Z$(14): AF = 1
10895 RETURN

10900 Q = 0: FOR T = 1 TO 7
10910   IF Q = 0 THEN Q = INSTR(K2$, K$(T)): IF Q = 1 THEN Q = T
10920 NEXT T
10930 IF Q = 0 THEN 11700 ' Search instead
10940 PRINT : IF P%(Q) <> 0 AND P%(Q) <> R THEN PRINT W$(13); LCASE$(K2$): BEEP: GOTO 10995
10950 PRINT W$(14); S$(Q)
10995 RETURN

11000 IF P%(2) <> 0 AND P%(3) <> 0 AND P%(5) <> 0 THEN PRINT : PRINT Z$(4): BEEP: GOTO 11095
11005 IF RTRIM$(K2$) <> "UPP" THEN 11050
11010 IF P%(2) = 0 AND R = 7 THEN PRINT "Ok.": V%(7) = 6 ELSE GOTO 11020
11012 PRINT Z$(15): P%(2) = 20
11020 IF P%(3) = 0 AND R = 5 THEN PRINT Z$(16): GOTO 50000
11030 PRINT : BEEP: PRINT Z$(17): GOTO 11095
11050 PRINT : BEEP: PRINT W$(15): GOTO 11095 ' LOCK
11095 RETURN

11100 IF INSTR(K2$, Z$(18)) AND (R = 1 OR R = 5) THEN 11700 ' Search
11110 IF P%(2) <> 0 AND P%(3) <> 0 AND P%(5) <> 0 THEN PRINT : PRINT Z$(4): BEEP: GOTO 11095
11115 GOTO 11010 ' LÅS UPP

11200 PRINT : PRINT Z$(19)
11210 RETURN

11300 IF P%(6) <> 0 THEN PRINT : PRINT Z$(20): BEEP: RETURN
11310 PRINT : PRINT Z$(21): PRINT Z$(22): GOTO 30000

11400 GOTO 11100 ' DRINK = OPEN

11500 IF INSTR(K2$, K$(5)) OR INSTR(K2$, K$(2)) THEN 11100
11510 IF INSTR(K2$, K$(1)) THEN 10700 ' DRILL
11520 IF INSTR(K2$, K$(4)) AND P%(4) = 0 THEN PRINT : PRINT "Ok. "; Z$(23): GOTO 11595
11530 PRINT Z$(24)
11595 RETURN

11600 GOTO 11520 ' Use the knife

11700 FOR T = 1 TO 7
11710   IF (P%(T) AND 127) = R THEN P(T) = R: PRINT W$(16); S$(T)
11720 NEXT T
11730 RETURN

20000 SOUND 100, 1: SOUND 140, 1
20010 FOR F = 12 TO 1 STEP -1
20020   SOUND 2 ^ (F / 9) * 100, 1
20030 NEXT F
20040 PRINT "-----": COLOR 12
20050 FOR Z = 25 TO 27
20060   PRINT Z$(Z)
20070 NEXT Z
20080 PRINT

30000 COLOR 12
30010 PRINT Z$(28); " :-)"
30020 COLOR 7
30030 END

50000 FOR T = 1 TO 5
50010   PRINT
50020   FOR D = 1 TO 5
50030     E! = TIMER: WHILE TIMER = E!: WEND
50040   NEXT D
50050 NEXT T
50060 FOR D = 1 TO 11
50070   E! = TIMER: WHILE TIMER = E!: WEND
50080 NEXT D
50090 PRINT "---------------------------------------------"
50100 COLOR 14
50110 PRINT Z$(29)
50120 PRINT Z$(30); TM; Z$(31)
50130 COLOR 7
50140 END

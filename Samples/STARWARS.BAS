CLS
PRINT
COLOR 15
PRINT "   Star Wars: The Game"
COLOR 7
PRINT "  Star wars: The Game "; : COLOR 15: PRINT "     Programmed by: Jonathan Gilbert"
COLOR 8
PRINT " Star wars: The Game  "; : COLOR 7: PRINT "      Programmed by: Jonathan Gilbert"
COLOR 8
PRINT "                             Programmed by: Jonathan Gilbert"
PRINT
PRINT
COLOR 1
PRINT "P   r   e   s   s       a   n   y     k   e   y    t   o       s   t   a   r   t"
COLOR 9
PRINT "        P  r  e  s  s     a  n  y    k  e  y    t  o   s  t  a  r  t"
COLOR 11
PRINT "                P r e s s   a n y   k e y   t o   s t a r t"
COLOR 15
PRINT "                         Press any key to start"
DO
 READ a$
 IF a$ = "over" THEN RESTORE music: READ a$
 IF a$ = "nline" THEN c% = 0: l% = l% + 1: READ a$
 c% = c% + 1
 PLAY a$
LOOP UNTIL INKEY$ <> ""
RESTORE level1
CLS
TYPE tietype
 a AS INTEGER
 x AS INTEGER
 y AS INTEGER
 m AS INTEGER
 l AS INTEGER
 r AS INTEGER
END TYPE
TYPE sdtype
 a AS INTEGER
 x AS INTEGER
 y AS INTEGER
 m AS INTEGER
 s AS INTEGER
END TYPE
TYPE shottype
 a AS INTEGER
 x AS INTEGER
 y AS INTEGER
END TYPE
DIM backscreen(20) AS STRING
DIM tiefighters(15) AS tietype
DIM stardestroyers(15) AS sdtype
DIM xwingshot(15) AS shottype
DIM tieshot(15) AS shottype
DIM sdshot(15) AS shottype
nextlevel:
COLOR 15
CLS
FOR j% = 1 TO 20
 FOR i% = 1 TO 80
  IF INT(RND * 15 - 1) = 0 THEN backscreen(j%) = backscreen(j%) + "ù" ELSE backscreen(j%) = backscreen(j%) + " "
 NEXT i%
NEXT j%
x% = 40
t% = 200
nx% = -1
df% = 15
tf% = 0
sd% = 0
total% = 0
s% = 25
DO
 st# = TIMER
 k$ = INKEY$
 k% = INP(96)
 FOR i% = 19 TO 1 STEP -1
  backscreen(i% + 1) = backscreen(i%)
 NEXT i%
 backscreen(1) = ""
 FOR i% = 1 TO 80
  IF INT(RND * 30 - 1) = 0 THEN backscreen(1) = backscreen(1) + "ù" ELSE backscreen(1) = backscreen(1) + " "
 NEXT i%
 IF c% = 0 THEN
  c% = 1
  IF tf% + sd% <> 0 THEN
   FOR i% = 1 TO tf%
    tiefighters(i%).y = tiefighters(i%).y + 1
    IF tiefighters(i%).l + tiefighters(i%).r = 0 THEN
     SELECT CASE tiefighters(i%).m
      CASE 2:
       SELECT CASE tiefighters(i%).y
        CASE 9 TO 20: tiefighters(i%).x = tiefighters(i%).x + 1
       END SELECT
      CASE 3
       SELECT CASE tiefighters(i%).y
        CASE 9 TO 20: tiefighters(i%).x = tiefighters(i%).x - 1
       END SELECT
     END SELECT
    ELSE
     IF tiefighters(i%).l = 1 THEN tiefighters(i%).x = tiefighters(i%).x - 1
     IF tiefighters(i%).r = 1 THEN tiefighters(i%).x = tiefighters(i%).x + 1
     IF tiefighters(i%).l + tiefighters(i%).r = 2 THEN tiefighters(i%).y = tiefighters(i%).y - 2
    END IF
    IF ((tiefighters(i%).y = 0) OR (tiefighters(i%).y = 20)) OR (tiefighters(i%).x < 2) OR (tiefighters(i%).x > 78) THEN tiefighters(i%).a = 0
   NEXT i%
   FOR i% = 1 TO sd%
    stardestroyers(i%).y = stardestroyers(i%).y + 1
    SELECT CASE stardestroyers(i%).m
     CASE 2:
      SELECT CASE stardestroyers(i%).y
       CASE 9 TO 20: stardestroyers(i%).x = stardestroyers(i%).x + 1
      END SELECT
     CASE 3
      SELECT CASE stardestroyers(i%).y
       CASE 9 TO 20: stardestroyers(i%).x = stardestroyers(i%).x - 1
      END SELECT
    END SELECT
    IF ((stardestroyers(i%).y = 0) OR (stardestroyers(i%).y = 28)) OR (stardestroyers(i%).x < 2) OR (stardestroyers(i%).x > 78) THEN stardestroyers(i%).a = 0
   NEXT i%
  END IF
 ELSEIF c% >= 1 THEN
  c% = (c% + 1) MOD df%
 END IF
 garbage$ = INKEY$
 a$ = ""
 FOR i% = 1 TO 20
  a$ = a$ + backscreen(i%)
 NEXT i%
 FOR i% = 1 TO tf%
  IF tiefighters(i%).a <> 0 THEN
   location% = tiefighters(i%).x + (tiefighters(i%).y - 1) * 80
   s$ = "Ãé´"
   IF tiefighters(i%).l = 1 THEN location% = location% + 1: s$ = "é´"
   IF tiefighters(i%).r = 1 THEN s$ = LEFT$(s$, LEN(s$) - 1)
   MID$(a$, location%, LEN(s$)) = s$
  END IF
 NEXT i%
 FOR i% = 1 TO sd%
  IF stardestroyers(i%).a <> 0 THEN
   FOR j% = stardestroyers(i%).y - 8 TO stardestroyers(i%).y
    IF (j% > 0) AND (j% < 21) THEN
     beepbeep% = (stardestroyers(i%).y - 8) - j%
     location% = (stardestroyers(i%).x - 1) + (j%) * 80 - 4
     IF location% < 1600 THEN
      SELECT CASE beepbeep%
       CASE 0: MID$(a$, location%, 8) = "²²²²²²²²"
       CASE -1: MID$(a$, location%, 7) = "²²²²²²²"
       CASE -2: MID$(a$, location% + 1, 6) = "²²²²²²"
       CASE -3: MID$(a$, location% + 1, 6) = "²²²²²"
       CASE -4: MID$(a$, location% + 2, 6) = "²²²²"
       CASE -5: MID$(a$, location% + 2, 6) = "²²²"
       CASE -6: MID$(a$, location% + 3, 6) = "²²"
       CASE IS <= -7: MID$(a$, location% + 3, 6) = "²"
      END SELECT
     END IF
    END IF
   NEXT j%
  END IF
 NEXT i%
 SELECT CASE k%
  CASE 75, 51, 30: x% = x% - 1: nx% = -1: c2% = df% * 5 - 1: IF x% = 3 THEN x% = 4: nx% = 0
  CASE 77, 52, 32: x% = x% + 1: nx% = -1: c2% = df% * 5 - 1: IF x% = 77 THEN x% = 76: nx% = 0
  CASE 72, 80, 76, 57, 31, 17, 53, 33, 50:
   numxwingshots% = numxwingshots% + 1
   IF numxwingshots% = 16 THEN
    numxwingshots% = 15
    farthest% = 200
    FOR i% = 0 TO 15
     IF xwingshot(i%).y < farthest% THEN farthest% = xwingshot(i%).y: replace% = i%
    NEXT i%
    xwingshot(replace%).a = 0
   ELSE
    FOR i% = 0 TO 15
     IF xwingshot(i%).a = 0 THEN replace% = i%: EXIT FOR
    NEXT i%
   END IF
   xwingshot(replace%).y = 20
   xwingshot(replace%).x = x%
   xwingshot(replace%).a = -1
  CASE 1: GOTO gameover
 END SELECT
 IF numxwingshots% <> 0 THEN
  FOR i% = 0 TO 15
   IF xwingshot(i%).a <> 0 THEN
    xwingshot(i%).y = xwingshot(i%).y - 1
    IF xwingshot(i%).y = 0 THEN
     xwingshot(i%).a = 0
    ELSE
     location% = xwingshot(i%).x + (xwingshot(i%).y - 1) * 80
     tx% = xwingshot(i%).x - 1
     ty% = xwingshot(i%).y
     FOR j% = 1 TO tf%
      IF tiefighters(j%).a <> 0 THEN
       IF tiefighters(j%).y = ty% THEN
        IF ABS(tiefighters(j%).x - tx%) <= 1 THEN
         IF tiefighters(j%).x - tx% = 1 THEN IF tiefighters(j%).l = 0 THEN tiefighters(j%).l = 1: p% = p% + 50
         IF tiefighters(j%).x - tx% = 0 THEN tiefighters(j%).a = 0: p% = p% + 100: destroyed% = destroyed% + 1
         IF tiefighters(j%).x - tx% = -1 THEN IF tiefighters(j%).r = 0 THEN tiefighters(j%).r = 1: p% = p% + 50
         xwingshot(i%).a = 0
        END IF
       END IF
      END IF
     NEXT j%
     FOR j% = 1 TO sd%
      IF stardestroyers(j%).a <> 0 THEN
       IF (stardestroyers(j%).y - ty% < 8) AND (stardestroyers(j%).y - ty% > 0) THEN
        IF (stardestroyers(j%).x - tx% < 8) AND (stardestroyers(j%).y - ty% > 0) THEN
         IF MID$(a$, tx% + (ty% - 1) * 80, 1) = "²" THEN
          stardestroyers(j%).s = stardestroyers(j%).s - 1: p% = p% + 30
          IF stardestroyers(j%).s = 0 THEN stardestroyers(j%).a = 0: destroyed% = destroyed% + 1
          xwingshot(i%).a = 0
         END IF
        END IF
       END IF
      END IF
     NEXT j%
     MID$(a$, location%, 1) = "^"
    END IF
   END IF
  NEXT i%
 END IF
 LOCATE 1, 1
 PRINT a$;
 IF nx% THEN
  LOCATE 21, 1
  PRINT SPACE$(x% - 1); "³"; SPACE$(ABS(80 - POS(0)));
  LOCATE 22, 1
  PRINT SPACE$(x% - 3); "Æðºðµ"; SPACE$(ABS(80 - POS(0)));
 END IF
 IF c3% = 0 THEN c3% = 1
 IF c2% = 0 THEN
  c2% = 1
  c3% = 1
  FOR i% = 1 TO tf%
   IF tiefighters(i%).a <> 0 THEN
    IF ABS(tiefighters(i%).x - x%) <= 1 THEN
     numtfshots% = numtfshots% + 1
     IF numtfshots% = 16 THEN
      numtfshots% = 15
      farthest% = 200
      FOR j% = 0 TO 15
       IF tieshot(j%).y < farthest% THEN farthest% = tieshot(j%).y: replace% = j%
      NEXT j%
      tieshot(replace%).a = 0
     ELSE
      FOR j% = 0 TO 15
       IF tieshot(j%).a = 0 THEN replace% = j%: EXIT FOR
      NEXT j%
     END IF
     tieshot(replace%).y = tiefighters(i%).y
     tieshot(replace%).x = tiefighters(i%).x + 1
     tieshot(replace%).a = -1
    END IF
   END IF
  NEXT i%
  FOR i% = 1 TO sd%
   IF stardestroyers(i%).a <> 0 THEN
    IF ABS(stardestroyers(i%).x - x%) <= 1 THEN
     numsdshots% = numsdshots% + 1
     IF numsdshots% = 16 THEN
      numsdshots% = 15
      farthest% = 200
      FOR j% = 0 TO 15
       IF sdshot(j%).y < farthest% THEN farthest% = sdshot(j%).y: replace% = j%
      NEXT j%
      sdshot(replace%).a = 0
     ELSE
      FOR j% = 0 TO 15
       IF sdshot(j%).a = 0 THEN replace% = j%: EXIT FOR
      NEXT j%
     END IF
     sdshot(replace%).y = stardestroyers(i%).y
     sdshot(replace%).x = x%
     sdshot(replace%).a = -1
    END IF
   END IF
  NEXT i%
 ELSE
  c2% = (c2% + 1) MOD (df% * 5)
  c3% = (c3% + 1) MOD (df% * 1)
 END IF
 FOR i% = 0 TO 15
  IF (c3% MOD (df% - 2)) = 0 THEN
   IF tieshot(i%).a <> 0 THEN
    tieshot(i%).y = tieshot(i%).y + 1
    IF tieshot(i%).y = 23 THEN tieshot(i%).a = 0
    IF tieshot(i%).y = 21 AND tieshot(i%).x = x% THEN
     s% = s% - 3
     IF s% <= 0 THEN GOTO gameover
     tieshot(i%).a = 0
    ELSEIF tieshot(i%).y = 22 AND ABS(tieshot(i%).x - x%) < 2 THEN
     s% = s% - 1
     IF s% = 0 THEN GOTO gameover
     tieshot(i%).a = 0
    END IF
   END IF
  END IF
  IF tieshot(i%).a <> 0 THEN
   LOCATE tieshot(i%).y, tieshot(i%).x
   PRINT "º";
  END IF
 NEXT i%
 FOR i% = 0 TO 15
  IF (c3% MOD (df% - 2)) = 0 THEN
   IF sdshot(i%).a <> 0 THEN
    sdshot(i%).y = sdshot(i%).y + 1
    IF sdshot(i%).y = 23 THEN sdshot(i%).a = 0
    IF sdshot(i%).y = 21 AND sdshot(i%).x = x% THEN
     s% = s% - 5
     IF s% <= 0 THEN GOTO gameover
     sdshot(i%).a = 0
    ELSEIF sdshot(i%).y = 21 AND ABS(sdshot(i%).x - x%) <= 2 THEN
     s% = s% - 3
     IF s% <= 0 THEN GOTO gameover
     sdshot(i%).a = 0
    ELSEIF sdshot(i%).y = 22 AND ABS(sdshot(i%).x - x%) <= 4 THEN
     s% = s% - 1
     IF s% = 0 THEN GOTO gameover
     sdshot(i%).a = 0
    END IF
   END IF
  END IF
  IF sdshot(i%).a <> 0 THEN
   LOCATE sdshot(i%).y, sdshot(i%).x - 1
   PRINT "ÀÄÙ";
  END IF
 NEXT i%
 t% = t% + 1
 IF t% = df% * 20 THEN
  t% = 0
  FOR i% = 1 TO tf%
   tiefighters(i%).a = 0
  NEXT i%
  FOR i% = 1 TO sd%
   stardestroyers(i%).a = 0
  NEXT i%
  tf% = 0: sd% = 0
  DO
   READ typ%
   SELECT CASE typ%
    CASE 0: EXIT DO
    CASE 1: tf% = tf% + 1: READ tiefighters(tf%).x, tiefighters(tf%).y, tiefighters(tf%).m: tiefighters(tf%).a = 1: tiefighters(tf%).l = 0: tiefighters(tf%).r = 0
    CASE 2: sd% = sd% + 1: READ stardestroyers(sd%).x, stardestroyers(sd%).y, stardestroyers(sd%).m: stardestroyers(sd%).a = 1: stardestroyers(sd%).s = 10
    CASE 3: READ df%
    CASE 4: fl% = -1: EXIT DO
    CASE 5: GOTO gamewon
   END SELECT
  LOOP
  total% = total% + tf% + sd%
  IF fl% = -1 THEN EXIT DO
 END IF
 LOCATE 24, 1
 PRINT USING " Points: ###,###  Shield: ##"; p%; s%;
 WHILE ABS(TIMER - st#) < .01: WEND
LOOP
fl% = 0
CLS
WHILE INKEY$ <> "": WEND
PRINT
PRINT "                Level"; level%; "completed."
PRINT
PRINT
PRINT "                 Perfect bonus: ";
IF destroyed% = total% THEN PRINT "3,000": p% = p% + 3000 ELSE PRINT "    0"
PRINT
PRINT "                         Score: "; p%
PRINT
PRINT
PRINT
PRINT "              Press any key for level"; level% + 1
level% = level% + 1
SLEEP: WHILE INKEY$ <> "": WEND
FOR i% = 1 TO 20
 backscreen(i%) = ""
NEXT i%
FOR i% = 1 TO 15
 xwingshot(i%).a = 0
 tieshot(i%).a = 0
 sdshot(i%).a = 0
NEXT i%
numxwingshots% = 0
numtfshots% = 0
numsdshots% = 0
GOTO nextlevel
gameover:
CLS
PRINT
PRINT
PRINT "You lose... :( But we'll put your score into the history list."
GOTO savetofile
gamewon:
CLS
PRINT
PRINT
PRINT "     You win! You win! You WIN! You have now formally beaten the game."
savetofile:
LINE INPUT "     How do you wish to identify yourself? ", a$
PRINT "     Writing score to file...";
ON ERROR GOTO wrong
OPEN "starwars.sco" FOR APPEND AS #1
ON ERROR GOTO 0
IF dontwrite% = 0 THEN
 PRINT #1, "On "; DATE$; ", "; a$; " scored"; p%; "points."
 CLOSE #1
 PRINT "appended to STARWARS.SCO."
END IF
PRINT
PRINT "         Do you wish to play again? ";
DO
a$ = UCASE$(INPUT$(1))
LOOP UNTIL INSTR("YN", a$)
PRINT a$
IF a$ = "Y" THEN RUN
PRINT
PRINT "Hope you enjoyed the ride!"
COLOR 7, 0
END

wrong:
SELECT CASE ERR
 CASE 75:
  PRINT "Error! Score file appears to be read-only!"
  dontwrite% = -1
  RESUME NEXT
 CASE ELSE: RESUME NEXT
END SELECT

DATA over
level1:
DATA 1,20,1,1,1,60,1,1,3,4,0
DATA 0
DATA 0
DATA 1,20,1,1,1,40,1,1,1,60,1,1,0
DATA 0
DATA 0
DATA 1,30,1,2,0
DATA 0
DATA 1,50,1,3,0
DATA 0
DATA 0
DATA 1,5,1,2,1,75,1,3,0
DATA 1,15,1,2,1,65,1,3,0
DATA 1,20,1,1,1,60,1,1,0
DATA 1,23,1,1,1,57,1,1,0
DATA 1,30,1,1,2,40,1,1,1,50,1,1,3,6,0

DATA 4

DATA 1,5,1,2,1,75,1,3,1,30,1,3,1,50,1,2,0
DATA 1,15,1,2,1,65,1,3,1,20,1,3,1,30,1,2,1,35,1,3,1,45,1,2,1,50,1,3,1,60,1,2,1,40,1,1,0
DATA 0
DATA 2,15,1,2,2,65,1,3,1,30,1,2,1,40,1,1,1,50,1,2,0
DATA 0
DATA 0

DATA 5

music:
DATA t120,o1,ms,l4,c,<,b,a,l12,g,a,b,>,l4,c,<,b,a,l12,g,a,ml,l24,b,>,l12,g,nline
DATA l12,c,l6,>,c,<,<,l12,b,l6,>,>,c,<,<,l12,a,l6,>,>,g,<,<,l18,g,>,>,f,<,<,nline
DATA a,>,>,e,<,<,b,>,>,d,<,l12,c,>,>,l6,c,<,<,<,l12,b,>,>,>,l6,c,<,<,<,l12,nline
DATA a,>,>,l6,g,<,<,l18,g,>,>,f,<,<,a,>,>,e,<,<,b,>,>,d,<,l12,c,>,>,l6,c,<,nline
DATA <,<,l12,b,>,>,>,l6,c,<,<,<,l12,a,>,>,l6,g,<,<,l18,a,>,>,f,<,<,b,>,>,e,nline
DATA <,c,>,f,l6,<,<,l12,b,>,>,l6,d,<,<,l12,a,>,>,l6,d,<,<,l12,g,>,l4,g,p32,nline
DATA l64,g,f,e,d,c,<,b,a,l12,g,l12,c,l6,>,c,<,<,l12,b,l6,>,>,c,<,<,l12,a,l6,nline
DATA >,>,g,<,<,l18,g,>,>,f,<,<,a,>,>,e,<,<,b,>,>,d,<,l12,c,>,>,l6,c,<,<,<,nline
DATA l12,b,>,>,>,l6,c,<,<,<,l12,a,>,>,l6,g,<,<,l18,g,>,>,f,<,<,a,>,>,e,<,<,nline
DATA b,>,>,d,<,l12,c,>,>,l6,c,<,<,<,l12,b,>,>,>,l6,c,<,<,<,l12,a,>,>,l6,g,<,nline
DATA <,l18,a,>,>,f,<,<,b,>,>,e,<,c,>,f,l6,<,<,l12,b,>,>,l6,d,<,<,l12,a,>,>,nline
DATA l6,d,<,<,l12,g,>,l4,g,p32,l64,g,f,e,d,c,<,b,a,l12,g,mn,t140,l3,g,l6,a,nline
DATA p12,>,f,p12,f,p12,<,l6,a,l64,a,l6,>,f,ml,e,d,c,<,l32,g,>,l3,c,l6,d,e,nline
DATA l3,d,<,g,l32,g,l6,a,l12,>,f,p12,f,p12,<,l6,a,l64,a,l6,>,f,ml,e,d,c,<,nline
DATA l32,g,>,l3,g,<,l6,p32,l6,g,p12,l32,g,p32,l6,g,t140,mn,l3,g,l6,a,l12,>,nline
DATA f,p12,f,p12,<,l6,a,l64,a,l6,>,f,ml,e,d,c,<,l32,g,>,l3,c,l6,d,e,l3,d,g,nline
DATA p32,l32,g,>,l6,c,<,b-,a-,g,f,e-,d,c,l3,g,<,p32,l6,g,p12,l32,g,p32,l6,g,nline
DATA t120,o1,ms,l12,g,a,ml,l24,b,>,l12,g,l12,c,l6,>,c,<,<,l12,b,l6,>,>,c,<,nline
DATA <,l12,a,l6,>,>,g,<,<,l18,g,>,>,f,<,<,a,>,>,e,<,<,b,>,>,d,<,l12,c,>,>,nline
DATA l6,c,<,<,<,l12,b,>,>,>,l6,c,<,<,<,l12,a,>,>,l6,g,<,<,l18,g,>,>,f,<,<,nline
DATA a,>,>,e,<,<,b,>,>,d,<,l12,c,>,>,l6c,<,<,<,l12,b,>,>,>,l6,c,<,<,<,l12,a,nline
DATA >,>,l6,g,<,<,<,l18,g,>,>,g,<,<,a,>,>,g,<,b,>,g,l12,c,>,l6,c,<,l18,c,p18,nline
DATA c,p18,c,p18,l1,c,p1,p1,over


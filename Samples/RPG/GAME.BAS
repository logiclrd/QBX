DECLARE SUB echo (a$)
DECLARE SUB readgif (a$, pal() AS ANY)
DECLARE SUB loadmap (file$, map%(), playerx%, playery%, mapx%, mapy%, resource$())
DECLARE SUB processkey (a$, playerx%, playery%, map%(), herodir%, mapx%, mapy%, resource$())
DECLARE FUNCTION resources$ (x%, y%, resource$())
DECLARE SUB fadein (pal() AS ANY)
DECLARE SUB fadeout (pal() AS ANY)
DECLARE SUB loadpal (pal() AS ANY)
DECLARE SUB fadout ()
DECLARE SUB intro ()
DECLARE FUNCTION readfile% (a$, pal() AS ANY)
DECLARE SUB drawmap (x1%, y1%, x2%, y2%, xo%, yo%, map%(), playerx%, playery%, herodir%)
DECLARE SUB setuppalette ()
DECLARE SUB hidepalette ()
DECLARE SUB drawmain ()
DECLARE SUB expand (t$, x%, y%)
DECLARE SUB playmap (map$)
DECLARE SUB loaddata ()
DECLARE SUB putongrass (s%)
DECLARE SUB monstercycle ()
TYPE rgbtype
 r AS INTEGER
 g AS INTEGER
 b AS INTEGER
END TYPE
DIM SHARED crclsize%(16), selectit%(1505), unselectit%(1505)
DIM SHARED sprite%(52, 200)
DIM pal(255) AS rgbtype
SCREEN 13
loaddata
CLS
intro
dr% = 3
choice% = 1
DO
 hidepalette
 LINE (0, 0)-(149, 19), 8, BF: GET (0, 0)-(149, 19), unselectit%
 LINE (0, 0)-(149, 19), 7, BF: GET (0, 0)-(149, 19), selectit%
 CLS
 drawmain
 setuppalette
 DO
  FOR i% = 1 TO 16
   crclsize%(i%) = crclsize%(i%) - 1
   IF crclsize%(i%) = 0 THEN crclsize%(i%) = 16
   x% = SIN(6.283185 * i% / 16) * 15 + 50
   y% = COS(6.283185 * i% / 16) * 15 + 50
   CIRCLE (x%, y%), crclsize%(i%), crclsize%(i%) + 15
   PAINT (x%, y%), crclsize%(i%) + 15, crclsize%(i%) + 15
   CIRCLE (320 - x%, y%), crclsize%(i%), crclsize%(i%) + 15
   PAINT (320 - x%, y%), crclsize%(i%) + 15, crclsize%(i%) + 15
  NEXT i%
  OUT &H3C8, 15
  OUT &H3C9, r%
  OUT &H3C9, 0
  OUT &H3C9, 0
  r% = r% + dr%
  IF r% = 63 THEN dr% = -3
  IF r% = 0 THEN dr% = 3
  a$ = INKEY$
  SELECT CASE a$
   CASE CHR$(0) + "H":
    PUT (85, choice% * 20 + 60), unselectit%, AND
    choice% = choice% - 1: IF choice% = 0 THEN choice% = 5
    PUT (85, choice% * 20 + 60), selectit%, XOR
   CASE CHR$(0) + "P":
    PUT (85, choice% * 20 + 60), unselectit%, AND
    choice% = choice% + 1: IF choice% = 6 THEN choice% = 1
    PUT (85, choice% * 20 + 60), selectit%, XOR
  END SELECT
 LOOP UNTIL a$ = CHR$(13)
 fadout
 CLS
 PALETTE
 SELECT CASE choice%
  CASE 1:
   OUT &H3C7, 0: OUT &H3C8, 0: FOR i% = 0 TO 767: OUT &H3C9, 0: NEXT i%
   DEF SEG = &HA000
   BLOAD "playbg.bsv", 0
   OPEN "playbg.pal" FOR BINARY AS #1
   FOR i% = 0 TO 255
    pal(i%).r = ASC(INPUT$(1, 1))
    pal(i%).g = ASC(INPUT$(1, 1))
    pal(i%).b = ASC(INPUT$(1, 1))
   NEXT i%
   CLOSE #1
   fadein pal()
   playmap "game.map"
  CASE 2:
  CASE 3:
  CASE 4:
  CASE 5: EXIT DO
 END SELECT
LOOP
SCREEN 0: WIDTH 80

SUB dead
DIM pal(255) AS rgbtype, whitemsk%(482), reverse%(482)
FOR i% = 0 TO 255
 OUT &H3C8, i%: OUT &H3C9, 0: OUT &H3C9, 0: OUT &H3C9, 0
NEXT i%
LOCATE 1, 1
COLOR 255
PRINT "This.. is you.."
GET (0, 0)-(119, 7), whitemsk%
LOCATE 1, 1
COLOR 251
PRINT "This.. is you.."
GET (0, 0)-(119, 7), reverse%
a% = readfile%("dead.acg", pal())
pal(4).r = 0: pal(4).g = 0: pal(4).b = 0
FOR i% = 1 TO 63
 FOR j% = 0 TO 255
  OUT &H3C8, j%
  OUT &H3C9, pal(j%).r * i% \ 63
  OUT &H3C9, pal(j%).g * i% \ 63
  OUT &H3C9, pal(j%).b * i% \ 63
 NEXT j%
NEXT i%
st# = TIMER: WHILE ABS(TIMER - st#) < 1: WEND
PUT (120, 150), whitemsk%, OR
PUT (120, 150), reverse%, XOR
FOR i% = 0 TO 63
 st# = TIMER
 OUT &H3C8, 4
 OUT &H3C9, i%
 OUT &H3C9, i%
 OUT &H3C9, i%
 WHILE ABS(TIMER - st#) < .01: WEND
NEXT i%
st# = TIMER: WHILE ABS(TIMER - st#) < .5: WEND
SLEEP: WHILE INKEY$ <> "": WEND
pal(4).r = 63: pal(4).g = 63: pal(4).b = 63
FOR i% = 62 TO 0 STEP -1
 FOR j% = 0 TO 255
  OUT &H3C8, j%
  OUT &H3C9, pal(j%).r * i% \ 63
  OUT &H3C9, pal(j%).g * i% \ 63
  OUT &H3C9, pal(j%).b * i% \ 63
 NEXT j%
NEXT i%
CLS
PALETTE
END SUB

SUB drawmain
SHARED choice%
COLOR 31
expand "Arena", 120, 42
COLOR 8
IF choice% = 1 THEN COLOR 15
expand "New Game", 96, 80
IF choice% = 1 THEN COLOR 8
IF choice% = 2 THEN COLOR 15
expand "Help", 128, 100
IF choice% = 2 THEN COLOR 8
IF choice% = 3 THEN COLOR 15
expand "Storyline", 88, 120
IF choice% = 3 THEN COLOR 8
IF choice% = 4 THEN COLOR 15
expand "ACS", 136, 140
IF choice% = 4 THEN COLOR 8
IF choice% = 5 THEN COLOR 15
expand "Quit Game", 88, 160
LINE (0, 0)-(319, 7), 0, BF
END SUB

SUB drawmap (x1%, y1%, x2%, y2%, xo%, yo%, map%(), playerx%, playery%, herodir%)
FOR i% = x1% TO x2%
 FOR j% = y1% TO y2%
  IF (i% = playerx%) AND (j% = playery%) THEN
   SELECT CASE map%(i%, j%)
    CASE 1 TO 6: herooffset% = 12
    CASE 0: herooffset% = 0
    CASE 64: herooffset% = 33
   END SELECT
   SELECT CASE herodir%
    CASE 1: PUT ((i% - x1% + xo%) * 10 - 10, (j% - y1% + yo%) * 10 - 10), sprite%(0, herooffset% + 35), PSET
    CASE 2: PUT ((i% - x1% + xo%) * 10 - 10, (j% - y1% + yo%) * 10 - 10), sprite%(0, herooffset% + 34), PSET
    CASE 3, 5, 6: PUT ((i% - x1% + xo%) * 10 - 10, (j% - y1% + yo%) * 10 - 10), sprite%(0, herooffset% + 32), PSET
    CASE 4, 7, 8: PUT ((i% - x1% + xo%) * 10 - 10, (j% - y1% + yo%) * 10 - 10), sprite%(0, herooffset% + 33), PSET
   END SELECT
  ELSE
   PUT ((i% - x1% + xo%) * 10 - 10, (j% - y1% + yo%) * 10 - 10), sprite%(0, map%(i%, j%)), PSET
  END IF
 NEXT j%
NEXT i%
END SUB

SUB echo (a$)
DIM moveup%(9602)
DO
 GET (8, 28)-(135, 169), moveup%
 PUT (8, 20), moveup%, PSET
 LINE (8, 160)-(135, 169), 0, BF
 b$ = ""
 DO
  oa$ = a$: ob$ = b$
  b$ = b$ + LEFT$(a$, INSTR(a$, " "))
  a$ = MID$(a$, INSTR(a$, " ") + 1)
 LOOP UNTIL LEN(b$) > 16 OR INSTR(a$, " ") = 0
 IF INSTR(a$, " ") = 0 THEN
  IF LEN(a$) + LEN(b$) + 1 < 16 THEN b$ = b$ + a$: a$ = ""
  IF LEN(b$) > 16 THEN
   a$ = " " + a$
   DO
    DO
     a$ = RIGHT$(b$, 1) + a$
     b$ = LEFT$(b$, LEN(b$) - 1)
    LOOP UNTIL RIGHT$(b$, 1) = " "
   LOOP UNTIL LEN(b$) <= 16
  END IF
 ELSE
  a$ = oa$: b$ = ob$
 END IF
 FOR i% = 1 TO 16
  st# = TIMER
  LOCATE 21, i% + 1
  PRINT MID$(b$, i%, 1)
  IF c$ = "" THEN WHILE ABS(TIMER - st#) < ((RND * RND * RND) / 2): WEND
  c$ = c$ + INKEY$
 NEXT i%
LOOP UNTIL a$ = ""
END SUB

SUB expand (t$, x%, y%)
LOCATE 1, 1
PRINT t$
FOR i% = 0 TO LEN(t$) * 8 - 1
 FOR j% = 0 TO 7
  LINE (i% * 2 + x%, j% * 2 + y%)-(i% * 2 + x% + 1, j% * 2 + y% + 1), POINT(i%, j%), BF
 NEXT j%
NEXT i%
END SUB

SUB fadein (pal() AS rgbtype)
FOR i% = 1 TO 63
 FOR j% = 0 TO 255
  OUT &H3C8, j%
  OUT &H3C9, pal(j%).r * i% \ 63
  OUT &H3C9, pal(j%).g * i% \ 63
  OUT &H3C9, pal(j%).b * i% \ 63
 NEXT j%
NEXT i%
END SUB

SUB fadeout (pal() AS rgbtype)
FOR i% = 62 TO 0 STEP -1
 FOR j% = 0 TO 255
  OUT &H3C8, j%
  OUT &H3C9, pal(j%).r * i% \ 63
  OUT &H3C9, pal(j%).g * i% \ 63
  OUT &H3C9, pal(j%).b * i% \ 63
 NEXT j%
NEXT i%
END SUB

SUB fadout
DIM pal(255) AS rgbtype
loadpal pal()
fadeout pal()
END SUB

SUB genmap (map%())
RANDOMIZE TIMER
FOR i% = 1 TO 150
 FOR j% = 1 TO 150
  map%(i%, j%) = 6
 NEXT j%
 LINE (114 + (i% * 28) \ 300, 102)-(114 + (i% * 28) \ 300, 118), 31
NEXT i%
FOR i% = 1 TO 15000
 map%(INT(RND * 150 + 1), INT(RND * 150 + 1)) = 1
 oldp% = p%
 p% = 128 + (i% * 28&) \ 30000
 IF p% <> oldp% THEN LINE (p%, 102)-(p%, 118), 31
NEXT i%
FOR i% = 1 TO 150
 FOR j% = 1 TO 150
  IF map%(i%, j%) = 1 THEN
   IF j% > 1 THEN
    IF j% < 150 THEN
     IF (map%(i%, j% - 1) = 6) AND (map%(i%, j% + 1) <> 6) THEN map%(i%, j%) = 4
     IF (map%(i%, j% + 1) = 6) AND (map%(i%, j% - 1) <> 6) THEN map%(i%, j%) = 5
    END IF
   END IF
   IF i% > 1 THEN
    IF i% < 150 THEN
     IF (map%(i% - 1, j%) = 6) AND (map%(i% + 1, j%) <> 6) THEN map%(i%, j%) = 2
     IF (map%(i% + 1, j%) = 6) AND (map%(i% - 1, j%) <> 6) THEN map%(i%, j%) = 3
    END IF
   END IF
  END IF
 NEXT j%
 LINE (142 + (i% * 28&) \ 150, 102)-(142 + (i% * 28&) \ 150, 118), 31
NEXT i%
END SUB

SUB hidepalette
FOR i% = 0 TO 31
 OUT &H3C8, i%: OUT &H3C9, 0: OUT &H3C9, 0: OUT &H3C9, 0
NEXT i%
END SUB

SUB intro
DIM pal(255) AS rgbtype
FOR i% = 0 TO 255
 OUT &H3C8, i%: OUT &H3C9, 0: OUT &H3C9, 0: OUT &H3C9, 0
NEXT i%
a% = readfile%("arena.acg", pal())
FOR i% = 1 TO 63
 FOR j% = 0 TO 255
  OUT &H3C8, j%
  OUT &H3C9, pal(j%).r * i% \ 63
  OUT &H3C9, pal(j%).g * i% \ 63
  OUT &H3C9, pal(j%).b * i% \ 63
 NEXT j%
NEXT i%
st# = TIMER: WHILE (ABS(TIMER - st#) < 10) AND (INKEY$ = ""): WEND
FOR i% = 62 TO 0 STEP -1
 FOR j% = 0 TO 255
  OUT &H3C8, j%
  OUT &H3C9, pal(j%).r * i% \ 63
  OUT &H3C9, pal(j%).g * i% \ 63
  OUT &H3C9, pal(j%).b * i% \ 63
 NEXT j%
NEXT i%
CLS
PALETTE
END SUB

SUB loaddata
SHARED sprite%()
LOCATE 12, 15
COLOR 31
PRINT "Please wait"
LINE (112, 100)-(200, 120), 31, B
FOR i% = 1 TO 15
 OUT &H3C8, i%: OUT &H3C9, 0: OUT &H3C9, 0: OUT &H3C9, 0
NEXT i%
OPEN "game.spt" FOR BINARY AS #1
numsprites% = LOF(1) \ 100
LINE (0, 0)-(9, 9), 0, BF
GET (0, 0)-(9, 9), sprite%(0, 0)
FOR i% = 1 TO numsprites%
 FOR j% = 1 TO 10
  FOR k% = 1 TO 10
   PSET (k%, j%), ASC(INPUT$(1, 1))
  NEXT k%
 NEXT j%
 GET (1, 1)-(10, 10), sprite%(0, i%)
 LINE (114, 102)-(114 + (i% * 84&) \ numsprites%, 118), 31, BF
NEXT i%
CLOSE #1
putongrass 44
putongrass 45
putongrass 46
putongrass 47
putongrass 48
putongrass 49
putongrass 50
putongrass 51
FOR i% = 62 TO 2 STEP -2
 st# = TIMER
 OUT &H3C8, 31: OUT &H3C9, 63: OUT &H3C9, i%: OUT &H3C9, i%
 IF i% < 15 THEN i% = i% - 1
 IF i% < 0 THEN EXIT FOR
 WHILE ABS(TIMER - st#) < .01: WEND
NEXT i%
FOR i% = 60 TO 0 STEP -2
 st# = TIMER
 OUT &H3C8, 31: OUT &H3C9, i%: OUT &H3C9, 0: OUT &H3C9, 0
 IF i% < 15 THEN i% = i% - 1
 IF i% < 0 THEN EXIT FOR
 WHILE ABS(TIMER - st#) < .01: WEND
NEXT i%
CLS
PALETTE
END SUB

SUB loadmap (file$, map%(), playerx%, playery%, mapx%, mapy%, resource$())
LOCATE 12, 15
COLOR 31
PRINT "Loading map"
LINE (112, 100)-(200, 120), 31, B
LINE (113, 101)-(199, 119), 0, BF
FOR i% = 1 TO 15
 OUT &H3C8, i%: OUT &H3C9, 0: OUT &H3C9, 0: OUT &H3C9, 0
NEXT i%
OPEN file$ FOR BINARY AS #1
FOR i% = 1 TO 150
 a$ = INPUT$(150, 1)
 FOR j% = 1 TO 150
  map%(i%, j%) = ASC(MID$(a$, j%, 1))
 NEXT j%
 LINE (114 + (i% * 84&) \ 150, 102)-(114 + (i% * 84&) \ 150, 118), 31
NEXT i%
playerx% = CVI(INPUT$(2, 1))
playery% = CVI(INPUT$(2, 1))
resource$(0) = LTRIM$(RTRIM$(STR$(CVI(INPUT$(2, 1)))))
FOR i% = 1 TO VAL(resource$(0))
 resource$(i%) = INPUT$(4, 1) + INPUT$(ASC(INPUT$(1, 1)), 1)
NEXT i%
mapx% = playerx% - 5
mapy% = playery% - 5
IF mapx% < 1 THEN mapx% = 1
IF mapy% < 1 THEN mapy% = 1
CLOSE #1
DEF SEG = &HA000
BLOAD "playbg.bsv", 0
OPEN "playbg.pal" FOR BINARY AS #1
OUT &H3C7, 0: OUT &H3C8, 0: FOR i% = 0 TO 767: OUT &H3C9, ASC(INPUT$(1, 1)): NEXT i%
CLOSE #1
END SUB

SUB loadpal (pal() AS rgbtype)
FOR i% = 0 TO 255
 OUT &H3C7, i%
 pal(i%).r = INP(&H3C9)
 pal(i%).g = INP(&H3C9)
 pal(i%).b = INP(&H3C9)
NEXT i%
END SUB

SUB monstercycle
END SUB

SUB playmap (map$)
SHARED abort%, level%
level% = level% + 1
DIM map%(151, 151), resource$(250), saveecho%(9602)
GET (8, 20)-(135, 169), saveecho%
DEF SEG = &HA000: BLOAD "playbg.bsv"
OPEN "playbg.pal" FOR BINARY AS #1
OUT &H3C7, 0: OUT &H3C8, 0: FOR i% = 0 TO 767: OUT &H3C9, ASC(INPUT$(1, 1)): NEXT i%
CLOSE #1
loadmap map$, map%(), playerx%, playery%, mapx%, mapy%, resource$()
herodir% = 4
LINE (148, 18)-(301, 171), 15, B
LINE (149, 19)-(300, 170), 0, B
LINE (6, 18)-(137, 171), 15, B
LINE (7, 19)-(136, 170), 0, BF
IF level% > 1 THEN PUT (8, 20), saveecho%, PSET
drawmap mapx%, mapy%, mapx% + 14, mapy% + 14, 16, 3, map%(), playerx%, playery%, herodir%
a$ = resources$(playerx%, playery%, resource$())
IF (a$ <> "") THEN
 SELECT CASE ASC(LEFT$(a$, 1))
  CASE 1: playmap MID$(a$, 2) + ".map"
  CASE 2: level% = level% - 1: EXIT SUB
  CASE 3: 'trap
  CASE 4: echo MID$(a$, 2)
  CASE 5: 'same
 END SELECT
END IF
DO
 drawmap mapx%, mapy%, mapx% + 14, mapy% + 14, 16, 3, map%(), playerx%, playery%, herodir%
 DO
  a$ = INKEY$
  st# = TIMER
  IF ABS(TIMER - st#) > .01 THEN monstercycle: st# = TIMER
 LOOP WHILE a$ = ""
 IF a$ = CHR$(27) THEN abort% = -1: EXIT DO
 b$ = a$
 processkey a$, playerx%, playery%, map%(), herodir%, mapx%, mapy%, resource$()
 IF (a$ <> b$) AND (a$ <> "") THEN
  SELECT CASE ASC(LEFT$(a$, 1))
   CASE 1: playmap MID$(a$, 2) + ".map"
   CASE 2: EXIT DO
   CASE 3: 'trap
   CASE 4: echo MID$(a$, 2)
   CASE 5: 'same
  END SELECT
 END IF
LOOP UNTIL abort%
level% = level% - 1
END SUB

SUB processkey (a$, playerx%, playery%, map%(), herodir%, mapx%, mapy%, resource$())
 SELECT CASE a$
  CASE CHR$(0) + "H", "8":
   playery% = playery% - 1
   IF playery% = 0 THEN playery% = 1
   a$ = resources$(playerx%, playery%, resource$())
   IF (map%(playerx%, playery%) <> 6) AND (map%(playerx%, playery%) <> 0) AND (map%(playerx%, playery%) <> 64) THEN
    playery% = playery% + 1
   ELSEIF playery% < 142 THEN
    mapy% = mapy% - 1
    IF mapy% = 0 THEN mapy% = 1
   END IF
   herodir% = 1
  CASE CHR$(0) + "P", "2":
   playery% = playery% + 1
   IF playery% = 151 THEN playery% = 150
   a$ = resources$(playerx%, playery%, resource$())
   IF (map%(playerx%, playery%) <> 6) AND (map%(playerx%, playery%) <> 0) AND (map%(playerx%, playery%) <> 64) THEN
    playery% = playery% - 1
   ELSEIF playery% > 8 THEN
    mapy% = mapy% + 1
    IF mapy% = 137 THEN mapy% = 136
   END IF
   herodir% = 2
  CASE CHR$(0) + "K", "4":
   playerx% = playerx% - 1
   IF playerx% = 0 THEN playerx% = 1
   a$ = resources$(playerx%, playery%, resource$())
   IF (map%(playerx%, playery%) <> 6) AND (map%(playerx%, playery%) <> 0) AND (map%(playerx%, playery%) <> 64) THEN
    playerx% = playerx% + 1
   ELSEIF playerx% < 142 THEN
    mapx% = mapx% - 1
    IF mapx% = 0 THEN mapx% = 1
   END IF
   herodir% = 3
  CASE CHR$(0) + "M", "6":
   playerx% = playerx% + 1
   IF playerx% = 151 THEN playerx% = 150
   a$ = resources$(playerx%, playery%, resource$())
   IF (map%(playerx%, playery%) <> 6) AND (map%(playerx%, playery%) <> 0) AND (map%(playerx%, playery%) <> 64) THEN
    playerx% = playerx% - 1
   ELSEIF playerx% > 8 THEN
    mapx% = mapx% + 1
    IF mapx% = 137 THEN mapx% = 136
   END IF
   herodir% = 4
  CASE CHR$(0) + "G", "7":
   playerx% = playerx% - 1
   playery% = playery% - 1
   IF playerx% = 0 THEN playerx% = 1
   IF playery% = 0 THEN playery% = 1
   a$ = resources$(playerx%, playery%, resource$())
   IF (map%(playerx%, playery%) <> 6) AND (map%(playerx%, playery%) <> 0) AND (map%(playerx%, playery%) <> 64) THEN
    playerx% = playerx% + 1
    playery% = playery% + 1
   ELSE
    IF playerx% < 142 THEN
     mapx% = mapx% - 1
     IF mapx% = 0 THEN mapx% = 1
    END IF
    IF playery% < 142 THEN
     mapy% = mapy% - 1
     IF mapy% = 0 THEN mapy% = 1
    END IF
   END IF
   herodir% = 5
  CASE CHR$(0) + "O", "1":
   playerx% = playerx% - 1
   playery% = playery% + 1
   IF playerx% = 0 THEN playerx% = 1
   IF playery% = 151 THEN playery% = 150
   a$ = resources$(playerx%, playery%, resource$())
   IF (map%(playerx%, playery%) <> 6) AND (map%(playerx%, playery%) <> 0) AND (map%(playerx%, playery%) <> 64) THEN
    playerx% = playerx% + 1
    playery% = playery% - 1
   ELSE
    IF playerx% < 142 THEN
     mapx% = mapx% - 1
     IF mapx% = 0 THEN mapx% = 1
    END IF
    IF playery% > 8 THEN
     mapy% = mapy% + 1
     IF mapy% = 137 THEN mapy% = 136
    END IF
   END IF
   herodir% = 6
  CASE CHR$(0) + "I", "9":
   playerx% = playerx% + 1
   playery% = playery% - 1
   IF playerx% = 151 THEN playerx% = 150
   IF playery% = 0 THEN playery% = 1
   a$ = resources$(playerx%, playery%, resource$())
   IF (map%(playerx%, playery%) <> 6) AND (map%(playerx%, playery%) <> 0) AND (map%(playerx%, playery%) <> 64) THEN
    playerx% = playerx% - 1
    playery% = playery% + 1
   ELSE
    IF playerx% > 8 THEN
     mapx% = mapx% + 1
     IF mapx% = 137 THEN mapx% = 136
    END IF
    IF playery% < 142 THEN
     mapy% = mapy% - 1
     IF mapy% = 0 THEN mapy% = 1
    END IF
   END IF
   herodir% = 7
  CASE CHR$(0) + "Q", "3":
   playerx% = playerx% + 1
   playery% = playery% + 1
   IF playerx% = 151 THEN playerx% = 150
   IF playery% = 151 THEN playery% = 150
   a$ = resources$(playerx%, playery%, resource$())
   IF (map%(playerx%, playery%) <> 6) AND (map%(playerx%, playery%) <> 0) AND (map%(playerx%, playery%) <> 64) THEN
    playerx% = playerx% - 1
    playery% = playery% - 1
   ELSE
    IF playerx% > 8 THEN
     mapx% = mapx% + 1
     IF mapx% = 137 THEN mapx% = 136
    END IF
    IF playery% > 8 THEN
     mapy% = mapy% + 1
     IF mapy% = 137 THEN mapy% = 136
    END IF
   END IF
   herodir% = 8
 END SELECT
END SUB

SUB putongrass (s%)
SHARED sprite%()
PUT (1, 1), sprite%(0, 6), PSET
PUT (11, 1), sprite%(0, s%), PSET
FOR i% = 11 TO 20
 FOR j% = 1 TO 10
  IF POINT(i%, j%) <> 0 THEN
   PSET (i% - 10, j%), POINT(i%, j%)
  END IF
 NEXT j%
NEXT i%
GET (1, 1)-(10, 10), sprite%(0, s%)
END SUB

FUNCTION readfile% (a$, pal() AS rgbtype)
OPEN a$ FOR BINARY AS #25
IF INPUT$(35, 25) <> "ArCoSoft Compressed Graphics Format" THEN
 CLOSE #1
 readfile% = 1
 EXIT FUNCTION
ELSE
 garbage$ = INPUT$(1, 25)
 gmode% = ASC(INPUT$(1, 25))
 cols% = ASC(INPUT$(1, 25))
END IF
FOR i% = 0 TO cols%
 a$ = INPUT$(3, 25)
 pal(i%).r = ASC(LEFT$(a$, 1))
 pal(i%).g = ASC(MID$(a$, 2, 1))
 pal(i%).b = ASC(RIGHT$(a$, 1))
NEXT i%
FOR i% = 0 TO 199
 linecount% = ASC(INPUT$(1, 25))
 IF linecount% = 1 THEN
  j% = 1
  thiscol% = ASC(INPUT$(1, 25))
  PSET (0, i%), thiscol%
  thislen% = CVI(INPUT$(2, 25))
  DO
   LINE (j%, i%)-(j% + thislen% - 1, i%), thiscol%
   j% = j% + thislen%
   thiscol% = ASC(INPUT$(1, 25))
   thislen% = CVI(INPUT$(2, 25))
  LOOP UNTIL thiscol% + thislen% = 0
 ELSEIF linecount% = 2 THEN
  a$ = INPUT$(320, 25)
  FOR j% = 1 TO 320
   PSET (j% - 1, i%), ASC(MID$(a$, j%, 1))
  NEXT j%
 ELSEIF linecount% = 3 THEN
  thiscol% = ASC(INPUT$(1, 25))
  LINE (0, i%)-(319, i%), thiscol%
 ELSE
  LINE (0, i%)-(319, i%), 0
 END IF
NEXT i%
CLOSE #25
END FUNCTION

SUB readgif (a$, pal() AS rgbtype)
' Author unknown
DEFINT A-Z
DIM Prefix(4095), Suffix(4095), OutStack(4095), shiftout%(8)
DIM Ybase AS LONG, powersof2(11) AS LONG, WorkCode AS LONG
FOR a% = 0 TO 7: shiftout%(8 - a%) = 2 ^ a%: NEXT a%
FOR a% = 0 TO 11: powersof2(a%) = 2 ^ a%: NEXT a%
OPEN a$ FOR BINARY AS #1
a$ = "      ": GET #1, , a$
'IF A$ <> "GIF87a" THEN PRINT "Not a GIF87a file.": END
GET #1, , TotalX: GET #1, , TotalY: GOSUB GetByte
NumColors = 2 ^ ((a% AND 7) + 1): NoPalette = (a% AND 128) = 0
GOSUB GetByte: Background = a%
GOSUB GetByte: IF a% <> 0 THEN PRINT "Bad screen descriptor.": END
IF NoPalette = 0 THEN p$ = SPACE$(NumColors * 3): GET #1, , p$
DO
 GOSUB GetByte
 IF a% = 44 THEN
  EXIT DO
 ELSEIF a% <> 33 THEN
  PRINT "Unknown extension type.": END
 END IF
 GOSUB GetByte
 DO: GOSUB GetByte: a$ = SPACE$(a%): GET #1, , a$: LOOP UNTIL a% = 0
LOOP
GET #1, , XStart: GET #1, , YStart: GET #1, , XLength: GET #1, , YLength
XEnd = XStart + XLength: YEnd = YStart + YLength: GOSUB GetByte
IF a% AND 128 THEN PRINT "Can't handle local colormaps.": END
Interlaced = a% AND 64: PassNumber = 0: PassStep = 8
GOSUB GetByte
ClearCode = 2 ^ a%
EOSCode = ClearCode + 1
FirstCode = ClearCode + 2: NextCode = FirstCode
StartCodeSize = a% + 1: CodeSize = StartCodeSize
StartMaxCode = 2 ^ (a% + 1) - 1: MaxCode = StartMaxCode
BitsIn = 0: BlockSize = 0: BlockPointer = 1
x% = XStart: y% = YStart: Ybase = y% * 320&
DEF SEG = &HA000
IF NoPalette = 0 THEN
 FOR a% = 0 TO NumColors - 1
  pal(a%).r = ASC(MID$(p$, a% * 3 + 1, 1)) \ 4
  pal(a%).g = ASC(MID$(p$, a% * 3 + 2, 1)) \ 4
  pal(a%).b = ASC(MID$(p$, a% * 3 + 3, 1)) \ 4
 NEXT a%
END IF
LINE (0, 0)-(319, 199), Background, BF
DO
 GOSUB GetCode
 IF Code <> EOSCode THEN
  IF Code = ClearCode THEN
   NextCode = FirstCode
   CodeSize = StartCodeSize
   MaxCode = StartMaxCode
   GOSUB GetCode
   CurCode = Code: LastCode = Code: LastPixel = Code
   IF x% < 320 THEN POKE x% + Ybase, LastPixel
   x% = x% + 1: IF x% = XEnd THEN GOSUB NextScanLine
  ELSE
   CurCode = Code: StackPointer = 0
   IF Code > NextCode THEN EXIT DO
   IF Code = NextCode THEN
    CurCode = LastCode
    OutStack(StackPointer) = LastPixel
    StackPointer = StackPointer + 1
   END IF
   DO WHILE CurCode >= FirstCode
    OutStack(StackPointer) = Suffix(CurCode)
    StackPointer = StackPointer + 1
    CurCode = Prefix(CurCode)
   LOOP
   LastPixel = CurCode
   IF x% < 320 THEN POKE x% + Ybase, LastPixel
   x% = x% + 1: IF x% = XEnd THEN GOSUB NextScanLine
   FOR a% = StackPointer - 1 TO 0 STEP -1
    IF x% < 320 THEN POKE x% + Ybase, OutStack(a%)
    x% = x% + 1: IF x% = XEnd THEN GOSUB NextScanLine
   NEXT a%
   IF NextCode < 4096 THEN
    Prefix(NextCode) = LastCode
    Suffix(NextCode) = LastPixel
    NextCode = NextCode + 1
    IF NextCode > MaxCode AND CodeSize < 12 THEN
     CodeSize = CodeSize + 1
     MaxCode = MaxCode * 2 + 1
    END IF
   END IF
   LastCode = Code
  END IF
 END IF
LOOP UNTIL DoneFlag OR Code = EOSCode
CLOSE #1
EXIT SUB

GetByte:
 a$ = " ": GET #1, , a$: a% = ASC(a$): RETURN

NextScanLine:
    IF Interlaced THEN
        y% = y% + PassStep
        IF y% >= YEnd THEN
            PassNumber = PassNumber + 1
            SELECT CASE PassNumber
            CASE 1: y% = 4: PassStep = 8
            CASE 2: y% = 2: PassStep = 4
            CASE 3: y% = 1: PassStep = 2
            END SELECT
        END IF
    ELSE
        y% = y% + 1
    END IF
    x% = XStart: Ybase = y% * 320&: DoneFlag = y% > 199
RETURN
GetCode:
    IF BitsIn = 0 THEN GOSUB ReadBufferedByte: LastChar = a%: BitsIn = 8
    WorkCode = LastChar \ shiftout%(BitsIn)
    DO WHILE CodeSize > BitsIn
        GOSUB ReadBufferedByte: LastChar = a%
        WorkCode = WorkCode OR LastChar * powersof2(BitsIn)
        BitsIn = BitsIn + 8
    LOOP
    BitsIn = BitsIn - CodeSize
    Code = WorkCode AND MaxCode
RETURN
ReadBufferedByte:
    IF BlockPointer > BlockSize THEN
        GOSUB GetByte: BlockSize = a%
        a$ = SPACE$(BlockSize): GET #1, , a$
        BlockPointer = 1
    END IF
    a% = ASC(MID$(a$, BlockPointer, 1)): BlockPointer = BlockPointer + 1
RETURN
END SUB

DEFSNG A-Z
FUNCTION resources$ (x%, y%, resource$())
chk$ = CHR$(x%) + CHR$(y%)
FOR i% = 1 TO VAL(resource$(0))
 IF MID$(resource$(i%), 2, 2) = chk$ THEN EXIT FOR
NEXT i%
IF i% <= VAL(resource$(0)) THEN resources$ = MID$(resource$(i%), 4)
IF LEFT$(resource$(i%), 1) = CHR$(1) THEN resource$(i%) = STRING$(3, 155)
END FUNCTION

SUB setuppalette
DIM pal(255) AS rgbtype
FOR i% = 1 TO 16
 pal(i% + 15).r = 0
 pal(i% + 15).g = i% * 4 - 1
 pal(i% + 15).b = 0
 crclsize%(i%) = i%
NEXT i%
pal(8).r = 0: pal(8).g = 0: pal(8).b = 63
pal(7).r = 0: pal(7).g = 0: pal(7).b = 0
fadein pal()
END SUB


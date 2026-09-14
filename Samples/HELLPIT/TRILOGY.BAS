DECLARE SUB words (x!, y!, word$, c!)
SCREEN 8
CLS
PAINT (1, 1), 8

words 40, 0, "FIG INC. PRESENTS HELLPIT TRILOGY", 9
words 38, 0, "FIG INC. PRESENTS HELLPIT TRILOGY", 1
                                                           
words 40, 20, "THIS GAME MAY BE DISTRIBUTED, AS LONG AS IT IS NOT EDITED", 4
words 38, 20, "THIS GAME MAY BE DISTRIBUTED, AS LONG AS IT IS NOT EDITED", 12

words 10, 30, "IN ANY WAY.  THIS GAME REQUIRES NO DONATIONS OR REGISTRATIONS", 4
words 8, 30, "IN ANY WAY.  THIS GAME REQUIRES NO DONATIONS OR REGISTRATIONS", 12

words 10, 40, "OF ANY SORT.  THERE ARE THREE HELLPIT GAMES.  YOU SHOULD PLAY", 4
words 8, 40, "OF ANY SORT.  THERE ARE THREE HELLPIT GAMES.  YOU SHOULD PLAY", 12

words 10, 50, "THEM IN ORDER, FROM THE FIRST TO THE THIRD.", 4
words 8, 50, "THEM IN ORDER, FROM THE FIRST TO THE THIRD.", 12

words 40, 70, "THIS GAME WAS MADE BY                   OF FIG INC.", 4
words 38, 70, "THIS GAME WAS MADE BY                   OF FIG INC.", 12

words 260, 70, "ANTHONY FIGLIUZZI", 11
words 258, 70, "ANTHONY FIGLIUZZI", 1

words 40, 80, "SEND ALL COMMENTS AND QUESTIONS TO", 4
words 38, 80, "SEND ALL COMMENTS AND QUESTIONS TO", 12

words 390, 80, "PANTERA55@AOL.COM", 11
words 388, 80, "PANTERA55@AOL.COM", 1

words 40, 100, "PLEASE READ TRILOGY.TXT BEFORE YOU PLAY THE GAME", 2
words 38, 100, "PLEASE READ TRILOGY.TXT BEFORE YOU PLAY THE GAME", 10

SLEEP

CLS
PAINT (1, 1), 8

words 180, 0, "HELLPIT TRILOGY MAIN MENU", 4
words 178, 0, "HELLPIT TRILOGY MAIN MENU", 10

words 240, 80, "HELLPIT I", 4
words 238, 80, "HELLPIT I", 1

words 240, 90, "HELLPIT II", 4
words 238, 90, "HELLPIT II", 1

words 240, 100, "HELLPIT III", 4
words 238, 100, "HELLPIT III", 1

y = 83

DO

LINE (200, y)-(230, y), 14
LINE -(215, y - 5), 14
LINE (230, y)-(215, y + 5), 14

LINE (200, y - 5)-(230, y + 5), 8, BF

a$ = INKEY$
IF a$ = CHR$(0) + "P" THEN y = y + 10
IF a$ = CHR$(0) + "H" THEN y = y - 10
IF a$ = CHR$(13) THEN GOTO 1

IF y < 83 THEN y = 103
IF y > 103 THEN y = 83

LOOP

1
CLS
IF y = 83 THEN CHAIN "hellpit1.bas"
IF y = 93 THEN CHAIN "hellpit2.bas"
IF y = 103 THEN CHAIN "hellpit3.bas"

SUB words (x, y, word$, c)
FOR t = 1 TO LEN(word$)
a$ = MID$(word$, t, 1)
COLOR c
SELECT CASE a$
        CASE IS = "A"
        LINE (x - 3, y + 6)-(x - 3, y + 3)
        LINE -(x, y)
        LINE -(x + 3, y + 3)
        LINE -(x + 3, y + 6)
        LINE (x - 3, y + 3)-(x + 3, y + 3)
      
        CASE IS = "B"
        LINE (x - 3, y)-(x + 2, y)
        LINE -(x + 3, y + 1)
        LINE -(x + 3, y + 2)
        LINE -(x + 2, y + 3)
        LINE -(x + 3, y + 4)
        LINE -(x + 3, y + 5)
        LINE -(x + 2, y + 6)
        LINE -(x - 3, y + 6)
        LINE (x - 2, y)-(x - 2, y + 6)
        LINE (x - 3, y + 3)-(x + 2, y + 3)
     
        CASE IS = "C"
        LINE (x + 3, y)-(x, y)
        LINE -(x - 3, y + 3)
        LINE -(x, y + 6)
        LINE -(x + 3, y + 6)

        CASE IS = "D"
        LINE (x - 3, y)-(x + 2, y)
        LINE -(x + 3, y + 1)
        LINE -(x + 3, y + 5)
        LINE -(x + 2, y + 6)
        LINE -(x - 3, y + 6)
        LINE (x - 2, y)-(x - 2, y + 6)

        CASE IS = "E"
        LINE (x - 3, y)-(x + 3, y)
        LINE (x - 2, y + 3)-(x + 2, y + 3)
        LINE (x - 3, y + 6)-(x + 3, y + 6)
        LINE (x - 2, y)-(x - 2, y + 6)

        CASE IS = "F"
        LINE (x - 3, y)-(x + 3, y)
        LINE (x - 2, y + 3)-(x + 2, y + 3)
        LINE (x - 2, y)-(x - 2, y + 6)
        LINE (x - 3, y + 6)-(x - 1, y + 6)

        CASE IS = "G"
        LINE (x + 3, y + 2)-(x + 3, y)
        LINE -(x - 3, y)
        LINE (x - 3, y)-(x - 3, y + 6)
        LINE (x - 3, y + 6)-(x + 3, y + 6)
        LINE -(x + 3, y + 4)
        LINE -(x + 1, y + 4)

        CASE IS = "H"
        LINE (x - 3, y)-(x - 3, y + 6)
        LINE (x + 3, y)-(x + 3, y + 6)
        LINE (x - 3, y + 3)-(x + 3, y + 3)

        CASE IS = "I"
        LINE (x + 3, y)-(x - 3, y)
        LINE (x, y)-(x, y + 6)
        LINE (x - 3, y + 6)-(x + 3, y + 6)

        CASE IS = "J"
        LINE (x - 3, y)-(x + 3, y)
        LINE (x + 1, y)-(x + 1, y + 6)
        LINE -(x - 3, y + 6)
        LINE -(x - 3, y + 4)

        CASE IS = "K"
        LINE (x - 3, y)-(x - 2, y)
        LINE -(x - 2, y + 6)
        LINE -(x - 3, y + 6)
        LINE (x + 3, y)-(x - 1, y + 3)
        LINE -(x + 3, y + 6)

        CASE IS = "L"
        LINE (x - 3, y)-(x - 1, y)
        LINE (x - 2, y)-(x - 2, y + 6)
        LINE (x - 3, y + 6)-(x + 3, y + 6)
        LINE -(x + 3, y + 5)

        CASE IS = "M"
        LINE (x - 3, y)-(x - 1, y)
        LINE (x + 3, y)-(x + 1, y)
        LINE (x - 3, y)-(x - 3, y + 6)
        LINE (x + 3, y)-(x + 3, y + 6)
        LINE (x - 2, y)-(x, y + 2)
        LINE -(x + 2, y)

        CASE IS = "N"
        LINE (x - 3, y)-(x - 1, y)
        LINE (x - 3, y + 6)-(x - 3, y + 6)
        LINE (x - 2, y + 6)-(x - 2, y)
        LINE -(x + 2, y + 6)
        LINE -(x + 2, y)
        LINE (x + 3, y)-(x + 1, y)
        LINE (x + 3, y + 6)-(x + 3, y + 6)
      
        CASE IS = "O"
        LINE (x - 3, y)-(x + 3, y)
        LINE -(x + 3, y + 6)
        LINE -(x - 3, y + 6)
        LINE -(x - 3, y)
      
        CASE IS = "P"
        LINE (x - 3, y)-(x + 2, y)
        LINE -(x + 3, y + 1)
        LINE -(x + 3, y + 2)
        LINE -(x + 2, y + 3)
        LINE -(x - 2, y + 3)
        LINE (x - 2, y)-(x - 2, y + 6)
        LINE (x - 3, y + 6)-(x - 1, y + 6)
      
        CASE IS = "Q"
        LINE (x - 3, y)-(x + 3, y)
        LINE -(x + 3, y + 6)
        LINE -(x - 3, y + 6)
        LINE -(x - 3, y)
        LINE (x + 1, y + 4)-(x + 3, y + 6)
      
        CASE IS = "R"
        LINE (x - 3, y)-(x + 2, y)
        LINE -(x + 3, y + 1)
        LINE -(x + 3, y + 2)
        LINE -(x + 2, y + 3)
        LINE -(x - 2, y + 3)
        LINE (x - 2, y)-(x - 2, y + 6)
        LINE (x - 3, y + 6)-(x - 1, y + 6)
        LINE (x + 2, y + 3)-(x + 2, y + 6)
        LINE (x + 1, y + 6)-(x + 3, y + 6)
      
        CASE IS = "S"
        LINE (x + 3, y + 1)-(x + 3, y)
        LINE -(x - 3, y)
        LINE -(x - 3, y + 3)
        LINE -(x + 3, y + 3)
        LINE -(x + 3, y + 6)
        LINE -(x - 3, y + 6)
        LINE -(x - 3, y + 5)
      
        CASE IS = "T"
        LINE (x - 3, y + 1)-(x - 3, y)
        LINE -(x + 3, y)
        LINE -(x + 3, y + 1)
        LINE (x, y)-(x, y + 6)
        LINE (x - 1, y + 6)-(x + 1, y + 6)
     
        CASE IS = "U"
        LINE (x - 3, y)-(x - 3, y + 6)
        LINE -(x + 3, y + 6)
        LINE -(x + 3, y)
       
        CASE IS = "V"
        LINE (x - 3, y)-(x - 3, y + 3)
        LINE -(x, y + 6)
        LINE -(x + 3, y + 3)
        LINE -(x + 3, y)

        CASE IS = "W"
        LINE (x - 3, y)-(x - 3, y + 6)
        LINE -(x, y + 3)
        LINE -(x + 3, y + 6)
        LINE -(x + 3, y)
     
        CASE IS = "X"
        LINE (x - 3, y)-(x + 3, y + 6)
        LINE (x + 3, y)-(x - 3, y + 6)
             
        CASE IS = "Y"
        LINE (x - 3, y)-(x, y + 3)
        LINE -(x + 3, y)
        LINE (x, y + 3)-(x, y + 6)
        LINE (x - 1, y + 6)-(x + 1, y + 6)
     
        CASE IS = "Z"
        LINE (x - 3, y + 1)-(x - 3, y)
        LINE -(x + 3, y)
        LINE -(x - 3, y + 6)
        LINE -(x + 3, y + 6)
        LINE -(x + 3, y + 5)
      
        CASE IS = "'"
        LINE (x - 3, y + 3)-(x, y)
        x = x - 2

        CASE IS = ","
        LINE (x, y + 6)-(x - 3, y + 9)

        CASE IS = "."
        LINE (x - 2, y + 6)-(x - 2, y + 6)

        CASE IS = "5"
        LINE (x + 3, y)-(x - 3, y)
        LINE -(x - 3, y + 3)
        LINE -(x + 2, y + 3)
        LINE -(x + 3, y + 4)
        LINE -(x + 3, y + 5)
        LINE -(x + 2, y + 6)
        LINE -(x - 3, y + 6)
     
        CASE IS = "@"
        LINE (x + 1, y + 4)-(x + 1, y + 3)
        LINE -(x + 2, y + 3)
        LINE -(x + 2, y + 4)
        LINE -(x + 3, y + 4)
        LINE -(x + 3, y)
        LINE -(x - 2, y)
        LINE -(x - 3, y + 1)
        LINE -(x - 3, y + 5)
        LINE -(x - 2, y + 6)
        LINE -(x + 1, y + 6)
        END SELECT

x = x + 10
NEXT
END SUB


DECLARE SUB LoadSprites (sprites%())
DECLARE SUB SaveSprites (sprites%())
DECLARE SUB GenerateSprites (sprites%())
DECLARE SUB TryOutSprites (sprites%())

'$DYNAMIC

SCREEN 13

RANDOMIZE TIMER

DIM sprites%(1, 1)

GenerateSprites sprites%()
TryOutSprites sprites%()

PRINT "Press any key to save sprites"
SLEEP
CLS

SaveSprites sprites%()
REDIM sprites%(1, 1)

PRINT "Press any key to load sprites"
SLEEP
CLS

LoadSprites sprites%()
TryOutSprites sprites%()

SUB GenerateSprites (sprites%())
REDIM sprites%(51, 1 TO 5)

CIRCLE (5, 5), 3, 14

LINE (10, 0)-(12, 2), 9
LINE (19, 0)-(17, 2), 9
LINE (10, 9)-(12, 7), 9
LINE (19, 9)-(17, 7), 9

x1% = 23 + RND * 3: y1% = RND * 3
x2% = 20 + RND * 3: y2% = RND * 3 + 3
x3% = 26 + RND * 3: y3% = RND * 3 + 6
LINE (x1%, y1%)-(x2%, y2%), 1
LINE (x2%, y2%)-(x3%, y3%), 2
LINE (x3%, y3%)-(x1%, y1%), 4

FOR x% = 30 TO 39
  y% = 5 + 3 * SIN(x%)
  PSET (x%, y% - 1), x% + 20
  PSET (x%, y%), x%
  PSET (x%, y% + 1), x% + 40
NEXT x%

FOR x% = 40 TO 49
  FOR y% = 0 TO 9
    dx! = x% - 44.5
    dy! = y% - 4.5
    r! = SQR(dx! * dx! + dy! * dy!)
    IF r! < 5 THEN
      dx! = x% - 43
      dy! = y% - 2.5
      r! = SQR(dx! * dx! + dy! * dy!)
      PSET (x%, y%), 31 - r! * 2
    END IF
  NEXT y%
NEXT x%

FOR i% = 1 TO 5
  GET (i% * 10 - 10, 0)-(i% * 10 - 1, 9), sprites%(0, i%)
NEXT i%

CLS
END SUB

SUB LoadSprites (sprites%())
OPEN "sprites.txt" FOR INPUT AS #1

' File header line: number of sprites and how many array entries per sprite
INPUT #1, numsprites%, spritelength%

REDIM sprites%(0 TO spritelength% - 1, 1 TO numsprites%)

FOR i% = 1 TO numsprites%
  ' GET/PUT header
  INPUT #1, sprites%(0, i%), sprites%(1, i%)

  ' Sprite data
  element$ = ".."
  FOR j% = spritestart% + 2 TO UBOUND(sprites%, 1)
    k% = 1
    WHILE k% <= LEN(element$)
      hexchars$ = INPUT$(2, 1)

      ' Discard non-hex characters until we have two hex characters
      WHILE INSTR(".0123456789ABCDEF", LEFT$(hexchars$, 1)) = 0
        hexchars$ = MID$(hexchars$, 2) + INPUT$(1, 1)
      WEND

      WHILE INSTR(".0123456789ABCDEF", RIGHT$(hexchars$, 1)) = 0
        hexchars$ = LEFT$(hexchars$, 1) + INPUT$(1, 1)
      WEND

      bytevalue% = VAL("&H" + hexchars$)

      MID$(element$, k%, 1) = CHR$(bytevalue%)

      k% = k% + 1
    WEND

    sprites%(j%, i%) = CVI(element$)
  NEXT j%

  ' Eat CRLF after each sprite
  hexchars$ = INPUT$(2, 1)
NEXT i%
END SUB

SUB SaveSprites (sprites%())
numsprites% = UBOUND(sprites%, 2) - LBOUND(sprites%, 2) + 1
spritestart% = LBOUND(sprites%, 1)
spritelength% = UBOUND(sprites%, 1) - spritestart% + 1

OPEN "sprites.txt" FOR OUTPUT AS #1

' File header line: number of sprites and how many array entries per sprite
PRINT #1, numsprites%; ","; spritelength%

FOR i% = LBOUND(sprites%, 2) TO UBOUND(sprites%, 2)
  bytesperscan% = sprites%(0, i%) \ 8 ' Just for file layout, does not affect actual data

  ' GET/PUT header
  PRINT #1, sprites%(spritestart%, i%); ","; sprites%(spritestart% + 1, i%)

  ' Sprite data
  scanremaining% = bytesperscan%

  FOR j% = spritestart% + 2 TO UBOUND(sprites%, 1)
    element$ = MKI$(sprites%(j%, i%))

    FOR k% = 1 TO LEN(element$)
      IF scanremaining% = 0 THEN PRINT #1, : scanremaining% = bytesperscan%

      byte% = ASC(MID$(element$, k%, 1))

      IF byte% = 0 THEN PRINT #1, "..";  ELSE PRINT #1, RIGHT$("0" + HEX$(byte%), 2);

      scanremaining% = scanremaining% - 1
    NEXT k%
  NEXT j%

  PRINT #1,
NEXT i%

CLOSE #1
END SUB

SUB TryOutSprites (sprites%())
firstsprite% = LBOUND(sprites%, 2)
spritecount% = UBOUND(sprites%, 2) - LBOUND(sprites%, 2) + 1

FOR i% = 1 TO 50
  PUT (RND * 310, RND * 190), sprites%(0, FIX(RND * spritecount% + firstsprite%)), PSET
NEXT i%
END SUB

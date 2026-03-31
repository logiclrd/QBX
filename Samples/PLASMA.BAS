' Translated from C64 BASIC:
'
' https://github.com/sidchoudhuri/C64-BASIC/blob/main/plasma%2Freadme.md

DECLARE SUB SetC64Palette ()

ShowColourCodes% = 0

WIDTH 40, 25

SetC64Palette

' Build sine table

DIM s(255)

FOR i = 0 TO 255
  s(i) = INT(8 + 7 * SIN(i * .0245))
NEXT

DEF SEG = &HB800

' Build character table
DIM char%(15)

IF ShowColourCodes% THEN
  FOR i% = 0 TO 9
    char%(i%) = i% + 48
  NEXT i%

  FOR i% = 10 TO 15
    char%(i%) = i% + 55 ' 65 = A
  NEXT i%
ELSE
  FOR i% = 0 TO 15
    char%(i%) = 2 ' Closest to a filled oval :-)
  NEXT i%
END IF

' Build plasma

FOR y = 0 TO 24
  FOR x = 0 TO 39
    o = y * 80 + x * 2

    c = (s((x * 8) AND 255) + s((y * 8) AND 255)) AND 15

    POKE o + 1, c
    POKE o, char%(c)
  NEXT
NEXT

' Cycle plasma
DO
  FOR i = 0 TO 999
    o = i * 2
    c = (PEEK(o + 1) + 1) AND 15
    POKE o + 1, c
    POKE o, char%(c)
  NEXT i

  st! = TIMER: WHILE st! = TIMER: WEND
LOOP UNTIL INKEY$ <> ""

C64Palette:
  DATA 0,0,0, 255,255,255, 135,0,0, 170,253,238, 204,67,202, 0,203,84, 0,0,169, 236,238,118
  DATA 221,135,84, 100,69,1, 254,118,118, 48,48,48, 118,118,118, 168,253,100, 1,134,254, 186,186,186

SUB SetC64Palette
' Map attributes to colours directly
FOR i% = 0 TO 15
  PALETTE i%, i%
NEXT i%

' Configure colours in the RAMDAC
RESTORE C64Palette

OUT &H3C8, 0

FOR i% = 0 TO 47
  READ a%
  OUT &H3C9, a% * 63 \ 255
NEXT i%
END SUB


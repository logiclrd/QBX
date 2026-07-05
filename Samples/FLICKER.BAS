' If this updates consistently fast enough, then the alternating
' colours in the bottom half of the screen will appear similar
' to the solid colours in the top half. If the screen is recorded
' with a physical camera at 30 fps, then the frames will blend to
' similar solid colours.

SCREEN 7

CIRCLE (60, 50), 30, 1
PAINT (60, 50), 1, 1
CIRCLE (60, 150), 30, 4
PAINT (60, 150), 4, 4

FOR i% = 1 TO 30
  CIRCLE (160, 50), i%, 2
  CIRCLE (160, 150), i%, 5
NEXT i%

CIRCLE (260, 50), 30, 3
CIRCLE (260, 150), 30, 6

OUT &H3C8, 1
FOR i% = 36 TO 63 STEP 63 - 36
  OUT &H3C9, i%
  OUT &H3C9, 0
  OUT &H3C9, 0
  OUT &H3C9, i%
  OUT &H3C9, i%
  OUT &H3C9, 0
  OUT &H3C9, 0
  OUT &H3C9, i%
  OUT &H3C9, 0
NEXT i%

PALETTE 6, 6

DO
  WAIT &H3DA, 8
  PALETTE 4, 0
  PALETTE 5, 6
  PALETTE 6, 6
  WAIT &H3DA, 8, 8
  WAIT &H3DA, 8
  PALETTE 4, 4
  PALETTE 5, 4
  PALETTE 6, 0
  WAIT &H3DA, 8, 8
LOOP WHILE INKEY$ = ""

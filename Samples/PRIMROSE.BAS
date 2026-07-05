' Source: https://github.com/oonap0oo/QB64-projects/blob/main/optical_illusion.bas
'
' Primrose's field          K Moerman 2026
' Visual illusion based on FB reel by 'Conscious Lines':
' https://www.facebook.com/reel/2040723323204086
' A variant of illusions known as 'Primrose's field'
' This program only uses legacy QBASIC and QUICKBASIC functionality
' so no added functions of QB64
' This program was tested to also run on QBASIC 1.1 in DOSBOX

DECLARE SUB cross (x%, y%, crosscol%, crossl%, crosst%)
DECLARE SUB delay (t!)

SCREEN 12 ' 640x480 standard mode
w% = 640: h% = 480
s% = 40 ' width and height of squares
' alter colors 12 and 14 using 6 bit rgb components
PALETTE 12, 52 + 32 * 256 + 12 * 65536
PALETTE 14, 39 + 12 * 256 + 7 * 65536
' read color data for crosses and store in array
DIM crosscol%(8)
FOR index% = 0 TO 7
    READ crosscol%(index%)
NEXT index%
' draw the checkerboard using colors 12 and 14
col% = 12
FOR y% = 0 TO h% STEP s%
    FOR x% = 0 TO w% STEP s%
        LINE (x%, y%)-STEP(s%, s%), col%, BF
        col% = 14 - (col% - 12)
    NEXT x%
NEXT y%
' draw the crosses at the square's corners using colors 0 and 15
FOR y% = 0 TO h% STEP s%
    index% = (y% \ s%) MOD 8
    FOR x% = 0 TO w% STEP s%
        cross x%, y%, crosscol%(index% MOD 8), 12, 4
        index% = index% + 1
    NEXT x%
NEXT y%
' main loop altering rgb components of color 15
DO
    SLEEP 2
    FOR grey% = 63 TO 0 STEP -2
        PALETTE 15, grey% + grey% * 256 + grey% * 65536
        delay (.06)
    NEXT grey%
    FOR grey% = 0 TO 63 STEP 2
        PALETTE 15, grey% + grey% * 256 + grey% * 65536
        delay (.06)
    NEXT grey%
LOOP UNTIL INKEY$ <> ""
END

' color pattern of successive crosses
DATA 0,15,0,15,15,0,15,0

' draw a cross at x%,y% with given length, width of stripes and color
SUB cross (x%, y%, crosscol%, crossl%, crosst%)
    LINE (x% - crossl% / 2, y% - crosst% / 2)-STEP(crossl%, crosst%), crosscol%, BF
    LINE (x% - crosst% / 2, y% - crossl% / 2)-STEP(crosst%, crossl%), crosscol%, BF
END SUB

' wait for given time, not using the QB64 statement _DELAY()
' using qbasic compatible TIMER version with 1/18th or 56ms seconds resolution
SUB delay (t!)
    tend = TIMER + t!
    WHILE TIMER < tend: WEND
END SUB

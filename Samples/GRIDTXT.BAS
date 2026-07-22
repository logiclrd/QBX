10 'Gridtext         K Moerman 2026
11 'Origin: https://github.com/oonap0oo/PC-BASIC-projects/blob/main/gridtxt.bas
20 'SCREEN 9: 640x350 pixels, 80x25 chars, 1 char is 8x14 pixels, 2 pages
30 W% = 640: H% = 350: CHARW% = 8: CHARH% = 14: TXT$ = "BASIC"
40 XSTART% = 40: YSTART% = 70: WAVE! = 20: GRID% = 14
50 CLS : KEY OFF: SCREEN 9, , 1, 0: PRINT TXT$'print text on page 1 show page 0
60 FOR XX% = -CHARW% / 4 TO 5 * CHARW%
70 X% = XSTART% + XX% * GRID%
80 FOR YY% = 0 TO CHARH%
90 Y% = YSTART% + YY% * GRID%
100 SCREEN , , 1, 0: COL% = POINT(XX%, YY%) ' work on page 1, show page 0
110 SCREEN , , 0, 0 ' work on page 0, show page 0
120 X2% = X% + WAVE! * SIN(7 * Y% / H%) ' displace x and y for wavy effect
130 Y2% = Y% + WAVE! * SIN(7 * X% / W%)
140 PLOTCOL% = 1 + ABS(XX% + YY%) MOD 15
150 IF COL% > 0 THEN GOSUB 280 ELSE GOSUB 300
160 NEXT YY%: NEXT XX%
170 SCREEN , , 0, 0 ' set work and show page to 0
180 ' cycle colors of palette
190 FOR PAL% = 1 TO 15
200 PALETTE PAL%, (PAL% + OFFSET%) MOD 64
210 NEXT PAL%
220 T! = TIMER + .06 'delay using TIMER function
230 WHILE TIMER < T!: WEND
240 OFFSET% = (OFFSET% + 1) MOD 64 'offset ramps 0..63
250 IF INKEY$ = "" THEN GOTO 190
260 PALETTE : END'undo palette changes before exit
270 ' sub to draw box if pixel text source is other color then black
280 LINE (X2% - 3, Y2% - 3)-(X2% + 3, Y2% + 3), PLOTCOL%, BF
290 RETURN
300 ' sub to draw point if pixel text source is black
310 PSET (X2%, Y2%), PLOTCOL%
320 RETURN

1 REM This program was originally posted to Facebonk by VintageWatches PK
2 REM The original version had terrible flickering, because it made no
3 REM attempt to draw off-screen, and it also had no provision for ending
4 REM the program. This version uses page flipping to eliminate flickering
5 REM and exits when a key is pressed. The changes were made by ChatGPT.

10 SCREEN 7
20 DIM GX(13), GY(13), GZ(13)
30 DIM X(13), Y(13), Z(13)
40 DIM SX(13), SY(13)

50 FOR I = 1 TO 13
60   READ GX(I), GY(I), GZ(I)
70 NEXT I

80 DATA -25,-25,-25, 25,-25,-25, 25,25,-25, -25,25,-25
90 DATA -25,-25,25, 25,-25,25, 25,25,25, -25,25,25
100 DATA 0,-50,0
110 DATA -25,25,-25, 25,25,-25, 25,25,25, -25,25,25

120 AX = 0
130 AY = 0
140 AZ = 0

150 DAX = .04
160 DAY = .03
170 DAZ = .02

180 AP = 1
190 VP = 0
200 SCREEN 7,,AP,VP

210 ' Draw the first frame
220 GOTO 270

230 ' ----------------------------------------
240 ' Main animation loop
250 ' ----------------------------------------

260 ' Update rotation angles
270 AX = AX + DAX
280 AY = AY + DAY
290 AZ = AZ + DAZ

300 ' Calculate rotation constants
310 CX = COS(AX): SX = SIN(AX)
320 CY = COS(AY): SY = SIN(AY)
330 CZ = COS(AZ): SZ = SIN(AZ)

340 ' Erase the active (hidden) page.
350 ' LINE is used instead of CLS because GW-BASIC
360 ' CLS does not reliably respect the page selection.
370 LINE (0,0)-(319,199),0,BF

380 ' Transform the original geometry.
390 ' GX/GY/GZ are never modified.
400 FOR I = 1 TO 13
410   Y1 = GY(I) * CX - GZ(I) * SX
420   Z1 = GY(I) * SX + GZ(I) * CX
430   X2 = GX(I) * CY + Z1 * SY
440   Z2 = -GX(I) * SY + Z1 * CY
450   X(I) = X2 * CZ - Y1 * SZ
460   Y(I) = X2 * SZ + Y1 * CZ
470   Z(I) = Z2

480   D = 250 / (250 - Z(I))
490   IF I <= 8 THEN OFX = 80 ELSE OFX = 240
500   SX(I) = OFX + X(I) * D * 1.5
510   SY(I) = 100 + Y(I) * D
520 NEXT I

530 ' Draw cube
540 FOR I = 1 TO 3
550   LINE (SX(I),SY(I))-(SX(I+1),SY(I+1)),10
560 NEXT I
570 LINE (SX(4),SY(4))-(SX(1),SY(1)),10

580 FOR I = 5 TO 7
590   LINE (SX(I),SY(I))-(SX(I+1),SY(I+1)),10
600 NEXT I
610 LINE (SX(8),SY(8))-(SX(5),SY(5)),10

620 FOR I = 1 TO 4
630   LINE (SX(I),SY(I))-(SX(I+4),SY(I+4)),10
640 NEXT I

650 ' Draw pyramid
660 FOR I = 10 TO 12
670   LINE (SX(I),SY(I))-(SX(I+1),SY(I+1)),10
680 NEXT I
690 LINE (SX(13),SY(13))-(SX(10),SY(10)),10

700 FOR I = 10 TO 13
710   LINE (SX(9),SY(9))-(SX(I),SY(I)),10
720 NEXT I

730 ' Exit on any keypress
740 IF INKEY$ <> "" THEN END

750 ' Swap the pages.
760 T = AP
770 AP = VP
780 VP = T
790 SCREEN 7,,AP,VP

800 GOTO 270

DECLARE SUB SetPalette ()
DECLARE SUB SetPaletteEntry (i%, r%, g%, b%)
DECLARE SUB DrawBall (x%, y%, c%)
DECLARE SUB DrawNeedles ()
DECLARE SUB DrawStars ()
DECLARE SUB DrawText ()
DECLARE SUB DrawTrunk ()

' Adapted from ChristmasTree2.bas sample from micro(A)

DIM SHARED bpx%, bpy%

bpx% = 220: bpy% = 410

SCREEN 12

SetPalette


DrawText
DrawStars
DrawTrunk
DrawNeedles
DrawBall 190, 100, 14
DrawBall 175, 165, 5
DrawBall 250, 180, 12
DrawBall 140, 225, 9
DrawBall 270, 245, 11
DrawBall 90, 302, 13
DrawBall 295, 302, 13

SUB DrawBall (x%, y%, c%)
COLOR c%
FOR r% = 2 TO 6 STEP 2
  CIRCLE (x%, y%), r%
NEXT r%
END SUB

SUB DrawNeedles
'green...............................

COLOR 2

tpx% = bpx%: tpy% = bpy% - 40
rad% = 160

FOR ht% = 1 TO 39
  FOR xs% = -100 TO 100 STEP 40
    xshorten! = xs% / 100!
    rise! = RND * .93
    tpxx! = tpx% + xshorten! * rad%: tpyy! = tpy% - rise! * rad%

    LINE (tpx%, tpy%)-(tpxx!, tpyy!)

    FOR aa% = 1 TO 29
      frad! = RND * .9 * rad%
      x1% = tpx% + xshorten! * frad!
      y1% = tpy% - rise! * frad!
      x2% = tpx% + xshorten! * (frad! + rad% / 5)
      y2% = tpy% - rise! * frad! + (-rise! + (RND * .9 - .456)) * (rad% / 5)
      LINE (x1%, y1%)-(x2%, y2%)
    NEXT aa%
  NEXT xs%

  rad% = rad% - 4
  tpy% = tpy% - 9
NEXT ht%
END SUB

SUB DrawStars
PAINT (0, 0), 1

FOR x% = 10 TO 400 STEP 4
  px% = RND * 640
  py% = RND * 480

  PSET (px%, py%), 15
NEXT x%
END SUB

SUB DrawText
COLOR 7
LOCATE 3, 38: PRINT " * * * Merry Christmas ! * * * "
LOCATE 6, 38: PRINT " * * * Happy New Year  ! * * * "
LOCATE 9, 38: PRINT "             QBX               "

FOR r% = 3 TO 9 STEP 3
  FOR y% = r% * 16 - 16 TO r% * 16 - 1
    FOR x% = 296 TO 543
      IF POINT(x%, y%) = 0 THEN PSET (x%, y%), 1
    NEXT x%
  NEXT y%
NEXT r%
END SUB

SUB DrawTrunk
'brown ..............................
COLOR 6
FOR aa% = -4 TO 3
  LINE (bpx% + aa%, bpy%)-(bpx%, bpy% - 390)
NEXT aa%
END SUB

SUB SetPalette
SetPaletteEntry 1, 0, 0, 20
SetPaletteEntry 2, 7, 30, 10
SetPaletteEntry 6, 32, 25, 0

SetPaletteEntry 14, 55, 50, 25
SetPaletteEntry 12, 62, 25, 25
SetPaletteEntry 5, 37, 25, 62
SetPaletteEntry 9, 20, 37, 63
SetPaletteEntry 11, 50, 37, 63
SetPaletteEntry 13, 62, 45, 63
END SUB

SUB SetPaletteEntry (i%, r%, g%, b%)
OUT &H3C8, i%
OUT &H3C9, r%
OUT &H3C9, g%
OUT &H3C9, b%
END SUB

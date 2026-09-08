DECLARE SUB setpalette ()
DECLARE SUB expandcolors (img%())
DECLARE SUB createimg (img%())
DECLARE SUB smooth (img%())
'$DYNAMIC
SCREEN 13
LOCATE 1, 1
PRINT "Please wait.. initializing..";
DIM px%(180, 150), py%(180, 150), img%(16200), dblbuf&(8101)
createimg img%()
PRINT ".";
FOR i% = 0 TO 180
 FOR j% = 0 TO 150
  px%(i%, j%) = -1
  py%(i%, j%) = -1
 NEXT j%
NEXT i%
DEF SEG = &HA000
FOR i! = 0 TO -90 STEP -.5
 FOR j! = -90 TO 90 STEP .5
  thisx% = 160 + COS(j! / 57.295779#) * SIN(i! / 57.295779#) * 90
  thisy% = 100 - SIN(j! / 57.295779#) * 75
  IF PEEK(thisx% + 320& * thisy%) <> 16 THEN
   POKE thisx% + 320& * thisy%, 16
   px%(thisx% - 70, thisy% - 25) = i! + 90
   py%(thisx% - 70, thisy% - 25) = j! + 90
  END IF
 NEXT j!
 IF (i! * 2) MOD 60 = 0 THEN PRINT ".";
NEXT i!
FOR i! = .5 TO 90 STEP .5
 FOR j! = -90 TO 90 STEP .5
  thisx% = 160 + COS(j! / 57.295779#) * SIN(i! / 57.295779#) * 90
  thisy% = 100 - SIN(j! / 57.295779#) * 75
  IF PEEK(thisx% + 320& * thisy%) <> 16 THEN
   POKE thisx% + 320& * thisy%, 16
   px%(thisx% - 70, thisy% - 25) = i! + 90
   py%(thisx% - 70, thisy% - 25) = j! + 90
  END IF
 NEXT j!
 IF (i! * 2) MOD 60 = 0 THEN PRINT ".";
NEXT i!
GET (70, 25)-(250, 175), dblbuf&
CLS
setpalette
txofs% = 0
imgseg% = VARSEG(img%(0)): imgptr% = VARPTR(img%(0))
bufseg% = VARSEG(dblbuf&(1)): bufptr% = VARPTR(dblbuf&(1))
DEF SEG = imgseg%
phase! = 0
thisx% = 0
thisy% = 0
DO
 txofs% = (txofs% + 4) MOD 180
 tyofs% = (tyofs% + 4) MOD 180
 distphase! = distphase! + .15
 IF distphase! > 6.283185 THEN distphase! = distphase! - 6.283185
 distfactor% = SIN(distphase!) * 25 + 125
 o& = -1820
 da! = da! + .1
 IF da! > 6.283185 THEN da! = da! - 6.283185
 a! = a! + SIN(da!) / 5
 IF a! > 6.283185 THEN a! = a! - 6.283185
 IF a! < 0 THEN a! = a! + 6.283185
 cosa% = COS(a!) * 100
 sina% = SIN(a!) * 100
 FOR j% = -10 TO 160
  FOR i% = -10 TO 190
   midi% = (i% - 90) * distfactor% \ 100
   midj% = (j% - 75) * distfactor% * 6& \ 500
   reali% = (midi% * cosa% - midj% * sina%) \ 100 + 90
   realj% = (midi% * sina% + midj% * cosa%) \ 120 + 75
   IF (reali% >= 0) AND (reali% <= 180) AND (realj% >= 0) AND (realj% <= 150) THEN
    thisx% = px%(reali%, realj%)
    thisy% = py%(reali%, realj%)
    IF (thisx% >= 0) AND (thisy% >= 0) THEN
     thisx% = (thisx% + txofs%) MOD 180
     thisy% = (thisy% + tyofs%) MOD 180
     IF (thisx% > 179) OR (thisy% > 179) THEN BEEP
     thisc% = PEEK(thisx% + thisy% * 180 + imgptr%)
    ELSE
     thisc% = 0
    END IF
   ELSE
    thisc% = 0
   END IF
   IF (o& >= 0) AND (o& < 27800) AND ((o& - j% * 181) >= 0) THEN
    DEF SEG = bufseg%
    POKE o& + bufptr%, thisc%
    DEF SEG = imgseg%
   END IF
   o& = o& + 1
  NEXT i%
  o& = o& - 20
 NEXT j%
 phase! = phase! + .1
 IF phase! > 6.283185 THEN phase! = phase! - 6.283185
 thisx% = COS(phase!) * 69 + 70
 thisy% = SIN(phase! * 2) * 24 + 25
 IF thisx% < otx% THEN
  LINE (thisx% + 181, oty%)-(otx% + 181, oty% + 150), 0, BF
 ELSEIF thisx% > otx% THEN
  LINE (otx%, oty%)-(thisx% - 1, oty% + 150), 0, BF
 END IF
 IF thisy% < oty% THEN
  LINE (otx%, thisy% + 151)-(otx% + 180, oty% + 151), 0, BF
 ELSEIF thisy% > oty% THEN
  LINE (otx%, oty%)-(otx% + 180, thisy% - 1), 0, BF
 END IF
 PUT (thisx%, thisy%), dblbuf&, PSET
 otx% = thisx%: oty% = thisy%
LOOP UNTIL INKEY$ <> ""
SCREEN 0: WIDTH 80
PRINT "SPHRTMP3.EXE - Real-time texture-mapped sphere with rotation & zooming by"
PRINT "               logiclrd in '98"

REM $STATIC
SUB createimg (img%())
DEF SEG = VARSEG(img%(0))
p% = VARPTR(img%(0))
FOR i% = p% TO 32399 + p%
 thisi% = RND * RND * RND * RND * 127
 IF INT(RND * 2) = 1 THEN thisi% = 255 - thisi%
 POKE i%, thisi%
NEXT i%
smooth img%()
smooth img%()
smooth img%()
smooth img%()
smooth img%()
expandcolors img%()
smooth img%()
smooth img%()
smooth img%()
expandcolors img%()
END SUB

SUB expandcolors (img%())
DEF SEG = VARSEG(img%(0))
p% = VARPTR(img%(0))
min% = 255
max% = 0
FOR i% = p% TO p% + 32399
 a% = PEEK(i%)
 IF a% < min% THEN min% = a%
 IF a% > max% THEN max% = a%
 b% = a%
NEXT i%
range% = max% - min%
FOR i% = p% TO p% + 32399
 POKE i%, (PEEK(i%) - min%) * 240& \ range%
NEXT i%
END SUB

SUB setpalette
FOR i% = 0 TO 94
 OUT &H3C8, i%
 OUT &H3C9, 0
 OUT &H3C9, 0
 OUT &H3C9, i% \ 2
NEXT i%
FOR i% = 95 TO 127
 OUT &H3C8, i%
 OUT &H3C9, (i% - 94) * 3 \ 2
 OUT &H3C9, (i% - 94) * 3 \ 2
 OUT &H3C9, i% \ 2
NEXT i%
FOR i% = 128 TO 255
 OUT &H3C8, i%
 OUT &H3C9, (255 - i%) \ 8
 OUT &H3C9, (i% - 128) \ 4 + 31
 OUT &H3C9, 0
NEXT i%
END SUB

SUB smooth (img%())
DEF SEG = VARSEG(img%(0))
p% = VARPTR(img%(0))
POKE p%, (PEEK(p% + 32219) + PEEK(p% + 32220) + PEEK(p% + 32221) + PEEK(p% + 32399) + PEEK(p%) + PEEK(p% + 1) + PEEK(p% + 179) + PEEK(p% + 180) + PEEK(p% + 181)) \ 9
FOR i% = p% + 1 TO p% + 178
 POKE i%, (PEEK(i% + 32219) + PEEK(i% + 32220) + PEEK(i% + 32221) + PEEK(i% - 1) + PEEK(i%) + PEEK(i% + 1) + PEEK(i% + 179) + PEEK(i% + 180) + PEEK(i% + 181)) \ 9
NEXT i%
POKE p% + 179, (PEEK(p% + 32219) + PEEK(p% + 32220) + PEEK(p%) + PEEK(p% + 178) + PEEK(p% + 179) + PEEK(p% + 180) + PEEK(p% + 358) + PEEK(p% + 359) + PEEK(p% + 360)) \ 9
POKE p% + 180, (PEEK(p% + 32219) + PEEK(p%) + PEEK(p% + 1) + PEEK(p% + 179) + PEEK(p% + 180) + PEEK(p% + 181) + PEEK(p% + 359) + PEEK(p% + 360) + PEEK(p% + 361)) \ 9
FOR i% = p% + 181 TO p% + 32218
 POKE i%, (PEEK(i% - 181) + PEEK(i% - 180) + PEEK(i% - 179) + PEEK(i% - 1) + PEEK(i%) + PEEK(i% + 1) + PEEK(i% + 179) + PEEK(i% + 180) + PEEK(i% + 181)) \ 9
NEXT i%
POKE p% + 32219, (PEEK(p% + 32038) + PEEK(p% + 32039) + PEEK(p% + 32040) + PEEK(p% + 32218) + PEEK(p% + 32219) + PEEK(p% + 32220) + PEEK(p% + 32398) + PEEK(p% + 32399) + PEEK(p%)) \ 9
POKE p% + 32220, (PEEK(p% + 32039) + PEEK(p% + 32040) + PEEK(p% + 32041) + PEEK(p% + 32219) + PEEK(p% + 32220) + PEEK(p% + 32221) + PEEK(p% + 32399) + PEEK(p%) + PEEK(p% + 1)) \ 9
FOR i% = p% + 32221 TO p% + 32398
 POKE i%, (PEEK(i% - 181) + PEEK(i% - 180) + PEEK(i% - 179) + PEEK(i% - 1) + PEEK(i%) + PEEK(i% + 1) + PEEK(i% - 32221) + PEEK(i% - 32220) + PEEK(i% - 32219)) \ 9
NEXT i%
POKE p% + 32399, (PEEK(p% + 32218) + PEEK(p% + 32219) + PEEK(p% + 32220) + PEEK(p% + 32398) + PEEK(p% + 32399) + PEEK(p%) + PEEK(p% + 178) + PEEK(p% + 179) + PEEK(p% + 180)) \ 9
END SUB


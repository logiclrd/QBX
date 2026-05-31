DECLARE SUB FadeOut12 ()
DECLARE SUB FadeOut13 ()
DECLARE SUB SetPalette ()

' Alien Fireworks
' Written by K Moerman for BBC Basic 2026
' (loosely inspired by work of Simone Conradi https://fb.watch/Hqr6gkxun5/)
' Translated to QuickBASIC by Jonathan Gilbert

CONST Niter% = 1500  ' N iterations with same parameters
CONST Nparam% = 1600 ' N values parameters 0..2*PI

CONST ScreenMode% = 13

DIM SHARED palettebrightnessleveloffset%

SCREEN ScreenMode%

SELECT CASE ScreenMode%
  CASE 12: basecolour% = 9: palettebrightnessleveloffset% = 8
  CASE 13: basecolour% = 246: palettebrightnessleveloffset% = 6: SetPalette
END SELECT

DIM coloursequence%(0 TO 6)

WINDOW (-1, -1)-(1, 1) ' put coordinates x=0, y=0 to center of image

CONST PI! = 3.141593
CONST PI2! = 6.283185

CONST stepab! = PI2! / Nparam%

colourindex! = 6 ' trigger colour sequence immediately
coloursequence%(6) = basecolour%

x! = 0!: y! = 0!
DO
  FOR a! = PI2! TO 0 STEP -stepab!
    FOR b! = 0 TO PI2! STEP stepab!
      SELECT CASE ScreenMode%
        CASE 12: FadeOut12
        CASE 13: FadeOut13
      END SELECT

      colourindex! = colourindex! + .2
      IF colourindex! > 6 THEN
        colourindex! = colourindex! - 6
        coloursequence%(0) = coloursequence%(6)
        FOR i% = 1 TO 6
          coloursequence%(i%) = FIX(RND * 6) + basecolour%
        NEXT i%
      END IF
      FOR t% = 1 TO Niter%
        xnew! = SIN(x! * x! - y! * y! + a!)' iterate x and y through map formulas
        y! = SIN(2 * x! * y! + b!)' x and y are within -1 to +1
        x! = xnew!
        PSET (x!, y!), coloursequence%(FIX(colourindex! + RND))
      NEXT
      SWAP x, y' next initial values are swapped end values of x,y
      IF INKEY$ <> "" THEN EXIT DO
    NEXT
  NEXT
LOOP

SUB FadeOut12
' SCREEN 12 gives us 16 colours to work with.
' The second 8 are bright versions of the first 8.
' That's not much to work with to simulate old output fading away :-)
FOR i% = 1 TO 750
  x! = RND * 2 - 1
  y! = RND * 2 - 1
  c% = POINT(x!, y!)
  IF c% >= 8 THEN c% = c% - 8 ELSE c% = 0
  PSET (x!, y!), c%
NEXT i%
END SUB

SUB FadeOut13
STATIC dither%

' SCREEN 13 gives us 256 colours. I use them in much
' the same way as the SCREEN 12 palette, just with
' many more brightness levels for a smoother fade out.

DEF SEG = &HA000

SELECT CASE dither%
  CASE 0: dither% = 321
  CASE 321: dither% = 1
  CASE 1: dither% = 320
  CASE 320: dither% = 0
END SELECT

FOR o& = dither% TO 63999 STEP 640
  FOR t% = 0 TO 319 STEP 2
    c% = PEEK(o& + t%)
    IF RND * 246 < c% THEN ' fade more slowly when already dark
      c% = c% - palettebrightnessleveloffset%
      IF c% < 0 THEN c% = 0
    END IF
    POKE o& + t%, c%
  NEXT t%
NEXT o&
END SUB

SUB SetPalette
' The core rendering dithers across a 6-colour palette
' of hues. In SCREEN 13, we get 256 colours to play
' with, so we repeat that palette every 6 colours to
' fade gradually.

DIM r%(8), g%(8), b%(8)

r%(0) = 31: g%(0) = 31: b%(0) = 53
r%(1) = 31: g%(1) = 53: b%(1) = 53
r%(2) = 31: g%(2) = 53: b%(2) = 31
r%(3) = 53: g%(3) = 53: b%(3) = 31
r%(4) = 53: g%(4) = 31: b%(4) = 31
r%(5) = 53: g%(5) = 31: b%(5) = 53

OUT &H3C8, 0

FOR brightness% = 0 TO 41
  FOR c% = 0 TO 5
    OUT &H3C9, r%(c%) * brightness% \ 41
    OUT &H3C9, g%(c%) * brightness% \ 41
    OUT &H3C9, b%(c%) * brightness% \ 41
  NEXT c%
NEXT brightness%
END SUB

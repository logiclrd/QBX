DECLARE SUB SetPalette ()

'specBas effect in micro(A)-by Aurel, translated to QuickBASIC by Jonathan Gilbert

SCREEN 13

SetPalette

TAU! = 6.283185
PI! = TAU! * .5

hw! = 200 / 2
hh! = 200 / 2
n% = 180
r! = TAU! / PI!

FOR t% = 1 TO 50
  FOR i% = 10 TO n% - 1
    FOR j% = 1 TO n% - 1
      u! = SIN(i% + v!) + SIN(r! * i% + x!)
      v! = COS(i% + v!) + COS(r! * i% + x!)

      x! = u! + t%

      ii% = i% * 15 \ 255
      jj% = j% * 15 \ 255

      COLOR ii% * 16 + jj%
      PSET (60 + hw! + u! * hw! * .4, hh! + v! * hh! * .4)
    NEXT j%
    st! = TIMER: WHILE st! = TIMER: WEND
    IF INKEY$ <> "" THEN END
  NEXT i%
NEXT t%

SUB SetPalette
FOR i% = 0 TO 15
  FOR j% = 0 TO 15
    OUT &H3C8, i% * 16 + j%
    OUT &H3C9, i% * 63 \ 15
    OUT &H3C9, j% * 63 \ 15

    IF i% = 0 AND j% = 0 THEN OUT &H3C9, 0 ELSE OUT &H3C9, 49
  NEXT j%
NEXT i%
END SUB

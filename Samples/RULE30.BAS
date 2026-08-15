' This program draws the Rule 30 Elementary Cellular Automaton. It
' increments the seed each frame, generating a visual similar to that
' seen in the YouTube video "The Hidden Structure of Rule 30" by Eric
' Rowland (https://youtube.com/watchv=HcGjCcLEgN4).

SCREEN 13

' Make colour 1 a similar blue to that used in the video. :-)
OUT &H3C8, 1
OUT &H3C9, 20
OUT &H3C9, 41
OUT &H3C9, 63

DEF SEG = &HA000

DIM seed%(321)
DIM roweven%(321), rowodd%(321)

DO
  st! = TIMER

  ' Advance the seed. This is just simple binary addition with carry.
  ' Technically, if you run this code long enough, eventually it will
  ' crash -- when you hit seed 2^320. This will happen after a bit
  ' less than 2 * 10^88 years -- beware!
  seed%(2) = seed%(2) + 1

  i% = 2
  WHILE seed%(i%) = 2
    seed%(i%) = 0
    i% = i% + 1
    seed%(i%) = seed%(i%) + 1
  WEND

  FOR j% = 0 TO 321
    IF seed%(j%) THEN EXIT FOR
  NEXT j%

  FOR i% = 0 TO 321 - j% + 1
    roweven%(i%) = seed%(i% + j% - 1)
  NEXT i%
  FOR i% = 321 - j% + 2 TO 321
    roweven%(i%) = 0
  NEXT i%

  ' Fill the screen
  o& = 0
  FOR y% = 0 TO 199 STEP 2
    ' Even -> Odd
    FOR x% = 2 TO 321
      POKE o&, roweven%(x%): o& = o& + 1
      rowodd%(x%) = roweven%(x% - 2) XOR (roweven%(x% - 1) OR roweven%(x%))
    NEXT x%

    ' Odd -> Even
    FOR x% = 2 TO 321
      POKE o&, rowodd%(x%): o& = o& + 1
      roweven%(x%) = rowodd%(x% - 2) XOR (rowodd%(x% - 1) OR rowodd%(x%))
    NEXT x%
  NEXT y%

  FOR tick% = 1 TO 5
    WHILE st! = TIMER: WEND
    st! = TIMER
  NEXT tick%
LOOP WHILE INKEY$ = ""

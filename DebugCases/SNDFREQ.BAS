PLAY "mb ml t32 c5 c5 c5"

FOR a# = 0 TO 30 STEP .1
  frequency# = COS(a#) * 30 + 120

  divisor& = 1193181 / frequency#
  
  OUT &H43, &HB6
  OUT &H42, divisor& AND 255
  OUT &H42, divisor& \ 256

  FOR i& = 1 TO 100000: NEXT i&
  PRINT ".";
NEXT a#

SOUND 100, 1

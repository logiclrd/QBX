PLAY "mb ml t32 c5 c5"

FOR a# = 0 TO 16 STEP .2
  frequency# = COS(a#) * 30 + 120

  divisor& = 1193181 / frequency#
  
  OUT &H43, &HB6
  OUT &H42, divisor& AND 255
  OUT &H42, divisor& \ 256

  PRINT ".";

  t! = TIMER
  WHILE TIMER = t!: WEND
NEXT a#

TYPE RegTypeX
  AX AS INTEGER
  BX AS INTEGER
  CX AS INTEGER
  DX AS INTEGER
  BP AS INTEGER
  SI AS INTEGER
  DI AS INTEGER
  FLAGS AS INTEGER
  DS AS INTEGER
  ES AS INTEGER
END TYPE

DECLARE SUB InterruptX (i%, rin AS RegTypeX, rout AS RegTypeX)

DIM rin AS RegTypeX, rout AS RegTypeX

SCREEN 13

rin.AX = 13 ' Enable light pen emulation

CALL InterruptX(&H33, rin, rout)

rin.AX = 1 ' Show cursor

CALL InterruptX(&H33, rin, rout)

'PEN ON

enabled% = -1

DO
  rin.AX = 2 ' Hide cursor

  CALL InterruptX(&H33, rin, rout)

  CLS 

  FOR i% = 0 TO 9
    PRINT i%; ":"; PEN(i%)
  NEXT i%

  rin.AX = 1 ' Show cursor

  CALL InterruptX(&H33, rin, rout)

  st# = TIMER
  WHILE st# = TIMER: WEND

  IF INKEY$ <> "" THEN
    enabled% = NOT enabled%

    rin.AX = 14 + enabled% ' 13 = enable, 14 = disable light pen emulation

    CALL InterruptX(&H33, rin, rout)
  END IF
LOOP


CONST PI! = 3.141593

ap% = 1

f! = .6

DO
  SWAP ap%, vp%
  SCREEN 7, , ap%, vp%

  f! = f! + .005
  IF f! > 2 * PI! THEN f! = f! - 2 * PI!

  a! = SIN(f!) + 1
  IF a! > 1 THEN a! = 1 / (2 - a!)

  CLS
  CIRCLE (160, 100), 50, 15, , , a!
  PAINT (160, 100), 9, 15
LOOP WHILE INKEY$ = ""

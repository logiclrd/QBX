' Curlicue fractal - adapted from the micro(A) sample curcilue.bas
x! = 270
y! = 300

df! = .19
f! = 0

SCREEN 12
COLOR 9

WHILE f! < 2000
  x! = ABS(-x! + COS(f! * f!) / .4)
  y! = ABS(-y! + SIN(f! * f!) / .4)
  PSET (x!, y!)

  f! = f! + df!
WEND

COLOR 7
LOCATE 1, 29
PRINT "curlicue fractal in QB"

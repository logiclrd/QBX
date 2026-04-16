m% = 9

SCREEN m%, , 1, 0

FOR i% = 0 TO 100
  LINE (RND * 640, RND * 350)-(RND * 640, RND * 350), RND * 8
NEXT i%

SLEEP

SCREEN m%, , 0, 1

FOR i% = 0 TO 100
  LINE (RND * 640, RND * 350)-(RND * 640, RND * 350), RND * 8 + 8
NEXT i%

SLEEP

SCREEN m%, , 1, 0

SLEEP

SCREEN m%, , 0, 1

SLEEP

SCREEN m%, , 1, 0


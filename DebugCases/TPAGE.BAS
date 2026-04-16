m% = 0

SCREEN m%, , 1, 0

FOR i% = 0 TO 1999
  PRINT CHR$(65 + RND * 20);
NEXT i%

SLEEP

SCREEN m%, , 0, 1

FOR i% = 0 TO 1999
  PRINT CHR$(105 + RND * 20);
NEXT i%

SLEEP

SCREEN m%, , 1, 0

SLEEP

SCREEN m%, , 0, 1

SLEEP

SCREEN m%, , 1, 0


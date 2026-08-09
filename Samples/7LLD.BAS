SCREEN 13
1 Counter& = Counter& + 1
FOR I = 0 TO 512
 IF Counter& = 1 AND (I AND 1) THEN PALETTE I \ 2, 65536 * (I \ 8)
 PSET ((COS(.0174 * (I * .7050781)) * (Counter& MOD 800 \ 4)) + 159, (SIN(.0174 * (I * .7)) * (Counter& MOD 800 \ 4)) + 100), ((POINT((COS(.0174 * (I * .7)) * (Counter& MOD 800 \ 4)) + 159, (SIN(.0174 * (I * .7)) * (Counter& MOD 800 \ 4)) + 100) +  _
POINT((COS(.0174 * (I * .7)) * (Counter& MOD 800 \ 4)) + 161, (SIN(.0174 * (I * .7)) * (Counter& MOD 800 \ 4)) + 100) + POINT((COS(.0174 * (I * .7)) * (Counter& MOD 800 \ 4)) + 160, (SIN(.0174 * (I * .7)) * (Counter& MOD 800 \ 4)) + 99) + POINT((COS _
(.0174 * (I * .7)) * (Counter& MOD 800 \ 4)) + 160, (SIN(.0174 * (I * .7)) * (Counter& MOD 800 \ 4)) + 101)) \ 4 + ((Counter&) * -(Counter& < 200)) * 8) + 5
NEXT
IF INKEY$ = "" THEN 1


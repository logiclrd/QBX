LO$ = CHR$(220)
HI$ = CHR$(223)

C134$ = HI$ + LO$
C137$ = LO$ + HI$

CLS

RANDOMIZE TIMER

X0 = 11: Y0 = 11

FOR R = 0 TO 11
  FOR A = -R TO R
    S = TIMER
    FOR B = -R TO R
      IF A * A + B * B <= R * R THEN
        X = X0 + A: Y = Y0 + B

        IF X >= 0 AND X <= 21 AND Y >= 0 AND Y <= 21 THEN
          C = 1 + INT(RND * 8)
          COLOR C
          LOCATE Y + 1, 2 * (5 + X): PRINT C137$;
          LOCATE Y + 1, 2 * (26 - X): PRINT C134$;
          LOCATE 22 - Y, 2 * (5 + X): PRINT C134$;
          LOCATE 22 - Y, 2 * (26 - X): PRINT C137$;
        END IF
      END IF
    NEXT B
    WHILE TIMER = S: WEND
  NEXT A
NEXT R

LOCATE 24, 1
COLOR 7


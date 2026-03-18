LO$ = CHR$(220)
HI$ = CHR$(223)

C134$ = HI$ + LO$
C137$ = LO$ + HI$

CLS

SetZXSpectrumPalette

RANDOMIZE TIMER

X0 = 11: Y0 = 11

FOR R = 0 TO 11
  FOR A = -R TO R
    S = TIMER
    FOR B = -R TO R
      IF A * A + B * B <= R * R THEN
        X = X0 + A: Y = Y0 + B

        IF X >= 0 AND X <= 21 AND Y >= 0 AND Y <= 21 THEN
          C = 8 + INT(RND * 8)
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

SUB SetZXSpectrumPalette

C% = 0

FOR hh% = 0 TO 1
  FOR gg% = 0 TO 1
    FOR rr% = 0 TO 1
      FOR bb% = 0 TO 1
        R% = rr% * (54 + 9 * hh%)
        G% = gg% * (54 + 9 * hh%)
        B% = bb% * (54 + 9 * hh%)
        GOSUB SetColour
      NEXT bb%
    NEXT rr%
  NEXT gg%
NEXT hh%

EXIT SUB

SetColour:
  PALETTE C%, C%

  OUT &H3C8, C%
  OUT &H3C9, R%
  OUT &H3C9, G%
  OUT &H3C9, B%

  C% = C% + 1

  RETURN
END SUB

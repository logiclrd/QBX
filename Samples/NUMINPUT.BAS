PRINT "Input: ";

LOCATE , , 1

accum& = 0
d% = 0
DO
  a$ = INKEY$
  IF a$ <> "" THEN
    c% = ASC(a$)
    SELECT CASE c%
      CASE 48 TO 57
        e% = c% - 48
        IF d% = 1 AND accum& = 0 THEN
          PRINT CHR$(8); a$;
          accum& = e%
        ELSEIF (e% > 0 OR d% = 0 OR accum& <> 0) AND (d% < 9 OR (d% = 9 AND (accum& < 214748364 OR (accum& = 21474364 AND e% < 8)))) THEN
          d% = d% + 1
          accum& = accum& * 10 + e%
          PRINT a$;
        END IF
      CASE 8:
        IF d% > 0 THEN
          accum& = accum& \ 10
          d% = d% - 1
          PRINT a$; " "; a$; ' caveat: won't return to previous line
        END IF
      CASE 13:
        EXIT DO
    END SELECT
  END IF
LOOP

LOCATE , , 0

PRINT
PRINT "Your input: "; accum&

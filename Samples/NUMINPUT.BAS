PRINT "Input: ";

LOCATE , , 1

accum& = 0
d% = 0
DO
  a$ = INKEY$
  reject% = 0
  IF a$ <> "" THEN
    c% = ASC(a$)
    SELECT CASE c%
      CASE 48 TO 57
        e% = c% - 48
        IF d% = 1 AND accum& = 0 THEN
          IF POS(0) > 1 THEN LOCATE , POS(0) - 1
          PRINT a$;
          accum& = e%
        ELSEIF (e% > 0 OR d% = 0 OR accum& <> 0) AND (d% < 9 OR (d% = 9 AND (accum& < 214748364 OR (accum& = 214748364 AND e% < 8)))) THEN
          d% = d% + 1
          accum& = accum& * 10 + e%
          PRINT a$;
        ELSE
          reject% = 1
        END IF
      CASE 8:
        IF d% > 0 THEN
          accum& = accum& \ 10
          d% = d% - 1
          ' caveat: won't return to previous line
          IF POS(0) > 1 THEN LOCATE , POS(0) - 1
          PRINT " ";
          IF POS(0) > 1 THEN LOCATE , POS(0) - 1
        END IF
      CASE 13:
        EXIT DO
      CASE ELSE
        reject% = 1
    END SELECT

    IF reject% THEN
      x% = POS(0): y% = CSRLIN

      COLOR , 4
      PRINT a$;
      LOCATE y%, x%

      st! = TIMER: WHILE st! = TIMER: WEND

      COLOR , 0
      PRINT " ";
      LOCATE y%, x%
    END IF
  END IF
LOOP

LOCATE , , 0

PRINT
PRINT "Your input: "; accum&

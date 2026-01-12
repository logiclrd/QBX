FOR i = 1 TO 10
  SELECT CASE i
    CASE 1: PRINT "one"
    CASE 2 TO 4, 7: PRINT "two three four seven"
    CASE IS >= 8: PRINT "greater than eight"
    CASE ELSE: PRINT "everything else"
  END SELECT
NEXT i

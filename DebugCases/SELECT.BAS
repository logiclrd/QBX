FOR i = 1 TO 5
  SELECT CASE i
    CASE 1: PRINT "one"
    CASE 2 TO 4: PRINT "two three four"
    CASE IS >= 8: PRINT "greater than eight"
    CASE ELSE: PRINT "everything else"
  END SELECT
NEXT i

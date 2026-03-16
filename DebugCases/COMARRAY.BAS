'$STATIC
COMMON z%()

DIM z%(2 TO 5)

PRINT UBOUND(z%); "should be 5"

ERASE z%

ON ERROR RESUME NEXT
PRINT UBOUND(z%); "should have been an error"
e% = ERR
ON ERROR GOTO 0

IF e% <> 9 THEN
  ERROR 5 ' should have generated "Subscript out of range"
ELSE
  PRINT "Correct error generated"
END IF

DIM z%(6 TO 10)

PRINT UBOUND(z%); "should be 10"


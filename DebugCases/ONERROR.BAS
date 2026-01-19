ON ERROR GOTO foo

ERROR 5
PRINT "zappo"

foo: 
  PRINT "hi"
  RESUME NEXT

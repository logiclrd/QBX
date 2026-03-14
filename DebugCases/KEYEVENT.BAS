ON KEY(8) GOSUB foo

KEY(8) STOP

st# = TIMER + 3
WHILE TIMER < st#: WEND

KEY(8) ON

st# = TIMER + 3
WHILE TIMER < st#: WEND

END

foo: 
  PRINT "hello"
  RETURN 

DECLARE SUB SetUEvent

ON UEVENT GOSUB foop

UEVENT ON

SetUEvent

END

foop:
  PRINT "got it!"
  RETURN

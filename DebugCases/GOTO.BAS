PRINT "first"

n% = -1

top: 
IF n% AND 1 = 0 THEN
  label1: PRINT "second A"
  GOTO label2
ELSE
  PRINT "second B"
  GOTO label3
END IF

label2: 
  n% = n% + 1
  PRINT "third"
  GOTO label4

label3: 
  n% = n% + 1
  PRINT "fourth"
  IF n% >= 100 THEN END
  GOTO label1

label4: 
  PRINT "fifth"
  GOTO top

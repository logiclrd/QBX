DECLARE SUB foo ()
DECLARE SUB bar (n%)

foo


foo

SUB bar (n%)
PRINT "a"; n%


      PRINT "b"; n%



PRINT "c"; n%

END SUB

SUB foo
bar 1

bar 2
END SUB


DECLARE FUNCTION DhryProc1& (a&, b&)
DECLARE FUNCTION DhryProc2& (a&)
DECLARE SUB PrintTimer (label$, t!)

' QuickBASIC Dhrystone-style benchmark FOR comparison with Dave Plummer's runs
' 100,000 iterations + automatic Dhrystones/sec calculation
CLS
PRINT "QuickBASIC Dhrystone-Style Benchmark"
PRINT "================================"

CONST Loops& = 100000

PRINT "Starting benchmark... (100,000 loops)"

st! = TIMER
PrintTimer "Time Start: ", st!

FOR i& = 1 TO Loops&
  arr1& = i&
  arr2& = i& * 2

  sum& = DhryProc1&(arr1&, arr2&)
  sum& = sum& + DhryProc2&(arr1&)
  IF sum& > 100000 THEN flag% = -1

  dotcount% = dotcount% + 1
  IF dotcount% = 1000 THEN PRINT "."; : dotcount% = 0
NEXT i&

PRINT

et! = TIMER
PrintTimer "Time End: ", et!

WHILE et! < st!: et! = et! + 86400: WEND

elapsed! = et! - st!

dhryPerSec! = Loops& / elapsed!

PRINT
PRINT USING "Loops performed:  ###,###"; Loops&
PRINT USING "Elapsed seconds:       ##.###"; elapsed!
PRINT USING "Dhrystones / sec: ###,###.###"; dhryPerSec!

FUNCTION DhryProc1& (a&, b&)
x& = a& + b&
y& = a& * 3
z& = b& - a&
DhryProc1& = x& + y& + z&
END FUNCTION

FUNCTION DhryProc2& (a&)
temp& = a& * 5
IF temp& > 1000 THEN temp& = temp& - 500
DhryProc2& = temp&
END FUNCTION

SUB PrintTimer (label$, t!)
h% = FIX(t! / 3600)
t! = t! - h% * 3600!
m% = FIX(t! / 60)
t! = t! - m% * 60!
s% = FIX(t!)

PRINT label$;

IF h% < 10 THEN PRINT "0";
PRINT LTRIM$(STR$(h%)); ":";
IF m% < 10 THEN PRINT "0";
PRINT LTRIM$(STR$(m%)); ":";
IF s% < 10 THEN PRINT "0";
PRINT LTRIM$(STR$(s%))
END SUB

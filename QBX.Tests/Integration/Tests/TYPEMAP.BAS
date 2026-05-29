' When crossing module boundaries, QuickBASIC does not care about
' the names of types or their fields. Types are compatible if they
' have fields of the same data types in the same order, period.
'
'
' TYPEMAP.BAS defines TYPE abcdef with INTEGER fields foo and bar.
' TYPEMAPB.BA2 defines TYPE qwerty with INTEGER fields dog and cat.
'
' In this test, a variable of type abcdef is passed to a SUB in
' TYPEMAPB.BA2, which receives it as type qwerty and copies the
' value from the first field into the second. The test passes if
' the result shows that the value flowed correctly in both directions.

TYPE abcdef
  foo AS INTEGER
  bar AS INTEGER
END TYPE

DIM zeep AS abcdef

zeep.foo = 42

sploof zeep

IF zeep.bar = 42 THEN
  OPEN "TESTOUT$" FOR OUTPUT AS #1
  PRINT #1, "PASS"
  CLOSE #1
END IF

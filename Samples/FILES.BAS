'$DYNAMIC

DIM samples$(50)

samples$(1) = DIR$("*.bas")

FOR i% = 2 TO UBOUND(samples$)
  samples$(i%) = DIR$

  IF samples$(i%) = "" THEN EXIT FOR
NEXT i%

REDIM PRESERVE samples$(i% - 1)

FOR i% = 1 TO UBOUND(samples$)
  PRINT RIGHT$("  " + i%, 3); ". "; samples$(i%)
NEXT i%

PRINT
PRINT "Enter number of file: ";

LINE INPUT selection$

selectedFile% = VAL(selection$)

PRINT

IF (selectedFile% < 1) OR (selectedFile% > UBOUND(samples$)) THEN
  PRINT "Out of range"
ELSE
  selectedFilePath$ = samples$(selectedFile%)

  PRINT "You selected: "; selectedFilePath$
  PRINT "First 10 lines:"

  OPEN selectedFilePath$ FOR INPUT AS #1

  ON ERROR RESUME NEXT

  FOR i% = 1 TO 10
    LINE INPUT #1, l$
    IF ERR THEN EXIT FOR
    PRINT USING "[##] "; i%;
    PRINT l$
  NEXT i%

  CLOSE #1
END IF


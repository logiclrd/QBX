10 REM Spoiled Spaghetti code
20 REM QB64PE 4.4.0/Win11 x64
100 FOR i = 1 TO 4
110    GOTO 2000
200    REM Bottom half of for i=201 to 205 loop
210    PRINT "Line 210: i = ", i
220 NEXT i
230 PRINT "End at 240"
240 END
250 REM Top half of for i=201 to 205 loop
260 FOR i = 201 TO 205
270    GOTO 200
2000    REM Bottom half of for i=1 to 4 loop
2010    PRINT "Line 2010: i=", i
2020 NEXT i
2030 PRINT "reached 2030"
2040 GOTO 250
2050 PRINT "End at 2060"
2060 END

SCREEN 7

DEF SEG = &H40

low% = PEEK(&H4C)
high% = PEEK(&H4D)

pagesize% = low% + 256 * high%

pagecount% = 65536 \ pagesize%

PRINT "Page size: "; pagesize%
PRINT "Page count:"; pagecount%

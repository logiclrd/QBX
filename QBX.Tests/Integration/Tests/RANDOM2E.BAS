TYPE RegTypeX
  AX AS INTEGER
  BX AS INTEGER
  CX AS INTEGER
  DX AS INTEGER
  BP AS INTEGER
  SI AS INTEGER
  DI AS INTEGER
  FLAGS AS INTEGER
  DS AS INTEGER
  ES AS INTEGER
END TYPE

DECLARE SUB InterruptX (i%, rin AS RegTypeX, rout AS RegTypeX)

DIM rin AS RegTypeX, rout AS RegTypeX

' Arrange
OPEN "random2e.tmp" FOR OUTPUT AS #1
PRINT #1, CHR$(3) + CHR$(0) + "abc";
CLOSE #1

OPEN "random2e.tmp" FOR RANDOM AS #1 LEN = 7

ON ERROR RESUME NEXT
GET #1, , a$
e% = ERR

rin.AX = &H4201 ' seek relative to current location
rin.BX = FILEATTR(1, 2)
rin.CX = 0 ' delta
rin.DX = 0 ' |

CALL InterruptX(&H21, rin, rout)

l& = rout.DX * 65536 + rout.AX

CLOSE #1

OPEN "TESTOUT$" FOR OUTPUT AS #1
IF l& = 5 THEN PRINT #1, "PASS"
CLOSE #1


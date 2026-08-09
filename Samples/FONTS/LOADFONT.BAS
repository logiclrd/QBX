'$DYNAMIC

DECLARE SUB refreshfont ()
DECLARE FUNCTION loadfont% (fontfile$)
DECLARE FUNCTION savefont% (fontfile$)

TYPE regtype
 ax AS INTEGER
 bx AS INTEGER
 cx AS INTEGER
 dx AS INTEGER
 bp AS INTEGER
 si AS INTEGER
 di AS INTEGER
 flags AS INTEGER
 ds AS INTEGER
 es AS INTEGER
END TYPE

DIM SHARED regs AS regtype, font AS STRING * 4096, oldfont AS STRING * 4096
CLS

COLOR 9, 0

DEF SEG = &HB800
p% = 2722
FOR i% = 0 TO 7
 FOR j% = 0 TO 31
  POKE i% * 160 + j% * 4 + 2730, i% * 32 + j%
  POKE i% * 160 + j% * 4 + 2731, 11
 NEXT j%
NEXT i%

numfiles% = 0

DIM filenames$(20)

f$ = DIR$("*.vga")

DO WHILE f$ <> ""
  IF numfiles% > UBOUND(filenames$) THEN
    REDIM PRESERVE filenames$(numfiles% * 2)
  END IF

  filenames$(numfiles%) = f$
  numfiles% = numfiles% + 1

  f$ = DIR$
LOOP

COLOR 9

LOCATE 16, 1
PRINT STRING$(80, 196);

scrolltop% = 0
selectedindex% = 0

visiblerows% = 14

DO
  a% = loadfont%(filenames$(selectedindex%))

  FOR i% = 1 TO visiblerows%
    index% = scrolltop% + (i% - 1)

    LOCATE i%, 31

    COLOR 7
    IF index% = selectedindex% THEN
      PRINT "-> ";
    ELSE
      PRINT "   ";
    END IF

    COLOR 9
    IF index% < numfiles% THEN
      PRINT filenames$(index%);
    ELSE
      PRINT "            ";
    END IF
  NEXT i%

  DO: k$ = INKEY$: LOOP WHILE k$ = ""

  IF k$ = CHR$(13) THEN EXIT DO
  IF k$ = CHR$(27) THEN EXIT DO

  IF k$ = CHR$(0) + "H" THEN
    selectedindex% = selectedindex% - 1
  ELSEIF k$ = CHR$(0) + "P" THEN
    selectedindex% = selectedindex% + 1
  ELSEIF k$ = CHR$(0) + "I" THEN
    selectedindex% = selectedindex% - visiblerows%
  ELSEIF k$ = CHR$(0) + "Q" THEN
    selectedindex% = selectedindex% + visiblerows%
  END IF

  IF selectedindex% < 0 THEN selectedindex% = 0
  IF selectedindex% < scrolltop% THEN scrolltop% = selectedindex%

  IF selectedindex% >= numfiles% THEN selectedindex% = numfiles% - 1
  IF selectedindex% >= scrolltop% + visiblerows% THEN scrolltop% = selectedindex% - visiblerows% + 1
LOOP

LOCATE 25, 1
COLOR 7
PRINT
PRINT

FUNCTION loadfont% (fontfile$)
 ON LOCAL ERROR RESUME NEXT

 OPEN fontfile$ FOR INPUT AS #1
 IF ERR THEN loadfont% = 1: EXIT FUNCTION
 CLOSE #1

 ON LOCAL ERROR GOTO 0

 OPEN fontfile$ FOR BINARY AS #1
 a$ = INPUT$(17, 1)
 IF a$ <> "fontedit 1.0 file" THEN loadfont% = 2: EXIT FUNCTION
 font = INPUT$(4096, 1)
 CLOSE #1
 refreshfont
END FUNCTION

SUB refreshfont
 fontseg% = VARSEG(font)
 fontofs% = VARPTR(font) + 1
 regs.es = fontseg%
 regs.bp = fontofs%
 regs.ax = &H1100
 regs.bx = &H1000
 regs.cx = 256
 regs.dx = 0
 CALL interruptx(&H10, regs, regs)
END SUB

FUNCTION savefont% (fontfile$)
 IF DIR$(fontfile$) <> "" THEN savefont% = 1: EXIT FUNCTION
 OPEN fontfile$ FOR OUTPUT AS #1
 PRINT #1, "fontedit 1.0 file"; font;
 CLOSE #1
END FUNCTION

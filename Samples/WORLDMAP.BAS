DECLARE SUB AppendDRAWCommand (buffer$, bufferlength%, c$)
DECLARE SUB ParseSVGWorldMap ()
DECLARE SUB ReadCoordinates (argument$, svgpath$, cstart%, cend%, lastx!, lasty!)
DECLARE SUB ReadLongLine (filenum%, buf$)
DECLARE SUB ShowOffDRAWCommands ()
DECLARE SUB TranslateCoordinates (svgx!, svgy!, scale!, offsetx!, offsety!, drawx!, drawy!)
DECLARE FUNCTION ExtractStringAttributeValue$ (a$)
DECLARE FUNCTION ForceSign$ (n!)
DECLARE FUNCTION TranslateSVGPathToDRAWCommand$ (svgpath$, scale!, offsetx!, offsety!)

' We have data we're calling a "continent" for extra detail, like
' "Canada". We know it's not really its own continent :-)
DIM SHARED Continents$(1 TO 9)

DIM SHARED ShapeContinents%(1 TO 360)
DIM SHARED ShapeDRAWCommands$(1 TO 360)
DIM SHARED NumShapes%

ParseSVGWorldMap

ShowOffDRAWCommands

SCREEN 12

FOR i% = 1 TO NumShapes%
  DRAW "C" + STR$(ShapeContinents%(i%))
  DRAW ShapeDRAWCommands$(i%)
NEXT i%

SLEEP

SUB AppendDRAWCommand (buffer$, bufferlength%, c$)
idx% = bufferlength% + 1

bufferlength% = bufferlength% + LEN(c$)

IF bufferlength% > LEN(buffer$) THEN buffer$ = buffer$ + SPACE$(1000)

MID$(buffer$, idx%, LEN(c$)) = c$
END SUB

FUNCTION ExtractStringAttributeValue$ (a$)
quote% = INSTR(a$, CHR$(34))
IF quote% > 0 THEN
  a$ = MID$(a$, quote% + 1)

  quote% = INSTR(a$, CHR$(34))
  IF quote% > 0 THEN
    a$ = LEFT$(a$, quote% - 1)

    ExtractStringAttributeValue$ = a$
  END IF
END IF
END FUNCTION

FUNCTION ForceSign$ (n!)
IF n! < 0 THEN ForceSign$ = STR$(n!) ELSE ForceSign$ = "+" + LTRIM$(STR$(n!))
END FUNCTION

SUB ParseSVGWorldMap
OPEN "WORLDMAP.SVG" FOR INPUT AS #1

PRINT "Loading & translating..."

' Not a general-purpose parser; this is written to the exact expectations of this particular WORLDMAP.SVG file.

ci% = 0
si% = 0

DO
  ReadLongLine 1, a$

  a$ = LTRIM$(RTRIM$(a$))

  IF newcontinent% THEN
    IF LEFT$(a$, 3) = "id=" THEN
      IF ci% = UBOUND(Continents$) THEN EXIT DO ' We've reached Borders, skip

      newcontinent% = 0

      ci% = ci% + 1
      Continents$(ci%) = ExtractStringAttributeValue$(a$)
    END IF
  ELSE
    IF LEFT$(a$, 6) = "width=" THEN
      canvaswidth! = VAL(ExtractStringAttributeValue$(a$))
    ELSEIF LEFT$(a$, 7) = "height=" THEN
      canvasheight! = VAL(ExtractStringAttributeValue$(a$))

      ' We now have width & height.
      scale1! = 640 / canvaswidth!
      scale2! = 480 / canvasheight!

      IF scale1! < scale2! THEN
        ' Letterbox
        scale! = scale1!
        offsety! = (480 - canvasheight! * scale!) / 2
      ELSE
        ' Pillarbox
        scale! = scale2!
        offsetx! = (640 - canvaswidth! * scale!) / 2
      END IF
    ELSEIF a$ = "<g" THEN
      newcontinent% = -1
    ELSEIF LEFT$(a$, 2) = "d=" THEN
      PRINT ".";

      si% = si% + 1

      svgpath$ = ExtractStringAttributeValue$(a$)

      l% = LEN(svgpath$)

      ShapeContinents%(si%) = ci%
      ShapeDRAWCommands$(si%) = TranslateSVGPathToDRAWCommand$(svgpath$, scale!, offsetx!, offsety!)
    END IF
  END IF
LOOP WHILE NOT EOF(1)

CLOSE #1

NumShapes% = si%
END SUB

SUB ReadCoordinates (argument$, svgpath$, cstart%, cend%, lastx!, lasty!)
comma% = INSTR(argument$, ",")
IF comma% > 0 THEN
  lastx! = lastx! + VAL(LEFT$(argument$, comma% - 1))
  lasty! = lasty! + VAL(MID$(argument$, comma% + 1))
ELSE
  ' Space-separated
  lastx! = lastx! + VAL(argument$)

  cstart% = cend% + 1
  cend% = INSTR(cstart%, svgpath$, " ")

  argument$ = MID$(svgpath$, cstart%, cend% - cstart%)

  lasty! = lasty! + VAL(argument$)
END IF
END SUB

SUB ReadLongLine (filenum%, buf$)
STATIC pushedback$

buf$ = SPACE$(256)
buflen% = 0

IF LEN(pushedback$) THEN
  MID$(buf$, 1, LEN(pushedback$)) = pushedback$
  buflen% = LEN(pushedback$)
  pushedback$ = ""
END IF

DO WHILE NOT EOF(filenum%)
  c$ = INPUT$(1, filenum%)

  IF ASC(c$) = 13 THEN
    ' Windows CRLF or Mac CR
    IF NOT EOF(filenum%) THEN
      c$ = INPUT$(1, filenum%)
      ' Discard LF from CRLF, keep anything else
      IF ASC(c$) <> 10 THEN pushedback$ = c$
    END IF

    EXIT DO
  ELSEIF ASC(c$) = 10 THEN
    ' UNIX style newline
    EXIT DO
  END IF

  buflen% = buflen% + 1

  IF buflen% > LEN(buf$) THEN buf$ = buf$ + SPACE$(256)

  MID$(buf$, buflen%, 1) = c$
LOOP

buf$ = LEFT$(buf$, buflen%)
END SUB

SUB ShowOffDRAWCommands
FOR i% = 1 TO NumShapes%
  dc$ = ShapeDRAWCommands$(i%)
  IF LEN(dc$) < 200 THEN PRINT dc$ ELSE PRINT LEFT$(dc$, 200); "..."
  IF (i% MOD 10) = 0 THEN
    st! = TIMER: WHILE TIMER = st!: WEND
  END IF
NEXT i%
PRINT
END SUB

SUB TranslateCoordinates (svgx!, svgy!, scale!, offsetx!, offsety!, drawx!, drawy!)
drawx! = svgx! * scale! + offsetx!
drawy! = svgy! * scale! + offsety!

' We are using DRAW scale factor S1, or 1:4
drawx! = INT(drawx! * 4)
drawy! = INT(drawy! * 4)
END SUB

FUNCTION TranslateSVGPathToDRAWCommand$ (svgpath$, scale!, offsetx!, offsety!)
drawcommand$ = "BM0,0S1"
drawcommandlength% = LEN(drawcommand$)

IF RIGHT$(svgpath$, 1) <> " " THEN svgpath$ = svgpath$ + " "

lastx! = 0
lasty! = 0

cstart% = 1
cend% = INSTR(svgpath$, " ")

isfirst% = -1
isclosed% = 0

WHILE cend% <> 0
  argument$ = MID$(svgpath$, cstart%, cend% - cstart%)

  a% = ASC(argument$)

  uppercaseletter% = (a% >= 65) AND (a% <= 90)
  lowercaseletter% = (a% >= 97) AND (a% <= 122)

  IF uppercaseletter% OR lowercaseletter% THEN
    svgcommand$ = argument$

    cstart% = cend% + 1
    cend% = INSTR(cstart%, svgpath$, " ")

    IF cend% = 0 THEN
      cend% = LEN(svgpath$) + 1
      argument$ = ""
    ELSE
      argument$ = MID$(svgpath$, cstart%, cend% - cstart%)
    END IF

    IF uppercaseletter% THEN
      ' Uppercase commands mean absolute coordinates
      lastx! = 0
      lasty! = 0
    END IF
  END IF

  SELECT CASE svgcommand$
    CASE "m", "M":
      ReadCoordinates argument$, svgpath$, cstart%, cend%, lastx!, lasty!

      TranslateCoordinates lastx!, lasty!, scale!, offsetx!, offsety!, drawx!, drawy!

      IF isfirst% THEN
        firstx! = lastx!
        firsty! = lasty!
      END IF

      ' We always use relative moves, because they are subpixel, whereas
      ' absolute moves have to align to pixel boundary.
      IF drawx! <> lastdrawx! OR drawy! <> lastdrawy! THEN
        dx! = drawx! - lastdrawx!
        dy! = drawy! - lastdrawy!

        AppendDRAWCommand drawcommand$, drawcommandlength%, "BM" + ForceSign$(dx!) + "," + ForceSign$(dy!)
      END IF
    CASE "l", "L":
      IF isfirst% THEN
        firstx! = lastx!
        firsty! = lasty!
      END IF

      ReadCoordinates argument$, svgpath$, cstart%, cend%, lastx!, lasty!

      TranslateCoordinates lastx!, lasty!, scale!, offsetx!, offsety!, drawx!, drawy!

      IF drawx! <> lastdrawx! OR drawy! <> lastdrawy! THEN
        dx! = drawx! - lastdrawx!
        dy! = drawy! - lastdrawy!

        AppendDRAWCommand drawcommand$, drawcommandlength%, "M" + ForceSign$(dx!) + "," + ForceSign$(dy!)
      END IF
    CASE "h", "H":
      IF isfirst% THEN
        firstx! = lastx!
        firsty! = lasty!
      END IF

      lastx! = lastx! + VAL(argument$)

      TranslateCoordinates lastx!, lasty!, scale!, offsetx!, offsety!, drawx!, drawy!

      IF drawx! <> lastdrawx! THEN
        dx! = drawx! - lastdrawx!

        IF dx! > 0 THEN
          AppendDRAWCommand drawcommand$, drawcommandlength%, "R" + LTRIM$(STR$(dx!))
        ELSE
          AppendDRAWCommand drawcommand$, drawcommandlength%, "L" + LTRIM$(STR$(-dx!))
        END IF
      END IF
    CASE "v", "V":
      IF isfirst% THEN
        firstx! = lastx!
        firsty! = lasty!
      END IF

      lasty! = lasty! + VAL(argument$)

      TranslateCoordinates lastx!, lasty!, scale!, offsetx!, offsety!, drawx!, drawy!

      IF drawy! <> lastdrawy! THEN
        dy! = drawy! - lastdrawy!

        IF dy! > 0 THEN
          AppendDRAWCommand drawcommand$, drawcommandlength%, "D" + LTRIM$(STR$(dy!))
        ELSE
          AppendDRAWCommand drawcommand$, drawcommandlength%, "U" + LTRIM$(STR$(-dy!))
        END IF
      END IF
    CASE "z", "Z":
      TranslateCoordinates firstx!, firsty!, scale!, offsetx!, offsety!, drawx!, drawy!

      IF drawx! <> lastdrawx! OR drawy! <> lastdrawy! THEN
        dx! = drawx! - lastdrawx!
        dy! = drawy! - lastdrawy!

        AppendDRAWCommand drawcommand$, drawcommandlength%, "M" + ForceSign$(dx!) + "," + ForceSign$(dy!)
      END IF

      isclosed% = -1
    CASE ELSE: STOP
  END SELECT

  cstart% = cend% + 1
  cend% = INSTR(cstart%, svgpath$, " ")

  lastdrawx! = drawx!
  lastdrawy! = drawy!

  isfirst% = 0
WEND

IF NOT isclosed% THEN
  TranslateCoordinates firstx!, firsty!, scale!, offsetx!, offsety!, drawx!, drawy!

  IF drawx! <> lastdrawx! OR drawy! <> lastdrawy! THEN
    dx! = drawx! - lastdrawx!
    dy! = drawy! - lastdrawy!

    AppendDRAWCommand drawcommand$, drawcommandlength%, "M" + ForceSign$(dx!) + "," + ForceSign$(dy!)
  END IF
END IF

TranslateSVGPathToDRAWCommand$ = LEFT$(drawcommand$, drawcommandlength%)
END FUNCTION

DECLARE SUB OverPrint (a$)
' 1960s-Style ASCII Overstrike Simulator
' Processes an uncompressed 24-bit Windows BMP image.
DIM bChannel AS STRING * 1, gChannel AS STRING * 1, rChannel AS STRING * 1
DIM bmpWidth AS LONG, bmpHeight AS LONG, dataOffset AS LONG

' Set the name of the BMP file.
bmpFile$ = "mham1995.bmp"

SCREEN 12
' 640 by 480 pixels is 80 columns by 60 rows of characters.
WIDTH 80, 60

' Draw some white "paper".
LINE (0, 0)-(640, 480), 15, BF

COLOR 15

' Target dimensions for an 80-column screen.
targetWidth = 67 ' set to 132 for line printers.
aspectCorrection# = 1

' Define characters for 3-pass overstriking.
pass1$ = " .:-=+*#%W"
pass2$ = "   .:-=+*#M"
pass3$ = "     ..:=H@"

' Open the 24-bit BMP file as a binary file.
OPEN bmpFile$ FOR BINARY AS #1

' Read the BMP file signature to verify the format.
sig$ = "  "
GET #1, 1, sig$
IF sig$ <> "BM" THEN
    PRINT "Error: File is not a valid BMP image."
    CLOSE #1: END
END IF

' Extract image pixel dimensions from the BMP header.
GET #1, 11, dataOffset     ' File offset where raw pixel data begins
GET #1, 19, bmpWidth       ' Total image width in pixels
GET #1, 23, bmpHeight      ' Total image height in pixels

' Determine the target print height based on the aspect ratio.
targetHeight = INT((targetWidth * (bmpHeight / bmpWidth)) * aspectCorrection#)
' Calculate row shift padding in the 24-bit BMP file.
rowBytes = bmpWidth * 3
padding = (4 - (rowBytes MOD 4)) MOD 4
paddedRowBytes = rowBytes + padding

' Process the image row by row from top to bottom.
' Note: BMP pixels are usually stored upside down (bottom to top).
FOR tRow = targetHeight - 1 TO 0 STEP -1
    
  ' Map the target text line back to the closest source BMP row.
  srcRow& = INT((tRow / targetHeight) * bmpHeight)

  lineP1$ = ""
  lineP2$ = ""
  lineP3$ = ""

  FOR tCol = 0 TO targetWidth - 1
    ' Map target text column back to the closest source BMP pixel column
    srcCol& = INT((tCol / targetWidth) * bmpWidth)

    ' Calculate the byte position for the target pixel.
    pixelPos& = dataOffset + 1 + (srcRow& * paddedRowBytes) + (srcCol& * 3)

    ' Extract raw byte values.
    GET #1, pixelPos&, bChannel
    GET #1, pixelPos& + 1, gChannel
    GET #1, pixelPos& + 2, rChannel

    blue = ASC(bChannel)
    green = ASC(gChannel)
    red = ASC(rChannel)

    ' Convert RGB channels to grayscale value (0 to 255)
    gray = INT((red * .299) + (green * .587) + (blue * .114))

    ' Invert grayscale value: 0 is pure white, 255 is heavy dark ink shadows
    invGray = 255 - gray

    ' Match ink density to overstrike string array indices
    idx1 = INT((invGray / 255) * (LEN(pass1$) - 1)) + 1
    idx2 = INT((invGray / 255) * (LEN(pass2$) - 1)) + 1
    idx3 = INT((invGray / 255) * (LEN(pass3$) - 1)) + 1

    ' Accumulate characters for each pass row string
    lineP1$ = lineP1$ + MID$(pass1$, idx1, 1)
    lineP2$ = lineP2$ + MID$(pass2$, idx2, 1)
    lineP3$ = lineP3$ + MID$(pass3$, idx3, 1)
  NEXT tCol

  ' --- Write the 3 overstrike passes to paper/file ---
  ' Pass 1 prints, then a carriage return CHR$(13) resets without advancing.
  OverPrint lineP1$ + CHR$(13)
  ' Pass 2 strikes over Pass 1, then carriage return resets again.
  OverPrint lineP2$ + CHR$(13)
  ' Pass 3 strikes the last layer and advances the line using CR and
  ' line feed.
  OverPrint lineP3$ + CHR$(13) + CHR$(10)
  IF INKEY$ = CHR$(27) THEN
    END
  END IF
NEXT tRow

' Close the file.
CLOSE #1
WHILE INKEY$ = "": WEND
END

' Prints text in graphics mode using overstriking.
' Uses XOR and OR modes by setting the EGA/VGA mode registers.
SUB OverPrint (a$)
  FOR i = 1 TO LEN(a$)
    c$ = MID$(a$, i, 1)
    SELECT CASE c$
      CASE CHR$(13)
        LOCATE , 1
      CASE CHR$(10)
        IF CSRLIN < 60 THEN
          LOCATE CSRLIN + 1, 1
        END IF
      CASE ELSE
        ' Print a character using overstrike.  It first prints a filled-block
        ' character in XOR mode, then prints the character in OR mode,
        ' then undoes the filled-block character using XOR mode again.
        ' The XOR mode would not be needed if the colors were white on
        ' black.
        col = POS(0)  ' current column
        OUT &H3CE, &H3
        OUT &H3CF, &H18  'XOR mode
        PRINT CHR$(219);
        LOCATE , col
        OUT &H3CE, &H3
        OUT &H3CF, &H10  ' OR mode.
        PRINT c$;
        LOCATE , col
        OUT &H3CE, &H3
        OUT &H3CF, &H18 ' XOR
        PRINT CHR$(219);
    END SELECT
  NEXT i
  ' Return to standard "replace mode".
  OUT &H3CE, &H3
  OUT &H3CF, &H0  ' Replace mode
END SUB


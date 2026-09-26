'$DYNAMIC

' This program was produced through a process of iteration with ChatGPT.
' Portions of it are hand-written, and the AI-generated portions have,
' to some degree, been reworked to improve code quality.

TYPE BMPFILEHEADER
    bfType AS STRING * 2
    bfSize AS LONG
    bfReserved1 AS INTEGER
    bfReserved2 AS INTEGER
    bfOffBits AS LONG
END TYPE

TYPE BMPINFOHEADER
    biSize AS LONG
    biWidth AS LONG
    biHeight AS LONG
    biPlanes AS INTEGER
    biBitCount AS INTEGER
    biCompression AS LONG
    biSizeImage AS LONG
    biXPelsPerMeter AS LONG
    biYPelsPerMeter AS LONG
    biClrUsed AS LONG
    biClrImportant AS LONG
END TYPE

DECLARE SUB AnalyzeBrightnessRange (fileName$, blackPoint%, whitePoint%)
DECLARE SUB ApplyBleed (img%(), w%, h%)
DECLARE SUB ApplyLevels (r%, g%, b%, blackPoint%, whitePoint%)
DECLARE SUB DisplayCentered (img%(), w%, h%)
DECLARE SUB GetBMP24Size (fileName$, w%, h%)
DECLARE SUB LoadAndQuantizeBMP24 (fileName$, img%(), w%, h%)
DECLARE SUB SetPalette ()
DECLARE FUNCTION AdjustedSigmoid# (x#)
DECLARE FUNCTION Bayer4% (x%, y%)
DECLARE FUNCTION HasNeighbourOriginal% (img%(), w%, h%, x%, y%, wanted%)
DECLARE FUNCTION HueToRGB# (p#, q#, t#)
DECLARE FUNCTION OriginalColour% (c%)
DECLARE FUNCTION Quantize4Dither% (r%, g%, b%, x%, y%)

CONST SIGMOIDK# = 6#

DIM SHARED SIGMOIDLO#, SIGMOIDHI#

SIGMOIDLO# = 1# / (1# + EXP(SIGMOIDK# * .5))
SIGMOIDHI# = 1# / (1# + EXP(-SIGMOIDK# * .5))

fileName$ = COMMAND$
IF fileName$ = "" THEN
    FILES "*.BMP"
    INPUT "24-bit BMP filename"; fileName$
END IF

GetBMP24Size fileName$, w%, h%

REDIM img%(0 TO w% - 1, 0 TO h% - 1)

LoadAndQuantizeBMP24 fileName$, img%(), w%, h%
ApplyBleed img%(), w%, h%

SCREEN 13

SetPalette

LINE (0, 0)-(319, 199), 3, BF

DisplayCentered img%(), w%, h%

WHILE INKEY$ = "": WEND
END


FUNCTION AdjustedSigmoid# (x#)
    ' Ordinary logistic sigmoid centred at 0.5.
    y# = 1# / (1# + EXP(-SIGMOIDK! * (x# - .5)))

    ' Normalize it so:
    '
    '   f(0) = 0
    '   f(1) = 1
    '

    AdjustedSigmoid# = (y# - SIGMOIDLO#) / (SIGMOIDHI# - SIGMOIDLO#)
END FUNCTION

SUB AnalyzeBrightnessRange (fileName$, blackPoint%, whitePoint%)
    DIM fh AS BMPFILEHEADER
    DIM ih AS BMPINFOHEADER

    DIM hist&(0 TO 255)
    DIM rowSize&
    DIM row$
    DIM fileRow%
    DIM x%, p%
    DIM r%, g%, b%
    DIM brightness%
    DIM offset&

    DIM total&
    DIM trimCount&
    DIM count&
    DIM lowPoint%
    DIM highPoint%
    DIM range&

    OPEN fileName$ FOR BINARY AS #1

    GET #1, 1, fh
    GET #1, , ih

    rowSize& = ((ih.biWidth * 3 + 3) \ 4) * 4
    row$ = SPACE$(rowSize&)

    total& = CLNG(ih.biWidth) * ABS(ih.biHeight)

    ' Build a 256-entry luminance histogram.  Row order does not
    ' matter for this analysis, so just read the rows as stored.
    FOR fileRow% = 0 TO ABS(ih.biHeight) - 1
        offset& = fh.bfOffBits + 1 + fileRow% * rowSize&
        GET #1, offset&, row$

        p% = 1

        FOR x% = 0 TO ih.biWidth - 1
            b% = ASC(MID$(row$, p%, 1))
            g% = ASC(MID$(row$, p% + 1, 1))
            r% = ASC(MID$(row$, p% + 2, 1))

            ' Approximation of:
            '
            '   .299 R + .587 G + .114 B
            '
            ' Coefficients add to 256.
            brightness% = (77& * r% + 150& * g% + 29& * b% + 128&) \ 256

            hist&(brightness%) = hist&(brightness%) + 1
            p% = p% + 3
        NEXT x%
    NEXT fileRow%

    CLOSE #1

    ' Throw away 5% at each end.
    trimCount& = total& * 5 \ 100

    ' Find the lower 5th-percentile boundary.
    count& = 0

    FOR brightness% = 0 TO 255
        count& = count& + hist&(brightness%)

        IF count& > trimCount& THEN
            lowPoint% = brightness%
            EXIT FOR
        END IF
    NEXT brightness%

    ' Find the upper 95th-percentile boundary.
    count& = 0

    FOR brightness% = 255 TO 0 STEP -1
        count& = count& + hist&(brightness%)

        IF count& > trimCount& THEN
            highPoint% = brightness%
            EXIT FOR
        END IF
    NEXT brightness%

    range% = highPoint% - lowPoint%

    ' Degenerate image -- no useful dynamic range.
    IF range% <= 0 THEN
        blackPoint% = 0
        whitePoint% = 255
        EXIT SUB
    END IF

    ' Blow the range out a little bit.
    blackPoint% = lowPoint% + range% * 15 \ 100
    whitePoint% = highPoint% * 105 \ 100

    ' Just in case rounding leaves no useful interval.
    IF whitePoint% <= blackPoint% THEN
        blackPoint% = lowPoint%
        whitePoint% = highPoint%
    END IF
END SUB

SUB ApplyBleed (img%(), w%, h%)
    FOR y% = 0 TO h% - 1
        ' First, coalesce out-of-phase whites.
        FOR x% = 0 TO w% - 2
            IF img%(x%, y%) = 1 AND img%(x% + 1, y%) = 2 THEN
                IF (x% + y%) AND 1 THEN
                    img%(x%, y%) = 3
                ELSE
                    img%(x% + 1, y%) = 3
                END IF
            END IF
        NEXT x%

        FOR x% = 0 TO w% - 1
            SELECT CASE img%(x%, y%)
                CASE 0
                    ' Black:
                    '
                    ' 4 = next to blue on left
                    ' 5 = next to orange on right
                    ' 6 = white on left
                    ' 7 = white on right
                    ' 8 = white on both sides
                    '
                    ' We only bleed when the colour is in the directly-
                    ' adjacent subpixel.

                    blueLeft% = 0
                    orangeRight% = 0
                    whiteLeft% = 0
                    whiteRight% = 0

                    IF x% > 0 THEN
                        SELECT CASE OriginalColour%(img%(x% - 1, y%))
                            CASE 1: blueLeft% = -1
                            CASE 3: whiteLeft% = -1
                        END SELECT
                    END IF

                    IF x% < w% - 1 THEN
                        SELECT CASE OriginalColour%(img%(x% + 1, y%))
                            CASE 2: orangeRight% = -1
                            CASE 3: whiteRight% = -1
                        END SELECT
                    END IF

                    IF whiteLeft% AND whiteRight% THEN
                        img%(x%, y%) = 8
                    ELSEIF whiteLeft% THEN
                        img%(x%, y%) = 6
                    ELSEIF whiteRight% THEN
                        img%(x%, y%) = 7
                    ELSEIF orangeRight% THEN
                        img%(x%, y%) = 5
                    ELSEIF blueLeft% THEN
                        img%(x%, y%) = 4
                    END IF

                CASE 1, 2
                    ' If colour is surrounded by white, then the white
                    ' bleeds in a little.
                    IF x% = 0 THEN
                        whiteLeft% = -1
                    ELSEIF OriginalColour%(img%(x% - 1, y%)) = 3 THEN
                        whiteLeft% = -1
                    ELSE
                        whiteLeft% = 0
                    END IF

                    IF x% = w% - 1 THEN
                        whiteRight% = -1
                    ELSEIF OriginalColour%(img%(x% + 1, y%)) = 3 THEN
                        whiteRight% = -1
                    ELSE
                        whiteRight% = 0
                    END IF

                    IF whiteLeft% AND whiteRight% THEN
                        SELECT CASE img%(x%, y%)
                            CASE 1: img%(x%, y%) = 12
                            CASE 2: img%(x%, y%) = 13
                        END SELECT
                    END IF

                CASE 3
                    ' White:
                    '
                    ' White next to black is unchanged.
                    '
                    ' 9  = next to blue on the right
                    ' 10 = next to orange on the left
                    '
                    ' The logic seems backwards here, because we're
                    ' basing the bleed on subpixels.
                    '
                    ' Layout: OBOBOBOB
                    '
                    ' Blue on the right: WW.B
                    ' Orange on the left: O.WW
                    '
                    ' In these cases, we depict colour bleed, as long
                    ' as the colour doesn't have another white on the
                    ' other side.
                    '
                    ' Blue on the left: .BWW
                    ' Orange on the right: WWO.
                    '
                    ' In these cases, we leave the white alone

                    blueRight% = 0
                    orangeLeft% = 0

                    IF x% > 0 THEN
                        SELECT CASE OriginalColour%(img%(x% - 1, y%))
                            CASE 2:
                                orangeLeft% = -1
                                IF x% > 1 THEN
                                    IF OriginalColour%(img%(x% - 2, y%)) = 3 THEN
                                        orangeLeft% = 0
                                    END IF
                                END IF
                        END SELECT
                    END IF

                    IF x% < w% - 1 THEN
                        SELECT CASE OriginalColour%(img%(x% + 1, y%))
                            CASE 1:
                                blueRight% = -1
                                IF x% + 1 < w% - 1 THEN
                                    IF OriginalColour%(img%(x% + 2, y%)) = 3 THEN
                                        blueRight% = 0
                                    END IF
                                END IF
                        END SELECT
                    END IF

                    IF blueRight% AND orangeLeft% THEN
                        img%(x%, y%) = 11
                    ELSEIF orangeLeft% THEN
                        img%(x%, y%) = 10
                    ELSEIF blueRight% THEN
                        img%(x%, y%) = 9
                    END IF
            END SELECT
        NEXT x%
    NEXT y%
END SUB

SUB ApplyLevels (r%, g%, b%, blackPoint%, whitePoint%)
    ' Convert RGB to 0..1.
    rf# = r% / 255#
    gf# = g% / 255#
    bf# = b% / 255#

    ' Find min/max components.
    max# = rf#
    IF gf# > max# THEN max# = gf#
    IF bf# > max# THEN max# = bf#

    min# = rf#
    IF gf# < min# THEN min# = gf#
    IF bf# < min# THEN min# = bf#

    delta# = max# - min#

    ' HSL lightness.
    l# = (max# + min#) / 2!

    ' Hue and saturation.
    IF delta# = 0# THEN
        h# = 0#
        s# = 0#
    ELSE
        IF l# < .5# THEN
            s# = delta# / (max# + min#)
        ELSE
            s# = delta# / (2# - max# - min#)
        END IF

        IF max# = rf# THEN
            h# = (gf# - bf#) / delta#

            IF gf# < bf# THEN
                h# = h# + 6#
            END IF
        ELSEIF max# = gf# THEN
            h# = (bf# - rf#) / delta# + 2#
        ELSE
            h# = (rf# - gf#) / delta# + 4#
        END IF

        h# = h# / 6#
    END IF


    ' ----------------------------------------------------------
    ' Apply the black/white points to LIGHTNESS, not RGB.
    ' ----------------------------------------------------------

    black# = blackPoint% / 255#
    white# = whitePoint% / 255#

    IF white# <= black# THEN
        ' Should not normally happen, but avoid division by zero.
        white# = 1#
        black# = 0#
    END IF

    l# = (l# - black#) / (white# - black#)

    ' Clamp to the new dynamic range.
    IF l# < 0# THEN l# = 0#
    IF l# > 1# THEN l# = 1#


    ' ----------------------------------------------------------
    ' Additional S-curve contrast adjustment.
    ' ----------------------------------------------------------

    'l# = AdjustedSigmoid#(l#)

    ' Protect against floating-point roundoff.
    IF l# < 0# THEN l# = 0#
    IF l# > 1# THEN l# = 1#


    ' ----------------------------------------------------------
    ' Convert HSL back to RGB.
    ' ----------------------------------------------------------

    IF s# = 0# THEN
        ' Achromatic.
        ro# = l#
        go# = l#
        bo# = l#
    ELSE
        IF l# < .5# THEN
            q# = l# * (1# + s#)
        ELSE
            q# = l# + s# - l# * s#
        END IF

        p# = 2# * l# - q#

        ro# = HueToRGB#(p#, q#, h# + 1# / 3#)
        go# = HueToRGB#(p#, q#, h#)
        bo# = HueToRGB#(p#, q#, h# - 1# / 3#)
    END IF

    ' Convert back to bytes, rounding to nearest.
    r% = CINT(ro# * 255#)
    g% = CINT(go# * 255#)
    b% = CINT(bo# * 255#)

    ' Just in case of numerical roundoff.
    IF r% < 0 THEN r% = 0
    IF r% > 255 THEN r% = 255

    IF g% < 0 THEN g% = 0
    IF g% > 255 THEN g% = 255

    IF b% < 0 THEN b% = 0
    IF b% > 255 THEN b% = 255
END SUB

FUNCTION Bayer4% (x%, y%)
    DIM n%

    n% = (y% MOD 4) * 4 + (x% MOD 4)

    SELECT CASE n%
        CASE 0:  Bayer4% = 0
        CASE 1:  Bayer4% = 8
        CASE 2:  Bayer4% = 2
        CASE 3:  Bayer4% = 10

        CASE 4:  Bayer4% = 12
        CASE 5:  Bayer4% = 4
        CASE 6:  Bayer4% = 14
        CASE 7:  Bayer4% = 6

        CASE 8:  Bayer4% = 3
        CASE 9:  Bayer4% = 11
        CASE 10: Bayer4% = 1
        CASE 11: Bayer4% = 9

        CASE 12: Bayer4% = 15
        CASE 13: Bayer4% = 7
        CASE 14: Bayer4% = 13
        CASE 15: Bayer4% = 5
    END SELECT
END FUNCTION

SUB DisplayCentered (img%(), w%, h%)
    DIM x%, y%
    DIM ox%, oy%

    ox% = (320 - w%) \ 2
    oy% = (200 - h%) \ 2

    FOR y% = 0 TO h% - 1
        FOR x% = 0 TO w% - 1
            PSET (ox% + x%, oy% + y%), img%(x%, y%)
        NEXT x%
    NEXT y%
END SUB

SUB GetBMP24Size (fileName$, w%, h%)
    DIM fh AS BMPFILEHEADER
    DIM ih AS BMPINFOHEADER

    OPEN fileName$ FOR BINARY AS #1
    GET #1, 1, fh
    GET #1, , ih
    CLOSE #1

    IF fh.bfType <> "BM" THEN
        PRINT "Not a BMP file."
        END
    END IF

    IF ih.biSize < 40 THEN
        PRINT "Unsupported BMP header."
        END
    END IF

    IF ih.biPlanes <> 1 THEN
        PRINT "Unsupported BMP: biPlanes <> 1."
        END
    END IF

    IF ih.biBitCount <> 24 THEN
        PRINT "Only 24-bit BMP files are supported."
        END
    END IF

    IF ih.biCompression <> 0 THEN
        PRINT "Compressed BMP files are not supported."
        END
    END IF

    w% = ih.biWidth
    IF ih.biHeight < 0 THEN
        h% = -ih.biHeight
    ELSE
        h% = ih.biHeight
    END IF

    IF w% < 1 OR w% > 320 OR h% < 1 OR h% > 200 THEN
        PRINT "Image must be no larger than 320x200."
        END
    END IF
END SUB

FUNCTION HasNeighbourOriginal% (img%(), w%, h%, x%, y%, wanted%)
    DIM xx%, yy%

    HasNeighbourOriginal% = 0

    FOR yy% = y% TO y%
        IF yy% >= 0 AND yy% < h% THEN
            FOR xx% = x% - 1 TO x% + 1
                IF xx% >= 0 AND xx% < w% THEN
                    IF xx% <> x% OR yy% <> y% THEN
                        IF OriginalColour%(img%(xx%, yy%)) = wanted% THEN
                            HasNeighbourOriginal% = -1
                            EXIT FUNCTION
                        END IF
                    END IF
                END IF
            NEXT xx%
        END IF
    NEXT yy%
END FUNCTION

FUNCTION HueToRGB# (p#, q#, t#)
    IF t# < 0# THEN t# = t# + 1#
    IF t# > 1# THEN t# = t# - 1#

    IF t# < 1# / 6# THEN
        HueToRGB# = p# + (q# - p#) * 6# * t#
    ELSEIF t# < 1# / 2# THEN
        HueToRGB# = q#
    ELSEIF t# < 2# / 3# THEN
        HueToRGB# = p# + (q# - p#) * (2# / 3# - t#) * 6#
    ELSE
        HueToRGB# = p#
    END IF
END FUNCTION

SUB LoadAndQuantizeBMP24 (fileName$, img%(), w%, h%)
    DIM fh AS BMPFILEHEADER
    DIM ih AS BMPINFOHEADER

    DIM rowSize&
    DIM row$
    DIM srcY%, fileRow%
    DIM x%, p%
    DIM r%, g%, b%
    DIM offset&

    DIM blackPoint%
    DIM whitePoint%

    ' First pass: analyze the source image's brightness distribution.
    PRINT "Analyzing brightness range..."

    CALL AnalyzeBrightnessRange(fileName$, blackPoint%, whitePoint%)

    PRINT "Loading and dithering..."

    OPEN fileName$ FOR BINARY AS #1

    GET #1, 1, fh
    GET #1, , ih

    rowSize& = ((w% * 3 + 3) \ 4) * 4
    row$ = SPACE$(rowSize&)

    FOR srcY% = 0 TO h% - 1

        ' Positive BMP height means rows are stored bottom-up.
        IF ih.biHeight > 0 THEN
            fileRow% = h% - 1 - srcY%
        ELSE
            fileRow% = srcY%
        END IF

        offset& = fh.bfOffBits + 1 + CLNG(fileRow%) * rowSize&
        GET #1, offset&, row$

        p% = 1

        FOR x% = 0 TO w% - 1
            b% = ASC(MID$(row$, p%, 1))
            g% = ASC(MID$(row$, p% + 1, 1))
            r% = ASC(MID$(row$, p% + 2, 1))

            ' Expand the dynamic range before quantization.
            ApplyLevels r%, g%, b%, blackPoint%, whitePoint%

            img%(x%, srcY%) = Quantize4Dither%(r%, g%, b%, x%, srcY%)

            p% = p% + 3
        NEXT x%
    NEXT srcY%

    CLOSE #1
END SUB

FUNCTION OriginalColour% (c%)
    SELECT CASE c%
        CASE 0, 4, 5, 6, 7, 8
            OriginalColour% = 0

        CASE 1, 12
            OriginalColour% = 1

        CASE 2, 13
            OriginalColour% = 2

        CASE 3, 9, 10, 11
            OriginalColour% = 3
    END SELECT
END FUNCTION

FUNCTION Quantize4Dither% (r%, g%, b%, x%, y%)
    DIM i%
    DIM pr%, pg%, pb%
    DIM dr&, dg&, db&
    DIM d&
    DIM bestD&, secondD&
    DIM best%, second%

    DIM r0%, g0%, b0%
    DIM r1%, g1%, b1%
    DIM vr&, vg&, vb&
    DIM sr&, sg&, sb&
    DIM numerator&, denominator&
    DIM mix!
    DIM threshold!

    bestD& = 2147483647
    secondD& = 2147483647
    best% = 0
    second% = 0

    ' Find the two nearest colours.
    FOR i% = 0 TO 3

        SELECT CASE i%
            CASE 0
                pr% = 0
                pg% = 0
                pb% = 0

            CASE 1
                pr% = 0
                pg% = 125
                pb% = 243

            CASE 2
                pr% = 255
                pg% = 119
                pb% = 11

            CASE 3
                pr% = 255
                pg% = 255
                pb% = 255
        END SELECT

        dr& = CLNG(r%) - pr%
        dg& = CLNG(g%) - pg%
        db& = CLNG(b%) - pb%

        d& = dr& * dr& + dg& * dg& + db& * db&

        IF d& < bestD& THEN
            secondD& = bestD&
            second% = best%

            bestD& = d&
            best% = i%

        ELSEIF d& < secondD& THEN
            secondD& = d&
            second% = i%
        END IF
    NEXT i%


    ' Get RGB values for the nearest colour.
    SELECT CASE best%
        CASE 0
            r0% = 0:   g0% = 0:   b0% = 0
        CASE 1
            r0% = 0:   g0% = 125: b0% = 243
        CASE 2
            r0% = 255: g0% = 119: b0% = 11
        CASE 3
            r0% = 255: g0% = 255: b0% = 255
    END SELECT

    ' And the second-nearest colour.
    SELECT CASE second%
        CASE 0
            r1% = 0:   g1% = 0:   b1% = 0
        CASE 1
            r1% = 0:   g1% = 125: b1% = 243
        CASE 2
            r1% = 255: g1% = 119: b1% = 11
        CASE 3
            r1% = 255: g1% = 255: b1% = 255
    END SELECT


    ' Project the source colour onto the line joining the two
    ' nearest palette colours. This gives the desired proportion
    ' of the second colour.
    '
    ' V = second - best
    ' S = source - best

    vr& = CLNG(r1%) - r0%
    vg& = CLNG(g1%) - g0%
    vb& = CLNG(b1%) - b0%

    sr& = CLNG(r%) - r0%
    sg& = CLNG(g%) - g0%
    sb& = CLNG(b%) - b0%

    numerator& = sr& * vr& + sg& * vg& + sb& * vb&
    denominator& = vr& * vr& + vg& * vg& + vb& * vb&

    IF denominator& = 0 THEN
        Quantize4Dither% = best%
        EXIT FUNCTION
    END IF

    mix! = numerator& / denominator&

    IF mix! < 0! THEN mix! = 0!
    IF mix! > 1! THEN mix! = 1!


    ' Bayer values 0..15 are regarded as thresholds centred
    ' within sixteen equal intervals.
    threshold! = (Bayer4%(x%, y%) + .5) / 16!

    IF mix! > threshold! THEN
        Quantize4Dither% = second%
    ELSE
        Quantize4Dither% = best%
    END IF
END FUNCTION

SUB SetPalette
' Core colours map the TRS-80 two-pixel arrangement:
'   0 - 00 - black (0, 0, 0)
'   1 - 01 - blue half on (0, 31, 60)
'   2 - 10 - red half on (63, 29, 3)
'   3 - 11 - white (63, 63, 63)
'
' Blended colours for the bleed effect
'   4 - black next to blue on the left (0, 11, 31)
'   5 - black next to orange on the right (42, 20, 15)
'   6 - black next to white on left (33, 57, 55)
'   7 - black next to white on right (63, 55, 37)
'   8 - black next to white on both sides (38, 40, 38)
'   9 - white next to blue on right (33, 57, 55)
'   10 - white next to orange on left (63, 55, 37)
'   11 - white next to black on both sides (38, 40, 38)
'   12 - blue with white on both sides (33, 57, 55)
'   13 - orange with white on both sides (63, 55, 37)
'
' Colours are repeated to allow the original colour to be extracted.

OUT &H3C8, 0
OUT &H3C9, 0: OUT &H3C9, 0: OUT &H3C9, 0
OUT &H3C9, 0: OUT &H3C9, 31: OUT &H3C9, 60
OUT &H3C9, 63: OUT &H3C9, 29: OUT &H3C9, 3
OUT &H3C9, 63: OUT &H3C9, 63: OUT &H3C9, 63
OUT &H3C9, 0: OUT &H3C9, 11: OUT &H3C9, 31
OUT &H3C9, 42: OUT &H3C9, 20: OUT &H3C9, 15
OUT &H3C9, 33: OUT &H3C9, 57: OUT &H3C9, 55
OUT &H3C9, 63: OUT &H3C9, 55: OUT &H3C9, 37
OUT &H3C9, 38: OUT &H3C9, 40: OUT &H3C9, 38
OUT &H3C9, 33: OUT &H3C9, 57: OUT &H3C9, 55
OUT &H3C9, 63: OUT &H3C9, 55: OUT &H3C9, 37
OUT &H3C9, 38: OUT &H3C9, 40: OUT &H3C9, 38
OUT &H3C9, 33: OUT &H3C9, 57: OUT &H3C9, 55
OUT &H3C9, 63: OUT &H3C9, 55: OUT &H3C9, 37
END SUB

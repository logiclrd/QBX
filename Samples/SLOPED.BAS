DECLARE SUB Scale2x (x%, y%, w%, h%, ox%, oy%)
DECLARE SUB Scale3x (x%, y%, w%, h%, ox%, oy%)

' Based on a ZX Spectrum program published in the Czechoslovak
' magazine 'ZX magazin' 2/1993. Taken from the book 'Spectrum
' graphics and sound' by S. Money.

CONST UseScale% = 2 ' 1 .. 3

SCREEN 12
PRINT "#"

cx% = 8
cy% = 16

SELECT CASE UseScale%
  CASE 1
    scaledx% = 0
    scaledy% = 0
    scaledcx% = cx%
    scaledcy% = cy%
  CASE 2, 3
    scaledx% = 10
    scaledy% = 0

    SELECT CASE UseScale%
      CASE 2: Scale2x 0, 0, cx% - 1, cy% - 1, scaledx%, scaledy%
      CASE 3: Scale3x 0, 0, cx% - 1, cy% - 1, scaledx%, scaledy%
    END SELECT

    scaledcx% = cx% * UseScale%
    scaledcy% = cy% * UseScale%
END SELECT

slopedx% = scaledx% + scaledcx%
slopedy% = scaledy%

oy% = slopedy%

FOR v% = 1 TO 5
  ox% = slopedx%

  FOR h% = 1 TO 5
    FOR y% = 0 TO scaledcy% - 1
      FOR x% = 0 TO scaledcx% - 1
        c% = POINT(scaledx% + x%, scaledy% + y%)

        FOR j% = 0 TO v% - 1
          FOR i% = 0 TO h% - 1
            PSET (ox% + h% * x% + i% + (scaledcy% - y%) * h% \ 2, oy% + v% * y% + j%), c%
          NEXT i%
        NEXT j%
      NEXT x%
    NEXT y%

    ox% = ox% + (h% + 1) * scaledcx%
  NEXT h%

  oy% = oy% + v% * scaledcy% - 2
NEXT v%

SLEEP

SUB Scale2x (x%, y%, w%, h%, ox%, oy%)
DIM ipix%(0 TO 2, 0 TO 2)
DIM opix%(0 TO 1, 0 TO 1)

FOR dy% = 0 TO h% - 1
  yy% = y% + dy%

  IF yy% > 0 THEN
    IF x% > 0 THEN ipix%(1, 0) = POINT(x% - 1, yy% - 1) ELSE ipix%(1, 0) = 0
    ipix%(2, 0) = POINT(x%, yy% - 1)
  ELSE
    ipix%(1, 0) = 0
    ipix%(2, 0) = 0
  END IF

  IF x% > 0 THEN ipix%(1, 1) = POINT(x% - 1, yy%) ELSE ipix%(1, 1) = 0
  ipix%(2, 1) = POINT(x%, yy%)

  IF yy% < 479 THEN
    IF x% > 0 THEN ipix%(1, 2) = POINT(x% - 1, yy% + 1) ELSE ipix%(1, 2) = 0
    ipix%(2, 2) = POINT(x%, yy% + 1)
  ELSE
    ipix%(1, 2) = 0
    ipix%(2, 2) = 0
  END IF

  FOR dx% = 0 TO w% - 1
    xx% = x% + dx%

    ' Shift window, since 2 out of 3 columns are
    ' in common with the previous pixel
    FOR i% = 0 TO 1
      FOR j% = 0 TO 2
        ipix%(i%, j%) = ipix%(i% + 1, j%)
      NEXT j%
    NEXT i%

    ' Read new column
    IF xx% < 639 THEN
      IF yy% > 0 THEN ipix%(2, 0) = POINT(xx% + 1, yy% - 1)
      ipix%(2, 1) = POINT(xx% + 1, yy%)
      IF yy% < 479 THEN ipix%(2, 2) = POINT(xx% + 1, yy% + 1)
    ELSE
      ipix%(2, 0) = 0
      ipix%(2, 1) = 0
      ipix%(2, 2) = 0
    END IF

    ' Initial: Expand middle pixel to 2x2 block
    FOR i% = 0 TO 1
      FOR j% = 0 TO 1
        opix%(i%, j%) = ipix%(1, 1)
      NEXT j%
    NEXT i%

    ' Rules
    IF ipix%(1, 0) <> ipix%(1, 2) AND ipix%(0, 1) <> ipix%(2, 1) THEN
      IF ipix%(0, 1) = ipix%(1, 0) THEN opix%(0, 0) = ipix%(0, 1)
      IF ipix%(1, 0) = ipix%(2, 1) THEN opix%(1, 0) = ipix%(2, 1)
      IF ipix%(0, 1) = ipix%(1, 2) THEN opix%(0, 1) = ipix%(0, 1)
      IF ipix%(1, 2) = ipix%(2, 1) THEN opix%(1, 1) = ipix%(2, 1)
    END IF

    ' Render
    oxx% = ox% + 2 * dx%
    oyy% = oy% + 2 * dy%

    FOR i% = 0 TO 1
      FOR j% = 0 TO 1
        PSET (oxx% + i%, oyy% + j%), opix%(i%, j%)
      NEXT j%
    NEXT i%
  NEXT dx%
NEXT dy%
END SUB

SUB Scale3x (x%, y%, w%, h%, ox%, oy%)
DIM ipix%(0 TO 2, 0 TO 2)
DIM opix%(0 TO 2, 0 TO 2)

FOR dy% = 0 TO h% - 1
  yy% = y% + dy%

  IF yy% > 0 THEN
    IF x% > 0 THEN ipix%(1, 0) = POINT(x% - 1, yy% - 1) ELSE ipix%(1, 0) = 0
    ipix%(2, 0) = POINT(x%, yy% - 1)
  ELSE
    ipix%(1, 0) = 0
    ipix%(2, 0) = 0
  END IF

  IF x% > 0 THEN ipix%(1, 1) = POINT(x% - 1, yy%) ELSE ipix%(1, 1) = 0
  ipix%(2, 1) = POINT(x%, yy%)

  IF yy% < 479 THEN
    IF x% > 0 THEN ipix%(1, 2) = POINT(x% - 1, yy% + 1) ELSE ipix%(1, 2) = 0
    ipix%(2, 2) = POINT(x%, yy% + 1)
  ELSE
    ipix%(1, 2) = 0
    ipix%(2, 2) = 0
  END IF

  FOR dx% = 0 TO w% - 1
    xx% = x% + dx%

    ' Shift window, since 2 out of 3 columns are
    ' in common with the previous pixel
    FOR i% = 0 TO 1
      FOR j% = 0 TO 2
        ipix%(i%, j%) = ipix%(i% + 1, j%)
      NEXT j%
    NEXT i%

    ' Read new column
    IF xx% < 639 THEN
      IF yy% > 0 THEN ipix%(2, 0) = POINT(xx% + 1, yy% - 1)
      ipix%(2, 1) = POINT(xx% + 1, yy%)
      IF yy% < 479 THEN ipix%(2, 2) = POINT(xx% + 1, yy% + 1)
    ELSE
      ipix%(2, 0) = 0
      ipix%(2, 1) = 0
      ipix%(2, 2) = 0
    END IF

    ' Initial: Expand middle pixel to 3x3 block
    FOR i% = 0 TO 2
      FOR j% = 0 TO 2
        opix%(i%, j%) = ipix%(1, 1)
      NEXT j%
    NEXT i%

    ' Rules
    IF ipix%(1, 0) <> ipix%(1, 2) AND ipix%(0, 1) <> ipix%(2, 1) THEN
      IF ipix%(0, 1) = ipix%(1, 0) THEN opix%(0, 0) = ipix%(0, 1)
      IF ipix%(2, 1) = ipix%(1, 0) THEN opix%(2, 0) = ipix%(2, 1)
      IF ipix%(0, 1) = ipix%(1, 2) THEN opix%(0, 2) = ipix%(0, 1)
      IF ipix%(2, 1) = ipix%(1, 2) THEN opix%(2, 2) = ipix%(2, 1)

      IF ipix%(0, 1) = ipix%(1, 0) AND ipix%(1, 1) <> ipix%(2, 0) THEN
        opix%(1, 0) = ipix%(1, 0)
      ELSEIF ipix%(1, 0) = ipix%(2, 1) AND ipix%(1, 1) <> ipix%(0, 0) THEN
        opix%(1, 0) = ipix%(1, 0)
      END IF

      IF ipix%(0, 1) = ipix%(1, 0) AND ipix%(1, 1) <> ipix%(0, 2) THEN
        opix%(0, 1) = ipix%(0, 1)
      ELSEIF ipix%(0, 1) = ipix%(1, 2) AND ipix%(1, 1) <> ipix%(0, 0) THEN
        opix%(0, 1) = ipix%(0, 1)
      END IF

      IF ipix%(1, 0) = ipix%(2, 1) AND ipix%(1, 1) <> ipix%(2, 2) THEN
        opix%(2, 1) = ipix%(2, 1)
      ELSEIF ipix%(1, 2) = ipix%(2, 1) AND ipix%(1, 1) <> ipix%(2, 0) THEN
        opix%(2, 1) = ipix%(2, 1)
      END IF

      IF ipix%(0, 1) = ipix%(1, 2) AND ipix%(1, 1) <> ipix%(2, 2) THEN
        opix%(1, 2) = ipix%(1, 2)
      ELSEIF ipix%(1, 2) = ipix%(0, 1) AND ipix%(1, 1) <> ipix%(0, 2) THEN
        opix%(1, 2) = ipix%(1, 2)
      END IF
    END IF

    ' Render
    oxx% = ox% + 3 * dx%
    oyy% = oy% + 3 * dy%

    FOR i% = 0 TO 2
      FOR j% = 0 TO 2
        PSET (oxx% + i%, oyy% + j%), opix%(i%, j%)
      NEXT j%
    NEXT i%
  NEXT dx%
NEXT dy%
END SUB

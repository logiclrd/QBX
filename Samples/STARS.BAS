' Written by Jonathan Gilbert in February 2026
TYPE Pt
  X AS INTEGER
  Y AS INTEGER
  Z AS INTEGER

  ProjectedX AS INTEGER
  ProjectedY AS INTEGER
END TYPE

CONST NumPts% = 100

CONST Speed% = 1

CONST FieldWidth% = 120
CONST FieldHeight% = 80
CONST FieldDepth% = 50

CONST Scale& = 10

CONST ClipDepth& = 5 * Scale&

CONST MinX% = Scale& * -FieldWidth% \ 2
CONST MaxX% = Scale& * FieldWidth% \ 2
CONST MinY% = Scale& * -FieldHeight% \ 2
CONST MaxY% = Scale& * FieldHeight% \ 2
CONST MinZ% = ClipDepth&
CONST MaxZ% = ClipDepth& + Scale& * FieldDepth%

CONST ScreenCentreX% = 40.5 * Scale&
CONST ScreenCentreY% = 25.5 * Scale&

CONST SpeedScale% = Speed% * Scale&

DEF FNRnd% (Min%, Max%) = INT(RND * (Max% - Min%)) + Min%

DIM Pts(0 TO NumPts% - 1) AS Pt

FOR i% = 0 TO NumPts% - 1
  Pts(i%).X = FNRnd%(MinX%, MaxX%)
  Pts(i%).Y = FNRnd%(MinY%, MaxY%)
  Pts(i%).Z = MinZ% + i% * Scale& * FieldDepth% \ NumPts%
NEXT i%

ClosestPt% = 1

WIDTH 80, 50
CLS 

' Set gradient palette
OUT &H3C8, 1

FOR i% = 1 TO 15
  PALETTE i%, i%

  Intensity% = i% * 63 \ 15
  OUT &H3C9, Intensity%
  OUT &H3C9, Intensity%
  OUT &H3C9, Intensity%
NEXT i%

Characters$ = CHR$(250) + CHR$(250) + CHR$(249) + CHR$(249) + CHR$(254)

DO
  i% = ClosestPt%
  FOR LoopIndex% = 1 TO NumPts%
    i% = i% - 1
    IF i% < 0 THEN i% = NumPts% - 1

    Pts(i%).Z = Pts(i%).Z - SpeedScale%

    IF Pts(i%).Z < ClipDepth& THEN
      Pts(i%).X = FNRnd%(MinX%, MaxX%)
      Pts(i%).Y = FNRnd%(MinY%, MaxY%)
      Pts(i%).Z = MaxZ%

      ClosestPt% = i% + 1
    END IF

    OldProjectedX% = Pts(i%).ProjectedX
    OldProjectedY% = Pts(i%).ProjectedY

    Pts(i%).ProjectedX = Pts(i%).X * ClipDepth& \ Pts(i%).Z
    Pts(i%).ProjectedY = Pts(i%).Y * ClipDepth& \ Pts(i%).Z

    ' At this point, ProjectedX/Y have their origin at the middle
    ' of the screen and treat positive Y as up. Plus, they're
    ' scaled by Scale&. Translated these to coordinates for LOCATE.
    Pts(i%).ProjectedX = (ScreenCentreX% + Pts(i%).ProjectedX) \ Scale&
    Pts(i%).ProjectedY = (ScreenCentreY% - Pts(i%).ProjectedY) \ Scale&

    IF (OldProjectedX% <> Pts(i%).ProjectedX) OR (OldProjectedY% <> Pts(i%).ProjectedY) THEN
      IF (OldProjectedX% >= 1) AND (OldProjectedX% <= 80) THEN
        IF (OldProjectedY% >= 1) AND (OldProjectedY% <= 50) THEN
          LOCATE OldProjectedY%, OldProjectedX%
          PRINT " ";
        END IF
      END IF
    END IF

    IF (Pts(i%).ProjectedX >= 1) AND (Pts(i%).ProjectedX <= 80) THEN
      IF (Pts(i%).ProjectedY >= 1) AND (Pts(i%).ProjectedY <= 50) THEN
        RawZ = (Pts(i%).Z - MinZ%)
        CONST DarkestZStart% = 2 * FieldDepth% \ 3
        CONST MaxIntensity% = 15
        CONST IntensityConversionNumerator% = MaxIntensity% - 2
        CONST IntensityConversionDenominator% = DarkestZStart% * 2

        Intensity% = MaxIntensity% * Scale& \ ((Pts(i%).Z - MinZ%) * IntensityConversionNumerator% \ IntensityConversionDenominator% + Scale&)

        COLOR Intensity%

        CONST SmallestZStart% = Scale& * FieldDepth% \ 4
        CONST MaxSize% = 5 ' LEN(Characters$)
        CONST SizeConversionNumerator% = MaxSize% - 2
        CONST SizeConversionDenominator% = SmallestZStart% * 2

        Size% = MaxSize% * Scale& \ ((Pts(i%).Z - MinZ%) * SizeConversionNumerator% \ SizeConversionDenominator% + Scale&)

        IF Size% < 1 THEN Size% = 1
        IF Size% > MaxSize% THEN Size% = MaxSize%
        
        LOCATE Pts(i%).ProjectedY, Pts(i%).ProjectedX
        PRINT MID$(Characters$, Size%, 1);
      END IF
    END IF
  NEXT LoopIndex%

  st! = TIMER
  WHILE st! = TIMER: WEND
LOOP


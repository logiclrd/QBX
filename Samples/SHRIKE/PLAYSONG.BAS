DECLARE SUB InitializeVisualization (songnotes() AS NoteType, numnotes%, chan1line%)
DECLARE SUB UpdateVisualization (chan1line%, channelvisage%(), channelvisx%(), songsamples() AS SampleType, songnotes() AS NoteType, songrownotes%(), nextrow%, lastrow%, lastnote%, rowspersecond!)

FILES "*.s3m"

LINE INPUT "Select: ", filename$

CLS

PRINT "Playing: "; filename$

OPEN filename$ FOR BINARY AS #1

S3MMeasure 1, numsamples%, numnotes%, numrows%

SEEK #1, 1

TYPE SampleType
  StartAddress AS LONG
  EndAddress AS LONG
  LoopStartAddress AS LONG
  DefaultVolume AS SINGLE
  EnableLoop AS INTEGER
  Is16bitSample AS INTEGER
  C4Frequency AS LONG
  VisualizationColour AS INTEGER
END TYPE

TYPE NoteType
  ChannelNumber AS INTEGER
  SampleNumber AS INTEGER ' -1 means note cut
  NoteFrequency AS LONG
  Volume AS SINGLE
  JumpToRowNumber AS INTEGER
  NewGlobalVolume AS SINGLE
END TYPE

DIM songsamples(1 TO numsamples%) AS SampleType
DIM songnotes(1 TO numnotes%) AS NoteType
DIM songrownotes%(1 TO numrows%)

GUSReset

S3MLoad 1, songsamples(), songnotes(), songrownotes%(), globalvolume!, 60, frameticks%, rowticks%, tickfrequency%

CLOSE #1

PRINT "Song data:"; numsamples%; "samples,"; numnotes%; "notes total,"; numrows%; "rows total"
PRINT

InitializeVisualization songnotes(), numnotes%, chan1line%

DIM channelvisx%(31), channelvisage%(31)

DIM channellastsample%(31)

ticksperrow% = 2

rowspersecond! = tickfrequency% / rowticks%

nextrow% = 1
nextnote% = 1

lastrow% = 1
lastnote% = 1

S3MProgramTimerRate (tickfrequency%)

S3MInitializeTiming lastframetick&, lastrowtick&, tickbase&

DO
  S3MFrameWait frameticks%, lastframetick&, rowticks%, lastrowtick&, tickbase&, songsamples(), songnotes(), songrownotes%(), nextrow%, nextnote%, channellastsample%(), globalvolume!

  UpdateVisualization chan1line%, channelvisage%(), channelvisx%(), songsamples(), songnotes(), songrownotes%(), nextrow%, lastrow%, lastnote%, rowspersecond!

  framenumber& = framenumber& + 1
LOOP WHILE nextrow% <= UBOUND(songrownotes%) AND INKEY$ = ""

GUSReset

END

DO
  S3MPlayNextRow songsamples(), songnotes(), songrownotes%(), nextrow%, nextnote%, channellastsample%(), globalvolume!

  UpdateVisualization chan1line%, channelvisage%(), channelvisx%(), songsamples(), songnotes(), songrownotes%(), nextrow%, lastrow%, lastnote%, rowspersecond!

  st! = TIMER: WHILE TIMER = st!: WEND
  st! = TIMER: WHILE TIMER = st!: WEND
LOOP WHILE nextrow% <= UBOUND(songrownotes%) AND INKEY$ = ""

GUSReset

SUB InitializeVisualization (songnotes() AS NoteType, numnotes%, chan1line%)
chan1line% = CSRLIN

dots$ = STRING$(72, 250)

COLOR 8

FOR i% = 1 TO numnotes%
  IF songnotes(i%).ChannelNumber > maxchannel% THEN
    maxchannel% = songnotes(i%).ChannelNumber
  END IF
NEXT i%

FOR i% = chan1line% TO chan1line% + maxchannel%
  IF i% > 25 THEN EXIT FOR

  LOCATE i%, 1
  PRINT dots$;
NEXT i%
END SUB

SUB UpdateVisualization (chan1line%, channelvisage%(), channelvisx%(), songsamples() AS SampleType, songnotes() AS NoteType, songrownotes%(), nextrow%, lastrow%, lastnote%, rowspersecond!)
STATIC dot$, block$

IF LEN(dot$) = 0 THEN
  dot$ = CHR$(250)
  block$ = CHR$(254)
END IF

WHILE lastrow% < nextrow%
  FOR channel% = 0 TO 31
    chanline% = chan1line% + channel%

    IF chanline% > 25 THEN EXIT FOR

    IF channelvisage%(channel%) > 0 THEN
      channelvisage%(channel%) = channelvisage%(channel%) - 1

      IF channelvisage%(channel%) = 0 THEN
        IF channelvisx%(channel%) > 0 THEN
          COLOR 8
          LOCATE chanline%, channelvisx%(channel%)
          PRINT dot$;

          channelvisx%(channel%) = 0
        END IF
      END IF
    END IF
  NEXT channel%

  IF lastnote% < UBOUND(songnotes) THEN
    FOR i% = lastnote% TO lastnote% + songrownotes%(lastrow%) - 1
      channel% = songnotes(i%).ChannelNumber

      chanline% = chan1line% + channel%

      IF chanline% <= 25 THEN
        IF channelvisx%(channel%) > 0 THEN
          COLOR 8
          LOCATE chanline%, channelvisx%(channel%)
          PRINT dot$;
        END IF

        IF songnotes(i%).SampleNumber > 0 THEN
          sample% = songnotes(i%).SampleNumber

          channelvisx%(channel%) = LOG(songnotes(i%).NoteFrequency) * 12 / LOG(2) - 150
          IF channelvisx%(channel%) > 72 THEN channelvisx%(channel%) = 0

          IF songsamples(sample%).EnableLoop THEN
            channelvisage%(channel%) = -1
          ELSE
            samplelength& = songsamples(sample%).EndAddress - songsamples(sample%).StartAddress
            channelvisage%(channel%) = 2 + rowspersecond! * samplelength& / songnotes(i%).NoteFrequency
          END IF

          IF channelvisx%(channel%) > 0 THEN
            COLOR songsamples(sample%).VisualizationColour
            LOCATE chanline%, channelvisx%(channel%)
            PRINT block$;
          END IF
        END IF
      ELSEIF songnotes(i%).SampleNumber < 0 THEN
        IF channelvisx%(channel%) > 0 THEN
          COLOR 15
          LOCATE chanline%, channelvisx%(channel%)
          PRINT block$;
        END IF
      ELSEIF songnotes(i%).Volume >= 0 THEN
        IF channelvisx%(channel%) > 0 THEN
          v% = songnotes(i%).Volume * 10
          IF v% > 9 THEN v% = 9

          COLOR songsamples(songnotes(i%).SampleNumber).VisualizationColour
          LOCATE chanline%, channelvisx%(channel%)
          PRINT CHR$(48 + v%);
        END IF
      END IF
    NEXT i%

    lastnote% = lastnote% + songrownotes%(lastrow%)
  END IF

  lastrow% = lastrow% + 1
WEND
END SUB

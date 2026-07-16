'$DYNAMIC

DECLARE FUNCTION SelectFile$ ()
DECLARE FUNCTION SelectSong% (numsongs%, numrows%, songs() AS SongType, notes() AS NoteType, rownotes%(), rowticks%, tickfrequency%)

DECLARE SUB InitializeVisualization (songnotes() AS NoteType, numnotes%, chan1line%)
DECLARE SUB UpdateVisualization (chan1line%, channelvisage%(), channelvisx%(), songsamples() AS SampleType, songnotes() AS NoteType, songrownotes%(), nextrow%, lastrow%, lastnote%, rowspersecond!)

filename$ = SelectFile$

IF filename$ = "" THEN END

CLS

LOCATE 1, 79 - LEN(filename$)
PRINT filename$;

OPEN filename$ FOR BINARY AS #1

S3MMeasure 1, numsamples%, numsongs%, numnotes%, numrows%

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

TYPE SongType
  FirstRowNumber AS INTEGER
  FirstNoteNumber AS INTEGER
END TYPE

TYPE NoteType
  ChannelNumber AS INTEGER
  NoteAction AS INTEGER ' encodes row number for jump, sample for note
  NoteFrequency AS LONG ' or note number for jump
  Volume AS SINGLE
END TYPE

CONST NoteActionStartNote% = 0 ' plus sample number
CONST NoteActionStartNoteLast% = 255
CONST NoteActionJump% = 256 ' plus row number
CONST NoteActionSetNoteVolume% = 257
CONST NoteActionSetGlobalVolume% = 258
CONST NoteActionNoteCut% = 259
CONST NoteActionEndSong% = 260

DIM songsamples(1 TO numsamples%) AS SampleType
DIM songsongs(1 TO numsongs%) AS SongType
DIM songnotes(1 TO numnotes%) AS NoteType
DIM songrownotes%(1 TO numrows%)

GUSReset

S3MLoad 1, songsamples(), songsongs(), songnotes(), songrownotes%(), globalvolume!, 60, frameticks%, rowticks%, tickfrequency%

CLOSE #1

PRINT "Song data:"; numsamples%; "samples,"; numnotes%; "notes total,"; numrows%; "rows total"
PRINT

toprow% = CSRLIN

DIM channelvisx%(31), channelvisage%(31)

DIM channellastsample%(31)

DO
  LOCATE toprow%, 1
  COLOR 11

  selectedsong% = SelectSong%(numsongs%, numrows%, songsongs(), songnotes(), songrownotes%(), rowticks%, tickfrequency%)

  IF selectedsong% = 0 THEN EXIT DO

  InitializeVisualization songnotes(), numnotes%, chan1line%

  rowspersecond! = tickfrequency% / rowticks%

  S3MJumpToSong songsongs(selectedsong%), nextrow%, nextnote%

  lastrow% = nextrow%
  lastnote% = nextnote%

  S3MProgramTimerRate (tickfrequency%)

  S3MInitializeTiming lastframetick&, lastrowtick&, tickbase&

  DO
    S3MFrameWait frameticks%, lastframetick&, rowticks%, lastrowtick&, tickbase&, songsamples(), songnotes(), songrownotes%(), nextrow%, nextnote%, channellastsample%(), globalvolume!

    UpdateVisualization chan1line%, channelvisage%(), channelvisx%(), songsamples(), songnotes(), songrownotes%(), nextrow%, lastrow%, lastnote%, rowspersecond!

    ' Track looping in the visualization state.
    IF nextnote% < lastnote% THEN
      lastrow% = nextrow%
      lastnote% = nextnote%
    END IF

    framenumber& = framenumber& + 1
  LOOP WHILE nextrow% <= UBOUND(songrownotes%) AND INKEY$ = ""

  FOR i% = 0 TO 31
    GUSStopNote i%
  NEXT i%

  S3MProgramTimerRate 18.20645
LOOP

GUSReset

END

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

'$DYNAMIC
FUNCTION SelectFile$
w% = 15
h% = 8

maxoption% = 10
numoptions% = 0

DIM options$(1 TO maxoption%)

f$ = DIR$("*.s3m")

WHILE f$ <> ""
  numoptions% = numoptions% + 1

  IF numoptiosn% > maxoptions% THEN
    maxoptions% = maxoptions% + 10
    REDIM PRESERVE options$(1 TO maxoption%)
  END IF

  IF LEN(f$) > w% THEN f$ = LEFT$(f$, w%)

  options$(numoptions%) = f$

  f$ = DIR$
WEND

CLS

h$ = CHR$(196)
v$ = CHR$(179)

tl$ = CHR$(218)
tr$ = CHR$(191)
bl$ = CHR$(192)
br$ = CHR$(217)

u$ = CHR$(24)
d$ = CHR$(25)

ss$ = SPACE$(w%)

x1% = 40 - w% \ 2
x2% = x1% + w% + 1
y1% = 4
y2% = y1% + h% + 1

m$ = "GUS Mini S3M"

COLOR 11
LOCATE 2, 40 - LEN(m$) \ 2
PRINT m$;

COLOR 8

LOCATE y1%, x1%
PRINT tl$; STRING$(w%, h$); tr$;

FOR y% = y1% + 1 TO y2% - 1
  LOCATE y%, x1%: PRINT v$;
  LOCATE y%, x2%:  PRINT v$;
NEXT y%

LOCATE y2%, x1%
PRINT bl$; STRING$(w% - 2, h$);
COLOR 7
PRINT u$; d$;
COLOR 8
PRINT br$

COLOR 7

KeyUpArrow$ = CHR$(0) + "H"
KeyDownArrow$ = CHR$(0) + "P"

scrolltop% = 1
selection% = 1

DO
  FOR y% = y1% + 1 TO y2% - 1
    idx% = (y% - y1% - 1) + scrolltop%

    IF idx% = selection% THEN COLOR 0, 7 ELSE COLOR 7, 0

    LOCATE y%, x1% + 1

    IF idx% > numoptions% THEN
      PRINT ss$;
    ELSE
      PRINT options$(idx%);
      ol% = LEN(options$(idx%))
      IF ol% < w% THEN
        PRINT LEFT$(ss$, w% - ol%);
      END IF
    END IF
  NEXT y%

  DO
    k$ = INKEY$
  LOOP WHILE k$ = ""

  kk% = ASC(k$)

  IF kk% = 27 THEN EXIT FUNCTION
  IF kk% = 13 THEN EXIT DO

  IF k$ = KeyUpArrow$ THEN
    selection% = selection% - 1
    IF selection% < 1 THEN selection% = 1
    IF selection% < scrolltop% THEN scrolltop% = selection%
  ELSEIF k$ = KeyDownArrow$ THEN
    selection% = selection% + 1
    IF selection% > numoptions% THEN selection% = numoptions%
    IF selection% - h% + 1 > scrolltop% THEN scrolltop% = selection% - h% + 1
  END IF
LOOP

IF selection% <= numoptions% THEN
  SelectFile$ = options$(selection%)
END IF
END FUNCTION

FUNCTION SelectSong% (numsongs%, numrows%, songs() AS SongType, notes() AS NoteType, rownotes%(), rowticks%, tickfrequency%)
toprow% = CSRLIN

VIEW PRINT toprow% TO 25

CLS
LOCATE toprow%, 1

FOR i% = 1 TO numsongs%
  lastrow% = numrows%
  IF i% < numsongs% THEN lastrow% = songs(i% + 1).FirstRowNumber - 1

  rowcount% = lastrow% - songs(i%).FirstRowNumber + 1
  notecount% = 0

  FOR j% = songs(i%).FirstRowNumber TO lastrow%
    notecount% = notecount% + rownotes%(j%)
  NEXT j%

  duration% = rowcount% * 1& * rowticks% \ tickfrequency%

  durationmins% = duration% \ 60
  durationsecs% = duration% MOD 60

  duration$ = LTRIM$(RTRIM$(STR$(durationmins%))) + ":"
  duration$ = duration$ + RIGHT$("0" + LTRIM$(RTRIM$(STR$(durationsecs%))), 2)

  PRINT "   Song"; i%; ":"; notecount%; " notes,"; rowcount%; " rows, ~"; duration$
NEXT i%

selectedsong% = 1

LOCATE toprow% + selectedsong% - 1, 1: PRINT "->";

KeyUpArrow$ = CHR$(0) + "H"
KeyDownArrow$ = CHR$(0) + "P"

DO
  k$ = INKEY$

  IF LEN(k$) THEN
    LOCATE toprow% + selectedsong% - 1, 1: PRINT "  ";

    IF k$ = KeyUpArrow$ THEN
      selectedsong% = selectedsong% - 1
      IF selectedsong% = 0 THEN selectedsong% = numsongs%
    ELSEIF k$ = KeyDownArrow$ THEN
      selectedsong% = selectedsong% + 1
      IF selectedsong% > numsongs% THEN selectedsong% = 1
    ELSE
      kk% = ASC(k$)
    END IF

    LOCATE toprow% + selectedsong% - 1, 1: PRINT "->";
  END IF
LOOP UNTIL kk% = 13 OR kk% = 27

IF kk% = 13 THEN SelectSong% = selectedsong%

CLS
VIEW PRINT
LOCATE toprow%, 1
END FUNCTION

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

        SELECT CASE songnotes(i%).NoteAction
          CASE NoteActionStartNote% TO NoteActionStartNoteLast%:
            sample% = songnotes(i%).NoteAction - NoteActionStartNote%

            IF sample% > 0 THEN
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
          CASE NoteActionNoteCut%:
            IF channelvisx%(channel%) > 0 THEN
              COLOR 15
              LOCATE chanline%, channelvisx%(channel%)
              PRINT block$;
            END IF
          CASE NoteActionSetNoteVolume%:
            IF channelvisx%(channel%) > 0 THEN
              v% = songnotes(i%).Volume * 10
              IF v% > 9 THEN v% = 9

              COLOR 7
              LOCATE chanline%, channelvisx%(channel%)
              PRINT CHR$(48 + v%);
            END IF
        END SELECT
      END IF
    NEXT i%

    lastnote% = lastnote% + songrownotes%(lastrow%)
  END IF

  lastrow% = lastrow% + 1
WEND
END SUB

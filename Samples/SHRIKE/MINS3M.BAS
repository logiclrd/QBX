'$STATIC

DECLARE SUB S3MLoadPattern (filenum%, ignorechannel%(), samples() AS SampleType, mastervolume!, notes() AS NoteType, rownotes%(), numnotes%, numrows%, order%, jumptoorder%, jumptorow%)
DECLARE SUB S3MLoadSample (filenum%, sample AS SampleType, signedsampledata%, nextfreedramaddress&, mastervolume!, visualize%)
DECLARE SUB S3MMeasurePattern (filenum%, ignorechannel%(), numnotes%, numrows%, order%, jumptoorder%, jumptorow%)
DECLARE SUB S3MPlayNote (samples() AS SampleType, note AS NoteType, channellastsample%(), globalvolume!)
DECLARE SUB S3MPlayNextRow (samples() AS SampleType, notes() AS NoteType, rownotes%(), nextrow%, nextnote%, channellastsample%(), globalvolume!)

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

CONST TicksPerTIMERUnit# = 1193181# / 65536#
CONST MaxPITRate% = 1500

DIM s3msemitonetable#(15)


SUB S3MFrameWait (frameticks%, lastframetick&, rowticks%, lastrowtick&, tickbase&, samples() AS SampleType, notes() AS NoteType, rownotes%(), nextrow%, nextnote%, channellastsample%(), globalvolume!)
nextframetick& = lastframetick& + frameticks%
nextrowtick& = lastrowtick& + rowticks%

DO
  tick& = INT(TIMER * TicksPerTIMERUnit# + .5) + tickbase&

  IF tick& < lasttick& THEN
    tickbase& = tickbase& + 86400
    tick& = tick& + 86400
  END IF

  WHILE nextrowtick& <= tick&
    S3MPlayNextRow samples(), notes(), rownotes%(), nextrow%, nextnote%, channellastsample%(), globalvolume!

    lastrowtick& = nextrowtick&
    nextrowtick& = lastrowtick& + rowticks%
  WEND

  IF tick& >= nextframetick& THEN
    lastframetick& = nextframetick&

    ' Just in case we stall
    WHILE lastframetick& + frameticks% < tick&
      lastframetick& = lastframetick& + frameticks%
    WEND

    EXIT DO
  END IF
LOOP
END SUB

SUB S3MInitializeTiming (lastframetick&, lastrowtick&, tickbase&)
tick& = INT(TIMER * TicksPerTIMERUnit# + .5) + tickbase&

lastframetick& = tick&
lastrowtick& = tick&
tickbase& = 0
END SUB

SUB S3MJumpToSong (song AS SongType, nextrow%, nextnote%)
nextrow% = song.FirstRowNumber
nextnote% = song.FirstNoteNumber
END SUB

SUB S3MLoad (filenum%, samples() AS SampleType, songs() AS SongType, notes() AS NoteType, rownotes%(), globalvolume!, fps%, frameticks%, rowticks%, tickfrequency%)
SHARED s3msemitonetable#()

' If loading fails, use the provided FPS directly.
tickfrequency% = fps%
frameticks% = 1
rowticks% = 32000

IF s3msemitonetable#(0) = 0 THEN
  FOR i% = 0 TO 15
    s3msemitonetable#(i%) = 2 ^ (i% / 12)
  NEXT i%
END IF

DIM emptysample AS SampleType
DIM emptynote AS NoteType

FOR i% = LBOUND(samples) TO UBOUND(samples)
  samples(i%) = emptysample
NEXT i%

FOR i% = LBOUND(notes) TO UBOUND(notes)
  notes(i%) = emptynote
NEXT i%

FOR i% = LBOUND(rownotes%) TO UBOUND(rownotes%)
  rownotes%(i%) = 0
NEXT i%

ignore$ = INPUT$(28, #filenum%) ' title
signature$ = INPUT$(1, #filenum%)
s3msongtype$ = INPUT$(1, #filenum%)

IF ASC(signature$) <> &H1A THEN EXIT SUB
IF ASC(s3msongtype$) <> &H10 THEN EXIT SUB

ignore$ = INPUT$(2, #filenum%) ' reserved (expected to be 00 00)

numorders% = CVI(INPUT$(2, #filenum%))
numsamples% = CVI(INPUT$(2, #filenum%))
numpatterns% = CVI(INPUT$(2, #filenum%))

IF numsamples% > UBOUND(samples) THEN EXIT SUB

flags% = CVI(INPUT$(2, #filenum%))

ignore$ = INPUT$(2, #filenum%) ' tracker version

sampleformat% = CVI(INPUT$(2, #filenum%))

signedsampledata% = (sampleformat% = 1)

signature$ = INPUT$(4, #filenum%)

IF signature$ <> "SCRM" THEN GOTO failexit

' Global Volume is a variable that can be adjusted during the
' song with Vxx.
'
' Master Volume is an up-front constant that prescales everything.

globalvolume% = ASC(INPUT$(1, #filenum%)) / 64

IF globalvolume% > 64 THEN globalvolume% = 64

globalvolume! = globalvolume% / 64

speed% = ASC(INPUT$(1, #filenum%))
tempo% = ASC(INPUT$(1, #filenum%))

mastervolume% = ASC(INPUT$(1, #filenum%)) AND 127 ' bit 128 == stereo flag

mastervolume! = mastervolume% / 128

' ultra click removal + default pan + reserved[8]
ignore$ = INPUT$(10, #filenum%)

' "additional data" parapointer + channel_settings[32]
ignore$ = INPUT$(2, #filenum%)

' channel settings - channels with the high bit set are disabled
channelsettings$ = INPUT$(32, #filenum%)

DIM ignorechannel%(0 TO 31)

FOR i% = 1 TO 32
  ignorechannel%(i% - 1) = (ASC(MID$(channelsettings$, i%, 1)) AND 128) <> 0
NEXT i%

' Okay, great, we're at the end the header now :-)
orderbytes% = numorders%
sampleptrbytes% = numsamples% * 2
patternptrbytes% = numpatterns% * 2

bytesneeeded% = orderbytes% + sampleptrbytes% + patternptrbytes%

IF LOF(filenum%) - LOC(filenum%) < bytesneeded% THEN GOTO failexit

' Save for later
order$ = INPUT$(numorders%, #filenum%)

sampletableoffset% = LOC(filenum%)

ON LOCAL ERROR RESUME NEXT

nuls$ = STRING$(2, 0)

nextfreedramaddress& = 0

visualize% = -1

FOR i% = 1 TO numsamples%
  sampleptroffset% = sampletableoffset% + 2 * (i% - 1)

  SEEK #filenum%, sampleptroffset%
  sampleparapointer$ = INPUT$(2, #filenum%)

  sampleoffset& = CVL(sampleparapointer$ + nuls$) * 16 + 1

  SEEK #1, sampleoffset&

  S3MLoadSample filenum%, samples(i%), signedsampledata%, nextfreedramaddress&, mastervolume!, visualize%
NEXT i%

numnotes% = 0
numrows% = 0

patterntableoffset% = sampletableoffset% + numsamples% * 2

order% = 1

skiptoorder% = -1
skiptorow% = -1

numnotes% = 0
numrows% = 0

numsongs% = 0

betweensongs% = -1

DIM firstorderrow%(1 TO LEN(order$))

x% = POS(0): y% = CSRLIN
PRINT STRING$(LEN(order$), ".")
x2% = POS(0): y2% = CSRLIN
LOCATE y%, x%

DO WHILE order% <= LEN(order$)
  pattern% = ASC(MID$(order$, order%, 1))

  IF pattern% = 255 THEN
    PRINT ".";
    betweensongs% = -1
  ELSEIF pattern% = 254 THEN
    PRINT "=";
    betweensongs% = -1
  ELSEIF pattern% < 254 THEN
    PRINT "|";

    IF betweensongs% THEN
      IF numnotes% > 0 THEN
        numnotes% = numnotes% + 1
        rownotes%(numrows%) = rownotes%(numrows%) + 1

        notes(numnotes%).NoteAction = NoteActionEndSong%
      END IF

      numsongs% = numsongs% + 1

      songs(numsongs%).FirstRowNumber = numrows% + 1
      songs(numsongs%).FirstNoteNumber = numnotes% + 1

      betweensongs% = 0
    END IF

    patternptroffset% = patterntableoffset% + 2 * pattern%

    SEEK #filenum%, patternptroffset%
    patternparapointer$ = INPUT$(2, #filenum%)

    patternoffset& = CVL(patternparapointer$ + nuls$) * 16 + 1

    SEEK #1, patternoffset&

    IF ERR THEN GOTO failexit

    firstorderrow%(order%) = numrows% + 1

    S3MLoadPattern filenum%, ignorechannel%(), samples(), mastervolume!, notes(), rownotes%(), numnotes%, numrows%, order%, jumptoorder%, jumptorow%
  END IF

  IF jumptoorder% > 0 THEN
    IF jumptoorder% > order% THEN
      order% = jumptoorder%
    ELSE
      numnotes% = numnotes% + 1
      rownotes%(numrows%) = rownotes%(numrows%) + 1

      notes(numnotes%).NoteAction = NoteActionJump%

      ' Calculate absolute row and note numbers from
      ' jumptorow% relative to the target pattern.
      jumptorow% = jumptorow% + firstorderrow%(jumptoorder%)

      jumptonote% = 1

      FOR i% = 1 TO jumptorow% - 1
        jumptonote% = jumptonote% + rownotes%(i%)
      NEXT i%

      ' Pack the values into a spare 32-bit field.
      notes(numnotes%).NoteFrequency = jumptonote% * 16384& + jumptorow% ' naughty overloading

      ' Keep decoding to find next song start
      order% = order% + 1
    END IF

    jumptoorder% = -1
    jumptorow% = -1
  ELSE
    order% = order% + 1
  END IF
LOOP

LOCATE y2%, x2%

' Load succeeded -- calculate timing variables

' S3M tempo is expressed in human-centric BPM, but it's really
' configuring the number of ticks per second. The conversion
' is chosen so that the default tempo of 125 BPM corresponds
' exactly to the PAL vertical refresh rate of 50 Hz (which
' itself comes from the European power grid).
tickspersecond# = tempo% * .4

' S3M speed is really misnamed, because the relationship is
' reciprocal. It is simply the number of ticks per pattern row.
' We need the number of pattern rows per second.
rowspersecond# = tickspersecond# / speed%

' Now we need to find a core timer rate that will generate
' ticks at a rate that can be divided (by an integer) to
' produce exactly rowspersecond# and as close as possible to
' the requested fps%. It also can't be ridiculously large,
' as IRQ 8 firing too quickly will start to impose system
' overhead. We consider integer multiples that are themselves
' close to integers, and which don't exceed MaxPITRate%.

' Usable default in case the loop somehow fails to find
' anything (shouldn't happen).
besttickfrequency% = MaxPITRate%
bestrowticks% = besttickfrequency% / rowspersecond#
bestfpserror# = fps%

rowticks% = -FIX(-fps% / rowspersecond#)

DO
  targetfrequency# = rowspersecond# * rowticks%

  IF targetfrequency# >= MaxPITRate% THEN EXIT DO

  proposedfrequency% = INT(targetfrequency# + .5)

  proposederror# = ABS(targetfrequency# - proposedfrequency%) / rowticks%

  IF proposederror# < .2 THEN
    frameticks% = INT(proposedfrequency% / fps% + .5)

    IF frameticks% >= 1 THEN
      proposedfps# = proposedfrequency% / frameticks%

      IF proposedfps# < fps% THEN
        proposedfpserror# = fps% / proposedfps#
      ELSE
        proposedfpserror# = proposedfps# / fps%
      END IF

      IF proposedfpserror# < bestfpserror# THEN
        besttickfrequency% = proposedfrequency%
        bestrowticks% = rowticks%
        bestfpserror# = proposedfpserror#

        IF bestfpserror# = 1 THEN EXIT DO
      END IF
    END IF
  END IF

  rowticks% = rowticks% + 1
LOOP

tickfrequency% = besttickfrequency%
rowticks% = bestrowticks%
frameticks% = (tickfrequency% + fps% \ 2) \ fps%

EXIT SUB

failexit:
  numsamples% = 0
  numnotes% = 0
  numrows% = 0
END SUB

SUB S3MLoadPattern (filenum%, ignorechannel%(), samples() AS SampleType, mastervolume!, notes() AS NoteType, rownotes%(), numnotes%, numrows%, order%, jumptoorder%, jumptorow%)
SHARED s3msemitonetable#()

packedlen% = CVI(INPUT$(2, #filenum%))
packeddata$ = INPUT$(packedlen% - 2, #filenum%)

haverow% = 0
jumptoorder% = -1

newglobalvol! = -1

i% = 1
rownumber% = 1

firstrowplayed% = jumptorow%
jumptorow% = 0

DO WHILE i% <= LEN(packeddata$)
  rowisplayed% = (rownumber% >= firstrowplayed%)

  what% = ASC(MID$(packeddata$, i%, 1))
  i% = i% + 1

  IF what% = 0 THEN
    IF newglobalvol! >= 0 THEN
      IF rowisplayed% THEN
        IF NOT haverow% THEN
          haverow% = -1
          numrows% = numrows% + 1
        END IF

        numnotes% = numnotes% + 1
        rownotes%(numrows%) = rownotes%(numrows%) + 1

        rowfirstnote% = numnotes% - rownotes%(numrows%) + 1

        FOR i% = numnotes% TO rowfirstnote% + 1 STEP -1
          notes(i%) = notes(i% - 1)
        NEXT i%

        notes(rowfirstnote%).NoteAction = NoteActionSetGlobalVolume%
        notes(rowfirstnote%).Volume = newglobalvol!
      END IF

      newglobalvol! = -1
    END IF

    IF haverow% = 0 THEN numrows% = numrows% + 1 ' empty row

    rownumber% = rownumber% + 1
    haverow% = 0

    IF jumptoorder% >= 0 THEN
      EXIT DO
    END IF
  ELSE
    IF haverow% = 0 THEN
      haverow% = -1
      IF rowisplayed% THEN numrows% = numrows% + 1
    END IF

    effectpresent% = (what% AND 128) <> 0
    volumepresent% = (what% AND 64) <> 0
    notepresent% = (what% AND 32) <> 0

    IF notepresent% THEN notedata$ = MID$(packeddata$, i%, 2): i% = i% + 2
    IF volumepresent% THEN volumedata$ = MID$(packeddata$, i%, 1): i% = i% + 1
    IF effectpresent% THEN effectdata$ = MID$(packeddata$, i%, 2): i% = i% + 2

    channel% = what% AND 31

    IF NOT ignorechannel%(channel%) THEN
      IF rowisplayed% AND (notepresent% OR volumepresent%) THEN
        numnotes% = numnotes% + 1

        rownotes%(numrows%) = rownotes%(numrows%) + 1

        notes(numnotes%).ChannelNumber = channel%
        notes(numnotes%).NoteAction = -1
        notes(numnotes%).Volume = -1

        IF notepresent% THEN
          snote% = ASC(notedata$)

          IF snote% >= 254 THEN
            notes(numnotes%).NoteAction = NoteActionNoteCut%
          ELSE
            sample% = ASC(MID$(notedata$, 2, 1))

            notes(numnotes%).NoteAction = NoteActionStartNote% + sample%

            IF sample% > 0 THEN
              notes(numnotes%).NoteFrequency = samples(sample%).C4Frequency
            ELSE
              ' Will need to scale once we know which sample
              ' it is during playback.
              notes(numnotes%).NoteFrequency = 44100
            END IF

            ' octaves
            FOR o% = &H50 TO snote% STEP &H10
              notes(numnotes%).NoteFrequency = notes(numnotes%).NoteFrequency + notes(numnotes%).NoteFrequency
            NEXT o%
            FOR o% = &H30 TO snote% STEP -&H10
              notes(numnotes%).NoteFrequency = notes(numnotes%).NoteFrequency \ 2
            NEXT o%

            ' semitones
            notes(numnotes%).NoteFrequency = notes(numnotes%).NoteFrequency * s3msemitonetable#(snote% AND 15)
          END IF
        END IF

        IF volumepresent% THEN
          s3mvolume% = ASC(volumedata$)

          IF notes(numnotes%).NoteAction < 0 THEN
            notes(numnotes%).NoteAction = NoteActionSetNoteVolume%
          END IF

          notes(numnotes%).Volume = s3mvolume% * mastervolume! / 64
        END IF
      END IF

      IF effectpresent% THEN
        effect% = ASC(effectdata$)

        SELECT CASE effect%
          CASE 2: ' B - order jump
            jumptoorder% = ASC(MID$(effectdata$, 2, 1)) + 1 ' 1-based
          CASE 3: ' C - skip to next pattern
            jumptoorder% = order% + 1
            jumptorow% = ASC(MID$(effectdata$, 2, 1))

            ' jumptorow% is now "decimal-as-hex"
            tens% = jumptorow% \ 16
            jumptorow% = (jumptorow% AND 15) + tens% * 10
          CASE 22: ' V - set global volume
            globalvolume% = ASC(MID$(effectdata$, 2, 1))
            IF globalvolume% > 64 THEN globalvolume% = 64

            ' This note needs to take effect immediately for the
            ' entire row. So, we need to assign the value to the
            ' first note of the row. Keeping in mind that we
            ' might not actually have any notes on the row at
            ' this point, but might yet see a note. So, we defer
            ' processing to the end of the row.

            newglobalvol! = globalvolume% * mastervolume! / 64
        END SELECT
      END IF
    END IF
  END IF
LOOP
END SUB

SUB S3MLoadSample (filenum%, sample AS SampleType, signedsampledata%, nextfreedramaddress&, mastervolume!, visualize%)
typebyte% = ASC(INPUT$(1, #filenum%))
filename$ = INPUT$(12, #filenum%)

IF typebyte% = 1 THEN
  parapointerhigh$ = INPUT$(1, #filenum%)
  parapointerlow$ = INPUT$(2, #filenum%)

  dataaddressbytes$ = parapointerlow$ + parapointerhigh$ + CHR$(0)

  address& = CVL(dataaddressbytes$) * 16&

  numbytes& = CVL(INPUT$(4, #filenum%))
  loopstartbyte& = CVL(INPUT$(4, #filenum%))
  loopendbyte& = CVL(INPUT$(4, #filenum%))

  defaultvol% = ASC(INPUT$(1, #filenum%))

  reserved$ = INPUT$(1, #filenum%)
  pack$ = INPUT$(1, #filenum%) ' in practice always 0
  flags% = ASC(INPUT$(1, #filenum%))

  frequency& = CVL(INPUT$(4, #filenum%))

  reserved$ = INPUT$(12, #filenum%)
  reserved$ = INPUT$(28, #filenum%) ' title
  reserved$ = INPUT$(4, #filenum%) ' signature "SCRS"

  enablesampleloop% = (flags% AND 1) <> 0
  isstereo% = (flags% AND 2) <> 0
  is16bit% = (flags% AND 4) <> 0

  IF is16bit% THEN
    numbytes& = numbytes& * 2
    loopstartbyte& = loopstartbyte& * 2
    loopendbyte& = loopendbyte& * 2
  END IF

  savedoffset& = LOC(filenum%)

  sample.StartAddress = nextfreedramaddress&
  sample.EndAddress = nextfreedramaddress& + numbytes&
  sample.LoopStartAddress = nextfreedramaddress& + loopstart&
  sample.DefaultVolume = defaultvol% * mastervolume! / 64
  sample.EnableLoop = (flags% AND 1) <> 0
  sample.Is16bitSample = (flags% AND 4) <> 0
  sample.C4Frequency = frequency&
  sample.VisualizationColour = ABS(visualize%)

  SEEK #filenum%, address& + 1

  IF sample.Is16bitSample THEN samplebits% = 16 ELSE samplebits% = 8

  GUSUploadSampleData filenum%, numbytes&, signedsampledata%, samplebits%, isstereo%, nextfreedramaddress&, visualize%

  nextfreedramaddress& = nextfreedramaddress& + numbytes&

  SEEK #filenum%, savedoffset&

  visualize% = ABS(visualize%) + 1

  IF visualize% = 7 THEN visualize% = 9
  IF visualize% = 15 THEN visualize% = 1
END IF
END SUB

SUB S3MMeasure (filenum%, numsamples%, numsongs%, numnotes%, numrows%)
ignore$ = INPUT$(28, #filenum%) ' title
signature$ = INPUT$(1, #filenum%)
s3msongtype$ = INPUT$(1, #filenum%)

IF ASC(signature$) <> &H1A THEN GOTO failexit
IF ASC(s3msongtype$) <> &H10 THEN GOTO failexit

ignore$ = INPUT$(2, #filenum%) ' reserved (expected to be 00 00)

numorders% = CVI(INPUT$(2, #filenum%))
numsamples% = CVI(INPUT$(2, #filenum%))
numpatterns% = CVI(INPUT$(2, #filenum%))

flags% = CVI(INPUT$(2, #filenum%))

ignore$ = INPUT$(2, #filenum%) ' tracker version

sampleformat% = CVI(INPUT$(2, #filenum%))

signature$ = INPUT$(4, #filenum%)

IF signature$ <> "SCRM" THEN GOTO failexit

' global volume + initial speed + tempo + master volume
' + ultra click removal + default pan + reserved[8]
ignore$ = INPUT$(14, #filenum%)

' "additional data" parapointer + channel_settings[32]
ignore$ = INPUT$(2, #filenum%)

' channel settings - channels with the high bit set are disabled
channelsettings$ = INPUT$(32, #filenum%)

DIM ignorechannel%(0 TO 31)

FOR i% = 1 TO 32
  ignorechannel%(i% - 1) = (ASC(MID$(channelsettings$, i%, 1)) AND 128) <> 0
NEXT i%

' Okay, great, we're at the end the header now :-)
orderbytes% = numorders%
sampleptrbytes% = numsamples% * 2
patternptrbytes% = numpatterns% * 2

bytesneeeded% = orderbytes% + sampleptrbytes% + patternptrbytes%

IF LOF(filenum%) - LOC(filenum%) < bytesneeded% THEN GOTO failexit

order$ = INPUT$(numorders%, #filenum%)

ignore$ = INPUT$(sampleptrbytes%, #filenum%) ' not pertinent to measuring

numnotes% = 0
numrows% = 0

patterntableoffset% = LOC(filenum%)

nuls$ = STRING$(2, 0)

ON LOCAL ERROR RESUME NEXT

order% = 1
numsongs% = 0

betweensongs% = -1

DO WHILE order% <= LEN(order$)
  pattern% = ASC(MID$(order$, order%, 1))

  IF pattern% >= 254 THEN
    betweensongs% = -1
  ELSE
    IF betweensongs% THEN
      ' We may need a separate note record to end the song.
      numnotes% = numnotes% + 1

      betweensongs% = 0
      numsongs% = numsongs% + 1
    END IF

    patternptroffset% = patterntableoffset% + 2 * pattern%

    SEEK #filenum%, patternptroffset%
    patternparapointer$ = INPUT$(2, #filenum%)

    patternoffset& = CVL(patternparapointer$ + nuls$) * 16 + 1

    SEEK #1, patternoffset&

    IF ERR THEN GOTO failexit

    S3MMeasurePattern filenum%, ignorechannel%(), numnotes%, numrows%, order%, jumptoorder%, jumptorow%
  END IF

  IF jumptoorder% >= 0 THEN
    IF jumptoorder% > order% THEN
      order% = jumptoorder%
    ELSE
      ' just in case we need a dedicated note (only thing in row)
      numnotes% = numnotes% + 1

      order% = order% + 1
    END IF

    jumptoorder% = -1
  ELSE
    order% = order% + 1
  END IF
LOOP

EXIT SUB

failexit:
  numsamples% = 0
  numnotes% = 0
  numrows% = 0
END SUB

SUB S3MMeasurePattern (filenum%, ignorechannel%(), numnotes%, numrows%, order%, jumptoorder%, jumptorow%)
packedlen% = CVI(INPUT$(2, #filenum%))
packeddata$ = INPUT$(packedlen% - 2, #filenum%)

haverow% = 0
jumptoorder% = -1

i% = 1
rownumber% = 1

DO WHILE i% <= LEN(packeddata$)
  what% = ASC(MID$(packeddata$, i%, 1))
  i% = i% + 1

  IF what% = 0 THEN
    IF haverow% = 0 THEN numrows% = numrows% + 1 ' empty row

    rownumber% = rownumber% + 1
    haverow% = 0

    IF jumptoorder% >= 0 THEN EXIT DO
  ELSE
    rowisplayed% = (rownumber% >= jumptorow%)

    IF haverow% = 0 THEN
      haverow% = -1
      IF rowisplayed% THEN numrows% = numrows% + 1
    END IF

    effectpresent% = (what% AND 128) <> 0
    volumepresent% = (what% AND 64) <> 0
    notepresent% = (what% AND 32) <> 0

    IF notepresent% THEN notedata$ = MID$(packeddata$, i%, 2): i% = i% + 2
    IF volumepresent% THEN volumedata$ = MID$(packeddata$, i%, 1): i% = i% + 1
    IF effectpresent% THEN effectdata$ = MID$(packeddata$, i%, 2): i% = i% + 2

    channel% = what% AND 31

    IF NOT ignorechannel%(channel%) THEN
      IF rowisplayed% THEN
        IF notepresent% OR volumepresent% THEN numnotes% = numnotes% + 1

        IF effectpresent% THEN
          effect% = ASC(effectdata$)

          SELECT CASE effect%
            CASE 2: ' B - order jump
              numnotes% = numnotes% + 1
              jumptoorder% = ASC(MID$(effectdata$, 2, 1)) + 1 ' 1-based
            CASE 3: ' C - skip to next pattern
              numnotes% = numnotes% + 1

              jumptoorder% = order% + 1
              jumptorow% = ASC(MID$(effectdata$, 2, 1))

              ' jumptorow% is now "decimal-as-hex"
              tens% = jumptorow% \ 16
              jumptorow% = (jumptorow% AND 15) + tens% * 10
            CASE 22: ' V - set global volume
              numnotes% = numnotes% + 1
          END SELECT
        END IF
      END IF
    END IF
  END IF
LOOP

jumptorow% = 0
END SUB

SUB S3MPlayNextRow (samples() AS SampleType, notes() AS NoteType, rownotes%(), nextrow%, nextnote%, channellastsample%(), globalvolume!)
jumptorow% = nextrow% + 1

FOR i% = 1 TO rownotes%(nextrow%)
  S3MPlayNote samples(), notes(nextnote%), channellastsample%(), globalvolume!

  IF notes(nextnote%).NoteAction = NoteActionJump% THEN
    ' Unpack jump target values.
    jumptorow% = notes(nextnote%).NoteFrequency AND 16383
    nextnote% = notes(nextnote%).NoteFrequency \ 16384
  ELSE
    nextnote% = nextnote% + 1
  END IF
NEXT i%

nextrow% = jumptorow%
END SUB

SUB S3MPlayNote (samples() AS SampleType, note AS NoteType, channellastsample%(), globalvolume!)
SELECT CASE note.NoteAction
  CASE NoteActionSetGlobalVolume%:
    globalvolume! = note.Volume
  CASE NoteActionNoteCut%:
    GUSStopNote note.ChannelNumber
  CASE NoteActionSetNoteVolume%:
    GUSSetVoiceVolume note.ChannelNumber, note.Volume * globalvolume!
  CASE NoteActionStartNote% TO NoteActionStartNoteLast%:
    samplenum% = note.NoteAction - NoteActionStartNote%
    notefreq& = note.NoteFrequency

    IF samplenum% > 0 THEN
      channellastsample%(note.ChannelNumber) = samplenum%
    ELSE
      samplenum% = channellastsample%(note.ChannelNumber)

      IF samplenum% > 0 THEN
        ' Can't calculate ahead of time when we don't know the sample number yet.
        notefreq& = samples(samplenum%).C4Frequency * (note.NoteFrequency / 44100#)
      END IF
    END IF

    IF samplenum% > 0 THEN
      notevolume! = note.Volume

      IF notevolume! < 0 THEN notevolume! = samples(samplenum%).DefaultVolume

      is16bit% = samples(samplenum%).Is16bitSample
      startaddr& = samples(samplenum%).StartAddress
      endaddr& = samples(samplenum%).EndAddress
      loopsample% = samples(samplenum%).EnableLoop

      GUSPlayNote note.ChannelNumber, notefreq&, notevolume!, is16bit%, startaddr&, endaddr&, loopsample%
    END IF
END SELECT
END SUB

SUB S3MProgramTimerRate (tickspersecond#)
CONST BaseFrequency& = 1193181

interval& = BaseFrequency& / tickspersecond#

OUT &H43, &H36
OUT &H40, interval&
OUT &H40, interval& \ 256
END SUB

SUB S3MRowUpdateCheck (rowticks%, lastrowtick&, tickbase&, samples() AS SampleType, notes() AS NoteType, rownotes%(), nextrow%, nextnote%, channellastsample%(), globalvolume!)
tick& = INT(TIMER * TicksPerTIMERUnit# + .5) + tickbase&

IF tick& < lastrowtick& THEN
  tickbase& = tickbase& + 86400
  tick& = tick& + 86400
END IF

nextrowtick& = lastrowtick& + rowticks%

WHILE nextrowtick& <= tick&
  S3MPlayNextRow samples(), notes(), rownotes%(), nextrow%, nextnote%, channellastsample%(), globalvolume!

  lastrowtick& = nextrowtick&
  nextrowtick& = lastrowtick& + rowticks%
WEND
END SUB

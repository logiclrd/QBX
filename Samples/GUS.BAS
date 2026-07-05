DECLARE SUB ResetGUS ()
DECLARE SUB CreateSample ()
DECLARE SUB PlayNote (voicenumber%, notefreq&)
DECLARE SUB StopNote (voicenumber%)
DECLARE SUB ProcessSongLine (songline$, notelength%)

CLS

ResetGUS

CreateSample

divider$ = STRING$(20, "-")

DIM order%(50)

FOR i% = 1 TO 50
  READ order%(i%)
  IF order%(i%) < 0 THEN EXIT FOR
NEXT i%

currentorder% = 0
notelength% = 3

DO
  st! = TIMER

  ' Play song line
  READ songline$

  IF songline$ = "=" THEN
    PRINT divider$

    currentorder% = currentorder% + 1

    SELECT CASE order%(currentorder%)
      CASE 1: RESTORE pat1
      CASE 2: RESTORE pat2
      CASE 3: RESTORE pat3
      CASE 4: RESTORE pat4

      CASE -1: EXIT DO
    END SELECT

    READ songline$
  END IF

  ProcessSongLine songline$, notelength%

  PRINT songline$

  ' Note delay: N ticks, including the one we've been using.
  WHILE st! = TIMER: WEND

  FOR i% = 2 TO notelength%
    st! = TIMER
    WHILE st! = TIMER: WEND
  NEXT i%
LOOP WHILE INKEY$ = ""

FOR i% = 0 TO 13
  StopNote i%
NEXT i%

order:
DATA 1,1,2,1,3,1,3,1,4,-1

DATA "="

pat1:
DATA "C-4 ---"
DATA "G-4 ---"
DATA "C-5 Eb5"
DATA "G-4 |--"
DATA "A#4 D-5"
DATA "C-5 Eb5"
DATA "G-4 |--"
DATA "A#3 |--"

DATA "C-4 ---"
DATA "G-4 ---"
DATA "C-5 Eb5"
DATA "G-4 |--"
DATA "A#4 D-5"
DATA "C-5 Eb5"
DATA "G-4 |--"
DATA "A#3 |--"

DATA "C-4 ---"
DATA "G-4 ---"
DATA "C-5 Eb5"
DATA "G-4 |--"
DATA "A#4 D-5"
DATA "C-5 Eb5"
DATA "G-4 |--"
DATA "A#3 |--"

DATA "C-4 ---"
DATA "G-4 ---"
DATA "C-5 Eb5"
DATA "G-4 |--"
DATA "A#4 D-5"
DATA "C-5 Eb5"
DATA "G-4 |--"
DATA "A#3 |--"

DATA "="

pat2:
DATA "C-4 ---"
DATA "G-4 ---"
DATA "Ab4 Eb5"
DATA "G-4 |--"
DATA "Ab4 D-5"
DATA "Ab4 Eb5"
DATA "G-4 |--"
DATA "A#3 |--"

DATA "C-4 ---"
DATA "G-4 ---"
DATA "Ab4 Eb5"
DATA "G-4 |--"
DATA "Ab4 D-5"
DATA "Ab4 Eb5"
DATA "G-4 |--"
DATA "A#3 |--"

DATA "C-4 ---"
DATA "G-4 ---"
DATA "Ab4 Eb5"
DATA "G-4 |--"
DATA "Ab4 D-5"
DATA "Ab4 Eb5"
DATA "G-4 |--"
DATA "A#3 |--"

DATA "C-4 ---"
DATA "G-4 ---"
DATA "Ab4 Eb5"
DATA "G-4 |--"
DATA "Ab4 D-5"
DATA "Ab4 Eb5"
DATA "G-4 |--"
DATA "A#3 |--"

DATA "="

pat3:
DATA "F-3 ---"
DATA "C-4 ---"
DATA "Ab4 Eb5"
DATA "C-4 |--"
DATA "Ab4 D-5"
DATA "Ab4 Eb5"
DATA "C-4 |--"
DATA "Eb3 |--"

DATA "F-3 ---"
DATA "C-4 ---"
DATA "Ab4 Eb5"
DATA "C-4 |--"
DATA "Ab4 D-5"
DATA "Ab4 Eb5"
DATA "C-4 |--"
DATA "Eb3 |--"

DATA "Ab3 ---"
DATA "C-4 ---"
DATA "G-4 Eb5"
DATA "C-4 |--"
DATA "G-4 D-5"
DATA "G-4 Eb5"
DATA "C-4 |--"
DATA "G-3 |--"

DATA "Ab3 ---"
DATA "C-4 ---"
DATA "G-4 Eb5"
DATA "C-4 |--"
DATA "F-4 D-5"
DATA "G-5 Eb5"
DATA "C-4 |--"
DATA "A#3 |--"

DATA "="

pat4:
DATA "C-4 ---"
DATA "|-- ---"
DATA "C-4 ---"
DATA "|-- ---"
DATA "|-- ---"
DATA "--- ---"
DATA "--- ---"
DATA "--- ---"

DATA "C-4 --- --- N1-"
DATA "|-- G-4 --- ---"
DATA "|-- |-- C-5 ---"
DATA "|-- |-- |-- Eb5"
DATA "N5- |-- |-- |--"
DATA "|-- |-- |-- |--"
DATA "|-- |-- |-- |--"
DATA "|-- |-- |-- |--"
DATA "|-- |-- |-- |--"
DATA "--- --- --- ---"

DATA "--- ---"
DATA "--- ---"
DATA "--- ---"
DATA "--- ---"
DATA "--- ---"

DATA "="

SUB CreateSample
' RAM IO address high -- always 0 in this case
OUT &H343, &H44
OUT &H344, 0
OUT &H345, 0

' RAM IO address low -- will be written repeatedly in the loop
OUT &H343, &H43

' Write a 100 Hz (at 44100 Hz) sine wave to the start of DRAM.
FOR i% = 0 TO 440
  a% = SIN(i% * 6.283185 / 441) * 25000
  b$ = MKI$(a%)

  ' RAM IO address low
  OUT &H344, (i% + i%) AND 255
  OUT &H345, (i% + i%) \ 256

  ' RAM IO
  OUT &H347, ASC(b$)

  ' RAM IO address low
  OUT &H344, (i% + i% + 1) AND 255
  OUT &H345, (i% + i% + 1) \ 256

  OUT &H347, ASC(MID$(b$, 2))
NEXT i%
END SUB

SUB PlayNote (voicenumber%, notefreq&)
' With 14 voices, the output sample rate will be 44,100.
' The frequency control for a voice is the ratio between
' the desired playback frequency and the output sample
' rate, as a fixed-point number with scale 512.

notefreqcontrol& = INT(.5# + notefreq& * 512# / 44100)

' The frequency control register stores its value in bits 15:1.
' Bit 0 is unused.
notefreqcontrol& = notefreqcontrol& * 2

' Start and end address: The registers storing these values
' use a fixed-point representation that matches the
' scale of the frequency control word.

startaddress& = 0 * 512&
endaddress& = 441 * 512&

' The volume register stores an extra 4 bits of fractional precision
' for smooth ramping.
volume& = 4095 * 16&

' Select Voice
OUT &H342, voicenumber%

' Voice: Start address
OUT &H343, 2 ' high
OUT &H344, startaddress& \ 65536
OUT &H345, startaddress& \ 16777216

OUT &H343, 3 ' low
OUT &H344, startaddress& AND 255
OUT &H345, startaddress& \ 256

' Voice: End address
OUT &H343, 4 ' high
OUT &H344, endaddress& \ 65536
OUT &H345, endaddress& \ 16777216

OUT &H343, 5 ' low
OUT &H344, endaddress& AND 255
OUT &H345, endaddress& \ 256

' Voice: Frequency
OUT &H343, 1
OUT &H344, notefreqcontrol& AND 255
OUT &H345, notefreqcontrol& \ 256

' Voice: Volume
OUT &H343, 9
OUT &H344, volume& AND 255
OUT &H345, volume& \ 256

' Voice: Volume Ramp High End
OUT &H343, 8
OUT &H344, volume& AND 255
OUT &H345, volume& \ 256

'volume& = volume& - 192 ' 12% volume -- it's logarithmic
volume& = 0

' Voice: Volume Ramp Low End
OUT &H343, 7
OUT &H344, volume& AND 255
OUT &H345, volume& \ 256

' Voice: Volume Ramp Rate
OUT &H343, 6
OUT &H345, 4 OR 128 ' subtract 4 from volume on every 64th sample

' Voice: Volume Ramp Control
' - Bit 0: Stop when set
' - Bit 1: Force voice stop
' - Bit 2: (Rollover condition for sample offset)
' - Bit 3: Loop
' - Bit 4: Ping Pong Loop
' - Bit 5: Enable IRQ on end volume
' - Bit 6: Currently ramping down (pong)
' - Bit 7: IRQ pending
vc% = 64 ' backward, not stopped

OUT &H343, &HD
OUT &H345, vc%

' Voice: Panning
OUT &H343, &HC
OUT &H345, 7 ' just left of centre

' Voice: Control
' - Bit 0: Stop when set
' - Bit 1: Force voice stop
' - Bit 2: 16-bit data
' - Bit 3: Loop
' - Bit 4: Ping Pong Loop
' - Bit 5: Enable IRQ on end address
' - Bit 6: Currently playing backward (pong)
' - Bit 7: IRQ pending
vc% = 4 OR 8

OUT &H343, 0
OUT &H345, vc%
END SUB

SUB ProcessSongLine (songline$, notelength%)
i% = 1
voicenumber% = 0

DO WHILE i% + 2 <= LEN(songline$)
  noteoff% = 0
  continue% = 0

  SELECT CASE MID$(songline$, i%, 1)
    CASE "C": basenote% = 0
    CASE "D": basenote% = 2
    CASE "E": basenote% = 4
    CASE "F": basenote% = 5
    CASE "G": basenote% = 7
    CASE "A": basenote% = 9
    CASE "B": basenote% = 11

    CASE "-": noteoff% = -1
    CASE "|": continue% = -1

    CASE "N": notelength% = VAL(MID$(songline$, i% + 1, 1))

    CASE ELSE: EXIT DO
  END SELECT

  IF noteoff% THEN
    StopNote voicenumber%
  ELSEIF NOT continue% THEN
    SELECT CASE MID$(songline$, i% + 1, 1)
      CASE "b": basenote% = basenote% - 1
      CASE "#": basenote% = basenote% + 1
    END SELECT

    octave% = ASC(MID$(songline$, i% + 2, 1)) - 48

    ' Frequences for octave 9. Hardcode values rather than doing actual math. :-)
    SELECT CASE basenote%
      CASE 0: frequency& = 8372
      CASE 1: frequency& = 8870
      CASE 2: frequency& = 9397
      CASE 3: frequency& = 9956
      CASE 4: frequency& = 10548
      CASE 5: frequency& = 11175
      CASE 6: frequency& = 11840
      CASE 7: frequency& = 12544
      CASE 8: frequency& = 13290
      CASE 9: frequency& = 14080
      CASE 10: frequency& = 14917
      CASE 11: frequency& = 15804
    END SELECT

    frequency& = frequency& * 441

    FOR o% = octave% TO 8
      frequency& = frequency& \ 2 ' If only this were a shift :-)
    NEXT o%

    PlayNote voicenumber%, frequency&
  END IF

  voicenumber% = voicenumber% + 1

  i% = i% + 3
  IF i% > LEN(songline$) THEN EXIT DO

  WHILE ASC(MID$(songline$, i%, 1)) = 32
    i% = i% + 1
    IF i% > LEN(songline$) THEN EXIT DO
  WEND
LOOP
END SUB

SUB ResetGUS
' Reset
st! = TIMER: WHILE TIMER = st!: WEND

OUT &H343, &H4C
OUT &H345, 0

' Wait 20ms, which is actually 55ms because TIMER only changes that frequently.
st! = TIMER: WHILE TIMER = st!: WEND

' Enable GF1. This clears bits 2 and 4, so we can't enable the DAC yet.
OUT &H345, 1
a% = INP(&H345)

' Now also enable the DAC
OUT &H345, 3
a% = INP(&H345)

' Max Active Voice Number
OUT &H343, &HE
OUT &H345, 13 OR 192 ' top 2 bits must be set

' Mix: Enable line out
'OUT &H240, 1
END SUB

SUB StopNote (voicenumber%)
' Select Voice
OUT &H342, voicenumber%

' Voice: Control
' - Bit 0: Stop when set
' - Bit 1: Force voice stop
' - Bit 2: 16-bit data
' - Bit 3: Loop
' - Bit 4: Ping Pong Loop
' - Bit 5: Enable IRQ on end address
' - Bit 6: Currently playing backward (pong)
' - Bit 7: IRQ pending
vc% = 1 OR 2

OUT &H343, 0
OUT &H345, vc%
END SUB

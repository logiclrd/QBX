DECLARE FUNCTION GUSEncodeVolume% (linearvolume!)

FUNCTION GUSEncodeVolume% (linearvolume!)
linearvolume& = linearvolume! * 65536

IF linearvolume& > 65535 THEN linearvolume& = 65535

IF linearvolume& = 0 THEN
  ' GUS volume 0 is special-cased to silence
  GUSEncodeVolume% = 0
ELSE
  ' GUS volumes other than 0 are stored using a logarithmic
  ' scale. The starting point is a scale that increases by
  ' 10 dB every 256 steps, increasing evenly for every step.
  ' But to simplify implementation at the hardware level,
  ' they decided to use linear volume for each 256 steps,
  ' doubling the size of the step each time it moves to a
  ' new "exponent".
  '
  ' Thus, to accurately convert a linear volume such as
  ' those used by S3M to volume levels as used by the GUS,
  ' you need to first convert the linear volume to a
  ' logarithmic volume with the correct scale, and then
  ' translate that to the GUS' exponent and "mantissa".
  '
  ' First, we convert the linear volume to a range where
  ' a logarithm base 2 will return the correct exponent
  ' value directly. Then, having identified the exponent,
  ' we independently compute the mantissa from the linear
  ' volume based on the range selected by the exponent.

  logvolume# = LOG(linearvolume&) / LOG(2)

  exponent% = INT(logvolume#)

  loglow& = 2 ^ exponent%
  loghigh& = 2 * loglow&

  mantissa% = (linearvolume& - loglow&) * 256 \ (loghigh& - loglow&)

  GUSEncodeVolume% = exponent% * 256 + mantissa%
END IF
END FUNCTION

SUB GUSPlayNote (voicenumber%, notefreq&, volume!, is16bit%, startaddress&, endaddress&, enableloop%)
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
'
' The values are counted in samples, meaning they're bytes
' when the sample is 8-bit and words when the sample is
' 16-bit.
IF is16bit% THEN
  startaddressscaled& = startaddress& * 256&
  endaddressscaled& = endaddress& * 256&
ELSE
  startaddressscaled& = startaddress& * 512&
  endaddressscaled& = endaddress& * 512&
END IF

' The volume register stores an extra 4 bits of fractional precision
' for smooth ramping.
volume& = GUSEncodeVolume%(.5 * volume!) * 16&

' Select Voice
OUT &H342, voicenumber%

' Voice: Start address
OUT &H343, 2 ' high
OUT &H344, startaddressscaled& \ 65536
OUT &H345, startaddressscaled& \ 16777216

OUT &H343, 3 ' low
OUT &H344, startaddressscaled& AND 255
OUT &H345, startaddressscaled& \ 256

' Voice: End address
OUT &H343, 4 ' high
OUT &H344, endaddressscaled& \ 65536
OUT &H345, endaddressscaled& \ 16777216

OUT &H343, 5 ' low
OUT &H344, endaddressscaled& AND 255
OUT &H345, endaddressscaled& \ 256

' Voice: Current address
OUT &H343, 10 ' high
OUT &H344, startaddressscaled& \ 65536
OUT &H345, startaddressscaled& \ 16777216

OUT &H343, 11 ' low
OUT &H344, startaddressscaled& AND 255
OUT &H345, startaddressscaled& \ 256

' Voice: Frequency
OUT &H343, 1
OUT &H344, notefreqcontrol& AND 255
OUT &H345, notefreqcontrol& \ 256

' Voice: Volume
OUT &H343, 9
OUT &H344, volume& AND 255
OUT &H345, volume& \ 256

' Voice: Volume Ramp Control
' - Bit 0: Stop when set
' - Bit 1: Force voice stop
' - Bit 2: (Rollover condition for sample offset)
' - Bit 3: Loop
' - Bit 4: Ping Pong Loop
' - Bit 5: Enable IRQ on end volume
' - Bit 6: Currently ramping down (pong)
' - Bit 7: IRQ pending
vc% = 1 OR 2 ' very stopped

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
vc% = 0

IF is16bit% THEN vc% = vc% OR 4
IF enableloop% THEN vc% = vc% OR 8

OUT &H343, 0
OUT &H345, vc%
END SUB

SUB GUSReset
SHARED gusvolumetable%()

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
OUT &H240, 1
END SUB

SUB GUSSetVoiceVolume (voicenumber%, volume!)
volume% = GUSEncodeVolume%(volume!)

' Select Voice
OUT &H342, voicenumber%

' Voice: Volume
OUT &H343, 9
OUT &H344, volume& AND 255
OUT &H345, volume& \ 256
END SUB

SUB GUSStopNote (voicenumber%)
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

SUB GUSUploadSampleData (filenum%, numbytes&, signedsampledata%, samplebits%, stereodownmix%, address&, visualize%)
STATIC vislastchar%

IF visualize% < 0 THEN
  COLOR 7, 0
  CLS
  PRINT "GUS Memory"
  PRINT "00000"
  PRINT STRING$(80 * 8, 249)
  PRINT TAB(76); "fffff";

  visualize% = -visualize%
  vislastchar% = -1
END IF

IF visualize% THEN COLOR visualize%

' RAM IO address low -- will be written repeatedly in the loop
OUT &H343, &H43

curaddresshigh% = -1

bytespersample& = samplebits% \ 8

buffer$ = SPACE$(bytespersample&)

shouldbesigned% = (samplebits% = 16)
needsignflip% = (signedsampledata% <> 0) XOR shouldbesigned%

IF needsignflip% THEN
  SELECT CASE samplebits%
    CASE 8: signflip% = &H80
    CASE 16: signflip% = &H8000
  END SELECT
ELSE
  signflip% = 0
END IF

numchannels% = 1
IF stereodownmix% THEN numchannels% = 2

sampleoffset& = LOC(filenum%)

channeloffset& = numbytes& \ 2

WHILE numbytes& > 0
  ' Read sample data
  GET #filenum%, sampleoffset&, buffer$
  numbytes& = numbytes& - bytespersample&

  IF samplebits% = 16 THEN samplevalue% = CVI(buffer$) ELSE samplevalue% = ASC(buffer$)

  samplevalue% = samplevalue% XOR signflip%

  IF stereodownmix% THEN
    GET #filenum%, sampleoffset& + channeloffset&, buffer$
    numbytes& = numbytes& - bytespersample&

    IF samplebits% = 16 THEN samplevalue2% = CVI(buffer$) ELSE samplevalue2% = ASC(buffer$)

    samplevalue2% = samplevalue2% XOR signflip%

    samplevalue% = (samplevalue% + 0& + samplevalue2%) \ 2
  END IF

  SELECT CASE samplebits%
    CASE 8: samplebytes$ = CHR$(samplevalue%)
    CASE 16: samplebytes$ = MKI$(samplevalue%)
  END SELECT

  FOR i% = 1 TO LEN(samplebytes$)
    addresshigh% = address& \ 65536

    IF addresshigh% <> curaddresshigh% THEN
      ' RAM IO address high
      OUT &H343, &H44
      OUT &H345, addresshigh% AND 255

      curaddresshigh% = addresshigh%

      ' RAM IO address low
      OUT &H343, &H43
    END IF

    ' RAM IO address low
    OUT &H344, address& AND 255
    OUT &H345, (address& \ 256) AND 255

    ' RAM IO
    IF msboffset% = msbinterval% THEN
      msboffset% = 0
    ELSE
      msboffset% = msboffset% + 1
    END IF

    OUT &H347, ASC(MID$(samplebytes$, i%, 1))

    address& = address& + 1
  NEXT i%

  sampleoffset& = sampleoffset& + bytespersample&

  IF visualize% THEN
    vischar% = address& * 5 \ 8192 ' map to range of chars across 8 rows

    IF vischar% <> vislastchar% THEN
      vislastchar% = vischar%

      visrow% = vischar% \ 80
      viscol% = vischar% MOD 80

      LOCATE visrow% + 3, viscol% + 1
      PRINT CHR$(254);
      LOCATE 12, 1
    END IF
  END IF
WEND

IF visualize% THEN COLOR 7
END SUB

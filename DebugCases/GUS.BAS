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

' Max Active Voice Number
OUT &H343, &HE
OUT &H345, 13 OR 192 ' top 2 bits must be set

' With 14 voices, the output sample rate will be 44,100.
' The frequency control for a voice is the ratio between
' the desired playback frequency and the output sample
' rate, as a fixed-point number with scale 512.

outputfreq& = 44100
notefreq& = 44100

notefreqcontrol& = INT(.5# + notefreq& * 512# / outputfreq&)

' The frequency control register stores its value in bits 15:1.
' Bit 0 is unused.
notefreqcontrol& = notefreqcontrol& * 2

' Start and end address: The registers storing these values
' use a fixed-point representation that matches the
' scale of the frequency control word.

startaddress& = 0 * 512&
endaddress& = 441 * 512&

volumeexponent% = &HF
volumemantissa% = &HFF

volume& = volumeexponent% * 256 + volumemantissa%

' The volume register stores an extra 4 bits of fractional precision
' for smooth ramping.
volume& = volume& * 16

' Select Voice 0
OUT &H342, 0

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

' Voice: Panning
OUT &H343, &HC
OUT &H345, 7 ' just left of centre

' Mix: Enable line out
OUT &H240, 1

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

PRINT "Press any key to stop"

SLEEP

vc% = 1 OR 2

OUT &H345, vc%


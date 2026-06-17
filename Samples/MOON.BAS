TYPE LZWEntryType
  EntryLength AS INTEGER
  EntryPrefix AS INTEGER
  EntrySuffix AS INTEGER
END TYPE

DECLARE SUB DecodeLZW (d$, framesize%, resultbuffer%())
DECLARE SUB EnableBrightBackground ()
DECLARE SUB SetCharacterWidth8 ()
DECLARE SUB SetGradientPalette ()
DECLARE FUNCTION DecodeBase85Data$ ()
DECLARE FUNCTION ReadBits% (a$, idx%, bits%, byte%, shift%, sublen%)

DIM framebuffer%(624)

lzw$ = DecodeBase85Data$

DecodeLZW lzw$, 625, framebuffer%()

SetCharacterWidth8
EnableBrightBackground
SetGradientPalette

CLS

cch$ = STRING$(2, 32) + STRING$(2, 176) + STRING$(2, 177) + STRING$(2, 178)

maxc% = 0
FOR i% = 0 TO UBOUND(framebuffer%)
  IF framebuffer%(i%) > maxc% THEN maxc% = framebuffer%(i%)
NEXT i%

o% = 0

FOR y% = 0 TO 24
  IF y% > 0 THEN PRINT

  FOR x% = 0 TO 24
    c% = framebuffer%(o%) * 63 \ maxc%

    cg% = c% \ 4

    IF cg% >= 15 THEN
      COLOR 31, 7
      PRINT STRING$(2, 219);
    ELSE
      fg% = cg% + 1
      bg% = cg%

      IF bg% >= 8 THEN
        fg% = fg% + 16
        bg% = bg% - 8
      END IF

      COLOR fg%, bg%
      PRINT MID$(cch$, (c% AND 3) * 2 + 1, 2);
    END IF

    o% = o% + 1
  NEXT x%
NEXT y%

SLEEP

DATA "#>G,+Q?4*U'iP2Q(>M/wNXZ$$:1srJbK[967W&`EB&*Hu'N,%e7bh*l&lV5JesKic$5<K`"
DATA "2J-BTM+mv(11Ui&BIn,^:O`LN=+q)+&E#lZ?r.DISs62-???fheBK-TOAZV^E+%%I=e2#:"
DATA "7g,@O&kkfBYs]N$$VmUgL4gOV$;I2C>#k^f#HV[\Yw+#CR:WLn'i]jU`dBF#R%U79DJ%jW"
DATA "M2cRf3aH\&nj+__)g-efLMT[wA)>`udC>-R%T',CK$lnpg9u>8Bq[q$8:tm,GZ:'d*B\0="
DATA "a:%3&/v%['%NoOKSph9TcS2h,;K2tNmUIZ^(')Oq&CRNBo;Ubs8t&dqhYtm5,[h*Qa@=PW"
DATA "Z^/&TpU6`N)--o:8Bi9Sg#1q-r0[W=t$+W?)GpO?EX`_wL=3L]qJ#].3*^MC'ZE@H#,,(D"
DATA "*?5Akc:v$@93$ct&PrLsCY]P>,6U_M%kwC'+'aOZ-,.u[ENb_uQm@c%XZQiWQbJRI(#@nv"
DATA "C`m,(=&hE=",""

FUNCTION DecodeBase85Data$
b85$ = ""

DO
  READ b85part$
  b85$ = b85$ + b85part$
LOOP WHILE LEN(b85part$)

FOR i% = 1 TO LEN(b85$) STEP 5
  l& = 0

  FOR j% = 0 TO 4
    l& = l& * 85 + (ASC(MID$(b85$, i% + j%, 1)) - 35)
  NEXT j%

  lzw$ = lzw$ + MKL$(l&)
NEXT i%

DecodeBase85Data$ = lzw$
END FUNCTION

SUB DecodeLZW (d$, framesize%, resultbuffer%())
idx% = 1

keysize% = ASC(MID$(d$, idx%, 1))

clearkey% = 2 ^ keysize%
stopkey% = clearkey% + 1

initbulk% = 2 ^ (keysize% + 1)
IF initbulk% < 256 THEN initbulk% = 256

DIM table(initbulk%) AS LZWEntryType
DIM entry AS LZWEntryType

numtableentries% = 2 ^ keysize% + 2

FOR i% = 0 TO numtableentries% - 2
  table(i%).EntryLength = 1
  table(i%).EntryPrefix = &HFFF
  table(i%).EntrySuffix = i%
NEXT i%

keysize% = keysize% + 1
initkeysize% = keysize%
sublen% = 0
shift% = 0

keytype% = ReadBits&(d$, idx%, keysize%, byte%, shift%, sublen%)

frameoffset% = 0
ret% = 0

DO WHILE frameoffset% < framesize%
  IF keytype% = clearkey% THEN
    keysize% = initkeysize%
    numtableentries% = 2 ^ (keysize% - 1) + 2
    tableisfull% = 0
  ELSEIF NOT tableisfull% THEN
    IF numtableentries% > UBOUND(table) THEN
      REDIM PRESERVE table(UBOUND(table) * 2)
    END IF

    table(numtableentries%).EntryLength = strlen% + 1
    table(numtableentries%).EntryPrefix = keytype%
    table(numtableentries%).EntrySuffix = entry.EntrySuffix

    numtableentries% = numtableentries% + 1

    ret% = ((numtableentries% AND (numtableentries% - 1)) = 0)

    IF numtableentries% = &H1000 THEN tableisfull% = -1: ret% = 0
  END IF

  keytype% = ReadBits&(d$, idx%, keysize%, byte%, shift%, sublen%)

  IF keytype% <> clearkey% THEN
    IF keytype% = stopkey% OR keytype% = &H1000 THEN EXIT FOR

    IF ret% THEN keysize% = keysize% + 1

    entry = table(keytype%)
    strlen% = entry.EntryLength

    FOR i% = 0 TO strlen% - 1
      p% = frameoffset% + entry.EntryLength - 1
      resultbuffer%(p%) = entry.EntrySuffix

      IF entry.EntryPrefix = &HFFF THEN EXIT FOR

      entry = table(entry.EntryPrefix)
    NEXT i%

    frameoffset% = frameoffset% + strlen%

    IF keytype% < numtableentries% - 1 AND NOT tableisfull% THEN
      table(numtableentries% - 1).EntrySuffix = entry.EntrySuffix
    END IF
  END IF
LOOP
END SUB

SUB EnableBrightBackground
' Reset Attribute Controller port mode
a% = INP(&H3DA)

' Point to the Attribute Mode Control Register
OUT &H3C0, 16

' Clear the Blink Enable bit
v% = INP(&H3C0)
v% = v% AND &HF7
OUT &H3C0, v%
END SUB

FUNCTION ReadBits% (a$, idx%, bits%, byte%, shift%, sublen%)
accum% = 0
bitvalue% = 1
shiftbit% = 1

FOR i% = shift% TO 7
  shiftbit% = shiftbit% + shiftbit%
NEXT i%

FOR i% = 1 TO bits%
  IF shift% = 0 THEN
    idx% = idx% + 1
    IF idx% <= LEN(a$) THEN
      IF sublen% = 0 THEN
        sublen% = ASC(MID$(a$, idx%, 1))
        idx% = idx% + 1
      END IF

      byte% = ASC(MID$(a$, idx%, 1))
      sublen% = sublen% - 1
    ELSE
      byte% = 0
    END IF

    shift% = 8
    shiftbit% = 1
  END IF

  IF byte% AND shiftbit% THEN accum% = accum% OR bitvalue%

  bitvalue% = bitvalue% + bitvalue%
  shiftbit% = shiftbit% + shiftbit%
  shift% = shift% - 1
NEXT i%

ReadBits% = accum%
END FUNCTION

SUB SetCharacterWidth8
' Read current mode from Misc Output
mode% = INP(&H3CC)

' Mask off clock bits -- 00 == 25MHz
mode% = mode% AND &HF3

' Write the new mode
OUT &H3C2, mode%

' Read the Sequencer Clocking Mode register
OUT &H3C4, 1
mode% = INP(&H3C5)

' Set the Character Width 8 bit
mode% = mode% OR 1

' Write the new mode
OUT &H3C4, 1
OUT &H3C5, mode%
END SUB

SUB SetGradientPalette
OUT &H3C8, 0

FOR i% = 0 TO 15
  PALETTE i%, i%

  OUT &H3C9, i% * 63 \ 15
  OUT &H3C9, i% * 63 \ 15
  OUT &H3C9, i% * 63 \ 15
NEXT i%
END SUB

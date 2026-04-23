DECLARE SUB CalcTables ()
DECLARE SUB DoTunnel (radoff%, angoff%)
DECLARE SUB FreeMemory ()
DECLARE SUB Initialize ()
DECLARE SUB LoadTga (TgaName$, TgaSeg%)
DECLARE SUB Mode256 ()

TYPE RegTypeX
  AX AS INTEGER
  BX AS INTEGER
  CX AS INTEGER
  DX AS INTEGER
  BP AS INTEGER
  SI AS INTEGER
  DI AS INTEGER
  FLAGS AS INTEGER
  DS AS INTEGER
  ES AS INTEGER
END TYPE

DECLARE SUB InterruptX (n%, rin AS RegTypeX, rout AS RegTypeX)

CONST VidSeg% = &HA000

DIM SHARED RadTableSeg%, AngTableSeg%, TunTexSeg%

Initialize

Frames& = 0
T1! = TIMER

DO
  DoTunnel -T% * 3, T%

  Ft! = Ft! + .9
  T% = FIX(Ft!) AND 255

  Frames& = Frames& + 1&
LOOP WHILE INKEY$ = ""

SCREEN 0

T1! = TIMER - T1!

PRINT "Code by Gedge"
PRINT "QuickBASIC port by logiclrd"
PRINT
PRINT USING "#.## Frames per second"; (Frames& / T1!)

FreeMemory

SUB CalcTables
  DEF SEG = RadTableSeg%

  FOR j% = -128 TO 127
    FOR i% = -128 TO 127
      IF i% <> 0 THEN i2% = i% ELSE i2% = 1
      IF j% <> 0 THEN j2% = j% ELSE j2% = 1

      rad% = SQR(0& + i2% * i2% + j2% * j2%)
      rad% = FIX(8 * 32 * ((256 * 1024& * rad%) ^ .1))
      rad% = rad% + FIX(32 * SIN(rad% * 1.284 / 64))

      POKE (j% + 128) * 256& + i% + 128&, rad% AND 255
    NEXT i%
  NEXT j%

  DEF SEG = AngTableSeg%

  FOR j% = -128 TO 127
    FOR i% = -128 TO 127
      IF i% <> 0 THEN x! = i% ELSE x! = .0001
      IF j% <> 0 THEN y! = j% ELSE y! = .0001

      angle! = ATN(y! / x!) * (256 / 6.284)
      angleI% = FIX(angle!)

      POKE (j% + 128) * 256& + i% + 128&, angleI% AND 255
    NEXT i%
  NEXT j%
END SUB

SUB DoTunnel (radoff%, angoff%)
  o& = 256

  FOR o& = 256 TO 65279
    DEF SEG = RadTableSeg%
    texel& = (PEEK(o&) + radoff%) * 256&
    DEF SEG = AngTableSeg%
    texel& = texel& + angoff% + 2 * PEEK(o&)
    DEF SEG = TunTexSeg%
    pel% = PEEK(texel&)
    DEF SEG = &HA000
    POKE o&, pel%
  NEXT o&
END SUB

SUB FreeMemory
  DIM rin AS RegTypeX, rout AS RegTypeX

  rin.AX = &H4900

  rin.ES = RadTableSeg%
  CALL InterruptX(&H21, rin, rout)

  rin.ES = AngTableSeg%
  CALL InterruptX(&H21, rin, rout)

  rin.ES = TunTexSeg%
  CALL InterruptX(&H21, rin, rout)
END SUB

SUB Initialize
  CLS
  PRINT
  PRINT "This routine runs in a tweaked 256*256*256 mode so if your gfx card can't handle"
  PRINT "it then Tough ;)"
  PRINT
  PRINT "Please Wait - Performing some pre-calcs just now which can take a while :( "

  DIM rin AS RegTypeX, rout AS RegTypeX

  rin.AX = &H4800
  rin.BX = &H1000

  CALL InterruptX(&H21, rin, rout)
  RadTableSeg% = rout.AX

  CALL InterruptX(&H21, rin, rout)
  AngTableSeg% = rout.AX

  CALL InterruptX(&H21, rin, rout)
  TunTexSeg% = rout.AX

  PRINT
  PRINT "Calcing Angle and Radius tables............... "

  CalcTables

  SCREEN 13
  Mode256

  LoadTga "MESS.TGA", TunTexSeg%
END SUB

SUB LoadTga (TgaName$, TgaSeg%)

  F% = FREEFILE

  OPEN TgaName$ FOR BINARY AS #F%

  Dummy$ = INPUT$(18, #F%)
  Pal$ = INPUT$(768, #F%)

  DEF SEG = TgaSeg%

  o& = 0

  FOR i% = 0 TO 255
    Row$ = INPUT$(256, #F%)

    FOR j% = 1 TO 256
      POKE o&, ASC(MID$(Row$, j%, 1))
      o& = o& + 1
    NEXT j%
  NEXT i%

  CLOSE #F%

  OUT &H3C8, 0

  FOR LoopIndex% = 1 TO 768 STEP 3
    OUT &H3C9, ASC(MID$(Pal$, LoopIndex% + 2, 1)) \ 4
    OUT &H3C9, ASC(MID$(Pal$, LoopIndex% + 1, 1)) \ 4
    OUT &H3C9, ASC(MID$(Pal$, LoopIndex%, 1)) \ 4
  NEXT LoopIndex%
END SUB

SUB Mode256
  OUT &H3D4, &H11: OUT &H3D5, INP(&H3D5) AND &H7F
  OUT &H3C2, &HE3
  OUT &H3D4, &H0:  OUT &H3D5, &H5F ' Horizontal Total = 95 => 100 characters
  OUT &H3D4, &H1:  OUT &H3D5, &H1F ' End Horizontal Display = 31 (use 63 when dot doubling is disabled)
  OUT &H3D4, &H2:  OUT &H3D5, &H40 ' Start Horizontal Blanking = 64
  OUT &H3D4, &H3:  OUT &H3D5, &H82 ' End Horizontal Blanking = 130
  OUT &H3D4, &H4:  OUT &H3D5, &H4A ' Start Horizontal Retrace = 74
  OUT &H3D4, &H5:  OUT &H3D5, &H9A ' End Horizontal Retrace = 154
  OUT &H3D4, &H6:  OUT &H3D5, &H6F ' Vertical Total = 0x6F in this register plus overflow bits => 0x26F = 623
  ' Overflow: 1011 0010 => Vertical Retrace Start 10........,
  '                        Vertical Display End 01........,
  '                        Vertical Total 10........,
  '                        Line Compare 1........,
  '                        Start Vertical Blanking 0........
  OUT &H3D4, &H7:  OUT &H3D5, &HB2
  OUT &H3D4, &H8:  OUT &H3D5, &H0 ' Preset Row Scan = 0
  OUT &H3D4, &H9:  OUT &H3D5, &H61 ' Maximum Scan Line = 1 (row doubling), Line Compare 1........., Start Vertical Blanking 1.........
  OUT &H3D4, &H10: OUT &H3D5, &H28 ' Vertical Retrace Start = 0x28 in this register plus overflow bits => 0x228 = 552
  OUT &H3D4, &H11: OUT &H3D5, &H8A ' Vertical Retrace End = 138
  OUT &H3D4, &H12: OUT &H3D5, &HFF ' Vertical Display End = 0xFF in this register plus overflow bits => 0x1FF = 511
  OUT &H3D4, &H13: OUT &H3D5, &H20 ' Offset = 32
  OUT &H3D4, &H14: OUT &H3D5, &H40 ' Underline Location = 0, enable dword addressing
  OUT &H3D4, &H15: OUT &H3D5, &H7 ' Start Vertical Blanking = 0x07 in this register plus overflow bits => 0x007 = 7
  OUT &H3D4, &H16: OUT &H3D5, &H1A ' End Vertical Blanking = 26
  OUT &H3D4, &H17: OUT &H3D5, &HA3 ' Mode Control = 1010 0011 => Sync, Address Wrap, Map14 + Map13
  OUT &H3C4, &H1:  OUT &H3C5, &H9 ' Clocking Mode = 1 => 8 dots per character
  OUT &H3C4, &H3:  OUT &H3C5, &H0 ' Character Map = 0
  OUT &H3C4, &H4:  OUT &H3C5, &HE ' Memory Mode = 1110 => Chain 4, O/E Disabled, Extended Memory
  OUT &H3CE, &H5:  OUT &H3CF, &H40 ' Graphics Mode = 0100 0000 => Shift 256
  OUT &H3CE, &H6:  OUT &H3CF, &H5 ' Misc Graphics = 0101 => Map 64KB at 0xA000, Disable character generator (graphics mode)
END SUB

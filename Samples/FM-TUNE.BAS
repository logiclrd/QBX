' Program FM-TUNE.BAS
' Wouter Bergmann Tiest
' 17th December 1995

' This program plays a simple tune using FM music

DECLARE SUB SetReg (Reg%, Value%)
CONST BaseAddr = &H380 'Change if your sound card uses another base address

CONST RegAddr = BaseAddr + 8, DataAddr = BaseAddr + 9

DEFINT A-Z

FOR i = 0 TO 224
  SetReg i, 0 'Clear all registers
NEXT i
SetReg &H20, &H1 'Plays carrier note at specified octave ch. 1
SetReg &H23, &H1 'Plays modulator note at specified octave ch. 1
SetReg &H40, &H1F 'Set carrier total level to softest ch. 1
SetReg &H43, &H0 'Set modulator level to loudest ch. 1
SetReg &H60, &HE4 'Set carrier attack and decay ch. 1
SetReg &H63, &HE4 'Set modulator attack and decay ch. 1
SetReg &H80, &H9D 'Set carrier sustain and release ch. 1
SetReg &H83, &H9D 'Set modulator sustain and release ch. 1
SetReg &H21, &H1 'Plays carrier note at specified octave ch. 2
SetReg &H24, &H1 'Plays modulator note at specified octave ch. 2
SetReg &H41, &H1F 'Set carrier total level to softest ch. 2
SetReg &H44, &H0 'Set modulator level to loudest ch. 2
SetReg &H61, &HE4 'Set carrier attack and decay ch. 2
SetReg &H64, &HE4 'Set modulator attack and decay ch. 2
SetReg &H81, &H9D 'Set carrier sustain and release ch. 2
SetReg &H84, &H9D 'Set modulator sustain and release ch. 2
SetReg &H22, &H1 'Plays carrier note at specified octave ch. 3
SetReg &H25, &H1 'Plays modulator note at specified octave ch. 3
SetReg &H42, &H1F 'Set carrier total level to softest ch. 3
SetReg &H45, &H0 'Set modulator level to loudest ch. 3
SetReg &H62, &HE4 'Set carrier attack and decay ch. 3
SetReg &H65, &HE4 'Set modulator attack and decay ch. 3
SetReg &H82, &H9D 'Set carrier sustain and release ch. 3
SetReg &H85, &H9D 'Set modulator sustain and release ch. 3

READ NoOfNotes

FOR i = 1 TO NoOfNotes
  time! = TIMER
  FOR j = 0 TO 2 'Voices 0, 1 and 2
    READ octave
    READ note$
    SELECT CASE note$
    CASE "C#"
      SetReg &HA0 + j, &H6B 'Set note number
      SetReg &HB0 + j, &H21 + 4 * octave 'Set octave and switch on channel
    CASE "D"
      SetReg &HA0 + j, &H81
      SetReg &HB0 + j, &H21 + 4 * octave
    CASE "D#"
      SetReg &HA0 + j, &H98
      SetReg &HB0 + j, &H21 + 4 * octave
    CASE "E"
      SetReg &HA0 + j, &HB0
      SetReg &HB0 + j, &H21 + 4 * octave
    CASE "F"
      SetReg &HA0 + j, &HCA
      SetReg &HB0 + j, &H21 + 4 * octave
    CASE "F#"
      SetReg &HA0 + j, &HE5
      SetReg &HB0 + j, &H21 + 4 * octave
    CASE "G"
      SetReg &HA0 + j, &H2
      SetReg &HB0 + j, &H22 + 4 * octave
    CASE "G#"
      SetReg &HA0 + j, &H20
      SetReg &HB0 + j, &H22 + 4 * octave
    CASE "A"
      SetReg &HA0 + j, &H41
      SetReg &HB0 + j, &H22 + 4 * octave
    CASE "A#"
      SetReg &HA0 + j, &H63
      SetReg &HB0 + j, &H22 + 4 * octave
    CASE "B"
      SetReg &HA0 + j, &H87
      SetReg &HB0 + j, &H22 + 4 * octave
    CASE "C"
      SetReg &HA0 + j, &HAE
      SetReg &HB0 + j, &H22 + 4 * octave
    END SELECT
  NEXT j
  READ duration!
  DO
  LOOP WHILE time! + duration! > TIMER 'Wait as long as duration
  FOR j = 0 TO 2
    SetReg &HB0 + j, 0 'Switch channel off
  NEXT j
NEXT i

END

DATA 15: REM Number of notes
'Data below: octave1, note1, octave2, note2, octave3, note3, duration
DATA 4,B,4,G,4,D,.5
DATA 4,B,4,G,4,D,.5
DATA 4,B,4,G,4,D,.5
DATA 4,B,4,G,4,D,.5
DATA 5,D,4,B,4,F#,.25
DATA 4,C,4,A,4,E,.25
DATA 4,C,4,A,4,E,.25
DATA 4,B,4,G,4,D,.25
DATA 4,A,4,E,3,C,1
DATA 4,A,4,F#,4,D,.5
DATA 4,A,4,F#,4,D,.5
DATA 4,B,4,G,4,E,.5
DATA 4,C,4,A,4,F#,.5
DATA 5,D,4,A,4,F#,1
DATA 5,G,5,D,4,B,.5

SUB SetReg (Reg, Value)
OUT RegAddr, Reg
OUT DataAddr, Value
END SUB


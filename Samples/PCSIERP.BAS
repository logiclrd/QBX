10 REM Sierpinski triangle for PC-BASIC
15 REM (source: https://github.com/oonap0oo/PC-BASIC-projects/blob/main/PCSIERP.BAS)
20 RANDOMIZE TIMER
30 XMAX% = 640: YMAX% = 350: BORDER% = 10
40 SCREEN 9: CLS
50 LOCATE 2, 3: PRINT "Sierpinski triangle"
60 DIM CX%(2), CY%(2): REM coord. of triangle vertices
70 CX%(0) = XMAX% / 2: CY%(0) = BORDER%
80 CX%(2) = BORDER%: CY%(2) = YMAX% - BORDER%
90 CX%(1) = XMAX% - BORDER%: CY%(1) = YMAX% - BORDER%
100 PX% = CX%(0): PY% = CY%(0)
110 COL% = 0: COL2% = 0: COL3% = 0
120 FOR K! = 1 TO 60000!
130 INDEX% = INT(RND * 3)
140 PX% = (PX% + CX%(INDEX%)) / 2
150 PY% = (PY% + CY%(INDEX%)) / 2
160 PSET (PX%, PY%), COL3%
170 COL3% = COL2%: COL2% = COL%: COL% = 13 + INDEX%
180 NEXT K!
190 LOCATE 3, 3: PRINT "Drawing completed"
200 END

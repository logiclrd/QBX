'$STATIC
DEFDBL A-Z
DEFINT N, O, X, Y

                      A=0:B=0:DIM z(1919)
                CLS:p$="............,-~:;=!*#$@"
             DO:buf$=SPACE$(1920):ERASE z:cA=COS(A)
           sA=SIN(A):cB=COS(B):FOR j=0TO 6.28STEP .07
         sB=SIN(B):cj=COS(j)          :sj=SIN(j):h=cj+2
        cAsj=cA*sj: FOR i=0            TO 6.28 STEP .04
       ci=COS(i):si=                      SIN(i):sih=si*h
       cih=ci*h:D=1                       /(cAsj+sA*sih+5)
       t=cA*sih-sA*sj                       :C=cB*cih-sB*t
       x=40+30*D                               *C:R=Sb*cih
       y=12+15*D*(R+cB                      *t):o=x+80*y+1
        Q=sB*ci*cj+                          cAsj+sA*si*cj
        N%=8*(cB*(sA*                     sj-cA*si*cj)-Q)
          SELECT CASE D>                   z(o):CASE -1
             z(o)=D: MID$(buf$,o,  1)=MID$(p$,N+12,1)
                END SELECT :NEXT i,j :LOCATE 1,1
                   PRINT buf$:A=A+.04:B=B+.02
                       LOOP WHILE INKEY$=""


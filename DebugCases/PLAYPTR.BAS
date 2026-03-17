a$ = "L16CDE N0"

b$ = "L16EFG N0"

c$ = "X" + VARPTR$(a$) + " X" + VARPTR$(b$)

d$ = "X" + VARPTR$(c$)

d$ = d$ + d$

PLAY d$

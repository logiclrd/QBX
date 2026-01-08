TYPE foo
  a AS INTEGER
  b AS CURRENCY
  c AS STRING
END TYPE

DIM x AS foo

x.a = 2
x.b = x.a + 1
COLOR x.a

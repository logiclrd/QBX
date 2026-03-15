TYPE abcdef
  foo AS INTEGER
  bar AS INTEGER
END TYPE

DIM zeep AS abcdef

zeep.foo = 42
zeep.bar = 69

sploof zeep

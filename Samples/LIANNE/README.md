# Lianne in... The Dark Crown

The classic QBRPG Lianne in... The Dark Crown was written in QBASIC. QBX is an environment emulator of QuickBASIC 7.1. There are some differences between these two environments that prevent DC1.BAS from running in QuickBASIC:

* In QBASIC, parameters to a `SUB` or `FUNCTION` can occlude a `CONST`. DC1.BAS depends on this, defining a `CONST PosX` at the module level and then several `SUB`s which have parameters named `PosX`. In QBASIC, these `SUB`s simply can't see the `PosX` constant. In QuickBASIC, this is a compile error.

* In QBASIC, you can use the word `Update` as identifier. In QuickBASIC 7.1, a keyword `UPDATE` was introduced as part of ISAM database functionality. QBASIC was forked from QuickBASIC 4.5 and does not have this keyword. DC1.BAS has a `SUB` called `Update`, which produces a syntax error in QuickBASIC 7.1.

In addition to these differences, there is a considerable difference in speed between the execution environments. The `SUB Shake` takes a count of iterations and updates a VGA register that many times. In the original operating environment, this produced a brief visible shaking effect when called with a count of 2000. In QBX, all 2000 iterations complete before a single frame is rendered.

In this directory, `DC1X.BAS` is a version of `DC1.BAS` modified to work with QBX.

* The conflicting `CONST` definitions are renamed.

* The `SUB Update` is renamed so that its name is once again an identifier.

* The `SUB Shake` is updated to tie its operating speed to the wall clock. The `Duration` parameter is reinterpreted to mean milliseconds, and the 8253 timer is temporarily reprogrammed to allow synchronization at a higher framerate. In addition, the values written to the CRT Controller register 8 "Preset Row Scan" are constrained to valid values. The original code writes values 0-255 to this register, but bit 7 is not defined in the VGA specification. In DC1X.BAS, bit 7 is masked off. On VGA implementations that ignored bit 7, including the emulation in QBX, there is no visible difference.

This directory must be populated with the contents of the DC1.ZIP release to provide all of the necessary game assets to run Lianne in... The Dark Crown.
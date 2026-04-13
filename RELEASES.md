# QBX Releases

## 1.3.0 - 2026-04-13

This release contains a number of fixes and refinements that resulted from running the example programs supplied on the supplemental disks for "Learn BASIC Now". This Microsoft Press publication can be found on Internet Archive.

### Added

- `WINDOW` statement for setting up graphics windows. Aspect ratio calculation for `CIRCLE` updated to correctly account for the graphics window.
- `OPTION BASE` statement.
- `FILES` statement.

### Fixed

- Missing parentheses added correcting the calculation of byte offsets when drawing text characters in 1bpp graphics modes (e.g. `SCREEN 2`).
- PC Speaker is now correctly tied to the port I/O bus.
- PC Speaker port I/O now interacts with the Speaker device change queue properly.
- `LBOUND` and `UBOUND` were incorrectly treating their optional dimension argument as 0-based.
- Tabs are now expanded to spaces during file load.
- Underlying graphics window function now works with right-to-left / top-to-bottom coordinates.
- `EOF` now takes into account partially-parsed lines that have been buffered.
- `OPEN` now correctly permits `ACCESS READ WRITE` to be used with `FOR APPEND`.
- `OPEN` now correctly handles omitted `ACCESS` clauses.
- `SOUND` now operates with 2.5ms ticks, despite the documentation stating that `duration%` must be an integer count of 55ms ticks.
- `SOUND` with a duration of 0 now properly immediately cuts off playback, including queued `PLAY` commands.
- `PRINT` now handles arguments with no expression, such as `PRINT ,`.
- Arrays of fixed-length strings no longer lose the fixed length aspect of the `STRING` type.
- `INSTR` now correctly handles not finding a match with a non-zero start index.

## 1.2.1 - 2026-04-12

### Fixed

- `PRINT` statement parsing now allows semicolons to be omitted (`? 1 2` parses to `PRINT 1; 2`).

## 1.2.0 - 2026-04-11

### Added

- `CHAIN` statements, including propagating forward `COMMON` data blocks.

### Fixed

- The `SPC` pseudofunction `PRINT` argument type no longer switches to `TAB`.
- The `SPC` pseudofunction `PRINT` argument type now produces the correct output.
- `PRINT` statements now implicitly add implied semicolons after `SPC` and `TAB` arguments.
- Pressing Ctrl+Break during the evaluation of `INPUT$(n)` no longer crashes QBX.
- `FUNCTION`s declared with `STATIC` no longer treat the last parameter as a local variable with static storage.
- `FOR` loop iteration now leaves the iterator variable in the correct state at the completion of the loop.
- `SWAP` now works correctly with UDT values.
- The `VARPTR$` function now works with variables of type other than `STRING`.
- `COMMON` statements no longer generate a "Duplicate definition" error when used with dimensioned arrays.
- Multiple `COMMON` statements for the same common block may now be used.
- Arrays of primitive types now work as parameters to `SUB`s and `FUNCTION`s.
- `DRAW` statements now parse `TA` commands correctly.
- `DRAW` and `PLAY` statements now support the `"=" + VARPTR$(..)` syntax for numeric arguments.

## 1.1.0 - 2026-04-11

### Notable

* The O.G. QBRPG "Lianne in... The Dark Crown" now runs in QBX. The original text has a couple of minor incompatibilities with QuickBASIC 7.1 and thus also with QBX (such as a `SUB Update` -- `UPDATE` is a keyword in QuickBASIC 7.1). QBX now ships with a sample file `DC1X.BAS` which is identical in functionality to the released `DC1.BAS` for "Lianne in... The Dark Crown" but with these incompatibilities fixed.

    * To play the game, download the release of "Lianne in... The Dark Crown" and unzip it into the directory `Samples/LIANNE` containing `DC1X.BAS`, then load and run `DC1X.BAS`.

    * See `Samples/LIANNE/README.md` for more information.

### Added

* When memory address 0040:006C is read, it now reflects a 32-bit timer tick count in line with a real PC bios.

### Fixed

* The `Maximum Scan Line` VGA register is now properly processed. If mode 13h is switched from using `Scan Doubling` to using `Maximum Scan Line` to duplicate each scan, the visual appearance does not change. (LIANNE uses this technique.)

## 1.0.2 - 2026-04-10

### Added

* `FRE` function. It doesn't return anything meaningful, but the values it returns are in line with the documentation.

### Fixed

* Keywords with a `$` suffix are now recognized as identifiers, as long as they don't conflict with other keywords (e.g. `integer$` can be used as a variable or function name, `left$` cannot).
* `INPUT #` statements can now straddle lines. Unused tokens after the last item read are not lost, and line boundaries may occur in the middle of the list of targets.
* `BLOAD` now uses the offset from the file header if the offset expression is omitted.

## 1.0.1 - 2026-04-09

### Added

* `WRITE` statements.

### Fixed

* Keywords followed by a numeric type character are no longer tokenized as identifiers (e.g. `integer%`).
* Hexadecimal and octal literals now correctly parse negative values (e.g. `&HC000` is -16384 and `&HFFFFC000` is -16384&).
* Errors resulting from parsing expressions (that thus precede the creation of the statement object) now highlight in the correct viewport.
* Octal numbers with invalid digits `8` and `9` now raise the correct error "Overflow" during the parsing phase rather than being rejected during lexical analysis.

## 1.0.0 - 2026-04-07

First build published by the Release pipeline.

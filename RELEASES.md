# QBX Releases

## 1.7.0 - 2026-04-26

### Fixed

- VGA registers associated with horizontal and vertical limits are now processed better, permitting nonstandard modes like 256x256x256.
- CGA shift interleave modes no display the full frame again.
- `GET` (graphics) now raises a proper `ERROR 5` (Illegal Function Call) when the coordinates are out-of-range, like `PUT` (graphics).
- `GET` and `PUT` (graphics) now factor in array element size when supplied with an array index to start at.fs
- Arrays with `$STATIC` allocation are no longer deallocated by `ERASE`.
- String values in `SUB` and `FUNCTION` parameters are now dealiased.

### Added

- `PALETTE` with no arguments is now supported.

## 1.6.2 - 2026-04-21

### Fixed

- `DRAW` handling of `M` commands now correctly parses the arguments.
- `DRAW` handling of `B` and `N` modifiers now correctly only applies them to the draw command that immediately follows.

## 1.6.1 - 2026-04-21

### Fixed

- `DRAW` now applies rotation correctly. The previous calculation had signs in the wrong order, causing the angle to be effectively 180 degrees off.

## 1.6.0 - 2026-04-21

### Changed

- The distribution layout is now closer to a QB71 installation, with `BIN`, `SRC` and `HELP` directories. The `HELP` directory contains a hint about how to enable help functionality.

### Added

- `STOP` statement.
- `PMAP` function.

### Fixed

- `DIM` statements now preserve extra whitespace before the `AS` keyword if present.
- The visible display page field within the video firmware is now included in the save state blob. This fixes `Press any key to continue` sometimes not appearing on program termination.
- Help files are now located based on the location of QBX.exe, regardless of what the current directory is.
- When switching between `SUB`s/`FUNCTION`s/modules, the selection state is reset so that pressing Shift right after no longer selects a chunk of text back to the previous view's cursor position.
- Text inputs in dialogs now handle backspace more consistently with QuickBASIC: Modifier keypresses no longer cancel selection, Ctrl-Backspace is now equivalent to Delete, and Backspace now clears any selection.

## 1.5.0 - 2026-04-18

### Added

- The Start, Restart, Continue and Modify COMMAND$ menu items are now implemented.
- `COMMAND$`

### Fixed

- The video mode is only reset to 80x25 text on program start if it isn't already in that mode.
- Cursor addresses for pages other than the visible page are now included in saved video state information.

## 1.4.1 - 2026-04-16

### Fixed

- An edge case in `PAINT` that caused an artefact when the border was encountered at X = 0 has been fixed.
- `INKEY$` now skips ephemeral key events that caused it to spuriously return `""`.

## 1.4.0 - 2026-04-16

### Added

- `VIEW` (graphics) statements, and the associated interaction with `WINDOW`.
- `PCOPY` statement.

### Fixed

- Fixed a crash in the TextInput widget if the selection range becomes invalid.
- Fixed a crash in the Menu Bar if Up or Down is pressed with no menu selected.
- `PUT` (graphics) now raises a proper Illegal Function Call error when coordinates are not in-range.
- Fixed calling a DEF FN function multiple times in the same statement.
- When an action causes the running program to be terminated, resuming execution no longer tries to resume the terminated program.
- When execution breaks, queued keyboard input events are no longer received by the IDE instead.
- When starting a new program or loading a new file, if there is a currently executing program, it is terminated.
- `SCREEN` no longer clears the screen if the mode number hasn't changed (e.g. using `SCREEN` for page flipping).
- `WINDOW` now raises Illegal Function Call if X1 = X2 or Y1 = Y2.
- The VGA Start Address is now calculated properly, as a starting plane offset rather than a starting linear address.
- When writing to VGA memory with Chain Odd/Even enabled, the offset's parity is now checked before the offset is forced to an even value.
- When starting a program, the video mode is once again reset to SCREEN 0.
- The execution epilogue now ensures it is writing "Press any key to continue" to the visible page, in case the program left the visual library configured to an offscreen page.
- The FILES.BAS sample now assumes it will be running in the `Samples` directory.
- The MAZERUN.BAS sample has been updated in the same way that PIPELINE.BAS was previously to account for corrections in the VGA Odd/Even handling.
- The STARS.BAS sample now exits on a keypress.

### Changed

- Video page sizes are now rounded up to a multiple of 256 bytes.

## 1.3.1 - 2026-04-13

This release contains a few more fixes based on the "Learn BASIC Now" example programs.

### Fixed

- `INPUT` no longer interferes with the graphics mode last point, for the purpose of `STEP` in drawing statements.
- Some situations where line numbers and labels would be missed because the compiler thought the line could be skipped have been fixed.
- The range check on the `mode` parameter to `SetMode` in the `Video` firmware has been corrected.

### Improvements

- When `ResolveJumpStatements` is running on the main routine, a redundant second call to `CollectLabels` is avoided.
- Execution engine classes now use specific types for the reference back to their corresponding code model objects.

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

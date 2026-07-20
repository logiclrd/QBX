# QBX Releases

## 1.16.2 - 2026-07-20

### Fixed

- A corner case with operator precedence where a `-` operator next to a number was initially misinterpreted as being part of a negative number is fixed.
- Hexadecimal and octal literals that resolve to different values as `INTEGER` and as `LONG` now preserve the `&` type declaration character if it is supplied.
- Hexadecimal and octal literal values that are valid `INTEGER` values that are intrinsically negative now evaluate to these `INTEGER` values, unless they are explicitly qualified with `&`.
- When using Shift-F5 to restart the execution of a program, if a compile error occurs, it is no longer thrown & presented twice (resulting in stacked error dialogs).
- Some paths where context was lost, preventing compile-time errors from jumping to the error location in the IDE, have been fixed.

## 1.16.1 - 2026-07-18

### Fixed

- The `DRAW` processor now limits colours by clamping to the maximum rather than ignoring higher bits.
- When the Set Mouse Cursor Position function of INT 33h is activated (and when the cursor is reset to the centre of the screen during a reset), the physical cursor position set on the host system is now correctly translated to the window client area.

### Added

- Frame and song row joint timing in `MINS3M.BAS`.
- Support for multiple songs in an S3M file in `MINS3M.BAS`, treating empty order list entries as dividers.
- Improved user interface for `PLAYSONG.BAS`, allowing interactive selection of S3M file and song.
- SHRIKE now fully integrates music and sound effects.

## 1.16.0 - 2026-07-12

### Fixed

- The conversion of unsigned 8-bit sample data to signed 16-bit in the Gravis UltraSound mix loop is now correct.
- The `LOC` function now returns 0-based record numbers for files in `RANDOM` mode.
- The `GET #` and `PUT #` functions now treat the record number provided, if any, as 1-based.
- The `GET #` and `PUT #` functions now read/write string length prefixes when operating on a variable-length string in `RANDOM` mode.

### Added

- In the SHRIKE `Samples` folder, there is now a minimal S3M audio file player using the Gravis UltraSound for wavetable synthesis. Check out `PLAYSONG.BAS`.

## 1.15.4 - 2026-07-11

### Fixed

- Identifiers whose names start with "data" are no longer incorrectly interpreted as the `DATA` keyword.
- The "Press any key to continue" prompt at program termination now clears the entire text row.
- The default character line window now correctly excludes the last physical row of text.
- The `LOCATE` statement can now place the cursor on rows outside of the current character line window. (Newlines still move the cursor back into the character line window.)
- `PRINT` at the very end of a line of text no longer emits a newline before emitting the characters.
- The `CSRLIN` and `POS(0)` functions now return the _effective_ cursor position, even if the physical cursor is still at the end of the line to delay newline and possible scrolling.
- In the IDE, the viewport can no longer be scrolled to the null space to the left of the document with Ctrl-PageUp.
- In the IDE, when viewports are bumped around (e.g. by adding a Watch), if the cursor is bumped out of the viewport's content area, it is scrolled back into view.

## 1.15.3 - 2026-07-06

### Fixed

- Soft transitions between notes now function correctly when the voice is being stopped.
- When soft transitions are started, the previous sample values are now always correctly latched.
- Gravis UltraSound emulation now implements the reset function.
- The `EnableDAC` and `EnableIRQ` flags are now cleared when transitioning from the reset state rather that to it.
- Gravis UltraSound emulation now stops updating voices when the top two bits of the page select register aren't both set.
- The last sample emitted counter is now updated at the correct rate.

## 1.15.2 - 2026-07-05

### Fixed

- Gravis UltraSound audio quality has been improved by smoothing abrupt changes of voice playhead address.
- Volume ramp looping now updates the correct variable.

## 1.15.1 - 2026-07-05

### Fixed

- Gravis UltraSound voice selection ("Page Register" port output) now works correctly.
- `PALETTE` now updates DAC entries in `SCREEN 12`.

## 1.15.0 - 2026-07-05

### Fixed

- VGA port &H3DA now correctly reports the Vertical Retrace bit (8), in addition to Display Disabled, in between frames.
- The `INP` function now correctly returns value &HFF when used on an unmapped port number.
- In the `TREX.BAS` sample, the high score is now latched from the correct variable.

### Added

- Gravis UltraSound emulation! Enable it with the `/GUS` command-line option. Currently hardwired to base port &H240.

## 1.14.0 - 2026-06-26

### Fixed

- The `DRAW` statement's colour is now correctly updated by other graphics commands (`CIRCLE`, `LINE`, `PAINT`, `PSET`).
- A crash that could occur when starting a program file whose only context is static array initialization has been fixed.
- Errors that occur during static array initialization are now properly presented.
- 8042 keyboard controller I/O ports are now emulated. An error in internal mapping of keys to scancodes that is visible on port &H60 as a result of this change has been fixed.
- Unterminated empty string literals no longer cause QBX to crash.
- Array elements in `TYPE` definitions, broken by a previous change, now work again.
- `UBOUND` and `LBOUND` now work on array elements in `TYPE`s.
- Statements inside `TYPE` definitions now produce the appropriate compile-time error.
- `CONST`s can now be used in the bounds of array elements in `TYPE`s.
- `DECLARE` statements that have been pasted are no longer ignored by the `DECLARE` statement generator when saving files.
- The `LOC` function is now properly 1-based instead of 0-based.
- The `LOC` function now returns the correct values for files opened `FOR INPUT`.

### Added

- QBX now supports reading `.BAS` files with line continuations (`_` at the end of lines, usually used when they exceed 250 characters in length).
- QBX now emits line continuation characters when writing lines that exceed 250 characters (excluding comments and `DATA` statements).
- Complete translation of the Chromium T-Rex game (chrome://dino) in `TREX.BAS` under `Samples`. :-)

## 1.13.0 - 2026-06-20

### Fixed

- String substrings passed into functions using `LEFT$()`, `MID$()` and `RIGHT$()` no longer alias the underlying storage.
- Lines with end-of-line comments and `IF` statements with multiple statements in the `THEN` or `ELSE` clause no longer generate extraneous empty statements (shows up as extra `:` characters).
- Expressions using the unary operator `NOT` now parse correctly.
- Pasting a buffer in the virtual space after the end of the current line no longer crashes QBX.
- `PAINT` starting on a border pixel no longer crashes.
- `DIM` now correctly throws `Subscript out of range` if the range for a subscript is in the wrong order.
- Parsing now only tries to interpret a line as a `TYPE` element if the `AS` keyword is either specifically the second token, or the second token is an open parenthesis and the `AS` keyword comes directly after the corresponding close parenthesis.
- `FOR` statements now assign the initial value to the iterator even if the end value is _before_ the initial value, causing the loop to be skipped entirely.

### Added

- Top scroller game `SHRIKE` under `Samples`. Worth checking out!

## 1.12.0 - 2026-06-05

### Fixed

- Corrected `ELSE` clause matching in `IF` statement parsing in complex nesting scenarios.
- `IF` statements parsed with no statements in the `THEN` or `ELSE` clauses no longer crash when rendering.
- Lines with a jump target (label or line number) and a trailing comment but no statements no longer break block statement processing.

## 1.11.4 - 2026-06-05

### Fixed

- `INSTR` no longer crashes if the `start` offset is specified more than one byte past the end of the string.
- Trailing `:` characters no longer disappear (as employed commonly with e.g. `CASE 3:`).

## 1.11.3 - 2026-06-02

### Fixed

- `DATA` statements can now be terminated by `:`, allowing statements and comments after them on the same line.
- Parsing of data items (`READ` from `DATA` and `INPUT`) now permits non-quoted items (that is, items surrounded at the ends by '"') to contain quoted substrings that can contain commas (data item parsing) and colons (lexical analysis).
- `WIDTH` now correctly avoids readjusting the height when the same value as the last mode change is supplied.
- Text-mode `CLS` once again correctly clears the screen. A regression test would have caught this bug. :-)

## 1.11.2 - 2026-06-01

### Fixed

- `VAL` now correctly handles hexadecimal and octal representations, including replicating a QuickBASIC bug that occurs when there are more octal digits in the string that fit into the bits of a `LONG`.
- `INPUT` parsing of data items and `INPUT$()` reading bytes can now be used within the same line.
- Mouse events no longer trigger processing on visual library objects when they aren't active.
- Function 0h of INT 33h "Reset" now actually resets the mouse driver.

## 1.11.1 - 2026-05-31

### Fixed

- Corrected the encoding of `SKOLADV.BAS` to Code Page 437.

## 1.11.0 - 2026-05-31

### Fixed

- The initial plane mask is no longer 0b0000 for `SCREEN 12` and `SCREEN 13`.
- `PRINT` now starts a new line if it is outputting a `STRING` value and it doesn't fit into the remaining characters on the line (and the cursor isn't already in the first column).
- `CHR$(8)` (backspace) handling is now correct. A string emitted via `PRINT` will display the character 8 glyph, while a string emitted via INT 21h function 9h will process the character 8 as a control character (backspace).
- Math functions `COS`, `SIN`, `TAN`, `LOG`, `EXP` and the exponentiation operator `^` now raise appropriate errors when used with edge case/not-a-number values.
- Math functions `FIX` and `INT` now return the expected values when used on non-finite values.
- Math functions `SQR` and `ATN` now return the correct non-finite values for error cases.
- Syntax errors that failed to highlight because the tokens didn't have code line objects to link back to are now linked to the code element explicitly and highlight properly.
- Statements that fail to parse inside nested clauses (`THEN` and `ELSE`) are now properly retried and required to parse in order to start execution of the program.

### Added

- Floating point handling is now considerably closer to QuickBASIC, with explicit handling of Indeterminate (-1.#IND) vs. Quiet NaN (1.#QNAN) values in all math operations.
- Math functions `SQR`, `EXP`, `LOG`, `COS`, `SIN`, `TAN` and `ATN` now have separate `SINGLE` and `DOUBLE` implementations selected contextually based on the type of the input expression.

## 1.10.0 - 2026-05-30

### Added

- The `POINT` function now supports the single-argument mode, which returns information about the graphics library's last point.

### Fixed

- `CIRCLE` now correctly handles negative aspect values.
- `CIRCLE`'s aspect-corrected height is slightly more correct in 350-scan video modes.
- An edge case in the `PAINT` border fill algorithm where a scanning primitive couldn't infer the scan direction if the span was only 1 pixel wide has been corrected. This bug would leave individual pixels at the left edge of the region unpainted in some circumstances.
- A small incorrect rounding bias in the translation of coordinates from viewport space to window space has been removed.
- The graphics library's last point is now correctly preserved across `WINDOW` changes.
- By reference arguments are now consistently handled while still correctly avoiding passing the assignable result of MID$() by reference.
- Numeric literals specifying the `!` type character but with more than 7 digits of precision no longer disappear.
- A parsing bug that prevented unary negation of field accesses (expressions like `-a.x`) has been corrected.
- Errors that result from parameter list mismatches in DECLARE statements now correctly indicate their context, allowing the IDE to jump to the location of the error.
- `SYSTEM` now terminates the entire environment, instead of being a literal synonym for `END`. This only happens during autorun (QBX.EXE /RUN FILE.BAS), and is cancelled if the QBX instance enters break mode at any point.
- A crash that occurred during the parsing of some command-lines has been fixed.
- `GORILLAX.BAS` now paces the explosion of a hit gorilla according to wall clock time. Unpaced, the entire animation completes in 1 or 2 frames, and with a full tick delay between each frame, the animation progresses far too slowly.
- `DebugCases` files `SSEGADD2.BAS` and `VARPTR2.BAS` now use the correct register to pass in the buffer memory offset.

### Infrastructure

- QBX now has an integration test mechanism with various tests written in QuickBASIC code to verify behaviour in ways that are difficult to isolate to a unit test.
- QLB management no longer reuses instances attached to other `Machine`s.

## 1.9.0 - 2026-05-23

### Fixed

- `SCREEN 2` now uses the correct memory layout. The VGA adapter now processes the Host Odd/Even Read register value. A new `GraphicsLibrary` implementation has been added to work with 1bpp interleaved data.
- The palette used to emulate CGA modes has been updated using DOSBox-X instead DOSBx.
- `SCREEN 2` now configures attribute palette mappings appropriately for a 1bpp mode, using palette entry 63 for set pixels.

## 1.8.1 - 2026-05-18

### Fixed

- Ctrl-Return now correctly updates the cursor column to match the indentation of the line being entered.
- Ctrl-Return now correctly appends blank lines to the end of the text as needed.
- When joining lines, Delete now correctly reindents the line being joined to the cursor column, collapsing any existing indentation it has.

## 1.8.0 - 2026-05-16

### Fixed

- Errors in the way `PRINT USING` handled negative values between -1 and 0 have been fixed.
- Passing L-values of the incorrect type into `SUB` and `FUNCTION`s now correctly generates a Type Mismatch error.
- `GET` (graphics) now correctly marks the array content (in "packed" form) as having been updated, so future accesses use the packed data.
- When arrays are "pinned" (by using `VARSEG` / `VARPTR`), if they are already "packed" then the packed data is copied into the pinned memory area.
- In the IDE, selecting a block of lines and deleting it no longer clears existing clipboard content.
- In the IDE, Ctrl-Backspace no longer attempts to insert character 127.
- In the IDE, pressing Enter in the middle of a line to split it now correctly indents the new line.

## 1.7.9 - 2026-05-13

### Fixed

- Corrected cursor movement with respect to indentation in the IDE editor in response to Enter, Ctrl-Enter and Backspace.

## 1.7.8 - 2026-05-09

### Fixed

- VGA emulation now pays attention to the "Start Horizontal Retrace" register value.

## 1.7.7 - 2026-05-08

### Fixed

- Blinking (instead of high-intensity background colours) is now correctly enabled in text modes.

## 1.7.6 - 2026-05-07

### Fixed

- `SLITHER.BAS` now limits the maximum snake length to avoid crashing if the player or a bot manages to reach & exceed the predefined maximum length.

## 1.7.5 - 2026-05-07

### Fixed

- Loop index variable reuse corrected in `SLITHER.BAS`.

## 1.7.4 - 2026-05-07

### Fixed

- `SLITHER.BAS` now renders snakes so that their heads are always visible.
- `SLITHER.BAS` now initializes coordinate data immediately, so that parts of snakes do not get processed and rendered at stale coordinates.

## 1.7.3 - 2026-05-06

### Fixed

- Fixed the Game Over screen display in the `SLITHER.BAS` sample.

## 1.7.2 - 2026-05-06

### Fixed

- `SCREEN` no longer resets the video mode if the mode number is the same as the last call to `SCREEN`.

## 1.7.1 - 2026-05-02

### Fixed

- `SPACE$` treats its argument as a 16-bit integer instead of an 8-bit byte, and performs range checking.
- `LOCATE` raises an error for out-of-range coordinates.
- Assigning to `MID$(..)` targets no longer erroneously applies the offset twice.

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

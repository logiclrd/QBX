<!-- Compiled from: http://www.osdever.net/FreeVGA/vga/vga.htm -->
# Hardware Level VGA and SVGA Video Programming Information Page
## VGA Chipset Reference 

* [Introduction](#introduction) -- introduction to the VGA reference
* [General Programming Information](#general-programming-information) -- details of the functional operation of the VGA hardware.
* [Input/Output Register Information](#inputoutput-register-information) -- details on the VGA registers themselves
* [Indices](#indices) -- convenient listings of fields and their locations alphabetically and by function

### Introduction

This section is intended to be a reference to the common functionality of the original IBM VGA and compatible adapters. If you are writing directly to hardware then this is the lowest common denominator of nearly all video cards in use today. Nearly all programs requiring the performance of low-level hardware access resort to this baseline capacity, so this information is still valuable to programmers. In addition most of the VGA functions apply to SVGA cards when operating in SVGA modes, so it is best to know how to use them even when programming more advanced hardware.  

Most VGA references I have seen document the VGA by describing its operation in the various BIOS modes. However, because BIOS was designed for use in MS-DOS real mode applications, its functionality is limited in other environments. This document is structured in a way that explains the VGA hardware and its operation independent of the VGA BIOS modes, which will allow for better understanding of the capabilities of the VGA hardware.  

This reference has grown out of my own notes and experimentation while learning to program the VGA hardware. During this process I have identified errors in various references that I have used and have attempted to document the VGA hardware's actual behavior as best as possible. If in your experience you find any of this information to be inaccurate, or even if you find this information to be misleading or inaccurate, please let me know!  

One of the reasons I started this reference was that I was using existing references and found myself wishing for a hypertext reference as almost every register is affected by the operation of another, and was constantly flipping pages. Here I simply use links for the register references, such as [Offset Register](#crtcreg-13), rather than stating something like: Offset Register (CRTC: Offset = 13h, bits 7-0). While the second method is more informative, using them for every reference to the register makes the text somewhat bogged down. HTML allows simply clicking on the register name and all of the details are provided. Another is that no single reference had all of the information I was looking for, and that I had penciled many corrections and clarifications into the references themselves. This makes it difficult to switch to a newer version of a book when another edition comes out -- I still use my heavily annotated second edition of Ferarro's book, rather than the more up-to-date third edition.

### General Programming Information

This section is intended to provide functional information on various aspects of the VGA. If you are looking simply for VGA register descriptions look in the next section. The VGA hardware is complex and can be confusing to program. Rather than attempt to document the VGA better than existing references by using more words to describe the registers, this section breaks down the functionality of the VGA into specific categories of similar functions or by detailing procedures for performing certain operations.

* [Accessing the VGA Display Memory](#accessing-the-vga-display-memory) -- details on the memory interface between the CPU and VGA frame buffer.
* [Sequencer Operation](#vga-sequencer-operation) -- details on how the VGA hardware rasterizes the display buffer
    * Text-mode
        * [VGA Text Mode Operation](#vga-text-mode-operation) -- details concerning text mode operation, including attributes and fonts.
        * [Manipulating the Text-mode Cursor](#manipulating-the-text-mode-cursor) -- details controlling the appearance and location of the cursor.
    * [Special Effects Hardware](#special-effects-hardware) -- details on hardware support for windowing, paging, smooth scrolling and panning, and split-screen operation.
* [Attribute Controller Operation](#attribute-controller-operation) -- details on the conversion of sequenced display data into DAC input. **(WIP)**
* [DAC Operation](#dac-operation) -- details controlling the conversion of palette data into analog signals.
* [Display Generation](#display-generation) -- details on formatting of the produced video signal for output to the display.

### Input/Output Register Information

This section is intended to provide a detailed reference of the VGA's internal registers. It attempts to combine information from a variety of sources, including the references listed in the reference section of the home page; however, rather than attempting to condense this information into one reference, leaving out significant detail, I have attempted to expand upon the information available and provide an accurate, detailed reference that should be useful to any programmer of the VGA and SVGA. Only those registers that are present and functional on the VGA are given, so if you are seeking information specific to the CGA, EGA, MCGA, or MGA adapters try the Other References section on the home page.  

In some cases I have changed the name of the register, not to protect the innocent but simply to make it clearer to understand. One clarification is the use of "Enable" and "Disable". A the function of a field with the name ending with "Enable" is enabled when it is 1, and likewise a field with a name ending in Disable is disabled when it is 1. Another case is when two fields have similar or identical names, I have added more description to the name to differentiate them.  

It can be difficult to understand how to manipulate the VGA registers as many registers have been packed into a small number of I/O ports and accessing them can be non-intuituve, especially the Attribute Controller Registers, so I have provided a tutorial for doing this.

*  [Accessing the VGA Registers](#accessing-the-vga-registers) -- methods of manipulating the VGA registers

In order to facilitate understanding of the registers, one should view them as groups of similar registers, based upon how they are accessed, as the VGA uses indexed registers to access most parameters. This also roughly places them in groups of similar functionality; however, in many cases the fields do not fit neatly into their category. In certain cases I have utilized quotes from the IBM VGA Programmer's Reference, this information is given in "_italic._"  This is meant to be a temporary placeholder until a better description can be written, it may not be applicable to a standard VGA implementation.  Presented to roughly based upon their place in the graphics pipeline between the CPU and the video outputs are the:

* [Graphics Registers](#graphics-registers) -- control the way the CPU accesses video RAM.
* [Sequencer Registers](#sequencer-registers) -- control how video data is sent to the DAC.
* [Attribute Controller Registers](#attribute-controller-registers) -- selects the 16 color and 64 color palettes used for EGA/CGA compatibility.
* [CRT Controller Registers](#crt-controller-registers) -- control how the video is output to the display.
* [Color Registers](#color-registers) -- selects the 256 color palette from the maximum possible colors.
* [External Registers](#external-registers) -- miscellaneous registers used to control video operation.

### Indices

In order to locate a particular register quickly, the following indexes are provided. The first is a listing of all of the register fields of the VGA hardware. This is especially useful for fields that are split among multiple registers, or for finding the location of a field that are packed in with other fields in one register. The second is indexed by function groups each pertaining to a particular part of the VGA hardware. This makes understanding and programming the VGA hardware easier by listing the fields by subsystem, as the VGA's fields are grouped in a somewhat haphazard fashion. The third is intended for matching a read or write to a particular I/O port address to the section where it is described.

* [VGA Field Index](#vga-field-index) -- An alphabetical listing of all fields and links to their location.
* [VGA Functional Index](#vga-functional-index) \-\- A listing of all fields and links to their location grouped by function.
* [VGA I/O Port Index](portidx.htm) \-\- A listing of VGA I/O ports in numerical order.

## Accessing the VGA Display Memory 

* [Introduction](#vgamem-introduction) -- gives an overview of the VGA display memory.
* [Detecting the Amount of Display Memory on the Adapter](#detecting-the-amount-of-display-memory-on-the-adapter) -- details how to determine the amount of memory present on the VGA.
* [Mapping of Display Memory into CPU Address Space](#mapping-of-display-memory-into-cpu-address-space) -- details how to control the location and size of the memory aperture.
* [Host Address to Display Address Translation](#host-address-to-display-address-translation) -- detail how the VGA hardware maps a host access to a display memory access
* [Manipulating Display Memory](#manipulating-display-memory) -- Details on reading and writing to VGA memory
* [Reading from Display Memory](#reading-from-display-memory) -- Details the hardware mechanisms used when reading display memory.
* [Writing to Display Memory](#writing-to-display-memory) -- Details the hardware mechanisms used when writing display memory.

### <span id="vgamem-introduction">Introduction</span>

The standard VGA hardware contains up to 256K of onboard display memory. While it would seem logical that this memory would be directly available to the processor, this is not the case. The host CPU accesses the display memory through a window of up to 128K located in the high memory area. (Note that many SVGA chipsets provide an alternate method of accessing video memory directly, called a Linear Frame Buffer.) Thus in order to be able to access display memory you must deal with registers that control the mapping into host address space. To further complicate things, the VGA hardware provides support for memory models similar to that used by the monochrome, CGA, EGA, and MCGA adapters. In addition, due to the way the VGA handles 16 color modes, additional hardware is included that can speed access immensely. Also, hardware is present that allows the programer to rapidly copy data from one area of display memory to another. While it is quite complicated to understand, learning to utilize the VGA's hardware at a low level can vastly improve performance. Many game programmers utilize the BIOS mode 13h, simply because it offers the simplest memory model and doesn't require having to deal with the VGA's registers to draw pixels. However, this same decision limits them from being able to use the infamous X modes, or higher resolution modes.

### Detecting the Amount of Display Memory on the Adapter

Most VGA cards in existence have 256K on board; however there is the possibility that some VGA boards have less. To actually determine further if the card has 256K one must actually write to display memory and read back values. If RAM is not present in a location, then the value read back will not equal the value written. It is wise to utilize multiple values when doing this, as the undefined result may equal the value written. Also, the card may alias addresses, causing say the same 64K of RAM to appear 4 times in the 256K address space, thus it is wise to change an address and see if the change is reflected anywhere else in display memory. In addition, the card may buffer one location of video memory in the chipset, making it appear that there is RAM at an address where there is none present, so you may have to read or write to a second location to clear the buffer. Not that if the [Extended Memory](#seqreg-04) field is not set to 1, the adapter appears to only have 64K onboard, thus this bit should be set to 1 before attempting to determine the memory size.

### Mapping of Display Memory into CPU Address Space

The first element that defines this mapping is whether or not the VGA decodes accesses from the CPU. This is controlled by the [RAM Enable](#3CCR3C2W) field. If display memory decoding is disabled, then the VGA hardware ignores writes to its address space. The address range that the VGA hardware decodes is based upon the [Memory Map Select](#graphreg-06) field. The following table shows the address ranges in absolute 32-bit form decoded for each value of this field:

* 00 -- A0000h-BFFFFh -- 128K
* 01 -- A0000h-AFFFFh -- 64K
* 10 -- B0000h-B7FFFh -- 32K
* 11 -- B8000h-BFFFFh -- 32K

Note -- It would seem that by setting the [Memory Map Select](#graphreg-06) field to 00 and then using planar memory access that you could gain access to more than 256K of memory on an SVGA card. However, I have found that some cards simply mirror the first 64K twice within the 128K address space. This memory map is intended for use in the Chain Odd/Even modes, eliminating the need to use the Odd/Even Page Select field. Also I have found that MS-DOS memory managers don't like this very much and are likely to lock up the system if configured to use the area from B0000h-B7FFFh for loading device drivers high.

### Host Address to Display Address Translation

The most complicated part of accessing display memory involves the translation between a host address and a display memory address. Internally, the VGA has a 64K 32-bit memory locations. These are divided into four 64K bit planes. Because the VGA was designed for 8 and 16 bit bus systems, and due to the way the Intel chips handle memory accesses, it is impossible for the host CPU to access the bit planes directly, instead relying on I/O registers to make part of the memory accessible. The most straightforward display translation is where a host access translates directly to a display memory address. What part of the particular 32-bit memory location is dependent on certain registers and is discussed in more detail in Manipulating Display Memory below. The VGA has three modes for addressing, Chain 4, Odd/Even mode, and normal mode:

* Chain 4: This mode is used for MCGA emulation in the 320x200 256-color mode. The address is mapped to memory MOD 4 (shifted right 2 places.)

*&lt; More to be added here. &gt;*

### Manipulating Display Memory

The VGA hardware contains hardware that can perform bit manipulation on data and allow the host to operate on all four display planes in a single operation. These features are fairly straightforward, yet complicated enough that most VGA programmers choose to ignore them. This is unfortunate, as properly utilization of these registers is crucial to programming the VGA's 16 color modes. Also, knowledge of this functionality can in many cases enhance performance in other modes including text and 256 color modes. In addition to normal read and write operations the VGA hardware provides enhanced operations such as the ability to perform rapid comparisons, to write to multiple planes simultaneously, and to rapidly move data from one area of display memory to another, faster logical operations (AND/OR/XOR) as well as bit rotation and masking.

### Reading from Display Memory

The VGA hardware has two read modes, selected by the [Read Mode](#graphreg-05) field. The first is a straightforward read of one or more consecutive bytes (depending on whether a byte, word or dword operation is used) from one bit plane. The value of the [Read Map Select](#graphreg-04) field is the page that will be read from. The second read mode returns the result of a comparison of the display memory and the [Color Compare](#graphreg-02) field and masked by the [Color Don't Care](#graphreg-07) field. This mode which can be used to rapidly perform up to 32 pixel comparisons in one operation in the planar video modes, helpful for the implementation of fast flood-fill routines. A read from display memory also loads a 32 bit latch register, one byte from each plane. This latch register, is not directly accessible from the host CPU; rather it can be used as data for the various write operations. The latch register retains its value until the next read and thus may be used with more than one write operation.  

The two read modes, simply called Read Mode 0-1 based on the value of the [Read Mode](#graphreg-05) field are:

* **Read Mode 0:**
  
    Read Mode 0 is used to read one byte from a single plane of display memory. The plane read is the value of the [Read Map Select](#graphreg-04) field. In order to read a single pixel's value in planar modes, four read operations must be performed, one for each plane. If more than one bytes worth of data is being read from the screen it is recommended that you read it a plane at a time instead of having to perform four I/O operations to the [Read Map Select](#graphreg-04) field for each byte, as this will allow the use of faster string copy instructions and reduce the number I/O operations performed.

* **Read Mode 1:**
  
    Read Mode 1 is used to perform comparisons against a reference color, specified by the [Color Compare](#graphreg-02) field. If a bit is set in the [Color Don't Care](#graphreg-07) field then the corresponding color plane is considered for by the comparison, otherwise it is ignored. Each bit in the returned result represents one comparison between the reference color from the [Color Compare](#graphreg-02) field, with the bit being set if the comparison is true. This mode is mainly used by flood fill algorithms that fill an area of a specific color, as it requires 1/4 the number of reads to determine the area that needs to be filled in addition to the additional work done by the comparison. Also an efficient "search and replace" operation that replaces one color with another can be performed when this mode is combined with Write Mode 3.

### Writing to Display Memory

The VGA has four write modes, selected by the [Write Mode](#graphreg-05) field. This controls how the write operation and host data affect the display memory. The VGA, depending on the [Write Mode](#graphreg-05) field performs up to five distinct operations before the write affects display memory. Note that not all write modes use all of pipelined stages in the write hardware, and others use some of the pipelined stages in different ways.  

The first of these allows the VGA hardware to perform a bitwise rotation on the data written from the host. This is accomplished via a barrel rotator that rotates the bits to the right by the number of positions specified by the [Rotate Count](#graphreg-03) field. This performs the same operation as the 8086 ROR instruction, shifting bits to the right (from bit 7 towards bit 0.) with the bit shifted out of position 0 being "rolled" into position 7. Note that if the rotate count field is zero then no rotation is performed.  

The second uses the [Enable Set/Reset](#graphreg-01) and [Set/Reset](#graphreg-00) fields. These fields can provide an additional data source in addition to the data written and the latched value from the last read operation performed. Normally, data from the host is replicated four times, one for each plane. In this stage, a 1 bit in the [Enable Set/Reset](#graphreg-01) field will cause the corresponding bit plane to be replaced by the bit value in the corresponding [Set/Reset](#graphreg-00) field location, replicated 8 times to fill the byte, giving it either the value 00000000b or 11111111b. If the [Enable Set/Reset](#graphreg-01) field for a given plane is 0 then the host data byte is used instead. Note that in some write modes, the host data byte is used for other purposes, and the set/reset register is always used as data, and in other modes the set/reset mechanism is not used at all.  

The third stage performs logical operations between the host data, which has been split into four planes and is now 32-bits wide, and the latch register, which provides a second 32-bit operand. The [Logical Operation](#graphreg-03) field selects the operation that this stage performs. The four possibilities are: NOP (the host data is passed directly through, performing no operation), AND (the data is logically ANDed with the latched data.), OR (the data is logically ORed with the latched data), and XOR (the data is logically XORed with the latched data.) The result of this operation is then passed on. whilst the latched data remains unchanged, available for use in successive operations.  

In the fourth stage, individual bits may be selected from the result or copied from the latch register. Each bit of the [Bit Mask](#graphreg-08) field determines whether the corresponding bits in each plane are the result of the previous step or are copied directly from the latch register. This allows the host CPU to modify only a single bit, by first performing a dummy read to fill the latch register  

The fifth stage allows specification of what planes, if any a write operation affects, via the [Memory Plane Write Enable](#seqreg-02) field. The four bits in this field determine whether or not the write affects the corresponding plane If the a planes bit is 1 then the data from the previous step will be written to display memory, otherwise the display buffer location in that plane will remain unchanged.  

The four write modes, of which the current one is set by writing to the [Write Mode](#graphreg-05) field The four write modes, simply called write modes 0-3, based on the value of the [Write Mode](#graphreg-05) field are:

* **Write Mode 0:**
  
    Write Mode 0 is the standard and most general write mode. While the other write modes are designed to perform a specific task, this mode can be used to perform most tasks as all five operations are performed on the data. The data byte from the host is first rotated as specified by the [Rotate Count](#graphreg-03) field, then is replicated across all four planes. Then the [Enable Set/Reset](#graphreg-01) field selects which planes will receive their values from the host data and which will receive their data from that plane's [Set/Reset](#graphreg-00) field location. Then the operation specified by the [Logical Operation](#graphreg-03) field is performed on the resulting data and the data in the read latches. The [Bit Mask](#graphreg-08) field is then used to select between the resulting data and data from the latch register. Finally, the resulting data is written to the display memory planes enabled in the [Memory Plane Write Enable](#seqreg-02) field.

* **Write Mode 1:**
  
    Write Mode 1 is used to transfer the data in the latches register directly to the screen, affected only by the [Memory Plane Write Enable](#seqreg-02) field. This can facilitate rapid transfer of data on byte boundaries from one area of video memory to another or filling areas of the display with a pattern of 8 pixels. When Write Mode 0 is used with the [Bit Mask](#graphreg-08) field set to 00000000b the operation of the hardware is identical to this mode, although it is entirely possible that this mode is faster on some cards.

* **Write Mode 2:**
  
    Write Mode 2 is used to unpack a pixel value packed into the lower 4 bits of the host data byte into the 4 display planes. In the byte from the host, the bit representing each plane will be replicated across all 8 bits of the corresponding planes. Then the operation specified by the [Logical Operation](#graphreg-03) field is performed on the resulting data and the data in the read latches. The [Bit Mask](#graphreg-08) field is then used to select between the resulting data and data from the latch register. Finally, the resulting data is written to the display memory planes enabled in the [Memory Plane Write Enable](#seqreg-02) field.

* **Write Mode 3:**
  
    Write Mode 3 is used when the color written is fairly constant but the [Bit Mask](#graphreg-08) field needs to be changed frequently, such as when drawing single color lines or text. The value of the [Set/Reset](#graphreg-00) field is expanded as if the [Enable Set/Reset](#graphreg-01) field were set to 1111b, regardless of its actual value. The host data is first rotated as specified by the [Rotate Count](#graphreg-03) field, then is ANDed with the [Bit Mask](#graphreg-08) field. The resulting value is used where the [Bit Mask](#graphreg-08) field normally would be used, selecting data from either the expansion of the [Set/Reset](#graphreg-00) field or the latch register. Finally, the resulting data is written to the display memory planes enabled in the [Memory Plane Write Enable](#seqreg-02) field.

## VGA Sequencer Operation 

### Introduction

The sequencer portion of the VGA hardware reads the display memory and converts it into data that is sent to the attribute controller.  This would normally be a simple part of the video hardware, but the VGA hardware was designed to provide a degree of software compatibility with monochrome, CGA, EGA, and MCGA adapters.  For this reason, the sequencer has quite a few different modes of operation.  Further complicating programming, the sequencer has been poorly documented, resulting in many variances between various VGA/SVGA implementations.  
   
### Sequencer Memory Addressing

The sequencer operates by loading a display address into memory, then shifting it out pixel by pixel.   The memory is organized internally as 64K addresses, 32 bits wide.  The seqencer maintains an internal 16-bit counter that is used to calculate the actual index of the 32-bit location to be loaded and shifted out.  There are several different mappings from this counter to actual memory addressing, some of which use other bits from other counters, as required to provide compatibility with older hardware that uses those addressing schemes.

*&lt; More to be added here &gt;*
   
### Graphics Shifting Modes

When the [Alphanumeric Mode Disable](#graphreg-06) field is set to 1, the sequencer operates in graphics mode where data in memory references pixel values, as opposed to the character map based operation used for alphanumeric mode.  

The sequencer has three methods of taking the 32-bit memory location loaded and shifting it into 4-bit pixel values suitable for graphics modes, one of which combines 2 pixel values to form 8-bit pixel values.  The first method is the one used for the VGA's 16 color modes.  This mode is selected when both the [256-Color Shift Mode](#graphreg-05) and [Shift Register Interleave Mode](#graphreg-05) fields are set to 0.  In this mode, one bit from each of the four 8-bit planes in the 32-bit memory is used to form a 16 color value. This is shown in the diagram below, where the most significant bit of each of the four planes is shifted out into a pixel value, which is then sent to the attribute controller to be converted into an index into the DAC palette.  Following this, the remaining bits will be shifted out one bit at a time, from most to least significant bit, with the bits from planes 0-3 going to pixel bits 0-3.  
   
[![Click here for Textified Planar Shift Mode Diagram](seqplanr.gif)](seqplanr.txt)

The second shift mode is the packed shift mode, which is selected when both the [256-Color Shift Mode](#graphreg-05) field is set to 0 and the [Shift Register Interleave Mode](#graphreg-05) field is set to 1.This is used by the VGA bios to support video modes compatible with CGA video modes.  However, the CGA only uses planes 0 and 1 providing for a 4 color packed mode; however, the VGA hardware actually uses bits from two different bit planes, providing for 16 color modes.  The bits for the first four pixels shifted out for a given address are stored in planes 0 and 2.  The second four are stored in planes 1 and 3.  For each pixel, bits 3-2 are shifted out of the higher numbered plane and bits 1-0 are shifted out of the lower numbered plane.  For example, bits 3-2 of the first pixel shifted out are located in bits 7-6 of plane 2; likewise, bits 1-0 of the same pixel are located in bits 7-6 of plane 0.  
   
[![Click for Textified Packed Shift Mode Diagram](seqpack.gif)](seqpack.txt)

The third shift mode is used for 256-color modes, which is selected when the [256-Color Shift Mode](#graphreg-05) field is set to 1 (this field takes precedence over the [Shift Register Interleave Mode](#graphreg-05) field.)  This behavior of this shift mode varies among VGA implementations, due to it normally being used in combination with the [8-bit Color Enable](#attrreg-10) field of the attribute controller.  Thus certain variances in the sequencing operations can be masked by similar variances in the attribute controller.  However, the implementations I have experimented with seem to fall into one of two similar behaviors, and thus it is possible to describe both here.  Note that one is essentially a mirror image of the other, leading me to believe that the designers knew how it should work to be 100% IBM VGA compatible, but managed to get it backwards in the actual implementation. Due to being very poorly documented and understood, it is very possible that there are other implementations that vary significantly from these two cases.  I do, however, feel that attempting to specify each field's function as accurately possible can allow more powerful utilization of the hardware.  

When this shift mode is enabled, the VGA hardware shifts 4 bit pixel values out of the 32-bit memory location each dot clock.  This 4-bit value is processed by the attribute controller, and the lower 4 bits of the resulting DAC index is combined with the lower 4 bits of the previous attribute lookup to produce an 8-bit index into the DAC palette.  This is why, for example, a 320 pixel wide 256 color mode needs to be programmed with timing values for a 640 pixel wide normal mode.  In 256-color mode, each plane holds a 8-bit value which is intended to be the DAC palette index for that pixel.  Every second 8-bit index generated should correspond to the values in planes 0-3, appearing left to right on the display.  This is masked by the attribute controller, which in 256 color mode latches every second 8-bit value as well.  This means that the intermediate 8-bit values are not normally seen, and is where implementations can vary.  Another variance is whether the even or odd pixel values generated are the intended data bytes.  This also is masked by the attribute controller, which latches the appropriate even or odd pixel values.  

The first case is where the 8-bit values are formed by shifting the 4 8-bit planes left.  This is shown in the diagram below.  The first pixel value generated will be the value held in bits 7-4 of plane 0, which is then followed by bits 3-0 of plane 0.  This continues, shifting out the upper four bits of each plane in sequence before the lower four bits, ending up with bits 3-0 of plane 3.  Each pixel value is fed to the attribute controller, where a lookup operation is performed using the attribute table.  The previous 8-bit DAC index is shifted left by four, moving from the lower four bits to the upper four bits of the DAC index, and the lower 4 bits of the attribute table entry for the current pixel is shifted into the lower 4 bits of the 8-bit value, producing a new 8-bit DAC index.  Note how one 4-bit result carries over into the next display memory location sequenced.  

For example, assume planes 0-3 hold 01h, 23h, 45h, and 67h respectively, and the lower 4 bits of the the attribute table entries hold the value of the index itself, essentially using the index value as the result, and the last 8-bit DAC index generated was FEh. The first cycle, the pixel value generated is 0h, which is fed to the attribute controller and looked up, producing the table entry 0h (surprise!) The previous DAC index, FEh, is shifted left by four bits, while the new value, 0h is shifted into the lower four bits.  Thus, the new DAC index output for this pixel is E0h.  The next pixel is 1h, which produces 1h at the other end of the attribute controller.  The previous DAC index, E0h is shifted again producing 01h.  This process continues, producing the DAC indexes, in order, 12h, 23h, 34h, 45h, 56h, and 67h.  Note that every second DAC index is the appropriate 8-bit value for a 256-color mode, while the values in between contain four bits of the previous and four bits of the next DAC index.  
   
[![Click for Textified 256-Color Shift Mode Diagram (Left)](256left.gif)](256left.txt)

The second case is where the 8-bit values are formed by shifting the 8-bit values right, as depicted in the diagram below.  The first pixel value generated is the lower four bits of plane 0, followed by the upper four bits.  This continues for planes 1-3 until the last pixel value produced, which is the upper four bits of Plane 3.  These pixel values are fed to the attribute controller, where the corresponding entry in the attribute table is looked up.  The previous 8-bit DAC index is shifted right 4 places. and the lower four bits of the attribute table entry generated is used as the upper four bits of the new DAC index.  

For example, assume planes 0-3 hold 01h, 23h, 45h, and 67h respectively, and the lower 4 bits of the the attribute table entries hold the value of the index itself, essentially using the index value as the result, and the last 8-bit DAC index generated was FEh. The first cycle, the pixel value generated is 1h, which is fed to the attribute controller and looked up, producing the table entry 1h. The previous DAC index, FEh, is shifted right by four bits, while the new value, 1h is shifted into the upper four bits.  Thus, the new DAC index output for this pixel is 1Fh.  The next pixel is 0h, which produces 0h at the other end of the attribute controller.  The previous DAC index, 1Fh is shifted again producing 01h.  This process continues, producing the DAC indexes, in order, 30h, 23h, 52h, 45h, 74h, and 67h.  Again, note that every second DAC index is the appropriate 8-bit value for a 256-color mode, while the values in between contain four bits of the previous and four bits of the next DAC index.  
   
[![Click for Textified 256-Color Shift Mode Diagram (Right)](256right.gif)](256right.txt)

Another variance that can exist is whether the first or second DAC index generated at the beginning of a scan line is the appropriate 8-bit value.  If it is the second, the first DAC index contains 4 bits from the contents of the DAC index prior to the start of the scan line.  This could conceivably contain any value, as it is normally masked by the attribute controller when in 256-color mode whcih would latch the odd pixel values.  Likely this value will be either 00h or whatever the contents were at the end of the previous scan line.  A similar circumstance arises where the last pixel value generated falls on a boundary between memory addresses.  In this circumstance, however, the value generated is produced by sequencing the next display memory address as if the line continued, and is thus more predictable.  

## VGA Text Mode Operation 

* [Introduction](#vgatext-introduction) -- gives scope of this page.
* [Display Memory Organization](#display-memory-organization) -- details how the VGA's planes are utilized when in text mode.
* [Attributes](#attributes) -- details the fields of the attribute byte.
* [Fonts](#fonts) -- details the operation of the character generation hardware.
* [Cursor](#cursor) -- details on manipulating the text-mode cursor.

### <span id="vgatext-introduction">Introduction</a>

This section is intended to document the VGA's operation when it is in the text modes, including attributes and fonts. While it would seem that the text modes are adequately supported by the VGA BIOS, there is actually much that can be done with the VGA text modes that can only be accomplished by going directly to the hardware. Furthermore, I have found no good reference on the VGA text modes; most VGA references take them for granted without delving into their operation.

### Display Memory Organization

The four display memory planes are used for different purposes when the VGA is in text mode. Each byte in plane 0 is used to store an index into the character font map. The corresponding byte in plane 1 is used to specify the attributes of the character possibly including color, font select, blink, underline and reverse. For more details on attribute operation see the Attributes section below. Display plane 2 is used to store the bitmaps for the characters themselves. This is discussed in the Fonts section below. Normally, the odd/even read and write addressing mode is used to make planes 0 and 1 accessible at interleaved host memory addresses.

### Attributes

The attribute byte is divided into two four bit fields. The field from 7-4 is used as an index into the palette registers for the background color which used when a font bit is 0. The field from 3-0 is used as an index into the palette registers for the foreground which is used when a font bit is 1. Also the attribute can control several other aspects which may modify the way the character is displayed.  

If the [Blink Enable](#attrreg-10) field is set to 1, character blinking is enabled. When blinking is enabled, bit 3 of the background color is forced to 0 for attribute generation purposes, and if bit 7 of the attribute byte for a character is set to 1, the foreground color alternates between the foreground and background, causing the character to blink. The blink rate is determined by the vertical sync rate divided by 32.  

If the bits 2-0 of the attribute byte is equal to 001b and bits 6-4 of the attribute byte is equal to 000b, then the line of the character specified by the [Underline Location](#crtcreg-14) field is replaced with the foreground color. Note if the line specified by the [Underline Location](#crtcreg-14) field is not normally displayed because it is greater than the maximum scan line of the characters displayed, then the underline capability is effectively disabled.  

Bit 3 of the attribute byte, as well as selecting the foreground color for its corresponding character, also is used to select between the two possible character sets (see [Fonts](#fonts) below.) If both character sets are the same, then the bit effectively functions only to select the foreground color.

### Fonts

The VGA's text-mode hardware provides for a very fast text mode. While this mode is not used as often these days, it used to be the predominant mode of operation for applications. The reason that the text mode was fast, much faster than a graphics mode at the same resolution was that in text mode, the screen is partitioned into characters. A single character/attribute pair is written to screen, and the hardware uses a font table in video memory to map those character and attribute pairs into video output, as opposed to having to write all of the bits in a character, which could take over 16 operations to write to screen. As CPU display memory bandwidth is somewhat limited (particularly on on older cards), this made text mode the mode of choice for applications which did not require graphics.

For each character position, bit 3 of the attribute byte selects which character set is used, and the character byte selects which of the 256 characters in that font are used. Up to eight sets of font bitmaps can be stored simultaneously in display memory plane 2. The VGA's hardware provides for two banks of 256 character bitmaps to displayed simultaneously. Two fields, [Character Set A Select](#seqreg-03) and [Character Set B Select](#seqreg-03) field are used to determine which of the eight font bitmaps are currently displayed. If bit 3 of a character's attribute byte is set to 1, then the character set selected by [Character Set A Select](#seqreg-03) field, otherwise the character set specified by [Character Set B Select](#seqreg-03) field is used. Ordinarily, both character sets use the same map in memory, as utilizing 2 different character sets causes character set A to be limited to colors 0-7, and character set B to be limited to colors 8-15.  

Fonts are either 8 or 9 pixels wide and can be from 1 to 32 pixels high. The width is determined by the [9/8 Dot Mode](#seqreg-01) field. Characters normally have a line of blank pixels to the right and bottom of the character to separate the character from its neighbor. Normally this is included in the character's bitmap, leaving only 7 bit columns for the character. Characters such as the capital M have to be squished to fit this, and would look better if all 8 pixels in the bitmap could be used, as in 9 Dot mode where the characters have an extra ninth bit in width, which is displayed in the text background color, However, this causes the line drawing characters to be discontinuous due to the blank column. Fortunately, the [Line Graphics Enable](#attrreg-10) field can be set to allow character codes C0h-DFh to have their ninth column be identical to their eighth column, providing for continuity between line drawing characters. The height is determined by the [Maximum Scan Line](#crtcreg-09) field which is set to one less than the number of scan lines in the character.  

Display memory plane 2 is divided up into eight 8K banks of characters, each of which holds 256 character bitmaps. Each character is on a 32 byte boundary and is 32 bytes long. The offset in plane 2 of a character within a bank is determined by taking the character's value and multiplying it by 32. The first byte at this offset contains the 8 pixels of the top scan line of the characters. Each successive byte contains another scan line's worth of pixels. The best way to read and write fonts to display memory, assuming familiarity with the information from the [Accessing the Display Memory](#accessing-the-display-memory) page, is to use standard (not Odd/Even) addressing and Read Mode 0 and Write Mode 0 with plane 2 selected for read or write.  

The following example shows three possible bitmap representations of text characters. In the left example an 8x8 character box is used. In this case, the [Maximum Scan Line](#crtcreg-09) field is programmed to 7 and the [9/8 Dot Mode](#seqreg-01) field is programmed to 0. Note that the bottom row and right-most column is blank. This is used to provide inter-character spacing. The middle example shows an 8x16 character. In this case the [Maximum Scan Line](#crtcreg-09) field is programmed to 15 and the [9/8 Dot Mode](#seqreg-01) field is programmed to 0. Note that the character has extra space at the bottom below the baseline of the character. This is used by characters with parts descending below the baseline, such as the lowercase letter "g". The right example shows a 9x16 character. In this case the [Maximum Scan Line](#crtcreg-09) field is programmed to 15 and the [9/8 Dot Mode](#seqreg-01) field is programmed to 1. Note that the rightmost column is used by the character, as the ninth column for 9-bit wide characters is assumed blank (excepting for the behavior of the the [Line Graphics Enable](#attrreg-10) field.) allowing all eight bits of width to be used to specify the character, instead of having to devote an entire column for inter-character spacing.

[![Click for Textified Examples of Text Mode Bitmap Characters](Char.gif)](char.txt)

### Cursor

The VGA has the hardware capability to display a cursor in the text modes. Further details on the text-mode cursor's operation can be found in the following section.

## Manipulating the Text-mode Cursor

* [Introduction](#textcur-introduction) -- gives overview of text-mode cursor capabilities
* [Enabling/Disabling the Cursor](#enablingdisabling-the-cursor) -- details on making the cursor visible or not visible
* [Manipulating the Cursor Position](#manipulating-the-cursor-position) -- details on controlling the cursor's placement
* [Manipulating the Cursor Shape](#manipulating-the-cursor-shape) -- details on controlling the cursor's appearance
* [Cursor Blink Rate](#cursor-blink-rate) -- provides information about the cursor's blink rate
* [Cursor Color](#cursor-color) -- provides information regarding the cursor's color

### <span id="textcur-introduction">Introduction</a>
 When dealing with the cursor in most high-level languages, the cursor is defined as the place where the next text output will appear on the display. When dealing directly with the display, the cursor is simply a blinking area of a particular character cell. A program may write text directly to the display independent of the current location of the cursor. The VGA provides facilities for specifying whether a cursor is to be displayed, where the cursor is to appear, and the shape of the cursor itself. Note that this cursor is only used in the text modes of the standard VGA and is not to be confused with the graphics cursor capabilities of particular SVGA chipsets.

### Enabling/Disabling the Cursor

On the VGA there are three main ways of disabling the cursor. The most straightforward is to set the [Cursor Disable](#crtcreg-0A) field to 1. Another way is to set the [Cursor Scan Line End](#crtcreg-0B) field to a value less than that of the [Cursor Scan Line Start](#crtcreg-0A) field. On some adapters such as the IBM EGA, this will result instead in a split block cursor. The third way is to set the cursor location to a location off-screen. The first two methods are specific to VGA and compatible adapters and are not guaranteed to work on non-VGA adapters, while the third method should.

### Manipulating the Cursor Position

When dealing with the cursor in standard BIOS text modes, the cursor position is specified by row and column. The VGA hardware, due to its flexibility to display any different text modes, specifies cursor position as a 16-bit address. The upper byte of this address is specified by the [Cursor Location High Register](#crtcreg-0E), and the lower by the [Cursor Location Low Register](#crtcreg-0F). In addition this value is affected by the [Cursor Skew](#crtcreg-0B) field. When the hardware fetches a character from display memory it compares the address of the character fetched to that of the cursor location added to the [Cursor Skew](#crtcreg-0B) field. If they are equal and the cursor is enabled, then the character is written with the current cursor pattern superimposed. Note that the address compared to the cursor location is the address in display memory, not the address in host memory. Characters and their attributes are stored at the same address in display memory in different planes, and it is the odd/even addressing mode usually used in text modes that makes the interleaved character/attribute pairs in host memory possible. Note that it is possible to set the cursor location to an address not displayed, effectively disabling the cursor.  

The [Cursor Skew](#crtcreg-0B) field was used on the EGA to synchronize the cursor with internal timing. On the VGA this is not necessary, and setting this field to any value other than 0 may result in undesired results. For example, on one particular card, setting the cursor position to the rightmost column and setting the skew to 1 made the cursor disappear entirely. On the same card, setting the cursor position to the leftmost column and setting the skew to 1 made an additional cursor appear above and to the left of the correct cursor. At any other position, setting the skew to 1 simply moved the cursor right one position. Other than these undesired effects, there is no function that this register can provide that could not be obtained by simply increasing the cursor location.

### Manipulating the Cursor Shape

On the VGA, the text-mode cursor consists of a line or block of lines that extend horizontally across the entire scan line of a character cell. The first, topmost line is specified by the [Cursor Scan Line Start](#crtcreg-0A) field. The last, bottom most line is specified by the [Cursor Scan Line End](#crtcreg-0B) field. The scan lines in a character cell are numbered from 0 up to the value of the [Maximum Scan Line](#crtcreg-09) field. On the VGA if the [Cursor Scan Line End](#crtcreg-0B) field is less than the [Cursor Scan Line Start](#crtcreg-0A) field, no cursor will be displayed. Some adapters, such as the IBM EGA may display a split-block cursor instead.

### Cursor Blink Rate

On the standard VGA, the blink rate is dependent on the vertical frame rate. The on/off state of the cursor changes every 16 vertical frames, which amounts to 1.875 blinks per second at 60 vertical frames per second. The cursor blink rate is thus fixed and cannot be software controlled on the standard VGA. Some SVGA chipsets provide non-standard means for changing the blink rate of the text-mode cursor.

### Cursor Color

On the standard VGA, the cursor color is obtained from the foreground color of the character that the cursor is superimposing. On the standard VGA there is no way to modify this behavior.  

## Special Effects Hardware 

* [Introduction](#vgafx-introduction) -- describes the capabilities of the VGA special effects hardware.
* [Windowing](#windowing) -- provides rough panning and scrolling of a larger virtual image.
* [Paging](#paging) -- provides the ability to switch between multiple screens rapidly.
* [Smooth Panning and Scrolling](#smooth-panning-and-scrolling) -- provides more precise control when panning and scrolling.
* [Split-Screen Operation](#split-screen-operation) -- provides a horizontal division which allows independent scrolling and panning of the top window.

### <span id="vgafx-introduction">Introduction</span>

This section describes the capabilities of the VGA hardware that can be used to implement special effects such as windowing, paging, smooth panning and scrolling, and split screen operation.. These functions are probably the least utilized of all of the VGA's capabilities, possibly because most texts devoted to video hardware provide only brief documentation. Also, the video BIOS provides no support for these capabilities so the VGA card must be programmed at the hardware level in order to utilize these capabilities. Windowing allows a program to view a portion of an image in display memory larger than the current display resolution, providing rough panning and scrolling. Paging allows multiple display screens to be stored in the display memory allowing rapid switching between them. Smooth panning and scrolling works in conjunction with windowing to provide more precise control of window position. Split-screen operation allows the creation of a horizontal division on the screen that creates a window below that remains fixed in place independent of the panning and scrolling of the window above. These features can be combined to provide powerful control of the display with minimal demand on the host CPU.

### Windowing

The VGA hardware has the ability treat the display as a window which can pan and/or scroll across an image larger than the screen, which is used by some windowing systems to provide a virtual scrolling desktop, and by some games and assembly demos to provide scrolling. Some image viewers use this to allow viewing of images larger than the screen. This capability is not limited to graphics mode; some terminal programs use this capability to provide a scroll-back buffer, and some editors use this to provide an editing screen wider than 80 columns.  
        This feature can be implemented by brute force by simply copying the portion of the image to be displayed to the screen. Doing this, however takes significant processor horsepower. For example, scrolling a 256 color 320x200 display at 30 frames per second by brute force requires a data transfer rate of 1.92 megabytes/second. However, using the hardware capability of the VGA the same operation would require a data transfer rate of only 120 bytes/second. Obviously there is an advantage to using the VGA hardware. However, there are some limitations--one being that the entire screen must scroll (or the top portion of the screen if split-screen mode is used.) and the other being that the maximum size of the virtual image is limited to the amount of video memory accessible, although it is possible to redraw portions of the display memory to display larger virtual images.  

In text mode, windowing allows panning at the character resolution. In graphics mode, windowing allows panning at 8-bit resolution and scrolling at scan-line resolution. For more precise control, see [Smooth Panning and Scrolling](#smooth) below. Because the VGA BIOS and most programming environment's graphics libraries do not support windowing, you must modify or write your own routines to write to the display for functions such as writing text or graphics. This section assumes that you have the ability to work with the custom resolutions possible when windowing is used.  

In order to understand virtual resolutions it is necessary to understand how the VGA's [Start Address High Register](#crtcreg-0C), [Start Address Low Register](#crtcreg-0D), and [Offset](#crtcreg-13) field work. Because display memory in the VGA is accessed by a 32-bit bus, a 16-bit address is sufficient to uniquely identify any location in the VGA's 256K address space. The [Start Address High Register](#crtcreg-0C) and [Start Address Low Register](#crtcreg-0D) provide such an address. This address is used to specify either the location of the first character in text mode or the position of the first byte of pixels in graphics mode. At the end of the vertical retrace, the current line start address is loaded with this value. This causes one scan line of pixels or characters to be output starting at this address. At the beginning of the next scan-line (or character row in text mode) the value of the [Offset Register](#crtcreg-13) multiplied by the current memory address size * 2 is added to the current line start address. The [Double-Word Addressing](#crtcreg-14) field and the [Word/Byte](#crtcreg-17) field specify the current memory address size. If the value of the [Double-Word Addressing](#crtcreg-14) field is 1, then the current memory address size is four (double-word). Otherwise, the [Word/Byte](#crtcreg-17) field specifies the current memory address size. If the value of the [Word/Byte](#crtcreg-17) field is 0 then the current memory address size is 2 (word) otherwise, the current memory address size is 1 (byte).  

Normally in graphics modes, the offset register is programmed to represent (after multiplication) the number of bytes in a scan line. This means that (unless a CGA/MDA emulation mode is in effect) scan lines will be arranged sequentially in memory with no space in between, allowing for the most compact representation in display memory. However, this does not have to be the case--in fact, by increasing the value of the offset register we can leave "extra space" between lines. This is what provides for virtual widths. By programming the offset register to the value of the equation:

```
Offset = VirtualWidth / ( PixelsPerAddress * MemoryAddressSize * 2 )
```

`VirtualWidth` is the width of the virtual resolution in pixels, and `PixelsPerAddress` is the number of pixels per display memory address (1, 2, 4 or 8) depending on the current video mode. For virtual text modes, the offset register is programmed with the value of the equation:

```
Offset = VirtualWidth / ( MemoryAddressSize * 2 )
```

In text mode, there is always one character per display memory address. In standard CGA compatible text modes, MemoryAddressSize is 2 (word).  

After you have programmed the new offset, the screen will now display only a portion of a virtual display. The screen will display the number of scan-lines as specified by the current mode. If the screen reaches the last byte of memory, the next byte of memory will wrap around to the first byte of memory. Remember that the Start Address specifies the display memory address of the upper-left hand character or pixel. Thus the maximum height of a virtual screen depends on the width of the virtual screen. By increasing this by the number of bytes in a scan-line (or character row), the display will scroll one scan-line or character row vertically downwards. By increasing the Start Address by less than the number of bytes in a scan line, you can move the virtual window horizontally to the right. If the virtual width is the same as the actual width, one can create a vertical scrolling mode. This is used sometimes as an "elevator" mode or to provide rapid scrollback capability in text mode. If the virtual height is the same as the actual height, then only horizontal panning is possible, sometimes called "panoramic" mode. In any case, the equation for calculating the Start Address is:

```
Start Address = StartingOffset + Y * BytesPerVirtualRow + X
```

`Y` is the vertical position, from 0 to the value of the `VirtualHeight - ActualHeight`. `X` is the horizontal position, from 0 to the value of `BytesPerVirtualRow - BytesPerActualRow` . These ranges prevent wrapping around to the left side of the screen, although you may find it useful to use the wrap-around for whatever your purpose. Note that the wrap-around simply starts displaying the next row/scan-line rather than the current one, so is not that useful (except when using programming techniques that take this factor into account.) Normally StartingOffset is 0, but if paging or split-screen mode is being used, or even if you simply want to relocate the screen, you must change the starting offset to the address of the upper-left hand pixel of the virtual screen.  

For example, a 512x300 virtual screen in a 320x200 16-color 1 bit/pixel planar display would require 512 pixels / 8 pixels/byte = 64 bytes per row and 64 bytes/row * 300 lines = 19200 bytes per screen. Assuming the VGA is in byte addressing mode, this means that we need to program the offset register [Offset](#crtcreg-13) field with 512 pixels / (8 pixels/byte * 1 * 2) = 32 (20h). Adding one to the start address will move the display screen to the right eight pixels. More precise control is provided by the smooth scrolling mechanism. Adding 64 to the start address will move the virtual screen down one scan line. See the following chart which shows the virtual screen when the start address is calculated with an X and Y of 0:

[![Click for Textified Virtual Screen Mode Example](virtual.gif)](virtual.txt)

### Paging

The video display memory may be able to hold more than one screen of data (or virtual screen if virtual resolutions are used.) These multiple screens, called pages, allows rapid switching between them. As long as they both have the same actual (and virtual if applicable) resolution, simply changing the Start Address as given by the [Start Address High Register](#crtcreg-0C) and [Start Address Low Register](#crtcreg-0D) pair to point to the memory address of the first byte of the page (or set the StartingOffset term in the equation for virtual resolutions to the first memory address of the page.) If they have different virtual widths, then the [Offset](#crtcreg-13) field must be reprogrammed. It is possible to store both graphics and text pages simultaneously in memory, in addition to different graphics mode pages. In this case, the video mode must be changed when changing pages. In addition, in text mode the Cursor Location must be reprogrammed for each page if it is to be displayed. Also paging allows for double buffering of the display -- the CPU can write to one page while the VGA hardware is displaying another. By switching between pages during the vertical retrace period, flicker free screen updates can be implemented.  

An example of paging is that used by the VGA BIOS in the 80x25 text mode. Each page of text takes up 2000 memory address locations, and the VGA uses a 32K memory aperture, with the Odd/Even addressing enabled. Because Odd/Even addressing is enabled, each page of text takes up 4000 bytes in host memory, thus 32768 / 4000 = 8 (rounded down) pages can be provided and can be accessed at one time by the CPU. Each page starts at a multiple of 4096 (1000h). Because the display controller circuitry works independent of the host memory access mode, this means that each page starts at a display address that is a multiple of 2048 (800h), thus the Starting Address is programmed to the value obtained by multiplying the page to be displayed by 2048 (800h). See the following chart which shows the arrangement of these pages in display memory:  

[![Click here to display a textified Paging Memory Utilization Example](paging.gif)](paging.txt)

### Smooth Panning and Scrolling

Because the Start Address field only provides for scrolling and panning at the memory address level, more precise panning and scrolling capability is needed to scroll at the pixel level as multiple pixels may reside at the same memory address especially in text mode where the Start Address field only allows panning and scrolling at the character level.  

Pixel level panning is controlled by the [Pixel Shift Count](#attrreg-13) and [Byte Panning](#crtcreg-08) fields. The [Pixel Shift Count](#attrreg-13) field specifies the number of pixels to shift left. In all graphics modes and text modes except 9 dot text modes and 256 color graphics modes, the [Pixel Shift Count](#attrreg-13) is defined for values 0-7. This provides the pixel level control not provided by the [Start Address Register](#crtcreg-0D) or the [Byte Panning](#crtcreg-08) fields. In 9 dot text modes the [Pixel Shift Count](#attrreg-13) is field defined for values 8, and 0-7, with 8 being the minimum shift amount and 7 being the maximum. In 256 color graphics modes, due to the way the hardware makes a 256 color value by combining 2 16-bit values, the [Pixel Shift Count](#attrreg-13) field is only defined for values 0, 2, 4, and 6. Values 1, 3, 5, and 7 cause the screen to be distorted due to the hardware combining 4 bits from each of 2 adjacent pixels. The [Byte Panning](#crtcreg-08) field is added to the [Start Address Register](#crtcreg-0D) when determining the address of the top-left hand corner of the screen, and has the value from 0-3. Combined, both panning fields allow a shift of 15, 31, or 35 pixels, dependent upon the video mode. Note that programming the [Pixel Shift Count](#attrreg-13) field to an undefined value may cause undesired effects and these effects are not guaranteed to be identical on all chipsets, so it is best to be avoided.  

Pixel level scrolling is controlled by the [Preset Row Scan](#crtcreg-08) field. This field may take any value from 0 up to the value of the [Maximum Scan Line](#crtcreg-09) field; anything greater causes interesting artifacts (there is no guarantee that the result will be the same for all VGA chipsets.) Incrementing this value will shift the screen upwards by one scan line, allowing for smooth scrolling in modes where the Offset field does not provide precise control.

### Split-screen Operation

The VGA hardware provides the ability to specify a horizontal division which divides the screen into two windows which can start at separate display memory addresses. In addition, it provides the facility for panning the top window independent of the bottom window. The hardware does not provide for split-screen modes where multiple video modes are possible in one display screen as provided by some non-VGA graphics controllers. In addition, there are some limitations, the first being that the bottom window's starting display memory address is fixed at 0. This means that (unless you are using split screen mode to duplicate memory on purpose) the bottom screen must be located first in memory and followed by the top. The second limitation is that either both windows are panned by the same amount, or only the top window pans, in which case, the bottom window's panning values are fixed at 0. Another limitation is that the [Preset Row Scan](#crtcreg-08) field only applies to the top window -- the bottom window has an effective Preset Row Scan value of 0.  

The Line Compare field in the VGA, of which bit 9 is in the [Maximum Scan Line Register](#crtcreg-09), bit 8 is in the [Overflow Register](#crtcreg-07), and bits 7-0 are in the [Line Compare Register](#crtcreg-18), specifies the scan line address of the horizontal division. When the line counter reaches the value in the Line Compare Register, the current scan line start address is reset to 0. If the [Pixel Panning Mode](#attrreg-10) field is set to 1 then the [Pixel Shift Count](#attrreg-13) and [Byte Panning](#crtcreg-08) fields are reset to 0 for the remainder of the display cycle allowing the top window to pan while the bottom window remains fixed. Otherwise, both windows pan by the same amount.  
 
## DAC Operation 

* [Introduction](#vgadac-introduction) -- details the standard VGA DAC capabilities.
* [DAC Subsystem](#dac-subsystem) -- gives a description of the DAC hardware.
* [Programming the DAC](#programming-the-dac) -- details reading and writing to DAC memory.
* [Programming Precautions](#programming-precautions) -- details potential problems that can be encountered with DAC hardware.
* [Eliminating Flicker](#eliminating-flicker) -- details on how to manipulate DAC memory without causing visible side-effects.
* [The DAC State](#the-dac-state) -- details one possible use for an otherwise useless field

### Introduction

One of the improvements the VGA has over the EGA hardware is in the amount of possible colors that can be generated, in addition to an increase in the amount of colors that can be displayed at once. The VGA hardware has provisions for up to 256 colors to be displayed at once, selected from a range of 262,144 (256K) possible colors. This capability is provided by the DAC subsystem, which accepts attribute information for each pixel and converts it into an analog signal usable by VGA displays.

### DAC Subsystem

The VGA's DAC subsystem accepts an 8 bit input from the attribute subsystem and outputs an analog signal that is presented to the display circuitry. Internally it contains 256 18-bit memory locations that store 6 bits each of red, blue, and green signal levels which have values ranging from 0 (minimum intensity) to 63 (maximum intensity.) The DAC hardware takes the 8-bit value from the attribute subsystem and uses it as an index into the 256 memory locations and obtains a red, green, and blue triad and produces the necessary output.  

Note -- the DAC subsystem can be implemented in a number of ways, including discrete components, in a DAC chip which may or may not contain internal ram, or even integrated into the main chipset ASIC itself. Many modern DAC chipsets include additional functionality such as hardware cursor support, extended color mapping, video overlay, gamma correction, and other functions. Partly because of this it is difficult to generalize the DAC subsystem's exact behavior. This document focuses on the common functionality of all VGA DACs; functionality specific to a particular chipset are described elsewhere.

### Programming the DAC

The DAC's primary host interface (there may be a secondary non-VGA compatible access method) is through a set of four external registers containing the [DAC Write Address](#3C8), the [DAC Read Address](#3C7W), the [DAC Data](#3C9), and the [DAC State](#3C7R) fields. The DAC memory is accessed by writing an index value to the [DAC Write Address](#3C8) field for write operations, and to the [DAC Read Address](#3C7W) field for read operations. Then reading or writing the [DAC Data](#3C9) field, depending on the selected operation, three times in succession returns 3 bytes, each containing 6 bits of red, green, and blue intensity values, with red being the first value and blue being the last value read/written. The read or write index then automatically increments such that the next entry can be read without having to reprogramming the address. In this way, the entire DAC memory can be read or written in 768 consecutive I/O cycles to/from the [DAC Data](#3C9) field. The [DAC State](#3C7R) field reports whether the DAC is setup to accept reads or writes next.

### Programming Precautions

Due to the variances in the different implementations, programming the DAC takes extra care to ensure proper operation across the range of possible implementations. There are a number of things can cause undesired effects, but the simplest way to avoid problems is to ensure that you program the [DAC Read Address](#3C7W) field or the [DAC Write Address](#3C8) field before each read operation (note that a read operation may include reads/writes to multiple DAC memory entries.) And always perform writes and reads in groups of 3 color values. The DAC memory may not be updated properly otherwise. Reading the value of the [DAC Write Address](#3C8) field may not produce the expected result, as some implementations may return the current index and some may return the next index. This operation may even be dependent on whether a read or write operation is being performed. While it may seem that the DAC implements 2 separate indexes for read and write, this is often not the case, and interleaving read and write operations may not work properly without reprogramming the appropriate index.

* **Read Operation**
    * Disable interrupts (this will ensure that a interrupt service routine will not change the DAC's state)
    * Output beginning DAC memory index to the [DAC Read Address](#3C7W) register.
    * Input red, blue, and green values from the [DAC Data](#3C9) register, repeating for the desired number of entries to be read.
    * Enable interrupts
* **Write Operation**
    * Disable interrupts (this will ensure that a interrupt service routine will not change the DAC's state)
    * Output beginning DAC memory index to the [DAC Write Address](#3C8) register.
    * Output red, blue, and green values to the [DAC Data](#3C9) register, repeating for the desired number of entries to be read.
    * Enable interrupts

### Eliminating Flicker

An important consideration when programming the DAC memory is the possible effects on the display generation. If the DAC memory is accessed by the host CPU at the same time the DAC memory is being used by the DAC hardware, the resulting display output may experience side effects such as flicker or "snow". Note that both reading and writing to the DAC memory has the possibility of causing these effects. The exact effects, if any, are dependent on the specific DAC implementation. Unfortunately, it is not possible to detect when side-effects will occur in all circumstances. The best measure is to only access the DAC memory during periods of horizontal or vertical blanking. However, this puts a needless burden on programs run on chipsets that are not affected. If performance is an issue, then allowing the user to select between flicker-prone and flicker-free access methods could possibly improve performance.

### The DAC State

The [DAC State](#3C7R) field seems to be totally useless, as the DAC state is usually known by the programmer and it does not give enough information (about whether a red, green, or blue value is expected next) for a interrupt routine or such to determine the DAC state. However, I can think of one possible use for it. You can use the DAC state to allow an interrupt driven routine to access the palette (like for palette rotation effects or such) while still allowing the main thread to write to the DAC memory. When the interrupt routine executes it should check the DAC state. If the DAC state is in a write state, it should not access the DAC memory. If it is in a read state, the routine should perform the necessary DAC accesses then return the DAC to a read state. This means that the main thread use the DAC state to control the execution of the ISR. Also it means that it can perform writes to the DAC without having to disable interrupts or otherwise inhibit the ISR.  
 
## VGA Display Generation 

### Introduction

This page documents the configuration of the VGA's CRTC registers which control the framing and timing of video signals sent to the display device, usually a monitor.

### Dot Clocks

The standard VGA has two "standard" dot clock frequencies available to it, as well as a possible "external" clock source, which is implementation dependent.  The two standard clock frequencies are nominally 25 Mhz and 28 MHz.  Some chipsets use 25.000 MHz and 28.000 MHz, while others use slightly greater clock frequencies.  The IBM VGA chipset I have uses 25.1750 MHz  Mhz and 28.3220 crystals.  Some newer cards use the closest generated frequency produced by their clock chip.  In most circumstances the IBM VGA timings can be assumed as the monitor should allow an amount of variance; however, if you know the actual frequencies used you should use them in your timing calculations.  

The dot clock source in the VGA hardware is selected using the [Clock Select](#3CCR3C2W) field.  For the VGA, two of the values are undefined; some SVGA chipsets use the undefined values for clock frequencies used for 132 column mode and such.  The 25 MHz clock is designed for 320 and 640 pixel modes and the 28 MHz is designed for 360 and 720 pixel modes. The [Dot Clock Rate](#seqreg-01) field specifies whether to use the dot clock source directly or to divide it in half before using it as the actual dot clock rate.

### Horizontal Timing

The VGA measures horizontal timing periods in terms of character clocks, which can either be 8 or 9 dot clocks, as specified by the [9/8 Dot Mode](#seqreg-01) field.  The 9 dot clock mode was included for monochrome emulation and 9-dot wide character modes, and can be used to provide 360 and 720 pixel wide modes that work on all standard VGA monitors, when combined with a 28 Mhz dot clock. The VGA uses a horizontal character counter which is incremented at each character, which the horizontal timing circuitry compares against the values of the horizontal timing fields to control the horizontal state. The horizontal periods that are controlled are the active display, overscan, blanking, and refresh periods.  

The start of the active display period coincides with the resetting of the horizontal character counter, thus is fixed at zero.  The value at which the horizontal character is reset is controlled by the [Horizontal Total](#crtcreg-00) field. Note, however, that the value programmed into the [Horizontal Total](#crtcreg-00) field is actually 5 less than the actual value due to timing concerns.  

The end of the active display period is controlled by the [End Horizontal Display](#crtcreg-01) field.  When the horizontal character counter is equal to the value of this field, the sequencer begins outputting the color specified by the [Overscan Palette Index](#attrreg-11) field.  This continues until the active display begins at the beginning of the next scan line when the active display begins again.  Note that the horizontal blanking takes precedence over the sequencer and attribute controller.  

The horizontal blanking period begins when the character clock equals the value of the [Start Horizontal Blanking](#crtcreg-02) field.  During the horizontal blanking period, the output voltages of the DAC signal the monitor to turn off the guns.   Under normal conditions, this prevents the overscan color from being displayed during the horizontal retrace period.  This period extends until the lower 6 bits of the [End Horizontal Blanking](#crtcreg-03) field match the lower 6 bits of the horizontal character counter.  This allows for a blanking period from 1 to 64 character clocks, although some implementations may treat 64 as 0 character clocks in length.  The blanking period may occur anywhere in the scan line, active display or otherwise even though its meant to appear outside the active display period.  It takes precedence over all other VGA output.  There is also no requirement that blanking occur at all.  If the [Start Horizontal Blanking](#crtcreg-02) field falls outside the maximum value of the character clock determined by the [Horizontal Total](#crtcreg-00) field, then no blanking will occur at all.  Note that due to the setting of the [Horizontal Total](#crtcreg-00) field, the first match for the [End Horizontal Blanking](#crtcreg-03) field may be on the following scan line.  

Similar to the horizontal blanking period, the horizontal retrace period is specified by the [Start Horizontal Retrace](#crtcreg-04) and [End Horizontal Retrace](#crtcreg-05) fields. The horizontal retrace period begins when the character clock equals the value stored in the [Start Horizontal Retrace](#crtcreg-04) field.  The horizontal retrace ends when the lower 5 bits of the character clock match the bit pattern stored in the [End Horizontal Retrace](#crtcreg-05) field, allowing a retrace period from 1 to 32 clocks; however, a particular implementation may treat 32 clocks as zero clocks in length.  The operation of this is identical to that of the horizontal blanking mechanism with the exception of being a 5 bit comparison instead of 6, and affecting the horizontal retrace signal instead of the horizontal blanking.  

There are two horizontal timing fields that are described as being related to internal timings of the VGA, the [Display Enable Skew](#crtcreg-03) and [Horizontal Retrace Skew](#crtcreg-05) fields.  In the VGA they do seem to affect the timing, but also do not seem to be necessary for the operation of the VGA and are pretty much unused.  These registers were required by the IBM VGA implementations, so I'm assuming this was added in the early stages of the VGA design for EGA compatibility, but the internal timings were changed to more friendly ones making the use of these fields unnecessary.  It seems to be totally safe to set these fields to 0 and ignore them.  See the register descriptions for more details, if you have to deal with software that programs them.

### Vertical Timing

The VGA maintains a scanline counter which is used to measure vertical timing periods.  This counter begins at zero which coincides with the first scan line of the active display.  This counter is set to zero before the beginning of the first scanline of the active display.  Depending on the setting of the [Divide Scan Line Clock by 2](#crtcreg-17) field, this counter is incremented either every scanline, or every second scanline.  The vertical scanline counter is incremented before the beginning of each horizontal scan line, as all of the VGA's vertical timing values are measured at the beginning of the scan line, after the counter has ben set/incremented.  The maximum value of the scanline counter is specified by the [Vertical Total](#crtcreg-06) field.  Note that, like the rest of the vertical timing values that "overflow" an 8-bit register, the most significant bits are located in the [Overflow Register](#crtcreg-07).  The [Vertical Total](#crtcreg-06) field is programmed with the value of the scanline counter at the beginning of the last scanline.  

The vertical active display period begins when the scanline counter is at zero, and extends up to the value specified by the [Vertical Display End](#crtcreg-12) field.  This field is set with the value of the scanline counter at the beginning of the first inactive scanline, telling the video hardware when to stop outputting scanlines of sequenced pixel data and outputs the attribute specified by the [Overscan Palette Index](#attrreg-11) field in the horizontal active display period of those scanlines.  This continues until the start of the next frame when the active display begins again.  

The [Start Vertical Blanking](#crtcreg-15) and [End Vertical Blanking](#crtcreg-16) fields control the vertical blanking interval.  The [Start Vertical Blanking](#crtcreg-15) field is programmed with the value of the scanline counter at the beginning of the scanline to begin blanking at.  The value of the [End Vertical Blanking](#crtcreg-16) field is set to the lower eight bits of the scanline counter at the beginning of the scanline after the last scanline of vertical blanking.  

The [Vertical Retrace Start](#crtcreg-10) and [Vertical Retrace End](#crtcreg-11) fields determine the length of the vertical retrace interval.  The [Vertical Retrace Start](#crtcreg-10) field contains the value of the scanline counter at the beginning of the first scanline where the vertical retrace signal is asserted.  The [Vertical Retrace End](#crtcreg-11) field is programmed with the value of the lower four bits of the scanline counter at the beginning of the scanline after the last scanline where the vertical retrace signal is asserted.

### Monitoring Timing

There are certain operations that should be performed during certain periods of the display cycle to minimize visual artifacts, such as attribute and DAC writes.  There are two bit fields that return the current state of the VGA, the [Display Disabled](#3xAR) and [Vertical Retrace](#3xAR) fields. The [Display Disabled](#3xAR) field is set to 1 when the display enable signal is not asserted, providing the programmer with a means to determine if the video hardware is currently refreshing the active display or it is currently outputting blanking.  

The [Vertical Retrace](#3xAR) field signals whether or not the VGA is in a vertical retrace period.  This is useful for determining the end of a display period, which can be used by applications that need to update the display every period such as when doing animation.  Under normal conditions, when the blanking signal is asserted during the entire vertical retrace, this can also be used to detect this period of blanking, such that a large amount of register accesses can be performed, such as reloading the complete set of DAC entries.

### Miscellaneous

There are a few registers that affect display generation, but don't fit neatly into the horizontal or vertical timing categories.  The first is the [Sync Enable](#crtcreg-17) field which controls whether the horizontal and vertical sync signals are sent to the display or masked off.  The sync signals should be disabled while setting up a new mode to ensure that an improper signal that could damage the display is not being output.  Keeping the sync disabled for a period of one or more frames helps the display determine that a mode change has occurred as well.  

The [Memory Refresh Bandwidth](#crtcreg-11) field is used by the original IBM VGA hardware and some compatible VGA/SVGA chipsets to control how often the display memory is refreshed.  This field controls whether the VGA hardware provides 3 or 5 memory refresh cycles per scanline.  At or above VGA horizontal refresh rates, this field should be programmed for 3 memory refresh cycles per scanline.  Below this rate, for compatibility's sake the 5 memory refresh cycles per scanline setting might be safer, see the [Memory Refresh Bandwidth](#crtcreg-11) field for (slightly) more information.
   
## Accessing the VGA Registers 

* [Introduction](#vgareg-introduction) -- provides a general overview of accessing the VGA registers.
* [General Advice](#general-advice) -- basic guidelines for use when accessing VGA registers.
* [I/O Fudge Factor](#io-fudge-factor) -- discusses delays between I/O accesses.
* [Paranoia](#paranoia) -- discusses making code more robust.
* [Accessing the External Registers](#accessing-the-external-registers) -- details and guidelines for accessing these registers.
* [Accessing the Sequencer, Graphics, and CRT Controller Registers](#accessing-the-sequencer-graphics-and-crt-controller-registers) -- details and guidelines for accessing these registers, including step-by-step instructions.
* [Accessing the Attribute Registers](#accessing-the-attribute-registers) -- details and guidelines for accessing this register, including step-by-step instructions.
* [Accessing the Color Registers](#accessing-the-color-registers) -- details and guidelines for accessing this register, including step-by-step instructions.
* [Binary Operations](#binary-operations) -- details on the operation of the logical operators OR, AND, and XOR.
* [Example Register](#example-register) -- an example register selected to demonstrate both the format they will be presented in and how fields work.
* [Masking Bit-Fields](#masking-bit-fields) -- details on changing specific fields within registers using the logical operators.

### <span id="vgareg-introduction">Introduction</span>

This section discusses methods of manipulating the particular registers present in VGA hardware. Depending upon which register one is accessing, the method of accessing them is different and sometimes difficult to understand. The VGA has many more registers than it has I/O ports, thus it must provide a way to re-use or multiplex many registers onto a relatively small number of ports. All of the VGA ports are accessed by inputting and outputting bytes to I/O ports; however, in many cases it is necessary to perform additional steps to ready the VGA adapter for reading and writing data. Port addresses are given at their hexadecimal address, such as 3C2h.

### General Advice

If a program takes control of the video card and changes its state, it is considered good programming practice to keep track of the original values of any register it changes such that upon termination (normal or abnormal) it can write them back to the hardware to restore the state. Anyone who has seen a graphics application abort in the middle of a graphics screen knows how annoying this can be. Almost all of the VGA registers can be saved and restored in this fashion. In addition when changing only a particular field of a register, the value of the register should be read and the byte should be masked so that only the field one is trying to change is actually changed.

### I/O Fudge Factor

Often a hardware device is not capable handling I/O accesses as fast as the processor can issue them. In this case, a program must provide adequate delay between I/O accesses to the same device. While many modern chipsets provide this delay in hardware, there are still many implementations in existence that do not provide this delay. If you are attempting to write programs for the largest possible variety of hardware configurations, then it is necessary to know the amount of delay necessary. Unfortunately, this delay is not often specified, and varies from one VGA implementation to another. In the interest of performance it is ideal to keep this delay to the minimum necessary. In the interest of compatibility it is necessary to implement a delay independent of clock speed. (Faster processors are continuously being developed, and also a user may change clock speed dynamically via the Turbo button on their case.)

### Paranoia

If one wishes to be extra cautious when writing to registers, after writing to a register one can read the value back and compare it with the original value. If they differ it may mean that the VGA hardware has a stuck bit in one its registers, that you are attempting to modify a locked or unsupported register, or that you are not providing enough delay between I/O accesses. As long as reading the register twice doesn't have any unintended side effects, when reading a registers value, one can read the register twice and compare the values read, after masking out any fields that may change without CPU intervention. If the values read back are different it may mean that you are not providing enough delay between I/O accesses, that the hardware is malfunctioning, or are reading the wrong register or field. Other problems that these techniques can address are noise on the I/O bus due to faulty hardware, dirty contacts, or even sunspots! When perform I/O operations and these checks fail, try repeating the operation, possibly with increased I/O delay time. By providing extra robustness, I have found that my own programs will work properly on hardware that causes less robust programs to fail.

### Accessing the External Registers

The external registers are the easiest to program, because they each have their own separate I/O address. Reading and writing to them is as simple as inputting and outputting bytes to their respective port address. Note, however some, such as the Miscellaneous Output Register is written at port 3C2h, but is read at port 3CCh. The reason for this is for backwards compatibility with the EGA and previous adapters. Many registers in the EGA were write only, and thus the designers placed read-only registers at the same location as write-only ones. However, the biggest complaint programmers had with the EGA was the inability to read the EGA's video state and thus in the design of the VGA most of these write-only registers were changed to read/write registers. However, for backwards compatibility, the read-only register had to remain at 3C2h, so they used a different port.

### Accessing the Sequencer, Graphics, and CRT Controller Registers

These registers are accessed in an indexed fashion. Each of the three have two unique read/write ports assigned to them. The first port is the Address Register for the group. The other is the Data Register for the group. By writing a byte to the Address Register equal to the index of the particular sub-register you wish to access, one can address the data pointed to by that index by reading and writing the Data Register. The current value of the index can be read by reading the Address Register. It is best to save this value and restore it after writing data, particularly so in an interrupt routine because the interrupted process may be in the middle of writing to the same register when the interrupt occurred. To read and write a data register in one of these register groups perform the following procedure:

1.  Input the value of the Address Register and save it for step 6
2.  Output the index of the desired Data Register to the Address Register.
3.  Read the value of the Data Register and save it for later restoration upon termination, if needed.
4.  If writing, modify the value read in step 3, making sure to mask off bits not being modified.
5.  If writing, write the new value from step 4 to the Data register.
6.  Write the value of Address register saved in step 1 to the Address Register.

If you are paranoid, then you might want to read back and compare the bytes written in step 2, 5, and 6 as in the [Paranoia](#paranoia) section above. Note that certain CRTC registers can be protected from read or write access for compatibility with programs written prior to the VGA's existence. This protection is controlled via the [Enable Vertical Retrace Access](#crtcreg-03) and [CRTC Registers Protect Enable](#crtcreg-11) fields. Ensuring that access is not prevented even if your card does not normally protect these registers makes your

### Accessing the Attribute Registers

The attribute registers are also accessed in an indexed fashion, albeit in a more confusing way. The address register is read and written via port 3C0h. The data register is written to port 3C0h and read from port 3C1h. The index and the data are written to the same port, one after another. A flip-flop inside the card keeps track of whether the next write will be handled is an index or data. Because there is no standard method of determining the state of this flip-flop, the ability to reset the flip-flop such that the next write will be handled as an index is provided. This is accomplished by reading the Input Status #1 Register (normally port 3DAh) (the data received is not important.) This can cause problems with interrupts because there is no standard way to find out what the state of the flip-flop is; therefore interrupt routines require special card when reading this register. (Especially since the Input Status #1 Register's purpose is to determine whether a horizontal or vertical retrace is in progress, something likely to be read by an interrupt routine that deals with the display.) If an interrupt were to read 3DAh in the middle of writing to an address/data pair, then the flip-flop would be reset and the data would be written to the address register instead. Any further writes would also be handled incorrectly and thus major corruption of the registers could occur. To read and write an data register in the attribute register group, perform the following procedure:

1.  Input a value from the Input Status #1 Register (normally port 3DAh) and discard it.
2.  Read the value of the Address/Data Register and save it for step 7.
3.  Output the index of the desired Data Register to the Address/Data Register
4.  Read the value of the Data Register and save it for later restoration upon termination, if needed.
5.  If writing, modify the value read in step 4, making sure to mask off bits not being modified.
6.  If writing, write the new value from step 5 to the Address/Data register.
7.  Write the value of Address register saved in step 1 to the Address/Data Register.
8.  If you wish to leave the register waiting for an index, input a value from the Input Status #1 Register (normally port 3DAh) and discard it.

If you have control over interrupts, then you can disable interrupts while in the middle of writing to the register. If not, then you may be able to implement a critical section where you use a byte in memory as a flag whether it is safe to modify the attribute registers and have your interrupt routine honor this. And again, it pays to be paranoid. Resetting the flip-flop even though it **should** be in the reset state already helps prevent catastrophic problems. Also, you might want to read back and compare the bytes written in step 3, 6, and 7 as in the [Paranoia](#paranoia) section above.  

On the IBM VGA implementation, an undocumented register (CRTC Index=24h, bit 7) can be read to determine the status of the flip-flop (0=address,1=data) and many VGA compatible chipsets duplicate this behavior, but it is not guaranteed. However, it is a simple matter to determine if this is the case. Also, some SVGA chipsets provide the ability to access the attribute registers in the same fashion as the CRT, Sequencer, and Graphics controllers. Because this functionality is vendor specific it is really only useful when programming for that particular chipset. To determine if this undocumented bit is supported, perform the following procedure:

1.  Input a value from the Input Status #1 Register (normally port 3DAh) and discard it.
2.  Verify that the flip-flop status bit (CRTC Index 24, bit 7) is 0. If bit=1 then feature is not supported, else continue to step 3.
3.  Output an address value to the Attribute Address/Data register.
4.  Verify that the flip-flop status bit (CRTC Index 24, bit 7) is 1. If bit=0 then feature is not supported, else continue to step 5.
5.  Input a value from the Input Status #1 Register (normally port 3DAh) and discard it.
6.  Verify that the flip-flop status bit (CRTC Index 24, bit 7) is 0. If bit=1 then feature is not supported, else feature is supported.

### Accessing the Color Registers

The color registers require an altogether different technique; this is because the 256-color palette requires 3 bytes to store 18-bit color values. In addition the hardware supports the capability to load all or portions of the palette rapidly. To write to the palette, first you must output the value of the palette entry to the PEL Address Write Mode Register (port 3C8h.) Then you should output the component values to the PEL Data Register (port 3C9h), in the order red, green, then blue. The PEL Address Write Mode Register will then automatically increment, allowing the component values of the palette entry to be written to the PEL Data Register. Reading is performed similarly, except that the PEL Address Read Mode Register (port 3C7h) is used to specify the palette entry to be read, and the values are read from the PEL Data Register. Again, the PEL Address Read Mode Register auto-increments after each triplet is written. The current index for the current operation can be read from the PEL Address Write Mode Register. Reading port 3C7h gives the DAC State Register, which specifies whether a read operation or a write operation is in effect. As in the attribute registers, there is guaranteed way for an interrupt routine to access the color registers and return the color registers to the state they were in prior to access without some communication between the ISR and the main program. For some workarounds see the [Accessing the Attribute Registers](#accessing-the-attribute-registers) section above. To read the color registers:

1.  Read the DAC State Register and save the value for use in step 8.
2.  Read the PEL Address Write Mode Register for use in step 8.
3.  Output the value of the first color entry to be read to the PEL Address Read Mode Register.
4.  Read the PEL Data Register to obtain the red component value.
5.  Read the PEL Data Register to obtain the green component value.
6.  Read the PEL Data Register to obtain the blue component value.
7.  If more colors are to be read, repeat steps 4-6.
8.  Based upon the DAC State from step 1, write the value saved in step 2 to either the PEL Address Write Mode Register or the PEL Address Read Mode Register.

Note: Steps 1, 2, and 8 are hopelessly optimistic. This in no way guarantees that the state is preserved, and with some DAC implementations this may actually guarantee that the state is never preserved. See the [DAC Operation](#dac-operation) page for more details.

### Binary Operations

In order to better understand dealing with bit fields it is necessary to know a little bit about logical operations such as logical-and (AND), logical-or (OR), and exclusive-or(XOR.) These operations are performed on a bit by bit basis using the truth tables below. All of these operations are commutative, i.e. A OR B = B OR A, so you look up one bit in the left column and the other in the top row and consult the intersecting row and column for the answer.  
 
|     |     |     |     |     |     |     |     |     |     |     |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| **AND** |     |     |     | **OR** |     |     |     | **XOR** |     |     |
|     | **0** | **1** |     |     | **0** | **1** |     |     | **0** | **1** |
| **0** | 0   | 0   |     | **0** | 0   | 1   |     | **0** | 0   | 1   |
| **1** | 0   | 1   |     | **1** | 1   | 1   |     | **1** | 1   | 0   |

  
### Example Register

The following table is an example of one particular register, the Mode Register of the Graphics Register. Each number from 7-0 represents the bit position in the byte. Many registers contain more than one field, each of which performs a different function. This particular chart contains four fields, two of which are two bits in length. It also contains two bits which are not implemented (to the best of my knowledge) by the standard VGA hardware.  
 
<table class="bitfield">
<tr><th colspan="8">
<b>Mode Register (Index 05h)</b>
</th></tr>
<tr>
<td>7</td><td>6</td><td>5</td><td>4</td><td>3</td><td>2</td><td>1</td><td>0</td>
</tr>
<tr>
<td></td>
<td colspan="2">Shift Register</td>
<td>Odd/Even</td>
<td>RM</td>
<td></td>
<td colspan="2">Write Mode</td>
</tr>
</table>

### Masking Bit-Fields

Your development environment may provide some assistance in dealing with bit fields. Consult your documentation for this. In addition it can be performed using the logical operators AND, OR, and XOR (for details on these operators see the [Binary Operations](#binary-operations) section above.) To change the value of the Shift Register field of the example register above, we would first mask out the bits we do not wish to change. This is accomplished by performing a logical AND of the value read from the register and a binary value in which all of the bits we wish to leave alone are set to 1, which would be 10011111b for our example. This leaves all of the bits except the Shift Register field alone and set the Shift Register field to zero. If this was our goal, then we would stop here and write the value back to the register. We then OR the value with a binary number in which the bits are shifted into position. To set this field to 10b we would OR the result of the AND with 01000000b. The resulting byte would then be written to the register. To set a bitfield to all ones the AND step is not necessary, similar to setting the bitfield to all zeros using AND. To toggle a bitfield you can XOR a value with a byte with a ones in the positions to toggle. For example XORing the value read with 01100000b would toggle the value of the Shift Register bitfield. By using these techniques you can assure that you do not cause any unwanted "side-effects" when modifying registers.

## Graphics Registers 

The Graphics Registers are accessed via a pair of registers, the Graphics Address Register and the Graphics Data Register. See the [Accessing the VGA Registers](#accessing-the-vga-registers) section for more details. The Address Register is located at port 3CEh and the Data Register is located at port 3CFh.

* Index 00h -- Set/Reset Register
* Index 01h -- Enable Set/Reset Register
* Index 02h -- Color Compare Register
* Index 03h -- Data Rotate Register
* Index 04h -- Read Map Select Register
* Index 05h -- _Graphics Mode Register_
* Index 06h -- _Miscellaneous Graphics Register_
* Index 07h -- Color Don't Care Register
* Index 08h -- Bit Mask Register

<table class="bitfield">
<tr><th colspan="8">
<b id="graphreg-00">Set/Reset Register (Index 00h)</b>
</th></tr>
<tr>
<td>7</td><td>6</td><td>5</td><td>4</td><td>3</td><td>2</td><td>1</td><td>0</td>
</tr>
<tr>
<td></td>
<td></td>
<td></td>
<td></td>
<td colspan="4">Set/Reset</td>
</tr>
</table>

* **Set/Reset**
    Bits 3-0 of this field represent planes 3-0 of the VGA display memory. This field is used by Write Mode 0 and Write Mode 3 (See the [Write Mode](#graphreg-05) field.) In Write Mode 0, if the corresponding bit in the [Enable Set/Reset](#graphreg-01) field is set, and in Write Mode 3 regardless of the [Enable Set/Reset](#graphreg-01) field, the value of the bit in this field is expanded to 8 bits and substituted for the data of the respective plane and passed to the next stage in the graphics pipeline, which for Write Mode 0 is the [Logical Operation](#graphreg-03) unit and for Write Mode 3 is the [Bit Mask](#graphreg-08) unit.

<table class="bitfield">
<tr><th colspan="8">
<b id="graphreg-01">Enable Set/Reset Register (Index 01h)</b>
</th></tr>
<tr>
<td>7</td><td>6</td><td>5</td><td>4</td><td>3</td><td>2</td><td>1</td><td>0</td>
</tr>
<tr>
<td></td>
<td></td>
<td></td>
<td></td>
<td colspan="4">Enable Set/Reset</td>
</tr>
</table>

* **Enable Set/Reset**
    Bits 3-0 of this field represent planes 3-0 of the VGA display memory. This field is used in Write Mode 0 (See the [Write Mode](#graphreg-05) field) to select whether data for each plane is derived from host data or from expansion of the respective bit in the [Set/Reset](#graphreg-00) field.

<table class="bitfield">
<tr><th colspan="8">
<b id="graphreg-02">Color Compare Register (Index 02h)</b>
</th></tr>
<tr>
<td>7</td><td>6</td><td>5</td><td>4</td><td>3</td><td>2</td><td>1</td><td>0</td>
</tr>
<tr>
<td></td>
<td></td>
<td></td>
<td></td>
<td colspan="4">Color Compare</td>
</tr>
</table>

* **Color Compare**
    Bits 3-0 of this field represent planes 3-0 of the VGA display memory. This field holds a reference color that is used by Read Mode 1 (See the [Read Mode](#graphreg-05) field.) Read Mode 1 returns the result of the comparison between this value and a location of display memory, modified by the [Color Don't Care](#graphreg-07) field.

<table class="bitfield">
<tr><th colspan="8">
<b id="graphreg-03">Data Rotate Register (Index 03h)</b>
</th></tr>
<tr>
<td>7</td><td>6</td><td>5</td><td>4</td><td>3</td><td>2</td><td>1</td><td>0</td>
</tr>
<tr>
<td></td>
<td></td>
<td></td>
<td colspan="2">Logical Operation</td>
<td colspan="3">Rotate Count</td>
</tr>
</table>

* **Logical Operation**
    This field is used in Write Mode 0 and Write Mode 2 (See the [Write Mode](#graphreg-05) field.) The logical operation stage of the graphics pipeline is 32 bits wide (1 byte * 4 planes) and performs the operations on its inputs from the previous stage in the graphics pipeline and the latch register. The latch register remains unchanged and the result is passed on to the next stage in the pipeline. The results based on the value of this field are:

   * 00b - Result is input from previous stage unmodified.
   * 01b - Result is input from previous stage logical ANDed with latch register.
    * 10b - Result is input from previous stage logical ORed with latch register.
    * 11b - Result is input from previous stage logical XORed with latch register.

* **Rotate Count**
This field is used in Write Mode 0 and Write Mode 3 (See the [Write Mode](#graphreg-05) field.) In these modes, the host data is rotated to the right by the value specified by the value of this field. A rotation operation consists of moving bits 7-1 right one position to bits 6-0, simultaneously wrapping bit 0 around to bit 7, and is repeated the number of times specified by this field.

<table class="bitfield">
<tr><th colspan="8">
<b id="graphreg-04">Read Map Select Register (Index 04h)</b>
</th></tr>
<tr>
<td>7</td><td>6</td><td>5</td><td>4</td><td>3</td><td>2</td><td>1</td><td>0</td>
</tr>
<tr>
<td></td>
<td></td>
<td></td>
<td></td>
<td></td>
<td></td>
<td colspan="2">Read Map Select</td>
</tr>
</table>

* **Read Map Select**
    This value of this field is used in Read Mode 0 (see the [Read Mode](#graphreg-05) field) to specify the display memory plane to transfer data from. Due to the arrangement of video memory, this field must be modified four times to read one or more pixels values in the planar video modes.

<table class="bitfield">
<tr><th colspan="8">
<b id="graphreg-05">Graphics Mode Register (Index 05h)</b>
</th></tr>
<tr>
<td>7</td><td>6</td><td>5</td><td>4</td><td>3</td><td>2</td><td>1</td><td>0</td>
</tr>
<tr>
<td></td>
<td>Shift256</td>
<td>Shift Reg.</td>
<td>Host O/E</td>
<td>Read Mode</td>
<td></td>
<td colspan="2">Write Mode</td>
</tr>
</table>

* **Shift256 -- 256-Color Shift Mode**
    "_When set to 0, this bit allows bit 5 to control the loading of the shift registers. When set to 1, this bit causes the shift registers to be loaded in a manner that supports the 256-color mode._"
  
* **Shift Reg. -- Shift Register Interleave Mode**
    "_When set to 1, this bit directs the shift registers in the graphics controller to format the serial data stream with even-numbered bits from both maps on even-numbered maps, and odd-numbered bits from both maps on the odd-numbered maps. This bit is used for modes 4 and 5._"  

* **Host O/E -- Host Odd/Even Memory Read Addressing Enable**
    "_When set to 1, this bit selects the odd/even addressing mode used by the IBM Color/Graphics Monitor Adapter. Normally, the value here follows the value of Memory Mode register bit 2 in the sequencer._"

* **Read Mode**
    This field selects between two read modes, simply known as Read Mode 0, and Read Mode 1, based upon the value of this field:

    * 0b -- Read Mode 0: In this mode, a byte from one of the four planes is returned on read operations. The plane from which the data is returned is determined by the value of the [Read Map Select](#graphreg-04) field.
    * 1b -- Read Mode 1: In this mode, a comparison is made between display memory and a reference color defined by the [Color Compare](#graphreg-02) field. Bit planes not set in the [Color Don't Care](#graphreg-07) field then the corresponding color plane is not considered in the comparison. Each bit in the returned result represents one comparison between the reference color, with the bit being set if the comparison is true.

* **Write Mode**
    This field selects between four write modes, simply known as Write Modes 0-3, based upon the value of this field:

    * 00b -- Write Mode 0: In this mode, the host data is first rotated as per the [Rotate Count](#graphreg-03) field, then the [Enable Set/Reset](#graphreg-01) mechanism selects data from this or the [Set/Reset](#graphreg-00) field. Then the selected [Logical Operation](#graphreg-03) is performed on the resulting data and the data in the latch register. Then the [Bit Mask](#graphreg-08) field is used to select which bits come from the resulting data and which come from the latch register. Finally, only the bit planes enabled by the [Memory Plane Write Enable](#seqreg-02) field are written to memory.
    * 01b -- Write Mode 1: In this mode, data is transferred directly from the 32 bit latch register to display memory, affected only by the [Memory Plane Write Enable](#seqreg-02) field. The host data is not used in this mode.
    * 10b -- Write Mode 2: In this mode, the bits 3-0 of the host data are replicated across all 8 bits of their respective planes. Then the selected [Logical Operation](#graphreg-03) is performed on the resulting data and the data in the latch register. Then the [Bit Mask](#graphreg-08) field is used to select which bits come from the resulting data and which come from the latch register. Finally, only the bit planes enabled by the [Memory Plane Write Enable](#seqreg-02) field are written to memory.
    * 11b -- Write Mode 3: In this mode, the data in the [Set/Reset](#graphreg-00) field is used as if the [Enable Set/Reset](#graphreg-01) field were set to 1111b. Then the host data is first rotated as per the [Rotate Count](#graphreg-03) field, then logical ANDed with the value of the [Bit Mask](#graphreg-08) field. The resulting value is used on the data obtained from the Set/Reset field in the same way that the [Bit Mask](#08) field would ordinarily be used. to select which bits come from the expansion of the [Set/Reset](#graphreg-00) field and which come from the latch register. Finally, only the bit planes enabled by the [Memory Plane Write Enable](#seqreg-02) field are written to memory.

<table class="bitfield">
<tr><th colspan="8">
<b id="graphreg-06">Miscellaneous Graphics Register (Index 06h)</b>
</th></tr>
<tr>
<td>7</td><td>6</td><td>5</td><td>4</td><td>3</td><td>2</td><td>1</td><td>0</td>
</tr>
<tr>
<td></td>
<td></td>
<td></td>
<td></td>
<td colspan="2">Memory Map Select</td>
<td>Chain O/E</td>
<td>Alpha Dis.</td>
</tr>
</table>

* **Memory Map Select**
    This field specifies the range of host memory addresses that is decoded by the VGA hardware and mapped into display memory accesses.  The values of this field and their corresponding host memory ranges are:

    * 00b -- A0000h-BFFFFh (128K region)
    * 01b -- A0000h-AFFFFh (64K region)
    * 10b -- B0000h-B7FFFh (32K region)
    * 11b -- B8000h-BFFFFh (32K region)

* **Chain O/E -- Chain Odd/Even Enable**
    "_When set to 1, this bit directs the system address bit, A0, to be replaced by a higher-order bit. The odd map is then selected when A0 is 1, and the even map when A0 is 0._"  

* **Alpha Dis. -- Alphanumeric Mode Disable**
    "_This bit controls alphanumeric mode addressing. When set to 1, this bit selects graphics modes, which also disables the character generator latches."_

<table class="bitfield">
<tr><th colspan="8">
<b id="graphreg-07">Color Don't Care Register (Index 07h)</b>
</th></tr>
<tr>
<td>7</td><td>6</td><td>5</td><td>4</td><td>3</td><td>2</td><td>1</td><td>0</td>
</tr>
<tr>
<td></td>
<td></td>
<td></td>
<td></td>
<td colspan="4">Color Don't Care</td>
</tr>
</table>

* **Color Don't Care**
    Bits 3-0 of this field represent planes 3-0 of the VGA display memory. This field selects the planes that are used in the comparisons made by Read Mode 1 (See the [Read Mode](#graphreg-05) field.) Read Mode 1 returns the result of the comparison between the value of the [Color Compare](#graphreg-02) field and a location of display memory. If a bit in this field is set, then the corresponding display plane is considered in the comparison. If it is not set, then that plane is ignored for the results of the comparison.

<table class="bitfield">
<tr><th colspan="8">
<b id="graphreg-08">Bit Mask Register (Index 08h)</b>
</th></tr>
<tr>
<td>7</td><td>6</td><td>5</td><td>4</td><td>3</td><td>2</td><td>1</td><td>0</td>
</tr>
<tr>
<td colspan="8">Bit Mask</td>
</tr>
</table>

* **Bit Mask**
    This field is used in Write Modes 0, 2, and 3 (See the [Write Mode](#graphreg-05) field.) It it is applied to one byte of data in all four display planes. If a bit is set, then the value of corresponding bit from the previous stage in the graphics pipeline is selected; otherwise the value of the corresponding bit in the latch register is used instead. In Write Mode 3, the incoming data byte, after being rotated is logical ANDed with this byte and the resulting value is used in the same way this field would normally be used by itself.

## Sequencer Registers 

The Sequencer Registers are accessed via a pair of registers, the Sequencer Address Register and the Sequencer Data Register. See the [Accessing the VGA Registers](#accessing-the-vga-registers) section for more detals. The Address Register is located at port 3C4h and the Data Register is located at port 3C5h.

* Index 00h -- _Reset Register_
* Index 01h -- _Clocking Mode Register_
* Index 02h -- _Map Mask Register_
* Index 03h -- Character Map Select Register
* Index 04h -- _Sequencer Memory Mode Register_

<table class="bitfield">
<tr><th colspan="8">
<b id="seqreg-00">Reset Register (Index 00h)</b>
</th></tr>
<tr>
<td>7</td><td>6</td><td>5</td><td>4</td><td>3</td><td>2</td><td>1</td><td>0</td>
</tr>
<tr>
<td></td>
<td></td>
<td></td>
<td></td>
<td></td>
<td></td>
<td>SR</td>
<td>AR</td>
</tr>
</table>
 
* **SR -- Sychnronous Reset**  
    "_When set to 0, this bit commands the sequencer to synchronously clear and halt. Bits 1 and 0 must be 1 to allow the sequencer to operate. To prevent the loss of data, bit 1 must be set to 0 during the active display interval before changing the clock selection. The clock is changed through the Clocking Mode register or the Miscellaneous Output register._"  
* **AR -- Asynchronous Reset**  
    "_When set to 0, this bit commands the sequencer to asynchronously clear and halt. Resetting the sequencer with this bit can cause loss of video data_"

<table class="bitfield">
<tr><th colspan="8">
<b id="seqreg-01">Clocking Mode Register (Index 01h)</b>
</th></tr>
<tr>
<td>7</td><td>6</td><td>5</td><td>4</td><td>3</td><td>2</td><td>1</td><td>0</td>
</tr>
<tr>
<td></td>
<td></td>
<td>SD</td>
<td>S4</td>
<td>DCR</td>
<td>SLR</td>
<td></td>
<td>9/8DM</td>
</tr>
</table>
 
* **SD -- Screen Disable**  
    "_When set to 1, this bit turns off the display and assigns maximum memory bandwidth to the system. Although the display is blanked, the synchronization pulses are maintained. This bit can be used for rapid full-screen updates._"  
* **S4 -- Shift Four Enable**  
    "_When the Shift 4 field and the Shift Load Field are set to 0, the video serializers are loaded every character clock. When the Shift 4 field is set to 1, the video serializers are loaded every forth character clock, which is useful when 32 bits are fetched per cycle and chained together in the shift registers._"  
* **DCR -- Dot Clock Rate**  
    "_When set to 0, this bit selects the normal dot clocks derived from the sequencer master clock input. When this bit is set to 1, the master clock will be divided by 2 to generate the dot clock. All other timings are affected because they are derived from the dot clock. The dot clock divided by 2 is used for 320 and 360 horizontal PEL modes._"  
* **SLR -- Shift/Load Rate**  
    "_When this bit and bit 4 are set to 0, the video serializers are loaded every character clock. When this bit is set to 1, the video serializers are loaded every other character clock, which is useful when 16 bits are fetched per cycle and chained together in the shift registers. The Type 2 video behaves as if this bit is set to 0; therefore, programs should set it to 0._"
* **9/8DM -- 9/8 Dot Mode**
    This field is used to select whether a character is 8 or 9 dots wide. This can be used to select between 720 and 640 pixel modes (or 360 and 320) and also is used to provide 9 bit wide character fonts in text mode. The possible values for this field are:
    * 0 - Selects 9 dots per character.
    * 1 - Selects 8 dots per character.

<table class="bitfield">
<tr><th colspan="8">
<b id="seqreg-02">Map Mask Register (Index 02h)</b>
</th></tr>
<tr>
<td>7</td><td>6</td><td>5</td><td>4</td><td>3</td><td>2</td><td>1</td><td>0</td>
</tr>
<tr>
<td></td>
<td></td>
<td></td>
<td></td>
<td colspan="4">Memory Plane Write Enable</td>
</tr>
</table>

* **Memory Plane Write Enable**
    Bits 3-0 of this field correspond to planes 3-0 of the VGA display memory. If a bit is set, then write operations will modify the respective plane of display memory. If a bit is not set then write operations will not affect the respective plane of display memory.

<table class="bitfield">
<tr><th colspan="8">
<b id="seqreg-03">Character Map Select Register (Index 03h)</b>
</th></tr>
<tr>
<td>7</td><td>6</td><td>5</td><td>4</td><td>3</td><td>2</td><td>1</td><td>0</td>
</tr>
<tr>
<td></td>
<td></td>
<td>CSAS2</td>
<td>CSBS2</td>
<td colspan="2">Character Set A Select</td>
<td colspan="2">Character Set B Select</td>
</tr>
</table>

* **CSAS2 -- Bit 2 of Character Set A Select**
    This is bit 2 of the Character Set A Select field. See [Character Set A Select](#seqreg-03) below.
* **CSBS2 -- Bit 2 of Character Set B Select**
    This is bit 2 of the Character Set B field. See [Character Set B Select](#seqreg-03) below.
* **Character Set A Select**
    This field is used to select the font that is used in text mode when bit 3 of the attribute byte for a character is set to 1. Note that this field is not contiguous in order to provide EGA compatibility. The font selected resides in plane 2 of display memory at the address specified by this field, as follows:
    * 000b -- Select font residing at 0000h - 1FFFh
    * 001b -- Select font residing at 4000h - 5FFFh
    * 010b -- Select font residing at 8000h - 9FFFh
    * 011b -- Select font residing at C000h - DFFFh
    * 100b -- Select font residing at 2000h - 3FFFh
    * 101b -- Select font residing at 6000h - 7FFFh
    * 110b -- Select font residing at A000h - BFFFh
    * 111b -- Select font residing at E000h - FFFFh
* **Character Set B Select**
    This field is used to select the font that is used in text mode when bit 3 of the attribute byte for a character is set to 0. Note that this field is not contiguous in order to provide EGA compatibility. The font selected resides in plane 2 of display memory at the address specified by this field, identical to the mapping used by [Character Set A Select](#seqreg-03) above.

<table class="bitfield">
<tr><th colspan="8">
<b id="seqreg-04">Sequencer Memory Mode Register (Index 04h)</b>
</th></tr>
<tr>
<td>7</td><td>6</td><td>5</td><td>4</td><td>3</td><td>2</td><td>1</td><td>0</td>
</tr>
<tr>
<td></td>
<td></td>
<td></td>
<td></td>
<td>Chain 4</td>
<td>O/E Dis.</td>
<td>Ext. Mem</td>
<td></td>
</tr>
</table>
 
* **Chain 4 -- Chain 4 Enable**  
    "_This bit controls the map selected during system read operations. When set to 0, this bit enables system addresses to sequentially access data within a bit map by using the Map Mask register. When setto 1, this bit causes the two low-order bits to select the map accessed as shown below._  
    <table>
    <tr><td colspan="2"><i>Address Bits</i></td></tr>
    <tr><td><i>A0</i></td><td><i>A1</i></td><td><i>Map Selected</i></td></tr>
    <tr><td><i>0</i></td><td><i>0</i></td><td><i>0</i></td></tr>
    <tr><td><i>0</i></td><td><i>1</i></td><td><i>1</i></td></tr>
    <tr><td><i>1</i></td><td><i>0</i></td><td><i>2</i></td></tr>
    <tr><td><i>1</i></td><td><i>1</i></td><td><i>3</i></td></tr>
    </table>
* **O/E Dis. -- Odd/Even Host Memory Write Adressing Disable**"
    _When this bit is set to 0, even system addresses access maps 0 and 2, while odd system addresses access maps 1 and 3. When this bit is set to 1, system addresses sequentially access data within a bit map, and the maps are accessed according to the value in the Map Mask register (index 0x02)._"
* **Ext. Mem -- Extended Memory**
    "_When set to 1, this bit enables the video memory from 64KB to 256KB. This bit must be set to 1 to enable the character map selection described for the previous register._"

## Attribute Controller Registers 

The Attribute Controller Registers are accessed via a pair of registers, the Attribute Address/Data Register and the Attribute Data Read Register. See the [Accessing the VGA Registers](#accessing-the-vga-registers) section for more detals. The Address/Data Register is located at port 3C0h and the Data Read Register is located at port 3C1h.  
 
<table class="bitfield">
<tr><th colspan="8">
<b id="3C0">Attribute Address Register (3C0h)</b>
</th></tr>
<tr>
<td>7</td><td>6</td><td>5</td><td>4</td><td>3</td><td>2</td><td>1</td><td>0</td>
</tr>
<tr>
<td></td>
<td></td>
<td>PAS</td>
<td colspan="5">Attribute Address</td>
</tr>
</table>

* **PAS -- Palette Address Source **
    "_This bit is set to 0 to load color values to the registers in the internal palette. It is set to 1 for normal operation of the attribute controller. Note: Do not access the internal palette while this bit is set to 1. While this bit is 1, the Type 1 video subsystem disables accesses to the palette; however, the Type 2 does not, and the actual color value addressed cannot be ensured._"
* **Attribute Address**
    This field specifies the index value of the attribute register to be read or written.

<table class="bitfield">
<tr><th colspan="8">
<b id="attrreg-00-0F">Palette Registers (Index 00-0Fh)</b>
</th></tr>
<tr>
<td>7</td><td>6</td><td>5</td><td>4</td><td>3</td><td>2</td><td>1</td><td>0</td>
</tr>
<tr>
<td></td>
<td></td>
<td colspan="6">Internal Palette Index</td>
</tr>
</table>

* **Internal Palette Index**
    "_These 6-bit registers allow a dynamic mapping between the text attribute or graphic color input value and the display color on the CRT screen. When set to 1, this bit selects the appropriate color. The Internal Palette registers should be modified only during the vertical retrace interval to avoid problems with the displayed image. These internal palette values are sent off-chip to the video DAC, where they serve as addresses into the DAC registers._"

<table class="bitfield">
<tr><th colspan="8">
<b id="attrreg-10">Attribute Mode Control Register (Index 10h)</b>
</th></tr>
<tr>
<td>7</td><td>6</td><td>5</td><td>4</td><td>3</td><td>2</td><td>1</td><td>0</td>
</tr>
<tr>
<td>P54S</td>
<td>8BIT</td>
<td>PPM</td>
<td></td>
<td>BLINK</td>
<td>LGE</td>
<td>MONO</td>
<td>ATGE</td>
</tr>
</table>

* **P54S -- Palette Bits 5-4 Select**
    "_This bit selects the source for the P5 and P4 video bits that act as inputs to the video DAC. When this bit is set to 0, P5 and P4 are the outputs of the Internal Palette registers. When this bit is set to 1, P5 and P4 are bits 1 and 0 of the Color Select register._"  
* **8BIT -- 8-bit Color Enable**
    "_When this bit is set to 1, the video data is sampled so that eight bits are available to select a color in the 256-color mode (0x13). This bit is set to 0 in all other modes._"
* **PPM -- Pixel Panning Mode**
    This field allows the upper half of the screen to pan independently of the lower screen. If this field is set to 0 then nothing special occurs during a successful line compare (see the [Line Compare](#crtcreg-18) field.) If this field is set to 1, then upon a successful line compare, the bottom portion of the screen is displayed as if the [Pixel Shift Count](#attrreg-13) and [Byte Panning](#crtcreg-08) fields are set to 0.  
* **BLINK - Blink Enable**
    "_When this bit is set to 0, the most-significant bit of the attribute selects the background intensity (allows 16 colors for background). When set to 1, this bit enables blinking._"
* **LGA - Line Graphics Enable**
    This field is used in 9 bit wide character modes to provide continuity for the horizontal line characters in the range C0h-DFh. If this field is set to 0, then the 9th column of these characters is replicated from the 8th column of the character. Otherwise, if it is set to 1 then the 9th column is set to the background like the rest of the characters.
* **MONO - Monochrome Emulation**
    This field is used to store your favorite bit. According to IBM, "When this bit is set to 1, monochrome emulation mode is selected. When this bit is set to 0, color |emulation mode is selected." It is present and programmable in all of the hardware but it apparently does nothing. The internal palette is used to provide monochrome emulation instead.
* **ATGE - Attribute Controller Graphics Enable**
    "_When set to 1, this bit selects the graphics mode of operation._"

<table class="bitfield">
<tr><th colspan="8">
<b id="attrreg-11">Overscan Color Register (Index 11h)</b>
</th></tr>
<tr>
<td>7</td><td>6</td><td>5</td><td>4</td><td>3</td><td>2</td><td>1</td><td>0</td>
</tr>
<tr>
<td colspan="8">Overscan Palette Index</td>
</tr>
</table>

* **Overscan Palette Index**
    "_These bits select the border color used in the 80-column alphanumeric modes and in the graphics modes other than modes 4, 5, and D. (Selects a color from one of the DAC registers.)_"

<table class="bitfield">
<tr><th colspan="8">
<b id="attrreg-12">Color Plane Enable Register (Index 12h)</b>
</th></tr>
<tr>
<td>7</td><td>6</td><td>5</td><td>4</td><td>3</td><td>2</td><td>1</td><td>0</td>
</tr>
<tr>
<td></td>
<td></td>
<td></td>
<td></td>
<td colspan="4">Color Plane Enable</td>
</tr>
</table>
 
* **Color Plane Enable**
    "_Setting a bit to 1, enables the corresponding display-memory color plane._"

<table class="bitfield">
<tr><th colspan="8">
<b id="attrreg-13">Horizontal Pixel Panning Register (Index 13h)</b>
</th></tr>
<tr>
<td>7</td><td>6</td><td>5</td><td>4</td><td>3</td><td>2</td><td>1</td><td>0</td>
</tr>
<tr>
<td></td>
<td></td>
<td></td>
<td></td>
<td colspan="4">Pixel Shift Count</td>
</tr>
</table>

* **Pixel Shift Count**
    "_These bits select the number of pels that the video data is shifted to the left. PEL panning is available in both alphanumeric and graphics modes._"

<table class="bitfield">
<tr><th colspan="8">
<b id="attrreg-14">Color Select Register (Index 14h)</b>
</th></tr>
<tr>
<td>7</td><td>6</td><td>5</td><td>4</td><td>3</td><td>2</td><td>1</td><td>0</td>
</tr>
<tr>
<td></td>
<td></td>
<td></td>
<td></td>
<td colspan="2">Color Select 7-6</td>
<td colspan="2">Color Select 5-4</td>
</tr>
</table>

* **Color Select 7-6**
    "_In modes other than mode 13 hex, these are the two most-significant bits of the 8-bit digital color value to the video DAC. In mode 13 hex, the 8-bit attribute is the digital color value to the video DAC. These bits are used to rapidly switch between sets of colors in the video DAC._"  
* **Color Select 5-4**
    "_These bits can be used in place of the P4 and P5 bits from the Internal Palette registers to form the  8-bit digital color value to the video DAC. Selecting these bits is done in the Attribute Mode Control  register (index 0x10). These bits are used to rapidly switch between colors sets within the video DAC._"

## CRT Controller Registers 

The CRT Controller (CRTC) Registers are accessed via a pair of registers, the CRTC Address Register and the CRTC Data Register. See the [Accessing the VGA Registers](#accessing-the-vga-registers) section for more details. The Address Register is located at port 3x4h and the Data Register is located at port 3x5h.  The value of the x in 3x4h and 3x5h is dependent on the state of the [Input/Output Address Select](#3CCR3C2W) field, which allows these registers to be mapped at 3B4h-3B5h or 3D4h-3D5h.   Note that when the [CRTC Registers Protect Enable](#crtcreg-11) field is set to 1, writing to register indexes 00h-07h is prevented, with the exception of the [Line Compare](#crtcreg-07) field of the [Overflow Register](#crtcreg-07).

* Index 00h -- [Horizontal Total Register](#crtcreg-00)
* Index 01h -- [End Horizontal Display Register](#crtcreg-01)
* Index 02h -- [Start Horizontal Blanking Register](#crtcreg-02)
* Index 03h -- [End Horizontal Blanking Register](#crtcreg-03)
* Index 04h -- [Start Horizontal Retrace Register](#crtcreg-04)
* Index 05h -- [End Horizontal Retrace Register](#crtcreg-05)
* Index 06h -- [Vertical Total Register](#crtcreg-06)
* Index 07h -- [Overflow Register](#crtcreg-07)
* Index 08h -- [Preset Row Scan Register](#crtcreg-08)
* Index 09h -- _[Maximum Scan Line Register](#crtcreg-09)_
* Index 0Ah -- [Cursor Start Register](#crtcreg-0A)
* Index 0Bh -- [Cursor End Register](#crtcreg-0B)
* Index 0Ch -- [Start Address High Register](#crtcreg-0C)
* Index 0Dh -- [Start Address Low Register](#crtcreg-0D)
* Index 0Eh -- [Cursor Location High Register](#crtcreg-0E)
* Index 0Fh -- [Cursor Location Low Register](#crtcreg-0F)
* Index 10h -- [Vertical Retrace Start Register](#crtcreg-10)
* Index 11h -- [Vertical Retrace End Register](#crtcreg-11)
* Index 12h -- [Vertical Display End Register](#crtcreg-12)
* Index 13h -- [Offset Register](#crtcreg-13)
* Index 14h -- _[Underline Location Register](#crtcreg-14)_
* Index 15h -- [Start Vertical Blanking Register](#crtcreg-15)
* Index 16h -- [End Vertical Blanking](#crtcreg-16)
* Index 17h -- _[CRTC Mode Control Register](#crtcreg-17)_
* Index 18h -- [Line Compare Register](#crtcreg-18)

<table class="bitfield">
<tr><th colspan="8">
<b id="crtcreg-00">Horizontal Total Register (Index 00h)</b>
</th></tr>
<tr>
<td>7</td><td>6</td><td>5</td><td>4</td><td>3</td><td>2</td><td>1</td><td>0</td>
</tr>
<tr>
<td colspan="8">Horizontal Total</td>
</tr>
</table>

* **Horizontal Total**
    This field is used to specify the number of character clocks per scan line.  This field, along with the dot rate selected, controls the horizontal refresh rate of the VGA by specifying the amount of time one scan line takes.  This field is not programmed with the actual number of character clocks, however.  Due to timing factors of the VGA hardware (which, for compatibility purposes has been emulated by VGA compatible  chipsets), the actual horizontal total is 5 character clocks more than the value stored in this field, thus one needs to subtract 5 from the actual horizontal total value desired before programming it into this register.

<table class="bitfield">
<tr><th colspan="8">
<b id="crtcreg-01">End Horizontal Display Register (Index 01h)</b>
</th></tr>
<tr>
<td>7</td><td>6</td><td>5</td><td>4</td><td>3</td><td>2</td><td>1</td><td>0</td>
</tr>
<tr>
<td colspan="8">End Horizontal Display</td>
</tr>
</table>

* **End Horizontal Display**
    This field is used to control the point that the sequencer stops outputting pixel values from display memory, and sequences the pixel value specified by the [Overscan Palette Index](#attrreg-11) field for the remainder of the scan line.  The overscan begins the character clock after the the value programmed into this field.  This register should be programmed with the number of character clocks in the active display - 1.  Note that the active display may be affected by the [Display Enable Skew](#crtcreg-03) field.  
 
<table class="bitfield">
<tr><th colspan="8">
<b id="crtcreg-02">Start Horizontal Blanking Register (Index 02h)</b>
</th></tr>
<tr>
<td>7</td><td>6</td><td>5</td><td>4</td><td>3</td><td>2</td><td>1</td><td>0</td>
</tr>
<tr>
<td colspan="8">Start Horizontal Blanking</td>
</tr>
</table>

* **Start Horizontal Blanking**
    This field is used to specify the character clock at which the horizontal blanking period begins.  During the horizontal blanking period, the VGA hardware forces the DAC into a blanking state, where all of the intensities output are at minimum value, no matter what color information the attribute controller is sending to the DAC.  This field works in conjunction with the [End Horizontal Blanking](#crtcreg-03) field to specify the horizontal blanking period.  Note that the horizontal blanking can be programmed to appear anywhere within the scan line, as well as being programmed to a value greater than the [Horizontal Total](#crtcreg-00) field preventing the horizontal blanking from occurring at all.

<table class="bitfield">
<tr><th colspan="8">
<b id="crtcreg-03">End Horizontal Blanking Register (Index 03h)</b>
</th></tr>
<tr>
<td>7</td><td>6</td><td>5</td><td>4</td><td>3</td><td>2</td><td>1</td><td>0</td>
</tr>
<tr>
<td>EVRA</td>
<td colspan="2">Display Enable Skew</td>
<td colspan="5">End Horizontal Blanking</td>
</tr>
</table>

* **EVRA -- Enable Vertical Retrace Access**
    This field was used in the IBM EGA to provide access to the light pen input values as the light pen registers were mapped over CRTC indexes 10h-11h.  The VGA lacks capability for light pen input, thus this field is normally forced to 1 (although always writing it as 1 might be a good idea for compatibility) , which in the EGA would enable access to the vertical retrace fields instead of the light pen fields.
* **Display Enable Skew**
    This field affects the timings of the display enable circuitry in the VGA. The value of this field is the number of character clocks that the display enable "signal" is delayed.  In all the VGA/SVGA chipsets I've tested, including a PS/2 VGA this field is always programmed to 0.  Programming it to non-zero values results in the overscan being displayed over the number of characters programmed into this field at the beginning of the scan line, as well as the end of the active display being shifted the number of characters programmed into this field.  The characters that extend past the normal end of the active display can be garbled in certain circumstances that is dependent on the particular VGA implementation.  According to documentation from IBM, "_This skew control is needed to provide sufficient time for the CRT controller to read a character and attribute code from the video buffer, to gain access to the character generator, and go through the Horizontal PEL Panning register in the attribute controller. Each access requires the 'display enable' signal to be skewed one character clock so that the video output is synchronized with the horizontal and vertical retrace signals._" as well as "_Note: Character skew is not adjustable on the Type 2 video and the bits are ignored; however, programs should set these bits for the appropriate skew to maintain compatibility._"  This may be required for some early IBM VGA implementations or may be simply an unused "feature" carried over along with its register description from the IBM EGA implementations that require the use of this field.
* **End Horizontal Blanking**
    This contains bits 4-0 of the End Horizontal Blanking field which specifies the end of the horizontal blanking period.  Bit 5 is located in the [End Horizontal Retrace](#crtcreg-05) register.
    After the period has begun as specified by the [Start Horizontal Blanking](#crtcreg-02) field, the 6-bit value of this field is compared against the lower 6 bits of the character clock.  When a match occurs, the horizontal blanking signal is disabled.  This provides from 1 to 64 character clocks although some implementations may match in the character clock specified by the [Start Horizontal Blanking](#crtcreg-02) field, in which case the range is 0 to 63.  Note that if blanking extends past the end of the scan line, it will end on the first match of this field on the next scan line.

<table class="bitfield">
<tr><th colspan="8">
<b id="crtcreg-04">Start Horizontal Retrace Register (Index 04h)</b>
</th></tr>
<tr>
<td>7</td><td>6</td><td>5</td><td>4</td><td>3</td><td>2</td><td>1</td><td>0</td>
</tr>
<tr>
<td colspan="8">Start Horizontal Retrace</td>
</tr>
</table>

* **Start Horizontal Retrace**
    This field specifies the character clock at which the VGA begins sending the horizontal synchronization pulse to the display which signals the monitor to retrace back to the left side of the screen.  The end of this pulse is controlled by the [End Horizontal Retrace](#crtcreg-05) field.  This pulse may appear anywhere in the scan line, as well as set to a position beyond the [Horizontal Total](#crtcreg-00) field which effectively disables the horizontal synchronization pulse.

<table class="bitfield">
<tr><th colspan="8">
<b id="crtcreg-05">End Horizontal Retrace Register (Index 05h)</b>
</th></tr>
<tr>
<td>7</td><td>6</td><td>5</td><td>4</td><td>3</td><td>2</td><td>1</td><td>0</td>
</tr>
<tr>
<td>EHB5</td>
<td colspan="2">Horiz. Retrace Skew</td>
<td colspan="5">End Horizontal Retrace</td>
</tr>
</table>

* **EHB5 -- End Horizontal Blanking (bit 5)**
    This contains bit 5 of the End Horizontal Blanking field.  See the [End Horizontal Blanking Register](#crtcreg-03) for details.
* **Horiz. Retrace Skew -- Horizontal Retrace Skew**
    This field delays the start of the horizontal retrace period by the number of character clocks equal to the value of this field.  From observation, this field is programmed to 0, with the exception of the 40 column text modes where this field is set to 1.  The VGA hardware simply acts as if this value is added to the [Start Horizontal Retrace](#crtcreg-04) field.  According to IBM documentation, "_For certain modes, the 'horizontal retrace' signal takes up the entire blanking interval. Some internal timings are generated by the falling edge of the 'horizontal retrace' signal. To ensure that the signals are latched properly, the 'retrace' signal is started before the end of the 'display enable' signal and then skewed several character clock times to provide the proper screen centering._"  This does not appear to be the case, leading me to believe this is yet another holdout from the IBM EGA implementations that do require the use of this field.
* **End Horizontal Retrace**
    This field specifies the end of the horizontal retrace period, which begins at the character clock specified in the [Start Horizontal Retrace](#crtcreg-04) field.  The horizontal retrace signal is enabled until the lower 5 bits of the character counter match the 5 bits of this field.  This provides for a horizontal retrace period from 1 to 32 character clocks.  Note that some implementations may match immediately instead of 32 clocks away, making the effective range 0 to 31 character clocks.

<table class="bitfield">
<tr><th colspan="8">
<b id="crtcreg-06">Vertical Total Register (Index 06h)</b>
</th></tr>
<tr>
<td>7</td><td>6</td><td>5</td><td>4</td><td>3</td><td>2</td><td>1</td><td>0</td>
</tr>
<tr>
<td colspan="8">Vertical Total</td>
</tr>
</table>

* **Vertical Total**  
    This contains the lower 8 bits of the Vertical Total field.  Bits 9-8 of this field are located in the [Overflow Register](#crtcreg-07). This field determines the number of scanlines in the active display and thus the length of each vertical retrace.  This field contains the value of the scanline counter at the beginning of the last scanline in the vertical period.

 <table class="bitfield">
<tr><th colspan="8">
<b id="crtcreg-07">Overflow Register (Index 07h)</b>
</th></tr>
<tr>
<td>7</td><td>6</td><td>5</td><td>4</td><td>3</td><td>2</td><td>1</td><td>0</td>
</tr>
<tr>
<td>VRS9</td>
<td>VDE9</td>
<td>VT9</td>
<td>LC8</td>
<td>SVB8</td>
<td>VRS8</td>
<td>VDE8</td>
<td>VT8</td>
</tr>
</table>

* **VRS9 -- Vertical Retrace Start (bit 9)**
    Specifies bit 9 of the Vertical Retrace Start field.  See the [Vertical Retrace Start Register](#crtcreg-10) for details.
* **VDE9 -- Vertical Display End (bit9)**
    Specifies bit 9 of the Vertical Display End field.  See the [Vertical Display End Register](#crtcreg-12) for details.
* **VT9 -- Vertical Total (bit 9)**
    Specifies bit 9 of the Vertical Total field.  See the [Vertical Total Register](#crtcreg-06) for details.
* **LC8 -- Line Compare (bit 8)**
    Specifies bit 8 of the Line Compare field. See the [Line Compare Register](#crtcrg-18) for details.
* **SVB8 -- Start Vertical Blanking (bit 8)**
    Specifies bit 8 of the Start Vertical Blanking field.  See the [Start Vertical Blanking Register](#crtcreg-15) for details.
* **VRS8 -- Vertical Retrace Start (bit 8)**
    Specifies bit 8 of the Vertical Retrace Start field.  See the [Vertical Retrace Start Register](#crtcreg-10) for details.
* **VDE8 -- Vertical Display End (bit 8)**
    Specifies bit 8 of the Vertical Display End field.  See the [Vertical Display End Register](#crtcreg-12) for details.
* **VT8 -- Vertical Total (bit 8)**
    Specifies bit 8 of the Vertical Total field.  See the [Vertical Total Register](#crtcreg-06) for details.

 <table class="bitfield">
<tr><th colspan="8">
<b id="crtcreg-08">Preset Row Scan Register (Index 08h)</b>
</th></tr>
<tr>
<td>7</td><td>6</td><td>5</td><td>4</td><td>3</td><td>2</td><td>1</td><td>0</td>
</tr>
<tr>
<td></td>
<td colspan="2">Byte Panning</td>
<td colspan="5">Preset Row Scan</td>
</tr>
</table>

* **Byte Panning**
    The value of this field is added to the [Start Address Register](#crtcreg-0D) when calculating the display memory address for the upper left hand pixel or character of the screen. This allows for a maximum shift of 15, 31, or 35 pixels without having to reprogram the [Start Address Register](#crtcreg-0D).
* **Preset Row Scan**
    This field is used when using text mode or any mode with a non-zero [Maximum Scan Line](#crtcreg-09) field to provide for more precise vertical scrolling than the [Start Address Register](#crtcreg-0D) provides. The value of this field specifies how many scan lines to scroll the display upwards. Valid values range from 0 to the value of the [Maximum Scan Line](#crtcreg-09) field. Invalid values may cause undesired effects and seem to be dependent upon the particular VGA implementation.

 <table class="bitfield">
<tr><th colspan="8">
<b id="crtcreg-09">Maximum Scan Line Register (Index 09h)</b>
</th></tr>
<tr>
<td>7</td><td>6</td><td>5</td><td>4</td><td>3</td><td>2</td><td>1</td><td>0</td>
</tr>
<tr>
<td>SD</td>
<td>LC9</td>
<td>SVB9</td>
<td colspan="5">Maximum Scan Line</td>
</tr>
</table>

* **SD -- Scan Doubling**
    "_When this bit is set to 1, 200-scan-line video data is converted to 400-scan-line output. To do this, the clock in the row scan counter is divided by 2, which allows the 200-line modes to be displayed as 400 lines on the display (this is called double scanning; each line is displayed twice). When this bit is set to 0, the clock to the row scan counter is equal to the horizontal scan rate._"
* **LC9 -- Line Compare (bit 9)**
    Specifies bit 9 of the Line Compare field. See the [Line Compare Register](#crtcreg-18) for details.
* **SVB9 -- Start Vertical Blanking (bit 9)**
    Specifies bit 9 of the Start Vertical Blanking field.  See the [Start Vertical Blanking Register](#crtcreg-15) for details.
* **Maximum Scan Line**
    In text modes, this field is programmed with the character height - 1 (scan line numbers are zero based.) In graphics modes, a non-zero value in this field will cause each scan line to be repeated by the value of this field + 1.

 <table class="bitfield">
<tr><th colspan="8">
<b id="crtcreg-0A">Cursor Start Register (Index 0Ah)</b>
</th></tr>
<tr>
<td>7</td><td>6</td><td>5</td><td>4</td><td>3</td><td>2</td><td>1</td><td>0</td>
</tr>
<tr>
<td></td>
<td></td>
<td>CD</td>
<td colspan="5">Cursor Scan Line Start</td>
</tr>
</table>

* **CD -- Cursor Disable**
    This field controls whether or not the text-mode cursor is displayed. Values are:
    * 0 -- Cursor Enabled
    * 1 -- Cursor Disabled
* **Cursor Scan Line Start**
    This field controls the appearance of the text-mode cursor by specifying the scan line location within a character cell at which the cursor should begin, with the top-most scan line in a character cell being 0 and the bottom being with the value of the [Maximum Scan Line](#crtcreg-09) field.

 <table class="bitfield">
<tr><th colspan="8">
<b id="crtcreg-0B">Cursor End Register (Index 0Bh)</b>
</th></tr>
<tr>
<td>7</td><td>6</td><td>5</td><td>4</td><td>3</td><td>2</td><td>1</td><td>0</td>
</tr>
<tr>
<td></td>
<td colspan="2">Cursor Skew</td>
<td colspan="5">Cursor Scan Line End</td>
</tr>
</table>

* **CSK -- Cursor Skew**
    This field was necessary in the EGA to synchronize the cursor with internal timing. In the VGA it basically is added to the cursor location. In some cases when this value is non-zero and the cursor is near the left or right edge of the screen, the cursor will not appear at all, or a second cursor above and to the left of the actual one may appear. This behavior may not be the same on all VGA compatible adapter cards.
* **Cursor Scan Line End**
    This field controls the appearance of the text-mode cursor by specifying the scan line location within a character cell at which the cursor should end, with the top-most scan line in a character cell being 0 and the bottom being with the value of the [Maximum Scan Line](#crtcreg-09) field. If this field is less than the [Cursor Scan Line Start](#crtcreg-0A) field, the cursor is not drawn. Some graphics adapters, such as the IBM EGA display a split-block cursor instead.

 <table class="bitfield">
<tr><th colspan="8">
<b id="crtcreg-0C">Start Address High Register (Index 0Ch)</b>
</th></tr>
<tr>
<td>7</td><td>6</td><td>5</td><td>4</td><td>3</td><td>2</td><td>1</td><td>0</td>
</tr>
<tr>
<td colspan="8">Start Address High</td>
</tr>
</table>

* **Start Address High**
    This contains specifies bits 15-8 of the Start Address field. See the [Start Address Low Register](#crtcreg-0D) for details.

 <table class="bitfield">
<tr><th colspan="8">
<b id="crtcreg-0D">Start Address Low Register (Index 0Dh)</b>
</th></tr>
<tr>
<td>7</td><td>6</td><td>5</td><td>4</td><td>3</td><td>2</td><td>1</td><td>0</td>
</tr>
<tr>
<td colspan="8">Start Address Low</td>
</tr>
</table>

* **Start Address Low**
    This contains the bits 7-0 of the Start Address field. The upper 8 bits are specified by the [Start Address High Register](#crtcreg-0C). The Start Address field specifies the display memory address of the upper left pixel or character of the screen. Because the standard VGA has a maximum of 256K of memory, and memory is accessed 32 bits at a time, this 16-bit field is sufficient to allow the screen to start at any memory address. Normally this field is programmed to 0h, except when using virtual resolutions, paging, and/or split-screen operation. Note that the VGA display will wrap around in display memory if the starting address is too high. (This may or may not be desirable, depending on your intentions.)

<table class="bitfield">
<tr><th colspan="8">
<b id="crtcreg-0E">Cursor Location High Register (Index 0Eh)</b>
</th></tr>
<tr>
<td>7</td><td>6</td><td>5</td><td>4</td><td>3</td><td>2</td><td>1</td><td>0</td>
</tr>
<tr>
<td colspan="8">Cursor Location High</td>
</tr>
</table>
 
* **Cursor Location High**
    This field specifies bits 15-8 of the Cursor Location field. See the [Cursor Location Low Register](#crtcreg-0F) for details.

 <table class="bitfield">
<tr><th colspan="8">
<b id="crtcreg-0F">Cursor Location Low Register (Index 0Fh)</b>
</th></tr>
<tr>
<td>7</td><td>6</td><td>5</td><td>4</td><td>3</td><td>2</td><td>1</td><td>0</td>
</tr>
<tr>
<td colspan="8">Cursor Location Low</td>
</tr>
</table>

* **Cursor Location Low**
    This field specifies bits 7-0 of the Cursor Location field. When the VGA hardware is displaying text mode and the text-mode cursor is enabled, the hardware compares the address of the character currently being displayed with sum of value of this field and the sum of the [Cursor Skew](#crtcreg-0B) field. If the values equal then the scan lines in that character specified by the [Cursor Scan Line Start](#crtcreg-0A) field and the [Cursor Scan Line End](#crtcreg-0B) field are replaced with the foreground color.

 <table class="bitfield">
<tr><th colspan="8">
<b id="crtcreg-10">Vertical Retrace Start Register (Index 10h)</b>
</th></tr>
<tr>
<td>7</td><td>6</td><td>5</td><td>4</td><td>3</td><td>2</td><td>1</td><td>0</td>
</tr>
<tr>
<td colspan="8">Vertical Retrace Start</td>
</tr>
</table>
 
* **Vertical Retrace Start**
    This field specifies bits 7-0 of the Vertical Retrace Start field.  Bits 9-8 are located in the [Overflow Register](#crtcreg-07).  This field controls the start of the vertical retrace pulse which signals the display to move up to the beginning of the active display.  This field contains the value of the vertical scanline counter at the beginning of the first scanline where the vertical retrace signal is asserted.

 <table class="bitfield">
<tr><th colspan="8">
<b id="crtcreg-11">Vertical Retrace End Register (Index 11h)</b>
</th></tr>
<tr>
<td>7</td><td>6</td><td>5</td><td>4</td><td>3</td><td>2</td><td>1</td><td>0</td>
</tr>
<tr>
<td>Protect</td>
<td>Bandwidth</td>
<td></td>
<td></td>
<td colspan="4">Vertical Retrace End</td>
</tr>
</table>

* **Protect -- CRTC Registers Protect Enable**
    This field is used to protect the video timing registers from being changed by programs written for earlier graphics chipsets that attempt to program these registers with values unsuitable for VGA timings.  When this field is set to 1, the CRTC register indexes 00h-07h ignore write access, with the exception of bit 4 of the [Overflow Register](#crtcreg-07), which holds bit 8 of the [Line Compare](#crtcreg-18) field.
* **Bandwidth -- Memory Refresh Bandwidth**
    Nearly all video chipsets include a few registers that control memory, bus, or other timings not directly related to the output of the video card.  Most VGA/SVGA implementations ignore the value of this field; however, in the least, IBM VGA adapters do utilize it and thus for compatibility with these chipsets this field should be programmed.  This register is used in the IBM VGA hardware to control the number of DRAM refresh cycles per scan line.  The three refresh cycles per scanline is appropriate for the IBM VGA horizontal frequency of approximately 31.5 kHz.  For horizontal frequencies greater than this, this setting will work as the DRAM will be refreshed more often.  However, refreshing not often enough for the DRAM can cause memory loss.  Thus at some point slower than 31.5 kHz the five refresh cycle setting should be used.  At which particular point this should occur, would require better knowledge of the IBM VGA's schematics than I have available.  According to IBM documentation, "_Selecting five refresh cycles allows use of the VGA chip with 15.75 kHz displays._" which isn't really enough to go by unless the mode you are defining has a 15.75 kHz horizontal frequency.
* **Vertical Retrace End**
    This field determines the end of the vertical retrace pulse, and thus its length.  This field contains the lower four bits of the vertical scanline counter at the beginning of the scanline immediately after the last scanline where the vertical retrace signal is asserted.

 <table class="bitfield">
<tr><th colspan="8">
<b id="crtcreg-12">Vertical Display End Register (Index 12h)</b>
</th></tr>
<tr>
<td>7</td><td>6</td><td>5</td><td>4</td><td>3</td><td>2</td><td>1</td><td>0</td>
</tr>
<tr>
<td colspan="8">Vertical Display End</td>
</tr>
</table>

* **Vertical Display End**
    This contains the bits 7-0 of the Vertical Display End field.  Bits 9-8 are located in the [Overflow Register](#crtcreg-07).  The field contains the value of the vertical scanline counter at the beggining of the scanline immediately after the last scanline of active display.

 <table class="bitfield">
<tr><th colspan="8">
<b id="crtcreg-13">Offset Register (Index 13h)</b>
</th></tr>
<tr>
<td>7</td><td>6</td><td>5</td><td>4</td><td>3</td><td>2</td><td>1</td><td>0</td>
</tr>
<tr>
<td colspan="8">Offset</td>
</tr>
</table>

* **Offset**
    This field specifies the address difference between consecutive scan lines or two lines of characters. Beginning with the second scan line, the starting scan line is increased by twice the value of this register multiplied by the current memory address size (byte = 1, word = 2, double-word = 4) each line. For text modes the following equation is used:
    ```
    Offset = Width / ( MemoryAddressSize * 2 )
    ```  
    and in graphics mode, the following equation is used:
    ```
    Offset = Width / ( PixelsPerAddress * MemoryAddressSize * 2 )
    ```
    where Width is the width in pixels of the screen. This register can be modified to provide for a virtual resolution, in which case Width is the width is the width in pixels of the virtual screen. PixelsPerAddress is the number of pixels stored in one display memory address, and MemoryAddressSize is the current memory addressing size.

 <table class="bitfield">
<tr><th colspan="8">
<b id="crtcreg-14">Underline Location Register (Index 14h)</b>
</th></tr>
<tr>
<td>7</td><td>6</td><td>5</td><td>4</td><td>3</td><td>2</td><td>1</td><td>0</td>
</tr>
<tr>
<td></td>
<td>DW</td>
<td>DIV4</td>
<td colspan="5">Underline Location</td>
</tr>
</table>

* **DW - Double-Word Addressing**
    "_When this bit is set to 1, memory addresses are doubleword addresses. See the description of the word/byte mode bit (bit 6) in the CRT Mode Control Register_"  
* **DIV4 - Divide Memory Address Clock by 4**
    "_When this bit is set to 1, the memory-address counter is clocked with the character clock divided by 4, which is used when doubleword addresses are used._"  
* **Underline Location**
    "_These bits specify the horizontal scan line of a character row on which an underline occurs. The value programmed is the scan line desired minus 1._"

 <table class="bitfield">
<tr><th colspan="8">
<b id="crtcreg-15">Start Vertical Blanking Register (Index 15h)</b>
</th></tr>
<tr>
<td>7</td><td>6</td><td>5</td><td>4</td><td>3</td><td>2</td><td>1</td><td>0</td>
</tr>
<tr>
<td colspan="8">Start Vertical Blanking</td>
</tr>
</table>

* **Start Vertical Blanking**
    This contains bits 7-0 of the Start Vertical Blanking field.  Bit 8 of this field is located in the [Overflow Register](#crtcreg-07), and bit 9 is located in the [Maximum Scan Line Register](#crtcreg-09).  This field determines when the vertical blanking period begins, and contains the value of the vertical scanline counter at the beginning of the first vertical scanline of blanking.

 <table class="bitfield">
<tr><th colspan="8">
<b id="crtcreg-16">End Vertical Blanking Register (Index 16h)</b>
</th></tr>
<tr>
<td>7</td><td>6</td><td>5</td><td>4</td><td>3</td><td>2</td><td>1</td><td>0</td>
</tr>
<tr>
<td colspan="8">End Vertical Blanking</td>
</tr>
</table>

* **End Vertical Blanking**
    This field determines when the vertical blanking period ends, and contains the value of the vertical scanline counter at the beginning of the vertical scanline immediately after the last scanline of blanking.

 <table class="bitfield">
<tr><th colspan="8">
<b id="crtcreg-17">CRTC Mode Control Register (Index 17h)</b>
</th></tr>
<tr>
<td>7</td><td>6</td><td>5</td><td>4</td><td>3</td><td>2</td><td>1</td><td>0</td>
</tr>
<tr>
<td>SE</td>
<td>Word/Byte</td>
<td>AW</td>
<td></td>
<td>DIV2</td>
<td>SLDIV</td>
<td>MAP14</td>
<td>MAP13</td>
</tr>
</table>

* **SE -- Sync Enable**
    "_When set to 0, this bit disables the horizontal and vertical retrace signals and forces them to an inactive level. When set to 1, this bit enables the horizontal and vertical retrace signals. This bit does not reset any other registers or signal outputs._"  
* **Word/Byte -- Word/Byte Mode Select**
    "_When this bit is set to 0, the word mode is selected. The word mode shifts the memory-address counter bits to the left by one bit; the most-significant bit of the counter appears on the least-significant bit of the memory address outputs.  The doubleword bit in the Underline Location register (0x14) also controls the addressing. When the doubleword bit is 0, the word/byte bit selects the mode. When the doubleword bit is set to 1, the addressing is shifted by two bits. When set to 1, bit 6 selects the byte address mode._"  
* **AW -- Address Wrap Select**
    "_This bit selects the memory-address bit, bit MA 13 or MA 15, that appears on the output pin MA 0, in the word address mode. If the VGA is not in the word address mode, bit 0 from the address counter appears on the output pin, MA 0. When set to 1, this bit selects MA 15. In odd/even mode, this bit should be set to 1 because 256KB of video memory is installed on the system board. (Bit MA 13 is selected in applications where only 64KB is present. This function maintains compatibility with the IBM Color/Graphics Monitor Adapter.)_"  
* **DIV2 -- Divide Memory Address clock by 2**
    "_When this bit is set to 0, the address counter uses the character clock. When this bit is set to 1, the address counter uses the character clock input divided by 2. This bit is used to create either a byte or word refresh address for the display buffer._"  
* **SLDIV -- Divide Scan Line clock by 2**
    "_This bit selects the clock that controls the vertical timing counter. The clocking is either the horizontal retrace clock or horizontal retrace clock divided by 2. When this bit is set to 1. the horizontal retrace clock is divided by 2. Dividing the clock effectively doubles the vertical resolution of the CRT controller. The vertical counter has a maximum resolution of 1024 scan lines because the vertical total value is 10-bits wide. If the vertical counter is clocked with the horizontal retrace divided by 2, the vertical resolution is doubled to 2048 scan lines._"  
* **MAP14 -- Map Display Address 14**
    "_This bit selects the source of bit 14 of the output multiplexer. When this bit is set to 0, bit 1 of the row scan counter is the source. When this bit is set to 1, the bit 14 of the address counter is the source._"  
* **MAP13 -- Map Display Address 13**
    "_This bit selects the source of bit 13 of the output multiplexer. When this bit is set to 0, bit 0 of the row scan counter is the source, and when this bit is set to 1, bit 13 of the address counter is the source. The CRT controller used on the IBM Color/Graphics Adapter was capable of using 128 horizontal scan-line addresses. For the VGA to obtain 640-by-200 graphics resolution, the CRT controller is  programmed for 100 horizontal scan lines with two scan-line addresses per character row. Row scan  address bit 0 becomes the most-significant address bit to the display buffer. Successive scan lines of  the display image are displaced in 8KB of memory. This bit allows compatibility with the graphics modes of earlier adapters._"

 <table class="bitfield">
<tr><th colspan="8">
<b id="crtcreg-18">Line Compare Register (Index 18h)</b>
</th></tr>
<tr>
<td>7</td><td>6</td><td>5</td><td>4</td><td>3</td><td>2</td><td>1</td><td>0</td>
</tr>
<tr>
<td colspan="8">Line Compare Register</td>
</tr>
</table>

* **Line Compare Register**
    This field specifies bits 7-0 of the Line Compare field. Bit 9 of this field is located in the [Maximum Scan Line Register](#crtcreg-09), and bit 8 of this field is located in the [Overflow Register](#crtcreg-07). The Line Compare field specifies the scan line at which a horizontal division can occur, providing for split-screen operation. If no horizontal division is required, this field should be set to 3FFh. When the scan line counter reaches the value in the Line Compare field, the current scan line address is reset to 0 and the Preset Row Scan is presumed to be 0. If the [Pixel Panning Mode](#attrreg-10) field is set to 1 then the Pixel Shift Count and Byte Panning fields are reset to 0 for the remainder of the display cycle.

## Color Registers

The Color Registers in the standard VGA provide a mapping between the palette of between 2 and 256 colors to a larger 18-bit color space. This capability allows for efficient use of video memory while providing greater flexibility in color choice. The standard VGA has 256 palette entries containing six bits each of red, green, and blue values. The palette RAM is accessed via a pair of address registers and a data register. To write a palette entry, output the palette entry's index value to the [DAC Address Write Mode Register](#3C8) then perform 3 writes to the [DAC Data Register](#3C9), loading the red, green, then blue values into the palette RAM. The internal write address automatically advances allowing the next value's RGB values to be loaded without having to reprogram the [DAC Address Write Mode Register.  This](#3C8) allows the entire palette to be loaded in one write operation. To read a palette entry, output the palette entry's index to the [DAC Address Read Mode Register](#3C7W). Then perform 3 reads from the [DAC Data Register](#3C9), loading the red, green, then blue values from palette RAM. The internal write address automatically advances allowing the next RGB values to be written without having to reprogram the [DAC Address Read Mode Register](#3C7W).

<span id="colorreg-note">Note</span>: I have noticed some great variance in the actual behavior of these registers on VGA chipsets. The best way to ensure compatibility with the widest range of cards is to start an operation by writing to the appropriate address register and performing reads and writes in groups of 3 color values. While the automatic increment works fine on all cards tested, reading back the value from the [DAC Address Write Mode Register](#3C8) may not always produce the expected result. Also interleaving reads and writes to the [DAC Data Register](#3C9) without first writing to the respected address register may produce unexpected results. In addition, writing values in anything other than groups of 3 to the [DAC Data Register](#3C9) and then performing reads may produce unexpected results. I have found that some cards fail to perform the desired update until the third value is written.

* Port 3C8h -- [DAC Address Write Mode Register](#3C8)
* Port 3C7h -- [DAC Address Read Mode Register](#3C7W)
* Port 3C9h -- [DAC Data Register](#3C9)
* Port 3C7h -- [DAC State Register](#3C7R)

 <table class="bitfield">
<tr><th colspan="8">
<b id="3C8">DAC Address Write Mode Register (Read/Write at 3C8h)</b>
</th></tr>
<tr>
<td>7</td><td>6</td><td>5</td><td>4</td><td>3</td><td>2</td><td>1</td><td>0</td>
</tr>
<tr>
<td colspan="8">DAC Write Address</td>
</tr>
</table>

* **DAC Write Address**
    Writing to this register prepares the DAC hardware to accept writes of data to the [DAC Data Register](#3C9). The value written is the index of the first DAC entry to be written (multiple DAC entries may be written without having to reset the write address due to the auto-increment.) Reading this register returns the current index, or at least theoretically it should. However it is likely the value returned is not the one expected, and is dependent on the particular DAC implementation. (See [note](#colorreg-note) above)

 <table class="bitfield">
<tr><th colspan="8">
<b id="3C7W">DAC Address Read Mode Register (Write at 3C7h)</b>
</th></tr>
<tr>
<td>7</td><td>6</td><td>5</td><td>4</td><td>3</td><td>2</td><td>1</td><td>0</td>
</tr>
<tr>
<td colspan="8">DAC Read Address</td>
</tr>
</table>

* **DAC Read Address**
    Writing to this register prepares the DAC hardware to accept reads of data to the [DAC Data Register](#3C9). The value written is the index of the first DAC entry to be read (multiple DAC entries may be read without having to reset the write address due to the auto-increment.)

 <table class="bitfield">
<tr><th colspan="8">
<b id="3C9">DAC Data Register (Read/Write at 3C9h)</b>
</th></tr>
<tr>
<td>7</td><td>6</td><td>5</td><td>4</td><td>3</td><td>2</td><td>1</td><td>0</td>
</tr>
<tr>
<td></td>
<td></td>
<td colspan="6">DAC Data</td>
</tr>
</table>

* **DAC Data**
    Reading or writing to this register returns a value from the DAC memory. Three successive I/O operations accesses three intensity values, first the red, then green, then blue intensity values. The index of the DAC entry accessed is initially specified by the [DAC Address Read Mode Register](#3C7W) or the [DAC Address Write Mode Register](#3C8), depending on the I/O operation performed. After three I/O operations the index automatically increments to allow the next DAC entry to be read without having to reload the index. I/O operations to this port should always be performed in sets of three, otherwise the results are dependent on the DAC implementation. (See [note](#colorreg-note) above)

 <table class="bitfield">
<tr><th colspan="8">
<b id="3C7R">DAC State Register (Read at 3C7h)</b>
</th></tr>
<tr>
<td>7</td><td>6</td><td>5</td><td>4</td><td>3</td><td>2</td><td>1</td><td>0</td>
</tr>
<tr>
<td></td>
<td></td>
<td></td>
<td></td>
<td></td>
<td></td>
<td colspan="2">DAC State</td>
</tr>
</table>

* **DAC State**
    This field returns whether the DAC is prepared to accept reads or writes to the [DAC Data Register](#3C9). In practice, this field is seldom used due to the DAC state being known after the index has been written. This field can have the following values:
    * 00 -- DAC is prepared to accept reads from the [DAC Data Register](#3C9).
    * 11 -- DAC is prepared to accept writes to the [DAC Data Register](#3C9).

## External Regsters

The External Registers (sometimes called the General Registers) each have their own unique I/O location in the VGA, although sometimes the Read Port differs from the Write port, and some are Read-only.. See the [Accessing the VGA Registers](#accessing-the-vga-registers) section for more detals.

* Port 3CCh/3C2h -- _Miscellaneous Output Register_
* Port 3CAh/3xAh -- _Feature Control Register_
* Port 3C2h -- _Input Status #0 Register_
* Port 3xAh -- _Input Status #1 Register_

<table class="bitfield">
<tr><th colspan="8">
<b id="3CCR3C2W">Miscellaneous Output Register (Read at 3CCh, Write at 3C2h)</b>
</th></tr>
<tr>
<td>7</td><td>6</td><td>5</td><td>4</td><td>3</td><td>2</td><td>1</td><td>0</td>
</tr>
<tr>
<td>VSYNCP</td>
<td>HSYNCP</td>
<td>O/E Page</td>
<td></td>
<td colspan="2">Clock Select</td>
<td>RAM En.</td>
<td>I/OAS</td>
</tr>
</table>

* **VSYNCP -- Vertical Sync Polarity**
    "_Determines the polarity of the vertical sync pulse and can be used (with HSP) to control the vertical size of the display by utilizing the autosynchronization feature of VGA displays._  
    _= 0 selects a positive vertical retrace sync pulse._"
* **HSYNCP -- Horizontal Sync Polarity**
    "_Determines the polarity of the horizontal sync pulse._  
    _= 0 selects a positive horizontal retrace sync pulse._"  
* **O/E Page -- Odd/Even Page Select**
    "_Selects the upper/lower 64K page of memory when the system is in an eve/odd mode (modes 0,1,2,3,7)._  
    _= 0 selects the low page_  
    _= 1 selects the high page_"
* **Clock Select**
    This field controls the selection of the dot clocks used in driving the display timing.  The standard hardware has 2 clocks available to it, nominally 25 Mhz and 28 Mhz.  It is possible that there may be other "external" clocks that can be selected by programming this register with the undefined values.  The possible valuse of this register are:
    * 00 -- select 25 Mhz clock (used for 320/640 pixel wide modes)
    * 01 -- select 28 Mhz clock (used for 360/720 pixel wide modes)
    * 10 -- undefined (possible external clock)
    * 11 -- undefined (possible external clock)

* **RAM En. -- RAM Enable**"
    _Controls system access to the display buffer._  
    _= 0 disables address decode for the display buffer from the system_  
    _= 1 enables address decode for the display buffer from the system_"  

* **I/OAS -- Input/Output Address Select**
    "_This bit selects the CRT controller addresses. When set to 0, this bit sets the CRT controller addresses to 0x03Bx and the address for the Input Status Register 1 to 0x03BA for compatibility withthe monochrome adapter.  When set to 1, this bit sets CRT controller addresses to 0x03Dx and the Input Status Register 1 address to 0x03DA for compatibility with the color/graphics adapter. The Write addresses to the Feature Control register are affected in the same manner._"

<table class="bitfield">
<tr><th colspan="8">
<b id="3CAR3xAW">Feature Control Register (Read at 3CAh, Write at 3BAh (mono) or 3DAh (color))</b>
</th></tr>
<tr>
<td>7</td><td>6</td><td>5</td><td>4</td><td>3</td><td>2</td><td>1</td><td>0</td>
</tr>
<tr>
<td></td>
<td></td>
<td></td>
<td></td>
<td></td>
<td></td>
<td>FC1</td>
<td>FC0</td>
</tr>
</table>

* **FC1 -- Feature Control bit 1**
    "_All bits are reserved._"
* **FC2 -- Feature Control bit 0**
    "_All bits are reserved._"

 <table class="bitfield">
<tr><th colspan="8">
<b id="3C2R">Input Status #0 Register (Read-only at 3C2h)</b>
</th></tr>
<tr>
<td>7</td><td>6</td><td>5</td><td>4</td><td>3</td><td>2</td><td>1</td><td>0</td>
</tr>
<tr>
<td></td>
<td></td>
<td></td>
<td>SS</td>
<td></td>
<td></td>
<td></td>
<td></td>
</tr>
</table>

* **SS - Switch Sense**
    "_Returns the status of the four sense switches as selected by the CS field of the Miscellaneous Output Register._"

 <table class="bitfield">
<tr><th colspan="8">
<b id="3xAR">Input Status #1 Register (Read at 3BAh (mono) or 3DAh (color))</b>
</th></tr>
<tr>
<td>7</td><td>6</td><td>5</td><td>4</td><td>3</td><td>2</td><td>1</td><td>0</td>
</tr>
<tr>
<td></td>
<td></td>
<td></td>
<td></td>
<td>VRetrace</td>
<td></td>
<td></td>
<td>DD</td>
</tr>
</table>

* **VRetrace -- Vertical Retrace**
    "_When set to 1, this bit indicates that the display is in a vertical retrace interval.This bit can be programmed, through the Vertical Retrace End register, to generate an interrupt at the start of the vertical retrace._"  
* **DD -- Display Disabled**
    "_When set to 1, this bit indicates a horizontal or vertical retrace interval. This bit is the real-time status of the inverted 'display enable' signal. Programs have used this status bit to restrict screen updates to the inactive display intervals in order to reduce screen flicker. The video subsystem is designed to eliminate this software requirement; screen updates may be made at any time without screen degradation._"

## VGA Field Index

[A](#vgargidx-A) | [B](#vgargidx-B) | [C](#vgargidx-C) | [D](#vgargidx-D)   | [E](#vgargidx-E) | [F](#vgargidx-F) | G | [H](#vgargidx-H) | [I](#vgargidx-I) | J | K | [L](#vgargidx-L) | [M](#vgargidx-M) | N | [O](#vgargidx-O) | [P](#vgargidx-P) | Q | [R](#vgargidx-R) | [S](#vgargidx-S) | T | [U](#vgargidx-U) | [V](#vgargidx-V) | [W](#vgargidx-W) | X | Y | Z 

* 256-Color Shift Mode -- [Graphics Mode Register](#graphreg-05)
* 8-bit Color Enable -- [Attribute Mode Control Register](#attrreg-10)
* 9/8 Dot Mode -- [Clocking Mode Register](#seqreg-01)
* <span id="#vgargidx-A">Address</span> Wrap Select -- [CRTC Mode Control Register](#crtcreg-17)
* Alphanumeric Mode Disable -- [Miscellaneous Graphics Register](#graphreg-06)
* Asynchronous Reset -- [Reset Register](#seqreg-00)
* Attribute Address -- [Attribute Address Register](#3C0)
* Attribute Controller Graphics Enable -- [Attribute Mode Control Register](#attrreg-10)
* <span id="#vgargidx-B">Bit</span> Mask -- [Bit Mask Register](#graphreg-08)
* Blink Enable -- [Attribute Mode Control Register](#attrreg-10)
* Byte Panning -- [Preset Row Scan Register](#crtcreg-08)
* <span id="#vgargidx-C">Chain</span> 4 Enable -- [Sequencer Memory Mode Register](#seqreg-04)
* Clock Select -- [Miscellaneous Output Register](#3CCR3C2W)
* Chain Odd/Even Enable -- [Miscellaneous Graphics Register](#graphreg-06)
* Character Set A Select -- [Character Map Select Register](#seqreg-03)
* Character Set B Select -- [Character Map Select Register](#seqreg-03)
* Color Compare -- [Color Compare Register](#graphreg-02)
* Color Don't Care -- [Color Don't Care Register](#graphreg-07)
* Color Plane Enable -- [Color Plane Enable Register](#attrreg-12)
* Color Select 5-4 -- [Color Select Register](#attrreg-14)
* Color Select 7-6 -- [Color Select Register](#attrreg-14)
* CRTC Registers Protect Enable -- [Vertical Retrace End Register](#crtcreg-11)
* Cursor Disable -- [Cursor Start Reguster](#crtcreg-0A)
* Cursor Location -- bits 15-8: [Cursor Location High Register](#crtcreg-0E), bits 7-0: [Cursor Location Low Register](#crtcreg-0F)
* Cursor Scan Line End -- [Cursor End Register](#crtcreg-0B)
* Cursor Scan Line Start -- [Cursor Start Reguster](#crtcreg-0A)
* Cursor Skew -- [Cursor End Register](#crtcreg-0B)
* <span id="#vgargidx-D">DAC</span> Data -- [DAC Data Register](#3C9)
* DAC Read Address -- [DAC Address Read Mode Register](#3C7W)
* DAC State -- [DAC State Register](#3C7R)
* DAC Write Address -- [DAC Address Write Mode Register](#3C8)
* Display Disabled -- [Input Status #1 Register](#3xAR)
* Display Enable Skew -- [End Horizontal Blanking Register](#crtcreg-03)
* Divide Memory Address Clock by 4 -- [Underline Location Register](#crtcreg-14)
* Divide Scan Line Clock by 2 -- [CRTC Mode Control Register](#crtcreg-17)
* Dot Clock Rate -- [Clocking Mode Register](#seqreg-01)
* Double-Word Addressing -- [Underline Location Register](#crtcreg-14)
* <span id="#vgargidx-E">Enable</span> Set/Reset -- [Enable Set/Reset Register](#graphreg-01)
* Enable Vertical Retrace Access -- [End Horizontal Blanking Register](#crtcreg-03)
* End Horizontal Display -- [End Horizontal Display Register](#crtcreg-01)
* End Horizontal Blanking -- bit 5: [End Horizontal Retrace Register](#crtcreg-05), bits 4-0: [End Horizontal Blanking Register](#crtcreg-03),
* End Horizontal Retrace -- [End Horizontal Retrace Register](#crtcreg-05)
* End Vertical Blanking -- [End Vertical Blanking Register](#crtcreg-16)
* Extended Memory -- [Sequencer Memory Mode Register](#seqreg-04)
* <span id="#vgargidx-F">Feature</span> Control Bit 0 -- [Feature Control Register](#3CAR3xAW)
* Feature Control Bit 1 -- [Feature Control Register](#3CAR3xAW)
* <span id="#vgargidx-H">Horizontal</span> Retrace Skew -- [End Horizontal Retrace Register](#crtcreg-05)
* Horizontal Sync Polarity -- [Miscellaneous Output Register](#3CCR3C2W)
* Horizontal Total -- [Horizontal Total Register](#crtcreg-00)
* Host Odd/Even Memory Read Addressing Enable -- [Graphics Mode Register](#graphreg-05)
* Host Odd/Even Memory Write Addressing Enable -- [Sequencer Memory Mode Register](#seqreg-04)
* <span id="#vgargidx-I">Input/Output</span> Address Select -- [Miscellaneous Output Register](#3CCR3C2W)
* Internal Palette Index -- [Palette Registers](#attrreg-00-0F)
* <span id="#vgargidx-L">Line</span> Compare -- bit 9: [Maximum Scan Line Register](#crtcreg-09), bit 8: [Overflow Register](#crtcreg-07), bits 7-0: [Line Compare Register](#crtcreg-18)
* Line Graphics Enable -- [Attribute Mode Control Register](#attrreg-10)
* Logical Operation -- [Data Rotate Register](#graphreg-03)
* <span id="#vgargidx-M">Map</span> Display Address 13 -- [CRTC Mode Control Register](#crtcreg-17)
* Map Display Address 14 -- [CRTC Mode Control Register](#crtcreg-17)
* Maximum Scan Line -- [Maximum Scan Line Register](#crtcreg-09)
* Memory Map Select -- [Miscellaneous Graphics Register](#graphreg-06)
* Memory Plane Write Enable -- [Map Mask Register](#seqreg-02)
* Memory Refresh Bandwidth -- [Vertical Retrace End Register](#crtcreg-11)
* Monochrome Emulation -- [Attribute Mode Control Register](#attrreg-10)
* <span id="#vgargidx-O">Odd/Even</span> Page Select -- [Miscellaneous Output Register](#3CCR3C2W)
* Offset -- [Offset Register](#crtcreg-13)
* Overscan Palette Index -- [Overscan Color Register](#attrreg-11)
* <span id="#vgargidx-P">Palette</span> Address Source -- [Attribute Address Register](#3C0)
* Palette Bits 5-4 Select -- [Attribute Mode Control Register](#attrreg-10)
* Pixel Panning Mode -- [Attribute Mode Control Register](#attrreg-10)
* Pixel Shift Count -- [Horizontal Pixel Panning Register](#attrreg-13)
* Preset Row Scan -- [Preset Row Scan Register](#crtcreg-08)
* <span id="#vgargidx-R">RAM</span> Enable -- [Miscellaneous Output Register](#3CCR3C2W)
* Read Map Select -- [Read Map Select Register](#graphreg-04)
* Read Mode - [Graphics Mode Register](#graphreg-05)
* Rotate Count -- [Data Rotate Register](#graphreg-03)
* <span id="#vgargidx-S">Scan</span> Doubling -- [Maximum Scan Line Register](#crtcreg-09)
* Screen Disable -- [Clocking Mode Register](#seqreg-01)
* Set/Reset -- [Set/Reset Register](#graphreg-00)
* Shift Four Enable -- [Clocking Mode Register](#seqreg-01)
* Shift/Load Rate -- [Clocking Mode Register](#seqreg-01)
* Shift Register Interleave Mode -- [Graphics Mode Register](#graphreg-05)
* Start Address -- bits 15-8: [Start Address High Register](#crtcreg-0C), bits 7-0: [Start Address Low Register](#crtcreg-0D)
* Start Horizontal Blanking -- [Start Horizontal Blanking Register](#crtcreg-02)
* Start Horizontal Retrace -- [Start Horizontal Retrace Register](#crtcreg-04)
* Start Vertical Blanking -- bit 9: [Maximum Scan Line Register](#crtcreg-09), bit 8: [Overflow Register](#crtcreg-07), bits 7-0: [Start Vertical Blanking Register](#crtcreg-15)
* Switch Sense -- [Input Status #0 Register](#3C2R)
* Sync Enable -- [CRTC Mode Control Register](#crtcreg-17)
* Sycnchronous Reset -- [Reset Register](#seqreg-00)
* <span id="#vgargidx-U">Underline</span> Location -- [Underline Location Register](#crtcreg-14)
* <span id="#vgargidx-V">Vertical</span> Display End -- bits 9-8: [Overflow Register](#crtcreg-07), bits 7-0: [Vertical Display End Register](#crtcreg-12)
* Vertical Retrace -- [Input Status #1 Register](#3xAR)
* Vertical Retrace End -- [Vertical Retrace End Register](#crtcreg-11)
* Vertical Retrace Start -- bits 9-8: [Overflow Register](#crtcreg-07), bits 7-0: [Vertical Retrace Start Register](#crtcreg-10)
* Vertical Sync Polarity -- [Miscellaneous Output Register](#3CCR3C2W)
* Vertical Total -- bits 9-8: [Overflow Register](#crtcreg-07), bits 7-0: [Vertical Total Register](#crtcreg-06)
* <span id="#vgargidx-W">Word/Byte</span> Mode Select -- [CRTC Mode Control Register](#crtcreg-17)
* Write Mode -- [Graphics Mode Register](#graphreg-05)

## VGA Functional Index 

### Register Access Functions

These fields control the acessability/inaccessability of the VGA registers. These registers are used for compatibiltiy with older programs that may attempt to program the VGA in a fashion suited only to an EGA, CGA, or monochrome card.

* CRTC Registers Protect Enable -- [Vertical Retrace End Register](#crtcreg-11)
* Enable Vertical Retrace Access -- [End Horizontal Blanking Register](#crtcreg-03)
* Input/Output Address Select -- [Miscellaneous Output Register](#3CCR3C2W)

### Display Memory Access Functions

These fields control the way the video RAM is mapped into the host CPU's address space and how memory reads/writes affect the display memory.

* Bit Mask -- [Bit Mask Register](#graphreg-08)
* Chain 4 Enable -- [Sequencer Memory Mode Register](#seqreg-04)
* Chain Odd/Even Enable -- [Miscellaneous Graphics Register](#graphreg-06)
* Color Compare -- [Color Compare Register](#graphreg-02)
* Color Don't Care -- [Color Don't Care Register](#graphreg-07)
* Enable Set/Reset -- [Enable Set/Reset Register](#graphreg-01)
* Extended Memory -- [Sequencer Memory Mode Register](#seqreg-04)
* Host Odd/Even Memory Read Addressing Enable -- [Graphics Mode Register](#graphreg-05)
* Host Odd/Even Memory Write Addressing Enable -- [Sequencer Memory Mode Register](#seqreg-04)
* Logical Operation -- [Data Rotate Register](#graphreg-03)
* Memory Map Select -- [Miscellaneous Graphics Register](#graphreg-06)
* Memory Plane Write Enable -- [Map Mask Register](#seqreg-02)
* Odd/Even Page Select -- [Miscellaneous Output Register](#3CCR3C2W)
* RAM Enable -- [Miscellaneous Output Register](#3CCR3C2W)
* Read Map Select -- [Read Map Select Register](#graphreg-04)
* Read Mode - [Graphics Mode Register](#graphreg-05)
* Rotate Count -- [Data Rotate Register](#graphreg-03)
* Set/Reset -- [Set/Reset Register](#graphreg-00)
* Write Mode -- [Graphics Mode Register](#graphreg-05)

### Display Sequencing Functions

These fields affect the way the video memory is serialized for display.

* 256-Color Shift Mode -- [Graphics Mode Register](#graphreg-05)
* 9/8 Dot Mode -- [Clocking Mode Register](#seqreg-01)
* Address Wrap Select -- [CRTC Mode Control Register](#crtcreg-17)
* Alphanumeric Mode Disable -- [Miscellaneous Graphics Register](#graphreg-06)
* Asynchronous Reset -- [Reset Register](#seqreg-00)
* Byte Panning -- [Preset Row Scan Register](#crtcreg-08)
* Character Set A Select -- [Character Map Select Register](#seqreg-03)
* Character Set B Select -- [Character Map Select Register](#seqreg-03)
* Divide Memory Address Clock by 4 -- [Underline Location Register](#crtcreg-14)
* Double-Word Addressing -- [Underline Location Register](#crtcreg-14)
* Pixel Shift Count -- [Horizontal Pixel Panning Register](#attrreg-13)
* Line Compare -- bit 9: [Maximum Scan Line Register](#crtcreg-09), bit 8: [Overflow Register](#crtcreg-07), bits 7-0: [Line Compare Register](#crtcreg-18)
* Line Graphics Enable -- [Attribute Mode Control Register](#attrreg-10)
* Map Display Address 13 -- [CRTC Mode Control Register](#crtcreg-17)
* Map Display Address 14 -- [CRTC Mode Control Register](#crtcreg-17)
* Maximum Scan Line -- [Maximum Scan Line Register](#crtcreg-09)
* Offset -- [Offset Register](#crtcreg-13)
* Pixel Panning Mode -- [Attribute Mode Control Register](#attrreg-10)
* Preset Row Scan -- [Preset Row Scan Register](#crtcreg-08)
* Scan Doubling -- [Maximum Scan Line Register](#crtcreg-09)
* Screen Disable -- [Clocking Mode Register](#seqreg-01)
* Shift Four Enable -- [Clocking Mode Register](#seqreg-01)
* Shift/Load Rate -- [Clocking Mode Register](#seqreg-01)
* Shift Register Interleave Mode -- [Graphics Mode Register](#graphreg-05)
* Start Address -- bits 15-8: [Start Address High Register](#crtcreg-0C), bits 7-0: [Start Address Low Register](#crtcreg-0D)
* Sycnchronous Reset -- [Reset Register](#seqreg-00)
* Word/Byte Mode Select -- [CRTC Mode Control Register](#crtcreg-17)

### Cursor Functions

These fields affect the operation of the cursor displayed while the VGA hardware is in text mode.

* Cursor Disable -- [Cursor Start Reguster](#crtcreg-0A)
* Cursor Location -- bits 15-8: [Cursor Location High Register](#crtcreg-0E), bits 7-0: [Cursor Location Low Register](#crtcreg-0F)
* Cursor Scan Line End -- [Cursor End Register](#crtcreg-0B)
* Cursor Scan Line Start -- [Cursor Start Reguster](#crtcreg-0A)
* Cursor Skew -- [Cursor End Register](#crtcreg-0B)

### Attribute Functions

These fields control the way the video data is submitted to the RAMDAC, providing color/blinking capability in text mode and facilitating the mapping of colors in graphics mode.

* 8-bit Color Enable -- [Attribute Mode Control Register](#attrreg-10)
* Attribute Address -- [Attribute Address Register](#3C0)
* Attribute Controller Graphics Enable -- [Attribute Mode Control Register](#attrreg-10)
* Blink Enable -- [Attribute Mode Control Register](#attrreg-10)
* Color Plane Enable -- [Color Plane Enable Register](#attrreg-12)
* Color Select 5-4 -- [Color Select Register](#attrreg-14)
* Color Select 7-6 -- [Color Select Register](#attrreg-14)
* Internal Palette Index -- [Palette Registers](#attrreg-00-0F)
* Monochrome Emulation -- [Attribute Mode Control Register](#attrreg-10)
* Overscan Palette Index -- [Overscan Color Register](#attrreg-11)
* Underline Location -- [Underline Location Register](#crtcreg-14)
* Palette Address Source -- [Attribute Address Register](#3C0)
* Palette Bits 5-4 Select -- [Attribute Mode Control Register](#attrreg-10)

### DAC Functions

These fields allow control of the VGA's 256-color palette that is part of the RAMDAC.

* DAC Write Address -- [DAC Address Write Mode Register](#3C8)
* DAC Read Address -- [DAC Address Read Mode Register](#3C7W)
* DAC Data -- [DAC Data Register](#3C9)
* DAC State -- [DAC State Register](#3C7R)

### Display Generation Functions

These fields control the formatting and timing of the VGA's video signal output.

* Clock Select -- [Miscellaneous Output Register](#3CCR3C2W)
* Display Disabled -- [Input Status #1 Register](#3xAR)
* Display Enable Skew -- [End Horizontal Blanking Register](#crtcreg-03)
* Divide Scan Line Clock by 2 -- [CRTC Mode Control Register](#crtcreg-17)
* Dot Clock Rate -- [Clocking Mode Register](#seqreg-01)
* End Horizontal Display -- [End Horizontal Display Register](#crtcreg-01)
* End Horizontal Blanking -- bit 5: [End Horizontal Retrace Register](#crtcreg-05), bits 4-0: [End Horizontal Blanking Register](#crtcreg-03),
* End Horizontal Retrace -- [End Horizontal Retrace Register](#crtcreg-05)
* End Vertical Blanking -- [End Vertical Blanking Register](#crtcreg-16)
* Horizontal Retrace Skew -- [End Horizontal Retrace Register](#crtcreg-05)
* Horizontal Sync Polarity -- [Miscellaneous Output Register](#3CCR3C2W)
* Horizontal Total -- [Horizontal Total Register](#crtcreg-00)
* Memory Refresh Bandwidth -- [Vertical Retrace End Register](#crtcreg-11)
* Start Horizontal Blanking -- [Start Horizontal Blanking Register](#crtcreg-02)
* Start Horizontal Retrace -- [Start Horizontal Retrace Register](#crtcreg-04)
* Start Vertical Blanking -- bit 9: [Maximum Scan Line Register](#crtcreg-09), bit 8: [Overflow Register](#crtcreg-07), bits 7-0: [Start Vertical Blanking Register](#crtcreg-15)
* Sync Enable -- [CRTC Mode Control Register](#crtcreg-17)
* Vertical Display End -- bits 9-8: [Overflow Register](#crtcreg-07), bits 7-0: [Vertical Display End Register](#crtcreg-12)
* Vertical Retrace End -- [Vertical Retrace End Register](#crtcreg-11)
* Vertical Retrace -- [Input Status #1 Register](#3xAR)
* Vertical Retrace Start -- bits 9-8: [Overflow Register](#crtcreg-07), bits 7-0: [Vertical Retrace Start Register](#crtcreg-10)
* Vertical Sync Polarity -- [Miscellaneous Output Register](#3CCR3C2W)
* Vertical Total -- bits 9-8: [Overflow Register](#crtcreg-07), bits 7-0: [Vertical Total Register](#crtcreg-06)

### Miscellaneous Functions

These fields are used to detect the state of possible VGA hardware such as configuration switches/jumpers and feature connector inputs.

* Feature Control Bit 0 -- [Feature Control Register](#3CAR3xAW)
* Feature Control Bit 1 -- [Feature Control Register](#3CAR3xAW)
* Switch Sense -- [Input Status #0 Register](#3C2R)

## VGA I/O Port Index 

### Introduction  

This index lists the VGA's I/O ports in numerical order, making looking up a specific I/O port access simpler.  
 
* 3B4h -- [CRTC Controller Address Register](#crt-controller-registers)
* 3B5h -- [CRTC Controller Data Register](#crt-controller-registers)
* 3BAh Read -- [Input Status #1 Register](#3xAR)
* 3BAh Write -- [Feature Control Register](#3CAR3xAW)
* 3C0h -- [Attribute Address/Data Register](#attribute-controller-registers)
* 3C1h -- [Attribute Data Read Register](#attribute-controller-registers)
* 3C2h Read -- [Input Status #0 Register](#3C2R)
* 3C2h Write -- [Miscellaneous Output Register](#3CCR3C2W)
* 3C4h -- [Sequencer Address Register](#sequencer-registers)
* 3C5h -- [Sequencer Data Register](#sequencer-registers)
* 3C7h Read -- [DAC State Register](#3C7R)
* 3C7h Write -- [DAC Address Read Mode Register](#3C7W)
* 3C8h -- [DAC Address Write Mode Register](#3C8)
* 3C9h -- [DAC Data Register](#3C9)
* 3CAh Read -- [Feature Control Register](#3CAR3xAW)
* 3CCh Read -- [Miscellaneous Output Register](#3CCR3C2W)
* 3CEh -- [Graphics Controller Address Register](#graphics-registers)
* 3CFh -- [Graphics Controller Data Register](#graphics-registers)
* 3D4h -- [CRTC Controller Address Register](#crt-controller-registers)
* 3D5h -- [CRTC Controller Data Register](#crt-controller-registers)
* 3DAh Read -- [Input Status #1 Register](#3xAR)
* 3DAh Write -- [Feature Control Register](#3CAR3xAW)

Notice: All trademarks used or referred to on this page are the property of their respective owners.  
All pages are Copyright © 1997, 1998, J. D. Neal, except where noted. Permission for utilization and distribution is subject to the terms of the [FreeVGA Project Copyright License](../license.htm).
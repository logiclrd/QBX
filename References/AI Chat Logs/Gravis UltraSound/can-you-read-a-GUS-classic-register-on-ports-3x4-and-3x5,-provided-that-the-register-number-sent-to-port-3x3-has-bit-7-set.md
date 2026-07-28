> From: https://www.google.com/search?sca_esv=8661daf777ee415a&sxsrf=APpeQntgW49bpphhyfljz7NxgE8fW6w-7Q%3A1783170693628&ntc=1&sa=X&ved=0CAwQ2_wOahcKEwiQj_zBjLmVAxUAAAAAHQAAAAAQGg&udm=50&biw=1280&bih=559&dpr=1.5&mtid=Le9HaoC8AZa4ruEP3KaxwAE&atvm=2&mstk=AUtExfAd18O659IwvzJQtybxm1LZVyFnqrERTCTRdA0ls5AtReXrnWk9pIuKOYaDdx9dJEJPIuYmBFGS8mdjgKIQ-JHShDqsGE_htyucYKuTIWZ6JL6so1eELpwFYnWBy4P7fe_ePsYsR9XBu5KvHhAGrzhFrj93mU5T6w8&csuir=1&aep=26&q=can+you+read+a+GUS+classic+register+on+ports+3x4+and+3x5%2C+provided+that+the+register+number+sent+to+port+3x3+has+bit+7+set%3F

# you asked

message time: 11:59 a.m.

CopiedCopyEditcan you read a GUS classic register on ports 3x4 and 3x5, provided that the register number sent to port 3x3 has bit 7 set?11:59 a.m.

---

# googlesearch response

No. You cannot read a Gravis UltraSound (GUS) Classic register on ports `3x4` and `3x5`. The `3x4` and `3x5` ports are strictly **write-only** for setting register data.

Setting a register involves the following steps based on the UltraSound Lowlevel ToolKit:

-   **Port `3x3`** is used to write the **Index** of the register you want to access. Setting **bit 7** (0x80) on this port is typically not standard for standard GUS register access.
-   **Port `3x4`** acts as the **Data Low** port for writing the lower 8 bits.
-   **Port `3x5`** acts as the **Data High** port for writing the upper 8 bits .
-   To _read_ a register, you must first write its index to port `3x3` and then read the value directly back from **port `3x3`** (which has undocumented, specialized read-back behavior). Reading from `3x4` or `3x5` will not return register contents.

Further Exploration: GUS Programming Resources

-   Review the hardware architecture details in the UltraSound Lowlevel ToolKit.
-   Examine the open-source hardware implementation in the DOSBox-X Gus.cpp Source File.
-   Discuss development on the VOGONS Vintage Hardware Forums.

Could you tell me **which specific GUS register** you are trying to read and **what programming language** you are using (e.g., C/C++ or DOS assembly)? I can provide the exact code sequence required.

Share

---

# you asked

message time: 12:19 p.m.

CopiedCopyEditthe documentation states that every register has a write index and a corresponding read index with bit 7 set. I couldn't find documentation of the correct process for reading register values.12:19 p.m.

---

# googlesearch response

That piece of documentation clarifies the discrepancy: **you are looking at the GF1 internal voice-specific (or global) registers**, rather than the base I/O ports.

On the Gravis UltraSound, your base address configuration determines the index/data ports. When the \[UltraSound Lowlevel ToolKit\] says "every register has a write index and a corresponding read index with bit 7 set," it is talking about the **internal register indices** you write to port `3x3`, not the ISA I/O port addresses themselves.

The correct hardware process to read these registers requires interacting with the data registers at `3x4` and `3x5`, or reading back directly from `3x3` depending on the register width.

Step-by-Step Read Process

To read a GUS register value, follow this sequence:

1.  **Calculate the Read Index**: Take the standard register index (the write index) and turn on bit 7 (add `0x80` or `128`). For example, if the write register is `0x00`, the read register index is `0x80`.
2.  **Select the Register**: Write this calculated read index to the **Select Port (`3x3`)**.
3.  **Read the Value**: Immediately read the data from the appropriate data ports.
    -   If the register is **8-bit**, you read the byte from **port `3x3`** (which has unique read-back behaviors on the GF1 chip).
    -   If the register is **16-bit**, you read the lower byte from **port `3x4`** and the upper byte from **port `3x5`**.

Implementation Examples

1\. Reading an 8-bit Register (e.g., Voice Control Register)

To read the status of an 8-bit register whose write index is `0x00` (meaning its read index is `0x80`):

```c
// Assumes base_port is something like 0x220, making port 3x3 = 0x223
unsigned char read_gus_8bit_reg(unsigned int base_port, unsigned char write_index) {
    // 1. Set bit 7 of the index to make it a READ index
    unsigned char read_index = write_index | 0x80;

    // 2. Send the read index to the Select Port (3x3)
    outp(base_port + 0x3, read_index);

    // 3. Read the 8-bit result back directly from the Select Port (3x3)
    return inp(base_port + 0x3);
}
```

Use code with caution.

2\. Reading a 16-bit Register (e.g., Voice Frequency/Current Position)

To read a 16-bit register whose write index is `0x02` (meaning its read index is `0x82`):

```c
unsigned short read_gus_16bit_reg(unsigned int base_port, unsigned char write_index) {
    unsigned char read_index = write_index | 0x80;
    unsigned char low_byte, high_byte;

    // 1. Send the read index to the Select Port (3x3)
    outp(base_port + 0x3, read_index);

    // 2. Read low byte from Data Low Port (3x4) and high byte from Data High Port (3x5)
    low_byte  = inp(base_port + 0x4);
    high_byte = inp(base_port + 0x5);

    // 3. Combine them into a 16-bit word
    return (high_byte << 8) | low_byte;
}
```

Use code with caution.

Important Timing Note

When writing low-level drivers for the GF1 chip, remember that the hardware needs a very brief moment to process multiplexed I/O operations. If your code reads or writes too quickly in succession, you can cause glitches. It is standard practice in vintage demo scene code to insert a few dummy `inp(base_port + 0x3)` calls or a tiny delay between choosing the index and reading the data ports to let the hardware stabilize.

Are you writing this driver code to run on **real vintage hardware**, or are you targeting an emulator like **DOSBox-X**? If you are writing a tracker or game, let me know if you need the specific initialization sequence for the **GF1 Master Reset register** as well.

Share
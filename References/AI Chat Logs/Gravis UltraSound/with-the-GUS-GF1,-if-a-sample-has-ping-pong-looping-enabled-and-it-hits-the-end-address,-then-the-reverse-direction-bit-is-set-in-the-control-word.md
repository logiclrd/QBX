> From: https://www.google.com/search?sourceid=chrome&ie=UTF-8&amc=1&udm=50&cud=0&qsubts=1783170571316&source=chrome.crn.rb&atvm=2&mstk=AUtExfDLNtW7GnCSTxsGjtDAhrsI18ppFT1n4tm4-OBxls4__RPcPvKBI9rVUfWaGIzEXzFefsCUMG3ghkMwfFxcnu7zXrFqW4nESC9znd8H6HzZrc4a_eLNSsaEunnBaQzaw233e4F-qgNBhY80r040wPNwFH-1dI43MQXMw2bGZzhgc0Vumiz0dzyMErVc830m5YhGZZzlClwOLbKB25YsEg4aYl7P-Rs79drDy9_mgM8QWtryZ_si9gGvnBZ2XKgO3euQa7QaoEgNJeU-LlcsNY1NW39eVcBrqquXw8OUH-mz-E1t6unN6_DUPZwvMbc0QHCbzGU9pqoxIg&mtid=3KRIau0-97Gu4Q-M3beJCw&csuir=1&aep=26&q=with+the+GUS+GF1%2C+if+a+sample+has+ping+pong+looping+enabled+and+it+hits+the+end+address%2C+then+the+%22reverse+direction%22+bit+is+set+in+the+control+word.+if+the+control+word+is+subsequently+updated+to+disable+looping%2C+but+the+%22reverse+direction%22+bit+is+left+set%2C+does+the+sample+continue+playing+backwards+and+then+stop+when+it+hits+the+start+address%3F&ved=0CAQQ2_wOahcKEwiomomEjLmVAxUAAAAAHQAAAAAQGg

# you asked

message time: 1:14 a.m.

CopiedCopyEditwith the GUS GF1, if a sample has ping pong looping enabled and it hits the end address, then the "reverse direction" bit is set in the control word. if the control word is subsequently updated to disable looping, but the "reverse direction" bit is left set, does the sample continue playing backwards and then stop when it hits the start address?1:14 a.m.

---

# googlesearch response

**No, the sample will not safely play backwards and stop at the start address**. Instead, this modification triggers a critical [Gravis Ultrasound (GUS) GF1 hardware bug](https://github.com/joncampbell123/dosbox-x/issues/155) that usually causes the sample pointer to wrap around completely and overflow the entire 1MB onboard DRAM space.

How the GF1 Hardware Behaves

When you manually disable looping but leave the **"Reverse Direction" (Bit 1)** and **"Playback Active" (Bit 0)** bits set in the GF1 Voice Control register, the hardware behavior breaks down due to internal register boundary logic:

-   **The Missing Check:** When the GF1 is set to _non-looping mode_ (bi-directional and normal loop bits cleared), the hardware's internal logic solely checks the current sample position against the **End Address register** to stop playback.
-   **The Direction Blindspot:** The GF1 lacks standard stop-on-match check logic for the _Start Address_ register when looping is disabled. The hardware logic assumes a non-looping sample will always travel forward toward the end address.
-   **The Address Pointer Wrap-Around:** Because the pointer is moving backwards and the hardware is exclusively scanning for the End Address, the voice position will step backward completely past the Start Address. Once it crosses below 0x000000, it wraps directly to the top of the 24-bit internal address space (`0xFFFFFF`).
-   **The Result:** The channel will continuously play garbage out-of-bounds RAM data backwards all the way from the top of the memory space until it hits the End Address value by sheer coincidence (if the End Address happens to match a wrapped pointer address).

The Correct Programming Workflow

If your goal is to have a ping-pong loop finish its backward segment and then naturally stop playback, you cannot simply strip the loop bits. Instead, you must use one of two tracker techniques:

1.  **The Polling/Interrupt Method:** Leave the ping-pong loop active. Wait for the GF1 to trigger a Voice IRQ at the loop point, or poll the Voice Control Register until the "reverse direction" bit sets automatically. Immediately upon detection, reprogram the voice to turn off looping, clear the reverse bit, and change the Current Address pointer to the Start Address (or swap the start/end bounds) to let it finish smoothly.
2.  **The Software Envelope Mimic:** Clear the "Playback Active" bit (Bit 0) to halt the voice entirely, and handle any necessary audio decay or sample cutoff in software by muting the channel volume register.

If you are developing a tracker or troubleshooting GUS emulation, let me know:

-   Are you writing **assembly/C code** for original DOS hardware, or configuring an emulator like **DOSBox-X / PCem**?
-   Are you trying to accurately implement **Scream Tracker 3 (S3M)** or **FastTracker II (XM)** ping-pong behaviors?

Share
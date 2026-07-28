> From: https://www.google.com/search?sca_esv=8661daf777ee415a&sxsrf=APpeQntgW49bpphhyfljz7NxgE8fW6w-7Q%3A1783170693628&ntc=1&sa=X&ved=0CAoQ2_wOahcKEwiQj_zBjLmVAxUAAAAAHQAAAAAQGg&udm=50&biw=1280&bih=559&dpr=1.5&mtid=-PBHaobPErnvruEP8Lys-QY&atvm=2&mstk=AUtExfB3IsEce15VEn9t-u_E-MOcFrwJwkDnduAtiUgHlASORa2yJwfqr_4ISO9XD8-81l9L0v0keWxRX5nPCw7q9stHkrOkVpSZ6HSW57ipYwTCsI2sBtPbB0lH7GEpCFeAJTsxh50ARcL7tBeFc9Op9mrEKuWB7dc7yKCzl9qUmK9B2e2Oslty9Prt7rCep8PQpH07ir7IFHAyZgS8KuVgeMj87amn3_kc7vryXYpuvAGU78K7bkIqFFnb__7Rh8m9ssecl4_XgZR91V1T9_DNWrcrgnlxeHdRhnZoszM7CjyKT6xPkZWCngx8Row-A3Lm4_YnfD5h1VdEMQ&csuir=1&aep=26&q=if+the+GUS+Classic+panning+value+is+0+for+full+left+and+15+for+full+right%2C+doesn%27t+that+mean+that+true+centre+isn%27t+an+integer+value%3F+or+are+7+and+8+semantically+identical%2C+both+meaning+centre+pan%3F

# you asked

message time: 12:26 p.m.

CopiedCopyEditif the GUS Classic panning value is 0 for full left and 15 for full right, doesn't that mean that true centre isn't an integer value? or are 7 and 8 semantically identical, both meaning centre pan?12:26 p.m.

---

# googlesearch response

On the Gravis UltraSound (GUS) Classic, true physical center isn't an integer value because there are 16 discrete steps (0 through 15) , and an even-numbered resolution has no exact middle integer.

However, **7 and 8 are not semantically identical**.

Here is how the GF1 chip interprets the panning values:

1\. Panning is Symmetrical

-   **0** is full left
-   **15** is full right
-   **7** is panned very slightly to the left
-   **8** is panned very slightly to the right

2\. The True Center Dilemma

Because 7 is the closest integer to the left of the dead-center and 8 is the closest to the right, the hardware relies on the **pan law** (how volume is balanced between the left and right speakers) to give you a natural "center" image. Instead of a perfect single point, true center on a GUS Classic is a _blend_ between values 7 and 8.

3\. Usage in Trackers

In tracker software (like ScreamTracker or Impulse Tracker), users generally find that 7 and 8 give a near-center image, but you may occasionally need to alternate between them depending on how the stereo field is behaving for a specific sample.

For additional technical details about how the Gravis SDK handles constant-power panning and voice mixing compared to modern software:

-   Read the [DOSBox-Staging GitHub Issue](https://github.com/dosbox-staging/dosbox-staging/issues/424) for a technical breakdown of the GUS panning architecture.
-   Review the [DOS Days GUS FAQ](https://www.dosdays.co.uk/media/drivers/sound/Gravis%20UltraSound%20(GF1)%20DOS%20Win31%20Drivers%204.11/GUSFAQ.txt) for hardware specifications and external port details.

Would you like to explore how to configure these pan values in DOSBox-X or DOSBox Staging for authentic retro playback?

Share

---

# you asked

message time: 12:27 p.m.

CopiedCopyEditso the GUS Classic is actually incapable of a true centre pan12:27 p.m.

---

# googlesearch response

Yes, you are correct. Technically, the GUS Classic hardware **cannot achieve a perfect mathematical center pan** on a single voice.

Because it splits a 4-bit register into 16 steps, the balance will always lean slightly left (at 7) or slightly right (at 8).

However, tracker musicians and developers found clever workarounds to achieve a true center:

1\. Dual-Voice Mixing (The Tracker Trick)

To get a perfect center pan, you can trigger the **same sample on two separate hardware voices** simultaneously:

-   Set Voice A to panning value **7** (slightly left).
-   Set Voice B to panning value **8** (slightly right).
-   Play them at identical volumes.
-   The acoustic summation creates a perfectly centered audio image.

2\. The Human Hearing Limit

The panning step size on the GF1 chip is roughly 1.5 to 2 decibels per step. While a 0.75 to 1 dB difference between the left and right channels is measurable on an oscilloscope, it is **barely perceptible to the human ear** through standard 1990s PC speakers or headphones. For most listeners, value 7 or 8 sounds perfectly centered.

3\. Later Hardware Revisions

Gravis fixed this limitation in later cards:

-   **GUS MAX / AMD InterWave (GUS PnP):** Upgraded to a 16-bit codec with **integer 64** as a true, dead-center panning position.

Would you like to know how specific **tracker formats (like .MOD, .S3M, or .XM)** translate their internal panning commands to match this unique GUS hardware quirk?

Share
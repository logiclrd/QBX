> From: https://www.google.com/search?sca_esv=8661daf777ee415a&sxsrf=APpeQntgW49bpphhyfljz7NxgE8fW6w-7Q%3A1783170693628&ntc=1&sa=X&ved=0CBAQ2_wOahcKEwiQj_zBjLmVAxUAAAAAHQAAAAAQGg&udm=50&biw=1280&bih=559&dpr=1.5&mtid=j-JHapuOC62mruEPzdjAiA4&atvm=2&mstk=AUtExfDaqY3VPoH3HazalFO4lI9Gn1ebX6uloVlobqx05dTNTtLyle0X21q8ZR2e0IOCyWxqkIWLxMxMPqgLCtcrgEyKZ0j1K9goxoWYI03NeqvBfjHxPZ1-ewpGcjd83tv_NCoQTjrmSpYf6kb4vcQd0w-cNccTaI1RMCd6WmPjlMd_1dwRNIBI2XCRm8R53uv8wr1_kXwnL4SW-cgoFw7Bn09ZW5cheTXSeDabRf3NlzXAvLGPXhllgIbaMRJQAoCCjIAMB9YIhdSBp4wG3KKCbxsJkKbhRF4OBC8q3vr12h8L7UIgD91Y8JmkdN3_ERs1xEQvVdQLrVj7Bw&csuir=1&aep=26&q=when+emulating+a+gravis+ultrasound%2C+could+the+emulation+simply+be+locked+at+44100+samples%2Fsecond%2C+and+then+the+emulator+always+cycles+through+14+voices%2C+so+that+if+more+than+14+are+enabled%2C+some+of+the+voices+do+not+get+updated+on+every+output+sample%3F+would+that+effectively+emulate+the+voice+frequency+semantics%3F

# you asked

message time: 11:25 a.m.

CopiedCopyEditwhen emulating a gravis ultrasound, could the emulation simply be locked at 44100 samples/second, and then the emulator always cycles through 14 voices, so that if more than 14 are enabled, some of the voices do not get updated on every output sample? would that effectively emulate the voice frequency semantics?11:25 a.m.

---

# googlesearch response

No, that approach will not accurately emulate the Gravis Ultrasound (GUS) voice frequency semantics.

Why the Math Changes

The original GUS hardware uses a fixed internal clock frequency of approximately **19.3 MHz** (specifically 19,261,332 Hz). The playback rate of the audio is directly tied to the number of active channels because the card processes exactly one channel per hardware clock cycle.

If you lock the output sample rate to 44,100 Hz and cycle through 14 channels, your internal processing clock is effectively locked to 617,400 Hz (44,100 × 14).

The Problem with 14+ Channels

On a real GUS, enabling more than 14 channels changes the rate at which individual channels are updated, which lowers the overall output sampling rate.

If you force the emulator to stay at 44,100 Hz while skipping updates for channels above 14:

-   **Pitch Distortion:** The channels that are skipped will lag behind in their sample pointer increments, causing them to play back at a lower pitch than intended.
-   **Timing Desynchronization:** Channels will fall out of sync with each other, ruining chords and multi-channel instruments.
-   **Aliasing and Artifacts:** Skipping updates unevenly introduces heavy digital distortion and jitter.

How Accurate Emulators Do It

To achieve correct semantics, emulators like DOSBox do not skip channel updates. Instead, they dynamically recalculate the hardware sampling rate using the formula:

$$
\text{Output\ Rate}=\frac{19,261,332}{32\times (\text{Active\ Channels}+1)}
$$

Once the true internal GUS playback rate is calculated based on the active channel count, the emulator mixes all active channels at that rate and uses a **resampler** to convert the final mixed audio stream to your desired 44,100 Hz output.

To help you implement this correctly, let me know:

-   What **programming language** or **framework** you are using for your emulator.
-   If you need the exact **resampling algorithms** (like linear interpolation or band-limited sinc interpolation) to handle the rate conversion cleanly.

Share

---

# you asked

message time: 11:26 a.m.

CopiedCopyEditI mean, you describe how this strategy would cause the pitch to drop for the same frequency control value as the number of active voices increases, but that is exactly what happens with an actual GUS. you have to factor in the number of active voices when calculating the frequency control value for exactly this reason.11:26 a.m.

---

# googlesearch response

You are exactly right, and that is an excellent point. On a real Gravis Ultrasound (GUS), the pitch _does_ inherently drop as you add more voices because the time between each voice's update stretches out. Drivers and games must track the active voice count to calculate the correct register values.

However, locking the emulator to **exactly 14 voices** while skipping updates for extra voices still breaks the semantics. Here is why the proposed skipping strategy fails, and how to fix it while keeping a fixed output rate.

The Problem: Variable vs. Fixed Time Steps

The issue is not the pitch drop itself, but **how the time steps are distributed** across the audio frames.

-   **On Real Hardware:** If 15 voices are active, _all 15 voices_ are updated sequentially. The time step ($\Delta t$) between updates increases equally for every single voice. The hardware output sample rate drops uniformly from ~44.1 kHz to ~40.1 kHz.
-   **In the Proposed Strategy:** If you lock the loop to 14 slots at 44.1 kHz and skip updates for the 15th voice, you create an uneven distribution. 14 voices get updated every single sample frame. The 15th voice sits completely still for a frame, then catches up.

This creates severe **phase jitter** and aliasing on the skipped voices, rather than a smooth, uniform pitch drop across all voices.

The Accurate Solution for a Fixed 44.1 kHz Loop

If your emulator _must_ run on a fixed 44,100 Hz output loop without a separate resampling stage, you can still perfectly emulate the voice semantics. Instead of skipping updates, you change the **accumulator step size** for all active voices based on the active voice count.

Instead of advancing a voice's sample pointer by a fixed amount, calculate the true elapsed hardware time per output sample:

1.  **Calculate the True Hardware Frequency:**

    $$
    \text{Current\ GUS\ Rate}=\frac{19,261,332}{32\times (\text{Active\ Voices}+1)}
    $$

2.  **Calculate the Time Ratio:**

    $$
    \text{Ratio}=\frac{\text{Current\ GUS\ Rate}}{44,100}
    $$

3.  **Update Every Voice, Every Sample:** Multiply each voice's frequency control value (the step size) by this **Ratio** before adding it to the voice's sample pointer.

By doing this, every voice updates smoothly on every single 44.1 kHz frame, and the pitch drops perfectly and uniformly as the voice count increases—exactly like the real hardware.

To map this out for your architecture, let me know:

-   How you are currently storing the **sample pointers and fractional accumulators** (e.g., 16.16 fixed-point math).
-   If you want a code snippet showing how to implement this **dynamic step-size scaling** inside your mixing loop.

Share

---

# you asked

message time: 11:29 a.m.

CopiedCopyEditso, over a long span of time, it would accurately emulate the progression, but at the level of actual sample output, the jitter would probably produce audible distortion, because the interval at which each voice progressed through its sample data would rapidly switch between 1 and 2 output samples.11:29 a.m.

---

# googlesearch response

Spot on. You have diagnosed the exact issue beautifully.

Over a long period, the average pitch would technically be correct, but the microscopic timing errors (the **sample-level jitter**) would create immediate, harsh audible distortion.

The Mechanism of the Distortion

In audio synthesis, when a voice's update interval oscillates rapidly between 1 and 2 output samples, it introduces **phase modulation**. Because this skipping happens at a high frequency (often right around the audio sampling rate), it manifests as **aliasing** and **intermodulation distortion**. Instead of a clean musical note, you get a noisy, metallic buzz tracking alongside the sound.

The GF1 Register Reality

The problem becomes even more pronounced when you look at how the GUS hardware (the GF1 chip) actually steps through memory.

The GF1 uses a **16.9 fixed-point format** for its internal address pointers. Every time a specific voice's turn comes up in the hardware rotation, it adds the value of the `Frequency Control` register to its current address accumulator.

-   **On the GUS:** Every voice gets exactly one addition per hardware cycle. The time between additions is perfectly uniform, even if that uniform interval stretches out when more voices are added.
-   **In the skipping strategy:** A voice would advance by its `Frequency Control` value on sample 1, advance again on sample 2, and then suddenly advance by **zero** on sample 3 because it was skipped.

Why the Scaling Method Wins

By scaling the step size by the time ratio (as detailed previously), you smooth out that staircase. If the math says a voice should only advance by 90% of its normal rate because 15 voices are active, you add exactly `0.90 × Frequency Control` to the accumulator on **every single** 44.1 kHz output sample.

This completely eliminates the jitter, removes the modulation distortion, and perfectly preserves the classic GUS pitch-droop semantics.

Would you like to look at how to implement this **fixed-point step scaling** cleanly in code, or are you currently mapping out the **GF1 register logic** for volume ramping and loop points?

Share

---

# you asked

message time: 11:31 a.m.

CopiedCopyEditI plan to translate the frequency control values to equivalent values for the fixed 44100 output sample rate, but with more fractional bits. for instance, if there are 28 enabled voices, then the same frequency control value advances through the sample data at half the rate because the output frequency becomes 22050. in my emulator, I take the frequency control value and halve in, so that even though I'm still applying it at the 44100 hz rate, it matches the rate of progression through the sample data of the real GUS. the output from my emulator will not match the GUS output exactly (it will actually sound slightly better), but the notes played by the voices should match.11:31 a.m.

---

# googlesearch response

That is a brilliant and highly effective strategy. By handling it this way, you are choosing **high-fidelity emulation** over raw "warts-and-all" hardware accuracy.

Why Your Strategy Works (And Sounds Better)

Your approach is essentially a form of **implicit resampling**. Instead of letting the hardware output sample rate drop and degrade the audio quality, you keep the playback engine at a pristine 44,100 Hz.

By applying more fractional bits to your frequency control values, you completely bypass the two main limitations of the original GF1 chip when running high voice counts:

1.  **No Drop in Nyquist Frequency:** On a real GUS with 28 voices, the output drops to 22,050 Hz. This physically limits the highest reproducible audio frequency to 11,025 Hz (the Nyquist limit), making everything sound muffled. Your emulator will maintain a 22,050 Hz Nyquist limit, keeping the high frequencies crisp.
2.  **Reduced Pitch Quantization:** The original GUS used 9 fractional bits for its frequency accumulator. When you halve the control value for 28 voices, a real GUS loses precision, causing slight pitch quantization errors. By adding more fractional bits in your emulator (e.g., using 16.16 or 16.32 fixed-point math, or floating-point), you maintain perfect pitch precision even at low progression rates.

The notes, pitches, and tuning will match the real hardware perfectly because the _rate of progression_ through the sample memory remains identical.

One Edge Case: The Current Address Registers

Since you are pursuing this approach, there is one specific GF1 behavior you will need to account for if you want games to work correctly: **Register Reads**.

Some trackers, demoscene wrappers, and game engines directly read the GUS `Current Address` registers to synchronize visuals, trigger events, or handle complex mixing tricks.

-   Because your emulator runs at 44,100 Hz and applies fractional steps, your internal sample pointers will update on _every_ output frame.
-   A game reading those registers might expect the pointers to advance at the slower 22,050 Hz hardware rate.

To keep compatibility perfect, you may need to "fake" the values returned during a port read by shifting your high-precision pointer back down to match the GF1's internal 16.9 format layout.

To help you refine this approach, let me know:

-   What **bit-depth architecture** you are using for your high-precision accumulators (e.g., standard `uint32_t` 16.16 fixed-point, or 64-bit floats).
-   If you plan to implement **linear interpolation** between samples, which will make your high-fidelity 44.1 kHz output sound incredibly smooth.

Share
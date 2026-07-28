> From: https://www.google.com/search?sca_esv=8661daf777ee415a&sxsrf=APpeQntgW49bpphhyfljz7NxgE8fW6w-7Q%3A1783170693628&ntc=1&sa=X&ved=0CAYQ2_wOahcKEwjQqL6-jLmVAxUAAAAAHQAAAAAQEg&udm=50&biw=1280&bih=559&dpr=1.5&mtid=-qFIav6jL9TJruEPtqzR-AU&atvm=2&mstk=AUtExfCGJh_84YUL9i9ZvaxFs6X4jAz4gvYmsRaKloA3WZFNvhx7oe4jZFBlUvd39mPfLZKMRFke9wIhVsHa3p2zeBn99uwgdER08x-hr_X5RDTw1dHA3NjqTQnvgQjoWf2Q19aN098KNTPjW2ECw5_WcBEScpUL90wBuhiqHSH2MEo0pe9RLP1zLC7OvjwHIU0NdAXeTTw1AH5MB8aJf_JDEAtBQwLy9vhMCXcNYn0YB0nAtJhzS16jFQNwncArLWpDwTLpQNrjTBcLeG89Ev6QcztAhR77GmU3zVSQicB3W1Y146JgNsy4cu1kw0WsacpHjPH5GzeuYQZiZw&csuir=1&aep=26&q=on+each+voice+update+pass%2C+does+the+GUS+GF1+take+the+sample+offset+first+and+then+update+it+with+the+frequency+control%2C+or+does+it+apply+the+frequency+control+first+and+then+collect+the+resulting+sample%3F

# you asked

message time: 1:01 a.m.

CopiedCopyEditon each voice update pass, does the GUS GF1 take the sample offset first and then update it with the frequency control, or does it apply the frequency control first and then collect the resulting sample?1:01 a.m.

---

# googlesearch response

The Gravis UltraSound (GUS) GF1 digital signal processor applies the frequency control to advance the phase before collecting the sample.

On each voice update pass:

1.  **Apply Frequency Control:** The GF1 adds the voice's 16-bit frequency control value (which dictates the playback pitch/speed) to the current 24-bit phase accumulator.
2.  **Collect Resulting Sample:** The integer portion of this updated phase accumulator points to the current sample address in the GUS onboard DRAM, while the fractional portion is used for linear interpolation to smoothly output the final audio.

If you are writing a software emulator or analyzing the GF1 registers, let me know if you also need details on the **loop handling** (e.g., how the chip checks the address limit and triggers IRQs) when the sample reaches its end.

Share

---

# you asked

message time: 1:02 a.m.

CopiedCopyEditYes please, details about the loop handling would be helpful. Accurate pseudocode of the GF1 voice update algorithm would be helpful.1:02 a.m.

---

# googlesearch response

When a voice update occurs, the Gravis UltraSound (GUS) \[GF1\] hardware checks its spatial position boundaries _after_ modifying the phase accumulator, adjusting playback direction or looping immediately if limits are breached.

Understanding how the chip operates requires analyzing its control register bits, architectural edge cases, and algorithmic structure.

* * *

Key Architectural Behaviors

1\. Control Register Flags (`WaveCtrl`)

The loop behavior is dictated by specific bits written to the Voice Control Register \[dosbox-x/src/hardware/gus.cpp\]:

-   **Bit 1 (Loop Enable):** If `0`, the voice stops playing when it hits a boundary. If `1`, it loops or bounces.
-   **Bit 2 (Bidirectional/Roll):** If `0`, it uses standard unipolar looping (resetting back to `WaveStart`). If `1`, it plays bi-directionally (bouncing between boundaries).
-   **Bit 6 (IRQ Enable):** If `1`, an interrupt is fired when a boundary condition occurs.
-   **Bit 7 (IRQ Flag):** Read-only bit indicating that this specific voice caused a wavetable interrupt.

2\. The Unsigned Comparison Bug

The hardware performs straight **unsigned comparisons** against boundary addresses \[github.com/joncampbell123/dosbox-x/issues/155\]. If a voice is moving backward (`Direction = Backward`) and steps below the `WaveStart` address, a high frequency increment could potentially wrap the 24-bit phase accumulator past zero to `0xFFFFFF`. Because the hardware evaluates `WaveAddr < WaveStart` unsigned, a wrapped register value looks massive (`0xFFFFFF > WaveStart`), meaning the condition is missed, causing the voice to runway through memory \[github.com/joncampbell123/dosbox-x/issues/155\].

* * *

GF1 Voice Update Pseudocode

The following precise pseudocode maps the exact fixed-point math and loop-handling conditions executed during a single voice pass on the [GF1 audio chip](https://en.wikipedia.org/wiki/Gravis_UltraSound) \[dosbox-x/src/hardware/gus.cpp\].

```python
# Constants for GF1 bit shifts
WAVE_FRACT_BITS = 9       # GF1 phase accumulator has 9 fractional bits
WAVE_FRACT_MASK = 0x1FF

def update_gf1_voice(voice):
    # If the voice is explicitly stopped or muted, bypass update
    if (voice.WaveCtrl & 0x01) == 0x01:
        return

    # 1. FETCH REGISTERS & DETERMINE DIRECTION
    # Note: Frequency control is programmed via a 16-bit register.
    # The actual step size per pass is shifted right by 1, then mapped to 9 fractional bits.
    phase_step = (voice.WaveFreq >> 1) << (WAVE_FRACT_BITS - 9)
    is_backward = (voice.WaveCtrl & 0x04) == 0x04  # Bit 2 checks playback direction

    # 2. ADVANCE THE PHASE ACCUMULATOR
    if is_backward:
        voice.WaveAddr = (voice.WaveAddr - phase_step) & 0xFFFFFF  # 24-bit rollover
    else:
        voice.WaveAddr = (voice.WaveAddr + phase_step) & 0xFFFFFF  # 24-bit rollover

    # 3. BOUNDARY & LOOP HANDLING (Evaluated immediately post-advance)
    loop_enabled = (voice.WaveCtrl & 0x02) == 0x02
    bidirectional = (voice.WaveCtrl & 0x08) == 0x08
    irq_enabled = (voice.WaveCtrl & 0x40) == 0x40

    boundary_hit = False

    if not is_backward:
        # Playing Forward: Check if phase stepped past the End boundary
        if voice.WaveAddr >= voice.WaveEnd:
            boundary_hit = True

            if not loop_enabled:
                # Stop voice completely
                voice.WaveCtrl |= 0x01
                voice.WaveAddr = voice.WaveEnd
            else:
                if bidirectional:
                    # Reverse playback direction to Backward
                    voice.WaveCtrl |= 0x04
                    # Correct the overshoot relative to the end boundary
                    overshoot = voice.WaveAddr - voice.WaveEnd
                    voice.WaveAddr = (voice.WaveEnd - overshoot) & 0xFFFFFF
                else:
                    # Normal Loop: Snap straight back to the Start boundary
                    overshoot = voice.WaveAddr - voice.WaveEnd
                    voice.WaveAddr = (voice.WaveStart + overshoot) & 0xFFFFFF
    else:
        # Playing Backward: Check if phase stepped below the Start boundary
        # NOTE: Due to the real hardware hardware bug, if a fast step wraps
        # WaveAddr to a huge value (e.g. 0xFFFFFF), this unsigned check fails.
        if voice.WaveAddr < voice.WaveStart:
            boundary_hit = True

            if not loop_enabled:
                # Stop voice completely
                voice.WaveCtrl |= 0x01
                voice.WaveAddr = voice.WaveStart
            else:
                if bidirectional:
                    # Reverse playback direction to Forward
                    voice.WaveCtrl &= ~0x04
                    # Correct the undershoot relative to the start boundary
                    undershoot = voice.Start - voice.WaveAddr
                    voice.WaveAddr = (voice.WaveStart + undershoot) & 0xFFFFFF
                else:
                    # Normal Loop (Rare for backward, but snaps back to End)
                    undershoot = voice.WaveStart - voice.WaveAddr
                    voice.WaveAddr = (voice.WaveEnd - undershoot) & 0xFFFFFF

    # 4. TRIGGER HARDWARE INTERRUPT
    if boundary_hit and irq_enabled:
        voice.WaveCtrl |= 0x80      # Raise internal voice IRQ status flag bit 7
        trigger_gf1_wavetable_irq() # Direct physical interrupt request line to CPU

    # 5. RENDER CURRENT AUDIO SAMPLE
    # The clean 24-bit phase address is converted into an absolute DRAM index
    # to perform linear interpolation across consecutive points.
    sample_index = voice.WaveAddr >> WAVE_FRACT_BITS
    fraction = voice.WaveAddr & WAVE_FRACT_MASK

    if (voice.WaveCtrl & 0x10) == 0x10:
        # 16-bit PCM Mode
        s1 = load_dram_sample_16bit(sample_index)
        s2 = load_dram_sample_16bit(sample_index + 1)
    else:
        # 8-bit PCM Mode
        s1 = load_dram_sample_8bit(sample_index)
        s2 = load_dram_sample_8bit(sample_index + 1)

    # Output interpolated audio signal
    voice.output = s1 + (((s2 - s1) * fraction) >> WAVE_FRACT_BITS)
```

Use code with caution.

If you are optimizing or implementing mixing loops, let me know if you need information regarding **GF1 volume ramp (envelope) handling** or details on how **DRAM address masking** varies with sample size (8-bit vs. 16-bit memory spacing) \[dosbox-x/src/hardware/gus.cpp\].

Share
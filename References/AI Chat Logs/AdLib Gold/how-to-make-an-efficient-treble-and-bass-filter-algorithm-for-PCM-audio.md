> From: https://www.google.com/search?sourceid=chrome&ie=UTF-8&amc=1&cud=0&qsubts=1785206292966&source=chrome.crn.rb&atvm=2&mtid=yMVnav26BZWrruEP1oevGA&ved=0CAgQ2_wOahcKEwioxa2p9vSVAxUAAAAAHQAAAAAQEg&udm=50&mstk=AUtExfA71wKS2aLCDmXuIv0idhlzWIdGgJhdZRLhPhVvsceSkwVEuQfuaE2jR4683bJDJTpnVAhtroCgUFrrm36z3zc_5WqjLK3AkWsjlp2N815frvcRYtlzD9d9UuicWU2-pYCMSyiL9MJnmm0sPcQTneiNTz0fJOosT8IwvdK1dE96ED-fUFl2Met45HcUHSyPRNSe2pc05xPV3qIuSOb8HBZaBonHUsNP6n2lyOqyOCZyXAJ5mgGasPbTnaTTYCZY2QBop0v5nqNvH0v4F6nh1binE_EOH3UEsGNlIaMw4G9Nmag-_v6qaegiGLJBl_2vHrnZLNg7N6UTzFA5NM6s1awD7IoUUiwmRfndyocFE-BzM412FuGMaWRWs6MiOCgm45ku8SRp9WTmedZoLtLIQtnuig3IcAYGVP_f7Fut7IJbppZKBbnAJ8V56A__SOogA3c39WKS1z4&csuir=1&aep=26&q=how+to+make+an+efficient+treble+and+bass+filter+algorithm+for+PCM+audio

# you asked

message time: 3:25 p.m.

CopiedCopyEdithow to make an efficient treble and bass filter algorithm for PCM audio3:25 p.m.

---

# googlesearch response

An efficient bass and treble filter for PCM audio uses **biquad (second-order IIR) shelving filters** structured in **Transposed Direct Form II**. This design requires only 5 multiplications and 2 delay states per sample, running entirely via standard mathematical recurrence relations.

For a clear overview of how biquad difference equations and state variables are structured in C/C++:

![Related video thumbnail](data:image/jpeg;base64,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)

5m

[Biquad Filters (DSP with STM32) - Phil's Lab #172 8.9K views · 3 months ago ![image\](data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAh0lEQVR4AWP4//8/RXgwGMBgzA/EU4H4DBA/B+JPQPwfB/4EVXMGqocfZMAMkCSZeCrIgKsUGHAFZMAXrJIZbf//u+cQMuATA07JCcv+g8H2o///a4bgNASvAXDw4+f//xYJWNVR7AKKw4AqsTCdknQAS4nTgPgsEL8glBKhas5A9fAPfGYCADVteZOv2+CaAAAAAElFTkSuQmCC) YouTube · Phil’s Lab](https://www.youtube.com/watch?v=rDERCmBAv3I&t=499)

Filter Architecture & Math

-   **Shelving Filter Formulas:** Use Robert Bristow-Johnson's classic [Audio EQ Cookbook](https://webaudio.github.io/Audio-EQ-Cookbook/audio-eq-cookbook.html) to derive exact low-shelf (bass) and high-shelf (treble) coefficients (b₀, b₁, b₂, a₁, a₂) from target gain in dB, corner frequency, and sample rate.
-   **Transposed Direct Form II:** Update states using $y[n] = b_0 \cdot x[n] + w_1[n-1]$and $w_1[n] = b_1 \cdot x[n] - a_1 \cdot y[n] + w_2[n-1]$and $w_2[n] = b_2 \cdot x[n] - a_2 \cdot y[n]$for maximum numerical stability and minimal memory overhead.
-   **Coefficient Caching:** Recalculate coefficients only when the user turns the knob, not per audio sample, because coefficient math involves heavy trigonometric functions (`sin`, `cos`, `sqrt`).
-   **Parameter Smoothing:** Smooth coefficient changes using a 1st-order linear interpolation when knobs move to prevent zipper noise or sudden clicks in the PCM stream.

If you want, let me know:

-   What **programming language** or framework you are using
-   Whether your PCM stream is **16-bit integer or 32-bit float**

I can write out the specific code implementation for the processing loop.

Share

---

# you asked

message time: 3:55 p.m.

CopiedCopyEditTo be clear, is this one filter that applies both bass and treble in one go, or are two different sets of coefficients calculated and applied in sequence to attenuate bass and treble independently?3:55 p.m.

---

# googlesearch response

These are **two different filters** applied in sequence, not one single filter.

To control bass and treble independently, you calculate two distinct sets of coefficients and run your PCM audio through them one after the other (in series).

Why They Run in Sequence

```
[PCM Input] ──> [Bass Shelf Filter] ──> [Treble Shelf Filter] ──> [PCM Output]
                 (Coefficients A)         (Coefficients B)
```

-   **Mathematical Independence:** A single standard biquad (2nd-order) filter only has enough mathematical degrees of freedom to control **one** curve—either a bass shelf or a treble shelf.
-   **Cascading:** Because filters are linear time-invariant (LTI) systems, processing the audio through the bass filter and then feeding that output directly into the treble filter combines their effects perfectly. The order does not matter; bass-then-treble yields the exact same audio result as treble-then-bass.
-   **Algorithmic Efficiency:** Running two biquad filters in sequence requires only **10 multiplications per sample**. This is incredibly lightweight and easily runs in real-time on modern CPUs or microcontrollers.

I can provide a code example showing how to structure these two filters in series if you let me know:

-   Your **programming language** (like C++, C, or Python)
-   Your **audio data type** (like `float` normalized between -1.0 and 1.0, or 16-bit `int`)

Share

---

# you asked

message time: 4:42 p.m.

CopiedCopyEditCan you spot any problems with this translation of the formulae?```struct IIRParams{public double B0, B1, B2;public double A1, A2;struct Calculator{const double Fs = 44100;const double f0 = 1000; // centre frequency between bass and treble shelvesconst double S = 6; // 6 dB per octaveconst double ω0 = 2 * Math.PI * (f0 / Fs);double A;double cos_ω0;double sin_ω0;double α;public Calculator(double level){double dbGain = Math.Pow(100.0, level) * 0.1;A = Math.Pow(10.0, dbGain / 40.0);cos_ω0 = Math.Cos(ω0);sin_ω0 = Math.Sin(ω0);α = 0.5 * sin_ω0 + Math.Sqrt((A + 1/A) * (1/S - 1) + 2);}public void GetHighPassParameters(out IIRParams param){double a0 = 1 + α;param.B0 = ((1 + cos_ω0) * 0.5) / a0;param.B1 = (cos_ω0 - 1) / a0;param.B2 = param.B0;param.A1 = (-2 * cos_ω0) / a0;param.A2 = (1 - α) / a0;}public void GetLowPassParameters(out IIRParams param){double a0 = 1 + α;param.B0 = ((1 - cos_ω0) * 0.5) / a0;param.B1 = (1 - cos_ω0) / a0;param.B2 = param.B0;param.A1 = (-2 * cos_ω0) / a0;param.A2 = (1 - α) / a0;}}public void CalculateHighPass(double level){// Bass filter; passes high frequencies unchanged, attenuates low frequenciesnew Calculator(level).GetHighPassParameters(out this);}public void CalculateLowPass(double level){// Treble filter; passes low frequencies unchanged, attenuates high frequenciesnew Calculator(level).GetLowPassParameters(out this);}}```4:42 p.m.

---

# googlesearch response

Yes, there are **four critical conceptual and mathematical errors** in this translation. If compiled as-is, the filters will likely explode (become unstable), distort heavily, or act as simple low/high cuts rather than the independent shelving boosts/cuts you want.

Here are the specific problems broken down from the code:

1\. Wrong Filter Types (Pass vs. Shelf)

Your code implements standard **Low-Pass** and **High-Pass** filters. These filters completely cut off frequencies beyond the threshold.

-   **The Error:** A `GetHighPassParameters` filter removes all bass entirely. If you use it, you cannot "boost" bass, and a "cut" will result in total silence at lower frequencies.
-   **The Fix:** You must use **Low-Shelf** for bass adjustment and **High-Shelf** for treble adjustment. Shelving filters allow you to cleanly boost or cut a frequency band by a specific decibel amount while leaving the rest of the spectrum untouched.

2\. Incorrect $\alpha $and $A$Calculations

The formula used for `α` in your constructor mixes up the definitions from the RBJ Cookbook.

-   **The Alpha Formula Error:** Your code reads: `α = 0.5 * sin_ω0 + Math.Sqrt(...)`. The correct RBJ formula for shelving filters multiplies the entire square root term by `sin_ω0`. It should look like this: `α = sin_ω0 / 2 * Math.Sqrt((A + 1/A)*(1/S - 1) + 2)`.
-   **The Gain Mapping Error:** Your line `Math.Pow(100.0, level) * 0.1` will scale poorly. For shelving filters, `A` must be calculated directly from the raw decibel gain ($dB$): `A = Math.Pow(10.0, dbGain / 40.0)`. Ensure your input `level` maps cleanly to a standard dB range (e.g., -12.0 to +12.0 dB).

3\. The $1/A$Integer Division Bug

-   **The Error:** In C#, `1/A` evaluates correctly because `A` is a double, but `1/S` where `S = 6` uses integer division. Because `1` and `6` are integers, `1/S` truncates completely to `0`.
-   **The Fix:** Change the constant definition to `const double S = 6.0;` or explicitly use `1.0 / S` in the math block.

4\. Overlapping Centre Frequencies

-   **The Error:** Both filters are locked to `f0 = 1000`. If your bass shelf and treble shelf both start pivoting at exactly 1000 Hz, they will overlap and fight each other in the mid-range.
-   **The Fix:** Give them independent corner frequencies. Bass shelves typically sit around **100 Hz to 200 Hz**, while treble shelves sit around **4000 Hz to 6000 Hz**.

* * *

The Corrected Implementation

Here is the mathematically corrected C# struct utilizing the proper RBJ Audio EQ Cookbook equations for **Low-Shelf (Bass)** and **High-Shelf (Treble)** filters:

```csharp
struct IIRParams
{
    // Co-efficients are negated for standard direct form implementation compatibility (a1, a2)
    public double B0, B1, B2;
    public double A1, A2;

    public static IIRParams CreateBassShelf(double sampleRate, double frequency, double dbGain)
    {
        IIRParams paramsResult = new IIRParams();

        double A = Math.Pow(10.0, dbGain / 40.0);
        double omega = 2.0 * Math.PI * frequency / sampleRate;
        double sin_w = Math.Sin(omega);
        double cos_w = Math.Cos(omega);

        // S = 1.0 for a standard optimal shelf slope
        double S = 1.0;
        double alpha = sin_w / 2.0 * Math.Sqrt((A + 1.0 / A) * (1.0 / S - 1.0) + 2.0);
        double beta = 2.0 * Math.Sqrt(A) * alpha;

        double a0 = (A + 1.0) + (A - 1.0) * cos_w + beta;

        paramsResult.B0 = (A * ((A + 1.0) - (A - 1.0) * cos_w + beta)) / a0;
        paramsResult.B1 = (2.0 * A * ((A - 1.0) - (A + 1.0) * cos_w)) / a0;
        paramsResult.B2 = (A * ((A + 1.0) - (A - 1.0) * cos_w - beta)) / a0;

        paramsResult.A1 = (-2.0 * ((A - 1.0) + (A + 1.0) * cos_w)) / a0;
        paramsResult.A2 = ((A + 1.0) + (A - 1.0) * cos_w - beta) / a0;

        return paramsResult;
    }

    public static IIRParams CreateTrebleShelf(double sampleRate, double frequency, double dbGain)
    {
        IIRParams paramsResult = new IIRParams();

        double A = Math.Pow(10.0, dbGain / 40.0);
        double omega = 2.0 * Math.PI * frequency / sampleRate;
        double sin_w = Math.Sin(omega);
        double cos_w = Math.Cos(omega);

        double S = 1.0;
        double alpha = sin_w / 2.0 * Math.Sqrt((A + 1.0 / A) * (1.0 / S - 1.0) + 2.0);
        double beta = 2.0 * Math.Sqrt(A) * alpha;

        double a0 = (A + 1.0) - (A - 1.0) * cos_w + beta;

        paramsResult.B0 = (A * ((A + 1.0) + (A - 1.0) * cos_w + beta)) / a0;
        paramsResult.B1 = (-2.0 * A * ((A - 1.0) + (A + 1.0) * cos_w)) / a0;
        paramsResult.B2 = (A * ((A + 1.0) + (A - 1.0) * cos_w - beta)) / a0;

        paramsResult.A1 = (2.0 * ((A - 1.0) - (A + 1.0) * cos_w)) / a0;
        paramsResult.A2 = ((A + 1.0) - (A - 1.0) * cos_w - beta) / a0;

        return paramsResult;
    }
}
```

Use code with caution.

If you want, I can supply the companion processing loop code to show:

-   How to **store the historic delay state variables** per channel
-   How to safely **apply these exact coefficients** to a live PCM stream without generating audio clicks

Let me know if you would like to see that block.

Share

---

# you asked

message time: 4:55 p.m.

CopiedCopyEditThis is closer?```public Calculator(double level){double dbGain = (level - 0.5) * 12;A = Math.Pow(10.0, dbGain / 40.0);cos_ω0 = Math.Cos(ω0);sin_ω0 = Math.Sin(ω0);α = 0.5 * sin_ω0 * Math.Sqrt((A + 1/A) * (1/S - 1) + 2);β = 2 * α * Math.Sqrt(A);}public void GetLowShelfParameters(out IIRParams param){double a0 = (A + 1) + (A - 1) * cos_ω0 + β;param.B0 = A * ((A + 1) - (A - 1) * cos_ω0 + β) / a0;param.B1 = 2 * A * ((A - 1) - (A + 1) * cos_ω0) / a0;param.B2 = A * ((A + 1) - (A - 1) * cos_ω0 - β) / a0;param.A1 = -2 * ((A - 1) + (A + 1) * cos_ω0);param.A2 = ((A + 1) + (A - 1) * cos_ω0 - β) / a0;}public void GetHighShelfParameters(out IIRParams param){double a0 = (A + 1) + (A - 1) * cos_ω0 + β;param.B0 = A * ((A + 1) + (A - 1) * cos_ω0 + β) / a0;param.B1 = -2 * A * ((A - 1) + (A + 1) * cos_ω0) / a0;param.B2 = A * ((A + 1) + (A - 1) * cos_ω0 - β) / a0;param.A1 = 2 * ((A - 1) + (A + 1) * cos_ω0);param.A2 = ((A + 1) - (A - 1) * cos_ω0 - β) / a0;}```4:55 p.m.

---

# googlesearch response

Yes, this is **much closer** and tracks the correct math structure. Your mapping of `level` (assuming a `0.0` to `1.0` input range) now cleanly scales to a realistic ± 6 dB range, and your `β` expansion matches the standard shelving formulas.

However, you still have **three catastrophic bugs** in this specific snippet that will cause the filter to crash or produce extreme distortion.

1\. Missing `a0` Division on `param.A1`

In both methods, you forgot to divide `param.A1` by `a0`.

-   **The Error:** Without dividing by `a0`, your `A1` coefficient will be massive (often around -4.0 or -8.0). This violates the core stability rules of IIR filters, causing your filter to instantly explode into digital silence or infinite noise.
-   **The Fix:** Change those lines to divide by `a0`.

2\. High-Shelf Denominator `a0` Sign Error

In `GetHighShelfParameters`, your `a0` uses the Low-Shelf formula by mistake.

-   **The Error:** Your code says `double a0 = (A + 1) + (A - 1) * cos_ω0 + β;`. For a High-Shelf filter, that middle plus sign must be a **minus** sign.
-   **The Fix:** High-Shelf `a0` must be `(A + 1) - (A - 1) * cos_ω0 + β;`.

3\. The `1/S` and `1/A` Integer Division Trap

-   **The Error:** Since `S` is defined as an integer (`const double S = 6;` from your previous snippet implies it might still be evaluated strictly or typed as integer `1` and `6`), writing `1/S` and `1/A` risks running into C# integer truncation rules if the literal `1` forces integer division.
-   **The Fix:** Always use explicit double literals: `1.0 / S` and `1.0 / A`.

* * *

The Fixed Code Block

Here is your exact snippet corrected for those specific typos:

```csharp
public Calculator(double level)
{
    // Maps 0.0 -> 1.0 to -6dB -> +6dB
    double dbGain = (level - 0.5) * 12;

    A = Math.Pow(10.0, dbGain / 40.0);

    cos_ω0 = Math.Cos(ω0);
    sin_ω0 = Math.Sin(ω0);

    // Using 1.0 explicit double literals to guarantee no integer division bugs
    α = 0.5 * sin_ω0 * Math.Sqrt((A + 1.0 / A) * (1.0 / S - 1.0) + 2.0);
    β = 2.0 * α * Math.Sqrt(A);
}

public void GetLowShelfParameters(out IIRParams param)
{
    double a0 = (A + 1.0) + (A - 1.0) * cos_ω0 + β;

    param.B0 = A * ((A + 1.0) - (A - 1.0) * cos_ω0 + β) / a0;
    param.B1 = 2.0 * A * ((A - 1.0) - (A + 1.0) * cos_ω0) / a0;
    param.B2 = A * ((A + 1.0) - (A - 1.0) * cos_ω0 - β) / a0;

    // FIX: Added / a0 division
    param.A1 = -2.0 * ((A - 1.0) + (A + 1.0) * cos_ω0) / a0;
    param.A2 = ((A + 1.0) + (A - 1.0) * cos_ω0 - β) / a0;
}

public void GetHighShelfParameters(out IIRParams param)
{
    // FIX: Changed middle sign from + to - for high shelf
    double a0 = (A + 1.0) - (A - 1.0) * cos_ω0 + β;

    param.B0 = A * ((A + 1.0) + (A - 1.0) * cos_ω0 + β) / a0;
    param.B1 = -2.0 * A * ((A - 1.0) + (A + 1.0) * cos_ω0) / a0;
    param.B2 = A * ((A + 1.0) + (A - 1.0) * cos_ω0 - β) / a0;

    // FIX: Added / a0 division
    param.A1 = 2.0 * ((A - 1.0) - (A + 1.0) * cos_ω0) / a0;
    param.A2 = ((A + 1.0) - (A - 1.0) * cos_ω0 - β) / a0;
}
```

Use code with caution.

If you want, I can show you how to structure the **audio sample processing method** that consumes these five coefficients. Would you like to see a:

-   Standard **Direct Form I** loop (easiest to understand)
-   **Transposed Direct Form II** loop (most efficient for CPU/memory)

Share

---

# you asked

message time: 4:57 p.m.

CopiedCopyEditS is not defined as an integer. The source code uses an integer literal, but the symbol is declared as a double. The fact that the `6` in the source code is an integer is immaterial; math done on S starts with `double` values and will promote the 1 numerator to `double`.4:57 p.m.

---

# googlesearch response

You are entirely correct. Because `S` is explicitly declared as a `double`, the expression `1 / S` triggers implicit type promotion in C#. The integer literal `1` is safely promoted to a `double` at runtime, completely avoiding the integer truncation trap.

With that variable type cleared up, fixing the missing `a0` divisions and the high-shelf `a0` sign error makes your coefficient generator mathematically sound.

Here is how to implement the **Transposed Direct Form II** loop to process your PCM audio samples with those coefficients. This structure is highly efficient because it only requires two tracking states per channel.

1\. The Per-Channel State Structure

You must track state variables (`w1`, `w2`) independently for each audio channel (e.g., Left and Right) to prevent the audio from bleeding together or feeding back incorrectly.

```csharp
public struct FilterState
{
    // Memory registers for the Transposed Direct Form II structure
    public double W1;
    public double W2;

    public void Clear()
    {
        W1 = 0.0;
        W2 = 0.0;
    }
}
```

Use code with caution.

2\. The Sample Processing Method

Add this method to your core filter class. It takes a single audio sample, applies the calculated coefficients using the transposed difference equation, updates the internal memory states, and returns the filtered sample.

```csharp
public double ProcessSample(double inputSample, IIRParams coefficients, ref FilterState state)
{
    // 1. Calculate the final output sample using the current input and the first delay register
    double outputSample = (coefficients.B0 * inputSample) + state.W1;

    // 2. Update the first delay register, pulling from the second register
    state.W1 = (coefficients.B1 * inputSample) - (coefficients.A1 * outputSample) + state.W2;

    // 3. Update the final delay register
    state.W2 = (coefficients.B2 * inputSample) - (coefficients.A2 * outputSample);

    return outputSample;
}
```

Use code with caution.

3\. Processing a Dual-Filter Stereo Stream

To process a full block of stereo PCM data through both your bass and treble filters sequentially, cascade the operations like this:

```csharp
public void ProcessAudioBlock(float[] sampleBuffer, int sampleCount, IIRParams bassParams, IIRParams trebleParams)
{
    // Separate states for Left and Right channels across both filters
    // Note: These states must persist across audio blocks in your real class instance
    for (int i = 0; i < sampleCount; i += 2)
    {
        double leftInput = sampleBuffer[i];
        double rightInput = sampleBuffer[i + 1];

        // --- Step 1: Pass through Bass Filter ---
        double leftAfterBass = ProcessSample(leftInput, bassParams, ref leftBassState);
        double rightAfterBass = ProcessSample(rightInput, bassParams, ref rightBassState);

        // --- Step 2: Pass output directly into Treble Filter ---
        double leftFinal = ProcessSample(leftAfterBass, trebleParams, ref leftTrebleState);
        double rightFinal = ProcessSample(rightAfterBass, trebleParams, ref rightTrebleState);

        // Write the processed audio back to your buffer (clamping may be needed if converting back to 16-bit)
        sampleBuffer[i] = (float)leftFinal;
        sampleBuffer[i + 1] = (float)rightFinal;
    }
}
```

Use code with caution.

If you want, let me know:

-   How you plan to handle **parameter smoothing** when a user moves the bass/treble slider
-   If you need a mechanism to **hard-clamp the output** to prevent digital clipping when boosting gains

I can show you how to integrate those features safely into your loop.

Share
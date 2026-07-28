namespace QBX.Hardware.AdLib;

/* Envelope Generator phases */
enum EnvelopeGeneratorPhase : byte
{
	Attack = 4,
	Decay = 3,
	Sustain = 2,
	Release = 1,

	Off = 0,
}

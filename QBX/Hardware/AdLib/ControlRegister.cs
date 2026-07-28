namespace QBX.Hardware.AdLib;

enum ControlRegister
{
	First,

	ControlID = First,
	TelephoneControl,
	SamplingGainLeft,
	SamplingGainRight,
	FinalOutputVolumeLeft,
	FinalOutputVolumeRight,
	Bass,
	Treble,
	OutputMode,
	Volume_FMLeft,
	Volume_FMRight,
	Volume_PCM,
	Volume_Microphone,
	Volume_AuxInput,
	Volume_MasterLeft,
	Volume_MasterRight,
	Volume_Tone,
	AudioSelection,
	Unused,
	SamplingConfig0,
	SamplingConfig1,
	BasePort, // >> 3
	SCSIConfig,
	SCSIPort, // >> 3
	Surround,

	Last = Surround,

	RegisterCount,
}

namespace QBX.Hardware.AdLib;

enum OPLType : byte
{
	WaveformSelect = 0x01,  /* waveform select     */
	ADPCM          = 0x02,  /* DELTA-T ADPCM unit  */
	Keyboard       = 0x04,  /* keyboard interface  */
	IO             = 0x08,  /* I/O port            */
}

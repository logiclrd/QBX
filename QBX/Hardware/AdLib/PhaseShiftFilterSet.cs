using System;

namespace QBX.Hardware.AdLib;

struct PhaseShiftFilterSet
{
	double _α1;
	double _α2;

	public PhaseShiftFilterSet()
	{
		// Layer 1: cutoff ~707.35 Hz
		const double Fc1 = 1_000_000_000.0 / (2 * Math.PI * 15000 * 15);

		const double ω01 = 2 * Math.PI * Fc1;

		double k1 = Math.Tan(ω01 / (2 * AdLibGold.SampleRate));

		_α1 = (k1 - 1) / (k1 + 1);

		// Layer 2: cutoff ~7073.5 Hz
		const double Fc2 = 1_000_000_000.0 / (2 * Math.PI * 15000 * 1.5);

		const double ω02 = 2 * Math.PI * Fc2;

		double k2 = Math.Tan(ω02 / (2 * AdLibGold.SampleRate));

		_α2 = (k2 - 1) / (k2 + 1);
	}

	double _lastInput1;
	double _lastInput2;

	double _lastOutput1;
	double _lastOutput2;

	public double ProcessSample(double value)
	{
		// Layer 1: cutoff ~707.35 Hz
		double intermediateValue = _α1 * value + _lastInput1 - _α1 * _lastOutput1;

		_lastInput1 = value;
		_lastOutput1 = intermediateValue;

		// Layer 2: cutoff ~7073.5 Hz
		value = _α2 * intermediateValue + _lastInput2 - _α2 * _lastOutput2;

		_lastInput2 = intermediateValue;
		_lastOutput2 = value;

		return value;
	}
}

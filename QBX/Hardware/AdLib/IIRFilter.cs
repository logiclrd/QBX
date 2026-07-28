namespace QBX.Hardware.AdLib;

struct IIRFilter
{
	public IIRParams Params;
	public IIRState StateLeft;
	public IIRState StateRight;

	public (double Left, double Right) ProcessSample(double left, double right)
	{
		double outputLeft = Params.B0 * left + StateLeft.W1;

		StateLeft.W1 = Params.B1 * left - Params.A1 * outputLeft + StateLeft.W2;
		StateLeft.W2 = Params.B2 * left - Params.A2 * outputLeft;

		double outputRight = Params.B0 * right + StateRight.W1;

		StateRight.W1 = Params.B1 * right - Params.A1 * outputRight + StateRight.W2;
		StateRight.W2 = Params.B2 * right - Params.A2 * outputRight;

		return (outputLeft, outputRight);
	}
}

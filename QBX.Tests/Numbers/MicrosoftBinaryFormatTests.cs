using QBX.Numbers;

namespace QBX.Tests.Numbers
{
	public class MicrosoftBinaryFormatTests
	{
		static IEnumerable<TestCaseData<float, byte[]>> MBF32Cases()
		{
			yield return new TestCaseData<float, byte[]>(3.141593f, [0xDC, 0x0F, 0x49, 0x82]);
			yield return new TestCaseData<float, byte[]>(-3.141593f, [0xDC, 0x0F, 0xC9, 0x82]);
			yield return new TestCaseData<float, byte[]>(314.1593f, [0x64, 0x14, 0x1D, 0x89]);
			yield return new TestCaseData<float, byte[]>(.005f, [0x0A, 0xD7, 0x23, 0x79]);
			yield return new TestCaseData<float, byte[]>(1.1754945e-38f, [0x01, 0x00, 0x00, 0x03]);
			yield return new TestCaseData<float, byte[]>(1.1754944e-38f, [0x00, 0x00, 0x00, 0x03]);
			yield return new TestCaseData<float, byte[]>(1.1754942e-38f, [0x00, 0x00, 0x00, 0x00]);
			yield return new TestCaseData<float, byte[]>(1.7014117e+38f, [0xFF, 0xFF, 0x7F, 0xFF]);
		}

		[TestCaseSource(nameof(MBF32Cases))]
		public void GetBytes_float_should_return_32_bit_MBF_bytes(float testValue, byte[] expectedBytes)
		{
			// Act
			var bytes = MicrosoftBinaryFormat.GetBytes(testValue);

			// Assert
			bytes.Should().BeEquivalentTo(expectedBytes, options => options.WithStrictOrdering());
		}

		[TestCaseSource(nameof(MBF32Cases))]
		public void ToSingle_should_parse_32_bit_MBF_bytes(float expectedValue, byte[] testBytes)
		{
			// Act
			var value = MicrosoftBinaryFormat.ToSingle(testBytes);

			// Assert
			float error = Math.Abs(value - expectedValue);

			error.Should().BeLessThan(1.2e-38f);
		}

		static IEnumerable<TestCaseData<double, byte[]>> MBF64Cases()
		{
			yield return new TestCaseData<double, byte[]>(1, [0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x81]);
			yield return new TestCaseData<double, byte[]>(3.1415926535, [0x20, 0xBA, 0x08, 0xA2, 0xDA, 0x0F, 0x49, 0x82]);
			yield return new TestCaseData<double, byte[]>(-3.1415926535, [0x20, 0xBA, 0x08, 0xA2, 0xDA, 0x0F, 0xC9, 0x82]);
			yield return new TestCaseData<double, byte[]>(314.15926535, [0x68, 0xD1, 0x96, 0xCE, 0x62, 0x14, 0x1D, 0x89]);
			yield return new TestCaseData<double, byte[]>(.00526535262, [0x20, 0xF2, 0x03, 0xA7, 0xFA, 0x88, 0x2C, 0x79]);
			yield return new TestCaseData<double, byte[]>(2.9387358770557187e-39, [0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01]);
			yield return new TestCaseData<double, byte[]>(2.9387358770557186e-39, [0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00]);
			yield return new TestCaseData<double, byte[]>(1.7014118346046922e+38, [0xF8, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x7F, 0xFF]);
		}


		[TestCaseSource(nameof(MBF64Cases))]
		public void GetBytes_double_should_return_64_bit_MBF_bytes(double testValue, byte[] expectedBytes)
		{
			// Act
			var bytes = MicrosoftBinaryFormat.GetBytes(testValue);

			// Assert
			bytes.Should().BeEquivalentTo(expectedBytes, options => options.WithStrictOrdering());
		}

		[TestCaseSource(nameof(MBF64Cases))]
		public void ToDouble_should_parse_64_bit_MBF_bytes(double expectedValue, byte[] testBytes)
		{
			// Act
			var value = MicrosoftBinaryFormat.ToDouble(testBytes);

			// Assert
			double error = Math.Abs(value - expectedValue);

			error.Should().BeLessThan(1.47e-39f);
		}
	}
}

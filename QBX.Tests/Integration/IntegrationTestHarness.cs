using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

using QBX.CodeModel;
using QBX.ExecutionEngine;
using QBX.ExecutionEngine.Compiled;
using QBX.Hardware;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.Tests.Integration;

public class IntegrationTestHarness
{
	static readonly TimeSpan TestTimeLimit = TimeSpan.FromSeconds(0.8);

	static IEnumerable<TestCaseData<string>> FindAndEnumerateIntegrationTests()
		=> EnumerateIntegrationTests();

	static IEnumerable<TestCaseData<string>> EnumerateIntegrationTests([CallerFilePath] string testHarnessFilePath = "")
	{
		string integrationTestingBaseDirectory = Path.GetDirectoryName(testHarnessFilePath)
			?? throw new Exception("Unable to locate integration test files");

		string integrationTestsDirectory = Path.Combine(integrationTestingBaseDirectory, "Tests");

		return Directory.EnumerateFiles(integrationTestsDirectory, "*.BAS", SearchOption.AllDirectories)
			.Select(path => new TestCaseData<string>(path));
	}

	[TestCaseSource(nameof(FindAndEnumerateIntegrationTests))]
	public void RunIntegrationTest(string filePath)
	{
		// Guard against hangs
		using (var hardLimitTimer = new System.Threading.Timer(
			_ => Environment.Exit(99)))
		{
			hardLimitTimer.Change(dueTime: TestTimeLimit, period: Timeout.InfiniteTimeSpan);

			bool testOfFailureDetection = Path.GetFileName(filePath).StartsWith("FAIL");

			if (!testOfFailureDetection)
				RunIntegrationTestImplementation(filePath);
			else
			{
				var testAction = () => RunIntegrationTestImplementation(filePath);

				testAction.Should().Throw<AssertionException>();
			}
		}
	}

	void RunIntegrationTestImplementation(string filePath)
	{
		// Arrange
		var cancellationTokenSource = new CancellationTokenSource(TestTimeLimit);

		cancellationTokenSource = new CancellationTokenSource();

		var dispatcher = IntegrationTestHostDispatcher.Start(cancellationTokenSource.Token);

		var machine = new Machine();

		var testOutput = new TestOutputFileDescriptor();

		machine.DOS.Devices.RegisterDevice("TESTOUT$", testOutput);

		var qbx = new DevelopmentEnvironment.Program(commandLine: "QBX.exe", machine, dispatcher);

		qbx.LoadQLB("QBX");

		qbx.LoadFile(filePath, replaceExistingProgram: true);

		qbx.AutoRun = true;
		qbx.AbortOnBreak = true;

		// Act
		qbx.Run(cancellationTokenSource.Token);

		// Assert
		string capturedOutput = testOutput.CapturedOutput.ToString();

		capturedOutput.Should().Contain("PASS");
		capturedOutput.Should().NotContain("FAIL");
	}
}

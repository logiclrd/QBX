using System;
using System.Threading;

using QBX.Hardware;
using QBX.OperatingSystem.Breaks;
using QBX.OperatingSystem.FileStructures;

namespace QBX.OperatingSystem.FileDescriptors;

public class StandardInputFileDescriptor(DOS owner) : FileDescriptor
{
	protected override bool CanRead => base.CanRead;

	Machine _machine = owner.Machine;

	bool _blockingInput = true;

	class NonBlockingScope(StandardInputFileDescriptor owner) : IDisposable
	{
		bool saved = Interlocked.Exchange(ref owner._blockingInput, false);
		public void Dispose() => owner._blockingInput = saved;
	}

	public override IDisposable? NonBlocking() => new NonBlockingScope(this);

	protected override uint SeekCore(int offset, MoveMethod moveMethod)
	{
		throw new DOSException(DOSError.InvalidFunction);
	}

	protected override uint SeekCore(uint offset, MoveMethod moveMethod)
	{
		throw new DOSException(DOSError.InvalidFunction);
	}

	protected override void ReadCore(FileBuffer buffer)
	{
		do
		{
			if (_blockingInput)
			{
				if (!owner.BreakEnabled)
					_machine.Keyboard.WaitForInput();
				else
				{
					using (var scope = owner.CancelOnBreak())
					{
						_machine.Keyboard.WaitForInput(scope.Token);

						if (scope.BreakReceived)
							throw new Break();
					}
				}
			}

			var evt = _machine.Keyboard.GetNextEvent();

			if ((evt != null) && !evt.IsEphemeral)
			{
				if (evt.IsNormalText)
					ReadBuffer.Push(unchecked((byte)evt.TextCharacter));
				else
				{
					ReadBuffer.Push(0);
					ReadBuffer.Push(unchecked((byte)evt.ScanCode));
				}

				return;
			}
		} while (_blockingInput);

		WouldHaveBlocked = true;
	}
}

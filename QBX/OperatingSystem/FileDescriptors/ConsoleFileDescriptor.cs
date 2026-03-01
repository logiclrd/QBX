using System;
using System.Collections.Generic;
using System.Threading;

using QBX.Hardware;
using QBX.OperatingSystem.Breaks;
using QBX.OperatingSystem.FileStructures;

namespace QBX.OperatingSystem.FileDescriptors;

public class ConsoleFileDescriptor(DOS owner) : FileDescriptor("CON")
{
	public override bool WriteThrough => true;

	public override bool CanRead => true;
	public override bool CanWrite => true;

	public override bool ReadyToRead => owner.Machine.Keyboard.HasQueuedTangibleInput;
	public override bool ReadyToWrite => true;

	Machine _machine = owner.Machine;

	bool _blockingInput = true;
	bool _waitForNewLine = false;

	class NonBlockingScope(ConsoleFileDescriptor owner) : IDisposable
	{
		bool saved = Interlocked.Exchange(ref owner._blockingInput, false);
		public void Dispose() => owner._blockingInput = saved;
	}

	class WaitForCarriageReturnScope(ConsoleFileDescriptor owner) : IDisposable
	{
		bool saved = Interlocked.Exchange(ref owner._waitForNewLine, true);
		public void Dispose() => owner._waitForNewLine = saved;
	}

	public override IDisposable? NonBlocking() => new NonBlockingScope(this);
	public IDisposable? WaitForCarriageReturn() => new WaitForCarriageReturnScope(this);

	protected override uint SeekCore(int offset, MoveMethod moveMethod)
	{
		throw new DOSException(DOSError.InvalidFunction);
	}

	protected override uint SeekCore(uint offset, MoveMethod moveMethod)
	{
		throw new DOSException(DOSError.InvalidFunction);
	}

	Queue<byte> _lineBuffer = new Queue<byte>(capacity: 255);
	bool _lineBufferEndsAtLineBoundary = false;

	public override Boolean AtReadBoundary => ReadBuffer.IsEmpty && (_lineBuffer.Count == 0) && _lineBufferEndsAtLineBoundary;

	protected override void ReadCore(ref FileBuffer buffer)
	{
		if (_lineBuffer.Count == 0)
		{
			_lineBufferEndsAtLineBoundary = false;

			using (var scope = owner.BreakEnabled ? owner.CancelOnBreak() : default)
			{
				while (true)
				{
					if (_blockingInput)
					{
						if (!owner.BreakEnabled)
							_machine.Keyboard.WaitForInput();
						else
						{
							_machine.Keyboard.WaitForInput(scope!.Token);

							if (scope.BreakReceived)
								throw new Break();
						}
					}

					var evt = _machine.Keyboard.GetNextEvent();

					if ((evt != null) && !evt.IsEphemeral && !evt.IsRelease)
					{
						if (evt.TextCharacter == '\n')
						{
							// ignore
						}
						else
						{
							if (_lineBuffer.Count + 1 > _lineBuffer.Capacity)
								break;

							if (evt.HasTextCharacter)
								_lineBuffer.Enqueue(unchecked((byte)evt.TextCharacter));
							else
							{
								_lineBuffer.Enqueue(0);

								if (_lineBuffer.Count + 1 > _lineBuffer.Capacity)
									break;

								_lineBuffer.Enqueue(unchecked((byte)evt.ScanCode));
							}

							if (evt.TextCharacter == '\x1A') // soft EOF
								break;

							if (evt.TextCharacter == '\r') // CR
							{
								if (_lineBuffer.Count < _lineBuffer.Capacity)
								{
									_lineBuffer.Enqueue(10); // CR -> CRLF
									_lineBufferEndsAtLineBoundary = true;
								}

								break;
							}
						}
					}

					// Exit the loop if:
					// - There is no more queued input.
					// - AND either of the following conditions are true:
					//   * We are configured to be non-blocking.
					//   * Any bytes have been collected, and we're not configured to wait for a newline.
					if (!_machine.Keyboard.HasQueuedTangibleInput)
					{
						if (!_blockingInput)
							break;
						if ((_lineBuffer.Count > 0) && !_waitForNewLine)
							break;
					}
				}
			}
		}

		if (_lineBuffer.Count == 0)
			WouldHaveBlocked = true;
		else
		{
			while ((ReadBuffer.Available > 0) && (_lineBuffer.Count > 0))
				ReadBuffer.Push(_lineBuffer.Dequeue());
		}
	}

	protected override int WriteCore(ReadOnlySpan<byte> buffer)
	{
		var visual = owner.Machine.VideoFirmware.VisualLibrary;

		bool savedControlCharacterFlag = visual.ProcessControlCharacters;

		try
		{
			visual.ProcessControlCharacters = true;
			visual.WriteText(buffer);

			return buffer.Length;
		}
		finally
		{
			visual.ProcessControlCharacters = savedControlCharacterFlag;
		}
	}
}

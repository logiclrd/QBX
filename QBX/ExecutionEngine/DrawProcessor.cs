using System;
using System.Runtime.InteropServices;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;

using QBX.Firmware;

using static QBX.Firmware.Fonts.CP437Encoding;

using ExecutionContext = QBX.ExecutionEngine.Execution.ExecutionContext;

namespace QBX.ExecutionEngine;

public class DrawProcessor : ProcessorCommon
{
	// Local state: Colour, angle, scale
	// Remote state: Last (x, y)

	GraphicsLibrary? _graphics;
	int _colourMask;
	int _colour;
	double _angle;
	double _scale;

	public void Disable()
	{
		_graphics = null;
	}

	public void Initialize(GraphicsLibrary graphics, int colourMask)
	{
		_graphics = graphics;

		_colour = graphics.DrawingAttribute & colourMask;
		_colourMask = colourMask;
		_angle = 0;
		_scale = 1.0;
	}

	public void SetColour(int colour)
	{
		_colour = colour & _colourMask;
	}

	public void DrawCommandString(StringValue commandString, CodeModel.Statements.Statement? source)
		=> DrawCommandString(commandString.AsSpan(), source);

	public void DrawCommandString(Span<byte> commandString, CodeModel.Statements.Statement? source)
		=> DrawCommandString(commandString, executionContext: null, source);

	public void DrawCommandString(StringValue commandString, ExecutionContext? executionContext, CodeModel.Statements.Statement? source)
		=> DrawCommandString(commandString.AsSpan(), executionContext, source);

	public void DrawCommandString(Span<byte> commandString, ExecutionContext? executionContext, CodeModel.Statements.Statement? source)
	{
		if (_graphics == null)
			throw RuntimeException.IllegalFunctionCall(source);

		var input = commandString;

		SkipWhitespace(ref input);

		bool suppressNextDraw = false;
		bool suppressNextMove = false;

		while (input.Length > 0)
		{
			byte ch = ToUpper(input[0]);

			switch (ch)
			{
				case Space:
				case Tab:
				{
					SkipWhitespace(ref input);
					break;
				}
				case B: // move but do not plot
				{
					AdvanceAndSkipWhitespace(ref input);
					suppressNextDraw = true;
					break;
				}
				case N: // plot but do not move
				{
					AdvanceAndSkipWhitespace(ref input);
					suppressNextMove = true;
					break;
				}
				case U: // up
				case D: // down
				case L: // left
				case R: // right
				case E: // up-right
				case F: // down-right
				case G: // down-left
				case H: // up-left
				case M: // move
				{
					AdvanceAndSkipWhitespace(ref input);

					Point lastPoint = _graphics.LastPoint;
					Point newPoint;

					bool relative;
					double dx;
					double dy;

					if (ch == M)
					{
						int sign = 1;

						relative = false;

						if (input.Length > 0)
						{
							switch (input[0])
							{
								case Plus: relative = true; break;
								case Minus: relative = true; sign = -1; break;
							}

							if (relative)
								AdvanceAndSkipWhitespace(ref input);
						}

						dx = _scale * sign * ExpectInteger(ref input, executionContext);

						AdvanceAndSkipWhitespace(ref input);

						if ((input.Length == 0) || (input[0] != Comma))
							Fail();

						dy = _scale * sign * ExpectInteger(ref input, executionContext);
					}
					else
					{
						relative = true;

						double commandAngle =
							ch switch
							{
								R => 0,
								E => 45,
								U => 90,
								H => 135,
								L => 180,
								G => 225,
								D => 270,
								F => 315,
								_ => throw new Exception("Sanity failure")
							};

						double commandLength =
							ch switch
							{
								U or D or L or R => 1,
								E or F or G or H => Sqrt2,
								_ => throw new Exception("Sanity failure")
							};

						if ((input.Length > 0) && IsDigit(input[0]))
							commandLength *= ExpectInteger(ref input, executionContext);

						commandLength *= _scale;

						commandAngle = double.DegreesToRadians(commandAngle);

						// First calculate the position
						dx = commandLength * Math.Cos(commandAngle);
						dy = -commandLength * Math.Sin(commandAngle);

						// Then scale it to normalize pixel size
						double aspect = (_graphics.Height / (double)_graphics.PhysicalHeight) / (_graphics.Width / (double)_graphics.PhysicalWidth);

						dy /= aspect;

						// Then apply rotation
						double a = double.DegreesToRadians(_angle);

						double ca = Math.Cos(a);
						double sa = -Math.Sin(a);

						double udx = dx;
						double udy = dy;

						dx = udx * ca + udy * sa;
						dy = udx * sa - udy * ca;

						// Then undo the scaling
						dy *= aspect;
					}

					newPoint =
						relative
						? lastPoint + (dx, dy)
						: (dx, dy);

					if (!suppressNextDraw)
					{
						_graphics.LineTo(newPoint, _colour);

						if (suppressNextMove)
							_graphics.LastPoint = lastPoint;
					}
					else if (!suppressNextMove)
						_graphics.LastPoint = newPoint;

					break;
				}

				case A: // angle (orthogonal)
				{
					AdvanceAndSkipWhitespace(ref input);

					_angle = 90 * ExpectIntegerInRange(ref input, 0, 3, executionContext);

					break;
				}

				case T: // TA: angle (fine)
				{
					AdvanceAndSkipWhitespace(ref input);

					if ((input.Length == 0) || ToUpper(input[0]) != 'A')
						Fail();

					Advance(ref input);
					AdvanceAndSkipWhitespace(ref input);

					_angle = ExpectIntegerInRange(ref input, -360, +360, executionContext);

					break;
				}

				case S: // scale
				{
					AdvanceAndSkipWhitespace(ref input);

					int scaleFactor = ExpectIntegerInRange(ref input, 1, 255, executionContext);

					_scale = scaleFactor * 0.25;

					break;
				}

				case C: // colour
				{
					AdvanceAndSkipWhitespace(ref input);

					_colour = ExpectInteger(ref input, executionContext);
					_colour &= _colourMask;

					break;
				}

				case P: // paint
				{
					AdvanceAndSkipWhitespace(ref input);

					_colour = ExpectInteger(ref input, executionContext);
					_colour &= _colourMask;

					if ((input.Length == 0) || (input[0] != Comma))
						Fail();

					Advance(ref input);

					int borderColour;

					borderColour = ExpectInteger(ref input, executionContext);
					borderColour &= _colourMask;

					_graphics.BorderFill(
						_graphics.LastPoint.X,
						_graphics.LastPoint.Y,
						borderColour,
						_colour);

					break;
				}

				case X:
				{
					Advance(ref input);

					if (input.Length < 3)
						Fail();

					var descriptorBytes = input.Slice(0, 3);
					var descriptorSpan = MemoryMarshal.Cast<byte, SurfacedVariableDescriptor>(descriptorBytes);

					var descriptor = descriptorSpan[0];

					input = input.Slice(3);

					var surfaced = executionContext?.GetSurfacedVariable(descriptor.Key);

					if (surfaced is StringVariable surfacedString)
						DrawCommandString(surfacedString.ValueSpan, executionContext, source);

					break;
				}

				default:
					throw Fail();
			}
		}
	}
}

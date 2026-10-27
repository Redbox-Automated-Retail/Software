using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;

namespace Redbox.HAL.Client.Services;

public sealed class MoveHelper
{
	private readonly HardwareService Service;

	private readonly int Timeout;

	public ErrorCodes MoveTo(int deck, int slot, string mode)
	{
		string statement = $"MOVE DECK={deck} SLOT={slot} MODE={mode}";
		if (!Service.ExecuteImmediate(statement, Timeout, out var job).Success)
		{
			return ErrorCodes.ServiceChannelError;
		}
		return Enum<ErrorCodes>.ParseIgnoringCase(job.GetTopOfStack(), ErrorCodes.ServiceChannelError);
	}

	public ErrorCodes MoveTo(int deck, int slot)
	{
		return MoveTo(deck, slot, "NORMAL");
	}

	public IControllerPosition GetPosition()
	{
		using MotionControlDataExecutor motionControlDataExecutor = new MotionControlDataExecutor(Service);
		motionControlDataExecutor.Run();
		return motionControlDataExecutor.Position;
	}

	public ErrorCodes MoveAbs(Axis axis, int units)
	{
		return MoveAbs(axis, units, checkSensors: true);
	}

	public ErrorCodes MoveAbs(Axis axis, int units, bool checkSensors)
	{
		string statement = $"MOVEABS AXIS={axis.ToString().ToUpper()} UNITS={units.ToString()} SENSOR-CHECK={checkSensors.ToString().ToUpper()}";
		if (!Service.ExecuteImmediate(statement, Timeout, out var job).Success)
		{
			return ErrorCodes.ServiceChannelError;
		}
		return Enum<ErrorCodes>.ParseIgnoringCase(job.GetTopOfStack(), ErrorCodes.ServiceChannelError);
	}

	public IMotionControlLimitResponse ReadLimits()
	{
		using MotionControlDataExecutor motionControlDataExecutor = new MotionControlDataExecutor(Service);
		motionControlDataExecutor.Run();
		return motionControlDataExecutor.Limits;
	}

	public MoveHelper(HardwareService service, int timeout)
	{
		Service = service;
		Timeout = timeout;
	}

	public MoveHelper(HardwareService service)
		: this(service, 64000)
	{
	}
}

using System.Collections.Generic;
using Redbox.HAL.Client;

namespace Redbox.HAL.Common.GUI.Functions;

public sealed class MoveHelper
{
	private readonly HardwareService Service;

	private readonly int Timeout;

	public MoveHelperResult MoveTo(int deck, int slot, string mode)
	{
		string statement = $"MOVE DECK={deck} SLOT={slot} MODE={mode}";
		HardwareJob job;
		return new MoveHelperResult(Service.ExecuteImmediate(statement, Timeout, out job).Success ? job.GetTopOfStack() : "SERVICE DOWN");
	}

	public MoveHelperResult MoveTo(int deck, int slot)
	{
		return MoveTo(deck, slot, "NORMAL");
	}

	public MoveHelperResult MoveTo(int deck, int slot, bool readLimits)
	{
		MoveHelperResult moveHelperResult = MoveTo(deck, slot);
		if (readLimits)
		{
			moveHelperResult.Limits = ReadLimits();
		}
		return moveHelperResult;
	}

	public void GetPosition(ref MotorPosition position)
	{
		string statement = string.Format("MOTIONCONTROL {0}", (position.Axis == MoveAxis.X) ? "XPOS" : "YPOS");
		HardwareCommandResult hardwareCommandResult = Service.ExecuteImmediate(statement, out var job);
		position.ReadOk = hardwareCommandResult.Success;
		if (!hardwareCommandResult.Success)
		{
			return;
		}
		hardwareCommandResult = job.GetStack(out var stack);
		if (hardwareCommandResult.Success && stack.Count >= 1)
		{
			if (int.TryParse(stack.Pop(), out var result))
			{
				position.ReadOk = true;
				position.Position = result;
			}
			else
			{
				position.ReadOk = false;
			}
		}
	}

	public MoveHelperResult MoveAbs(MoveAxis axis, int units)
	{
		return MoveAbs(axis, units, checkSensors: true);
	}

	public MoveHelperResult MoveAbs(MoveAxis axis, int units, bool checkSensors)
	{
		string statement = $"MOVEABS AXIS={axis.ToString().ToUpper()} UNITS={units.ToString()} SENSOR-CHECK={checkSensors.ToString().ToUpper()}";
		HardwareJob job;
		return new MoveHelperResult(Service.ExecuteImmediate(statement, Timeout, out job).Success ? job.GetTopOfStack() : "SERVICE DOWN");
	}

	public string[] ReadLimits()
	{
		List<string> list = new List<string>();
		HardwareJob hardwareJob = CommonFunctions.ExecuteInstruction(Service, "MOTIONCONTROL READLIMITS");
		if (hardwareJob == null)
		{
			return list.ToArray();
		}
		if (hardwareJob.GetStack(out var stack).Errors.Count > 0)
		{
			return list.ToArray();
		}
		string s = stack.Peek();
		try
		{
			int num = int.Parse(s);
			stack.Pop();
			for (int i = 0; i < num; i++)
			{
				list.Add(stack.Pop());
			}
		}
		catch
		{
			list.Clear();
		}
		return list.ToArray();
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

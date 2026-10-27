using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal struct ArcusMotorStatus
{
	private MoveOperation Operation;

	internal int ArcusStatusCode { get; private set; }

	internal ErrorCodes Error { get; private set; }

	internal int Timeout { get; private set; }

	internal string Diagnostic => ArcusStatusCode switch
	{
		0 => "Motor stopped", 
		1 => "Accelerating", 
		2 => "Decelerating", 
		4 => "Constant speed", 
		8 => "Upper Limit Error", 
		16 => "Lower Limit Error", 
		_ => ArcusStatusCode.ToString(), 
	};

	internal bool MotorRunning { get; private set; }

	internal Axis[] Axes { get; private set; }

	internal void TestResponse(ArcusCommandResponse response)
	{
		TestResponse(response.Response);
	}

	internal void TestResponse(string rawResponse)
	{
		MotorRunning = true;
		if (!int.TryParse(rawResponse, out var result))
		{
			ArcusStatusCode = 999;
			MotorRunning = false;
			Error = ErrorCodes.MotorError;
			return;
		}
		ArcusStatusCode = result;
		Error = ErrorCodes.Success;
		MotorRunning = ArcusStatusCode != 0;
		switch (Operation)
		{
		case MoveOperation.Dropback:
			if (16 == ArcusStatusCode)
			{
				Error = ErrorCodes.Success;
				ArcusStatusCode = 0;
				MotorRunning = false;
			}
			break;
		case MoveOperation.Normal:
			if (16 == ArcusStatusCode)
			{
				Error = ErrorCodes.LowerLimitError;
				MotorRunning = false;
			}
			break;
		}
	}

	internal ArcusMotorStatus(string instruction, int timeout)
	{
		this = default(ArcusMotorStatus);
		Operation = MoveOperation.Normal;
		Timeout = timeout;
		Decode(instruction);
	}

	private ErrorCodes ComputeError()
	{
		if (ArcusStatusCode < 8)
		{
			return ErrorCodes.Success;
		}
		if (8 == ArcusStatusCode)
		{
			return ErrorCodes.UpperLimitError;
		}
		if (16 == ArcusStatusCode)
		{
			return ErrorCodes.LowerLimitError;
		}
		return ErrorCodes.MotorError;
	}

	private void Decode(string waitCommand)
	{
		if (waitCommand.Equals("WAITYDB", StringComparison.CurrentCultureIgnoreCase))
		{
			Operation = MoveOperation.Dropback;
			Axes = new Axis[1] { Axis.Y };
		}
		else
		{
			string text = null;
			text = ((!waitCommand.StartsWith("WAITH")) ? waitCommand.Substring("WAIT".Length) : waitCommand.Substring("WAITH".Length));
			ComputeAxis(text);
		}
	}

	private void ComputeAxis(string axisAsString)
	{
		if ("X".Equals(axisAsString.ToUpper()))
		{
			Axes = new Axis[1];
		}
		else if ("Y".Equals(axisAsString.ToUpper()))
		{
			Axes = new Axis[1] { Axis.Y };
		}
		else if ("XY".Equals(axisAsString.ToUpper()))
		{
			Axes = new Axis[2]
			{
				Axis.Y,
				Axis.X
			};
		}
		else
		{
			Axes = null;
		}
	}
}

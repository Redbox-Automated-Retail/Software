using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal struct MotorWaitDecoder
{
	internal int StatusCode { get; private set; }

	internal ErrorCodes Error { get; private set; }

	internal string Diagnostic => StatusCode switch
	{
		0 => "Motor stopped", 
		1 => "Accelerating", 
		2 => "Decelerating", 
		4 => "Constant speed", 
		8 => "Upper Limit Error", 
		16 => "Lower Limit Error", 
		_ => StatusCode.ToString(), 
	};

	internal string FormatError(Axis axis, string response)
	{
		return $"Unable to reach target on axis {axis.ToString().ToUpper()}. Response {response} ( diagnostic = {Diagnostic} )";
	}

	internal bool MotorRunning(string response, MoveOperation operation)
	{
		if (!int.TryParse(response, out var status))
		{
			StatusCode = 999;
			Error = ErrorCodes.MotorError;
			return false;
		}
		StatusCode = status;
		Error = ErrorCodes.Success;
		bool running = StatusCode != 0;
		if (16 == StatusCode)
		{
			if (operation == MoveOperation.Dropback)
			{
				Error = ErrorCodes.Success;
				StatusCode = 0;
				running = false;
			}
			else if (MoveOperation.Normal == operation)
			{
				Error = ErrorCodes.LowerLimitError;
				running = false;
			}
		}
		return running;
	}
}

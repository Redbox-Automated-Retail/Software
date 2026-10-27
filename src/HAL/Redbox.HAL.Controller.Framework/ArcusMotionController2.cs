using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Timers;

namespace Redbox.HAL.Controller.Framework;

internal sealed class ArcusMotionController2 : AbstractMotionController
{
	private readonly List<string> MovePreamble = new List<string>();

	private readonly string[] InvalidQueryResponse = new string[1] { "INVALID QUERY RESPONSE" };

	private readonly IDeviceDescriptor ArcusDescriptor;

	private readonly List<string> ErrorCorrectPreamble = new List<string>();

	private readonly List<string> InitCommands = new List<string>();

	private const string HaltMotorCommand = "RSTOP";

	private const string NopCommand = "NOP";

	private bool DebugEnabled;

	private bool UseSmoothMove;

	private ICommPort Port;

	internal override void OnConfigurationLoad()
	{
		LogHelper.Instance.Log("[ArcusMotionControl] Notify configuration loaded.");
		ComputeMoveCommands();
		UseSmoothMove = ControllerConfiguration.Instance.ArcusSmoothMove;
		DebugEnabled = ControllerConfiguration.Instance.EnableArcusTrace;
	}

	internal override void OnConfigurationChangeStart()
	{
	}

	internal override void OnConfigurationChangeEnd()
	{
		LogHelper.Instance.Log("[ArcusMotionControl] On configuration end.");
		ComputeMoveCommands();
		UseSmoothMove = ControllerConfiguration.Instance.ArcusSmoothMove;
		DebugEnabled = ControllerConfiguration.Instance.EnableArcusTrace;
	}

	internal override ErrorCodes MoveToTarget(ref MoveTarget target)
	{
		ErrorCodes errorCodes = ClearYMotorError();
		if (errorCodes != ErrorCodes.Success)
		{
			return errorCodes;
		}
		return MoveAbsolute(ref target);
	}

	internal override ErrorCodes HomeAxis(Axis axis)
	{
		if (axis != Axis.X)
		{
			return HomeYAxis();
		}
		return HomeXAxis();
	}

	internal override ErrorCodes MoveToVend(MoveMode mode)
	{
		int num = ControllerConfiguration.Instance.VendYPosition;
		if (ControllerConfiguration.Instance.VendPositionReceiveOffset != 0)
		{
			switch (mode)
			{
			case MoveMode.Get:
				num -= ControllerConfiguration.Instance.VendPositionReceiveOffset;
				break;
			case MoveMode.Put:
				num += ControllerConfiguration.Instance.VendPositionReceiveOffset;
				break;
			}
		}
		if (ControllerConfiguration.Instance.QueryPositionForVendMove)
		{
			IControllerPosition controllerPosition = ReadPositions();
			if (controllerPosition.ReadOk && controllerPosition.YCoordinate.Value == num)
			{
				if (LogHelper.Instance.IsLevelEnabled(LogEntryType.Debug))
				{
					LogHelper.Instance.Log("[ArcusMotionController2] Picker currently at position Y = {0}", controllerPosition.YCoordinate.Value);
				}
				return ErrorCodes.Success;
			}
		}
		ErrorCodes errorCodes = ClearYMotorError();
		if (errorCodes != ErrorCodes.Success)
		{
			return errorCodes;
		}
		MoveTarget target = new MoveTarget
		{
			Axis = Axis.Y,
			YCoordinate = num
		};
		return MoveAbsolute(ref target);
	}

	internal override bool CommunicationOk()
	{
		string text = SendReciveRaw("$");
		if (text == null)
		{
			return false;
		}
		LogHelper.Instance.Log(LogEntryType.Debug, "Arcus comm ok: command returns {0}", text);
		return text.StartsWith("OK");
	}

	internal override IControllerPosition ReadPositions()
	{
		ArcusControllerPosition arcusControllerPosition = new ArcusControllerPosition();
		arcusControllerPosition.XCoordinate = null;
		arcusControllerPosition.YCoordinate = null;
		arcusControllerPosition.XCoordinate = DecodePositionResponse(Axis.X);
		arcusControllerPosition.YCoordinate = DecodePositionResponse(Axis.Y);
		return arcusControllerPosition;
	}

	internal override bool OnShutdown()
	{
		return Port.Close();
	}

	internal override IMotionControlLimitResponse ReadLimits()
	{
		string text = SendReciveRaw("???");
		ArcusLimitResponse arcusLimitResponse = new ArcusLimitResponse(text);
		if (!arcusLimitResponse.ReadOk)
		{
			WriteToLog("[ReadLimits] The response is incorrect {0}", (text == null) ? "NONE" : text);
		}
		return arcusLimitResponse;
	}

	internal override bool OnResetDeviceDriver()
	{
		OnShutdown();
		bool result = ArcusDescriptor.ResetDriver();
		LogHelper.Instance.WithContext("Arcus device reset returned {0}", result.ToString().ToUpper());
		return result;
	}

	internal override bool OnStartup()
	{
		if (!Port.Open())
		{
			LogHelper.Instance.Log(LogEntryType.Error, "[ArcusMotionControl] Unable to open port {0}.", Port.DisplayName);
			Port.Close();
			return false;
		}
		InitCommands.ForEach(delegate(string each)
		{
			SendReciveRaw(each);
		});
		return true;
	}

	internal ArcusMotionController2()
	{
		IUsbDeviceService service = ServiceLocator.Instance.GetService<IUsbDeviceService>();
		ArcusDescriptor = new ArcusDeviceDescriptor(service);
		SerialPort port = new SerialPort(ControllerConfiguration.Instance.MotionControllerPortName, 115200, Parity.None, 8, StopBits.One);
		IPortManagerService service2 = ServiceLocator.Instance.GetService<IPortManagerService>();
		Port = service2.Create(port);
		Port.WriteTerminator = new byte[2] { 13, 0 };
		Port.WritePause = ControllerConfiguration.Instance.ArcusWritePause;
		Port.WriteTimeout = ControllerConfiguration.Instance.MotionControllerTimeout;
		Port.ValidateResponse = (IChannelResponse response) => response.GetIndex(4) != -1;
		Port.DisplayName = "Motion Control";
		Port.EnableDebugging = DebugEnabled;
		LogHelper.Instance.Log("[ArcusMotionController2] ctor");
	}

	private void ComputeMoveCommands()
	{
		LogHelper.Instance.Log("Re-computing move commands.");
		InitCommands.Clear();
		InitCommands.AddRange(new string[19]
		{
			"$",
			"$",
			"$",
			"RSTOP",
			"MECLEARX",
			"MECLEARY",
			"MECLEARZ",
			"MECLEARU",
			"I1=4204544",
			"I2=3136",
			"I3=" + ControllerConfiguration.Instance.GearX.GetEncoderRatio(),
			"I4=0",
			"I5=0",
			"I6=0",
			"I7=10",
			"I8=0",
			"I9=2000",
			"I10=0",
			"I11=2"
		});
		ErrorCorrectPreamble.Clear();
		ErrorCorrectPreamble.Add($"LSPD {(int)ControllerConfiguration.Instance.GearX.GetStepRatio() * 100}");
		ErrorCorrectPreamble.Add($"I7={(ControllerConfiguration.Instance.WidenArcusTolerance ? 5 : 2)}");
		MovePreamble.Clear();
		MovePreamble.AddRange(new string[10]
		{
			"$",
			"RSTOP",
			"MECLEARY",
			"MECLEARX",
			ControllerConfiguration.Instance.MoveAxisXYSpeed.GetHighSpeedCommand(),
			ControllerConfiguration.Instance.MoveAxisXYSpeed.GetLowSpeedCommand(),
			ControllerConfiguration.Instance.MoveAxisXYSpeed.GetAccelerationCommand(),
			"ABS",
			"I7=15",
			"I2=3136"
		});
	}

	private ErrorCodes OnWaitMotor(int timeout, Axis axis)
	{
		return OnWaitMotor(timeout, axis, MoveOperation.Normal);
	}

	private ErrorCodes OnWaitMotor(int timeout, Axis axis, MoveOperation operation)
	{
		IRuntimeService service = ServiceLocator.Instance.GetService<IRuntimeService>();
		TimeSpan timespan = new TimeSpan(0, 0, 0, 0, ControllerConfiguration.Instance.ArcusMotorQueryPause);
		IDoorSensorService service2 = ServiceLocator.Instance.GetService<IDoorSensorService>();
		using ExecutionTimer executionTimer = new ExecutionTimer();
		string command = "MST" + axis.ToString().ToUpper();
		string text;
		MotorWaitDecoder motorWaitDecoder;
		do
		{
			DoorSensorResult doorSensorResult = service2.Query();
			if (doorSensorResult != DoorSensorResult.Ok)
			{
				WriteToLog($"Door sensor query returned {doorSensorResult.ToString()}: halting motion.");
				HaltMotor();
				return ErrorCodes.DoorOpen;
			}
			text = SendReciveRaw(command);
			if (text == null)
			{
				return ErrorCodes.ArcusNotResponsive;
			}
			motorWaitDecoder = default(MotorWaitDecoder);
			if (!motorWaitDecoder.MotorRunning(text, operation))
			{
				if (motorWaitDecoder.Error != ErrorCodes.Success)
				{
					string text2 = motorWaitDecoder.FormatError(axis, text);
					LogHelper.Instance.Log(text2, LogEntryType.Error);
					WriteToLog(text2);
				}
				return motorWaitDecoder.Error;
			}
			service.SpinWait(timespan);
		}
		while (executionTimer.ElapsedMilliseconds < timeout);
		string text3 = motorWaitDecoder.FormatError(axis, text);
		LogHelper.Instance.Log(text3, LogEntryType.Error);
		WriteToLog(text3);
		HaltMotor();
		return ErrorCodes.Timeout;
	}

	private string HaltMotor()
	{
		return SendReciveRaw("RSTOP");
	}

	private ErrorCodes HomeXAxis()
	{
		string[] array = new string[11]
		{
			"$",
			"RSTOP",
			"MECLEARX",
			"I1=4204544",
			ControllerConfiguration.Instance.InitAxisXSpeed.GetHighSpeedCommand(),
			ControllerConfiguration.Instance.InitAxisXSpeed.GetLowSpeedCommand(),
			ControllerConfiguration.Instance.InitAxisXSpeed.GetAccelerationCommand(),
			"INC",
			"EX=0",
			"PX=0",
			"I2=3072"
		};
		foreach (string command in array)
		{
			if (SendReciveRaw(command) == null)
			{
				return ErrorCodes.ArcusNotResponsive;
			}
		}
		ErrorCodes errorCodes = OnWaitMotor(5000, Axis.X);
		if (errorCodes != ErrorCodes.Success)
		{
			return errorCodes;
		}
		if (SendReciveRaw("HOMEX-") == null)
		{
			return ErrorCodes.ArcusNotResponsive;
		}
		errorCodes = OnWaitMotor(64000, Axis.X);
		if (errorCodes != ErrorCodes.Success)
		{
			return errorCodes;
		}
		array = new string[2] { "I2=3136", "I7=15" };
		foreach (string command2 in array)
		{
			if (SendReciveRaw(command2) == null)
			{
				return ErrorCodes.ArcusNotResponsive;
			}
		}
		errorCodes = OnWaitMotor(5000, Axis.X);
		if (errorCodes != ErrorCodes.Success)
		{
			return errorCodes;
		}
		if (SendReciveRaw("MECLEARX") != null)
		{
			return ErrorCodes.Success;
		}
		return ErrorCodes.ArcusNotResponsive;
	}

	private ErrorCodes HomeYAxis()
	{
		string[] array = new string[9]
		{
			"$",
			"RSTOP",
			"MECLEARY",
			ControllerConfiguration.Instance.InitAxisYSpeed.GetHighSpeedCommand(),
			ControllerConfiguration.Instance.InitAxisYSpeed.GetLowSpeedCommand(),
			ControllerConfiguration.Instance.InitAxisYSpeed.GetAccelerationCommand(),
			"INC",
			"EY=0",
			"PY=0"
		};
		foreach (string command in array)
		{
			if (SendReciveRaw(command) == null)
			{
				return ErrorCodes.ArcusNotResponsive;
			}
		}
		if (ControllerConfiguration.Instance.HomeYDropBack != 0)
		{
			int num = ControllerConfiguration.Instance.HomeYDropBack;
			if (Math.Sign(num) == 1)
			{
				num = -num;
			}
			if (SendReciveRaw("MIOY") == null)
			{
				return ErrorCodes.ArcusNotResponsive;
			}
			string command2 = $"Y{num}";
			if (SendReciveRaw(command2) == null)
			{
				return ErrorCodes.ArcusNotResponsive;
			}
			ErrorCodes errorCodes = OnWaitMotor(6000, Axis.Y, MoveOperation.Dropback);
			if (errorCodes != ErrorCodes.Success)
			{
				return errorCodes;
			}
			if (SendReciveRaw("MECLEARY") == null)
			{
				return ErrorCodes.ArcusNotResponsive;
			}
		}
		if (SendReciveRaw("HOMEY+") != null)
		{
			return OnWaitMotor(60000, Axis.Y);
		}
		return ErrorCodes.ArcusNotResponsive;
	}

	private string SendReciveRaw(string command)
	{
		if (string.IsNullOrEmpty(command) || command.Equals("NOP", StringComparison.CurrentCultureIgnoreCase))
		{
			return "OK";
		}
		bool flag = LogHelper.Instance.IsLevelEnabled(LogEntryType.Debug) || DebugEnabled;
		StringBuilder stringBuilder = (flag ? new StringBuilder() : null);
		if (flag)
		{
			stringBuilder.AppendFormat("ArcusMotionController: Instruction={0}", command);
		}
		using IChannelResponse channelResponse = Port.SendRecv(command, ControllerConfiguration.Instance.MotionControllerTimeout);
		if (!channelResponse.CommOk)
		{
			LogHelper.Instance.Log("ArcusMotionControl.SendCommand(): channel error returned {0}", channelResponse.Error);
			WriteToLog("[ArcusMotionController] Send command {0}; error = {1}", command, ErrorCodes.ArcusNotResponsive.ToString());
			if (flag)
			{
				stringBuilder.AppendFormat(", Error = {0}", ErrorCodes.ArcusNotResponsive.ToString());
				WriteToLog(stringBuilder.ToString());
			}
			return null;
		}
		int index = channelResponse.GetIndex(13);
		int count = ((index == -1) ? (channelResponse.RawResponse.Length - 1) : index);
		string text = Encoding.ASCII.GetString(channelResponse.RawResponse, 0, count);
		if (flag)
		{
			stringBuilder.AppendFormat(", Response={0}", text);
			WriteToLog(stringBuilder.ToString());
		}
		return text;
	}

	private int? DecodePositionResponse(Axis axis)
	{
		string command = ((axis == Axis.X) ? "EX" : "PY");
		string text = SendReciveRaw(command);
		if (text != null && int.TryParse(text.Substring(text.IndexOf('=') + 1), out var result))
		{
			return result;
		}
		return null;
	}

	private ErrorCodes MoveAbsolute(ref MoveTarget target)
	{
		foreach (string item in MovePreamble)
		{
			if (SendReciveRaw(item) == null)
			{
				return ErrorCodes.ArcusNotResponsive;
			}
		}
		ErrorCodes result = ErrorCodes.Success;
		switch (target.Axis)
		{
		case Axis.X:
			result = OnAxisXMove(ref target);
			break;
		case Axis.Y:
			result = OnAxisYMove(ref target);
			break;
		case Axis.XY:
			result = OnMultiAxisMove(ref target);
			break;
		}
		ErrorCodes errorCodes = IsMotorErrored(Axis.XY);
		if (errorCodes != ErrorCodes.Success)
		{
			return errorCodes;
		}
		if (ControllerConfiguration.Instance.PrintEncoderPositionAfterMove2)
		{
			IControllerPosition controllerPosition = ReadPositions();
			if (controllerPosition.ReadOk)
			{
				LogHelper.Instance.Log("MoveAbsoluteInternal: encoder positions (x,y) = {0}, {1}.", controllerPosition.XCoordinate.Value, controllerPosition.YCoordinate.Value);
			}
			else
			{
				LogHelper.Instance.Log("Unable to determine encoder position from ARCUS.");
			}
		}
		return result;
	}

	private ErrorCodes OnMultiAxisMove(ref MoveTarget target)
	{
		int moveTimeout = ControllerConfiguration.Instance.MoveTimeout;
		if (!UseSmoothMove)
		{
			if (SendReciveRaw($"X{target.XCoordinate.Value}") == null)
			{
				return ErrorCodes.ArcusNotResponsive;
			}
			if (SendReciveRaw($"Y{target.YCoordinate.Value}") == null)
			{
				return ErrorCodes.ArcusNotResponsive;
			}
		}
		else
		{
			string command = $"Y{target.YCoordinate.Value}X{target.XCoordinate.Value}";
			if (SendReciveRaw(command) == null)
			{
				return ErrorCodes.ArcusNotResponsive;
			}
		}
		ErrorCodes errorCodes = OnWaitMotor(moveTimeout, Axis.Y);
		if (errorCodes != ErrorCodes.Success)
		{
			return errorCodes;
		}
		errorCodes = OnWaitMotor(moveTimeout, Axis.X);
		if (errorCodes != ErrorCodes.Success)
		{
			return errorCodes;
		}
		foreach (string item in ErrorCorrectPreamble)
		{
			if (SendReciveRaw(item) == null)
			{
				return ErrorCodes.ArcusNotResponsive;
			}
		}
		string command2 = $"Y{target.YCoordinate.Value}X{target.XCoordinate.Value}";
		if (SendReciveRaw(command2) == null)
		{
			return ErrorCodes.ArcusNotResponsive;
		}
		errorCodes = OnWaitMotor(moveTimeout, Axis.Y);
		if (errorCodes != ErrorCodes.Success)
		{
			return errorCodes;
		}
		errorCodes = OnWaitMotor(moveTimeout, Axis.X);
		if (errorCodes != ErrorCodes.Success)
		{
			return errorCodes;
		}
		if (SendReciveRaw("I7=10") != null)
		{
			return ErrorCodes.Success;
		}
		return ErrorCodes.ArcusNotResponsive;
	}

	private ErrorCodes OnAxisYMove(ref MoveTarget target)
	{
		string command = $"Y{target.YCoordinate.Value}";
		if (SendReciveRaw(command) != null)
		{
			return OnWaitMotor(ControllerConfiguration.Instance.MoveTimeout, Axis.Y);
		}
		return ErrorCodes.ArcusNotResponsive;
	}

	private ErrorCodes OnAxisXMove(ref MoveTarget target)
	{
		int moveTimeout = ControllerConfiguration.Instance.MoveTimeout;
		string command = $"X{target.XCoordinate.Value}";
		if (SendReciveRaw(command) == null)
		{
			return ErrorCodes.ArcusNotResponsive;
		}
		ErrorCodes errorCodes = OnWaitMotor(moveTimeout, Axis.X);
		if (errorCodes != ErrorCodes.Success)
		{
			return errorCodes;
		}
		foreach (string item in ErrorCorrectPreamble)
		{
			if (SendReciveRaw(item) == null)
			{
				return ErrorCodes.ArcusNotResponsive;
			}
		}
		if (SendReciveRaw(command) == null)
		{
			return ErrorCodes.ArcusNotResponsive;
		}
		errorCodes = OnWaitMotor(moveTimeout, Axis.X);
		if (errorCodes != ErrorCodes.Success)
		{
			return errorCodes;
		}
		if (SendReciveRaw("I7=10") != null)
		{
			return ErrorCodes.Success;
		}
		return ErrorCodes.ArcusNotResponsive;
	}

	private ErrorCodes ClearYMotorError()
	{
		ErrorCodes errorCodes = IsMotorErrored(Axis.Y);
		if (ErrorCodes.MotorError != errorCodes)
		{
			return errorCodes;
		}
		HomeAxis(Axis.Y);
		return IsMotorErrored(Axis.Y);
	}

	private ErrorCodes IsMotorErrored(Axis axis)
	{
		if (Axis.XY == axis || Axis.Y == axis)
		{
			string text = SendReciveRaw("MIOY");
			if (text == null || SendReciveRaw("MECLEARY") == null)
			{
				return ErrorCodes.ArcusNotResponsive;
			}
			if (int.TryParse(text, out var result))
			{
				if (result <= 0 || result >= 8)
				{
					return ErrorCodes.Success;
				}
				return ErrorCodes.MotorError;
			}
			return ErrorCodes.ArcusNotResponsive;
		}
		return ErrorCodes.Success;
	}
}

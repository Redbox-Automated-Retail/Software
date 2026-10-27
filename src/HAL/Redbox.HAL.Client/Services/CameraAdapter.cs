using System;
using System.Text;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Client.Services;

public sealed class CameraAdapter
{
	private class _SnapResult : ISnapResult, IDisposable
	{
		public bool SnapOk { get; internal set; }

		public string Path { get; internal set; }

		public void Dispose()
		{
		}

		internal _SnapResult()
		{
			SnapOk = false;
			Path = string.Empty;
		}
	}

	private CameraState State;

	private readonly byte[] CameraSnapImmediate;

	private readonly HardwareService Service;

	public bool LegacyCamera { get; private set; }

	public bool CameraInError()
	{
		string text = RunInstruction("CAMERA RETURNERRORCOUNT");
		if (string.IsNullOrEmpty(text))
		{
			return false;
		}
		int num = 0;
		try
		{
			num = int.Parse(text);
		}
		catch
		{
			num = 0;
		}
		return num > 0;
	}

	public CameraState ToggleState()
	{
		if (State == CameraState.Unknown)
		{
			return State;
		}
		string instruction = ((CameraState.Started == State) ? "CAMERA STOP FORCE=TRUE" : "CAMERA START");
		string value = RunInstruction(instruction);
		if (string.IsNullOrEmpty(value))
		{
			State = CameraState.Unknown;
		}
		else if ("TRUE".Equals(value, StringComparison.CurrentCultureIgnoreCase))
		{
			CameraState state = ((CameraState.Started != State) ? CameraState.Started : CameraState.Stopped);
			State = state;
		}
		return State;
	}

	public CameraState GetCameraStatus()
	{
		if (State == CameraState.Unknown)
		{
			string text = RunInstruction("CAMERA STATUS");
			if (!string.IsNullOrEmpty(text))
			{
				State = ((text == "RUNNING") ? CameraState.Started : CameraState.Stopped);
			}
		}
		return State;
	}

	public void Reset(bool legacy)
	{
		LegacyCamera = legacy;
		State = CameraState.Unknown;
	}

	public ISnapResult Snap()
	{
		if (!Service.ExecuteImmediateProgram(CameraSnapImmediate, out var job).Success)
		{
			return null;
		}
		_SnapResult snapResult = new _SnapResult();
		string topOfStack = job.GetTopOfStack();
		if ("SUCCESS" == topOfStack)
		{
			string[] stackEntries = job.GetStackEntries(2);
			snapResult.SnapOk = true;
			snapResult.Path = stackEntries[1];
		}
		return snapResult;
	}

	public bool ResetReturnCounter()
	{
		HardwareJob job;
		return Service.ExecuteImmediate("CAMERA RESETRETURNCOUNTER", out job).Success;
	}

	public CameraAdapter(HardwareService service)
	{
		Service = service;
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("CLEAR");
		stringBuilder.AppendLine(" RINGLIGHT ON");
		stringBuilder.AppendLine(" CAMERA SNAP");
		stringBuilder.AppendLine(" RINGLIGHT OFF");
		CameraSnapImmediate = Encoding.ASCII.GetBytes(stringBuilder.ToString());
		State = CameraState.Unknown;
	}

	private string RunInstruction(string instruction)
	{
		using InstructionHelper instructionHelper = new InstructionHelper(Service);
		return instructionHelper.ExecuteGeneric(instruction, 120000);
	}
}

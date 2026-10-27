using System;
using System.Text;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal sealed class PickerSensorReadResult : IPickerSensorReadResult, IReadInputsResult<PickerInputs>
{
	private readonly ReadPickerInputsResult ReadResult;

	private readonly int m_blockedCount;

	private static readonly PickerInputs[] PickerSensors = new PickerInputs[6]
	{
		PickerInputs.Sensor1,
		PickerInputs.Sensor2,
		PickerInputs.Sensor3,
		PickerInputs.Sensor4,
		PickerInputs.Sensor5,
		PickerInputs.Sensor6
	};

	public ErrorCodes Error { get; private set; }

	public bool Success => Error == ErrorCodes.Success;

	public int InputCount => PickerSensors.Length;

	public bool IsFull => m_blockedCount > 0;

	public int BlockedCount => m_blockedCount;

	public void Log()
	{
		Log(LogEntryType.Info);
	}

	public void Log(LogEntryType type)
	{
		if (!Success)
		{
			OnError();
			return;
		}
		StringBuilder builder = new StringBuilder();
		builder.Append("PickerSensors: ");
		int count = 1;
		Array.ForEach(PickerSensors, delegate(PickerInputs each)
		{
			builder.AppendFormat("{0} = {1};", count++, IsInputActive(each) ? "BLOCKED" : "CLEAR");
		});
		LogHelper.Instance.Log(builder.ToString(), type);
	}

	public bool IsInputActive(PickerInputs input)
	{
		if (Success)
		{
			return ReadResult.IsInputActive(input);
		}
		return true;
	}

	public bool IsInState(PickerInputs input, InputState state)
	{
		return ReadResult.IsInState(input, state);
	}

	public void Foreach(Action<PickerInputs> action)
	{
		Array.ForEach(PickerSensors, delegate(PickerInputs sensor)
		{
			action(sensor);
		});
	}

	internal PickerSensorReadResult(ErrorCodes notSuccess)
	{
		Error = notSuccess;
		m_blockedCount = PickerSensors.Length;
		OnError();
	}

	internal PickerSensorReadResult(ReadPickerInputsResult result)
	{
		PickerSensorReadResult pickerSensorReadResult = this;
		ReadResult = result;
		Error = result.Error;
		if (Error != ErrorCodes.Success)
		{
			m_blockedCount = PickerSensors.Length;
			OnError();
			return;
		}
		int bc = 0;
		Array.ForEach(PickerSensors, delegate(PickerInputs each)
		{
			if (pickerSensorReadResult.ReadResult.IsInputActive(each))
			{
				int num = bc + 1;
				bc = num;
			}
		});
		m_blockedCount = bc;
	}

	private void OnError()
	{
		if (Error != ErrorCodes.Success)
		{
			LogHelper.Instance.WithContext(LogEntryType.Error, "Read Picker sensors failed with error {0}", Error);
		}
	}
}

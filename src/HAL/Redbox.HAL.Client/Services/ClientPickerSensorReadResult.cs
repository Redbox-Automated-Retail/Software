using System;
using System.Collections.Generic;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Client.Services;

internal sealed class ClientPickerSensorReadResult : IPickerSensorReadResult, IReadInputsResult<PickerInputs>
{
	private const int SensorCount = 6;

	private readonly bool[] m_sensors = new bool[6];

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

	public int InputCount => 6;

	public bool IsFull => BlockedCount > 0;

	public int BlockedCount { get; private set; }

	public void Log()
	{
	}

	public void Log(LogEntryType type)
	{
	}

	public bool IsInputActive(PickerInputs input)
	{
		int num = Array.IndexOf(PickerSensors, input);
		if (-1 == num)
		{
			throw new ArgumentException();
		}
		return m_sensors[num];
	}

	public bool IsInState(PickerInputs input, InputState state)
	{
		throw new NotImplementedException();
	}

	public void Foreach(Action<PickerInputs> action)
	{
		Array.ForEach(PickerSensors, delegate(PickerInputs each)
		{
			action(each);
		});
	}

	internal ClientPickerSensorReadResult(ErrorCodes error)
	{
		BlockedCount = 6;
		Array.ForEach(m_sensors, delegate(bool each)
		{
			each = false;
		});
		Error = error;
	}

	internal ClientPickerSensorReadResult(Stack<string> jobStack)
	{
		for (int num = 5; num >= 0; num--)
		{
			string text = jobStack.Pop();
			m_sensors[num] = (text.Contains("BLOCKED") ? true : false);
			if (m_sensors[num])
			{
				int blockedCount = BlockedCount + 1;
				BlockedCount = blockedCount;
			}
		}
		Error = ErrorCodes.Success;
	}
}

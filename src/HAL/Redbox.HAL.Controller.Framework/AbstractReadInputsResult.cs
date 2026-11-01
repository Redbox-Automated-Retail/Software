using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal abstract class AbstractReadInputsResult<T> : IReadInputsResult<T>
{
	protected readonly InputState[] Inputs;

	public ErrorCodes Error { get; protected set; }

	public bool Success => Error == ErrorCodes.Success;

	public int InputCount => Inputs.Length;

	protected abstract string LogHeader { get; }

	internal CoreResponse RawResponse { get; private set; }

	public void Log()
	{
		Log(LogEntryType.Info);
	}

	public void Log(LogEntryType type)
	{
		if (!Success)
		{
			LogHelper.Instance.Log(RawResponse.Diagnostic);
			return;
		}
		LogHelper.Instance.Log(type, "{0} {1}", LogHeader, RawResponse.OpCodeResponse);
	}

	public bool IsInputActive(T input)
	{
		return IsInState(input, InputState.Active);
	}

	public bool IsInState(T input, InputState state)
	{
		if (!Success)
		{
			throw new InvalidOperationException("Sensor read state is invalid");
		}
		return OnGetInputState(input) == state;
	}

	public void Foreach(Action<T> action)
	{
		OnForeachInput(action);
	}

	protected abstract InputState OnGetInputState(T input);

	protected abstract void OnForeachInput(Action<T> a);

	protected InputState GetInputState(int input)
	{
		if (input < 0 || input >= Inputs.Length)
		{
			throw new InvalidOperationException("Specified input exceeds bounds");
		}
		return Inputs[input];
	}

	protected AbstractReadInputsResult(CoreResponse response)
	{
		RawResponse = response;
		if (!RawResponse.Success)
		{
			Error = ErrorCodes.CommunicationError;
			return;
		}
		Inputs = new InputState[20];
		Error = ErrorCodes.Success;
		if (ControllerConfiguration.Instance.ValidateInputsReadResponse)
		{
			FromValidated();
		}
		else
		{
			From();
		}
	}

	private void From()
	{
		try
		{
			int inputIndex = 0;
			for (int i = 0; i < RawResponse.OpCodeResponse.Length; i++)
			{
				if (char.IsDigit(RawResponse.OpCodeResponse[i]))
				{
					Inputs[inputIndex++] = ((RawResponse.OpCodeResponse[i] == '1') ? InputState.Active : InputState.Inactive);
				}
			}
			if (20 != inputIndex)
			{
				OnBogusResponse();
			}
		}
		catch (Exception e)
		{
			OnResponseException(e);
		}
	}

	private void FromValidated()
	{
		int index = RawResponse.OpCodeResponse.IndexOf("R");
		if (21 != index || !char.IsWhiteSpace(RawResponse.OpCodeResponse[16]))
		{
			OnBogusResponse();
			return;
		}
		int inputIndex = 0;
		try
		{
			for (int i = 0; i < index; i++)
			{
				switch (RawResponse.OpCodeResponse[i])
				{
				case '0':
					Inputs[inputIndex++] = InputState.Inactive;
					break;
				case '1':
					Inputs[inputIndex++] = InputState.Active;
					break;
				}
			}
		}
		catch (Exception e)
		{
			OnResponseException(e);
			inputIndex = 0;
		}
		if (inputIndex != 20)
		{
			OnBogusResponse();
		}
	}

	private void OnResponseException(Exception e)
	{
		LogHelper.Instance.Log("[ReadInputs] Unhandled exception parsing response '{0}'", RawResponse.OpCodeResponse);
		LogHelper.Instance.Log(e.Message);
		Error = ErrorCodes.CommunicationError;
	}

	private void OnBogusResponse()
	{
		LogHelper.Instance.WithContext(LogEntryType.Error, "[ReadInputs] Unexpected response from port {0}", RawResponse.OpCodeResponse);
		Error = ErrorCodes.CommunicationError;
	}
}

using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;

namespace Redbox.HAL.Client.Services;

internal abstract class AbstractClientReadInputsResult<T> : IReadInputsResult<T>
{
	protected readonly string[] Inputs;

	private static readonly string[] ErrorInputs = new string[0];

	public ErrorCodes Error { get; private set; }

	public bool Success => Error == ErrorCodes.Success;

	public int InputCount => Inputs.Length;

	protected abstract string LogHeader { get; }

	public void Log()
	{
		Log(LogEntryType.Info);
	}

	public void Log(LogEntryType type)
	{
		LogHelper.Instance.Log("--{0} inputs dump Error = {1} icount = {2}--", LogHeader, Error, Inputs.Length);
		int input = 0;
		Array.ForEach(Inputs, delegate
		{
			LogHelper.Instance.Log(" input {0} = {1}", input, Inputs[input]);
			int num = input + 1;
			input = num;
		});
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

	protected AbstractClientReadInputsResult(HardwareService service, string instruction)
	{
		if (!service.ExecuteImmediate(instruction, out var job).Success)
		{
			Error = ErrorCodes.ServiceChannelError;
			return;
		}
		if (!job.GetStack(out var stack).Success)
		{
			Error = ErrorCodes.ServiceChannelError;
			return;
		}
		ErrorCodes errorCodes = Enum<ErrorCodes>.ParseIgnoringCase(stack.Pop(), ErrorCodes.CommunicationError);
		LogHelper.Instance.Log("[ClientReadInputsResult] Error = {0}", errorCodes);
		if (errorCodes != ErrorCodes.Success)
		{
			Error = errorCodes;
			return;
		}
		int num = Convert.ToInt32(stack.Pop());
		LogHelper.Instance.Log("[ClientReadInputsResult] Stack count = {0}", num);
		Error = ErrorCodes.Success;
		Inputs = new string[num];
		for (int num2 = num - 1; num2 >= 0; num2--)
		{
			Inputs[num2] = stack.Pop();
		}
	}
}

using System.Collections.Generic;
using Redbox.HAL.Client;

namespace HALUtilities;

internal class CounterHelper
{
	private const string DuplicateCounterName = "DUPLICATE-COUNT";

	internal bool ResetDuplicateCounterValue(HardwareService service)
	{
		ResetCounter(service, "DUPLICATE-COUNT");
		return true;
	}

	internal bool GetDuplicateCounterValue(HardwareService service, out int val)
	{
		return GetCounterValue(service, "DUPLICATE-COUNT", out val);
	}

	internal bool GetCounterValue(HardwareService service, string name, out int v)
	{
		v = 0;
		if (service == null)
		{
			return false;
		}
		if (!service.ExecuteImmediate($"COUNTER NAME=\"{name}\" GET ", out var job).Success)
		{
			return false;
		}
		Stack<string> stack = GetStack(service, job);
		if (stack.Count > 0 && stack.Peek() == "SUCCESS")
		{
			stack.Pop();
			v = int.Parse(stack.Peek());
		}
		service.ExecuteImmediate("CLEAR", out job);
		return true;
	}

	internal void ResetCounter(HardwareService service, string name)
	{
		service?.ExecuteImmediate($"COUNTER NAME=\"{name}\" RESET", out var _);
	}

	private Stack<string> GetStack(HardwareService service, HardwareJob job)
	{
		if (service == null)
		{
			return new Stack<string>();
		}
		if (!job.GetStack(out var stack).Success)
		{
			return new Stack<string>();
		}
		return stack;
	}
}

using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Client.Services;

public static class HardwareCommandResultExtensions
{
	public static void Dump(this HardwareCommandResult result)
	{
		if (!result.Success && result.Errors.Count > 0)
		{
			LogHelper.Instance.Log("\nClient Errors:");
			foreach (Error error in result.Errors)
			{
				LogHelper.Instance.Log(error.ToString());
				LogHelper.Instance.Log("Details: {0}\n", error.Details);
			}
		}
		if (result.CommandMessages.Count > 0)
		{
			LogHelper.Instance.Log("Command Messages:");
			for (int i = 0; i < result.CommandMessages.Count; i++)
			{
				LogHelper.Instance.Log(result.CommandMessages[i]);
			}
		}
		LogHelper.Instance.Log("Command {0}, execution time = {1}", result.Success ? "successfully executed" : "failed during execution", result.ExecutionTime);
	}
}

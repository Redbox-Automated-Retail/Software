using System.Xml;
using Redbox.HAL.Client;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Common.GUI.Functions;

public static class CommonFunctions
{
	private const int SellThruSlotWidth = 466;

	public static ClientHelper Helper { get; private set; }

	public static bool ComputeQuadrants(decimal? startOffset, int? numberOfQuadrants, int? sellThruSlots, int? sellThruOffset, int? slotsPerQuadrant, decimal? slotWidth, XmlNode deckNode)
	{
		decimal? num = startOffset;
		for (int i = 0; i < numberOfQuadrants; i++)
		{
			deckNode.ChildNodes[i].SetAttributeValue("Offset", (int)num.Value);
			decimal? num2 = 0m;
			num2 = ((!sellThruSlots.HasValue || !sellThruOffset.HasValue) ? ((decimal?)slotsPerQuadrant.Value * slotWidth + slotWidth) : ((decimal?)(slotsPerQuadrant - 1).Value * slotWidth + (decimal?)0.5m + (decimal?)466));
			num += num2;
		}
		return true;
	}

	public static HardwareJob ExecuteInstruction(HardwareService service, string instruction)
	{
		HardwareJob job = null;
		HardwareCommandResult hardwareCommandResult = service.ExecuteImmediate(instruction, out job);
		DumpResult(hardwareCommandResult);
		if (!hardwareCommandResult.Success)
		{
			return null;
		}
		return job;
	}

	public static HardwareJob ExecuteInstruction(HardwareService service, string instruction, int timeout)
	{
		HardwareJob job = null;
		HardwareCommandResult hardwareCommandResult = service.ExecuteImmediate(instruction, timeout, out job);
		DumpResult(hardwareCommandResult);
		if (!hardwareCommandResult.Success)
		{
			return null;
		}
		return job;
	}

	public static void DumpResult(HardwareCommandResult result)
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

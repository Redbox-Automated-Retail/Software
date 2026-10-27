using System.Xml;
using Redbox.HAL.Component.Model.Extensions;

namespace Redbox.HAL.MSHALTester;

public static class CommonFunctions
{
	private const int SellThruSlotWidth = 466;

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
}

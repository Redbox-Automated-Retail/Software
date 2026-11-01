using System.Xml;
using Redbox.HAL.Component.Model.Extensions;

namespace Redbox.HAL.MSHALTester;

public static class CommonFunctions
{
	private const int SellThruSlotWidth = 466;

	public static bool ComputeQuadrants(decimal? startOffset, int? numberOfQuadrants, int? sellThruSlots, int? sellThruOffset, int? slotsPerQuadrant, decimal? slotWidth, XmlNode deckNode)
	{
		decimal? offset = startOffset;
		for (int i = 0; i < numberOfQuadrants; i++)
		{
			deckNode.ChildNodes[i].SetAttributeValue("Offset", (int)offset.Value);
			decimal? quadrantWidth = 0m;
			quadrantWidth = ((!sellThruSlots.HasValue || !sellThruOffset.HasValue) ? ((decimal?)slotsPerQuadrant.Value * slotWidth + slotWidth) : ((decimal?)(slotsPerQuadrant - 1).Value * slotWidth + (decimal?)0.5m + (decimal?)466));
			offset += quadrantWidth;
		}
		return true;
	}
}

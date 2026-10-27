using System.Collections.Generic;

namespace Redbox.HAL.Component.Model;

public interface IDeck
{
	int SlotsPerQuadrant { get; }

	bool IsSparse { get; }

	bool IsQlm { get; }

	int Number { get; }

	int NumberOfSlots { get; }

	int YOffset { get; }

	IList<IQuadrant> Quadrants { get; }

	bool IsSlotValid(int slot);

	bool IsSlotSellThru(int slot);

	int GetSlotOffset(int slot);
}

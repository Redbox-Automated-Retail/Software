using System;

namespace Redbox.HAL.Component.Model;

public interface ILocation
{
	bool IsEmpty { get; }

	bool IsWide { get; }

	int Deck { get; }

	int Slot { get; }

	string ID { get; set; }

	DateTime? ReturnDate { get; set; }

	bool Excluded { get; set; }

	int StuckCount { get; set; }

	MerchFlags Flags { get; set; }
}

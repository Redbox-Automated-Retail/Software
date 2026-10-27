using System;

namespace Redbox.HAL.Client;

public interface IInventoryLocation
{
	Location Location { get; }

	string Matrix { get; }

	int EmptyStuck { get; }

	string MerchMetadata { get; }

	DateTime? ReturnTime { get; }

	bool Excluded { get; }
}

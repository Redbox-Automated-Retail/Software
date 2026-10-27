using System;

namespace Redbox.HAL.Component.Model;

public interface IDumpBinInventoryItem
{
	DateTime PutTime { get; }

	string ID { get; }
}

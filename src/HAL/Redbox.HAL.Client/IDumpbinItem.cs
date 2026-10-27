using System;

namespace Redbox.HAL.Client;

public interface IDumpbinItem
{
	string Matrix { get; }

	DateTime PutTime { get; }
}

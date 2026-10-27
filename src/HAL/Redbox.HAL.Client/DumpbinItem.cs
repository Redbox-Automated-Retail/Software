using System;

namespace Redbox.HAL.Client;

internal sealed class DumpbinItem : IDumpbinItem
{
	public string Matrix { get; private set; }

	public DateTime PutTime { get; private set; }

	internal DumpbinItem(string matrix, DateTime putTime)
	{
		Matrix = matrix;
		PutTime = putTime;
	}
}

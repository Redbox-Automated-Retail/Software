using System;

namespace Redbox.HAL.Component.Model;

public interface IFraudItem
{
	ILocation FiledAt { get; }

	DateTime DetectionTime { get; }

	string DataMatrix { get; }
}

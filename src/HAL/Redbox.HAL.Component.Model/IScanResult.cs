using System;
using System.Collections.Generic;

namespace Redbox.HAL.Component.Model;

public interface IScanResult : IDisposable
{
	bool DeviceError { get; }

	List<IDecodeResult> DecodeResults { get; }

	TimeSpan ExecutionTime { get; }

	string ImagePath { get; }
}

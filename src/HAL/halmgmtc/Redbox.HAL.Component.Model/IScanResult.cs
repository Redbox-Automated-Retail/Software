using System;
using System.Collections.Generic;

namespace Redbox.HAL.Component.Model;

public interface IScanResult : IDisposable
{
	bool DeviceError { get; }

	IList<IDecodeResult> DecodeResults { get; }

	TimeSpan ExecutionTime { get; }
}

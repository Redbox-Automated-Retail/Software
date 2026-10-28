using System;

namespace Redbox.HAL.Core;

public interface IScanResult
{
	TimeSpan ExecutionTime { get; }

	string[] ReadCodes { get; }
}

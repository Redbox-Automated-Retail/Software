using System;

namespace Redbox.Core;

public interface ISleepEvaluator
{
	int SleepMilliseconds(int retryCount, Exception exception);
}

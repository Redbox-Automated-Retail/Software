using System;

namespace Redbox.Core;

public interface IRethrowEvaluator
{
	bool Rethrow(int retryCount, Exception exception);
}

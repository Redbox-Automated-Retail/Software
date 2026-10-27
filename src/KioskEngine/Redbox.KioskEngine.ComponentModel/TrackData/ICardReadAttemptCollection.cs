using System;
using System.Collections;
using System.Collections.Generic;

namespace Redbox.KioskEngine.ComponentModel.TrackData;

public interface ICardReadAttemptCollection : IList<ICardReadAttempt>, ICollection<ICardReadAttempt>, IEnumerable<ICardReadAttempt>, IEnumerable, ITechnicalFallbackTracking
{
	int CancelledCount { get; }

	int TimedOutCount { get; }

	int SuccessCount { get; }

	int ErrorCount { get; }

	ICardReadAttempt GetMostRecent();

	ICardReadAttempt GetByCardReadJobCommandRequestId(Guid cardReadJobCommandRequestId);
}

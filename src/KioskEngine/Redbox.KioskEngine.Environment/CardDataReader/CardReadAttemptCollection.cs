using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Redbox.KioskEngine.ComponentModel.TrackData;

namespace Redbox.KioskEngine.Environment.CardDataReader;

public class CardReadAttemptCollection : List<ICardReadAttempt>, ICardReadAttemptCollection, IList<ICardReadAttempt>, ICollection<ICardReadAttempt>, IEnumerable<ICardReadAttempt>, IEnumerable, ITechnicalFallbackTracking
{
	public int CancelledCount => this.Count((ICardReadAttempt x) => x.CardReadAttemptStatus == CardReadAttemptStatus.Cancelled);

	public int TimedOutCount => this.Count((ICardReadAttempt x) => x.CardReadAttemptStatus == CardReadAttemptStatus.TimedOut);

	public int SuccessCount => this.Count((ICardReadAttempt x) => x.CardReadAttemptStatus == CardReadAttemptStatus.Success);

	public int ErrorCount => this.Count((ICardReadAttempt x) => x.CardReadAttemptStatus == CardReadAttemptStatus.Error);

	public bool SupportsTechnicalFallbackTracking => this.Any((ICardReadAttempt x) => x.SupportsTechnicalFallbackTracking);

	public bool IsInTrackableTechnicalFallback => this.Any((ICardReadAttempt x) => x.IsInTrackableTechnicalFallback);

	public ICardReadAttempt GetMostRecent()
	{
		ICardReadAttempt result = null;
		if (base.Count > 0)
		{
			result = this.FirstOrDefault(delegate(ICardReadAttempt x)
			{
				DateTime? startDateTime = x.StartDateTime;
				DateTime? dateTime = this.Max((ICardReadAttempt y) => y.StartDateTime);
				if (startDateTime.HasValue != dateTime.HasValue)
				{
					return false;
				}
				return !startDateTime.HasValue || startDateTime.GetValueOrDefault() == dateTime.GetValueOrDefault();
			});
		}
		return result;
	}

	public ICardReadAttempt GetByCardReadJobCommandRequestId(Guid cardReadJobCommandRequestId)
	{
		return this.FirstOrDefault((ICardReadAttempt x) => x.CardReadJobCommandRequestId == cardReadJobCommandRequestId);
	}
}

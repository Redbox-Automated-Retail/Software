using System;
using DeviceService.ComponentModel.Commands;

namespace Redbox.KioskEngine.ComponentModel.TrackData;

public interface ITrackDataService
{
	bool IsOlderReader { get; }

	bool IsNewReader { get; }

	void StartCardRead(CardReadRequestInfo request, Action<IReadCardJob> OnReadJobStart, Action<Guid> OnCardProcessing, Action<CardReadRequestInfo, CardReadCompleteDetails> onComplete);

	void StopCardRead(CancelReason cancelReason);

	void StopAllSessionCardReads(Guid sessionId, CancelReason cancelReason);
}

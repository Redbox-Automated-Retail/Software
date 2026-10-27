using System;
using DeviceService.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;

namespace Redbox.KioskEngine.ComponentModel.TrackData;

public interface ICardReadAttempt : ITechnicalFallbackTracking
{
	Guid CardReadJobCommandRequestId { get; set; }

	DateTime? StartDateTime { get; set; }

	DateTime? EndDateTime { get; set; }

	TimeSpan Duration { get; }

	DeviceInputType DeviceInputType { get; set; }

	ITrackData TrackData { get; set; }

	CardReadAttemptStatus CardReadAttemptStatus { get; set; }

	bool AttemptedCancel { get; set; }

	CancelReason CancelReason { get; set; }

	IAuthorizeResult AuthorizeResult { get; set; }
}

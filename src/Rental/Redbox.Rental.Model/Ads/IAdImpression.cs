using System;

namespace Redbox.Rental.Model.Ads;

public interface IAdImpression
{
	IAd Ad { get; set; }

	DateTime? StartDisplayTime { get; }

	DateTime? EndDisplayTime { get; }

	bool HasEnded { get; }

	bool IsDisplayTimeLongEnough { get; }

	int DisplayMilliseconds { get; }

	AdLocation AdLocation { get; set; }

	void Start();

	void End();
}

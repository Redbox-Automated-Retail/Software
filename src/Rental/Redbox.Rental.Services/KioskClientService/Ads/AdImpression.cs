using System;
using Redbox.Rental.Model.Ads;

namespace Redbox.Rental.Services.KioskClientService.Ads;

public class AdImpression : IAdImpression
{
	public IAd Ad { get; set; }

	public DateTime? StartDisplayTime { get; private set; }

	public bool HasStarted => StartDisplayTime.HasValue;

	public DateTime? EndDisplayTime { get; private set; }

	public bool HasEnded => EndDisplayTime.HasValue;

	public int DisplayMilliseconds
	{
		get
		{
			DateTime dateTime = (EndDisplayTime.HasValue ? EndDisplayTime.Value : DateTime.Now);
			if (HasStarted)
			{
				DateTime value = dateTime;
				DateTime? startDisplayTime = StartDisplayTime;
				if (value >= startDisplayTime)
				{
					return (int)(dateTime - StartDisplayTime.Value).TotalMilliseconds;
				}
			}
			return 0;
		}
	}

	public bool IsDisplayTimeLongEnough
	{
		get
		{
			if (Ad != null && HasStarted)
			{
				return DisplayMilliseconds >= Ad.MinimumDisplay;
			}
			return false;
		}
	}

	public AdLocation AdLocation { get; set; }

	public void Start()
	{
		if (!HasStarted)
		{
			StartDisplayTime = DateTime.Now;
		}
	}

	public void End()
	{
		if (!HasEnded)
		{
			EndDisplayTime = DateTime.Now;
		}
	}
}

using System;
using System.Linq;
using DeviceService.ComponentModel;

namespace Redbox.KioskEngine.ComponentModel.TrackData;

public static class ITrackDataExtension
{
	private static class ErrorCodes
	{
		public const string SwipedCardIsChipEnabled = "T020";

		public const string ReservationMobileDevice = "T022";
	}

	public static bool CardSourceTypeIsEMVContactOrQuickChip(this ITrackData td)
	{
		if (td != null)
		{
			if (td.CardSourceType != CardSourceType.EMVContact)
			{
				return td.CardSourceType == CardSourceType.QuickChip;
			}
			return true;
		}
		return false;
	}

	public static bool CardSourceTypeIsEMVContactlessOrMobileOrMSDContactless(this ITrackData td)
	{
		if (td != null)
		{
			if (td.CardSourceType != CardSourceType.EMVContactless && td.CardSourceType != CardSourceType.Mobile)
			{
				return td.CardSourceType == CardSourceType.MSDContactless;
			}
			return true;
		}
		return false;
	}

	public static bool CardSourceTypeIsMobileAndHasPayAndNotVas(this ITrackData td)
	{
		if (td != null && td.CardSourceType == CardSourceType.Mobile && td.HasPay)
		{
			return !td.HasVas;
		}
		return false;
	}

	public static bool WalletTypeIsGoogleAndHasPayAndHasVas(this ITrackData td)
	{
		if (td != null && td.WalletType == WalletType.Google && td.HasPay)
		{
			return td.HasVas;
		}
		return false;
	}

	public static bool WalletTypeIsGoogleAndNotVas(this ITrackData td)
	{
		if (td != null && td.WalletType == WalletType.Google)
		{
			return !td.HasVas;
		}
		return false;
	}

	public static bool HasErrors(this ITrackData td)
	{
		if (td != null && td.Errors != null)
		{
			return td.Errors.Count > 0;
		}
		return false;
	}

	public static bool HasSwipedCardIsChipEnabledError(this ITrackData td)
	{
		if (td.HasErrors())
		{
			return td.Errors.ContainsCode("T020");
		}
		return false;
	}

	public static bool HasReservationMobileDeviceError(this ITrackData td)
	{
		if (td.HasErrors())
		{
			return td.Errors.ContainsCode("T022");
		}
		return false;
	}

	public static bool HasTamperedError(this ITrackData td)
	{
		if (td.HasErrors())
		{
			return td.Errors.Any((Error e) => string.Equals(e.Code, "TAMPERED", StringComparison.CurrentCultureIgnoreCase));
		}
		return false;
	}
}

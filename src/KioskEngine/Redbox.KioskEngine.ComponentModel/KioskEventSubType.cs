using System.Collections.Generic;

namespace Redbox.KioskEngine.ComponentModel;

public static class KioskEventSubType
{
	public const int KioskHealth_TouchScreenInactivity = 100;

	public const int KioskHealth_TouchScreenReset = 101;

	public const int KioskHealth_TouchScreenActivity = 102;

	public const int KioskHealth_ArcusReset = 200;

	public const int KisokHealth_ArcusResetVendStatus = 201;

	public const int KioskHealth_RouterIsOffline = 300;

	public const int KioskHealth_RouterReset = 301;

	public const int KioskHealth_RouterIsOnline = 302;

	public const int KioskHealth_HardwareStatNone = 400;

	public const int KioskHealth_HardwareStatArcus = 401;

	public const int KioskHealth_HardwareStatCamera = 402;

	public const int KioskHealth_HardwareStatTouchscreen = 403;

	public const int KioskHealth_HardwareStatControllerRestart = 404;

	public const int KioskHealth_HardwareStatRouterRecycle = 405;

	public const int KioskHealth_HardwareStatUnexpectedPowerLoss = 406;

	public const int KioskHealth_ViewTimeout = 500;

	public const int KioskHealth_ViewReset = 501;

	public const int KioskHealth_ViewActivity = 502;

	public const int KioskHealth_CCReaderInactivity = 600;

	public const int KioskHealth_CCReaderReadError = 601;

	public const int KioskHealth_CCReaderActivity = 602;

	public const int KioskHealth_EMVReaderTechnicalFallbackSession = 700;

	public const int KioskHealth_EMVReaderTechnicalFallbackSessionReset = 701;

	private static readonly IDictionary<int, string> Map = new Dictionary<int, string>
	{
		{ 100, "KioskHealth_TouchScreenInactivity" },
		{ 101, "KioskHealth_TouchScreenReset" },
		{ 102, "KioskHealth_TouchScreenActivity" },
		{ 200, "KioskHealth_ArcusReset" },
		{ 201, "KisokHealth_ArcusResetVendStatus" },
		{ 300, "KioskHealth_RouterIsOffline" },
		{ 301, "KioskHealth_RouterReset" },
		{ 302, "KioskHealth_RouterIsOnline" },
		{ 400, "KioskHealth_HardwareStatNone" },
		{ 401, "KioskHealth_HardwareStatArcus" },
		{ 402, "KioskHealth_HardwareStatCamera" },
		{ 403, "KioskHealth_HardwareStatTouchscreen" },
		{ 404, "KioskHealth_HardwareStatControllerRestart" },
		{ 405, "KioskHealth_HardwareStatRouterRecycle" },
		{ 406, "KioskHealth_HardwareStatUnexpectedPowerLoss" },
		{ 500, "KioskHealth_ViewTimeout" },
		{ 501, "KioskHealth_ViewReset" },
		{ 502, "KioskHealth_ViewActivity" },
		{ 600, "KioskHealth_CCReaderInactivity" },
		{ 601, "KioskHealth_CCReaderReadError" },
		{ 602, "KioskHealth_CCReaderActivity" },
		{ 700, "KioskHealth_EMVReaderTechnicalFallbackSession" },
		{ 701, "KioskHealth_EMVReaderTechnicalFallbackSessionReset" }
	};

	public static string GetName(int value)
	{
		if (!Map.ContainsKey(value))
		{
			return "";
		}
		return Map[value];
	}
}

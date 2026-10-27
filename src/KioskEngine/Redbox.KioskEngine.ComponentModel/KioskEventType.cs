using System.Collections.Generic;

namespace Redbox.KioskEngine.ComponentModel;

public static class KioskEventType
{
	public const int KioskHealthTouchScreen = 1;

	public const int KioskHealthArcus = 2;

	public const int KioskHealthRouter = 3;

	public const int KioskHealthHardwareCorrectionStat = 4;

	public const int KioskHealthView = 5;

	public const int KioskHealthCCReader = 6;

	public const int KioskHealthEMVReader = 7;

	private static readonly IDictionary<int, string> Map = new Dictionary<int, string>
	{
		{ 1, "KioskHealthTouchScreen" },
		{ 2, "KioskHealthArcus" },
		{ 3, "KioskHealthRouter" },
		{ 4, "KioskHealthHardwareCorrectionStat" },
		{ 5, "KioskHealthView" },
		{ 6, "KioskHealthCCReader" },
		{ 7, "KioskHealthEMVReader" }
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

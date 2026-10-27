using System;

namespace Redbox.HAL.Component.Model;

public static class InventoryConstants
{
	public const string Empty = "EMPTY";

	public const string Unknown = "UNKNOWN";

	public const string Duplicate = "(DUPLICATE)";

	public const string Backwards = "redbox";

	public static bool CodeIsUnknown(string s)
	{
		if (!"UNKNOWN".Equals(s, StringComparison.CurrentCultureIgnoreCase))
		{
			return "redbox".Equals(s, StringComparison.CurrentCultureIgnoreCase);
		}
		return true;
	}

	public static bool IsKnownInventoryToken(string id)
	{
		if (string.Compare(id, "UNKNOWN", ignoreCase: true) != 0 && string.Compare(id, "redbox", ignoreCase: true) != 0)
		{
			return string.Compare(id, "EMPTY", ignoreCase: true) == 0;
		}
		return true;
	}
}

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Redbox.Core;

internal static class Redbox_002EMacros1309_002EStringCollectionExtensions
{
	public static List<string> ToList(this StringCollection value)
	{
		return new List<string>(value.Cast<string>());
	}
}

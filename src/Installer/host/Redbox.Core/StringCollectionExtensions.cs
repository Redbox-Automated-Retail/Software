using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Redbox.Core;

public static class StringCollectionExtensions
{
	public static List<string> ToList(this StringCollection value)
	{
		return new List<string>(value.Cast<string>());
	}
}

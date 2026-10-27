using System.Collections.Generic;
using System.Linq;

namespace Redbox.KioskEngine.ComponentModel;

public static class DictionaryExtensions
{
	public static Dictionary<TKey, TValue> ShallowCopy<TKey, TValue>(this IDictionary<TKey, TValue> source)
	{
		return source.ToDictionary((KeyValuePair<TKey, TValue> kvp) => kvp.Key, (KeyValuePair<TKey, TValue> kvp) => kvp.Value);
	}
}

using System.Collections.Generic;
using System.Collections.ObjectModel;
using Redbox.Core;

namespace Redbox.GetOpts;

public class OptionValueList : LinkedList<OptionValue>
{
	public void WriteOptions(IConsole console)
	{
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			OptionValue current = enumerator.Current;
			console.WriteLine(current);
			console.WriteLine();
		}
	}

	public OptionValue GetOptionValue(string name)
	{
		using (Enumerator enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				OptionValue current = enumerator.Current;
				if (current.IsValidName(name))
				{
					return current;
				}
			}
		}
		return null;
	}

	public ReadOnlyCollection<string> GetRequiredOptions()
	{
		List<string> list = new List<string>();
		using (Enumerator enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				OptionValue current = enumerator.Current;
				if (current.Required)
				{
					list.Add(current.GetName());
				}
			}
		}
		return list.AsReadOnly();
	}
}

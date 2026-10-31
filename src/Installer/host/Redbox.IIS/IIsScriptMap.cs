using System.Collections.Generic;
using System.Text;

namespace Redbox.IIS;

public class IIsScriptMap
{
	private List<string> m_verbs;

	public string Extension { get; set; }

	public List<string> Verbs
	{
		get
		{
			if (m_verbs == null)
			{
				m_verbs = new List<string>();
			}
			return m_verbs;
		}
	}

	public string ExecutablePath { get; set; }

	public IIsScriptMapAction? Action { get; set; }

	public IIsScriptMapOption Options { get; set; }

	public static IIsScriptMap Parse(string metabaseEntry)
	{
		if (metabaseEntry == null)
		{
			return null;
		}
		string[] array = metabaseEntry.Split(",".ToCharArray(), 4);
		if (array.Length < 2)
		{
			return null;
		}
		IIsScriptMap isScriptMap = new IIsScriptMap();
		isScriptMap.Extension = array[0];
		isScriptMap.ExecutablePath = array[1];
		isScriptMap.Options = (IIsScriptMapOption)int.Parse(array[2]);
		IIsScriptMap isScriptMap2 = isScriptMap;
		if (array.Length == 4)
		{
			isScriptMap2.Verbs.AddRange(array[3].Split(",".ToCharArray()));
		}
		return isScriptMap2;
	}

	public override string ToString()
	{
		return $"{Extension},{ExecutablePath},{(int)Options}{GetVerbList()}";
	}

	private string GetVerbList()
	{
		if (Verbs.Count == 0)
		{
			return null;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(",");
		foreach (string verb in Verbs)
		{
			if (stringBuilder.Length > 1)
			{
				stringBuilder.Append(",");
			}
			stringBuilder.Append(verb.ToUpper());
		}
		return stringBuilder.ToString();
	}
}

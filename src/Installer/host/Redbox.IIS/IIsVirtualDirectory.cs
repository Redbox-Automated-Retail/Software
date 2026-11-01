using System.Collections.Generic;
using System.DirectoryServices;

namespace Redbox.IIS;

public class IIsVirtualDirectory : IIsObject
{
	public string Path
	{
		get
		{
			return base.Entry.Properties["Path"][0].ToString();
		}
		set
		{
			base.Entry.Properties["Path"][0] = value;
		}
	}

	public bool AuthNTLM
	{
		get
		{
			return (bool)base.Entry.Properties["AuthNTLM"][0];
		}
		set
		{
			base.Entry.Properties["AuthNTLM"][0] = value;
		}
	}

	public bool AccessRead
	{
		get
		{
			return (bool)base.Entry.Properties["AccessRead"][0];
		}
		set
		{
			base.Entry.Properties["AccessRead"][0] = value;
		}
	}

	public string AppPoolID
	{
		get
		{
			return base.Entry.Properties["AppPoolID"][0].ToString();
		}
		set
		{
			base.Entry.Properties["AppPoolID"][0] = value;
		}
	}

	public bool AccessWrite
	{
		get
		{
			return (bool)base.Entry.Properties["AccessWrite"][0];
		}
		set
		{
			base.Entry.Properties["AccessWrite"][0] = value;
		}
	}

	public bool AccessScript
	{
		get
		{
			return (bool)base.Entry.Properties["AccessScript"][0];
		}
		set
		{
			base.Entry.Properties["AccessScript"][0] = value;
		}
	}

	public bool AccessExecute
	{
		get
		{
			return (bool)base.Entry.Properties["AccessExecute"][0];
		}
		set
		{
			base.Entry.Properties["AccessExecute"][0] = value;
		}
	}

	public bool AuthAnonymous
	{
		get
		{
			return (bool)base.Entry.Properties["AuthAnonymous"][0];
		}
		set
		{
			base.Entry.Properties["AuthAnonymous"][0] = value;
		}
	}

	public string DefaultDocument
	{
		get
		{
			return base.Entry.Properties["DefaultDoc"][0].ToString();
		}
		set
		{
			base.Entry.Properties["DefaultDoc"][0] = value;
		}
	}

	public bool EnableDirBrowsing
	{
		get
		{
			return (bool)base.Entry.Properties["EnableDirBrowsing"][0];
		}
		set
		{
			base.Entry.Properties["EnableDirBrowsing"][0] = value;
		}
	}

	public bool EnableDefaultDoc
	{
		get
		{
			return (bool)base.Entry.Properties["EnableDefaultDoc"][0];
		}
		set
		{
			base.Entry.Properties["EnableDefaultDoc"][0] = value;
		}
	}

	public string AppFriendlyName
	{
		get
		{
			return base.Entry.Properties["AppFriendlyName"][0].ToString();
		}
		set
		{
			base.Entry.Properties["AppFriendlyName"][0] = value;
		}
	}

	public List<IIsScriptMap> ScriptMaps
	{
		get
		{
			List<IIsScriptMap> list = new List<IIsScriptMap>();
			foreach (string item in base.Entry.Properties["ScriptMaps"])
			{
				list.Add(IIsScriptMap.Parse(item));
			}
			return list;
		}
		set
		{
			List<IIsScriptMap> scriptMaps = ScriptMaps;
			foreach (IIsScriptMap item in value)
			{
				string currentExtension = item.Extension;
				int existingIndex = scriptMaps.FindIndex((IIsScriptMap eachExistingMap) => string.Compare(eachExistingMap.Extension, currentExtension, ignoreCase: true) == 0);
				if (existingIndex != -1)
				{
					scriptMaps[existingIndex] = item;
				}
				else
				{
					scriptMaps.Add(item);
				}
			}
			string[] metabaseValues = new string[scriptMaps.Count];
			int index = 0;
			foreach (IIsScriptMap scriptMap in scriptMaps)
			{
				metabaseValues[index++] = scriptMap.ToString();
			}
			base.Entry.Properties["ScriptMaps"].Value = metabaseValues;
		}
	}

	internal IIsVirtualDirectory(DirectoryEntry entry)
		: base(entry)
	{
	}

	public void CreateApplication(string appPoolName, ApplicationIsolation isolation)
	{
		if (base.Entry == null)
		{
			return;
		}
		try
		{
			base.Entry.Invoke("AppCreate3", isolation, appPoolName, false);
		}
		catch
		{
		}
	}

	public void Delete()
	{
		try
		{
			if (base.Name.ToUpper() != "ROOT")
			{
				base.Entry.Parent.Children.Remove(base.Entry);
			}
		}
		catch
		{
		}
	}
}

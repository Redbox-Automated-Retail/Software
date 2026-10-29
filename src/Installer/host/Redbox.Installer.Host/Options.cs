using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Redbox.Core;
using Redbox.GetOpts;

namespace Redbox.Installer.Host;

[Usage("setup [-D:key=value ...] [--nosplash] [--silent] [--remove] [--force] [--log] [--logsource]")]
public class Options
{
	[Description("Run installer in silent mode, without a user interface.  Apply any properties to the necessary prompts.")]
	[Option]
	public bool Silent;

	[Option]
	[Description("Remove the packages installed by this setup program.")]
	public bool Remove;

	[Description("Suppress splash screen on startup.")]
	[Option]
	public bool NoSplash;

	[Option]
	[Description("Force the creation of the installation path if run in silent mode.")]
	public bool Force;

	[Description("File name the installer host will use for logging.")]
	[Option(LongName = "log")]
	public string LogFile;

	[Option(LongName = "logsource")]
	[Description("Output generated JScript.NET source for dianostic purposes.")]
	public bool LogGeneratedSource;

	[Option(ShortName = "D")]
	[Description("Create/set the value of property specified by key to the value.")]
	public IDictionary<string, string> Properties = new Dictionary<string, string>();

	internal void LoadPropertiesFromResource(LibraryModule module)
	{
		byte[] resource = ResourceTool.GetResource(module, "PREREQ", "PROPS");
		if (resource == null)
		{
			return;
		}
		using MemoryStream serializationStream = new MemoryStream(resource);
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		IDictionary<string, string> dictionary = (IDictionary<string, string>)binaryFormatter.Deserialize(serializationStream);
		foreach (string key in dictionary.Keys)
		{
			if (!Properties.ContainsKey(key))
			{
				Properties[key] = dictionary[key];
			}
		}
	}

	private Options()
	{
	}
}

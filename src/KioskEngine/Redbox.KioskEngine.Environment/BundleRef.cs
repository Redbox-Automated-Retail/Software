using System.Collections.Generic;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Environment;

public class BundleRef : IBundleRef
{
	public string Name { get; set; }

	public string Path { get; set; }

	public IDictionary<string, IBuildSetting> BuildSettings { get; set; }

	public BundleRef()
	{
		BuildSettings = new Dictionary<string, IBuildSetting>();
	}
}

using System;
using System.Collections.Generic;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Environment;

public class BuildSetting : IBuildSetting
{
	public bool IsActive { get; set; }

	public DateTime? BuiltOn { get; set; }

	public string OutputName { get; set; }

	public IDictionary<string, string> Properties { get; set; }

	public BuildSetting()
	{
		Properties = new Dictionary<string, string>();
	}
}

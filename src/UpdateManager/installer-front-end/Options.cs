using System.Collections.Generic;
using System.ComponentModel;
using Redbox.Core;
using Redbox.GetOpts;

namespace Redbox.UpdateManager.StoreInstallerFrontEnd;

[Usage("installer --store-number:number | --kaseya-id:[redbox-]<id>.<group-name>\r\nAlways give a store number or a kaseya id.")]
public class Options
{
	[Option(ShortName = "D")]
	[Description("Define or override property key/value pairs.")]
	public IDictionary<string, string> Properties = new Dictionary<string, string>();

	[Option(LongName = "eval")]
	[Description("Executes the script specified in the store updater kernel. All other options ignored.")]
	public string Script { get; set; }

	[Option(LongName = "store-number", ShortName = "n")]
	[Description("Number of Current Store.")]
	public string StoreNumber { get; set; }

	private Options()
	{
	}
}

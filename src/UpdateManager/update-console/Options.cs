using System.Collections.Generic;
using System.ComponentModel;
using Redbox.Core;
using Redbox.GetOpts;

namespace Redbox.UpdateManager.Tool;

[Usage("update-console --store-number:number | --kaseya-id:[redbox-]<id>.<group-name>\r\nAlways give a store number or a kaseya id.")]
public class Options
{
	[Option(ShortName = "D")]
	[Description("Define or override property key/value pairs.")]
	public IDictionary<string, string> Properties = new Dictionary<string, string>();

	[Option(LongName = "update-service-url", ShortName = "h", Required = false)]
	[Description("Update Service Url override.")]
	public string UpdateServiceUrl { get; set; }

	[Option(LongName = "activate", ShortName = "a")]
	[Description("Activate staged files.")]
	public bool Activate { get; set; }

	[Option(LongName = "diff", ShortName = "d")]
	[Description("Display the differnce in the current tree and what is avaliable to update to.")]
	public bool Diff { get; set; }

	[Option(LongName = "eval")]
	[Description("Executes the script specified in the store updater kernel. All other options ignored.")]
	public string Script { get; set; }

	[Option(LongName = "hash", ShortName = "h")]
	[Description("Desired version for an update.")]
	public string Hash { get; set; }

	[Option(LongName = "kaseya-id", ShortName = "k")]
	[Description("Kaseya Machine Id of Current Store.")]
	public string KaseyaId { get; set; }

	[Option(LongName = "list", ShortName = "l")]
	[Description("Lists the current applied revisions.")]
	public bool List { get; set; }

	[Option(LongName = "list-all")]
	[Description("Lists the all revision present on the machine applied or otherwise.")]
	public bool ListAll { get; set; }

	[Option(LongName = "store-number", ShortName = "n")]
	[Description("Number of Current Store.")]
	public string StoreNumber { get; set; }

	[Option(LongName = "start-download")]
	[Description("Attempt to start downloads.")]
	public bool StartDownload { get; set; }

	[Option(LongName = "poll", ShortName = "p")]
	[Description("Cause the underlying system to poll for changes from the server and BITS.")]
	public bool Poll { get; set; }

	[Option(LongName = "reset", ShortName = "r")]
	[Description("Deletes all repository entries and cancels all bits jobs.")]
	public bool ResetAll { get; set; }

	[Option(LongName = "tree", ShortName = "t")]
	[Description("The name of the tree to operate on. Required for update")]
	public string Tree { get; set; }

	[Option(LongName = "update", ShortName = "u")]
	[Description("Updates the tree given in name to the the revision specified in hash or head if none.")]
	public bool Update { get; set; }

	[Option(LongName = "filter", ShortName = "f")]
	[Description("work filter to use when getting work from server.")]
	public string WorkFilter { get; set; }

	[Option(LongName = "reset-bits")]
	[Description("Cancels all BITS jobs.")]
	public bool ResetBITS { get; set; }

	[Option(LongName = "reset-repository")]
	[Description("Cancels all BITS jobs.")]
	public bool ResetManifest { get; set; }

	[Option(LongName = "print-unfinished-changes")]
	[Description("Prints unfinished changes.")]
	public bool PrintUnfinishedChanges { get; set; }

	[Option(LongName = "repository-name")]
	[Description("Sets the repository name.")]
	public string RepositoryName { get; set; }

	[Option(LongName = "dump")]
	[Description("Formats and outputs a repository file. Valid values: active, current, label, revlog.")]
	public string Dump { get; set; }

	private Options()
	{
	}
}

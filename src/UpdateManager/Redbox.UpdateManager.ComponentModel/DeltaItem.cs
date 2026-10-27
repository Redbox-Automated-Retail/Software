using System.IO;

namespace Redbox.UpdateManager.ComponentModel;

public class DeltaItem
{
	public string Revision { get; set; }

	public string VersionHash { get; set; }

	public string ContentHash { get; set; }

	public string TargetName { get; set; }

	public string TargetPath { get; set; }

	public bool IsSeed { get; set; }

	public bool IsPlaceHolder { get; set; }

	public string FormatFileName()
	{
		return Path.Combine(TargetPath, TargetName);
	}
}

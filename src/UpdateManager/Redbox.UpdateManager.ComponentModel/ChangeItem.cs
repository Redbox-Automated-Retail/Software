using System.IO;

namespace Redbox.UpdateManager.ComponentModel;

public class ChangeItem
{
	// The string properties are set by the object initializer that creates the item.
	public string VersionHash { get; set; } = null!;

	public string ContentHash { get; set; } = null!;

	public string TargetName { get; set; } = null!;

	public string TargetPath { get; set; } = null!;

	public bool IsSeed { get; set; }

	public bool Composite { get; set; }

	public string FormatFileName()
	{
		return Path.Combine(TargetPath, TargetName);
	}
}

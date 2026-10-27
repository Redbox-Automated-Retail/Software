namespace Redbox.HAL.Component.Model;

public sealed class ZipResult
{
	public bool Success { get; internal set; }

	public int EntryCount { get; internal set; }

	public string ZipFile { get; internal set; }
}

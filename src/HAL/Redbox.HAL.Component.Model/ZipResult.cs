namespace Redbox.HAL.Component.Model;

public sealed class ZipResult
{
	public bool Success { get; internal set; }

	public int EntryCount { get; internal set; }

	// Set by ZipFileService.Zip's object initializer.
	public string ZipFile { get; internal set; } = null!;
}

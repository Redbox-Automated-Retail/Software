namespace Redbox.HAL.Component.Model;

public sealed class UnzipResult
{
	// SourceZip and OutputPath are set by ZipFileService's object initializers.
	public string SourceZip { get; internal set; } = null!;

	public string OutputPath { get; internal set; } = null!;

	public bool Success { get; internal set; }

	public int EntryCount { get; internal set; }
}

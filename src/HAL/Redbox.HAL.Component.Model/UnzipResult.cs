namespace Redbox.HAL.Component.Model;

public sealed class UnzipResult
{
	public string SourceZip { get; internal set; }

	public string OutputPath { get; internal set; }

	public bool Success { get; internal set; }

	public int EntryCount { get; internal set; }
}

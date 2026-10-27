namespace Redbox.HAL.Configuration;

public sealed class GampBackupResult
{
	public bool Success { get; internal set; }

	public string OriginalFile { get; internal set; }

	public string BackupFile { get; internal set; }

	internal GampBackupResult()
	{
		Success = false;
		OriginalFile = (BackupFile = string.Empty);
	}
}

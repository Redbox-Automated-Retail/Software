using ICSharpCode.SharpZipLib.Zip;

namespace Redbox.Compression;

public static class ZipHelper
{
	public static void CreateZipFile(string zipFileName, string sourcePath, bool recurse, string fileFilter)
	{
		FastZip fastZip = new FastZip();
		fastZip.CreateZip(zipFileName, sourcePath, recurse, fileFilter);
	}

	public static void CreateZipFile(string zipFileName, string sourcePath, bool recurse, string fileFilter, string directoryFilter)
	{
		FastZip fastZip = new FastZip();
		fastZip.CreateZip(zipFileName, sourcePath, recurse, fileFilter, directoryFilter);
	}

	public static void ExtractAllFiles(string fileName, string path)
	{
		FastZip fastZip = new FastZip();
		fastZip.ExtractZip(fileName, path, FastZip.Overwrite.Always, null, null, null, restoreDateTime: true);
	}
}

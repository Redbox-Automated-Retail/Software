using System.Collections.Generic;
using System.IO;
using ICSharpCode.SharpZipLib.Tar;

namespace Redbox.Compression;

public static class TarHelper
{
	public static void Archive(string sourceRoot, string target)
	{
		using FileStream outputStream = File.Create(target);
		using TarArchive tarArchive = TarArchive.CreateOutputTarArchive(outputStream);
		tarArchive.AsciiTranslate = false;
		tarArchive.ApplyUserInfoOverrides = false;
		tarArchive.SetKeepOldFiles(keepOldFiles: false);
		tarArchive.WriteEntry(TarEntry.CreateEntryFromFile(sourceRoot), recurse: true);
	}

	public static void Archive(string target, Dictionary<string, string> files, bool shouldStripPaths)
	{
		using FileStream outputStream = File.Create(target);
		using TarArchive tarArchive = TarArchive.CreateOutputTarArchive(outputStream);
		tarArchive.AsciiTranslate = false;
		tarArchive.ApplyUserInfoOverrides = false;
		tarArchive.SetKeepOldFiles(keepOldFiles: false);
		foreach (KeyValuePair<string, string> file in files)
		{
			TarEntry tarEntry = TarEntry.CreateEntryFromFile(file.Value);
			tarEntry.Name = (shouldStripPaths ? Path.GetFileName(file.Key) : file.Key);
			tarArchive.WriteEntry(tarEntry, recurse: false);
		}
	}

	public static void Unpack(string source, string root)
	{
		if (!Directory.Exists(root))
		{
			throw new DirectoryNotFoundException(root);
		}
		using FileStream inputStream = File.OpenRead(source);
		using TarArchive tarArchive = TarArchive.CreateInputTarArchive(inputStream);
		tarArchive.AsciiTranslate = false;
		tarArchive.ApplyUserInfoOverrides = false;
		tarArchive.SetKeepOldFiles(keepOldFiles: false);
		tarArchive.ExtractContents(root);
	}
}

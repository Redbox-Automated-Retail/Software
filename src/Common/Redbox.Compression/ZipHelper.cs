using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

namespace Redbox.Compression;

public static class ZipHelper
{
	public static void CreateZipFile(string zipFileName, string sourcePath, bool recurse, string fileFilter)
	{
		new FastZip().CreateZip(zipFileName, sourcePath, recurse, fileFilter);
	}

	public static void CreateZipFile(string zipFileName, string sourcePath, bool recurse, string fileFilter, string directoryFilter)
	{
		new FastZip().CreateZip(zipFileName, sourcePath, recurse, fileFilter, directoryFilter);
	}

	public static void CreateZipFile(string zipFilename, string sourcePath, bool recurse, string fileFilter, string directoryFilter, IDictionary<string, string> metadata)
	{
		ZipFile zipFile = ZipFile.Create(zipFilename);
		FileSystemScanner fileSystemScanner2;
		FileSystemScanner fileSystemScanner = (fileSystemScanner2 = new FileSystemScanner(fileFilter, directoryFilter));
		fileSystemScanner2.ProcessFile = (ProcessFileHandler)Delegate.Combine(fileSystemScanner2.ProcessFile, (ProcessFileHandler)delegate(object s, ScanEventArgs e)
		{
			string entryName = e.Name.Substring(sourcePath.Length);
			zipFile.Add(e.Name, entryName);
		});
		JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
		if (metadata == null)
		{
			metadata = new Dictionary<string, string>();
		}
		string comment = javaScriptSerializer.Serialize(metadata);
		zipFile.BeginUpdate();
		zipFile.SetComment(comment);
		fileSystemScanner.Scan(sourcePath, recurse);
		zipFile.CommitUpdate();
		zipFile.Close();
	}

	public static IDictionary<string, string> GetMetadata(string filename)
	{
		string zipFileComment = new ZipFile(filename).ZipFileComment;
		if (string.IsNullOrEmpty(zipFileComment))
		{
			return new Dictionary<string, string>();
		}
		return new JavaScriptSerializer().Deserialize<IDictionary<string, string>>(zipFileComment);
	}

	public static void ExtractAllFiles(string fileName, string path)
	{
		new FastZip().ExtractZip(fileName, path, FastZip.Overwrite.Always, null, null, null, restoreDateTime: true);
	}
}

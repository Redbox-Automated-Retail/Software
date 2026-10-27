using System;
using System.Collections.ObjectModel;
using System.IO;
using Redbox.Compression;
using Redbox.Core;
using Redbox.Lua;
using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.Kernel;

public static class FileSystemFunctions
{
	[KernelFunction(Name = "kernel.getfileinfo")]
	internal static LuaTable GetFileInfo(string file)
	{
		LuaTable luaTable = new LuaTable(KernelService.Instance.LuaRuntime);
		FileInfo fileInfo = new FileInfo(file);
		luaTable["directory"] = fileInfo.DirectoryName;
		luaTable["length"] = fileInfo.Length;
		luaTable["name"] = fileInfo.Name;
		luaTable["exists"] = fileInfo.Exists;
		luaTable["extension"] = fileInfo.Extension;
		luaTable["full_name"] = fileInfo.FullName;
		luaTable["is_read_only"] = fileInfo.IsReadOnly;
		luaTable["last_write_time"] = fileInfo.LastWriteTime.ToString("yyyyMMddHHmmss");
		return luaTable;
	}

	[KernelFunction(Name = "kernel.getfilelength")]
	internal static long GetFileLength(string file)
	{
		return new FileInfo(file).Length;
	}

	[KernelFunction(Name = "kernel.tar")]
	internal static bool Tar(string source, string target)
	{
		try
		{
			IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
			string text = service.ExpandProperties(source);
			string text2 = service.ExpandProperties(target);
			LogHelper.Instance.Log("TAR: source={0}, target={1}", text, text2);
			TarHelper.Archive(text, text2);
			return true;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in FileSystemFunctions.Tar.", e);
		}
		return false;
	}

	[KernelFunction(Name = "kernel.untar")]
	internal static bool UnTar(string source, string target)
	{
		try
		{
			IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
			TarHelper.Unpack(service.ExpandProperties(source), service.ExpandProperties(target));
			return true;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in FileSystemFunctions.UnTar.", e);
		}
		return false;
	}

	[KernelFunction(Name = "kernel.zip")]
	internal static bool Zip(string source, string target, bool recurse, string fileFilter, string dirFilter)
	{
		try
		{
			IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
			string text = service.ExpandProperties(source);
			string text2 = service.ExpandProperties(target);
			string text3 = service.ExpandProperties(fileFilter);
			string text4 = service.ExpandProperties(dirFilter);
			LogHelper.Instance.Log("ZIP: source={0}, target={1}, recurse={2}, fileFilter={3}, dirFilter={4}", text, text2, recurse, text3, text4);
			if (string.IsNullOrEmpty(dirFilter))
			{
				ZipHelper.CreateZipFile(text2, text, recurse, text3);
			}
			else
			{
				ZipHelper.CreateZipFile(text2, text, recurse, text3, text4);
			}
			return true;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in FileSystemFunctions.Zip.", e);
		}
		return false;
	}

	[KernelFunction(Name = "kernel.unzip")]
	internal static bool UnZip(string source, string target)
	{
		try
		{
			IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
			string text = service.ExpandProperties(source);
			string text2 = service.ExpandProperties(target);
			LogHelper.Instance.Log("UNZIP: source={0}, target={1}", text, text2);
			ZipHelper.ExtractAllFiles(text, text2);
			return true;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in FileSystemFunctions.UnZip.", e);
		}
		return false;
	}

	[KernelFunction(Name = "kernel.gzipdecompress")]
	internal static bool GzipDecompress(string source, string target)
	{
		try
		{
			IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
			string text = service.ExpandProperties(source);
			string path = service.ExpandProperties(target);
			if (!File.Exists(text))
			{
				throw new FileNotFoundException("File does not exists and can not be decompressed.", text);
			}
			CompressionAlgorithm algorithm = CompressionAlgorithm.GetAlgorithm(CompressionType.GZip);
			using (FileStream source2 = File.OpenRead(text))
			{
				using FileStream target2 = File.Create(path);
				algorithm.Decompress(source2, target2);
			}
			return true;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in FileSystemFunctions.GzipDecompress.", e);
		}
		return false;
	}

	[KernelFunction(Name = "kernel.gzipcompress")]
	internal static bool GzipCompress(string source, string target)
	{
		try
		{
			IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
			string text = service.ExpandProperties(source);
			string path = service.ExpandProperties(target);
			if (!File.Exists(text))
			{
				throw new FileNotFoundException("File does not exists and can not be decompressed.", text);
			}
			CompressionAlgorithm algorithm = CompressionAlgorithm.GetAlgorithm(CompressionType.GZip);
			using (FileStream source2 = File.OpenRead(text))
			{
				using FileStream target2 = File.Create(path);
				algorithm.Compress(source2, target2);
			}
			return true;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in FileSystemFunctions.GzipCompress.", e);
		}
		return false;
	}

	[KernelFunction(Name = "kernel.getdirectoryfrompath")]
	internal static string GetDirectoryNameFromPath(string fullPath)
	{
		return Path.GetDirectoryName(ServiceLocator.Instance.GetService<IMacroService>().ExpandProperties(fullPath));
	}

	[KernelFunction(Name = "kernel.getfilenamefrompath")]
	internal static string GetFileNameFromPath(string fullPath)
	{
		return Path.GetFileName(ServiceLocator.Instance.GetService<IMacroService>().ExpandProperties(fullPath));
	}

	[KernelFunction(Name = "kernel.getfilenamewithoutextension")]
	internal static string GetFileNameWithOutExtension(string fullPath)
	{
		return Path.GetFileNameWithoutExtension(ServiceLocator.Instance.GetService<IMacroService>().ExpandProperties(fullPath));
	}

	[KernelFunction(Name = "kernel.directorycreate")]
	internal static void CreateDirectory(string path)
	{
		Directory.CreateDirectory(ServiceLocator.Instance.GetService<IMacroService>().ExpandProperties(path));
	}

	[KernelFunction(Name = "kernel.directorycopy")]
	internal static bool DirectoryCopy(string source, string target)
	{
		try
		{
			IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
			DirectoryInfo source2 = new DirectoryInfo(service.ExpandProperties(source));
			DirectoryInfo target2 = new DirectoryInfo(service.ExpandProperties(target));
			DirectoryCopyInternal(source2, target2);
			return true;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in FileSystemFunctions.FileCopy.", e);
		}
		return false;
	}

	[KernelFunction(Name = "kernel.filecopy")]
	internal static bool FileCopy(string source, string target)
	{
		try
		{
			IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
			File.Copy(service.ExpandProperties(source), service.ExpandProperties(target), overwrite: true);
			return true;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in FileSystemFunctions.FileCopy.", e);
		}
		return false;
	}

	[KernelFunction(Name = "kernel.filemove")]
	internal static bool FileMove(string source, string target)
	{
		try
		{
			IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
			File.Move(service.ExpandProperties(source), service.ExpandProperties(target));
			return true;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in FileSystemFunctions.FileMove.", e);
		}
		return false;
	}

	[KernelFunction(Name = "kernel.filemoveandoverwrite")]
	internal static bool FileMoveOverwrite(string source, string target)
	{
		try
		{
			IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
			File.Delete(service.ExpandProperties(target));
			File.Move(service.ExpandProperties(source), service.ExpandProperties(target));
			return true;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in FileSystemFunctions.FileMove.", e);
		}
		return false;
	}

	[KernelFunction(Name = "kernel.fileexists")]
	internal static bool FileExists(string file)
	{
		return File.Exists(ServiceLocator.Instance.GetService<IMacroService>().ExpandProperties(file));
	}

	[KernelFunction(Name = "kernel.directoryexists")]
	internal static bool DirectoryExists(string path)
	{
		return Directory.Exists(ServiceLocator.Instance.GetService<IMacroService>().ExpandProperties(path));
	}

	[KernelFunction(Name = "kernel.directoryremove")]
	internal static bool RemoveDirectory(string path, bool recurse)
	{
		try
		{
			IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
			if (!Directory.Exists(service.ExpandProperties(path)))
			{
				return false;
			}
			Directory.Delete(service.ExpandProperties(path), recurse);
			return true;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in FileSystemFunctions.RemoveDirectory.", e);
		}
		return true;
	}

	[KernelFunction(Name = "kernel.filedelete")]
	internal static bool FileDelete(string file)
	{
		try
		{
			File.Delete(ServiceLocator.Instance.GetService<IMacroService>().ExpandProperties(file));
			return true;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in FileSystemFunctions.FileDelete.", e);
		}
		return false;
	}

	[KernelFunction(Name = "kernel.filewritealltext")]
	internal static bool FileWriteAllText(string file, string text)
	{
		try
		{
			IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
			File.WriteAllText(service.ExpandProperties(file), service.ExpandProperties(text));
			return true;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in FileSystemFunctions.FileWriteAllText.", e);
		}
		return false;
	}

	[KernelFunction(Name = "kernel.fileappendtext")]
	internal static bool FileAppendText(string file, string text)
	{
		try
		{
			IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
			File.AppendAllText(service.ExpandProperties(file), service.ExpandProperties(text));
			return true;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in FileSystemFunctions.FileAppendText.", e);
		}
		return false;
	}

	[KernelFunction(Name = "kernel.filereadalltext")]
	internal static string FileReadAllText(string file)
	{
		try
		{
			return File.ReadAllText(ServiceLocator.Instance.GetService<IMacroService>().ExpandProperties(file));
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in FileSystemFunctions.FileReadAllText.", e);
		}
		return null;
	}

	[KernelFunction(Name = "kernel.filereadalllines")]
	internal static LuaTable FileReadAllLinesText(string file)
	{
		IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
		LuaTable luaTable = new LuaTable(KernelService.Instance.LuaRuntime);
		try
		{
			string[] array = File.ReadAllLines(service.ExpandProperties(file));
			int num = 1;
			string[] array2 = array;
			foreach (string value in array2)
			{
				luaTable[num] = value;
				num++;
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in FileSystemFunctions.FileReadAllLinesText.", e);
		}
		return luaTable;
	}

	[KernelFunction(Name = "kernel.getdirectoryname")]
	internal static string GetDirectoryName(string path)
	{
		return Path.GetDirectoryName(ServiceLocator.Instance.GetService<IMacroService>().ExpandProperties(path));
	}

	[KernelFunction(Name = "kernel.pathcombine")]
	internal static string PathCombine(string path1, string path2)
	{
		IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
		return Path.Combine(service.ExpandProperties(path1), service.ExpandProperties(path2));
	}

	[KernelFunction(Name = "kernel.directorygetfiles")]
	internal static LuaTable GetFiles(string path, string pattern, bool recurse)
	{
		IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
		LuaTable luaTable = new LuaTable(KernelService.Instance.LuaRuntime);
		try
		{
			string[] files = Directory.GetFiles(service.ExpandProperties(path), pattern, recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
			int num = 1;
			string[] array = files;
			foreach (string value in array)
			{
				luaTable[num++] = value;
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in FileSystemFunctions.GetFiles.", e);
		}
		return luaTable;
	}

	[KernelFunction(Name = "kernel.directorygetdirectories")]
	internal static LuaTable GetDirectories(string path, string pattern, bool recurse)
	{
		IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
		LuaTable luaTable = new LuaTable(KernelService.Instance.LuaRuntime);
		try
		{
			string[] directories = Directory.GetDirectories(service.ExpandProperties(path), pattern, recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
			int num = 1;
			string[] array = directories;
			foreach (string value in array)
			{
				luaTable[num++] = value;
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in FileSystemFunctions.GetDirectories.", e);
		}
		return luaTable;
	}

	[KernelFunction(Name = "kernel.editxmlfile")]
	internal static LuaTable EditXmlFile(string path, bool editAllMatches, bool raiseErrors, object xmlNamespaces, object xmlEdits)
	{
		IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
		LuaTable luaTable = new LuaTable(KernelService.Instance.LuaRuntime);
		XmlFileEditor xmlFileEditor = new XmlFileEditor(service.ExpandProperties(path))
		{
			EditAllMatches = editAllMatches,
			RaiseErrors = raiseErrors
		};
		if (xmlNamespaces is LuaTable luaTable2)
		{
			foreach (object key in luaTable2.Keys)
			{
				if (luaTable2[key] is LuaTable luaTable3)
				{
					xmlFileEditor.Namespaces.Add(new XmlNamespace((string)luaTable3["prefix"], (string)luaTable3["uri"]));
				}
			}
		}
		if (xmlEdits is LuaTable luaTable4)
		{
			foreach (object key2 in luaTable4.Keys)
			{
				if (luaTable4[key2] is LuaTable luaTable5)
				{
					XmlEdit xmlEdit = new XmlEdit(service.ExpandProperties((string)luaTable5["xpath"]), service.ExpandProperties((string)luaTable5["value"]));
					LogHelper.Instance.Log("xpath = {0}, value = {1}", xmlEdit.XPath, xmlEdit.Value);
					xmlFileEditor.Edits.Add(xmlEdit);
				}
			}
		}
		ReadOnlyCollection<XmlEditorError> readOnlyCollection = xmlFileEditor.Apply();
		luaTable["success"] = readOnlyCollection.Count == 0;
		LuaTable luaTable6 = new LuaTable(KernelService.Instance.LuaRuntime);
		int num = 1;
		foreach (XmlEditorError item in readOnlyCollection)
		{
			LuaTable luaTable7 = new LuaTable(KernelService.Instance.LuaRuntime);
			luaTable7["code"] = item.Code;
			luaTable7["details"] = item.Details;
			luaTable7["description"] = item.Description;
			luaTable6[num++] = luaTable7;
		}
		luaTable["errors"] = luaTable6;
		return luaTable;
	}

	[KernelFunction(Name = "kernel.getfilehash")]
	internal static string GetSHA1Hash(string fileName)
	{
		string path = ServiceLocator.Instance.GetService<IMacroService>().ExpandProperties(fileName);
		if (!File.Exists(path))
		{
			return "0000000000000000000000000000000000000000";
		}
		using FileStream inputStream = new FileStream(path, FileMode.Open, FileAccess.Read);
		return inputStream.ToASCIISHA1Hash();
	}

	private static void DirectoryCopyInternal(DirectoryInfo source, DirectoryInfo target)
	{
		if (!Directory.Exists(target.FullName))
		{
			Directory.CreateDirectory(target.FullName);
		}
		FileInfo[] files = source.GetFiles();
		foreach (FileInfo fileInfo in files)
		{
			fileInfo.CopyTo(Path.Combine(target.ToString(), fileInfo.Name), overwrite: true);
		}
		DirectoryInfo[] directories = source.GetDirectories();
		foreach (DirectoryInfo directoryInfo in directories)
		{
			DirectoryInfo target2 = target.CreateSubdirectory(directoryInfo.Name);
			DirectoryCopyInternal(directoryInfo, target2);
		}
	}
}

using System.IO;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Kernel;

internal static class PathFunctions
{
	[KernelFunction(Name = "kernel.getdirectoryname")]
	internal static string GetDirectoryName(string path)
	{
		if (string.IsNullOrEmpty(path) || HasInvalidChars(path))
		{
			return null;
		}
		return Path.GetDirectoryName(path);
	}

	[KernelFunction(Name = "kernel.getfullpath")]
	internal static string GetFullPath(string path)
	{
		if (string.IsNullOrEmpty(path) || HasInvalidChars(path))
		{
			return null;
		}
		return Path.GetFullPath(path);
	}

	[KernelFunction(Name = "kernel.combinepaths")]
	internal static string CombinePaths(string first, string second)
	{
		if (string.IsNullOrEmpty(first) || string.IsNullOrEmpty(second) || HasInvalidChars(first) || HasInvalidChars(second))
		{
			return null;
		}
		return Path.Combine(first, second);
	}

	[KernelFunction(Name = "kernel.getpathextension")]
	internal static string GetExtension(string path)
	{
		if (string.IsNullOrEmpty(path) || HasInvalidChars(path))
		{
			return null;
		}
		return Path.GetExtension(path);
	}

	[KernelFunction(Name = "kernel.getfilenamefrompath")]
	internal static string GetFileName(string path)
	{
		if (string.IsNullOrEmpty(path) || HasInvalidChars(path))
		{
			return null;
		}
		return Path.GetFileName(path);
	}

	[KernelFunction(Name = "kernel.doesdirectoryexist")]
	internal static bool DoesDirectoryExist(string path)
	{
		if (string.IsNullOrEmpty(path) || HasInvalidChars(path))
		{
			return false;
		}
		return Directory.Exists(path);
	}

	[KernelFunction(Name = "kernel.doesfileexist")]
	internal static bool DoesFileExist(string path)
	{
		if (string.IsNullOrEmpty(path) || HasInvalidChars(path))
		{
			return false;
		}
		return File.Exists(path);
	}

	private static bool HasInvalidChars(string path)
	{
		char[] invalidPathChars = Path.GetInvalidPathChars();
		foreach (char value in invalidPathChars)
		{
			if (path.IndexOf(value) >= 0)
			{
				return true;
			}
		}
		return false;
	}
}

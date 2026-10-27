using System;
using System.Globalization;
using System.IO;

namespace Redbox.Macros.Functions;

[FunctionSet("directory", "Directory")]
public class DirectoryFunctions : FunctionSetBase
{
	public DirectoryFunctions(PropertyDictionary properties)
		: base(properties)
	{
	}

	[Function("get-creation-time")]
	public DateTime GetCreationTime(string path)
	{
		if (!Directory.Exists(path))
		{
			throw new IOException(string.Format(CultureInfo.InvariantCulture, "Could not find a part of the path \"{0}\".", path));
		}
		return Directory.GetCreationTime(path);
	}

	[Function("get-last-write-time")]
	public DateTime GetLastWriteTime(string path)
	{
		if (!Directory.Exists(path))
		{
			throw new IOException(string.Format(CultureInfo.InvariantCulture, "Could not find a part of the path \"{0}\".", path));
		}
		return Directory.GetLastWriteTime(path);
	}

	[Function("get-last-access-time")]
	public DateTime GetLastAccessTime(string path)
	{
		if (!Directory.Exists(path))
		{
			throw new IOException(string.Format(CultureInfo.InvariantCulture, "Could not find a part of the path \"{0}\".", path));
		}
		return Directory.GetLastAccessTime(path);
	}

	[Function("get-current-directory")]
	public static string GetCurrentDirectory()
	{
		return Directory.GetCurrentDirectory();
	}

	[Function("get-parent-directory")]
	public string GetParentDirectory(string path)
	{
		DirectoryInfo parent = new DirectoryInfo(path).Parent;
		if (parent == null)
		{
			return string.Empty;
		}
		return parent.FullName;
	}

	[Function("get-directory-root")]
	public string GetDirectoryRoot(string path)
	{
		return StringUtils.ConvertNullToEmpty(Directory.GetDirectoryRoot(path));
	}

	[Function("exists")]
	public bool Exists(string path)
	{
		return Directory.Exists(path);
	}
}

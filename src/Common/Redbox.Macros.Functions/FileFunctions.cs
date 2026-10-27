using System;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace Redbox.Macros.Functions;

[FunctionSet("file", "File")]
public class FileFunctions : FunctionSetBase
{
	public FileFunctions(PropertyDictionary properties)
		: base(properties)
	{
	}

	[Function("get-creation-time")]
	public DateTime GetCreationTime(string path)
	{
		if (!File.Exists(path))
		{
			throw new IOException(string.Format(CultureInfo.InvariantCulture, "Could not find a part of the path \"{0}\".", path));
		}
		return File.GetCreationTime(path);
	}

	[Function("get-last-write-time")]
	public DateTime GetLastWriteTime(string path)
	{
		if (!File.Exists(path))
		{
			throw new IOException(string.Format(CultureInfo.InvariantCulture, "Could not find a part of the path \"{0}\".", path));
		}
		return File.GetLastWriteTime(path);
	}

	[Function("get-last-access-time")]
	public DateTime GetLastAccessTime(string path)
	{
		if (!File.Exists(path))
		{
			throw new IOException(string.Format(CultureInfo.InvariantCulture, "Could not find a part of the path \"{0}\".", path));
		}
		return File.GetLastAccessTime(path);
	}

	[Function("exists")]
	public bool Exists(string file)
	{
		return File.Exists(file);
	}

	[Function("get-length")]
	public long GetLength(string file)
	{
		return new FileInfo(file).Length;
	}

	[Function("is-assembly")]
	public bool IsAssembly(string assemblyFile)
	{
		try
		{
			AssemblyName.GetAssemblyName(assemblyFile);
			return true;
		}
		catch (FileLoadException)
		{
			return false;
		}
		catch (BadImageFormatException)
		{
			return false;
		}
	}
}

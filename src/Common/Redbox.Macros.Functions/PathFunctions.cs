using System.IO;

namespace Redbox.Macros.Functions;

[FunctionSet("path", "Path")]
public class PathFunctions : FunctionSetBase
{
	public PathFunctions(PropertyDictionary properties)
		: base(properties)
	{
	}

	[Function("get-full-path")]
	public string GetFullPath(string path)
	{
		return path;
	}

	[Function("combine")]
	public static string Combine(string path1, string path2)
	{
		return Path.Combine(path1, path2);
	}

	[Function("change-extension")]
	public static string ChangeExtension(string path, string extension)
	{
		return Path.ChangeExtension(path, extension);
	}

	[Function("get-directory-name")]
	public static string GetDirectoryName(string path)
	{
		return StringUtils.ConvertNullToEmpty(Path.GetDirectoryName(path));
	}

	[Function("get-extension")]
	public static string GetExtension(string path)
	{
		return Path.GetExtension(path);
	}

	[Function("get-file-name")]
	public static string GetFileName(string path)
	{
		return Path.GetFileName(path);
	}

	[Function("get-file-name-without-extension")]
	public static string GetFileNameWithoutExtension(string path)
	{
		return Path.GetFileNameWithoutExtension(path);
	}

	[Function("get-path-root")]
	public static string GetPathRoot(string path)
	{
		return StringUtils.ConvertNullToEmpty(Path.GetPathRoot(path));
	}

	[Function("get-temp-file-name")]
	public static string GetTempFileName()
	{
		return Path.GetTempFileName();
	}

	[Function("get-temp-path")]
	public static string GetTempPath()
	{
		return Path.GetTempPath();
	}

	[Function("has-extension")]
	public static bool HasExtension(string path)
	{
		return Path.HasExtension(path);
	}

	[Function("is-path-rooted")]
	public static bool IsPathRooted(string path)
	{
		return Path.IsPathRooted(path);
	}
}

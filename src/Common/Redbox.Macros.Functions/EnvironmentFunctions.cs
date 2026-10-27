using System;
using System.Globalization;

namespace Redbox.Macros.Functions;

[FunctionSet("environment", "Environment")]
public class EnvironmentFunctions : FunctionSetBase
{
	public EnvironmentFunctions(PropertyDictionary properties)
		: base(properties)
	{
	}

	[Function("get-folder-path")]
	public static string GetFolderPath(Environment.SpecialFolder folder)
	{
		return Environment.GetFolderPath(folder);
	}

	[Function("get-machine-name")]
	public static string GetMachineName()
	{
		return Environment.MachineName;
	}

	[Function("get-operating-system")]
	public static OperatingSystem GetOperatingSystem()
	{
		return Environment.OSVersion;
	}

	[Function("get-user-name")]
	public static string GetUserName()
	{
		return Environment.UserName;
	}

	[Function("get-variable")]
	public static string GetVariable(string name)
	{
		if (!VariableExists(name))
		{
			throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1095"), name));
		}
		return Environment.GetEnvironmentVariable(name);
	}

	[Function("variable-exists")]
	public static bool VariableExists(string name)
	{
		return Environment.GetEnvironmentVariable(name) != null;
	}

	[Function("get-version")]
	public static Version GetVersion()
	{
		return Environment.Version;
	}

	[Function("newline")]
	public static string NewLine()
	{
		return Environment.NewLine;
	}
}

using System;
using System.IO;

namespace Redbox.HAL.Core;

public class RuntimeService
{
	private string m_assemblyDirectory;

	public static RuntimeService Instance => Singleton<RuntimeService>.Instance;

	public string AssemblyDirectory
	{
		get
		{
			if (string.IsNullOrEmpty(m_assemblyDirectory))
			{
				m_assemblyDirectory = Path.GetDirectoryName(typeof(RuntimeService).Assembly.Location);
			}
			return m_assemblyDirectory;
		}
	}

	private Platform Platform { get; set; }

	public string RuntimePath(string fileName)
	{
		return Path.Combine(AssemblyDirectory, fileName);
	}

	public Platform GetPlatform()
	{
		if (Platform != Platform.None)
		{
			return Platform;
		}
		Platform = Platform.Unknown;
		OperatingSystem oSVersion = Environment.OSVersion;
		Version version = oSVersion.Version;
		if (oSVersion.Platform != PlatformID.Win32NT)
		{
			LogHelper.Instance.Log("Unrecognized platform version {0}", oSVersion.Platform.ToString());
			return Platform;
		}
		switch (version.Major)
		{
		case 5:
			if (version.Minor != 0)
			{
				Platform = Platform.WindowsXP;
			}
			break;
		case 6:
			if (version.Minor != 0)
			{
				Platform = Platform.Windows7;
			}
			break;
		}
		return Platform;
	}

	private RuntimeService()
	{
		Platform = Platform.None;
	}
}

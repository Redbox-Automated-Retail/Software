using System;
using System.Reflection;

namespace Redbox.Core;

public static class BuildConfiguration
{
	private static readonly string[] m_configParts;

	public static bool IsDevelopmentBuild()
	{
		return m_configParts[0].Length == 0 || m_configParts[0] == "Debug";
	}

	public static bool IsIntegrationBuild()
	{
		return m_configParts.Length == 2 && m_configParts[0].IndexOf("Integration", StringComparison.CurrentCultureIgnoreCase) != -1;
	}

	public static string GetBuildConfigurationLabel()
	{
		string result = null;
		if (IsIntegrationBuild())
		{
			result = $"[Integration Build: {m_configParts[1]}] INTERNAL USE ONLY";
		}
		else if (IsDevelopmentBuild())
		{
			string text = m_configParts[0];
			if (text.Length == 0)
			{
				text = "Not Specified";
			}
			result = $"[Development Build: {text}] NOT FOR DEPLOYMENT";
		}
		return result;
	}

	static BuildConfiguration()
	{
		AssemblyConfigurationAttribute assemblyConfigurationAttribute = (AssemblyConfigurationAttribute)Attribute.GetCustomAttribute(Assembly.GetEntryAssembly(), typeof(AssemblyConfigurationAttribute));
		m_configParts = assemblyConfigurationAttribute.Configuration.Split('|');
	}
}

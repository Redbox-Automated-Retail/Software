using System;
using System.Reflection;

namespace Redbox.Core;

internal static class Redbox_002EMacros1309_002EBuildConfiguration
{
	private static readonly string[] m_configParts;

	public static bool IsDevelopmentBuild()
	{
		if (m_configParts[0].Length != 0)
		{
			return m_configParts[0] == "Debug";
		}
		return true;
	}

	public static bool IsIntegrationBuild()
	{
		if (m_configParts.Length == 2)
		{
			return m_configParts[0].IndexOf("Integration", StringComparison.CurrentCultureIgnoreCase) != -1;
		}
		return false;
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

	static Redbox_002EMacros1309_002EBuildConfiguration()
	{
		AssemblyConfigurationAttribute assemblyConfigurationAttribute = (AssemblyConfigurationAttribute)Attribute.GetCustomAttribute(Assembly.GetEntryAssembly(), typeof(AssemblyConfigurationAttribute));
		m_configParts = assemblyConfigurationAttribute.Configuration.Split('|');
	}
}

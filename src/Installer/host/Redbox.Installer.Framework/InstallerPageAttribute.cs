using System;

namespace Redbox.Installer.Framework;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class InstallerPageAttribute : Attribute
{
	private readonly string m_alias;

	public string Alias => m_alias;

	public InstallerPageAttribute(string alias)
	{
		m_alias = alias;
	}
}

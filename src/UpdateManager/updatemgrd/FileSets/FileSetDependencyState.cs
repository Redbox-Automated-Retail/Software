using System;
using Redbox.Core;

namespace Redbox.UpdateManager.FileSets;

internal class FileSetDependencyState
{
	public long FileSetId { get; set; }

	public long ActiveRevisionId { get; set; }

	public string ActiveVersion { get; set; }

	public long InProgressRevisionId { get; set; }

	public string InProgressVersion { get; set; }

	public bool IsInProgressStaged { get; set; }

	public bool NeedsDependency { get; set; }

	public bool IsDependencyMet(DependencyType dependencyType, string minVersion, string maxVersion)
	{
		try
		{
			string sourceVersion;
			if (InProgressRevisionId > 0)
			{
				if (!IsInProgressStaged)
				{
					return false;
				}
				sourceVersion = InProgressVersion;
			}
			else
			{
				if (ActiveRevisionId <= 0)
				{
					return false;
				}
				sourceVersion = ActiveVersion;
			}
			switch (dependencyType)
			{
			case DependencyType.Version:
				return IsVersionDependencyMet(sourceVersion, minVersion, maxVersion);
			case DependencyType.String:
				return IsStringDependencyMet(sourceVersion, minVersion, maxVersion);
			case DependencyType.Long:
				return IsLongDependencyMet(sourceVersion, minVersion, maxVersion);
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("FileSetDependencyState.IsDependencyMet - An unhandled exception occurred", e, LogEntryType.Error);
		}
		return false;
	}

	private bool IsVersionDependencyMet(string sourceVersion, string minVersion, string maxVersion)
	{
		Version version = new Version(sourceVersion);
		Version version2 = new Version(minVersion);
		if (version < version2)
		{
			return false;
		}
		if (string.IsNullOrEmpty(maxVersion))
		{
			return true;
		}
		Version version3 = new Version(maxVersion);
		if (version > version3)
		{
			return false;
		}
		return true;
	}

	private bool IsStringDependencyMet(string sourceVersion, string minVersion, string maxVersion)
	{
		string text = sourceVersion.Trim();
		string strB = minVersion.Trim();
		if (text.CompareTo(strB) < 0)
		{
			return false;
		}
		if (string.IsNullOrEmpty(maxVersion))
		{
			return true;
		}
		string strB2 = maxVersion.Trim();
		if (text.CompareTo(strB2) > 0)
		{
			return false;
		}
		return true;
	}

	private bool IsLongDependencyMet(string sourceVersion, string minVersion, string maxVersion)
	{
		long num = Convert.ToInt64(sourceVersion);
		long num2 = Convert.ToInt64(minVersion);
		if (num < num2)
		{
			return false;
		}
		if (string.IsNullOrEmpty(maxVersion))
		{
			return true;
		}
		long num3 = Convert.ToInt64(maxVersion);
		if (num > num3)
		{
			return false;
		}
		return true;
	}
}

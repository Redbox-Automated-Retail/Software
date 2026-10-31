using System.DirectoryServices;

namespace Redbox.IIS;

public class IIsObject
{
	internal static class Constants
	{
		public const string WebServerSchemaName = "IIsWebServer";

		public const string ApplicationPoolSchemaName = "IIsApplicationPool";

		public const string VirtualDirectorySchemaName = "IIsWebVirtualDir";

		public const string IIsWebSitesPath = "IIS://localhost/w3svc";

		public const string IIsAppPoolsPath = "IIS://localhost/w3svc/AppPools";
	}

	private DirectoryEntry m_entry;

	public string Name
	{
		get
		{
			if (m_entry != null)
			{
				return m_entry.Name;
			}
			return null;
		}
	}

	public string MetabasePath
	{
		get
		{
			if (m_entry != null)
			{
				return m_entry.Path;
			}
			return null;
		}
	}

	protected DirectoryEntry Entry
	{
		get
		{
			return m_entry;
		}
		set
		{
			m_entry = value;
		}
	}

	internal DirectoryEntries Children
	{
		get
		{
			if (Entry == null)
			{
				return null;
			}
			return Entry.Children;
		}
	}

	public void Save()
	{
		if (Entry != null)
		{
			Entry.CommitChanges();
		}
	}

	public override string ToString()
	{
		return Name;
	}

	protected IIsObject()
	{
	}

	protected IIsObject(DirectoryEntry entry)
	{
		Entry = entry;
	}

	internal static DirectoryEntry GetOrMakeNode(string basePath, string relPath, string schemaClassName)
	{
		if (DirectoryEntryExists(basePath + relPath))
		{
			return new DirectoryEntry(basePath + relPath);
		}
		DirectoryEntry directoryEntry = new DirectoryEntry(basePath);
		directoryEntry.RefreshCache();
		DirectoryEntry directoryEntry2 = directoryEntry.Children.Add(relPath.Trim('/'), schemaClassName);
		directoryEntry2.CommitChanges();
		directoryEntry.CommitChanges();
		directoryEntry.Close();
		return directoryEntry2;
	}

	internal static bool DirectoryEntryExists(string path)
	{
		DirectoryEntry directoryEntry = new DirectoryEntry(path);
		try
		{
			_ = directoryEntry.NativeObject;
			return true;
		}
		catch
		{
			return false;
		}
		finally
		{
			directoryEntry.Dispose();
		}
	}
}

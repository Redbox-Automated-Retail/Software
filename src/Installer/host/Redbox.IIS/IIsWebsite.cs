using System.Collections.Generic;
using System.DirectoryServices;

namespace Redbox.IIS;

public class IIsWebsite : IIsObject
{
	private IIsVirtualDirectory m_root;

	private IList<IIsServerBinding> m_serverBindings;

	private IList<IIsServerBinding> m_secureBindings;

	private IList<IIsVirtualDirectory> m_virtualDirectories;

	public string ServerComment
	{
		get
		{
			if (base.Entry.Properties["ServerComment"] != null)
			{
				return base.Entry.Properties["ServerComment"].Value.ToString();
			}
			return null;
		}
	}

	public bool ServerAutoStart
	{
		get
		{
			return (bool)base.Entry.Properties["ServerAutoStart"][0];
		}
		set
		{
			base.Entry.Properties["ServerAutoStart"][0] = value;
		}
	}

	public IIsVirtualDirectory Root
	{
		get
		{
			if (m_root == null)
			{
				try
				{
					m_root = new IIsVirtualDirectory(base.Entry.Children.Find("Root", "IIsWebVirtualDir"));
				}
				catch
				{
				}
			}
			return m_root;
		}
	}

	public IIsServerState ServerState
	{
		get
		{
			return (IIsServerState)base.Entry.Properties["ServerState"][0];
		}
		set
		{
			base.Entry.Properties["ServerState"][0] = value;
		}
	}

	public IList<IIsServerBinding> ServerBindings
	{
		get
		{
			if (m_serverBindings == null)
			{
				m_serverBindings = new List<IIsServerBinding>();
				foreach (object item in base.Entry.Properties["ServerBindings"])
				{
					m_serverBindings.Add(new IIsServerBinding(item.ToString()));
				}
			}
			return m_serverBindings;
		}
		set
		{
			base.Entry.Properties["ServerBindings"].Value = CreateVariantServerBindings(value);
		}
	}

	public IList<IIsServerBinding> SecureBindings
	{
		get
		{
			if (m_secureBindings == null)
			{
				m_secureBindings = new List<IIsServerBinding>();
				foreach (object item in base.Entry.Properties["SecureBindings"])
				{
					m_secureBindings.Add(new IIsServerBinding(item.ToString()));
				}
			}
			return m_secureBindings;
		}
	}

	public IList<IIsVirtualDirectory> VirtualDirectories
	{
		get
		{
			if (m_virtualDirectories == null)
			{
				try
				{
					m_virtualDirectories = new List<IIsVirtualDirectory>();
					foreach (DirectoryEntry child in Root.Children)
					{
						m_virtualDirectories.Add(new IIsVirtualDirectory(child));
					}
				}
				catch
				{
				}
			}
			return m_virtualDirectories;
		}
	}

	public static IIsWebsite CreateNewSite(string name, string path, IList<IIsServerBinding> serverBindings)
	{
		try
		{
			DirectoryEntry directoryEntry = new DirectoryEntry("IIS://localhost/w3svc");
			object[] array = CreateVariantServerBindings(serverBindings);
			object obj = directoryEntry.Invoke("CreateNewSite", name, array, path);
			return GetByID((int)obj);
		}
		catch
		{
			return null;
		}
	}

	public static IIsWebsite GetByName(string name)
	{
		foreach (IIsWebsite item in GetAll())
		{
			if (item.ServerComment == name)
			{
				return item;
			}
		}
		return null;
	}

	public static IIsWebsite GetByID(int siteNumber)
	{
		try
		{
			return new IIsWebsite(new DirectoryEntry("IIS://localhost/w3svc/" + siteNumber));
		}
		catch
		{
			return null;
		}
	}

	public static IList<IIsWebsite> GetAll()
	{
		try
		{
			List<IIsWebsite> list = new List<IIsWebsite>();
			DirectoryEntry directoryEntry = new DirectoryEntry("IIS://localhost/w3svc");
			foreach (DirectoryEntry child in directoryEntry.Children)
			{
				if (child.SchemaClassName == "IIsWebServer")
				{
					list.Add(new IIsWebsite(child));
				}
			}
			return list;
		}
		catch
		{
			return new List<IIsWebsite>();
		}
	}

	public void Start()
	{
		try
		{
			base.Entry.Invoke("Start", null);
		}
		catch
		{
		}
	}

	public IIsVirtualDirectory CreateVirtualDirectory(string name)
	{
		try
		{
			DirectoryEntry orMakeNode = IIsObject.GetOrMakeNode(base.Entry.Path, "/Root/" + name, "IIsWebVirtualDir");
			if (m_virtualDirectories != null)
			{
				m_virtualDirectories.Clear();
				m_virtualDirectories = null;
			}
			return new IIsVirtualDirectory(orMakeNode);
		}
		catch
		{
			return null;
		}
	}

	public void Delete()
	{
		try
		{
			DirectoryEntry directoryEntry = new DirectoryEntry("IIS://localhost/w3svc");
			directoryEntry.Invoke("Delete", "IIsWebServer", base.Name);
		}
		catch
		{
		}
	}

	public IIsVirtualDirectory GetVirtualDirectoryByName(string name)
	{
		foreach (IIsVirtualDirectory virtualDirectory in VirtualDirectories)
		{
			if (virtualDirectory.Name == name)
			{
				return virtualDirectory;
			}
		}
		return null;
	}

	public override string ToString()
	{
		return $"{ServerComment}";
	}

	protected IIsWebsite(DirectoryEntry entry)
		: base(entry)
	{
	}

	private static object[] CreateVariantServerBindings(ICollection<IIsServerBinding> serverBindings)
	{
		int num = 0;
		object[] array = new object[serverBindings.Count];
		foreach (IIsServerBinding serverBinding in serverBindings)
		{
			array[num++] = serverBinding.ToMetabaseString();
		}
		return array;
	}
}

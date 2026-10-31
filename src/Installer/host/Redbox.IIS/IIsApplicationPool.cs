using System.Collections.Generic;
using System.DirectoryServices;

namespace Redbox.IIS;

public class IIsApplicationPool : IIsObject
{
	public PropertyCollection Properties => base.Entry.Properties;

	public string WAMUserName
	{
		get
		{
			return (string)base.Entry.Properties["WAMUserName"][0];
		}
		set
		{
			base.Entry.Properties["WAMUserName"][0] = value;
		}
	}

	public string WAMUserPass
	{
		get
		{
			return (string)base.Entry.Properties["WAMUserPass"][0];
		}
		set
		{
			base.Entry.Properties["WAMUserPass"][0] = value;
		}
	}

	public AppPoolIdentityType IdentityType
	{
		get
		{
			return (AppPoolIdentityType)base.Entry.Properties["AppPoolIdentityType"][0];
		}
		set
		{
			base.Entry.Properties["AppPoolIdentityType"][0] = value;
		}
	}

	public AspNetVersion ManagedRuntimeVersion
	{
		get
		{
			return base.Entry.Properties["ManagedRuntimeVersion"][0].ToString().ToAspNetVersion();
		}
		set
		{
			base.Entry.Properties["ManagedRuntimeVersion"][0] = value.ToAppPoolManagedRuntimeVersion();
		}
	}

	public AppPoolManagedPipelineMode ManagedPipelineMode
	{
		get
		{
			if (base.Entry.Properties["ManagedPipelineMode"] != null)
			{
				return (AppPoolManagedPipelineMode)base.Entry.Properties["ManagedPipelineMode"][0];
			}
			return AppPoolManagedPipelineMode.Classic;
		}
		set
		{
			if (base.Entry.Properties["ManagedPipelineMode"] != null)
			{
				base.Entry.Properties["ManagedPipelineMode"][0] = value;
			}
		}
	}

	public bool AutoStart
	{
		get
		{
			return (bool)base.Entry.Properties["AppPoolAutoStart"][0];
		}
		set
		{
			base.Entry.Properties["AppPoolAutoStart"][0] = value;
		}
	}

	public static IList<IIsApplicationPool> GetAll()
	{
		try
		{
			IList<IIsApplicationPool> list = new List<IIsApplicationPool>();
			DirectoryEntry directoryEntry = new DirectoryEntry("IIS://localhost/w3svc/AppPools");
			foreach (DirectoryEntry child in directoryEntry.Children)
			{
				list.Add(new IIsApplicationPool(child));
			}
			return list;
		}
		catch
		{
			return new List<IIsApplicationPool>();
		}
	}

	public static IIsApplicationPool GetByName(string name)
	{
		try
		{
			DirectoryEntry directoryEntry = new DirectoryEntry(string.Format("{0}/{1}", "IIS://localhost/w3svc/AppPools", name));
			_ = directoryEntry.NativeObject;
			return new IIsApplicationPool(directoryEntry);
		}
		catch
		{
			return null;
		}
	}

	public static IIsApplicationPool CreateAppPool(string name)
	{
		try
		{
			DirectoryEntry directoryEntry = new DirectoryEntry("IIS://localhost/w3svc/AppPools");
			DirectoryEntry directoryEntry2 = directoryEntry.Children.Add(name, "IIsApplicationPool");
			directoryEntry2.CommitChanges();
			return GetByName(name);
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
			base.Entry.Parent.Children.Remove(base.Entry);
		}
		catch
		{
		}
	}

	protected IIsApplicationPool(DirectoryEntry entry)
		: base(entry)
	{
	}
}

using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Core;

internal class GenericDeviceDescriptor : IDeviceDescriptor
{
	private static readonly char[] HwidSeparator = new char[1] { '&' };

	// Vendor and Product are set by Create's object initializer.
	public string Vendor { get; internal set; } = null!;

	public string Product { get; internal set; } = null!;

	public string Friendlyname
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public IDeviceSetupClass? SetupClass { get; internal set; }

	public override string ToString()
	{
		if (SetupClass != null)
		{
			return $"{SetupClass.Class.ToString()}\\{Vendor}&{Product}";
		}
		return Vendor;
	}

	public bool ResetDriver()
	{
		throw new NotImplementedException();
	}

	public bool MatchDriver()
	{
		throw new NotImplementedException();
	}

	public bool Locate()
	{
		throw new NotImplementedException();
	}

	public DeviceStatus GetStatus()
	{
		throw new NotImplementedException();
	}

	internal static GenericDeviceDescriptor? Create(string? hwid, DeviceClass clazz)
	{
		if (string.IsNullOrEmpty(hwid))
		{
			return null;
		}
		hwid = hwid!.ToLower();
		string[] parts = hwid.Split('\\');
		if (parts.Length != 2)
		{
			return null;
		}
		_ = parts[0];
		string deviceId = parts[1];
		int index = deviceId.IndexOf("vid");
		if (-1 == index)
		{
			return Create(string.Empty, deviceId, clazz);
		}
		deviceId = deviceId.Substring(index);
		string[] ids = deviceId.Split(HwidSeparator, StringSplitOptions.RemoveEmptyEntries);
		if (ids.Length < 2)
		{
			return null;
		}
		return Create(ids[0].Substring(4).ToLower(), ids[1].Substring(4).ToLower(), clazz);
	}

	internal static GenericDeviceDescriptor Create(string v, string p, DeviceClass clazz)
	{
		GenericDeviceDescriptor genericDeviceDescriptor = new GenericDeviceDescriptor
		{
			Vendor = v,
			Product = p
		};
		if (clazz != DeviceClass.None)
		{
			IDeviceSetupClassFactory deviceSetupClassFactory = ServiceLocator.Instance.GetService<IDeviceSetupClassFactory>();
			genericDeviceDescriptor.SetupClass = deviceSetupClassFactory.Get(clazz);
		}
		return genericDeviceDescriptor;
	}
}

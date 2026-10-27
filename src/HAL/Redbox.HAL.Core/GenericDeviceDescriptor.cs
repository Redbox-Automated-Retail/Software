using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Core;

internal class GenericDeviceDescriptor : IDeviceDescriptor
{
	private static readonly char[] HwidSeparator = new char[1] { '&' };

	public string Vendor { get; internal set; }

	public string Product { get; internal set; }

	public string Friendlyname
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public IDeviceSetupClass SetupClass { get; internal set; }

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

	internal static GenericDeviceDescriptor Create(string hwid, DeviceClass clazz)
	{
		if (string.IsNullOrEmpty(hwid))
		{
			return null;
		}
		hwid = hwid.ToLower();
		string[] array = hwid.Split('\\');
		if (array.Length != 2)
		{
			return null;
		}
		_ = array[0];
		string text = array[1];
		int num = text.IndexOf("vid");
		if (-1 == num)
		{
			return Create(string.Empty, text, clazz);
		}
		text = text.Substring(num);
		string[] array2 = text.Split(HwidSeparator, StringSplitOptions.RemoveEmptyEntries);
		if (array2.Length < 2)
		{
			return null;
		}
		return Create(array2[0].Substring(4).ToLower(), array2[1].Substring(4).ToLower(), clazz);
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
			IDeviceSetupClassFactory service = ServiceLocator.Instance.GetService<IDeviceSetupClassFactory>();
			genericDeviceDescriptor.SetupClass = service.Get(clazz);
		}
		return genericDeviceDescriptor;
	}
}

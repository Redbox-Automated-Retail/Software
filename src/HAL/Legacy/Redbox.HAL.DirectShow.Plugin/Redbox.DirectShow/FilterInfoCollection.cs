using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using Redbox.DirectShow.Interop;
using Redbox.HAL.Core;

namespace Redbox.DirectShow;

public class FilterInfoCollection : CollectionBase
{
	private static bool? m_useOldOption = null;

	public FilterInfo this[int index] => (FilterInfo)base.InnerList[index];

	private static bool UseOld
	{
		get
		{
			if (m_useOldOption.HasValue)
			{
				return m_useOldOption.Value;
			}
			m_useOldOption = RegistryOptions.Instance.GetSecretValue<bool>("SOFTWARE\\Redbox\\HAL", "UseOldEnumerator", false);
			return m_useOldOption.Value;
		}
	}

	public FilterInfoCollection(Guid category)
	{
		CollectFilters(category);
	}

	private void CollectFilters(Guid category)
	{
		object comObj = null;
		ICreateDevEnum createDevEnum = null;
		IEnumMoniker enumMoniker = null;
		IMoniker[] devMon = new IMoniker[1];
		try
		{
			Type typeFromCLSID = Type.GetTypeFromCLSID(Clsid.SystemDeviceEnum);
			if (typeFromCLSID == null)
			{
				throw new ApplicationException("Failed creating device enumerator");
			}
			comObj = Activator.CreateInstance(typeFromCLSID);
			createDevEnum = (ICreateDevEnum)comObj;
			if (createDevEnum.CreateClassEnumerator(ref category, out enumMoniker, 0) != 0)
			{
				throw new ApplicationException("No devices of the category");
			}
			IntPtr fetched = IntPtr.Zero;
			while (enumMoniker.Next(1, devMon, fetched) == 0)
			{
				if (devMon[0] == null)
				{
					if (UseOld)
					{
						break;
					}
				}
				else
				{
					FilterInfo filter = new FilterInfo(devMon[0]);
					base.InnerList.Add(filter);
					Marshal.ReleaseComObject(devMon[0]);
					devMon[0] = null;
				}
			}
			base.InnerList.Sort();
		}
		catch
		{
		}
		finally
		{
			createDevEnum = null;
			if (comObj != null)
			{
				Marshal.ReleaseComObject(comObj);
				comObj = null;
			}
			if (enumMoniker != null)
			{
				Marshal.ReleaseComObject(enumMoniker);
				enumMoniker = null;
			}
			if (devMon[0] != null)
			{
				Marshal.ReleaseComObject(devMon[0]);
				devMon[0] = null;
			}
		}
	}
}

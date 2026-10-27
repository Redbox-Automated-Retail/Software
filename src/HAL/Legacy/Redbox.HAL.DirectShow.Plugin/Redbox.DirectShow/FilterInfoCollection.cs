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
		object obj = null;
		ICreateDevEnum createDevEnum = null;
		IEnumMoniker enumMoniker = null;
		IMoniker[] array = new IMoniker[1];
		try
		{
			Type typeFromCLSID = Type.GetTypeFromCLSID(Clsid.SystemDeviceEnum);
			if (typeFromCLSID == null)
			{
				throw new ApplicationException("Failed creating device enumerator");
			}
			obj = Activator.CreateInstance(typeFromCLSID);
			createDevEnum = (ICreateDevEnum)obj;
			if (createDevEnum.CreateClassEnumerator(ref category, out enumMoniker, 0) != 0)
			{
				throw new ApplicationException("No devices of the category");
			}
			IntPtr zero = IntPtr.Zero;
			while (enumMoniker.Next(1, array, zero) == 0)
			{
				if (array[0] == null)
				{
					if (UseOld)
					{
						break;
					}
				}
				else
				{
					FilterInfo value = new FilterInfo(array[0]);
					base.InnerList.Add(value);
					Marshal.ReleaseComObject(array[0]);
					array[0] = null;
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
			if (obj != null)
			{
				Marshal.ReleaseComObject(obj);
				obj = null;
			}
			if (enumMoniker != null)
			{
				Marshal.ReleaseComObject(enumMoniker);
				enumMoniker = null;
			}
			if (array[0] != null)
			{
				Marshal.ReleaseComObject(array[0]);
				array[0] = null;
			}
		}
	}
}

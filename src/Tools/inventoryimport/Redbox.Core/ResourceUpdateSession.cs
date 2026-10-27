using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Redbox.Core;

public class ResourceUpdateSession
{
	internal class ResourceKey
	{
		public string Name;

		public string Group;

		public ResourceKey(string group, string name)
		{
			Group = group.ToUpper();
			Name = name.ToUpper();
		}

		public override bool Equals(object obj)
		{
			if (object.ReferenceEquals(obj, this))
			{
				return true;
			}
			if (!(obj is ResourceKey resourceKey))
			{
				return false;
			}
			return resourceKey.Group == Group && resourceKey.Name == Name;
		}

		public override int GetHashCode()
		{
			return (Group + Name).GetHashCode();
		}
	}

	internal const int NumberOfUpdatesPerSession = 8;

	private readonly string m_fileName;

	private readonly bool m_deleteExisting;

	private readonly Dictionary<ResourceKey, byte[]> m_resources = new Dictionary<ResourceKey, byte[]>();

	public ResourceUpdateSession(string fileName)
		: this(fileName, deleteExisting: false)
	{
	}

	public ResourceUpdateSession(string fileName, bool deleteExisting)
	{
		m_fileName = fileName;
		m_deleteExisting = deleteExisting;
	}

	public void Save()
	{
		if (m_resources.Count == 0)
		{
			return;
		}
		IntPtr intPtr = BeginUpdateResource(m_fileName, m_deleteExisting);
		if (intPtr == IntPtr.Zero)
		{
			throw new Win32Exception(Marshal.GetLastWin32Error());
		}
		List<ResourceKey> list = new List<ResourceKey>(m_resources.Keys);
		for (int i = 0; i < m_resources.Keys.Count; i++)
		{
			ResourceKey resourceKey = list[i];
			UpdateResource(intPtr, resourceKey.Group, resourceKey.Name, (ushort)MAKELANGID(9, 1), m_resources[resourceKey], m_resources[resourceKey].Length);
			int lastWin32Error = Marshal.GetLastWin32Error();
			if (lastWin32Error != 0)
			{
				throw new Win32Exception(lastWin32Error);
			}
			if ((i + 1) % 8 != 0)
			{
				continue;
			}
			EndUpdateResource(intPtr, fDiscard: false);
			lastWin32Error = Marshal.GetLastWin32Error();
			if (lastWin32Error != 0)
			{
				throw new Win32Exception(lastWin32Error);
			}
			if (i + 1 < m_resources.Keys.Count)
			{
				intPtr = BeginUpdateResource(m_fileName, bDeleteExistingResources: false);
				if (intPtr == IntPtr.Zero)
				{
					throw new Win32Exception(Marshal.GetLastWin32Error());
				}
			}
		}
		if (list.Count % 8 > 0)
		{
			EndUpdateResource(intPtr, fDiscard: false);
			int lastWin32Error = Marshal.GetLastWin32Error();
			if (lastWin32Error != 0)
			{
				throw new Win32Exception(lastWin32Error);
			}
		}
	}

	public void AddResource(string group, string name, byte[] buffer)
	{
		m_resources[new ResourceKey(group, name)] = buffer;
	}

	[DllImport("kernel32")]
	private static extern IntPtr BeginUpdateResource(string pFileName, bool bDeleteExistingResources);

	[DllImport("kernel32", SetLastError = true)]
	private static extern int UpdateResource(IntPtr hUpdate, string lpType, string lpName, ushort wLanguage, byte[] lpData, int cbData);

	[DllImport("kernel32", SetLastError = true)]
	private static extern int EndUpdateResource(IntPtr hUpdate, bool fDiscard);

	public static int MAKELANGID(int primary, int sub)
	{
		return ((ushort)sub << 10) | (ushort)primary;
	}

	public static int PRIMARYLANGID(int lcid)
	{
		return (ushort)lcid & 0x3FF;
	}

	public static int SUBLANGID(int lcid)
	{
		return (ushort)lcid >> 10;
	}
}

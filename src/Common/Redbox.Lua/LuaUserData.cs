using System;
using System.Runtime.InteropServices;

namespace Redbox.Lua;

public class LuaUserData : LuaObject
{
	private readonly byte[] m_buffer;

	public byte this[int offset]
	{
		get
		{
			if (base.Reference.HasValue)
			{
				IntPtr userDataPointer = GetUserDataPointer();
				if (userDataPointer == IntPtr.Zero)
				{
					return 0;
				}
				return Marshal.ReadByte(userDataPointer, offset);
			}
			return m_buffer[offset];
		}
		set
		{
			if (base.Reference.HasValue)
			{
				IntPtr userDataPointer = GetUserDataPointer();
				if (!(userDataPointer == IntPtr.Zero))
				{
					Marshal.WriteByte(userDataPointer, offset, value);
				}
			}
			else
			{
				m_buffer[offset] = value;
			}
		}
	}

	public LuaUserData(LuaVirtualMachine virtualMachine, uint size)
		: base(virtualMachine, null)
	{
		m_buffer = new byte[size];
	}

	public LuaUserData(LuaVirtualMachine virtualMachine, int reference)
		: base(virtualMachine, reference)
	{
	}

	public override string ToString()
	{
		return "(userdata)";
	}

	public override void Push()
	{
		if (base.Reference.HasValue)
		{
			base.Push();
			return;
		}
		IntPtr destination = base.VirtualMachineInstance.Stack.NewUserData(m_buffer.Length);
		Marshal.Copy(m_buffer, 0, destination, m_buffer.Length);
	}

	public byte[] Read(int offset, int count)
	{
		byte[] array = new byte[count];
		if (base.Reference.HasValue)
		{
			IntPtr userDataPointer = GetUserDataPointer();
			if (userDataPointer != IntPtr.Zero)
			{
				Marshal.Copy(new IntPtr(userDataPointer.ToInt32() + offset), array, 0, count);
			}
		}
		else
		{
			Array.Copy(m_buffer, offset, array, 0, count);
		}
		return array;
	}

	public short ReadInt16(int offset)
	{
		if (base.Reference.HasValue)
		{
			IntPtr userDataPointer = GetUserDataPointer();
			if (!(userDataPointer == IntPtr.Zero))
			{
				return Marshal.ReadInt16(new IntPtr(userDataPointer.ToInt32() + offset));
			}
			return 0;
		}
		return BitConverter.ToInt16(m_buffer, offset);
	}

	public int ReadInt32(int offset)
	{
		if (base.Reference.HasValue)
		{
			IntPtr userDataPointer = GetUserDataPointer();
			if (!(userDataPointer == IntPtr.Zero))
			{
				return Marshal.ReadInt32(new IntPtr(userDataPointer.ToInt32() + offset));
			}
			return 0;
		}
		return BitConverter.ToInt32(m_buffer, offset);
	}

	public void Write(byte[] bytes, int offset, int count)
	{
		if (base.Reference.HasValue)
		{
			IntPtr userDataPointer = GetUserDataPointer();
			if (!(userDataPointer == IntPtr.Zero))
			{
				Marshal.Copy(bytes, 0, new IntPtr(userDataPointer.ToInt32() + offset), count);
			}
		}
		else
		{
			Array.Copy(bytes, 0, m_buffer, offset, count);
		}
	}

	public void WriteInt16(int offset, short value)
	{
		if (base.Reference.HasValue)
		{
			IntPtr userDataPointer = GetUserDataPointer();
			if (!(userDataPointer == IntPtr.Zero))
			{
				Marshal.WriteInt16(userDataPointer, offset, value);
			}
		}
		else
		{
			Array.Copy(BitConverter.GetBytes(value), 0, m_buffer, offset, 2);
		}
	}

	public void WriteInt32(int offset, int value)
	{
		if (base.Reference.HasValue)
		{
			IntPtr userDataPointer = GetUserDataPointer();
			if (!(userDataPointer == IntPtr.Zero))
			{
				Marshal.WriteInt32(userDataPointer, offset, value);
			}
		}
		else
		{
			Array.Copy(BitConverter.GetBytes(value), 0, m_buffer, offset, 4);
		}
	}

	private IntPtr GetUserDataPointer()
	{
		if (!base.Reference.HasValue)
		{
			return IntPtr.Zero;
		}
		base.VirtualMachineInstance.Stack.GetReference(base.Reference.Value);
		IntPtr result = base.VirtualMachineInstance.Stack.ToUserData(1);
		base.VirtualMachineInstance.Stack.Pop();
		return result;
	}
}

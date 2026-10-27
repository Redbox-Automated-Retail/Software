using System;

namespace Redbox.Lua;

public class LuaObject : IDisposable
{
	private bool m_disposed;

	protected static object _lock = new object();

	protected int? Reference { get; private set; }

	protected LuaVirtualMachine VirtualMachineInstance { get; private set; }

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	~LuaObject()
	{
		Dispose(disposing: false);
	}

	public virtual void Push()
	{
		if (VirtualMachineInstance != null && Reference.HasValue)
		{
			VirtualMachineInstance.Stack.GetReference(Reference.Value);
		}
	}

	public override bool Equals(object obj)
	{
		if (this == obj)
		{
			return true;
		}
		if (!(obj is LuaObject { Reference: var reference } luaObject))
		{
			return false;
		}
		if (!reference.HasValue || !Reference.HasValue)
		{
			return false;
		}
		if (!VirtualMachineInstance.CompareReference(luaObject.Reference.Value, Reference.Value))
		{
			return base.Equals(obj);
		}
		return true;
	}

	protected LuaObject(LuaVirtualMachine virtualMachine, int? reference)
	{
		VirtualMachineInstance = virtualMachine;
		Reference = reference;
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!m_disposed)
		{
			if (Reference.HasValue && VirtualMachineInstance != null)
			{
				VirtualMachineInstance.RemoveReference(Reference.Value);
			}
			m_disposed = true;
		}
	}
}

using System.Collections.ObjectModel;
using System.Threading;

namespace Redbox.Lua;

public class LuaFunction : LuaObject
{
	public LuaFunction(LuaVirtualMachine virtualMachine, int reference)
		: base(virtualMachine, reference)
	{
	}

	public override string ToString()
	{
		return "(function)";
	}

	public ReadOnlyCollection<object> Call(params object[] args)
	{
		try
		{
			lock (LuaObject._lock)
			{
				int count = base.VirtualMachineInstance.Stack.Count;
				if (!base.VirtualMachineInstance.Stack.Check(args.Length + 6))
				{
					throw new LuaException("Lua stack overflow");
				}
				Push();
				foreach (object value in args)
				{
					base.VirtualMachineInstance.Stack.Push(value);
				}
				base.VirtualMachineInstance.IsExecuting = true;
				if (base.VirtualMachineInstance.Stack.ProtectedCall(args.Length, -1, 0) == 0)
				{
					return base.VirtualMachineInstance.Stack.PopTo(count);
				}
				base.VirtualMachineInstance.ThrowExceptionFromError(count);
				Monitor.Pulse(this);
			}
		}
		finally
		{
			base.VirtualMachineInstance.IsExecuting = false;
		}
		return null;
	}
}

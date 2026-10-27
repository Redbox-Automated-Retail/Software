using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Threading;
using Redbox.Core;

namespace Redbox.Lua;

public class LuaFunction : LuaObject
{
	public class TrackLuaCall
	{
		public DateTime EnteredCall { get; set; }

		public long Elapsed { get; set; }

		public string Stack { get; set; }
	}

	private static ConcurrentDictionary<Guid, TrackLuaCall> _trackLuaCall = new ConcurrentDictionary<Guid, TrackLuaCall>();

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
		TrackLuaCall trackLuaCall = null;
		Guid key = Guid.NewGuid();
		try
		{
			trackLuaCall = new TrackLuaCall
			{
				EnteredCall = DateTime.Now,
				Stack = Environment.StackTrace
			};
			_trackLuaCall[key] = trackLuaCall;
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
			if (trackLuaCall != null)
			{
				_trackLuaCall.TryRemove(key, out var _);
			}
		}
		return null;
	}

	public static void LogLuaFunctions()
	{
		LogHelper.Instance.Log($"Logging the stack for {_trackLuaCall.Count} LuaFunction objects");
		_trackLuaCall.Values.ForEach(delegate(TrackLuaCall x)
		{
			x.Elapsed = (long)(DateTime.Now - x.EnteredCall).TotalMilliseconds;
			LogHelper.Instance.Log(x.ToJson());
		});
	}
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace Redbox.Lua;

public class LuaDebugger : ILuaDebugFileFactory, ILuaDebugBreakpointFactory
{
	private struct NativeMessage
	{
		public IntPtr hWnd;

		public uint msg;

		public IntPtr wParam;

		public IntPtr lParam;

		public uint time;

		public Point p;
	}

	private int m_stackLevel;

	private bool m_inDebugHook;

	private int m_stepStackLevel;

	private DebuggerActions m_action;

	private LuaDebuggerState m_state;

	private readonly LuaDebugFileContainer m_files = new LuaDebugFileContainer();

	public bool Enabled
	{
		get
		{
			return m_state != LuaDebuggerState.Disabled;
		}
		set
		{
			State = (value ? LuaDebuggerState.Running : LuaDebuggerState.Disabled);
		}
	}

	public bool FullTrace { get; set; }

	public LuaDebugFileContainer Files => m_files;

	public LuaVirtualMachine Lua { get; private set; }

	public ILuaDebugFileFactory FileFactory { get; set; }

	public ILuaDebugBreakpointFactory BreakpointFactory { get; set; }

	public LuaDebuggerState State
	{
		get
		{
			return m_state;
		}
		set
		{
			switch (value)
			{
			case LuaDebuggerState.Disabled:
				m_stackLevel = 0;
				if (m_state == LuaDebuggerState.Stopped)
				{
					Run();
				}
				if (m_state != LuaDebuggerState.Disabled)
				{
					Lua.RemoveDebugHook();
					Lua.DebugHook -= OnDebugHook;
				}
				m_state = LuaDebuggerState.Disabled;
				break;
			case LuaDebuggerState.Running:
				switch (m_state)
				{
				case LuaDebuggerState.Disabled:
					Lua.DebugHook += OnDebugHook;
					Lua.SetDebugHook(LuaHookEventMasks.LuaMaskAll, 0);
					m_state = LuaDebuggerState.Running;
					break;
				case LuaDebuggerState.Stopped:
					Run();
					break;
				case LuaDebuggerState.Running:
					break;
				}
				break;
			case LuaDebuggerState.Stopped:
				throw new InvalidOperationException("Cant set state to Stoped");
			default:
				throw new ArgumentException("Unknown value for state");
			}
		}
	}

	public event EventHandler<StoppingEventArgs> Stopping;

	public event EventHandler<EventArgs> WaitingForAction;

	public event EventHandler<DebugHookEventArgs> FullTraceData;

	public LuaDebugger(LuaVirtualMachine lua)
	{
		Lua = lua;
		FullTrace = false;
		FileFactory = this;
		BreakpointFactory = this;
	}

	public void Run()
	{
		if (m_state == LuaDebuggerState.Stopped)
		{
			m_action = DebuggerActions.Run;
			m_state = LuaDebuggerState.Running;
		}
	}

	public void Stop()
	{
		m_action = DebuggerActions.Stop;
	}

	public void StepInto()
	{
		if (m_state == LuaDebuggerState.Stopped)
		{
			m_action = DebuggerActions.StepInto;
			m_state = LuaDebuggerState.Running;
		}
	}

	public void StepOver()
	{
		if (m_state == LuaDebuggerState.Stopped)
		{
			m_action = DebuggerActions.StepOver;
			m_stepStackLevel = m_stackLevel;
			m_state = LuaDebuggerState.Running;
		}
	}

	public void StepOut()
	{
		if (m_state == LuaDebuggerState.Stopped)
		{
			m_action = DebuggerActions.StepOut;
			m_stepStackLevel = m_stackLevel - 1;
			m_state = LuaDebuggerState.Running;
		}
	}

	public LuaVar[] GetGlobalVars()
	{
		List<LuaVar> list = new List<LuaVar>();
		LuaTable luaTable = (LuaTable)Lua["_G"];
		int num = 1;
		foreach (object key in luaTable.Keys)
		{
			if (!(key.ToString() == "_G"))
			{
				list.Add(new LuaVar(num++, key.ToString(), luaTable[key]));
			}
		}
		return list.ToArray();
	}

	public CallStackEntry[] GetCallStack()
	{
		List<CallStackEntry> list = new List<CallStackEntry>();
		LuaDebug luaDebug;
		for (int i = 0; Lua.GetStack(i, out luaDebug); i++)
		{
			Lua.GetInfo("nSl", ref luaDebug);
			list.Add(new CallStackEntry(new LuaDebugInfo(luaDebug)));
		}
		return list.ToArray();
	}

	public LuaVar[] GetLocalVars()
	{
		if (Lua.GetStack(0, out var luaDebug))
		{
			Lua.GetInfo("nSl", ref luaDebug);
			return GetLocalVars(luaDebug);
		}
		return new List<LuaVar>().ToArray();
	}

	public LuaVar[] GetLocalVars(LuaDebug luaDebugInfo)
	{
		List<LuaVar> list = new List<LuaVar>();
		int n = 1;
		for (string local = Lua.GetLocal(luaDebugInfo, n); local != null; local = Lua.GetLocal(luaDebugInfo, n))
		{
			object value = Lua.Stack.Pop();
			list.Add(new LuaVar(n++, local, value));
		}
		return list.ToArray();
	}

	public void SetLocalVar(LuaDebug luaDebugInfo, ref LuaVar var, object newValue)
	{
		if (m_state == LuaDebuggerState.Stopped)
		{
			var.Value = newValue;
			Lua.Stack.Push(newValue);
			Lua.SetLocal(luaDebugInfo, var.Index);
		}
	}

	public bool SetLocalVar(LuaDebug luaDebugInfo, string varName, object newValue)
	{
		if (m_state != LuaDebuggerState.Stopped)
		{
			return false;
		}
		LuaVar[] localVars = GetLocalVars(luaDebugInfo);
		for (int i = 0; i < localVars.Length; i++)
		{
			LuaVar var = localVars[i];
			if (string.Compare(varName, var.Name, StringComparison.OrdinalIgnoreCase) == 0)
			{
				SetLocalVar(luaDebugInfo, ref var, newValue);
				return true;
			}
		}
		return false;
	}

	public LuaVar[] GetUpValues(int functionIndex)
	{
		List<LuaVar> list = new List<LuaVar>();
		if (m_state != LuaDebuggerState.Stopped)
		{
			return list.ToArray();
		}
		int num = 1;
		for (string upValue = Lua.GetUpValue(functionIndex, num); upValue != null; upValue = Lua.GetUpValue(functionIndex, num))
		{
			object value = Lua.Stack.Pop();
			list.Add(new LuaVar(num, upValue, value));
			num++;
		}
		return list.ToArray();
	}

	public void SetUpValue(int functionIndex, ref LuaVar var, object newValue)
	{
		if (m_state == LuaDebuggerState.Stopped)
		{
			var.Value = newValue;
			Lua.Stack.Push(newValue);
			Lua.SetUpValue(functionIndex, var.Index);
		}
	}

	public bool SetUpValue(int functionIndex, string varName, object newValue)
	{
		if (m_state != LuaDebuggerState.Stopped)
		{
			return false;
		}
		LuaVar[] upValues = GetUpValues(functionIndex);
		for (int i = 0; i < upValues.Length; i++)
		{
			LuaVar var = upValues[i];
			if (string.Compare(varName, var.Name, StringComparison.OrdinalIgnoreCase) == 0)
			{
				SetUpValue(functionIndex, ref var, newValue);
				return true;
			}
		}
		return false;
	}

	public LuaDebugFile AddFile(string fileName)
	{
		LuaDebugFile luaDebugFile = GetFile(fileName);
		if (luaDebugFile == null)
		{
			luaDebugFile = FileFactory.CreateFile(this, fileName);
			m_files.Add(luaDebugFile);
		}
		return luaDebugFile;
	}

	public LuaDebugFile GetFile(string fileName)
	{
		foreach (LuaDebugFile file in m_files)
		{
			if (string.Compare(fileName, file.FileName, StringComparison.OrdinalIgnoreCase) == 0)
			{
				return file;
			}
		}
		return null;
	}

	public void RemoveBreakpoint(string fileName, int line)
	{
		GetFile(fileName)?.RemoveBreakpoint(line);
	}

	public void ClearAllBreakpoints()
	{
		m_files.Clear();
	}

	public LuaDebugFileContainer GetFiles()
	{
		return m_files;
	}

	public LuaDebugBreakpoint GetBreakpoint(string fileName, int line)
	{
		return GetFile(fileName)?.GetBreakpoint(line);
	}

	public LuaDebugBreakpoint AddBreakpoint(string fileName, int line)
	{
		LuaDebugFile luaDebugFile = GetFile(fileName) ?? AddFile(fileName);
		return luaDebugFile.GetBreakpoint(line) ?? luaDebugFile.AddBreakpoint(line);
	}

	public LuaDebugBreakpointContainer GetBreakpoints(string fileName)
	{
		LuaDebugFile file = GetFile(fileName);
		if (file == null)
		{
			return new LuaDebugBreakpointContainer();
		}
		return file.Breakpoints;
	}

	LuaDebugFile ILuaDebugFileFactory.CreateFile(LuaDebugger debugger, string fileName)
	{
		return new LuaDebugFile(debugger, fileName);
	}

	LuaDebugBreakpoint ILuaDebugBreakpointFactory.CreateBreakpoint(LuaDebugFile file, int line)
	{
		return new LuaDebugBreakpoint(file, line);
	}

	[DllImport("user32.dll")]
	private static extern bool PeekMessage(out NativeMessage msg, IntPtr hWnd, uint messageFilterMin, uint messageFilterMax, uint flags);

	private void OnDebugHook(object sender, DebugHookEventArgs e)
	{
		if (m_inDebugHook)
		{
			return;
		}
		m_inDebugHook = true;
		try
		{
			if (FullTrace)
			{
				OnFullTraceData(e);
			}
			if (m_state == LuaDebuggerState.Disabled)
			{
				return;
			}
			if (e.DebugInfo.EventCode == LuaHookEventCodes.LuaHookCall)
			{
				m_stackLevel++;
			}
			else if (e.DebugInfo.EventCode == LuaHookEventCodes.LuaHookRet || e.DebugInfo.EventCode == LuaHookEventCodes.LuaHookTailRet)
			{
				m_stackLevel--;
			}
			if (e.DebugInfo.EventCode != LuaHookEventCodes.LuaHookCall && e.DebugInfo.EventCode != LuaHookEventCodes.LuaHookLine)
			{
				return;
			}
			LuaDebug luaDebug = e.DebugInfo.LuaDebug;
			Lua.GetInfo("nS", ref luaDebug);
			LuaDebugInfo luaDebugInfo = new LuaDebugInfo(luaDebug);
			int line = ((luaDebugInfo.EventCode == LuaHookEventCodes.LuaHookCall) ? luaDebugInfo.LineDefined : luaDebugInfo.CurrentLine);
			if (luaDebugInfo.Source.Length <= 0)
			{
				return;
			}
			switch (m_action)
			{
			case DebuggerActions.Run:
			{
				LuaDebugBreakpoint breakpoint = GetBreakpoint(luaDebugInfo.Source, line);
				if (breakpoint != null && breakpoint.Enabled)
				{
					StopExecution(luaDebugInfo, m_action, breakpoint);
				}
				break;
			}
			case DebuggerActions.Stop:
			case DebuggerActions.StepInto:
				StopExecution(luaDebugInfo, m_action, null);
				break;
			case DebuggerActions.StepOver:
			case DebuggerActions.StepOut:
			{
				if (m_stackLevel <= m_stepStackLevel)
				{
					StopExecution(luaDebugInfo, m_action, null);
					break;
				}
				LuaDebugBreakpoint breakpoint = GetBreakpoint(luaDebugInfo.Source, line);
				if (breakpoint != null && breakpoint.Enabled)
				{
					StopExecution(luaDebugInfo, m_action, breakpoint);
				}
				break;
			}
			}
		}
		finally
		{
			m_inDebugHook = false;
		}
	}

	private void StopExecution(LuaDebugInfo luaDebugInfo, DebuggerActions action, LuaDebugBreakpoint breakpoint)
	{
		m_state = LuaDebuggerState.Stopped;
		try
		{
			OnStopping(new StoppingEventArgs(luaDebugInfo.Source, luaDebugInfo.CurrentLine, action, breakpoint));
			OnWaitingForAction(new EventArgs());
			do
			{
				if (!PeekMessage(out var _, IntPtr.Zero, 0u, 0u, 0u))
				{
					Thread.Sleep(1);
				}
				Application.DoEvents();
			}
			while (m_state == LuaDebuggerState.Stopped);
		}
		finally
		{
			m_state = LuaDebuggerState.Running;
		}
	}

	private void OnWaitingForAction(EventArgs e)
	{
		WaitingForAction?.Invoke(this, e);
	}

	private void OnStopping(StoppingEventArgs e)
	{
		Stopping?.Invoke(this, e);
	}

	private void OnFullTraceData(DebugHookEventArgs e)
	{
		FullTraceData?.Invoke(this, e);
	}
}

using System;
using System.Text;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal sealed class CommandTrace : IDisposable
{
	private int spaceCount;

	private readonly bool TraceEnabled;

	private readonly StringBuilder TraceBuffer;

	public void Dispose()
	{
		if (TraceEnabled)
		{
			LogHelper.Instance.Log(TraceBuffer.ToString());
		}
	}

	internal void Enter()
	{
		spaceCount++;
	}

	internal void Exit()
	{
		spaceCount--;
	}

	internal void Trace(string fmt, params object[] p)
	{
		if (TraceEnabled)
		{
			Trace(string.Format(fmt, p));
		}
	}

	internal void Trace(string msg)
	{
		if (TraceEnabled)
		{
			for (int i = 0; i < spaceCount; i++)
			{
				TraceBuffer.Append(" ");
			}
			TraceBuffer.AppendLine(msg);
		}
	}

	internal CommandTrace(bool enabled)
	{
		TraceEnabled = enabled;
		if (TraceEnabled)
		{
			TraceBuffer = new StringBuilder();
		}
	}
}

using System;
using System.Collections.Generic;

namespace Redbox.HAL.Core;

public sealed class ClearImageScanResult : IScanResult, IDisposable
{
	private readonly List<string> m_rawCodes = new List<string>();

	public TimeSpan ExecutionTime { get; set; }

	public string[] ReadCodes => m_rawCodes.ToArray();

	private bool Disposed { get; set; }

	public void Dispose()
	{
		if (!Disposed)
		{
			Disposed = true;
			m_rawCodes.Clear();
		}
	}

	public ClearImageScanResult()
	{
		ExecutionTime = default(TimeSpan);
	}

	public void ResetOnException()
	{
		m_rawCodes.Clear();
		ExecutionTime = default(TimeSpan);
	}

	public void AddCode(string code)
	{
		m_rawCodes.Add(code);
	}
}

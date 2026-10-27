using System;
using System.IO;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.DataMatrix.Framework;

internal abstract class ScannerAdapter : IDisposable
{
	protected readonly IRuntimeService RuntimeService;

	private bool m_disposed;

	public void Dispose()
	{
		if (!m_disposed)
		{
			m_disposed = true;
			DisposeInner();
		}
	}

	protected string GenerateSnapFileName()
	{
		string path = RuntimeService.GenerateUniqueFile("jpg");
		if (BarcodeConfiguration.Instance.UseRuntimePath)
		{
			return Path.Combine(RuntimeService.RuntimePath("Video"), path);
		}
		return Path.Combine(BarcodeConfiguration.Instance.WorkingPath, path);
	}

	protected ScannerAdapter()
	{
		RuntimeService = ServiceLocator.Instance.GetService<IRuntimeService>();
	}

	protected abstract void DisposeInner();
}

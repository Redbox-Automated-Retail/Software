using System;
using System.IO;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.DataMatrix.Framework;

internal sealed class SnapResult : ISnapResult, IDisposable
{
	private bool Disposed;

	public string Path { get; private set; }

	public bool SnapOk { get; private set; }

	public void Dispose()
	{
		if (Disposed)
		{
			return;
		}
		Disposed = true;
		if (TestPath(Path))
		{
			try
			{
				File.Delete(Path);
			}
			catch (Exception ex)
			{
				LogHelper.Instance.Log("[SnapResult] unable to delete path '{0}'", Path);
				LogHelper.Instance.Log(ex.Message);
			}
		}
	}

	internal SnapResult(string path)
	{
		SnapOk = TestPath(path);
		Path = (SnapOk ? path : string.Empty);
	}

	private bool TestPath(string path)
	{
		if (!string.IsNullOrEmpty(path))
		{
			return File.Exists(path);
		}
		return false;
	}
}

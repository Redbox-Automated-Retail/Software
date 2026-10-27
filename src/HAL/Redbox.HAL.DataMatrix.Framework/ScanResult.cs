using System;
using System.Collections.Generic;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.DataMatrix.Framework;

internal sealed class ScanResult : IScanResult, IDisposable
{
	private readonly ISnapResult SnapResult;

	private readonly List<DecodeResult> RawDecodeResults = new List<DecodeResult>();

	private bool Disposed;

	private bool DeleteOnDispose;

	public bool DeviceError { get; internal set; }

	public TimeSpan ExecutionTime { get; internal set; }

	public List<IDecodeResult> DecodeResults
	{
		get
		{
			List<IDecodeResult> rv = new List<IDecodeResult>();
			RawDecodeResults.ForEach(delegate(DecodeResult each)
			{
				rv.Add(each);
			});
			return rv;
		}
	}

	public string ImagePath { get; private set; }

	public void Dispose()
	{
		if (!Disposed)
		{
			Disposed = true;
			if (DeleteOnDispose && DecodeResults.Count > 0 && SnapResult != null)
			{
				SnapResult.Dispose();
			}
			RawDecodeResults.Clear();
		}
	}

	internal void ResetOnException()
	{
		ExecutionTime = default(TimeSpan);
		RawDecodeResults.Clear();
	}

	internal void Add(string code)
	{
		if (!code.Equals("redbox", StringComparison.CurrentCultureIgnoreCase))
		{
			DecodeResult decodeResult = RawDecodeResults.Find((DecodeResult each) => each.Matrix == code);
			if (decodeResult == null)
			{
				decodeResult = new DecodeResult(code);
				RawDecodeResults.Add(decodeResult);
			}
			else
			{
				decodeResult.IncrementCount();
			}
		}
	}

	internal ScanResult(ISnapResult sr)
		: this(sr.Path)
	{
		SnapResult = sr;
		ImagePath = sr.Path;
	}

	internal ScanResult(string imagePath)
	{
		ExecutionTime = default(TimeSpan);
		ImagePath = imagePath;
	}

	internal ScanResult()
		: this(string.Empty)
	{
	}
}

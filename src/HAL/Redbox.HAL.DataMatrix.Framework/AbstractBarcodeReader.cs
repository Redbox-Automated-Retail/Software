using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.DataMatrix.Framework;

internal abstract class AbstractBarcodeReader : IBarcodeReader, IDisposable
{
	private bool Disposed;

	public bool IsLicensed { get; protected set; }

	public BarcodeServices Service { get; private set; }

	public void Dispose()
	{
		if (!Disposed)
		{
			OnDispose(disposing: true);
		}
	}

	public IScanResult Scan(string file)
	{
		ScanResult scanResult = new ScanResult(file);
		OnScan(scanResult);
		return scanResult;
	}

	public IScanResult Scan(ISnapResult sr)
	{
		ScanResult scanResult = new ScanResult(sr);
		OnScan(scanResult);
		return scanResult;
	}

	protected abstract void OnScan(ScanResult sr);

	protected virtual void OnDispose(bool disposing)
	{
	}

	protected AbstractBarcodeReader(BarcodeServices s)
	{
		Service = s;
	}
}

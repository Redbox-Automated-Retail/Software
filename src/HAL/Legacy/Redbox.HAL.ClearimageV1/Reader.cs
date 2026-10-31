using System;
using CiBcfKo86572612;
using ClearImage;
using Redbox.HAL.Core;

namespace Redbox.HAL.ClearimageV1;

public class Reader : IClearImageReader, IBarcodeReader
{
	private bool IsLicensed { get; set; }

	private CiBcFlow TheReader { get; set; }

	public IScanResult Scan(string file, int expectedCodes)
	{
		ClearImageScanResult val = new ClearImageScanResult();
		if (!IsLicensed)
		{
			LogHelper.Instance.Log("This version isn't licensed.");
			val.ResetOnException();
			return (IScanResult)(object)val;
		}
		for (int i = 0; i < 2; i++)
		{
			bool flag = false;
			try
			{
				TheReader.Image.Open(file);
				int num = 0;
				ExecutionTimer val2 = new ExecutionTimer();
				try
				{
					num = TheReader.Find(expectedCodes);
					val2.Stop();
					val.ExecutionTime = val2.Elapsed;
				}
				finally
				{
					((IDisposable)val2)?.Dispose();
				}
				if (num > 0)
				{
					foreach (CiBarcode barcode in TheReader.Barcodes)
					{
						val.AddCode(barcode.Text);
					}
				}
			}
			catch (Exception ex)
			{
				flag = true;
				LogHelper.Instance.Log("ClearImageScanner: exception during scan.", ex);
				ProcessHelper.DumpProcessMemoryMetrics();
				val.ResetOnException();
			}
			try
			{
				TheReader.Image.Close();
			}
			catch (Exception ex2)
			{
				LogHelper.Instance.Log("Exception closing barcode image.", ex2);
			}
			if (!flag)
			{
				return (IScanResult)(object)val;
			}
		}
		val.ResetOnException();
		return (IScanResult)(object)val;
	}

	public bool Init(int data)
	{
		LogHelper.Instance.Log("Clearimage v1 reader.init()");
		CiServer ciServer = new CiServerClass();
		try
		{
			ciServer.OpenExt(data, 74391114, 0);
			TheReader = new CiBcFlowClass
			{
				Ci = ciServer,
				Image = ciServer.CreateImage()
			};
			return IsLicensed = true;
		}
		catch (Exception ex)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in BarcodeReader.InitializeClearImage.  Calling CiServer.OpenExt to send license key failed.", ex);
			TheReader = null;
			return false;
		}
	}
}

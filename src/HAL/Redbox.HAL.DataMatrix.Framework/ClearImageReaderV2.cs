using System;
using System.Runtime.InteropServices;
using ClearImage;
using RBDM;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Timers;

namespace Redbox.HAL.DataMatrix.Framework;

internal sealed class ClearImageReaderV2 : AbstractBarcodeReader
{
	private readonly clsRBDM TheReader;

	protected override void OnScan(ScanResult result)
	{
		if (!base.IsLicensed)
		{
			LogHelper.Instance.Log("This version isn't licensed.");
			result.ResetOnException();
			return;
		}
		int maxBcIn = 6;
		for (int i = 0; i < 2; i++)
		{
			bool flag = false;
			try
			{
				TheReader.Image.Open(result.ImagePath);
				using ExecutionTimer executionTimer = new ExecutionTimer();
				int num = TheReader.Find(maxBcIn);
				executionTimer.Stop();
				result.ExecutionTime = executionTimer.Elapsed;
				if (num > 0)
				{
					foreach (CiBarcode barcode in TheReader.Barcodes)
					{
						result.Add(barcode.Text);
					}
				}
			}
			catch (Exception e)
			{
				flag = true;
				LogHelper.Instance.Log("[ClearImageReaderV2] Exception during scan.", e);
				result.ResetOnException();
			}
			try
			{
				TheReader.Image.Close();
			}
			catch (Exception e2)
			{
				LogHelper.Instance.Log("[ClearImageReaderV2] Exception closing barcode image.", e2);
			}
			if (!flag)
			{
				return;
			}
		}
		result.ResetOnException();
	}

	internal ClearImageReaderV2()
		: base(BarcodeServices.Inlite)
	{
		try
		{
			int hModule = Marshal.GetHINSTANCE(GetType().Module).ToInt32();
			CiServer ciServer = new CiServerClass();
			ciServer.OpenExt(hModule, 74391114, 0);
			TheReader = new clsRBDM
			{
				Ci = ciServer,
				Image = ciServer.CreateImage()
			};
			base.IsLicensed = true;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in BarcodeReader.InitializeClearImage. Calling CiServer.OpenExt to send license key failed.", e);
			TheReader = null;
			base.IsLicensed = false;
		}
	}
}

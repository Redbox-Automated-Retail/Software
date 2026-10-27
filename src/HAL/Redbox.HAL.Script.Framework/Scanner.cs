using System;
using System.Collections.Generic;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

internal struct Scanner : IDisposable
{
	private readonly IScannerDeviceService ScannerDeviceService;

	private readonly IControlSystem ControlSystem;

	private readonly IScannerDevice Device;

	private bool Disposed;

	private static CenterDiskMethod[] m_smartReadMethods = new CenterDiskMethod[2]
	{
		CenterDiskMethod.VendDoorAndBack,
		CenterDiskMethod.DrumAndBack
	};

	public void Dispose()
	{
		if (!Disposed)
		{
			Disposed = true;
			if (Device != null && Device.RequiresExternalLighting)
			{
				ControlSystem.ToggleRingLight(on: false, Device.SupportsSecureReads ? ControllerConfiguration.Instance.IRRinglightShutdownPause : 0);
			}
			GC.SuppressFinalize(this);
		}
	}

	internal static ScanResult SmartReadDisk()
	{
		return SmartReadDisk(m_smartReadMethods);
	}

	internal static ScanResult SmartReadDisk(CenterDiskMethod[] methods)
	{
		IControlSystem service = ServiceLocator.Instance.GetService<IControlSystem>();
		ScanResult result = (ServiceLocator.Instance.GetService<IScannerDeviceService>().GetConfiguredDevice().SupportsSecureReads ? OnIRScan(service, methods) : OnNonIRScan(service, methods));
		CreateJsonResult(result);
		return result;
	}

	internal static string ReadDiskInPicker(ReadDiskOptions options, ExecutionContext context)
	{
		string result = "UNKNOWN";
		IControlSystem service = ServiceLocator.Instance.GetService<IControlSystem>();
		using (Scanner scanner = new Scanner(CenterDiskMethod.VendDoorAndBack, service))
		{
			using ScanResult scanResult = scanner.Read();
			result = scanResult.ScannedMatrix;
			if (!scanResult.SnapOk)
			{
				if ((options & ReadDiskOptions.LeaveCaptureResult) != ReadDiskOptions.None)
				{
					context.CreateCameraCaptureErrorResult();
				}
			}
			else if (scanResult.ReadCode)
			{
				if ((options & ReadDiskOptions.CheckForDuplicate) != ReadDiskOptions.None && ServiceLocator.Instance.GetService<IInventoryService>().IsBarcodeDuplicate(scanResult.ScannedMatrix, out var _))
				{
					context.CreateDuplicateItemResult(scanResult.ScannedMatrix);
				}
			}
			else if ((options & ReadDiskOptions.LeaveNoReadResult) != ReadDiskOptions.None)
			{
				context.CreateNoBarcodeReadResult();
			}
		}
		return result;
	}

	internal ScanResult Read()
	{
		ScanResult scanResult = new ScanResult(Device.Scan());
		IExecutionContext activeContext = ServiceLocator.Instance.GetService<IExecutionService>().GetActiveContext();
		if (!scanResult.SnapOk)
		{
			LogHelper.Instance.Log(LogEntryType.Error, "[{0}, {1}]: Read disk returned a camera error status CAMERA CAPTURE ERROR", activeContext.ProgramName, activeContext.ID);
			activeContext?.ContextLog.WriteFormatted("No image was captured.");
		}
		else if (!scanResult.ReadCode)
		{
			LogHelper.Instance.WithContext(toFormattedLog: true, LogEntryType.Error, "No barcodes read on image.");
		}
		return scanResult;
	}

	internal Scanner(IControlSystem cs)
	{
		this = default(Scanner);
		Disposed = false;
		ControlSystem = cs;
		ScannerDeviceService = ServiceLocator.Instance.GetService<IScannerDeviceService>();
		Device = ScannerDeviceService.GetConfiguredDevice();
		Device.Start();
		OnStartRinglight();
	}

	internal Scanner(CenterDiskMethod method, IControlSystem cs)
	{
		this = default(Scanner);
		Disposed = false;
		ControlSystem = cs;
		ScannerDeviceService = ServiceLocator.Instance.GetService<IScannerDeviceService>();
		Device = ScannerDeviceService.GetConfiguredDevice();
		Device.Start();
		if (Device.SupportsSecureReads)
		{
			if (method != CenterDiskMethod.None)
			{
				ControlSystem.Center(method);
			}
			OnStartRinglight();
		}
		else
		{
			OnStartRinglight();
			if (method != CenterDiskMethod.None)
			{
				ControlSystem.Center(method);
			}
		}
	}

	private static ScanResult OnNonIRScan(IControlSystem cs, CenterDiskMethod[] methods)
	{
		ScanResult scanResult = null;
		using Scanner scanner = new Scanner(cs);
		for (int i = 0; i < methods.Length; i++)
		{
			cs.Center(methods[i]);
			scanResult = scanner.OnReadWithRetry();
			scanResult.ReadAttempts = i + 1;
			if (!scanResult.SnapOk || scanResult.ReadCode)
			{
				return scanResult;
			}
			if (i != methods.Length - 1)
			{
				scanResult.Dispose();
			}
		}
		return scanResult;
	}

	private static ScanResult OnIRScan(IControlSystem cs, CenterDiskMethod[] methods)
	{
		ScanResult scanResult = null;
		CheckAndResizeForAdditionalFraudAttempts(ref methods);
		LogHelper.Instance.Log($"Number Of Center Methods: {methods.Length}, RetryEnabled: {ControllerConfiguration.Instance.RetryReadOnNoMarkersFound}, AdditionalAttempts: {ControllerConfiguration.Instance.AdditionalFraudReadAttempts}");
		for (int i = 0; i < methods.Length; i++)
		{
			using Scanner scanner = new Scanner(methods[i], cs);
			scanResult = scanner.OnReadWithRetry();
			scanResult.ReadAttempts = i + 1;
			if (!scanResult.SnapOk || (scanResult.ReadCode && (!ControllerConfiguration.Instance.RetryReadOnNoMarkersFound || (ControllerConfiguration.Instance.RetryReadOnNoMarkersFound && scanResult.SecureTokensFound > 0))))
			{
				return scanResult;
			}
			if (scanResult.ReadCode && ControllerConfiguration.Instance.RetryReadOnNoMarkersFound && scanResult.SecureTokensFound <= 0)
			{
				LogHelper.Instance.Log($"{i} - OnIRScan Retry Read Image - No Secure Tokens Found.");
			}
			if (i != methods.Length - 1)
			{
				scanResult.Dispose();
			}
		}
		return scanResult;
	}

	private void OnStartRinglight()
	{
		if (Device.RequiresExternalLighting)
		{
			ControlSystem.ToggleRingLight(on: true, Device.SupportsSecureReads ? ControllerConfiguration.Instance.IRRinglightStartupPause : ControllerConfiguration.Instance.RinglightWarmupPause2);
		}
	}

	private ScanResult OnReadWithRetry()
	{
		ScanResult scanResult = Read();
		if (scanResult.SnapOk || !ControllerConfiguration.Instance.EnableCameraReset)
		{
			return scanResult;
		}
		if (Device.Restart())
		{
			scanResult = Read();
		}
		return scanResult;
	}

	internal static void CheckAndResizeForAdditionalFraudAttempts(ref CenterDiskMethod[] methods)
	{
		if (ControllerConfiguration.Instance.RetryReadOnNoMarkersFound && ControllerConfiguration.Instance.AdditionalFraudReadAttempts > 0)
		{
			int num = methods.Length;
			int num2 = num + ControllerConfiguration.Instance.AdditionalFraudReadAttempts;
			LogHelper.Instance.Log($"original length: {num}, new length: {num2}");
			Array.Resize(ref methods, num2);
			for (int i = num; i < num2; i++)
			{
				methods[i] = CenterDiskMethod.None;
			}
		}
	}

	internal static void CreateJsonResult(ScanResult result)
	{
		ServiceLocator.Instance.GetService<IExecutionService>().GetActiveContext().CreateJSONResult(jsonMessage: new Dictionary<string, object>
		{
			{ "BarcodesRead", result.ReadCount },
			{ "SecureCodesRead", result.SecureTokensFound },
			{ "ReadAttempts", result.ReadAttempts },
			{
				"ExecutionTime",
				result.ExecutionTime.ToString()
			}
		}, code: "CameraStatsJson", item: result.ScannedMatrix.ToString());
	}
}

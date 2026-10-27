using System;
using System.IO;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.DataMatrix.Framework;

namespace Redbox.HAL.Script.Framework.Services;

public sealed class FraudValidatorService : IFraudService
{
	private string m_fraudImagesPath;

	public bool IsConfigured
	{
		get
		{
			IScannerDeviceService service = ServiceLocator.Instance.GetService<IScannerDeviceService>();
			BarcodeConfiguration.NewInstance();
			IScannerDevice configuredDevice = service.GetConfiguredDevice();
			LogHelper.Instance.Log($"IsLegacy: {configuredDevice.IsLegacy}, Camera Generation: {configuredDevice.CurrentCameraGeneration.ToString()}");
			LogHelper.Instance.Log($"Device.SupportsSecureReads: {configuredDevice.SupportsSecureReads}, EnablePhotocopyScan: {ControllerConfiguration.Instance.EnablePhotocopyScan} - IsConfigured: {configuredDevice.SupportsSecureReads && ControllerConfiguration.Instance.EnablePhotocopyScan}");
			if (configuredDevice.SupportsSecureReads)
			{
				return ControllerConfiguration.Instance.EnablePhotocopyScan;
			}
			return false;
		}
	}

	public string FraudImagesPath
	{
		get
		{
			if (string.IsNullOrEmpty(m_fraudImagesPath))
			{
				IBarcodeReaderFactory service = ServiceLocator.Instance.GetService<IBarcodeReaderFactory>();
				m_fraudImagesPath = service.ImagePath;
				if (!Directory.Exists(m_fraudImagesPath))
				{
					try
					{
						Directory.CreateDirectory(m_fraudImagesPath);
					}
					catch (Exception e)
					{
						LogHelper.Instance.WithContext(false, LogEntryType.Error, "Failed to create directory {0}", m_fraudImagesPath);
						LogHelper.Instance.Log("Exception info ", e);
					}
				}
			}
			return m_fraudImagesPath;
		}
	}
}

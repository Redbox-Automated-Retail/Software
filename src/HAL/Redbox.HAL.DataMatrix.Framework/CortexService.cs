using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.DataMatrix.Framework.Cortex;

namespace Redbox.HAL.DataMatrix.Framework;

internal sealed class CortexService : ScannerAdapter, IScannerDevice, IDisposable, IConfigurationObserver
{
	private class ValidatorSink : IMessageSink
	{
		public bool Send(string message)
		{
			LogHelper.Instance.Log("[CortexService] <Validate message> {0}", message);
			return true;
		}
	}

	private ICommPort Port;

	private string ConfiguredPortName = "NONE";

	private bool LogDetailedScan;

	private readonly CommPortReadModes Mode = CommPortReadModes.Callback;

	private readonly ICommChannelConfiguration Configuration;

	public bool RequiresExternalLighting => false;

	public bool IsConnected { get; private set; }

	public bool InError => false;

	public bool IsLegacy => false;

	public bool SupportsSecureReads => true;

	public ScannerServices Service => ScannerServices.Cortex;

	public CameraGeneration CurrentCameraGeneration => CameraGeneration.Fifth;

	public bool Start()
	{
		return Start(unused: false);
	}

	public bool Start(bool unused)
	{
		if (IsConnected)
		{
			return true;
		}
		if (Port != null)
		{
			if (!Port.Open())
			{
				LogHelper.Instance.Log("[Cortex Service] Unable to open communcation port {0}.", Port.DisplayName);
				return false;
			}
		}
		else
		{
			IPortManagerService service = ServiceLocator.Instance.GetService<IPortManagerService>();
			Port = ((ConfiguredPortName == "NONE") ? service.Scan(Configuration, TestPort, Mode) : service.Scan(ConfiguredPortName, Configuration, TestPort, Mode));
			if (Port == null)
			{
				LogHelper.Instance.Log("[Cortex Service] Unable to locate communcation port.");
				return false;
			}
			ConfiguredPortName = Port.PortName;
		}
		new InfaredLightCommand().Send(Port);
		RuntimeService.Wait(300);
		ScanResult scanResult = OnScan();
		return IsConnected = !scanResult.DeviceError;
	}

	public bool Stop()
	{
		if (!IsConnected)
		{
			return true;
		}
		if (Port != null)
		{
			ServiceLocator.Instance.GetService<IPortManagerService>().Dispose(Port);
			Port = null;
		}
		IsConnected = false;
		return true;
	}

	public bool Restart()
	{
		Stop();
		RuntimeService.Wait(500);
		return Start();
	}

	public IScanResult Scan()
	{
		if (!IsConnected)
		{
			LogHelper.Instance.Log("[Cortex Service] Scan is called without initialization.");
			ScanResult scanResult = new ScanResult();
			scanResult.ResetOnException();
			scanResult.DeviceError = true;
			return scanResult;
		}
		return OnScan();
	}

	public ISnapResult Snap()
	{
		string text = GenerateSnapFileName();
		if (!IsConnected)
		{
			LogHelper.Instance.Log("[Cortex Service] Snap: camera is not connected.");
			return new SnapResult(text);
		}
		if (BarcodeConfiguration.Instance.UseCortexHDField)
		{
			using SetHighDensityFieldCommand setHighDensityFieldCommand = new SetHighDensityFieldCommand();
			setHighDensityFieldCommand.Send(Port);
		}
		using TakePictureCommand takePictureCommand = new TakePictureCommand();
		takePictureCommand.Send(Port);
		if (takePictureCommand.PortError)
		{
			LogHelper.Instance.Log("[CortexService]: There was a communication error.");
			return new SnapResult(text);
		}
		ImagePacket imagePacket = takePictureCommand.ImagePacket;
		if (imagePacket == null)
		{
			LogHelper.Instance.Log("[CortexService]: unable to locate image packet during snap.");
			return new SnapResult(text);
		}
		using (GetImageCommand getImageCommand = new GetImageCommand(imagePacket))
		{
			getImageCommand.Send(Port);
			if (getImageCommand.PortError)
			{
				LogHelper.Instance.Log("[CortexService]: failed to read image data from device.");
				return new SnapResult(text);
			}
			LogHelper.Instance.Log("[Cortex Service] Snap time: {0}ms", getImageCommand.ExecutionTime.Milliseconds);
			getImageCommand.CreateImage(text);
		}
		new DeleteFileCommand(imagePacket).Send(Port);
		return new SnapResult(text);
	}

	public bool ValidateSettings()
	{
		return ValidateSettings(new ValidatorSink());
	}

	public bool ValidateSettings(IMessageSink sink)
	{
		return new CortexSettingsValidator(this, RuntimeService).Validate(sink);
	}

	public void NotifyConfigurationLoaded()
	{
		UpdateConfiguration();
	}

	public void NotifyConfigurationChangeStart()
	{
	}

	public void NotifyConfigurationChangeEnd()
	{
		UpdateConfiguration();
	}

	protected override void DisposeInner()
	{
		Stop();
	}

	internal CortexService()
	{
		IPortManagerService service = ServiceLocator.Instance.GetService<IPortManagerService>();
		Configuration = service.CreateConfiguration();
		int cortexPortBufferSize = BarcodeConfiguration.Instance.CortexPortBufferSize;
		Configuration.ReceiveBufferSize = cortexPortBufferSize;
		LogHelper.Instance.Log("[CortexService] Receive buffer size = {0}", cortexPortBufferSize);
		Configuration.WriteTimeout = 2000;
	}

	internal ICommPort ConfiguredPort()
	{
		return Port;
	}

	private void UpdateConfiguration()
	{
		Configuration.OpenPause = BarcodeConfiguration.Instance.CortexPortOpenWait;
		Configuration.WritePause = BarcodeConfiguration.Instance.WritePause;
		LogDetailedScan = BarcodeConfiguration.Instance.LogDetailedScan;
		Configuration.EnableDebug = LogDetailedScan;
	}

	private ScanResult OnScan()
	{
		ISnapResult sr = Snap();
		ScanResult result = new ScanResult(sr);
		using (DecodeCommand decodeCommand = new DecodeCommand())
		{
			decodeCommand.Send(Port);
			if (decodeCommand.PortError)
			{
				LogHelper.Instance.Log("[Cortex Service] Scan: There was a communication error with the device.");
				result.ResetOnException();
				result.DeviceError = true;
				return result;
			}
			decodeCommand.FoundCodes.ForEach(delegate(string each)
			{
				result.Add(each);
			});
			result.ExecutionTime = decodeCommand.ExecutionTime;
		}
		return result;
	}

	private bool TestPort(ICommPort port)
	{
		using GetRegisterValueCommand getRegisterValueCommand = new GetRegisterValueCommand(new CortexRegister("21D", "1"));
		getRegisterValueCommand.Send(port);
		bool isCorrectlySet = getRegisterValueCommand.IsCorrectlySet;
		LogHelper.Instance.Log("[Cortex Service] Test port returned {0} on port {1}", isCorrectlySet, port.DisplayName);
		return isCorrectlySet;
	}
}

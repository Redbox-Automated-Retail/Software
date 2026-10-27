using System;
using System.IO;
using System.Xml;
using Redbox.HAL.Core;

namespace Redbox.HAL.Camera.Framework;

public sealed class CameraService : IDisposable
{
	public bool IsRunning { get; private set; }

	public bool SupportsRestart
	{
		get
		{
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Invalid comparison between I4 and Unknown
			if (Plugin.SupportsReset)
			{
				return 3 == (int)RuntimeService.Instance.GetPlatform();
			}
			return false;
		}
	}

	public bool CameraInErroredState { get; private set; }

	public static CameraService Instance => Singleton<CameraService>.Instance;

	private ICameraPlugin Plugin { get; set; }

	public void Dispose()
	{
		Plugin.TurnOff();
		GC.SuppressFinalize(this);
	}

	public bool Start()
	{
		if (IsRunning)
		{
			return true;
		}
		try
		{
			return IsRunning = Plugin.Start();
		}
		catch (Exception ex)
		{
			LogHelper.Instance.Log("CameraService.Start() caught an exception.", ex);
			return IsRunning = false;
		}
	}

	public string Snap()
	{
		if (CameraConfiguration.Instance.Mode == CameraMode.Emulated)
		{
			return CameraEmulationState.Instance.ExpectedStartCaptureResult;
		}
		string text = Plugin.Snap();
		try
		{
			if (string.IsNullOrEmpty(text) || !File.Exists(text))
			{
				LogHelper.Instance.Log("File doesn't exist; turn off camera.");
				ProcessHelper.DumpProcessMemoryMetrics();
				if (Plugin.SupportsReset)
				{
					TurnOffCamera();
				}
			}
		}
		catch (Exception ex)
		{
			LogHelper.Instance.Log("CameraService.Snap() caught an exception.", ex);
			CameraInErroredState = true;
		}
		return text;
	}

	public bool Stop()
	{
		if (!CameraConfiguration.Instance.CycleCameraOnUse)
		{
			return true;
		}
		if (!IsRunning)
		{
			return false;
		}
		IsRunning = false;
		try
		{
			Plugin.Stop();
			return true;
		}
		catch (Exception ex)
		{
			LogHelper.Instance.Log("CameraService.Stop() caught an exception.", ex);
			return false;
		}
	}

	public void TurnOffCamera()
	{
		if (IsRunning)
		{
			try
			{
				Plugin.TurnOff();
			}
			catch (Exception ex)
			{
				LogHelper.Instance.Log("TurnOffCamera encountered an exception.", ex);
			}
			IsRunning = false;
			CameraInErroredState = false;
		}
	}

	public bool RestartCamera(bool cycleDriver)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected O, but got Unknown
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Invalid comparison between I4 and Unknown
		if (!Plugin.SupportsReset)
		{
			return false;
		}
		TurnOffCamera();
		PerfFunctions.SpinWait(500);
		if (cycleDriver)
		{
			DeviceHelper val = new DeviceHelper(false);
			try
			{
				CameraInfo cameraInfo = new CameraInfo(val);
				if (cameraInfo.FindActiveCamera())
				{
					LogHelper.Instance.Log("Found camera {0}", new object[1] { cameraInfo.Revision.ToString() });
					if (cameraInfo.Revision == KnownCameras.ThirdGen || cameraInfo.Revision == KnownCameras.FourthGen)
					{
						LogHelper.Instance.Log("The running driver for the camera {0}", new object[1] { cameraInfo.CorrectDriverRunning() ? "is correct" : "is not correct." });
					}
					if (1 == (int)cameraInfo.Status)
					{
						bool flag = cameraInfo.ResetDriver();
						LogHelper.Instance.Log(" RESET camera returned {0}", new object[1] { flag ? "OK" : "FAILURE" });
					}
				}
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		return Start();
	}

	private CameraService()
	{
		if (CameraConfiguration.Instance.Mode != CameraMode.Emulated)
		{
			if (!Directory.Exists(CameraConfiguration.Instance.WorkingPath))
			{
				Directory.CreateDirectory(CameraConfiguration.Instance.WorkingPath);
			}
			LoadPluginsInner();
		}
	}

	private void LoadPluginsInner()
	{
		string cameraDriver = CameraConfiguration.Instance.CameraDriver;
		CameraPluginDescriptor cameraPluginDescriptor = PluginLoader.Instance.LocatePlugin(cameraDriver, out var plugin);
		if (cameraPluginDescriptor == null)
		{
			LogHelper.Instance.Log($"Unable to locate plugin {cameraDriver}", (LogEntryType)2);
			Plugin = (ICameraPlugin)(object)new NilCameraPlugin();
		}
		else
		{
			LogHelper.Instance.Log($"Successfully loaded camera plugin {cameraDriver}");
			Plugin = plugin;
			if (cameraPluginDescriptor.DescriptorPath != null)
			{
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.Load(cameraPluginDescriptor.DescriptorPath);
				if (xmlDocument.DocumentElement != null)
				{
					XmlNode xmlNode = xmlDocument.SelectSingleNode("Plugin/Properties");
					if (xmlNode != null)
					{
						foreach (XmlNode childNode in xmlNode.ChildNodes)
						{
							if (!childNode.Name.Equals("property"))
							{
								continue;
							}
							string value = childNode.Attributes["name"].Value;
							string value2 = childNode.Attributes["value"].Value;
							XmlAttribute xmlAttribute = childNode.Attributes["type"];
							if (xmlAttribute == null)
							{
								CameraConfiguration.Instance.Properties[value] = value2;
								continue;
							}
							try
							{
								Type type = Type.GetType(xmlAttribute.Value);
								CameraConfiguration.Instance.Properties[value] = Convert.ChangeType(value2, type);
							}
							catch
							{
								CameraConfiguration.Instance.Properties[value] = value2;
							}
						}
					}
				}
			}
		}
		Plugin.InitWithProperties(CameraConfiguration.Instance.Properties);
	}
}

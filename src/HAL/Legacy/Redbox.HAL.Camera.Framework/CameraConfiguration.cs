using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Xml;
using Redbox.HAL.Core;

namespace Redbox.HAL.Camera.Framework;

public sealed class CameraConfiguration : XmlConfiguration
{
	private const string Win7CameraDriver = "DirectShowFrame";

	private string m_workingDirectory;

	public static CameraConfiguration Instance => Singleton<CameraConfiguration>.Instance;

	[DisplayName("Driver Mode")]
	[ExcludeType]
	[Category("Settings")]
	[Description("The mode under which the camera driver runs. If set to Emulated, all camera driver functions will be emulated based on the configured emulation state.")]
	[ValidValueListProvider("GetValidModes")]
	public CameraMode Mode { get; set; }

	[Description("The size of the video capture frame.  Larger frame sizes yield a higher capture resolution at the cost of high CPU utilization, memory usage, and disk space.")]
	[Category("Settings")]
	[DisplayName("Video Format")]
	public Size VideoFormat { get; set; }

	[Description("If true, starts/stops the camera; otherwise, keeps it on.")]
	[Category("Settings")]
	[DisplayName("Cycle Camera On Use")]
	public bool CycleCameraOnUse { get; set; }

	[DisplayName("Delete Bitmap On Successful Read")]
	[Description("If true, will delete the bitmap used to read the barcode, if successfully read.")]
	[Category("Settings")]
	public bool DeleteBitmapOnSuccessfulRead { get; set; }

	[Description("The plugin to use.")]
	[Category("Settings")]
	[DisplayName("Camera Driver")]
	public string CameraDriver { get; set; }

	internal IDictionary<string, object> Properties { get; private set; }

	internal string WorkingPath
	{
		get
		{
			if (!string.IsNullOrEmpty(m_workingDirectory))
			{
				return m_workingDirectory;
			}
			return m_workingDirectory = Path.GetFullPath(Path.Combine(RuntimeService.Instance.AssemblyDirectory, "..\\Video"));
		}
	}

	public string[] GetValidModes()
	{
		return new string[2]
		{
			CameraMode.Normal.ToString(),
			CameraMode.Emulated.ToString()
		};
	}

	protected override void LoadProperties(XmlDocument document, ErrorList errors)
	{
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Invalid comparison between Unknown and I4
		Mode = GetNodeValue<CameraMode>((XmlNode)document.DocumentElement, "Camera/Mode", CameraMode.Normal);
		int nodeValue = GetNodeValue<int>((XmlNode)document.DocumentElement, "Camera/VideoFormat/Width", 640);
		int nodeValue2 = GetNodeValue<int>((XmlNode)document.DocumentElement, "Camera/VideoFormat/Height", 480);
		VideoFormat = new Size(nodeValue, nodeValue2);
		DeleteBitmapOnSuccessfulRead = GetNodeValue<bool>((XmlNode)document.DocumentElement, "Camera/DeleteBitmapOnSuccessfulRead", true);
		CameraDriver = GetNodeValue<string>((XmlNode)document.DocumentElement, "Camera/CameraDriver", "Vidcap");
		if ((int)RuntimeService.Instance.GetPlatform() == 3 && CameraDriver == "Vidcap")
		{
			LogHelper.Instance.Log("Platform is Windows7; hijack control to {0}.", new object[1] { "DirectShowFrame" });
			CameraDriver = "DirectShowFrame";
		}
		CycleCameraOnUse = GetNodeValue<bool>((XmlNode)document.DocumentElement, "Camera/CycleCameraOnUse", false);
		if (CycleCameraOnUse)
		{
			LogHelper.Instance.Log("WARNING: CycleCameraOnUse is set to TRUE.");
		}
		SaveProperties();
	}

	protected override void StoreProperties(XmlDocument document, ErrorList errors)
	{
		XmlNode xmlNode = document.DocumentElement.SelectSingleNode("Camera/Mode");
		if (xmlNode != null)
		{
			xmlNode.InnerText = Mode.ToString();
		}
		XmlNode xmlNode2 = document.DocumentElement.SelectSingleNode("Camera/VideoFormat/Width");
		if (xmlNode2 != null)
		{
			xmlNode2.InnerText = XmlConvert.ToString(VideoFormat.Width);
		}
		XmlNode xmlNode3 = document.DocumentElement.SelectSingleNode("Camera/VideoFormat/Height");
		if (xmlNode3 != null)
		{
			xmlNode3.InnerText = XmlConvert.ToString(VideoFormat.Height);
		}
		XmlNode xmlNode4 = document.DocumentElement.SelectSingleNode("Camera/DeleteBitmapOnSuccessfulRead");
		if (xmlNode4 != null)
		{
			xmlNode4.InnerText = XmlConvert.ToString(DeleteBitmapOnSuccessfulRead);
		}
		XmlNode xmlNode5 = document.DocumentElement.SelectSingleNode("Camera/CameraDriver");
		if (xmlNode5 != null)
		{
			xmlNode5.InnerText = CameraDriver;
		}
		else
		{
			SetSingleNodeValue((XmlNode)document.DocumentElement, "Camera/CameraDriver", (object)CameraDriver);
		}
		XmlNode xmlNode6 = document.DocumentElement.SelectSingleNode("Camera/CycleCameraOnUse");
		if (xmlNode6 != null)
		{
			xmlNode6.InnerText = XmlConvert.ToString(CycleCameraOnUse);
		}
		else
		{
			SetSingleNodeValue((XmlNode)document.DocumentElement, "Camera/CycleCameraOnUse", (object)CycleCameraOnUse);
		}
	}

	private CameraConfiguration()
		: base("cameraConfig")
	{
		Mode = CameraMode.Normal;
		VideoFormat = new Size(640, 480);
		CameraDriver = "DirectShowFrame";
		Properties = new Dictionary<string, object>();
		SaveProperties();
	}

	private void SaveProperties()
	{
		Properties["WorkingPath"] = WorkingPath;
		Properties["Mode"] = Mode;
		Properties["VideoFormat"] = VideoFormat;
	}
}

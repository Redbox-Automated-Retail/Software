using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using VIDEOCAPLib;
using stdole;

namespace AxVIDEOCAPLib;

[DefaultEvent("Capturing")]
[DesignTimeVisible(true)]
[Clsid("{aa408ecc-e50b-401f-8036-021e81b9e5a1}")]
[ComVisible(true)]
public class AxVideoCap : AxHost
{
	private _DVideoCap ocx;

	private AxVideoCapEventMulticaster eventMulticaster;

	private ConnectionPointCookie cookie;

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[DispId(1)]
	public virtual short Device
	{
		get
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("Device", ActiveXInvokeKind.PropertyGet);
			}
			return ocx.Device;
		}
		set
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("Device", ActiveXInvokeKind.PropertySet);
			}
			ocx.Device = value;
		}
	}

	[DispId(2)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public virtual short VideoFormat
	{
		get
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("VideoFormat", ActiveXInvokeKind.PropertyGet);
			}
			return ocx.VideoFormat;
		}
		set
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("VideoFormat", ActiveXInvokeKind.PropertySet);
			}
			ocx.VideoFormat = value;
		}
	}

	[DispId(3)]
	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public virtual Devices Devices
	{
		get
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("Devices", ActiveXInvokeKind.PropertyGet);
			}
			return ocx.Devices;
		}
		set
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("Devices", ActiveXInvokeKind.PropertySet);
			}
			ocx.Devices = value;
		}
	}

	[Browsable(false)]
	[DispId(4)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public virtual VideoFormats VideoFormats
	{
		get
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("VideoFormats", ActiveXInvokeKind.PropertyGet);
			}
			return ocx.VideoFormats;
		}
		set
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("VideoFormats", ActiveXInvokeKind.PropertySet);
			}
			ocx.VideoFormats = value;
		}
	}

	[Browsable(false)]
	[DispId(5)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public virtual VideoCompressors VideoCompressors
	{
		get
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("VideoCompressors", ActiveXInvokeKind.PropertyGet);
			}
			return ocx.VideoCompressors;
		}
		set
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("VideoCompressors", ActiveXInvokeKind.PropertySet);
			}
			ocx.VideoCompressors = value;
		}
	}

	[DispId(6)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public virtual bool CaptureMode
	{
		get
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("CaptureMode", ActiveXInvokeKind.PropertyGet);
			}
			return ocx.CaptureMode;
		}
		set
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("CaptureMode", ActiveXInvokeKind.PropertySet);
			}
			ocx.CaptureMode = value;
		}
	}

	[DispId(7)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public virtual string CaptureFileName
	{
		get
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("CaptureFileName", ActiveXInvokeKind.PropertyGet);
			}
			return ocx.CaptureFileName;
		}
		set
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("CaptureFileName", ActiveXInvokeKind.PropertySet);
			}
			ocx.CaptureFileName = value;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	[DispId(8)]
	public virtual Brightness Brightness
	{
		get
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("Brightness", ActiveXInvokeKind.PropertyGet);
			}
			return ocx.Brightness;
		}
		set
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("Brightness", ActiveXInvokeKind.PropertySet);
			}
			ocx.Brightness = value;
		}
	}

	[DispId(9)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public virtual Contrast Contrast
	{
		get
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("Contrast", ActiveXInvokeKind.PropertyGet);
			}
			return ocx.Contrast;
		}
		set
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("Contrast", ActiveXInvokeKind.PropertySet);
			}
			ocx.Contrast = value;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	[DispId(10)]
	public virtual Hue Hue
	{
		get
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("Hue", ActiveXInvokeKind.PropertyGet);
			}
			return ocx.Hue;
		}
		set
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("Hue", ActiveXInvokeKind.PropertySet);
			}
			ocx.Hue = value;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[DispId(11)]
	[Browsable(false)]
	public virtual Saturation Saturation
	{
		get
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("Saturation", ActiveXInvokeKind.PropertyGet);
			}
			return ocx.Saturation;
		}
		set
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("Saturation", ActiveXInvokeKind.PropertySet);
			}
			ocx.Saturation = value;
		}
	}

	[DispId(12)]
	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public virtual Sharpness Sharpness
	{
		get
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("Sharpness", ActiveXInvokeKind.PropertyGet);
			}
			return ocx.Sharpness;
		}
		set
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("Sharpness", ActiveXInvokeKind.PropertySet);
			}
			ocx.Sharpness = value;
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[DispId(13)]
	public virtual Gamma Gamma
	{
		get
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("Gamma", ActiveXInvokeKind.PropertyGet);
			}
			return ocx.Gamma;
		}
		set
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("Gamma", ActiveXInvokeKind.PropertySet);
			}
			ocx.Gamma = value;
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[DispId(14)]
	public virtual ColorEnable ColorEnable
	{
		get
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("ColorEnable", ActiveXInvokeKind.PropertyGet);
			}
			return ocx.ColorEnable;
		}
		set
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("ColorEnable", ActiveXInvokeKind.PropertySet);
			}
			ocx.ColorEnable = value;
		}
	}

	[DispId(15)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public virtual WhiteBalance WhiteBalance
	{
		get
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("WhiteBalance", ActiveXInvokeKind.PropertyGet);
			}
			return ocx.WhiteBalance;
		}
		set
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("WhiteBalance", ActiveXInvokeKind.PropertySet);
			}
			ocx.WhiteBalance = value;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	[DispId(16)]
	public virtual BacklightCompensation BacklightCompensation
	{
		get
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("BacklightCompensation", ActiveXInvokeKind.PropertyGet);
			}
			return ocx.BacklightCompensation;
		}
		set
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("BacklightCompensation", ActiveXInvokeKind.PropertySet);
			}
			ocx.BacklightCompensation = value;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[DispId(17)]
	[Browsable(false)]
	public virtual Gain Gain
	{
		get
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("Gain", ActiveXInvokeKind.PropertyGet);
			}
			return ocx.Gain;
		}
		set
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("Gain", ActiveXInvokeKind.PropertySet);
			}
			ocx.Gain = value;
		}
	}

	[DispId(18)]
	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public virtual Pan Pan
	{
		get
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("Pan", ActiveXInvokeKind.PropertyGet);
			}
			return ocx.Pan;
		}
		set
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("Pan", ActiveXInvokeKind.PropertySet);
			}
			ocx.Pan = value;
		}
	}

	[DispId(19)]
	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public virtual Tilt Tilt
	{
		get
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("Tilt", ActiveXInvokeKind.PropertyGet);
			}
			return ocx.Tilt;
		}
		set
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("Tilt", ActiveXInvokeKind.PropertySet);
			}
			ocx.Tilt = value;
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[DispId(20)]
	public virtual Roll Roll
	{
		get
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("Roll", ActiveXInvokeKind.PropertyGet);
			}
			return ocx.Roll;
		}
		set
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("Roll", ActiveXInvokeKind.PropertySet);
			}
			ocx.Roll = value;
		}
	}

	[DispId(21)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public virtual Zoom Zoom
	{
		get
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("Zoom", ActiveXInvokeKind.PropertyGet);
			}
			return ocx.Zoom;
		}
		set
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("Zoom", ActiveXInvokeKind.PropertySet);
			}
			ocx.Zoom = value;
		}
	}

	[DispId(22)]
	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public virtual Exposure Exposure
	{
		get
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("Exposure", ActiveXInvokeKind.PropertyGet);
			}
			return ocx.Exposure;
		}
		set
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("Exposure", ActiveXInvokeKind.PropertySet);
			}
			ocx.Exposure = value;
		}
	}

	[DispId(23)]
	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public virtual Iris Iris
	{
		get
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("Iris", ActiveXInvokeKind.PropertyGet);
			}
			return ocx.Iris;
		}
		set
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("Iris", ActiveXInvokeKind.PropertySet);
			}
			ocx.Iris = value;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	[DispId(24)]
	public virtual Focus CtlFocus
	{
		get
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("CtlFocus", ActiveXInvokeKind.PropertyGet);
			}
			return ocx.Focus;
		}
		set
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("CtlFocus", ActiveXInvokeKind.PropertySet);
			}
			ocx.Focus = value;
		}
	}

	[DispId(25)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public virtual AudioDevices AudioDevices
	{
		get
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("AudioDevices", ActiveXInvokeKind.PropertyGet);
			}
			return ocx.AudioDevices;
		}
		set
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("AudioDevices", ActiveXInvokeKind.PropertySet);
			}
			ocx.AudioDevices = value;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[DispId(26)]
	public virtual short AudioDevice
	{
		get
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("AudioDevice", ActiveXInvokeKind.PropertyGet);
			}
			return ocx.AudioDevice;
		}
		set
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("AudioDevice", ActiveXInvokeKind.PropertySet);
			}
			ocx.AudioDevice = value;
		}
	}

	[DispId(27)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public virtual bool CaptureAudio
	{
		get
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("CaptureAudio", ActiveXInvokeKind.PropertyGet);
			}
			return ocx.CaptureAudio;
		}
		set
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("CaptureAudio", ActiveXInvokeKind.PropertySet);
			}
			ocx.CaptureAudio = value;
		}
	}

	[DispId(28)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public virtual short VideoCompressor
	{
		get
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("VideoCompressor", ActiveXInvokeKind.PropertyGet);
			}
			return ocx.VideoCompressor;
		}
		set
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("VideoCompressor", ActiveXInvokeKind.PropertySet);
			}
			ocx.VideoCompressor = value;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[DispId(29)]
	public virtual short AudioCompressor
	{
		get
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("AudioCompressor", ActiveXInvokeKind.PropertyGet);
			}
			return ocx.AudioCompressor;
		}
		set
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("AudioCompressor", ActiveXInvokeKind.PropertySet);
			}
			ocx.AudioCompressor = value;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[DispId(30)]
	public virtual bool UseVideoCompressor
	{
		get
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("UseVideoCompressor", ActiveXInvokeKind.PropertyGet);
			}
			return ocx.UseVideoCompressor;
		}
		set
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("UseVideoCompressor", ActiveXInvokeKind.PropertySet);
			}
			ocx.UseVideoCompressor = value;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[DispId(31)]
	public virtual bool UseAudioCompressor
	{
		get
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("UseAudioCompressor", ActiveXInvokeKind.PropertyGet);
			}
			return ocx.UseAudioCompressor;
		}
		set
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("UseAudioCompressor", ActiveXInvokeKind.PropertySet);
			}
			ocx.UseAudioCompressor = value;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[DispId(32)]
	[Browsable(false)]
	public virtual AudioCompressors AudioCompressors
	{
		get
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("AudioCompressors", ActiveXInvokeKind.PropertyGet);
			}
			return ocx.AudioCompressors;
		}
		set
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("AudioCompressors", ActiveXInvokeKind.PropertySet);
			}
			ocx.AudioCompressors = value;
		}
	}

	[DispId(33)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public virtual bool ShowPreview
	{
		get
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("ShowPreview", ActiveXInvokeKind.PropertyGet);
			}
			return ocx.ShowPreview;
		}
		set
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("ShowPreview", ActiveXInvokeKind.PropertySet);
			}
			ocx.ShowPreview = value;
		}
	}

	[DispId(34)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public virtual bool CaptureVideo
	{
		get
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("CaptureVideo", ActiveXInvokeKind.PropertyGet);
			}
			return ocx.CaptureVideo;
		}
		set
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("CaptureVideo", ActiveXInvokeKind.PropertySet);
			}
			ocx.CaptureVideo = value;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[DispId(35)]
	public virtual bool AspectRatio
	{
		get
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("AspectRatio", ActiveXInvokeKind.PropertyGet);
			}
			return ocx.AspectRatio;
		}
		set
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("AspectRatio", ActiveXInvokeKind.PropertySet);
			}
			ocx.AspectRatio = value;
		}
	}

	[ComAliasName("System.UInt32")]
	[DispId(36)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public virtual Color BackgroundColor
	{
		get
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("BackgroundColor", ActiveXInvokeKind.PropertyGet);
			}
			return AxHost.GetColorFromOleColor(ocx.BackgroundColor);
		}
		set
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("BackgroundColor", ActiveXInvokeKind.PropertySet);
			}
			ocx.BackgroundColor = AxHost.GetOleColorFromColor(value);
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	[DispId(37)]
	public virtual TVChannels TVChannels
	{
		get
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("TVChannels", ActiveXInvokeKind.PropertyGet);
			}
			return ocx.TVChannels;
		}
		set
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("TVChannels", ActiveXInvokeKind.PropertySet);
			}
			ocx.TVChannels = value;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[DispId(38)]
	public virtual short CountryCode
	{
		get
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("CountryCode", ActiveXInvokeKind.PropertyGet);
			}
			return ocx.CountryCode;
		}
		set
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("CountryCode", ActiveXInvokeKind.PropertySet);
			}
			ocx.CountryCode = value;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[DispId(39)]
	public virtual short Channel
	{
		get
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("Channel", ActiveXInvokeKind.PropertyGet);
			}
			return ocx.Channel;
		}
		set
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("Channel", ActiveXInvokeKind.PropertySet);
			}
			ocx.Channel = value;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[DispId(40)]
	public virtual bool TVMute
	{
		get
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("TVMute", ActiveXInvokeKind.PropertyGet);
			}
			return ocx.TVMute;
		}
		set
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("TVMute", ActiveXInvokeKind.PropertySet);
			}
			ocx.TVMute = value;
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[DispId(41)]
	public virtual VideoInputs VideoInputs
	{
		get
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("VideoInputs", ActiveXInvokeKind.PropertyGet);
			}
			return ocx.VideoInputs;
		}
		set
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("VideoInputs", ActiveXInvokeKind.PropertySet);
			}
			ocx.VideoInputs = value;
		}
	}

	[DispId(42)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public virtual short VideoInput
	{
		get
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("VideoInput", ActiveXInvokeKind.PropertyGet);
			}
			return ocx.VideoInput;
		}
		set
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("VideoInput", ActiveXInvokeKind.PropertySet);
			}
			ocx.VideoInput = value;
		}
	}

	[DispId(43)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public virtual WMVProfiles WMVProfiles
	{
		get
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("WMVProfiles", ActiveXInvokeKind.PropertyGet);
			}
			return ocx.WMVProfiles;
		}
		set
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("WMVProfiles", ActiveXInvokeKind.PropertySet);
			}
			ocx.WMVProfiles = value;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[DispId(44)]
	public virtual short WMVProfile
	{
		get
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("WMVProfile", ActiveXInvokeKind.PropertyGet);
			}
			return ocx.WMVProfile;
		}
		set
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("WMVProfile", ActiveXInvokeKind.PropertySet);
			}
			ocx.WMVProfile = value;
		}
	}

	[DispId(45)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public virtual bool OutputWMV
	{
		get
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("OutputWMV", ActiveXInvokeKind.PropertyGet);
			}
			return ocx.OutputWMV;
		}
		set
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("OutputWMV", ActiveXInvokeKind.PropertySet);
			}
			ocx.OutputWMV = value;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[DispId(46)]
	public virtual short VideoStandard
	{
		get
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("VideoStandard", ActiveXInvokeKind.PropertyGet);
			}
			return ocx.VideoStandard;
		}
		set
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("VideoStandard", ActiveXInvokeKind.PropertySet);
			}
			ocx.VideoStandard = value;
		}
	}

	[DispId(47)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public virtual short FrameRate
	{
		get
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("FrameRate", ActiveXInvokeKind.PropertyGet);
			}
			return ocx.FrameRate;
		}
		set
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("FrameRate", ActiveXInvokeKind.PropertySet);
			}
			ocx.FrameRate = value;
		}
	}

	[DispId(48)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public virtual string WMVCustomFileName
	{
		get
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("WMVCustomFileName", ActiveXInvokeKind.PropertyGet);
			}
			return ocx.WMVCustomFileName;
		}
		set
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("WMVCustomFileName", ActiveXInvokeKind.PropertySet);
			}
			ocx.WMVCustomFileName = value;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[DispId(49)]
	public virtual string LicenseKey
	{
		get
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("LicenseKey", ActiveXInvokeKind.PropertyGet);
			}
			return ocx.LicenseKey;
		}
		set
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("LicenseKey", ActiveXInvokeKind.PropertySet);
			}
			ocx.LicenseKey = value;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[ComAliasName("System.UInt32")]
	[DispId(50)]
	public virtual Color ControlBackColor
	{
		get
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("ControlBackColor", ActiveXInvokeKind.PropertyGet);
			}
			return AxHost.GetColorFromOleColor(ocx.ControlBackColor);
		}
		set
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("ControlBackColor", ActiveXInvokeKind.PropertySet);
			}
			ocx.ControlBackColor = AxHost.GetOleColorFromColor(value);
		}
	}

	[DispId(51)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public virtual short AudioInputPin
	{
		get
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("AudioInputPin", ActiveXInvokeKind.PropertyGet);
			}
			return ocx.AudioInputPin;
		}
		set
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("AudioInputPin", ActiveXInvokeKind.PropertySet);
			}
			ocx.AudioInputPin = value;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	[DispId(52)]
	public virtual AudioInputPins AudioInputPins
	{
		get
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("AudioInputPins", ActiveXInvokeKind.PropertyGet);
			}
			return ocx.AudioInputPins;
		}
		set
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("AudioInputPins", ActiveXInvokeKind.PropertySet);
			}
			ocx.AudioInputPins = value;
		}
	}

	public event EventHandler ClickEvent;

	public event _DVideoCapEvents_MouseMoveEventHandler MouseMoveEvent;

	public event EventHandler DblClick;

	public event _DVideoCapEvents_CapturingEventHandler Capturing;

	public AxVideoCap()
		: base("aa408ecc-e50b-401f-8036-021e81b9e5a1")
	{
		SetAboutBoxDelegate(AboutBox);
	}

	public virtual short Start()
	{
		if (ocx == null)
		{
			throw new InvalidActiveXStateException("Start", ActiveXInvokeKind.MethodInvoke);
		}
		return ocx.Start();
	}

	public virtual void Stop()
	{
		if (ocx == null)
		{
			throw new InvalidActiveXStateException("Stop", ActiveXInvokeKind.MethodInvoke);
		}
		ocx.Stop();
	}

	public virtual bool SnapShot(string strFileName)
	{
		if (ocx == null)
		{
			throw new InvalidActiveXStateException("SnapShot", ActiveXInvokeKind.MethodInvoke);
		}
		return ocx.SnapShot(strFileName);
	}

	public virtual bool ShowVideoCapturePropertyPage()
	{
		if (ocx == null)
		{
			throw new InvalidActiveXStateException("ShowVideoCapturePropertyPage", ActiveXInvokeKind.MethodInvoke);
		}
		return ocx.ShowVideoCapturePropertyPage();
	}

	public virtual bool ScanTVChannels()
	{
		if (ocx == null)
		{
			throw new InvalidActiveXStateException("ScanTVChannels", ActiveXInvokeKind.MethodInvoke);
		}
		return ocx.ScanTVChannels();
	}

	public virtual bool ShowAudioCapturePropertyPage()
	{
		if (ocx == null)
		{
			throw new InvalidActiveXStateException("ShowAudioCapturePropertyPage", ActiveXInvokeKind.MethodInvoke);
		}
		return ocx.ShowAudioCapturePropertyPage();
	}

	public virtual bool ShowCrossbarPropertyPage()
	{
		if (ocx == null)
		{
			throw new InvalidActiveXStateException("ShowCrossbarPropertyPage", ActiveXInvokeKind.MethodInvoke);
		}
		return ocx.ShowCrossbarPropertyPage();
	}

	public virtual bool ShowVideoFormatPropertyPage()
	{
		if (ocx == null)
		{
			throw new InvalidActiveXStateException("ShowVideoFormatPropertyPage", ActiveXInvokeKind.MethodInvoke);
		}
		return ocx.ShowVideoFormatPropertyPage();
	}

	public virtual bool SnapShotJPEG(string strFileName, short iJPEGQuality)
	{
		if (ocx == null)
		{
			throw new InvalidActiveXStateException("SnapShotJPEG", ActiveXInvokeKind.MethodInvoke);
		}
		return ocx.SnapShotJPEG(strFileName, iJPEGQuality);
	}

	public virtual void Pause()
	{
		if (ocx == null)
		{
			throw new InvalidActiveXStateException("Pause", ActiveXInvokeKind.MethodInvoke);
		}
		ocx.Pause();
	}

	public virtual Image SnapShot2Picture()
	{
		if (ocx == null)
		{
			throw new InvalidActiveXStateException("SnapShot2Picture", ActiveXInvokeKind.MethodInvoke);
		}
		StdPicture picture = ocx.SnapShot2Picture();
		return AxHost.GetPictureFromIPicture(picture);
	}

	public virtual int SnapShot2HBITMAP()
	{
		if (ocx == null)
		{
			throw new InvalidActiveXStateException("SnapShot2HBITMAP", ActiveXInvokeKind.MethodInvoke);
		}
		return ocx.SnapShot2HBITMAP();
	}

	public virtual void Resume()
	{
		if (ocx == null)
		{
			throw new InvalidActiveXStateException("Resume", ActiveXInvokeKind.MethodInvoke);
		}
		ocx.Resume();
	}

	public virtual void ResizeControl(short iwidth, short iheight)
	{
		if (ocx == null)
		{
			throw new InvalidActiveXStateException("ResizeControl", ActiveXInvokeKind.MethodInvoke);
		}
		ocx.ResizeControl(iwidth, iheight);
	}

	public virtual void RefreshDevice()
	{
		if (ocx == null)
		{
			throw new InvalidActiveXStateException("RefreshDevice", ActiveXInvokeKind.MethodInvoke);
		}
		ocx.RefreshDevice();
	}

	public virtual void AboutBox()
	{
		if (ocx == null)
		{
			throw new InvalidActiveXStateException("AboutBox", ActiveXInvokeKind.MethodInvoke);
		}
		ocx.AboutBox();
	}

	protected override void CreateSink()
	{
		try
		{
			eventMulticaster = new AxVideoCapEventMulticaster(this);
			cookie = new ConnectionPointCookie(ocx, eventMulticaster, typeof(_DVideoCapEvents));
		}
		catch (Exception)
		{
		}
	}

	protected override void DetachSink()
	{
		try
		{
			cookie.Disconnect();
		}
		catch (Exception)
		{
		}
	}

	protected override void AttachInterfaces()
	{
		try
		{
			ocx = (_DVideoCap)GetOcx();
		}
		catch (Exception)
		{
		}
	}

	internal void RaiseOnClickEvent(object sender, EventArgs e)
	{
		if (ClickEvent != null)
		{
			ClickEvent(sender, e);
		}
	}

	internal void RaiseOnMouseMoveEvent(object sender, _DVideoCapEvents_MouseMoveEvent e)
	{
		if (MouseMoveEvent != null)
		{
			MouseMoveEvent(sender, e);
		}
	}

	internal void RaiseOnDblClick(object sender, EventArgs e)
	{
		if (DblClick != null)
		{
			DblClick(sender, e);
		}
	}

	internal void RaiseOnCapturing(object sender, _DVideoCapEvents_CapturingEvent e)
	{
		if (Capturing != null)
		{
			Capturing(sender, e);
		}
	}
}

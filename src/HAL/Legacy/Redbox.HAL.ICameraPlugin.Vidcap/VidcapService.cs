using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Redbox.HAL.Core;

namespace Redbox.HAL.ICameraPlugin.Vidcap;

internal class VidcapService : ICameraPlugin
{
	private Vidcapform m_vidcapForm;

	private int m_spinTime = 300;

	private ManualResetEvent m_waiter = new ManualResetEvent(initialState: false);

	public static VidcapService Instance => Singleton<VidcapService>.Instance;

	public bool IsRunning { get; private set; }

	public bool SupportsReset => false;

	private string ImagesDirectory { get; set; }

	public bool Start()
	{
		if (IsRunning)
		{
			if (LogHelper.Instance.IsLevelEnabled((LogEntryType)1))
			{
				LogHelper.Instance.Log("Camera is running - don't need to start.");
			}
			return true;
		}
		try
		{
			m_vidcapForm = new Vidcapform();
			m_vidcapForm.Visible = false;
			m_vidcapForm.Videocap.LicenseKey = "1121 special developer license";
			short num = m_vidcapForm.Videocap.Start();
			IsRunning = num == 0;
			if (!IsRunning)
			{
				LogHelper.Instance.Log($"Videocap.Start() returned {num}.", (LogEntryType)2);
			}
			return IsRunning;
		}
		catch (Exception ex)
		{
			LogHelper.Instance.Log("VidcapService.StartCameraInner() caught an exception.", ex);
			return false;
		}
	}

	public string Snap()
	{
		if (!IsRunning)
		{
			LogHelper.Instance.Log("VideocapService.Snap(): camera isn't running.", (LogEntryType)2);
			return null;
		}
		DateTime now = DateTime.Now;
		string path = $"m{now.Month}d{now.Day}y{now.Year}_h{now.Hour}m{now.Minute}s{now.Second}t{now.Millisecond}.jpg";
		string text = Path.Combine(ImagesDirectory, path);
		try
		{
			bool flag = m_vidcapForm.Videocap.SnapShotJPEG(text, 100);
			if (!flag)
			{
				LogHelper.Instance.Log($"Snapshot returned {flag} for {text}", (LogEntryType)2);
				return null;
			}
			return text;
		}
		catch (Exception ex)
		{
			LogHelper.Instance.Log("VideocapService.SnapInner() caught an exception.", ex);
			return null;
		}
	}

	public bool Stop()
	{
		if (!IsRunning)
		{
			LogHelper.Instance.Log("VidcapService.Stop(): camera isn't running - bypass stop.");
			return false;
		}
		return StopInner();
	}

	public void TurnOff()
	{
		StopInner();
		m_vidcapForm.Dispose();
		m_vidcapForm = null;
	}

	public void InitWithProperties(IDictionary<string, object> props)
	{
		if (props.ContainsKey("WorkingPath"))
		{
			ImagesDirectory = props["WorkingPath"] as string;
		}
		if (props.ContainsKey("VideocapActiveXSpinTime"))
		{
			m_spinTime = (int)props["VideocapActiveXSpinTime"];
		}
	}

	private VidcapService()
	{
	}

	private bool StopInner()
	{
		try
		{
			m_vidcapForm.Videocap.Stop();
			IsRunning = false;
			return true;
		}
		catch (Exception ex)
		{
			LogHelper.Instance.Log("VidcapService.StopInner() caught an exception", ex);
			return false;
		}
	}
}

using System;
using Redbox.HAL.Client;
using Redbox.HAL.Core;
using Redbox.HAL.Management.Console.Properties;
using Redbox.IPC.Framework;

namespace Redbox.HAL.Management.Console;

public class ProfileManager
{
	private string DefaultProtocol = Settings.Default.DefaultConnectionURL;

	private ClientHelper Helper;

	private HardwareService m_service;

	public static ProfileManager Instance => Singleton<ProfileManager>.Instance;

	public bool IsConnected { get; private set; }

	public HardwareService Service
	{
		get
		{
			if (m_service != null)
			{
				return m_service;
			}
			OutputWindow.Instance.Append("The service isn't started");
			return null;
		}
		private set
		{
			m_service = value;
		}
	}

	public event EventHandler Connected;

	public event EventHandler Disconnected;

	public void OnConnect(object sender, EventArgs e)
	{
		Connect();
	}

	public void OnDisconnect(object sender, EventArgs e)
	{
		Disconnect();
	}

	public bool Connect()
	{
		if (Service == null)
		{
			IPCProtocol protocol = IPCProtocol.Parse(DefaultProtocol);
			Service = new HardwareService(protocol);
			Helper = new ClientHelper(Service);
		}
		if (!Helper.TestCommunication())
		{
			return false;
		}
		IsConnected = true;
		if (Connected != null)
		{
			Connected(this, EventArgs.Empty);
		}
		return true;
	}

	public void Disconnect()
	{
		IsConnected = false;
		JobHelper.DisconnectJobs();
		Service = null;
		if (Disconnected != null)
		{
			Disconnected(this, EventArgs.Empty);
		}
	}

	public bool GetJob(string jobID, out HardwareJob job)
	{
		job = null;
		if (!IsConnected)
		{
			return false;
		}
		if (string.IsNullOrEmpty(jobID))
		{
			return false;
		}
		HardwareCommandResult job2 = Instance.Service.GetJob(jobID, out job);
		if (!job2.Success)
		{
			OutputWindow.Instance.Append($"Failed to get job {jobID}");
			CommonFunctions.ProcessCommandResult(job2);
			return false;
		}
		return true;
	}

	private ProfileManager()
	{
	}
}

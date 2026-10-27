using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.Environment;

public class CertificateService
{
	private Timer _timer;

	private bool _isRunning;

	private int _inDoWork;

	private string _certDataPath = "C:\\Program Files\\Redbox\\reds\\Kiosk Engine\\data\\certs";

	public static CertificateService Instance => Singleton<CertificateService>.Instance;

	public ErrorList Initialize()
	{
		ErrorList errorList = new ErrorList();
		try
		{
			_timer = new Timer(delegate
			{
				DoWork();
			});
			_isRunning = false;
			ServiceLocator.Instance.AddService(typeof(CertificateService), this);
			LogHelper.Instance.Log("Initialized the CertificateService", LogEntryType.Info);
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("CertificateService", "Unhandled exception occurred in Initialize.", e));
		}
		return errorList;
	}

	public ErrorList Start()
	{
		ErrorList errorList = new ErrorList();
		try
		{
			_timer.Change(new TimeSpan(0, 2, 0), new TimeSpan(0, 10, 0));
			_isRunning = true;
			LogHelper.Instance.Log("Starting the CertificateService.", LogEntryType.Info);
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("CertificateService", "An unhandled exception occurred.", e));
		}
		return errorList;
	}

	public ErrorList Stop()
	{
		ErrorList errorList = new ErrorList();
		try
		{
			_timer.Change(-1, -1);
			_isRunning = false;
			LogHelper.Instance.Log("Stopping the CertificateService.", LogEntryType.Info);
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("CertificateService", "An unhandled exception occurred while stopping the CertificateService.", e));
		}
		return errorList;
	}

	private void DoWork()
	{
		try
		{
			if (!_isRunning)
			{
				LogHelper.Instance.Log("The CertificateService service is not running.", LogEntryType.Info);
				return;
			}
			if (Interlocked.CompareExchange(ref _inDoWork, 1, 0) == 1)
			{
				LogHelper.Instance.Log("Already in CertificateService.DoWork", LogEntryType.Info);
				return;
			}
			try
			{
				LogHelper.Instance.Log("Executing CertificateService.DoWork");
				LookForCerts();
			}
			finally
			{
				_inDoWork = 0;
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("There was an exception in CertificateService.DoWork()", e);
		}
	}

	private void LookForCerts()
	{
		if (!Directory.Exists(_certDataPath))
		{
			Directory.CreateDirectory(_certDataPath);
			return;
		}
		string[] files = Directory.GetFiles(_certDataPath, "*.staged");
		foreach (string text in files)
		{
			try
			{
				Path.GetFileNameWithoutExtension(text);
				byte[] data = File.ReadAllBytes(text);
				if (!CertificateHelper.Exists(StoreName.My, StoreLocation.LocalMachine, data))
				{
					LogHelper.Instance.Log("CertificateService.LookForCerts - Adding Cert Name: {0} to the capi store", text);
					CertificateHelper.Add(StoreName.My, StoreLocation.LocalMachine, data);
					LogHelper.Instance.Log("CertificateService.LookForCerts - Added Cert Name: {0} to the capi store", text);
				}
				else
				{
					LogHelper.Instance.Log("CertificateService.LookForCerts - Cert Name: {0} already exists in capi store", text);
				}
				string text2 = text.Replace(".staged", "");
				if (File.Exists(text2))
				{
					File.Delete(text2);
				}
				File.Move(text, text2);
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("There was an exception in CertificateService.LookForCerts()", e);
			}
		}
	}

	private CertificateService()
	{
	}
}

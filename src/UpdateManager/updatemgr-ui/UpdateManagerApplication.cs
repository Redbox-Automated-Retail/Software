using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Redbox.Core;
using Redbox.IPC.Framework;
using Redbox.UpdateManager.ComponentModel;
using Redbox.UpdateManager.ServiceProxies;
using Redbox.UpdateManager.UI.Properties;

namespace Redbox.UpdateManager.UI;

public class UpdateManagerApplication : ApplicationContext
{
	public static UpdateManagerApplication Instance => Singleton<UpdateManagerApplication>.Instance;

	public bool DeveloperMode { get; private set; }

	public bool InitialSubscriptionState { get; private set; }

	public string UpdateManagerServiceUrl { get; private set; }

	public bool Run()
	{
		Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
		MainForm mainForm = new MainForm();
		base.MainForm = mainForm;
		if (!errorList.ContainsError())
		{
			base.MainForm.Show();
			return true;
		}
		ErrorForm errorForm = new ErrorForm();
		errorForm.Errors = errorList;
		errorForm.ShowDialog();
		return false;
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList Initialize()
	{
		Redbox.UpdateManager.ComponentModel.ErrorList errorlist = new Redbox.UpdateManager.ComponentModel.ErrorList();
		Path.GetDirectoryName(typeof(UpdateManagerApplication).Assembly.Location);
		UpdateManagerServiceUrl = Settings.Default.UpdateManagerUrl;
		SchedulerProxy.Instance.Initialize(UpdateManagerServiceUrl);
		QueueServiceProxy.Instance.Initialize(UpdateManagerServiceUrl);
		TransferServiceProxy.Instance.Initialize(UpdateManagerServiceUrl);
		RepositoryServiceProxy.Instance.Initialize(UpdateManagerServiceUrl);
		DataStoreServiceProxy.Instance.Initialize(UpdateManagerServiceUrl);
		bool isDeveloperUI;
		bool initialSubscriptionState;
		using (UpdateManagerService updateManagerService = UpdateManagerService.GetService(UpdateManagerServiceUrl))
		{
			ClientCommandResult storeNumber = updateManagerService.GetStoreNumber(out var _);
			if (!storeNumber.Success)
			{
				storeNumber.Errors.ForEach(delegate(Redbox.IPC.Framework.Error e)
				{
					errorlist.Add(e.IsWarning ? Redbox.UpdateManager.ComponentModel.Error.NewWarning(e.Code, e.Description, e.Details) : Redbox.UpdateManager.ComponentModel.Error.NewError(e.Code, e.Description, e.Details));
				});
				return errorlist;
			}
			storeNumber = updateManagerService.IsDeveloperUI(out isDeveloperUI);
			if (!storeNumber.Success)
			{
				storeNumber.Errors.ForEach(delegate(Redbox.IPC.Framework.Error e)
				{
					errorlist.Add(e.IsWarning ? Redbox.UpdateManager.ComponentModel.Error.NewWarning(e.Code, e.Description, e.Details) : Redbox.UpdateManager.ComponentModel.Error.NewError(e.Code, e.Description, e.Details));
				});
				return errorlist;
			}
			storeNumber = updateManagerService.GetInitialSubscriptionState(out initialSubscriptionState);
			if (!storeNumber.Success)
			{
				storeNumber.Errors.ForEach(delegate(Redbox.IPC.Framework.Error e)
				{
					errorlist.Add(e.IsWarning ? Redbox.UpdateManager.ComponentModel.Error.NewWarning(e.Code, e.Description, e.Details) : Redbox.UpdateManager.ComponentModel.Error.NewError(e.Code, e.Description, e.Details));
				});
				return errorlist;
			}
		}
		DeveloperMode = isDeveloperUI;
		InitialSubscriptionState = initialSubscriptionState;
		UpdateServiceProxy.Instance.Initialize(UpdateManagerServiceUrl, TimeSpan.FromSeconds(120.0));
		return errorlist;
	}

	public void ThreadSafeHostUpdate(MethodInvoker invoker)
	{
		if (base.MainForm.InvokeRequired)
		{
			base.MainForm.Invoke(invoker);
		}
		else
		{
			invoker();
		}
	}

	public void AsyncButtonAction(Control button, Action action)
	{
		if (button != null)
		{
			button.Enabled = false;
		}
		ThreadPool.QueueUserWorkItem(delegate
		{
			Cursor cursor = base.MainForm.Cursor;
			try
			{
				if (button != null)
				{
					ThreadSafeHostUpdate(delegate
					{
						base.MainForm.Cursor = Cursors.WaitCursor;
					});
				}
				ThreadSafeHostUpdate(delegate
				{
					action();
				});
			}
			catch (Exception exception)
			{
				ThreadSafeExceptionDisplay(exception);
			}
			finally
			{
				if (button != null)
				{
					Instance.ThreadSafeHostUpdate(delegate
					{
						button.Enabled = true;
						base.MainForm.Cursor = cursor;
					});
				}
			}
		});
	}

	public void ThreadSafeErrorDisplay(Redbox.UpdateManager.ComponentModel.ErrorList errors)
	{
		Instance.ThreadSafeHostUpdate(delegate
		{
			ErrorForm errorForm = new ErrorForm();
			errorForm.Errors = errors;
			errorForm.ShowDialog();
		});
	}

	public void ThreadSafeExceptionDisplay(Exception exception)
	{
		Instance.ThreadSafeHostUpdate(delegate
		{
			Redbox.UpdateManager.ComponentModel.ErrorList errors = new Redbox.UpdateManager.ComponentModel.ErrorList { Redbox.UpdateManager.ComponentModel.Error.NewError("E988", "An unhandled exception was thrown in Update Mananger.", exception) };
			ErrorForm errorForm = new ErrorForm();
			errorForm.Errors = errors;
			errorForm.ShowDialog();
		});
	}

	private UpdateManagerApplication()
	{
		Application.ThreadException += delegate(object sender, ThreadExceptionEventArgs e)
		{
			LogHelper.Instance.Log("An unhandled exception was rasied in the Application.ThreadException handler.", e.Exception);
			MessageBox.Show(e.Exception.ToString());
		};
	}
}

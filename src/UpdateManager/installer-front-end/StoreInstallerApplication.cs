using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Redbox.Core;
using Redbox.UpdateManager.BITS;
using Redbox.UpdateManager.ComponentModel;
using Redbox.UpdateManager.Environment;
using Redbox.UpdateManager.Kernel;
using Redbox.UpdateManager.Remoting;
using Redbox.UpdateManager.StoreInstallerFrontEnd.Properties;
using Redbox.UpdateManager.TaskScheduler;

namespace Redbox.UpdateManager.StoreInstallerFrontEnd;

public class StoreInstallerApplication : ApplicationContext, IEventNotifyService, IInputService
{
	private Thread? m_scriptThread;

	public static StoreInstallerApplication Instance => Singleton<StoreInstallerApplication>.Instance;

	public bool ScriptCompleted => KernelService.Instance.ScriptCompleted;

	public string Script { get; set; } = null!; // set by Initialize

	public DialogResult AskYesNoQuestion(string title, string message)
	{
		DialogResult result = DialogResult.None;
		ThreadSafeHostUpdate(delegate
		{
			result = MessageBox.Show(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button2);
		});
		return result;
	}

	public void NotifyInfo(string title, string message)
	{
		Font font = new Font(FontFamily.GenericSansSerif, 12f, FontStyle.Bold);
		ThreadSafeHostUpdate(delegate
		{
			CustomMessageBox.Show(title, message, font);
		});
	}

	public FileInfo LocateFile()
	{
		FileInfo? file = null;
		ThreadSafeHostUpdate(delegate
		{
			file = ((Host)base.MainForm!).ShowFileDialog();
		});
		return file!; // null when the dialog is cancelled, but IInputService.LocateFile is declared non-null
	}

	public ErrorList AddEvent(string name, string description)
	{
		try
		{
			ThreadSafeHostUpdate(delegate
			{
				((Host)base.MainForm!).AddEvent(name, description);
			});
		}
		catch (Exception e)
		{
			return new ErrorList { Redbox.UpdateManager.ComponentModel.Error.NewError("E999", "Unhandled exception in AddEvent.", e) };
		}
		return new ErrorList();
	}

	public ErrorList EventStart(string name)
	{
		try
		{
			ThreadSafeHostUpdate(delegate
			{
				((Host)base.MainForm!).EventStart(name);
			});
		}
		catch (Exception e)
		{
			return new ErrorList { Redbox.UpdateManager.ComponentModel.Error.NewError("E999", "Unhandled exception in AddEvent.", e) };
		}
		return new ErrorList();
	}

	public ErrorList EventErrored(string name, string code, string description, string details)
	{
		try
		{
			ThreadSafeHostUpdate(delegate
			{
				((Host)base.MainForm!).EventError(name, Redbox.UpdateManager.ComponentModel.Error.NewError(code, description, details));
			});
		}
		catch (Exception e)
		{
			return new ErrorList { Redbox.UpdateManager.ComponentModel.Error.NewError("E999", "Unhandled exception in AddEvent.", e) };
		}
		return new ErrorList();
	}

	public ErrorList EventComplete(string name)
	{
		try
		{
			ThreadSafeHostUpdate(delegate
			{
				((Host)base.MainForm!).EventComplete(name);
			});
		}
		catch (Exception e)
		{
			return new ErrorList { Redbox.UpdateManager.ComponentModel.Error.NewError("E999", "Unhandled exception in AddEvent.", e) };
		}
		return new ErrorList();
	}

	public ErrorList Exit()
	{
		try
		{
			ThreadSafeHostUpdate(delegate
			{
				((Host)base.MainForm!).ShowFinish();
			});
		}
		catch (Exception e)
		{
			return new ErrorList { Redbox.UpdateManager.ComponentModel.Error.NewError("E999", "Unhandled exception in AddEvent.", e) };
		}
		return new ErrorList();
	}

	public bool Run()
	{
		ErrorList errorList = new ErrorList();
		Host host = new Host();
		base.MainForm = host;
		m_scriptThread = new Thread((ParameterizedThreadStart)delegate
		{
			try
			{
				string chunk = File.ReadAllText(Script);
				KernelService.Instance.ExecuteChunk(chunk);
			}
			catch (Exception ex)
			{
				LogHelper.Instance.Log("Unhandled exception caught in execution of script.", ex);
				ThreadSafeExceptionDisplay(ex);
				ThreadSafeHostUpdate(delegate
				{
					((Host)base.MainForm!).ErrorAll();
				});
			}
		})
		{
			IsBackground = true
		};
		if (!errorList.ContainsError())
		{
			base.MainForm.Show();
			m_scriptThread.Start();
			return true;
		}
		ErrorForm errorForm = new ErrorForm();
		errorForm.Errors = errorList;
		errorForm.ShowDialog();
		return false;
	}

	public ErrorList Initialize(string? script, string? storeNumber)
	{
		MacroService macroService = new MacroService();
		ServiceLocator.Instance.AddService(typeof(IMacroService), macroService);
		string directoryName = Path.GetDirectoryName(typeof(StoreInstallerApplication).Assembly.Location)!;
		ErrorList errorList = new ErrorList();
		string expanded = macroService.ExpandProperties(script);
		if (!Path.IsPathRooted(expanded))
		{
			expanded = Path.Combine(directoryName, expanded);
		}
		if (!File.Exists(expanded))
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("F999", $"Script file at {script} not found.", "Supply a valid script file."));
			return errorList;
		}
		Script = expanded;
		macroService["StoreNumber"] = storeNumber;
		macroService["ProgramFiles"] = "C:\\Program Files";
		macroService["REDS"] = "C:\\Program Files\\Redbox\\REDS";
		macroService["data"] = "C:\\Program Files\\Redbox\\REDS\\data";
		macroService["RedboxProgramFiles"] = "C:\\Program Files\\Redbox";
		macroService["engine"] = "C:\\Program Files\\Redbox\\REDS\\Kiosk Engine";
		macroService["Scripts"] = "C:\\Program Files\\Redbox\\REDS\\Update Manager\\scripts";
		macroService["WorkingDirectory"] = Path.GetDirectoryName(typeof(StoreInstallerApplication).Assembly.Location);
		WindowsTaskScheduler.Instance.Initialize();
		Redbox.UpdateManager.Remoting.UpdateService.Instance.Initialize(Settings.Default.UpdateServiceUrl, TimeSpan.FromSeconds(120.0), storeNumber!, Settings.Default.MinimumRetryDelay);
		KernelService.Instance.Initialize();
		DataStoreService.Instance.Initialize(Settings.Default.DataStoreRoot);
		Scheduler.Instance.Initialize();
		QueueService.Instance.Initialize(Settings.Default.DataStoreRoot);
		TransferService.Instance.Initialize();
		RepositoryService.Instance.Initialize(Settings.Default.ManifestRoot);
		ServiceLocator.Instance.AddService(typeof(IEventNotifyService), this);
		ServiceLocator.Instance.AddService(typeof(IInputService), this);
		return errorList;
	}

	public void ThreadSafeHostUpdate(MethodInvoker invoker)
	{
		if (base.MainForm!.InvokeRequired)
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
			try
			{
				action();
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
					});
				}
			}
		});
	}

	public void ThreadSafeErrorDisplay(ErrorList errors)
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
			ErrorList errors = new ErrorList { Redbox.UpdateManager.ComponentModel.Error.NewError("E999", "An unhandled exception was thrown in Update Mananger.", exception) };
			ErrorForm errorForm = new ErrorForm();
			errorForm.Errors = errors;
			errorForm.ShowDialog();
		});
	}

	private StoreInstallerApplication()
	{
		Application.ThreadException += delegate(object sender, ThreadExceptionEventArgs e)
		{
			LogHelper.Instance.Log("An unhandled exception was rasied in the Application.ThreadException handler.", e.Exception);
			MessageBox.Show(e.Exception.ToString());
		};
	}
}

using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Environment;

public class DebugService : IDebugService
{
	private bool m_isApplicationRunning;

	public static DebugService Instance => Singleton<DebugService>.Instance;

	public bool IsApplicationRunning
	{
		get
		{
			return m_isApplicationRunning;
		}
		set
		{
			m_isApplicationRunning = value;
		}
	}

	public object DebuggerInstance { get; private set; }

	public bool IsEnabled
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>()?.GetValue("Core", "DebuggerEnabled", defaultValue: false) ?? false;
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>()?.SetValue("Core", "DebuggerEnabled", value);
		}
	}

	public ErrorList Initialize()
	{
		ErrorList result = new ErrorList();
		ServiceLocator.Instance.AddService(typeof(IDebugService), Instance);
		return result;
	}

	public void ActivateDebugger()
	{
		if (IsEnabled)
		{
			ServiceLocator.Instance.GetService<IResourceBundleService>();
			ServiceLocator.Instance.GetService<IIdeService>().ActivateDebugger(null, 1, null);
		}
	}

	public void ActivateDebugger(string resourceName)
	{
		if (IsEnabled)
		{
			ServiceLocator.Instance.GetService<IResourceBundleService>();
			ServiceLocator.Instance.GetService<IIdeService>().ActivateDebugger(resourceName, 1, null);
		}
	}

	public void ActivateDebugger(string resourceName, int lineNumber, string error)
	{
		if (IsEnabled)
		{
			ServiceLocator.Instance.GetService<IResourceBundleService>();
			ServiceLocator.Instance.GetService<IIdeService>().ActivateDebugger(resourceName, lineNumber, error);
		}
	}

	public void SetDebuggerInstance(object instance)
	{
		IKernelService service = ServiceLocator.Instance.GetService<IKernelService>();
		DebuggerInstance = instance;
		service.SetDebuggingState(IsEnabled);
	}

	private DebugService()
	{
		ServiceLocator.Instance.GetService<IKernelService>().DebuggerStopping += delegate(string source, int lineNumber, string action)
		{
			if (lineNumber != -1 && action == "Run")
			{
				ActivateDebugger(source, lineNumber, "Breakpoint hit");
			}
		};
	}
}

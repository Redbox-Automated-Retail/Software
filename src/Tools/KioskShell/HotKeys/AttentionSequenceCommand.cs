using Outerwall.Shell.HotKeys;
using Redbox.Core;
using Redbox.Shell.ComponentModel;

namespace Redbox.Shell.HotKeys;

public class AttentionSequenceCommand : IHotKeyCommand
{
	public void Execute()
	{
		LogHelper.Instance.Log("AttentionSequenceCommand.Execute() - start");
		IKioskEngineService service = ServiceLocator.Instance.GetService<IKioskEngineService>();
		if (service == null)
		{
			return;
		}
		ErrorList errorList = service.IsEngineRunning(out var isRunning);
		if (errorList.ContainsError())
		{
			errorList.ForEach(delegate(Redbox.Shell.ComponentModel.Error e)
			{
				LogHelper.Instance.Log("{0} Details: {1}", e, e.Details);
			});
			return;
		}
		LogHelper.Instance.Log("AttentionSequenceCommand.Execute() - running: " + isRunning);
		errorList = (isRunning ? service.ActivateControlPanel() : service.StartEngineWithControlPanel());
		if (errorList.ContainsError())
		{
			errorList.ForEach(delegate(Redbox.Shell.ComponentModel.Error e)
			{
				LogHelper.Instance.Log("{0} Details: {1}", e, e.Details);
			});
		}
		else
		{
			LogHelper.Instance.Log("Launch control panel successful.");
		}
	}
}

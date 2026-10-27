using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.Kernel;

public static class DownloadFileFunctions
{
	[KernelFunction(Name = "kernel.markrebootrequired")]
	internal static void SetScriptComplete()
	{
		ServiceLocator.Instance.GetService<IDownloadFileService>()?.MarkRebootRequired();
	}
}

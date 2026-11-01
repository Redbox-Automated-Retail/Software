using System.IO;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework.Natives;

[NativeJob(ProgramName = "decode-image")]
internal sealed class DecodeImageJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		base.Context.ShouldCleanup = false;
		string path = base.Context.PopTop<string>();
		IBarcodeReaderFactory barcodeReaderFactory = ServiceLocator.Instance.GetService<IBarcodeReaderFactory>();
		string text = Path.Combine(barcodeReaderFactory.ImagePath, path);
		if (!File.Exists(text))
		{
			LogHelper.Instance.WithContext("The image '{0}' was not found", text);
			AddError("Image not found.");
		}
		else
		{
			new ScanResult(barcodeReaderFactory.GetConfiguredReader().Scan(text)).PushTo(base.Context);
		}
	}

	internal DecodeImageJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}

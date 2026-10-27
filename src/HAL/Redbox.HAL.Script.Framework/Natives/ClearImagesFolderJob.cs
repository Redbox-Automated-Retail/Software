using System;
using System.IO;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework.Natives;

[NativeJob(ProgramName = "clear-images-folder")]
internal sealed class ClearImagesFolderJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		string[] files = Directory.GetFiles(ServiceLocator.Instance.GetService<IBarcodeReaderFactory>().ImagePath);
		int errors = 0;
		Array.ForEach(files, delegate(string file)
		{
			try
			{
				File.Delete(file);
			}
			catch (Exception ex)
			{
				LogHelper.Instance.WithContext("Failed to delete file '{0}'", file);
				LogHelper.Instance.Log(ex.Message);
				int num = errors + 1;
				errors = num;
			}
		});
		if (errors > 0)
		{
			AddError("Failed to delete some images.");
		}
	}

	internal ClearImagesFolderJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}

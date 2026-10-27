using System;
using System.IO;
using System.Xml;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "reset-inventory-job", Operand = "RESET-INVENTORY-JOB")]
internal sealed class ResetInventoryJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		string text = base.Context.PopTop<string>();
		if (!File.Exists(text))
		{
			text = Path.Combine(ServiceLocator.Instance.GetService<IRuntimeService>().DataPath, text);
			if (!File.Exists(text))
			{
				AddError("The file {0} doesn't exist.");
				return;
			}
		}
		LogHelper.Instance.WithContext("Reset inventory from file '{0}'", text);
		try
		{
			string xml = File.ReadAllText(text);
			XmlDocument xmlDocument = new XmlDocument();
			try
			{
				xmlDocument.LoadXml(xml);
				if (xmlDocument.DocumentElement == null)
				{
					base.Result.Errors.Add(Error.NewError("I001", "Invalid document.", "The specified document is invalid."));
					return;
				}
			}
			catch (Exception e)
			{
				base.Result.Errors.Add(Error.NewError("I001", "Invalid document.", e));
				return;
			}
			ServiceLocator.Instance.GetService<IInventoryService>().ResetState(xmlDocument, base.Result.Errors);
			ServiceLocator.Instance.GetService<IDumpbinService>().ResetState(xmlDocument, base.Result.Errors);
			if (base.Result.Errors.Count > 0)
			{
				LogHelper.Instance.Log("RESET INVENTORY Failed to import inventory.", LogEntryType.Error);
			}
		}
		catch (Exception e2)
		{
			LogHelper.Instance.Log("Reset-inventory caught an exception.", e2);
			base.Result.Errors.Add(Error.NewError("I003", "Reset inventory failure.", e2));
		}
	}

	internal ResetInventoryJob(ExecutionResult r, ExecutionContext c)
		: base(r, c)
	{
	}
}

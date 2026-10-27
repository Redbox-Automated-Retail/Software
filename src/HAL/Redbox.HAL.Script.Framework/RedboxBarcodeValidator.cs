using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

internal sealed class RedboxBarcodeValidator : IBarcodeValidatorService
{
	public bool IsValid(string barcode)
	{
		if (!ControllerConfiguration.Instance.EnableUnrecognizedBarcodeReject)
		{
			return true;
		}
		bool flag = false;
		try
		{
			int.Parse(barcode);
			flag = true;
		}
		catch
		{
			flag = false;
		}
		if (flag)
		{
			return barcode.Length == 9;
		}
		return false;
	}

	internal RedboxBarcodeValidator()
	{
	}
}

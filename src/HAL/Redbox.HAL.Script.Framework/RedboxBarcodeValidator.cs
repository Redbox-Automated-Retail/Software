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
		bool isNumeric = false;
		try
		{
			int.Parse(barcode);
			isNumeric = true;
		}
		catch
		{
			isNumeric = false;
		}
		if (isNumeric)
		{
			return barcode.Length == 9;
		}
		return false;
	}

	internal RedboxBarcodeValidator()
	{
	}
}

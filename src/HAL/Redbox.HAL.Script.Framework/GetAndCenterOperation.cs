using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

internal class GetAndCenterOperation
{
	private readonly ExecutionContext Context;

	internal bool Run()
	{
		IControlSystem controlSystem = ServiceLocator.Instance.GetService<IControlSystem>();
		IPickerSensorReadResult pickerSensorReadResult = controlSystem.ReadPickerSensors();
		if (!pickerSensorReadResult.Success)
		{
			Context.CreateResult("ErrorMessage", $"Read picker sensors returned error {pickerSensorReadResult.Error}", null);
			return false;
		}
		if (pickerSensorReadResult.IsFull)
		{
			Context.CreateResult("ErrorMessage", "The picker is full.", null, null, null, null, null);
			return false;
		}
		int slot = Context.PopTop<int>();
		int deck = Context.PopTop<int>();
		bool center = Context.PopTop<bool>();
		ErrorCodes errorCodes = ServiceLocator.Instance.GetService<IMotionControlService>().MoveTo(deck, slot, MoveMode.Get, Context.AppLog);
		string message = $"MOVE DECK={deck} SLOT={slot} MODE=GET {errorCodes.ToString().ToUpper()}";
		Context.CreateResult((errorCodes == ErrorCodes.Success) ? "SuccessMessage" : "ErrorMessage", message, deck, slot, null, null, null);
		if (errorCodes != ErrorCodes.Success)
		{
			return false;
		}
		IGetResult getResult = ServiceLocator.Instance.GetService<IControllerService>().Get();
		if (!getResult.Success)
		{
			Context.CreateResult("ErrorMessage", $"GET returned error status {getResult.ToString()}", deck, slot, null, null, null);
			return false;
		}
		if (center)
		{
			controlSystem.Center(CenterDiskMethod.VendDoorAndBack);
		}
		Context.CreateInfoResult("SuccessMessage", "GET SUCCESS");
		return true;
	}

	internal GetAndCenterOperation(ExecutionContext context)
	{
		Context = context;
	}
}

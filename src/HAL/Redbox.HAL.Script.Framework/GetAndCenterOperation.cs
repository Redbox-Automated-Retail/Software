using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

internal class GetAndCenterOperation
{
	private readonly ExecutionContext Context;

	internal bool Run()
	{
		IControlSystem service = ServiceLocator.Instance.GetService<IControlSystem>();
		IPickerSensorReadResult pickerSensorReadResult = service.ReadPickerSensors();
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
		int num = Context.PopTop<int>();
		int num2 = Context.PopTop<int>();
		bool flag = Context.PopTop<bool>();
		ErrorCodes errorCodes = ServiceLocator.Instance.GetService<IMotionControlService>().MoveTo(num2, num, MoveMode.Get, Context.AppLog);
		string message = $"MOVE DECK={num2} SLOT={num} MODE=GET {errorCodes.ToString().ToUpper()}";
		Context.CreateResult((errorCodes == ErrorCodes.Success) ? "SuccessMessage" : "ErrorMessage", message, num2, num, null, null, null);
		if (errorCodes != ErrorCodes.Success)
		{
			return false;
		}
		IGetResult getResult = ServiceLocator.Instance.GetService<IControllerService>().Get();
		if (!getResult.Success)
		{
			Context.CreateResult("ErrorMessage", $"GET returned error status {getResult.ToString()}", num2, num, null, null, null);
			return false;
		}
		if (flag)
		{
			service.Center(CenterDiskMethod.VendDoorAndBack);
		}
		Context.CreateInfoResult("SuccessMessage", "GET SUCCESS");
		return true;
	}

	internal GetAndCenterOperation(ExecutionContext context)
	{
		Context = context;
	}
}

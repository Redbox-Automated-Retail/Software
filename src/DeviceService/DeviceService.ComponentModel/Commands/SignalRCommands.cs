namespace DeviceService.ComponentModel.Commands;

public class SignalRCommands
{
	public const string ExecuteCancelCommand = "ExecuteCancelCommand";

	public const string ExecuteDeviceServiceShutDownCommand = "ExecuteDeviceServiceShutDownCommand";

	public const string ExecuteDeviceServiceCanShutDownCommand = "ExecuteDeviceServiceCanShutDownCommand";

	public const string SendShutDownStartingEvent = "SendShutDownStartingEvent";

	public const string SetCommandQueueState = "SetCommandQueueState";

	public const string SendCardReaderStateEvent = "SendCardReaderStateEvent";

	public const string ExecuteCommand = "ExecuteCommand";

	public const string ExecuteBaseCommand = "ExecuteBaseCommand";

	public const string ExecuteReadCardCommand = "ExecuteReadCardCommand";

	public const string SendEvent = "SendEvent";

	public const string SendSimpleEvent = "SendSimpleEvent";

	public const string ExecuteReportAuthorizeResultCommand = "ExecuteReportAuthorizeResultCommand";

	public const string ExecuteValidateVersionCommand = "ExecuteValidateVersionCommand";

	public const string ExecuteCheckActivationCommand = "ExecuteCheckActivationCommand";

	public const string ExecuteCheckDeviceStatusCommand = "ExecuteCheckDeviceStatusCommand";

	public const string SendDeviceTamperedEvent = "SendDeviceTamperedEvent";
}

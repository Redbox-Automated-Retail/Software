namespace RBA_SDK_ComponentModel;

public interface IRBA_API
{
	LogHandler logHandler { get; set; }

	PinPadMessageHandler pinpadHandler { get; set; }

	ERROR_ID Initialize();

	void SetDefaultLogLevel(LOG_LEVEL level);

	ERROR_ID Connect(string comPort);

	ERROR_ID Disconnect();

	ERROR_ID SetParam(PARAMETER_ID id, byte[] data);

	ERROR_ID SetParam(PARAMETER_ID id, string data);

	ERROR_ID ResetParam(PARAMETER_ID id);

	string GetParam(PARAMETER_ID id);

	int GetParamLen(PARAMETER_ID id);

	ERROR_ID AddTagParam(MESSAGE_ID msg, int tagId, byte[] data);

	ERROR_ID AddTagParam(MESSAGE_ID msg, int tagId, string data);

	int GetTagParamLen(MESSAGE_ID msg);

	int GetTagParam(MESSAGE_ID msg, out byte[] data);

	ERROR_ID ProcessMessage(MESSAGE_ID messageId);

	ERROR_ID SetNotifyRbaDisconnected(DisconnectHandler handler);

	ERROR_ID SetCommTimeouts(SETTINGS_COMM_TIMEOUTS timeouts);

	string GetVersion_NonStatic();

	ERROR_ID AddParam(PARAMETER_ID id, string v);

	object GetMutableParam(PARAMETER_ID id);

	ERROR_ID SendCustomMessage(string RawData, bool bWaitResponce);
}

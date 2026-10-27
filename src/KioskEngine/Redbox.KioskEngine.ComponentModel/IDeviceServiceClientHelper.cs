using System;
using DeviceService.ComponentModel;
using DeviceService.ComponentModel.Commands;
using DeviceService.ComponentModel.Requests;
using DeviceService.ComponentModel.Responses;

namespace Redbox.KioskEngine.ComponentModel;

public interface IDeviceServiceClientHelper
{
	bool IsConnectedToDeviceService { get; }

	bool IsCardReaderConnected { get; }

	bool IsCardReaderTampered { get; set; }

	Version DeviceServiceClientVersion { get; }

	Version DeviceServiceVersion { get; }

	IDeviceServiceShutDownInfo DeviceServiceShutDownInfo { get; }

	bool SupportsEMV { get; }

	bool SupportsVas { get; }

	bool ConnectToDeviceService();

	ValidateVersionModel ValidateVersion();

	IReadCardJob StartCardRead(DeviceInputType deviceInputType, Action<BaseResponseEvent> readCardResponseHandler, Action<BaseResponseEvent> readCardEventHandler, int timeout, VasMode vasMode);

	void CancelCommand(Guid id);

	void ReportAuthorizeResult(bool authorizeSuccessful);

	void PollForCardReaderConnectionAndValidVersion();

	void CheckDeviceActivation();

	void GetCardInsertedStatus(Action<GetCardInsertedStatusResponseEvent> completeCallback);

	void RegisterOnDeviceServiceCanShutDownHandler(OnDeviceServiceCanShutDown onDeviceServiceCanShutDown);

	bool ShutDownDeviceService(bool forceShutdown, ShutDownReason shutDownReason);

	void CheckDeviceStatus();

	void SendTamperedCardReaderKioskAlertMessage();
}

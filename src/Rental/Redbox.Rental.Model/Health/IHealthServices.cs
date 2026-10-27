using System;
using System.Collections.Generic;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;

namespace Redbox.Rental.Model.Health;

public interface IHealthServices
{
	int PingPeriod { get; set; }

	void Ping(string storeNumber);

	void SendAlert(string storeNumber, string alertType, string subType, string message, DateTime dateTime, RemoteServiceCallback callback, Action action = null, bool skipQueue = false);

	void SendAuditEvent(string storeNumber, string eventType, DateTime logDate, string userName, string message, string source, RemoteServiceCallback callback);

	void SendKioskFunctionCheck(string storeNumber, string userName, DateTime reportTime, string verticalSlot, string initTest, string vendDoor, string trackTest, string snapDecode, string touchScreenDriver, string cameraDriver, RemoteServiceCallback callback, Action dotNetCallBack);

	void SendKioskEvents(long kioskId, List<KioskEvent> events);

	void StartPingTimer();

	void StopPingTimer();

	void EnqueueStatistic(string name, string value);

	void EnqueueProperty(string name, string value);

	void ResetMinutesInMaintenanceMode();
}

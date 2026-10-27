using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.Rental.Model.KioskClientService.Data;

public interface IKioskClientServiceData
{
	Task KioskVersions(int kioskId, IDictionary<string, string> versions);

	Task KioskStatistics(int kioskId, IDictionary<string, string> statistics);

	Task KioskProperties(int kioskId, IDictionary<string, string> properties);

	Task<BaseResponse> KioskAlert(Guid messageId, int kioskId, int alertType, string subAlertType, string message, DateTime dateTime);

	Task<BaseResponse> AuditEvent(Guid messageId, int kioskId, string eventType, DateTime logDate, string userName, string message, string source);

	Task<BaseResponse> KioskEvents(Guid messageId, long kioskId, DateTime createdOn, IList<KioskEvent> events);

	Task<BaseResponse> KioskFunctionCheck(Guid messageId, long kioskId, string userName, DateTime reportTime, string verticalSlot, string initTest, string vendDoor, string trackTest, string snapDecode, string touchScreenDriver, string cameraDriver);
}

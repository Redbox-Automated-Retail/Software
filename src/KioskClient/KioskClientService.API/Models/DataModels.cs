using System;
using System.Collections.Generic;
using KioskClientService.API.App;
using KioskClientService.API.Services.HttpQueue;

namespace KioskClientService.API.Models;

public static class DataModels
{
	public abstract class KioskBaseRequest : BaseRequest, IMessageScrub
	{
		public Guid MessageId { get; set; }

		public long KioskId { get; set; }

		public bool? SendToTable { get; set; }

		public abstract object Scrub();
	}

	public class AuditEventRequest : KioskBaseRequest
	{
		public DateTime LogDate { get; set; }

		public string CreatedOnLocal { get; set; }

		public string EventType { get; set; }

		public string Username { get; set; }

		public string Message { get; set; }

		public string Source { get; set; }

		public override object Scrub()
		{
			return new
			{
				MessageId = base.MessageId,
				KioskId = base.KioskId,
				LogDate = LogDate,
				CreatedOnLocal = CreatedOnLocal,
				EventType = EventType,
				Username = ObfuscationHelper.ObfuscateStringData(Username, 1, 1),
				Message = Message,
				Source = Source
			}.ToJsonIndented();
		}
	}

	public class KioskAlertRequest : KioskBaseRequest
	{
		private Guid _uniqueId;

		public Guid UniqueId
		{
			get
			{
				return _uniqueId;
			}
			set
			{
				Guid uniqueId = (base.MessageId = value);
				_uniqueId = uniqueId;
			}
		}

		public DateTime CreatedOn { get; set; }

		public string CreatedOnLocal { get; set; }

		public int AlertType { get; set; }

		public string SubAlertType { get; set; }

		public string AlertMessage { get; set; }

		public override object Scrub()
		{
			return this.ToJsonIndented();
		}
	}

	public class KioskEventsRequest : KioskBaseRequest
	{
		public DateTime CreatedOn { get; set; }

		public string CreatedOnLocal { get; set; }

		public string KioskEventsData { get; set; }

		public override object Scrub()
		{
			return new
			{
				CreatedOn = CreatedOn,
				CreatedOnLocal = CreatedOnLocal,
				KioskEventsData = KioskEventsData.EllipseAt(30),
				MessageId = base.MessageId,
				KioskId = base.KioskId,
				SendToTable = base.SendToTable
			}.ToJsonIndented();
		}
	}

	public class KioskFunctionCheckRequest : KioskBaseRequest
	{
		public DateTime CreatedOn { get; set; }

		public string CreatedOnLocal { get; set; }

		public DateTime ReportTime { get; set; }

		public string UserName { get; set; }

		public string InitTest { get; set; }

		public string TrackTest { get; set; }

		public string VendDoor { get; set; }

		public string VerticalSlot { get; set; }

		public string CameraDriver { get; set; }

		public string SnapDecode { get; set; }

		public string TouchScreenDriver { get; set; }

		public override object Scrub()
		{
			return this.ToJsonIndented();
		}
	}

	public class KioskPropertiesRequest : KioskBaseRequest
	{
		public DateTime CreatedOn { get; set; }

		public string CreatedOnLocal { get; set; }

		public IDictionary<string, string> Properties { get; set; }

		public override object Scrub()
		{
			return this.ToJsonIndented();
		}
	}

	public class KioskStatisticsRequest : KioskBaseRequest
	{
		public DateTime CreatedOn { get; set; }

		public string CreatedOnLocal { get; set; }

		public IDictionary<string, string> Statistics { get; set; }

		public override object Scrub()
		{
			return this.ToJsonIndented();
		}
	}

	public class KioskVersionRequest : KioskBaseRequest
	{
		public DateTime CreatedOn { get; set; }

		public string CreatedOnLocal { get; set; }

		public IDictionary<string, string> Versions { get; set; }

		public override object Scrub()
		{
			return this.ToJsonIndented();
		}
	}

	public class KioskDataResponse : BaseResponse, IMessageScrub, IReplayableApiCall
	{
		public bool Replayable => !base.Success;

		public KioskDataResponse()
			: base(200)
		{
			base.Success = true;
		}

		public override object Scrub()
		{
			return this.ToJsonIndented();
		}

		public override string ToString()
		{
			return this.ToJsonIndented();
		}
	}
}

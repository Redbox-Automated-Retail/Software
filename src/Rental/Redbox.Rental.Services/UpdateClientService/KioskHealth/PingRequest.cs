using System;
using Redbox.Rental.Model;
using Redbox.Rental.Model.UpdateClientService.KioskHealth;

namespace Redbox.Rental.Services.UpdateClientService.KioskHealth;

public class PingRequest : IPingRequest, IMessageBase
{
	public Guid MessageId { get; set; }

	public long KioskId { get; set; }

	public string AppName { get; set; }

	public string UTCDate { get; set; }

	public string LocalDate { get; set; }

	public bool? SendToTable { get; set; }
}

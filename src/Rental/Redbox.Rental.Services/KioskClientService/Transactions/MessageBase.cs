using System;
using DeviceService.ComponentModel;
using Redbox.Rental.Model;

namespace Redbox.Rental.Services.KioskClientService.Transactions;

public abstract class MessageBase : IMessageBase, IMessageScrub
{
	public Guid MessageId { get; set; }

	public long KioskId { get; set; }

	public override string ToString()
	{
		return $"{MessageId}";
	}

	public virtual object Scrub()
	{
		return ToString();
	}

	public virtual void ScrubData()
	{
	}
}

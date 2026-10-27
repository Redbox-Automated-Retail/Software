using System;
using KioskClientService.API.App;

namespace KioskClientService.API.Services.Transaction;

public abstract class MessageBase : IMessageScrub
{
	public Guid MessageId { get; set; }

	public long KioskId { get; set; }

	public override string ToString()
	{
		return $"MessageId: {MessageId}, KioskId: {KioskId}";
	}

	public virtual object Scrub()
	{
		return ToString();
	}

	public virtual void ScrubData()
	{
	}
}

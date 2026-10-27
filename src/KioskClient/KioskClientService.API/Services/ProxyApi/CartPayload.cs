using System;
using System.ComponentModel.DataAnnotations;
using KioskClientService.API.App;
using KioskClientService.API.Services.Transaction;
using KioskClientService.API.Validation;

namespace KioskClientService.API.Services.ProxyApi;

public class CartPayload : IMessageScrub
{
	[NotDefault]
	public Guid MessageId { get; set; }

	[Range(1, int.MaxValue, ErrorMessage = "{0} is not within the valid range of {1} and {2}")]
	public long KioskId { get; set; }

	[RegularExpression("([0-9]+)")]
	public string CustomerProfileNumber { get; set; }

	public DateTime RentDate => DateTime.Now;

	public ShoppingCart ShoppingCart { get; set; } = new ShoppingCart();

	public object Scrub()
	{
		return this.ToJsonIndented();
	}
}

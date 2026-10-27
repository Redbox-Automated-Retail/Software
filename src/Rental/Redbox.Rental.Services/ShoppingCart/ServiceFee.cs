using System;
using Redbox.Core;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.ShoppingCart;

namespace Redbox.Rental.Services.ShoppingCart;

public class ServiceFee : IServiceFee
{
	public decimal DefaultAmount { get; set; }

	public decimal ActualAmount { get; set; }

	public decimal TaxRate { get; set; }

	public decimal TaxAmount
	{
		get
		{
			string text = (ServiceLocator.Instance.GetService<IDataService>()?.GetStore())?.State;
			if (text == "SC" || text == "MD")
			{
				return 0m;
			}
			return Math.Max(decimal.Round(ActualAmount * TaxRate / 100m, 16, MidpointRounding.AwayFromZero), 0m);
		}
	}
}

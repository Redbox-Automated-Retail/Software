using System;
using System.Globalization;
using System.Windows.Data;
using Redbox.Rental.Model.KioskClientService.Transactions;

namespace Redbox.KioskEngine.PreferenceManager.TransactionService;

public class RadioOffersToEnumConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return ToOffer(parameter.ToString());
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return parameter;
	}

	public static KioskClientServiceTransactionConstants.TestData.AfterSwipeCustomerOffers.Offers ToOffer(string offer)
	{
		KioskClientServiceTransactionConstants.TestData.AfterSwipeCustomerOffers.Offers result = KioskClientServiceTransactionConstants.TestData.AfterSwipeCustomerOffers.Offers.NoOffer;
		if (Enum.TryParse<KioskClientServiceTransactionConstants.TestData.AfterSwipeCustomerOffers.Offers>(offer, out var result2))
		{
			result = result2;
		}
		return result;
	}
}

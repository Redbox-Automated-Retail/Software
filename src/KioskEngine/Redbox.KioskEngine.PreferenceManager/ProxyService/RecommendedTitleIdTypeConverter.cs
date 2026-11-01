using System;
using System.Globalization;
using System.Windows.Data;
using Redbox.Rental.Model.Personalization;

namespace Redbox.KioskEngine.PreferenceManager.ProxyService;

public class RecommendedTitleIdTypeConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		RecommendedTitlesIdType expectedType = (RecommendedTitlesIdType)Enum.Parse(typeof(RecommendedTitlesIdType), (string)parameter);
		RecommendedTitlesIdType recommendedTitlesIdType = (RecommendedTitlesIdType)value;
		return expectedType == recommendedTitlesIdType;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return Enum.Parse(targetType, (string)parameter);
	}
}

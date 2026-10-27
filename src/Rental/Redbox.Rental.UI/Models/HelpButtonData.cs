using System.Windows;
using Redbox.Rental.Model.Session;

namespace Redbox.Rental.UI.Models;

public class HelpButtonData : BaseModel<HelpButtonData>
{
	public static DependencyProperty IsEnabledProperty = BaseModel<HelpButtonData>.CreateDependencyProperty("IsEnabled", TYPES.BOOL, false);

	public string ButtonText { get; set; }

	public HelpDocuments DocumentToShow { get; set; }

	public bool IsEnabled
	{
		get
		{
			return base.Dispatcher.Invoke(() => (bool)GetValue(IsEnabledProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(IsEnabledProperty, value);
			});
		}
	}
}

using System.Collections.ObjectModel;

namespace Redbox.Rental.UI.Models.DesignTime;

public class DesignHelpViewModel : HelpViewModel
{
	public DesignHelpViewModel()
	{
		base.ButtonList = new ObservableCollection<HelpButtonData>
		{
			new HelpButtonData
			{
				ButtonText = "Perks",
				IsEnabled = true
			},
			new HelpButtonData
			{
				ButtonText = "Rental FAQs",
				IsEnabled = false
			},
			new HelpButtonData
			{
				ButtonText = "Mobile FAQs",
				IsEnabled = true
			},
			new HelpButtonData
			{
				ButtonText = "Transaction Terms",
				IsEnabled = true
			},
			new HelpButtonData
			{
				ButtonText = "Redbox Card Terms",
				IsEnabled = true
			},
			new HelpButtonData
			{
				ButtonText = "Subscriptions",
				IsEnabled = true
			},
			new HelpButtonData
			{
				ButtonText = "Redbox+",
				IsEnabled = true
			},
			new HelpButtonData
			{
				ButtonText = "Privacy Policy",
				IsEnabled = true
			},
			new HelpButtonData
			{
				ButtonText = "CA Privacy Notice",
				IsEnabled = true
			},
			new HelpButtonData
			{
				ButtonText = "Do Not Sell",
				IsEnabled = true
			}
		};
	}
}

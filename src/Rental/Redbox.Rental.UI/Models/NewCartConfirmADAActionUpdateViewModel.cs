using System.Collections.Generic;
using System.Windows;

namespace Redbox.Rental.UI.Models;

public class NewCartConfirmADAActionUpdateViewModel : BaseModel<NewCartConfirmADAActionUpdateViewModel>
{
	public static readonly DependencyProperty DisplayProductModelsProperty = BaseModel<NewCartConfirmADAActionUpdateViewModel>.CreateDependencyProperty("DisplayProductModels", typeof(List<DisplayCheckoutProductModel>));

	public DynamicRoutedCommand OkButtonCommand { get; set; }

	public string Title { get; set; }

	public string OkButtonText { get; set; }

	public List<DisplayCheckoutProductModel> DisplayProductModels
	{
		get
		{
			return base.Dispatcher.Invoke(() => (List<DisplayCheckoutProductModel>)GetValue(DisplayProductModelsProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(DisplayProductModelsProperty, value);
			});
		}
	}
}

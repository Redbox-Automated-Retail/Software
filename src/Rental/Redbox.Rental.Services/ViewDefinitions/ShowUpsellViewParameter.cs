using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.KioskProduct;

namespace Redbox.Rental.Services.ViewDefinitions;

public class ShowUpsellViewParameter : IViewAnalyticsName
{
	public string ViewAnalyticsName
	{
		get
		{
			string result = null;
			switch (TitleType)
			{
			case TitleType.DVD:
				result = "blu_ray_upsell_view";
				break;
			case TitleType.Bluray:
				result = "4k_upsell_view";
				break;
			}
			return result;
		}
	}

	public TitleType TitleType { get; set; }
}

using System;
using System.Windows.Media.Imaging;
using Redbox.Rental.Model.Browse;

namespace Redbox.Rental.UI.Models;

public class DisplayProductMoreLikeThisModel : BrowseItemModel
{
	public BitmapImage Image { get; set; }

	public event Action<IBrowseItemModel> OnMoreLikeThisSelected;

	public void ProcessMoreLikeThisSelected(IBrowseItemModel browseItemModel)
	{
		OnMoreLikeThisSelected?.Invoke(browseItemModel);
	}
}

using System;
using System.Collections.Generic;
using System.Windows;

namespace Redbox.Rental.UI.Models;

public class ShoppingCartUpdateADAViewModel : BaseModel<ShoppingCartUpdateADAViewModel>
{
	public Action OnOkButtonPress;

	public Action OnAddMovieButtonPress;

	public Action<DisplayProductModel, object> OnCancelBrowseItemModel;

	public static readonly DependencyProperty AddMovieButtonVisibilityProperty = BaseModel<ShoppingCartUpdateADAViewModel>.CreateDependencyProperty("AddMovieButtonVisibility", typeof(Visibility), Visibility.Visible);

	public static readonly DependencyProperty DisplayProductModelsProperty = BaseModel<ShoppingCartUpdateADAViewModel>.CreateDependencyProperty("DisplayProductModels", typeof(List<DisplayProductModel>));

	public string Title { get; set; }

	public string Message { get; set; }

	public string OkButtonText { get; set; }

	public string AddMovieButtonText { get; set; }

	public Visibility AddMovieButtonVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(AddMovieButtonVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(AddMovieButtonVisibilityProperty, value);
			});
		}
	}

	public List<DisplayProductModel> DisplayProductModels
	{
		get
		{
			return base.Dispatcher.Invoke(() => (List<DisplayProductModel>)GetValue(DisplayProductModelsProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(DisplayProductModelsProperty, value);
			});
		}
	}

	public void ProcessOkButtonPress()
	{
		OnOkButtonPress?.Invoke();
	}

	public void ProcessAddMovieButtonPress()
	{
		OnAddMovieButtonPress?.Invoke();
	}

	public void ProcessOnCancelBrowseItemModel(DisplayProductModel displayProductModel, object parameter)
	{
		OnCancelBrowseItemModel?.Invoke(displayProductModel, parameter);
	}
}

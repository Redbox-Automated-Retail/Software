using System;
using System.Collections.Generic;
using System.Windows;
using Redbox.Rental.Model.Browse;

namespace Redbox.Rental.UI.Models;

public class CarouselModel : DependencyObject
{
	public static readonly DependencyProperty CarouselProductsProperty = DependencyProperty.Register("CarouselProducts", typeof(List<DisplayProductModel>), typeof(CarouselModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty TitleRollupOverlayModelProperty = DependencyProperty.Register("TitleRollupOverlayModel", typeof(TitleRollupOverlayModel), typeof(CarouselModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty IsAnimationOnProperty = DependencyProperty.Register("IsAnimationOn", typeof(bool), typeof(CarouselModel), new FrameworkPropertyMetadata(true, OnIsAnimationOnPropertyChanged));

	public bool ShowAddButton { get; set; }

	public List<DisplayProductModel> CarouselDisplayProductModels
	{
		get
		{
			return base.Dispatcher.Invoke(() => (List<DisplayProductModel>)GetValue(CarouselProductsProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(CarouselProductsProperty, value);
			});
		}
	}

	public TitleRollupOverlayModel TitleRollupOverlayModel
	{
		get
		{
			return base.Dispatcher.Invoke(() => (TitleRollupOverlayModel)GetValue(TitleRollupOverlayModelProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(TitleRollupOverlayModelProperty, value);
			});
		}
	}

	public bool IsAnimationOn
	{
		get
		{
			return base.Dispatcher.Invoke(() => (bool)GetValue(IsAnimationOnProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(IsAnimationOnProperty, value);
			});
		}
	}

	public event BrowseItemModelEvent OnCarouselRotation;

	public event BrowseItemModelEvent OnDisplayProductModelSelected;

	public event BrowseItemModelEvent OnAddDisplayProductModel;

	public event BrowseItemModelEvent OnTitleRollupOverlayCancel;

	public event BrowseItemModelEvent OnTitleRollupOverlayTimeout;

	public event BrowseItemModelEvent OnTitleRollupOverlayAddDVD;

	public event BrowseItemModelEvent OnTitleRollupOverlayAddBluray;

	public event BrowseItemModelEvent OnTitleRollupOverlayAdd4kUhd;

	public event VisibilityChanged OnTitleRollupOverlayVisibilityChange;

	public event Action<CarouselModel> OnIsAnimationOnChanged;

	public CarouselModel()
	{
		TitleRollupOverlayModel = new TitleRollupOverlayModel();
		TitleRollupOverlayModel.OnTitleRollupOverlayCancel += TitleRollupOverlayModel_OnTitleRollupOverlayCancel;
		TitleRollupOverlayModel.OnTitleRollupOverlayTimeout += TitleRollupOverlayModel_OnTitleRollupOverlayTimeout;
		TitleRollupOverlayModel.OnTitleRollupOverlayAddDVD += TitleRollupOverlayModel_OnTitleRollupOverlayAddDVD;
		TitleRollupOverlayModel.OnTitleRollupOverlayAddBluray += TitleRollupOverlayModel_OnTitleRollupOverlayAddBluray;
		TitleRollupOverlayModel.OnTitleRollupOverlayAdd4kUhd += TitleRollupOverlayModel_OnTitleRollupOverlayAdd4kUhd;
		TitleRollupOverlayModel.OnTitleRollupOverlayVisibilityChange += TitleRollupOverlayModel_OnTitleRollupOverlayVisibilityChange;
	}

	private void TitleRollupOverlayModel_OnTitleRollupOverlayVisibilityChange(Visibility newVisibility)
	{
		ProcessTitleRollupVisibilityChange(newVisibility);
	}

	private void TitleRollupOverlayModel_OnTitleRollupOverlayAddBluray(IBrowseItemModel browseItemModel, object parameter)
	{
		ProcessTitleRollupOverlayAddBluray(browseItemModel, parameter);
	}

	private void TitleRollupOverlayModel_OnTitleRollupOverlayAdd4kUhd(IBrowseItemModel browseItemModel, object parameter)
	{
		ProcessTitleRollupOverlayAdd4kUhd(browseItemModel, parameter);
	}

	private void TitleRollupOverlayModel_OnTitleRollupOverlayAddDVD(IBrowseItemModel browseItemModel, object parameter)
	{
		ProcessTitleRollupOverlayAddDVD(browseItemModel, parameter);
	}

	private void TitleRollupOverlayModel_OnTitleRollupOverlayTimeout(IBrowseItemModel browseItemModel, object parameter)
	{
		ProcessTitleRollupOverlayTimeout(browseItemModel, parameter);
	}

	private void TitleRollupOverlayModel_OnTitleRollupOverlayCancel(IBrowseItemModel browseItemModel, object parameter)
	{
		ProcessTitleRollupOverlayCancel(browseItemModel, parameter);
	}

	public void ProcessTitleRollupVisibilityChange(Visibility newVisibility)
	{
		if (OnTitleRollupOverlayVisibilityChange != null)
		{
			OnTitleRollupOverlayVisibilityChange(newVisibility);
		}
	}

	public void ProcessTitleRollupOverlayAdd4kUhd(IBrowseItemModel browseItemModel, object parameter)
	{
		if (OnTitleRollupOverlayAdd4kUhd != null)
		{
			OnTitleRollupOverlayAdd4kUhd(browseItemModel, parameter);
		}
	}

	public void ProcessTitleRollupOverlayAddBluray(IBrowseItemModel browseItemModel, object parameter)
	{
		if (OnTitleRollupOverlayAddBluray != null)
		{
			OnTitleRollupOverlayAddBluray(browseItemModel, parameter);
		}
	}

	public void ProcessTitleRollupOverlayAddDVD(IBrowseItemModel browseItemModel, object parameter)
	{
		if (OnTitleRollupOverlayAddDVD != null)
		{
			OnTitleRollupOverlayAddDVD(browseItemModel, parameter);
		}
	}

	public void ProcessTitleRollupOverlayTimeout(IBrowseItemModel browseItemModel, object parameter)
	{
		if (OnTitleRollupOverlayTimeout != null)
		{
			OnTitleRollupOverlayTimeout(browseItemModel, parameter);
		}
	}

	public void ProcessTitleRollupOverlayCancel(IBrowseItemModel browseItemModel, object parameter)
	{
		if (OnTitleRollupOverlayCancel != null)
		{
			OnTitleRollupOverlayCancel(browseItemModel, parameter);
		}
	}

	public void ProcessAddDisplayProductModel(DisplayProductModel displayProductModel, object parameter)
	{
		if (OnAddDisplayProductModel != null)
		{
			OnAddDisplayProductModel(displayProductModel, parameter);
		}
	}

	public void ProcessCarouselRotation(DisplayProductModel displayProductModel, object parameter)
	{
		if (OnCarouselRotation != null)
		{
			OnCarouselRotation(displayProductModel, parameter);
		}
	}

	public void ProcessDisplayProductModelSelected(DisplayProductModel displayProductModel, object parameter)
	{
		if (OnDisplayProductModelSelected != null)
		{
			OnDisplayProductModelSelected(displayProductModel, parameter);
		}
	}

	private static void OnIsAnimationOnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is CarouselModel { OnIsAnimationOnChanged: not null } carouselModel)
		{
			carouselModel.OnIsAnimationOnChanged(carouselModel);
		}
	}
}

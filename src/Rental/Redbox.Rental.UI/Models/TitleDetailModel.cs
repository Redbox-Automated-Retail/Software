using System;
using System.Windows;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.Model.Browse;
using Redbox.Rental.Model.KioskProduct;

namespace Redbox.Rental.UI.Models;

public class TitleDetailModel : DependencyObject
{
	public Func<ISpeechControl> OnGetSpeechControl;

	public static readonly DependencyProperty BrowseProductControlModelProperty = DependencyProperty.Register("BrowseProductControlModel", typeof(BrowseControlModel), typeof(TitleDetailModel), new FrameworkPropertyMetadata(null, OnBrowseProductControlModelPropertyChanged));

	public static readonly DependencyProperty BackTextProperty = DependencyProperty.Register("BackText", typeof(string), typeof(TitleDetailModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty BackgroundImageVisibilityProperty = DependencyProperty.Register("BackgroundImageVisibility", typeof(Visibility), typeof(TitleDetailModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

	public BrowseControlModel BrowseProductControlModel
	{
		get
		{
			return base.Dispatcher.Invoke(() => (BrowseControlModel)GetValue(BrowseProductControlModelProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BrowseProductControlModelProperty, value);
			});
		}
	}

	public string BackText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(BackTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BackTextProperty, value);
			});
		}
	}

	public Visibility BackgroundImageVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(BackgroundImageVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BackgroundImageVisibilityProperty, value);
			});
		}
	}

	public event Action<IBrowseItemModel, object, TitleType, bool> RentProduct;

	public event Action<IBrowseItemModel, object> BuyButtonHit;

	public event Action<IBrowseItemModel> MoreInfoHit;

	public event Action GoBackHit;

	public event Action<IBrowseItemModel> SeeFullDetails;

	public event Action RedboxPlusInfo;

	public event Action DualInStockLearnMore;

	public event Action StatisticPopup;

	public event Action<TitleDetailModel> OnTitleDetailModelChanged;

	internal ISpeechControl ProcessGetSpeechControl()
	{
		return OnGetSpeechControl?.Invoke();
	}

	public void ProcessAddProduct(IBrowseItemModel model, object o, TitleType t)
	{
		RentProduct?.Invoke(model, o, t, arg4: false);
	}

	public void ProcessBuyButtonHit(IBrowseItemModel model, object o)
	{
		BuyButtonHit?.Invoke(model, o);
	}

	public void ProcessMoreInfoHit(IBrowseItemModel model)
	{
		MoreInfoHit?.Invoke(model);
	}

	public void ProcessGoBackHit()
	{
		GoBackHit?.Invoke();
	}

	public void ProcessSeeFullDetails(IBrowseItemModel model)
	{
		SeeFullDetails?.Invoke(model);
	}

	public void ProcessRedboxPlusInfo()
	{
		RedboxPlusInfo?.Invoke();
	}

	public void ProcessDualInStockLearnMore()
	{
		DualInStockLearnMore?.Invoke();
	}

	public void ProcessStatisticPopup()
	{
		StatisticPopup?.Invoke();
	}

	private static void OnBrowseProductControlModelPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is TitleDetailModel { OnTitleDetailModelChanged: not null } titleDetailModel)
		{
			titleDetailModel.OnTitleDetailModelChanged(titleDetailModel);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Windows;
using Redbox.Rental.Model.KioskProduct;

namespace Redbox.Rental.UI.Models;

public class UpsellViewModel : DependencyObject
{
	public Action OnBackButtonClicked;

	public Action OnNoThanksButtonClicked;

	public Action OnUpgradeButtonClicked;

	public Func<bool> OnCanExecuteUpgradeButton;

	public static readonly DependencyProperty DisplayProductModelsProperty = DependencyProperty.Register("DisplayProductModels", typeof(List<DisplayUpsellProductModel>), typeof(UpsellViewModel), new FrameworkPropertyMetadata(null)
	{
		AffectsRender = true
	});

	public TitleType UpsellFromTitleType { get; set; }

	public Action<DisplayUpsellProductModel> OnCheckBoxClick { get; set; }

	public string TitleText { get; set; }

	public string MessageText { get; set; }

	public string Message2Text { get; set; }

	public string BackButtonText { get; set; }

	public string NoThanksButtonText { get; set; }

	public string UpgradeButtonText { get; set; }

	public List<DisplayUpsellProductModel> DisplayProductModels
	{
		get
		{
			return base.Dispatcher.Invoke(() => (List<DisplayUpsellProductModel>)GetValue(DisplayProductModelsProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(DisplayProductModelsProperty, value);
			});
		}
	}

	public bool ProcessOnCanExecuteUpgradeButton()
	{
		return OnCanExecuteUpgradeButton?.Invoke() ?? false;
	}

	public void ProcessOnCheckBoxClick(DisplayUpsellProductModel displayUpsellProductModel)
	{
		OnCheckBoxClick?.Invoke(displayUpsellProductModel);
	}

	public void ProcessOnBackButtonClicked()
	{
		OnBackButtonClicked?.Invoke();
	}

	public void ProcessOnNoThanksButtonClicked()
	{
		OnNoThanksButtonClicked?.Invoke();
	}

	public void ProcessOnUpgradeButtonClicked()
	{
		OnUpgradeButtonClicked?.Invoke();
	}
}

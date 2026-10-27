using System.Collections.Generic;
using System.Windows;
using Redbox.Rental.UI.ControllersLogic;

namespace Redbox.Rental.UI.Models;

public class MemberPointsModel : BaseModel<MemberPointsModel>
{
	public static readonly DependencyProperty PointsTitleProperty = BaseModel<MemberPointsModel>.CreateDependencyProperty("PointsTitle", TYPES.STRING);

	public static readonly DependencyProperty PointsMessageProperty = BaseModel<MemberPointsModel>.CreateDependencyProperty("PointsMessage", TYPES.STRING);

	public static readonly DependencyProperty PointsCartMessageProperty = BaseModel<MemberPointsModel>.CreateDependencyProperty("PointsCartMessage", TYPES.STRING);

	public static readonly DependencyProperty PointsMarketingProperty = BaseModel<MemberPointsModel>.CreateDependencyProperty("PointsMarketing", TYPES.STRING);

	public static readonly DependencyProperty CancelButtonTextProperty = BaseModel<MemberPointsModel>.CreateDependencyProperty("CancelButtonText", TYPES.STRING);

	public static readonly DependencyProperty ContinueButtonTextProperty = BaseModel<MemberPointsModel>.CreateDependencyProperty("ContinueButtonText", TYPES.STRING);

	public static readonly DependencyProperty ContinueButtonEnabledProperty = BaseModel<MemberPointsModel>.CreateDependencyProperty("ContinueButtonEnabled", TYPES.BOOL, false);

	public static readonly DependencyProperty SelectedProductIdsProperty = BaseModel<MemberPointsModel>.CreateDependencyProperty("SelectedProductIds", typeof(List<long>));

	public static readonly DependencyProperty DisplayProductModelsProperty = BaseModel<MemberPointsModel>.CreateDependencyProperty("DisplayProductModels", typeof(List<DisplayProductModel>));

	public MemberPointsLogic Logic { get; set; }

	public List<long> EntryShoppingProductIds { get; set; }

	public DynamicRoutedCommand PointsIncludeCommand { get; set; }

	public DynamicRoutedCommand PointsExcludeCommand { get; set; }

	public DynamicRoutedCommand CancelButtonCommand { get; set; }

	public DynamicRoutedCommand ContinueButtonCommand { get; set; }

	public string PointsTitle
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(PointsTitleProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(PointsTitleProperty, value);
			});
		}
	}

	public string PointsMessage
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(PointsMessageProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(PointsMessageProperty, value);
			});
		}
	}

	public string PointsCartMessage
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(PointsCartMessageProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(PointsCartMessageProperty, value);
			});
		}
	}

	public string PointsMarketing
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(PointsMarketingProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(PointsMarketingProperty, value);
			});
		}
	}

	public string CancelButtonText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(CancelButtonTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(CancelButtonTextProperty, value);
			});
		}
	}

	public string ContinueButtonText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(ContinueButtonTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ContinueButtonTextProperty, value);
			});
		}
	}

	public bool ContinueButtonEnabled
	{
		get
		{
			return base.Dispatcher.Invoke(() => (bool)GetValue(ContinueButtonEnabledProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ContinueButtonEnabledProperty, value);
			});
		}
	}

	public List<long> SelectedProductIds
	{
		get
		{
			return base.Dispatcher.Invoke(() => (List<long>)GetValue(SelectedProductIdsProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(SelectedProductIdsProperty, value);
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

	public MemberPointsModel()
	{
		SelectedProductIds = new List<long>();
		EntryShoppingProductIds = new List<long>();
	}
}

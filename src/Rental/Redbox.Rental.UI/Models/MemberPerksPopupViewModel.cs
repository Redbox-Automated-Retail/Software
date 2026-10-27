using System.Collections.ObjectModel;
using System.Windows;
using Redbox.Rental.UI.ControllersLogic;

namespace Redbox.Rental.UI.Models;

public class MemberPerksPopupViewModel : BaseModel<MemberPerksPopupViewModel>
{
	public static readonly DependencyProperty TitleProperty = BaseModel<MemberPerksPopupViewModel>.CreateDependencyProperty("Title", TYPES.STRING);

	public static readonly DependencyProperty NextLevelRentalsProperty = BaseModel<MemberPerksPopupViewModel>.CreateDependencyProperty("NextLevelRentals", TYPES.OBJECT);

	public static readonly DependencyProperty BulletPointsProperty = BaseModel<MemberPerksPopupViewModel>.CreateDependencyProperty("BulletPoints", typeof(ObservableCollection<string>));

	public static readonly DependencyProperty MemberTierProperty = BaseModel<MemberPerksPopupViewModel>.CreateDependencyProperty("MemberTier", typeof(TierType), TierType.Member);

	public DynamicRoutedCommand CancelButtonCommand { get; set; }

	public string CancelButtonText { get; set; }

	public string Title
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(TitleProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(TitleProperty, value);
			});
		}
	}

	public string NextLevelRentals
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(NextLevelRentalsProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(NextLevelRentalsProperty, value);
			});
		}
	}

	public ObservableCollection<string> BulletPoints
	{
		get
		{
			return base.Dispatcher.Invoke(() => (ObservableCollection<string>)GetValue(BulletPointsProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BulletPointsProperty, value);
			});
		}
	}

	public TierType MemberTier
	{
		get
		{
			return base.Dispatcher.Invoke(() => (TierType)GetValue(MemberTierProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(MemberTierProperty, value);
			});
		}
	}
}

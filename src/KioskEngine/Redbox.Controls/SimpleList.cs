using System.Windows;
using System.Windows.Controls;
using Redbox.Controls.Utilities;

namespace Redbox.Controls;

public class SimpleList : ItemsControl
{
	public static readonly DependencyProperty ItemsHeightProperty = Dependency<SimpleList>.CreateDependencyProperty("ItemsHeight", typeof(int), 0);

	public static readonly DependencyProperty ItemsMarginProperty = Dependency<SimpleList>.CreateDependencyProperty("ItemsMargin", typeof(Thickness), default(Thickness));

	public int ItemsHeight
	{
		get
		{
			return (int)GetValue(ItemsHeightProperty);
		}
		set
		{
			SetValue(ItemsHeightProperty, value);
		}
	}

	public Thickness ItemsMargin
	{
		get
		{
			return (Thickness)GetValue(ItemsMarginProperty);
		}
		set
		{
			SetValue(ItemsMarginProperty, value);
		}
	}

	protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
	{
		ContentPresenter contentPresenter = element as ContentPresenter;
		Thickness margin = contentPresenter.Margin;
		_ = margin.GetType().Name;
		if (ItemsHeight > 0)
		{
			contentPresenter.Height = ItemsHeight;
		}
		_ = ItemsMargin;
		contentPresenter.Margin = ItemsMargin;
		base.PrepareContainerForItemOverride(element, item);
	}

	protected override bool ShouldApplyItemContainerStyle(DependencyObject container, object item)
	{
		_ = container.GetType().Name;
		return true;
	}
}

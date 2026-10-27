using System.Windows;
using Redbox.Rental.Model.Browse;

namespace Redbox.Rental.UI.Models;

public class BrowseItemModel : DependencyObject, IBrowseItemModel
{
	public static readonly DependencyProperty CanAddProperty = DependencyProperty.Register("CanAdd", typeof(bool), typeof(BrowseItemModel), new FrameworkPropertyMetadata(false));

	private int _visibleItemIndex;

	private object _data;

	private bool _isBorderItem;

	private bool _isBottomRow;

	public string Name { get; set; }

	public object Data
	{
		get
		{
			return GetData();
		}
		set
		{
			SetData(value);
		}
	}

	public bool IsBorderItem
	{
		get
		{
			return GetIsBorderItem();
		}
		set
		{
			SetIsBorderItem(value);
		}
	}

	public bool IsBottomRow
	{
		get
		{
			return GetIsBottomRow();
		}
		set
		{
			SetIsBottomRow(value);
		}
	}

	public bool CanAdd
	{
		get
		{
			return GetCanAdd();
		}
		set
		{
			SetCanAdd(value);
		}
	}

	public int VisibleItemIndex
	{
		get
		{
			return GetVisibleItemIndex();
		}
		set
		{
			SetVisibleItemIndex(value);
		}
	}

	public int CurrentGridRow { get; set; }

	public int CurrentGridColumn { get; set; }

	protected virtual object GetData()
	{
		return _data;
	}

	protected virtual void SetData(object value)
	{
		_data = value;
	}

	protected virtual bool GetIsBorderItem()
	{
		return _isBorderItem;
	}

	protected virtual void SetIsBorderItem(bool value)
	{
		_isBorderItem = value;
	}

	protected virtual bool GetIsBottomRow()
	{
		return _isBottomRow;
	}

	protected virtual void SetIsBottomRow(bool value)
	{
		_isBottomRow = value;
	}

	protected virtual bool GetCanAdd()
	{
		return base.Dispatcher.Invoke(() => (bool)GetValue(CanAddProperty));
	}

	protected virtual void SetCanAdd(bool value)
	{
		base.Dispatcher.Invoke(delegate
		{
			SetValue(CanAddProperty, value);
		});
	}

	protected virtual int GetVisibleItemIndex()
	{
		return _visibleItemIndex;
	}

	protected virtual void SetVisibleItemIndex(int value)
	{
		_visibleItemIndex = value;
	}
}

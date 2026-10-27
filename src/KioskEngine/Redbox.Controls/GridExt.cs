using System;
using System.Windows;
using System.Windows.Controls;
using Redbox.Controls.Utilities;

namespace Redbox.Controls;

public class GridExt : Grid
{
	public static readonly DependencyProperty SingleRadiusProperty = Dependency<GridExt>.CreateDependencyProperty("SingleRadius", typeof(double), 0.0);

	public static readonly DependencyProperty BorderRectProperty = Dependency<GridExt>.CreateDependencyProperty("BorderRect", typeof(Rect), new Rect(0.0, 0.0, 0.0, 0.0));

	public static readonly DependencyProperty NumberOfRowsProperty = Dependency<GridExt>.CreateDependencyProperty("NumberOfRows", typeof(int), 1, OnNumberOfRowsChanged);

	public static readonly DependencyProperty NumberOfColumnsProperty = Dependency<GridExt>.CreateDependencyProperty("NumberOfColumns", typeof(int), 1, OnNumberOfColumnsChanged);

	public static readonly DependencyProperty RowHeightsProperty = Dependency<GridExt>.CreateDependencyProperty("RowHeights", typeof(string), "*", OnRowHeightsChanged);

	public static readonly DependencyProperty ColumnWidthsProperty = Dependency<GridExt>.CreateDependencyProperty("ColumnWidths", typeof(string), "*", OnColumnWidthsChanged);

	public double SingleRadius
	{
		get
		{
			return (double)GetValue(SingleRadiusProperty);
		}
		private set
		{
			SetValue(SingleRadiusProperty, value);
		}
	}

	public Rect BorderRect
	{
		get
		{
			return (Rect)GetValue(BorderRectProperty);
		}
		private set
		{
			SetValue(BorderRectProperty, value);
		}
	}

	public int NumberOfRows
	{
		get
		{
			return base.RowDefinitions.Count;
		}
		set
		{
			SetValue(NumberOfRowsProperty, value);
		}
	}

	public int NumberOfColumns
	{
		get
		{
			return base.ColumnDefinitions.Count;
		}
		set
		{
			SetValue(NumberOfColumnsProperty, value);
		}
	}

	public string RowHeights
	{
		get
		{
			return (string)GetValue(RowHeightsProperty);
		}
		set
		{
			SetValue(RowHeightsProperty, value);
		}
	}

	public string ColumnWidths
	{
		get
		{
			return (string)GetValue(ColumnWidthsProperty);
		}
		set
		{
			SetValue(ColumnWidthsProperty, value);
		}
	}

	private void SetNumberOfRows(int count)
	{
		base.RowDefinitions.Clear();
		for (int i = 0; i < count; i++)
		{
			base.RowDefinitions.Add(new RowDefinition());
		}
		if (count > 0 && base.ColumnDefinitions.Count == 0)
		{
			base.ColumnDefinitions.Add(new ColumnDefinition());
		}
	}

	private void SetNumberOfColumns(int count)
	{
		base.ColumnDefinitions.Clear();
		for (int i = 0; i < count; i++)
		{
			base.ColumnDefinitions.Add(new ColumnDefinition());
		}
		if (count > 0 && base.RowDefinitions.Count == 0)
		{
			base.RowDefinitions.Add(new RowDefinition());
		}
	}

	private void SetRowHeights(string heights)
	{
		base.RowDefinitions.Clear();
		ConvertToDefinitions(heights, typeof(RowDefinition));
		if (base.ColumnDefinitions.Count == 0)
		{
			SetTypeValue(typeof(ColumnDefinition), new GridLength(1.0, GridUnitType.Star));
		}
	}

	private void SetColumnWidths(string heights)
	{
		base.ColumnDefinitions.Clear();
		ConvertToDefinitions(heights, typeof(ColumnDefinition));
		if (base.RowDefinitions.Count == 0)
		{
			SetTypeValue(typeof(RowDefinition), new GridLength(1.0, GridUnitType.Star));
		}
	}

	private static void OnNumberOfRowsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((GridExt)d).SetNumberOfRows((int)e.NewValue);
	}

	private static void OnNumberOfColumnsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((GridExt)d).SetNumberOfColumns((int)e.NewValue);
	}

	private static void OnRowHeightsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((GridExt)d).SetRowHeights((string)e.NewValue);
	}

	private static void OnColumnWidthsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((GridExt)d).SetColumnWidths((string)e.NewValue);
	}

	private static bool IsWidthHeightValid(object value)
	{
		double num = (double)value;
		if (double.IsNaN(num))
		{
			return true;
		}
		if (num >= 0.0)
		{
			return !double.IsPositiveInfinity(num);
		}
		return false;
	}

	private void SetTypeValue(Type defType, GridLength gridLength)
	{
		if (defType == typeof(ColumnDefinition))
		{
			ColumnDefinition columnDefinition = new ColumnDefinition();
			columnDefinition.Width = gridLength;
			base.ColumnDefinitions.Add(columnDefinition);
			return;
		}
		if (defType == typeof(RowDefinition))
		{
			RowDefinition rowDefinition = new RowDefinition();
			rowDefinition.Height = gridLength;
			base.RowDefinitions.Add(rowDefinition);
			return;
		}
		throw new InvalidOperationException(string.Format("Invalid Type passed to GridExt:SetTypeValue: {0}", (defType == null) ? "null" : defType.ToString()));
	}

	private void ConvertToDefinitions(string value, Type defType)
	{
		string[] array = value.Split(',');
		for (int i = 0; i < array.Length; i++)
		{
			value = array[i].Trim().ToLower();
			if (double.TryParse(value, out var result))
			{
				SetTypeValue(defType, new GridLength(result, GridUnitType.Pixel));
				continue;
			}
			if (string.Empty.Equals(value))
			{
				SetTypeValue(defType, new GridLength(1.0, GridUnitType.Star));
				continue;
			}
			if (value.Contains("*"))
			{
				value = value.Replace("*", "");
				if (double.TryParse(value, out result))
				{
					SetTypeValue(defType, new GridLength(result, GridUnitType.Star));
				}
				else
				{
					SetTypeValue(defType, new GridLength(1.0, GridUnitType.Star));
				}
				continue;
			}
			if (value.Equals("auto"))
			{
				SetTypeValue(defType, new GridLength(0.0, GridUnitType.Auto));
				continue;
			}
			string message = ((!(defType == typeof(ColumnDefinition))) ? $"RowDefinition: Height value: {value} is invalid" : $"ColumnDefinition: Width value: {value} is invalid");
			throw new InvalidOperationException(message);
		}
	}

	protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
	{
		base.OnRenderSizeChanged(sizeInfo);
		BorderRect = new Rect(sizeInfo.NewSize);
		SingleRadius = base.ActualHeight / 2.0;
	}
}

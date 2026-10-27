using System;
using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using Redbox.Controls.Utilities;

namespace Redbox.Controls.BaseControls;

[Browsable(false)]
[ContentProperty("Child")]
public class BaseContainer : FrameworkElement, IAddChild
{
	private UIElement _childElement;

	[DefaultValue(null)]
	public virtual UIElement Child
	{
		get
		{
			return _childElement;
		}
		set
		{
			SetChild(value);
		}
	}

	protected override IEnumerator LogicalChildren
	{
		get
		{
			if (_childElement != null)
			{
				return new SingleChildEnumerator(_childElement);
			}
			return EmptyEnumerator.Instance;
		}
	}

	protected override int VisualChildrenCount => (_childElement != null) ? 1 : 0;

	protected UIElement IntChild
	{
		get
		{
			return _childElement;
		}
		set
		{
			_childElement = value;
		}
	}

	void IAddChild.AddChild(object value)
	{
		if (!(value is UIElement))
		{
			throw new ArgumentException($"UnexpectedParameterType: {value.GetType()}, Parameter must be of type: {typeof(UIElement)}", "value");
		}
		if (Child != null)
		{
			throw new ArgumentException($"{GetType()} CanOnlyHaveOneChild of type: {value.GetType()}");
		}
		Child = (UIElement)value;
	}

	void IAddChild.AddText(string text)
	{
		if (!string.IsNullOrEmpty(text))
		{
			throw new ArgumentException(string.Format("Container cannot have text: {1}", GetType()));
		}
	}

	private void SetChild(UIElement child)
	{
		if (_childElement != child)
		{
			RemoveVisualChild(_childElement);
			RemoveLogicalChild(_childElement);
			_childElement = child;
			AddLogicalChild(child);
			AddVisualChild(child);
			InvalidateMeasure();
		}
	}

	protected override Visual GetVisualChild(int index)
	{
		if (_childElement == null || index != 0)
		{
			throw new ArgumentOutOfRangeException("index", index, "Visual_ArgumentOutOfRange");
		}
		return _childElement;
	}

	protected override Size MeasureOverride(Size constraint)
	{
		UIElement child = Child;
		if (child == null)
		{
			return default(Size);
		}
		child.Measure(constraint);
		return child.DesiredSize;
	}

	protected override Size ArrangeOverride(Size arrangeSize)
	{
		Child?.Arrange(new Rect(arrangeSize));
		return arrangeSize;
	}
}

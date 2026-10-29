using System;
using System.Windows.Forms;
using Redbox.Extensibility.ComponentModel;
using VisualHint.SmartPropertyGrid;

namespace Redbox.Extensibility.Shell.PropertyGrid;

public class DynamicPropertyGrid : VisualHint.SmartPropertyGrid.PropertyGrid
{
	public DynamicPropertyGrid()
	{
		base.ToolbarVisibility = true;
		base.CommentsVisibility = true;
		base.ShowDefaultValues = true;
		base.ShowAdditionalIndentation = true;
		base.ToolTipMode = ToolTipModes.ToolTipsOnValuesAndLabels;
		ToolStripLabel value = new ToolStripLabel("Filter:");
		ToolStripTextBox toolStripTextBox = new ToolStripTextBox("filter")
		{
			BorderStyle = BorderStyle.None
		};
		base.Toolbar.Items.Add(value);
		base.Toolbar.Items.Add(toolStripTextBox);
		EventHandler value2 = delegate(object sender, EventArgs e)
		{
			FilterProperties(((ToolStripTextBox)sender).Text);
		};
		toolStripTextBox.TextChanged += value2;
	}

	protected internal override void OnDisplayModeChanged(EventArgs e)
	{
		FilterProperties(base.Toolbar.Items[4].Text);
	}

	protected internal override void OnPropertyButtonClicked(PropertyButtonClickedEventArgs e)
	{
		if (e.PropertyEnum.Property.Tag is IPropertyGridItem propertyGridItem)
		{
			propertyGridItem.RaiseActivated();
		}
		base.OnPropertyButtonClicked(e);
	}

	protected override void OnPropertyChanged(PropertyChangedEventArgs e)
	{
		IPropertyGridItem propertyGridItem = e.PropertyEnum.Property.Tag as IPropertyGridItem;
		if (propertyGridItem == null && e.PropertyEnum.Parent != null)
		{
			propertyGridItem = e.PropertyEnum.Parent.Property.Tag as IPropertyGridItem;
		}
		propertyGridItem?.RaisePropertyChanged();
		base.OnPropertyChanged(e);
	}

	protected override void OnPropertyChanging(PropertyChangingEventArgs e)
	{
		IPropertyGridItem propertyGridItem = e.PropertyEnum.Property.Tag as IPropertyGridItem;
		if (propertyGridItem == null && e.PropertyEnum.Parent != null)
		{
			propertyGridItem = e.PropertyEnum.Parent.Property.Tag as IPropertyGridItem;
		}
		propertyGridItem?.RaisePropertyChanging();
		base.OnPropertyChanging(e);
	}

	protected override void OnValueValidation(ValueValidationEventArgs e)
	{
		if (e.PropertyEnum.Property.Tag is IPropertyGridItem propertyGridItem)
		{
			propertyGridItem.RaiseValueValidate();
		}
		base.OnValueValidation(e);
	}

	protected internal override void OnHyperLinkPropertyClicked(PropertyHyperLinkClickedEventArgs e)
	{
		if (e.PropertyEnum.Property.Tag is IPropertyGridItem propertyGridItem)
		{
			propertyGridItem.RaiseActivated();
		}
		base.OnHyperLinkPropertyClicked(e);
	}

	private void FilterProperties(string filter)
	{
		BeginUpdate();
		PropertyEnumerator firstProperty = base.FirstProperty;
		while (firstProperty != firstProperty.RightBound)
		{
			if (firstProperty.Property.DisplayName.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)
			{
				PropertyEnumerator deepEnumerator = firstProperty.GetDeepEnumerator();
				while (deepEnumerator != deepEnumerator.RightBound)
				{
					if (!deepEnumerator.Property.Visible)
					{
						ShowProperty(deepEnumerator, show: true);
					}
					deepEnumerator.MoveParent();
				}
			}
			else
			{
				ShowProperty(firstProperty, show: false);
			}
			firstProperty.MoveNext();
		}
		EndUpdate();
	}
}

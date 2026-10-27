using System;
using System.Windows.Forms;
using Redbox.KioskEngine.IDE.Properties;
using VisualHint.SmartPropertyGrid;

namespace Redbox.KioskEngine.IDE;

public class DynamicPropertyGrid : VisualHint.SmartPropertyGrid.PropertyGrid
{
	public event EventHandler RefreshGrid;

	public DynamicPropertyGrid()
	{
		base.ToolbarVisibility = true;
		base.ShowDefaultValues = true;
		base.CommentsVisibility = false;
		base.ShowAdditionalIndentation = true;
		base.ToolTipMode = ToolTipModes.ToolTipsOnValuesAndLabels;
		ToolStripButton toolStripButton = new ToolStripButton(Resources.Refresh)
		{
			ToolTipText = "Refresh"
		};
		toolStripButton.Click += delegate(object sender, EventArgs e)
		{
			if (RefreshGrid != null)
			{
				RefreshGrid(sender, e);
			}
		};
		ToolStripLabel value = new ToolStripLabel("Filter:");
		ToolStripTextBox toolStripTextBox = new ToolStripTextBox("filter")
		{
			BorderStyle = BorderStyle.None,
			Width = 350
		};
		base.Toolbar.Items.Add(toolStripButton);
		base.Toolbar.Items.Add(new ToolStripSeparator());
		base.Toolbar.Items.Add(value);
		base.Toolbar.Items.Add(toolStripTextBox);
		toolStripTextBox.TextChanged += delegate(object sender, EventArgs e)
		{
			FilterProperties(((ToolStripTextBox)sender).Text);
		};
	}

	protected override void OnDisplayModeChanged(EventArgs e)
	{
		FilterProperties(base.Toolbar.Items[4].Text);
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

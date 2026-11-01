// Rewritten on the modernize branch. The IDE's property grid was VisualHint SmartPropertyGrid, a commercial .NET 2.0
// control that cannot be created on .NET 10 (its static constructor reads System.dll's resources). This one is WinForms'
// PropertyGrid with the part of SmartPropertyGrid's API the IDE used, under the same names: categories and typed
// properties added in order (AppendRootCategory / AppendManagedProperty), a tag and a "button" feel per property, nested
// properties that are filled when expanded (Lua tables), and the PropertyChanged / PropertyButtonClicked /
// PropertyExpanded events. The toolbar keeps the IDE's Refresh button and name filter.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Redbox.KioskEngine.IDE.Properties;

namespace Redbox.KioskEngine.IDE;

public class DynamicPropertyGrid : UserControl, ISupportInitialize
{
	public enum DisplayModes
	{
		Categorized,
		Alphabetical
	}

	public enum ToolTipModes
	{
		None,
		ToolTipsOnValuesAndLabels
	}

	private static readonly object ButtonFeel = new object();

	private readonly ToolStrip m_toolbar;

	private readonly ToolStripTextBox m_filterTextBox;

	private readonly PropertyGrid m_grid;

	private readonly List<GridProperty> m_categories = new List<GridProperty>();

	private int m_updateDepth;

	private bool m_raisingExpanded;

	public DynamicPropertyGrid()
	{
		m_grid = new PropertyGrid
		{
			Dock = DockStyle.Fill,
			HelpVisible = false,
			ToolbarVisible = false,
			PropertySort = PropertySort.Categorized
		};
		ToolStripButton refreshButton = new ToolStripButton(Resources.Refresh)
		{
			ToolTipText = "Refresh"
		};
		refreshButton.Click += delegate(object sender, EventArgs e)
		{
			RefreshGrid?.Invoke(sender, e);
		};
		m_filterTextBox = new ToolStripTextBox("filter")
		{
			BorderStyle = BorderStyle.None,
			Width = 350
		};
		m_filterTextBox.TextChanged += delegate
		{
			FilterProperties(m_filterTextBox.Text);
		};
		m_toolbar = new ToolStrip
		{
			Dock = DockStyle.Top,
			GripStyle = ToolStripGripStyle.Hidden
		};
		m_toolbar.Items.Add(refreshButton);
		m_toolbar.Items.Add(new ToolStripSeparator());
		m_toolbar.Items.Add(new ToolStripLabel("Filter:"));
		m_toolbar.Items.Add(m_filterTextBox);
		Controls.Add(m_grid);
		Controls.Add(m_toolbar);
	}

	public event EventHandler RefreshGrid;

	public event EventHandler<GridPropertyChangedEventArgs> PropertyChanged;

	public event EventHandler<PropertyButtonClickedEventArgs> PropertyButtonClicked;

	public event EventHandler<PropertyExpandedEventArgs> PropertyExpanded;

	public DisplayModes DisplayMode
	{
		get
		{
			return (m_grid.PropertySort != PropertySort.Alphabetical) ? DisplayModes.Categorized : DisplayModes.Alphabetical;
		}
		set
		{
			m_grid.PropertySort = (value == DisplayModes.Categorized) ? PropertySort.Categorized : PropertySort.Alphabetical;
		}
	}

	public bool ToolbarVisibility
	{
		get
		{
			return m_toolbar.Visible;
		}
		set
		{
			m_toolbar.Visible = value;
		}
	}

	public Color PropertyLabelBackColor
	{
		get
		{
			return m_grid.ViewBackColor;
		}
		set
		{
			m_grid.ViewBackColor = value;
		}
	}

	public Color PropertyValueBackColor
	{
		get
		{
			return m_grid.ViewBackColor;
		}
		set
		{
			m_grid.ViewBackColor = value;
		}
	}

	// SmartPropertyGrid display options the IDE set; WinForms' grid always indents nested properties, shows non-default
	// values in bold and has tool tips.
	public bool ShowAdditionalIndentation { get; set; }

	public bool ShowDefaultValues { get; set; }

	public ToolTipModes ToolTipMode { get; set; }

	public object GetRegisteredFeel(string name)
	{
		return (name == "button") ? ButtonFeel : null;
	}

	public void BeginUpdate()
	{
		m_updateDepth++;
	}

	public void EndUpdate()
	{
		if (m_updateDepth > 0)
		{
			m_updateDepth--;
		}
		if (m_updateDepth == 0 && !m_raisingExpanded)
		{
			Rebuild();
		}
	}

	public void Clear()
	{
		m_categories.Clear();
		RebuildUnlessUpdating();
	}

	public void ClearUnderProperty(PropertyEnumerator parent)
	{
		parent.Property.Children.Clear();
		RebuildUnlessUpdating();
	}

	public PropertyEnumerator AppendRootCategory(int id, string name)
	{
		GridProperty category = new GridProperty(id, name, typeof(string), null, string.Empty, new Attribute[0], parent: null);
		m_categories.Add(category);
		RebuildUnlessUpdating();
		return new PropertyEnumerator(category);
	}

	public PropertyEnumerator AppendManagedProperty(PropertyEnumerator parent, int id, string name, Type type, object value, string comment, params Attribute[] attributes)
	{
		GridProperty property = new GridProperty(id, name, type, value, comment, attributes ?? new Attribute[0], parent.Property);
		parent.Property.Children.Add(property);
		RebuildUnlessUpdating();
		return new PropertyEnumerator(property);
	}

	internal void OnPropertyChanged(GridProperty property)
	{
		PropertyChanged?.Invoke(this, new GridPropertyChangedEventArgs(new PropertyEnumerator(property)));
	}

	internal void OnPropertyButtonClicked(GridProperty property)
	{
		PropertyButtonClicked?.Invoke(this, new PropertyButtonClickedEventArgs(new PropertyEnumerator(property)));
	}

	// Raised the first time the grid asks for a property's children, so they can be filled in then.
	internal void OnPropertyExpanding(GridProperty property)
	{
		if (property.ExpandRaised)
		{
			return;
		}
		property.ExpandRaised = true;
		m_raisingExpanded = true;
		try
		{
			PropertyExpanded?.Invoke(this, new PropertyExpandedEventArgs(new PropertyEnumerator(property), expanded: true));
		}
		finally
		{
			m_raisingExpanded = false;
		}
	}

	void ISupportInitialize.BeginInit()
	{
	}

	void ISupportInitialize.EndInit()
	{
	}

	private void RebuildUnlessUpdating()
	{
		if (m_updateDepth == 0 && !m_raisingExpanded)
		{
			Rebuild();
		}
	}

	private void Rebuild()
	{
		m_grid.SelectedObject = new PropertyBag(this, m_categories);
	}

	private void FilterProperties(string filter)
	{
		foreach (GridProperty property in m_categories.SelectMany((GridProperty category) => category.Children))
		{
			property.Visible = string.IsNullOrEmpty(filter) || property.Name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0;
		}
		Rebuild();
	}

	// The object the WinForms grid shows: one property per visible property of every category.
	private sealed class PropertyBag : ICustomTypeDescriptor
	{
		private readonly DynamicPropertyGrid m_owner;

		private readonly List<GridProperty> m_categories;

		internal PropertyBag(DynamicPropertyGrid owner, List<GridProperty> categories)
		{
			m_owner = owner;
			m_categories = categories;
		}

		public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
		{
			return new PropertyDescriptorCollection(m_categories.SelectMany((GridProperty category) => from property in category.Children
				where property.Visible
				select new GridPropertyDescriptor(m_owner, property, category.Name)).ToArray<PropertyDescriptor>());
		}

		public PropertyDescriptorCollection GetProperties()
		{
			return GetProperties(null);
		}

		public AttributeCollection GetAttributes()
		{
			return AttributeCollection.Empty;
		}

		public string GetClassName()
		{
			return null;
		}

		public string GetComponentName()
		{
			return null;
		}

		public TypeConverter GetConverter()
		{
			return null;
		}

		public EventDescriptor GetDefaultEvent()
		{
			return null;
		}

		public PropertyDescriptor GetDefaultProperty()
		{
			return null;
		}

		public object GetEditor(Type editorBaseType)
		{
			return null;
		}

		public EventDescriptorCollection GetEvents(Attribute[] attributes)
		{
			return EventDescriptorCollection.Empty;
		}

		public EventDescriptorCollection GetEvents()
		{
			return EventDescriptorCollection.Empty;
		}

		public object GetPropertyOwner(PropertyDescriptor pd)
		{
			return this;
		}
	}

	private sealed class GridPropertyDescriptor : PropertyDescriptor
	{
		private readonly DynamicPropertyGrid m_owner;

		private readonly GridProperty m_property;

		private readonly string m_category;

		internal GridPropertyDescriptor(DynamicPropertyGrid owner, GridProperty property, string category)
			: base("p" + property.Id.ToString(CultureInfo.InvariantCulture), property.Attributes)
		{
			m_owner = owner;
			m_property = property;
			m_category = category;
		}

		public override string DisplayName => m_property.Name;

		public override string Category => m_category;

		public override string Description => m_property.Comment;

		public override Type ComponentType => typeof(PropertyBag);

		// Properties with children (Lua tables) or a button are shown, not edited, as in the IDE before.
		public override bool IsReadOnly => m_property.HasChildren || m_property.Feel == ButtonFeel || m_property.Attributes.OfType<ReadOnlyAttribute>().Any((ReadOnlyAttribute a) => a.IsReadOnly);

		public override Type PropertyType => m_property.HasChildren ? typeof(GridProperty) : m_property.Type;

		public override TypeConverter Converter => m_property.HasChildren ? new NodeConverter(m_owner) : TypeDescriptor.GetConverter(m_property.Type);

		public override object GetValue(object component)
		{
			return m_property.HasChildren ? m_property : m_property.Value.GetValue();
		}

		public override void SetValue(object component, object value)
		{
			m_property.Value.SetValue(value);
			m_owner.OnPropertyChanged(m_property);
		}

		public override object GetEditor(Type editorBaseType)
		{
			if (m_property.Feel == ButtonFeel && editorBaseType == typeof(UITypeEditor))
			{
				return new ButtonEditor(m_owner, m_property);
			}
			return base.GetEditor(editorBaseType);
		}

		public override bool CanResetValue(object component)
		{
			return !IsReadOnly && m_property.DefaultValue != null && !Equals(m_property.DefaultValue.Value, m_property.Value.GetValue());
		}

		public override void ResetValue(object component)
		{
			SetValue(component, m_property.DefaultValue?.Value);
		}

		public override bool ShouldSerializeValue(object component)
		{
			return m_property.DefaultValue == null || !Equals(m_property.DefaultValue.Value, m_property.Value.GetValue());
		}
	}

	// A property with children: shown with its own value, expandable to its children.
	private sealed class NodeConverter : ExpandableObjectConverter
	{
		private readonly DynamicPropertyGrid m_owner;

		internal NodeConverter(DynamicPropertyGrid owner)
		{
			m_owner = owner;
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType == typeof(string) && value is GridProperty node)
			{
				return Convert.ToString(node.Value.GetValue(), culture) ?? string.Empty;
			}
			return base.ConvertTo(context, culture, value, destinationType);
		}

		public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
		{
			if (!(value is GridProperty node))
			{
				return base.GetProperties(context, value, attributes);
			}
			m_owner.OnPropertyExpanding(node);
			return new PropertyDescriptorCollection((from child in node.Children
				where child.Visible && !child.IsPlaceholder
				select new GridPropertyDescriptor(m_owner, child, null)).ToArray<PropertyDescriptor>());
		}
	}

	// SmartPropertyGrid's "button" feel: a "..." button that raises PropertyButtonClicked.
	private sealed class ButtonEditor : UITypeEditor
	{
		private readonly DynamicPropertyGrid m_owner;

		private readonly GridProperty m_property;

		internal ButtonEditor(DynamicPropertyGrid owner, GridProperty property)
		{
			m_owner = owner;
			m_property = property;
		}

		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
		{
			return UITypeEditorEditStyle.Modal;
		}

		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			m_owner.OnPropertyButtonClicked(m_property);
			return value;
		}
	}
}

// A category or property of a DynamicPropertyGrid (SmartPropertyGrid's Property).
public sealed class GridProperty
{
	internal GridProperty(int id, string name, Type type, object value, string comment, Attribute[] attributes, GridProperty parent)
	{
		Id = id;
		Name = name;
		Type = type;
		Comment = comment;
		Attributes = attributes;
		Parent = parent;
		Value = new GridPropertyValue(value);
		DefaultValue = attributes.OfType<DefaultValueAttribute>().FirstOrDefault();
	}

	public int Id { get; }

	public string Name { get; }

	public string DisplayName => Name;

	public Type Type { get; }

	public string Comment { get; }

	public object Tag { get; set; }

	public object Feel { get; set; }

	public bool Visible { get; set; } = true;

	public GridPropertyValue Value { get; }

	public GridProperty Parent { get; }

	internal Attribute[] Attributes { get; }

	internal DefaultValueAttribute DefaultValue { get; }

	internal List<GridProperty> Children { get; } = new List<GridProperty>();

	internal bool HasChildren => Children.Count > 0;

	internal bool ExpandRaised { get; set; }

	// The IDE adds a "hidden" child to a Lua table so that it shows as expandable before its entries are read.
	internal bool IsPlaceholder => Name == "hidden" && Value.GetValue() == null;

	public override string ToString()
	{
		return Name;
	}
}

public sealed class GridPropertyValue
{
	private object m_value;

	internal GridPropertyValue(object value)
	{
		m_value = value;
	}

	public object GetValue()
	{
		return m_value;
	}

	public void SetValue(object value)
	{
		m_value = value;
	}
}

// SmartPropertyGrid's handle on a property in the grid.
public sealed class PropertyEnumerator
{
	internal PropertyEnumerator(GridProperty property)
	{
		Property = property;
	}

	public GridProperty Property { get; }
}

public sealed class GridPropertyChangedEventArgs : EventArgs
{
	internal GridPropertyChangedEventArgs(PropertyEnumerator propertyEnum)
	{
		PropertyEnum = propertyEnum;
	}

	public PropertyEnumerator PropertyEnum { get; }
}

public sealed class PropertyButtonClickedEventArgs : EventArgs
{
	internal PropertyButtonClickedEventArgs(PropertyEnumerator propertyEnum)
	{
		PropertyEnum = propertyEnum;
	}

	public PropertyEnumerator PropertyEnum { get; }
}

public sealed class PropertyExpandedEventArgs : EventArgs
{
	internal PropertyExpandedEventArgs(PropertyEnumerator propertyEnum, bool expanded)
	{
		PropertyEnum = propertyEnum;
		Expanded = expanded;
	}

	public PropertyEnumerator PropertyEnum { get; }

	public bool Expanded { get; }
}

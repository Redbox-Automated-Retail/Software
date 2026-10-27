using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.Lua;
using VisualHint.SmartPropertyGrid;
using WeifenLuo.WinFormsUI.Docking;

namespace Redbox.KioskEngine.IDE;

public class SymbolsToolWindow : ToolWindow
{
	private IContainer components;

	private DynamicPropertyGrid m_dynamicPropertyGrid;

	public event EventHandler RefreshRequested;

	public SymbolsToolWindow()
	{
		InitializeComponent();
		m_dynamicPropertyGrid.PropertyButtonClicked += delegate(object sender, PropertyButtonClickedEventArgs e)
		{
			if (e.PropertyEnum.Property.Tag is LuaTable luaTable)
			{
				DebuggerVariableForm debuggerVariableForm = new DebuggerVariableForm();
				debuggerVariableForm.VariableName = e.PropertyEnum.Property.Name;
				debuggerVariableForm.VariableValue = luaTable.ToString();
				debuggerVariableForm.ShowDialog();
			}
			else
			{
				_ = e.PropertyEnum.Property.Tag is LuaFunction;
			}
		};
		m_dynamicPropertyGrid.PropertyExpanded += delegate(object sender, PropertyExpandedEventArgs e)
		{
			if (e.Expanded)
			{
				if (m_dynamicPropertyGrid.DisplayMode != VisualHint.SmartPropertyGrid.PropertyGrid.DisplayModes.Categorized)
				{
					m_dynamicPropertyGrid.DisplayMode = VisualHint.SmartPropertyGrid.PropertyGrid.DisplayModes.Categorized;
				}
				else if (e.PropertyEnum.Property.Tag is LuaTable table)
				{
					try
					{
						Cursor = Cursors.WaitCursor;
						m_dynamicPropertyGrid.SuspendLayout();
						m_dynamicPropertyGrid.BeginUpdate();
						m_dynamicPropertyGrid.ClearUnderProperty(e.PropertyEnum);
						BuildNestedTableProperties(m_dynamicPropertyGrid, e.PropertyEnum, e.PropertyEnum.Property.Id + 1000, table, new List<LuaTable>());
					}
					finally
					{
						Cursor = Cursors.Default;
						m_dynamicPropertyGrid.EndUpdate();
						m_dynamicPropertyGrid.ResumeLayout();
					}
				}
			}
		};
	}

	public void RefreshSymbols()
	{
		OnRefresh(this, EventArgs.Empty);
	}

	public void ShowSymbols(string categoryName, IEnumerable<LuaVar> vars)
	{
		RefreshVariableGrid(m_dynamicPropertyGrid, categoryName, vars);
	}

	protected override string GetPersistString()
	{
		return GetType()?.ToString() + "," + Text;
	}

	private void OnRefresh(object sender, EventArgs e)
	{
		if (RefreshRequested != null)
		{
			RefreshRequested(sender, e);
		}
	}

	private static object GetAdjustedValue(object var, out Type type, out bool isReadOnly)
	{
		isReadOnly = false;
		object obj = "nil";
		if (var != null)
		{
			obj = var;
		}
		type = obj.GetType();
		if (type == typeof(LuaTable))
		{
			obj = "(table)";
			type = typeof(string);
			isReadOnly = true;
		}
		else if (type == typeof(LuaFunction))
		{
			obj = "(function)";
			type = typeof(string);
			isReadOnly = true;
		}
		return obj;
	}

	private void RefreshVariableGrid(VisualHint.SmartPropertyGrid.PropertyGrid grid, string category, IEnumerable<LuaVar> vars)
	{
		try
		{
			Cursor = Cursors.WaitCursor;
			grid.SuspendLayout();
			grid.BeginUpdate();
			grid.Clear();
			grid.DisplayMode = VisualHint.SmartPropertyGrid.PropertyGrid.DisplayModes.Categorized;
			PropertyEnumerator underCategory = grid.AppendRootCategory(1, category);
			int num = 2;
			foreach (LuaVar var in vars)
			{
				if (!(var.Name == "package"))
				{
					object adjustedValue = GetAdjustedValue(var.Value, out var type, out var _);
					List<Attribute> list = new List<Attribute>
					{
						new DefaultValueAttribute(adjustedValue)
					};
					LuaTable obj = var.Value as LuaTable;
					PropertyEnumerator propertyEnumerator = grid.AppendManagedProperty(underCategory, num++, var.Name, type, adjustedValue, string.Empty, list.ToArray());
					if (obj != null)
					{
						propertyEnumerator.Property.Tag = var.Value;
						propertyEnumerator.Property.Feel = grid.GetRegisteredFeel("button");
						grid.AppendManagedProperty(propertyEnumerator, num++, "hidden", typeof(string), null, string.Empty);
					}
					if (var.Value is LuaFunction)
					{
						propertyEnumerator.Property.Tag = var.Value;
						propertyEnumerator.Property.Feel = grid.GetRegisteredFeel("button");
					}
				}
			}
			grid.EndUpdate();
			grid.ResumeLayout();
		}
		finally
		{
			Cursor = Cursors.Default;
		}
	}

	private static void BuildNestedTableProperties(VisualHint.SmartPropertyGrid.PropertyGrid grid, PropertyEnumerator parentEnumerator, int id, LuaTable table, ICollection<LuaTable> visited)
	{
		if (visited.Contains(table))
		{
			return;
		}
		visited.Add(table);
		foreach (object key in table.Keys)
		{
			object adjustedValue = GetAdjustedValue(table[key], out var type, out var _);
			List<Attribute> list = new List<Attribute>
			{
				new DefaultValueAttribute(adjustedValue)
			};
			PropertyEnumerator propertyEnumerator = grid.AppendManagedProperty(parentEnumerator, id++, key.ToString(), type, adjustedValue, key.ToString(), list.ToArray());
			if (table[key] is LuaTable tag)
			{
				propertyEnumerator.Property.Tag = tag;
				propertyEnumerator.Property.Feel = grid.GetRegisteredFeel("button");
				grid.AppendManagedProperty(propertyEnumerator, id++, "hidden", typeof(string), null, string.Empty);
			}
			if (table[key] is LuaFunction tag2)
			{
				propertyEnumerator.Property.Tag = tag2;
				propertyEnumerator.Property.Feel = grid.GetRegisteredFeel("button");
			}
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Redbox.KioskEngine.IDE.SymbolsToolWindow));
		this.m_dynamicPropertyGrid = new Redbox.KioskEngine.IDE.DynamicPropertyGrid();
		((System.ComponentModel.ISupportInitialize)this.m_dynamicPropertyGrid).BeginInit();
		base.SuspendLayout();
		this.m_dynamicPropertyGrid.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.m_dynamicPropertyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
		this.m_dynamicPropertyGrid.Location = new System.Drawing.Point(0, 0);
		this.m_dynamicPropertyGrid.Name = "m_dynamicPropertyGrid";
		this.m_dynamicPropertyGrid.PropertyLabelBackColor = System.Drawing.SystemColors.Window;
		this.m_dynamicPropertyGrid.PropertyValueBackColor = System.Drawing.SystemColors.Window;
		this.m_dynamicPropertyGrid.ShowAdditionalIndentation = true;
		this.m_dynamicPropertyGrid.ShowDefaultValues = true;
		this.m_dynamicPropertyGrid.Size = new System.Drawing.Size(398, 318);
		this.m_dynamicPropertyGrid.TabIndex = 0;
		this.m_dynamicPropertyGrid.Text = "m_dynamicPropertyGrid";
		this.m_dynamicPropertyGrid.ToolbarVisibility = true;
		this.m_dynamicPropertyGrid.ToolTipMode = VisualHint.SmartPropertyGrid.PropertyGrid.ToolTipModes.ToolTipsOnValuesAndLabels;
		this.m_dynamicPropertyGrid.RefreshGrid += new System.EventHandler(OnRefresh);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(398, 318);
		base.Controls.Add(this.m_dynamicPropertyGrid);
		this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		base.HideOnClose = true;
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		base.Name = "SymbolsToolWindow";
		base.ShowHint = WeifenLuo.WinFormsUI.Docking.DockState.DockBottom;
		this.Text = "Symbols";
		((System.ComponentModel.ISupportInitialize)this.m_dynamicPropertyGrid).EndInit();
		base.ResumeLayout(false);
	}
}

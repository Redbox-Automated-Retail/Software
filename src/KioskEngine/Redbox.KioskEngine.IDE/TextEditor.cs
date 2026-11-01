using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Skybound.VisualTips;
using Skybound.VisualTips.Rendering;

namespace Redbox.KioskEngine.IDE;

public class TextEditor : UserControl
{
	private IContainer components;

	private CodeEditor m_codeEditor;

	private VisualTipProvider m_visualTipProvider;

	public TextEditor()
	{
		InitializeComponent();
	}

	public void SetDocumentText(string text)
	{
		m_codeEditor.Text = text;
	}

	public void SetSyntaxResource(string name)
	{
		m_codeEditor.SetSyntaxResource(name);
	}

	private void ShowCurrentLineSymbol()
	{
		m_codeEditor.SetCurrentLineMarker(m_codeEditor.CaretLine, on: true);
	}

	private void ClearCurrentLineSymbol()
	{
		m_codeEditor.SetCurrentLineMarker(m_codeEditor.CaretLine, on: false);
	}

	public void SetCurrentDebugLine(int lineNumber, string error)
	{
		if (lineNumber < m_codeEditor.LineCount)
		{
			ClearCurrentLineSymbol();
			int lineIndex = lineNumber - 1;
			if (lineIndex < 0)
			{
				lineIndex = 0;
			}
			m_codeEditor.GotoLine(lineIndex);
			int firstVisibleLine = lineIndex - 5;
			if (firstVisibleLine < 0)
			{
				firstVisibleLine = 0;
			}
			m_codeEditor.ScrollIntoView(firstVisibleLine);
			ShowCurrentLineSymbol();
		}
		if (!string.IsNullOrEmpty(error) && string.Compare(error, "breakpoint hit", ignoreCase: true) != 0)
		{
			VisualTip tip = new VisualTip(error, "REDS Engine Runtime Error")
			{
				FooterText = $"The error occurred on or near line: {lineNumber}."
			};
			Point point = m_codeEditor.PointToScreen(m_codeEditor.Cursor.HotSpot);
			m_visualTipProvider.ShowTip(tip, new Rectangle(point.X + 250, point.Y, 100, 100));
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
		this.components = new System.ComponentModel.Container();
		Skybound.VisualTips.Rendering.VisualTipOfficeRenderer renderer = new Skybound.VisualTips.Rendering.VisualTipOfficeRenderer();
		this.m_codeEditor = new Redbox.KioskEngine.IDE.CodeEditor();
		this.m_visualTipProvider = new Skybound.VisualTips.VisualTipProvider(this.components);
		base.SuspendLayout();
		this.m_codeEditor.BorderStyle = ScintillaNET.BorderStyle.None;
		this.m_codeEditor.Dock = System.Windows.Forms.DockStyle.Fill;
		this.m_codeEditor.Location = new System.Drawing.Point(0, 0);
		this.m_codeEditor.Name = "m_codeEditor";
		this.m_codeEditor.Size = new System.Drawing.Size(590, 501);
		this.m_codeEditor.TabIndex = 0;
		this.m_visualTipProvider.Animation = Skybound.VisualTips.VisualTipAnimation.Enabled;
		this.m_visualTipProvider.DisplayAtMousePosition = false;
		this.m_visualTipProvider.DisplayMode = Skybound.VisualTips.VisualTipDisplayMode.Manual;
		this.m_visualTipProvider.Renderer = renderer;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.m_codeEditor);
		base.Name = "TextEditor";
		base.Size = new System.Drawing.Size(590, 501);
		base.ResumeLayout(false);
	}
}

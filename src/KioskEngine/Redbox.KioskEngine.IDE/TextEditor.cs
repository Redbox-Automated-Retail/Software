using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Alsing.SourceCode;
using Alsing.Windows.Forms;
using Skybound.VisualTips;
using Skybound.VisualTips.Rendering;

namespace Redbox.KioskEngine.IDE;

public class TextEditor : UserControl
{
	private IContainer components;

	private SyntaxBoxControl m_syntaxBoxControl;

	private VisualTipProvider m_visualTipProvider;

	private SyntaxDocument m_syntaxDocument;

	private ImageList m_gutterImageList;

	public TextEditor()
	{
		InitializeComponent();
		m_syntaxBoxControl.GutterIcons.Images.Add(m_gutterImageList.Images[0]);
	}

	public void SetDocumentText(string text)
	{
		m_syntaxDocument.Text = text;
	}

	public void SetSyntaxResource(string name)
	{
		m_syntaxDocument.SetSyntaxFromEmbeddedResource(typeof(DebuggerForm).Assembly, $"Redbox.KioskEngine.IDE.{name}.syn");
	}

	private void ShowCurrentLineSymbol()
	{
		m_syntaxBoxControl.Document[m_syntaxBoxControl.Caret.Position.Y].Images.Add(2);
	}

	private void ClearCurrentLineSymbol()
	{
		m_syntaxBoxControl.Document[m_syntaxBoxControl.Caret.Position.Y].Images.Remove(2);
	}

	public void SetCurrentDebugLine(int lineNumber, string error)
	{
		if (lineNumber < m_syntaxDocument.Lines.Length)
		{
			ClearCurrentLineSymbol();
			int num = lineNumber - 1;
			if (num < 0)
			{
				num = 0;
			}
			m_syntaxBoxControl.GotoLine(num);
			int num2 = num - 5;
			if (num2 < 0)
			{
				num2 = 0;
			}
			m_syntaxBoxControl.ScrollIntoView(num2);
			ShowCurrentLineSymbol();
		}
		if (!string.IsNullOrEmpty(error) && string.Compare(error, "breakpoint hit", ignoreCase: true) != 0)
		{
			VisualTip tip = new VisualTip(error, "REDS Engine Runtime Error")
			{
				FooterText = $"The error occurred on or near line: {lineNumber}."
			};
			Point point = m_syntaxBoxControl.PointToScreen(m_syntaxBoxControl.Cursor.HotSpot);
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Redbox.KioskEngine.IDE.TextEditor));
		Skybound.VisualTips.Rendering.VisualTipOfficeRenderer renderer = new Skybound.VisualTips.Rendering.VisualTipOfficeRenderer();
		this.m_syntaxBoxControl = new Alsing.Windows.Forms.SyntaxBoxControl();
		this.m_visualTipProvider = new Skybound.VisualTips.VisualTipProvider(this.components);
		this.m_syntaxDocument = new Alsing.SourceCode.SyntaxDocument(this.components);
		this.m_gutterImageList = new System.Windows.Forms.ImageList(this.components);
		base.SuspendLayout();
		this.m_syntaxBoxControl.ActiveView = Alsing.Windows.Forms.ActiveView.BottomRight;
		this.m_syntaxBoxControl.AutoListPosition = null;
		this.m_syntaxBoxControl.AutoListSelectedText = "a123";
		this.m_syntaxBoxControl.AutoListVisible = false;
		this.m_syntaxBoxControl.BackColor = System.Drawing.Color.White;
		this.m_syntaxBoxControl.BorderStyle = Alsing.Windows.Forms.BorderStyle.None;
		this.m_syntaxBoxControl.CopyAsRTF = false;
		this.m_syntaxBoxControl.Dock = System.Windows.Forms.DockStyle.Fill;
		this.m_syntaxBoxControl.Document = this.m_syntaxDocument;
		this.m_syntaxBoxControl.FontName = "Courier new";
		this.m_syntaxBoxControl.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.m_syntaxBoxControl.InfoTipCount = 1;
		this.m_syntaxBoxControl.InfoTipPosition = null;
		this.m_syntaxBoxControl.InfoTipSelectedIndex = 1;
		this.m_syntaxBoxControl.InfoTipVisible = false;
		this.m_syntaxBoxControl.Location = new System.Drawing.Point(0, 0);
		this.m_syntaxBoxControl.LockCursorUpdate = false;
		this.m_syntaxBoxControl.Name = "m_syntaxBoxControl";
		this.m_syntaxBoxControl.ShowScopeIndicator = false;
		this.m_syntaxBoxControl.Size = new System.Drawing.Size(590, 501);
		this.m_syntaxBoxControl.SmoothScroll = false;
		this.m_syntaxBoxControl.SplitviewH = -4;
		this.m_syntaxBoxControl.SplitviewV = -4;
		this.m_syntaxBoxControl.TabGuideColor = System.Drawing.Color.FromArgb(222, 219, 214);
		this.m_syntaxBoxControl.TabIndex = 0;
		this.m_syntaxBoxControl.Text = "syntaxBoxControl1";
		this.m_syntaxBoxControl.WhitespaceColor = System.Drawing.SystemColors.ControlDark;
		this.m_syntaxDocument.Lines = new string[1] { "" };
		this.m_syntaxDocument.MaxUndoBufferSize = 1000;
		this.m_syntaxDocument.Modified = false;
		this.m_syntaxDocument.UndoStep = 0;
		this.m_visualTipProvider.Animation = Skybound.VisualTips.VisualTipAnimation.Enabled;
		this.m_visualTipProvider.DisplayAtMousePosition = false;
		this.m_visualTipProvider.DisplayMode = Skybound.VisualTips.VisualTipDisplayMode.Manual;
		this.m_visualTipProvider.Renderer = renderer;
		this.m_gutterImageList.ImageStream = (System.Windows.Forms.ImageListStreamer)resources.GetObject("m_gutterImageList.ImageStream");
		this.m_gutterImageList.TransparentColor = System.Drawing.Color.Transparent;
		this.m_gutterImageList.Images.SetKeyName(0, "breakpoint-disabled.png");
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.m_syntaxBoxControl);
		base.Name = "TextEditor";
		base.Size = new System.Drawing.Size(590, 501);
		base.ResumeLayout(false);
	}
}

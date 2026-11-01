// Written for the modernize branch (not decompiled). The find/replace and go-to-line dialogs of CodeEditor (SyntaxBox
// brought its own).
using System;
using System.Drawing;
using System.Windows.Forms;
using ScintillaNET;

namespace Redbox.KioskEngine.IDE;

internal sealed class FindReplaceForm : Form
{
	private readonly CodeEditor m_editor;

	private readonly TextBox m_findTextBox = new TextBox();

	private readonly TextBox m_replaceTextBox = new TextBox();

	private readonly Label m_replaceLabel = new Label { Text = "Replace with:", AutoSize = true };

	private readonly CheckBox m_matchCaseCheckBox = new CheckBox { Text = "Match case", AutoSize = true };

	private readonly CheckBox m_wholeWordCheckBox = new CheckBox { Text = "Whole word", AutoSize = true };

	private readonly Button m_findNextButton = new Button { Text = "Find Next" };

	private readonly Button m_replaceButton = new Button { Text = "Replace" };

	private readonly Button m_replaceAllButton = new Button { Text = "Replace All" };

	private readonly Label m_statusLabel = new Label { AutoSize = true, ForeColor = SystemColors.GrayText };

	internal FindReplaceForm(CodeEditor editor)
	{
		m_editor = editor;
		Text = "Find and Replace";
		FormBorderStyle = FormBorderStyle.FixedToolWindow;
		StartPosition = FormStartPosition.CenterParent;
		ShowInTaskbar = false;
		KeyPreview = true;
		ClientSize = new Size(420, 150);
		Label findLabel = new Label { Text = "Find what:", AutoSize = true, Location = new Point(10, 14) };
		m_findTextBox.SetBounds(100, 10, 210, 22);
		m_replaceLabel.Location = new Point(10, 44);
		m_replaceTextBox.SetBounds(100, 40, 210, 22);
		m_matchCaseCheckBox.Location = new Point(100, 72);
		m_wholeWordCheckBox.Location = new Point(200, 72);
		m_findNextButton.SetBounds(320, 9, 90, 24);
		m_replaceButton.SetBounds(320, 39, 90, 24);
		m_replaceAllButton.SetBounds(320, 69, 90, 24);
		m_statusLabel.Location = new Point(10, 120);
		m_findNextButton.Click += delegate
		{
			Report(m_editor.FindNext(m_findTextBox.Text, Flags, fromStart: false) ? string.Empty : "Not found.");
		};
		m_replaceButton.Click += delegate
		{
			m_editor.ReplaceSelection(m_findTextBox.Text, m_replaceTextBox.Text, Flags);
		};
		m_replaceAllButton.Click += delegate
		{
			Report($"{m_editor.ReplaceAll(m_findTextBox.Text, m_replaceTextBox.Text, Flags)} replaced.");
		};
		Controls.AddRange(new Control[] { findLabel, m_findTextBox, m_replaceLabel, m_replaceTextBox, m_matchCaseCheckBox, m_wholeWordCheckBox, m_findNextButton, m_replaceButton, m_replaceAllButton, m_statusLabel });
		AcceptButton = m_findNextButton;
	}

	private SearchFlags Flags => (m_matchCaseCheckBox.Checked ? SearchFlags.MatchCase : SearchFlags.None) | (m_wholeWordCheckBox.Checked ? SearchFlags.WholeWord : SearchFlags.None);

	internal void ShowFor(bool replace, string text)
	{
		m_replaceLabel.Visible = replace;
		m_replaceTextBox.Visible = replace;
		m_replaceButton.Visible = replace;
		m_replaceAllButton.Visible = replace;
		Text = replace ? "Replace" : "Find";
		if (!string.IsNullOrEmpty(text))
		{
			m_findTextBox.Text = text;
		}
		Report(string.Empty);
		if (!Visible)
		{
			Show(m_editor.FindForm());
		}
		Activate();
		m_findTextBox.SelectAll();
		m_findTextBox.Focus();
	}

	protected override void OnKeyDown(KeyEventArgs e)
	{
		base.OnKeyDown(e);
		if (e.KeyCode == Keys.Escape)
		{
			Hide();
		}
	}

	protected override void OnFormClosing(FormClosingEventArgs e)
	{
		// Kept for the next Find/Replace, like SyntaxBox's dialog.
		if (e.CloseReason == CloseReason.UserClosing)
		{
			e.Cancel = true;
			Hide();
		}
		base.OnFormClosing(e);
	}

	private void Report(string message)
	{
		m_statusLabel.Text = message;
	}
}

internal sealed class GoToLineForm : Form
{
	private readonly NumericUpDown m_lineNumber = new NumericUpDown();

	internal GoToLineForm(int lineCount, int currentLine)
	{
		Text = "Go To Line";
		FormBorderStyle = FormBorderStyle.FixedDialog;
		StartPosition = FormStartPosition.CenterParent;
		MinimizeBox = false;
		MaximizeBox = false;
		ShowInTaskbar = false;
		ClientSize = new Size(260, 90);
		Label label = new Label { Text = $"Line number (1 - {Math.Max(1, lineCount)}):", AutoSize = true, Location = new Point(10, 12) };
		m_lineNumber.SetBounds(10, 32, 240, 22);
		m_lineNumber.Minimum = 1;
		m_lineNumber.Maximum = Math.Max(1, lineCount);
		m_lineNumber.Value = Math.Max(1, Math.Min(currentLine, lineCount));
		Button ok = new Button { Text = "OK", DialogResult = DialogResult.OK };
		ok.SetBounds(94, 60, 75, 24);
		Button cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel };
		cancel.SetBounds(175, 60, 75, 24);
		Controls.AddRange(new Control[] { label, m_lineNumber, ok, cancel });
		AcceptButton = ok;
		CancelButton = cancel;
	}

	internal int LineNumber => (int)m_lineNumber.Value;
}

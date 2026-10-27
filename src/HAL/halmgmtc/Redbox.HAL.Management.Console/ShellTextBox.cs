using System;
using System.Drawing;
using System.Windows.Forms;

namespace Redbox.HAL.Management.Console;

public class ShellTextBox : TextBox
{
	protected const int WM_PASTE = 770;

	protected const int WM_CUT = 768;

	protected const int WM_SETTEXT = 12;

	protected const int WM_CLEAR = 771;

	private string m_prompt = "> ";

	private readonly CommandHistory m_commandHistory = new CommandHistory();

	public string Prompt
	{
		get
		{
			return m_prompt;
		}
		set
		{
			SetPromptText(value);
		}
	}

	public ShellTextBox()
	{
		Initialize();
	}

	public void Reset()
	{
		m_commandHistory.Clear();
		Clear();
		PrintPrompt();
	}

	protected override void WndProc(ref Message m)
	{
		switch (m.Msg)
		{
		case 12:
		case 768:
		case 770:
			if (!IsCaretAtWritablePosition())
			{
				MoveCaretToEndOfText();
			}
			break;
		case 771:
			return;
		}
		base.WndProc(ref m);
	}

	private void Initialize()
	{
		Dock = DockStyle.Fill;
		MaxLength = 0;
		Multiline = true;
		base.AcceptsTab = true;
		base.AcceptsReturn = true;
		base.ScrollBars = ScrollBars.Both;
		base.Size = new Size(400, 176);
		base.TabIndex = 0;
		Text = string.Empty;
		base.KeyPress += OnKeyPress;
		base.KeyDown += OnKeyDown;
		base.Name = "ShellTextBox";
		base.Size = new Size(400, 176);
		PrintPrompt();
	}

	private void PrintPrompt()
	{
		string text = Text;
		if (text.Length != 0)
		{
			if (text[text.Length - 1] != '\n')
			{
				PrintLine();
			}
		}
		AddText(m_prompt);
	}

	private void PrintLine()
	{
		AddText(Environment.NewLine);
	}

	private void OnKeyPress(object sender, KeyPressEventArgs e)
	{
		if (e.KeyChar == '\b' && IsCaretJustBeforePrompt())
		{
			e.Handled = true;
		}
		else if (IsTerminatorKey(e.KeyChar))
		{
			e.Handled = true;
			string textAtPrompt = GetTextAtPrompt();
			if (textAtPrompt.Length != 0)
			{
				PrintLine();
				((ShellControl)base.Parent).FireCommandEntered(textAtPrompt);
				m_commandHistory.Add(textAtPrompt);
			}
			PrintPrompt();
		}
	}

	private void OnKeyDown(object sender, KeyEventArgs e)
	{
		if (!IsCaretAtWritablePosition() && !e.Control && !IsTerminatorKey(e.KeyCode))
		{
			MoveCaretToEndOfText();
		}
		if (e.KeyCode == Keys.Left && IsCaretJustBeforePrompt())
		{
			e.Handled = true;
		}
		else if (e.KeyCode == Keys.Down)
		{
			if (m_commandHistory.DoesNextCommandExist())
			{
				ReplaceTextAtPrompt(m_commandHistory.GetNextCommand());
			}
			e.Handled = true;
		}
		else if (e.KeyCode == Keys.Up)
		{
			if (m_commandHistory.DoesPreviousCommandExist())
			{
				ReplaceTextAtPrompt(m_commandHistory.GetPreviousCommand());
			}
			e.Handled = true;
		}
		else if (e.KeyCode == Keys.Right)
		{
			string textAtPrompt = GetTextAtPrompt();
			string lastCommand = m_commandHistory.LastCommand;
			if (lastCommand != null && (textAtPrompt.Length == 0 || lastCommand.StartsWith(textAtPrompt)) && lastCommand.Length > textAtPrompt.Length)
			{
				AddText(lastCommand[textAtPrompt.Length].ToString());
			}
		}
	}

	private string GetCurrentLine()
	{
		if (base.Lines.Length != 0)
		{
			return (string)base.Lines.GetValue(base.Lines.GetLength(0) - 1);
		}
		return string.Empty;
	}

	private string GetTextAtPrompt()
	{
		return GetCurrentLine().Substring(m_prompt.Length);
	}

	private void ReplaceTextAtPrompt(string text)
	{
		int num = GetCurrentLine().Length - m_prompt.Length;
		if (num == 0)
		{
			AddText(text);
			return;
		}
		Select(TextLength - num, num);
		SelectedText = text;
	}

	private bool IsCaretAtCurrentLine()
	{
		return TextLength - base.SelectionStart <= GetCurrentLine().Length;
	}

	private void MoveCaretToEndOfText()
	{
		base.SelectionStart = TextLength;
		ScrollToCaret();
	}

	private bool IsCaretJustBeforePrompt()
	{
		if (IsCaretAtCurrentLine())
		{
			return GetCurrentCaretColumnPosition() == m_prompt.Length;
		}
		return false;
	}

	private int GetCurrentCaretColumnPosition()
	{
		string currentLine = GetCurrentLine();
		return base.SelectionStart - TextLength + currentLine.Length;
	}

	private bool IsCaretAtWritablePosition()
	{
		if (IsCaretAtCurrentLine())
		{
			return GetCurrentCaretColumnPosition() >= m_prompt.Length;
		}
		return false;
	}

	private void SetPromptText(string val)
	{
		Select(0, m_prompt.Length);
		SelectedText = val;
		m_prompt = val;
	}

	public string[] GetCommandHistory()
	{
		return m_commandHistory.GetCommandHistory();
	}

	public void WriteText(string text)
	{
		AddText(text);
	}

	private static bool IsTerminatorKey(Keys key)
	{
		return key == Keys.Return;
	}

	private static bool IsTerminatorKey(char keyChar)
	{
		return keyChar == '\r';
	}

	private void AddText(string text)
	{
		Text += text;
		MoveCaretToEndOfText();
	}
}

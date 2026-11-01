// Written for the modernize branch (not decompiled).
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using ScintillaNET;

namespace Redbox.KioskEngine.IDE;

// The IDE's code editor: Scintilla (Scintilla5.NET) with what the IDE used of Puzzle.SyntaxBox, a .NET 3.5 control
// whose own resources needed the BinaryFormatter on .NET 10: syntax colouring chosen by the IDE's .syn resources,
// breakpoints in a gutter (clicked or set from code, with the events the debugger listens to), the debugger's line
// markers, find/replace/go-to dialogs and the whitespace/end-of-line views.
public class CodeEditor : Scintilla
{
	private const int BreakpointMargin = 1;

	private const int BreakpointMarker = 1;

	private const int DisabledBreakpointMarker = 2;

	private const int CurrentLineMarker = 3;

	private bool m_allowBreakpoints;

	private bool m_settingText;

	private FindReplaceForm m_findReplaceForm;

	public CodeEditor()
	{
		StyleResetDefault();
		Styles[Style.Default].Font = "Courier New";
		Styles[Style.Default].Size = 10;
		StyleClearAll();
		Margins[0].Type = MarginType.Number;
		Margins[0].Width = 36;
		Margins[BreakpointMargin].Type = MarginType.Symbol;
		Margins[BreakpointMargin].Mask = (1u << BreakpointMarker) | (1u << DisabledBreakpointMarker) | (1u << CurrentLineMarker);
		Margins[BreakpointMargin].Width = 16;
		Margins[BreakpointMargin].Sensitive = true;
		Markers[BreakpointMarker].Symbol = MarkerSymbol.Circle;
		Markers[BreakpointMarker].SetBackColor(Color.FromArgb(171, 0, 0));
		Markers[BreakpointMarker].SetForeColor(Color.FromArgb(171, 0, 0));
		Markers[DisabledBreakpointMarker].Symbol = MarkerSymbol.Circle;
		Markers[DisabledBreakpointMarker].SetBackColor(Color.White);
		Markers[DisabledBreakpointMarker].SetForeColor(Color.FromArgb(171, 0, 0));
		Markers[CurrentLineMarker].Symbol = MarkerSymbol.ShortArrow;
		Markers[CurrentLineMarker].SetBackColor(Color.Yellow);
		Markers[CurrentLineMarker].SetForeColor(Color.Black);
		TabWidth = 4;
	}

	// Raised when a breakpoint is set or cleared on a line (0-based), by a gutter click or through SetBreakpoint.
	public event EventHandler<int> BreakpointAdded;

	public event EventHandler<int> BreakpointRemoved;

	// SyntaxBox's CaretChange: the caret moved to another position.
	public event EventHandler CaretChange;

	// SyntaxBox's ModifiedChanged: the text was edited after it was last set.
	public event EventHandler ModifiedChanged;

	public bool AllowBreakPoints
	{
		get
		{
			return m_allowBreakpoints;
		}
		set
		{
			m_allowBreakpoints = value;
		}
	}

	public bool ShowWhitespace
	{
		get
		{
			return ViewWhitespace != WhitespaceMode.Invisible;
		}
		set
		{
			ViewWhitespace = value ? WhitespaceMode.VisibleAlways : WhitespaceMode.Invisible;
		}
	}

	public bool ShowEOLMarker
	{
		get
		{
			return ViewEol;
		}
		set
		{
			ViewEol = value;
		}
	}

	// The breakpoint/marker column.
	public bool ShowGutterMargin
	{
		get
		{
			return Margins[BreakpointMargin].Width > 0;
		}
		set
		{
			Margins[BreakpointMargin].Width = value ? 16 : 0;
		}
	}

	public bool TabsToSpaces
	{
		get
		{
			return !UseTabs;
		}
		set
		{
			UseTabs = !value;
		}
	}

	// The 0-based line of the caret.
	public int CaretLine => CurrentLine;

	public int LineCount => Lines.Count;

	// Setting the text starts a new document: nothing to undo, not modified, no ModifiedChanged (as with SyntaxDocument.Text);
	// TextChanged is raised again once that is so.
	public override string Text
	{
		get
		{
			return base.Text;
		}
		set
		{
			bool readOnly = ReadOnly;
			m_settingText = true;
			try
			{
				ReadOnly = false;
				base.Text = value ?? string.Empty;
				ReadOnly = readOnly;
				EmptyUndoBuffer();
				SetSavePoint();
			}
			finally
			{
				m_settingText = false;
			}
			OnTextChanged(EventArgs.Empty);
		}
	}

	public void ClearText()
	{
		Text = string.Empty;
	}

	// Colouring for one of the IDE's syntax resources (Lua, JavaScript, XML, SQLServer2K_SQL, HALScript); the keywords
	// come from the .syn file, as they did for SyntaxBox.
	public void SetSyntaxResource(string name)
	{
		string lexer;
		switch (name)
		{
		case "Lua":
			lexer = "lua";
			break;
		case "JavaScript":
			lexer = "cpp";
			break;
		case "XML":
			lexer = "xml";
			break;
		case "SQLServer2K_SQL":
			lexer = "mssql";
			break;
		default:
			lexer = null;
			break;
		}
		StyleClearAll();
		// Lexilla's "null" lexer: plain text (HALScript has no Scintilla lexer).
		LexerName = lexer ?? "null";
		if (lexer == null)
		{
			return;
		}
		SetKeywords(0, string.Join(" ", ReadKeywords(name)));
		switch (lexer)
		{
		case "lua":
			SetStyles(Style.Lua.Comment, Style.Lua.CommentLine, Style.Lua.CommentDoc, Style.Lua.Word, Style.Lua.String, Style.Lua.Character, Style.Lua.LiteralString, Style.Lua.Number);
			break;
		case "cpp":
			SetStyles(Style.Cpp.Comment, Style.Cpp.CommentLine, Style.Cpp.CommentDoc, Style.Cpp.Word, Style.Cpp.String, Style.Cpp.Character, Style.Cpp.Verbatim, Style.Cpp.Number);
			break;
		case "mssql":
			SetStyles(Style.Sql.Comment, Style.Sql.CommentLine, Style.Sql.CommentDoc, Style.Sql.Word, Style.Sql.String, Style.Sql.Character, Style.Sql.QuotedIdentifier, Style.Sql.Number);
			break;
		case "xml":
			Styles[Style.Xml.Tag].ForeColor = Color.Maroon;
			Styles[Style.Xml.TagEnd].ForeColor = Color.Maroon;
			Styles[Style.Xml.Attribute].ForeColor = Color.Red;
			Styles[Style.Xml.DoubleString].ForeColor = Color.Blue;
			Styles[Style.Xml.SingleString].ForeColor = Color.Blue;
			Styles[Style.Xml.Comment].ForeColor = Color.Green;
			Styles[Style.Xml.CData].ForeColor = Color.Gray;
			break;
		}
	}

	public bool HasBreakpoint(int line)
	{
		return (Lines[line].MarkerGet() & (1u << BreakpointMarker)) != 0;
	}

	public void SetBreakpoint(int line, bool on)
	{
		if (line < 0 || line >= Lines.Count || HasBreakpoint(line) == on)
		{
			return;
		}
		if (on)
		{
			Lines[line].MarkerAdd(BreakpointMarker);
			BreakpointAdded?.Invoke(this, line);
		}
		else
		{
			Lines[line].MarkerDelete(BreakpointMarker);
			BreakpointRemoved?.Invoke(this, line);
		}
	}

	public void ClearBreakpoints()
	{
		MarkerDeleteAll(BreakpointMarker);
		MarkerDeleteAll(DisabledBreakpointMarker);
	}

	// SyntaxBox gutter image 13: a breakpoint that is set but disabled.
	public void SetDisabledBreakpointMarker(int line, bool on)
	{
		SetMarker(line, DisabledBreakpointMarker, on);
	}

	// SyntaxBox gutter image 2: the line the debugger stopped on.
	public void SetCurrentLineMarker(int line, bool on)
	{
		SetMarker(line, CurrentLineMarker, on);
	}

	// The caret to the start of a 0-based line.
	public void GotoLine(int line)
	{
		if (Lines.Count == 0)
		{
			return;
		}
		Lines[Math.Max(0, Math.Min(line, Lines.Count - 1))].Goto();
	}

	// Scrolls so that a 0-based line is the first one shown.
	public void ScrollIntoView(int line)
	{
		LineScroll(line - FirstVisibleLine, 0);
	}

	public void ShowFind()
	{
		ShowFindReplace(replace: false);
	}

	public void ShowReplace()
	{
		ShowFindReplace(replace: true);
	}

	public void ShowGotoLine()
	{
		using GoToLineForm form = new GoToLineForm(Lines.Count, CurrentLine + 1);
		if (form.ShowDialog(FindForm()) == DialogResult.OK)
		{
			GotoLine(form.LineNumber - 1);
			ScrollCaret();
		}
	}

	public bool FindNext(string text, SearchFlags flags, bool fromStart)
	{
		if (string.IsNullOrEmpty(text))
		{
			return false;
		}
		SearchFlags = flags;
		TargetStart = fromStart ? 0 : SelectionEnd;
		TargetEnd = TextLength;
		if (SearchInTarget(text) < 0)
		{
			if (fromStart)
			{
				return false;
			}
			return FindNext(text, flags, fromStart: true);
		}
		SetSelection(TargetEnd, TargetStart);
		ScrollCaret();
		return true;
	}

	public void ReplaceSelection(string text, string replacement, SearchFlags flags)
	{
		if (!ReadOnly && SelectedText.Length > 0 && string.Compare(SelectedText, text, (flags & SearchFlags.MatchCase) == 0) == 0)
		{
			ReplaceSelection(replacement ?? string.Empty);
		}
		FindNext(text, flags, fromStart: false);
	}

	public int ReplaceAll(string text, string replacement, SearchFlags flags)
	{
		if (ReadOnly || string.IsNullOrEmpty(text))
		{
			return 0;
		}
		int count = 0;
		SearchFlags = flags;
		TargetWholeDocument();
		BeginUndoAction();
		try
		{
			while (SearchInTarget(text) >= 0)
			{
				ReplaceTarget(replacement ?? string.Empty);
				count++;
				TargetStart = TargetEnd;
				TargetEnd = TextLength;
			}
		}
		finally
		{
			EndUndoAction();
		}
		return count;
	}

	protected override void OnMarginClick(MarginClickEventArgs e)
	{
		base.OnMarginClick(e);
		if (m_allowBreakpoints && e.Margin == BreakpointMargin)
		{
			int line = LineFromPosition(e.Position);
			SetBreakpoint(line, !HasBreakpoint(line));
		}
	}

	protected override void OnUpdateUI(UpdateUIEventArgs e)
	{
		base.OnUpdateUI(e);
		if ((e.Change & UpdateChange.Selection) != 0)
		{
			CaretChange?.Invoke(this, EventArgs.Empty);
		}
	}

	protected override void OnSavePointLeft(EventArgs e)
	{
		base.OnSavePointLeft(e);
		if (!m_settingText)
		{
			ModifiedChanged?.Invoke(this, EventArgs.Empty);
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			m_findReplaceForm?.Dispose();
		}
		base.Dispose(disposing);
	}

	private void ShowFindReplace(bool replace)
	{
		if (m_findReplaceForm == null || m_findReplaceForm.IsDisposed)
		{
			m_findReplaceForm = new FindReplaceForm(this);
		}
		m_findReplaceForm.ShowFor(replace, SelectedText.Contains("\n") ? null : SelectedText);
	}

	private void SetMarker(int line, int marker, bool on)
	{
		if (line < 0 || line >= Lines.Count)
		{
			return;
		}
		if (on)
		{
			Lines[line].MarkerAdd(marker);
		}
		else
		{
			Lines[line].MarkerDelete(marker);
		}
	}

	private void SetStyles(int comment, int commentLine, int commentDoc, int word, int @string, int character, int literal, int number)
	{
		Styles[comment].ForeColor = Color.Green;
		Styles[commentLine].ForeColor = Color.Green;
		Styles[commentDoc].ForeColor = Color.Green;
		Styles[word].ForeColor = Color.Blue;
		Styles[@string].ForeColor = Color.Maroon;
		Styles[character].ForeColor = Color.Maroon;
		Styles[literal].ForeColor = Color.Maroon;
		Styles[number].ForeColor = Color.Teal;
	}

	// The word patterns of the .syn file's keyword groups (operators and the regular-expression patterns are left to
	// the lexer).
	private static IEnumerable<string> ReadKeywords(string syntaxName)
	{
		using Stream stream = typeof(CodeEditor).Assembly.GetManifestResourceStream($"Redbox.KioskEngine.IDE.{syntaxName}.syn");
		if (stream == null)
		{
			return Enumerable.Empty<string>();
		}
		XmlDocument document = new XmlDocument();
		document.Load(stream);
		return document.SelectNodes("//Keywords/PatternGroup/Patterns").Cast<XmlNode>().SelectMany((XmlNode patterns) => Regex.Split(patterns.InnerText.Trim(), "\\s+")).Where((string word) => Regex.IsMatch(word, "^[A-Za-z_][A-Za-z0-9_]*$")).Distinct(StringComparer.Ordinal).ToList();
	}
}

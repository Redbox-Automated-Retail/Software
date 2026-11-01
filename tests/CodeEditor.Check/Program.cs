using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Redbox.KioskEngine.IDE;
using ScintillaNET;

internal static class Program
{
	private static readonly List<string> Failures = new List<string>();

	private static int Checks;

	[STAThread]
	private static int Main(string[] args)
	{
		string log = args.Length > 0 ? args[0] : null;
		using StringWriter output = new StringWriter();
		try
		{
			Run(output);
		}
		catch (Exception e)
		{
			Failures.Add("unhandled: " + e);
		}
		foreach (string failure in Failures)
		{
			output.WriteLine("FAIL " + failure);
		}
		output.WriteLine(Failures.Count == 0 ? $"ALL PASSED ({Checks} checks, {Environment.Version})" : $"{Failures.Count} of {Checks} checks failed ({Environment.Version})");
		Console.Write(output.ToString());
		if (log != null)
		{
			File.WriteAllText(log, output.ToString());
		}
		return Failures.Count == 0 ? 0 : 1;
	}

	private static void Run(TextWriter output)
	{
		using Form form = new Form { ShowInTaskbar = false, Width = 600, Height = 300, StartPosition = FormStartPosition.Manual, Left = -2000 };
		CodeEditor editor = new CodeEditor { Dock = DockStyle.Fill };
		form.Controls.Add(editor);
		form.Show();
		Application.DoEvents();
		int modifiedChanged = 0, textChanged = 0;
		bool canUndoAtLastTextChanged = true;
		List<string> breakpointEvents = new List<string>();
		editor.ModifiedChanged += delegate { modifiedChanged++; };
		editor.TextChanged += delegate { textChanged++; canUndoAtLastTextChanged = editor.CanUndo; };
		editor.BreakpointAdded += (s, line) => breakpointEvents.Add("+" + line);
		editor.BreakpointRemoved += (s, line) => breakpointEvents.Add("-" + line);

		editor.SetSyntaxResource("Lua");
		Check("Lua lexer", editor.LexerName == "lua", editor.LexerName);
		string script = string.Join("\n", "local function f(x)", "  -- comment", "  if x then", "    return 1", "  end", "  return 2", "end", "", "for i = 1, 3 do", "  f(i)", "end");
		editor.Text = script;
		Check("text set", editor.Text == script, editor.Text);
		Check("text set: nothing to undo", !editor.CanUndo);
		Check("text set: not modified", !editor.Modified);
		Check("text set: no ModifiedChanged", modifiedChanged == 0, modifiedChanged.ToString());
		Check("text set: TextChanged seen without undo", textChanged > 0 && !canUndoAtLastTextChanged);
		Check("line count", editor.LineCount == 11, editor.LineCount.ToString());

		editor.GotoLine(3);
		Check("GotoLine", editor.CaretLine == 3, editor.CaretLine.ToString());
		editor.InsertText(editor.CurrentPosition, "x");
		Check("edit: ModifiedChanged once", modifiedChanged == 1, modifiedChanged.ToString());
		Check("edit: can undo", editor.CanUndo);
		Check("edit: TextChanged with undo", canUndoAtLastTextChanged);
		editor.Text = script;

		editor.SetBreakpoint(2, true);
		editor.SetBreakpoint(2, true);
		Check("breakpoint set", editor.HasBreakpoint(2));
		editor.SetBreakpoint(2, false);
		Check("breakpoint cleared", !editor.HasBreakpoint(2));
		Check("breakpoint events", string.Join(",", breakpointEvents) == "+2,-2", string.Join(",", breakpointEvents));
		editor.SetBreakpoint(4, true);
		editor.SetDisabledBreakpointMarker(4, on: true);
		editor.SetCurrentLineMarker(6, on: true);
		Check("markers", editor.Lines[4].MarkerGet() == ((1u << 1) | (1u << 2)) && editor.Lines[6].MarkerGet() == (1u << 3), $"{editor.Lines[4].MarkerGet()} {editor.Lines[6].MarkerGet()}");
		editor.SetCurrentLineMarker(6, on: false);
		editor.ClearBreakpoints();
		Check("markers cleared", editor.Lines[4].MarkerGet() == 0 && editor.Lines[6].MarkerGet() == 0);
		editor.SetBreakpoint(99, true);
		Check("out-of-range line ignored", breakpointEvents.Count == 3, string.Join(",", breakpointEvents));

		editor.Text = string.Join("\n", new string[200]);
		editor.ScrollIntoView(50);
		Application.DoEvents();
		Check("ScrollIntoView", editor.FirstVisibleLine == 50, editor.FirstVisibleLine.ToString());
		editor.Text = script;

		editor.GotoLine(0);
		Check("FindNext", editor.FindNext("return", SearchFlags.MatchCase, fromStart: false) && editor.SelectedText == "return" && editor.CaretLine == 3, editor.CaretLine.ToString());
		Check("FindNext again", editor.FindNext("return", SearchFlags.MatchCase, fromStart: false) && editor.CaretLine == 5, editor.CaretLine.ToString());
		Check("FindNext wraps", editor.FindNext("return", SearchFlags.MatchCase, fromStart: false) && editor.CaretLine == 3, editor.CaretLine.ToString());
		Check("FindNext missing", !editor.FindNext("nothing-here", SearchFlags.None, fromStart: false));
		Check("ReplaceAll", editor.ReplaceAll("end", "END", SearchFlags.MatchCase | SearchFlags.WholeWord) == 3 && editor.Text.Contains("  END") && editor.Text.EndsWith("END"));
		editor.ReadOnly = true;
		Check("ReplaceAll read-only", editor.ReplaceAll("END", "end", SearchFlags.None) == 0);
		editor.Text = "read-only text";
		Check("read-only: text set", editor.Text == "read-only text" && editor.ReadOnly);
		editor.ReadOnly = false;

		editor.ShowWhitespace = true;
		editor.ShowEOLMarker = true;
		editor.TabsToSpaces = true;
		editor.ShowGutterMargin = false;
		Check("view options", editor.ViewWhitespace == WhitespaceMode.VisibleAlways && editor.ViewEol && !editor.UseTabs && !editor.ShowGutterMargin);

		foreach (string syntax in new[] { "JavaScript", "XML", "SQLServer2K_SQL", "HALScript" })
		{
			editor.SetSyntaxResource(syntax);
		}
		Check("other syntaxes", editor.LexerName == "null", editor.LexerName);

		using (TextEditor textEditor = new TextEditor())
		{
			form.Controls.Add(textEditor);
			textEditor.SetSyntaxResource("Lua");
			textEditor.SetDocumentText(script);
			textEditor.SetCurrentDebugLine(5, null);
			Check("TextEditor", true);
		}
		using (NewKeyValueForm keyValueForm = new NewKeyValueForm())
		{
			keyValueForm.Value = "return 42";
			Check("NewKeyValueForm", keyValueForm.Value == "return 42", keyValueForm.Value);
		}
		output.WriteLine($"Scintilla {typeof(Scintilla).Assembly.GetName().Version}");
	}

	private static void Check(string name, bool ok, string detail = null)
	{
		Checks++;
		if (!ok)
		{
			Failures.Add(detail == null ? name : $"{name}: {detail}");
		}
	}
}
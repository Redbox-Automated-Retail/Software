using System;
using System.Collections.Generic;
using System.IO;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Timers;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

public class Compiler
{
	private static Dictionary<string, bool> m_warningTable;

	private const string DirectiveToken = "@";

	private const string IncludeDirective = "INC";

	private const string PragmaDirective = "PRAGMA";

	private static readonly Deque<string> m_configScriptPaths;

	public Dictionary<string, bool> ConstTokens { get; set; }

	public Stream Stream { get; set; }

	public string ProgramName { get; set; }

	public CFA.CFAInternalsVersion CFAVersion { get; set; }

	private CFAInternals ControlFlowAnalyzer { get; set; }

	public Compiler(Stream stream, string programName)
	{
		Stream = stream;
		ProgramName = programName;
		CFAVersion = CFA.CFAInternalsVersion.Mock;
	}

	public ExecutionResult Compile(out ProgramInstruction compiledProgram)
	{
		int num = 0;
		using ExecutionTimer executionTimer = new ExecutionTimer();
		compiledProgram = null;
		ExecutionResult executionResult = new ExecutionResult();
		Stack<Instruction> stack = new Stack<Instruction>();
		stack.Push(ProgramInstruction.CreateEmpty(ProgramName));
		int num2 = 0;
		Deque<string> deque = new Deque<string>();
		ConstTokens = new Dictionary<string, bool>();
		IPersistentMap map = ServiceLocator.Instance.GetService<IPersistentMapService>().GetMap();
		bool value = map.GetValue("Compiler.EnablePreprocessor", def: true);
		ControlFlowAnalyzer = CFA.Initialize(CFAVersion, executionResult);
		Instruction instruction = null;
		using (TextReader textReader = new StreamReader(Stream))
		{
			while (true)
			{
				string text = ((deque.Count >= 1) ? deque.PopTop() : textReader.ReadLine());
				if (text == null)
				{
					if (instruction == null || instruction.IsBranch() || instruction.EndsContext)
					{
						CFAInternals controlFlowAnalyzer = ControlFlowAnalyzer;
						NopInstruction nopInstruction = new NopInstruction();
						num2 = (nopInstruction.LineNumber = num2 + 1);
						controlFlowAnalyzer.ProcessInstruction(nopInstruction, null, executionResult);
					}
					break;
				}
				if (value && IsDirective(text, out var directiveName))
				{
					ProcessDirective(directiveName, text, executionResult, deque);
					if (executionResult.Errors.ContainsError())
					{
						break;
					}
					continue;
				}
				Tokenizer tokenizer = new Tokenizer(++num2, text, ConstTokens);
				tokenizer.Tokenize();
				if (tokenizer.Errors.Count != 0)
				{
					executionResult.Errors.AddRange(tokenizer.Errors);
					break;
				}
				if (tokenizer.Tokens.HasOnlyComments() || tokenizer.Tokens.Count == 0)
				{
					continue;
				}
				if (tokenizer.Tokens.HasOnlyLabel())
				{
					string value2 = tokenizer.Tokens.GetLabel().Value;
					instruction = Instruction.Parse(new TokenList(new Token[1]
					{
						new Token(TokenType.Mnemonic, "NOP", isKeyValuePair: false)
					}), num2, executionResult);
					instruction.LabelName = value2;
				}
				else
				{
					instruction = Instruction.Parse(tokenizer.Tokens, num2, executionResult);
				}
				if (instruction == null)
				{
					executionResult.Errors.Add(Error.NewError("C004", "The statement contained an unknown mnemonic: " + text, "Correct your source and recompile."));
					break;
				}
				Token label = tokenizer.Tokens.GetLabel();
				if (label != null)
				{
					ControlFlowAnalyzer.AddLabel(label.Value, instruction, executionResult);
					if (executionResult.Errors.ContainsError())
					{
						break;
					}
					instruction.IsLabel = true;
				}
				TokenList symbols = tokenizer.Tokens.GetSymbols();
				foreach (Token item in symbols)
				{
					string text2 = TranslateKeyValuePairToSymbol(item);
					if (!string.IsNullOrEmpty(text2) && !instruction.OperandRecognized(text2) && instruction.BranchType != Instruction.Branch.Unconditional)
					{
						if (instruction.CreatesVariableDef)
						{
							ControlFlowAnalyzer.AddVariableDef(text2, instruction.LineNumber, executionResult);
						}
						else
						{
							ControlFlowAnalyzer.AddVariableUse(text2, instruction.LineNumber, executionResult);
						}
					}
					if (item.Value.StartsWith("__"))
					{
						executionResult.Errors.Add(Error.NewError("C005", "Symbols may not start with '__'; they are reserved. ", "Correct your source and recompile."));
						break;
					}
				}
				ControlFlowAnalyzer.ProcessInstruction(instruction, symbols, executionResult);
				if (executionResult.Errors.ContainsError())
				{
					break;
				}
				if (instruction.IsCooperativeMultitask)
				{
					num++;
				}
				stack.Peek().Instructions.Add(instruction);
				if (instruction.BeginsContext)
				{
					stack.Push(instruction);
				}
				else if (instruction.EndsContext)
				{
					Instruction instruction2 = null;
					string endContextMnemonic = stack.Peek().EndContextMnemonic;
					if (stack.Count <= 1)
					{
						executionResult.Errors.Add(Error.NewError("C004", "An unmatched context block detected: " + text, "Correct your source and recompile."));
						break;
					}
					instruction2 = stack.Pop();
					if (string.Compare(endContextMnemonic, tokenizer.Tokens.GetMnemonic().Value, ignoreCase: true) != 0)
					{
						executionResult.Errors.Add(Error.NewError("C004", "Expected context block closing mnemonic: " + endContextMnemonic, "Correct your source and recompile."));
						break;
					}
					ControlFlowAnalyzer.ProcessControlBlock(instruction2, instruction, executionResult);
					if (executionResult.Errors.ContainsError())
					{
						break;
					}
				}
			}
		}
		if (stack.Count > 1)
		{
			executionResult.Errors.Add(Error.NewError("C005", "A context block was not properly closed: " + stack.Peek()?.ToString() + ", expected closing mnemonic " + stack.Peek().EndContextMnemonic, "Correct your source and recompile."));
		}
		if (!executionResult.Errors.ContainsError())
		{
			ControlFlowAnalyzer.FixupUnconditionalBranches(executionResult);
			if (!executionResult.Errors.ContainsError())
			{
				if (CFA.DumpBeforeFinalize)
				{
					ControlFlowAnalyzer.Dump(ProgramName + "-prefinalize");
				}
				ControlFlowAnalyzer.RemoveDeadBlocks(executionResult);
				if (CFA.DumpAfterDeadBlockRemoval)
				{
					ControlFlowAnalyzer.Dump(ProgramName + "-afterdeadremove");
				}
				if (!executionResult.Errors.ContainsError())
				{
					ControlFlowAnalyzer.FindReferencedUndefinedVariables(executionResult);
					if (!executionResult.Errors.ContainsError())
					{
						ControlFlowAnalyzer.Finalize(executionResult);
						if (CFA.DumpAfterFinalize)
						{
							ControlFlowAnalyzer.Dump(ProgramName + "-postfinalize");
						}
					}
				}
			}
		}
		if (num == 0 && m_warningTable["W008"])
		{
			executionResult.Errors.Add(Error.NewWarning("W008", "Script does not contain cooperative multi-tasking instruction", "The script doesn't contain an instruction, such as YIELD, that allows another script to run. Is this the expected behavior?"));
		}
		if (!executionResult.Errors.ContainsError())
		{
			compiledProgram = (ProgramInstruction)stack.Peek();
		}
		executionResult.ExecutionTime = new TimeSpan(executionTimer.ElapsedTicks);
		if (compiledProgram != null)
		{
			compiledProgram.IsCooperative = num > 0;
		}
		if (map.GetValue("Compiler.DumpCompiledProgram", def: false) && compiledProgram != null)
		{
			InstructionList instructions = compiledProgram.Instructions;
			using TextWriter textWriter = new StreamWriter("C:\\temp\\" + ProgramName + ".preprocessed");
			for (int i = 0; i < instructions.Count; i++)
			{
				Instruction value3 = instructions[i];
				textWriter.WriteLine(value3);
			}
		}
		return executionResult;
	}

	public static void DisableWarning(string warning)
	{
		if (!warning.StartsWith("w", StringComparison.CurrentCultureIgnoreCase))
		{
			warning = "W" + warning;
		}
		if (m_warningTable.ContainsKey(warning))
		{
			m_warningTable[warning] = false;
		}
	}

	public static void EnableWarning(string warning)
	{
		if (!warning.StartsWith("w", StringComparison.CurrentCultureIgnoreCase))
		{
			warning = "W" + warning;
		}
		if (m_warningTable.ContainsKey(warning))
		{
			m_warningTable[warning] = true;
		}
	}

	public static void AppendSearchPath(string wholePath)
	{
		string[] array = ParseDelimitedPaths(wholePath);
		if (array.Length != 0)
		{
			m_configScriptPaths.AddManyToFront(array);
		}
	}

	private bool IsDirective(string maybeDirective, out string directiveName)
	{
		int num = maybeDirective.IndexOf("@") + 1;
		if (num == 0)
		{
			directiveName = string.Empty;
			return false;
		}
		if (string.Compare(maybeDirective, num, "INC", 0, "INC".Length) == 0)
		{
			directiveName = "INC";
			return true;
		}
		directiveName = string.Empty;
		return false;
	}

	private void ProcessDirective(string directive, string line, ExecutionResult result, Deque<string> results)
	{
		string directive2 = GetDirective(line);
		if (directive.Equals("INC"))
		{
			ProcessInclude(directive2, result, results);
		}
		else
		{
			LogHelper.Instance.Log("PREPROCESSOR: invalid directive type " + directive, LogEntryType.Debug);
		}
	}

	private string TranslateKeyValuePairToSymbol(Token symbol)
	{
		if (symbol.Type == TokenType.StringLiteral || symbol.Type == TokenType.NumericLiteral)
		{
			return null;
		}
		if (symbol.IsKeyValuePair)
		{
			if (symbol.Value.StartsWith("ENTRYTYPE") || symbol.Value.StartsWith("MODE") || symbol.Value.StartsWith("TYPE") || symbol.Value.StartsWith("END"))
			{
				return null;
			}
			if (symbol.Value.Contains("TRUE") || symbol.Value.Contains("FALSE"))
			{
				return null;
			}
			return symbol.Value.Split(new string[1] { symbol.PairSeparator }, StringSplitOptions.RemoveEmptyEntries)[1];
		}
		return symbol.Value;
	}

	private string GetDirective(string line)
	{
		int num = line.IndexOf('"');
		if (++num == 0)
		{
			return string.Empty;
		}
		int num2 = line.LastIndexOf('"');
		if (-1 != num2)
		{
			return line.Substring(num, num2 - num);
		}
		return string.Empty;
	}

	private void GenExitHandler()
	{
	}

	private void ProcessInclude(string fileName, ExecutionResult result, Deque<string> results)
	{
		try
		{
			bool flag = false;
			if (!File.Exists(fileName))
			{
				if (-1 != fileName.IndexOf(Path.DirectorySeparatorChar))
				{
					result.Errors.Add(Error.NewError("C006", "PREPROCESSOR Error processing @INC file: " + fileName + " doesn't exist.", "Correct your source and recompile."));
					return;
				}
				foreach (string configScriptPath in m_configScriptPaths)
				{
					string text = Path.Combine(configScriptPath, fileName);
					if (File.Exists(text))
					{
						fileName = text;
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					result.Errors.Add(Error.NewError("C006", "PREPROCESSOR Error processing @INC file " + fileName + ": unable to locate.", "Correct your source and recompile."));
					return;
				}
			}
			Deque<string> deque = new Deque<string>();
			using (TextReader textReader = new StreamReader(fileName))
			{
				while (true)
				{
					string text2 = textReader.ReadLine();
					if (text2 != null)
					{
						deque.AddToBack(text2);
						continue;
					}
					break;
				}
			}
			int count = deque.Count;
			for (int i = 0; i < count; i++)
			{
				string text3 = deque.PopBottom();
				if (!string.IsNullOrEmpty(text3))
				{
					results.PushTop(text3);
				}
			}
			deque = null;
		}
		catch (Exception ex)
		{
			result.Errors.Add(Error.NewError("C006", "PREPROCESSOR Error streaming include text (file " + fileName + "):" + ex.Message + ";", "Check the target file for existence and accessibility."));
		}
	}

	private static string[] ParseDelimitedPaths(string wholeLot)
	{
		List<string> rv = new List<string>();
		Array.ForEach(wholeLot.Split(Path.PathSeparator), delegate(string path)
		{
			if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
			{
				rv.Add(path);
			}
		});
		return rv.ToArray();
	}

	static Compiler()
	{
		m_configScriptPaths = new Deque<string>();
		IRuntimeService service = ServiceLocator.Instance.GetService<IRuntimeService>();
		m_configScriptPaths.AddToBack(service.InstallPath("Scripts"));
		m_warningTable = new Dictionary<string, bool>();
		m_warningTable["W008"] = false;
	}
}

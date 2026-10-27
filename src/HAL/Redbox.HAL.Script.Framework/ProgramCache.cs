using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Threading;
using Redbox.HAL.Component.Model.Timers;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

internal sealed class ProgramCache : IDisposable
{
	private bool Disposed;

	private readonly ReaderWriterLockSlim CacheLock = new ReaderWriterLockSlim();

	private readonly IDictionary<string, Type> OperandToType = new Dictionary<string, Type>();

	private readonly Dictionary<string, string> JobToOperand = new Dictionary<string, string>();

	private readonly IDictionary<string, CompiledProgram> CompiledPrograms = new Dictionary<string, CompiledProgram>();

	private readonly List<string> NoShowJobs = new List<string>();

	public void Dispose()
	{
		if (!Disposed)
		{
			Disposed = true;
			JobToOperand.Clear();
			OperandToType.Clear();
			CompiledPrograms.Clear();
			GC.SuppressFinalize(this);
		}
	}

	internal void CompilePrograms(string scriptsDirectory, ErrorList errorList)
	{
		if (JobToOperand.Keys.Count > 0)
		{
			return;
		}
		JobToOperand["return"] = "RETURN";
		JobToOperand["quick-return"] = "QUICKRETURN";
		IRuntimeService service = ServiceLocator.Instance.GetService<IRuntimeService>();
		using (ExecutionTimer executionTimer = new ExecutionTimer())
		{
			string text = service.RuntimePath(Assembly.GetExecutingAssembly().GetName().Name + ".dll");
			try
			{
				Assembly assembly = Assembly.LoadFrom(text);
				RegisterNatives(assembly.GetTypes());
			}
			catch (BadImageFormatException)
			{
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log($"Unable to load assembly '{text}' to scan for commands.", e);
			}
			executionTimer.Stop();
			LogHelper.Instance.Log($"[Program Cache] Time to scan for {JobToOperand.Keys.Count} native jobs: {executionTimer.Elapsed}");
		}
		foreach (string key in JobToOperand.Keys)
		{
			string text2 = key;
			string programSource = $"NATIVEJOB {JobToOperand[key]}";
			LogHelper.Instance.Log(LogEntryType.Debug, "Compile native program {0}", text2);
			ExecutionResult executionResult = CompileStringSource(text2, programSource);
			errorList.AddRange(executionResult.Errors);
			if (!executionResult.Errors.ContainsError())
			{
				continue;
			}
			LogHelper.Instance.Log($"There were errors compiling native script {text2}", LogEntryType.Error);
			foreach (Error error in executionResult.Errors)
			{
				LogHelper.Instance.Log(error.ToString(), LogEntryType.Error);
			}
		}
		using ExecutionTimer executionTimer2 = new ExecutionTimer();
		string[] files = Directory.GetFiles(scriptsDirectory, "*.hs");
		string[] array = files;
		foreach (string text3 in array)
		{
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(text3);
			if (LogHelper.Instance.IsLevelEnabled(LogEntryType.Debug))
			{
				LogHelper.Instance.Log($"Compile program {fileNameWithoutExtension} from file {text3}", LogEntryType.Info);
			}
			using MemoryStream stream = new MemoryStream(ServiceLocator.Instance.GetService<IRuntimeService>().ReadToBuffer(text3));
			ExecutionResult executionResult2 = CompileInner(stream, fileNameWithoutExtension);
			errorList.AddRange(executionResult2.Errors);
			if (!executionResult2.Errors.ContainsError())
			{
				continue;
			}
			LogHelper.Instance.Log($"There were errors compiling script {fileNameWithoutExtension}", LogEntryType.Error);
			foreach (Error error2 in executionResult2.Errors)
			{
				LogHelper.Instance.Log(error2.ToString(), LogEntryType.Error);
			}
		}
		executionTimer2.Stop();
		LogHelper.Instance.Log("[Program Cache] Time to compile {0} programs: {1}", files.Length, executionTimer2.Elapsed);
	}

	internal NativeJobAdapter MakeJobFromOperand(ExecutionContext ctx, ExecutionResult result, string key)
	{
		NativeJobAdapter result2 = null;
		if (key == "RETURN")
		{
			result2 = new ReturnJob(result, ctx);
		}
		else if (OperandToType.ContainsKey(key))
		{
			try
			{
				result2 = (NativeJobAdapter)Activator.CreateInstance(OperandToType[key], BindingFlags.Instance | BindingFlags.NonPublic, null, new object[2] { result, ctx }, null);
			}
			catch (Exception ex)
			{
				LogHelper.Instance.Log("Unable to create job {0}", key);
				LogHelper.Instance.Log(ex.Message);
				result2 = null;
			}
		}
		return result2;
	}

	internal ExecutionResult CompileStringSource(string programName, string programSource)
	{
		using MemoryStream memoryStream = new MemoryStream();
		using StreamWriter streamWriter = new StreamWriter(memoryStream);
		streamWriter.WriteLine(programSource);
		streamWriter.Flush();
		memoryStream.Seek(0L, SeekOrigin.Begin);
		return CompileInner(memoryStream, programName);
	}

	internal ExecutionResult CompileInner(Stream stream, string programName)
	{
		using (new WithUpgradeableReadLock(CacheLock))
		{
			CompiledProgram program = GetProgram(programName);
			if (program == null)
			{
				using (new WithWriteLock(CacheLock))
				{
					return CompileProgramInner(stream, programName);
				}
			}
			if (programName.Equals("$$$immediate$$$", StringComparison.CurrentCultureIgnoreCase))
			{
				using (new WithWriteLock(CacheLock))
				{
					CompiledPrograms.Remove("$$$immediate$$$");
					return CompileProgramInner(stream, programName);
				}
			}
			if (ControllerConfiguration.Instance.AllowProgramCacheReplacement2)
			{
				using (new WithWriteLock(CacheLock))
				{
					CompiledPrograms.Remove(programName);
					LogHelper.Instance.Log($"Allow replacement of program {programName} - allow cache replacement2 is true.", LogEntryType.Info);
					return CompileProgramInner(stream, programName);
				}
			}
			if (LogHelper.Instance.IsLevelEnabled(LogEntryType.Debug))
			{
				LogHelper.Instance.Log($"Not allowing replacement of program {programName}", LogEntryType.Info);
			}
			return new ExecutionResult
			{
				ExecutionTime = new TimeSpan(0L)
			};
		}
	}

	internal CompiledProgram GetProgram(string name)
	{
		using (new WithReadLock(CacheLock))
		{
			return CompiledPrograms.ContainsKey(name) ? CompiledPrograms[name] : null;
		}
	}

	internal bool RemoveProgram(string name)
	{
		using (new WithWriteLock(CacheLock))
		{
			return CompiledPrograms.Remove(name);
		}
	}

	internal List<string> ProgramList()
	{
		List<string> list = new List<string>();
		using (new WithReadLock(CacheLock))
		{
			foreach (string key in CompiledPrograms.Keys)
			{
				string text = NoShowJobs.Find((string each) => each.Equals(key, StringComparison.CurrentCultureIgnoreCase));
				if (text == null)
				{
					list.Add(key);
				}
			}
			return list;
		}
	}

	internal ProgramCache()
	{
	}

	private ExecutionResult CompileProgramInner(Stream stream, string programName)
	{
		ExecutionResult executionResult = new Compiler(stream, programName).Compile(out var compiledProgram);
		CompiledPrograms.Add(programName, new CompiledProgram(executionResult, compiledProgram));
		return executionResult;
	}

	private void RegisterNatives(IEnumerable<Type> types)
	{
		int num = 0;
		int num2 = 0;
		foreach (Type type in types)
		{
			NativeJobAttribute[] array = (NativeJobAttribute[])type.GetCustomAttributes(typeof(NativeJobAttribute), inherit: false);
			if (array == null || array.Length == 0)
			{
				continue;
			}
			NativeJobAttribute[] array2 = array;
			foreach (NativeJobAttribute nativeJobAttribute in array2)
			{
				if (nativeJobAttribute.ProgramName == null)
				{
					continue;
				}
				if (nativeJobAttribute.Operand == null)
				{
					nativeJobAttribute.Operand = nativeJobAttribute.ProgramName.ToUpper();
				}
				OperandToType.Add(nativeJobAttribute.Operand, type);
				JobToOperand.Add(nativeJobAttribute.ProgramName, nativeJobAttribute.Operand);
				num2++;
				if (nativeJobAttribute.HideFromList)
				{
					NoShowJobs.Add(nativeJobAttribute.ProgramName);
				}
				ConstructorInfo[] constructors = type.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic);
				if (constructors == null || constructors.Length == 0)
				{
					LogHelper.Instance.Log("Type {0} does not express a non-public ctor.", type.ToString());
					continue;
				}
				ConstructorInfo[] array3 = constructors;
				for (int j = 0; j < array3.Length; j++)
				{
					ParameterInfo[] parameters = array3[j].GetParameters();
					if (parameters == null || parameters.Length == 0)
					{
						LogHelper.Instance.Log("Type {0} has a ctor that does not match.", type.ToString());
					}
					else if (parameters.Length != 2)
					{
						LogHelper.Instance.Log("Type {0} does not have a 2-parameter ctor.", type.ToString());
					}
					else if (parameters[0].ParameterType.Name != "ExecutionResult" || parameters[1].ParameterType.Name != "ExecutionContext")
					{
						LogHelper.Instance.Log("Type {0} does not have a ctor that matches the expected parameter list.", type.ToString());
					}
					else
					{
						num++;
					}
				}
			}
		}
		if (num != num2)
		{
			LogHelper.Instance.Log("There is a mismatch between ctor count ({0}) & job keys ({1})", num, num2);
		}
	}
}

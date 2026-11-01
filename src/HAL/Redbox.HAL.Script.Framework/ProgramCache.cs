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
			string scriptAssembly = service.RuntimePath(Assembly.GetExecutingAssembly().GetName().Name + ".dll");
			try
			{
				Assembly assembly = Assembly.LoadFrom(scriptAssembly);
				RegisterNatives(assembly.GetTypes());
			}
			catch (BadImageFormatException)
			{
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log($"Unable to load assembly '{scriptAssembly}' to scan for commands.", e);
			}
			executionTimer.Stop();
			LogHelper.Instance.Log($"[Program Cache] Time to scan for {JobToOperand.Keys.Count} native jobs: {executionTimer.Elapsed}");
		}
		foreach (string jobName in JobToOperand.Keys)
		{
			string programSource = $"NATIVEJOB {JobToOperand[jobName]}";
			LogHelper.Instance.Log(LogEntryType.Debug, "Compile native program {0}", jobName);
			ExecutionResult executionResult = CompileStringSource(jobName, programSource);
			errorList.AddRange(executionResult.Errors);
			if (!executionResult.Errors.ContainsError())
			{
				continue;
			}
			LogHelper.Instance.Log($"There were errors compiling native script {jobName}", LogEntryType.Error);
			foreach (Error error in executionResult.Errors)
			{
				LogHelper.Instance.Log(error.ToString(), LogEntryType.Error);
			}
		}
		using ExecutionTimer compileTimer = new ExecutionTimer();
		string[] files = Directory.GetFiles(scriptsDirectory, "*.hs");
		foreach (string file in files)
		{
			string programName = Path.GetFileNameWithoutExtension(file);
			if (LogHelper.Instance.IsLevelEnabled(LogEntryType.Debug))
			{
				LogHelper.Instance.Log($"Compile program {programName} from file {file}", LogEntryType.Info);
			}
			using MemoryStream stream = new MemoryStream(ServiceLocator.Instance.GetService<IRuntimeService>().ReadToBuffer(file));
			ExecutionResult compileResult = CompileInner(stream, programName);
			errorList.AddRange(compileResult.Errors);
			if (!compileResult.Errors.ContainsError())
			{
				continue;
			}
			LogHelper.Instance.Log($"There were errors compiling script {programName}", LogEntryType.Error);
			foreach (Error error in compileResult.Errors)
			{
				LogHelper.Instance.Log(error.ToString(), LogEntryType.Error);
			}
		}
		compileTimer.Stop();
		LogHelper.Instance.Log("[Program Cache] Time to compile {0} programs: {1}", files.Length, compileTimer.Elapsed);
	}

	internal NativeJobAdapter MakeJobFromOperand(ExecutionContext ctx, ExecutionResult result, string key)
	{
		NativeJobAdapter job = null;
		if (key == "RETURN")
		{
			job = new ReturnJob(result, ctx);
		}
		else if (OperandToType.ContainsKey(key))
		{
			try
			{
				job = (NativeJobAdapter)Activator.CreateInstance(OperandToType[key], BindingFlags.Instance | BindingFlags.NonPublic, null, new object[2] { result, ctx }, null);
			}
			catch (Exception ex)
			{
				LogHelper.Instance.Log("Unable to create job {0}", key);
				LogHelper.Instance.Log(ex.Message);
				job = null;
			}
		}
		return job;
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
				string hidden = NoShowJobs.Find((string each) => each.Equals(key, StringComparison.CurrentCultureIgnoreCase));
				if (hidden == null)
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
		using MemoryStream source = new MemoryStream();
		stream.CopyTo(source);
		source.Position = 0L;
		ExecutionResult executionResult = new Compiler(source, programName).Compile(out var compiledProgram);
		CompiledPrograms.Add(programName, new CompiledProgram(executionResult, compiledProgram, source.ToArray()));
		return executionResult;
	}

	private void RegisterNatives(IEnumerable<Type> types)
	{
		int matchingConstructors = 0;
		int registeredJobs = 0;
		foreach (Type type in types)
		{
			NativeJobAttribute[] attributes = (NativeJobAttribute[])type.GetCustomAttributes(typeof(NativeJobAttribute), inherit: false);
			if (attributes.Length == 0)
			{
				continue;
			}
			foreach (NativeJobAttribute nativeJobAttribute in attributes)
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
				registeredJobs++;
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
				foreach (ConstructorInfo constructor in constructors)
				{
					ParameterInfo[] parameters = constructor.GetParameters();
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
						matchingConstructors++;
					}
				}
			}
		}
		if (matchingConstructors != registeredJobs)
		{
			LogHelper.Instance.Log("There is a mismatch between ctor count ({0}) & job keys ({1})", matchingConstructors, registeredJobs);
		}
	}
}

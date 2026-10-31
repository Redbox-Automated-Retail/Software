using System;
using System.Data;
using System.Diagnostics;
using System.Reflection;

namespace Redbox.NAnt.StuDiscs.Tasks;

public class StuClient
{
	private const string STU_RULE_LOGGER_PROPERTY = "Logger";

	private const string STU_RULE_LOG_HANDLER_DELEGATE = "LogHandler";

	private const string STU_RULE_LOG_HANDLER_METHOD = "LogWrapper";

	private const string STU_RULE_INITIALIZE_METHOD = "Initialize";

	protected Assembly _assembly;

	private object _rule;

	private string _ruleVersion;

	public string RuleVersion => _ruleVersion;

	protected Assembly RuleAssembly
	{
		get
		{
			if ((object)_assembly == null)
			{
				_assembly = Assembly.LoadFrom("C:\\Program Files\\Redbox\\StuRule.dll");
			}
			return _assembly;
		}
	}

	public void Initialize()
	{
		lock (this)
		{
			string ruleAlias = GetRuleAlias();
			string fileVersion = FileVersionInfo.GetVersionInfo(RuleAssembly.Location).FileVersion;
			Type type = RuleAssembly.GetType(ruleAlias);
			_rule = Activator.CreateInstance(type);
			InitializeRuleLogger(_rule, _rule.GetType());
			InvokeRuleInitialize(_rule, _rule.GetType());
			_ruleVersion = $"Rule: {ruleAlias} Version: {fileVersion}";
		}
	}

	private void InitializeRuleLogger(object ruleObject, Type ruleType)
	{
		Type type = RuleAssembly.GetType("LogHandler");
		MethodInfo method = GetType().GetMethod("LogWrapper", BindingFlags.Static | BindingFlags.NonPublic);
		Delegate value = Delegate.CreateDelegate(type, method);
		PropertyInfo property = ruleType.GetProperty("Logger");
		property.SetValue(ruleObject, value, null);
	}

	private void InvokeRuleInitialize(object ruleObject, Type ruleType)
	{
		Type type = ruleType.Assembly.GetType("RuleExecutionMode");
		object obj = Enum.Parse(type, "Client");
		MethodInfo method = ruleType.GetMethod("Initialize");
		method.Invoke(ruleObject, new object[1] { obj });
	}

	public DataSet ExecuteRule()
	{
		lock (this)
		{
			Type type = RuleAssembly.GetType(GetRuleAlias());
			MethodInfo method = type.GetMethod("Execute");
			object[] array = new object[1];
			object[] array2 = new object[0];
			array[0] = array2;
			Initialize();
			return (DataSet)method.Invoke(_rule, array);
		}
	}

	protected virtual string GetRuleAlias()
	{
		object[] customAttributes = RuleAssembly.GetCustomAttributes(typeof(AssemblyDefaultAliasAttribute), inherit: false);
		AssemblyDefaultAliasAttribute assemblyDefaultAliasAttribute = (AssemblyDefaultAliasAttribute)customAttributes[0];
		return assemblyDefaultAliasAttribute.DefaultAlias;
	}

	private static void LogWrapper(string message, Exception e)
	{
	}
}

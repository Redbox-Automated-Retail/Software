using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Redbox.Core;
using Redbox.Macros;
using Redbox.ScriptingHost.Framework;

namespace Redbox.Installer.Framework;

[Element(XPath = "page-template")]
[DisplayName("Page Template")]
public class PageTemplateDefinition : Definition, IDisposable
{
	internal enum ElementNodeType
	{
		Control,
		Data
	}

	internal sealed class ElementNode
	{
		public List<ElementNode> Nodes = new List<ElementNode>();

		public List<EventHandlerData> EventHandlers = new List<EventHandlerData>();

		public IDictionary<string, string> Properties = new Dictionary<string, string>();

		public bool IsBuilt { get; set; }

		public object Instance { get; set; }

		public ElementNodeType Type { get; set; }

		public IEnumerable<string> GetProperties()
		{
			foreach (string each in Properties.Keys)
			{
				if (!(each == "Type") && !(each == "UseConstructor") && !each.StartsWith("Add-"))
				{
					yield return each;
				}
			}
		}

		public IEnumerable<string> GetAddProperties()
		{
			foreach (string each in Properties.Keys)
			{
				if (each.StartsWith("Add-") && !(each == "Add-Target"))
				{
					yield return each;
				}
			}
		}
	}

	internal sealed class EventHandlerData
	{
		public string EventName { get; set; }

		public string SourceText { get; set; }

		public string FunctionName { get; set; }
	}

	private string m_text;

	private string m_title;

	private string m_subTitle;

	private string m_proceedText;

	private PageKernelObject m_kernelObject;

	private VsaScriptingHost m_scriptingHost;

	private readonly StringBuilder m_classScriptBuilder = new StringBuilder();

	private readonly StringBuilder m_handlerScriptBuilder = new StringBuilder();

	[ElementProperty(UseNodeText = true)]
	public string Text
	{
		get
		{
			return MacroContainer.Instance.ExpandRuntimeMacros(m_text);
		}
		set
		{
			m_text = value;
		}
	}

	[ElementProperty(Name = "title", IsRequired = true)]
	public string Title
	{
		get
		{
			return MacroContainer.Instance.ExpandRuntimeMacros(m_title);
		}
		set
		{
			m_title = value;
		}
	}

	[ElementProperty(Name = "subtitle")]
	public string SubTitle
	{
		get
		{
			return MacroContainer.Instance.ExpandRuntimeMacros(m_subTitle);
		}
		set
		{
			m_subTitle = value;
		}
	}

	[ElementProperty(Name = "proceed-text")]
	public string ProceedText
	{
		get
		{
			return MacroContainer.Instance.ExpandRuntimeMacros(m_proceedText);
		}
		set
		{
			m_proceedText = value;
		}
	}

	[DefaultValue(true)]
	[ElementProperty(Name = "allow-cancel")]
	public bool AllowCancel { get; set; }

	[ElementProperty(Name = "allow-move-next")]
	[DefaultValue(true)]
	public bool AllowMoveNext { get; set; }

	[ElementProperty(Name = "allow-move-previous")]
	[DefaultValue(true)]
	public bool AllowMovePrevious { get; set; }

	public PageKernelObject KernelObject
	{
		get
		{
			if (m_kernelObject == null)
			{
				m_kernelObject = new PageKernelObject();
			}
			return m_kernelObject;
		}
	}

	internal ElementNode RootElement { get; set; }

	public void Dispose()
	{
		if (m_scriptingHost != null)
		{
			m_scriptingHost.Dispose();
		}
	}

	public bool Activate()
	{
		return KernelObject.RaiseActivate();
	}

	public bool Validate()
	{
		return KernelObject.RaiseValidate();
	}

	public void Configure()
	{
		KernelObject.RaiseConfigure();
	}

	public void Deactivate()
	{
		KernelObject.RaiseDeactivate();
	}

	public ErrorList BuildHost(string pageId, out Control control)
	{
		ErrorList errors = new ErrorList();
		if (!RootElement.IsBuilt)
		{
			KernelObject.PageID = pageId;
			m_classScriptBuilder.AppendLine("import System;");
			m_classScriptBuilder.AppendLine("import System.Collections;");
			m_classScriptBuilder.AppendLine("import System.IO;");
			m_classScriptBuilder.AppendLine("import System.Globalization;");
			m_classScriptBuilder.AppendLine("import System.Windows.Forms;\n");
			m_classScriptBuilder.AppendLine("import System.ServiceProcess;\n");
			m_classScriptBuilder.AppendLine("\nvar handlerInstance : PageHandler = new PageHandler();");
			m_classScriptBuilder.AppendLine("handlerInstance.start();\n");
			m_classScriptBuilder.AppendLine("public class PageHandler {");
			m_classScriptBuilder.AppendLine("\tpublic function start() {");
			foreach (EventHandlerData eventHandler in RootElement.EventHandlers)
			{
				m_classScriptBuilder.AppendFormat("\t\tpage.add_{0}( {1} );\n", eventHandler.EventName, eventHandler.FunctionName);
				m_handlerScriptBuilder.AppendLine(eventHandler.SourceText);
			}
			BuildNodes(RootElement.Instance, RootElement.Nodes);
			RootElement.IsBuilt = true;
			m_classScriptBuilder.AppendLine("\t}\n");
			m_classScriptBuilder.AppendLine(m_handlerScriptBuilder.ToString());
			m_classScriptBuilder.AppendLine("}");
			if (InstallerProject.Instance.LogGeneratedCode)
			{
				InstallerProject.Instance.ActiveConsole.WriteLine("Generated script for page template: '{0}', page ID: '{1}'", base.ID, pageId);
				InstallerProject.Instance.ActiveConsole.WriteLine(m_classScriptBuilder);
				InstallerProject.Instance.ActiveConsole.WriteLine("-----------------------------------------------------");
			}
			m_scriptingHost = new VsaScriptingHost();
			m_scriptingHost.SetKernelObject("rix", m_scriptKernelObject);
			m_scriptingHost.SetKernelObject("page", KernelObject);
			m_scriptingHost.CompileError += delegate(object sender, EventArgs args)
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine(sender.ToString());
				stringBuilder.AppendLine("---- Source Code ----");
				stringBuilder.AppendLine(m_classScriptBuilder.ToString());
				errors.Add(Error.NewError("J002", "JScript.NET compiler error.", stringBuilder.ToString()));
			};
			m_scriptingHost.ExecuteScript(m_classScriptBuilder.ToString());
		}
		control = RootElement.Instance as Control;
		return errors;
	}

	protected override void OnCustomParse(XmlNode node, IConsole console, ErrorList errors)
	{
		Panel panel = new Panel();
		panel.Name = "rootPanel";
		panel.Dock = DockStyle.Fill;
		Panel instance = panel;
		RootElement = new ElementNode
		{
			Instance = instance
		};
		WalkNodes(RootElement, node.ChildNodes);
		base.OnCustomParse(node, console, errors);
	}

	internal void WalkNodes(ElementNode elementNode, XmlNodeList nodes)
	{
		foreach (XmlNode node in nodes)
		{
			switch (node.Name)
			{
			case "event-handler":
			{
				EventHandlerData eventHandlerData = new EventHandlerData();
				eventHandlerData.EventName = node.GetExpandedAttributeValue<string>("name");
				eventHandlerData.FunctionName = node.GetExpandedAttributeValue<string>("function-name");
				eventHandlerData.SourceText = node.InnerText;
				EventHandlerData item = eventHandlerData;
				elementNode.EventHandlers.Add(item);
				break;
			}
			case "custom":
			case "element":
			{
				ElementNode elementNode2 = new ElementNode();
				elementNode2.Type = ((node.Name == "custom") ? ElementNodeType.Data : ElementNodeType.Control);
				ElementNode elementNode3 = elementNode2;
				foreach (XmlAttribute attribute in node.Attributes)
				{
					elementNode3.Properties[attribute.Name] = attribute.Value;
				}
				WalkNodes(elementNode3, node.ChildNodes);
				elementNode.Nodes.Add(elementNode3);
				break;
			}
			case "script":
				m_handlerScriptBuilder.AppendLine(node.InnerText);
				break;
			}
		}
	}

	internal void BuildNodes(object parent, IEnumerable<ElementNode> nodes)
	{
		foreach (ElementNode node in nodes)
		{
			if (!node.Properties.ContainsKey("Type") || node.Type == ElementNodeType.Data)
			{
				continue;
			}
			Type type = GetType(node.Properties["Type"]);
			node.Instance = Activator.CreateInstance(type);
			if (node.Properties.ContainsKey("UseConstructor") && string.Compare(node.Properties["UseConstructor"], true.ToString(), ignoreCase: true) == 0)
			{
				ConstructorInfo constructorInfo = null;
				List<ConstructorInfo> constructors = new List<ConstructorInfo>(type.GetConstructors());
				constructors.Sort((ConstructorInfo x, ConstructorInfo y) => y.GetParameters().Length.CompareTo(x.GetParameters().Length));
				foreach (ConstructorInfo candidate in constructors)
				{
					int matchedCount = 0;
					List<ParameterInfo> parameterInfoList = new List<ParameterInfo>(candidate.GetParameters());
					foreach (string property2 in node.GetProperties())
					{
						string candidateParameterName = property2;
						ParameterInfo parameterInfo = parameterInfoList.Find((ParameterInfo eachParam) => string.Compare(candidateParameterName, eachParam.Name, ignoreCase: true) == 0);
						if (parameterInfo != null)
						{
							matchedCount++;
						}
					}
					if (matchedCount == parameterInfoList.Count)
					{
						constructorInfo = candidate;
						break;
					}
				}
				if (constructorInfo != null)
				{
					List<object> constructorArguments = new List<object>();
					List<string> propertyNames = new List<string>(node.Properties.Keys);
					ParameterInfo[] parameters = constructorInfo.GetParameters();
					foreach (ParameterInfo parameterInfo2 in parameters)
					{
						string targetParameterName = parameterInfo2.Name;
						string key = propertyNames.Find((string eachKey) => string.Compare(eachKey, targetParameterName, ignoreCase: true) == 0);
						constructorArguments.Add(ConversionHelper.ChangeType(node.Properties[key], parameterInfo2.ParameterType));
					}
					node.Instance = constructorInfo.Invoke(constructorArguments.ToArray());
				}
			}
			else
			{
				foreach (string property3 in node.GetProperties())
				{
					PropertyInfo property = type.GetProperty(property3);
					property?.SetValue(node.Instance, ConversionHelper.ChangeType(node.Properties[property3], property.PropertyType), null);
				}
			}
			if (!node.Properties.ContainsKey("Add-Target"))
			{
				((Control)parent).Controls.Add(node.Instance as Control);
			}
			else
			{
				string addTarget = node.Properties["Add-Target"];
				List<PropertyInfo> propertyInfoList = new List<PropertyInfo>(parent.GetType().GetProperties());
				PropertyInfo propertyInfo = propertyInfoList.Find((PropertyInfo eachProp) => eachProp.Name == addTarget);
				if (propertyInfo != null)
				{
					object target = propertyInfo.GetValue(parent, null);
					if (target != null)
					{
						List<Type> types = new List<Type>();
						types.Add(type);
						List<object> arguments = new List<object>();
						arguments.Add(node.Instance);
						List<MethodInfo> addMethods = new List<MethodInfo>(propertyInfo.PropertyType.GetMethods()).FindAll((MethodInfo eachMethod) => eachMethod.Name == "Add");
						MethodInfo methodInfo = null;
						List<string> addPropertyNames = new List<string>(node.GetAddProperties());
						foreach (MethodInfo addMethod in addMethods)
						{
							int matchedAddCount = 0;
							List<ParameterInfo> addParameters = new List<ParameterInfo>(addMethod.GetParameters());
							for (int i = 1; i < addParameters.Count; i++)
							{
								ParameterInfo parameterInfo3 = addParameters[i];
								string testName = $"add-{parameterInfo3.Name}";
								string addPropertyName = addPropertyNames.Find((string eachAddParam) => string.Compare(eachAddParam, testName, ignoreCase: true) == 0);
								if (addPropertyName != null)
								{
									types.Add(parameterInfo3.ParameterType);
									object argument = ConversionHelper.ChangeType(node.Properties[addPropertyName], parameterInfo3.ParameterType);
									arguments.Add(argument);
									matchedAddCount++;
								}
							}
							if (matchedAddCount == addParameters.Count - 1)
							{
								methodInfo = addMethod;
								break;
							}
						}
						methodInfo?.Invoke(target, arguments.ToArray());
					}
				}
			}
			if (node.Properties.ContainsKey("Name"))
			{
				string name = node.Properties["Name"];
				if (node.Instance is Component component)
				{
					KernelObject[name] = component;
				}
				foreach (EventHandlerData eventHandler in node.EventHandlers)
				{
					m_classScriptBuilder.AppendFormat("\t\tpage.{0}.add_{1}( {2} );\n", name, eventHandler.EventName, eventHandler.FunctionName);
					m_handlerScriptBuilder.AppendLine(eventHandler.SourceText);
				}
			}
			BuildNodes(node.Instance, node.Nodes);
		}
	}

	private static Type GetType(string typeName)
	{
		if (string.IsNullOrEmpty(typeName))
		{
			return null;
		}
		if (typeName.StartsWith("System.Windows.Forms"))
		{
			Assembly assembly = Assembly.GetAssembly(typeof(Control));
			return assembly.GetType(typeName);
		}
		return StringExtensions.ToType(typeName);
	}
}

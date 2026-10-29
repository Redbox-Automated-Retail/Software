using System;
using System.ComponentModel;
using System.Reflection;
using System.Xml;
using Redbox.Core;
using Redbox.Macros;
using Redbox.ScriptingHost.Framework;

namespace Redbox.Installer.Framework;

public class Definition
{
	protected readonly ScriptKernelObject m_scriptKernelObject = new ScriptKernelObject();

	public int Size { get; set; }

	[ElementProperty(Name = "id", IsRequired = true)]
	public string ID { get; set; }

	[ElementProperty(Name = "if")]
	public string IfExpression { get; set; }

	[DefaultValue(10)]
	[ElementProperty(Name = "install-order")]
	public virtual int InstallSortOrder { get; set; }

	[ElementProperty(Name = "uninstall-order")]
	[DefaultValue(10)]
	public virtual int UninstallSortOrder { get; set; }

	protected virtual bool SupportsConditionalExpression => true;

	public static DefinitionList Parse(XmlDocument xmlDocument, string scriptBasePath, bool checkFiles, IConsole console, ErrorList errors)
	{
		DefinitionList definitionList = new DefinitionList();
		if (xmlDocument == null || xmlDocument.DocumentElement == null)
		{
			return definitionList;
		}
		Type[] types = Assembly.GetAssembly(typeof(Definition)).GetTypes();
		foreach (Type type in types)
		{
			if (!type.IsSubclassOf(typeof(Definition)))
			{
				continue;
			}
			ElementAttribute elementAttribute = (ElementAttribute)Attribute.GetCustomAttribute(type, typeof(ElementAttribute));
			if (elementAttribute == null)
			{
				continue;
			}
			DisplayNameAttribute displayNameAttribute = (DisplayNameAttribute)Attribute.GetCustomAttribute(type, typeof(DisplayNameAttribute));
			PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty);
			XmlNodeList xmlNodeList = xmlDocument.DocumentElement.SelectNodes(elementAttribute.XPath);
			if (xmlNodeList == null)
			{
				continue;
			}
			if (xmlNodeList.Count > 0)
			{
				console.WriteLine("\nParsing {0}:", (displayNameAttribute != null) ? displayNameAttribute.DisplayName : "Unknown Definition");
			}
			foreach (XmlNode item in xmlNodeList)
			{
				if (!(Activator.CreateInstance(type) is Definition definition))
				{
					continue;
				}
				PropertyInfo[] array = properties;
				foreach (PropertyInfo propertyInfo in array)
				{
					ElementPropertyAttribute elementPropertyAttribute = (ElementPropertyAttribute)Attribute.GetCustomAttribute(propertyInfo, typeof(ElementPropertyAttribute));
					if (elementPropertyAttribute == null)
					{
						continue;
					}
					object obj = null;
					if (elementPropertyAttribute.UseNodeText)
					{
						if (string.IsNullOrEmpty(item.InnerText) && elementPropertyAttribute.IsRequired)
						{
							errors.Add(Error.NewError("C004", string.Format("The '{0}' element requires valid inner text.", (displayNameAttribute != null) ? displayNameAttribute.DisplayName : "Unknown Definition"), "Correct the RIX script file and recompile."));
							continue;
						}
						obj = MacroContainer.Instance.ExpandConstantMacros(item.InnerText);
					}
					else
					{
						XmlAttribute xmlAttribute = item.Attributes[elementPropertyAttribute.Name];
						if (xmlAttribute == null && elementPropertyAttribute.IsRequired)
						{
							errors.Add(Error.NewError("C004", string.Format("The '{0}' element requires a valid '{1}' attribute.", (displayNameAttribute != null) ? displayNameAttribute.DisplayName : "Unknown Definition", elementPropertyAttribute.Name), "Correct the RIX script file and recompile."));
							continue;
						}
						if (xmlAttribute != null)
						{
							obj = MacroContainer.Instance.ExpandConstantMacros(xmlAttribute.Value);
						}
					}
					if (obj == null)
					{
						DefaultValueAttribute defaultValueAttribute = (DefaultValueAttribute)Attribute.GetCustomAttribute(propertyInfo, typeof(DefaultValueAttribute));
						if (defaultValueAttribute != null)
						{
							obj = defaultValueAttribute.Value;
						}
					}
					if (obj != null)
					{
						obj = ConversionHelper.ChangeType(obj, propertyInfo.PropertyType);
					}
					if (propertyInfo.Name == "ID" && definitionList.GetByID(obj as string) != null)
					{
						errors.Add(Error.NewError("C004", string.Format("The id attribute '{0}' on the '{1}' element is not unique.", obj, (displayNameAttribute != null) ? displayNameAttribute.DisplayName : "Unknown Definition"), "Correct the RIX script file and recompile."));
					}
					else
					{
						propertyInfo.SetValue(definition, obj, null);
					}
				}
				console.Write("\tAdd '" + definition.ID + "'...");
				definition.OnCustomParse(item, console, errors);
				definitionList.Add(definition);
				console.WriteLine("done.");
			}
		}
		return definitionList;
	}

	public ErrorList Install(LibraryModule module, Action<Definition> progress)
	{
		ErrorList errorList = new ErrorList();
		if (EvaluateIfExpression(errorList))
		{
			InstallerProject.Instance.ActiveConsole.WriteLine(ToExecutionMessage());
			OnInstall(module, progress, errorList);
		}
		return errorList;
	}

	public ErrorList Uninstall(Action<Definition> progress)
	{
		ErrorList errorList = new ErrorList();
		if (EvaluateIfExpression(errorList))
		{
			InstallerProject.Instance.ActiveConsole.WriteLine(ToExecutionMessage());
			OnUninstall(progress, errorList);
		}
		return errorList;
	}

	public virtual int GetCountValue()
	{
		return 0;
	}

	public virtual int GetTaskCount()
	{
		return 0;
	}

	public virtual void LoadFromResource(LibraryModule module)
	{
	}

	public virtual void SaveToResource(ResourceUpdateSession session, int number, XmlDocument xmlDocument, string basePath, ErrorList errors)
	{
	}

	public virtual string ToExecutionMessage()
	{
		return ToString();
	}

	public bool EvaluateIfExpression(ErrorList errors)
	{
		bool result = true;
		if (!string.IsNullOrEmpty(IfExpression))
		{
			try
			{
				using VsaScriptingHost vsaScriptingHost = new VsaScriptingHost();
				vsaScriptingHost.SetKernelObject("rix", m_scriptKernelObject);
				result = (bool)vsaScriptingHost.EvaluateScript(IfExpression);
			}
			catch (Exception e)
			{
				errors?.Add(Error.NewError("C414", "An unhandled exception was raised within VSA hosted script while evaluating an IfExpression.", e));
				result = false;
			}
		}
		return result;
	}

	protected virtual void OnInstall(LibraryModule module, Action<Definition> progress, ErrorList errors)
	{
	}

	protected virtual void OnUninstall(Action<Definition> progress, ErrorList errors)
	{
	}

	protected virtual void OnCustomParse(XmlNode node, IConsole console, ErrorList errors)
	{
	}
}

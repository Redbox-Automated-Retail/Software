using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml;
using Microsoft.Win32;
using Redbox.Core;
using Redbox.IIS;
using Redbox.Macros;

namespace Redbox.Installer.Framework;

[Element(XPath = "website")]
[DisplayName("Website")]
public class WebsiteDefinition : Definition
{
	private string m_name;

	private string m_appPool;

	private string m_documentRoot;

	private string m_parentWebsite;

	private string m_appFriendlyName;

	private string m_defaultDocuments;

	private List<IIsScriptMap> m_scriptMaps;

	private List<IIsServerBinding> m_bindings;

	private List<IIsServerBinding> m_secureBindings;

	private static readonly List<IIsScriptMap> m_aspNetScriptMaps = new List<IIsScriptMap>
	{
		new IIsScriptMap
		{
			Extension = ".asax",
			Options = (IIsScriptMapOption.ScriptEngine | IIsScriptMapOption.VerifyFileExists)
		},
		new IIsScriptMap
		{
			Extension = ".ascx",
			Options = (IIsScriptMapOption.ScriptEngine | IIsScriptMapOption.VerifyFileExists)
		},
		new IIsScriptMap
		{
			Extension = ".ashx",
			Options = IIsScriptMapOption.ScriptEngine
		},
		new IIsScriptMap
		{
			Extension = ".asmx",
			Options = IIsScriptMapOption.ScriptEngine
		},
		new IIsScriptMap
		{
			Extension = ".aspx",
			Options = IIsScriptMapOption.ScriptEngine
		},
		new IIsScriptMap
		{
			Extension = ".axd",
			Options = IIsScriptMapOption.ScriptEngine
		},
		new IIsScriptMap
		{
			Extension = ".vsdisco",
			Options = IIsScriptMapOption.ScriptEngine
		},
		new IIsScriptMap
		{
			Extension = ".rem",
			Options = IIsScriptMapOption.ScriptEngine
		},
		new IIsScriptMap
		{
			Extension = ".soap",
			Options = IIsScriptMapOption.ScriptEngine
		},
		new IIsScriptMap
		{
			Extension = ".config",
			Options = (IIsScriptMapOption.ScriptEngine | IIsScriptMapOption.VerifyFileExists)
		},
		new IIsScriptMap
		{
			Extension = ".cs",
			Options = (IIsScriptMapOption.ScriptEngine | IIsScriptMapOption.VerifyFileExists)
		},
		new IIsScriptMap
		{
			Extension = ".csproj",
			Options = (IIsScriptMapOption.ScriptEngine | IIsScriptMapOption.VerifyFileExists)
		},
		new IIsScriptMap
		{
			Extension = ".vb",
			Options = (IIsScriptMapOption.ScriptEngine | IIsScriptMapOption.VerifyFileExists)
		},
		new IIsScriptMap
		{
			Extension = ".vbproj",
			Options = (IIsScriptMapOption.ScriptEngine | IIsScriptMapOption.VerifyFileExists)
		},
		new IIsScriptMap
		{
			Extension = ".webinfo",
			Options = (IIsScriptMapOption.ScriptEngine | IIsScriptMapOption.VerifyFileExists)
		},
		new IIsScriptMap
		{
			Extension = ".licx",
			Options = (IIsScriptMapOption.ScriptEngine | IIsScriptMapOption.VerifyFileExists)
		},
		new IIsScriptMap
		{
			Extension = ".resx",
			Options = (IIsScriptMapOption.ScriptEngine | IIsScriptMapOption.VerifyFileExists)
		},
		new IIsScriptMap
		{
			Extension = ".resources",
			Options = (IIsScriptMapOption.ScriptEngine | IIsScriptMapOption.VerifyFileExists)
		},
		new IIsScriptMap
		{
			Extension = ".master",
			Options = (IIsScriptMapOption.ScriptEngine | IIsScriptMapOption.VerifyFileExists)
		},
		new IIsScriptMap
		{
			Extension = ".skin",
			Options = (IIsScriptMapOption.ScriptEngine | IIsScriptMapOption.VerifyFileExists)
		},
		new IIsScriptMap
		{
			Extension = ".compiled",
			Options = (IIsScriptMapOption.ScriptEngine | IIsScriptMapOption.VerifyFileExists)
		},
		new IIsScriptMap
		{
			Extension = ".browser",
			Options = (IIsScriptMapOption.ScriptEngine | IIsScriptMapOption.VerifyFileExists)
		},
		new IIsScriptMap
		{
			Extension = ".mdb",
			Options = (IIsScriptMapOption.ScriptEngine | IIsScriptMapOption.VerifyFileExists)
		},
		new IIsScriptMap
		{
			Extension = ".jsl",
			Options = (IIsScriptMapOption.ScriptEngine | IIsScriptMapOption.VerifyFileExists)
		},
		new IIsScriptMap
		{
			Extension = ".vjsproj",
			Options = (IIsScriptMapOption.ScriptEngine | IIsScriptMapOption.VerifyFileExists)
		},
		new IIsScriptMap
		{
			Extension = ".sitemap",
			Options = (IIsScriptMapOption.ScriptEngine | IIsScriptMapOption.VerifyFileExists)
		},
		new IIsScriptMap
		{
			Extension = ".msgx",
			Options = IIsScriptMapOption.ScriptEngine
		},
		new IIsScriptMap
		{
			Extension = ".ad",
			Options = (IIsScriptMapOption.ScriptEngine | IIsScriptMapOption.VerifyFileExists)
		},
		new IIsScriptMap
		{
			Extension = ".dd",
			Options = (IIsScriptMapOption.ScriptEngine | IIsScriptMapOption.VerifyFileExists)
		},
		new IIsScriptMap
		{
			Extension = ".ldd",
			Options = (IIsScriptMapOption.ScriptEngine | IIsScriptMapOption.VerifyFileExists)
		},
		new IIsScriptMap
		{
			Extension = ".sd",
			Options = (IIsScriptMapOption.ScriptEngine | IIsScriptMapOption.VerifyFileExists)
		},
		new IIsScriptMap
		{
			Extension = ".cd",
			Options = (IIsScriptMapOption.ScriptEngine | IIsScriptMapOption.VerifyFileExists)
		},
		new IIsScriptMap
		{
			Extension = ".adprototype",
			Options = (IIsScriptMapOption.ScriptEngine | IIsScriptMapOption.VerifyFileExists)
		},
		new IIsScriptMap
		{
			Extension = ".lddprototype",
			Options = (IIsScriptMapOption.ScriptEngine | IIsScriptMapOption.VerifyFileExists)
		},
		new IIsScriptMap
		{
			Extension = ".sdm",
			Options = (IIsScriptMapOption.ScriptEngine | IIsScriptMapOption.VerifyFileExists)
		},
		new IIsScriptMap
		{
			Extension = ".sdmDocument",
			Options = (IIsScriptMapOption.ScriptEngine | IIsScriptMapOption.VerifyFileExists)
		},
		new IIsScriptMap
		{
			Extension = ".ldb",
			Options = (IIsScriptMapOption.ScriptEngine | IIsScriptMapOption.VerifyFileExists)
		},
		new IIsScriptMap
		{
			Extension = ".svc",
			Options = IIsScriptMapOption.ScriptEngine
		},
		new IIsScriptMap
		{
			Extension = ".mdf",
			Options = (IIsScriptMapOption.ScriptEngine | IIsScriptMapOption.VerifyFileExists)
		},
		new IIsScriptMap
		{
			Extension = ".ldf",
			Options = (IIsScriptMapOption.ScriptEngine | IIsScriptMapOption.VerifyFileExists)
		},
		new IIsScriptMap
		{
			Extension = ".java",
			Options = (IIsScriptMapOption.ScriptEngine | IIsScriptMapOption.VerifyFileExists)
		},
		new IIsScriptMap
		{
			Extension = ".exclude",
			Options = (IIsScriptMapOption.ScriptEngine | IIsScriptMapOption.VerifyFileExists)
		},
		new IIsScriptMap
		{
			Extension = ".refresh",
			Options = (IIsScriptMapOption.ScriptEngine | IIsScriptMapOption.VerifyFileExists)
		}
	};

	[ElementProperty(Name = "name", IsRequired = true)]
	public string Name
	{
		get
		{
			return MacroContainer.Instance.ExpandRuntimeMacros(m_name);
		}
		set
		{
			m_name = value;
		}
	}

	[ElementProperty(Name = "auth-ntlm")]
	public bool AuthNTLM { get; set; }

	[ElementProperty(Name = "app-pool")]
	public string AppPool
	{
		get
		{
			return MacroContainer.Instance.ExpandRuntimeMacros(m_appPool);
		}
		set
		{
			m_appPool = value;
		}
	}

	[ElementProperty(Name = "access-read")]
	public bool AccessRead { get; set; }

	[ElementProperty(Name = "access-write")]
	public bool AccessWrite { get; set; }

	[ElementProperty(Name = "auth-anonymous")]
	public bool AuthAnonymous { get; set; }

	[ElementProperty(Name = "access-execute")]
	public bool AccessExecute { get; set; }

	[ElementProperty(Name = "path", IsRequired = true)]
	public string DocumentRoot
	{
		get
		{
			return MacroContainer.Instance.ExpandRuntimeMacros(m_documentRoot);
		}
		set
		{
			m_documentRoot = value;
		}
	}

	[ElementProperty(Name = "parent-website")]
	public string ParentWebsite
	{
		get
		{
			return MacroContainer.Instance.ExpandRuntimeMacros(m_parentWebsite);
		}
		set
		{
			m_parentWebsite = value;
		}
	}

	[ElementProperty(Name = "app-friendly-name")]
	public string AppFriendlyName
	{
		get
		{
			return MacroContainer.Instance.ExpandRuntimeMacros(m_appFriendlyName);
		}
		set
		{
			m_appFriendlyName = value;
		}
	}

	[ElementProperty(Name = "enable-dir-browsing")]
	public bool EnableDirBrowsing { get; set; }

	[ElementProperty(Name = "default-documents")]
	public string DefaultDocuments
	{
		get
		{
			return MacroContainer.Instance.ExpandRuntimeMacros(m_defaultDocuments);
		}
		set
		{
			m_defaultDocuments = value;
		}
	}

	public List<IIsScriptMap> ScriptMaps
	{
		get
		{
			if (m_scriptMaps == null)
			{
				m_scriptMaps = new List<IIsScriptMap>();
			}
			return m_scriptMaps;
		}
	}

	public List<IIsServerBinding> Bindings
	{
		get
		{
			if (m_bindings == null)
			{
				m_bindings = new List<IIsServerBinding>();
			}
			return m_bindings;
		}
	}

	[ElementProperty(Name = "asp-net-version")]
	public AspNetVersion? AspNetVersionToMap { get; set; }

	public List<IIsServerBinding> SecureBindings
	{
		get
		{
			if (m_secureBindings == null)
			{
				m_secureBindings = new List<IIsServerBinding>();
			}
			return m_secureBindings;
		}
	}

	[DefaultValue(11)]
	public override int InstallSortOrder { get; set; }

	[DefaultValue(2)]
	public override int UninstallSortOrder { get; set; }

	public override int GetTaskCount()
	{
		return 1;
	}

	public override string ToExecutionMessage()
	{
		return string.Format("{0} IIS {1} '{2}'", (InstallerProject.Instance.Mode == InstallMode.Install) ? "Create" : "Remove", IsVirtualDirectory() ? "virtual directory" : "website", Name);
	}

	public bool IsVirtualDirectory()
	{
		return !string.IsNullOrEmpty(ParentWebsite);
	}

	protected override void OnInstall(LibraryModule module, Action<Definition> progress, ErrorList errors)
	{
		try
		{
			List<string> list = new List<string>();
			string[] array = (string[])Registry.GetValue(InstallerProject.Instance.GetUninstallKey(), "IIsObjects", new string[0]);
			if (array != null)
			{
				list.AddRange(array);
			}
			IIsVirtualDirectory isVirtualDirectory = null;
			IIsWebsite isWebsite;
			if (!IsVirtualDirectory())
			{
				foreach (IIsServerBinding binding in Bindings)
				{
					binding.HostHeader = MacroContainer.Instance.ExpandRuntimeMacros(binding.HostHeader);
					binding.IpAddress = MacroContainer.Instance.ExpandRuntimeMacros(binding.IpAddress);
					binding.TcpPort = MacroContainer.Instance.ExpandRuntimeMacros(binding.TcpPort);
				}
				isWebsite = IIsWebsite.GetByName(Name);
				if (isWebsite == null)
				{
					isWebsite = IIsWebsite.CreateNewSite(Name, DocumentRoot, Bindings);
				}
				else if (Bindings.Count > 0)
				{
					isWebsite.ServerBindings = Bindings;
				}
				isWebsite.ServerState = IIsServerState.Started;
				isWebsite.ServerAutoStart = true;
				isWebsite.Save();
				isWebsite.Start();
				isVirtualDirectory = isWebsite.Root;
			}
			else
			{
				isWebsite = IIsWebsite.GetByName(ParentWebsite);
				if (isWebsite != null)
				{
					isVirtualDirectory = isWebsite.CreateVirtualDirectory(Name);
				}
			}
			if (isVirtualDirectory != null)
			{
				if (DefaultDocuments != null)
				{
					isVirtualDirectory.DefaultDocument = DefaultDocuments;
				}
				isVirtualDirectory.Path = DocumentRoot;
				isVirtualDirectory.AuthNTLM = AuthNTLM;
				isVirtualDirectory.AccessRead = AccessRead;
				isVirtualDirectory.AccessWrite = AccessWrite;
				isVirtualDirectory.AccessExecute = AccessExecute;
				isVirtualDirectory.AuthAnonymous = AuthAnonymous;
				isVirtualDirectory.AppFriendlyName = AppFriendlyName;
				isVirtualDirectory.EnableDirBrowsing = EnableDirBrowsing;
				isVirtualDirectory.EnableDefaultDoc = DefaultDocuments != null;
				if (AspNetVersionToMap.HasValue)
				{
					SetAspNetVersion(isVirtualDirectory, AspNetVersionToMap.Value);
				}
				if (ScriptMaps.Count > 0)
				{
					List<IIsScriptMap> scriptMaps = isVirtualDirectory.ScriptMaps;
					foreach (IIsScriptMap scriptMap in ScriptMaps)
					{
						scriptMap.ExecutablePath = MacroContainer.Instance.ExpandRuntimeMacros(scriptMap.ExecutablePath);
						IIsScriptMapAction? action = scriptMap.Action;
						if (action.GetValueOrDefault() == IIsScriptMapAction.Set && action.HasValue)
						{
							scriptMaps.Add(scriptMap);
							continue;
						}
						string currentExtension = scriptMap.Extension;
						int num = scriptMaps.FindIndex((IIsScriptMap eachExistingMap) => string.Compare(eachExistingMap.Extension, currentExtension, ignoreCase: true) == 0);
						if (num != -1)
						{
							scriptMaps.RemoveAt(num);
						}
					}
					isVirtualDirectory.ScriptMaps = scriptMaps;
				}
				isVirtualDirectory.CreateApplication(AppPool, ApplicationIsolation.Outproc);
				isVirtualDirectory.Save();
				list.Add(string.Format("{0}|{1}", IsVirtualDirectory() ? "vdir" : "site", IsVirtualDirectory() ? (isWebsite.Name + "/" + Name) : isWebsite.Name));
				Registry.SetValue(InstallerProject.Instance.GetUninstallKey(), "IIsObjects", list.ToArray(), RegistryValueKind.MultiString);
			}
		}
		catch (Exception ex)
		{
			errors.Add(Error.NewError("I015", "Unable to create website or virtual directory due to unhandled exception; see details for exception.", ex.ToString()));
		}
		base.OnInstall(module, progress, errors);
	}

	protected override void OnUninstall(Action<Definition> progress, ErrorList errors)
	{
		try
		{
			string[] array = (string[])Registry.GetValue(InstallerProject.Instance.GetUninstallKey(), "IIsObjects", new string[0]);
			foreach (string text in array)
			{
				try
				{
					string[] parts = text.Split('|');
					if (parts[0] == "vdir")
					{
						string[] array4 = parts[1].Split('/');
						IIsWebsite.GetByID(int.Parse(array4[0]))?.GetVirtualDirectoryByName(array4[1])?.Delete();
					}
					else
					{
						IIsWebsite.GetByID(int.Parse(parts[1]))?.Delete();
					}
				}
				catch
				{
				}
			}
		}
		catch (Exception ex)
		{
			errors.Add(Error.NewError("I015", "Unable to remove website or virtual directory due to unhandled exception; see details for exception.", ex.ToString()));
		}
		base.OnUninstall(progress, errors);
	}

	protected override void OnCustomParse(XmlNode node, IConsole console, ErrorList errors)
	{
		XmlNodeList xmlNodeList = node.SelectNodes("bindings/*");
		if (xmlNodeList != null)
		{
			foreach (XmlNode item2 in xmlNodeList)
			{
				bool flag = string.Compare(item2.GetExpandedAttributeValue<string>("type"), "secure", ignoreCase: true) == 0;
				IIsServerBinding item = new IIsServerBinding(item2.GetExpandedAttributeValue<string>("ip-address"), item2.GetExpandedAttributeValue("tcp-port", "80"), item2.GetExpandedAttributeValue<string>("host-header"));
				if (!flag)
				{
					Bindings.Add(item);
				}
				else
				{
					SecureBindings.Add(item);
				}
			}
		}
		if (Bindings.Count == 0)
		{
			Bindings.Add(new IIsServerBinding(null, "80", null));
		}
		XmlNodeList xmlNodeList2 = node.SelectNodes("script-maps/*");
		if (xmlNodeList2 == null)
		{
			return;
		}
		foreach (XmlNode item3 in xmlNodeList2)
		{
			IIsScriptMap isScriptMap = new IIsScriptMap();
			isScriptMap.Action = ((!(item3.Name == "set")) ? IIsScriptMapAction.Remove : IIsScriptMapAction.Set);
			isScriptMap.Extension = item3.GetExpandedAttributeValue<string>("extension");
			isScriptMap.ExecutablePath = item3.GetExpandedAttributeValue<string>("executable-path");
			IIsScriptMap isScriptMap2 = isScriptMap;
			if (string.IsNullOrEmpty(isScriptMap2.Extension) || !isScriptMap2.Extension.StartsWith("."))
			{
				errors.Add(Error.NewError("C004", "The website/script-maps set or remove node requires a valid extension attribute.", "Specify a valid extension attribute."));
				continue;
			}
			if (string.IsNullOrEmpty(isScriptMap2.ExecutablePath))
			{
				IIsScriptMapAction? action = isScriptMap2.Action;
				if (action.GetValueOrDefault() == IIsScriptMapAction.Set && action.HasValue)
				{
					errors.Add(Error.NewError("C004", "The website/script-maps set node requires a valid executable-path attribute.", "Specify a valid executable-path attribute."));
					continue;
				}
			}
			string expandedAttributeValue = item3.GetExpandedAttributeValue<string>("options");
			if (expandedAttributeValue != null)
			{
				isScriptMap2.Options = (IIsScriptMapOption)ConversionHelper.ChangeType(expandedAttributeValue, typeof(IIsScriptMapOption));
			}
			string expandedAttributeValue2 = item3.GetExpandedAttributeValue<string>("verbs");
			if (expandedAttributeValue2 != null)
			{
				isScriptMap2.Verbs.AddRange(expandedAttributeValue2.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries));
			}
			ScriptMaps.Add(isScriptMap2);
		}
	}

	internal static void SetAspNetVersion(IIsVirtualDirectory vdir, AspNetVersion version)
	{
		foreach (IIsScriptMap aspNetScriptMap in m_aspNetScriptMaps)
		{
			string arg = version switch
			{
				AspNetVersion.AspNet11 => "dotnetfx11-path", 
				AspNetVersion.AspNet20 => "dotnetfx20-path", 
				AspNetVersion.AspNet35 => "dotnetfx35-path", 
				AspNetVersion.AspNet40 => "dotnetfx40-path", 
				_ => throw new ApplicationException($"Unhandled AspNetVersion value: {version}!"), 
			};
			aspNetScriptMap.ExecutablePath = MacroContainer.Instance.ExpandRuntimeMacros($"${{{arg}}}\\aspnet_isapi.dll");
			if (aspNetScriptMap.Verbs.Count == 0)
			{
				aspNetScriptMap.Verbs.AddRange(new string[4] { "GET", "HEAD", "POST", "DEBUG" });
			}
		}
		vdir.ScriptMaps = m_aspNetScriptMaps;
	}
}

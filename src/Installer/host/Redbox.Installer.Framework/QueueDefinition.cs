using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Messaging;
using System.Xml;
using Redbox.Core;
using Redbox.Macros;

namespace Redbox.Installer.Framework;

[DisplayName("Queue")]
[Element(XPath = "queue")]
public class QueueDefinition : Definition
{
	internal class QueuePermission
	{
		public string User { get; set; }

		public MessageQueueAccessRights Rights { get; set; }
	}

	private string m_name;

	public bool m_transactional;

	private List<QueuePermission> m_permissions;

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

	[ElementProperty(Name = "transactional")]
	public bool Transactional
	{
		get
		{
			return m_transactional;
		}
		set
		{
			m_transactional = value;
		}
	}

	[DefaultValue(16)]
	public override int InstallSortOrder { get; set; }

	[DefaultValue(6)]
	public override int UninstallSortOrder { get; set; }

	private List<QueuePermission> Permissions
	{
		get
		{
			if (m_permissions == null)
			{
				m_permissions = new List<QueuePermission>();
			}
			return m_permissions;
		}
	}

	public override int GetTaskCount()
	{
		return 1;
	}

	public override string ToExecutionMessage()
	{
		return string.Format("{0} queue '{1}'", (InstallerProject.Instance.Mode == InstallMode.Install) ? "Create" : "Remove", Name);
	}

	protected override void OnInstall(LibraryModule module, Action<Definition> progress, ErrorList errors)
	{
		try
		{
			string path = FormatQueueName(Name);
			if (MessageQueue.Exists(path))
			{
				MessageQueue.Delete(path);
			}
			using MessageQueue messageQueue = MessageQueue.Create(path, Transactional);
			foreach (QueuePermission permission in Permissions)
			{
				messageQueue.SetPermissions(permission.User, permission.Rights);
			}
		}
		catch (Exception ex)
		{
			errors.Add(Error.NewError("I015", $"Unable to create private queue {Name}; see details for exception.", ex.ToString()));
		}
		base.OnInstall(module, progress, errors);
	}

	protected override void OnUninstall(Action<Definition> progress, ErrorList errors)
	{
		try
		{
			string path = FormatQueueName(Name);
			if (MessageQueue.Exists(path))
			{
				MessageQueue.Delete(path);
			}
		}
		catch (Exception ex)
		{
			errors.Add(Error.NewError("I015", $"Unable to remove private queue {Name}; see details for exception.", ex.ToString()));
		}
		base.OnUninstall(progress, errors);
	}

	protected override void OnCustomParse(XmlNode node, IConsole console, ErrorList errors)
	{
		foreach (XmlNode childNode in node.ChildNodes)
		{
			if (childNode.Name != "permission")
			{
				errors.Add(Error.NewError("C004", "Only permission is a valid under the queue node.", "Specify a node named member."));
				continue;
			}
			string expandedAttributeValue = childNode.GetExpandedAttributeValue<string>("name");
			if (string.IsNullOrEmpty(expandedAttributeValue))
			{
				errors.Add(Error.NewError("C004", "The permission node requires a valid name attribute.", "Specify a valid name attribute."));
			}
			string expandedAttributeValue2 = childNode.GetExpandedAttributeValue<string>("rights");
			if (expandedAttributeValue2 != null)
			{
				MessageQueueAccessRights messageQueueAccessRights = (MessageQueueAccessRights)ConversionHelper.ChangeType(expandedAttributeValue2, typeof(MessageQueueAccessRights));
				if (messageQueueAccessRights != 0)
				{
					Permissions.Add(new QueuePermission
					{
						User = expandedAttributeValue,
						Rights = messageQueueAccessRights
					});
				}
			}
		}
	}

	internal static string FormatQueueName(string name)
	{
		return $".\\private$\\{name}";
	}
}

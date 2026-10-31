using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Xml;
using Redbox.Core;
using Redbox.Macros;

namespace Redbox.Installer.Framework;

[Element(XPath = "page")]
[DisplayName("Page")]
public class PageDefinition : Definition, IDisposable
{
	private string m_text;

	private string m_title;

	private string m_subTitle;

	private string m_template;

	private bool? m_allowCancel;

	private string m_proceedText;

	private bool? m_allowMoveNext;

	private bool? m_allowMovePrevious;

	private PageTemplateDefinition m_pageTemplate;

	private IDictionary<string, string> m_properties;

	[ElementProperty(UseNodeText = true)]
	public string Text
	{
		get
		{
			if (m_text != null || PageTemplate == null)
			{
				return MacroContainer.Instance.ExpandRuntimeMacros(m_text);
			}
			return PageTemplate.Text;
		}
		set
		{
			m_text = value;
		}
	}

	[ElementProperty(Name = "title")]
	public string Title
	{
		get
		{
			if (m_title != null || PageTemplate == null)
			{
				return MacroContainer.Instance.ExpandRuntimeMacros(m_title);
			}
			return PageTemplate.Title;
		}
		set
		{
			m_title = value;
		}
	}

	[ElementProperty(Name = "type")]
	[DefaultValue(PageType.Custom)]
	public PageType Type { get; set; }

	[ElementProperty(Name = "template")]
	public string Template
	{
		get
		{
			return MacroContainer.Instance.ExpandRuntimeMacros(m_template);
		}
		set
		{
			m_template = value;
		}
	}

	[ElementProperty(Name = "subtitle")]
	public string SubTitle
	{
		get
		{
			if (m_subTitle != null || PageTemplate == null)
			{
				return MacroContainer.Instance.ExpandRuntimeMacros(m_subTitle);
			}
			return PageTemplate.SubTitle;
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
			if (m_proceedText != null || PageTemplate == null)
			{
				return MacroContainer.Instance.ExpandRuntimeMacros(m_proceedText);
			}
			return PageTemplate.ProceedText;
		}
		set
		{
			m_proceedText = value;
		}
	}

	[ElementProperty(Name = "allow-cancel")]
	public bool? AllowCancel
	{
		get
		{
			if (!m_allowCancel.HasValue && PageTemplate != null)
			{
				return PageTemplate.AllowCancel;
			}
			return m_allowCancel ?? true;
		}
		set
		{
			m_allowCancel = value;
		}
	}

	[ElementProperty(Name = "phase")]
	public WizardPhase Phase { get; set; }

	[ElementProperty(Name = "allow-move-next")]
	public bool? AllowMoveNext
	{
		get
		{
			if (!m_allowMoveNext.HasValue && PageTemplate != null)
			{
				return PageTemplate.AllowMoveNext;
			}
			return m_allowMoveNext ?? true;
		}
		set
		{
			m_allowMoveNext = value;
		}
	}

	[ElementProperty(Name = "allow-move-previous")]
	public bool? AllowMovePrevious
	{
		get
		{
			if (!m_allowMovePrevious.HasValue && PageTemplate != null)
			{
				return PageTemplate.AllowMovePrevious;
			}
			return m_allowMovePrevious ?? true;
		}
		set
		{
			m_allowMovePrevious = value;
		}
	}

	public PageTemplateDefinition PageTemplate
	{
		get
		{
			if (m_pageTemplate == null)
			{
				m_pageTemplate = InstallerProject.Instance.Definitions.GetByID(Template) as PageTemplateDefinition;
				if (m_pageTemplate != null)
				{
					foreach (string key in Properties.Keys)
					{
						m_pageTemplate.KernelObject[key] = Properties[key];
					}
				}
			}
			return m_pageTemplate;
		}
	}

	public IDictionary<string, string> Properties
	{
		get
		{
			if (m_properties == null)
			{
				m_properties = new Dictionary<string, string>();
			}
			return m_properties;
		}
	}

	public void Dispose()
	{
		if (PageTemplate != null)
		{
			PageTemplate.Dispose();
		}
	}

	public bool Activate()
	{
		bool result = false;
		if (PageTemplate != null)
		{
			result = PageTemplate.Activate();
		}
		return result;
	}

	public bool Validate()
	{
		bool result = false;
		if (PageTemplate != null)
		{
			result = PageTemplate.Validate();
		}
		return result;
	}

	public void Configure()
	{
		if (PageTemplate != null)
		{
			PageTemplate.Configure();
		}
	}

	public void Deactivate()
	{
		if (PageTemplate != null)
		{
			PageTemplate.Deactivate();
		}
	}

	public ErrorList BuildHost(out Control control)
	{
		control = null;
		if (PageTemplate == null)
		{
			return new ErrorList();
		}
		return PageTemplate.BuildHost(base.ID, out control);
	}

	protected override void OnCustomParse(XmlNode node, IConsole console, ErrorList errors)
	{
		XmlNodeList xmlNodeList = node.SelectNodes("property");
		if (xmlNodeList == null || xmlNodeList.Count == 0)
		{
			return;
		}
		foreach (XmlNode item in xmlNodeList)
		{
			string expandedAttributeValue = item.GetExpandedAttributeValue<string>("name");
			if (string.IsNullOrEmpty(expandedAttributeValue))
			{
				errors.Add(Error.NewError("C003", "The property node must have a valid name attribute.", "Specify a valid Name attribute."));
				continue;
			}
			string value = item.GetExpandedAttributeValue<string>("value");
			if (string.IsNullOrEmpty(value))
			{
				value = item.GetExpandedNodeValue<string>();
				if (string.IsNullOrEmpty(value))
				{
					value = Redbox.Core.XmlNodeExtensions.GetNodeInnerXml(item);
				}
			}
			if (string.IsNullOrEmpty(value))
			{
				errors.Add(Error.NewError("C003", "The property node must have a valid value attribute.", "Specify a valid Value attribute."));
			}
			else
			{
				Properties.Add(expandedAttributeValue, value);
			}
		}
	}
}

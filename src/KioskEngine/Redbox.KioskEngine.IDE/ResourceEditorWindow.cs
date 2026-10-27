using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.REDS.Framework;
using WeifenLuo.WinFormsUI.Docking;

namespace Redbox.KioskEngine.IDE;

public class ResourceEditorWindow : DockContent
{
	private IResource m_resource;

	private IContainer components;

	private ToolStrip m_toolStrip;

	private Panel m_panel;

	public IResource Resource
	{
		get
		{
			return m_resource;
		}
		set
		{
			m_resource = value;
			Text = m_resource.Name;
			SetAspects();
		}
	}

	public ResourceEditorWindow()
	{
		InitializeComponent();
	}

	public void SetDebugState(int lineNumber, string error)
	{
		string key = "text_editor_" + m_resource.Name;
		if (m_panel.Controls.ContainsKey(key))
		{
			((TextEditor)m_panel.Controls[key]).SetCurrentDebugLine(lineNumber, error);
		}
	}

	private void SetAspects()
	{
		foreach (IAspectType eachAspect in m_resource.Type.AspectTypes)
		{
			string text = eachAspect["label"] ?? eachAspect.Name;
			ToolStripButton toolStripButton = new ToolStripButton(text)
			{
				DisplayStyle = ToolStripItemDisplayStyle.ImageAndText,
				Tag = eachAspect
			};
			toolStripButton.Click += delegate
			{
				ResetToolStripButtons();
				toolStripButton.Checked = true;
				m_panel.SuspendLayout();
				m_panel.Controls.Clear();
				if (eachAspect["editor_type"] == "text")
				{
					TextEditor textEditor = new TextEditor
					{
						Name = "text_editor_" + m_resource.Name
					};
					if (eachAspect["syntax_schema"] != null)
					{
						textEditor.SetSyntaxResource(eachAspect["syntax_schema"]);
					}
					textEditor.SetDocumentText(Resource.GetAspect(eachAspect.Name).GetContent() as string);
					textEditor.Dock = DockStyle.Fill;
					m_panel.Controls.Add(textEditor);
				}
				else if (eachAspect["editor_type"] == "table")
				{
					SplitContainer splitContainer = new SplitContainer
					{
						Dock = DockStyle.Fill,
						Location = new Point(0, 0),
						Name = "splitContainerView",
						BorderStyle = BorderStyle.Fixed3D,
						Orientation = Orientation.Horizontal
					};
					TextEditor textEditor2 = new TextEditor();
					textEditor2.SetSyntaxResource("XML");
					textEditor2.Name = $"text_editor_{m_resource.Name}";
					if (Resource.GetAspect(eachAspect.Name).GetContent() is XmlLinkedNode xmlLinkedNode)
					{
						StringWriter stringWriter = new StringWriter(new StringBuilder());
						XmlTextWriter xmlTextWriter = new XmlTextWriter(stringWriter)
						{
							Formatting = Formatting.Indented
						};
						xmlLinkedNode.OwnerDocument.Save(xmlTextWriter);
						xmlTextWriter.Close();
						textEditor2.SetDocumentText(stringWriter.ToString());
					}
					textEditor2.Dock = DockStyle.Fill;
					splitContainer.Panel2.Controls.Add(textEditor2);
					m_panel.Controls.Add(splitContainer);
				}
				else if (eachAspect["editor_type"] == "view")
				{
					XmlNode xmlNode = (Resource.GetAspect(eachAspect.Name).GetContent() as XmlLinkedNode).SelectSingleNode("scene");
					_ = xmlNode.Attributes["name"].Value;
					ServiceLocator.Instance.GetService<IRenderingService>();
					IViewService service = ServiceLocator.Instance.GetService<IViewService>();
					service.Push(m_resource.Name);
					int num = 0;
					int num2 = 0;
					IViewFrame viewFrame = service.GetViewFrame(m_resource.Name) as IViewFrame;
					int num3;
					int num4;
					if (viewFrame != null && viewFrame.ViewWindow.HasValue)
					{
						num3 = viewFrame.ViewWindow.Value.Height;
						num4 = viewFrame.ViewWindow.Value.Width;
						num = viewFrame.ViewWindow.Value.X;
						num2 = viewFrame.ViewWindow.Value.Y;
					}
					else
					{
						num3 = int.Parse(xmlNode.Attributes["height"].Value);
						num4 = int.Parse(xmlNode.Attributes["width"].Value);
					}
					Rectangle targetRectangle = new Rectangle(0, 0, num4 + num, num3 + num2);
					IScene scene = viewFrame.Scene;
					service.ShowFrameInDebug();
					System.Windows.Forms.PictureBox pictureBox = new System.Windows.Forms.PictureBox();
					Bitmap bitmap = new Bitmap(num4 + num, num3 + num2);
					Graphics targetGraphics = Graphics.FromImage(bitmap);
					using (BufferedGraphics bufferedGraphics = BufferedGraphicsManager.Current.Allocate(targetGraphics, targetRectangle))
					{
						scene.MakeDirty(null);
						scene.Render(bufferedGraphics.Graphics, out var _);
						bufferedGraphics.Render();
					}
					if (num + num2 > 0)
					{
						Rectangle rect = new Rectangle(num, num2, num4, num3);
						pictureBox.Image = bitmap.Clone(rect, bitmap.PixelFormat);
					}
					else
					{
						pictureBox.Image = bitmap;
					}
					pictureBox.Dock = DockStyle.Fill;
					pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
					m_panel.Controls.Add(pictureBox);
					service.PopDiscard();
					service.Show();
				}
				else if (!(eachAspect["editor_type"] == "schema"))
				{
					if (m_resource.Name == "manifest")
					{
						TextEditor textEditor3 = new TextEditor();
						textEditor3.SetSyntaxResource("XML");
						IResourceBundleService service2 = ServiceLocator.Instance.GetService<IResourceBundleService>();
						object obj = m_resource["product_name"];
						textEditor3.Name = "text_editor_" + obj?.ToString() + m_resource.Name;
						foreach (IBundleSpecifier bundle2 in service2.ActiveBundleSet.GetBundles())
						{
							if (bundle2.Name == (string)obj)
							{
								IResourceBundle bundle = service2.GetBundle(bundle2);
								IResourceBundleStorageEntry entry = bundle.Storage.GetEntry("manifest.resource");
								string documentText = Encoding.UTF8.GetString(bundle.Storage.GetEntryContent(entry));
								textEditor3.SetDocumentText(documentText);
								break;
							}
						}
						textEditor3.Dock = DockStyle.Fill;
						m_panel.Controls.Add(textEditor3);
					}
					else if (eachAspect["editor_type"] == "bitmap")
					{
						System.Windows.Forms.PictureBox pictureBox2 = new System.Windows.Forms.PictureBox();
						MemoryStream stream = new MemoryStream((byte[])Resource.GetAspect("content").GetContent());
						pictureBox2.Image = Image.FromStream(stream);
						pictureBox2.Dock = DockStyle.Fill;
						m_panel.Controls.Add(pictureBox2);
					}
				}
				m_panel.ResumeLayout();
			};
			m_toolStrip.Items.Add(toolStripButton);
		}
		if (m_toolStrip.Items.Count > 0)
		{
			m_toolStrip.Items[0].PerformClick();
		}
	}

	private void ResetToolStripButtons()
	{
		foreach (ToolStripButton item in m_toolStrip.Items)
		{
			item.Checked = false;
		}
	}

	public static string RemoveTroublesomeCharacters(string inString)
	{
		if (inString == null)
		{
			return null;
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (char c in inString)
		{
			if ((c < 'ý' && c > '\u001f') || c == '\t' || c == '\n' || c == '\r')
			{
				stringBuilder.Append(c);
			}
		}
		return stringBuilder.ToString();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		this.m_toolStrip = new System.Windows.Forms.ToolStrip();
		this.m_panel = new System.Windows.Forms.Panel();
		base.SuspendLayout();
		this.m_toolStrip.Dock = System.Windows.Forms.DockStyle.Bottom;
		this.m_toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
		this.m_toolStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
		this.m_toolStrip.Location = new System.Drawing.Point(0, 485);
		this.m_toolStrip.Name = "m_toolStrip";
		this.m_toolStrip.ShowItemToolTips = false;
		this.m_toolStrip.Size = new System.Drawing.Size(723, 25);
		this.m_toolStrip.Stretch = true;
		this.m_toolStrip.TabIndex = 0;
		this.m_panel.AutoSize = true;
		this.m_panel.Dock = System.Windows.Forms.DockStyle.Fill;
		this.m_panel.Location = new System.Drawing.Point(0, 0);
		this.m_panel.Name = "m_panel";
		this.m_panel.Size = new System.Drawing.Size(1523, 1285);
		this.m_panel.TabIndex = 1;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(923, 810);
		base.Controls.Add(this.m_panel);
		base.Controls.Add(this.m_toolStrip);
		this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		base.Name = "ResourceEditorWindow";
		this.Text = "Resource Editor";
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}

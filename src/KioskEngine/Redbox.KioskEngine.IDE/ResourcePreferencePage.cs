using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.REDS.Framework;

namespace Redbox.KioskEngine.IDE;

public class ResourcePreferencePage : UserControl, IPreferencePageHost
{
	internal sealed class ScoredResource
	{
		public int Score { get; set; }

		public IResource Resource { get; set; }
	}

	private IContainer components;

	private ErrorProvider m_errorProvider;

	private Label m_ResourceToScoreLabel;

	private TextBox m_resourceNameTextBox;

	private Button m_resourceScoreButton;

	private ListView m_scoredResourceListView;

	private ColumnHeader m_colFullName;

	private ColumnHeader m_colScore;

	private ComboBox m_resourceTypeCombo;

	private RichTextBox m_resourceRichTextBox;

	private Label m_headerLabel;

	public ResourcePreferencePage()
	{
		InitializeComponent();
	}

	public bool SaveValues(IPreferencePage preferencePage)
	{
		preferencePage.SetValue("ResourceName", m_resourceNameTextBox.Text ?? string.Empty);
		preferencePage.SetValue("ResourceType", m_resourceTypeCombo.Text ?? string.Empty);
		return true;
	}

	public void LoadValues(IPreferencePage preferencePage)
	{
		m_resourceNameTextBox.Text = preferencePage.GetValue("ResourceName", string.Empty);
		IResourceBundleService service = ServiceLocator.Instance.GetService<IResourceBundleService>();
		if (service == null)
		{
			return;
		}
		m_resourceTypeCombo.Items.Clear();
		foreach (KeyValuePair<string, IResourceType> resourceType in service.GetResourceTypes())
		{
			m_resourceTypeCombo.Items.Add(resourceType.Key);
		}
	}

	private void m_resourceScoreButton_Click(object sender, EventArgs e)
	{
		if (m_resourceTypeCombo.SelectedIndex < 0)
		{
			MessageBox.Show("Please select a resource Type first.");
			return;
		}
		if (m_resourceNameTextBox.Text == null)
		{
			MessageBox.Show("Please enter the name of the resource to score.");
			return;
		}
		m_scoredResourceListView.Items.Clear();
		IResourceBundleService service = ServiceLocator.Instance.GetService<IResourceBundleService>();
		if (service == null)
		{
			return;
		}
		IEnumerable<IResource> enumerable = from r in service.GetResourcesUnfiltered(m_resourceTypeCombo.Text)
			where r.Name == m_resourceNameTextBox.Text
			select r;
		IResourceBundleFilter filter = service.Filter;
		List<ScoredResource> list = new List<ScoredResource>();
		foreach (IResource each in enumerable)
		{
			int num = 0;
			if (!IsResourceEffective(each))
			{
				num = -1;
			}
			else if (filter != null)
			{
				List<IPropertyType> list2 = (from f in each.Type.GetFilterProperties()
					where each[f.Name] != null
					select f).ToList();
				if (list2.Count == 0)
				{
					num++;
				}
				else if (list2.Count((IPropertyType eachFilter) => each.PropertyEquals2(eachFilter.Name, (eachFilter.Name.Substring(0, 2) == "!=") ? filter[eachFilter.Name.Substring(2)] : filter[eachFilter.Name])) == list2.Count)
				{
					num += list2.Sum((IPropertyType eachFilter) => eachFilter.FilterScore);
				}
			}
			list.Add(new ScoredResource
			{
				Score = num,
				Resource = each
			});
		}
		list.Sort((ScoredResource x, ScoredResource y) => y.Score.CompareTo(x.Score));
		foreach (ScoredResource item in list)
		{
			ListViewItem value = new ListViewItem(new string[2]
			{
				item.Resource.StorageEntry.Name,
				item.Score.ToString()
			});
			m_scoredResourceListView.Items.Add(value);
		}
	}

	private static bool IsResourceEffective(IResource resource)
	{
		DateTime now = DateTime.Now;
		DateTime? dateTime = null;
		if (resource["effective_date"] != null)
		{
			dateTime = (DateTime?)resource["effective_date"];
		}
		DateTime? dateTime2 = null;
		if (resource["expiry_date"] != null)
		{
			dateTime2 = (DateTime?)resource["expiry_date"];
		}
		if (dateTime.HasValue)
		{
			DateTime value = now;
			DateTime? dateTime3 = dateTime;
			if (value < dateTime3)
			{
				return false;
			}
		}
		if (dateTime2.HasValue)
		{
			DateTime value = now;
			DateTime? dateTime3 = dateTime2;
			if (value > dateTime3)
			{
				return false;
			}
		}
		if (resource["daypart_dayofweek_only"] != null && !resource.PropertyEquals2("daypart_dayofweek_only", now.DayOfWeek.ToString()))
		{
			return false;
		}
		DayOfWeek? dayOfWeek = null;
		if (resource["daypart_effective_weekday"] != null)
		{
			dayOfWeek = (DayOfWeek?)resource["daypart_effective_weekday"];
		}
		DayOfWeek? dayOfWeek2 = null;
		if (resource["daypart_expiry_weekday"] != null)
		{
			dayOfWeek2 = (DayOfWeek?)resource["daypart_expiry_weekday"];
		}
		if (dayOfWeek.HasValue && now.DayOfWeek < dayOfWeek)
		{
			return false;
		}
		if (dayOfWeek2.HasValue && now.DayOfWeek > dayOfWeek2)
		{
			return false;
		}
		TimeSpan? timeSpan = null;
		if (resource["daypart_effective_time"] != null)
		{
			timeSpan = (TimeSpan?)resource["daypart_effective_time"];
		}
		TimeSpan? timeSpan2 = null;
		if (resource["daypart_expiry_time"] != null)
		{
			timeSpan2 = (TimeSpan?)resource["daypart_expiry_time"];
		}
		if (timeSpan.HasValue)
		{
			TimeSpan timeOfDay = now.TimeOfDay;
			TimeSpan? timeSpan3 = timeSpan;
			if (timeOfDay < timeSpan3)
			{
				return false;
			}
		}
		if (timeSpan2.HasValue)
		{
			TimeSpan timeOfDay = now.TimeOfDay;
			TimeSpan? timeSpan3 = timeSpan2;
			if (timeOfDay > timeSpan3)
			{
				return false;
			}
		}
		return true;
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
		this.components = new System.ComponentModel.Container();
		this.m_errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
		this.m_resourceNameTextBox = new System.Windows.Forms.TextBox();
		this.m_ResourceToScoreLabel = new System.Windows.Forms.Label();
		this.m_resourceScoreButton = new System.Windows.Forms.Button();
		this.m_scoredResourceListView = new System.Windows.Forms.ListView();
		this.m_colFullName = new System.Windows.Forms.ColumnHeader();
		this.m_colScore = new System.Windows.Forms.ColumnHeader();
		this.m_resourceTypeCombo = new System.Windows.Forms.ComboBox();
		this.m_resourceRichTextBox = new System.Windows.Forms.RichTextBox();
		this.m_headerLabel = new System.Windows.Forms.Label();
		((System.ComponentModel.ISupportInitialize)this.m_errorProvider).BeginInit();
		base.SuspendLayout();
		this.m_errorProvider.ContainerControl = this;
		this.m_resourceNameTextBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
		this.m_resourceNameTextBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystemDirectories;
		this.m_resourceNameTextBox.Location = new System.Drawing.Point(34, 68);
		this.m_resourceNameTextBox.MaxLength = 1024;
		this.m_resourceNameTextBox.Name = "m_resourceNameTextBox";
		this.m_resourceNameTextBox.Size = new System.Drawing.Size(216, 20);
		this.m_resourceNameTextBox.TabIndex = 5;
		this.m_ResourceToScoreLabel.AutoSize = true;
		this.m_ResourceToScoreLabel.Location = new System.Drawing.Point(31, 52);
		this.m_ResourceToScoreLabel.Name = "m_ResourceToScoreLabel";
		this.m_ResourceToScoreLabel.Size = new System.Drawing.Size(84, 13);
		this.m_ResourceToScoreLabel.TabIndex = 6;
		this.m_ResourceToScoreLabel.Text = "Resource Name";
		this.m_resourceScoreButton.Location = new System.Drawing.Point(403, 65);
		this.m_resourceScoreButton.Name = "m_resourceScoreButton";
		this.m_resourceScoreButton.Size = new System.Drawing.Size(94, 23);
		this.m_resourceScoreButton.TabIndex = 7;
		this.m_resourceScoreButton.Text = "Score Resource";
		this.m_resourceScoreButton.UseVisualStyleBackColor = true;
		this.m_resourceScoreButton.Click += new System.EventHandler(m_resourceScoreButton_Click);
		this.m_scoredResourceListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[2] { this.m_colFullName, this.m_colScore });
		this.m_scoredResourceListView.Location = new System.Drawing.Point(37, 105);
		this.m_scoredResourceListView.Name = "m_scoredResourceListView";
		this.m_scoredResourceListView.Size = new System.Drawing.Size(459, 217);
		this.m_scoredResourceListView.TabIndex = 8;
		this.m_scoredResourceListView.UseCompatibleStateImageBehavior = false;
		this.m_scoredResourceListView.View = System.Windows.Forms.View.Details;
		this.m_colFullName.Text = "Full Name";
		this.m_colFullName.Width = 373;
		this.m_colScore.Text = "Score";
		this.m_resourceTypeCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.m_resourceTypeCombo.FormattingEnabled = true;
		this.m_resourceTypeCombo.Location = new System.Drawing.Point(256, 67);
		this.m_resourceTypeCombo.MaxDropDownItems = 20;
		this.m_resourceTypeCombo.Name = "m_resourceTypeCombo";
		this.m_resourceTypeCombo.Size = new System.Drawing.Size(141, 21);
		this.m_resourceTypeCombo.TabIndex = 9;
		this.m_resourceRichTextBox.Location = new System.Drawing.Point(37, 331);
		this.m_resourceRichTextBox.Name = "m_resourceRichTextBox";
		this.m_resourceRichTextBox.Size = new System.Drawing.Size(458, 138);
		this.m_resourceRichTextBox.TabIndex = 10;
		this.m_resourceRichTextBox.Text = "";
		this.m_headerLabel.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.m_headerLabel.BackColor = System.Drawing.Color.MediumBlue;
		this.m_headerLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.m_headerLabel.ForeColor = System.Drawing.Color.White;
		this.m_headerLabel.Location = new System.Drawing.Point(0, 0);
		this.m_headerLabel.Name = "m_headerLabel";
		this.m_headerLabel.Size = new System.Drawing.Size(542, 38);
		this.m_headerLabel.TabIndex = 11;
		this.m_headerLabel.Text = "  Engine Core: Resource Scoring";
		this.m_headerLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.m_headerLabel);
		base.Controls.Add(this.m_resourceRichTextBox);
		base.Controls.Add(this.m_resourceTypeCombo);
		base.Controls.Add(this.m_scoredResourceListView);
		base.Controls.Add(this.m_resourceScoreButton);
		base.Controls.Add(this.m_ResourceToScoreLabel);
		base.Controls.Add(this.m_resourceNameTextBox);
		base.Name = "ResourcePreferencePage";
		base.Size = new System.Drawing.Size(542, 506);
		((System.ComponentModel.ISupportInitialize)this.m_errorProvider).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}

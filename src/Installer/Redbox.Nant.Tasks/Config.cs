using NAnt.Core;
using NAnt.Core.Attributes;

namespace Redbox.Nant.Tasks;

[TaskName("config")]
public class Config : Task
{
	private AddCollection _addElements;

	private RemoveCollection _removeElements;

	protected Task _task;

	private string _configFile;

	private ConfigFile _config;

	public ConfigFile ConfigFile
	{
		get
		{
			if (_config == null)
			{
				_config = new ConfigFile(FileName, this);
			}
			return _config;
		}
		set
		{
			_config = value;
		}
	}

	[BuildElementArray("add")]
	public AddCollection AddElements => _addElements;

	[BuildElementArray("remove")]
	public RemoveCollection RemoveElements => _removeElements;

	[TaskAttribute("file", Required = true)]
	public string FileName
	{
		get
		{
			return _configFile;
		}
		set
		{
			_configFile = value;
		}
	}

	public Config()
	{
		_addElements = new AddCollection();
		_removeElements = new RemoveCollection();
	}

	protected override void ExecuteTask()
	{
		ConfigFile.Load();
		AddConfigElements();
		RemoveConfigElements();
		ConfigFile.Save();
	}

	protected void AddConfigElements()
	{
		foreach (AddElement addElement in AddElements)
		{
			ConfigFile.AddElement(addElement.KeyName, addElement.KeyValue, addElement.PreserveExisting);
		}
	}

	protected void RemoveConfigElements()
	{
		foreach (RemoveElement removeElement in RemoveElements)
		{
			ConfigFile.RemoveElement(removeElement.KeyName);
		}
	}
}

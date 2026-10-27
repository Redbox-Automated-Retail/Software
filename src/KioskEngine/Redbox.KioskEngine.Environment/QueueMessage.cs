using System;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Environment;

public class QueueMessage : IMessage
{
	public int ID { get; set; }

	public string GUID { get; set; }

	public byte[] Data { get; set; }

	public string Type { get; set; }

	public byte Priority { get; set; }

	public DateTime CreatedOn { get; set; }
}

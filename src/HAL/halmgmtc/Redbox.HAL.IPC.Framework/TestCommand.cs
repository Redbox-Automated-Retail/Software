using System;
using System.ComponentModel;
using System.Text;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Configuration;
using Redbox.IPC.Framework;

namespace Redbox.HAL.IPC.Framework;

[Command("ipctest")]
public sealed class TestCommand
{
	[CommandForm(Name = "test-ipc-xfer")]
	[Usage("ipctest test-ipc-xfer size: payloadSize")]
	[Description("")]
	public void TestIPCTransfer(CommandContext context, [CommandKeyValue(IsRequired = true, KeyName = "size")] int payloadSize)
	{
		Random random = new Random((int)DateTime.Now.Ticks);
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < payloadSize; i++)
		{
			char value = Convert.ToChar(Convert.ToInt32(Math.Floor(26.0 * random.NextDouble() + 65.0)));
			stringBuilder.Append(value);
		}
		string text = stringBuilder.ToString();
		LogHelper.Instance.Log("String: ");
		LogHelper.Instance.Log(text);
		context.Messages.Add(text);
	}

	[CommandForm(Name = "test-comm")]
	[Usage("ipctest test-comm")]
	[Description("")]
	public void TestIPCTransfer(CommandContext context)
	{
		context.Messages.Add("ACK");
	}
}

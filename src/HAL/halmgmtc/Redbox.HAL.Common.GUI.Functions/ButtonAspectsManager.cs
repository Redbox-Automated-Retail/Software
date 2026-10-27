using System.Drawing;
using System.Windows.Forms;

namespace Redbox.HAL.Common.GUI.Functions;

public sealed class ButtonAspectsManager
{
	private readonly Color Running;

	public ButtonAspectsManager(Color running)
	{
		Running = running;
	}

	public ButtonAspectsManager()
		: this(Color.Red)
	{
	}

	public ButtonAspects MakeAspect(object button)
	{
		return new ButtonAspects(button as Button, Running);
	}
}

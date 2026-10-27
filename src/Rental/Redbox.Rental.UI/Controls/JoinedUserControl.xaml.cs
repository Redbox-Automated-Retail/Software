using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows.Markup;

namespace Redbox.Rental.UI.Controls;

public partial class JoinedUserControl : RentalUserControl, IComponentConnector
{
	public JoinedUserControl()
	{
		InitializeComponent();
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
	internal Delegate _CreateDelegate(Type delegateType, string handler)
	{
		return Delegate.CreateDelegate(delegateType, this, handler);
	}
}

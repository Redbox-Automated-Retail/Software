using System;
using System.ComponentModel;
using System.Web;
using Redbox.TimeKeeper;

namespace Redbox.PaymentService;

public class Global : HttpApplication
{
	private IContainer components = null!; // set by InitializeComponent

	public Global()
	{
		InitializeComponent();
	}

	protected void Application_Start(object sender, EventArgs e)
	{
		RedboxLogger.Configure();
	}

	protected void Session_Start(object sender, EventArgs e)
	{
	}

	protected void Application_BeginRequest(object sender, EventArgs e)
	{
	}

	protected void Application_EndRequest(object sender, EventArgs e)
	{
	}

	protected void Application_AuthenticateRequest(object sender, EventArgs e)
	{
	}

	protected void Application_Error(object sender, EventArgs e)
	{
		try
		{
			Exception error = HttpContext.Current.Error;
			base.Response.AppendToLog("REDBOX_ERROR: " + error.ToString());
			if (error.InnerException != null)
			{
				base.Response.AppendToLog("REDBOX_ERROR: " + error.InnerException.ToString());
			}
		}
		catch
		{
		}
	}

	protected void Session_End(object sender, EventArgs e)
	{
	}

	protected void Application_End(object sender, EventArgs e)
	{
	}

	private void InitializeComponent()
	{
		components = new Container();
	}
}

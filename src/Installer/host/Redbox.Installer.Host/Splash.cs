using System.ComponentModel;
using System.Drawing;
using System.Threading;
using Redbox.Core;
using Redbox.Installer.Host.Properties;

namespace Redbox.Installer.Host;

public static class Splash
{
	public static void Show()
	{
		SplashForm splashForm = CreateSplash();
		splashForm.Show();
		BackgroundWorker backgroundWorker = new BackgroundWorker();
		backgroundWorker.DoWork += delegate
		{
			ManualResetEvent manualResetEvent = new ManualResetEvent(initialState: false);
			manualResetEvent.WaitOne(5000);
			manualResetEvent.Close();
		};
		backgroundWorker.RunWorkerCompleted += delegate
		{
			MainForm.Instance.ThreadSafeHostUpdate(splashForm.Close);
		};
		backgroundWorker.RunWorkerAsync();
	}

	public static void ShowAbout()
	{
		SplashForm splashForm = CreateSplash();
		splashForm.Show();
	}

	private static SplashForm CreateSplash()
	{
		SplashForm splashForm = new SplashForm();
		Image image = (Image)Resources.Splash.Clone();
		using (Graphics graphics = Graphics.FromImage(image))
		{
			using (Font font = new Font("Arial", 13f, FontStyle.Bold, GraphicsUnit.Point, 0))
			{
				DrawStringCentre(graphics, font, BuildConfiguration.GetBuildConfigurationLabel(), Brushes.Yellow, image.Width, image.Height - 45);
			}
			using (Font font2 = new Font("Arial", 11.25f, FontStyle.Regular, GraphicsUnit.Point, 0))
			{
				graphics.DrawString("Version", font2, Brushes.White, 40f, 155f);
			}
			using (Font font3 = new Font("Arial", 11.25f, FontStyle.Bold, GraphicsUnit.Point, 0))
			{
				graphics.DrawString(AssemblyInfoHelper.GetVersion(typeof(Splash).Assembly), font3, Brushes.White, 90f, 155f);
			}
			using Font font4 = new Font("Arial", 10.5f, FontStyle.Regular, GraphicsUnit.Point, 0);
			graphics.DrawString("This program is protected", font4, Brushes.LightGray, 40f, 200f);
			graphics.DrawString("by US and International", font4, Brushes.LightGray, 40f, 214f);
			graphics.DrawString("copyright laws.", font4, Brushes.LightGray, 40f, 228f);
			graphics.DrawString(AssemblyInfoHelper.GetCopyright(typeof(Splash).Assembly), font4, Brushes.LightGray, 40f, 252f);
			graphics.DrawString("All rights reserved.", font4, Brushes.LightGray, 40f, 266f);
		}
		splashForm.Image = image;
		return splashForm;
	}

	private static void DrawStringCentre(Graphics g, Font font, string text, Brush foreColor, int width, int y)
	{
		g.DrawString(text, font, foreColor, ((float)width - g.MeasureString(text, font).Width) / 2f, y);
	}
}

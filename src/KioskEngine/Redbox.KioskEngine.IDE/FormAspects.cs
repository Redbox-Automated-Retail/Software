using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.IDE;

public static class FormAspects
{
	public static void ActsAsPersistent(Form form, IUserSettingsStore store)
	{
		if (form == null || store == null)
		{
			return;
		}
		form.Load += delegate
		{
			string value = store.GetValue<string>("Shell", $"{form.GetType().FullName}_State");
			if (value != null)
			{
				string[] parts = value.Split("|".ToCharArray());
				if (parts.Length == 3)
				{
					if (parts[0] == "Maximized")
					{
						form.WindowState = FormWindowState.Maximized;
					}
					else
					{
						TypeConverter converter = TypeDescriptor.GetConverter(typeof(Size));
						TypeConverter converter2 = TypeDescriptor.GetConverter(typeof(Point));
						Rectangle rectangle = new Rectangle((Point)converter2.ConvertFromString(parts[1]), (Size)converter.ConvertFromString(parts[2]));
						bool flag = false;
						Screen[] allScreens = Screen.AllScreens;
						for (int i = 0; i < allScreens.Length; i++)
						{
							if (allScreens[i].WorkingArea.IntersectsWith(rectangle))
							{
								flag = true;
								break;
							}
						}
						form.Bounds = (flag ? rectangle : Screen.PrimaryScreen.Bounds);
					}
				}
			}
		};
		form.FormClosing += delegate
		{
			store.SetValue("Shell", $"{form.GetType().FullName}_State", $"{form.WindowState}|{form.Location.X}, {form.Location.Y}|{form.Size.Width}, {form.Size.Height}");
		};
	}
}

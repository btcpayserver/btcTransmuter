using System.Reflection;

namespace BtcTransmuter.Helpers
{
	public static class VersionHelper
	{
		public static string CurrentVersion => Assembly.GetExecutingAssembly().GetName().Version.ToString();
	}
}
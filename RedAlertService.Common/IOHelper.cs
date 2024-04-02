using System.Reflection;

namespace RedAlertService.Common
{
    public class IOHelper
    {
        public static string GetAppPath()
        {
            return System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            //return Assembly.GetEntryAssembly().Location;
        }
    }
}

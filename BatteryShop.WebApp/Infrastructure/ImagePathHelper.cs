using System.Configuration;
using System.Web;

namespace BatteryShop.WebApp.Infrastructure
{
    public static class ImagePathHelper
    {
        public static string GetProfileUrl(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return null;
            if (fileName.Contains("/")) return fileName;
            return ConfigurationManager.AppSettings["ProfileImagePath"] + "/" + fileName;
        }

        public static string GetProfilePhysicalPath(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return null;
            return HttpContext.Current.Server.MapPath(GetProfileUrl(fileName));
        }

        public static string GetTempProfileUrl(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return null;
            if (fileName.Contains("/")) return fileName;
            return ConfigurationManager.AppSettings["ProfileImageTempPath"] + "/" + fileName;
        }

        public static string GetTempProfilePhysicalPath(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return null;
            return HttpContext.Current.Server.MapPath(GetTempProfileUrl(fileName));
        }
    }
}

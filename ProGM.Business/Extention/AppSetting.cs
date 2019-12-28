/// <summary>
/// Author: nnthuong
/// Create Date: 11/10/2019
/// Description: 
/// </summary>

using System.Configuration;

namespace ProGM.Business.Extention
{
    public class AppSetting
    {
        /// <summary>
        /// Author: nnthuong
        /// Date of create: 10/10/2018
        /// Discription: Allow us to get the [value] of [key] from Web.config file
        /// <configuration>
        /// <appSettings>
        /// Anything else in here we can get
        /// </appSettings>
        /// </configuration>
        /// </summary>
        public static string GetString(string key)
        {
            return !string.IsNullOrEmpty(ConfigurationManager.AppSettings[key]) ? ConfigurationManager.AppSettings[key].Trim() : string.Empty;
        }

        public static int GetInt32(string key)
        {
            return !string.IsNullOrEmpty(ConfigurationManager.AppSettings[key]) ? int.Parse(ConfigurationManager.AppSettings[key]) : 0;
        }

        public static long GetInt64(string key)
        {
            return !string.IsNullOrEmpty(ConfigurationManager.AppSettings[key]) ? long.Parse(ConfigurationManager.AppSettings[key]) : 0;
        }

        public static bool GetBool(string key)
        {
            return !string.IsNullOrEmpty(ConfigurationManager.AppSettings[key]) ? bool.Parse(ConfigurationManager.AppSettings[key]) : false;
        }

        public static string GetConnection(string key)
        {
            return !string.IsNullOrEmpty(ConfigurationManager.ConnectionStrings[key].ConnectionString) ? ConfigurationManager.ConnectionStrings[key].ConnectionString : string.Empty;
        }
    }
}

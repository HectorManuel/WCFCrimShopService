using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections.Specialized;

namespace WcfCrimShopService.entities
{
    public class Configuration
    {
        public static NameValueCollection configuration()
        {
            NameValueCollection appsetting = System.Web.Configuration.WebConfigurationManager.AppSettings;
            if (appsetting.Count != 0)
            {
                return appsetting;
            }
            return appsetting;
        }
    }
}
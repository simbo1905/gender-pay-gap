using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Extensions;
using GenderPayGap.WebUI.Properties;
using Microsoft.IdentityModel;

namespace GenderPayGap.WebUI.Classes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple  = true, Inherited = false)]
    public class CustomUrlAttribute : RegularExpressionAttribute
    {
        //This Original pattern string below is never used, all the patterns below proved this! so why is this string even set? making it empty would still work!
        private const string pattern = @"^(http|https|ftp|)\://|[a-zA-Z0-9\-\.]+\.[a-zA-Z](:[a-zA-Z0-9]*)?/?([a-zA-Z0-9\-\._\?\,\'/\\\+&amp;%\$#\=~])*[^\.\,\)\(\s]$";

        //private const string pattern = @"^(http|https|ftp|\://)|[a-zA-Z0-9\-\.]+\.[a-zA-Z](:[a-zA-Z0-9]*)?/?([a-zA-Z0-9\-\._\?\,\'/\\\+&amp;%\$#\=~])*[^\.\,\)\(\s]$";
        //private const string pattern = @"^(http|https|ftp|\://)?|[a-zA-Z0-9\-\.]+\.[a-zA-Z](:[a-zA-Z0-9]*)?/?([a-zA-Z0-9\-\._\?\,\'/\\\+&amp;%\$#\=~])*[^\.\,\)\(\s]$";
        //private const string pattern = @"^(http|https|ftp|*)\://|[a-zA-Z0-9\-\.]+\.[a-zA-Z](:[a-zA-Z0-9]*)?/?([a-zA-Z0-9\-\._\?\,\'/\\\+&amp;%\$#\=~])*[^\.\,\)\(\s]$";
        //private const string pattern = @"^(www\.|(http|https|ftp|news|file|)+\:\/\/)?[&#95;.a-z0-9-]+\.[a-z0-9\/&#95;:@=.+?,##%&~-]*[^.|\'|\# |!|\(|?|,| |>|<|;|\)]$";
        //private const string pattern = @"^(www\.|(http|https|ftp|news|file|)+\:\/\/)|[a-zA-Z0-9\-\.]+\.[a-zA-Z](:[a-zA-Z0-9]*)?/?([a-zA-Z0-9\-\._\?\,\'/\\\+&amp;%\$#\=~])*[^\.\,\)\(\s]$";
        //private const string pattern = @"^(www\.|(http|https|ftp|news|file|)+\:\/\/)?|[a-zA-Z0-9\-\.]+\.[a-zA-Z](:[a-zA-Z0-9]*)?/?([a-zA-Z0-9\-\._\?\,\'/\\\+&amp;%\$#\=~])*[^\.\,\)\(\s]$";
        //private const string pattern = @"^(http|kkk)\://|[a-zA-Z0-9\-\.]+\.[a-zA-Z](:[a-zA-Z0-9]*)?/?([a-zA-Z0-9\-\._\?\,\'/\\\+&amp;%\$#\=~])*[^\.\,\)\(\s]$";
        //private const string pattern = @"";

        static CustomUrlAttribute() 
        {
            // necessary to enable client side validation
            DataAnnotationsModelValidatorProvider.RegisterAdapter(typeof(CustomUrlAttribute), typeof(RegularExpressionAttributeAdapter));
        }

        public CustomUrlAttribute():base(pattern)
        {
            base.ErrorMessage = Settings.Default.PasswordRegexError;
        }
    }
}
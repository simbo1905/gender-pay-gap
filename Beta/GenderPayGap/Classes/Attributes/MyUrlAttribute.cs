using GenderPayGap.WebUI.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace GenderPayGap.WebUI.Classes.Attribute
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
    public class UrlAttribute : RegularExpressionAttribute //ValidationAttribute
    {
        private const string pattern = "@((www\\.| (http | https | ftp | news | file |) +\\:\\/\\/)?[&#95;.a-z0-9-]+\\.[a-z0-9\\/&#95;:@=.+?,##%&~-]*[^.|\'|\\# |!|\\(|?|,| |>|<|;|\\)])";

        //NOT IN USE: regex does not need escaping but as a string above it does need cspecial chars escaped
       //Regex regex1 = new Regex(@"((www\.| (http | https | ftp | news | file |) +\:\/\/)?[&#95;.a-z0-9-]+\.[a-z0-9\/&#95;:@=.+?,##%&~-]*[^.|\'|\# |!|\(|?|,| |>|<|;|\)])");


        static UrlAttribute()
        {
            // necessary to enable client side validation
            DataAnnotationsModelValidatorProvider.RegisterAdapter(typeof(UrlAttribute), typeof(RegularExpressionAttributeAdapter));
        }


        public UrlAttribute():base(pattern)
        {
            base.ErrorMessage = Settings.Default.PasswordRegexError;
        }
    }
    
}

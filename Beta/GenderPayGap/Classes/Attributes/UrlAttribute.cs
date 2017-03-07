using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using GenderPayGap.WebUI.Properties;

namespace GenderPayGap.WebUI.Classes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple  = true, Inherited = false)]
    public class CustomUrlAttribute : RegularExpressionAttribute
    {
        private const string pattern = @"^(http|https|ftp|)\://|[a-zA-Z0-9\-\.]+\.[a-zA-Z](:[a-zA-Z0-9]*)?/?([a-zA-Z0-9\-\._\?\,\'/\\\+&amp;%\$#\=~])*[^\.\,\)\(\s]$";

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
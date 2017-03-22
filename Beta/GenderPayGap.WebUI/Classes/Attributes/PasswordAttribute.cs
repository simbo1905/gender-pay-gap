using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using GenderPayGap.WebUI.Properties;

namespace GenderPayGap.WebUI.Classes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple  = true, Inherited = false)]
    public class PasswordAttribute : RegularExpressionAttribute
    {
        static PasswordAttribute() 
        {
            // necessary to enable client side validation
            DataAnnotationsModelValidatorProvider.RegisterAdapter(typeof(PasswordAttribute), typeof(RegularExpressionAttributeAdapter));
        }

        public PasswordAttribute():base(Settings.Default.PasswordRegex)
        {
            base.ErrorMessage = Settings.Default.PasswordRegexError;
        }
    }
}
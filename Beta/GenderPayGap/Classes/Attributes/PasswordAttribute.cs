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
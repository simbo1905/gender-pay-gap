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
    public class PinAttribute : RegularExpressionAttribute
    {
        static PinAttribute() 
        {
            // necessary to enable client side validation
            DataAnnotationsModelValidatorProvider.RegisterAdapter(typeof(PinAttribute), typeof(RegularExpressionAttributeAdapter));
        }

        public PinAttribute():base(Settings.Default.PinRegex)
        {
            base.ErrorMessage = Settings.Default.PinRegexError;
        }
    }
}
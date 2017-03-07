using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using GenderPayGap.WebUI.Properties;

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
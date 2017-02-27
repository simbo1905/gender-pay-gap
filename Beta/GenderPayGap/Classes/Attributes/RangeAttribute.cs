﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Extensions;
using GenderPayGap.WebUI.Properties;
using Microsoft.IdentityModel;
using GenderPayGap.WebUI.Classes;
using GenderPayGap.WebUI.Models;
using System.ComponentModel.DataAnnotations;

namespace GenderPayGap.WebUI.Classes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple  = true, Inherited = false)]
    public class CustomRangeAttribute : RangeAttribute
    {
        static CustomRangeAttribute() 
        {
            // necessary to enable client side validation
            DataAnnotationsModelValidatorProvider.RegisterAdapter(typeof(CustomRangeAttribute), typeof(RangeAttributeAdapter));
        }

        public CustomRangeAttribute(int minimum,int maximum, int code=0,string ErrorMessage=null):base(minimum,maximum)
        {
            if (code != 0)
            {
                var customErrorMessage = CustomErrorMessages.Get(code);
                if (customErrorMessage != null && !string.IsNullOrWhiteSpace(customErrorMessage.Description)) ErrorMessage = customErrorMessage.Description;
            }
            if (!string.IsNullOrWhiteSpace(ErrorMessage))base.ErrorMessage = ErrorMessage;
        }
    }
}
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
    public class CustomStringLengthAttribute : StringLengthAttribute
    {
        static CustomStringLengthAttribute() 
        {
            // necessary to enable client side validation
            DataAnnotationsModelValidatorProvider.RegisterAdapter(typeof(CustomStringLengthAttribute), typeof(StringLengthAttributeAdapter));
        }

        public CustomStringLengthAttribute(int maximumLength,int code=0,string ErrorMessage=null,int MinimumLength = 0) :base(maximumLength)
        {
            if (code != 0)
            {
                var customErrorMessage = ErrorViewModel.CustomErrorMessages.Messages[code];
                if (customErrorMessage != null && !string.IsNullOrWhiteSpace(customErrorMessage.Description)) ErrorMessage = customErrorMessage.Description;
            }
            if (!string.IsNullOrWhiteSpace(ErrorMessage))base.ErrorMessage = ErrorMessage;
            base.MinimumLength = MinimumLength;
        }
    }
}
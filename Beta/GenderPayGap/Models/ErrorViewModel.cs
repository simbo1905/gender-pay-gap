﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GenderPayGap.WebUI.Classes;
using System.Configuration;
using System.Reflection;
using Extensions;

namespace GenderPayGap.WebUI.Models
{
    public class ErrorViewModel
    {
        public ErrorViewModel(int code, object parameters=null)
        {
            var customErrorMessage = CustomErrorMessages.Messages[code] ?? CustomErrorMessages.Messages.Default;

            Title = customErrorMessage.Title;
            Description = customErrorMessage.Description;
            CallToAction = customErrorMessage.CallToAction;
            ActionUrl = customErrorMessage.ActionUrl;
            ActionText = customErrorMessage.ActionText;

            //Assign any values to variables
            if (parameters!=null)
                foreach (var prop in parameters.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
                {
                    var value = prop.GetValue(parameters, null) as string;
                    if (string.IsNullOrWhiteSpace((prop.Name)) || string.IsNullOrWhiteSpace(value)) continue;
                    Title = customErrorMessage.Title.ReplaceI(""+prop.Name+"",value);
                    Description = customErrorMessage.Description.ReplaceI("" + prop.Name + "", value);
                    CallToAction = customErrorMessage.CallToAction.ReplaceI("" + prop.Name + "", value);
                    ActionUrl = customErrorMessage.ActionUrl.ReplaceI("" + prop.Name + "", value);
                    ActionText = customErrorMessage.ActionText.ReplaceI("" + prop.Name + "", value);
                }
        }

        static CustomErrorMessagesSection _CustomErrorMessages = null;
        private static CustomErrorMessagesSection CustomErrorMessages
        {
            get
            {
                if (_CustomErrorMessages == null) _CustomErrorMessages = (CustomErrorMessagesSection)ConfigurationManager.GetSection("CustomErrorMessages");
                return _CustomErrorMessages;
            }
        }

        public string Title { get; set; }
        public string Description { get; set; }
        public string CallToAction { get; set; }
        public string ActionText { get; set; } = "Continue";
        public string ActionUrl { get; set; }
    }
}
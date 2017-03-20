using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Globalization;

using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;
using Castle.Core.Internal;
using Extensions;
using Microsoft.Ajax.Utilities;

namespace GenderPayGap.WebUI.Classes
{
    public static class HtmlHelpers
    {
        public static MvcHtmlString PageIdentifier(this HtmlHelper htmlHelper)
        {
            return new MvcHtmlString($"Date:{DateTime.Now}, Version:{typeof(BaseController).Assembly.GetName().Version}, Machine:{Environment.MachineName}, Instance:{Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID")}");
        }

        public static MvcHtmlString AppSetting(this HtmlHelper htmlHelper,string appSettingKey)
        {
            return new MvcHtmlString(ConfigurationManager.AppSettings[appSettingKey]);
        }


        public static MvcHtmlString SetErrorClass<TModel, TProperty>(
            this HtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, TProperty>> expression,
            string errorClassName, string noErrorClassName=null)
        {
            var expressionText = ExpressionHelper.GetExpressionText(expression);
            var fullHtmlFieldName = htmlHelper.ViewContext.ViewData
                .TemplateInfo.GetFullHtmlFieldName(expressionText);
            var state = htmlHelper.ViewData.ModelState[fullHtmlFieldName];

            if (!string.IsNullOrWhiteSpace(noErrorClassName))
                return state == null || state.Errors.Count == 0 ? new MvcHtmlString(noErrorClassName) : new MvcHtmlString(errorClassName);

            return  state == null || state.Errors.Count == 0 ? MvcHtmlString.Empty : new MvcHtmlString(errorClassName);
        }

        #region Checkbox list
        /// <summary>
        /// Returns a checkbox for each of the provided <paramref name="items"/>.
        /// </summary>
        public static MvcHtmlString CheckBoxListFor<TModel, TValue>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TValue>> expression, List<Core.Classes.SelectedItem> items, object htmlAttributes = null)
        {
            var listName = ExpressionHelper.GetExpressionText(expression);
            var metaData = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);

            items = GetCheckboxListWithDefaultValues(metaData.Model, items);
            return htmlHelper.CheckBoxList(listName, items, htmlAttributes);
        }

        private static List<Core.Classes.SelectedItem> GetCheckboxListWithDefaultValues(object defaultValues, List<Core.Classes.SelectedItem> selectList)
        {
            var defaultValuesList = defaultValues as IList;

            if (defaultValuesList == null)
                return selectList;

            IEnumerable<string> values = from object value in defaultValuesList
                                         select Convert.ToString(value, CultureInfo.CurrentCulture);

            var selectedValues = new HashSet<string>(values, StringComparer.OrdinalIgnoreCase);
            var newSelectList = new List<Core.Classes.SelectedItem>();

            selectList.ForEach(item =>
            {
                item.Selected = (item.Value != null) ? selectedValues.Contains(item.Value) : selectedValues.Contains(item.Text);
                newSelectList.Add(item);
            });

            return newSelectList;
        }

        public static MvcHtmlString CheckBoxList(this HtmlHelper htmlHelper, string listName, List<Core.Classes.SelectedItem> items, object htmlAttributes = null)
        {
            var container = new TagBuilder("ul");
            int i = 0;
            foreach (var item in items)
            {
                i++;
                var label = new TagBuilder("label");
                label.MergeAttribute("class", "checkbox"); // default class
                label.MergeAttribute("for", $"{listName}_{i}"); // default class
                label.MergeAttributes(new RouteValueDictionary(htmlAttributes), true);

                var cb = new TagBuilder("input");
                cb.MergeAttribute("type", "checkbox");
                cb.MergeAttribute("id", $"{listName}_{i}"); 
                cb.MergeAttribute("name", listName);
                cb.MergeAttribute("value", item.Value ?? item.Text);
                if (item.Selected)
                    cb.MergeAttribute("checked", "checked");

                label.InnerHtml = item.Text;

                container.InnerHtml += $"<li>{cb.ToString(TagRenderMode.SelfClosing) + label}</li>";
            }

            return new MvcHtmlString(container.ToString());
        }
        #endregion

        #region Validation messages

        public static MvcHtmlString CustomValidationSummary(this HtmlHelper helper, bool excludePropertyErrors=true, string validationSummaryMessage = "The following errors were detected", object htmlAttributes = null)
        {
            helper.ViewBag.ValidationSummaryMessage = validationSummaryMessage;
            helper.ViewBag.ExcludePropertyErrors = excludePropertyErrors;

            return helper.Partial("_ValidationSummary");
        }

        private static Dictionary<string, object> CustomAttributesFor<TModel, TProperty>(Expression<Func<TModel, TProperty>> expression, object htmlAttributes = null)
        {
            var containerType = typeof(TModel);

            string propertyName = ExpressionHelper.GetExpressionText(expression);
            var propertyInfo = containerType.GetPropertyInfo(propertyName);

            var displayAttribute = propertyInfo.GetCustomAttributes(typeof(DisplayAttribute), false).FirstOrDefault() as DisplayAttribute;
            var displayName = displayAttribute == null ? propertyName : displayAttribute.Name;

            string par1 = null;
            string par2 = null;

            var htmlAttr = htmlAttributes.ToPropertyDictionary();
            if (propertyInfo != null)
                foreach (ValidationAttribute attribute in propertyInfo.GetCustomAttributes(typeof(ValidationAttribute), false))
                {
                    var validatorKey = $"{containerType.Name}.{propertyName}:{attribute.GetType().Name.TrimSuffix("Attribute")}";
                    var customError = CustomErrorMessages.GetValidationError(validatorKey);
                    if (customError == null)continue;

                    //Set the message from the description
                    if (attribute.ErrorMessage != customError.Description)
                        attribute.ErrorMessage = customError.Description;

                    //Set the inline error message
                    string errorMessageString = Misc.GetPropertyValue(attribute, "ErrorMessageString") as string;
                    if (string.IsNullOrWhiteSpace(errorMessageString)) errorMessageString = attribute.ErrorMessage;

                    //Set the summary error message
                    if (customError.Title != errorMessageString)
                        errorMessageString = customError.Title;

                    //Set the display name
                    if (!string.IsNullOrWhiteSpace(customError.DisplayName) && customError.DisplayName != displayName)
                    {
                        if (displayAttribute != null) Misc.SetPropertyValue(displayAttribute, "Name", customError.DisplayName);
                        displayName = customError.DisplayName;
                    }

                    string altAttr = null;
                    if (attribute is RequiredAttribute)
                        altAttr = "data-val-required-alt";
                    else if (attribute is System.ComponentModel.DataAnnotations.CompareAttribute)
                        altAttr = "data-val-equalto-alt";
                    else if (attribute is RegularExpressionAttribute)
                        altAttr = "data-val-regex-alt";
                    else if (attribute is RangeAttribute)
                    {
                        altAttr = "data-val-range-alt";
                        par1 = ((RangeAttribute)attribute).Minimum.ToString();
                        par2 = ((RangeAttribute)attribute).Maximum.ToString();
                    }
                    else if (attribute is DataTypeAttribute)
                    {
                        var type = ((DataTypeAttribute)attribute).DataType.ToString().ToLower();
                        switch (type)
                        {
                            case "password":
                                continue;
                            case "emailaddress":
                                type = "email";
                                break;
                            case "phonenumber":
                                type = "phone";
                                break;
                        }
                        altAttr = $"data-val-{type}-alt";
                    }
                    else if (attribute is MinLengthAttribute)
                    {
                        altAttr = "data-val-minlength-alt";
                        par1 = ((MinLengthAttribute)attribute).Length.ToString();
                    }
                    else if (attribute is MaxLengthAttribute)
                    {
                        altAttr = "data-val-maxlength-alt";
                        par1 = ((MaxLengthAttribute)attribute).Length.ToString();
                    }
                    else if (attribute is StringLengthAttribute)
                    {
                        altAttr = "data-val-length-alt";
                        par1 = ((StringLengthAttribute)attribute).MinimumLength.ToString();
                        par2 = ((StringLengthAttribute)attribute).MaximumLength.ToString();
                    }

                    htmlAttr[altAttr.TrimSuffix("-alt")] = string.Format(attribute.ErrorMessage, displayName, par1, par2);
                    htmlAttr[altAttr] = string.Format(errorMessageString, displayName, par1, par2); ;
                }

            return htmlAttr;
        }

        public static MvcHtmlString CustomEditorFor<TModel, TProperty>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TProperty>> expression, object htmlAttributes = null)
        {
            var htmlAttr = CustomAttributesFor(expression, htmlAttributes);

            return helper.EditorFor(expression, null, new { htmlAttributes = htmlAttr });
        }

        public static MvcHtmlString CustomRadioButtonFor<TModel, TProperty>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TProperty>> expression, object value, object htmlAttributes = null)
        {
            var htmlAttr = CustomAttributesFor(expression, htmlAttributes);

            return helper.RadioButtonFor(expression, value, htmlAttr);
        }
        #endregion

    }
}
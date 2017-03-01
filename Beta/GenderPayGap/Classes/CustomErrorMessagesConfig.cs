using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Xml;
using Extensions;
using System.Threading.Tasks;

namespace GenderPayGap.WebUI.Classes
{
    public class CustomErrorMessagesSection : ConfigurationSection
    {
        [ConfigurationProperty("", IsRequired = true, IsDefaultCollection = true)]
        [ConfigurationCollection(typeof(CustomErrorMessages), AddItemName = "CustomErrorMessage")]
        public CustomErrorMessages Messages
        {
            get { return (CustomErrorMessages)this[""]; }
            set { this[""] = value; }
        }


        public override bool IsReadOnly()
        {
            return false;
        }

        public string Serialize()
        {
            return SerializeSection(this, "CustomErrorMessages", ConfigurationSaveMode.Full);
        }

        public static CustomErrorMessagesSection Deserialize(string xml)
        {
            var CustomErrorMessagesSection = new CustomErrorMessagesSection();
            var rdr = new XmlTextReader(new StringReader(xml));
            CustomErrorMessagesSection.DeserializeSection(rdr);
            return CustomErrorMessagesSection;
        }
    }

    public class CustomErrorMessages : ConfigurationElementCollection
    {
        public override bool IsReadOnly()
        {
            return false;
        }

        public BindingList<CustomErrorMessage> GetBindingList()
        {
            var results = new BindingList<CustomErrorMessage>();
            foreach (CustomErrorMessage setting in this)
                results.Add(setting);
            return results;
        }

        Dictionary<int, CustomErrorMessage> _PageErrors = null;
        public Dictionary<int, CustomErrorMessage> PageErrors
        {
            get
            {
                if (_PageErrors == null) _PageErrors = this.ToList<CustomErrorMessage>().Where(e=>string.IsNullOrWhiteSpace(e.Validator)).ToDictionary(c => c.Code);
                return _PageErrors;

            }
        }

        Dictionary<string, CustomErrorMessage> _ValidationErrors = null;
        public Dictionary<string,CustomErrorMessage> ValidationErrors
        {
            get
            {
                if (_ValidationErrors == null) _ValidationErrors = this.ToList<CustomErrorMessage>().Where(e => !string.IsNullOrWhiteSpace(e.Validator)).ToDictionary(c=>c.Validator,StringComparer.CurrentCultureIgnoreCase);
                return _ValidationErrors;
            }
        }

        public CustomErrorMessage this[int code]
        {
            get
            {
                return PageErrors[code];
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new CustomErrorMessage();
        }
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((CustomErrorMessage)element).Code;
        }

        public void Add(CustomErrorMessage element)
        {
            base.BaseAdd(element);
        }

        public void RemoveAt(int index)
        {
            base.BaseRemoveAt(index);
        }

        public void Insert(int index, CustomErrorMessage element)
        {
            base.BaseAdd(index, element);
        }

        protected override string ElementName
        {
            get
            {
                return "CustomErrorMessage";
            }
        }

        protected override bool IsElementName(string elementName)
        {
            return elementName.Equals("CustomErrorMessage", StringComparison.InvariantCultureIgnoreCase);
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMap;
            }
        }

        internal static CustomErrorMessages Load(System.Configuration.Configuration config)
        {
            var CustomErrorMessagesSection = (CustomErrorMessagesSection)config.GetSection("CustomErrorMessages");

            //Get the account settings section
            if (CustomErrorMessagesSection == null) CustomErrorMessagesSection = new CustomErrorMessagesSection();
            if (CustomErrorMessagesSection.Messages == null) CustomErrorMessagesSection.Messages = new CustomErrorMessages();
            var results = CustomErrorMessagesSection.Messages;

            if (results == null) throw new Exception("You must enter all the http error codes and messages.");

            return results;
        }

        static CustomErrorMessagesSection _DefaultSection = null;
        static CustomErrorMessagesSection DefaultSection
        {
            get
            {
                if (_DefaultSection == null) _DefaultSection = (CustomErrorMessagesSection)ConfigurationManager.GetSection("CustomErrorMessages");
                return _DefaultSection;
            }
        }

        public static CustomErrorMessage DefaultPageError
        {
            get
            {
                return DefaultSection.Messages.PageErrors.Values.FirstOrDefault(e=>e.Default);
            }
        }

        public static CustomErrorMessage GetPageError(int code)
        {
            return DefaultSection.Messages.PageErrors[code];
        }

        public static CustomErrorMessage GetValidationError(string validator)
        {
            return DefaultSection.Messages.ValidationErrors.ContainsKey(validator) ? DefaultSection.Messages.ValidationErrors[validator] : null;
        }

        public static string GetTitle(int code)
        {
            return DefaultSection.Messages[code]?.Title;
        }

        public static string GetDescription(int code)
        {
            return DefaultSection.Messages[code]?.Description;
        }

        public static string GetTitle(string validator)
        {
            return DefaultSection.Messages.ValidationErrors[validator]?.Title;
        }

        public static string GetDescription(string validator)
        {
            return DefaultSection.Messages.ValidationErrors[validator]?.Description;
        }

    }

    [Serializable]
    public class CustomErrorMessage : ConfigurationElement
    {
        public CustomErrorMessage() : base()
        {
        }
        public CustomErrorMessage(int code)
        {
            Code = code;
        }

        public override bool IsReadOnly()
        {
            return true;
        }

        [ConfigurationProperty("code", IsKey = true, IsRequired = true)]
        public int Code
        {
            get
            {
                var result = (int)base["code"];
                return result;
            }
            set
            {
                base["code"] = value;
            }
        }

        [ConfigurationProperty("title", IsRequired = true)]
        public string Title
        {
            get { return (string)base["title"]; }
            set
            {
                base["description"] = value;
            }
        }

        [ConfigurationProperty("description", IsRequired = false)]
        public string Description
        {
            get { return (string)base["description"]; }
            set
            {
                base["description"] = value;
            }
        }

        [ConfigurationProperty("callToAction", IsRequired = false)]
        public string CallToAction
        {
            get { return (string)base["callToAction"]; }
            set
            {
                base["callToAction"] = value;
            }
        }

        [ConfigurationProperty("actionUrl", IsRequired = false)]
        public string ActionUrl
        {
            get { return (string)base["actionUrl"]; }
            set
            {
                base["actionUrl"] = value;
            }
        }

        [ConfigurationProperty("validator", IsRequired = false)]
        public string Validator
        {
            get { return (string)base["validator"]; }
            set
            {
                base["validator"] = value;
            }
        }

        [ConfigurationProperty("displayName", IsRequired = false)]
        public string DisplayName
        {
            get { return (string)base["displayName"]; }
            set
            {
                base["displayName"] = value;
            }
        }

        [ConfigurationProperty("actionText", IsRequired = false,DefaultValue = "Continue")]
        public string ActionText
        {
            get { return (string)base["actionText"]; }
            set
            {
                base["actionText"] = value;
            }
        }

        [ConfigurationProperty("default", IsRequired = false,DefaultValue = false)]
        public bool Default
        {
            get
            {
                var result = (bool)base["default"];
                return result;
            }
            set
            {
                base["default"] = value;
            }
        }

    }

}

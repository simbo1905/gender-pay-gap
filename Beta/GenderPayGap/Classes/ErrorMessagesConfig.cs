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
    public class ErrorMessagesSection : ConfigurationSection
    {
        [ConfigurationProperty("", IsRequired = true, IsDefaultCollection = true)]
        [ConfigurationCollection(typeof(ErrorMessages), AddItemName = "ErrorMessage")]
        public ErrorMessages Messages
        {
            get { return (ErrorMessages)this[""]; }
            set { this[""] = value; }
        }
        public override bool IsReadOnly()
        {
            return false;
        }

        public string Serialize()
        {
            return SerializeSection(this, "ErrorMessages", ConfigurationSaveMode.Full);
        }

        public static ErrorMessagesSection Deserialize(string xml)
        {
            var ErrorMessagesSection = new ErrorMessagesSection();
            var rdr = new XmlTextReader(new StringReader(xml));
            ErrorMessagesSection.DeserializeSection(rdr);
            return ErrorMessagesSection;
        }
    }

    public class ErrorMessages : ConfigurationElementCollection
    {
        public override bool IsReadOnly()
        {
            return false;
        }

        public BindingList<ErrorMessage> GetBindingList()
        {
            var results = new BindingList<ErrorMessage>();
            foreach (ErrorMessage setting in this)
                results.Add(setting);
            return results;
        }

        public ErrorMessage this[int code]
        {
            get
            {
                if (code<400) return null;
                foreach (ErrorMessage setting in this)
                {
                    if (setting.Code.EqualsI(code)) return setting;
                }
                return null;
            }
        }

        internal ErrorMessage Default
        {
            get
            {
                foreach (ErrorMessage setting in this)
                {
                    if (setting.Default == true) return setting;
                }
                return null;
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new ErrorMessage();
        }
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ErrorMessage)element).Code;
        }

        public void Add(ErrorMessage element)
        {
            base.BaseAdd(element);
        }

        public void RemoveAt(int index)
        {
            base.BaseRemoveAt(index);
        }

        public void Insert(int index, ErrorMessage element)
        {
            base.BaseAdd(index, element);
        }

        protected override string ElementName
        {
            get
            {
                return "ErrorMessage";
            }
        }

        protected override bool IsElementName(string elementName)
        {
            return elementName.Equals("ErrorMessage", StringComparison.InvariantCultureIgnoreCase);
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMap;
            }
        }

        internal static ErrorMessages Load(System.Configuration.Configuration config)
        {
            var ErrorMessagesSection = (ErrorMessagesSection)config.GetSection("ErrorMessages");

            //Get the account settings section
            if (ErrorMessagesSection == null) ErrorMessagesSection = new ErrorMessagesSection();
            if (ErrorMessagesSection.Messages == null) ErrorMessagesSection.Messages = new ErrorMessages();
            var results = ErrorMessagesSection.Messages;

            if (results == null) throw new Exception("You must enter all the http error codes and messages.");

            return results;
        }
    }

    [Serializable]
    public class ErrorMessage : ConfigurationElement
    {
        public ErrorMessage() : base()
        {
        }
        public ErrorMessage(int code)
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

        [ConfigurationProperty("description", IsRequired = true)]
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

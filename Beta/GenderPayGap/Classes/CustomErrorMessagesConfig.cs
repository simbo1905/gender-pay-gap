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

        public CustomErrorMessage this[int code]
        {
            get
            {
                if (code<400) return null;
                foreach (CustomErrorMessage setting in this)
                {
                    if (setting.Code.EqualsI(code)) return setting;
                }
                return null;
            }
        }

        internal CustomErrorMessage Default
        {
            get
            {
                foreach (CustomErrorMessage setting in this)
                {
                    if (setting.Default == true) return setting;
                }
                return null;
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

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
    public class HttpErrorMessagesSection : ConfigurationSection
    {
        [ConfigurationProperty("", IsRequired = true, IsDefaultCollection = true)]
        [ConfigurationCollection(typeof(HttpErrorMessages), AddItemName = "HttpErrorMessage")]
        public HttpErrorMessages Messages
        {
            get { return (HttpErrorMessages)this[""]; }
            set { this[""] = value; }
        }
        public override bool IsReadOnly()
        {
            return false;
        }

        public string Serialize()
        {
            return SerializeSection(this, "HttpErrorMessages", ConfigurationSaveMode.Full);
        }

        public static HttpErrorMessagesSection Deserialize(string xml)
        {
            var HttpErrorMessagesSection = new HttpErrorMessagesSection();
            var rdr = new XmlTextReader(new StringReader(xml));
            HttpErrorMessagesSection.DeserializeSection(rdr);
            return HttpErrorMessagesSection;
        }
    }

    public class HttpErrorMessages : ConfigurationElementCollection
    {
        public override bool IsReadOnly()
        {
            return false;
        }

        public BindingList<HttpErrorMessage> GetBindingList()
        {
            var results = new BindingList<HttpErrorMessage>();
            foreach (HttpErrorMessage setting in this)
                results.Add(setting);
            return results;
        }

        public HttpErrorMessage this[int code]
        {
            get
            {
                if (code<400) return null;
                foreach (HttpErrorMessage setting in this)
                {
                    if (setting.Code.EqualsI(code)) return setting;
                }
                return null;
            }
        }

        internal HttpErrorMessage Default
        {
            get
            {
                foreach (HttpErrorMessage setting in this)
                {
                    if (setting.Default == true) return setting;
                }
                return null;
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new HttpErrorMessage();
        }
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((HttpErrorMessage)element).Code;
        }

        public void Add(HttpErrorMessage element)
        {
            base.BaseAdd(element);
        }

        public void RemoveAt(int index)
        {
            base.BaseRemoveAt(index);
        }

        public void Insert(int index, HttpErrorMessage element)
        {
            base.BaseAdd(index, element);
        }

        protected override string ElementName
        {
            get
            {
                return "HttpErrorMessage";
            }
        }

        protected override bool IsElementName(string elementName)
        {
            return elementName.Equals("HttpErrorMessage", StringComparison.InvariantCultureIgnoreCase);
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMap;
            }
        }

        internal static HttpErrorMessages Load(System.Configuration.Configuration config)
        {
            var HttpErrorMessagesSection = (HttpErrorMessagesSection)config.GetSection("HttpErrorMessages");

            //Get the account settings section
            if (HttpErrorMessagesSection == null) HttpErrorMessagesSection = new HttpErrorMessagesSection();
            if (HttpErrorMessagesSection.Messages == null) HttpErrorMessagesSection.Messages = new HttpErrorMessages();
            var results = HttpErrorMessagesSection.Messages;

            if (results == null) throw new Exception("You must enter all the http error codes and messages.");

            return results;
        }
    }

    [Serializable]
    public class HttpErrorMessage : ConfigurationElement
    {
        public HttpErrorMessage() : base()
        {
        }
        public HttpErrorMessage(int code)
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

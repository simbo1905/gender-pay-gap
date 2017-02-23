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
    public class PublicSectorOrgsSection : ConfigurationSection
    {
        [ConfigurationProperty("", IsRequired = true, IsDefaultCollection = true)]
        [ConfigurationCollection(typeof(PublicSectorOrgs), AddItemName = "PublicSectorOrg")]
        public PublicSectorOrgs Messages
        {
            get { return (PublicSectorOrgs)this[""]; }
            set { this[""] = value; }
        }
        public override bool IsReadOnly()
        {
            return false;
        }

        public string Serialize()
        {
            return SerializeSection(this, "PublicSectorOrgs", ConfigurationSaveMode.Full);
        }

        public static PublicSectorOrgsSection Deserialize(string xml)
        {
            var PublicSectorOrgsSection = new PublicSectorOrgsSection();
            var rdr = new XmlTextReader(new StringReader(xml));
            PublicSectorOrgsSection.DeserializeSection(rdr);
            return PublicSectorOrgsSection;
        }
    }

    public class PublicSectorOrgs : ConfigurationElementCollection
    {
        public override bool IsReadOnly()
        {
            return false;
        }

        public BindingList<PublicSectorOrg> GetBindingList()
        {
            var results = new BindingList<PublicSectorOrg>();
            foreach (PublicSectorOrg setting in this)
                results.Add(setting);
            return results;
        }

        //Find function
        //public PublicSectorOrg this[int code]
        //{
        //    get
        //    {
        //        if (code<400) return null;
        //        foreach (PublicSectorOrg setting in this)
        //        {
        //            if (setting.Code.EqualsI(code)) return setting;
        //        }
        //        return null;
        //    }
        //}

        public PublicSectorOrg this[int code]
        {
            get
            {
                if (code < 400) return null;
                foreach (PublicSectorOrg setting in this)
                {
                    if (setting.Code.EqualsI(code)) return setting;
                }
                return null;
            }
        }

        internal PublicSectorOrg Default
        {
            get
            {
                foreach (PublicSectorOrg setting in this)
                {
                    if (setting.Default == true) return setting;
                }
                return null;
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new PublicSectorOrg();
        }
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((PublicSectorOrg)element).Code;
        }

        public void Add(PublicSectorOrg element)
        {
            base.BaseAdd(element);
        }

        public void RemoveAt(int index)
        {
            base.BaseRemoveAt(index);
        }

        public void Insert(int index, PublicSectorOrg element)
        {
            base.BaseAdd(index, element);
        }

        protected override string ElementName
        {
            get
            {
                return "PublicSectorOrg";
            }
        }

        protected override bool IsElementName(string elementName)
        {
            return elementName.Equals("PublicSectorOrg", StringComparison.InvariantCultureIgnoreCase);
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMap;
            }
        }

        internal static PublicSectorOrgs Load(System.Configuration.Configuration config)
        {
            var PublicSectorOrgsSection = (PublicSectorOrgsSection)config.GetSection("PublicSectorOrgs");

            //Get the account settings section
            if (PublicSectorOrgsSection == null) PublicSectorOrgsSection = new PublicSectorOrgsSection();
            if (PublicSectorOrgsSection.Messages == null) PublicSectorOrgsSection.Messages = new PublicSectorOrgs();
            var results = PublicSectorOrgsSection.Messages;

            if (results == null) throw new Exception("You must enter all the http error codes and messages.");

            return results;
        }
    }

    //elements for Public Sector change this for it
    [Serializable]
    public class PublicSectorOrg : ConfigurationElement
    {
        public PublicSectorOrg() : base()
        {
        }
        public PublicSectorOrg(int code)
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

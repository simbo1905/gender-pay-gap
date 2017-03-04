using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Xml;
using Extensions;

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

    
        IEnumerable<PublicSectorOrg> _List = null;
        public IEnumerable<PublicSectorOrg> List
        {
            get
            {
                if (_List == null) _List = this.ToList<PublicSectorOrg>().OrderBy(o => o.OrgName);
                return _List;
            }
        }

        public new PublicSectorOrg this[string orgName]
        {
            get
            {
                if (string.IsNullOrWhiteSpace(orgName)) return null;
                foreach (PublicSectorOrg setting in this)
                {
                    if (setting.OrgName.EqualsI(orgName)) return setting;
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
            return ((PublicSectorOrg)element).OrgName;
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

    [Serializable]
    public class PublicSectorOrg : ConfigurationElement
    {
        public PublicSectorOrg() : base()
        {
        }

        public override bool IsReadOnly()
        {
            return false;//true;
        }

        [ConfigurationProperty("orgName", IsKey = true, IsRequired = true)]
        public string  OrgName
        {
            get
            {
                var result = (string)base["orgName"];
                return result;
            }
            set
            {
                base["orgName"] = value;
            }
        }

        [ConfigurationProperty("emailPatterns", IsRequired = false)]
        public string EmailPatterns
        {
            get
            {
                var emailPatterns=(string)base["emailPatterns"];
                emailPatterns = emailPatterns.SplitI(";").Select(ep => ep.ContainsI("*@") ? ep : ep.Contains('@') ? "*"+ep : "*@"+ep).ToDelimitedString(";");
                return emailPatterns;
            }
            set
            {
                base["emailPatterns"] = value;
            }
        }


        public bool IsAuthorised(string emailAddress)
        {
            if (!emailAddress.IsEmailAddress()) throw new ArgumentException("Bad email address");
            if (string.IsNullOrWhiteSpace(EmailPatterns)) throw new ArgumentException("Missing email pattern");
            return emailAddress.LikeAny(EmailPatterns.SplitI(";"));
        }

    }

}

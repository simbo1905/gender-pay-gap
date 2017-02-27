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
using GenderPayGap.WebUI.Models;

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
        IEnumerable<PublicSectorOrg> List
        {
            get
            {
                if (_List == null) _List = this.ToList<PublicSectorOrg>().OrderBy(o => o.OrgName);
                return _List;
            }
        }
        public List<EmployerRecord> SearchOrg(out int totalRecords, string searchText, int page, int pageSize)
        {
            totalRecords = 0;
            List<PublicSectorOrg> returnList = null;
            List<EmployerRecord> tempEmployeeRecord = null;

            try
            {
                //original 
                //var results = List.Where(o =>  o.OrgName.ContainsI(searchText)).ToList();

                //using this to select all to fix pagination.
                var results = List.Where(o => (o.OrgName.ContainsI(searchText))).ToList();

                totalRecords = results.Count;

                if(totalRecords < 0 || page < 0 || pageSize < 0)
                {
                    throw new ArgumentOutOfRangeException("page, pageSize or totalRecords argument","argument value is less that zero!");
                }

                //set page to start from 0 for now zero based index.although ste said it should be 1
                page = 0;
                if (totalRecords >= 0)
                {
                    pageSize = totalRecords;
                }

                //fix the pagination bit after everything else
                ////set page to start from 0 for now zero based index.although ste said it should be 1
                //page = 0;
                ////if total records returned is greater than 0 and it is greater than page size
                //if (totalRecords >= 0 && totalRecords > pageSize)
                //{
                //    //then split the records by the required page size we want (which is 10) do this by deviding total records by page size
                //    //so we can get the number of pages to split the record into.
                //    var numberOfPages = totalRecords / pageSize;

                //    //TODO:FIX this line******
                //    //pageSize = totalRecords;
                //    //pageSize = totalRecords / pageSize;

                //    //if the split is done without any remainders then we have the required number of records per page
                //    var remainder = totalRecords % pageSize;

                //    //but if the split is done with remainders, this means we need an additional page to add the remaining records
                //    //which did not fit in the set of pages with 10 records each.
                //    if (remainder != 0 || remainder > 0)
                //    {
                //        pageSize = pageSize + 1;
                //    }
                //}

                var pageResult = results.GetRange(page, pageSize);
                //var pageResult = results.GetRange(0, 3);

                //var orgs = new List<PublicSectorOrg>();

                //foreach (var item in pageResult)
                //{
                //    var org = new PublicSectorOrg();

                //    org.OrgName = item.OrgName;
                //    org.EmailPatterns = item.EmailPatterns;

                //    orgs.Add(org);
                //}

                //returnList = orgs;

                tempEmployeeRecord =  new List<EmployerRecord>();

                foreach (var item in pageResult)
                {
                   tempEmployeeRecord.Add( new EmployerRecord() { OrgName = item.OrgName, EmailPatterns = item.EmailPatterns });
                }
            }
            catch(Exception e)
            {
                string Msg = "e.Message:" + e.Message  + "  " +   "e.InnerException: " + e.InnerException.ToString();
            }

            return tempEmployeeRecord;
        }


        public PublicSectorOrg this[string orgName]
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

        //internal PublicSectorOrg Default
        //{
        //    get
        //    {
        //        foreach (PublicSectorOrg setting in this)
        //        {
        //            if (setting.Default == true) return setting;
        //        }
        //        return null;
        //    }
        //}

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

        [ConfigurationProperty("emailPatterns", IsRequired = true)]
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

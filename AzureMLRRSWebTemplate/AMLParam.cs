using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace AzureMLRRSWebTemplate
{
    public class AMLParam
    {
        [XmlElement("Name")]
        private string name = "";

        [XmlElement("Type")]
        private string type = "";

        [XmlElement("Format")]
        private string format = "";

        [XmlElement("Enum")]
        private List<string> strEnum = new List<string>();

        [XmlElement("MinValue")]
        private string minValue = "0";

        [XmlElement("MaxValue")]
        private string maxValue = "100";

        [XmlElement("DefaultValue")]
        private string defaultValue = "";

        [XmlElement("Description")]
        private string description = "";

        [XmlElement("Alias")]
        private string @alias = "";

        [XmlElement("Enable")]
        private bool enable = true;

        public string Alias
        {
            get
            {
                return this.@alias;
            }
            set
            {
                this.@alias = value;
            }
        }

        public string DefaultValue
        {
            get
            {
                return this.defaultValue;
            }
            set
            {
                this.defaultValue = value;
            }
        }

        public string Description
        {
            get
            {
                return this.description;
            }
            set
            {
                this.description = value;
            }
        }

        public bool Enable
        {
            get
            {
                return this.enable;
            }
            set
            {
                this.enable = value;
            }
        }

        public string Format
        {
            get
            {
                return this.format;
            }
            set
            {
                this.format = value;
            }
        }

        public string MaxValue
        {
            get
            {
                return this.maxValue;
            }
            set
            {
                this.maxValue = value;
            }
        }

        public string MinValue
        {
            get
            {
                return this.minValue;
            }
            set
            {
                this.minValue = value;
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = value;
            }
        }

        public List<string> StrEnum
        {
            get
            {
                return this.strEnum;
            }
            set
            {
                this.strEnum = value;
            }
        }

        public string Type
        {
            get
            {
                return this.type;
            }
            set
            {
                this.type = value;
            }
        }

        public AMLParam()
        {
        }
    }
}
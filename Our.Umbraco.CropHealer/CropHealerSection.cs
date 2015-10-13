using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Our.Umbraco.CropHealer
{
    public class CropHealerConfigSection : ConfigurationSection
    {
        [ConfigurationProperty("exclusions", IsDefaultCollection = true)]
        public ExclusionsElement Exclusions
        {
            get { return (ExclusionsElement) this["exclusions"]; }
            set { this["exclusions"] = value; }
        }
    }

    public class ExclusionsElement : ConfigurationElement
    {
        [ConfigurationProperty("contentTypes", IsDefaultCollection = true)]
        public GenericTypeElementCollection DocumentGenericTypes
        {
            get { return (GenericTypeElementCollection) this["contentTypes"]; }
            set { this["contentTypes"] = value; }
        }

        [ConfigurationProperty("mediaTypes", IsDefaultCollection = true)]
        public GenericTypeElementCollection MediaGenericTypes
        {
            get { return (GenericTypeElementCollection)this["mediaTypes"]; }
            set { this["mediaTypes"] = value; }
        }

        [ConfigurationProperty("memberTypes", IsDefaultCollection = true)]
        public GenericTypeElementCollection MemberGenericTypes
        {
            get { return (GenericTypeElementCollection)this["memberTypes"]; }
            set { this["memberTypes"] = value; }
        }

        [ConfigurationProperty("dataTypes", IsDefaultCollection = true)]
        public DataTypeElementCollection DataTypes
        {
            get { return (DataTypeElementCollection)this["dataTypes"]; }
            set { this["dataTypes"] = value; }
        }
    }

    public class GenericTypeElement : ConfigurationElement
    {
        [ConfigurationProperty("alias", IsKey = true, IsRequired = true)]
        public string Alias
        {
            get { return (string)this["alias"]; }
            set { this["alias"] = value; }
        }
    }

    public class DataTypeElement : ConfigurationElement
    {
        [ConfigurationProperty("key", IsKey = false, IsRequired = true)]
        public Guid Key
        {
            get { return (Guid)this["key"]; }
            set { this["key"] = value; }
        }
    }

    [ConfigurationCollection(typeof(GenericTypeElement))]
    public class GenericTypeElementCollection : ConfigurationElementCollection, IEnumerable<GenericTypeElement>
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new GenericTypeElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((GenericTypeElement)element).Alias;
        }

        public new IEnumerator<GenericTypeElement> GetEnumerator()
        {
            int count = base.Count;
            for (int i = 0; i < count; i++)
            {
                yield return base.BaseGet(i) as GenericTypeElement;
            }
        }
    }

    [ConfigurationCollection(typeof(DataTypeElement))]
    public class DataTypeElementCollection : ConfigurationElementCollection, IEnumerable<DataTypeElement>
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new DataTypeElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((DataTypeElement)element).Key;
        }

        public new IEnumerator<DataTypeElement> GetEnumerator()
        {
            int count = base.Count;
            for (int i = 0; i < count; i++)
            {
                yield return base.BaseGet(i) as DataTypeElement;
            }
        }
    }
}

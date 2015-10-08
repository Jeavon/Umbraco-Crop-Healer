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
        [ConfigurationProperty("documentTypes", IsDefaultCollection = true)]
        public TypeElementCollection DocumentTypes
        {
            get { return (TypeElementCollection) this["documentTypes"]; }
            set { this["documentTypes"] = value; }
        }

        [ConfigurationProperty("mediaTypes", IsDefaultCollection = true)]
        public TypeElementCollection MediaTypes
        {
            get { return (TypeElementCollection)this["mediaTypes"]; }
            set { this["mediaTypes"] = value; }
        }
    }

    public class TypeElement : ConfigurationElement
    {
        [ConfigurationProperty("alias", IsKey = true, IsRequired = true)]
        public string Alias
        {
            get { return (string)this["alias"]; }
            set { this["alias"] = value; }
        }
    }

    [ConfigurationCollection(typeof(TypeElement))]
    public class TypeElementCollection : ConfigurationElementCollection, IEnumerable<TypeElement>
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new TypeElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((TypeElement)element).Alias;
        }

        public new IEnumerator<TypeElement> GetEnumerator()
        {
            int count = base.Count;
            for (int i = 0; i < count; i++)
            {
                yield return base.BaseGet(i) as TypeElement;
            }
        }
    }
}

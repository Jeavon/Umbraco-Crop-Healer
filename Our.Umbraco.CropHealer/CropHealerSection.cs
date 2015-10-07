using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Our.Umbraco.CropHealer
{
    public class CropHealerSection : ConfigurationSection
    {
        public ExclusionsElement Exclusions
        {
            get { return (ExclusionsElement) this["exclusions"]; }
            set { this["exclusions"] = value; }
        }
    }

    public class ExclusionsElement : ConfigurationElement
    {        
    }
}

using System;

namespace Our.Umbraco.CropHealer.Tests
{
    using System.Configuration;
    using System.Linq;

    using NUnit.Framework;

    [TestFixture]
    public class ConfigurationTests
    {
        [Test]
        public void BasicDocumentTypeElementTest()
        {
            var section = ConfigurationManager.GetSection("CropHealer")
                             as CropHealerConfigSection;

            Assert.AreEqual(section.Exclusions.DocumentGenericTypes.FirstOrDefault().Alias, "umbNewsItem");
        }

        [Test]
        public void BasicMediaTypeElementTest()
        {
            var section = ConfigurationManager.GetSection("CropHealer")
                             as CropHealerConfigSection;

            Assert.AreEqual(section.Exclusions.MediaGenericTypes.FirstOrDefault().Alias, "Image");
        }

        [Test]
        public void BasicDataTypeElementTest()
        {
            var section = ConfigurationManager.GetSection("CropHealer")
                             as CropHealerConfigSection;

            Assert.AreEqual(section.Exclusions.DataTypes.FirstOrDefault().Key, new Guid("ad22693b-ee84-4142-b601-35e43d615411"));
        }
    }
}

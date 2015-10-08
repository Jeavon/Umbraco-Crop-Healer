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

            Assert.AreEqual(section.Exclusions.DocumentTypes.FirstOrDefault().Alias, "umbNewsItem");
        }

        [Test]
        public void BasicMediaypeElementTest()
        {
            var section = ConfigurationManager.GetSection("CropHealer")
                             as CropHealerConfigSection;

            Assert.AreEqual(section.Exclusions.MediaTypes.FirstOrDefault().Alias, "Image");
        }
    }
}

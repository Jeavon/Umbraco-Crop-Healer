namespace Our.Umbraco.CropHealer.Tests
{
    using System.Collections.Generic;

    using NUnit.Framework;

    using global::Umbraco.Web.Models;

    [TestFixture]
    public class HealerTests
    {

        [Test]
        public void TestCheckAndHealUpload()
        {
            var sampleJson = "/media/1002/img_5227.jpg";

            var expectedJson = "{" + "\"src\":\"/media/1002/img_5227.jpg\"," + "\"focalPoint\":{" + "\"left\":0.5,"
                               + "\"top\":0.5" + "}" + "}";

            var cropDataSet = sampleJson.SerializeToCropDataSet();

            Assert.AreEqual(expectedJson, cropDataSet.ImageCropDataSetRepair(sampleJson));
        }

        [Test]
        public void TestCheckAndHealDoNothing()
        {
            var sampleJson = "{" + "\"src\":\"/media/1002/img_5227.jpg\"," + "\"focalPoint\":{" + "\"left\":0.5,"
                             + "\"top\":0.5" + "}," + "\"crops\":[" + "{" + "\"alias\":\"home\"," + "\"width\":270,"
                             + "\"height\":161" + "}" + "]" + "}";

            var cropDataSet = sampleJson.SerializeToCropDataSet();

            Assert.AreEqual(null, cropDataSet.ImageCropDataSetRepair(sampleJson));
        }

        [Test]
        public void TestCheckAndHealNoFocal()
        {
            var sampleJson = "{" + "\"src\":\"/media/1002/img_5227.jpg\"" + "}";
            var expectedJson = "{" + "\"src\":\"/media/1002/img_5227.jpg\"," + "\"focalPoint\":{" + "\"left\":0.5,"
                               + "\"top\":0.5" + "}" + "}";

            var cropDataSet = sampleJson.SerializeToCropDataSet();

            Assert.AreEqual(expectedJson, cropDataSet.ImageCropDataSetRepair(sampleJson));
        }

        [Test]
        public void TestCheckAndHealCrop()
        {
            var sampleJson = "{" + "\"src\":\"/media/1002/img_5227.jpg\"," + "\"focalPoint\":{" + "\"left\":0.5,"
                             + "\"top\":0.5" + "}" + "}";

            var expectedJson = "{" + "\"src\":\"/media/1002/img_5227.jpg\"," + "\"focalPoint\":{" + "\"left\":0.5,"
                               + "\"top\":0.5" + "}," + "\"crops\":[" + "{" + "\"alias\":\"home\"," + "\"width\":270,"
                               + "\"height\":161" + "}" + "]" + "}";

            var cropDataSet = sampleJson.SerializeToCropDataSet();
            var homeCrop = new List<ImageCropData>()
                               {
                                   new ImageCropData() { Alias = "home", Width = 270, Height = 161 }
                               };

            Assert.AreEqual(expectedJson, cropDataSet.ImageCropDataSetRepair(sampleJson, homeCrop));
        }

        [Test]
        public void TestCheckAndHealCropNoNothing()
        {
            var sampleJson = "{" + "\"src\":\"/media/1002/img_5227.jpg\"," + "\"focalPoint\":{" + "\"left\":0.5,"
                             + "\"top\":0.5" + "}," + "\"crops\":[" + "{" + "\"alias\":\"home\"," + "\"width\":270,"
                             + "\"height\":161" + "}" + "]" + "}";

            var cropDataSet = sampleJson.SerializeToCropDataSet();
            var homeCrop = new List<ImageCropData>()
                               {
                                   new ImageCropData() { Alias = "home", Width = 270, Height = 161 }
                               };
            Assert.AreEqual(null, cropDataSet.ImageCropDataSetRepair(sampleJson, homeCrop));
        }

        [Test]
        public void TestCheckAndHealCropNoNothingMoreThanOneCrop()
        {
            var sampleJson = "{" + "\"src\":\"/media/1002/img_5227.jpg\"," + "\"focalPoint\":{" + "\"left\":0.5,"
                             + "\"top\":0.5" + "}," + "\"crops\":[" + "{" + "\"alias\":\"home\"," + "\"width\":270,"
                             + "\"height\":161" + "}," + "{" + "\"alias\":\"thumb\"," + "\"width\":100,"
                             + "\"height\":100" + "}" + "]" + "}";

            var cropDataSet = sampleJson.SerializeToCropDataSet();
            var homeCrop = new List<ImageCropData>()
                               {
                                   new ImageCropData() { Alias = "home", Width = 270, Height = 161 },
                                   new ImageCropData()
                                       {
                                           Alias = "thumb",
                                           Width = 100,
                                           Height = 100
                                       }
                               };
            Assert.AreEqual(null, cropDataSet.ImageCropDataSetRepair(sampleJson, homeCrop));
        }

        [Test]
        public void TestCheckAndHealCropAddAdditionalCrop()
        {
            var sampleJson = "{" + "\"src\":\"/media/1002/img_5227.jpg\"," + "\"focalPoint\":{" + "\"left\":0.5,"
                             + "\"top\":0.5" + "}," + "\"crops\":[" + "{" + "\"alias\":\"thumb\"," + "\"width\":100,"
                             + "\"height\":100" + "}" + "]" + "}";

            var expectedJson = "{" + "\"src\":\"/media/1002/img_5227.jpg\"," + "\"focalPoint\":{" + "\"left\":0.5,"
                               + "\"top\":0.5" + "}," + "\"crops\":[" + "{" + "\"alias\":\"thumb\"," + "\"width\":100,"
                               + "\"height\":100" + "}," + "{" + "\"alias\":\"home\"," + "\"width\":270,"
                               + "\"height\":161" + "}" + "]" + "}";

            var cropDataSet = sampleJson.SerializeToCropDataSet();
            var homeCrop = new List<ImageCropData>()
                               {
                                   new ImageCropData() { Alias = "home", Width = 270, Height = 161 },
                                   new ImageCropData()
                                       {
                                           Alias = "thumb",
                                           Width = 100,
                                           Height = 100
                                       }
                               };
            Assert.AreEqual(expectedJson, cropDataSet.ImageCropDataSetRepair(sampleJson, homeCrop));
        }

        [Test]
        public void TestCheckAndHealCropRemoveThumb()
        {
            var sampleJson = "{" + "\"src\":\"/media/1002/img_5227.jpg\"," + "\"focalPoint\":{" + "\"left\":0.5,"
                             + "\"top\":0.5" + "}," + "\"crops\":[" + "{" + "\"alias\":\"thumb\"," + "\"width\":100,"
                             + "\"height\":100" + "}," + "{" + "\"alias\":\"home\"," + "\"width\":270,"
                             + "\"height\":161" + "}" + "]" + "}";

            var expectedJson = "{" + "\"src\":\"/media/1002/img_5227.jpg\"," + "\"focalPoint\":{" + "\"left\":0.5,"
                               + "\"top\":0.5" + "}," + "\"crops\":[" + "{" + "\"alias\":\"home\"," + "\"width\":270,"
                               + "\"height\":161" + "}" + "]" + "}";

            var cropDataSet = sampleJson.SerializeToCropDataSet();
            var homeCrop = new List<ImageCropData>()
                               {
                                   new ImageCropData() { Alias = "home", Width = 270, Height = 161 },
                               };
            Assert.AreEqual(expectedJson, cropDataSet.ImageCropDataSetRepair(sampleJson, homeCrop));
        }

        [Test]
        public void TestCheckAndHealCropUpdateDimension()
        {
            var sampleJson = "{" + "\"src\":\"/media/1002/img_5227.jpg\"," + "\"focalPoint\":{" + "\"left\":0.5,"
                             + "\"top\":0.5" + "}," + "\"crops\":[" + "{" + "\"alias\":\"thumb\"," + "\"width\":100,"
                             + "\"height\":100," + "\"coordinates\":{" + "\"x1\":0.019817004121936835,"
                             + "\"y1\":0.40766408479412913," + "\"x2\":0.21581283688907102,"
                             + "\"y2\":0.2504925941024605" + "}" + "}," + "{" + "\"alias\":\"home\"," + "\"width\":270,"
                             + "\"height\":161" + "}" + "]" + "}";

            var expectedJson = "{" + "\"src\":\"/media/1002/img_5227.jpg\"," + "\"focalPoint\":{" + "\"left\":0.5,"
                               + "\"top\":0.5" + "}," + "\"crops\":[" + "{" + "\"alias\":\"thumb\"," + "\"width\":200,"
                               + "\"height\":200," + "\"coordinates\":{" + "\"x1\":0.019817004121936835,"
                               + "\"y1\":0.40766408479412913," + "\"x2\":0.21581283688907102,"
                               + "\"y2\":0.2504925941024605" + "}" + "}," + "{" + "\"alias\":\"home\","
                               + "\"width\":270," + "\"height\":161" + "}" + "]" + "}";

            var cropDataSet = sampleJson.SerializeToCropDataSet();
            var homeCrop = new List<ImageCropData>()
                               {
                                   new ImageCropData() { Alias = "home", Width = 270, Height = 161 },
                                   new ImageCropData()
                                       {
                                           Alias = "thumb",
                                           Width = 200,
                                           Height = 200
                                       }
                               };
            Assert.AreEqual(expectedJson, cropDataSet.ImageCropDataSetRepair(sampleJson, homeCrop));
        }
    }
}

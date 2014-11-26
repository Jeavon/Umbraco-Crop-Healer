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

            Assert.AreEqual(expectedJson, cropDataSet.ImageCropDataSetRepair(sampleJson, 0, 0));
        }

        [Test]
        public void TestCheckAndHealDoNothing()
        {
            var sampleJson = "{" + "\"src\":\"/media/1002/img_5227.jpg\"," + "\"focalPoint\":{" + "\"left\":0.5,"
                             + "\"top\":0.5" + "}," + "\"crops\":[" + "{" + "\"alias\":\"home\"," + "\"width\":270,"
                             + "\"height\":161" + "}" + "]" + "}";

            var cropDataSet = sampleJson.SerializeToCropDataSet();

            Assert.AreEqual(null, cropDataSet.ImageCropDataSetRepair(sampleJson, 0, 0));
        }

        [Test]
        public void TestCheckAndHealNoFocal()
        {
            var sampleJson = "{" + "\"src\":\"/media/1002/img_5227.jpg\"" + "}";
            var expectedJson = "{" + "\"src\":\"/media/1002/img_5227.jpg\"," + "\"focalPoint\":{" + "\"left\":0.5,"
                               + "\"top\":0.5" + "}" + "}";

            var cropDataSet = sampleJson.SerializeToCropDataSet();

            Assert.AreEqual(expectedJson, cropDataSet.ImageCropDataSetRepair(sampleJson, 0, 0));
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

            Assert.AreEqual(expectedJson, cropDataSet.ImageCropDataSetRepair(sampleJson, 0, 0, homeCrop));
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
            Assert.AreEqual(null, cropDataSet.ImageCropDataSetRepair(sampleJson, 0, 0, homeCrop));
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
            Assert.AreEqual(null, cropDataSet.ImageCropDataSetRepair(sampleJson, 0, 0, homeCrop));
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
            Assert.AreEqual(expectedJson, cropDataSet.ImageCropDataSetRepair(sampleJson, 0, 0, homeCrop));
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
            Assert.AreEqual(expectedJson, cropDataSet.ImageCropDataSetRepair(sampleJson, 0, 0, homeCrop));
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
            Assert.AreEqual(expectedJson, cropDataSet.ImageCropDataSetRepair(sampleJson, 0, 0, homeCrop));
        }

        [Test]
        public void TestCheckAndHealCropIncreaseHeight()
        {
            var sampleJson = "{" + "\"src\":\"/media/1002/img_5227.jpg\"," + "\"focalPoint\":{" + "\"left\":0.5,"
                             + "\"top\":0.5" + "}," + "\"crops\":[" + "{" + "\"alias\":\"thumb\"," + "\"width\":300,"
                             + "\"height\":100," + "\"coordinates\":{" + "\"x1\":0.166,"
                             + "\"y1\":0.333," + "\"x2\":0.333,"
                             + "\"y2\":0.5" + "}" + "}" + "]" + "}";

            var expectedJson = "{" + "\"src\":\"/media/1002/img_5227.jpg\"," + "\"focalPoint\":{" + "\"left\":0.5,"
                             + "\"top\":0.5" + "}," + "\"crops\":[" + "{" + "\"alias\":\"thumb\"," + "\"width\":300,"
                             + "\"height\":200," + "\"coordinates\":{" + "\"x1\":0.166,"
                             + "\"y1\":0.333," + "\"x2\":0.333,"
                             + "\"y2\":0.33333333333333333" + "}" + "}" + "]" + "}";

            var cropDataSet = sampleJson.SerializeToCropDataSet();
            var homeCrop = new List<ImageCropData>()
                               {
                                   new ImageCropData() { Alias = "thumb", Width = 300, Height = 200 }
                               };
            Assert.AreEqual(expectedJson, cropDataSet.ImageCropDataSetRepair(sampleJson, 600, 600, homeCrop));
        }

        [Test]
        public void TestCheckAndHealCropIncreaseHeightBounce()
        {
            var sampleJson = "{" + "\"src\":\"/media/1002/img_5227.jpg\"," + "\"focalPoint\":{" + "\"left\":0.5,"
                             + "\"top\":0.5" + "}," + "\"crops\":[" + "{" + "\"alias\":\"thumb\"," + "\"width\":300,"
                             + "\"height\":100," + "\"coordinates\":{" + "\"x1\":0.166,"
                             + "\"y1\":0.833," + "\"x2\":0.333,"
                             + "\"y2\":0" + "}" + "}" + "]" + "}";

            var expectedJson = "{" + "\"src\":\"/media/1002/img_5227.jpg\"," + "\"focalPoint\":{" + "\"left\":0.5,"
                             + "\"top\":0.5" + "}," + "\"crops\":[" + "{" + "\"alias\":\"thumb\"," + "\"width\":300,"
                             + "\"height\":200," + "\"coordinates\":{" + "\"x1\":0.166,"
                             + "\"y1\":0.66633333333333333," + "\"x2\":0.333,"
                             + "\"y2\":0" + "}" + "}" + "]" + "}";

            var cropDataSet = sampleJson.SerializeToCropDataSet();
            var homeCrop = new List<ImageCropData>()
                               {
                                   new ImageCropData() { Alias = "thumb", Width = 300, Height = 200 }
                               };
            Assert.AreEqual(expectedJson, cropDataSet.ImageCropDataSetRepair(sampleJson, 600, 600, homeCrop));
        }


        [Test]
        public void TestCheckAndHealCropIncreaseHeightTooBig()
        {
            var sampleJson = "{" + "\"src\":\"/media/1002/img_5227.jpg\"," + "\"focalPoint\":{" + "\"left\":0.5,"
                             + "\"top\":0.5" + "}," + "\"crops\":[" + "{" + "\"alias\":\"thumb\"," + "\"width\":300,"
                             + "\"height\":100," + "\"coordinates\":{" + "\"x1\":0.166,"
                             + "\"y1\":0.833," + "\"x2\":0.333,"
                             + "\"y2\":0" + "}" + "}" + "]" + "}";

            var expectedJson = "{" + "\"src\":\"/media/1002/img_5227.jpg\"," + "\"focalPoint\":{" + "\"left\":0.5,"
                             + "\"top\":0.5" + "}," + "\"crops\":[" + "{" + "\"alias\":\"thumb\"," + "\"width\":300,"
                             + "\"height\":1000" + "}" + "]" + "}";

            var cropDataSet = sampleJson.SerializeToCropDataSet();
            var homeCrop = new List<ImageCropData>()
                               {
                                   new ImageCropData() { Alias = "thumb", Width = 300, Height = 1000 }
                               };
            Assert.AreEqual(expectedJson, cropDataSet.ImageCropDataSetRepair(sampleJson, 600, 600, homeCrop));
        }

        [Test]
        public void TestCheckAndHealCropIncreaseWidth()
        {
            var sampleJson = "{" + "\"src\":\"/media/1001/gridfw.png\"," + "\"focalPoint\":{" + "\"left\":0.5,"
                             + "\"top\":0.5" + "}," + "\"crops\":[" + "{" + "\"alias\":\"Thumb\"," + "\"width\":400,"
                             + "\"height\":300" + "}," + "{" + "\"alias\":\"Wide\"," + "\"width\":300,"
                             + "\"height\":100," + "\"coordinates\":{" + "\"x1\":0," + "\"y1\":0,"
                             + "\"x2\":0.49949949949949946," + "\"y2\":0.83316649983316649" + "}" + "}" + "]" + "}";

            var expectedJson = "{" + "\"src\":\"/media/1001/gridfw.png\"," + "\"focalPoint\":{" + "\"left\":0.5,"
                               + "\"top\":0.5" + "}," + "\"crops\":[" + "{" + "\"alias\":\"Thumb\"," + "\"width\":400,"
                               + "\"height\":300" + "}," + "{" + "\"alias\":\"Wide\"," + "\"width\":400,"
                               + "\"height\":100," + "\"coordinates\":{" + "\"x1\":0," + "\"y1\":0,"
                               + "\"x2\":0.33283283283283279," + "\"y2\":0.83316649983316649" + "}" + "}" + "]" + "}";

            var cropDataSet = sampleJson.SerializeToCropDataSet();
            var homeCrop = new List<ImageCropData>()
                               {
                                   new ImageCropData() { Alias = "Thumb", Width = 400, Height = 300 },
                                   new ImageCropData() { Alias = "Wide", Width = 400, Height = 100 }
                               };
            Assert.AreEqual(expectedJson, cropDataSet.ImageCropDataSetRepair(sampleJson, 600, 600, homeCrop));
        }

        [Test]
        public void TestCheckAndHealCropIncreaseWidthBounce()
        {
            var sampleJson = "{" + "\"src\":\"/media/1001/gridfw.png\"," + "\"focalPoint\":{" + "\"left\":0.5,"
                             + "\"top\":0.5" + "}," + "\"crops\":[" + "{" + "\"alias\":\"Thumb\"," + "\"width\":400,"
                             + "\"height\":300" + "}," + "{" + "\"alias\":\"Wide\"," + "\"width\":300,"
                             + "\"height\":100," + "\"coordinates\":{" + "\"x1\":0.49948385885885888,"
                             + "\"y1\":0.16683350016683351," + "\"x2\":0,"
                             + "\"y2\":0.666332999666333" + "}" + "}" + "]" + "}";

            var expectedJson = "{" + "\"src\":\"/media/1001/gridfw.png\"," + "\"focalPoint\":{" + "\"left\":0.5,"
                               + "\"top\":0.5" + "}," + "\"crops\":[" + "{" + "\"alias\":\"Thumb\"," + "\"width\":400,"
                               + "\"height\":300" + "}," + "{" + "\"alias\":\"Wide\"," + "\"width\":400,"
                               + "\"height\":100," + "\"coordinates\":{" + "\"x1\":0.33281719219219221,"
                               + "\"y1\":0.16683350016683351," + "\"x2\":0," + "\"y2\":0.666332999666333" + "}" + "}"
                               + "]" + "}";

            var cropDataSet = sampleJson.SerializeToCropDataSet();
            var homeCrop = new List<ImageCropData>()
                               {
                                   new ImageCropData() { Alias = "Thumb", Width = 400, Height = 300 },
                                   new ImageCropData() { Alias = "Wide", Width = 400, Height = 100 }
                               };
            Assert.AreEqual(expectedJson, cropDataSet.ImageCropDataSetRepair(sampleJson, 600, 600, homeCrop));
        }
    }
}

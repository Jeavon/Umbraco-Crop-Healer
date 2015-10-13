// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CropHealer.cs" company="Our Umbraco">
//   OurUmbraco
// </copyright>
// <summary>
//   Crop Healer class to update content, media & member image cropper JSON with new values
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Configuration;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Our.Umbraco.CropHealer.Tests")]

namespace Our.Umbraco.CropHealer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Newtonsoft.Json;

    using umbraco;
    using global::Umbraco.Core;
    using global::Umbraco.Core.IO;
    using global::Umbraco.Core.Logging;
    using global::Umbraco.Core.Models;
    using global::Umbraco.Core.Services;
    using global::Umbraco.Web.Models;

    public static class CropHealer
    {
        internal static void SeekAndHeal(IDataTypeDefinition dataType, IDataTypeService dts)
        {
            var cts = ApplicationContext.Current.Services.ContentTypeService;
            var mts = ApplicationContext.Current.Services.MemberTypeService;

            var cs = ApplicationContext.Current.Services.ContentService;
            var ms = ApplicationContext.Current.Services.MediaService;
            var mems = ApplicationContext.Current.Services.MemberService;

            var dataTypeId = dataType.Id;

            // A Image Cropper Data Type has been saved, lets go to work!
            var allContentTypes = cts.GetAllContentTypes();
            var allMediaTypes = cts.GetAllMediaTypes();
            var allMemberTypes = mts.GetAll();

            var cropHealerConfig = ConfigurationManager.GetSection("CropHealer") as CropHealerConfigSection;

            if (cropHealerConfig != null)
            {
                allContentTypes = allContentTypes.Where(ct => !cropHealerConfig.Exclusions.ContentTypes.Select(x => x.Alias).Contains(ct.Alias));
                allMediaTypes = allMediaTypes.Where(ct => !cropHealerConfig.Exclusions.MediaTypes.Select(x => x.Alias).Contains(ct.Alias));
                allMemberTypes = allMemberTypes.Where(ct => !cropHealerConfig.Exclusions.MemberTypes.Select(x => x.Alias).Contains(ct.Alias));
            }

            var dataTypeCrops = GetCropsFromDataType(dataTypeId, dts);

            // Content
            foreach (var contentType in allContentTypes)
            {
                var contentTypeId = contentType.Id;
                var cropperContentProperties = contentType.PropertyTypes.Where(x => x.DataTypeDefinitionId == dataTypeId);
                if (cropperContentProperties.Any())
                {
                    var allContentUsingThis = cs.GetContentOfContentType(contentTypeId);
                    HealContentItems(allContentUsingThis, cropperContentProperties, dataTypeCrops, dts, cs);
                }
            }

            // Media
            foreach (var mediaType in allMediaTypes)
            {
                var mediaTypeId = mediaType.Id;
                var cropperContentProperties = mediaType.PropertyTypes.Where(x => x.DataTypeDefinitionId == dataTypeId);
                if (cropperContentProperties.Any())
                {
                    var allMediaUsingThis = ms.GetMediaOfMediaType(mediaTypeId);
                    HealMediaItems(allMediaUsingThis, cropperContentProperties, dataTypeCrops, dts, ms);
                }
            }

            // Member
            foreach (var memberType in allMemberTypes)
            {
                var memberTypeId = memberType.Id;
                var cropperContentProperties = memberType.PropertyTypes.Where(x => x.DataTypeDefinitionId == dataTypeId);
                if (cropperContentProperties.Any())
                {
                    var allMembersUsingThis = mems.GetMembersByMemberType(memberTypeId);
                    HealMemberItems(allMembersUsingThis, cropperContentProperties, dataTypeCrops, dts, mems);
                }
            }            
        }

        private static void HealContentItems(IEnumerable<IContent> content, IEnumerable<PropertyType> cropperContentProperties, List<ImageCropData> dataTypeCrops, IDataTypeService dts, IContentService cs)
        {
            foreach (var contentItem in content)
            {
                var healedAProperty = false;

                // There could be multiple uses of the same datatype on the content type 
                foreach (var cropperContentProperty in cropperContentProperties)
                {
                    var cropperPropertyValue = contentItem.GetValue<string>(cropperContentProperty.Alias);
                    var cropDataSet = cropperPropertyValue.SerializeToCropDataSet();

                    if (!string.IsNullOrEmpty(cropDataSet.Src))
                    {

                        var imageDimensions = GetMediaFileDimensions(cropDataSet.Src);

                        if (imageDimensions != null)
                        {
                            var attemptHeal = ImageCropDataSetRepair(
                                cropDataSet,
                                cropperPropertyValue,
                                imageDimensions.Width,
                                imageDimensions.Height,
                                dataTypeCrops);
                            if (attemptHeal != null)
                            {
                                contentItem.SetValue(cropperContentProperty.Alias, attemptHeal);
                                healedAProperty = true;
                            }
                        }
                        else
                        {
                            LogHelper.Info(
                                typeof(CropHealer),
                                string.Format(
                                    "Unable to attempt heal Image Cropper content property, possibly a svg or other format (NodeId:{0} DocumentTypeAlias:{1}, PropertyAlias:{2})",
                                    contentItem.Id,
                                    contentItem.ContentType.Alias,
                                    cropperContentProperty.Alias));
                        }
                    }
                    else
                    {
                        LogHelper.Info(typeof(CropHealer), string.Format("Skipping content property as no image has been uploaded yet (NodeId:{0} DocumentTypeAlias:{1}, PropertyAlias:{2})",
                                    contentItem.Id,
                                    contentItem.ContentType.Alias,
                                    cropperContentProperty.Alias));                         
                    }
                }

                if (healedAProperty)
                {
                    if (contentItem.Published)
                    {
                        cs.SaveAndPublishWithStatus(contentItem);
                        LogHelper.Info(
                            typeof(CropHealer),
                            string.Format(
                                "Healed a Image Cropper in content (NodeId:{0} DocumentTypeAlias:{1}) and Published",
                                contentItem.Id,
                                contentItem.ContentType.Alias));
                    }
                    else
                    {
                        cs.Save(contentItem);
                        LogHelper.Info(
                            typeof(CropHealer),
                            string.Format(
                                "Healed a Image Cropper in content (NodeId:{0} DocumentTypeAlias:{1}) but didn't publish as it contained unpublished content",
                                contentItem.Id,
                                contentItem.ContentType.Alias));
                    }
                }
            }
        }

        private static void HealMediaItems(IEnumerable<IMedia> media, IEnumerable<PropertyType> cropperContentProperties, List<ImageCropData> dataTypeCrops, IDataTypeService dts, IMediaService ms)
        {
            foreach (var mediaItem in media)
            {
                var healedAProperty = false;
             
                // There could be multiple uses of the same datatype on the media type 
                foreach (var cropperContentProperty in cropperContentProperties)
                {
                    var cropperPropertyValue = mediaItem.GetValue<string>(cropperContentProperty.Alias);

                    var cropDataSet = cropperPropertyValue.SerializeToCropDataSet();

                    if (!string.IsNullOrEmpty(cropDataSet.Src))
                    {
                        // Use values saved in media item if available before seeking out from file system
                        var imageDimensions = new ImageDimensions();

                        if (mediaItem.HasProperty(Constants.Conventions.Media.Width)
                            && mediaItem.HasProperty(Constants.Conventions.Media.Height)
                            && mediaItem.GetValue<int>(Constants.Conventions.Media.Width) > 0
                            && mediaItem.GetValue<int>(Constants.Conventions.Media.Height) > 0)
                        {
                            imageDimensions.Width = mediaItem.GetValue<int>(Constants.Conventions.Media.Width);
                            imageDimensions.Height = mediaItem.GetValue<int>(Constants.Conventions.Media.Height);
                        }
                        else
                        {
                            imageDimensions = GetMediaFileDimensions(cropDataSet.Src);
                        }

                        if (imageDimensions != null)
                        {
                            var attemptHeal = ImageCropDataSetRepair(
                                cropDataSet,
                                cropperPropertyValue,
                                imageDimensions.Width,
                                imageDimensions.Height,
                                dataTypeCrops);
                            if (attemptHeal != null)
                            {
                                mediaItem.SetValue(cropperContentProperty.Alias, attemptHeal);
                                healedAProperty = true;
                            }
                        }
                        else
                        {
                            LogHelper.Info(
                                typeof(CropHealer),
                                string.Format(
                                    "Unable to attempt heal Image Cropper media property, possibly a svg or other format (NodeId:{0} DocumentTypeAlias:{1}, PropertyAlias:{2})",
                                    mediaItem.Id,
                                    mediaItem.ContentType.Alias,
                                    cropperContentProperty.Alias));
                        }
                    }
                    else
                    {
                        LogHelper.Info(typeof(CropHealer), string.Format("Skipping media property as no image has been uploaded yet (NodeId:{0} DocumentTypeAlias:{1}, PropertyAlias:{2})",
                                    mediaItem.Id,
                                    mediaItem.ContentType.Alias,
                                    cropperContentProperty.Alias));                        
                    }
                }

                if (healedAProperty)
                {
                    ms.Save(mediaItem);
                    LogHelper.Info(
                        typeof(CropHealer),
                        string.Format(
                            "Healed a Image Cropper in media (NodeId:{0} DocumentTypeAlias:{1})",
                            mediaItem.Id,
                            mediaItem.ContentType.Alias));
                }
            }
        }

        private static void HealMemberItems(IEnumerable<IMember> member, IEnumerable<PropertyType> cropperContentProperties, List<ImageCropData> dataTypeCrops, IDataTypeService dts, IMemberService mems)
        {
            foreach (var memberItem in member)
            {
                var healedAProperty = false;

                // There could be multiple uses of the same datatype on the content type 
                foreach (var cropperContentProperty in cropperContentProperties)
                {
                    var cropperPropertyValue = memberItem.GetValue<string>(cropperContentProperty.Alias);

                    var cropDataSet = cropperPropertyValue.SerializeToCropDataSet();

                    if (!string.IsNullOrEmpty(cropDataSet.Src))
                    {
                        var imageDimensions = GetMediaFileDimensions(cropDataSet.Src);

                        if (imageDimensions != null)
                        {
                            var attemptHeal = ImageCropDataSetRepair(
                                cropDataSet,
                                cropperPropertyValue,
                                imageDimensions.Width,
                                imageDimensions.Height,
                                dataTypeCrops);
                            if (attemptHeal != null)
                            {
                                memberItem.SetValue(cropperContentProperty.Alias, attemptHeal);
                                healedAProperty = true;
                            }
                        }
                        else
                        {
                            LogHelper.Info(
                                typeof(CropHealer),
                                string.Format(
                                    "Unable to attempt heal Image Cropper member property, possibly a svg or other format (NodeId:{0} DocumentTypeAlias:{1}, PropertyAlias:{2})",
                                    memberItem.Id,
                                    memberItem.ContentType.Alias,
                                    cropperContentProperty.Alias));
                        }
                    }
                    else
                    {
                        LogHelper.Info(typeof(CropHealer), string.Format("Skipping member property as no image has been uploaded yet (NodeId:{0} DocumentTypeAlias:{1}, PropertyAlias:{2})",
                                    memberItem.Id,
                                    memberItem.ContentType.Alias,
                                    cropperContentProperty.Alias));
                    }
                }

                if (healedAProperty)
                {
                    mems.Save(memberItem);
                    LogHelper.Info(
                        typeof(CropHealer),
                        string.Format(
                            "Healed a Image Cropper in member (NodeId:{0} DocumentTypeAlias:{1})",
                            memberItem.Id,
                            memberItem.ContentType.Alias));
                }
            }
        }

        private static List<ImageCropData> GetCropsFromDataType(int dataTypeId, IDataTypeService dts)
        {        
            // ** Currently there is a Umbraco bug which means this doesn't workm using the ever faithful uQuery as a workaround **

            //var preValueJson =
            //    dts.GetPreValuesCollectionByDataTypeId(dataTypeId)
            //        .PreValuesAsDictionary.FirstOrDefault(x => x.Key.ToLower(CultureInfo.InvariantCulture) == "crops")
            //        .Value.Value;

            //var cropperPreValues = JsonConvert.DeserializeObject<List<ImageCropData>>(preValueJson);

            var uQueryPreValues = uQuery.GetPreValues(dataTypeId);
            var uQueryCrops = uQueryPreValues.FirstOrDefault(x => x.Alias == "crops");
            if (uQueryCrops == null)
            {
                return null;
            }

            var uQueryCropperPreValues = JsonConvert.DeserializeObject<List<ImageCropData>>(uQueryCrops.Value);
            if (uQueryCropperPreValues != null && uQueryCropperPreValues.Any())
            {
                return uQueryCropperPreValues;
            }

            return null;
        }

        private static ImageDimensions GetMediaFileDimensions(string mediaItemUrl)
        {
            if (string.IsNullOrEmpty(mediaItemUrl))
            {
                return null;
            }

            try
            {
                var mediaFileSystem = FileSystemProviderManager.Current.GetFileSystemProvider<MediaFileSystem>();
                var fullPath = mediaFileSystem.GetFullPath(mediaFileSystem.GetRelativePath(mediaItemUrl));
                var umbracoFile = new UmbracoMediaFile(fullPath);

                if (umbracoFile.Extension != "jpg" && umbracoFile.Extension != "png" && umbracoFile.Extension != "jpeg")
                {
                    return null;
                }

                var umbracoFileDimensions = umbracoFile.GetDimensions();
                return new ImageDimensions
                           {
                               Width = umbracoFileDimensions.Width,
                               Height = umbracoFileDimensions.Height
                           };
            }
            catch (Exception ex)
            {
                LogHelper.Error(typeof(CropHealer), string.Format("An exception occured while trying to determin image dimensions, media item url:{0}", mediaItemUrl), ex);
                return null;
            }
            
        }

        private class ImageDimensions
        {
            public int Width { get; set; }
            public int Height { get; set; }
        }

        internal static string ImageCropDataSetRepair(this ImageCropDataSet cropDataSet, string json, int sourceWidthPx, int sourceHeightPx, List<ImageCropData> imageCrops = null)
        {
            var healedSomething = false;

            if (cropDataSet.Src == null)
            {
                // convert from image path to cropper
                cropDataSet.Src = json;
                healedSomething = true;
            }

            if (cropDataSet.FocalPoint == null)
            {
                cropDataSet.FocalPoint = new ImageCropFocalPoint() { Left = 0.5m, Top = 0.5m };
                healedSomething = true;
            }

            if (imageCrops != null && imageCrops.Any())
            {
                var currentCrops = new List<ImageCropData>();

                if (cropDataSet.Crops != null)
                {
                    currentCrops = cropDataSet.Crops.ToList();

                    // delete crops that have been removed from the data type
                    foreach (var crop in cropDataSet.Crops)
                    {
                        if (!imageCrops.Select(x => x.Alias).Contains(crop.Alias))
                        {
                            currentCrops.Remove(crop);
                            healedSomething = true;
                        }
                    }
                }

                foreach (var cropDef in imageCrops)
                {
                    var cropDefInCurrent = currentCrops.FirstOrDefault(x => x.Alias == cropDef.Alias);

                    // check if width or height different
                    if (cropDefInCurrent != null && (cropDef.Width != cropDefInCurrent.Width || cropDef.Height != cropDefInCurrent.Height))
                    {
                        var index = currentCrops.IndexOf(cropDefInCurrent);

                        // see if we can calculate new coordinates
                        if (cropDefInCurrent.Coordinates != null && sourceHeightPx > 0 && sourceWidthPx > 0)
                        {
                            // crop height changed
                            if (cropDef.Height != cropDefInCurrent.Height)
                            {
                                // try to extend down first
                                var heightChange = cropDef.Height - cropDefInCurrent.Height;
                                var heightChangePercentage = Math.Round((decimal)heightChange / sourceHeightPx, 17);
                                
                                // check we are not going to hit the bottom else extend up
                                if ((cropDefInCurrent.Coordinates.Y2 - heightChangePercentage) >= 0)
                                {
                                    cropDefInCurrent.Coordinates.Y2 = cropDefInCurrent.Coordinates.Y2
                                                                      - heightChangePercentage;
                                }
                                else if ((cropDefInCurrent.Coordinates.Y1 - heightChangePercentage) >= 0)
                                {
                                    cropDefInCurrent.Coordinates.Y1 = cropDefInCurrent.Coordinates.Y1
                                                                      - heightChangePercentage;
                                }
                                else
                                {
                                    // if it's too big to grow, clear the coords
                                    cropDefInCurrent.Coordinates = null;
                                }
                            }

                            // crop width changed
                            if (cropDefInCurrent.Coordinates != null && cropDef.Width != cropDefInCurrent.Width)
                            {
                                // try to extend to the right first
                                var widthChange = cropDef.Width - cropDefInCurrent.Width;
                                var widthChangePercentage = Math.Round((decimal)widthChange / sourceWidthPx, 17);

                                // check we are not going to hit the right else extend to the left
                                if ((cropDefInCurrent.Coordinates.X2 - widthChangePercentage) >= 0)
                                {
                                    cropDefInCurrent.Coordinates.X2 = cropDefInCurrent.Coordinates.X2
                                                                      - widthChangePercentage;
                                }
                                else if ((cropDefInCurrent.Coordinates.X1 - widthChangePercentage) >= 0)
                                {
                                    cropDefInCurrent.Coordinates.X1 = cropDefInCurrent.Coordinates.X1
                                                                      - widthChangePercentage;
                                }
                                else
                                {
                                    // if it's too big to grow, clear the coords
                                    cropDefInCurrent.Coordinates = null;
                                }
                            }
                        }

                        cropDefInCurrent.Width = cropDef.Width;
                        cropDefInCurrent.Height = cropDef.Height;

                        currentCrops.RemoveAt(index);
                        currentCrops.Insert(index, cropDefInCurrent);
                        healedSomething = true;
                    }

                    if (!currentCrops.Select(x => x.Alias).Contains(cropDef.Alias))
                    {
                        currentCrops.Add(cropDef);
                        healedSomething = true;
                    }
                }

                if (cropDataSet.Crops == null)
                {
                    cropDataSet.Crops = new List<ImageCropData>();
                    healedSomething = true;
                }

                cropDataSet.Crops = currentCrops;
            }

            if (healedSomething)
            {
                return
                    JsonConvert.SerializeObject(
                        cropDataSet,
                        new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore })
                        .Replace(":0.0,", ":0,")
                        .Replace(":0.0}", ":0}");
            }

            return null;
        }

        private static bool DetectIsJson(this string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return false;
            }

            input = input.Trim();
            return (input.StartsWith("{") && input.EndsWith("}"))
                   || (input.StartsWith("[") && input.EndsWith("]"));
        }

        internal static ImageCropDataSet SerializeToCropDataSet(this string json)
        {
            var imageCrops = new ImageCropDataSet();
            if (json.DetectIsJson())
            {
                try
                {
                    imageCrops = JsonConvert.DeserializeObject<ImageCropDataSet>(json);
                }
                catch (Exception ex)
                {
                    LogHelper.Error(typeof(CropHealer), "Could not parse the json string: " + json, ex);
                }
            }

            return imageCrops;
        }
    }
}

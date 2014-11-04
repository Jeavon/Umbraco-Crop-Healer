namespace Our.Umbraco.CropHealer
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using Newtonsoft.Json;

    using global::Umbraco.Core;
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

            // Content
            foreach (var contentType in allContentTypes)
            {
                var contentTypeId = contentType.Id;
                var cropperContentProperties = contentType.PropertyTypes.Where(x => x.DataTypeDefinitionId == dataTypeId);
                if (cropperContentProperties.Any())
                {
                    var allContentUsingThis = cs.GetContentOfContentType(contentTypeId); 
                    HealContentItems(allContentUsingThis, cropperContentProperties, dataTypeId, dts, cs);
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
                    HealMediaItems(allMediaUsingThis, cropperContentProperties, dataTypeId, dts, ms);
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
                    HealMemberItems(allMembersUsingThis, cropperContentProperties, dataTypeId, dts, mems);
                }
            }
            
        }

        private static void HealContentItems(IEnumerable<IContent> content, IEnumerable<PropertyType> cropperContentProperties, int dataTypeId, IDataTypeService dts, IContentService cs)
        {
            foreach (var contentItem in content)
            {
                // There could be multiple uses of the same datatype on the content type 
                foreach (var cropperContentProperty in cropperContentProperties)
                {
                    var cropperPropertyValue = contentItem.GetValue<string>(cropperContentProperty.Alias);

                    var cropDataSet = cropperPropertyValue.SerializeToCropDataSet();

                    var attemptHeal = GetCropFromDataType(dataTypeId, cropDataSet, cropperPropertyValue, dts);
                    if (attemptHeal != null)
                    {
                        contentItem.SetValue(cropperContentProperty.Alias, attemptHeal);
                    }
                }

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

        private static void HealMediaItems(IEnumerable<IMedia> media, IEnumerable<PropertyType> cropperContentProperties, int dataTypeId, IDataTypeService dts, IMediaService ms)
        {
            foreach (var mediaItem in media)
            {
                // There could be multiple uses of the same datatype on the content type 
                foreach (var cropperContentProperty in cropperContentProperties)
                {
                    var cropperPropertyValue = mediaItem.GetValue<string>(cropperContentProperty.Alias);

                    var cropDataSet = cropperPropertyValue.SerializeToCropDataSet();

                    var attemptHeal = GetCropFromDataType(dataTypeId, cropDataSet, cropperPropertyValue, dts);
                    if (attemptHeal != null)
                    {
                        mediaItem.SetValue(cropperContentProperty.Alias, attemptHeal);
                    }
                }

                ms.Save(mediaItem);
                LogHelper.Info(
                    typeof(CropHealer),
                    string.Format(
                        "Healed a Image Cropper in media (NodeId:{0} DocumentTypeAlias:{1})",
                        mediaItem.Id,
                        mediaItem.ContentType.Alias));             
            }
        }

        private static void HealMemberItems(IEnumerable<IMember> member, IEnumerable<PropertyType> cropperContentProperties, int dataTypeId, IDataTypeService dts, IMemberService mems)
        {
            foreach (var memberItem in member)
            {
                // There could be multiple uses of the same datatype on the content type 
                foreach (var cropperContentProperty in cropperContentProperties)
                {
                    var cropperPropertyValue = memberItem.GetValue<string>(cropperContentProperty.Alias);

                    var cropDataSet = cropperPropertyValue.SerializeToCropDataSet();

                    var attemptHeal = GetCropFromDataType(dataTypeId, cropDataSet, cropperPropertyValue, dts);
                    if (attemptHeal != null)
                    {
                        memberItem.SetValue(cropperContentProperty.Alias, attemptHeal);
                    }
                }

                mems.Save(memberItem);
                LogHelper.Info(
                    typeof(CropHealer),
                    string.Format(
                        "Healed a Image Cropper in member (NodeId:{0} DocumentTypeAlias:{1})",
                        memberItem.Id,
                        memberItem.ContentType.Alias));
            }
        }

        private static string GetCropFromDataType(int dataTypeId, ImageCropDataSet cropDataSet, string json, IDataTypeService dts)
        {
            var preValueJson =
                dts.GetPreValuesCollectionByDataTypeId(dataTypeId)
                    .PreValuesAsDictionary.FirstOrDefault(x => x.Key.ToLower(CultureInfo.InvariantCulture) == "crops")
                    .Value.Value;

            var cropperPreValues = JsonConvert.DeserializeObject<List<ImageCropData>>(preValueJson);

            if (cropperPreValues != null && cropperPreValues.Any())
            {
                var attemptHeal = ImageCropDataSetRepair(cropDataSet, json, cropperPreValues);
                return attemptHeal;
            }     

            return null;
        }

        private static string ImageCropDataSetRepair(this ImageCropDataSet cropDataSet, string json, List<ImageCropData> imageCrops = null)
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
                return JsonConvert.SerializeObject(
                    cropDataSet,
                    new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            }

            return null;
        }

        private static bool DetectIsJson(this string input)
        {
            input = input.Trim();
            return (input.StartsWith("{") && input.EndsWith("}"))
                   || (input.StartsWith("[") && input.EndsWith("]"));
        }

        private static ImageCropDataSet SerializeToCropDataSet(this string json)
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

        private static ImageCropData GetCrop(this ImageCropDataSet dataset, string cropAlias)
        {
            if (dataset == null || dataset.Crops == null || !dataset.Crops.Any())
                return null;

            return dataset.Crops.GetCrop(cropAlias);
        }

        private static ImageCropData GetCrop(this IEnumerable<ImageCropData> dataset, string cropAlias)
        {
            if (dataset == null || !dataset.Any())
                return null;

            if (string.IsNullOrEmpty(cropAlias))
                return dataset.FirstOrDefault();

            return dataset.FirstOrDefault(x => x.Alias.ToLowerInvariant() == cropAlias.ToLowerInvariant());
        }
    }
}

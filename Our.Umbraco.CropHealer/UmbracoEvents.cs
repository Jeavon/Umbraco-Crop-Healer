using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Our.Umbraco.CropHealer
{
    using global::Umbraco.Core;
    using global::Umbraco.Core.Models;
    using global::Umbraco.Core.Services;

    public class UmbracoEvents : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            DataTypeService.Saved += DataTypeService_Saved;
        }

        void DataTypeService_Saved(IDataTypeService sender, global::Umbraco.Core.Events.SaveEventArgs<global::Umbraco.Core.Models.IDataTypeDefinition> e)
        {
            var cts = ApplicationContext.Current.Services.ContentTypeService;
            var cs = ApplicationContext.Current.Services.ContentService;

            foreach (var dataType in e.SavedEntities)
            {
                var dataTypeId = dataType.Id;
                var propertyEditor = dataType.PropertyEditorAlias;
                if (propertyEditor == Constants.PropertyEditors.ImageCropperAlias)
                {
                    // A Image Cropper Data Type has been saved, lets go to work!
                    var allContentTypes = cts.GetAllContentTypes();
                    var allMediaTypes = cts.GetAllMediaTypes();

                    foreach (var contentType in allContentTypes)
                    {
                        var contentTypeId = contentType.Id;
                        var cropperContentProperties = contentType.PropertyTypes.Where(x => x.DataTypeDefinitionId == dataTypeId);
                        if (cropperContentProperties.Any())
                        {
                            var allContentUsingThis = cs.GetContentOfContentType(contentTypeId);
                            foreach (var contentItem in allContentUsingThis)
                            {
                                // There could be multiple uses of the same datatype on the content type 
                                foreach (var cropperContentProperty in cropperContentProperties)
                                {
                                    var cropperPropertyValue = contentItem.GetValue<string>(cropperContentProperty.Alias);
                                    // now we need to heal it!
                                }
                            }
                        }

                    }
                }
            }
        }
    }
}

namespace Our.Umbraco.CropHealer
{
    using System;
    using System.Configuration;
    using System.Linq;

    using global::Umbraco.Core;
    using global::Umbraco.Core.Services;

    public class UmbracoEvents : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            DataTypeService.Saved += this.DataTypeService_Saved;
        }

        void DataTypeService_Saved(IDataTypeService sender, global::Umbraco.Core.Events.SaveEventArgs<global::Umbraco.Core.Models.IDataTypeDefinition> e)
        {
            var excludedDataTypes = Enumerable.Empty<Guid>();

            var cropHealerConfig = ConfigurationManager.GetSection("CropHealer") as CropHealerConfigSection;
            if (cropHealerConfig != null)
            {
                 excludedDataTypes = cropHealerConfig.Exclusions.DataTypes.Select(g => g.Key);
            }

            foreach (var dataType in e.SavedEntities)
            {
                var propertyEditor = dataType.PropertyEditorAlias;
                if (propertyEditor == Constants.PropertyEditors.ImageCropperAlias && !excludedDataTypes.Contains(dataType.Key))
                {
                    CropHealer.SeekAndHeal(dataType, sender);
                }
            }
        }
    }
}

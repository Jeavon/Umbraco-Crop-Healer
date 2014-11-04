﻿using System;
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
            DataTypeService.Saved += this.DataTypeService_Saved;
        }

        void DataTypeService_Saved(IDataTypeService sender, global::Umbraco.Core.Events.SaveEventArgs<global::Umbraco.Core.Models.IDataTypeDefinition> e)
        {
            foreach (var dataType in e.SavedEntities)
            {
                CropHealer.SeekandHeal(dataType, sender);
            }
        }
    }
}

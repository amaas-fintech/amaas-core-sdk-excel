using AMaaS.Core.Sdk.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMaaS.Core.Sdk.Excel.Models
{
    public class EnrichedModel<TModel, TData>  where TModel : AMaaSModel
    {
        #region Properties

        public TModel Model { get; set; }
        public TData Data { get; set; }

        #endregion

        #region Constructor
        public EnrichedModel(TModel model, TData data)
        {
            Model = model;
            Data  = data;
        }
        #endregion
    }
}

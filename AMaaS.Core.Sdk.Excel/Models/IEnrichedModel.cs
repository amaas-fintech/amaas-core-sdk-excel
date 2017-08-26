using AMaaS.Core.Sdk.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMaaS.Core.Sdk.Excel.Models
{
    public interface IEnrichedModel<TModel, TData> where TModel : AMaaSModel
    {
        TData Data { get; set; }
    }
}

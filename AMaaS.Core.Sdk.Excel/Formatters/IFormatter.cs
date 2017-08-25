using AMaaS.Core.Sdk.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMaaS.Core.Sdk.Excel.Formatters
{
    public interface IFormatter<T> where T : AMaaSModel
    {
        object[] FormatData(T data);
        object[] Header { get; }
    }
}

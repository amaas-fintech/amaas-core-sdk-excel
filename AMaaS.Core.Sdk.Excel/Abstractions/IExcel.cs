using ExcelDna.Integration;
using ExcelDna.Integration.CustomUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMaaS.Core.Sdk.Excel.Abstractions
{
    public interface IExcel
    {
        void Initialize();
        object Run(string udfName, object parameters, ExcelFunc func);
        ExcelReference Call(int excelFunction);
        IRibbonUI Ribbon { get; set; }
    }
}

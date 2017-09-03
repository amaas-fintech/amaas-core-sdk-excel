using AMaaS.Core.Sdk.Excel.Abstractions;
using Autofac;
using NetOffice.ExcelApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMaaS.Core.Sdk.Excel
{
    public static class AddinContext
    {
        public static IExcel Excel { get; set; }
        public static IContainer Container { get; set; }
        public static string UserAmid { get; set; }
        public static string Username { get; set; }
        public static List<int> AssetManagerIds { get; set; } = new List<int>();
    }
}

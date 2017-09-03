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
        public static Application Excel { get; set; }
        public static IContainer Container { get; set; }
    }
}

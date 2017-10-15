using AMaaS.Core.Sdk.Excel.Abstractions;
using Autofac;
using System.Collections.Generic;

namespace AMaaS.Core.Sdk.Excel
{
    public static class AddinContext
    {
        public static IExcel Excel { get; set; }
        public static IContainer Container { get; set; }
        public static string UserAmid { get; set; }
        public static string Username { get; set; }
        public static int AssumedAmid { get; set; }
    }
}

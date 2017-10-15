﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExcelDna.Integration;
using ExcelDna.IntelliSense;
using ExcelDna.Integration.CustomUI;

namespace AMaaS.Core.Sdk.Excel.Abstractions
{
    public class ExcelAbstraction : IExcel
    {
        public ExcelReference Call(int excelFunction)
        {
            return XlCall.Excel(XlCall.xlfCaller) as ExcelReference;
        }

        public object Run(string udfName, object parameters, ExcelFunc func)
        {
            return ExcelAsyncUtil.Run(udfName, parameters, func);
        }

        public void Initialize()
        {
            IntelliSenseServer.Register();
            ExcelIntegration.RegisterUnhandledExceptionHandler(ex =>
            {
                var message = (ex as Exception)?.Message;
                if(ex is AggregateException)
                    message = ((AggregateException)ex).GetBaseException().Message;

                return $"#ERROR: {message}";
            });
        }

        public IRibbonUI Ribbon { get; set; }
    }
}

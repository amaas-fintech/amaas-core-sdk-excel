using AMaaS.Core.Sdk.Excel.Formatters;
using AMaaS.Core.Sdk.Models;
using ExcelDna.Integration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMaaS.Core.Sdk.Excel.Helpers
{
    public static class ExcelTable
    {
        public static void Write<T>(IEnumerable<T> data, IFormatter<T> formatter, ExcelReference caller) where T: AMaaSModel
        {   
            Task.Factory.StartNew(() =>
            {
                ExcelAsyncUtil.QueueAsMacro(() =>
                {
                    var table    = new object[][] { formatter.Header }.Union(data.Select(t => formatter.FormatData(t)));
                    int rows     = table.Count();
                    int columns  = formatter.Header.Length;
                    var target   = new ExcelReference(caller.RowFirst + 1, caller.RowFirst + rows, caller.ColumnFirst, caller.ColumnFirst + columns - 1, caller.SheetId);
                    var output   = new object[rows + 1, columns];
                    output[0, 0] = XlCall.Excel(XlCall.xlfGetFormula, target);
                    int rowIndex = 0, columnIndex = 0;

                    foreach (var row in table)
                    {
                        columnIndex = 0;
                        foreach (var cell in row)
                        {
                            output[rowIndex, columnIndex++] = cell;
                        }
                        rowIndex++;
                    }
                    target.SetValue(output);
                });
            });
        }
    }
}

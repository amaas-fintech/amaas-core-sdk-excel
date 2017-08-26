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
        public static void WriteCell(ExcelReference target, string value, int row = 0, int column = 0)
        {
            var output = new object[row, column];
            ExcelAsyncUtil.QueueAsMacro(() => target.SetValue(output));
        }

        public static void Write<T>(IEnumerable<T> data, IFormatter<T> formatter, ExcelReference caller) 
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

        public static object[,] Format<T>(IEnumerable<T> data, IFormatter<T> formatter, ExcelReference caller)
        {
            var table    = new object[][] { formatter.Header }.Union(data.Select(t => formatter.FormatData(t)));
            int rows     = table.Count();
            int columns  = formatter.Header.Length;
            var output   = new object[rows, columns];
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
            ArrayResizer.Resize(output, caller);
            return output;
        }
    }
}

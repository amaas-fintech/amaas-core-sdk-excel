// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using ExcelDna.Integration;

namespace Amaas.Core.Sdk.Excel.DataAccess
{
    internal class ArrayResizer
    {
        static Queue<Tuple<ExcelReference, string>> ResizeJobs = new Queue<Tuple<ExcelReference, string>>();

        // This function will run in the UDF context.
        public static object Resize(object[,] array, string rangeName)
        {
            string name;
            if (rangeName is ExcelMissing)
            {
                name = string.Empty;
            }
            else
            {
                name = (string)rangeName;
            }
            ExcelReference caller = XlCall.Excel(XlCall.xlfCaller) as ExcelReference;
            return Resize(array, caller, name);
        }

        internal static object ResizeObservable(object[,] array, ExcelReference caller, string name)
        {
            object callerAfter = XlCall.Excel(XlCall.xlfCaller);
            if (callerAfter == null)
            {
                // This is the good RTD array call
                return Resize(array, caller, name);
            }
            // Some spurious RTD call - just return.
            return array;
        }

        public static object Resize(object[,] array, ExcelReference caller, string name)
        {
            if (caller == null)
            {
                //Debug.Print("Resize - Abandoning - No Caller");
                return array;
            }

            int rows = array.GetLength(0);
            int columns = array.GetLength(1);

            if ((caller.RowLast - caller.RowFirst + 1 != rows) ||
                (caller.ColumnLast - caller.ColumnFirst + 1 != columns))
            {
                // Size problem: enqueue job, call async update and return #N/A
                EnqueueResize(caller, rows, columns, name);
                ExcelAsyncUtil.QueueAsMacro(DoResizing);
            }

            // Size is already OK - just return result
            return array;
        }

        static void EnqueueResize(ExcelReference caller, int rows, int columns, string name)
        {
            ExcelReference target = new ExcelReference(caller.RowFirst, caller.RowFirst + rows - 1, caller.ColumnFirst, caller.ColumnFirst + columns - 1, caller.SheetId);
            ResizeJobs.Enqueue(Tuple.Create(target, name));
        }

        static void DoResizing()
        {
            while (ResizeJobs.Count > 0)
            {
                var next = ResizeJobs.Dequeue();
                DoResize(next.Item1, next.Item2);
            }
        }

        static void DoResize(ExcelReference target, string name)
        {
            object oldEcho = XlCall.Excel(XlCall.xlfGetWorkspace, 40);
            object oldCalculationMode = XlCall.Excel(XlCall.xlfGetDocument, 14);
            try
            {
                // Get the current state for reset later
                XlCall.Excel(XlCall.xlcEcho, false);
                XlCall.Excel(XlCall.xlcOptionsCalculation, 3);

                // Get the formula in the first cell of the target
                string formula = (string)XlCall.Excel(XlCall.xlfGetCell, 41, target);
                ExcelReference firstCell = new ExcelReference(target.RowFirst, target.RowFirst, target.ColumnFirst, target.ColumnFirst, target.SheetId);

                bool isFormulaArray = (bool)XlCall.Excel(XlCall.xlfGetCell, 49, target);
                if (isFormulaArray)
                {
                    object oldSelectionOnActiveSheet = XlCall.Excel(XlCall.xlfSelection);
                    object oldActiveCell = XlCall.Excel(XlCall.xlfActiveCell);

                    // Remember old selection and select the first cell of the target
                    string firstCellSheet = (string)XlCall.Excel(XlCall.xlSheetNm, firstCell);
                    XlCall.Excel(XlCall.xlcWorkbookSelect, new object[] { firstCellSheet });
                    object oldSelectionOnArraySheet = XlCall.Excel(XlCall.xlfSelection);
                    XlCall.Excel(XlCall.xlcFormulaGoto, firstCell);

                    // Extend the selection to the whole array and clear
                    XlCall.Excel(XlCall.xlcSelectSpecial, 6);
                    ExcelReference oldArray = (ExcelReference)XlCall.Excel(XlCall.xlfSelection);

                    oldArray.SetValue(ExcelEmpty.Value);
                    XlCall.Excel(XlCall.xlcSelect, oldSelectionOnArraySheet);
                    XlCall.Excel(XlCall.xlcFormulaGoto, oldSelectionOnActiveSheet);
                }
                // Get the formula and convert to R1C1 mode
                bool isR1C1Mode = (bool)XlCall.Excel(XlCall.xlfGetWorkspace, 4);
                string formulaR1C1 = formula;
                if (!isR1C1Mode)
                {
                    //Catch any exception here and continue
                    try
                    {
                        // Set the formula into the whole target
                        formulaR1C1 = (string)XlCall.Excel(XlCall.xlfFormulaConvert, formula, true, false, ExcelMissing.Value, firstCell);
                    }
                    catch (Exception e)
                    { }
                }
                // Must be R1C1-style references
                object ignoredResult;
                XlCall.XlReturn retval = XlCall.TryExcel(XlCall.xlcFormulaArray, out ignoredResult, formulaR1C1, target);

                // TODO: Dummy action to clear the undo stack

                if (retval != XlCall.XlReturn.XlReturnSuccess)
                {
                    // TODO: Consider what to do now!?
                    // Might have failed due to array in the way.
                    firstCell.SetValue("'" + formula);
                }

                // rename range if name specified
                if (name != string.Empty)
                {
                    var address = XlCall.Excel(XlCall.xlfReftext, target, false);
                    var success = (bool)XlCall.Excel(XlCall.xlcDefineName, name, String.Format("={0}", address));
                    if (!success) { throw new ArgumentException("could not set range name", "value"); }
                }
            }
            finally
            {
                XlCall.Excel(XlCall.xlcEcho, oldEcho);
                XlCall.Excel(XlCall.xlcOptionsCalculation, oldCalculationMode);
            }
        }
    }
}


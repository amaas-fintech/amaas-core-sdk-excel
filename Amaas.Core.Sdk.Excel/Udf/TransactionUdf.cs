using ExcelDna.Integration;
using Amaas.Core.Sdk.Excel.DataAccess;
using System.Threading;
using System.Threading.Tasks;
using ExcelDna.IntelliSense;


namespace Amaas.Core.Sdk.Excel.Udf
{
    public class TransactionUdf : IExcelAddIn
    {
        public void AutoOpen()
        {
            IntelliSenseServer.Register();
            ExcelIntegration.RegisterUnhandledExceptionHandler(
                ex => "!!! EXCEPTION: " + ex.ToString());
        }
        public void AutoClose()
        {
        }     

        [ExcelFunction(Name = "GetPositionByBookID", IsMacroType = true, Description = "Get positions according to Book ID")]
        public static void GetPositionAsync([ExcelArgument(AllowReference = true, Name = "Asset manager ID")] string AMID, [ExcelArgument(AllowReference = true, Name = "Book ID")]string bookID, [ExcelArgument(AllowReference = true, Name = "Start Date of Position")]string startDate, [ExcelArgument(AllowReference = true, Name = "Number of trnasactions to display on a page")]string pageSize, [ExcelArgument(AllowReference = true, Name = "The page.no")]string pageNum, [ExcelArgument(AllowReference = true, Name = "fields search for position")]string fields)
        {
            ExcelReference caller = XlCall.Excel(XlCall.xlfCaller) as ExcelReference;
            object[,] getArray= { };
            Task.Factory.StartNew(() => Thread.Sleep(0))
            .ContinueWith(t =>
                ExcelAsyncUtil.QueueAsMacro(() =>
                {
                    //Application.UseWaitCursor = true;
                    TransactionDataAccess TDA = new TransactionDataAccess();
                    getArray = TDA.Position(AMID, bookID, startDate, pageSize, pageNum, fields);
                    // Create a Range of the correct size:
                    int rows = getArray.GetLength(0);
                    int columns = getArray.GetLength(1);
                    ExcelReference target = new ExcelReference(caller.RowFirst, caller.RowFirst + rows, caller.ColumnFirst, caller.ColumnFirst + columns - 1, caller.SheetId);//specify the cell to populate data on excel
                    //Assign the Array to the Range in one shot:
                    object[,] resizeArray = new object[rows + 1,columns];
                    resizeArray[0,0]= XlCall.Excel(XlCall.xlfGetFormula, target);
                    for (int i=1; i<rows+1; i++)
                    {
                        for (int j = 0; j < columns; j++) resizeArray[i, j] = getArray[i-1, j];
                    }
                    target.SetValue(resizeArray);
                }));
        }

        [ExcelFunction(Name = "GetTransactionByBookID", IsMacroType = true, Description = "Get transactions according to Book ID")]
        public static void GetTransactionByBookIDAsync([ExcelArgument(AllowReference = true, Name = "Asset manager ID")] string AMID, [ExcelArgument(AllowReference = true, Name = "Book ID")]string bookID, [ExcelArgument(AllowReference = true, Name = "Start date of the transaction")]string startDate, [ExcelArgument(AllowReference = true, Name = "End date of the transaction")]string endDate, [ExcelArgument(AllowReference = true, Name = "Number of trnasactions to display on a page")]string pageSize, [ExcelArgument(AllowReference = true, Name = "The page.no")]string pageNum, [ExcelArgument(AllowReference = true, Name = "Field search for the transaction")]string fields, [ExcelArgument(AllowReference = true, Name = "Field search for the asset")]string assetOptionalFields)
        {
            ExcelReference caller = XlCall.Excel(XlCall.xlfCaller) as ExcelReference;
            object[,] getArray = { };
            Task.Factory.StartNew(() => Thread.Sleep(0)) //Thread.Sleep(5000)
            .ContinueWith(t =>
                ExcelAsyncUtil.QueueAsMacro(() =>
                {
                    TransactionDataAccess TDA = new TransactionDataAccess();
                    getArray = TDA.Transaction(AMID, bookID, startDate, endDate, pageSize, pageNum, fields, assetOptionalFields, "TransactionByBookID");
                    // Create a Range of the correct size:
                    int rows = getArray.GetLength(0);
                    int columns = getArray.GetLength(1);
                    ExcelReference target = new ExcelReference(caller.RowFirst, caller.RowFirst + rows, caller.ColumnFirst, caller.ColumnFirst + columns - 1, caller.SheetId);
                    //Assign the Array to the Range in one shot:
                    if (getArray.GetLength(0) == 1 && getArray.GetLength(1) == 2) target.SetValue("The data does not exist.");
                    else {
                        object[,] resizeArray = new object[rows + 1, columns];
                        resizeArray[0, 0] = XlCall.Excel(XlCall.xlfGetFormula, target);
                        for (int i = 1; i < rows + 1; i++)
                        {
                            for (int j = 0; j < columns; j++) resizeArray[i, j] = getArray[i - 1, j];
                        }
                        target.SetValue(resizeArray);
                    } 
                }));
        }

        [ExcelFunction(Name = "GetFilteredTransactionByBookID", IsMacroType = true, Description = "Get filtered transactions according to Book ID")]
        public static void GetFilteredTransactionByBookIDAsync([ExcelArgument(AllowReference = true, Name = "Asset manager ID")] string AMID, [ExcelArgument(AllowReference = true, Name = "Book ID")]string bookID, [ExcelArgument(AllowReference = true, Name = "Start date of the transaction")]string startDate, [ExcelArgument(AllowReference = true, Name = "End date of the transaction")]string endDate, [ExcelArgument(AllowReference = true, Name = "Number of trnasactions to display on a page")]string pageSize, [ExcelArgument(AllowReference = true, Name = "The page.no")]string pageNum, [ExcelArgument(AllowReference = true, Name = "Field search for the transaction")]string fields, [ExcelArgument(AllowReference = true, Name = "Field search for the asset")]string assetOptionalFields)
        {
            ExcelReference caller = XlCall.Excel(XlCall.xlfCaller) as ExcelReference;
            object[,] getArray = { };
            Task.Factory.StartNew(() => Thread.Sleep(0)) //Thread.Sleep(5000)
            .ContinueWith(t =>
                ExcelAsyncUtil.QueueAsMacro(() =>
                {
                    TransactionDataAccess TDA = new TransactionDataAccess();
                    getArray = TDA.TransactionFiltered (AMID, bookID, startDate, endDate, pageSize, pageNum, fields, assetOptionalFields, "TransactionByBookID");
                    // Create a Range of the correct size:
                    int rows = getArray.GetLength(0);
                    int columns = getArray.GetLength(1);
                    ExcelReference target = new ExcelReference(caller.RowFirst, caller.RowFirst + rows, caller.ColumnFirst, caller.ColumnFirst + columns - 1, caller.SheetId);
                    //Assign the Array to the Range in one shot:
                    if (getArray.GetLength(0) == 1 && getArray.GetLength(1) == 2) target.SetValue("The data does not exist.");
                    else
                    {
                        object[,] resizeArray = new object[rows + 1, columns];
                        resizeArray[0, 0] = XlCall.Excel(XlCall.xlfGetFormula, target);
                        for (int i = 1; i < rows + 1; i++)
                        {
                            for (int j = 0; j < columns; j++) resizeArray[i, j] = getArray[i - 1, j];
                        }
                        target.SetValue(resizeArray);
                    }
                }));
        }

        [ExcelFunction(Name = "GetTransactionByTransactionID", IsMacroType = true, Description = "Get transactions according to transaction ID")]
        public static void GetTransactionByTransactionIDAsync([ExcelArgument(AllowReference = true, Name = "Asset manager ID")] string AMID, [ExcelArgument(AllowReference = true, Name = "Transaction ID")]string transactionID, [ExcelArgument(AllowReference = true, Name = "Start date of the transaction")]string startDate, [ExcelArgument(AllowReference = true, Name = "End date of the transaction")]string endDate, [ExcelArgument(AllowReference = true, Name = "Number of trnasactions to display on a page")]string pageSize, [ExcelArgument(AllowReference = true, Name = "The page.no")]string pageNum, [ExcelArgument(AllowReference = true, Name = "Field search for the transaction")]string fields, [ExcelArgument(AllowReference = true, Name = "Field search for the asset")]string assetOptionalFields)
        {
            ExcelReference caller = XlCall.Excel(XlCall.xlfCaller) as ExcelReference;
            object[,] getArray = { };
            Task.Factory.StartNew(() => Thread.Sleep(0))
            .ContinueWith(t =>
                ExcelAsyncUtil.QueueAsMacro(() =>
                {
                    TransactionDataAccess TDA = new TransactionDataAccess();
                    getArray = TDA.Transaction(AMID, transactionID, startDate, endDate, pageSize, pageNum, fields, assetOptionalFields, "TransactionByTransactionID");
                    // Create a Range of the correct size:
                    int rows = getArray.GetLength(0);
                    int columns = getArray.GetLength(1);
                    ExcelReference target = new ExcelReference(caller.RowFirst, caller.RowFirst + rows, caller.ColumnFirst, caller.ColumnFirst + columns - 1, caller.SheetId);
                    //Assign the Array to the Range in one shot:
                    object[,] resizeArray = new object[rows + 1, columns];
                    resizeArray[0, 0] = XlCall.Excel(XlCall.xlfGetFormula, target);
                    for (int i = 1; i < rows + 1; i++)
                    {
                        for (int j = 0; j < columns; j++) resizeArray[i, j] = getArray[i - 1, j];
                    }
                    target.SetValue(resizeArray);
                }));
        }

        public object GetPositions(string AMID, string bookID, string startDate, string pageSize, string pageNum, string fields)
        {
            TransactionDataAccess TDA = new TransactionDataAccess();
            object[,] getArray = TDA.Position(AMID, bookID, startDate, pageSize, pageNum, fields);
            return ArrayResizer.Resize(getArray, "position");
        }

        public object GetTransactionByTransactionID(string AMID, string transactionID, string startDate, string endDate, string pageSize, string pageNum, string fields, string assetOptionalFields)
        {
            TransactionDataAccess TDA = new TransactionDataAccess();
            object[,] getArray = TDA.Transaction(AMID, transactionID, startDate, endDate, pageSize, pageNum, fields, assetOptionalFields, "TransactionByTransactionID");
            return ArrayResizer.Resize(getArray, "transactions");
        }

        public object GetTransactionByBookID(string AMID, string bookID, string startDate, string endDate, string pageSize, string pageNum, string fields, string assetOptionalFields)
        {
            TransactionDataAccess TDA = new TransactionDataAccess();
            object[,] getArray = TDA.Transaction(AMID, bookID, startDate, endDate, pageSize, pageNum, fields, assetOptionalFields, "TransactionByBookID");
            return ArrayResizer.Resize(getArray, "transactions");
        }
       
    }
}

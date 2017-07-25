using ExcelDna.Integration;
using Amaas.Core.Sdk.Excel.DataAccess;
using System.Threading;
using System.Threading.Tasks;


namespace Amaas.Core.Sdk.Excel.Udf
{
    public class TransactionUdf
    {
        public void AutoOpen()
        {
            ExcelIntegration.RegisterUnhandledExceptionHandler(
                ex => "!!! EXCEPTION: " + ex.ToString());
        }
        public void AutoClose()
        {
        }     

        [ExcelFunction(Name = "GetPosition", IsMacroType = true)]
        public static void GetPositionAsync([ExcelArgument(AllowReference = true, Name = "AMID")] string AMID, [ExcelArgument(AllowReference = true, Name = "book ID")]string bookID, [ExcelArgument(AllowReference = true, Name = "startDate")]string startDate, [ExcelArgument(AllowReference = true, Name = "pageSize")]string pageSize, [ExcelArgument(AllowReference = true, Name = "pageNum")]string pageNum)
        {
            ExcelReference caller = XlCall.Excel(XlCall.xlfCaller) as ExcelReference;
            object[,] getArray= { };
            Task.Factory.StartNew(() =>Thread.Sleep(0))
            .ContinueWith(t =>
                ExcelAsyncUtil.QueueAsMacro(() =>
                {  
                    TransactionDataAccess TDA = new TransactionDataAccess();
                    getArray = TDA.Position(AMID, bookID, startDate, pageSize, pageNum);
                    // Create a Range of the correct size:
                    int rows = getArray.GetLength(0);
                    int columns = getArray.GetLength(1);
                    ExcelReference target = new ExcelReference(caller.RowFirst, caller.RowFirst + rows - 1, caller.ColumnFirst, caller.ColumnFirst + columns - 1, caller.SheetId);
                    //Assign the Array to the Range in one shot:
                    target.SetValue(getArray);
                }));
        }

        [ExcelFunction(Name = "GetTransactionByBookID", IsMacroType = true)]
        public static void GetTransactionByBookIDAsync([ExcelArgument(AllowReference = true, Name = "AMID")] string AMID, [ExcelArgument(AllowReference = true, Name = "book ID")]string bookID, [ExcelArgument(AllowReference = true, Name = "startDate")]string startDate, [ExcelArgument(AllowReference = true, Name = "endDate")]string endDate, [ExcelArgument(AllowReference = true, Name = "pageSize")]string pageSize, [ExcelArgument(AllowReference = true, Name = "pageNum")]string pageNum)
        {
            ExcelReference caller = XlCall.Excel(XlCall.xlfCaller) as ExcelReference;
            object[,] getArray = { };
            Task.Factory.StartNew(() => Thread.Sleep(5000))
            .ContinueWith(t =>
                ExcelAsyncUtil.QueueAsMacro(() =>
                {
                    TransactionDataAccess TDA = new TransactionDataAccess();
                    getArray = TDA.Transaction(AMID, bookID, startDate, endDate, pageSize, pageNum, "TransactionByBookID");
                    // Create a Range of the correct size:
                    int rows = getArray.GetLength(0);
                    int columns = getArray.GetLength(1);
                    ExcelReference target = new ExcelReference(caller.RowFirst, caller.RowFirst + rows - 1, caller.ColumnFirst, caller.ColumnFirst + columns - 1, caller.SheetId);
                    //Assign the Array to the Range in one shot:
                    target.SetValue(getArray);
                }));
        }

        [ExcelFunction(Name = "GetTransactionByTransactionID", IsMacroType = true)]
        public static void GetTransactionByTransactionIDAsync([ExcelArgument(AllowReference = true, Name = "AMID")] string AMID, [ExcelArgument(AllowReference = true, Name = "book ID")]string bookID, [ExcelArgument(AllowReference = true, Name = "startDate")]string startDate, [ExcelArgument(AllowReference = true, Name = "endDate")]string endDate, [ExcelArgument(AllowReference = true, Name = "pageSize")]string pageSize, [ExcelArgument(AllowReference = true, Name = "pageNum")]string pageNum)
        {
            ExcelReference caller = XlCall.Excel(XlCall.xlfCaller) as ExcelReference;
            object[,] getArray = { };
            Task.Factory.StartNew(() => Thread.Sleep(5000))
            .ContinueWith(t =>
                ExcelAsyncUtil.QueueAsMacro(() =>
                {
                    TransactionDataAccess TDA = new TransactionDataAccess();
                    getArray = TDA.Transaction(AMID, bookID, startDate, endDate, pageSize, pageNum, "TransactionByTransactionID");
                    // Create a Range of the correct size:
                    int rows = getArray.GetLength(0);
                    int columns = getArray.GetLength(1);
                    ExcelReference target = new ExcelReference(caller.RowFirst, caller.RowFirst + rows - 1, caller.ColumnFirst, caller.ColumnFirst + columns - 1, caller.SheetId);
                    //Assign the Array to the Range in one shot:
                    target.SetValue(getArray);
                }));
        }

        public object GetPositions(string AMID, string bookID, string startDate, string pageSize, string pageNum)
        {
            TransactionDataAccess TDA = new TransactionDataAccess();
            object[,] getArray = TDA.Position(AMID, bookID, startDate, pageSize, pageNum);
            return ArrayResizer.Resize(getArray, "position");
        }

        public object GetTransactionByTransactionID(string AMID, string transactionID, string startDate, string endDate, string pageSize, string pageNum)
        {
            TransactionDataAccess TDA = new TransactionDataAccess();
            object[,] getArray = TDA.Transaction(AMID, transactionID, startDate, endDate, pageSize, pageNum, "TransactionByTransactionID");
            return ArrayResizer.Resize(getArray, "transactions");
        }

        public object GetTransactionByBookID(string AMID, string bookID, string startDate, string endDate, string pageSize, string pageNum)
        {
            TransactionDataAccess TDA = new TransactionDataAccess();
            object[,] getArray = TDA.Transaction(AMID, bookID, startDate, endDate, pageSize, pageNum, "TransactionByBookID");
            return ArrayResizer.Resize(getArray, "transactions");
        }
    }
}

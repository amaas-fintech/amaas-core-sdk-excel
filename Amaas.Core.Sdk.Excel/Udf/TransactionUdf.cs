using ExcelDna.Integration;
using Amaas.Core.Sdk.Excel.DataAccess;


namespace Amaas.Core.Sdk.Excel.Udf
{
    public class TransactionUdf
    {
        [ExcelFunction(Name = "GetPosition", Description = "Get positions")]
        [ExcelCommand(ShortCut = "GP")]
        public static object GetPositionUdf([ExcelArgument(AllowReference = true)] string AMID, string bookID, string startDate, string pageSize, string pageNum) =>
           new TransactionUdf().GetPositions(AMID, bookID, startDate, pageSize, pageNum);

        [ExcelFunction(Name = "GetTransactionByBookID", Description = "Get transactions according to Book ID")]
        [ExcelCommand(ShortCut = "GTB")]
        public static object GetTransactionByBookIDUdf(
            [ExcelArgument(AllowReference = true, Name = "AMID")] string AMID, [ExcelArgument(Name = "bookID")]string bookID, [ExcelArgument(Name = "startDate")]string startDate, [ExcelArgument(Name = "endDate")]string endDate, [ExcelArgument(Name = "pageSize")]string pageSize, [ExcelArgument(Name = "pageNum")]string pageNum) =>
            new TransactionUdf().GetTransactionByBookID(AMID, bookID, startDate, endDate, pageSize, pageNum);

        [ExcelFunction(Name = "GetTransactionByTransactionID", Description = "Get transactions according to transaction ID")]
        [ExcelCommand(ShortCut = "GTT")]
        public static object GetTransactionByTransactionIDUdf(
            [ExcelArgument(AllowReference = true)] string AMID, string transactionID, string startDate, string endDate, string pageSize, string pageNum) =>
            new TransactionUdf().GetTransactionByTransactionID(AMID, transactionID, startDate, endDate, pageSize, pageNum);

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

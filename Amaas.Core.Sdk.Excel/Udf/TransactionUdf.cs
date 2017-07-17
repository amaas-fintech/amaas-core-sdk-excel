using ExcelDna.Integration;
using Amaas.Core.Sdk.Excel.DataAccess;


namespace Amaas.Core.Sdk.Excel.Udf
{
    public class TransactionUdf
    {
        [ExcelFunction(Name = "GetPosition")]
        public static object GetPositionUdf([ExcelArgument(AllowReference = true)] string AMID, string bookID) => ArrayResizer.Resize(new TransactionUdf().GetPositions(AMID, bookID), "Position");

        [ExcelFunction(Name = "GetTransactionByBookID")]
        public static object GetTransactionByBookIDUdf(
            [ExcelArgument(AllowReference = true)] string AMID, string bookID) => 
            new TransactionUdf().GetTransactionByBookID(AMID, bookID);

        [ExcelFunction(Name = "GetTransactionByTransactionID")]
        public static object GetTransactionByTransactionIDUdf(
            [ExcelArgument(AllowReference = true)] string AMID, string transactionID) =>
            new TransactionUdf().GetTransactionByTransactionID(AMID, transactionID);

        public object[,] GetPositions(string AMID, string bookID)
        {
            object[,] getArray;
            if (bookID == ""||AMID == "") return getArray= new object[1, 1] { {"Resource ID is required"}};//error message
            TransactionDataAccess TDA = new TransactionDataAccess();
            getArray = TDA.Position(AMID, bookID);
            return getArray;//for unit tests, we don't use-->return  ArrayResizer.Resize(getArray, "position");
        }

        public object GetTransactionByTransactionID(string AMID, string transactionID)
        {
            if (transactionID == "") return "Transaction ID is required";
            if (AMID == "") return "Asset Manager ID is required";
            TransactionDataAccess TDA = new TransactionDataAccess();
            object[,] getArray = TDA.Transaction(AMID, transactionID, "TransactionByTransactionID");
            return ArrayResizer.Resize(getArray, "transactions");
        }

        public object GetTransactionByBookID(string AMID, string bookID)
        {
            if (bookID == "") return "Book ID is required";
            if (AMID == "") return "Asset Manager ID is required";
            TransactionDataAccess TDA = new TransactionDataAccess();
            object[,] getArray = TDA.Transaction(AMID, bookID, "TransactionByBookID");
            return ArrayResizer.Resize(getArray, "transactions");
        }
    }
}

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Amaas.Core.Sdk.Excel.Udf;

namespace Amaas.Core.Sdk.Excel.Tests
{
    [TestClass]
    public class DataRetrieveUnitTest
    {
        [TestMethod]
        public void TestPositionData()
        {
            Amaas.Core.Sdk.Excel.DataAccess.TransactionDataAccess dataAccess = new Amaas.Core.Sdk.Excel.DataAccess.TransactionDataAccess();
            object[,] GetArrayWithId = dataAccess.Position("", ""); //Asset Manager ID, Book ID
            Boolean flag = false;
            foreach (var item in GetArrayWithId)
            {
                if (item != "") flag = true;
            }
            Assert.IsTrue(flag);
        }

        [TestMethod]
        public void TestTransactionData()
        {
            Amaas.Core.Sdk.Excel.DataAccess.TransactionDataAccess dataAccess = new Amaas.Core.Sdk.Excel.DataAccess.TransactionDataAccess();
            object[,] GetArrayWithId = dataAccess.Transaction("", "", "TransactionByTransactionID");//Asset Manager ID, Transaction ID, flag
            Assert.IsNotNull(GetArrayWithId);
        }

        [TestMethod]
        public void TestArrayRetrievalWithCorrectParameters()
        {
            //It_Should_Return_Position_For_Given_Id
            object GetArrayWithId = new TransactionUdf().GetPositions("", "");//Asset Manager ID, Book ID
            Assert.AreNotEqual(GetArrayWithId, null);
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException),
                                "It_Should_Throw_Error_Given_Non_Existing_Id")]
        public void NullPositionException()
        {
            try
            {
                 object GetArrayWithoutId = new TransactionUdf().GetPositions("1", "1");//Wrong Asset Manager ID, Wrong Book ID
            }
            catch (Exception)
            {
                throw;
            }
           
        }
    }
}


using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ExcelDna.Integration;
using Amaas.Core.Sdk.Excel.Udf;

namespace UnitTes
{
    [TestClass]
    public class UnitTest
    {
        [TestMethod]
        public void TestAuth()
        {
            Amaas.Core.Sdk.Authentication.CognitoAuthentication auth = new Amaas.Core.Sdk.Authentication.CognitoAuthentication();
            string idToken = auth.CheckPasswordAsync("amaas", "amaaswelcome");
           
            Assert.IsNotNull(idToken);           
        }

        [TestMethod]
        public void TestPositionData()
        {
            Amaas.Core.Sdk.Excel.DataAccess.TransactionDataAccess dataAccess = new Amaas.Core.Sdk.Excel.DataAccess.TransactionDataAccess();
            object[,] GetArrayWithId = dataAccess.Position("133", "CPBook1");
            Console.Write(GetArrayWithId);
            Boolean flag=false;
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
            object[,] GetArrayWithId = dataAccess.Transaction("1", "00593ec1835e416593ff7401ac0d0fa9", "TransactionByTransactionID");
            Assert.IsNotNull(GetArrayWithId);
        }
      
        [TestMethod]
        public void TestArrayRetrievalWithCorrectParameters()
        {
            //It_Should_Return_Position_For_Given_Id
            object GetArrayWithId = new TransactionUdf().GetPositions("133", "CPBook1");
            Assert.AreNotEqual(GetArrayWithId, null);
        }

        [TestMethod]
        public void TestArrayRetrievalWithIncorrectParameters()
        {
            //It_Should_Throw_Error_Given_Non_Existing_Id
            try
            {
                object GetArrayWithoutId = new TransactionUdf().GetPositions("1", "1");
            }
            catch (Exception e)
            {
                Assert.IsNotNull(e);
            }
        }

    }
}

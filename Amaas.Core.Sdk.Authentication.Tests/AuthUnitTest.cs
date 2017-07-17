using System;
using System.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Amaas.Core.Sdk.Authentication.Tests
{
    [TestClass]
    public class AuthUnitTest
    {
        [TestMethod]
        public void TestAuth()
        {
            Amaas.Core.Sdk.Authentication.CognitoAuthentication auth = new Amaas.Core.Sdk.Authentication.CognitoAuthentication();
            string idToken = auth.CheckPasswordAsync(ConfigurationManager.AppSettings["USERNAME"], ConfigurationManager.AppSettings["PASSWORD"]);

            Assert.IsNotNull(idToken);
        }
    }
}

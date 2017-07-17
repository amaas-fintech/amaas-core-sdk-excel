using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Configuration;

namespace Amaas.Core.Sdk.Excel.DataAccess
{
    class DataConnection
    {
        private static string Auth()
        {
            var auth = new Amaas.Core.Sdk.Authentication.CognitoAuthentication();
            string idToken = auth.CheckPasswordAsync(ConfigurationManager.AppSettings["USERNAME"], ConfigurationManager.AppSettings["PASSWORD"]);
            return idToken;
        }

        public static async Task<String> RetrieveData(string AMID, string resourceID, string flag)
        {
            string idToken = Auth();
            string responseResult = "";
            int statusCode = 0;

            using (var queryclient = new HttpClient())
            {
                while (statusCode != 200)
                {
                    //setup client
                    queryclient.BaseAddress = new Uri(ConfigurationManager.AppSettings["BASEURL"]);
                    queryclient.DefaultRequestHeaders.Accept.Clear();
                    queryclient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    var _CredentialBase64Query = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}", idToken.ToString())));
                    queryclient.DefaultRequestHeaders.Add("Authorization", idToken.ToString());

                    //make request
                    string url = "";
                    if (flag == "TransactionByTransactionID")
                    {
                        url = $"{ConfigurationManager.AppSettings["TRANSACTION"]}{AMID}/{resourceID}";
                    }
                    else if(flag== "TransactionByBookID")
                    {
                        url = ConfigurationManager.AppSettings["TRANSACTION"] + AMID + "?" + "asset_book_ids=" + resourceID;
                    }
                    else if (flag == "Position")
                    {
                        url = ConfigurationManager.AppSettings["POSITION"] + AMID + "?" + "book_ids=" + resourceID;
                    }
                    else
                    {
                        return "Invaid Parameters";
                    }
                    var response = await queryclient.GetAsync(url);
                    string testResponse = JsonConvert.SerializeObject(response);
                    responseResult = await queryclient.GetStringAsync(url);
                    statusCode = (int)response.StatusCode;
                }
            }
            return responseResult;
        }
    }
}


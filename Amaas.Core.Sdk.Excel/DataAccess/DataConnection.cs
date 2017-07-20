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

         public static async Task<String> RetrieveData(string AMID, string resourceID, string startDate, string endDate, string pageSize, string pageNum, string flag)
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
                        url = $"{ConfigurationManager.AppSettings["TRANSACTION"]}{AMID}/{resourceID}" + resourceID + "?" + "transaction_date_start=" + startDate + "&transaction_date_end=" + endDate + "&page_size=" + pageSize + "&page_no=" + pageNum;
                        url = RemoveQueryStringByKey(url);
                    }
                    else if (flag == "TransactionByBookID")
                    {
                        url = ConfigurationManager.AppSettings["TRANSACTION"] + AMID + "?" + "asset_book_ids=" + resourceID + "&transaction_date_start=" + startDate + "&transaction_date_end=" + endDate + "&page_size=" + pageSize + "&page_no=" + pageNum;
                        url = RemoveQueryStringByKey(url);
                    }
                    else if (flag == "Position")
                    {
                        url = ConfigurationManager.AppSettings["POSITION"] + AMID + "?" + "book_ids=" + resourceID + "&position_date=" + startDate + "&page_size=" + pageSize + "&page_no=" + pageNum;// + "&position_date=" + endDate;
                        url = RemoveQueryStringByKey(url);
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
        
         public static string RemoveQueryStringByKey(string url)
        {
            var uri = new Uri(url);

            // this gets all the query string key value pairs as a collection
            var newQueryString = HttpUtility.ParseQueryString(uri.Query);

            List<string> keysToRemove = new List<string>();
            foreach (string key in newQueryString)
            {
                string result = newQueryString.Get(key);
                if (newQueryString.Get(key) == "")
                {
                    // this removes the key if not exists
                    keysToRemove.Add(key);
                }
            }

            foreach (var key in (List<string>)keysToRemove)
            {
                newQueryString.Remove(key);
            }

            // this gets the page path from root without QueryString
            string pagePathWithoutQueryString = uri.GetLeftPart(UriPartial.Path);

            return newQueryString.Count > 0
                ? String.Format("{0}?{1}", pagePathWithoutQueryString, newQueryString)
                : pagePathWithoutQueryString;
        }
    }
}


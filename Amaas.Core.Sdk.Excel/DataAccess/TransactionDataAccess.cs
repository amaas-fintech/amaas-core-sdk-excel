using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Amaas.Core.Sdk.Excel.DataAccess
{
    public class TransactionDataAccess
    {
        public object[,] Position(string AMID, string bookID, string startDate, string pageSize, string pageNum)
        {
            object[,] newArray;
            var returnData = DataConnection.RetrieveData(AMID, bookID, startDate, "", pageSize, pageNum, "Position").Result;
            var arrayResult = JsonConvert.DeserializeObject<dynamic>(returnData);

            if (returnData.Equals("[]") || returnData.Equals("")) return new object[1, 1] { { "The data does not exist" } };
            
            JArray jobjArray = JsonConvert.DeserializeObject<dynamic>(returnData.ToString());
            int colnums = jobjArray.Count;
            int rows = 0;
            //count rows 
            foreach (var stringValue in arrayResult)
            {
                var token = JToken.Parse(stringValue.ToString());
                JObject jobj = JsonConvert.DeserializeObject<dynamic>(stringValue.ToString());
                int count = jobj.Count;
                if (count > rows) rows = count;
            }
            //create newArray to display
            newArray = new object[colnums+1, rows];
            colnums = 0;//reset colnums and rows 
            rows = 0;

            //proces the result
            var itemList = new List<string[]>();
            foreach (var stringValue in arrayResult)
            {
                var token = JToken.Parse(stringValue.ToString());
                JObject jobj = JsonConvert.DeserializeObject<dynamic>(stringValue.ToString());
                int count = jobj.Count;
                colnums++;
                rows = 0;   
                
                foreach(var item in jobj)
                {
                    newArray[0, rows++] = item.Key.ToString();//heder is always at col=0
                }                
                
                for (int i = 0; i < count; i++)
                {
                    rows = 0;
                    foreach (var rate in jobj)
                    {
                        newArray[colnums, rows++] = rate.Value.ToString();
                    }
                }           
            }

            return newArray;
        }

        public object[,] Transaction(string AMID, string resourceID, string startDate, string endDate, string pageSize, string pageNum, string flag)
        {
            string returnData = "";
            if (flag == "TransactionByTransactionID") returnData = DataConnection.RetrieveData(AMID, resourceID, startDate,endDate, pageSize, pageNum, "TransactionByTransactionID").Result;
            else if (flag == "TransactionByBookID") returnData = DataConnection.RetrieveData(AMID, resourceID, startDate, endDate, pageSize, pageNum, "TransactionByBookID").Result; //Received an array           

            var arrayResult = JsonConvert.DeserializeObject<dynamic>(returnData);

            object[,] array = convertToNestedArray(returnData); //veritcal array

            object[,] horizontalArray = new object[array.GetLength(1), array.GetLength(0)]; //horizontal array
            for (int i = 0; i < horizontalArray.GetLength(0); i++)
            {
                for (int j = 0; j < horizontalArray.GetLength(1); j++)
                {
                    if (array[j, i] != null) horizontalArray[i, j] = array[j, i];
                    else horizontalArray[i, j] = "";
                }
            }
            return horizontalArray;
        }

        private int countRows(string dataString)
        {
            int rowCounter = 0;
            string[] children = { "references", "parties", "codes", "comments" };
            var objectResult = JsonConvert.DeserializeObject<dynamic>(dataString);

            foreach (var item in objectResult)
            {
                rowCounter++;
                if (!(item.Value is JValue))
                {
                    string paramsToCheck = item.Name.ToString();
                    foreach (string x in children)
                    {
                        if (paramsToCheck.Contains(x)) //it is a children param
                        {
                            JObject innerRows = objectResult[paramsToCheck];
                            List<string> keysRows = innerRows.Properties().Select(p => p.Name).ToList();
                            foreach (string k in keysRows)
                            {
                                JObject inner2 = innerRows[k].Value<JObject>();
                                List<string> keys2 = inner2.Properties().Select(p => p.Name).ToList();
                                foreach (string value in keys2)
                                {
                                    rowCounter++;
                                }
                            }
                        }
                    }
                }
            }
            return rowCounter;
        }

        private Dictionary<object[,], int> getChildrenParamsValue(string dataString, object[,] newArray, int rows, int colnums)//for non-array-string
        {
            Dictionary<object[,], int> dataReturn = new Dictionary<object[,], int>();

            JToken outer = JToken.Parse(dataString);
            string[] children = { "references", "parties", "codes", "comments" };
            for (int i = 0; i < children.Length; i++)
            {
                string childObject = children[i];
                JObject inner = outer[childObject].Value<JObject>();
                List<string> keys = inner.Properties().Select(p => p.Name).ToList();
                int referenceTypeCounter = 0;
                foreach (string k in keys)
                {
                    referenceTypeCounter++;
                    newArray[rows, colnums ] = k;
                    JObject inner2 = inner[k].Value<JObject>();
                    List<string> keys2 = inner2.Properties().Select(p => p.Name).ToList();
                    foreach (string value in keys2)
                    {
                        double num;
                        var jtValue = inner2.GetValue(value);
                        if (double.TryParse(jtValue.ToString(), out num)) newArray[rows + 1, colnums] = Convert.ToDouble(jtValue.ToString());
                        else newArray[rows + 1, colnums] = jtValue.ToString();
                        rows++;
                    }
                    rows++;
                }
            }
            dataReturn.Add(newArray, rows);
            return dataReturn;
        }

        private List<string[]> getNonChildrenParamsValue(string dataString, List<string[]> itemList)
        {
            var itemdataResult = JsonConvert.DeserializeObject<dynamic>(dataString);
            foreach (var item in itemdataResult)//add non children value to list
            {
                if (item.Value is JValue)
                {
                    string[] itemArray = new string[]
                    {
                       item.Name,item.Value
                    };

                    itemList.Add(itemArray);
                }
            }

            return itemList;
        }

        private object[,] convertToNestedArray(string dataString)
        {
            var objectResult = JsonConvert.DeserializeObject<dynamic>(dataString);
            int rowCounter = 0;
            string[] children = { "references", "parties", "codes", "comments" };
            int colnumCounter;
            object[,] objectD2;
            Boolean isArrayString = false;
            if (dataString[0] == '[') isArrayString = true;

            List<int> testList = new List<int>();
            //count rows
            if (isArrayString)
            {
                int eachRowNum = 0;
               
                foreach (var itemString in objectResult) //for every non-array string
                {
                    var itemdataResult = JsonConvert.DeserializeObject<dynamic>(itemString.ToString());
                    eachRowNum= countRows(itemdataResult.ToString());
                    testList.Add(eachRowNum);
                    if (rowCounter < eachRowNum) rowCounter = eachRowNum;//Max row num which would be colnum later 
                }

                colnumCounter = objectResult.Count;
            }
            else
            {
                 rowCounter = countRows(dataString);
                colnumCounter = 1;
            }
           
            //create a new array to resize with rowCounter 
            object[,] newArray = new object[rowCounter+2, colnumCounter+1]; //FIXED COL/ +header   created array with rows-children, colnum-num of non-array string

            //sign every value to ""
            for (int i = 0; i < newArray.GetLength(0); i++)
            {
                for (int j = 0; j < newArray.GetLength(1); j++)
                {
                    newArray[i, j] = ""; //all the cells contain null otherwise it displays 0;
                }
                Console.WriteLine();
            }
            //Assign header to col 0
            if(isArrayString)
            {
                foreach (var itemString in objectResult) //for every non-array string
                {
                    var itemdataResult = JsonConvert.DeserializeObject<dynamic>(itemString.ToString()); //for each array string
                    int rows = 0;
                    if (countRows(itemdataResult.ToString()) == rowCounter)//for insertion of the header
                    {                  
                        foreach (var item in itemdataResult)//add non children value to list
                        {
                            if (item.Value is JValue)
                            {
                                newArray[rows++, 0] = item.Name;
                            }
                        }
                        //isHeader = true;
                        JToken outer = JToken.Parse(itemString.ToString());
                        for (int i = 0; i < children.Length; i++)
                        {
                            string childObject = children[i];
                            JObject inner = outer[childObject].Value<JObject>();
                            List<string> keys = inner.Properties().Select(p => p.Name).ToList();
                            int referenceTypeCounter = 0;
                            foreach (string k in keys)
                            {
                                referenceTypeCounter++;
                                newArray[rows++, 0] = childObject;                               
                                JObject inner2 = inner[k].Value<JObject>();
                                List<string> keys2 = inner2.Properties().Select(p => p.Name).ToList();
                                foreach (string value in keys2)
                                {
                                    var jtValue = inner2.GetValue(value);
                                    newArray[rows++, 0] = childObject + referenceTypeCounter + "." + value;                         
                                }
                            }
                        }                    
                    }
                }
            }

            if (!isArrayString)
            {
                List<string[]> itemList = new List<string[]>();
                itemList = getNonChildrenParamsValue(dataString, itemList);
                //convert list to array
                string[][] terms = itemList.ToArray();
                string[,] termsD2 = To2D(terms);
                objectD2 = (object[,]) termsD2;

                int rows = objectD2.GetLength(0);
                int colnums = 0;
                //copy non-children values 
                for (int i = 0; i < objectD2.GetLength(0); i++)
                {
                    for (int j = 0; j < objectD2.GetLength(1); j++)
                    {
                        double num ;
                        string thisData = objectD2[i, j].ToString();
                        if (double.TryParse(thisData, out num)) newArray[i, j] = Convert.ToDouble(thisData);
                        else newArray[i, j] = thisData;//copy 
                    }
                }
                //add children value to list
                foreach (var item in objectResult)
                {
                    if (! (item.Value is JValue))
                    {
                        //if it is children value
                        string childrenName= item.Name.ToString();
                        newArray[rows, colnums] = childrenName;
                        var itemdataResult = JsonConvert.DeserializeObject<dynamic>(item.Value.ToString());
                        int count = 1;
                        foreach (var childrenparams in itemdataResult)//children types 
                        {
                            newArray[rows, colnums+1] = childrenparams.Name.ToString();
                            rows++;
                            var subchildren = JsonConvert.DeserializeObject<dynamic>(childrenparams.Value.ToString());
                            foreach (var itemChildren in subchildren)
                            {
                                newArray[rows, colnums] = childrenName+count+"."+itemChildren.Name.ToString();
                                newArray[rows, colnums + 1] = itemChildren.Value.ToString();
                                rows++;
                            }
                            count++;
                        }
                    }
                }             
            }
            else
            {
                int rows=0;
                int colnums=0;
                //reserve colnum 0 for header
                foreach (var itemString in objectResult) //for every non-array string
                {
                    var itemdataResult = JsonConvert.DeserializeObject<dynamic>(itemString.ToString()); //for each array string
                    List<string[]> itemList = new List<string[]>();
                    colnums++;//for each non-array string
                    rows = 0;
                    //add non-children value to newAray             
                    foreach (var item in itemdataResult)//add non children value to list
                    {
                        if (item.Value is JValue)
                        {
                            double num; //convert to doouble;
                            if (double.TryParse(item.Value.ToString(), out num)) newArray[rows++, colnums] = Convert.ToDouble(item.Value);
                            else newArray[rows++, colnums] = item.Value.ToString();
                        }
                    }
                    //children value
                    Dictionary<object[,], int> returnData = getChildrenParamsValue(itemString.ToString(), newArray, rows, colnums);

                    foreach (KeyValuePair<object[,], int> pair in returnData)
                    {
                        newArray = pair.Key;
                        rows = pair.Value;
                    }
                }
            }
            return newArray;
        }
       
        public T[,] To2D<T>(T[][] source)
        {
            try
            {
                int FirstDim = source.Length;
                int SecondDim = source.GroupBy(row => row.Length).Single().Key; // throws InvalidOperationException if source is not rectangular

                var result = new T[FirstDim, SecondDim];
                for (int i = 0; i < FirstDim; ++i)
                    for (int j = 0; j < SecondDim; ++j)
                        result[i, j] = source[i][j];

                return result;
            }
            catch (Exception)
            {
                if(source.Count()!=0) throw new InvalidOperationException("The given jagged array is not rectangular.");
                else throw new System.ArgumentException("The transaction does not exist", "source");
            }
        }
    }
}

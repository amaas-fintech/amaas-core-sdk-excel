using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Amaas.Core.Sdk.Excel.DataAccess
{
    public class TransactionDataAccess
    {
        public object[,] Position(string AMID, string bookID)
        {
            var returnData = DataConnection.RetrieveData(AMID, bookID, "Position").Result;    
            var arrayResult = JsonConvert.DeserializeObject<dynamic>(returnData);

            //proces the result
            var token = JToken.Parse(returnData);
            var count = token.Count();

            var itemList = new List<string[]>();
            for (int i = 0; i < count; i++)
            {
                foreach (var rate in token[i])
                {
                    foreach (var item in rate)
                    {
                        var parentProperty = ((JProperty)item.Parent);
                        string name = parentProperty.Name;
                        string[] itemArray = new string[]
                        {
                             name,item.ToString()
                        };
                        itemList.Add(itemArray);
                    }
                }
            }
            string[][] terms = itemList.ToArray();
            string[,] termsD2 = To2D(terms);
            object[,] objectD2 = (object[,])termsD2;//vertical array

            object[,] horizontalArray = new object[objectD2.GetLength(1), objectD2.GetLength(0)];//horizontal array
            for (int i = 0; i < horizontalArray.GetLength(0); i++)
            {
                for (int j = 0; j < horizontalArray.GetLength(1); j++)
                {
                    if (objectD2[j, i] != null)
                    {
                        double num;
                        string thisData = objectD2[j, i].ToString();
                        if (double.TryParse(thisData, out num)) horizontalArray[i, j] = Convert.ToDouble(thisData);
                        else horizontalArray[i, j] = thisData;
                    }
                    else horizontalArray[i, j] = "";
                }
            }
            return horizontalArray;     
        }

        public object[,] Transaction(string AMID, string resourceID, string flag)
        {
            string returnData = "";
            if (flag == "TransactionByTransactionID") returnData = DataConnection.RetrieveData(AMID, resourceID, "TransactionByTransactionID").Result;
            else if (flag == "TransactionByBookID") returnData = DataConnection.RetrieveData(AMID, resourceID, "TransactionByBookID").Result; //Received an array           

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

        private object[,] convertToNestedArray(string dataString)
        {
            var objectResult = JsonConvert.DeserializeObject<dynamic>(dataString);
            int rowCounter = 0;
            List<string[]> itemList = new List<string[]>();
            int arrayTransaction = 1;
            foreach (var item in objectResult)
            {     
                if (dataString[0]=='[')//array string
                 {
                    arrayTransaction++;
                    foreach (var subItem in item)
                    {
                        rowCounter++;
                        if (subItem.Value is JValue)
                        {
                            string[] itemArray = new string[]
                            {
                                subItem.Name,subItem.Value
                            };

                            itemList.Add(itemArray);
                        }
                    }
                }
                else
                {
                    rowCounter++;
                    if (item.Value is JValue)
                    {
                        string[] itemArray = new string[]
                        {
                          item.Name,item.Value
                        };

                        itemList.Add(itemArray);
                    }
                }

             }
       
            string[][] terms = itemList.ToArray();
            string[,] termsD2 = To2D(terms);
            object[,] objectD2 = (object[,])termsD2;

            //count rows
            string[] children = { "", "", "", "" }; //Children params
            if (arrayTransaction != 1)//for array 
            {
                var dataResult = JsonConvert.DeserializeObject<dynamic>(dataString);
                foreach (var item in dataResult)
                {
                    var itemdataResult = JsonConvert.DeserializeObject<dynamic>(item.ToString());

                    for (int i = 0; i < children.Length; i++)
                    {
                        string childObject = children[i];
                        var eachObject = itemdataResult[childObject];
                        foreach(var arrayParam in eachObject)
                        { 
                            foreach(var subParam in arrayParam)
                            {
                                foreach(var subParamItem in subParam)
                                {
                                    rowCounter ++;
                                }
                            }
                            
                        }
                           
                    }                  
                }
            }
            else
            {
                JToken outerRows = JToken.Parse(dataString);
                for (int i = 0; i < children.Length; i++)
                {
                    string childObject = children[i];
                    JObject innerRows = outerRows[childObject].Value<JObject>();
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

            //create a new array to resize
            object[,] newArray = new object[rowCounter, 2]; //FIXED COL

            for (int i = 0; i < newArray.GetLength(0); i++)
            {
                for (int j = 0; j < newArray.GetLength(1); j++)
                {
                    newArray[i, j] = ""; //all the cells contain null otherwise it displays 0;
                }
                Console.WriteLine();
            }
            for (int i = 0; i < objectD2.GetLength(0); i++)
            {
                for (int j = 0; j < objectD2.GetLength(1); j++)
                {
                    double num;
                    string thisData = objectD2[i, j].ToString();
                    if (double.TryParse(thisData, out num)) newArray[i, j] = Convert.ToDouble(thisData);
                    else newArray[i, j] = thisData;//copy 
                }
            }

            int rows = objectD2.GetLength(0);
            int colnums = objectD2.GetLength(1);

            //copy data to the new array
            JToken outer = JToken.Parse(dataString);
            for (int i = 0; i < children.Length; i++)
            {
                string childObject = children[i];
                if (dataString[0] == '[') { 
                foreach (var subouter in outer) {
                    JObject inner = subouter[childObject].Value<JObject>();
                    List<string> keys = inner.Properties().Select(p => p.Name).ToList();
                    int referenceTypeCounter = 0;
                    foreach (string k in keys)
                    {
                        referenceTypeCounter++;
                        newArray[rows, colnums - 2] = childObject;
                        newArray[rows, colnums - 1] = k;
                        //will not increase rows here
                        JObject inner2 = inner[k].Value<JObject>();
                        List<string> keys2 = inner2.Properties().Select(p => p.Name).ToList();
                        foreach (string value in keys2)
                        {
                            double num;
                            var jtValue = inner2.GetValue(value);
                            newArray[rows + 1, colnums - 2] = childObject + referenceTypeCounter + "." + k + "." + value;
                            var jt = jtValue.ToString();
                            if (double.TryParse(jt, out num)) newArray[rows + 1, colnums - 1] = Convert.ToDouble(jtValue.ToString());
                            else newArray[rows + 1, colnums - 1] = jtValue.ToString();                            
                            rows++;
                        }
                        rows++;
                    }
                 }
                }
                else
                {
                    JObject inner = outer[childObject].Value<JObject>();
                    List<string> keys = inner.Properties().Select(p => p.Name).ToList();
                    int referenceTypeCounter = 0;
                    foreach (string k in keys)
                    {
                        referenceTypeCounter++;
                        newArray[rows, colnums - 2] = childObject;
                        newArray[rows, colnums - 1] = k;
                        //will not increase rows here
                        JObject inner2 = inner[k].Value<JObject>();
                        List<string> keys2 = inner2.Properties().Select(p => p.Name).ToList();
                        foreach (string value in keys2)
                        {
                            double num;
                            var jtValue = inner2.GetValue(value);
                            newArray[rows + 1, colnums - 2] = childObject + referenceTypeCounter + "." + k + "." + value;
                            if (double.TryParse(jtValue.ToString(), out num)) newArray[rows + 1, colnums - 1] = Convert.ToDouble(jtValue.ToString());
                            else newArray[rows + 1, colnums - 1] = jtValue.ToString();
                            rows++;
                        }
                        rows++;
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

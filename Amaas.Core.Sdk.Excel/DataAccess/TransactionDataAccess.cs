using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Amaas.Core.Sdk.Excel.DataAccess
{
    public class TransactionDataAccess
    {
        public object[,] Position(string AMID, string bookID, string startDate, string pageSize, string pageNum, string fields)
        {
            object[,] newArray;
            List<string> placeHolder = new List<string>();
            string assetOptionalFields="";
            var returnData = DataConnection.RetrieveData(AMID, bookID, startDate, "", pageSize, pageNum, fields, placeHolder, assetOptionalFields, "Position").Result; // there is a placeholder for asset_id
            if (returnData.Equals("[]") || returnData.Equals("")) return new object[1, 1] { { "The data does not exist" } };

            var arrayResult = JsonConvert.DeserializeObject<dynamic>(returnData);

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
            newArray = new object[colnums + 1, rows];
            colnums = 0;
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

                foreach (var item in jobj)
                {
                    newArray[0, rows++] = item.Key.ToString();//heder is always at col=0
                }

                for (int i = 0; i < count; i++)
                {
                    rows = 0;
                    foreach (var rate in jobj)//var rate in token[i]
                    {
                        newArray[colnums, rows++] = rate.Value.ToString();
                    }
                }
            }

            return newArray;
        }

        public object[,] Transaction(string AMID, string resourceID, string startDate, string endDate, string pageSize, string pageNum, string fields, string assetOptionalFields, string flag)
        {
            string returnData = "";
            List<string> placeHolder = new List<string>();
            if (flag == "TransactionByTransactionID") returnData = DataConnection.RetrieveData(AMID, resourceID, startDate, endDate, pageSize, pageNum, fields, placeHolder, assetOptionalFields, "TransactionByTransactionID").Result; //there is a placeholder for asset_id
            else if (flag == "TransactionByBookID") returnData = DataConnection.RetrieveData(AMID, resourceID, startDate, endDate, pageSize, pageNum, fields, placeHolder, assetOptionalFields, "TransactionByBookID").Result; //Received an array           

            var arrayResult = JsonConvert.DeserializeObject<dynamic>(returnData);

            object[,] array = convertToNestedArray(returnData); //veritcal array

            List<string> assetIDs = new List<string>();
            for (int i=0; i<array.GetLength(0); i++)
            {
                if((array[i,0].ToString()).Equals("asset_id"))
                {
                    for (int j=1; j<array.GetLength(1); j++) //data starts from col1
                    {
                        assetIDs.Add((array[i, j]).ToString());
                    }
                    break;
                }
            }
            //call asset search
            object[,] assetArray=AssetSearch(AMID, "", "", "", "", "", "", assetOptionalFields, assetIDs);
            object[,] horizontalArray = new object[array.GetLength(1), array.GetLength(0) + assetArray.GetLength(1)]; //horizontal array
            //combine transaction and asset together
            for (int i = 0; i < horizontalArray.GetLength(0); i++)
            {
                for (int j = 0; j < horizontalArray.GetLength(1); j++)
                {
                    if (j < array.GetLength(0))
                    {
                        if (array[j, i] != null) horizontalArray[i, j] = array[j, i];
                        else horizontalArray[i, j] = "";
                    }
                    else
                    {
                        if (assetArray[i, j - array.GetLength(0)] != null) horizontalArray[i, j] = assetArray[i, j - array.GetLength(0)];
                        else horizontalArray[i, j] = "";
                    }
                }
            }
            return horizontalArray;
        }  //with full asset information
    
        public object[,] TransactionFiltered(string AMID, string resourceID, string startDate, string endDate, string pageSize, string pageNum, string fields, string assetOptionalFields, string flag)
        {
            string returnData = "";
            List<string> placeHolder = new List<string>();
            if (flag == "TransactionByTransactionID") returnData = DataConnection.RetrieveData(AMID, resourceID, startDate, endDate, pageSize, pageNum, fields, placeHolder, assetOptionalFields, "TransactionByTransactionID").Result; //there is a placeholder for asset_id
            else if (flag == "TransactionByBookID") returnData = DataConnection.RetrieveData(AMID, resourceID, startDate, endDate, pageSize, pageNum, fields, placeHolder, assetOptionalFields, "TransactionByBookID").Result; //Received an array           

            var arrayResult = JsonConvert.DeserializeObject<dynamic>(returnData);

            object[,] array = convertToNestedArray(returnData); //veritcal array

            List<string> assetIDs = new List<string>();
            for (int i = 0; i < array.GetLength(0); i++)
            {
                if ((array[i, 0].ToString()).Equals("asset_id"))
                {
                    for (int j = 1; j < array.GetLength(1); j++) //data starts from col1
                    {
                        assetIDs.Add((array[i, j]).ToString());
                    }
                    break;
                }
            }
            //call asset search
            //object[,] assetArray = AssetSearch(AMID, "", "", "", "", "", "", assetIDs);
            object[,] horizontalArray = new object[array.GetLength(1), array.GetLength(0)]; //horizontal array
            //combine transaction and asset together
            for (int i = 0; i < horizontalArray.GetLength(0); i++)
            {
                for (int j = 0; j < horizontalArray.GetLength(1); j++)
                {
                    //if (j < array.GetLength(0))
                    {
                        if (array[j, i] != null) horizontalArray[i, j] = array[j, i];
                        else horizontalArray[i, j] = "";
                    }
                }
            }
            horizontalArray = filterFields(horizontalArray);
            return horizontalArray;
        }

        public object[,] AssetSearch(string AMID, string resourceID, string startDate, string endDate, string pageSize, string pageNum, string fields, string assetOptionalFields, List<string> assetIDs)
        {
           int rows = assetIDs.Count();
           List<object[,]> listOfArrays = new List<object[,]>();
           int rowCounter = 0;
           
           //foreach ( string assetID in assetIDs)
           //{
                var returnData = DataConnection.RetrieveData(AMID, resourceID, startDate, endDate, pageSize, pageNum, fields, assetIDs, assetOptionalFields, "AssetSearch").Result;

                var arrayResult = JsonConvert.DeserializeObject<dynamic>(returnData);

                object[,] array = convertToNestedArray(returnData); //veritcal array
                object[,] horizontalArray = new object[array.GetLength(1), array.GetLength(0)]; //initialize as a place holder but not the final 
                for (int i = 0; i < horizontalArray.GetLength(0); i++)
                {
                   for (int j = 0; j < horizontalArray.GetLength(1); j++)
                   {
                          if (array[j, i] != null) horizontalArray[i, j] = array[j, i];
                          else horizontalArray[i, j] = "";
                   }
                }

            /*
            if(rowCounter == 0)
            {
                horizontalArray = new object[assetIDs.Count() + 1, array.GetLength(0)];
                for (int i=0; i< array.GetLength(0); i++)
                {
                    for (int j=0; j< array.GetLength(1); j++)
                    {
                        horizontalArray[j, i] = array[i, j];
                    }
                }
            }
            else
            {
                for (int i=0; i<array.GetLength(1); i++)
                {
                    if((array[0,i].ToString()).Equals(horizontalArray[0,i].ToString()))
                    {
                        horizontalArray[rowCounter, i] = array[0, i].ToString();
                    }
                    else
                    {
                        //search for the correct header
                        //but for children params, the header will be different
                    }
                }
            }

            rowCounter++;
            */
            //}

            return horizontalArray;

        }

        private int countRows(string dataString)
        {
            int rowCounter = 0;
            string[] children = { "references", "parties", "codes", "comments", "charges", "rates", "links"};
            var objectResult = JsonConvert.DeserializeObject<dynamic>(dataString);

            foreach (var item in objectResult)
            {
                
                if (item.Value is JValue)
                {
                    if(item.Value != null)
                    rowCounter++;
                    //do nothing
                }
                else //for non-children params
                {
                    //rowCounter++;
                    string paramsToCheck = item.Name.ToString();
                    foreach(string x in children)
                    {
                        //string value = item.Value.Value.ToString();
                        if (paramsToCheck.Contains(x) && item.Value.Value != null) rowCounter++;
                    }
                    foreach (string x in children)
                    {
                        if (paramsToCheck.Contains(x)) //it is a children param
                        {
                            JObject innerRows = objectResult[paramsToCheck];
                            List<string> keysRows = innerRows.Properties().Select(p => p.Name).ToList(); //type: e.g. regerenceType
                            if (x =="links")
                            {
                                foreach (var k in keysRows)
                                {
                                    rowCounter++;
                                    string subArrayString = innerRows[k].ToString();
                                    var subArrayobjectResult = JsonConvert.DeserializeObject<dynamic>(subArrayString);
                                    foreach (var subArrayElement in subArrayobjectResult)
                                    {
                                        foreach(var element in subArrayElement)
                                        rowCounter++;
                                    }
                                }
                            }
                            else
                            {
                                foreach (string k in keysRows)
                                {
                                    rowCounter++;
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
            }
            return rowCounter;
        }

        private Dictionary<object[,], int> getChildrenParamsValue(string dataString, object[,] newArray, int rows, int colnums)//for non-array-string
        {

            Dictionary<object[,], int> dataReturn = new Dictionary<object[,], int>();

            JToken outer = JToken.Parse(dataString);
            string[] children = { "references", "parties", "codes", "comments", "charges", "rates", "links"};
            for (int i = 0; i < children.Length; i++)
            {
                    string childObject = children[i];
                    JObject inner;
                    if (outer[childObject]==null)
                    {
                       continue;
                    }
                    else
                    {
                       inner= outer[childObject].Value<JObject>();
                    }
                    //change
                    List<string> keys = inner.Properties().Select(p => p.Name).ToList();
                    int referenceTypeCounter = 0;
                    int counterToCheckHeader = 0;
                    foreach (string k in keys)
                    {                     
                      counterToCheckHeader++;
                     if (newArray[rows, 0].Equals(childObject+ counterToCheckHeader)) //if the child object matches the header
                     {
                        referenceTypeCounter++;
                        newArray[rows, colnums] = k;
                        if (!childObject.Equals("links"))
                        {
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
                        else //for links
                        {
                            foreach (var key in keys)
                            {
                                referenceTypeCounter++;
                                newArray[rows++, colnums] = key; // e.g. Block, single, multiple
                                string subArrayString = inner[key].ToString();
                                var subArrayobjectResult = JsonConvert.DeserializeObject<dynamic>(subArrayString);
                                //int innerReferenceCounter = 0;
                                foreach (var subArrayElement in subArrayobjectResult)
                                {
                                    //innerReferenceCounter++;
                                    foreach (var element in subArrayElement)
                                    {
                                        newArray[rows++, colnums] = element.Value.ToString();
                                    }
                                }
                            }
                            rows++;
                        }
                     }
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
            string[] children = { "references", "parties", "codes", "comments", "charges", "rates", "links"};
            List<string> assetIDs = new List<string>(); //caputure all assetIDs
            int assetIDPosition;
            int colnumCounter;
            object[,] objectD2;
            Boolean isArrayString = false;
            if (dataString[0] == '[') isArrayString = true;

            List<int> testList = new List<int>();
            //count rows--find the longest row
            if (isArrayString)
            {
                int eachRowNum = 0;

                foreach (var itemString in objectResult) //for every non-array string
                {
                    var itemdataResult = JsonConvert.DeserializeObject<dynamic>(itemString.ToString());
                    eachRowNum = countRows(itemdataResult.ToString());
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
            object[,] newArray = new object[rowCounter, colnumCounter + 1]; //FIXED COL/ +header   created array with rows-children, colnum-num of non-array string
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
            if (isArrayString)
            {
                foreach (var itemString in objectResult) //for every non-array string
                {
                    var itemdataResult = JsonConvert.DeserializeObject<dynamic>(itemString.ToString()); //for each array string
                    int rows = 0;
                    if (countRows(itemdataResult.ToString()) == rowCounter)//the longest row-for insertion of the header
                    {
                        int positionCounterForAssetID = 0;
                        foreach (var item in itemdataResult)//add non children value to list
                        {
                            if (item.Value is JValue)
                            {
                                if (item.Value != null)
                                {
                                    newArray[rows++, 0] = item.Name;
                                    if ((item.Name.ToString()).Equals("asset_id")) assetIDPosition = positionCounterForAssetID; //the array index for assetId
                                    positionCounterForAssetID++;
                                }
                            }
                        }
                        //header is at the new position
                        //headers for children 
                        JToken outer = JToken.Parse(itemString.ToString());
                        for (int i = 0; i < children.Length; i++)
                        {
                            string childObject = children[i];
                            JObject inner;
                            if (outer[childObject] == null)
                            {
                                continue;
                            }
                            else
                            {
                                inner = outer[childObject].Value<JObject>();
                            }//changed
                           
                            List<string> keys = inner.Properties().Select(p => p.Name).ToList();
                            int referenceTypeCounter = 0;
                            if (childObject == "links") //"links" is  different from other children parameters so it is needed to be dealt separately
                            {
                                foreach (var k in keys)
                                {
                                    referenceTypeCounter++;
                                    newArray[rows++, 0] = childObject + referenceTypeCounter; // e.g. links1
                                    string subArrayString = inner[k].ToString();
                                    var subArrayobjectResult = JsonConvert.DeserializeObject<dynamic>(subArrayString);
                                    foreach (var subArrayElement in subArrayobjectResult)
                                    {
                                        int innerReferenceCount = 0;
                                        innerReferenceCount++;
                                        foreach (var element in subArrayElement)
                                        {
                                            newArray[rows++, 0] = childObject + referenceTypeCounter + "." + k + innerReferenceCount + "." + element.Name;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                //newArray[rows++, 0] = childObject; //this is printed only once for each child params
                                foreach (string k in keys)
                                {
                                    referenceTypeCounter++;
                                    newArray[rows++, 0] = childObject + referenceTypeCounter; //like references2.. the xth children type
                                    JObject inner2 = inner[k].Value<JObject>();
                                    List<string> keys2 = inner2.Properties().Select(p => p.Name).ToList();
                                    foreach (string value in keys2)
                                    {
                                        var jtValue = inner2.GetValue(value); //content for value, like updated time (value) : 2017-08-10-18:00 (ocntent)
                                        newArray[rows++, 0] = childObject + referenceTypeCounter + "." + value;
                                    }
                                }
                            }
                        }
                        break; //insertion of header is done;
                    }
                    else continue;
                }
            }
           
            if (!isArrayString)
            {
                List<string[]> itemList = new List<string[]>();
                itemList = getNonChildrenParamsValue(dataString, itemList);
                //convert list to array
                string[][] terms = itemList.ToArray();
                string[,] termsD2 = To2D(terms);
                objectD2 = (object[,])termsD2;

                int rows = 0;
                int colnums = 0;
                int counter = 0;
                //copy non-children values 
                for (int i = 0; i < objectD2.GetLength(0); i++)
                {
                    //counter = 0;
                    for (int j = 0; j < objectD2.GetLength(1); j++)
                    {
                        double num;
                        string thisData;
                        object thisDataObj = objectD2[i, j];
                        if (j == 0 && objectD2[i, j + 1] == null || objectD2[i, j] == null)
                        {
                            continue;
                        }
                        else
                        {
                            if (thisDataObj != null)
                            {
                                thisData = thisDataObj.ToString();
                                if (double.TryParse(thisData, out num)) newArray[counter, j] = Convert.ToDouble(thisData);
                                else newArray[counter, j] = thisData;//copy 

                                if (j == (objectD2.GetLength(1)-1)) counter++; //header 
                            }
                        }
                    }
                }
                rows = counter;
                //add children value to list
                foreach (var item in objectResult)
                {
                    if (!(item.Value is JValue))
                    {
                        //if it is children value
                        string childrenName = item.Name.ToString();
                        //newArray[rows, colnums] = childrenName;
                        var itemdataResult = JsonConvert.DeserializeObject<dynamic>(item.Value.ToString());
                        int count = 1;
                        foreach (var childrenparams in itemdataResult)//children types 
                        {
                            newArray[rows, colnums] = childrenName+count; //e.g. reference1
                            newArray[rows, colnums + 1] = childrenparams.Name.ToString();
                            rows++;
                            var subchildren = JsonConvert.DeserializeObject<dynamic>(childrenparams.Value.ToString());
                            foreach (var itemChildren in subchildren)
                            {
                                newArray[rows, colnums] = childrenName + count + "." + itemChildren.Name.ToString();
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
                int rows = 0;
                int colnums = 0;
                //reserve colnum 0 for header
                foreach (var itemString in objectResult) //for every non-array string
                {
                    var itemdataResult = JsonConvert.DeserializeObject<dynamic>(itemString.ToString()); //for each array string
                    List<string[]> itemList = new List<string[]>();
                    colnums++;//for each non-array string
                    rows = 0;
                    //add non-children value to newAray             
                    foreach (var item in itemdataResult)//add non-children value to list
                    {
                        if (item.Value is JValue)
                        {
                            if ((item.Name.ToString()).Equals(newArray[rows, 0])) //if it is the correct header for the value
                            {
                                if ((item.Name.ToString()).Equals("asset_id")) newArray[rows++, colnums] = item.Value.ToString(); //if asset_id is a num such as 001, it is converted to 1 and this is not acceptable in query string in AssetSearch
                                else
                                {
                                    double num; //convert to doouble;
                                    if (double.TryParse(item.Value.ToString(), out num)) newArray[rows++, colnums] = Convert.ToDouble(item.Value); //if the value is a number
                                    else newArray[rows++, colnums] = item.Value.ToString();
                                }
                                if ((item.Name.ToString()).Equals("asset_id")) assetIDs.Add(item.Value.ToString()); //add all asset ids to list in order
                            }
                        }
                        else  //find the header of the corresponding non-children data
                        {
                            int arrayRows = newArray.GetLength(0);
                            for(int rowLength=0; rowLength< arrayRows; rowLength++)
                            {
                                if(newArray[rowLength, 0].Equals(item.Name.ToString()))
                                {
                                    newArray[rowLength, colnums] = item.Value;
                                    if ((item.Name.ToString()).Equals("asset_id")) assetIDs.Add(item.Value.ToString());
                                }
                            }
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
                if (source.Count() != 0) throw new InvalidOperationException("The given jagged array is not rectangular.");
                else throw new System.ArgumentException("The transaction does not exist", "source");
            }
        }

        public object[,] filterFields (object[,] array)
        {
            string[] wantedFields = { "asset_type", "asset_book_id", "transaction_date", "settlement_date", "transaction_action", "quantity", "price", "transaction_currency", "settlement_currency", "counterparty_book_id"};
            object[,] wantedFieldsArray= new object[ array.GetLength(0), wantedFields.Length-1]; 
            int columnCounter = 0;

            for (int j = 0; j < array.GetLength(1); j++)
            {
                string header = array[0, j].ToString();

                foreach (string field in wantedFields)
                {                  
                    //for (int j = 0; j < array.GetLength(1); j++)                     
                    if (header.Equals(field) && !field.Equals("references"))
                    {
                        for (int i = 0; i < array.GetLength(0); i++) wantedFieldsArray[i, columnCounter] = array[i, j];
                        columnCounter++;
                    }
                }
            }

            List<List<string>> referenceLists = new List<List<string>>();
            for(int j = 0; j < array.GetLength(1); j++)
            {
                string header = array[0, j].ToString();
                if (header.Contains("references") && !(header.Contains("updated_by") || header.Contains("created_by") || header.Contains("updated_time") || header.Contains("created_time") || header.Contains("version") || header.Contains("internal_id") || header.Contains("active")))
                {
                    List<string> referenceList = new List<string>();
                    for (int i=0; i < array.GetLength(0); i++)
                    {
                        referenceList.Add(array[i, j].ToString());
                    }
                    referenceLists.Add(referenceList);
                }
            }
            //convert list to array
            int rowCount = array.GetLength(0);
            int columnCount = referenceLists.Count();
            int columnCounter2 = 0;
            int rowCounter2 = 0;
            object[,] referenceArray = new object[rowCount, columnCount];
            foreach( List<string> referenceList in referenceLists)
            {
                foreach(string referenceData in referenceList)
                {
                    referenceArray[rowCounter2++, columnCounter2] = referenceData;
                }
                rowCounter2 = 0; //reset
                columnCounter2++; //shift column
            }

            object[,] returnedArray = new object[wantedFieldsArray.GetLength(0), wantedFieldsArray.GetLength(1) + referenceArray.GetLength(1)];
            for(int i=0; i<returnedArray.GetLength(0); i++)
            {
                for (int j=0; j<returnedArray.GetLength(1); j++)
                {
                    if (j < wantedFieldsArray.GetLength(1))
                        returnedArray[i, j] = wantedFieldsArray[i, j];
                    else
                        returnedArray[i, j] = referenceArray[i, j - wantedFieldsArray.GetLength(1)];
                }
            }
            return returnedArray;
        }
    }
}

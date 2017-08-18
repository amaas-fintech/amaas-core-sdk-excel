# AMaaS Core SDK for .Net
This is the Asset Management as a Service (AMaaS) Software Development Kit (SDK) for .Net. This module can be used in C# and in Visual Studio.

## What is AMaaS?
AMaaS is a modular platform for Asset Managers with an open, RESTful API for programmatic access to its functionality.

AMaaS Core features a secure, encrypted database, which serves as the foundation for asset management platforms and FinTech solutions. AMaaS also provides portfolio visualizations and analytics through AMaaS Web, and exception management & financial event notification through AMaaS Monitor.

## Quick start (For developer)
This project allows developers to develop Excel user defined functions (UDF) to retrieve data by calling AWS endpoint URL with SRP authentication.  

### Preparation
Install Packages and add them as your references for the project: 
```
Install-Package Excel-Dna
Install-Package BouncyCastle
Install-Package Newtonsoft.Json
Install-Package AWSSDK.ApplicationAutoScaling
Install-Package AWSSDK.CognitoIdentityProvider
Install-Package AWSSDK.Core
Install-Package ExcelDna.IntelliSense -- More documentation on: https://github.com/Excel-DNA/IntelliSense/wiki/Usage-Instructions
```
Add your credentials in App.config:
```
    <add key="USERNAME" value="" />
    <!--Your Username-->
    <add key="PASSWORD" value=""/>
    <!--Your Password-->
    <add key="CLIENT_ID" value="" />
    <!--Your Client Id-->
    <add key="USERPOOL_ID" value="" />
    <!--Your Userpool Id-->
    <add key="POOL_NAME" value=""/>
    <!--Your POOL Name Without the Region-->
    <add key="TRANSACTION" value="" />
    <!--Endpoint URL-->
    <add key="POSITION" value="" />
    <!--Endpoint URL-->
    <add key="BASEURL" value="" />
    <!--Endpoint URL-->
```
Install .xll:
```
   Excel-DNA/IntelliSense
```
IntelliSense can be downloaded at: https://github.com/Excel-DNA/IntelliSense. 

### To authenticate your AWS with Secure Remote Password (SRP)
```
Navigate to App.config:
    <add key="USERNAME" value="" /    
    <!--Your Username-->
    <add key="PASSWORD" value=""/>
    <!--Your Password-->
    <add key="CLIENT_ID" value="" />
    <!--Your Client Id-->
    <add key="USERPOOL_ID" value="" />
    <!--Your Userpool Id-->
    <add key="POOL_NAME" value=""/>
    <!--Your POOL Name Without the Region-->
Run Amaas.Core.Sdk.Authentication.Tests to check your authntication.
```
### To create your query string
```
1. Navigate to App.config:
    <add key="TRANSACTION" value="" />
    <!--Endpoint URL-->
    <add key="POSITION" value="" />
    <!--Endpoint URL-->
    <add key="BASEURL" value="" />
    <!--Endpoint URL-->  
    <add key="YourURL" value="" />
    <!--Your Endpoint URL-->  
2. Or add more keys for your endpoints and change the following code: 
   ->DataConnection.cs: 
     else if (flag == "Flag To Identify Different function calls which call the corresponding query string")
     {
        if (fields.Equals("")) //if users specify the fields to search 
        {
            url = ConfigurationManager.AppSettings["TRANSACTION"] + "/" + AMID + "?" + "asset_book_ids=" + resourceID + "&transaction_date_start=" + startDate + "&transaction_date_end=" + endDate + "&page_size=" + pageSize + "&page_no=" + pageNum;
            url = RemoveQueryStringByKey(url);
         }
         else //otherwise
         {
            url = ConfigurationManager.AppSettings["TRANSACTION"] + "?" + "asset_manager_ids=" + AMID + "&" + "asset_book_ids=" + resourceID + "&" + "fields=" + fields + "&" + "transaction_date_start=" + startDate + "&transaction_date_end=" + endDate + "&page_size=" + pageSize + "&page_no=" + pageNum;
            url = RemoveQueryStringByKey(url);
          }
     }
   ->TransactionDataAccess.cs:
       method indentifier: 
            Transaction(string AMID, string resourceID, string startDate, string endDate, string pageSize, string pageNum, string fields, string assetOptionalFields, string flag)
       and body:
            else if (flag == "Flag To Identify Different function calls which will call the corresponding query string") returnData = DataConnection.RetrieveData("create your parameters to be passed, if you have any").Result; //Receive an array   
3. Also have to change:
   ->TransactionDataAccess.cs:
     Update string[] children = { "children array in the json string1", "children array in the json string2" }; with your children parameters in countRows, getChildrenParamsValue, convertToNestedArray methods.    
```
### To create a UDF: 
```
Navigate to TransactionUdf.cs:
1. It must be a static method
2. To show information of the fomula on Excel: 
   [ExcelFunction(Name = "myFirstUDF")]
   [ExcelArgument(AllowReference = true, Name = "Name")] string name
   More documentation: https://github.com/Excel-DNA/ExcelDna/wiki/ExcelFunction-and-other-attributes
   
Example:   
[ExcelFunction(Name = "myFirstUDF", IsMacroType = true, Description = "Get transaction according to book ID")]
public static void myFirstUDFAsync(([ExcelArgument(AllowReference = true, Name = "Asset manager ID")] string AMID, 
                                    [ExcelArgument(AllowReference = true, Name = "Book ID")]string bookID, 
                                    [ExcelArgument(AllowReference = true, Name = "Start date of the transaction")]string startDate,  
                                    [ExcelArgument(AllowReference = true, Name = "End date of the transaction")]string endDate,
                                    [ExcelArgument(AllowReference = true, Name = "Number of transactions to retrieve in a page")]string pageSize,
                                    [ExcelArgument(AllowReference = true, Name = "The page.no")]string pageNum,
                                    [ExcelArgument(AllowReference = true, Name = "Fields search for transaction")]string fields,
                                    [ExcelArgument(AllowReference = true, Name = "Fields search for asset")]string assetOptionalFields)
{
    ExcelReference caller = XlCall.Excel(XlCall.xlfCaller) as ExcelReference;
            object[,] getArray = { };
            Task.Factory.StartNew(() => Thread.Sleep(0))// No delay 
            .ContinueWith(t =>
                ExcelAsyncUtil.QueueAsMacro(() =>
                {
                    TransactionDataAccess TDA = new TransactionDataAccess();
                    getArray = TDA.Transaction(AMID, bookID, startDate, endDate, pageSize, pageNum, fields, assetOptionalFields, "TransactionByBookID");
                    // Create a Range of the correct size:
                    int rows = getArray.GetLength(0);
                    int columns = getArray.GetLength(1);
                    ExcelReference target = new ExcelReference(caller.RowFirst, caller.RowFirst + rows - 1, caller.ColumnFirst,                                                     caller.ColumnFirst + columns - 1, caller.SheetId); //the target cell to store array data
                    //Assign the Array to the Range in one shot:
                    if (getArray.GetLength(0) == 1 && getArray.GetLength(1) == 2) target.SetValue("The data does not exist.");
                    else {
                        object[,] resizeArray = new object[rows + 1, columns];
                        resizeArray[0, 0] = XlCall.Excel(XlCall.xlfGetFormula, target);
                        for (int i = 1; i < rows + 1; i++)
                        {
                            for (int j = 0; j < columns; j++) resizeArray[i, j] = getArray[i - 1, j];
                        }
                        target.SetValue(resizeArray);
                    } 
                }));
}

```
Note:
```
convertedToNestedArray method in TransactionDataAccess.cs converts an json string to a 2D jagged array as Excel-dna currently only support the display of a 2D jagged array, so a nested array/a multidimensional array has to be flatened before sent to excel spead sheet.
```
## Test Project on Visual Studio 
The SDK contains unit tests to test the SRP authentication connection and data retrival by calling AWS endpoints. The way to run the suite is: Test->Run->All Tests.

## Test UDF on Excel
1. Build ExcelDnaPack file from Visual Studio with the naming covention: YouProjectName-AddIn.dna and YourProjectName-AddIn-packed.xll can be found in bin.
2. Navigate to Excel File->Options->Add-ins->Manage: Excel Add-ins->Go->YourProjectName-AddIn-packed.xll->OK. Then UDFs can be tested on Excel directly from the formula bar.

## Quick start (For Users)
1. Available formula:
   ```
   GetPositionAsync(string AMID, string bookID, string startDate, string pageSize, string pageNum, string fields)
   GetTransactionByBookIDAsync(string AMID, string bookID, string startDate, string endDate, string pageSize, string pageNum, string fields, string assetOptionalFields)
   GetFilteredTransactionByBookIDAsync(string AMID, string bookID, string startDate, string endDate, string pageSize, string pageNum, string fields, string assetOptionalFields)
   GetTransactionByTransactionIDAsync(string AMID, string transactionID, string startDate, string endDate, string pageSize, string pageNum, string fields, string assetOptionalFields)
   ```
  Note: By default, page_size is 100 and page_number is 1 (first page).
 2. To utilize the formula above:
   ...1.1 Download Amaas-core-sdk-net and look for the file: Amaas.core.sdk.Excel-AddIn-packed.xll.
   ...1.2 Navigate to Excel File->Options->Add-ins->Manage: Excel Add-ins->Go->Amaas.core.sdk.Excel-AddIn-packed.xll->OK. Then UDFs can be tested on Excel directly from the formula bar.
   ...1.3 For every formula, asset manager ID must be provided.
   ...1.4 Find Fields: Provides the functionality of the Field Search function directly in Excel, so you can locate, store, and use Argomi data fields, gaining access to the data you need for your analysis. When you specify the fileds to search, please follow the input format: client_id for client id.

## Support
For support with the SDKs, please raise issues on GitHub. The AMaaS team can be contacted at support@amaas.com. Customers who have purchased a support plan can find the contact details within AMaaS Admin.

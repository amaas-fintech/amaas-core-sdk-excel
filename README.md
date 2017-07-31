# AMaaS Core SDK for .Net
This is the Asset Management as a Service (AMaaS) Software Development Kit (SDK) for .Net. This module can be used in C# and in Visual Studio.

## What is AMaaS?
AMaaS is a modular platform for Asset Managers with an open, RESTful API for programmatic access to its functionality.

AMaaS Core features a secure, encrypted database, which serves as the foundation for asset management platforms and FinTech solutions. AMaaS also provides portfolio visualizations and analytics through AMaaS Web, and exception management & financial event notification through AMaaS Monitor.

## Preparation
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

## Quick start
This project allows developers to develop Excel user defined functions (UDF) to retrieve data by calling AWS endpoint URL.  

To authenticate your AWS with Secure Remote Password (SRP)
```
Navigate to App.config:
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
Run Amaas.Core.Sdk.Authentication.Tests to check your authntication.
```
To create your query string
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
         url = ConfigurationManager.AppSettings["YouNewKey"];
         url = RemoveQueryStringByKey(url); //remove the empty parameters in the query string in case users leave UDF parameters blank
     }
   ->TransactionDataAccess.cs:
       method indentifier: 
            Transaction(string AMID, string resourceID, string flag)
       and body:
            else if (flag == "Flag To Identify Different function calls which call the corresponding query string") returnData =                     DataConnection.RetrieveData("create your parameters to be passed, if you have any").Result; //Receive an array   
3. Also have to change:
   ->TransactionDataAccess.cs:
     string[] children = { "children array in the json string1", "children array in the json string2" }; in countRows,                        getChildrenParamsValue, convertToNestedArray methods.    
```
To create a UDF: 
```
Navigate to TransactionUdf.cs:
1. It must be a static method
2. [ExcelFunction(Name = "myFirstUDF")]
   [ExcelArgument(AllowReference = true, Name = "Name")] string name
   More documentation: https://github.com/Excel-DNA/ExcelDna/wiki/ExcelFunction-and-other-attributes
   
Example:   
[ExcelFunction(Name = "myFirstUDF")]
public static void myFirstUDFAsync([ExcelArgument(AllowReference = true, Name = "Asset Manager ID")] string AMID,
[ExcelArgument(AllowReference = true, Name = "Book ID")] string bookId)
{
    ExcelReference caller = XlCall.Excel(XlCall.xlfCaller) as ExcelReference;
            object[,] getArray = { };
            Task.Factory.StartNew(() => Thread.Sleep(0))// No delay 
            .ContinueWith(t =>
                ExcelAsyncUtil.QueueAsMacro(() =>
                {
                    TransactionDataAccess TDA = new TransactionDataAccess();
                    getArray = TDA.Transaction(AMID, bookID, "TransactionByBookID"); 
                    // Create a Range of the correct size:
                    int rows = getArray.GetLength(0);
                    int columns = getArray.GetLength(1);
                    ExcelReference target = new ExcelReference(caller.RowFirst, caller.RowFirst + rows - 1, caller.ColumnFirst,                                                     caller.ColumnFirst + columns - 1, caller.SheetId);
                    //Assign the Array to the Range in one shot:
                    if(getArray.GetLength(0)==1 && getArray.GetLength(1)==2) target.SetValue("The data does not exist.");
                    else target.SetValue(getArray);
                }));
}

```
Note:
```
convertedToNestedArray method in TransactionDataAccess.cs converts an json string to a 2D jagged array as Excel-dna currently only support the display of a 2D array, so a nested array has to be flatened before sent to excel spead sheet.
```
## Test Project on Visual Studio 
The SDK contains unit tests to test the SRP authentication connection and data retrival by calling AWS endpoints. The way to run the suite is: Test->Run->All Tests.

## Test UDF on Excel
For clients to utilize UDF methods on Excel, go to File->Options->Add-ins->Manage: Excel Add-ins->Go->Amaas.Core.Sdk.Excel Add-In->OK. Then UDFs can be tested through Excel directly on the formula bar.

## Support
For support with the SDKs, please raise issues on GitHub. The AMaaS team can be contacted at support@amaas.com. Customers who have purchased a support plan can find the contact details within AMaaS Admin.

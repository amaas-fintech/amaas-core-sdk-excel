# AMaaS Core SDK for .Net
This is the Asset Management as a Service (AMaaS) Software Development Kit (SDK) for .Net. This module can be used in C# and in Visual Studio.

## What is AMaaS?
AMaaS is a modular platform for Asset Managers with an open, RESTful API for programmatic access to its functionality.

AMaaS Core features a secure, encrypted database, which serves as the foundation for asset management platforms and FinTech solutions. AMaaS also provides portfolio visualizations and analytics through AMaaS Web, and exception management & financial event notification through AMaaS Monitor.

## Quick Start
Install Packages: 
```
Install-Package Excel-Dna
Install-Package BouncyCastle
Install-Package Newtonsoft.Json
Install-Package AWSSDK.ApplicationAutoScaling
Install-Package AWSSDK.CognitoIdentityProvider
Install-Package AWSSDK.Core
```
Add the following parameters in App.config with your own credentials:
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
IntelliSense can be downloaded at: https://github.com/Excel-DNA/IntelliSense. And load IntelliSense.xll to Excel.

## Test Project on Visual Studio 
The SDK contains unit tests to test the SRP authentication connection and data retrival by calling AWS endpoints. The way to run the suite is: Test->Run->All Tests.

## Test UDF on Excel
For clients to utilize UDF methods on Excel, go to File->Options->Add-ins->Manage: Excel Add-ins->Go->Amaas.Core.Sdk.Excel Add-In->OK. Then UDFs can be tested through Excel directly on the formula bar.

## Support
For support with the SDKs, please raise issues on GitHub. The AMaaS team can be contacted at support@amaas.com. Customers who have purchased a support plan can find the contact details within AMaaS Admin.

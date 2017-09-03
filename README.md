# Argomi Excel Addin
This add-in enables Argomi users to interface with Argomi using Microsoft Excel user defined functions. To start using the add-in, follow the steps below:

## Installation
1. Download the [Argomi Excel Add-in](https://github.com/amaas-fintech/amaas-core-sdk-excel/blob/master/Distribution/Argomi.ExcelAddIn.xll)
2. Open Microsoft Excel. Click the File tab, click Options, and then click the Add-Ins category.
3. In the Manage box, click Excel Add-ins, and then click Go.
    - The Add-Ins dialog box appears.
4. In the Add-Ins available box, click Browse and select the Argomi.ExcelAddin.xll file you downloaded in step 1.

## Quick Start
1. In order to use the Argomi UDFs, you must log in with your Argomi credentials first.
2. To login, select the Argomi tab in Excel and click Login.
3. Enter your Argomi credentials
    - if the Argomi tab is not available in Excel, make sure that you have done the Installation steps
    - if you don't have Argomi credentials, please contact support@argomi.com

## Argomi UDFs
The following list shows the currently supported user defined functions:

### 1. ARGO.POS(position date)
* Description: Get Positions
* Argument: position date [optional] - the date of the positions to retrieve

### 2. ARGO.BPOS(book, position date)
* Description: Get Positions by book
* Arguments: 
    + book [optional] - the containing book of the positions to retrieve
    + position date [optional] - the date of the positions to retrieve        
    
### 3. ARGO.TRANS(start date, end date)
* Description: Get Transactions
* Arguments:
    + start date [optional] - starting date of the date range filter of the transactions to retrieve
    + end date [optional] - end date of the date range filter of the transactions to retrieve

### 4. ARGO.BTRANS(book, start date, end date)
* Description: Get Transactions by book
* Arguments:
    + book [optional] - the containing book of the transactions to retrieve
    + start date [optional] - starting date of the date range filter of the transactions to retrieve
    + end date [optional] - end date of the date range filter of the transactions to retrieve


## Support
For support with the SDKs, please raise issues on GitHub. The AMaaS team can be contacted at support@argomi.com. Customers who have purchased a support plan can find the contact details within AMaaS Admin.

using AMaaS.Core.Sdk.Assets;
using AMaaS.Core.Sdk.Assets.Models;
using AMaaS.Core.Sdk.Excel.Constants;
using AMaaS.Core.Sdk.Excel.Formatters;
using AMaaS.Core.Sdk.Excel.Helpers;
using AMaaS.Core.Sdk.Excel.Models;
using AMaaS.Core.Sdk.Transactions;
using AMaaS.Core.Sdk.Transactions.Models;
using Autofac;
using ExcelDna.Integration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace AMaaS.Core.Sdk.Excel
{
    public class TransactionsAddin : AMaaSAddinBase, ITransactionsAddin
    {
        [ExcelFunction(Name = UdfNames.PositionSearch, Description = "Retrieve positions")]
        public static object GetPositions(
            [ExcelArgument(AllowReference = true, Name = "Position Date")]string businessDate = "")
        {
            return GetBookPositions(string.Empty, businessDate);
        }

        [ExcelFunction(Name = UdfNames.BookPositionSearch, Description = "Retrieve positions by book")]
        public static object GetBookPositions(
            [ExcelArgument(AllowReference = true, Name = "Book ID")]string bookId = "",
            [ExcelArgument(AllowReference = true, Name = "Position Date")]string businessDate = "")
        {
           
            var caller        = AddinContext.Excel.Call(XlCall.xlfCaller);
            ExcelFunc getData = () =>
            {
                if (!IsLoggedIn)
                    throw new ApplicationException($"Please login from the Argomi tab.");

                if (AddinContext.AssetManagerIds.Count == 0)
                    throw new ApplicationException($"User {AddinContext.Username} does not have a valid asset manager relationship");

                var bookIds      = bookId.MatchAll() ? null : new List<string> { bookId };
                var positionDate = !businessDate.MatchAll() &&
                                   DateTime.TryParse(businessDate, CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out DateTime businessDateParsed)
                                                ? (DateTime?)businessDateParsed
                                                : null;
                var api       = AddinContext.Container.Resolve<ITransactionsInterface>();
                var positions = api.SearchPositions(
                                    assetManagerIds: AddinContext.AssetManagerIds,
                                    bookIds: bookIds,
                                    positionDate: positionDate).Result;
                var assetsApi = AddinContext.Container.Resolve<IAssetsInterface>();
                var assets    = assetsApi.SearchAssets(
                                            assetManagerIds: AddinContext.AssetManagerIds,
                                            assetIds: positions.Select(p => p.AssetId).ToList(),
                                            pageNo: 1,
                                            pageSize: QueryConstants.DefaultPageSize).Result;
                var models = positions.Select(p =>
                                new EnrichedModel<Position, Asset>(p, assets.FirstOrDefault(a => a.AssetId == p.AssetId)));
                return ExcelTable.Format(models, AddinContext.Container.Resolve<IFormatter<EnrichedModel<Position, Asset>>>(), caller);
            };
            var output = AddinContext.Excel.Run(
                            UdfNames.BookTransactionSearch,
                            string.Join(",", bookId, businessDate),
                            getData);
            return output?.Equals(ExcelError.ExcelErrorNA) ?? true ? ExcelError.ExcelErrorGettingData : output;
        }

        [ExcelFunction(Name = UdfNames.TransactionSearch, Description = "Retrieve transactions by book")]
        public static object GetTransactions(
            [ExcelArgument(AllowReference = true, Name = "Begin date for the transaction search.")] string beginDate = "",
            [ExcelArgument(AllowReference = true, Name = "End date for the transaction search.")] string endDate = "")
        {
            return GetBookTransactions(string.Empty, beginDate, endDate);
        }

        [ExcelFunction(Name = UdfNames.BookTransactionSearch, Description = "Retrieve transactions by book")]
        public static object GetBookTransactions(
            [ExcelArgument(AllowReference = true, Name = "Book ID")] string bookId = "", 
            [ExcelArgument(AllowReference = true, Name = "Begin date for the transaction search.")] string beginDate = "", 
            [ExcelArgument(AllowReference = true, Name = "End date for the transaction search.")] string endDate = "")
        {
            var caller        = AddinContext.Excel.Call(XlCall.xlfCaller);
            ExcelFunc getData = () =>
            {
                if (!IsLoggedIn)
                    throw new ApplicationException($"Please login from the Argomi tab.");

                if (AddinContext.AssetManagerIds.Count == 0)
                    throw new ApplicationException($"User {AddinContext.Username} does not have a valid asset manager relationship");

                var bookIds = bookId.MatchAll() ? null : new List<string> { bookId };
                var transactionStartDate = !beginDate.MatchAll() &&
                                           DateTime.TryParse(beginDate, CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out DateTime beginDateParsed)
                                                ? (DateTime?)beginDateParsed
                                                : null;
                var transactionEndDate = !endDate.MatchAll() &&
                                         DateTime.TryParse(endDate, CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out DateTime endDateParsed)
                                                ? (DateTime?)endDateParsed
                                                : null;
                var api          = AddinContext.Container.Resolve<ITransactionsInterface>();
                var transactions = api.SearchTransactions(
                                        assetManagerIds: AddinContext.AssetManagerIds,
                                        assetBookIds: bookIds,
                                        transactionDateStart: transactionStartDate,
                                        transactionDateEnd: transactionEndDate,
                                        pageNo: 1,
                                        pageSize: QueryConstants.DefaultPageSize).Result.ToList();
                var assetsApi = AddinContext.Container.Resolve<IAssetsInterface>();
                var assets    = assetsApi.SearchAssets(
                                            assetManagerIds: AddinContext.AssetManagerIds,
                                            assetIds: transactions.Select(t => t.AssetId).ToList(),
                                            pageNo: 1,
                                            pageSize: QueryConstants.DefaultPageSize).Result;
                var models = transactions.Select(t =>
                                new EnrichedModel<Transaction, Asset>(t, assets.FirstOrDefault(a => a.AssetId == t.AssetId)));
                return ExcelTable.Format(models, AddinContext.Container.Resolve<IFormatter<EnrichedModel<Transaction, Asset>>>(), caller);
            };
            var output = AddinContext.Excel.Run(UdfNames.BookTransactionSearch, 
                                    string.Join(",", bookId, beginDate, endDate),
                                    getData);
            return output?.Equals(ExcelError.ExcelErrorNA) ?? true ? ExcelError.ExcelErrorGettingData : output;
        }
    }
}

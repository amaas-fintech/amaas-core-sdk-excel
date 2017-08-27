using AMaaS.Core.Sdk.Assets;
using AMaaS.Core.Sdk.Assets.Models;
using AMaaS.Core.Sdk.Configuration;
using AMaaS.Core.Sdk.Excel.Abstractions;
using AMaaS.Core.Sdk.Excel.Constants;
using AMaaS.Core.Sdk.Excel.Formatters;
using AMaaS.Core.Sdk.Excel.Helpers;
using AMaaS.Core.Sdk.Excel.Models;
using AMaaS.Core.Sdk.Transactions;
using AMaaS.Core.Sdk.Transactions.Models;
using Autofac;
using ExcelDna.Integration;
using ExcelDna.IntelliSense;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AMaaS.Core.Sdk.Excel
{
    public class TransactionsAddin : AMaaSAddinBase, ITransactionsAddin
    {
        [ExcelFunction(Name = UdfNames.PositionSearch, IsMacroType = true, Description = "Retrieve positions")]
        public static object GetPositionAsync(
            [ExcelArgument(AllowReference = true, Name = "Book ID")]string bookId = "",
            [ExcelArgument(AllowReference = true, Name = "Position Date")]string businessDate = "")
        {
            var caller = ExcelInterface.Call(XlCall.xlfCaller);
            var output = ExcelInterface.Run(UdfNames.TransactionSearch,
                                    string.Join(",", bookId, businessDate),
            delegate
            {
                if (AssetManagerIds.Count == 0)
                    throw new ApplicationException($"User {UserName} does not have a valid asset manager relationship");

                var bookIds      = bookId.MatchAll() ? null : new List<string> { bookId };
                var positionDate = !businessDate.MatchAll() && 
                                   DateTime.TryParse(businessDate, out DateTime businessDateParsed)
                                                ? (DateTime?)businessDateParsed 
                                                : null;

                var api       = Container.Resolve<ITransactionsInterface>();
                var positions = api.SearchPositions(
                                    assetManagerIds: AssetManagerIds,
                                    bookIds: bookIds,
                                    positionDate: positionDate).Result;
                var assetsApi = Container.Resolve<IAssetsInterface>();
                var assets    = assetsApi.SearchAssets(
                                            assetManagerIds: AssetManagerIds,
                                            assetIds: positions.Select(p => p.AssetId).ToList(),
                                            pageNo: 1,
                                            pageSize: QueryConstants.DefaultPageSize).Result;
                var models = positions.Select(p => 
                                new EnrichedModel<Position, Asset>(p, assets.FirstOrDefault(a => a.AssetId == p.AssetId)));
                return ExcelTable.Format(models, Container.Resolve<IFormatter<EnrichedModel<Position, Asset>>>(), caller);
            });
            return output?.Equals(ExcelError.ExcelErrorNA) ?? true ? ExcelError.ExcelErrorGettingData : output;
        }

        [ExcelFunction(Name = UdfNames.TransactionSearch, Description = "Retrieve transactions")]
        public static object SearchTransactions(
            [ExcelArgument(AllowReference = true, Name = "Book ID")] string bookId = "", 
            [ExcelArgument(AllowReference = true, Name = "Begin date for the transaction search.")] string beginDate = "", 
            [ExcelArgument(AllowReference = true, Name = "End date for the transaction search.")] string endDate = "")
        {
            var caller = ExcelInterface.Call(XlCall.xlfCaller);
            var output = ExcelInterface.Run(UdfNames.TransactionSearch, 
                                    string.Join(",", bookId, beginDate, endDate), 
            delegate
            {
                if (AssetManagerIds.Count == 0)
                    throw new ApplicationException($"User {UserName} does not have a valid asset manager relationship");

                var bookIds = bookId.MatchAll() ? null : new List<string> { bookId };
                var transactionStartDate = !beginDate.MatchAll() &&
                                           DateTime.TryParse(beginDate, out DateTime beginDateParsed)
                                                ? (DateTime?)beginDateParsed
                                                : null;
                var transactionEndDate = !endDate.MatchAll() &&
                                         DateTime.TryParse(endDate, out DateTime endDateParsed)
                                                ? (DateTime?)endDateParsed
                                                : null;
                var api          = Container.Resolve<ITransactionsInterface>();
                var transactions = api.SearchTransactions(
                                        assetManagerIds: AssetManagerIds,
                                        assetBookIds: bookIds,
                                        transactionDateStart: transactionStartDate,
                                        transactionDateEnd: transactionEndDate,
                                        pageNo: 1,
                                        pageSize: QueryConstants.DefaultPageSize).Result.ToList();
                var assetsApi = Container.Resolve<IAssetsInterface>();
                var assets    = assetsApi.SearchAssets(
                                            assetManagerIds: AssetManagerIds,
                                            assetIds: transactions.Select(t => t.AssetId).ToList(),
                                            pageNo: 1,
                                            pageSize: QueryConstants.DefaultPageSize).Result;
                var models = transactions.Select(t => 
                                new EnrichedModel<Transaction, Asset>(t, assets.FirstOrDefault(a => a.AssetId == t.AssetId)));
                return ExcelTable.Format(models, Container.Resolve<IFormatter<EnrichedModel<Transaction, Asset>>>(), caller);
            });

            return output?.Equals(ExcelError.ExcelErrorNA) ?? true ? ExcelError.ExcelErrorGettingData : output;
        }
    }
}

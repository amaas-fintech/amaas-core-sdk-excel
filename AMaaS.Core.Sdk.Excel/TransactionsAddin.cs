using AMaaS.Core.Sdk.Assets;
using AMaaS.Core.Sdk.Configuration;
using AMaaS.Core.Sdk.Excel.Constants;
using AMaaS.Core.Sdk.Excel.Formatters;
using AMaaS.Core.Sdk.Excel.Helpers;
using AMaaS.Core.Sdk.Extensions;
using AMaaS.Core.Sdk.Transactions;
using AMaaS.Core.Sdk.Transactions.Models;
using Autofac;
using ExcelDna.Integration;
using ExcelDna.IntelliSense;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMaaS.Core.Sdk.Excel
{
    public class TransactionsAddin : ITransactionsAddin
    {
        private static IContainer _container;

        public void AutoClose()
        {
            throw new NotImplementedException();
        }

        public void AutoOpen()
        {
            IntelliSenseServer.Register();
            ExcelIntegration.RegisterUnhandledExceptionHandler(ex => $"Error: {ex.ToString()}");

            var builder = new ContainerBuilder();
            builder.RegisterInstance(new AMaaSConfigDev("v1.0")).As<IAMaaSConfiguration>().SingleInstance();
            builder.RegisterType<AMaaSSession>().SingleInstance();
            builder.RegisterType<TransactionsInterface>().As<ITransactionsInterface>().InstancePerLifetimeScope();
            builder.RegisterType<TransactionFormatter>().As<IFormatter<Transaction>>().SingleInstance();
            builder.RegisterType<AssetsInterface>().As<IAssetsInterface>().InstancePerLifetimeScope();

            _container = builder.Build();
        }

        [ExcelFunction(Name = "ARGO.POS", IsMacroType = true, Description = "Retrieve positions")]
        public static object GetPositionAsync(
            [ExcelArgument(AllowReference = true, Name = "Asset manager ID")]string assetManagerId,
            [ExcelArgument(AllowReference = true, Name = "Book ID")]string bookId,
            [ExcelArgument(AllowReference = true, Name = "Business Date")]string businessDate)
        {
            if (!int.TryParse(assetManagerId, out int amid))
                throw new ArgumentException("Invalid AMID");

            var bookIds      = bookId.MatchAll() ? null : new List<string> { bookId };
            var positionDate = !businessDate.MatchAll() && 
                               DateTime.TryParse(businessDate, out DateTime businessDateParsed)
                                            ? (DateTime?)businessDateParsed 
                                            : null;

            var api     = _container.Resolve<ITransactionsInterface>();
            var results = api.SearchPositions(
                                assetManagerIds: new List<int> { amid },
                                bookIds: bookIds,
                                positionDate: positionDate);
            var jsonString = results.Result;
            return JsonConvert.SerializeObject(jsonString);
        }

        [ExcelFunction(Name = "ARGO.TRANS", IsMacroType = true, Description = "Retrieve transactions")]
        public static void SearchTransactions(
            [ExcelArgument(AllowReference = true, Name = "Asset manager ID")] string assetManagerId, 
            [ExcelArgument(AllowReference = true, Name = "Book ID")] string bookId, 
            [ExcelArgument(AllowReference = true, Name = "Begin date for the transaction search.")] string beginDate, 
            [ExcelArgument(AllowReference = true, Name = "End date for the transaction search.")] string endDate)
        {
            var caller = XlCall.Excel(XlCall.xlfCaller) as ExcelReference;

            if (!int.TryParse(assetManagerId, out int amid))
                throw new ArgumentException("Invalid Asset Manager ID");

            var bookIds = bookId.MatchAll() ? null : new List<string> { bookId };
            var transactionStartDate = !beginDate.MatchAll() && 
                                       DateTime.TryParse(beginDate, out DateTime beginDateParsed)
                                            ? (DateTime?)beginDateParsed 
                                            : null;
            var transactionEndDate = !endDate.MatchAll() && 
                                     DateTime.TryParse(endDate, out DateTime endDateParsed)
                                            ? (DateTime?)endDateParsed 
                                            : null;
            Task.Factory.StartNew(() =>
            {
                var api = _container.Resolve<ITransactionsInterface>();
                var transactions = api.SearchTransactions(
                                        assetManagerIds: new List<int> { amid },
                                        assetBookIds: bookIds,
                                        transactionDateStart: transactionStartDate,
                                        transactionDateEnd: transactionEndDate,
                                        pageNo: 1,
                                        pageSize: QueryConstants.DefaultPageSize).Result.ToList();
                var assetsApi = _container.Resolve<IAssetsInterface>();
                var assets = assetsApi.SearchAssets(
                                            assetManagerIds: new List<int> { amid },
                                            assetIds: transactions.Select(t => t.AssetId).ToList(),
                                            pageNo: 1,
                                            pageSize: QueryConstants.DefaultPageSize).Result;
                transactions.AsParallel().ForAll(t =>
                {
                    var asset = assets.FirstOrDefault(a => a.AssetId == t.AssetId);
                    Models.Reference ticker = null;
                    if (asset != null)
                    {
                        if (asset.References?.TryGetValue("Ticker", out ticker) ?? false)
                            t.References.Add("AssetTicker", ticker);
                        t.References.Add("AssetDescription", new Models.Reference { ReferenceValue = asset.DisplayName ?? asset.Description });
                    }
                });
                ExcelTable.Write(transactions, _container.Resolve<IFormatter<Transaction>>(), caller);
            });
        }

    }
}

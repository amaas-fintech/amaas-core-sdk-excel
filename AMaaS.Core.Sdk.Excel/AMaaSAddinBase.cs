using AMaaS.Core.Sdk.AssetManagers;
using AMaaS.Core.Sdk.Assets;
using AMaaS.Core.Sdk.Assets.Models;
using AMaaS.Core.Sdk.Configuration;
using AMaaS.Core.Sdk.Constants;
using AMaaS.Core.Sdk.Excel.Abstractions;
using AMaaS.Core.Sdk.Excel.Formatters;
using AMaaS.Core.Sdk.Excel.Models;
using AMaaS.Core.Sdk.Excel.UI;
using AMaaS.Core.Sdk.Extensions;
using AMaaS.Core.Sdk.Transactions;
using AMaaS.Core.Sdk.Transactions.Models;
using Autofac;
using ExcelDna.Integration;
using ExcelDna.IntelliSense;
using NetOffice.ExcelApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMaaS.Core.Sdk.Excel
{
    public abstract class AMaaSAddinBase : IExcelAddIn
    {
        public static bool IsLoggedIn => 
            AddinContext.Container?.Resolve<IUserViewModel>()?.IsLoggedIn ?? false;

        public void AutoClose()
        {
        }

        public void AutoOpen()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<AMaaSSession>();
            builder.RegisterType<TransactionsInterface>().As<ITransactionsInterface>();
            builder.RegisterType<AssetsInterface>().As<IAssetsInterface>();
            builder.RegisterType<AssetManagersInterface>().As<IAssetManagersInterface>();
            builder.RegisterType<TransactionFormatter>().As<IFormatter<EnrichedModel<Transaction, Asset>>>().SingleInstance();
            builder.RegisterType<PositionFormatter>().As<IFormatter<EnrichedModel<Position, Asset>>>().SingleInstance();
            builder.RegisterType<ExcelAbstraction>().As<IExcel>().SingleInstance();
            builder.RegisterType<UserViewModel>().As<IUserViewModel>().SingleInstance();
            builder.RegisterType<LoginView>().As<ILoginView>().SingleInstance();
            builder.RegisterType<ConfigurationViewModel>().As<IAMaaSConfiguration>().SingleInstance();

            var container      = builder.Build();
            var excelInterface = container.Resolve<IExcel>();
            excelInterface.Initialize();

            AddinContext.Container = container;
            AddinContext.Excel     = excelInterface;
        }
    }
}

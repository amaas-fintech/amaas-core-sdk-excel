using AMaaS.Core.Sdk.AssetManagers;
using AMaaS.Core.Sdk.Assets;
using AMaaS.Core.Sdk.Assets.Models;
using AMaaS.Core.Sdk.Configuration;
using AMaaS.Core.Sdk.Constants;
using AMaaS.Core.Sdk.Excel.Abstractions;
using AMaaS.Core.Sdk.Excel.Formatters;
using AMaaS.Core.Sdk.Excel.Models;
using AMaaS.Core.Sdk.Extensions;
using AMaaS.Core.Sdk.Transactions;
using AMaaS.Core.Sdk.Transactions.Models;
using Autofac;
using ExcelDna.Integration;
using ExcelDna.IntelliSense;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMaaS.Core.Sdk.Excel
{
    public abstract class AMaaSAddinBase : IExcelAddIn
    {
        protected static IContainer Container { get; private set; }
        protected static IExcel     ExcelInterface { get; private set; }
        protected static string     UserAmid { get; private set; }
        protected static string     UserName {get; private set;}
        protected static List<int> AssetManagerIds { get; private set; } = new List<int>();
        protected static Task       InitializeTask { get; private set; }

        public void AutoClose()
        {
        }

        public void AutoOpen()
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance(new AMaaSConfigDev("v1.0")).As<IAMaaSConfiguration>().SingleInstance();
            builder.RegisterType<AMaaSSession>().SingleInstance();
            builder.RegisterType<TransactionsInterface>().As<ITransactionsInterface>().InstancePerLifetimeScope();
            builder.RegisterType<AssetsInterface>().As<IAssetsInterface>().InstancePerLifetimeScope();
            builder.RegisterType<AssetManagersInterface>().As<IAssetManagersInterface>().InstancePerLifetimeScope();
            builder.RegisterType<TransactionFormatter>().As<IFormatter<EnrichedModel<Transaction, Asset>>>().SingleInstance();
            builder.RegisterType<PositionFormatter>().As<IFormatter<EnrichedModel<Position, Asset>>>().SingleInstance();
            builder.RegisterType<ExcelAbstraction>().As<IExcel>().SingleInstance();

            Container      = builder.Build();
            ExcelInterface = Container.Resolve<IExcel>();
            InitializeTask = Initialize();
            ExcelInterface.Initialize();
        }

        private async Task Initialize()
        {
            var assetManagerInterface = Container.Resolve<IAssetManagersInterface>();
            UserAmid                  = await assetManagerInterface.Session.GetTokenAttribute(CognitoAttributes.AssetManagerId);
            UserName                  = await assetManagerInterface.Session.GetTokenAttribute(CognitoAttributes.UserName);
            var relationships         = await assetManagerInterface.GetUserRelationships(int.Parse(UserAmid));
            AssetManagerIds           = relationships.Select(r => r.AssetManagerId).ToList();

#if DEBUG
            AssetManagerIds = AssetManagerIds.Count == 0 ? new List<int> { 1, 10 } : AssetManagerIds;
#endif
        }
    }
}

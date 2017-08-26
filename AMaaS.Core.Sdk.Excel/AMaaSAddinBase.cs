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
        protected static IContainer _container;
        protected static IExcel     _excel;
        protected static string     _userAmid;
        protected static string     _userName;
        protected static List<int>  _assetManagerIds;
        protected static Task       _initialize;

        public void AutoClose()
        {
            throw new NotImplementedException();
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

            _container  = builder.Build();
            _excel      = _container.Resolve<IExcel>();
            _initialize = Initialize();
            _excel.Initialize();
        }

        private async Task Initialize()
        {
            var assetManagerInterface = _container.Resolve<IAssetManagersInterface>();
            _userAmid                 = await assetManagerInterface.Session.GetTokenAttribute(CognitoAttributes.AssetManagerId);
            _userName                 = await assetManagerInterface.Session.GetTokenAttribute(CognitoAttributes.UserName);
            var relationships         = await assetManagerInterface.GetUserRelationships(int.Parse(_userAmid));
            _assetManagerIds          = relationships.Select(r => r.AssetManagerId).ToList();

#if DEBUG
            _assetManagerIds = _assetManagerIds.Count == 0 ? new List<int> { 1, 10 } : _assetManagerIds;
#endif
        }
    }
}

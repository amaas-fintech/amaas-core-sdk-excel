using AMaaS.Core.Sdk.Assets.Models;
using AMaaS.Core.Sdk.Excel.Constants;
using AMaaS.Core.Sdk.Excel.Models;
using AMaaS.Core.Sdk.Extensions;
using AMaaS.Core.Sdk.Transactions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMaaS.Core.Sdk.Excel.Formatters
{
    public class TransactionFormatter : IFormatter<EnrichedModel<Transaction, Asset>>
    {
        public object[] Header => new string[]
        {
            "Transaction ID",
            "Asset Book ID",
            "Transaction Type",
            "Transaction Date",
            "Settlement Date",
            "Counterparty Book",
            "Ticker",
            "Asset",
            "Asset Type",
            "Transaction Ccy",
            "Settlement Ccy",
            "Quantity",
            "Price",
            "Gross Settlement",
            "Net Settlement",
            "Commission",
            "Tax",
            "Other Charges"
        };

        public object[] FormatData(EnrichedModel<Transaction, Asset> model)
        {
            var transaction = model.Model;
            var asset       = model.Data;

            return new object[]
            {
                transaction.TransactionId,
                transaction.AssetBookId,
                transaction.TransactionType.GetEnumDisplay(),
                transaction.TransactionDate.ToISODateString(),
                transaction.SettlementDate.ToISODateString(),
                transaction.CounterpartyBookId,
                asset?.References.ContainsKey(References.Ticker) ?? false 
                    ? asset?.References[References.Ticker].ReferenceValue 
                    : asset?.AssetId ?? transaction.AssetId,
                asset?.DisplayName ?? asset?.Description ?? string.Empty,
                asset?.AssetType ?? string.Empty,
                transaction.TransactionCurrency,
                transaction.SettlementCurrency,
                transaction.Quantity,
                transaction.Price,
                transaction.GrossSettlement,
                transaction.NetSettlement,
                transaction.Charges.ContainsKey("Commission") ? (object)transaction.Charges["Commission"].ChargeValue : string.Empty,
                transaction.Charges.ContainsKey("Tax") 
                    ? (object)transaction.Charges["Tax"].ChargeValue 
                    : string.Empty,
                transaction.Charges.Any(x => x.Key != "Tax" && x.Key != "Commission") 
                    ? (object)transaction.Charges.FirstOrDefault(x => x.Key != "Tax" && x.Key != "Commission").Value.ChargeValue 
                    : string.Empty
            };
        }
    }
}

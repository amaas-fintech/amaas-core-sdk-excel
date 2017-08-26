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
            "Book",
            "Asset Type",
            "Asset Id",
            "Asset",
            "Action",
            "Quantity",
            "Price",
            "CCY",
            "Trade Date",
            "Settlement CCY",
            "Settlement",
            "CPTY",
            "CPTY Book",
            "Give-up CPTY",
            "Commission",
            "Tax",
            "Other Fees"
        };

        public object[] FormatData(EnrichedModel<Transaction, Asset> model)
        {
            var transaction = model.Model;
            var asset       = model.Data;

            return new object[]
            {
                transaction.AssetBookId,
                asset?.AssetType ?? string.Empty,
                asset?.References.ContainsKey(References.Ticker) ?? false 
                    ? asset?.References[References.Ticker].ReferenceValue 
                    : asset?.AssetId ?? transaction.AssetId,
                asset?.DisplayName ?? asset?.Description ?? string.Empty,
                transaction.TransactionAction.GetEnumDisplay(),
                transaction.Quantity,
                transaction.Price,
                transaction.TransactionCurrency,
                transaction.TransactionDate.ToISODateString(),
                transaction.SettlementCurrency,
                transaction.SettlementDate.ToISODateString(),
                transaction.Parties.ContainsKey("CounterParty") 
                    ? transaction.Parties["CounterParty"].PartyId 
                    : string.Empty,
                transaction.CounterPartyBookId,
                transaction.Parties.ContainsKey("Giveup CounterParty") 
                    ? transaction.Parties["Giveup CounterParty"].PartyId 
                    : string.Empty,
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

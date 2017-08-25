using AMaaS.Core.Sdk.Extensions;
using AMaaS.Core.Sdk.Transactions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMaaS.Core.Sdk.Excel.Formatters
{
    public class TransactionFormatter : IFormatter<Transaction>
    {
        public object[] Header => new string[]
        {
            "Book",
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

        public object[] FormatData(Transaction transaction)
        {
            return new object[]
            {
                transaction.AssetBookId,
                transaction.References.ContainsKey("AssetTicker") ? transaction.References["AssetTicker"].ReferenceValue : transaction.AssetId,
                transaction.References.ContainsKey("AssetDescription") ? transaction.References["AssetDescription"].ReferenceValue : string.Empty,
                transaction.TransactionAction.ToString(),
                transaction.Quantity,
                transaction.Price,
                transaction.TransactionCurrency,
                transaction.TransactionDate.ToISODateString(),
                transaction.SettlementCurrency,
                transaction.SettlementDate.ToISODateString(),
                transaction.Parties.ContainsKey("CounterParty") ? transaction.Parties["CounterParty"].PartyId : string.Empty,
                transaction.CounterPartyBookId,
                transaction.Parties.ContainsKey("Giveup CounterParty") ? transaction.Parties["Giveup CounterParty"].PartyId : string.Empty,
                transaction.Charges.ContainsKey("Commission") ? (decimal?)transaction.Charges["Commission"].ChargeValue : null,
                transaction.Charges.ContainsKey("Tax") ? (decimal?)transaction.Charges["Tax"].ChargeValue : null,
                transaction.Charges.Any(x => x.Key != "Tax" && x.Key != "Commission") 
                    ? (decimal?)transaction.Charges.FirstOrDefault(x => x.Key != "Tax" && x.Key != "Commission").Value.ChargeValue 
                    : null
            };
        }
    }
}

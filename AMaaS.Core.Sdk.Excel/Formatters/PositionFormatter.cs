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
    public class PositionFormatter : IFormatter<EnrichedModel<Position, Asset>>
    {
        public object[] Header => new string[]
        {
            "Book",
            "Asset Type",
            "Asset",
            "Ticker",
            "Quantity"
        };

        public object[] FormatData(EnrichedModel<Position, Asset> model)
        {
            var position = model.Model;
            var asset    = model.Data;
            return new object[]
            {
                position.BookId,
                asset?.AssetType ?? string.Empty,
                asset?.DisplayName ?? asset?.Description ?? string.Empty,
                asset?.References.ContainsKey(References.Ticker) ?? false
                    ? asset?.References[References.Ticker].ReferenceValue
                    : asset?.AssetId ?? position.AssetId,
                position.Quantity
            };
        }
    }
}

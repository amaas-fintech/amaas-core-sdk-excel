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
            "Asset Reference",
            "Asset Id",
            "Asset Name",
            "Quantity",
            "Asset Type"
        };

        public object[] FormatData(EnrichedModel<Position, Asset> model)
        {
            var position = model.Model;
            var asset    = model.Data;
            return new object[]
            {
                position.BookId,
                asset?.References?.Values.Where(r => r.ReferencePrimary).Select(r => r.ReferenceValue).FirstOrDefault() ?? string.Empty,
                asset?.AssetId ?? string.Empty,
                asset?.DisplayName ?? asset?.Description ?? string.Empty,
                position.Quantity,
                asset?.AssetType ?? string.Empty,
            };
        }
    }
}

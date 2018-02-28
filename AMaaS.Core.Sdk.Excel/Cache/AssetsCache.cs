using AMaaS.Core.Sdk.Assets;
using AMaaS.Core.Sdk.Assets.Models;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AMaaS.Core.Sdk.Excel.Constants;

namespace AMaaS.Core.Sdk.Excel.Cache
{
    public class AssetsCache: IAssetsInterface
    {
        private readonly ConcurrentDictionary<AssetKey, Asset> _cache = new ConcurrentDictionary<AssetKey, Asset>();
        private readonly IAssetsInterface _assetsClient;
        public string EndpointType => _assetsClient.EndpointType;
        public AMaaSSession Session => _assetsClient.Session;

        public AssetsCache(AMaaSSession session) 
        {
            _assetsClient = new AssetsInterface(session);
        }

        public async Task<IEnumerable<Asset>> SearchAssets(
            int assetManagerId, 
            List<string> assetIds, 
            List<string> assetClasses = null, 
            List<string> assetTypes = null, 
            int? pageNo = null, 
            int? pageSize = null)
        {
            if (assetIds == null || assetIds.Count == 0)
                throw new ArgumentException("Asset Ids are required.");

            var assetKeys = assetIds.Select(id => new AssetKey { AssetManagerId = assetManagerId, AssetId = id });
            var searchAssetIds = assetKeys.Where(key => !_cache.ContainsKey(key)).Select(key => key.AssetId).ToList();

            if (searchAssetIds.Count > 0)
            {
                try
                {
                    var assetIdFilters = searchAssetIds.Select((x, i) => new { Index = i, Value = x })
                                                   .GroupBy(x => x.Index / QueryConstants.DefaultFilterSize)
                                                   .Select(x => x.Select(v => v.Value).ToList())
                                                   .ToList();
                    var results = assetIdFilters.AsParallel().Select(async f => await _assetsClient.SearchAssets(
                                                                                        assetManagerId: assetManagerId,
                                                                                        assetIds: f,
                                                                                        assetTypes: assetTypes,
                                                                                        pageNo: pageNo,
                                                                                        pageSize: pageSize))
                                                             .Select(t => t.Result)
                                                             .SelectMany(a => a);
                    results.ToList().ForEach(a => _cache[new AssetKey { AssetManagerId = assetManagerId, AssetId = a.AssetId }] = a);
                }
                catch(AggregateException aex)
                {
                    throw new ApplicationException(aex.InnerExceptions.First().Message);
                }
            }

            return assetKeys.Select(key => _cache[key]).ToList();
        }

        private class AssetKey
        {
            public int AssetManagerId { get; set; }
            public string AssetId { get; set; }

            public override bool Equals(object obj)
            {
                var other = obj as AssetKey;
                return other?.AssetManagerId == this.AssetManagerId &&
                       other?.AssetId == this.AssetId;
            }

            public override int GetHashCode()
            {
                return AssetManagerId.GetHashCode() * 17 + AssetId.GetHashCode();
            }
        }
    }
}

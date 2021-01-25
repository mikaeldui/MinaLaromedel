using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Liber.Onlinebok
{
    [DebuggerStepThrough]
    public class LiberOnlinebokAssetNotFoundException : ApplicationException
    {
        public LiberOnlinebokAssetNotFoundException(Uri assetUri) : base(_getErrorMessage(assetUri))
        {
            AssetUri = assetUri;
        }

        public LiberOnlinebokAssetNotFoundException(Uri assetUri, Exception innerException) : base(_getErrorMessage(assetUri), innerException)
        {
            AssetUri = assetUri;
        }

        public Uri AssetUri { get; }

        [Pure]
        private static string _getErrorMessage(Uri assetUri) => $"Couldn't find the asset at Uri {assetUri}.";
    }
}

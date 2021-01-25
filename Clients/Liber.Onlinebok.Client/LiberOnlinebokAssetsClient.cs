using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Liber.Onlinebok
{
    public sealed class LiberOnlinebokAssetsClient : IDisposable
    {
        private HttpClient _httpClient;

        internal LiberOnlinebokAssetsClient(Uri assetsLocation)
        {
            _httpClient = new HttpClient
            {
                BaseAddress = assetsLocation
            };
        }

        /// <summary>
        /// Doesn't require authentication.
        /// </summary>
        public async Task<Stream> GetAssetAsync(Uri assetUri)
        {
            //var url = $"https://ttnpkgprd.s3.amazonaws.com/{_documentUuid}/assets/img/layout/{pageIndex}.jpg";

            var response = await _httpClient.GetAsync(assetUri);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound || response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    throw new LiberOnlinebokAssetNotFoundException(assetUri);

                throw new ApplicationException("Something happened getting the page.");
            }

            return await response.Content.ReadAsStreamAsync();
        }

        public void Dispose() => _httpClient.Dispose();
    }
}

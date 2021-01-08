using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HermodsLarobok.Clients
{
    public class LiberOnlinebokClient : IDisposable
    {
        private HttpClient _httpClient;

        public LiberOnlinebokClient()
        {
            _httpClient = new HttpClient();
        }

        /// <summary>
        /// Doesn't require authentication.
        /// </summary>
        /// <param name="ebookId"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public async Task<Stream> GetPageAsync(Guid ebookId, int page)
        {
            var url = $"https://ttnpkgprd.s3.amazonaws.com/{ebookId}/assets/img/layout/{page}.jpg";

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound || response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    return null;

                throw new ApplicationException("Something happened getting the page.");
            }

            return await response.Content.ReadAsStreamAsync();
        }

        public void Dispose() => _httpClient.Dispose();
    }
}

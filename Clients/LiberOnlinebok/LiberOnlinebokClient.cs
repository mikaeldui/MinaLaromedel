using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace LiberOnlinebok
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
        /// <param name="page">Starting at 1.</param>
        public async Task<LiberOnlinebokEbookPage> GetPageAsync(int pageIndex, LiberOnlinebokEbook ebook)
        {
            if (pageIndex < 1)
                throw new ArgumentOutOfRangeException(nameof(pageIndex), "The page index can't be less than 1.");

            var url = $"https://ttnpkgprd.s3.amazonaws.com/{ebook.Guid}/assets/img/layout/{pageIndex}.jpg";

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound || response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    throw new LiberOnlinebokPageNotFoundException(ebook, pageIndex);

                throw new ApplicationException("Something happened getting the page.");
            }

            return new LiberOnlinebokEbookPage(await response.Content.ReadAsStreamAsync(), pageIndex, ebook);
        }

        public void Dispose() => _httpClient.Dispose();
    }
}

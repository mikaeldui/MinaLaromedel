using Liber.Onlinebok;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Hermods.Novo
{
    public class HermodsNovoClient : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly HttpClientHandler _httpClientHandler;
        private readonly CookieContainer _cookieContainer;

        public HermodsNovoClient()
        {
            _cookieContainer = new CookieContainer();
            _httpClientHandler = new HttpClientHandler 
            { 
                AllowAutoRedirect = true,
                CookieContainer = _cookieContainer, 
                UseCookies = true 
            };
            _httpClient = new HttpClient(_httpClientHandler);
        }

        /// <summary>
        /// Throws <see cref="HermodsNovoInvalidCredentialsException"/> if the credentials are invalid.
        /// </summary>
        public async Task AuthenticateAsync(string username, string password)
        {
            var message = $"username={username}&password={password}";
            var content = new StringContent(message);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded");

            var response = await _httpClient.PostAsync("https://novo.hermods.se/login/index.php", content);

            if (!response.IsSuccessStatusCode || "https://novo.hermods.se/theme/frigg/layout/views/student/" != response.RequestMessage.RequestUri.ToString())
                throw new HermodsNovoInvalidCredentialsException("The credentials are not valid."); // TODO: returning bool is probably better.
        }

        public async Task<HermodsNovoEbook[]> GetEbooksAsync()
        {
            var response = await _httpClient.GetAsync("https://novo.hermods.se/?action=ebooks");

            _ensureSuccess(response);

            return await HermodsNovoHelper.ParseEbooksAsync(await response.Content.ReadAsStringAsync());
        }

        public async Task<LiberOnlinebokClient> GetLiberOnlinebokClientAsync(HermodsNovoEbook ebook)
        {
            if (ebook.Publisher != "Liber")
                throw new ArgumentException("The e-book is not published by Liber.");

            return await GetLiberOnlinebokClientAsync(ebook.Url.ToString());
        }

        public async Task<LiberOnlinebokClient> GetLiberOnlinebokClientAsync(string ebookUrl)
        {
            var response = await _httpClient.GetAsync(ebookUrl);

            _ensureSuccess(response);

            if (response.RequestMessage.RequestUri.ToString().StartsWith("https://novo.hermods.se/ham/linkresolver.php"))
                throw new ApplicationException("Redirection didn't work");

            return LiberOnlinebokClient.From(response.RequestMessage.RequestUri, _cookieContainer);
        }

        private bool _ensureSuccess(HttpResponseMessage response)
        {
            response.EnsureSuccessStatusCode();

            if (response.RequestMessage.RequestUri.ToString() == "https://novo.hermods.se/login/index.php")
                throw new HermodsNovoUnauthenticatedException("Redirected to https://novo.hermods.se/login/index.php");

            return true;
        }

        public void Dispose() => _httpClient.Dispose();
    }
}

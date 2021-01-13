using LiberOnlinebok;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;

namespace HermodsNovo
{
    public class HermodsNovoClient
    {
        private HttpClient _httpClient;
        private string _username;
        private string _password;

        public HermodsNovoClient()
        {
            HttpClientHandler handler = new HttpClientHandler();
            handler.AllowAutoRedirect = true;
            _httpClient = new HttpClient(handler);
        }

        public async Task AuthenticateAsync(string username, string password)
        {
            var message = $"username={username}&password={password}";
            var content = new StringContent(message);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded");

            var response = await _httpClient.PostAsync("https://novo.hermods.se/login/index.php", content);

            if (!response.IsSuccessStatusCode || "https://novo.hermods.se/theme/frigg/layout/views/student/" != response.RequestMessage.RequestUri.ToString())
                throw new HermodsNovoInvalidCredentialsException("The credentials are not valid."); // TODO: returning bool is probably better.

            _username = username;
            _password = password;            
        }

        private async Task _reauthenticateAsync() => await AuthenticateAsync(_username, _password);

        public async Task<HermodsNovoEbook[]> GetEbooksAsync()
        {
            var response = await _httpClient.GetAsync("https://novo.hermods.se/?action=ebooks");

            _ensureSuccess(response);

            return await HermodsNovoHelper.ParseEbooksAsync(await response.Content.ReadAsStringAsync());
        }

        public async Task<LiberOnlinebokEbook> GetLiberOnlinebokEbookAsync(HermodsNovoEbook ebook)
        {
            var response = await _httpClient.GetAsync(ebook.Url);

            _ensureSuccess(response);

            var uri = response.RequestMessage.RequestUri.ToString();

            var guid = Regex.Match(uri, "([0-9A-Fa-f]{8}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{12})").Value;

            return new LiberOnlinebokEbook(Guid.Parse(guid));
        }

        private bool _ensureSuccess(HttpResponseMessage response)
        {
            response.EnsureSuccessStatusCode();

            if (response.RequestMessage.RequestUri.ToString() == "https://novo.hermods.se/login/index.php")
                throw new HermodsNovoUnauthenticatedException("Redirected to https://novo.hermods.se/login/index.php");

            return true;
        }
    }
}

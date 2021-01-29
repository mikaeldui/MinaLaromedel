using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Credentials;

namespace MinaLaromedel.EbookProviders
{
    public interface IEbookProvider
    {
        Task<bool> AuthenticateAsync(PasswordCredential credential);

        Task<Ebook[]> GetEbooksAsync();

        Task DownloadEbookAsync(Ebook ebook);
    }
}

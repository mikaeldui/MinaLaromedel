using Hermods.Novo;
using MinaLaromedel.EbookProviders;
using MinaLaromedel.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Foundation.Collections;
using Windows.Security.Credentials;
using Windows.Storage;

namespace MinaLaromedel.Tasks
{
    public sealed class UpdateTask : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();

            // Credentials from settings to PasswordVault
            {
                IPropertySet LocalSettings = ApplicationData.Current.LocalSettings.Values;
                IPropertySet RoamingSettings = ApplicationData.Current.RoamingSettings.Values;

                if (LocalSettings.ContainsKey("username"))
                {
                    var vault = new PasswordVault();
                    vault.Add(new PasswordCredential("Hermods Novo", (string)LocalSettings["username"], (string)LocalSettings["password"]));

                    LocalSettings.Remove("username");
                    LocalSettings.Remove("password");
                    try
                    {
                        RoamingSettings.Remove("username");
                        RoamingSettings.Remove("password");
                    }
                    catch { }
                }
            }

            // From 1.0.5.0
            {
                const string FILE_NAME = "ebooksList.json";

                StorageFolder StorageFolder = ApplicationData.Current.LocalFolder;

                var hermodsNovoEbooks = await StorageFolder.ContainsFileAsync(FILE_NAME) ? await StorageFolder.GetObjectAsync<HermodsNovoEbook[]>(FILE_NAME) : new HermodsNovoEbook[0];

                if (hermodsNovoEbooks.Any(hne => hne.Url != null))
                {
                    var ebooks = hermodsNovoEbooks.Select(HermodsNovoEbookProvider.ConvertToEbook).ToArray();
                    await EbookStorage.SaveEbooksAsync(ebooks);
                }
            }

            deferral.Complete();
        }
    }
}

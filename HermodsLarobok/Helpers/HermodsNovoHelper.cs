using HermodsLarobok.Models;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HermodsLarobok.Helpers
{
    public static class HermodsNovoHelper
    {
        public static async Task<Ebook[]> ParseEbooksAsync(string html)
        {
            return await Task.Run(() =>
            {
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(html);

                var activeEbooks = doc.DocumentNode.Descendants().Where(n => n.HasClass("active_ebook")).ToArray();

                var result = new Ebook[activeEbooks.Length];

                for (int i = 0; i < activeEbooks.Length; i++)
                {
                    var ebook = activeEbooks[i];
                    result[i] = new Ebook()
                    {
                        Title = ebook.Descendants().First(n => n.HasClass("teaching_materials_title")).FirstChild.InnerText,
                        Publisher = ebook.Descendants().First(n => n.HasClass("teaching_materials_publisher")).FirstChild.InnerText,
                        Status = ebook.Descendants().First(n => n.HasClass("ebook_status")).FirstChild.InnerText.Trim(),
                        Isbn = ebook.Attributes["data-isbn"].Value,
                        StartDate = DateTime.Parse(ebook.Attributes["data-startdate"].Value),
                        EndDate = DateTime.Parse(ebook.Attributes["data-enddate"].Value),
                        Url = new Uri("https://novo.hermods.se/ham/" + ebook.Attributes["data-ebookurl"].Value)
                    };
                }

                return result;
            });
        }
    }
}

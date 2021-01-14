using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HermodsNovo
{
    public static class HermodsNovoHelper
    {
        /// <summary>
        /// Returns an array of the active ebooks found in the document.
        /// </summary>
        public static async Task<HermodsNovoEbook[]> ParseEbooksAsync(string html)
        {
            return await Task.Run(() =>
            {
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(html);

                var activeEbooks = doc.DocumentNode.Descendants().Where(n => n.HasClass("active_ebook")).ToArray();

                var result = new HermodsNovoEbook[activeEbooks.Length];

                for (int i = 0; i < activeEbooks.Length; i++)
                {
                    var ebook = activeEbooks[i];
                    result[i] = new HermodsNovoEbook()
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

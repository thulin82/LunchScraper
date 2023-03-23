using HtmlAgilityPack;
using LunchScraper.Models;
using Microsoft.AspNetCore.Mvc;

namespace LunchScraper.Controllers
{
    public class LunchController : Controller
    {
        public ActionResult Index()
        {
            var malmoArenaMenu = GetMalmoArenaLunch();
            var edgeKitchenMenu = GetEdgeKitchenLunch();

            ViewBag.MalmoArenaMenu = malmoArenaMenu;
            ViewBag.EdgeKitchenMenu = RemoveCenter(edgeKitchenMenu);

            return View();
        }

        /// <summary>
        /// Get lunch menu from Edge Kitchen
        /// </summary>
        /// <returns></returns>
        private List<string> GetEdgeKitchenLunch()
        {
            var web = new HtmlWeb();
            var doc = web.LoadFromWebAsync("https://edgekitchen.se/meny", System.Text.Encoding.UTF8).Result;
            var menu = doc.DocumentNode.SelectNodes("//body/div[4]/div[4]/div[1]/div[1]/div[1]/div[1]/div[2]/div[1]/div[1]/div[1]/div[1]/div[1]");
            return menu.Select(x => x.InnerHtml.Trim()).ToList();
        }

        /// <summary>
        /// Remove the string "style=\"text-align: center;\"" from the list
        /// </summary>
        /// <param name="menu"></param>
        /// <returns></returns>
        private List<string> RemoveCenter(List<string> menu)
        {
            return menu.Select(x => x.Replace("style=\"text-align: center;\"", "")).ToList();
        }

        /// <summary>
        /// Get lunch menu from Malmö Arena
        /// </summary>
        /// <returns></returns>
        private List<DailyLunch> GetMalmoArenaLunch()
        {
            var web = new HtmlWeb();
            var document = web.LoadFromWebAsync("https://www.malmoarena.com/mat-dryck/lunch", System.Text.Encoding.UTF8).Result;
            var nodes = document.DocumentNode.SelectNodes("//body/div[4]/div[1]/div[1]/div[1]/div[3]/div[1]/div[1]/div[3]/p[position()>1]");

            List<DailyLunch> items = new List<DailyLunch>();

            foreach (var node in nodes)
            {
                var lunchItems = new List<LunchItem>();

                var firstItemNode = node.SelectSingleNode("br[1]").NextSibling;
                var secondItemNode = node.SelectSingleNode("br[2]");
                var thirdItemNode = node.SelectSingleNode("br[3]");

                lunchItems.Add(new LunchItem()
                {
                    Name = HtmlEntity.DeEntitize(firstItemNode.InnerText)
                });

                if (secondItemNode != null && secondItemNode.NextSibling != null)
                {
                    lunchItems.Add(new LunchItem()
                    {
                        Name = HtmlEntity.DeEntitize(secondItemNode.NextSibling.InnerText)
                    });
                }

                if (thirdItemNode != null && thirdItemNode.NextSibling != null)
                {
                    lunchItems.Add(new LunchItem()
                    {
                        Name = HtmlEntity.DeEntitize(thirdItemNode.NextSibling.InnerText)
                    });
                }

                items.Add(new DailyLunch()
                {
                    DayOfTheWeek = HtmlEntity.DeEntitize(node.SelectSingleNode("strong").InnerText),
                    Items = lunchItems,
                    Price = HtmlEntity.DeEntitize(node.SelectSingleNode("span").InnerText),
                });
            }

            return items;
        }


    }
}
